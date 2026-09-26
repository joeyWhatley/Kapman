extends SceneTree
## Builds the editable scenes for the route map from seed data. Run once (it overwrites them!):
##   godot --headless --path <project> --script res://route_map/tools/build_scenes.gd
## Afterwards, edit route_map.tscn and the ui/*.tscn scenes in the Godot editor instead.

const Geo := preload("res://route_map/geo.gd")
const Style := preload("res://route_map/style.gd")
const Seed := preload("res://route_map/tools/seed_data.gd")

const UI := [
	["res://route_map/ui/port_panel.gd", "res://route_map/ui/port_panel.tscn", "PortPanel"],
	["res://route_map/ui/music_player.gd", "res://route_map/ui/music_player.tscn", "MusicPlayer"],
	["res://route_map/ui/incident_card.gd", "res://route_map/ui/incident_card.tscn", "IncidentCard"],
	["res://route_map/battle/battle_screen.gd", "res://route_map/battle/battle_screen.tscn", "BattleScreen"],
]


func _initialize() -> void:
	var theme := Style.make_theme()
	_save_res(theme, "res://route_map/ui/theme.tres")
	theme = load("res://route_map/ui/theme.tres")

	var scenes := {}
	for spec in UI:
		var node: Control = load(spec[0]).new()
		node._build()
		node.theme = theme
		_save_scene(node, spec[1])
		scenes[spec[2]] = load(spec[1])

	var hud: CanvasLayer = load("res://route_map/ui/hud.gd").new()
	hud._build(scenes)
	hud.get_node("Root").theme = theme
	_save_scene(hud, "res://route_map/ui/hud.tscn")

	_save_scene(_build_map(load("res://route_map/ui/hud.tscn")), "res://route_map/route_map.tscn")
	print("built: theme, %d UI scenes, hud.tscn, route_map.tscn" % UI.size())
	quit()


func _save_res(res: Resource, path: String) -> void:
	var err := ResourceSaver.save(res, path)
	if err != OK:
		push_error("could not save %s (%d)" % [path, err])


func _save_scene(root: Node, path: String) -> void:
	_own(root, root)
	var ps := PackedScene.new()
	var err := ps.pack(root)
	if err != OK:
		push_error("could not pack %s (%d)" % [path, err])
		return
	_save_res(ps, path)
	root.free()


## Every node gets the scene root as owner so it's saved; instanced sub-scenes keep their insides.
func _own(node: Node, root: Node) -> void:
	for c in node.get_children():
		c.owner = root
		if c.scene_file_path == "":
			_own(c, root)


func _layer(parent: Node, script: String, name: String) -> Node2D:
	var n: Node2D = load(script).new()
	n.name = name
	parent.add_child(n)
	return n


func _group(parent: Node, name: String) -> Node2D:
	var n := Node2D.new()
	n.name = name
	parent.add_child(n)
	return n


