extends SceneTree
## One-off data build: Natural Earth 10m land (public domain) -> route_map/data/coast.json
## Run: godot --headless --path <project> --script res://route_map/tools/build_coast.gd
## Output: detailed coastline, rough (fog) coastline, and a land/sea grid for route finding,
## all in map units (see route_map/geo.gd for the projection).

const Geo := preload("res://route_map/geo.gd")
const SOURCE := "res://design/source_data/ne_10m_land.geojson"
const LAKES := "res://design/source_data/ne_10m_lakes.geojson"
const RIVERS := "res://design/source_data/ne_10m_rivers_lake_centerlines.geojson"
const RIVERS_EU := "res://design/source_data/ne_10m_rivers_europe.geojson"
## Rivers you can sail or row up in the Viking age (Natural Earth names, as spelled in the data).
const NAVIGABLE := ["Rhine", "Rhein", "Rhin", "Nederrijn", "Lek", "Waal", "IJssel", "Maas", "Schelde", "Seine", "Loire", "Garonne",
	"Thames", "Trent", "Severn", "Shannon", "Elbe", "Weser", "Ems", "Oder", "Vistula", "Neman", "Daugava", "Narva", "Neva",
	"Svir", "Volkhov", "Msta", "Shelon", "Kasplya", "Dnepre", "Dnepr", "Dnipro", "Desna", "Pripyat", "Don", "Volga",
	"Oka", "Kama", "Sheksna", "Danube", "Donau", "Dniester", "Southern Bug", "Po", "Rhône", "Mosel", "Main", "Göta",
	"Glomma", "Severnaya Dvina", "Sukhona", "Bratul Sulina", "Bratul Chillia", "Bratul Sfintu Gheorghe", "Sava", "Tisa", "Dalälven",
	"Tvertsa"]
## Hand-traced where Natural Earth has nothing: [name, [[lat, lon] from source to mouth]]
const EXTRA_RIVERS := [
	["Lovat", [[55.85, 30.25], [56.34, 30.53], [56.75, 30.95], [57.15, 31.18], [57.6, 31.3], [58.05, 31.35], [58.25, 31.4]]],
]
## Portages: ships are hauled overland here. [name, [[lat, lon], ...]]
const PORTAGES := [
	["Lovat–Dnieper portage", [[55.85, 30.25], [55.5, 30.7], [55.15, 31.3], [54.8, 31.85]]],
	["Volga–Don portage", [[48.72, 44.5], [48.66, 44.0], [48.6, 43.55]]],
	["Hollingstedt–Hedeby portage", [[54.46, 9.33], [54.49, 9.57]]],
	["Vyshny Volochyok portage (Msta–Tvertsa)", [[57.66, 34.2], [57.6, 34.4], [57.58, 34.62]]],
]
const MIN_LAKE_AREA := 450.0
const OUT := "res://route_map/data/coast.json"
const DIST_OUT := "res://route_map/data/sea_distance.png"
## sea_distance.png: distance from land in grid cells x DIST_SCALE (0 = land, 255 = far out at sea).
const DIST_SCALE := 3.0
const DETAIL_TOLERANCE := 0.55     # before smoothing
const MEDIUM_TOLERANCE := 1.8      # zoomed-out coast
const ROUGH_TOLERANCE := 5.0
const MIN_DETAIL_AREA := 5.0        # drop islets smaller than ~6 km²
const MIN_MEDIUM_AREA := 40.0
const FAR_TOLERANCE := 7.0         # whole-map view
const MIN_FAR_AREA := 400.0
const MIN_ROUGH_AREA := 120.0
const WATERLINES := [2.2, 4.6, 7.4]  # engraved water-lining offsets out to sea, map units


