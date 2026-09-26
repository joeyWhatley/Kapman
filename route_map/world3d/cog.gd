@tool
extends Node3D
## The player's trading ship at true scale: a Frisian cog of about 800 AD, 18 m long, 5 m beam,
## flat-bottomed with raked posts, one mast and a square sail. Built from code (no model files).
## It floats on waves.gd: bow/stern/port/starboard samples drive heave, pitch and roll through
## springs, so it has weight; it sails by the same points-of-sail rule as the chart (voyage.gd).
## Forward is -Z. Absolute position is kept in 64-bit metres (abs_x, abs_z).

const Voyage := preload("res://route_map/voyage.gd")
const LENGTH := 18.0
const BEAM := 5.0
const DEPTH := 2.3          # keel to gunwale amidships
const DRAFT := 1.0
const MAST_H := 12.5

var abs_x := 0.0
var abs_z := 0.0
var heading := 0.0          # radians, 0 = north (-Z), clockwise seen from above
var speed := 0.0            # m/s
var sail_set := 1.0         # 0 = furled, 1 = full
var rudder := 0.0           # -1..1
@export var ship_name := "Aldgisl"
@export var crew := 6
var aground := false

var heave := 0.0
var heave_v := 0.0
var pitch := 0.0
var pitch_v := 0.0
var roll := 0.0
var roll_v := 0.0

var _level := 0.0
var wake: Array = []        # [abs_x, abs_z, time dropped, speed]
var _wake_t := 0.0
var sail: MeshInstance3D
var _sail_mat: ShaderMaterial


## Uses the Hull and Sail child nodes when the scene has them (edit or replace them freely);
## builds them from code otherwise.
func _ready() -> void:
	if get_node_or_null("Hull") == null:
		var hull := MeshInstance3D.new()
		hull.name = "Hull"
		hull.mesh = build_hull()
		hull.material_override = hull_material()
		add_child(hull)
	sail = get_node_or_null("Sail")
	if sail == null:
		sail = MeshInstance3D.new()
		sail.name = "Sail"
		sail.mesh = build_sail()
		sail.position = Vector3(0, MAST_H - 1.1, -1.2)
		add_child(sail)
	_sail_mat = sail.material_override as ShaderMaterial
	if _sail_mat == null:
		_sail_mat = ShaderMaterial.new()
		_sail_mat.shader = load("res://route_map/world3d/sail.gdshader")
	if not Engine.is_editor_hint():
		_sail_mat = _sail_mat.duplicate()     # animated per ship at run time
	sail.material_override = _sail_mat


static func hull_material() -> StandardMaterial3D:
	var m := StandardMaterial3D.new()
	m.vertex_color_use_as_albedo = true
	m.vertex_color_is_srgb = true
	m.roughness = 0.85
	m.cull_mode = BaseMaterial3D.CULL_DISABLED
	return m


