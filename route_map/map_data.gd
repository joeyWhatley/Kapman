@tool
extends RefCounted
## Coastline linework, the navigation grid, and route finding over sea, rivers, lakes and portages.
## Grid cells: SEA, LAND, RIVER (rivers and sailable lakes), PORTAGE (haul the ship overland).

const Geo := preload("res://route_map/geo.gd")

var detail_runs: Array = []   # PackedVector2Array open polylines (crisp coast)
var detail_rings: Array = []  # PackedVector2Array closed land outlines (for the 3D terrain)
var lake_rings: Array = []    # PackedVector2Array closed lake outlines
var rough_runs: Array = []    # PackedVector2Array open polylines (fog coast)
var lake_runs: Array = []     # PackedVector2Array closed rings (big lakes)
var medium_runs: Array = []   # generalised coast for zoomed-out views
var far_runs: Array = []      # calmest coast for the whole-map view
var water_runs: Array = []    # [level][run] engraved water-lines out to sea
var cols := 0
var rows := 0
var land := PackedByteArray()      # 1 = land (solid) — kept for is_land()
var kind := PackedByteArray()      # SEA / LAND / RIVER / PORTAGE per cell
var flow := {}                     # river cell index -> downstream direction
var river_runs: Array = []         # {"name", "rank", "pts"} oriented source -> mouth
var portage_runs: Array = []       # {"name", "pts"}
enum { SEA, LAND, RIVER, PORTAGE }
const RIVER_WEIGHT := 1.8
const PORTAGE_WEIGHT := 6.0
var astar := AStarGrid2D.new()


func load_data(path := "res://route_map/data/coast.json", with_grid := true) -> void:
	var d: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(path))
	cols = int(d["cols"])
	rows = int(d["rows"])
	if with_grid:
		var bits: String = d["land"]
		land.resize(cols * rows)
		kind.resize(cols * rows)
		var raw := bits.to_ascii_buffer()
		for i in raw.size():
			var k := raw[i] - 48
			kind[i] = k
			land[i] = 1 if k == LAND else 0
	for arr in d["detail"]:
		var ring := _unflat(arr)
		detail_rings.append(ring)
		detail_runs.append_array(_runs(ring))
	for arr in d["rough"]:
		for run in _runs(_unflat(arr)):
			rough_runs.append(_chaikin(_chaikin(run)))
	for arr in d.get("far", []):
		far_runs.append_array(_runs(_unflat(arr)))
	for arr in d.get("medium", []):
		medium_runs.append_array(_runs(_unflat(arr)))
	for level in d.get("water", []):
		var runs: Array = []
		for arr in level:
			runs.append(_unflat(arr))
		water_runs.append(runs)
	for arr in d.get("lakes", []):
		var ring := _unflat(arr)
		lake_rings.append(ring.duplicate())
		ring.append(ring[0])
		lake_runs.append(ring)
	for rv in d.get("rivers", []):
		river_runs.append({"name": rv["name"], "rank": rv["rank"], "pts": _unflat(rv["pts"])})
	for pg in d.get("portages", []):
		portage_runs.append({"name": pg["name"], "pts": _unflat(pg["pts"])})
	if not with_grid:
		return
	_build_flow()
	astar.region = Rect2i(0, 0, cols, rows)
	astar.cell_size = Vector2(Geo.CELL, Geo.CELL)
	astar.offset = Vector2(Geo.CELL, Geo.CELL) / 2.0
	astar.diagonal_mode = AStarGrid2D.DIAGONAL_MODE_ONLY_IF_NO_OBSTACLES
	astar.default_compute_heuristic = AStarGrid2D.HEURISTIC_OCTILE
	astar.default_estimate_heuristic = AStarGrid2D.HEURISTIC_OCTILE
	astar.update()
	for r in rows:
		for c in cols:
			match kind[r * cols + c]:
				LAND:
					astar.set_point_solid(Vector2i(c, r), true)
				RIVER:
					astar.set_point_weight_scale(Vector2i(c, r), RIVER_WEIGHT)
				PORTAGE:
					astar.set_point_weight_scale(Vector2i(c, r), PORTAGE_WEIGHT)


## Downstream direction for every river cell, from the oriented river lines.
func _build_flow() -> void:
	flow.clear()
	for rv in river_runs:
		var pts: PackedVector2Array = rv["pts"]
		for i in pts.size() - 1:
			var dir := (pts[i + 1] - pts[i]).normalized()
			var steps := int(pts[i].distance_to(pts[i + 1]) / (Geo.CELL * 0.5)) + 1
			for s in steps + 1:
				var c := cell_of(pts[i].lerp(pts[i + 1], float(s) / steps))
				var idx := c.y * cols + c.x
				if kind[idx] == RIVER:
					flow[idx] = dir


func kind_at(p: Vector2) -> int:
	if p.x < 0 or p.y < 0 or p.x >= cols * Geo.CELL or p.y >= rows * Geo.CELL:
		return LAND
	var c := cell_of(p)
	return kind[c.y * cols + c.x]


## Downstream direction at p (zero on lakes and the open sea).
func flow_at(p: Vector2) -> Vector2:
	var c := cell_of(p)
	return flow.get(c.y * cols + c.x, Vector2.ZERO)


