extends SceneTree
## Adds the sea, cloud shadows, ambient life and ocean currents to route_map.tscn WITHOUT touching
## anything already in it (your edited ports, hazards and corrections are kept). Safe to re-run:
## nodes that already exist are left alone.
##   godot --headless --path <project> --script res://route_map/tools/add_ambience.gd

const Geo := preload("res://route_map/geo.gd")
const SCENE := "res://route_map/route_map.tscn"

# Currents: [lat, lon] points in the direction the water flows.
const CURRENTS := [
	{"id": "NorwegianCoastal", "strength": 0.5, "km": 45.0, "note": "The Norwegian coastal current runs north along the whole coast.",
		"pts": [[58.0, 9.0], [57.9, 7.0], [58.6, 5.4], [59.8, 4.7], [61.5, 4.5], [63.3, 7.5], [65.5, 11.0], [68.0, 13.8], [69.8, 17.5]]},
	{"id": "BalticOutflow", "strength": 0.45, "km": 25.0, "note": "Fresh Baltic water pours out through the Sound and the Kattegat.",
		"pts": [[55.55, 12.75], [56.2, 12.4], [56.9, 11.8], [57.6, 11.1], [57.9, 10.2], [58.05, 9.2]]},
	{"id": "DoverStrait", "strength": 0.4, "km": 28.0, "note": "The net tidal stream sets north-east through the Dover Strait into the North Sea.",
		"pts": [[50.45, 0.4], [50.9, 1.35], [51.3, 2.0], [51.8, 2.6], [52.4, 3.2]]},
	{"id": "EastBritishCoast", "strength": 0.25, "km": 55.0, "note": "Water drifts south along the Scottish and English east coast.",
		"pts": [[58.6, -2.2], [57.2, -1.6], [55.9, -1.3], [54.7, -0.6], [53.7, 0.7], [53.0, 2.1]]},
	{"id": "ContinentalCoastal", "strength": 0.3, "km": 40.0, "note": "Along the Frisian and Jutland coast the water sets north-east.",
		"pts": [[52.4, 4.2], [53.2, 4.7], [53.65, 6.2], [54.0, 7.6], [55.0, 7.9], [56.4, 7.9], [57.4, 8.8], [57.8, 9.8]]},
	{"id": "AtlanticDrift", "strength": 0.3, "km": 90.0, "note": "Warm Atlantic water flows north-east past the Faroes toward Norway.",
		"pts": [[57.5, -13.0], [59.2, -8.0], [60.8, -3.0], [62.0, 1.5], [63.2, 5.0]]},
	{"id": "PentlandRace", "strength": 0.85, "km": 10.0, "note": "The tide races through the Pentland Firth.",
		"pts": [[58.66, -3.7], [58.72, -2.7]]},
]


func _initialize() -> void:
	var packed: PackedScene = load(SCENE)
	var root: Node = packed.instantiate()
	var map: Node = root.get_node("Map")
	var added: Array = []

	if map.get_node_or_null("Sea") == null:
		var sea: Node2D = load("res://route_map/layers/sea_layer.gd").new()
		sea.name = "Sea"
		map.add_child(sea)
		map.move_child(sea, map.get_node("Paper").get_index() + 1)
		added.append("Sea")

	if map.get_node_or_null("Currents") == null:
		var group := Node2D.new()
		group.name = "Currents"
		map.add_child(group)
		map.move_child(group, map.get_node("Hazards").get_index() + 1)
		for c in CURRENTS:
			var line := KapmanCurrent.new()
			line.name = c["id"]
			line.strength = c["strength"]
			line.width_km = c["km"]
			line.note = c["note"]
			for ll in c["pts"]:
				line.add_point(Geo.project(ll[0], ll[1]))
			group.add_child(line)
		added.append("Currents (%d)" % CURRENTS.size())

	if map.get_node_or_null("Ambient") == null:
		var amb: Node2D = load("res://route_map/layers/ambient_layer.gd").new()
		amb.name = "Ambient"
		map.add_child(amb)
		map.move_child(amb, map.get_node("HazardMarks").get_index() + 1)
		added.append("Ambient")

	if map.get_node_or_null("CloudShadows") == null:
		var clouds: Node2D = load("res://route_map/layers/sheet_layer.gd").new()
		clouds.name = "CloudShadows"
		map.add_child(clouds)
		map.move_child(clouds, map.get_node("Ambient").get_index() + 1)
		added.append("CloudShadows")

	_own(root, root)
	var out := PackedScene.new()
	out.pack(root)
	ResourceSaver.save(out, SCENE)
	root.free()
	print("added: ", added if not added.is_empty() else "nothing (already there)")
	quit()


func _own(node: Node, root: Node) -> void:
	for c in node.get_children():
		if c.owner == null:
			c.owner = root
		if c.scene_file_path == "":
			_own(c, root)
