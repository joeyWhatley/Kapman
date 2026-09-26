extends Node2D
## Kāpman route map: an ink chart under fog of war, and the voyage planner.
## Docked → New voyage → pick a vessel → click a destination → edit the course → set sail.
## Hazards hide under the fog; what you know shows as subtle marks on your course.

const Geo := preload("res://route_map/geo.gd")
const MapData := preload("res://route_map/map_data.gd")
const PortIndex := preload("res://route_map/port_index.gd")
const Voyage := preload("res://route_map/voyage.gd")
const Fog := preload("res://route_map/fog.gd")
const Hazards := preload("res://route_map/hazards.gd")
const Style := preload("res://route_map/style.gd")
const TradeState := preload("res://route_map/trade_state.gd")
const D := preload("res://scripts/data.gd")
const BattleRules := preload("res://route_map/battle/battle_rules.gd")
const FOG_SHADER := preload("res://route_map/fog_mask.gdshader")
const Weather := preload("res://route_map/weather.gd")
const Currents := preload("res://route_map/currents.gd")

const SECONDS_PER_DAY := 1.6
const FAST := 5.0
const REVEAL_RADIUS := 42.0
const START_YEAR := 793
const SHIP := {"name": "Seamew", "kind": "small Frisian trader"}
const MARK_SPACING := 9.0
## Home waters a Frisian trader already knows: the Rhine mouth, the Frisian islands, out to the Ems.
const HOME_WATERS := [[51.95, 4.05], [52.25, 4.30], [52.75, 4.55], [53.15, 4.85], [53.40, 5.40], [53.50, 6.10], [53.55, 6.80]]

enum Mode { DOCKED, PICK_VESSEL, PICK_DEST, EDIT, SAILING }

var state := TradeState.new()
var index := PortIndex.new()
var map := MapData.new()
var fog := Fog.new()
var hazards := Hazards.new()
var weather := Weather.new()
var currents := Currents.new()
var _dist := 0.0          # map units sailed along the course
var _rowing := false
var _ship_speed := 0.0
var sea_layer
var cloud_layer
var ambient
var night_rect: ColorRect
var _sea_offset := Vector2.ZERO
var view: SubViewport         # the flat chart renders here; shown tilted when zoomed in
var _display: TextureRect
var tilt := 0.0
var _h := [1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0]   # screen uv -> chart uv homography a..h
const TILT_START := 1.4
const TILT_FULL := 3.2
const TILT_MAX := 0.42
var _cloud_offset := Vector2.ZERO
var mode := Mode.DOCKED
var voyage = null
var clock := 0.0          # days since the start
var speed := 1.0          # 0 = paused
var sail_t := 0.0         # days of progress along the course
var _voyage_start := 0.0
var _hold := 0.0          # days to wait in place (storms, fog, soundings) before moving on
var _incidents: Array = []  # [time, zone] sorted by time
var _in_incident := false
var _last_reveal := Vector2.INF
var _follow := true

var camera: Camera2D
var zoom := 1.8
var _zoom_target := 1.8
var _zoom_anchor := Vector2.ZERO
var _pan_vel := Vector2.ZERO
var _opening_tw: Tween
var _estimates := {}
var _trail_last := Vector2.INF
var patches: Array = []
var coast_rough
var coast_detail
var lakes
var rivers
var fog_wash
var hazard_layer
var ports_layer
var route_layer
var compass
var _last_motion_ms := 0

var strip_label: Label
var silver_label: Label
var date_label: Label
var time_label: Label
var btn_play: Button
var btn_pause: Button
var btn_fast: Button
var action_label: Label
var action_hint: Label
var action_box: HBoxContainer
var port_panel
var music
var incident
var battle_screen
var _over := false
var toast: Label
var tip: PanelContainer
var tip_label: Label
var _toast_tween: Tween

var _pressed := false
var _press_screen := Vector2.ZERO
var _panning := false
var _drag_handle := -1


func _ready() -> void:
	map.load_data()
	index.build($Map/Ports)
	state.ports = index.trade_data()
	state.home = index.home
	state.new_game()
	state.draw_event()
	state.roll_mood(state.roll(), state.roll())
	for n in $Map/Corrections.get_children():
		if n is KapmanLandPatch:
			patches.append(n)
	map.apply_patches(patches)
	fog.setup(map.map_size())
	hazards.build($Map/Hazards)
	currents.build(get_node_or_null("Map/Currents"))
	weather.setup(map.map_size(), state.rng.randi())
	_bind_world()
	_bind_hud()
	fog.reveal(index.position(index.home), 75.0)
	fog.reveal_path(index.approach(index.home), REVEAL_RADIUS)
	var home_waters := PackedVector2Array()
	for ll in HOME_WATERS:
		home_waters.append(Geo.project(ll[0], ll[1]))
	fog.reveal_path(home_waters, 48.0)
	fog.flush(true)
	hazards.learn_from_fog(fog)
	route_layer.ship_pos = index.position(index.home)
	_setup_tilt()
	_opening_view()
	_set_mode(Mode.DOCKED)
	port_panel.show_port(index.home, true)
	_toast("Docked at %s. Press New voyage (or ▶) to plan your first course." % index.home, 7.0)

## Open on the whole known world, then glide in towards home.
func _opening_view() -> void:
	var world := map.map_size()
	camera.position = Vector2(world.x * 0.47, world.y * 0.52)
	zoom = 0.34
	_apply_zoom()
	var tw := create_tween().set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tw.tween_interval(1.4)
	tw.tween_method(_opening_step, 0.0, 1.0, 2.2)
	_opening_tw = tw


func _opening_step(t: float) -> void:
	var world := map.map_size()
	zoom = lerpf(0.34, 1.1, t)
	_zoom_target = zoom
	camera.position = Vector2(world.x * 0.47, world.y * 0.52).lerp(index.position(index.home) + Vector2(-40, -60), t)
	_apply_zoom()


func _season_now() -> int:
	return state.season_for((int(clock) % 60) + 1)


# --- World -----------------------------------------------------------------

