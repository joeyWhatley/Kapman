@tool
extends Node3D
## The 3D world, vertical slice: the Frisian coast around Dorestad at full detail, your cog on a
## live sea, and one continuous zoom from the waterline up to the 2D chart.
##
## Everything is a node in sea_view.tscn and can be edited there: the environment and sun, the
## Terrain and Sea (their look lives in terrain_material.tres / sea_material.tres), each plant
## layer under Plants (its own mesh, spacing, tints), the Ship (Hull and Sail), Camera, HUD and
## Marker. This script only wires them together and feeds them the map data. The editor viewport
## shows the world around the start position.
##
## Scale: 1 unit = 1 metre. Everything is placed relative to a floating origin (whole map units,
## re-centred as you travel) so positions stay precise; absolute positions are 64-bit metres.
##
## Controls: wheel zoom · right-drag orbit · left-drag pan · F follow ship · A/D steer ·
## W/S raise/lower sail · 1–4 wind (calm, breeze, fresh, gale) · Q/E turn the wind

const Geo := preload("res://route_map/geo.gd")
const MapData := preload("res://route_map/map_data.gd")
const PortIndex := preload("res://route_map/port_index.gd")
const Style := preload("res://route_map/style.gd")
const Waves := preload("res://route_map/world3d/waves.gd")
const TerrainStream := preload("res://route_map/world3d/terrain_stream.gd")

const M := 1112.0                   # metres per map unit
const REBASE := 6.0                 # map units the focus may drift before re-centring
const WINDS := [3.0, 8.0, 13.0, 20.0]
const DETAIL_LAMBDA := 160.0

@export_group("Start")
## Where the ship starts (latitude, longitude). Default: off the old Rhine mouth at Katwijk.
@export var start_lat := 52.225:
	set(v):
		start_lat = v
		_editor_refresh()
@export var start_lon := 4.355:
	set(v):
		start_lon = v
		_editor_refresh()
## Compass heading in degrees (0 = north).
@export var start_heading := 20.0
## Camera distance from the ship at start (metres).
@export var start_view := 140.0

@export_group("Wind and sea")
## Where the wind blows FROM, compass degrees (the default westerly: 297 = WNW).
@export var wind_from := 297.0
## Metres per second (8 = force 4, 13 = force 6, 20 = gale).
@export_range(0.5, 30.0) var wind_speed := 8.0
## 0 = rounded swells, 1 = sharp crests.
@export_range(0.0, 1.0) var choppiness := 0.75

@export_group("Weather")
## How much of the sky is cloud (0 clear, 1 overcast); also the cloud shadows on the ground.
@export_range(0.0, 1.0) var cloud_cover := 0.55
## How dark the clouds' shadows fall on land and sea.
@export_range(0.0, 1.0) var cloud_shadow := 0.45
## Sea mist: multiplies the distance haze.
@export_range(0.0, 4.0) var mist := 1.0

@export_group("Zoom")
## Camera distance (m) where the 2D chart starts to show...
@export var chart_fade_start := 38000.0
## ...and where it has fully taken over.
@export var chart_fade_end := 80000.0
## Plants are drawn below this camera distance (m).
@export var plants_below := 7000.0
## The woods' canopy shell takes over from the drawn trees between these distances (m).
@export var canopy_from := 380.0
@export var canopy_to := 470.0
## Render scale for sharper edges (1 = native; 1.33 is supersampled).
@export_range(0.5, 2.0) var render_scale := 1.33

@export_group("Terrain data")
## Where the baked tiles are (G:/kapman_data/baker/Baker.exe writes them).
@export_global_dir var tiles_dir := "G:/kapman_data/tiles"

@export_group("Editor")
## Draw the plant layers in the editor viewport too.
@export var plants_in_editor := true:
	set(v):
		plants_in_editor = v
		_editor_refresh()

var waves := Waves.new()
var wind_dir := Vector2(0.7, 0.35).normalized()   # the way the wind blows toward (x east, y south)

var origin := Vector2.ZERO          # map units (whole numbers)
var origin_x := 0.0                 # 64-bit metres
var origin_z := 0.0
var focus_x := 0.0                  # absolute metres
var focus_z := 0.0
var following := true
var dist := 140.0
var yaw := 0.6
var pitch_user := 0.0
var t := 0.0

