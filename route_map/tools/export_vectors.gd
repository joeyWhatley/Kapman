extends SceneTree
## Exports the chart's geography for the terrain baker (G:/kapman_data/baker):
## G:/kapman_data/vectors.txt, all coordinates in map units (see geo.gd):
##   LAND n / LAKE n / CUT n / ADD n   followed by n lines "x y"   (closed rings)
##   RIVER n rank name                 followed by n lines "x y"   (source to mouth)
## CUT/ADD are the period corrections (the Wantsum Channel, the Bosporus...) from route_map.tscn.

const Geo := preload("res://route_map/geo.gd")
const MapData := preload("res://route_map/map_data.gd")
## Rivers of 800 AD missing from the modern data (kept in step with tools/bake_slice.gd).
const EXTRA_RIVERS := [
	["Oude Rijn", 4, [[51.972, 5.345], [52.02, 5.24], [52.075, 5.165], [52.09, 5.12], [52.083, 5.0], [52.085, 4.88],
		[52.115, 4.76], [52.13, 4.66], [52.155, 4.55], [52.16, 4.49], [52.185, 4.44], [52.205, 4.395]]],
	["Vecht", 6, [[52.09, 5.12], [52.14, 5.04], [52.175, 5.0], [52.24, 5.02], [52.29, 5.04], [52.335, 5.07]]],
]


func _initialize() -> void:
	var md := MapData.new()
	md.load_data("res://route_map/data/coast.json", false)
	var out := PackedStringArray()
	for r in md.detail_rings:
		_ring(out, "LAND", r)
	for r in md.lake_rings:
		_ring(out, "LAKE", r)
	var scene: Node = load("res://route_map/route_map.tscn").instantiate()
	for n in scene.get_node("Map/Corrections").get_children():
		if n is KapmanLandPatch:
			var poly: PackedVector2Array = n.world_polygon()
			if poly.size() >= 3:
				_ring(out, "CUT" if n.mode == KapmanLandPatch.Mode.CUT_WATER else "ADD", poly)
	scene.free()
	var rivers := 0
	for rv in md.river_runs:
		_river(out, rv["pts"], float(rv["rank"]), str(rv["name"]))
		rivers += 1
	for ex in EXTRA_RIVERS:
		var pts := PackedVector2Array()
		for ll in ex[2]:
			pts.append(Geo.project(ll[0], ll[1]))
		_river(out, pts, float(ex[1]), ex[0])
		rivers += 1
	var f := FileAccess.open("G:/kapman_data/vectors.txt", FileAccess.WRITE)
	f.store_string("\n".join(out))
	f.close()
	print("exported %d land, %d lake rings, %d rivers" % [md.detail_rings.size(), md.lake_rings.size(), rivers])
	quit()


func _ring(out: PackedStringArray, kind: String, r: PackedVector2Array) -> void:
	if r.size() < 3:
		return
	out.append("%s %d" % [kind, r.size()])
	for p in r:
		out.append("%.5f %.5f" % [p.x, p.y])


func _river(out: PackedStringArray, pts: PackedVector2Array, rank: float, name: String) -> void:
	if pts.size() < 2:
		return
	out.append("RIVER %d %.1f %s" % [pts.size(), rank, name.replace(" ", "_")])
	for p in pts:
		out.append("%.5f %.5f" % [p.x, p.y])