func _fog_material(invert: float, strength: float, grain: float) -> ShaderMaterial:
	var m := ShaderMaterial.new()
	m.shader = FOG_SHADER
	m.set_shader_parameter("fog", fog.texture)
	m.set_shader_parameter("map_size", fog.size_units())
	m.set_shader_parameter("invert", invert)
	m.set_shader_parameter("strength", strength)
	m.set_shader_parameter("grain", grain)
	return m


func _bind_world() -> void:
	var rect := Rect2(Vector2.ZERO, map.map_size())
	var paper = $Map/Paper
	paper.rect = rect
	paper.queue_redraw()
	fog_wash = $Map/FogWash
	fog_wash.rect = rect
	fog_wash.material = _fog_material(1.0, 0.5, 0.35)
	var clouds := NoiseTexture2D.new()
	clouds.width = 256
	clouds.height = 256
	clouds.seamless = true
	var noise := FastNoiseLite.new()
	noise.frequency = 0.012
	noise.fractal_octaves = 3
	clouds.noise = noise
	fog_wash.material.set_shader_parameter("clouds", clouds)
	coast_rough = $Map/CoastRough
	coast_rough.runs = map.rough_runs
	coast_rough.material = _fog_material(1.0, 1.0, 0.0)
	coast_detail = $Map/CoastDetail
	coast_detail.runs = map.detail_runs
	coast_detail.medium = map.medium_runs
	coast_detail.far = map.far_runs
	coast_detail.water = map.water_runs
	coast_detail.material = _fog_material(0.0, 1.0, 0.0)
	lakes = $Map/Lakes
	lakes.runs = map.lake_runs
	lakes.material = _fog_material(0.0, 1.0, 0.0)
	for patch in patches:
		patch.paper = Style.PAPER
		patch.ink = Style.INK
		patch.material = _fog_material(0.0, 1.0, 0.0)
	rivers = $Map/Rivers
	var river_runs: Array = []
	for rv in map.river_runs:
		river_runs.append(rv["pts"])
	for p in index.names():
		river_runs.append(index.approach(p).slice(1))
	rivers.runs = river_runs
	rivers.material = _fog_material(0.0, 1.0, 0.0)
	hazard_layer = $Map/HazardMarks
	hazard_layer.hazards = hazards
	ports_layer = $Map/PortGlyphs
	ports_layer.index = index
	ports_layer.fog = fog
	ports_layer.here = index.home
	route_layer = $Map/Route
	compass = get_node_or_null("Map/Compass")
	sea_layer = get_node_or_null("Map/Sea")
	if sea_layer:
		sea_layer.set_fog(fog.texture, fog.size_units())
	cloud_layer = get_node_or_null("Map/CloudShadows")
	if cloud_layer:
		var cm := ShaderMaterial.new()
		cm.shader = load("res://route_map/clouds.gdshader")
		var cn := NoiseTexture2D.new()
		cn.width = 256
		cn.height = 256
		cn.seamless = true
		var cfn := FastNoiseLite.new()
		cfn.frequency = 0.01
		cfn.fractal_octaves = 3
		cn.noise = cfn
		cm.set_shader_parameter("noise_tex", cn)
		cloud_layer.material = cm
		cloud_layer.rect = Rect2(Vector2.ZERO, map.map_size())
		cloud_layer.color = Color.WHITE
	ambient = get_node_or_null("Map/Ambient")
	if ambient:
		ambient.setup(self)
	camera = $Camera
	camera.make_current()

func _apply_zoom() -> void:
	camera.zoom = Vector2(zoom, zoom)
	_update_tilt()
	get_tree().set_group("zoom_aware", "zoom", zoom)
	# Tiled ink layers and hazards only redraw when the zoom crosses a band (see coast_layer.gd).
	for layer in [coast_rough, coast_detail, lakes, rivers, hazard_layer, compass]:
		if layer:
			layer.zoom = zoom
	for layer in [ports_layer, route_layer] + patches:
		layer.zoom = zoom
		layer.queue_redraw()


## Smooth zoom: set a target; _camera_step eases toward it, keeping the point under the cursor still.
func _zoom_at(factor: float, screen_pos: Vector2) -> void:
	_stop_opening()
	_zoom_target = clampf(_zoom_target * factor, 0.3, 8.0)
	_zoom_anchor = screen_pos


func _stop_opening() -> void:
	if _opening_tw and _opening_tw.is_valid():
		_opening_tw.kill()
		_zoom_target = zoom


func _camera_step(delta: float) -> void:
	if absf(_zoom_target - zoom) > 0.0005:
		var before := _screen_to_world(_zoom_anchor)
		zoom = lerpf(zoom, _zoom_target, 1.0 - exp(-14.0 * delta))
		if absf(_zoom_target - zoom) <= 0.0005:
			zoom = _zoom_target
		_apply_zoom()
		camera.position += before - _screen_to_world(_zoom_anchor)
		_clamp_camera()
	if not _pressed and _pan_vel.length() > 5.0:
		camera.position -= _pan_vel * delta / zoom
		_pan_vel *= exp(-5.0 * delta)
		_clamp_camera()


func _screen_to_world(sp: Vector2) -> Vector2:
	var vs := get_viewport().get_visible_rect().size
	var src := _warp(sp / vs) * vs
	return camera.position + (src - vs / 2.0) / zoom


## Screen uv -> chart uv through the tilt homography.
func _warp(uv: Vector2) -> Vector2:
	var w: float = _h[6] * uv.x + _h[7] * uv.y + 1.0
	return Vector2((_h[0] * uv.x + _h[1] * uv.y + _h[2]) / w, (_h[3] * uv.x + _h[4] * uv.y + _h[5]) / w)


