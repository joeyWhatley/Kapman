extends SceneTree
## Bakes the high-detail terrain for one region of the 3D world (the Dorestad vertical slice).
## Source: AWS Terrain Tiles, terrarium encoding, zoom 11 (~47 m) in design/source_data/terrarium_z11/.
##   Credits: SRTM (NASA), ETOPO1 (NOAA), EU-DEM (Copernicus), and others; see route_map/world3d/CREDITS.md
## Output, in map projection (see geo.gd):
##   route_map/data/slice_height.exr   heights in metres, half float, PX_M metres per pixel
##   route_map/data/slice.json         region rect (map units), pixel size, river water profiles
## The chart's coastline is the authority: land it marks stays above the sea, sea stays below it.
##   godot --headless --path <project> --script res://route_map/tools/bake_slice.gd

const Geo := preload("res://route_map/geo.gd")
const MapData := preload("res://route_map/map_data.gd")
const TILES := "res://design/source_data/terrarium_z11/"
const Z := 11
const LAT := Vector2(51.35, 53.65)
const LON := Vector2(3.35, 7.45)
const PX_M := 50.0                                   # metres per pixel
const M_PER_UNIT := 1112.0                           # metres per map unit (geo.gd: 111.2 km per 100 units)
const OUT_H := "res://route_map/data/slice_height.exr"
const OUT_J := "res://route_map/data/slice.json"
## Rivers of 800 AD missing from the modern data, source to mouth: [name, rank, [[lat, lon], ...]].
## The Old Rhine was the Rhine's main mouth then, past Dorestad and Utrecht to the sea at Katwijk.
const EXTRA_RIVERS := [
	["Oude Rijn", 4, [[51.972, 5.345], [52.02, 5.24], [52.075, 5.165], [52.09, 5.12], [52.083, 5.0], [52.085, 4.88],
		[52.115, 4.76], [52.13, 4.66], [52.155, 4.55], [52.16, 4.49], [52.185, 4.44], [52.205, 4.395]]],
	["Vecht", 6, [[52.09, 5.12], [52.14, 5.04], [52.175, 5.0], [52.24, 5.02], [52.29, 5.04], [52.335, 5.07]]],
]

var _mosaic := PackedFloat32Array()
var _mw := 0
var _mh := 0
var _tx0 := 0
var _ty0 := 0