var cam: Camera3D
var sun: DirectionalLight3D
var env: Environment
var terrain
var ocean
var terrain_mat: ShaderMaterial
var ocean_mat: ShaderMaterial
var ship
var hud: Label
var marker
var plants: Array = []

var stream: TerrainStream
var _coarse_img: Image
var _coarse_size := Vector2.ZERO
var _ready_done := false

var _chart_vp: SubViewport
var _chart_cam: Camera2D
var _chart_rect: TextureRect
var _fade := 0.0
var _drag := 0
var _noise_offs := PackedVector2Array()
var _cloud_tex: Texture2D
var _cloud_move := Vector2.ZERO     # metres the cloud pattern has drifted
var _sky_mat: ShaderMaterial


func _ready() -> void:
	sun = $Sun
	env = $WorldEnvironment.environment
	terrain = $Terrain
	ocean = $Sea
	ship = $Ship
	cam = $Camera
	for n in $Plants.get_children():
		if n.has_method("setup"):
			plants.append(n)
	_load_data()
	_setup_materials()
	_ready_done = true
	var start := Geo.project(start_lat, start_lon) * M
	ship.abs_x = start.x
	ship.abs_z = start.y
	ship.heading = deg_to_rad(start_heading)
	focus_x = start.x
	focus_z = start.y
	dist = start_view
	var a := deg_to_rad(wind_from + 180.0)
	wind_dir = Vector2(sin(a), -cos(a))
	waves.choppiness = choppiness
	waves.configure(wind_dir, wind_speed)
	_rebase(true)
	if Engine.is_editor_hint():
		_editor_preview()
		return
	get_viewport().scaling_3d_mode = Viewport.SCALING_3D_MODE_BILINEAR
	get_viewport().scaling_3d_scale = render_scale
	cam.make_current()
	hud = get_node_or_null("HUD/Status")
	marker = get_node_or_null("Marker/ShipMarker")
	if marker:
		marker.cam = cam
		marker.ship = ship
		marker.ship_name = ship.ship_name
	_build_chart()


## Editor: a still picture of the world around the start (plants, ship at rest, waves drawn once).
func _editor_preview() -> void:
	var local := Vector3(ship.abs_x - origin_x, 0.0, ship.abs_z - origin_z)
	terrain.follow(local)
	ocean.follow(local)
	ship.position = local + Vector3(0, -ship.DRAFT, 0)
	ship.rotation = Vector3(0, -ship.heading, 0)
	waves.update(0.0, origin_x, origin_z)
	waves.push(ocean_mat)
	for layer in plants:
		layer.visible = plants_in_editor
		if plants_in_editor:
			layer.follow(local.x + origin_x, local.z + origin_z, origin_x, origin_z, origin, _noise_offs)
	terrain_mat.set_shader_parameter("shell_start", canopy_from)
	terrain_mat.set_shader_parameter("shell_end", canopy_to)


func _editor_refresh() -> void:
	if Engine.is_editor_hint() and _ready_done and is_inside_tree():
		var start := Geo.project(start_lat, start_lon) * M
		ship.abs_x = start.x
		ship.abs_z = start.y
		focus_x = start.x
		focus_z = start.y
		stream.prime(start.x, start.y)
		_rebase(true)
		_editor_preview()


# --- Data ----------------------------------------------------------------------

func _load_data() -> void:
	_coarse_img = Image.load_from_file(ProjectSettings.globalize_path("res://route_map/data/terrain_height.exr"))
	_coarse_size = Vector2(_coarse_img.get_size()) * 2.0
	stream = TerrainStream.new(tiles_dir)
	if not stream.available():
		push_warning("No baked terrain in %s: run G:/kapman_data/baker/Baker.exe bake" % tiles_dir)
	var start := Geo.project(start_lat, start_lon) * M
	stream.prime(start.x, start.y)


## Water surface (m) at absolute metres: 0 at sea, a lake's or river's own level.
func water_at(ax: float, az: float) -> float:
	return stream.sample(ax, az).y


## Depth of water (m) at absolute metres; negative on dry land.
func depth_at(ax: float, az: float) -> float:
	var s := stream.sample(ax, az)
	return s.y - s.x


