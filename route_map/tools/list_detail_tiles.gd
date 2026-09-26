extends SceneTree
## Writes G:/kapman_data/raw/coast_river_tiles.txt: the 1° lat/lon tiles (as "N52_E004") that a
## sea coastline passes through. Those get 30 m elevation; the rest (rivers too: the game carves
## their channels itself) get 90 m.

const Geo := preload("res://route_map/geo.gd")
const MapData := preload("res://route_map/map_data.gd")


func _initialize() -> void:
	var md := MapData.new()
	md.load_data("res://route_map/data/coast.json", false)
	var keys := {}
	var lines: Array = []
	lines.append_array(md.detail_rings)
	for pts in lines:
		for i in pts.size():
			var a: Vector2 = pts[i]
			var b: Vector2 = pts[(i + 1) % pts.size()]
			var n := int(a.distance_to(b) / 2.0) + 1        # every ~2 km along the line
			for k in n:
				var ll := Geo.unproject(a.lerp(b, float(k) / n))
				var lat := int(floor(ll.x))
				var lon := int(floor(ll.y))
				keys["%s%02d_%s%03d" % ["N" if lat >= 0 else "S", absi(lat), "E" if lon >= 0 else "W", absi(lon)]] = true
	var f := FileAccess.open("G:/kapman_data/raw/coast_river_tiles.txt", FileAccess.WRITE)
	for k in keys:
		f.store_line(k)
	f.close()
	print("%d coast tiles" % keys.size())
	quit()