func _initialize() -> void:
	var t0 := Time.get_ticks_msec()
	_load_tiles()
	print("mosaic %dx%d in %d ms" % [_mw, _mh, Time.get_ticks_msec() - t0])
	var rect := _region()
	var step := PX_M / M_PER_UNIT
	var w := int(rect.size.x / step)
	var h := int(rect.size.y / step)
	print("slice rect %s -> %dx%d px" % [rect, w, h])
	var hm := PackedFloat32Array()
	hm.resize(w * h)
	for y in h:
		for x in w:
			var ll := Geo.unproject(rect.position + (Vector2(x, y) + Vector2(0.5, 0.5)) * step)
			hm[y * w + x] = _sample(ll.x, ll.y)
	print("sampled in %d ms" % (Time.get_ticks_msec() - t0))
	# Land/sea from the chart's coastline.
	var md := MapData.new()
	md.load_data("res://route_map/data/coast.json", false)
	var land := PackedByteArray()
	land.resize(w * h)
	var rings: Array = []
	for r in md.detail_rings:
		if _bbox(r).intersects(rect):
			rings.append(r)
	_fill(land, w, h, rect, step, rings, 1)
	var lakes: Array = []
	for r in md.lake_rings:
		if _bbox(r).intersects(rect):
			lakes.append(r)
	_fill(land, w, h, rect, step, lakes, 0)
	var to_sea := _chamfer(land, w, h, 1)     # land pixels: distance to the sea (thirds of a pixel)
	var to_land := _chamfer(land, w, h, 0)    # sea pixels: distance to land
	_save_cover(to_sea, w, h)
	print("cover in %d ms" % (Time.get_ticks_msec() - t0))
	# The coast as a smooth slope through zero at the charted shoreline (signed distance), so the
	# waterline interpolates cleanly between pixels instead of stepping along the pixel grid.
	# Near the shore, the exact distance to the charted coastline (not the pixel staircase).
	var exact := _coast_distance(rings + lakes, w, h, rect, step, 8)
	for i in hm.size():
		if land[i] == 1:
			var sd := (to_sea[i] / 3.0 - 0.5) * PX_M
			if exact[i] >= 0.0:
				sd = exact[i]
			hm[i] = maxf(hm[i], minf(sd * 0.02, 0.7))   # polders below sea level were bog and meadow in 800
			hm[i] = minf(hm[i], sd * 0.12 + 0.4)        # no cliff right at the waterline
		else:
			var sd := -(to_land[i] / 3.0 - 0.5) * PX_M
			if exact[i] >= 0.0:
				sd = -exact[i]
			hm[i] = clampf(minf(hm[i], sd * 0.03), maxf(sd * 0.25, -90.0), -0.02)
	# Water surface: 0 (the sea) everywhere, raised along rivers to their own level.
	var water := PackedFloat32Array()
	water.resize(w * h)
	# Rivers: channels cut below a water level that only falls toward the mouth, and every mouth
	# joined to open water.
	var runs: Array = []
	for rv in md.river_runs:
		runs.append({"name": rv["name"], "rank": float(rv["rank"]), "pts": rv["pts"]})
	for ex in EXTRA_RIVERS:
		var pts := PackedVector2Array()
		for ll in ex[2]:
			pts.append(Geo.project(ll[0], ll[1]))
		runs.append({"name": ex[0], "rank": float(ex[1]), "pts": pts})
	var rivers: Array = []
	for rv in runs:
		var pts: PackedVector2Array = rv["pts"]
		var inside := false
		for p in pts:
			if rect.has_point(p):
				inside = true
				break
		if not inside:
			continue
		pts = _join_mouth(pts, land, w, h, rect, step)
		var dense := PackedVector2Array()
		for i in pts.size() - 1:
			var n := int(pts[i].distance_to(pts[i + 1]) * M_PER_UNIT / 40.0) + 1
			for k in n:
				dense.append(pts[i].lerp(pts[i + 1], float(k) / n))
		dense.append(pts[pts.size() - 1])
		var rank: float = rv["rank"]
		var half := 130.0 if rank <= 4.0 else (70.0 if rank <= 6.0 else 40.0)
		var levels := PackedFloat32Array()
		levels.resize(dense.size())
		var level := INF
		for i in dense.size():
			level = minf(level, _hm_at(hm, w, h, rect, step, dense[i]))
			levels[i] = maxf(level, 0.0)
		# ease down to the sea over the last stretch (about 0.4 m per km)
		var from_mouth := 0.0
		for i in range(dense.size() - 1, -1, -1):
			if i < dense.size() - 1:
				from_mouth += dense[i].distance_to(dense[i + 1]) * M_PER_UNIT
			levels[i] = minf(levels[i], 0.02 + from_mouth * 0.0004)
		_carve(hm, water, w, h, rect, step, dense, levels, half)
		var flat := []
		for i in dense.size():
			flat.append_array([snappedf(dense[i].x, 0.0001), snappedf(dense[i].y, 0.0001), snappedf(levels[i], 0.01)])
		rivers.append({"name": rv["name"], "half_m": half, "pts": flat})
	print("carved %d rivers in %d ms" % [rivers.size(), Time.get_ticks_msec() - t0])
	var rg := PackedFloat32Array()
	rg.resize(w * h * 2)
	for i in w * h:
		rg[i * 2] = hm[i]
		rg[i * 2 + 1] = water[i]
	var img := Image.create_from_data(w, h, false, Image.FORMAT_RGF, rg.to_byte_array())
	img.convert(Image.FORMAT_RGH)
	img.save_exr(ProjectSettings.globalize_path(OUT_H), false)
	var info := {"rect": [rect.position.x, rect.position.y, rect.size.x, rect.size.y], "px": [w, h],
		"px_m": PX_M, "m_per_unit": M_PER_UNIT, "rivers": rivers}
	var f := FileAccess.open(OUT_J, FileAccess.WRITE)
	f.store_string(JSON.stringify(info))
	f.close()
	var lo := INF
	var hi := -INF
	for v in hm:
		lo = minf(lo, v)
		hi = maxf(hi, v)
	print("slice %dx%d, %.1f..%.1f m, %d ms" % [w, h, lo, hi, Time.get_ticks_msec() - t0])
	quit()


