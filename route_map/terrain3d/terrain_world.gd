extends Node3D
## The low-poly 3D world, built from real data: NOAA ETOPO1 elevation (route_map/data/terrain_height.exr)
## and the Natural Earth coastline, lakes and rivers (route_map/data/coast.json).
## 3D x/z = map x/y (map units, ~1.1 km each); y = height, exaggerated so relief reads at this scale.
##
## Each chunk is triangulated from points rather than a grid: the exact coastline (crisp shores),
## lake shores, river beds and banks, and an adaptive interior (big calm facets on plains, small ones
## in the mountains). Chunks are built on worker threads around `focus`.

const Geo := preload("res://route_map/geo.gd")
const MapData := preload("res://route_map/map_data.gd")
const HEIGHT := "res://route_map/data/terrain_height.exr"
const PX := 2.0                 # map units per heightmap pixel (see tools/build_terrain.gd)
const CHUNK := 256.0
const FLAT_STEP := 12.0         # interior point spacing on plains (near the camera)
const RUGGED_STEP := 4.0        # ... in rugged country
const RUGGED_METRES := 120.0    # height range inside a cell that counts as rugged
const COAST_STEP := 1.4         # coastline points closer than this are merged
const EDGE_STEP := 4.0          # shared points along chunk edges (same at every LOD, so no cracks)
const BEACH_WIDTH := 1.8        # map units from the waterline to the inland ring
const LOD_NEAR := 700.0         # beyond this from focus: half density
const LOD_FAR := 1600.0         # beyond this: quarter density (the horizon)
const EXAG := 17.0
const METRES_PER_UNIT := 1112.0
const SHORE_Y := 0.3            # land height at the waterline (the sea sits at 0)
const BUILDS_PER_FRAME := 4
const HASH := 6.0               # spatial hash cell for point spacing

enum { TERRAIN, COAST, BED, BANK, LAKE_SHORE, DUNE }

@export var view_radius := 1400.0

var focus := Vector2(4000, 2200)
var heights := PackedFloat32Array()
var hw := 0
var hh := 0
var map_size := Vector2.ZERO
var height_tex: ImageTexture

var _land: Array = []           # {"ring", "bbox"}
var _lakes: Array = []          # {"ring", "bbox", "y"}
var _rivers: Array = []         # {"pts", "wy" (water y per point), "width"}
var _river_cells := {}          # Vector2i chunk -> [[river idx, point idx], ...]
var _chunks := {}
var _pending := {}
var _results := {}
var _mutex := Mutex.new()
var _land_mat: StandardMaterial3D


func _ready() -> void:
	var img := Image.load_from_file(ProjectSettings.globalize_path(HEIGHT))
	if img.get_format() != Image.FORMAT_RF:
		img.convert(Image.FORMAT_RF)
	hw = img.get_width()
	hh = img.get_height()
	map_size = Vector2(hw, hh) * PX
	heights = img.get_data().to_float32_array()
	height_tex = ImageTexture.create_from_image(img)
	_land_mat = StandardMaterial3D.new()
	_land_mat.vertex_color_use_as_albedo = true
	_land_mat.roughness = 0.95
	_land_mat.specular_mode = BaseMaterial3D.SPECULAR_DISABLED
	var md := MapData.new()
	md.load_data("res://route_map/data/coast.json", false)
	for ring in md.detail_rings:
		if ring.size() >= 3:
			_land.append({"ring": ring, "bbox": _bbox(ring)})
	_prepare_lakes(md.lake_rings)
	_prepare_rivers(md.river_runs)
	_build_rivers_mesh()
	_build_lakes_mesh()
	var ocean: Node3D = load("res://route_map/terrain3d/ocean.gd").new()
	ocean.name = "Ocean"
	ocean.set("world", self)
	add_child(ocean)


static func _bbox(ring: PackedVector2Array) -> Rect2:
	var b := Rect2(ring[0], Vector2.ZERO)
	for p in ring:
		b = b.expand(p)
	return b


