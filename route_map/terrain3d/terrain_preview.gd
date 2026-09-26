extends Node3D
## Stand-alone preview of the low-poly 3D world. Open terrain_preview.tscn and press F6.
## Right-drag: orbit · middle/left-drag: pan · wheel: zoom · 1–6: jump to places.

const Geo := preload("res://route_map/geo.gd")
const PLACES := [["Dorestad", 51.97, 5.34], ["Hedeby", 54.49, 9.57], ["Norway (Sognefjord)", 61.1, 6.0],
	["Kiev", 50.45, 30.52], ["Constantinople", 41.02, 28.98], ["Iceland", 64.9, -18.5]]

var world
var cam: Camera3D
var env_res: Environment
var sun_light: DirectionalLight3D
var yaw := -0.35
var pitch := 0.72
var dist := 420.0
var target := Vector2.ZERO
var _drag := 0


func _ready() -> void:
	var env := WorldEnvironment.new()
	var e := Environment.new()
	e.background_mode = Environment.BG_COLOR
	e.background_color = Color(0.78, 0.83, 0.86)
	e.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	e.ambient_light_color = Color(0.7, 0.74, 0.82)
	e.ambient_light_energy = 0.3
	e.fog_enabled = true
	e.fog_mode = Environment.FOG_MODE_DEPTH      # fades the far edge of the streamed world into haze
	e.fog_light_color = Color(0.78, 0.83, 0.86)
	e.fog_density = 1.0
	e.fog_depth_curve = 1.6
	env_res = e
	e.tonemap_exposure = 0.92
	e.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	env.environment = e
	add_child(env)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-34, -40, 0)
	sun.light_energy = 1.35
	sun.directional_shadow_max_distance = 3000.0
	sun.light_color = Color(1.0, 0.95, 0.86)
	sun.shadow_enabled = true
	sun.directional_shadow_blend_splits = true
	add_child(sun)
	sun_light = sun
	world = load("res://route_map/terrain3d/terrain_world.gd").new()
	add_child(world)
	cam = Camera3D.new()
	cam.far = 12000.0
	cam.fov = 45.0
	add_child(cam)
	_go(0)
	var tip := Label.new()
	tip.text = "3D preview · right-drag orbit · drag pan · wheel zoom · keys 1–6: Dorestad, Hedeby, Sognefjord, Kiev, Constantinople, Iceland"
	tip.position = Vector2(16, 12)
	var layer := CanvasLayer.new()
	layer.add_child(tip)
	add_child(layer)


func _go(i: int) -> void:
	target = Geo.project(PLACES[i][1], PLACES[i][2])


func _process(_d: float) -> void:
	var th: float = world.height_at(target)
	var centre := Vector3(target.x, maxf(th, 0.0), target.y)
	var offset := Vector3(sin(yaw) * cos(pitch), sin(pitch), cos(yaw) * cos(pitch)) * dist
	var pos := centre + offset
	# Never dip into a mountainside.
	var ground: float = maxf(world.height_at(Vector2(pos.x, pos.z)), 0.0)
	pos.y = maxf(pos.y, ground + 4.0)
	cam.position = pos
	cam.look_at(centre, Vector3.UP)
	world.focus = target
	var radius := clampf(dist * 7.0, 1800.0, 3400.0)
	world.view_radius = radius
	env_res.fog_depth_begin = radius * 0.4
	env_res.fog_depth_end = radius * 0.92
	sun_light.directional_shadow_max_distance = clampf(dist * 5.0, 250.0, 2500.0)


func _unhandled_input(e: InputEvent) -> void:
	if e is InputEventMouseButton:
		if e.button_index == MOUSE_BUTTON_WHEEL_UP and e.pressed:
			dist = maxf(60.0, dist / 1.15)
		elif e.button_index == MOUSE_BUTTON_WHEEL_DOWN and e.pressed:
			dist = minf(3000.0, dist * 1.15)
		elif e.button_index in [MOUSE_BUTTON_RIGHT, MOUSE_BUTTON_LEFT, MOUSE_BUTTON_MIDDLE]:
			_drag = e.button_index if e.pressed else 0
	elif e is InputEventMouseMotion and _drag != 0:
		if _drag == MOUSE_BUTTON_RIGHT:
			yaw -= e.relative.x * 0.006
			pitch = clampf(pitch + e.relative.y * 0.004, 0.25, 1.45)
		else:
			var fwd := Vector2(-sin(yaw), -cos(yaw))
			var right := Vector2(cos(yaw), -sin(yaw))
			target -= (right * e.relative.x - fwd * e.relative.y) * dist * 0.0022
	elif e is InputEventKey and e.pressed and e.keycode >= KEY_1 and e.keycode <= KEY_6:
		_go(e.keycode - KEY_1)