## Terrain height (m) at absolute metres, for grounding and the camera (not the rendered detail).
func ground_at(ax: float, az: float) -> float:
	var s := stream.sample(ax, az)
	if s.z >= 2549.0 and s.x == 0.0 and s.y == 0.0:
		var c := Vector2i(Vector2(ax / M, az / M) / _coarse_size * Vector2(_coarse_img.get_size()))
		return _coarse_img.get_pixelv(c.clamp(Vector2i.ZERO, _coarse_img.get_size() - Vector2i.ONE)).r
	return s.x


## Pass the streamed windows to every material that reads the terrain.
func _push_stream() -> void:
	var rects := stream.rects(origin_x, origin_z)
	for m in [terrain_mat, ocean_mat]:
		m.set_shader_parameter("lvl_rect", rects)
	for layer in plants:
		if layer.place_mat:
			layer.place_mat.set_shader_parameter("lvl_rect", rects)

# --- Materials -------------------------------------------------------------------

## Copies of the saved materials, with the map data added (the saved files stay small and
## editable; the data is never written into them).
func _setup_materials() -> void:
	var coarse_tex := ImageTexture.create_from_image(_coarse_img)
	terrain_mat = (terrain.material as ShaderMaterial).duplicate()
	ocean_mat = (ocean.material as ShaderMaterial).duplicate()
	for m in [terrain_mat, ocean_mat]:
		m.set_shader_parameter("height_tiles", stream.tex)
		m.set_shader_parameter("coarse_tex", coarse_tex)
		m.set_shader_parameter("coarse_size", _coarse_size)
		m.set_shader_parameter("m_per_unit", M)
	ocean_mat.set_shader_parameter("toon_spec", 0.45)      # small stepped sun-glints on the waves
	ocean_mat.set_shader_parameter("toon_spec_size", 0.994)
	terrain.use_material(terrain_mat)
	ocean.use_material(ocean_mat)
	# clouds: the sky's own noise, so the shadows match the look of the clouds
	if env and env.sky and env.sky.sky_material is ShaderMaterial:
		_sky_mat = env.sky.sky_material
		_cloud_tex = _sky_mat.get_shader_parameter("cloud_noise")
	if _cloud_tex == null:
		var nt := NoiseTexture2D.new()
		nt.seamless = true
		nt.generate_mipmaps = true
		var n := FastNoiseLite.new()
		n.frequency = 0.008
		n.fractal_octaves = 4
		nt.noise = n
		_cloud_tex = nt
	for m in [terrain_mat, ocean_mat]:
		m.set_shader_parameter("cloud_tex", _cloud_tex)
	_update_clouds()
	for layer in plants:
		layer.setup(terrain_mat)


## Re-centre the world on the focus (whole map units, so the shader's origin is exact).
func _rebase(force: bool) -> void:
	var fm := Vector2(focus_x / M, focus_z / M)
	if not force and fm.distance_to(origin) < REBASE:
		return
	origin = fm.round()
	origin_x = origin.x * M
	origin_z = origin.y * M
	var offs := PackedVector2Array()
	var lams: Array = []
	for o in 6:
		lams.append(DETAIL_LAMBDA / pow(2.0, o))
	lams.append_array([900.0, 140.0, 23.0])
	for i in lams.size():
		var lam: float = lams[i]
		var period := 1280.0 if i >= 6 else 256.0     # 6-8 feed the turned layers (landcover fbm3)
		offs.append(Vector2(fposmod(origin_x / lam, period), fposmod(origin_z / lam, period)))
	for m in [terrain_mat, ocean_mat]:
		m.set_shader_parameter("origin_map", origin)
		m.set_shader_parameter("noise_off", offs)
	_noise_offs = offs
	terrain_mat.set_shader_parameter("micro_off", PackedVector2Array([
		Vector2(fposmod(origin_x / 4.0, 256.0), fposmod(origin_z / 4.0, 256.0)),
		Vector2(fposmod(origin_x / 1.1, 256.0), fposmod(origin_z / 1.1, 256.0))]))
	for layer in plants:
		layer.invalidate()
	_push_stream()


# --- The 2D chart, for the zoomed-out end (built at run time from route_map.tscn) ---------