## Height in metres at a map position (bilinear).
func metres_at(p: Vector2) -> float:
	var fx := clampf(p.x / PX - 0.5, 0.0, hw - 1.001)
	var fy := clampf(p.y / PX - 0.5, 0.0, hh - 1.001)
	var x := int(fx)
	var y := int(fy)
	var tx := fx - x
	var ty := fy - y
	var a := heights[y * hw + x]
	var b := heights[y * hw + x + 1]
	var c := heights[(y + 1) * hw + x]
	var d := heights[(y + 1) * hw + x + 1]
	return lerpf(lerpf(a, b, tx), lerpf(c, d, tx), ty)


static func y_of(m: float) -> float:
	return maxf(m, 0.0) / METRES_PER_UNIT * EXAG


## Land surface height at p (never below the waterline, since the vector coast says it is land).
func land_y(p: Vector2) -> float:
	return SHORE_Y + y_of(metres_at(p))


func height_at(p: Vector2) -> float:
	return land_y(p)


# --- Rivers and lakes --------------------------------------------------------

func _prepare_rivers(runs: Array) -> void:
	for rv in runs:
		var src: PackedVector2Array = rv["pts"]
		if src.size() < 2:
			continue
		var pts := PackedVector2Array()
		for i in src.size() - 1:
			var n := int(src[i].distance_to(src[i + 1]) / 3.0) + 1
			for k in n:
				pts.append(src[i].lerp(src[i + 1], float(k) / n))
		pts.append(src[src.size() - 1])
		# The water level only ever falls toward the mouth, and sits on the valley floor.
		var wy := PackedFloat32Array()
		wy.resize(pts.size())
		var level := INF
		for i in pts.size():
			var lo := minf(metres_at(pts[i]), minf(metres_at(pts[i] + Vector2(2, 0)), metres_at(pts[i] + Vector2(0, 2))))
			level = minf(level, lo)
			wy[i] = 0.12 + y_of(level)
		var rank: float = rv["rank"]
		var width := 2.8 if rank <= 4.0 else (2.1 if rank <= 6.0 else 1.5)
		var idx := _rivers.size()
		_rivers.append({"pts": pts, "wy": wy, "width": width})
		for i in pts.size():
			var k := Vector2i(floori(pts[i].x / CHUNK), floori(pts[i].y / CHUNK))
			if not _river_cells.has(k):
				_river_cells[k] = []
			_river_cells[k].append([idx, i])


func _prepare_lakes(rings: Array) -> void:
	for ring in rings:
		if ring.size() < 3:
			continue
		var lo := INF
		for p in ring:
			lo = minf(lo, metres_at(p))
		_lakes.append({"ring": ring, "bbox": _bbox(ring), "y": SHORE_Y + y_of(lo)})


# --- Streaming ---------------------------------------------------------------

func _process(_delta: float) -> void:
	_stream()


func _stream() -> void:
	var c0 := Vector2i(floori((focus.x - view_radius) / CHUNK), floori((focus.y - view_radius) / CHUNK))
	var c1 := Vector2i(floori((focus.x + view_radius) / CHUNK), floori((focus.y + view_radius) / CHUNK))
	var wanted := {}
	for cy in range(maxi(0, c0.y), mini(int(map_size.y / CHUNK), c1.y) + 1):
		for cx in range(maxi(0, c0.x), mini(int(map_size.x / CHUNK), c1.x) + 1):
			var k := Vector2i(cx, cy)
			var centre := (Vector2(k) + Vector2(0.5, 0.5)) * CHUNK
			if centre.distance_to(focus) < view_radius + CHUNK:
				wanted[k] = centre.distance_to(focus)
	for k in _chunks.keys():
		if not wanted.has(k) and not _pending.has(k):
			_chunks[k]["mi"].queue_free()
			_chunks.erase(k)
	# Missing chunks, and chunks whose level of detail should change (the old mesh stays until the new one lands).
	var missing: Array = []
	for k in wanted:
		if not _pending.has(k) and (not _chunks.has(k) or _chunks[k]["lod"] != _lod_for(wanted[k])):
			missing.append(k)
	missing.sort_custom(func(a, b): return wanted[a] < wanted[b])
	for i in mini(BUILDS_PER_FRAME, missing.size()):
		var k: Vector2i = missing[i]
		_pending[k] = WorkerThreadPool.add_task(_build_chunk.bind(k, _lod_for(wanted[k])))
	_mutex.lock()
	var done := _results.duplicate()
	_results.clear()
	_mutex.unlock()
	for k in done:
		WorkerThreadPool.wait_for_task_completion(_pending[k])
		_pending.erase(k)
		if _chunks.has(k):
			_chunks[k]["mi"].queue_free()
			_chunks.erase(k)
		if not wanted.has(k):
			continue
		var mi := MeshInstance3D.new()
		mi.mesh = done[k][0]
		mi.material_override = _land_mat
		add_child(mi)
		_chunks[k] = {"mi": mi, "lod": done[k][1]}