func _initialize() -> void:
	var t0 := Time.get_ticks_msec()
	var json: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(SOURCE))
	var box := PackedVector2Array([Vector2(0, 0), Vector2(Geo.size().x, 0), Geo.size(), Vector2(0, Geo.size().y)])
	var detail: Array = []
	var detail_polys: Array = []
	var medium: Array = []
	var far: Array = []
	var rough: Array = []
	var land_polys: Array = []
	for f in json["features"]:
		var g: Dictionary = f["geometry"]
		var polys: Array = g["coordinates"] if g["type"] == "MultiPolygon" else [g["coordinates"]]
		for poly in polys:
			var ring: Array = poly[0]  # outer ring only; holes are lakes
			var pts := PackedVector2Array()
			var inside := false
			for c in ring:
				if c[0] > -60.0 and c[0] < 65.0 and c[1] > 35.0 and c[1] < 80.0:
					inside = true
			if not inside:
				continue
			for c in ring:
				pts.append(Geo.project(c[1], c[0]))
			if not inside:
				continue
			var outer_cw := Geometry2D.is_polygon_clockwise(pts)
			for clipped in Geometry2D.intersect_polygons(pts, box):
				if Geometry2D.is_polygon_clockwise(clipped) != outer_cw:
					continue  # a hole produced by clipping
				land_polys.append(clipped)
				var d := _simplify(clipped, DETAIL_TOLERANCE)
				if d.size() >= 3 and absf(_area(d)) >= MIN_DETAIL_AREA:
					d = _simplify(_chaikin(_chaikin(d)), 0.08)
					detail.append(_flat(d))
					detail_polys.append(d)
				var md := _simplify(clipped, MEDIUM_TOLERANCE)
				if md.size() >= 3 and absf(_area(md)) >= MIN_MEDIUM_AREA:
					medium.append(_flat(_simplify(_chaikin(_chaikin(md)), 0.2)))
				var fd := _simplify(clipped, FAR_TOLERANCE)
				if fd.size() >= 3 and absf(_area(fd)) >= MIN_FAR_AREA:
					far.append(_flat(_simplify(_chaikin(_chaikin(_chaikin(fd))), 0.3)))
				var r := _simplify(clipped, ROUGH_TOLERANCE)
				if r.size() >= 3 and absf(_area(r)) >= MIN_ROUGH_AREA:
					rough.append(_flat(r))
	# Big lakes on the river routes (Ladoga, Onega, Ilmen, Mälaren...): drawn, not sailed.
	var lakes_json: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(LAKES))
	var lakes: Array = []
	var reservoirs := 0
	for f in lakes_json["features"]:
		if str(f["properties"].get("featurecla", "")).to_lower().contains("reservoir"):
			reservoirs += 1
			continue  # modern dams: the Volga and Dnieper reservoirs did not exist in 790
		var g: Dictionary = f["geometry"]
		var polys: Array = g["coordinates"] if g["type"] == "MultiPolygon" else [g["coordinates"]]
		for poly in polys:
			var ring: Array = poly[0]
			var c0: Array = ring[0]
			if c0[0] < -60.0 or c0[0] > 65.0 or c0[1] < 35.0 or c0[1] > 80.0:
				continue
			var pts := PackedVector2Array()
			for c in ring:
				pts.append(Geo.project(c[1], c[0]))
			for clipped in Geometry2D.intersect_polygons(pts, box):
				var d := _simplify(clipped, DETAIL_TOLERANCE)
				if d.size() >= 3 and absf(_area(d)) >= MIN_LAKE_AREA:
					lakes.append(_flat(d))
	var water := _waterlines(detail_polys, box)
	print("skipped %d reservoirs" % reservoirs)
	var mask := _rasterize(land_polys)
	var nav := _rivers_and_portages(mask, lakes, box)
	var out := {
		"size": [Geo.size().x, Geo.size().y], "cell": Geo.CELL,
		"cols": mask["cols"], "rows": mask["rows"], "land": mask["bits"],
		"detail": detail, "medium": medium, "far": far, "rough": rough, "lakes": lakes, "water": water,
		"rivers": nav["rivers"], "portages": nav["portages"],
	}
	_save_distance(mask)
	var fa := FileAccess.open(OUT, FileAccess.WRITE)
	fa.store_string(JSON.stringify(out))
	fa.close()
	var pts_d := 0
	for d in detail:
		pts_d += d.size() / 2
	var pts_w := 0
	for level in water:
		for run in level:
			pts_w += run.size() / 2
	print("coast: %d detail polys (%d pts), %d medium, %d far, %d rough, %d lakes, %d water-line pts, grid %dx%d, map %s, %d ms" % [detail.size(), pts_d, medium.size(), far.size(), rough.size(), lakes.size(), pts_w, mask["cols"], mask["rows"], Geo.size(), Time.get_ticks_msec() - t0])
	quit()


