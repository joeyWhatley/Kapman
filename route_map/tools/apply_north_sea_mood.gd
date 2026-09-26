extends SceneTree
## Updates route_map/world3d/sea_view.tscn in place (all other edits are kept):
##  - trees: Oaks/Alders/Birches become one Trees layer (species shaped in the shader) plus
##    TreesMid and TreesFar, which carry the same trees out to 1.5 km before the canopy shell;
##  - rebuilds the tree, bush and grass meshes (muted colours) and adds meshes/oak_far.res;
##  - the North Sea mood: overcast sky shader with moving clouds and a warm break toward a low
##    sun, soft grey light, sea mist, muted colour grading.
##   godot --headless --path <project> --script res://route_map/tools/apply_north_sea_mood.gd

const DIR := "res://route_map/world3d/"
const SCENE := DIR + "sea_view.tscn"
const Plants := preload("res://route_map/world3d/plants.gd")


func _initialize() -> void:
	ResourceSaver.save(Plants.tree(1), DIR + "meshes/oak.res")
	ResourceSaver.save(Plants.tree(0), DIR + "meshes/oak_far.res")
	ResourceSaver.save(Plants.bush(), DIR + "meshes/bush.res")
	ResourceSaver.save(Plants.grass(9, 0.55, 0.25), DIR + "meshes/grass.res")
	var near_mesh: Mesh = ResourceLoader.load(DIR + "meshes/oak.res", "", ResourceLoader.CACHE_MODE_REPLACE)
	var far_mesh: Mesh = ResourceLoader.load(DIR + "meshes/oak_far.res", "", ResourceLoader.CACHE_MODE_REPLACE)
	ResourceLoader.load(DIR + "meshes/bush.res", "", ResourceLoader.CACHE_MODE_REPLACE)
	ResourceLoader.load(DIR + "meshes/grass.res", "", ResourceLoader.CACHE_MODE_REPLACE)
	var root: Node = (load(SCENE) as PackedScene).instantiate()
	var plants: Node = root.get_node("Plants")
	for old in ["Alders", "Birches"]:
		var n := plants.get_node_or_null(old)
		if n:
			plants.remove_child(n)
			n.free()
			print("removed Plants/", old)
	var tints: Array[Color] = [Color.WHITE, Color(0.8, 0.9, 0.84), Color(1.18, 1.14, 0.82), Color.WHITE]
	var trees: Node = plants.get_node_or_null("Oaks")
	if trees:
		trees.name = "Trees"
	else:
		trees = plants.get_node_or_null("Trees")
	_configure(trees, near_mesh, 96, 8.0, 0.0, 300.0, 1.0e9, 1.0, true, tints)
	var mid: Node = _layer(plants, "TreesMid", trees.get_index() + 1)
	_configure(mid, far_mesh, 200, 8.0, 300.0, 750.0, 1.0e9, 1.0, false, tints)
	var far: Node = _layer(plants, "TreesFar", mid.get_index() + 1)
	_configure(far, far_mesh, 200, 16.0, 750.0, 1500.0, 1300.0, 1.45, false, tints)
	# mood
	var we: WorldEnvironment = root.get_node("WorldEnvironment")
	we.environment = _environment()
	var sun: DirectionalLight3D = root.get_node("Sun")
	sun.rotation_degrees = Vector3(-17, -118, 0)      # low, in the south-west, behind the cloud
	sun.light_color = Color(1.0, 0.86, 0.68)
	sun.light_energy = 0.95
	sun.shadow_blur = 1.6
	var ps := PackedScene.new()
	ps.pack(root)
	ResourceSaver.save(ps, SCENE)
	print("saved ", SCENE)
	root.free()
	quit()


func _layer(plants: Node, name: String, index: int) -> Node:
	var n := plants.get_node_or_null(name)
	if n == null:
		n = Node3D.new()
		n.set_script(load(DIR + "scatter.gd"))
		n.name = name
		plants.add_child(n)
		plants.move_child(n, index)
		n.owner = plants.owner if plants.owner else plants.get_parent()
		print("added Plants/", name)
	return n


func _configure(n: Node, mesh: Mesh, cells: int, spacing: float, bmin: float, bmax: float, shrink: float, size: float, shadows: bool, tints: Array[Color]) -> void:
	n.kind = 0
	n.only_variant = -1
	n.mesh = mesh
	n.grid_n = cells
	n.spacing = spacing
	n.band_min = bmin
	n.band_max = bmax
	n.shrink_start = shrink
	n.size_mul = size
	n.shadows = shadows
	n.trees = true
	n.tints = tints
	n.sway = 0.004
	n.translucency = 0.2


func _environment() -> Environment:
	var env := Environment.new()
	var sky := Sky.new()
	var sm := ShaderMaterial.new()
	sm.shader = load(DIR + "sky.gdshader")
	var nt := NoiseTexture2D.new()
	nt.width = 1024
	nt.height = 1024
	nt.seamless = true
	nt.generate_mipmaps = true
	var n := FastNoiseLite.new()
	n.seed = 7
	n.frequency = 0.004
	n.fractal_octaves = 5
	n.fractal_gain = 0.55
	nt.noise = n
	sm.set_shader_parameter("cloud_noise", nt)
	sky.sky_material = sm
	sky.radiance_size = Sky.RADIANCE_SIZE_128
	env.background_mode = Environment.BG_SKY
	env.sky = sky
	env.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	env.ambient_light_energy = 0.95
	env.reflected_light_source = Environment.REFLECTION_SOURCE_SKY
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.tonemap_exposure = 1.05
	env.tonemap_white = 1.2
	env.fog_enabled = true
	env.fog_light_color = Color(0.6, 0.64, 0.66)
	env.fog_light_energy = 1.0
	env.fog_sun_scatter = 0.25                          # a warm lift in the mist toward the sun
	env.fog_aerial_perspective = 0.5
	env.fog_sky_affect = 0.35
	env.fog_density = 0.00005
	env.fog_height = 12.0
	env.fog_height_density = 0.0012                     # mist lying on the water and the low land
	env.glow_enabled = true
	env.glow_intensity = 0.3
	env.glow_bloom = 0.03
	env.glow_hdr_threshold = 0.9
	env.adjustment_enabled = true
	env.adjustment_saturation = 0.8
	env.adjustment_contrast = 1.06
	env.adjustment_brightness = 1.0
	return env