static func _lod_for(d: float) -> int:
	return 0 if d < LOD_NEAR else (1 if d < LOD_FAR else 2)


## True while chunks are still being built (for screenshot scripts).
func busy() -> bool:
	return not _pending.is_empty()


# --- Chunk building (worker threads) -------------------------------------------

func _build_chunk(k: Vector2i, lod: int) -> void:
	var sc := float(1 << lod)
	var flat_step := FLAT_STEP * sc
	var rugged_step := RUGGED_STEP * sc
	var coast_step := COAST_STEP * sc
	var rect := Rect2(Vector2(k) * CHUNK, Vector2(CHUNK, CHUNK))
	var box := PackedVector2Array([rect.position, Vector2(rect.end.x, rect.position.y), rect.end, Vector2(rect.position.x, rect.end.y)])
	var polys: Array = []
	for l in _land:
		if l["bbox"].intersects(rect):
			for c in Geometry2D.intersect_polygons(l["ring"], box):
				if c.size() >= 3:
					polys.append({"ring": c, "bbox": _bbox(c)})
	if polys.is_empty():
		_finish(k, lod, ArrayMesh.new())
		return
	var lakes: Array = []
	for lk in _lakes:
		if lk["bbox"].intersects(rect):
			lakes.append(lk)
	var s := {"pts": [], "kinds": [], "ys": [], "grid": {}}
	# 1. The coastline, exactly. Points on the chunk border get real heights so neighbours meet.
	# A second ring just inland keeps the beach an even strip instead of long spikes.
	var inland: Array = []
	for poly in polys:
		var ring: PackedVector2Array = poly["ring"]
		var rn := ring.size()
		for ri in rn:
			var p := ring[ri]
			if _on_edge(p, rect):
				_add(s, p, TERRAIN, land_y(p), 0.5)
				continue
			_add(s, p, COAST, SHORE_Y, coast_step)
			var tang := ring[(ri + 1) % rn] - ring[(ri - 1 + rn) % rn]
			if tang.length_squared() < 0.0001:
				continue
			var nrm := tang.normalized().orthogonal() * BEACH_WIDTH * sc
			var q := p + nrm if _in_polys(p + nrm, polys) else p - nrm
			if rect.grow(-0.5).has_point(q) and _in_polys(q, polys):
				inland.append(q)
	for q in inland:
		_add(s, q, DUNE, land_y(q), coast_step * 1.5)
	# 2. Shared points along the chunk edges.
	var e := 0.0
	while e <= CHUNK:
		for p in [rect.position + Vector2(e, 0), rect.position + Vector2(0, e), Vector2(rect.end.x, rect.position.y + e), Vector2(rect.position.x + e, rect.end.y)]:
			if _in_polys(p, polys):
				_add(s, p, TERRAIN, land_y(p), 1.0)
		e += EDGE_STEP
	# 3. Lake shores.
	for lk in lakes:
		for p in lk["ring"]:
			if rect.has_point(p) and not _on_edge(p, rect):
				_add(s, p, LAKE_SHORE, lk["y"] + 0.08, coast_step)
	# 4. River channels: a bed down the middle and a bank either side.
	for ref in _river_cells.get(k, []):
		var rv: Dictionary = _rivers[ref[0]]
		var i: int = ref[1]
		if lod > 0 and i % 2 == 1:
			continue
		var rp: PackedVector2Array = rv["pts"]
		var p: Vector2 = rp[i]
		if not rect.grow(-0.5).has_point(p) or not _in_polys(p, polys):
			continue
		var dir := (rp[mini(i + 1, rp.size() - 1)] - rp[maxi(i - 1, 0)]).normalized()
		var side: Vector2 = dir.orthogonal() * (float(rv["width"]) + 0.9)
		var wy: float = rv["wy"][i]
		_add(s, p, BED, wy - 0.6, 1.6)
		for q in [p + side, p - side]:
			if rect.grow(-0.5).has_point(q) and _in_polys(q, polys):
				_add(s, q, BANK, wy + 0.2, 1.6)
	# 5. Adaptive interior: coarse on plains, fine where the land is rugged.
	var n := int(CHUNK / flat_step)
	for j in n + 1:
		for i in n + 1:
			var c := rect.position + Vector2(i, j) * flat_step
			var hs := [metres_at(c), metres_at(c + Vector2(flat_step, 0)), metres_at(c + Vector2(0, flat_step)),
				metres_at(c + Vector2(flat_step, flat_step)), metres_at(c + Vector2(flat_step, flat_step) * 0.5)]
			var span: float = hs.max() - hs.min()
			var step := rugged_step if span > RUGGED_METRES else (flat_step * 0.5 if span > RUGGED_METRES * 0.35 else flat_step)
			var m := int(flat_step / step)
			for b in m:
				for a in m:
					var p := c + (Vector2(a, b) + Vector2(0.5, 0.5)) * step
					var h := hash(Vector2i(int(p.x * 3.0), int(p.y * 3.0)))
					p += Vector2((h & 255) / 255.0 - 0.5, ((h >> 8) & 255) / 255.0 - 0.5) * step * 0.5
					if rect.grow(-1.0).has_point(p) and _in_polys(p, polys) and not _in_lakes(p, lakes):
						_add(s, p, TERRAIN, land_y(p), step * 0.5)
	var pts := PackedVector2Array(s["pts"])
	var kinds: Array = s["kinds"]
	var ys: Array = s["ys"]
	if pts.size() < 3:
		_finish(k, lod, ArrayMesh.new())
		return
	# Carve: land near a river slopes down into its channel. River points go in a local hash
	# (cells of RIVER_REACH) so each terrain point only checks its neighbourhood.
	const RIVER_REACH := 9.0
	var rhash := {}
	var reach_rect := rect.grow(RIVER_REACH)
	for dy in range(-1, 2):
		for dx in range(-1, 2):
			for ref in _river_cells.get(k + Vector2i(dx, dy), []):
				var rv: Dictionary = _rivers[ref[0]]
				var q: Vector2 = rv["pts"][ref[1]]
				if reach_rect.has_point(q):
					var cell := Vector2i(floori(q.x / RIVER_REACH), floori(q.y / RIVER_REACH))
					if not rhash.has(cell):
						rhash[cell] = []
					rhash[cell].append([q, rv["wy"][ref[1]], rv["width"]])
	if not rhash.is_empty():
		for idx in pts.size():
			if kinds[idx] != TERRAIN:
				continue
			var p: Vector2 = pts[idx]
			var pc := Vector2i(floori(p.x / RIVER_REACH), floori(p.y / RIVER_REACH))
			var best := INF
			var bwy := 0.0
			var bw := 0.0
			for cy in range(-1, 2):
				for cx in range(-1, 2):
					for e2 in rhash.get(pc + Vector2i(cx, cy), []):
						var d: float = (e2[0] as Vector2).distance_to(p)
						if d < best:
							best = d
							bwy = e2[1]
							bw = e2[2]
			if best < bw * 1.1:
				ys[idx] = minf(ys[idx], bwy - 0.6)   # under the water: never poke through the river
			elif best < bw * 3.2:
				ys[idx] = minf(ys[idx], lerpf(bwy + 0.2, ys[idx], clampf((best - bw * 1.1) / (bw * 2.1), 0.0, 1.0)))
	var tris := Geometry2D.triangulate_delaunay(pts)
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var any := false
	for t in range(0, tris.size(), 3):
		var ia := tris[t]
		var ib := tris[t + 1]
		var ic := tris[t + 2]
		var centre := (pts[ia] + pts[ib] + pts[ic]) / 3.0
		if not _in_polys(centre, polys) or _in_lakes(centre, lakes):
			continue
		if absf((pts[ib] - pts[ia]).cross(pts[ic] - pts[ia])) < 0.02:
			continue   # zero-width slivers along chunk edges would show as thin walls
		any = true
		var v0 := Vector3(pts[ia].x, ys[ia], pts[ia].y)
		var v1 := Vector3(pts[ib].x, ys[ib], pts[ib].y)
		var v2 := Vector3(pts[ic].x, ys[ic], pts[ic].y)
		var nrm := (v1 - v0).cross(v2 - v0).normalized()
		if nrm.y > 0.0:
			# Godot's front faces wind clockwise; keep every facet facing up so lighting is consistent.
			var tmp := v1
			v1 = v2
			v2 = tmp
		else:
			nrm = -nrm
		var shore: bool = kinds[ia] in [COAST, DUNE] and kinds[ib] in [COAST, DUNE] and kinds[ic] in [COAST, DUNE]
		var river: bool = kinds[ia] == BANK or kinds[ib] == BANK or kinds[ic] == BANK or kinds[ia] == BED or kinds[ib] == BED or kinds[ic] == BED
		var r := float(hash(Vector2i(int(centre.x * 7.0), int(centre.y * 7.0))) & 1023) / 1023.0
		var col := _colour(metres_at(centre), nrm.y, centre, r, shore, river)
		for v in [v0, v1, v2]:
			st.set_color(col)
			st.set_normal(nrm)
			st.add_vertex(v)
	_finish(k, lod, st.commit() if any else ArrayMesh.new())