func _load_tiles() -> void:
	var xs: Array = []
	var ys: Array = []
	for fn in DirAccess.get_files_at(TILES):
		if fn.ends_with(".png"):
			var parts := fn.get_basename().split("_")
			xs.append(int(parts[0]))
			ys.append(int(parts[1]))
	_tx0 = xs.min()
	_ty0 = ys.min()
	_mw = (xs.max() - _tx0 + 1) * 256
	_mh = (ys.max() - _ty0 + 1) * 256
	_mosaic.resize(_mw * _mh)
	for fn in DirAccess.get_files_at(TILES):
		if not fn.ends_with(".png"):
			continue
		var parts := fn.get_basename().split("_")
		var ox := (int(parts[0]) - _tx0) * 256
		var oy := (int(parts[1]) - _ty0) * 256
		var img := Image.load_from_file(ProjectSettings.globalize_path(TILES + fn))
		img.convert(Image.FORMAT_RGB8)
		var d := img.get_data()
		for py in 256:
			for px in 256:
				var i := (py * 256 + px) * 3
				_mosaic[(oy + py) * _mw + ox + px] = d[i] * 256.0 + d[i + 1] + d[i + 2] / 256.0 - 32768.0


## Height (m) at lat/lon from the Web-Mercator tile mosaic, bilinear.
func _sample(lat: float, lon: float) -> float:
	var n := float(1 << Z) * 256.0
	var r := deg_to_rad(lat)
	var fx := (lon + 180.0) / 360.0 * n - _tx0 * 256.0 - 0.5
	var fy := (1.0 - log(tan(r) + 1.0 / cos(r)) / PI) / 2.0 * n - _ty0 * 256.0 - 0.5
	fx = clampf(fx, 0.0, _mw - 1.001)
	fy = clampf(fy, 0.0, _mh - 1.001)
	var x := int(fx)
	var y := int(fy)
	var tx := fx - x
	var ty := fy - y
	var a := _mosaic[y * _mw + x]
	var b := _mosaic[y * _mw + x + 1]
	var c := _mosaic[(y + 1) * _mw + x]
	var e := _mosaic[(y + 1) * _mw + x + 1]
	return lerpf(lerpf(a, b, tx), lerpf(c, e, tx), ty)


## The largest map-space rectangle inside the lat/lon box (the projection bends its edges).
func _region() -> Rect2:
	var x0 := -INF
	var x1 := INF
	var y0 := -INF
	var y1 := INF
	for i in 21:
		var lat := lerpf(LAT.x, LAT.y, i / 20.0)
		var lon := lerpf(LON.x, LON.y, i / 20.0)
		x0 = maxf(x0, Geo.project(lat, LON.x).x)
		x1 = minf(x1, Geo.project(lat, LON.y).x)
		y0 = maxf(y0, Geo.project(LAT.y, lon).y)
		y1 = minf(y1, Geo.project(LAT.x, lon).y)
	return Rect2(x0, y0, x1 - x0, y1 - y0)


static func _bbox(r: PackedVector2Array) -> Rect2:
	var b := Rect2(r[0], Vector2.ZERO)
	for p in r:
		b = b.expand(p)
	return b