## The chart, its camera, the night tint and the lights move into an off-screen view that is shown
## full-screen through tilt.gdshader. The scene file is unchanged (you still edit Map as before).
func _setup_tilt() -> void:
	var vs := get_viewport().get_visible_rect().size
	view = SubViewport.new()
	view.name = "ChartView"
	view.size = Vector2i(get_window().size)
	view.size_2d_override = Vector2i(vs)
	view.size_2d_override_stretch = true
	view.msaa_2d = Viewport.MSAA_4X
	view.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	add_child(view)
	for n in [$Map, camera, get_node("NightLayer"), get_node("LightsLayer")]:
		n.reparent(view)
	camera.make_current()
	var layer := CanvasLayer.new()
	layer.name = "Display"
	layer.layer = 0
	add_child(layer)
	_display = TextureRect.new()
	_display.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_display.stretch_mode = TextureRect.STRETCH_SCALE
	_display.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
	_display.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_display.texture = view.get_texture()
	var mat := ShaderMaterial.new()
	mat.shader = load("res://route_map/tilt.gdshader")
	mat.set_shader_parameter("haze", Style.PAPER)
	_display.material = mat
	layer.add_child(_display)
	get_viewport().size_changed.connect(_on_resized)
	_update_tilt()


func _on_resized() -> void:
	if view:
		view.size = Vector2i(get_window().size)
		view.size_2d_override = Vector2i(get_viewport().get_visible_rect().size)


## Tilt grows as you zoom in past TILT_START. Bottom of the screen = near, top = far.
func _update_tilt() -> void:
	tilt = smoothstep(TILT_START, TILT_FULL, zoom) * TILT_MAX
	var m := tilt * 0.5
	# Square -> quad (Heckbert): screen corners (0,0),(1,0),(1,1),(0,1) -> chart (0,0),(1,0),(1-m,1),(m,1)
	var x := [0.0, 1.0, 1.0 - m, m]
	var y := [0.0, 0.0, 1.0, 1.0]
	var dx1: float = x[1] - x[2]
	var dx2: float = x[3] - x[2]
	var dx3: float = x[0] - x[1] + x[2] - x[3]
	var dy1: float = y[1] - y[2]
	var dy2: float = y[3] - y[2]
	var dy3: float = y[0] - y[1] + y[2] - y[3]
	var den := dx1 * dy2 - dx2 * dy1
	var g := (dx3 * dy2 - dx2 * dy3) / den
	var h := (dx1 * dy3 - dx3 * dy1) / den
	_h = [x[1] - x[0] + g * x[1], x[3] - x[0] + h * x[3], x[0], y[1] - y[0] + g * y[1], y[3] - y[0] + h * y[3], y[0], g, h]
	if _display:
		_display.material.set_shader_parameter("h1", Vector4(_h[0], _h[1], _h[2], _h[3]))
		_display.material.set_shader_parameter("h2", Vector4(_h[4], _h[5], _h[6], _h[7]))
		_display.material.set_shader_parameter("tilt", tilt)
	if route_layer:
		route_layer.tilt = tilt
	if ports_layer:
		ports_layer.tilt = tilt


func _clamp_camera() -> void:
	var s := map.map_size()
	camera.position = camera.position.clamp(Vector2.ZERO, s)


# --- HUD -------------------------------------------------------------------

func _bind_hud() -> void:
	var hud = $HUD
	if hud.get_node_or_null("Root/Vignette") == null:
		var vig := ColorRect.new()
		vig.name = "Vignette"
		vig.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
		vig.mouse_filter = Control.MOUSE_FILTER_IGNORE
		var mat := ShaderMaterial.new()
		mat.shader = load("res://route_map/vignette.gdshader")
		vig.material = mat
		hud.get_node("Root").add_child(vig)
		hud.get_node("Root").move_child(vig, 0)
	# Layering: map (0) < night tint (1) < port lights, following the camera (2) < HUD (3).
	hud.layer = 3
	var night_layer := CanvasLayer.new()
	night_layer.name = "NightLayer"
	night_layer.layer = 1
	add_child(night_layer)
	night_rect = ColorRect.new()
	night_rect.name = "Night"
	night_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	night_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	night_rect.color = Color(0, 0, 0, 0)
	night_layer.add_child(night_rect)
	var lights_layer := CanvasLayer.new()
	lights_layer.name = "LightsLayer"
	lights_layer.layer = 2
	lights_layer.follow_viewport_enabled = true
	add_child(lights_layer)
	var lights := Node2D.new()
	lights.name = "Lights"
	lights_layer.add_child(lights)
	if ambient:
		ambient.lights = lights
	strip_label = hud.strip_label
	time_label = hud.time_label
	btn_play = hud.btn_play
	btn_pause = hud.btn_pause
	btn_fast = hud.btn_fast
	action_label = hud.action_label
	action_hint = hud.action_hint
	action_box = hud.action_box
	toast = hud.toast
	tip = hud.tip
	tip_label = hud.tip_label
	port_panel = hud.port_panel
	music = hud.music
	incident = hud.incident
	battle_screen = hud.battle_screen
	_build_status_row()
	btn_play.pressed.connect(_on_play)
	btn_pause.pressed.connect(_on_pause)
	btn_fast.pressed.connect(_on_fast)
	port_panel.state = state
	port_panel.index = index
	port_panel.buy.connect(_on_buy)
	port_panel.sell.connect(_on_sell)
	port_panel.closed.connect(func(): port_panel.hide_panel(); ports_layer.selected = ""; ports_layer.queue_redraw())
	port_panel.repair.connect(func(): if state.why_not_repair() == "": state.repair(); port_panel.refresh(); _update_strip())
	port_panel.hire.connect(func(): if state.why_not_hire() == "": state.hire(); port_panel.refresh(); _update_strip())

## Status strip: ship · hold · [sceatta] silver · date.
func _build_status_row() -> void:
	var strip: Control = strip_label.get_parent()
	var row := HBoxContainer.new()
	row.name = "Row"
	row.add_theme_constant_override("separation", 8)
	strip.remove_child(strip_label)
	strip.add_child(row)
	row.add_child(strip_label)
	var coin := TextureRect.new()
	coin.texture = _sceatta()
	coin.custom_minimum_size = Vector2(20, 20)
	coin.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	coin.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	coin.tooltip_text = "Silver sceattas"
	row.add_child(coin)
	silver_label = Label.new()
	silver_label.add_theme_font_override("font", Style.font("SemiBold"))
	row.add_child(silver_label)
	date_label = Label.new()
	date_label.add_theme_font_override("font", Style.font("Medium"))
	row.add_child(date_label)