func _add(s: Dictionary, p: Vector2, kind: int, y: float, gap: float) -> void:
	var pts: Array = s["pts"]
	var grid: Dictionary = s["grid"]
	var cell := Vector2i(floori(p.x / HASH), floori(p.y / HASH))
	var reach := int(ceil(gap / HASH))
	for dy in range(-reach, reach + 1):
		for dx in range(-reach, reach + 1):
			for j in grid.get(cell + Vector2i(dx, dy), []):
				if (pts[j] as Vector2).distance_to(p) < gap:
					return
	if not grid.has(cell):
		grid[cell] = []
	grid[cell].append(pts.size())
	pts.append(p)
	s["kinds"].append(kind)
	s["ys"].append(y)


func _finish(k: Vector2i, lod: int, mesh: ArrayMesh) -> void:
	_mutex.lock()
	_results[k] = [mesh, lod]
	_mutex.unlock()


## Inside the vector coastline (land below sea level, like the Dutch polders, still counts).
func _on_land(p: Vector2) -> bool:
	for l in _land:
		if l["bbox"].has_point(p) and Geometry2D.is_point_in_polygon(p, l["ring"]):
			return true
	return false


static func _on_edge(p: Vector2, r: Rect2) -> bool:
	return absf(p.x - r.position.x) < 0.01 or absf(p.x - r.end.x) < 0.01 or absf(p.y - r.position.y) < 0.01 or absf(p.y - r.end.y) < 0.01