func _flat(pts: PackedVector2Array) -> Array:
	var a: Array = []
	for p in pts:
		a.append(snappedf(p.x, 0.01))
		a.append(snappedf(p.y, 0.01))
	return a


func _area(pts: PackedVector2Array) -> float:
	var s := 0.0
	for i in pts.size():
		var a := pts[i]
		var b := pts[(i + 1) % pts.size()]
		s += a.x * b.y - b.x * a.y
	return s / 2.0


## Douglas-Peucker on a closed ring (iterative).
func _simplify(pts: PackedVector2Array, tol: float) -> PackedVector2Array:
	var n := pts.size()
	if n < 4:
		return pts
	var keep := PackedByteArray()
	keep.resize(n)
	keep[0] = 1
	# split the ring at the point farthest from pts[0]
	var far := 0
	var far_d := -1.0
	for i in n:
		var d := pts[0].distance_squared_to(pts[i])
		if d > far_d:
			far_d = d
			far = i
	keep[far] = 1
	var stack: Array = [[0, far], [far, n]]
	while not stack.is_empty():
		var seg: Array = stack.pop_back()
		var a: int = seg[0]
		var b: int = seg[1]
		if b - a < 2:
			continue
		var pa := pts[a]
		var pb := pts[b % n]
		var best := -1
		var best_d := tol
		for i in range(a + 1, b):
			var q := Geometry2D.get_closest_point_to_segment(pts[i], pa, pb)
			var d := q.distance_to(pts[i])
			if d > best_d:
				best_d = d
				best = i
		if best >= 0:
			keep[best] = 1
			stack.append([a, best])
			stack.append([best, b])
	var out := PackedVector2Array()
	for i in n:
		if keep[i]:
			out.append(pts[i])
	return out


## Scanline fill of land polygons into a grid; returns a string of '0'/'1' per cell (row-major).
func _rasterize(polys: Array) -> Dictionary:
	var cols := int(ceil(Geo.size().x / Geo.CELL))
	var rows := int(ceil(Geo.size().y / Geo.CELL))
	var grid := PackedByteArray()
	grid.resize(cols * rows)
	for poly in polys:
		var pts: PackedVector2Array = poly
		var crossings := {}  # row -> Array of x
		var n := pts.size()
		for i in n:
			var a := pts[i]
			var b := pts[(i + 1) % n]
			if a.y == b.y:
				continue   # exact only: an approx test drops nearly-flat edges and floods a whole row
			var y0 := minf(a.y, b.y)
			var y1 := maxf(a.y, b.y)
			var r0 := maxi(0, int(ceil(y0 / Geo.CELL - 0.5)))
			var r1 := mini(rows - 1, int(floor(y1 / Geo.CELL - 0.5)))
			for r in range(r0, r1 + 1):
				var cy := (r + 0.5) * Geo.CELL
				if cy < y0 or cy >= y1:
					continue
				var x := a.x + (cy - a.y) * (b.x - a.x) / (b.y - a.y)
				if not crossings.has(r):
					crossings[r] = []
				crossings[r].append(x)
		for r in crossings:
			var xs: Array = crossings[r]
			xs.sort()
			for k in range(0, xs.size() - 1, 2):
				var c0 := maxi(0, int(ceil(xs[k] / Geo.CELL - 0.5)))
				var c1 := mini(cols - 1, int(floor(xs[k + 1] / Geo.CELL - 0.5)))
				for c in range(c0, c1 + 1):
					grid[r * cols + c] = 1
	for i in grid.size():
		grid[i] = 49 if grid[i] else 48
	return {"cols": cols, "rows": rows, "bits": grid.get_string_from_ascii()}