## Scanline fill of polygons (map units) into the mask with `value`.
func _fill(mask: PackedByteArray, w: int, h: int, rect: Rect2, step: float, rings: Array, value: int) -> void:
	for ring in rings:
		var pts: PackedVector2Array = ring
		var rows := {}
		var n := pts.size()
		for i in n:
			var a := (pts[i] - rect.position) / step
			var b := (pts[(i + 1) % n] - rect.position) / step
			if a.y == b.y:
				continue
			var y0 := minf(a.y, b.y)
			var y1 := maxf(a.y, b.y)
			for r in range(maxi(0, int(ceil(y0 - 0.5))), mini(h - 1, int(floor(y1 - 0.5))) + 1):
				var cy := r + 0.5
				if cy < y0 or cy >= y1:
					continue
				if not rows.has(r):
					rows[r] = []
				rows[r].append(a.x + (cy - a.y) * (b.x - a.x) / (b.y - a.y))
		for r in rows:
			var xs: Array = rows[r]
			xs.sort()
			for k in range(0, xs.size() - 1, 2):
				for c in range(maxi(0, int(ceil(xs[k] - 0.5))), mini(w - 1, int(floor(xs[k + 1] - 0.5))) + 1):
					mask[r * w + c] = value


## Two-pass chamfer (3-4) distance, in thirds of a pixel, from every pixel whose mask equals
## `inside` to the nearest pixel where it doesn't (0 outside).
func _chamfer(mask: PackedByteArray, w: int, h: int, inside: int) -> PackedInt32Array:
	var big := 1 << 20
	var d := PackedInt32Array()
	d.resize(w * h)
	for i in d.size():
		d[i] = big if mask[i] == inside else 0
	for y in h:
		for x in w:
			var i := y * w + x
			var v := d[i]
			if v == 0:
				continue
			if x > 0: v = mini(v, d[i - 1] + 3)
			if y > 0:
				v = mini(v, d[i - w] + 3)
				if x > 0: v = mini(v, d[i - w - 1] + 4)
				if x < w - 1: v = mini(v, d[i - w + 1] + 4)
			d[i] = v
	for y in range(h - 1, -1, -1):
		for x in range(w - 1, -1, -1):
			var i := y * w + x
			var v := d[i]
			if v == 0:
				continue
			if x < w - 1: v = mini(v, d[i + 1] + 3)
			if y < h - 1:
				v = mini(v, d[i + w] + 3)
				if x < w - 1: v = mini(v, d[i + w + 1] + 4)
				if x > 0: v = mini(v, d[i + w - 1] + 4)
			d[i] = v
	return d


## Metres from each pixel centre to the nearest edge of the given polygons, for pixels within
## `band` pixels of an edge (-1 elsewhere).
func _coast_distance(rings: Array, w: int, h: int, rect: Rect2, step: float, band: int) -> PackedFloat32Array:
	var d := PackedFloat32Array()
	d.resize(w * h)
	d.fill(-1.0)
	for ring in rings:
		var pts: PackedVector2Array = ring
		var n := pts.size()
		for i in n:
			var a := (pts[i] - rect.position) / step
			var b := (pts[(i + 1) % n] - rect.position) / step
			var ab := b - a
			var len2 := maxf(ab.length_squared(), 1e-9)
			var x0 := maxi(0, int(minf(a.x, b.x)) - band)
			var x1 := mini(w - 1, int(maxf(a.x, b.x)) + band)
			var y0 := maxi(0, int(minf(a.y, b.y)) - band)
			var y1 := mini(h - 1, int(maxf(a.y, b.y)) + band)
			for y in range(y0, y1 + 1):
				for x in range(x0, x1 + 1):
					var q := Vector2(x + 0.5, y + 0.5)
					var t := clampf((q - a).dot(ab) / len2, 0.0, 1.0)
					var dist := (q - a - ab * t).length()
					if dist > band:
						continue
					var idx := y * w + x
					var m := dist * PX_M
					if d[idx] < 0.0 or m < d[idx]:
						d[idx] = m
	return d


## slice_cover.png, red = distance to the sea in 10 m steps (0 at sea, 255 = 2.55 km or more).
func _save_cover(to_sea: PackedInt32Array, w: int, h: int) -> void:
	var bytes := PackedByteArray()
	bytes.resize(w * h)
	for i in to_sea.size():
		bytes[i] = clampi(int(to_sea[i] / 3.0 * PX_M / 10.0), 0, 255)
	var img := Image.create_from_data(w, h, false, Image.FORMAT_L8, bytes)
	img.save_png(ProjectSettings.globalize_path("res://route_map/data/slice_cover.png"))


