extends SceneTree
## Applies the old Frisian chart look to route_map.tscn WITHOUT touching your ports, hazards,
## currents or corrections: recolours the ink layers, gives the paper a vellum shader, and adds the
## interlace border, the title cartouche, Latin region names and sea marginalia (if not there yet).
##   godot --headless --path <project> --script res://route_map/tools/apply_old_look.gd

const Geo := preload("res://route_map/geo.gd")
const Style := preload("res://route_map/style.gd")
const SCENE := "res://route_map/route_map.tscn"

# [text, lat, lon, size, rotation°, sea?]
const REGIONS := [
	["MARE FRESICUM", 55.6, 3.2, 62.0, -8.0, true],
	["OCEANUS OCCIDENTALIS", 61.8, -17.0, 70.0, 0.0, true],
	["MARE ORIENTALE", 57.4, 19.2, 48.0, -68.0, true],
	["FRESIA", 53.05, 6.6, 30.0, -4.0, false],
	["FRANCIA", 48.6, 3.0, 58.0, 0.0, false],
	["SAXONIA", 52.2, 9.8, 32.0, 0.0, false],
	["BRITANNIA", 52.6, -1.9, 44.0, -72.0, false],
	["HIBERNIA", 53.2, -8.0, 38.0, -62.0, false],
	["DANIA", 56.1, 9.2, 30.0, -80.0, false],
	["NORTHMANNIA", 61.6, 8.6, 46.0, -56.0, false],
	["SUEONIA", 60.2, 15.4, 42.0, -62.0, false],
	["SCLAVIA", 53.2, 16.8, 46.0, 0.0, false],
	["GARDARIKI", 57.6, 33.5, 58.0, 0.0, false],
	["THULE", 64.9, -18.6, 30.0, 0.0, false],
]
# [kind, lat, lon, size]
const MARGINALIA := [
	[KapmanMarginalia.Kind.SERPENT, 65.8, -6.0, 1.3],
	[KapmanMarginalia.Kind.WHALE, 59.5, -22.0, 1.2],
	[KapmanMarginalia.Kind.SHIP, 47.2, -13.0, 1.2],
	[KapmanMarginalia.Kind.SERPENT, 69.5, 4.0, 1.0],
]


func _initialize() -> void:
	var root: Node = (load(SCENE) as PackedScene).instantiate()
	var map: Node = root.get_node("Map")
	var size := Geo.size()

	var paper: Node2D = map.get_node("Paper")
	paper.color = Style.PAPER
	paper.outside = Style.PAPER_OUTSIDE
	var vm := ShaderMaterial.new()
	vm.shader = load("res://route_map/vellum.gdshader")
	var nt := NoiseTexture2D.new()
	nt.width = 256
	nt.height = 256
	nt.seamless = true
	var fn := FastNoiseLite.new()
	fn.frequency = 0.02
	fn.fractal_octaves = 4
	nt.noise = fn
	vm.set_shader_parameter("noise_tex", nt)
	vm.set_shader_parameter("chart", Vector4(0, 0, size.x, size.y))
	paper.material = vm
	map.get_node("FogWash").color = Style.FOG
	map.get_node("CoastRough").color = Style.ROUGH
	map.get_node("CoastDetail").color = Style.INK
	map.get_node("Lakes").color = Color(Style.INK, 0.75)
	map.get_node("Rivers").color = Style.RIVER
	var compass = map.get_node_or_null("Compass")
	if compass:
		compass.ink = Style.INK
		compass.paper = Style.PAPER
		compass.accent = Style.ACCENT

	var added: Array = []
	if map.get_node_or_null("Border") == null:
		var border := KapmanBorder.new()
		border.name = "Border"
		border.rect = Rect2(Vector2.ZERO, size)
		border.ink = Style.INK
		border.fill = Style.PAPER.darkened(0.04)
		border.accent = Style.ACCENT
		map.add_child(border)
		map.move_child(border, paper.get_index() + 1)
		added.append("Border")
	if map.get_node_or_null("Decor") == null:
		var decor := Node2D.new()
		decor.name = "Decor"
		map.add_child(decor)
		map.move_child(decor, map.get_node("Rivers").get_index() + 1)
		var cart := KapmanCartouche.new()
		cart.name = "Cartouche"
		cart.position = Geo.project(45.6, -16.5)
		cart.ink = Style.INK
		cart.fill = Style.PAPER.darkened(0.03)
		cart.accent = Style.ACCENT
		decor.add_child(cart)
		var labels := Node2D.new()
		labels.name = "RegionNames"
		decor.add_child(labels)
		for r in REGIONS:
			var l := KapmanRegionLabel.new()
			l.name = String(r[0]).replace(" ", "_").to_pascal_case()
			l.text = r[0]
			l.position = Geo.project(r[1], r[2])
			l.size = r[3]
			l.rotation_degrees = r[4]
			l.sea = r[5]
			l.ink = Color(Style.INK, 0.3 if r[5] else 0.4)
			labels.add_child(l)
		var marg := Node2D.new()
		marg.name = "Marginalia"
		decor.add_child(marg)
		for m in MARGINALIA:
			var d := KapmanMarginalia.new()
			d.name = ["Serpent", "Whale", "Ship"][m[0]] + str(marg.get_child_count() + 1)
			d.kind = m[0]
			d.position = Geo.project(m[1], m[2])
			d.size = m[3]
			d.ink = Style.INK
			d.fill = Style.PAPER.darkened(0.02)
			d.accent = Style.ACCENT
			marg.add_child(d)
		added.append("Decor (cartouche, %d region names, %d marginalia)" % [REGIONS.size(), MARGINALIA.size()])

	_own(root, root)
	var out := PackedScene.new()
	out.pack(root)
	ResourceSaver.save(out, SCENE)
	root.free()
	print("old look applied; added: ", added)
	quit()


func _own(node: Node, root: Node) -> void:
	for c in node.get_children():
		if c.owner == null:
			c.owner = root
		if c.scene_file_path == "":
			_own(c, root)
