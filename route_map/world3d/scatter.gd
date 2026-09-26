@tool
extends Node3D
## One layer of plants around the camera: a grid of `grid_n` x `grid_n` cells, `spacing` metres
## apart, each holding at most one plant. A placement pass (scatter_place.gdshader in a small
## SubViewport) decides for every cell whether something grows and how tall; it only reruns when
## the grid moves. The plants are one MultiMesh drawn by scatter.gdshader.
##
## The grid is aligned to absolute cells (64-bit), so plants stay put when the origin re-centres.

## What grows in this layer (decided by the land cover rules in scatter_place.gdshader).
@export_enum("Trees", "Bushes", "Grass", "Reeds") var kind := 0
## Only draw this variant: trees 0 oak, 1 alder, 2 birch; grass 0 meadow, 1 dune, 2 heath, 3 marsh
## (-1 = all variants).
@export var only_variant := -1
## Any mesh works: swap in your own model (origin at its foot, about the size of the real thing).
@export var mesh: Mesh
## Cells across the grid (layer reach = cells x spacing / 2).
@export var grid_n := 128
## Metres between cells (at most one plant per cell).
@export var spacing := 8.0
@export var inner_skip := 0.0
@export var shadows := false
## Colour multipliers per variant.
@export var tints: Array[Color] = [Color.WHITE, Color.WHITE, Color.WHITE, Color.WHITE]
## How far the plant bends in the wind, per metre of height.
@export var sway := 0.03
@export var translucency := 0.3
@export_group("Distance band")
## Draw plants from this distance from the camera (metres)...
@export var band_min := 0.0
## ...up to this one. Give near and far layers the same spacing and touching bands, and one
## tree swaps from its detailed to its distant mesh in place.
@export var band_max := 1.0e9
## Shrink plants away from here to band_max (for the last layer, into the canopy shell).
@export var shrink_start := 1.0e9
## Scale every plant in this layer (far layers: fewer but bigger trees).
@export var size_mul := 1.0
## Shape tree variants as species (alder, birch).
@export var trees := false

var place_mat: ShaderMaterial
var draw_mat: ShaderMaterial
var _vp: SubViewport
var _mmi: MultiMeshInstance3D
var _cell := Vector2i(1 << 30, 0)


func setup(shared: ShaderMaterial) -> void:
	if _vp:
		_vp.queue_free()
		_mmi.queue_free()
	_cell = Vector2i(1 << 30, 0)
	_vp = SubViewport.new()
	_vp.size = Vector2i(grid_n, grid_n)
	_vp.use_hdr_2d = true
	_vp.transparent_bg = true
	_vp.disable_3d = true
	_vp.render_target_update_mode = SubViewport.UPDATE_ONCE
	add_child(_vp)
	var rect := ColorRect.new()
	rect.size = Vector2(grid_n, grid_n)
	place_mat = ShaderMaterial.new()
	place_mat.shader = load("res://route_map/world3d/scatter_place.gdshader")
	for p in ["height_tiles", "lvl_rect", "coarse_tex", "coarse_size", "m_per_unit"]:
		place_mat.set_shader_parameter(p, shared.get_shader_parameter(p))
	place_mat.set_shader_parameter("kind", kind)
	place_mat.set_shader_parameter("grid_n", grid_n)
	place_mat.set_shader_parameter("spacing", spacing)
	place_mat.set_shader_parameter("inner_skip", inner_skip)
	rect.material = place_mat
	_vp.add_child(rect)
	var mm := MultiMesh.new()
	mm.transform_format = MultiMesh.TRANSFORM_3D
	mm.mesh = mesh
	mm.instance_count = grid_n * grid_n
	# identity transforms: the shader places every plant itself
	var buf := PackedFloat32Array()
	buf.resize(grid_n * grid_n * 12)
	for i in grid_n * grid_n:
		buf[i * 12] = 1.0
		buf[i * 12 + 5] = 1.0
		buf[i * 12 + 10] = 1.0
	mm.buffer = buf
	_mmi = MultiMeshInstance3D.new()
	_mmi.multimesh = mm
	_mmi.custom_aabb = AABB(Vector3(-1e6, -1000, -1e6), Vector3(2e6, 4000, 2e6))
	_mmi.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_ON if shadows else GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	draw_mat = ShaderMaterial.new()
	draw_mat.shader = load("res://route_map/world3d/scatter.gdshader")
	draw_mat.set_shader_parameter("place_tex", _vp.get_texture())
	draw_mat.set_shader_parameter("grid_n", grid_n)
	draw_mat.set_shader_parameter("spacing", spacing)
	var reach := grid_n * 0.5 * spacing
	# a layer without a band of its own fades out before the edge of its grid
	draw_mat.set_shader_parameter("band_min", band_min)
	var own_band := band_max < 1.0e8
	draw_mat.set_shader_parameter("band_max", band_max if own_band else reach * 0.97)
	draw_mat.set_shader_parameter("shrink_start", shrink_start if own_band else reach * 0.78)
	draw_mat.set_shader_parameter("size_mul", size_mul)
	draw_mat.set_shader_parameter("trees", trees)
	draw_mat.set_shader_parameter("sway", sway)
	draw_mat.set_shader_parameter("translucency", translucency)
	# foliage: softer, lighter bands than hard surfaces
	draw_mat.set_shader_parameter("toon_soft", 0.14)
	draw_mat.set_shader_parameter("toon_half_level", 0.68)
	draw_mat.set_shader_parameter("only_variant", only_variant)
	for i in 4:
		draw_mat.set_shader_parameter("tint%d" % i, tints[i])
	_mmi.material_override = draw_mat
	add_child(_mmi)


## Re-centre on the camera (absolute metres) and pass the world's origin uniforms along.
func follow(cam_x: float, cam_z: float, origin_x: float, origin_z: float, origin_map: Vector2, noise_off: PackedVector2Array) -> void:
	var c := Vector2i(int(floor(cam_x / spacing)), int(floor(cam_z / spacing)))
	# move in steps of 8 cells so the placement pass reruns rarely
	c = Vector2i(c.x - posmod(c.x, 8), c.y - posmod(c.y, 8))
	if c == _cell:
		return
	_cell = c
	var local := Vector2(c.x * spacing - origin_x, c.y * spacing - origin_z)
	place_mat.set_shader_parameter("grid_origin", local)
	place_mat.set_shader_parameter("grid_key", Vector2(posmod(c.x, 256), posmod(c.y, 256)))
	place_mat.set_shader_parameter("origin_map", origin_map)
	place_mat.set_shader_parameter("noise_off", noise_off)
	draw_mat.set_shader_parameter("grid_origin", local)
	_vp.render_target_update_mode = SubViewport.UPDATE_ONCE


## Call after the origin re-centres: the local grid position changes even if the cell doesn't.
func invalidate() -> void:
	_cell = Vector2i(1 << 30, 0)


func set_clouds(tex: Texture2D, off: Vector2, off2: Vector2, cover: float, shadow: float) -> void:
	draw_mat.set_shader_parameter("cloud_tex", tex)
	draw_mat.set_shader_parameter("cloud_off", off)
	draw_mat.set_shader_parameter("cloud_off2", off2)
	draw_mat.set_shader_parameter("cloud_cover", cover)
	draw_mat.set_shader_parameter("cloud_shadow", shadow)


func set_wind(dir: Vector2, strength: float) -> void:
	draw_mat.set_shader_parameter("wind_dir", dir)
	draw_mat.set_shader_parameter("wind_strength", strength)