## Rivers, sailable lakes and portages go into the grid: '2' = river/lake water, '3' = portage.
## Rivers are oriented so their last point is downstream (the mouth).
func _rivers_and_portages(mask: Dictionary, lakes: Array, box: PackedVector2Array) -> Dictionary:
	var cols: int = mask["cols"]
	var rows: int = mask["rows"]
	var grid: PackedByteArray = (mask["bits"] as String).to_ascii_buffer()
	_fill_sea_pockets(grid, cols, rows, 80)
	# Sailable lakes
	for flat in lakes:
		var poly := PackedVector2Array()
		for i in range(0, flat.size(), 2):
			poly.append(Vector2(flat[i], flat[i + 1]))
		var bb := Rect2(poly[0], Vector2.ZERO)
		for q in poly:
			bb = bb.expand(q)
		for y in range(maxi(0, int(bb.position.y / Geo.CELL)), mini(rows, int(bb.end.y / Geo.CELL) + 1)):
			for x in range(maxi(0, int(bb.position.x / Geo.CELL)), mini(cols, int(bb.end.x / Geo.CELL) + 1)):
				if grid[y * cols + x] == 49 and Geometry2D.is_point_in_polygon((Vector2(x, y) + Vector2(0.5, 0.5)) * Geo.CELL, poly):
					grid[y * cols + x] = 50
	# Rivers
	var runs: Array = []
	for src in [RIVERS, RIVERS_EU]:
		var js: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(src))
		for feat in js["features"]:
			var props: Dictionary = feat["properties"]
			var nm := str(props.get("name", ""))
			if not nm in NAVIGABLE:
				continue
			var g = feat["geometry"]
			if g == null:
				continue
			var lines: Array = g["coordinates"] if g["type"] == "MultiLineString" else [g["coordinates"]]
			for line in lines:
				var pts := PackedVector2Array()
				for c in line:
					pts.append(Geo.project(c[1], c[0]))
				for piece in Geometry2D.intersect_polyline_with_polygon(pts, box):
					if piece.size() >= 2:
						runs.append({"name": nm, "rank": float(props.get("scalerank", 8)), "pts": _simplify_open(piece, 0.4)})
	for er in EXTRA_RIVERS:
		var pts2 := PackedVector2Array()
		for ll in er[1]:
			pts2.append(Geo.project(ll[0], ll[1]))
		runs.append({"name": er[0], "rank": 8.0, "pts": pts2})
	for run in runs:
		_raster_line(grid, cols, rows, run["pts"], 49, 50)
	# Orient each river: the end with open sea (or other water) next to it is the mouth.
	for run in runs:
		var p: PackedVector2Array = run["pts"]
		if _mouth_score(grid, cols, rows, p[0]) > _mouth_score(grid, cols, rows, p[p.size() - 1]):
			p.reverse()
			run["pts"] = p
	# Join every river end that lies near the coast to the nearest open water (estuaries are often a
	# cell or two short in the data at this scale).
	for run in runs:
		var p: PackedVector2Array = run["pts"]
		for end in [p[0], p[p.size() - 1]]:
			var sea := _nearest_sea_cell(grid, cols, rows, end, 10)
			if sea.x >= 0:
				_raster_line(grid, cols, rows, PackedVector2Array([end, (Vector2(sea) + Vector2(0.5, 0.5)) * Geo.CELL]), 49, 50)
	var portages: Array = []
	for pg in PORTAGES:
		var pts3 := PackedVector2Array()
		for ll in pg[1]:
			pts3.append(Geo.project(ll[0], ll[1]))
		_raster_line(grid, cols, rows, pts3, 49, 51)
		portages.append({"name": pg[0], "pts": _flat(pts3)})
	mask["bits"] = grid.get_string_from_ascii()
	var out_runs: Array = []
	for run in runs:
		out_runs.append({"name": run["name"], "rank": run["rank"], "pts": _flat(run["pts"])})
	print("rivers: %d runs, %d portages" % [out_runs.size(), portages.size()])
	return {"rivers": out_runs, "portages": portages}


## Tiny enclosed bits of "sea" (estuary pockets, lagoons cut off at this scale) become land, so
## rivers and ports always connect to real open water.
func _fill_sea_pockets(grid: PackedByteArray, cols: int, rows: int, min_cells: int) -> void:
	var seen := PackedByteArray()
	seen.resize(cols * rows)
	var filled := 0
	for start in cols * rows:
		if grid[start] != 48 or seen[start]:
			continue
		var comp := PackedInt32Array([start])
		seen[start] = 1
		var head := 0
		while head < comp.size():
			var i := comp[head]
			head += 1
			var x := i % cols
			var y := i / cols
			for n in [i - 1 if x > 0 else -1, i + 1 if x < cols - 1 else -1, i - cols if y > 0 else -1, i + cols if y < rows - 1 else -1]:
				if n >= 0 and not seen[n] and grid[n] == 48:
					seen[n] = 1
					comp.append(n)
		if comp.size() < min_cells:
			for i in comp:
				grid[i] = 49
			filled += 1
	print("filled %d tiny sea pockets" % filled)