func _build_chart() -> void:
	var layer: CanvasLayer = get_node_or_null("Chart")
	if layer == null:
		layer = CanvasLayer.new()
		layer.name = "Chart"
		layer.layer = 1
		add_child(layer)
	_chart_vp = SubViewport.new()
	_chart_vp.disable_3d = true
	_chart_vp.size = Vector2i(get_viewport().get_visible_rect().size)
	_chart_vp.render_target_update_mode = SubViewport.UPDATE_DISABLED
	add_child(_chart_vp)
	var scene: Node = load("res://route_map/route_map.tscn").instantiate()
	var map_node: Node2D = scene.get_node("Map")
	scene.remove_child(map_node)
	scene.free()
	_chart_vp.add_child(map_node)
	var md := MapData.new()
	md.load_data()
	var patches: Array = []
	for n in map_node.get_node("Corrections").get_children():
		if n is KapmanLandPatch:
			patches.append(n)
			n.paper = Style.PAPER
			n.ink = Style.INK
	md.apply_patches(patches)
	map_node.get_node("Paper").rect = Rect2(Vector2.ZERO, md.map_size())
	for hidden in ["FogWash", "CoastRough", "HazardMarks", "Route", "Ambient", "CloudShadows"]:
		var n := map_node.get_node_or_null(hidden)
		if n:
			n.visible = false
			n.process_mode = Node.PROCESS_MODE_DISABLED
	var cd = map_node.get_node("CoastDetail")
	cd.runs = md.detail_runs
	cd.medium = md.medium_runs
	cd.far = md.far_runs
	cd.water = md.water_runs
	map_node.get_node("Lakes").runs = md.lake_runs
	var index := PortIndex.new()
	index.build(map_node.get_node("Ports"))
	var rr: Array = []
	for rv in md.river_runs:
		rr.append(rv["pts"])
	for p in index.names():
		rr.append(index.approach(p).slice(1))
	map_node.get_node("Rivers").runs = rr
	var glyphs = map_node.get_node_or_null("PortGlyphs")
	if glyphs:
		glyphs.index = index
	var sea = map_node.get_node_or_null("Sea")
	if sea:
		var clear := Image.create(1, 1, false, Image.FORMAT_L8)
		clear.fill(Color.WHITE)
		sea.set_fog(ImageTexture.create_from_image(clear), md.map_size())
	_chart_cam = Camera2D.new()
	_chart_vp.add_child(_chart_cam)
	_chart_cam.make_current()
	_chart_rect = TextureRect.new()
	_chart_rect.texture = _chart_vp.get_texture()
	_chart_rect.set_anchors_preset(Control.PRESET_FULL_RECT)
	_chart_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	_chart_rect.visible = false
	layer.add_child(_chart_rect)


# --- Frame -------------------------------------------------------------------------------

func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	t += delta
	var steer := 0.0
	if Input.is_physical_key_pressed(KEY_A):
		steer -= 1.0
	if Input.is_physical_key_pressed(KEY_D):
		steer += 1.0
	ship.rudder = steer
	if Input.is_physical_key_pressed(KEY_W):
		ship.sail_set = minf(ship.sail_set + delta * 0.4, 1.0)
	if Input.is_physical_key_pressed(KEY_S):
		ship.sail_set = maxf(ship.sail_set - delta * 0.4, 0.0)
	if Input.is_physical_key_pressed(KEY_Q):
		_set_wind(wind_dir.rotated(-delta * 0.6), wind_speed)
	if Input.is_physical_key_pressed(KEY_E):
		_set_wind(wind_dir.rotated(delta * 0.6), wind_speed)

	waves.update(t, origin_x, origin_z)
	ship.step(delta, waves, wind_dir, wind_speed, origin_x, origin_z, depth_at, water_at, t)
	if following:
		focus_x = lerpf(focus_x, ship.abs_x, 1.0 - exp(-delta * 4.0))
		focus_z = lerpf(focus_z, ship.abs_z, 1.0 - exp(-delta * 4.0))
	_rebase(false)
	stream.update(focus_x, focus_z)
	_push_stream()
	_place_camera()
	_update_shaders()
	_update_chart()
	_update_hud()


func _set_wind(dir: Vector2, speed: float) -> void:
	wind_dir = dir.normalized()
	wind_speed = speed
	waves.configure(wind_dir, wind_speed)