## Wind: direction it blows toward (x east, y south) and speed in m/s.
func step(delta: float, waves, wind: Vector2, wind_speed: float, origin_x: float, origin_z: float, depth: Callable, water: Callable, now: float) -> void:
	var fwd := Vector2(sin(heading), -cos(heading))
	# Speed through the water from the wind, as on the chart.
	var sf: Array = Voyage.sail_factor(fwd, wind.normalized() * clampf(wind_speed / 12.0, 0.0, 1.5), crew)
	var target := 0.0
	if sf[1]:
		target = 1.3 * clampf(crew / 6.0, 0.4, 1.3)                  # rowing
	else:
		target = float(sf[0]) * clampf(wind_speed * 0.22, 0.0, 3.4) * sail_set     # a cog makes 4-7 knots
	speed = lerpf(speed, target, 1.0 - exp(-delta * 0.25))
	heading += rudder * delta * 0.05 * clampf(speed, 0.4, 4.0)
	var nx := abs_x + fwd.x * speed * delta
	var nz := abs_z + fwd.y * speed * delta
	var bow_depth: float = depth.call(nx + fwd.x * LENGTH * 0.5, nz + fwd.y * LENGTH * 0.5)
	aground = bow_depth < DRAFT * 0.8
	if aground:
		speed = 0.0
	else:
		abs_x = nx
		abs_z = nz
	# Buoyancy: where the water is at the ends and sides.
	var lx := abs_x - origin_x
	var lz := abs_z - origin_z
	var side := Vector2(-fwd.y, fwd.x)   # starboard
	var level: float = water.call(abs_x, abs_z)
	_level = lerpf(_level, level, 1.0 - exp(-delta * 2.0))
	var sea_f := 1.0 - smoothstep(0.04, 0.3, level)      # rivers: no swell
	var hb: float = waves.height_at(lx + fwd.x * LENGTH * 0.42, lz + fwd.y * LENGTH * 0.42)
	var hs: float = waves.height_at(lx - fwd.x * LENGTH * 0.42, lz - fwd.y * LENGTH * 0.42)
	var hp: float = waves.height_at(lx - side.x * BEAM * 0.45, lz - side.y * BEAM * 0.45)
	var hr: float = waves.height_at(lx + side.x * BEAM * 0.45, lz + side.y * BEAM * 0.45)
	var hc: float = waves.height_at(lx, lz)
	hb *= sea_f; hs *= sea_f; hp *= sea_f; hr *= sea_f; hc *= sea_f
	var t_heave := (hb + hs + hp + hr + hc * 2.0) / 6.0
	var t_pitch := atan2(hb - hs, LENGTH * 0.84)
	var heel := 0.0
	if not sf[1]:
		var cross := absf(fwd.cross(wind.normalized()))
		heel = cross * clampf(wind_speed / 20.0, 0.0, 1.0) * 0.12 * sail_set * signf(fwd.cross(wind))
	var t_roll := atan2(hr - hp, BEAM * 0.9) + heel
	heave_v += ((t_heave - heave) * 6.0 - heave_v * 2.4) * delta
	heave += heave_v * delta
	pitch_v += ((t_pitch - pitch) * 5.0 - pitch_v * 2.0) * delta
	pitch += pitch_v * delta
	roll_v += ((t_roll - roll) * 3.2 - roll_v * 1.1) * delta     # a round hull rolls slower and longer
	roll += roll_v * delta
	position = Vector3(lx, _level + heave - DRAFT, lz)
	basis = Basis.from_euler(Vector3(pitch, -heading, roll), EULER_ORDER_YXZ)
	# Wake: a trail of points that age and spread.
	_wake_t += delta
	if _wake_t > 0.7 and speed > 0.2:
		_wake_t = 0.0
		wake.push_front([abs_x - fwd.x * LENGTH * 0.45, abs_z - fwd.y * LENGTH * 0.45, now, speed])
		if wake.size() > 24:
			wake.pop_back()
	# The sail fills with the wind across it.
	var fill := 0.0 if sf[1] else clampf(wind_speed / 10.0, 0.25, 1.0) * sail_set
	_sail_mat.set_shader_parameter("fill", fill)
	_sail_mat.set_shader_parameter("set_amount", sail_set)
	sail.visible = sail_set > 0.02
	# Brace the yard toward the wind (square sails swing about the mast).
	var rel := wrapf(atan2(wind.x, -wind.y) - heading, -PI, PI)
	sail.rotation.y = -clampf(rel * 0.5, -0.9, 0.9)


func push_to(mat: ShaderMaterial, origin_x: float, origin_z: float, now: float) -> void:
	var fwd := Vector2(sin(heading), -cos(heading))
	mat.set_shader_parameter("ship_frame", Vector4(abs_x - origin_x, abs_z - origin_z, fwd.x, fwd.y))
	mat.set_shader_parameter("ship_half", Vector2(LENGTH * 0.5, BEAM * 0.5))
	var arr := PackedVector4Array()
	# the first point is the stern itself, so the trail starts at the hull
	arr.append(Vector4(abs_x - fwd.x * LENGTH * 0.45 - origin_x, abs_z - fwd.y * LENGTH * 0.45 - origin_z, 0.0, speed))
	for w in wake:
		arr.append(Vector4(w[0] - origin_x, w[1] - origin_z, now - w[2], w[3]))
	while arr.size() < 24:
		arr.append(Vector4.ZERO)
	mat.set_shader_parameter("wake", arr)
	mat.set_shader_parameter("wake_count", mini(wake.size() + 1, 24))


