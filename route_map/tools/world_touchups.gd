extends SceneTree
## Wider-world touch-ups, safe to re-run: shortens the old hand-drawn Rus approaches so routes use
## the real river network, and adds region names for the new lands. Doesn't touch anything else.

const Geo := preload("res://route_map/geo.gd")
const Style := preload("res://route_map/style.gd")
const SCENE := "res://route_map/route_map.tscn"
const RIVER_PORTS := ["Aldeigjuborg", "Holmgard", "Kænugarðr"]
# [text, lat, lon, size, rotation°, sea?]
const REGIONS := [
	["PONTUS EUXINUS", 43.3, 34.5, 56.0, 0.0, true],
	["MARE CASPIUM", 44.8, 50.2, 36.0, -70.0, true],
	["KHAZARIA", 47.8, 45.0, 46.0, 0.0, false],
	["ROMANIA", 41.6, 24.5, 44.0, 0.0, false],
	["ITALIA", 43.4, 12.2, 48.0, -52.0, false],
	["HISPANIA", 41.3, -4.5, 54.0, 0.0, false],
	["GROENLANDIA", 66.0, -42.0, 58.0, 0.0, false],
]


func _initialize() -> void:
	var root: Node = (load(SCENE) as PackedScene).instantiate()
	var map: Node = root.get_node("Map")
	var done: Array = []
	for pn in RIVER_PORTS:
		var port: Node2D = null
		for n in map.get_node("Ports").get_children():
			if n is KapmanPort and n.display_name == pn:
				port = n
		if port == null:
			continue
		var line: Line2D = port.get_node_or_null("Approach")
		if line and line.points.size() > 2:
			line.points = PackedVector2Array([Vector2(0.5, 0.0), Vector2.ZERO])
			port.approach_note = "by the rivers themselves"
			done.append(pn)
	var labels: Node = map.get_node_or_null("Decor/RegionNames")
	if labels:
		for r in REGIONS:
			var node_name := String(r[0]).replace(" ", "_").to_pascal_case()
			if labels.get_node_or_null(node_name) != null:
				continue
			var l := KapmanRegionLabel.new()
			l.name = node_name
			l.text = r[0]
			l.position = Geo.project(r[1], r[2])
			l.size = r[3]
			l.rotation_degrees = r[4]
			l.sea = r[5]
			l.ink = Color(Style.INK, 0.3 if r[5] else 0.4)
			labels.add_child(l)
			l.owner = root
			done.append(r[0])
	var out := PackedScene.new()
	out.pack(root)
	ResourceSaver.save(out, SCENE)
	root.free()
	print("touch-ups: ", done)
	quit()