## A small silver sceatta, the Frisian penny of the 700s, with a "porcupine" on its face.
func _sceatta() -> ImageTexture:
	var n := 48
	var img := Image.create(n, n, false, Image.FORMAT_RGBA8)
	img.fill(Color(0, 0, 0, 0))
	var c := Vector2(n, n) / 2.0
	for y in n:
		for x in n:
			var d := Vector2(x + 0.5, y + 0.5).distance_to(c)
			if d > 22.5:
				continue
			var edge := clampf(22.5 - d, 0.0, 1.0)
			var col := Color(0.8, 0.79, 0.74).darkened(0.25 * smoothstep(12.0, 22.0, d))
			if d > 18.0 and d < 20.5 and int(atan2(y - c.y, x - c.x) * 7.0 + 30.0) % 2 == 0:
				col = col.darkened(0.45)                          # beaded border
			col.a = edge
			img.set_pixel(x, y, col)
	# The porcupine: a curved body with a comb of spines.
	var ink := Color(0.35, 0.33, 0.3)
	for i in 60:
		var t := i / 59.0
		var p := c + Vector2(lerpf(-11.0, 11.0, t), 4.0 - sin(t * PI) * 7.0)
		img.set_pixel(int(p.x), int(p.y), ink)
		img.set_pixel(int(p.x), int(p.y) + 1, ink)
		if i % 6 == 0:
			for k in 6:
				img.set_pixel(int(p.x), int(p.y) - 2 - k, ink)
	return ImageTexture.create_from_image(img)


func _small_button(text: String, tooltip: String, cb: Callable) -> Button:
	var b := Button.new()
	b.text = text
	b.tooltip_text = tooltip
	b.custom_minimum_size = Vector2(38, 28)
	b.pressed.connect(cb)
	return b


func _action_button(text: String, cb: Callable, enabled := true, primary := false) -> Button:
	var b := Button.new()
	b.text = text
	if primary:
		b.theme_type_variation = "PrimaryButton"
	b.custom_minimum_size = Vector2(0, 30)
	b.disabled = not enabled
	b.pressed.connect(cb)
	action_box.add_child(b)
	return b


func _toast(text: String, seconds := 3.5) -> void:
	toast.text = text
	if _toast_tween:
		_toast_tween.kill()
	_toast_tween = create_tween()
	_toast_tween.tween_property(toast, "modulate:a", 1.0, 0.2)
	_toast_tween.tween_interval(seconds)
	_toast_tween.tween_property(toast, "modulate:a", 0.0, 0.6)


func _date_text() -> String:
	var day := int(clock) + 1
	var year := START_YEAR + (day - 1) / 60
	var season: String = D.SEASONS[_season_now()]["name"]
	return "Day %d, %s %d" % [day, season, year]


func _update_strip() -> void:
	strip_label.text = "%s     Hold %d/%d   " % [SHIP["name"], state.hold.size(), D.START["hold"]]
	if silver_label:
		silver_label.text = str(state.silver)
		date_label.text = "     " + _date_text()
	else:
		strip_label.text += "  %d silver     %s" % [state.silver, _date_text()]
	var sailing := mode == Mode.SAILING
	btn_play.disabled = _in_incident or not (sailing or mode == Mode.DOCKED or (mode == Mode.EDIT and voyage != null and voyage.ok()))
	btn_play.tooltip_text = "Plan a voyage" if mode == Mode.DOCKED else ("Set sail" if mode == Mode.EDIT else "Start / resume (Space)")
	btn_pause.disabled = _in_incident or not sailing
	btn_fast.disabled = _in_incident or not (sailing or (mode == Mode.EDIT and voyage != null and voyage.ok()))
	if not sailing:
		time_label.text = "In port" if mode != Mode.EDIT else "Ready"
	elif speed == 0.0 or _in_incident:
		time_label.text = "Paused"
	else:
		time_label.text = "%d×" % int(speed)
	if hazard_layer.season != _season_now():
		hazard_layer.season = _season_now()
		hazard_layer.queue_redraw()


# --- Modes -----------------------------------------------------------------

func _set_mode(m: Mode) -> void:
	mode = m
	for c in action_box.get_children():
		c.queue_free()
	action_hint.text = ""
	route_layer.editing = m == Mode.EDIT
	tip.visible = false
	match m:
		Mode.DOCKED:
			action_label.text = "In port at %s" % state.port
			_action_button("New voyage", func(): _set_mode(Mode.PICK_VESSEL), true, true)
			_action_button("Market", func(): _select_port(state.port))
		Mode.PICK_VESSEL:
			action_label.text = "Which vessel?"
			var desc := "%s  ·  %s  ·  hold %d/%d  ·  crew %d  ·  hull %d/%d" % [SHIP["name"], SHIP["kind"], state.hold.size(), D.START["hold"], state.crew, state.hull, D.START["hull"]]
			_action_button(desc, func(): _set_mode(Mode.PICK_DEST), true, true)
			_action_button("Cancel", _cancel_voyage)
		Mode.PICK_DEST:
			action_label.text = "Where to? Click a port on the chart."
			action_hint.text = "You can shape the course once it's drawn."
			_action_button("Cancel", _cancel_voyage)
		Mode.EDIT:
			_refresh_readout()
		Mode.SAILING:
			_refresh_sailing_label()
	action_hint.visible = action_hint.text != ""
	_update_strip()


func _cancel_voyage() -> void:
	voyage = null
	route_layer.voyage = null
	route_layer.marks = []
	ports_layer.destination = ""
	ports_layer.queue_redraw()
	route_layer.queue_redraw()
	_set_mode(Mode.DOCKED)


func _evaluate(v) -> void:
	var w: Dictionary = hazards.wind(_season_now())
	v.evaluate(map, w["dir"], w["strength"], state.crew, currents.current_at)