## Camera: close in, it looks along the sea at the ship; climbing, it levels to straight down,
## north up, lining up exactly with the chart it fades into.
func _place_camera() -> void:
	var up_t := smoothstep(log(250.0), log(30000.0), log(dist))
	var pitch := clampf(lerpf(deg_to_rad(14.0), PI / 2.0, up_t) + pitch_user * (1.0 - up_t), deg_to_rad(4.0), PI / 2.0)
	var y := yaw * (1.0 - up_t)
	var fl := Vector3(focus_x - origin_x, 0.0, focus_z - origin_z)
	fl.y = maxf(ground_at(focus_x, focus_z), 0.0) + (2.5 if following and dist < 400.0 else 0.0)
	var back := Vector3(sin(y) * cos(pitch), sin(pitch), cos(y) * cos(pitch))
	var pos := fl + back * dist
	var floor_y := maxf(ground_at(pos.x + origin_x, pos.z + origin_z), 0.0) + 3.0
	if dist < 3000.0:
		floor_y = maxf(floor_y, waves.height_at(pos.x, pos.z) + 1.5)
	pos.y = maxf(pos.y, floor_y)
	var right := Vector3(cos(y), 0.0, -sin(y))
	var z := (pos - fl).normalized()
	var upv := z.cross(right).normalized()
	cam.global_transform = Transform3D(Basis(right, upv, z).orthonormalized(), pos)
	cam.near = clampf(dist * 0.002, 0.05, 40.0)
	cam.far = maxf(30000.0, dist * 40.0)
	var ground := Vector3(pos.x, 0.0, pos.z)
	terrain.follow(ground)
	ocean.follow(ground)
	var plants_on := dist < plants_below
	# centre the plants between the camera and what it looks at, so both are covered
	var pc := Vector2(pos.x, pos.z).lerp(Vector2(fl.x, fl.z), 0.65)
	# the canopy shell takes over from the drawn trees at the edge of their layer (terrain.gdshader)
	terrain_mat.set_shader_parameter("shell_start", canopy_from if plants_on else 0.0)
	terrain_mat.set_shader_parameter("shell_end", canopy_to if plants_on else 1.0)
	for layer in plants:
		layer.visible = plants_on
		if plants_on:
			layer.follow(pc.x + origin_x, pc.y + origin_z, origin_x, origin_z, origin, _noise_offs)
			layer.set_wind(wind_dir, clampf(wind_speed / 15.0, 0.1, 1.2))
	sun.directional_shadow_max_distance = clampf(dist * 3.0, 120.0, 2500.0)
	env.fog_density = mist * 0.8 / maxf(dist * 10.0, 16000.0)


## Cloud shadows (origin-safe offsets) and the sky's cloud drift, moving with the wind.
func _update_clouds() -> void:
	var o1 := Vector2(fposmod((origin_x - _cloud_move.x) / 4200.0, 1.0), fposmod((origin_z - _cloud_move.y) / 4200.0, 1.0))
	var o2 := Vector2(fposmod((origin_x - _cloud_move.x) / 1300.0, 1.0), fposmod((origin_z - _cloud_move.y) / 1300.0, 1.0))
	for m in [terrain_mat, ocean_mat]:
		m.set_shader_parameter("cloud_off", o1)
		m.set_shader_parameter("cloud_off2", o2)
		m.set_shader_parameter("cloud_cover", cloud_cover)
		m.set_shader_parameter("cloud_shadow", cloud_shadow)
	for layer in plants:
		if layer.draw_mat:
			layer.set_clouds(_cloud_tex, o1, o2, cloud_cover, cloud_shadow)
	if _sky_mat:
		_sky_mat.set_shader_parameter("drift", -_cloud_move / 60000.0)
		_sky_mat.set_shader_parameter("coverage", clampf(cloud_cover + 0.07, 0.0, 1.0))


func _update_shaders() -> void:
	_cloud_move += wind_dir * wind_speed * 1.6 * get_process_delta_time()
	_update_clouds()
	waves.push(ocean_mat)
	var drift := Vector2(wind_dir.x, wind_dir.y) * t
	ocean_mat.set_shader_parameter("ripple_off_a", Vector2(fposmod(origin_x / 38.0 + drift.x * 0.035, 1.0), fposmod(origin_z / 38.0 + drift.y * 0.035, 1.0)))
	ocean_mat.set_shader_parameter("ripple_off_b", Vector2(fposmod(origin_x / 11.0 - drift.y * 0.06, 1.0), fposmod(origin_z / 11.0 + drift.x * 0.06, 1.0)))
	ocean_mat.set_shader_parameter("foam_off", Vector2(fposmod(origin_x / 9.0 + drift.x * 0.05, 1.0), fposmod(origin_z / 9.0 + drift.y * 0.05, 1.0)))
	ship.push_to(ocean_mat, origin_x, origin_z, t)