## Nearest open-sea cell within `radius` cells of p, or (-1, -1).
func _nearest_sea_cell(grid: PackedByteArray, cols: int, rows: int, p: Vector2, radius: int) -> Vector2i:
	var c := Vector2i(int(p.x / Geo.CELL), int(p.y / Geo.CELL))
	var best := Vector2i(-1, -1)
	var best_d := INF
	for dy in range(-radius, radius + 1):
		for dx in range(-radius, radius + 1):
			var q := c + Vector2i(dx, dy)
			if q.x < 0 or q.y < 0 or q.x >= cols or q.y >= rows:
				continue
			if grid[q.y * cols + q.x] == 48:
				var d := Vector2(dx, dy).length()
				if d < best_d:
					best_d = d
					best = q
	return best


## How much a river end looks like a mouth: open sea close by counts most, other water a little.
func _mouth_score(grid: PackedByteArray, cols: int, rows: int, p: Vector2) -> int:
	var c := Vector2i(int(p.x / Geo.CELL), int(p.y / Geo.CELL))
	var score := 0
	for dy in range(-3, 4):
		for dx in range(-3, 4):
			var q := c + Vector2i(dx, dy)
			if q.x < 0 or q.y < 0 or q.x >= cols or q.y >= rows:
				continue
			var v := grid[q.y * cols + q.x]
			if v == 48:
				score += 10
			elif v == 50:
				score += 1
	return score


## Marks the cells under a polyline (4-connected, so ships can follow it), changing `from` to `to`.
func _raster_line(grid: PackedByteArray, cols: int, rows: int, pts: PackedVector2Array, from: int, to: int) -> void:
	for i in pts.size() - 1:
		var a := pts[i] / Geo.CELL
		var b := pts[i + 1] / Geo.CELL
		var steps := int(maxf(absf(b.x - a.x), absf(b.y - a.y)) * 2.0) + 1
		var prev := Vector2i(int(a.x), int(a.y))
		for s in steps + 1:
			var q := a.lerp(b, float(s) / steps)
			var c := Vector2i(int(q.x), int(q.y))
			if c.x != prev.x and c.y != prev.y:
				_set_cell(grid, cols, rows, Vector2i(c.x, prev.y), from, to)   # no diagonal gaps
			_set_cell(grid, cols, rows, c, from, to)
			prev = c


func _set_cell(grid: PackedByteArray, cols: int, rows: int, c: Vector2i, from: int, to: int) -> void:
	if c.x < 0 or c.y < 0 or c.x >= cols or c.y >= rows:
		return
	if grid[c.y * cols + c.x] == from:
		grid[c.y * cols + c.x] = to


## Chamfer distance transform over the land grid, saved as an 8-bit image for the sea shader.
func _save_distance(mask: Dictionary) -> void:
	var cols: int = mask["cols"]
	var rows: int = mask["rows"]
	var bits: String = mask["bits"]
	var raw := bits.to_ascii_buffer()
	var d := PackedFloat32Array()
	d.resize(cols * rows)
	for i in raw.size():
		d[i] = 0.0 if raw[i] != 48 else 1e9   # rivers, lakes and portages count as land for the sea
	for y in rows:
		for x in cols:
			var i := y * cols + x
			var v := d[i]
			if v == 0.0:
				continue
			if x > 0: v = minf(v, d[i - 1] + 1.0)
			if y > 0:
				v = minf(v, d[i - cols] + 1.0)
				if x > 0: v = minf(v, d[i - cols - 1] + 1.4142)
				if x < cols - 1: v = minf(v, d[i - cols + 1] + 1.4142)
			d[i] = v
	for y in range(rows - 1, -1, -1):
		for x in range(cols - 1, -1, -1):
			var i := y * cols + x
			var v := d[i]
			if v == 0.0:
				continue
			if x < cols - 1: v = minf(v, d[i + 1] + 1.0)
			if y < rows - 1:
				v = minf(v, d[i + cols] + 1.0)
				if x < cols - 1: v = minf(v, d[i + cols + 1] + 1.4142)
				if x > 0: v = minf(v, d[i + cols - 1] + 1.4142)
			d[i] = v
	var img := Image.create(cols, rows, false, Image.FORMAT_L8)
	for y in rows:
		for x in cols:
			var v := clampf(d[y * cols + x] * DIST_SCALE, 0.0, 255.0) / 255.0
			img.set_pixel(x, y, Color(v, v, v))
	img.save_png(DIST_OUT)