func _plan_to(dest: String) -> void:
	var v := Voyage.new()
	var course := map.find_route(index.anchorage(state.port), index.anchorage(dest))
	v.setup(state.port, dest, course, index.approach(state.port), index.approach(dest))
	_evaluate(v)
	voyage = v
	route_layer.voyage = v
	_update_marks()
	ports_layer.destination = dest
	ports_layer.queue_redraw()
	route_layer.queue_redraw()
	_set_mode(Mode.EDIT)


## Warning marks where the course crosses hazards you know or have heard of (active this season).
func _update_marks() -> void:
	var marks: Array = []
	var season := _season_now()
	for s in voyage.samples(MARK_SPACING):
		for z in hazards.zones:
			var k: int = hazards.knowledge[z["id"]]
			if k >= 1 and hazards.active(z, season) and hazards.contains(z, s[0]):
				marks.append([s[0], s[1], z["kind"], k, z["id"]])
				break
	route_layer.marks = marks
	route_layer.hover_mark = -1


func _known_dangers() -> Array:
	var seen := {}
	var out: Array = []
	for m in route_layer.marks:
		if seen.has(m[4]):
			continue
		seen[m[4]] = true
		var z: Dictionary = hazards.zone(m[4])
		out.append(z["name"] if m[3] >= 2 else "something the sailors whisper about")
	return out


func _fogged_share() -> float:
	var total := 0
	var fogged := 0
	for i in voyage.sea.size() - 1:
		var a: Vector2 = voyage.sea[i]
		var b: Vector2 = voyage.sea[i + 1]
		var steps := int(a.distance_to(b) / 10.0) + 1
		for s in steps:
			total += 1
			if fog.charted(a.lerp(b, float(s) / steps)) < 0.5:
				fogged += 1
	return float(fogged) / maxi(1, total)


func _wind_text() -> String:
	var from: String = hazards.wind(_season_now())["from"]
	var s: float = voyage.wind_score
	if s > 0.35:
		return "wind from %s, mostly behind you" % from
	if s < -0.35:
		return "wind from %s, mostly against you" % from
	return "wind from %s, across your course" % from


func _refresh_readout() -> void:
	if mode != Mode.EDIT or voyage == null:
		return
	for c in action_box.get_children():
		c.queue_free()
	var row_km := Voyage.km(voyage.row_units)
	var sea_txt := "%d km under sail" % Voyage.km(voyage.sea_units - voyage.row_units) if row_km == 0 else "%d km under sail, %d km rowing" % [Voyage.km(voyage.sea_units - voyage.row_units), row_km]
	var inland := "%d km by river" % Voyage.km(voyage.river_units)
	if voyage.portage_units > 0.0:
		inland += ", %d km hauled overland" % Voyage.km(voyage.portage_units)
	action_label.text = "%s → %s   ·   about %.1f days   ·   %s, %s   ·   %s" % [voyage.origin, voyage.dest, voyage.total_days, sea_txt, inland, _wind_text()]
	var hints: Array = []
	if not voyage.aground.is_empty():
		hints.append("The course runs over land. Drag it clear.")
	var dangers := _known_dangers()
	if not dangers.is_empty():
		hints.append("Your course passes " + ", ".join(dangers) + ". Hover the marks on the line.")
	var share := _fogged_share()
	if share > 0.6:
		hints.append("Most of this course crosses waters you haven't charted.")
	elif share > 0.15:
		hints.append("Part of this course crosses waters you haven't charted.")
	hints.append("Drag ⊕ to bend a leg · drag a waypoint to move it · right-click a waypoint to remove it")
	action_hint.text = "\n".join(hints)
	action_hint.visible = true
	action_hint.add_theme_color_override("font_color", Style.ACCENT if not voyage.aground.is_empty() else Style.INK_SOFT)
	_action_button("Set sail", _set_sail, voyage.ok(), true)
	_action_button("Straighten", func(): _plan_to(voyage.dest))
	_action_button("Cancel", _cancel_voyage)
	_update_strip()


func _refresh_sailing_label() -> void:
	var extra := "   ·   holding position" if _hold > 0.0 else ""
	action_label.text = "Under sail to %s   ·   day %.1f of about %.1f%s" % [voyage.dest, clock - _voyage_start, voyage.total_days, extra]
	action_hint.text = "Space to pause"
	action_hint.visible = true


## Rolls, once per voyage, which hazards strike and when. Unknown hazards strike just the same.
func _roll_incidents() -> void:
	_incidents.clear()
	var season := _season_now()
	var inside := {}   # id -> [first_t, last_t, days]
	var dt := 0.05
	var t := 0.0
	while t < voyage.total_days:
		var p: Vector2 = voyage.pose_at(t)[0]
		for z in hazards.zones:
			if hazards.active(z, season) and hazards.contains(z, p):
				var id: String = z["id"]
				if not inside.has(id):
					inside[id] = [t, t, 0.0]
				inside[id][1] = t
				inside[id][2] += dt
		t += dt
	for id in inside:
		var z: Dictionary = hazards.zone(id)
		var daily: float = Hazards.DAILY[z["kind"]] * z["sev"]
		var chance := 1.0 - pow(1.0 - minf(daily, 0.95), inside[id][2])
		if state.rng.randf() < chance:
			_incidents.append([state.rng.randf_range(inside[id][0], inside[id][1]), z])
	_incidents.sort_custom(func(a, b): return a[0] < b[0])


func _set_sail() -> void:
	if voyage == null or not voyage.ok():
		return
	state.port = ""
	state.mood.clear()
	state.swords_bought = 0
	ports_layer.here = ""
	port_panel.hide_panel()
	ports_layer.selected = ""
	sail_t = 0.0
	_dist = 0.0
	_hold = 0.0
	_voyage_start = clock
	speed = maxf(speed, 1.0)
	_follow = true
	route_layer.sailed_days = 0.0
	route_layer.clear_wake()
	_roll_incidents()
	_set_mode(Mode.SAILING)