static func _in_polys(p: Vector2, polys: Array) -> bool:
	for poly in polys:
		if poly["bbox"].grow(0.01).has_point(p) and Geometry2D.is_point_in_polygon(p, poly["ring"]):
			return true
	return false


static func _in_lakes(p: Vector2, lakes: Array) -> bool:
	for lk in lakes:
		if lk["bbox"].has_point(p) and Geometry2D.is_point_in_polygon(p, lk["ring"]):
			return true
	return false


## Stylised palette: by height, slope and latitude, with a little per-facet variation.
func _colour(m: float, up: float, p: Vector2, r: float, shore: bool, river: bool) -> Color:
	var ll := Geo.unproject(p)
	var lat := ll.x
	var steep := 1.0 - clampf(up, 0.0, 1.0)
	var c: Color
	if shore and m < 30.0 and steep < 0.5:
		c = Color(0.84, 0.76, 0.55)                                                          # beaches, dunes
	elif river and m < 400.0:
		c = Color(0.33, 0.52, 0.27)                                                          # water meadows
	elif m < 6.0:
		c = Color(0.45, 0.55, 0.34)                                                          # marsh and fen
	else:
		var lowland := Color(0.42, 0.6, 0.26).lerp(Color(0.2, 0.37, 0.23), smoothstep(52.0, 62.0, lat))   # meadow -> taiga
		lowland = lowland.lerp(Color(0.6, 0.6, 0.47), smoothstep(64.0, 69.0, lat))                         # tundra
		lowland = lowland.lerp(Color(0.64, 0.62, 0.4), smoothstep(50.0, 46.0, lat) * smoothstep(30.0, 38.0, ll.y))   # steppe
		var hills := Color(0.58, 0.54, 0.34)
		var rock := Color(0.5, 0.46, 0.42)
		var snow_line := lerpf(2700.0, 750.0, smoothstep(45.0, 70.0, lat))
		c = lowland.lerp(hills, smoothstep(250.0, 900.0, m))
		c = c.lerp(rock, clampf(smoothstep(900.0, 2100.0, m) + steep * 1.3, 0.0, 1.0))
		if m > snow_line or (lat > 60.0 and ll.y < -20.0 and m > 500.0):                  # snow; the Greenland ice cap
			c = Color(0.93, 0.94, 0.96)
	return c.darkened((r - 0.5) * 0.1)