# --- The model ---------------------------------------------------------------

## Half-width, keel rise and gunwale height at station s (-1 stern .. +1 bow).
func _section(s: float) -> Dictionary:
	var a := absf(s)
	var top := DEPTH + 0.55 * a * a + (0.25 if s > 0.0 else 0.35) * pow(a, 6.0)
	var w := BEAM * 0.5 * (1.0 - pow(a, 2.4) * 0.92)
	var bottom_w := w * 0.62 * (1.0 - pow(a, 1.6))
	var rise := 0.55 * pow(a, 3.0)
	return {"top": top, "w": maxf(w, 0.12), "bw": maxf(bottom_w, 0.02), "rise": rise}


## Points around one side of a section, keel to gunwale (x = out, y = up).
func _profile(s: float) -> Array:
	var sec := _section(s)
	var w: float = sec["w"]
	var bw: float = sec["bw"]
	var r: float = sec["rise"]
	var t: float = sec["top"]
	return [Vector2(0, r), Vector2(bw, r), Vector2(bw + (w - bw) * 0.55, r + (t - r) * 0.18),
		Vector2(w * 0.93, r + (t - r) * 0.45), Vector2(w * 0.99, r + (t - r) * 0.75), Vector2(w, t)]


## z along the hull for station s at height fraction f: raked posts, so the top reaches further.
func _z(s: float, f: float) -> float:
	var reach := lerpf(0.8, 1.0, f) if absf(s) > 0.0 else 1.0
	return -s * LENGTH * 0.5 * lerpf(1.0, reach, smoothstep(0.55, 1.0, absf(s)))