func _arrive() -> void:
	var dest: String = voyage.dest
	var days := int(ceil(clock - _voyage_start - 0.001))
	clock = _voyage_start + days
	state.port = dest
	state.day = int(clock) + 1
	var cost: int = state.upkeep(days, {})
	var short: String = state.charge(cost)
	var msgs: Array = ["Arrived at %s" % dest, "%d days at sea" % days, "upkeep %ds" % cost]
	if short != "":
		msgs.append(short)
	if state.new_season_due():
		var added: int = state.begin_season()
		msgs.append("%s begins (interest +%ds)" % [D.SEASONS[state.season]["name"], added])
	state.roll_mood(state.roll(), state.roll())
	fog.reveal_path(index.approach(dest), REVEAL_RADIUS * 0.8)
	fog.flush(true)
	_learn()
	voyage = null
	route_layer.voyage = null
	route_layer.marks = []
	route_layer.sailed_days = -1.0
	route_layer.clear_wake()
	route_layer.ship_pos = index.position(dest)
	ports_layer.here = dest
	ports_layer.destination = ""
	ports_layer.queue_redraw()
	route_layer.queue_redraw()
	_set_mode(Mode.DOCKED)
	_select_port(dest)
	_toast("   ·   ".join(msgs))


## Charts reveal hazards: rumours at the edges, names at the centre.
func _learn() -> void:
	var newly: Array = hazards.learn_from_fog(fog)
	if newly.is_empty():
		return
	hazard_layer.queue_redraw()
	for id in newly:
		var z: Dictionary = hazards.zone(id)
		if hazards.knowledge[id] >= 2:
			_toast("Charted: %s" % z["name"], 4.0)
		else:
			_toast("A rumour: " + z["rumor"], 5.0)


# --- Incidents -------------------------------------------------------------

func _resolve_incident(z: Dictionary) -> void:
	_in_incident = true
	_update_strip()
	var known: bool = hazards.knowledge[z["id"]] >= 2
	var title: String = z["name"] if known else {"shoals": "Shoal water", "storm": "A storm", "fog": "Fog", "raiders": "Sails on the horizon", "cursed": "The crew goes quiet"}[z["kind"]]
	var sev: int = z["sev"]
	var body := ""
	var options: Array = []
	var roll := state.roll()
	var result := ""
	match z["kind"]:
		"shoals":
			body = "The lead line comes up short. Sand, close under the keel."
			options = ["Sound your way through (lose a day)", "Press on"]
		"storm":
			body = "The sky goes the colour of a bruise and the sea starts to heave."
			options = ["Run before it (lose %d days)" % (1 + (1 if sev >= 3 else 0)), "Heave to and ride it out"]
		"fog":
			body = "Fog comes down so thick you can't see the prow from the steering oar."
			options = ["Wait for it to lift (lose a day)", "Feel your way on"]
		"raiders":
			body = "Too many oars for a trader, and they've turned toward you."
			options = ["Fight", "Negotiate", "Run for it"]
		"cursed":
			body = "Nobody will look over the side. An old hand mutters about what lives down there."
			options = ["Make an offering (%s)" % ("5 silver" if state.silver >= 5 else "a lot of cargo"), "Order them on"]
	incident.open(z["kind"], title, body, options)
	var pick: int = await incident.chosen
	var choice: String = options[pick]
	match z["kind"]:
		"shoals":
			if pick == 0:
				_hold += 1.0
				result = "You creep through on the lead line. A day lost."
			elif roll <= sev + 1:
				state.hull = maxi(0, state.hull - 2)
				_hold += 1.0
				result = "You run aground. Hull −2, and a day to float her off."
			else:
				result = "The keel kisses sand and slides free."
		"storm":
			if pick == 0:
				_hold += 1.0 + (1.0 if sev >= 3 else 0.0)
				result = "You run before the wind and lose ground."
			else:
				var dmg := sev if roll <= 3 else maxi(1, sev - 1)
				state.hull = maxi(0, state.hull - dmg)
				result = "She takes a beating. Hull −%d." % dmg
		"fog":
			if pick == 0:
				_hold += 1.0
				result = "The fog lifts after a day."
			elif roll <= 2:
				state.hull = maxi(0, state.hull - 1)
				result = "Something scrapes the hull in the murk. Hull −1."
			else:
				result = "You feel your way through."
		"raiders":
			match choice:
				"Fight":
					result = await _fight(sev, false, "")
				"Negotiate":
					result = await _negotiate(sev)
				_:
					if roll >= 2 + sev:
						_hold += 0.5
						result = "You pull away and lose them. Half a day off course."
					else:
						result = await _fight(sev, true, "They run you down.")
		"cursed":
			if pick == 0:
				if state.silver >= 5:
					state.silver -= 5
				elif not state.hold.is_empty():
					state.hold.pop_back()
				result = "The offering goes over the side. The crew breathes again."
			elif roll <= 3:
				state.crew = maxi(1, state.crew - 1)
				_hold += 1.0
				result = "In the night a man goes over the side and is not found. A day spent searching."
			else:
				result = "Nothing happens. The crew doesn't believe it."
	if _over:
		return
	hazards.knowledge[z["id"]] = 2
	hazard_layer.queue_redraw()
	_in_incident = false
	_update_strip()
	if state.hull <= 0:
		_game_over("The %s is lost" % SHIP["name"], "She breaks apart and the sea takes her, cargo and all.")
		return
	_toast(result, 5.0)


func _game_over(title: String, body: String) -> void:
	_over = true
	mode = Mode.DOCKED
	incident.open("the end", title, body, ["Start again"])
	await incident.chosen
	get_tree().reload_current_scene()