func _update_chart() -> void:
	_fade = smoothstep(chart_fade_start, chart_fade_end, dist)
	var show_chart := _fade > 0.0
	_chart_rect.visible = show_chart
	_chart_vp.render_target_update_mode = SubViewport.UPDATE_ALWAYS if show_chart else SubViewport.UPDATE_DISABLED
	var world_on := _fade < 1.0
	terrain.visible = world_on
	ocean.visible = world_on
	if not world_on:
		for layer in plants:
			layer.visible = false
	ship.visible = world_on
	if not show_chart:
		return
	var vp := get_viewport().get_visible_rect().size
	if Vector2i(vp) != _chart_vp.size:
		_chart_vp.size = Vector2i(vp)
	var visible_units := 2.0 * dist * tan(deg_to_rad(cam.fov) * 0.5) / M
	var z := vp.y / visible_units
	_chart_cam.position = Vector2(focus_x / M, focus_z / M)
	_chart_cam.zoom = Vector2(z, z)
	get_tree().set_group("zoom_aware", "zoom", z)
	_chart_rect.modulate.a = _fade


func _update_hud() -> void:
	if hud == null:
		return
	var knots: float = ship.speed * 1.944
	var bft := [0.3, 1.6, 3.4, 5.5, 8.0, 10.8, 13.9, 17.2, 20.8, 24.5, 28.5, 32.7]
	var b := 0
	while b < bft.size() and wind_speed > bft[b]:
		b += 1
	var dirs := ["N", "NE", "E", "SE", "S", "SW", "W", "NW"]
	var from := -wind_dir
	var ang := fposmod(rad_to_deg(atan2(from.x, -from.y)), 360.0)
	var alt := "%.0f m" % dist if dist < 2000.0 else "%.1f km" % (dist / 1000.0)
	hud.text = "Kāpman · the cog %s\nWind %s force %d (%.0f m/s) · waves %.1f m · %.1f knots%s · sail %d%%\nView %s\n\nWheel zoom · right-drag orbit · left-drag pan · F follow ship · A/D steer · W/S sail · 1–4 wind · Q/E turn wind" % [
		ship.ship_name, dirs[int(round(ang / 45.0)) % 8], b, wind_speed, waves.significant_height(), knots,
		" · AGROUND" if ship.aground else "", int(ship.sail_set * 100.0), alt]


func _unhandled_input(e: InputEvent) -> void:
	if Engine.is_editor_hint():
		return
	if e is InputEventMouseButton:
		if e.button_index == MOUSE_BUTTON_WHEEL_UP and e.pressed:
			dist = maxf(22.0, dist / 1.15)
		elif e.button_index == MOUSE_BUTTON_WHEEL_DOWN and e.pressed:
			dist = minf(5.0e6, dist * 1.15)
		elif e.button_index in [MOUSE_BUTTON_LEFT, MOUSE_BUTTON_RIGHT, MOUSE_BUTTON_MIDDLE]:
			_drag = e.button_index if e.pressed else 0
	elif e is InputEventMouseMotion and _drag != 0:
		if _drag == MOUSE_BUTTON_RIGHT:
			yaw -= e.relative.x * 0.006
			pitch_user = clampf(pitch_user + e.relative.y * 0.004, -0.2, 1.3)
		else:
			following = false
			var up_t := smoothstep(log(250.0), log(30000.0), log(dist))
			var y := yaw * (1.0 - up_t)
			var per_px := 2.0 * dist * tan(deg_to_rad(cam.fov) * 0.5) / get_viewport().get_visible_rect().size.y
			var r := Vector2(cos(y), -sin(y))
			var f := Vector2(-sin(y), -cos(y))
			var rel: Vector2 = e.relative
			var mv: Vector2 = (-r * rel.x + f * rel.y) * per_px
			focus_x += mv.x
			focus_z += mv.y
	elif e is InputEventKey and e.pressed and not e.echo:
		if e.physical_keycode == KEY_F:
			following = true
		elif e.physical_keycode >= KEY_1 and e.physical_keycode <= KEY_4:
			_set_wind(wind_dir, WINDS[e.physical_keycode - KEY_1])