func build_hull() -> ArrayMesh:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var n := 28
	var tar := Color(0.16, 0.12, 0.09)
	var plank := Color(0.36, 0.25, 0.16)
	var plank2 := Color(0.43, 0.31, 0.2)
	var inner := Color(0.52, 0.4, 0.27)
	var rows: Array = []
	for i in n + 1:
		var s := lerpf(-1.0, 1.0, float(i) / n)
		var prof := _profile(s)
		var top: float = prof[prof.size() - 1].y
		var row: Array = []
		for j in prof.size():
			var pt: Vector2 = prof[j]
			row.append(Vector3(pt.x, pt.y, _z(s, pt.y / top)))
		rows.append(row)
	var m: int = rows[0].size()
	for side in [1.0, -1.0]:
		for i in n:
			for j in m - 1:
				var a: Vector3 = rows[i][j]
				var b: Vector3 = rows[i + 1][j]
				var c: Vector3 = rows[i + 1][j + 1]
				var d: Vector3 = rows[i][j + 1]
				a.x *= side; b.x *= side; c.x *= side; d.x *= side
				var col := tar if j < 2 else (plank if j % 2 == 0 else plank2)
				_quad(st, a, b, c, d, col, side > 0.0)
				# the inside of the planking, a hand's breadth in
				var o := Vector3(-side * 0.07, 0.0, 0.0)
				_quad(st, d + o, c + o, b + o, a + o, inner, side > 0.0)
		# gunwale cap
		for i in n:
			var a: Vector3 = rows[i][m - 1]
			var b: Vector3 = rows[i + 1][m - 1]
			a.x *= side; b.x *= side
			var up := Vector3(0, 0.08, 0)
			var o := Vector3(-side * 0.1, 0, 0)
			_quad(st, a + up, b + up, b + up + o, a + up + o, Color(0.3, 0.21, 0.13), side > 0.0)
	# Hold floor and the short decks fore and aft.
	var floor_y := 0.75
	_box(st, Vector3(0, floor_y, 0), Vector3(1.9, 0.05, 6.5), Color(0.5, 0.38, 0.25))
	_box(st, Vector3(0, DEPTH - 0.25, -6.3), Vector3(1.4, 0.06, 1.8), Color(0.55, 0.42, 0.28))
	_box(st, Vector3(0, DEPTH - 0.1, 6.2), Vector3(1.6, 0.06, 2.0), Color(0.55, 0.42, 0.28))
	# Stem and stern posts.
	_post(st, Vector3(0, 0.5, -LENGTH * 0.4), Vector3(0, DEPTH + 1.4, -LENGTH * 0.52), 0.14, Color(0.25, 0.17, 0.11))
	_post(st, Vector3(0, 0.5, LENGTH * 0.4), Vector3(0, DEPTH + 1.6, LENGTH * 0.51), 0.14, Color(0.25, 0.17, 0.11))
	# Mast, yard, stays.
	var mast_base := Vector3(0, 0.6, -1.2)
	_post(st, mast_base, mast_base + Vector3(0, MAST_H, 0), 0.17, Color(0.4, 0.29, 0.18))
	_post(st, mast_base + Vector3(0, MAST_H - 0.3, 0), Vector3(0, DEPTH + 1.3, -LENGTH * 0.52), 0.025, Color(0.2, 0.16, 0.1))
	_post(st, mast_base + Vector3(0, MAST_H - 0.3, 0), Vector3(1.9, DEPTH, 3.5), 0.025, Color(0.2, 0.16, 0.1))
	_post(st, mast_base + Vector3(0, MAST_H - 0.3, 0), Vector3(-1.9, DEPTH, 3.5), 0.025, Color(0.2, 0.16, 0.1))
	# Cargo: barrels of wine and bales of Frisian cloth.
	var rng := RandomNumberGenerator.new()
	rng.seed = 800
	for k in 7:
		var z := lerpf(-4.8, 4.4, k / 6.0)
		if absf(z - (-1.2)) < 0.8:
			continue
		for sx in [-0.65, 0.65]:
			if rng.randf() < 0.55:
				_barrel(st, Vector3(sx, floor_y + 0.45, z), Color(0.47, 0.33, 0.2))
			else:
				var cloth: Color = [Color(0.62, 0.2, 0.16), Color(0.78, 0.72, 0.58), Color(0.3, 0.38, 0.52)][rng.randi() % 3]
				_box(st, Vector3(sx, floor_y + 0.35, z), Vector3(0.55, 0.32, 0.5), cloth)
	# Steering oar on the starboard quarter.
	_post(st, Vector3(BEAM * 0.5, DEPTH + 0.6, 6.4), Vector3(BEAM * 0.55, -0.9, 7.6), 0.09, Color(0.35, 0.25, 0.15))
	_box(st, Vector3(BEAM * 0.56, -0.6, 7.45), Vector3(0.05, 0.55, 0.28), Color(0.35, 0.25, 0.15))
	# Crew.
	var tunics := [Color(0.55, 0.22, 0.15), Color(0.38, 0.42, 0.3), Color(0.6, 0.55, 0.42), Color(0.3, 0.33, 0.45), Color(0.5, 0.4, 0.25)]
	var spots := [Vector3(1.2, DEPTH - 0.2, 6.6), Vector3(-0.4, floor_y + 0.05, 2.5), Vector3(0.5, floor_y + 0.05, -3.0), Vector3(-0.3, DEPTH - 0.3, -6.4), Vector3(0.9, floor_y + 0.05, 0.6)]
	for k in spots.size():
		_person(st, spots[k], tunics[k])
	st.generate_normals()
	return st.commit()


func _quad(st: SurfaceTool, a: Vector3, b: Vector3, c: Vector3, d: Vector3, col: Color, flip: bool) -> void:
	var tri := [a, b, c, a, c, d] if flip else [a, c, b, a, d, c]
	for v in tri:
		st.set_color(col)
		st.add_vertex(v)