## Talking your way out. Returns what happened.
func _negotiate(sev: int) -> String:
	var their_crew: int = BattleRules.RAIDERS[sev]["crew"]
	var opts: Array = []
	if state.silver >= BattleRules.TRIBUTE:
		opts.append("Pay their price (%d silver)" % BattleRules.TRIBUTE)
	opts.append("Bargain")
	if not state.hold.is_empty():
		opts.append("Offer your best lot of cargo")
	opts.append("Refuse, and fight")
	incident.open("raiders", "They hail you", "A big man in the prow names his price: %d silver, or they take what they like." % BattleRules.TRIBUTE, opts)
	var i: int = await incident.chosen
	var c: String = opts[i]
	if c.begins_with("Pay"):
		state.silver -= BattleRules.TRIBUTE
		return "You pay. They row off, jeering."
	if c == "Bargain":
		var r := state.roll() + (1 if state.crew >= their_crew else 0)
		if r >= 5:
			var paid := mini(7, state.silver)
			state.silver -= paid
			return "You haggle them down to %d silver." % paid
		if r >= 3:
			if state.silver >= BattleRules.TRIBUTE:
				state.silver -= BattleRules.TRIBUTE
				return "They won't budge. You pay the full %d." % BattleRules.TRIBUTE
			return await _fight(sev, true, "They won't budge, and you can't pay.")
		return await _fight(sev, true, "Your haggling insults them. They come alongside.")
	if c.begins_with("Offer"):
		var lots: Array = state.lots_by_value()
		if state.roll() >= 3:
			state.hold.erase(lots[0])
			return "They take the %s and let you go." % D.GOODS[lots[0]["good"]]
		for k in mini(2, lots.size()):
			state.hold.erase(lots[k])
		return "They laugh at the offer and take two lots instead."
	return await _fight(sev, false, "")


## The battle screen. Returns what happened (or ends the game).
func _fight(sev: int, start_close: bool, opener: String) -> String:
	var rules := BattleRules.new()
	rules.setup(state, sev, start_close)
	if opener != "":
		rules.lines.append(opener)
	battle_screen.start(rules, SHIP["name"])
	var outcome: String = await battle_screen.finished
	match outcome:
		"won":
			var loot := state.roll() * 3
			state.silver += loot
			return "The raiders flee. You take %d silver in loot." % loot
		"sank_them":
			return "Their ship goes down. No loot, but no raiders."
		"surrender":
			state.hold.clear()
			return "Your crew surrenders. The raiders strip your hold and let you go."
		"captured":
			_game_over("Taken", "Your last man falls. The raiders take you north, to be sold as a slave.")
		"sunk":
			_game_over("The %s is lost" % SHIP["name"], "Their ram opens her up and the sea takes her.")
		"fled":
			_hold += 1.0
			return "You row until your hands bleed and lose them. A day lost."
		"tribute":
			return "They take their price and row away laughing."
	return ""


# --- Time controls ---------------------------------------------------------

func _on_play() -> void:
	if mode == Mode.DOCKED:
		_set_mode(Mode.PICK_VESSEL)
	elif mode == Mode.EDIT:
		speed = 1.0
		_set_sail()
	elif mode == Mode.SAILING:
		speed = 1.0
	_update_strip()


func _on_pause() -> void:
	if mode == Mode.SAILING:
		speed = 0.0
	_update_strip()


func _on_fast() -> void:
	if mode == Mode.EDIT:
		speed = FAST
		_set_sail()
	elif mode == Mode.SAILING:
		speed = 1.0 if speed == FAST else FAST
	_update_strip()


func _process(delta: float) -> void:
	_camera_step(delta)
	fog.flush()
	_feed_world(delta)
	if mode == Mode.EDIT:
		route_layer.queue_redraw()   # marching dashes on the planned course
	if mode != Mode.SAILING or voyage == null or speed == 0.0 or _in_incident:
		return
	var dd := delta * speed / SECONDS_PER_DAY
	clock += dd
	weather.step(dd, _season_now())
	if _hold > 0.0:
		_hold = maxf(0.0, _hold - dd)
		_ship_speed = 0.0
	else:
		var here: Array = voyage.at_distance(_dist)
		var sp: Array = voyage.speed_at(here[0], here[1], here[3], weather.wind_at(here[0], _season_now()))
		_ship_speed = sp[0]
		_rowing = sp[1]
		_dist = minf(_dist + sp[0] * dd, voyage.total_len)
	sail_t = voyage.days_at_distance(_dist)
	var pose: Array = voyage.at_distance(_dist)
	route_layer.ship_rowing = _rowing
	route_layer.ship_speed = _ship_speed
	route_layer.ship_pos = pose[0]
	route_layer.ship_dir = route_layer.ship_dir.lerp(pose[1], minf(1.0, delta * 6.0)).normalized()
	route_layer.sailed_days = sail_t
	if pose[0].distance_to(_trail_last) > 2.0:
		_trail_last = pose[0]
		route_layer.add_wake(pose[0])
	route_layer.queue_redraw()
	if pose[0].distance_to(_last_reveal) > 5.0:
		_last_reveal = pose[0]
		fog.reveal(pose[0], REVEAL_RADIUS)
		fog.flush()
		ports_layer.queue_redraw()
		_learn()
	if _follow:
		camera.position = camera.position.lerp(pose[0], minf(1.0, delta * 1.2))
	_refresh_sailing_label()
	_update_strip()
	if not _incidents.is_empty() and sail_t >= _incidents[0][0]:
		var next: Array = _incidents.pop_front()
		_resolve_incident(next[1])
		return
	if _dist >= voyage.total_len and _hold <= 0.0:
		_arrive()


## Weather, time of day and the ship's state go to the sea, clouds, ambient life and the ship.
func _feed_world(delta: float) -> void:
	var season := _season_now()
	var focus := camera.get_screen_center_position()
	var wind := weather.wind_at(focus, season)
	var storms := weather.shader_storms()
	var base: Dictionary = Hazards.WINDS[season]
	var base_dir: Vector2 = base["dir"].normalized()
	_sea_offset += base_dir * (5.0 + 20.0 * base["strength"]) * delta
	_cloud_offset += base_dir * 14.0 * delta
	if sea_layer:
		sea_layer.set_weather(base_dir, base["strength"], storms, zoom, _sea_offset)
	if cloud_layer and cloud_layer.material:
		var packed: Array = []
		for i in 4:
			packed.append(storms[i] if i < storms.size() else Vector4.ZERO)
		cloud_layer.material.set_shader_parameter("storms", packed)
		cloud_layer.material.set_shader_parameter("offset", _cloud_offset)
	# One visual day-and-night per 4 game days (a game day is only 1.6 s), softened at fast forward.
	var f := fposmod(clock / 4.0, 1.0)
	var night := smoothstep(0.55, 0.7, f) * (1.0 - smoothstep(0.9, 0.99, f))
	if mode == Mode.SAILING and speed > 1.0:
		night *= 0.35
	if night_rect:
		night_rect.color = Color(0.04, 0.06, 0.14, 0.3 * night)
	if ambient:
		ambient.night = night
		ambient.zoom = zoom
	route_layer.ship_wind = weather.wind_at(route_layer.ship_pos, season)
	route_layer.docked = mode != Mode.SAILING
	if mode != Mode.SAILING and mode != Mode.EDIT:
		route_layer.queue_redraw()   # idle rocking at anchor


