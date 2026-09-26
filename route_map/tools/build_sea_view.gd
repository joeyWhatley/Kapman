extends SceneTree
## Builds route_map/world3d/sea_view.tscn as an editable scene, plus the resources it uses:
##   world3d/terrain_material.tres, sea_material.tres, sail_material.tres, hull_material.tres
##   world3d/meshes/oak.res, alder.res, birch.res, bush.res, grass.res, reeds.res, cog_hull.res, cog_sail.res
## Safe to re-run: existing resource files and existing nodes are kept (your edits survive);
## only missing pieces are added.
##   godot --headless --path <project> --script res://route_map/tools/build_sea_view.gd

const DIR := "res://route_map/world3d/"
const SCENE := DIR + "sea_view.tscn"
const Plants := preload("res://route_map/world3d/plants.gd")
const Cog := preload("res://route_map/world3d/cog.gd")

var root_node: Node3D


func _initialize() -> void:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(DIR + "meshes"))
	var terrain_mat := _material("terrain_material.tres", func() -> Resource:
		var m := ShaderMaterial.new()
		m.shader = load(DIR + "terrain.gdshader")
		m.set_shader_parameter("relief", 1.8)
		m.set_shader_parameter("height_scale", 1.0)
		return m)
	var sea_mat := _material("sea_material.tres", func() -> Resource:
		var m := ShaderMaterial.new()
		m.shader = load(DIR + "ocean.gdshader")
		m.render_priority = 1
		m.set_shader_parameter("ripple_a", _noise_tex(0.018, true, 1))
		m.set_shader_parameter("ripple_b", _noise_tex(0.03, true, 2))
		m.set_shader_parameter("foam_tex", _foam_tex())
		return m)
	var sail_mat := _material("sail_material.tres", func() -> Resource:
		var m := ShaderMaterial.new()
		m.shader = load(DIR + "sail.gdshader")
		return m)
	var hull_mat := _material("hull_material.tres", func() -> Resource: return Cog.hull_material())
	var cog = Cog.new()
	var meshes := {
		"oak": func() -> Resource: return Plants.tree(1),
		"alder": func() -> Resource: return Plants.alder(1),
		"birch": func() -> Resource: return Plants.birch(1),
		"bush": func() -> Resource: return Plants.bush(),
		"grass": func() -> Resource: return Plants.grass(9, 0.55, 0.25),
		"reeds": func() -> Resource: return Plants.reeds(),
		"cog_hull": func() -> Resource: return cog.build_hull(),
		"cog_sail": func() -> Resource: return cog.build_sail(),
	}
	var m := {}
	for k in meshes:
		m[k] = _material("meshes/%s.res" % k, meshes[k])
	cog.free()

	if ResourceLoader.exists(SCENE):
		root_node = (load(SCENE) as PackedScene).instantiate()
		print("updating existing scene (keeping your edits)")
	else:
		root_node = Node3D.new()
		root_node.name = "SeaView"
		root_node.set_script(load(DIR + "sea_view.gd"))

	var we: WorldEnvironment = _node(root_node, "WorldEnvironment", func() -> Node:
		var n := WorldEnvironment.new()
		n.environment = _environment()
		return n)
	_node(root_node, "Sun", func() -> Node:
		var sun := DirectionalLight3D.new()
		sun.rotation_degrees = Vector3(-32, 35, 0)
		sun.light_color = Color(1.0, 0.94, 0.84)
		sun.light_energy = 1.25
		sun.shadow_enabled = true
		sun.directional_shadow_mode = DirectionalLight3D.SHADOW_PARALLEL_2_SPLITS
		sun.directional_shadow_split_1 = 0.25
		sun.directional_shadow_blend_splits = true
		return sun)
	_node(root_node, "Terrain", func() -> Node:
		var c := Node3D.new()
		c.set_script(load(DIR + "clipmap.gd"))
		c.base = 2.0
		c.levels = 13
		c.material = terrain_mat
		return c)
	_node(root_node, "Sea", func() -> Node:
		var c := Node3D.new()
		c.set_script(load(DIR + "clipmap.gd"))
		c.base = 0.5
		c.levels = 14
		c.material = sea_mat
		c.cast_shadows = false
		return c)
	var plants: Node3D = _node(root_node, "Plants", func() -> Node: return Node3D.new())
	var w := Color.WHITE
	# name, kind, only variant, mesh, cells, spacing, shadows, tints, sway, translucency
	var specs := [
		["Oaks", 0, 0, m["oak"], 128, 8.0, true, [w, w, w, w], 0.004, 0.25],
		["Alders", 0, 1, m["alder"], 128, 8.0, true, [w, w, w, w], 0.004, 0.25],
		["Birches", 0, 2, m["birch"], 128, 8.0, true, [w, w, w, w], 0.006, 0.3],
		["Bushes", 1, -1, m["bush"], 96, 9.0, true, [w, Color(0.9, 0.95, 0.8), w, w], 0.01, 0.25],
		["Grass", 2, -1, m["grass"], 160, 1.1, false, [w, Color(0.95, 1.0, 0.78), Color(0.85, 0.62, 0.68), Color(0.98, 0.9, 0.66)], 0.12, 0.45],
		["Reeds", 3, -1, m["reeds"], 96, 1.6, false, [w, w, w, w], 0.05, 0.35],
	]
	for s in specs:
		_node(plants, s[0], func() -> Node:
			var n := Node3D.new()
			n.set_script(load(DIR + "scatter.gd"))
			n.kind = s[1]
			n.only_variant = s[2]
			n.mesh = s[3]
			n.grid_n = s[4]
			n.spacing = s[5]
			n.shadows = s[6]
			var tints: Array[Color] = []
			for c in s[7]:
				tints.append(c)
			n.tints = tints
			n.sway = s[8]
			n.translucency = s[9]
			return n)
	var ship: Node3D = _node(root_node, "Ship", func() -> Node:
		var n := Node3D.new()
		n.set_script(load(DIR + "cog.gd"))
		return n)
	_node(ship, "Hull", func() -> Node:
		var h := MeshInstance3D.new()
		h.mesh = m["cog_hull"]
		h.material_override = hull_mat
		return h)
	_node(ship, "Sail", func() -> Node:
		var s := MeshInstance3D.new()
		s.mesh = m["cog_sail"]
		s.material_override = sail_mat
		s.position = Vector3(0, Cog.MAST_H - 1.1, -1.2)
		return s)
	_node(root_node, "Camera", func() -> Node:
		var c := Camera3D.new()
		c.fov = 50.0
		c.position = Vector3(60, 25, 90)
		c.rotation_degrees = Vector3(-12, 33, 0)
		return c)
	_node(root_node, "Chart", func() -> Node:
		var l := CanvasLayer.new()
		l.layer = 1
		return l)
	var hud_layer: CanvasLayer = _node(root_node, "HUD", func() -> Node:
		var l := CanvasLayer.new()
		l.layer = 2
		return l)
	_node(hud_layer, "Status", func() -> Node:
		var hud := Label.new()
		hud.position = Vector2(18, 14)
		hud.add_theme_color_override("font_color", Color(0.97, 0.95, 0.9))
		hud.add_theme_color_override("font_shadow_color", Color(0, 0, 0, 0.55))
		hud.add_theme_constant_override("shadow_offset_x", 1)
		hud.add_theme_constant_override("shadow_offset_y", 1)
		var th = load("res://route_map/ui/theme.tres")
		if th is Theme:
			hud.theme = th
		return hud)
	var mk: CanvasLayer = _node(root_node, "Marker", func() -> Node:
		var l := CanvasLayer.new()
		l.layer = 3
		return l)
	_node(mk, "ShipMarker", func() -> Node:
		var c := Control.new()
		c.set_script(load(DIR + "ship_marker.gd"))
		c.set_anchors_preset(Control.PRESET_FULL_RECT)
		c.mouse_filter = Control.MOUSE_FILTER_IGNORE
		return c)

	_own(root_node, root_node)
	var ps := PackedScene.new()
	var err := ps.pack(root_node)
	if err != OK:
		push_error("pack failed: %d" % err)
	else:
		ResourceSaver.save(ps, SCENE)
		print("saved ", SCENE)
	root_node.free()
	quit()