## Period corrections: KapmanLandPatch polygons cut water through land or add land over sea.
## Also tells each patch which of its edges are not new coastline (so it doesn't draw them).
func apply_patches(patches: Array) -> void:
	for patch in patches:
		var poly: PackedVector2Array = patch.world_polygon()
		if poly.size() < 3:
			continue
		var cut: bool = patch.mode == KapmanLandPatch.Mode.CUT_WATER
		var skip: Array = []
		for i in poly.size():
			var mid := (poly[i] + poly[(i + 1) % poly.size()]) / 2.0
			if is_land(mid) != cut:
				skip.append(i)
		patch.skip_edges = skip
		var box := Rect2(poly[0], Vector2.ZERO)
		for p in poly:
			box = box.expand(p)
		var c0 := cell_of(box.position)
		var c1 := cell_of(box.end)
		for y in range(c0.y, c1.y + 1):
			for x in range(c0.x, c1.x + 1):
				var hit := false
				for o in [Vector2(0.5, 0.5), Vector2(0.15, 0.15), Vector2(0.85, 0.15), Vector2(0.15, 0.85), Vector2(0.85, 0.85)]:
					if Geometry2D.is_point_in_polygon((Vector2(x, y) + o) * Geo.CELL, poly):
						hit = true
						break
				if hit:
					land[y * cols + x] = 0 if cut else 1
					kind[y * cols + x] = SEA if cut else LAND
					astar.set_point_solid(Vector2i(x, y), not cut)


func map_size() -> Vector2:
	return Vector2(cols, rows) * Geo.CELL


func _unflat(arr: Array) -> PackedVector2Array:
	var pts := PackedVector2Array()
	for i in range(0, arr.size(), 2):
		pts.append(Vector2(arr[i], arr[i + 1]))
	return pts


## Splits a closed ring into open runs, dropping edges that lie on the map border (clip edges).
func _runs(ring: PackedVector2Array) -> Array:
	var out: Array = []
	var cur := PackedVector2Array()
	var n := ring.size()
	for i in n + 1:
		var a := ring[i % n]
		var b := ring[(i + 1) % n]
		if i == n:
			break
		if _on_border(a) and _on_border(b):
			if cur.size() >= 2:
				out.append(cur)
			cur = PackedVector2Array()
			continue
		if cur.is_empty():
			cur.append(a)
		cur.append(b)
	if cur.size() >= 2:
		out.append(cur)
	return out


## One round of Chaikin corner cutting: softens a polyline.
func _chaikin(pts: PackedVector2Array) -> PackedVector2Array:
	if pts.size() < 3:
		return pts
	var out := PackedVector2Array([pts[0]])
	for i in pts.size() - 1:
		out.append(pts[i].lerp(pts[i + 1], 0.25))
		out.append(pts[i].lerp(pts[i + 1], 0.75))
	out.append(pts[pts.size() - 1])
	return out


func _on_border(p: Vector2) -> bool:
	return p.x < 0.05 or p.y < 0.05 or p.x > Geo.size().x - 0.05 or p.y > Geo.size().y - 0.05


func cell_of(p: Vector2) -> Vector2i:
	return Vector2i(clampi(int(p.x / Geo.CELL), 0, cols - 1), clampi(int(p.y / Geo.CELL), 0, rows - 1))


func is_land(p: Vector2) -> bool:
	if p.x < 0 or p.y < 0 or p.x >= cols * Geo.CELL or p.y >= rows * Geo.CELL:
		return true
	var c := cell_of(p)
	return land[c.y * cols + c.x] == 1


## True if the straight line a->b stays on water. Points within `harbor` of any point in `ignore` don't count.
func line_clear(a: Vector2, b: Vector2, ignore: Array = [], harbor := 9.0) -> bool:
	var steps := int(a.distance_to(b) / (Geo.CELL * 0.5)) + 1
	for i in steps + 1:
		var p := a.lerp(b, float(i) / steps)
		if is_land(p) and not _near_any(p, ignore, harbor):
			return false
	return true


func _near_any(p: Vector2, pts: Array, r: float) -> bool:
	for q in pts:
		if p.distance_to(q) < r:
			return true
	return false


func _nearest_sea(c: Vector2i) -> Vector2i:
	if not astar.is_point_solid(c):
		return c
	for radius in range(1, 12):
		for dy in range(-radius, radius + 1):
			for dx in range(-radius, radius + 1):
				var q := c + Vector2i(dx, dy)
				if astar.region.has_point(q) and not astar.is_point_solid(q):
					return q
	return c


## A fairly direct sea route from a to b: straight if clear, otherwise bent around land.
func find_route(a: Vector2, b: Vector2) -> PackedVector2Array:
	var ends := [a, b]
	if line_clear(a, b, ends):
		return PackedVector2Array([a, b])
	var ca := _nearest_sea(cell_of(a))
	var cb := _nearest_sea(cell_of(b))
	var path := astar.get_point_path(ca, cb)
	if path.size() < 2:
		return PackedVector2Array([a, b])
	path[0] = a
	path[path.size() - 1] = b
	# String-pull: keep only the points needed to stay on water.
	var out := PackedVector2Array([a])
	var i := 0
	while i < path.size() - 1:
		var j := path.size() - 1
		while j > i + 1 and not line_clear(path[i], path[j], ends):
			j -= 1
		out.append(path[j])
		i = j
	return out