# --- Trading ---------------------------------------------------------------

func _on_buy(g: int) -> void:
	if state.why_not_buy(g) == "":
		state.buy(g)
	port_panel.refresh()
	_update_strip()


func _on_sell(g: int) -> void:
	if state.why_not_sell(g) == "":
		state.sell(g)
	port_panel.refresh()
	_update_strip()


func _select_port(p: String) -> void:
	ports_layer.selected = p
	ports_layer.queue_redraw()
	port_panel.extra = _estimate_text(p)
	port_panel.show_port(p, true)


func _estimate_text(p: String) -> String:
	if state.port == "" or p == state.port:
		return ""
	var key := "%s>%s@%d" % [state.port, p, _season_now()]
	if _estimates.has(key):
		return _estimates[key]
	var v := Voyage.new()
	v.setup(state.port, p, map.find_route(index.anchorage(state.port), index.anchorage(p)), index.approach(state.port), index.approach(p))
	_evaluate(v)
	_estimates[key] = "About %.0f days from %s on the direct course." % [v.total_days, state.port]
	return _estimates[key]


# --- Input -----------------------------------------------------------------

func _unhandled_input(event: InputEvent) -> void:
	if _in_incident:
		return
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_SPACE and mode == Mode.SAILING:
			speed = 1.0 if speed == 0.0 else 0.0
			_update_strip()
		elif event.keycode == KEY_ESCAPE and mode in [Mode.PICK_VESSEL, Mode.PICK_DEST, Mode.EDIT]:
			_cancel_voyage()
		return
	if event is InputEventMouseButton:
		_mouse_button(event)
	elif event is InputEventMouseMotion:
		_mouse_motion(event)


func _mouse_button(e: InputEventMouseButton) -> void:
	var world := _screen_to_world(e.position)
	if e.pressed and e.button_index == MOUSE_BUTTON_WHEEL_UP:
		_zoom_at(1.12, e.position)
	elif e.pressed and e.button_index == MOUSE_BUTTON_WHEEL_DOWN:
		_zoom_at(1.0 / 1.12, e.position)
	elif e.button_index == MOUSE_BUTTON_RIGHT and e.pressed:
		var h: int = route_layer.handle_at(world)
		if h >= 0:
			var s: PackedVector2Array = voyage.sea
			s.remove_at(h)
			voyage.sea = s
			_after_edit()
	elif e.button_index == MOUSE_BUTTON_LEFT:
		if e.pressed:
			_pressed = true
			_press_screen = e.position
			_panning = false
			_drag_handle = route_layer.handle_at(world)
			if _drag_handle < 0:
				var m: int = route_layer.mid_at(world)
				if m >= 0:
					var s: PackedVector2Array = voyage.sea
					s.insert(m + 1, (s[m] + s[m + 1]) / 2.0)
					voyage.sea = s
					_drag_handle = m + 1
					_after_edit()
		else:
			if _pressed and not _panning and _drag_handle < 0:
				_click(world)
			if Time.get_ticks_msec() - _last_motion_ms > 80:
				_pan_vel = Vector2.ZERO
			_pressed = false
			_panning = false
			_drag_handle = -1


func _mouse_motion(e: InputEventMouseMotion) -> void:
	_last_motion_ms = Time.get_ticks_msec()
	var world := _screen_to_world(e.position)
	if _pressed:
		if _drag_handle >= 0:
			var s: PackedVector2Array = voyage.sea
			s[_drag_handle] = world.clamp(Vector2.ZERO, map.map_size())
			voyage.sea = s
			_after_edit()
			return
		if not _panning and e.position.distance_to(_press_screen) > 4.0:
			_pan_vel = Vector2.ZERO
			_panning = true
			_follow = false
		if _panning:
			_stop_opening()
			camera.position -= e.relative / zoom
			_pan_vel = e.velocity
			_clamp_camera()
			return
	# Hover
	var hp: String = ports_layer.port_at(world)
	if hp != ports_layer.hover:
		ports_layer.hover = hp
		ports_layer.queue_redraw()
		if not port_panel.pinned:
			if hp != "":
				port_panel.extra = _estimate_text(hp)
				port_panel.show_port(hp, false)
			else:
				port_panel.hide_panel()
	if mode == Mode.EDIT:
		var h: int = route_layer.handle_at(world)
		var m: int = -1 if h >= 0 else route_layer.mid_at(world)
		var mk: int = -1 if h >= 0 or m >= 0 else route_layer.mark_at(world)
		if h != route_layer.hover_handle or m != route_layer.hover_mid or mk != route_layer.hover_mark:
			route_layer.hover_handle = h
			route_layer.hover_mid = m
			route_layer.hover_mark = mk
			route_layer.queue_redraw()
		if mk >= 0:
			var z: Dictionary = hazards.zone(route_layer.marks[mk][4])
			tip_label.text = hazards.hint(z, _season_now())
			tip.visible = true
			tip.reset_size()
			tip.position = e.position + Vector2(16, 14)
		else:
			tip.visible = false


func _click(world: Vector2) -> void:
	var p: String = ports_layer.port_at(world)
	match mode:
		Mode.PICK_DEST, Mode.EDIT:
			if p != "" and p != state.port:
				_plan_to(p)
		_:
			if p != "":
				_select_port(p)
			elif port_panel.pinned:
				port_panel.hide_panel()
				ports_layer.selected = ""
				ports_layer.queue_redraw()


func _after_edit() -> void:
	_evaluate(voyage)
	_update_marks()
	route_layer.queue_redraw()
	_refresh_readout()