## If a river ends on land (the data often stops short of the coast), extend it along the
## shortest way to open water (sea or lake), and a little way out into it.
func _join_mouth(pts: PackedVector2Array, land: PackedByteArray, w: int, h: int, rect: Rect2, step: float) -> PackedVector2Array:
	var end := pts[pts.size() - 1]
	var c := Vector2i(((end - rect.position) / step).floor())
	if c.x < 0 or c.y < 0 or c.x >= w or c.y >= h:
		return pts
	if land[c.y * w + c.x] == 0:
		return pts
	var prev := {c: c}
	var queue: Array[Vector2i] = [c]
	var head := 0
	var found := Vector2i(-1, -1)
	while head < queue.size() and head < 200000:
		var q: Vector2i = queue[head]
		head += 1
		if land[q.y * w + q.x] == 0:
			found = q
			break
		for dv in [Vector2i(1, 0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1), Vector2i(1, 1), Vector2i(-1, 1), Vector2i(1, -1), Vector2i(-1, -1)]:
			var n: Vector2i = q + dv
			if n.x < 0 or n.y < 0 or n.x >= w or n.y >= h or prev.has(n):
				continue
			prev[n] = q
			queue.append(n)
	if found.x < 0:
		return pts
	var path: Array[Vector2i] = []
	var at := found
	while at != c:
		path.push_front(at)
		at = prev[at]
	var out := pts.duplicate()
	for k in range(0, path.size(), 3):
		out.append(rect.position + (Vector2(path[k]) + Vector2(0.5, 0.5)) * step)
	var last := rect.position + (Vector2(found) + Vector2(0.5, 0.5)) * step
	var dir := (last - end).normalized()
	out.append(last)
	out.append(last + dir * 250.0 / M_PER_UNIT)
	print("  joined a river mouth to open water over %.1f km" % (path.size() * PX_M / 1000.0))
	return out

func _hm_at(hm: PackedFloat32Array, w: int, h: int, rect: Rect2, step: float, p: Vector2) -> float:
	var q := ((p - rect.position) / step).floor()
	var x := clampi(int(q.x), 0, w - 1)
	var y := clampi(int(q.y), 0, h - 1)
	return hm[y * w + x]


## Riverbed below the water level; natural levees either side (the Rhine branches ran on raised
## banks through the low peat), easing back into the land. The water surface is written to `water`.
func _carve(hm: PackedFloat32Array, water: PackedFloat32Array, w: int, h: int, rect: Rect2, step: float, pts: PackedVector2Array, levels: PackedFloat32Array, half: float) -> void:
	# Distance to each segment (not to its points), so the banks run smooth instead of scalloped.
	var bank := half * 2.4
	var wet := half * 1.3
	var r_px := int(ceil(bank / PX_M)) + 1
	for i in pts.size() - 1:
		var a := (pts[i] - rect.position) / step
		var b := (pts[i + 1] - rect.position) / step
		var ab := b - a
		var len2 := maxf(ab.length_squared(), 1e-6)
		var x0 := int(minf(a.x, b.x)) - r_px
		var x1 := int(maxf(a.x, b.x)) + r_px
		var y0 := int(minf(a.y, b.y)) - r_px
		var y1 := int(maxf(a.y, b.y)) + r_px
		for y in range(maxi(0, y0), mini(h, y1 + 1)):
			for x in range(maxi(0, x0), mini(w, x1 + 1)):
				var q := Vector2(x + 0.5, y + 0.5)
				var t := clampf((q - a).dot(ab) / len2, 0.0, 1.0)
				var d := (q - (a + ab * t)).length() * PX_M
				if d > bank:
					continue
				var idx := y * w + x
				var lvl := lerpf(levels[i], levels[i + 1], t)
				if d <= wet:
					water[idx] = maxf(water[idx], lvl)
				if d <= half:
					hm[idx] = minf(hm[idx], lvl - 2.5 + 1.8 * (d / half) * (d / half))
				elif hm[idx] > lvl - 0.4:            # not another river's bed, nor the sea
					var u := smoothstep(0.25, 1.0, (d - half) / (bank - half))
					hm[idx] = lerpf(lvl + 0.35, hm[idx], u)