func _on_border(p: Vector2) -> bool:
	var s := Geo.size()
	return p.x < 0.05 or p.y < 0.05 or p.x > s.x - 0.05 or p.y > s.y - 0.05


## One round of Chaikin corner-cutting on a closed ring. Edges along the map border stay put.
func _chaikin(pts: PackedVector2Array) -> PackedVector2Array:
	var out := PackedVector2Array()
	var n := pts.size()
	for i in n:
		var a := pts[i]
		var b := pts[(i + 1) % n]
		if _on_border(a) and _on_border(b):
			out.append(a)
			out.append(b)
		else:
			out.append(a.lerp(b, 0.25))
			out.append(a.lerp(b, 0.75))
	var clean := PackedVector2Array()
	for p in out:
		if clean.is_empty() or not clean[clean.size() - 1].is_equal_approx(p):
			clean.append(p)
	return clean


## Engraved water-lining: rings offset out to sea, cut where they run into other land or off the map.
## Returns [level][run] of flat [x, y, ...] polylines.
func _waterlines(polys: Array, box: PackedVector2Array) -> Array:
	var boxes: Array = []
	for p in polys:
		var b := Rect2(p[0], Vector2.ZERO)
		for q in p:
			b = b.expand(q)
		boxes.append(b)
	var levels: Array = []
	for off in WATERLINES:
		var runs: Array = []
		for i in polys.size():
			if absf(_area(polys[i])) < 30.0 and off > 3.0:
				continue  # small islands get one line only
			for ring in Geometry2D.offset_polygon(polys[i], off, Geometry2D.JOIN_ROUND):
				var closed: PackedVector2Array = ring.duplicate()
				closed.append(ring[0])
				var pieces: Array = Geometry2D.intersect_polyline_with_polygon(closed, box)
				var rb := Rect2(closed[0], Vector2.ZERO)
				for q in closed:
					rb = rb.expand(q)
				for j in polys.size():
					if j == i or not boxes[j].intersects(rb):
						continue
					var next: Array = []
					for piece in pieces:
						next.append_array(Geometry2D.clip_polyline_with_polygon(piece, polys[j]))
					pieces = next
				for piece in pieces:
					var s := _simplify_open(piece, 0.15)
					if s.size() >= 2 and _length(s) > 4.0:
						runs.append(_flat(s))
		levels.append(runs)
	return levels


func _length(pts: PackedVector2Array) -> float:
	var l := 0.0
	for i in pts.size() - 1:
		l += pts[i].distance_to(pts[i + 1])
	return l


## Douglas-Peucker on an open polyline.
func _simplify_open(pts: PackedVector2Array, tol: float) -> PackedVector2Array:
	if pts.size() < 3:
		return pts
	var keep := PackedByteArray()
	keep.resize(pts.size())
	keep[0] = 1
	keep[pts.size() - 1] = 1
	var stack: Array = [[0, pts.size() - 1]]
	while not stack.is_empty():
		var seg: Array = stack.pop_back()
		var a: int = seg[0]
		var b: int = seg[1]
		var best := -1
		var best_d := tol
		for i in range(a + 1, b):
			var d := Geometry2D.get_closest_point_to_segment(pts[i], pts[a], pts[b]).distance_to(pts[i])
			if d > best_d:
				best_d = d
				best = i
		if best >= 0:
			keep[best] = 1
			stack.append([a, best])
			stack.append([best, b])
	var out := PackedVector2Array()
	for i in pts.size():
		if keep[i]:
			out.append(pts[i])
	return out