func _build_map(hud_scene: PackedScene) -> Node2D:
	var root := Node2D.new()
	root.name = "RouteMap"
	root.set_script(load("res://route_map/route_map.gd"))
	var map := _group(root, "Map")
	var rect := Rect2(Vector2.ZERO, Geo.size())

	var paper := _layer(map, "res://route_map/layers/sheet_layer.gd", "Paper")
	paper.rect = rect
	paper.color = Style.PAPER
	paper.outside = Style.PAPER_OUTSIDE
	var wash := _layer(map, "res://route_map/layers/sheet_layer.gd", "FogWash")
	wash.rect = rect
	wash.color = Style.FOG
	wash.visible = true

	var rough := _layer(map, "res://route_map/layers/coast_layer.gd", "CoastRough")
	rough.source = "rough"
	rough.color = Style.ROUGH
	rough.width_px = 1.4
	rough.sketch = true
	var detail := _layer(map, "res://route_map/layers/coast_layer.gd", "CoastDetail")
	detail.source = "detail"
	detail.color = Style.INK
	detail.width_px = 1.15
	var lakes := _layer(map, "res://route_map/layers/coast_layer.gd", "Lakes")
	lakes.source = "lakes"
	lakes.color = Color(Style.INK, 0.75)
	lakes.width_px = 1.0

	var corrections := _group(map, "Corrections")
	for p in Seed.PATCHES:
		var patch := KapmanLandPatch.new()
		patch.name = p["id"]
		var pts := PackedVector2Array()
		for ll in p["poly"]:
			pts.append(Geo.project(ll[0], ll[1]))
		var c := _centroid(pts)
		patch.position = c
		var local := PackedVector2Array()
		for q in pts:
			local.append(q - c)
		patch.polygon = local
		patch.mode = KapmanLandPatch.Mode.CUT_WATER if p["mode"] == "cut" else KapmanLandPatch.Mode.ADD_LAND
		patch.note = p["note"]
		corrections.add_child(patch)

	var rivers := _layer(map, "res://route_map/layers/coast_layer.gd", "Rivers")
	rivers.source = "custom"
	rivers.color = Style.RIVER
	rivers.width_px = 1.0

	var hazards := _group(map, "Hazards")
	for z in Seed.ZONES:
		var h := KapmanHazard.new()
		h.name = z["id"]
		h.position = Geo.project(z["lat"], z["lon"])
		var rx: float = z["rx"] / Geo.KM_PER_UNIT
		var ry: float = z["ry"] / Geo.KM_PER_UNIT
		var rot := deg_to_rad(z["rot"])
		var poly := PackedVector2Array()
		for i in 24:
			var a := TAU * i / 24.0
			poly.append(Vector2(cos(a) * rx, sin(a) * ry).rotated(rot))
		h.polygon = poly
		h.display_name = z["name"]
		h.kind = KapmanHazard.KIND_NAMES.find(z["kind"])
		h.severity = z["sev"]
		var flags := 0
		for s in z["seasons"]:
			flags |= 1 << s
		h.seasons = flags
		h.known_at_start = z.get("known", 0)
		h.rumor = z["rumor"]
		h.known_text = z["known_text"]
		hazards.add_child(h)
	_layer(map, "res://route_map/layers/hazard_layer.gd", "HazardMarks")

	var ports := _group(map, "Ports")
	for name in Seed.PORTS:
		var d: Dictionary = Seed.PORTS[name]
		var pts := PackedVector2Array()
		for ll in d["approach"]:
			pts.append(Geo.project(ll[0], ll[1]))
		var port := KapmanPort.new()
		port.name = name.validate_node_name()
		port.display_name = name
		port.position = pts[pts.size() - 1]
		port.tier = d["tier"]
		port.faction = d["fac"]
		port.tag = d["tag"]
		port.approach_note = d["note"]
		port.prices = PackedInt32Array(d["prices"])
		port.swords = {"buy": KapmanPort.Swords.BUY, "sell": KapmanPort.Swords.SELL, "none": KapmanPort.Swords.NONE}[d["swords"]]
		port.home_port = d.get("home", false)
		var line := Line2D.new()
		line.name = "Approach"
		line.width = 2.0
		line.default_color = Color(0.7, 0.2, 0.12, 0.8)
		for q in pts:
			line.add_point(q - port.position)
		port.add_child(line)
		ports.add_child(port)
	_layer(map, "res://route_map/layers/port_layer.gd", "PortGlyphs")
	_layer(map, "res://route_map/layers/route_layer.gd", "Route")

	var cam := Camera2D.new()
	cam.name = "Camera"
	root.add_child(cam)
	var hud := hud_scene.instantiate()
	hud.name = "HUD"
	root.add_child(hud)
	return root


func _centroid(pts: PackedVector2Array) -> Vector2:
	var c := Vector2.ZERO
	for p in pts:
		c += p
	return c / maxi(1, pts.size())