func _box(st: SurfaceTool, c: Vector3, half: Vector3, col: Color) -> void:
	var p := [Vector3(-1, -1, -1), Vector3(1, -1, -1), Vector3(1, 1, -1), Vector3(-1, 1, -1),
		Vector3(-1, -1, 1), Vector3(1, -1, 1), Vector3(1, 1, 1), Vector3(-1, 1, 1)]
	var faces := [[0, 1, 2, 3], [5, 4, 7, 6], [4, 0, 3, 7], [1, 5, 6, 2], [3, 2, 6, 7], [4, 5, 1, 0]]
	for f in faces:
		var q: Array = []
		for i in f:
			q.append(c + p[i] * half)
		_quad(st, q[0], q[1], q[2], q[3], col, false)


func _post(st: SurfaceTool, a: Vector3, b: Vector3, r: float, col: Color) -> void:
	var axis := (b - a).normalized()
	var u := axis.cross(Vector3.RIGHT if absf(axis.x) < 0.9 else Vector3.UP).normalized()
	var v := axis.cross(u)
	var sides := 6
	for i in sides:
		var t0 := TAU * i / sides
		var t1 := TAU * (i + 1) / sides
		var o0 := (u * cos(t0) + v * sin(t0)) * r
		var o1 := (u * cos(t1) + v * sin(t1)) * r
		_quad(st, a + o0, a + o1, b + o1, b + o0, col, true)


func _barrel(st: SurfaceTool, c: Vector3, col: Color) -> void:
	var sides := 8
	for i in sides:
		var t0 := TAU * i / sides
		var t1 := TAU * (i + 1) / sides
		for k in 2:
			var y0 := -0.45 + k * 0.45
			var y1 := y0 + 0.45
			var r0 := 0.3 + (0.05 if k == 1 else 0.0) * 0.0 + (0.05 if y0 > -0.3 else 0.0)
			var r1 := 0.3 + (0.05 if y1 < 0.4 else 0.0)
			_quad(st, c + Vector3(cos(t0) * r0, y0, sin(t0) * r0), c + Vector3(cos(t1) * r0, y0, sin(t1) * r0),
				c + Vector3(cos(t1) * r1, y1, sin(t1) * r1), c + Vector3(cos(t0) * r1, y1, sin(t0) * r1),
				col.darkened(0.1 * k), true)
		_quad(st, c + Vector3(0, 0.45, 0), c + Vector3(0, 0.45, 0), c + Vector3(cos(t1) * 0.3, 0.45, sin(t1) * 0.3),
			c + Vector3(cos(t0) * 0.3, 0.45, sin(t0) * 0.3), col.lightened(0.1), true)


func _person(st: SurfaceTool, feet: Vector3, tunic: Color) -> void:
	_box(st, feet + Vector3(0, 0.45, 0), Vector3(0.12, 0.45, 0.09), Color(0.35, 0.3, 0.24))    # legs
	_box(st, feet + Vector3(0, 1.2, 0), Vector3(0.22, 0.34, 0.14), tunic)                      # body
	_box(st, feet + Vector3(0, 1.68, 0), Vector3(0.11, 0.13, 0.11), Color(0.85, 0.66, 0.52))   # head
	_box(st, feet + Vector3(0, 1.8, 0), Vector3(0.12, 0.04, 0.12), Color(0.45, 0.33, 0.2))     # cap


## A grid for the sail; the shader billows and colours it.
func build_sail() -> ArrayMesh:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var w := 8.6
	var h := 7.2
	var nx := 12
	var ny := 10
	for j in ny:
		for i in nx:
			var p := func(a: int, b: int) -> Vector3: return Vector3(lerpf(-w / 2, w / 2, float(a) / nx), -h * float(b) / ny, 0.0)
			var uv := func(a: int, b: int) -> Vector2: return Vector2(float(a) / nx, float(b) / ny)
			for c in [[i, j], [i + 1, j], [i + 1, j + 1], [i, j], [i + 1, j + 1], [i, j + 1]]:
				st.set_uv(uv.call(c[0], c[1]))
				st.set_normal(Vector3(0, 0, 1))
				st.add_vertex(p.call(c[0], c[1]))
	# the yard it hangs from
	return st.commit()
