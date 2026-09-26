extends SceneTree
## Real elevation -> a heightmap aligned with the route map. Source: NOAA ETOPO1 (public domain),
## full 1 arc-minute subset for 50°W–52°E, 39.5°N–71.5°N (design/source_data/etopo_1min.asc).
## Output: route_map/data/terrain_height.exr — one float per MAP_STEP map units, metres above sea
## level (negative = sea depth), sampled through the map projection.
##   godot --headless --path <project> --script res://route_map/tools/build_terrain.gd

const Geo := preload("res://route_map/geo.gd")
const SOURCE := "res://design/source_data/etopo_1min.asc"
const OUT := "res://route_map/data/terrain_height.exr"
const MAP_STEP := 2.0


func _initialize() -> void:
	var t0 := Time.get_ticks_msec()
	var lines := FileAccess.get_file_as_string(SOURCE).split("\n", false)
	var hdr := {}
	var row0 := 0
	for i in 6:
		var parts := lines[i].strip_edges().split(" ", false)
		hdr[parts[0].to_lower()] = float(parts[parts.size() - 1])
		row0 = i + 1
	var ncols := int(hdr["ncols"])
	var nrows := int(hdr["nrows"])
	var x0: float = hdr["xllcenter"]
	var y0: float = hdr["yllcenter"]
	var cs: float = hdr["cellsize"]
	var data := PackedFloat32Array()
	data.resize(ncols * nrows)
	for r in nrows:
		var vals := lines[row0 + r].strip_edges().split(" ", false)
		for c in mini(ncols, vals.size()):
			data[r * ncols + c] = float(vals[c])
	print("read %dx%d elevations in %d ms" % [ncols, nrows, Time.get_ticks_msec() - t0])
	var size := Geo.size()
	var w := int(ceil(size.x / MAP_STEP))
	var h := int(ceil(size.y / MAP_STEP))
	var img := Image.create(w, h, false, Image.FORMAT_RF)
	var lo := INF
	var hi := -INF
	for y in h:
		for x in w:
			var ll := Geo.unproject(Vector2(x + 0.5, y + 0.5) * MAP_STEP)
			# ESRI grid: first row is the northern edge.
			var fc := (ll.y - x0) / cs
			var fr := (nrows - 1) - (ll.x - y0) / cs
			var v := _bilinear(data, ncols, nrows, fc, fr)
			lo = minf(lo, v)
			hi = maxf(hi, v)
			img.set_pixel(x, y, Color(v, 0, 0))
	img.save_exr(ProjectSettings.globalize_path(OUT), true)
	print("terrain %dx%d (1 px = %.0f map units), %.0f m .. %.0f m, %d ms" % [w, h, MAP_STEP, lo, hi, Time.get_ticks_msec() - t0])
	quit()


func _bilinear(d: PackedFloat32Array, nc: int, nr: int, fx: float, fy: float) -> float:
	fx = clampf(fx, 0.0, nc - 1.001)
	fy = clampf(fy, 0.0, nr - 1.001)
	var x := int(fx)
	var y := int(fy)
	var tx := fx - x
	var ty := fy - y
	var a := d[y * nc + x]
	var b := d[y * nc + x + 1]
	var c := d[(y + 1) * nc + x]
	var e := d[(y + 1) * nc + x + 1]
	return lerpf(lerpf(a, b, tx), lerpf(c, e, tx), ty)