# --- Water meshes for rivers and lakes ---------------------------------------------

func _build_rivers_mesh() -> void:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	for rv in _rivers:
		var pts: PackedVector2Array = rv["pts"]
		var wy: PackedFloat32Array = rv["wy"]
		var w: float = rv["width"]
		var along := 0.0
		for i in pts.size() - 1:
			var a := pts[i]
			var b := pts[i + 1]
			var na := (pts[mini(i + 1, pts.size() - 1)] - pts[maxi(i - 1, 0)]).normalized().orthogonal() * w
			var nb := (pts[mini(i + 2, pts.size() - 1)] - pts[i]).normalized().orthogonal() * w
			var la := along
			along += a.distance_to(b)
			if metres_at(a) < -4.0 and metres_at(b) < -4.0 and not _on_land(a) and not _on_land(b):
				continue   # out at sea: the ocean covers it
			var q := [Vector3(a.x + na.x, wy[i], a.y + na.y), Vector3(a.x - na.x, wy[i], a.y - na.y),
				Vector3(b.x + nb.x, wy[i + 1], b.y + nb.y), Vector3(b.x - nb.x, wy[i + 1], b.y - nb.y)]
			var uv := [Vector2(la, 0), Vector2(la, 1), Vector2(along, 0), Vector2(along, 1)]
			for j in [0, 2, 1, 1, 2, 3]:
				st.set_uv(uv[j])
				st.set_normal(Vector3.UP)
				st.add_vertex(q[j])
	var mi := MeshInstance3D.new()
	mi.name = "Rivers"
	mi.mesh = st.commit()
	mi.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	var mat := ShaderMaterial.new()
	mat.shader = load("res://route_map/terrain3d/river.gdshader")
	mi.material_override = mat
	add_child(mi)


func _build_lakes_mesh() -> void:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	for lk in _lakes:
		var ring: PackedVector2Array = lk["ring"]
		for i in Geometry2D.triangulate_polygon(ring):
			st.set_uv(ring[i] * 0.05)
			st.set_normal(Vector3.UP)
			st.add_vertex(Vector3(ring[i].x, lk["y"], ring[i].y))
	var mi := MeshInstance3D.new()
	mi.name = "Lakes"
	mi.mesh = st.commit()
	mi.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	var mat := ShaderMaterial.new()
	mat.shader = load("res://route_map/terrain3d/river.gdshader")
	mat.set_shader_parameter("lake", true)
	mi.material_override = mat
	add_child(mi)