## Load a resource file, or make and save it if it doesn't exist yet.
func _material(file: String, make: Callable) -> Resource:
	var path := DIR + file
	if ResourceLoader.exists(path):
		return load(path)
	var r: Resource = make.call()
	ResourceSaver.save(r, path)
	print("saved ", path)
	return load(path)


## The child called `name`, or a new one from `make` if the scene doesn't have it.
func _node(parent: Node, name: String, make: Callable) -> Node:
	var n := parent.get_node_or_null(name)
	if n:
		return n
	n = make.call()
	n.name = name
	parent.add_child(n)
	print("  added ", parent.name, "/", name)
	return n


## Own only the nodes this tool placed in the tree (not children that scripts build at run time).
func _own(n: Node, owner_node: Node) -> void:
	for c in n.get_children():
		if c.owner == null and _placed(c):
			c.owner = owner_node
		if c.owner == owner_node:
			_own(c, owner_node)


func _placed(n: Node) -> bool:
	return n.name in ["WorldEnvironment", "Sun", "Terrain", "Sea", "Plants", "Ship", "Camera", "Chart", "HUD", "Marker",
		"Oaks", "Alders", "Birches", "Bushes", "Grass", "Reeds", "Hull", "Sail", "Status", "ShipMarker"]


func _environment() -> Environment:
	var env := Environment.new()
	var sky := Sky.new()
	var sm := ProceduralSkyMaterial.new()
	sm.sky_top_color = Color(0.3, 0.47, 0.68)
	sm.sky_horizon_color = Color(0.74, 0.8, 0.83)
	sm.ground_horizon_color = Color(0.7, 0.76, 0.78)
	sm.ground_bottom_color = Color(0.2, 0.28, 0.3)
	sm.sun_angle_max = 20.0
	sm.sky_curve = 0.12
	sky.sky_material = sm
	env.background_mode = Environment.BG_SKY
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_energy = 0.75
	env.reflected_light_source = Environment.REFLECTION_SOURCE_SKY
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.tonemap_exposure = 1.0
	env.fog_enabled = true
	env.fog_light_color = Color(0.72, 0.78, 0.82)
	env.fog_aerial_perspective = 0.35
	env.fog_sky_affect = 0.2
	env.fog_density = 0.00001
	env.glow_enabled = true
	env.glow_intensity = 0.25
	env.glow_bloom = 0.04
	return env


func _noise_tex(freq: float, normal: bool, seed_: int) -> NoiseTexture2D:
	var nt := NoiseTexture2D.new()
	nt.width = 512
	nt.height = 512
	nt.seamless = true
	nt.as_normal_map = normal
	nt.bump_strength = 2.5
	nt.generate_mipmaps = true
	var n := FastNoiseLite.new()
	n.seed = seed_
	n.frequency = freq
	n.fractal_octaves = 4
	nt.noise = n
	return nt


func _foam_tex() -> NoiseTexture2D:
	var nt := NoiseTexture2D.new()
	nt.width = 512
	nt.height = 512
	nt.seamless = true
	nt.generate_mipmaps = true
	var n := FastNoiseLite.new()
	n.noise_type = FastNoiseLite.TYPE_CELLULAR
	n.cellular_return_type = FastNoiseLite.RETURN_DISTANCE2_SUB
	n.frequency = 0.03
	n.fractal_type = FastNoiseLite.FRACTAL_FBM
	n.fractal_octaves = 3
	nt.noise = n
	return nt
