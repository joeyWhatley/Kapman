@tool
extends Node2D
## The living ink sea: one rectangle drawn with sea.gdshader. Shows (without animation extras)
## in the editor too. The route map feeds it wind, storms and zoom at runtime.

const DIST := "res://route_map/data/sea_distance.png"
const CELL := 5.0            # land/sea grid cell (see geo.gd)
const DIST_SCALE := 3.0      # see tools/build_coast.gd

var map_size := Vector2.ZERO
var _mat: ShaderMaterial


func _ready() -> void:
	var tex: Texture2D = load(DIST)
	if tex == null:
		return
	map_size = Vector2(tex.get_width(), tex.get_height()) * CELL
	_mat = ShaderMaterial.new()
	_mat.shader = load("res://route_map/sea.gdshader")
	_mat.set_shader_parameter("dist_tex", tex)
	_mat.set_shader_parameter("map_size", map_size)
	_mat.set_shader_parameter("dist_units", 255.0 / DIST_SCALE * CELL)
	var noise := NoiseTexture2D.new()
	noise.width = 256
	noise.height = 256
	noise.seamless = true
	var fn := FastNoiseLite.new()
	fn.frequency = 0.02
	fn.fractal_octaves = 2
	noise.noise = fn
	_mat.set_shader_parameter("noise_tex", noise)
	_mat.set_shader_parameter("storms", [Vector4.ZERO, Vector4.ZERO, Vector4.ZERO, Vector4.ZERO])
	material = _mat
	queue_redraw()


func set_fog(tex: Texture2D, size: Vector2) -> void:
	if _mat:
		_mat.set_shader_parameter("fog_tex", tex)
		_mat.set_shader_parameter("fog_size", size)


func set_weather(wind: Vector2, strength: float, storms: Array, zoom: float, offset: Vector2) -> void:
	if _mat == null:
		return
	_mat.set_shader_parameter("flow_offset", offset)
	_mat.set_shader_parameter("wind", wind)
	_mat.set_shader_parameter("wind_strength", strength)
	_mat.set_shader_parameter("zoom", zoom)
	var packed: Array = []
	for i in 4:
		packed.append(storms[i] if i < storms.size() else Vector4.ZERO)
	_mat.set_shader_parameter("storms", packed)


func _draw() -> void:
	if map_size != Vector2.ZERO:
		draw_rect(Rect2(Vector2.ZERO, map_size), Color.WHITE)
