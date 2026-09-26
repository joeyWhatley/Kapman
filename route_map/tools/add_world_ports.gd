extends SceneTree
## Adds the new ports of the wider world and the Bosporus correction to route_map.tscn, without
## touching anything you've already placed or edited. Safe to re-run (existing names are skipped).
##   godot --headless --path <project> --script res://route_map/tools/add_world_ports.gd

const Geo := preload("res://route_map/geo.gd")
const SCENE := "res://route_map/route_map.tscn"

# prices: Grain, Cloth, Wine, Furs, Amber, Swords. River ports sit on their river: approach = the port.
const PORTS := {
	"Miklagard": {"tier": 1, "fac": "G", "tag": "Constantinople · the Queen of Cities", "prices": [12, 17, 7, 28, 32, 30], "swords": "sell",
		"note": "the Golden Horn, behind the chain", "approach": [[41.02, 28.99], [41.02, 28.98]]},
	"Cherson": {"tier": 3, "fac": "G", "tag": "Chersonesos · Roman Crimea", "prices": [9, 15, 8, 22, 26, 28], "swords": "sell",
		"note": "the harbour below the walls", "approach": [[44.62, 33.47], [44.61, 33.49]]},
	"Bolghar": {"tier": 2, "fac": "K", "tag": "Volga Bulgars · the fur market", "prices": [10, 18, 21, 5, 18, 32], "swords": "sell",
		"note": "the bank of the Volga", "approach": [[54.98, 49.05], [54.98, 49.06]]},
	"Atil": {"tier": 2, "fac": "K", "tag": "Khazar khagan's city · silver road", "prices": [8, 17, 18, 9, 22, 34], "swords": "sell",
		"note": "the Volga delta", "approach": [[46.4, 47.95], [46.39, 47.97]]},
	"Parisius": {"tier": 2, "fac": "F", "tag": "Paris · Frankish", "prices": [5, 11, 9, 21, 25, 13], "swords": "buy",
		"note": "up the Seine", "approach": [[48.855, 2.34], [48.853, 2.35]]},
	"Rotomagus": {"tier": 3, "fac": "F", "tag": "Rouen · Frankish", "prices": [5, 10, 10, 20, 24, 13], "swords": "buy",
		"note": "the lower Seine", "approach": [[49.44, 1.08], [49.44, 1.1]]},
	"Colonia": {"tier": 2, "fac": "F", "tag": "Cologne · Rhenish swords", "prices": [6, 10, 6, 21, 24, 11], "swords": "buy",
		"note": "up the Rhine", "approach": [[50.94, 6.96], [50.94, 6.97]]},
	"Hammaburg": {"tier": 3, "fac": "F", "tag": "Hamburg · Saxon march", "prices": [7, 12, 11, 17, 20, -1], "swords": "none",
		"note": "up the Elbe", "approach": [[53.545, 9.97], [53.55, 9.99]]},
}

const PATCHES := [
	{"id": "Bosporus", "note": "The Bosporus is barely a kilometre wide: opened here so ships can reach Miklagard from the Black Sea.",
		"poly": [[41.26, 29.08], [41.2, 29.1], [41.12, 29.08], [41.03, 29.02], [40.98, 28.98], [40.96, 29.03], [41.02, 29.07], [41.1, 29.12], [41.18, 29.16], [41.25, 29.16]]},
	{"id": "Dardanelles", "note": "The Hellespont, opened a little wider than it is so it survives the chart grid.",
		"poly": [[40.45, 26.72], [40.3, 26.55], [40.12, 26.35], [40.02, 26.2], [40.05, 26.33], [40.15, 26.45], [40.28, 26.63], [40.42, 26.8]]},
]


func _initialize() -> void:
	var root: Node = (load(SCENE) as PackedScene).instantiate()
	var map: Node = root.get_node("Map")
	var ports: Node = map.get_node("Ports")
	var existing := {}
	for n in ports.get_children():
		if n is KapmanPort:
			existing[n.display_name] = true
	var added: Array = []
	for name in PORTS:
		if existing.has(name):
			continue
		var d: Dictionary = PORTS[name]
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
		var line := Line2D.new()
		line.name = "Approach"
		line.width = 2.0
		line.default_color = Color(0.7, 0.2, 0.12, 0.8)
		for q in pts:
			line.add_point(q - port.position)
		port.add_child(line)
		ports.add_child(port)
		added.append(name)
	var corrections: Node = map.get_node("Corrections")
	for p in PATCHES:
		if corrections.get_node_or_null(p["id"]) != null:
			continue
		var patch := KapmanLandPatch.new()
		patch.name = p["id"]
		var poly := PackedVector2Array()
		for ll in p["poly"]:
			poly.append(Geo.project(ll[0], ll[1]))
		var c := Vector2.ZERO
		for q in poly:
			c += q
		c /= poly.size()
		patch.position = c
		var local := PackedVector2Array()
		for q in poly:
			local.append(q - c)
		patch.polygon = local
		patch.mode = KapmanLandPatch.Mode.CUT_WATER
		patch.note = p["note"]
		corrections.add_child(patch)
		added.append(p["id"])
	_own(root, root)
	var out := PackedScene.new()
	out.pack(root)
	ResourceSaver.save(out, SCENE)
	root.free()
	print("added: ", added)
	quit()


func _own(node: Node, root: Node) -> void:
	for c in node.get_children():
		if c.owner == null:
			c.owner = root
		if c.scene_file_path == "":
			_own(c, root)
