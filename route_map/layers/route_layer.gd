extends Node2D
## The planned course (with edit handles) and the ship.

const Style := preload("res://route_map/style.gd")

var voyage                  # voyage.gd or null
var editing := false
var zoom := 1.0
var hover_handle := -1      # index into sea waypoints
var hover_mid := -1         # index of sea segment whose midpoint is hovered
var sailed_days := -1.0     # >= 0 while under way
var ship_pos := Vector2.ZERO
var ship_dir := Vector2.UP
var show_ship := true
var marks: Array = []        # [position, direction, kind, knowledge] warnings along the course
var hover_mark := -1
var wake: Array = []          # recent ship positions, newest last
var ship_rowing := false
var ship_speed := 0.0         # map units per day
var ship_wind := Vector2.ZERO # wind at the ship (toward, length = strength)
var docked := true
var tilt := 0.0                # 0 flat .. ~0.42 fully tilted (2.5D): mast and sail stand up
const WAKE_LEN := 48
const SHIP_SCALE := 1.6

const HANDLE_R := 6.0
const MID_R := 4.0


func _draw() -> void:
	if voyage != null and voyage.full_path().size() >= 2:
		_draw_course()
		_draw_marks()
	if show_ship:
		_draw_ship()


func add_wake(p: Vector2) -> void:
	wake.append(p)
	if wake.size() > WAKE_LEN:
		wake.pop_front()


func clear_wake() -> void:
	wake.clear()


## Dashes that march along the line (planning mode), phase in map units.
func _marching(a: Vector2, b: Vector2, col: Color, width: float, dash: float, phase: float) -> void:
	var l := a.distance_to(b)
	var dir := (b - a) / maxf(l, 0.001)
	var d := -fposmod(phase, dash * 2.0)
	while d < l:
		var s := maxf(d, 0.0)
		var e := minf(d + dash, l)
		if e > s:
			draw_line(a + dir * s, a + dir * e, col, width)
		d += dash * 2.0


func _draw_course() -> void:
	var w := 1.0 / zoom
	var path: PackedVector2Array = voyage.full_path()
	var sailing := sailed_days >= 0.0
	# River approaches: thin. Sea course: bold. Grounded legs: accent, dashed.
	draw_polyline(path, Color(Style.INK, 0.35 if sailing else 0.55), 1.4 * w, true)
	var sea: PackedVector2Array = voyage.sea
	for i in sea.size() - 1:
		var a := sea[i]
		var b := sea[i + 1]
		if i in voyage.aground:
			draw_dashed_line(a, b, Style.ACCENT, 2.4 * w, 7.0 * w, true, true)
		elif sailing:
			draw_line(a, b, Color(Style.INK, 0.45), 2.2 * w, true)
		else:
			var t := Time.get_ticks_msec() / 1000.0
			draw_line(a, b, Color(Style.INK, 0.18), 2.2 * w, true)
			_marching(a, b, Style.INK, 2.2 * w, 7.0 * w, -t * 22.0 * w)
	if sailing:
		# Ink over the part already sailed.
		var done := PackedVector2Array()
		var acc := 0.0
		done.append(path[0])
		for i in path.size() - 1:
			var seg: float = path[i].distance_to(path[i + 1])
			if ship_pos.distance_to(path[i]) + ship_pos.distance_to(path[i + 1]) <= seg + 0.5:
				done.append(ship_pos)
				break
			done.append(path[i + 1])
			acc += seg
		if done.size() >= 2:
			draw_polyline(done, Style.INK, 2.4 * w, true)
		return
	if not editing:
		return
	for i in sea.size() - 1:
		var m := (sea[i] + sea[i + 1]) / 2.0
		var r := (MID_R + (1.5 if i == hover_mid else 0.0)) * w
		draw_circle(m, r, Style.PAPER)
		draw_arc(m, r, 0, TAU, 20, Color(Style.INK, 0.5), 1.0 * w, true)
		draw_line(m - Vector2(r * 0.55, 0), m + Vector2(r * 0.55, 0), Color(Style.INK, 0.6), 1.0 * w)
		draw_line(m - Vector2(0, r * 0.55), m + Vector2(0, r * 0.55), Color(Style.INK, 0.6), 1.0 * w)
	for i in range(1, sea.size() - 1):
		var r := (HANDLE_R + (2.0 if i == hover_handle else 0.0)) * w
		draw_circle(sea[i], r, Style.PAPER)
		draw_arc(sea[i], r, 0, TAU, 24, Style.INK, 1.6 * w, true)
		draw_circle(sea[i], r * 0.35, Style.INK)


## Subtle warnings: small marks across the course where it passes known or rumoured trouble.
func _draw_marks() -> void:
	var w := 1.0 / zoom
	for i in marks.size():
		var m: Array = marks[i]
		var p: Vector2 = m[0]
		var d: Vector2 = m[1]
		var n := d.orthogonal()
		var a := 0.75 if m[3] >= 2 else 0.4
		if i == hover_mark:
			a = 1.0
		var col := Color(Style.INK, a)
		match m[2]:
			"shoals":
				draw_circle(p + n * 4.0 * w, 1.0 * w, col)
				draw_circle(p - n * 4.0 * w, 1.0 * w, col)
			"storm":
				draw_polyline(PackedVector2Array([p + n * 5.0 * w - d * 2.0 * w, p + n * 2.5 * w + d * 1.0 * w, p + n * 0.0 - d * 1.0 * w, p - n * 2.5 * w + d * 2.0 * w, p - n * 5.0 * w]), col, 1.0 * w, true)
			"fog":
				draw_line(p + n * 5.0 * w, p - n * 5.0 * w, Color(Style.INK, a * 0.5), 2.0 * w)
			"raiders":
				draw_line(p + (n + d) * 3.5 * w, p - (n + d) * 3.5 * w, Color(Style.ACCENT, a), 1.4 * w)
			"cursed":
				draw_arc(p + n * 5.0 * w, 1.8 * w, 0, TAU, 10, col, 0.9 * w, true)


## Index of the warning mark under p, or -1.
func mark_at(p: Vector2) -> int:
	for i in marks.size():
		if p.distance_to(marks[i][0]) < 7.0 / zoom:
			return i
	return -1


func _draw_ship() -> void:
	var s := 1.0 / zoom
	# Wake: a fading, tapering ink trail.
	for i in range(1, wake.size()):
		var k := float(i) / wake.size()
		draw_line(wake[i - 1], wake[i], Color(Style.INK, 0.28 * k), (0.6 + 1.6 * k) * s, true)
	var t := Time.get_ticks_msec() / 1000.0
	var heading := (ship_dir if ship_dir != Vector2.ZERO else Vector2.UP).angle()
	var roll := sin(t * 1.3) * 0.06 if docked else sin(t * 2.4) * 0.05
	var bob := 1.0 + sin(t * 2.1) * 0.02
	if docked:
		# Ripples ringing out from the ship at anchor.
		for k in 3:
			var ph := fposmod(t * 0.35 + k / 3.0, 1.0)
			draw_arc(ship_pos, (12.0 + ph * 32.0) * s, 0, TAU, 40, Color(Style.INK, 0.22 * (1.0 - ph)), 0.8 * s, true)
	draw_set_transform(ship_pos, heading + roll, Vector2(s * bob, s) * SHIP_SCALE)
	var hull := _hull_points()
	if not docked and ship_speed > 0.0:
		# Bow wave: two V strokes that grow with speed.
		var sp := clampf(ship_speed / 110.0, 0.0, 1.3)
		for side in [-1.0, 1.0]:
			draw_line(Vector2(15, 0), Vector2(7 - 4 * sp, side * (5.5 + 5.0 * sp)), Color(Style.INK, 0.45 * sp), 1.0, true)
			draw_line(Vector2(9, side * 4.4), Vector2(2 - 6 * sp, side * (7.0 + 5.0 * sp)), Color(Style.INK, 0.25 * sp), 0.8, true)
	draw_colored_polygon(hull, Style.PAPER)
	var ring := hull.duplicate()
	ring.append(hull[0])
	draw_polyline(ring, Style.INK, 1.3, true)
	# Strakes: two faint lines along the hull
	for f in [0.55, 0.25]:
		var strake := PackedVector2Array()
		for i in 13:
			var x := lerpf(-13.0, 13.0, i / 12.0)
			strake.append(Vector2(x, _half_beam(x / 15.0) * f))
		draw_polyline(strake, Color(Style.INK, 0.35), 0.7, true)
		var mirrored := PackedVector2Array()
		for q in strake:
			mirrored.append(Vector2(q.x, -q.y))
		draw_polyline(mirrored, Color(Style.INK, 0.35), 0.7, true)
	# Oars when rowing
	if ship_rowing and not docked:
		for i in 5:
			var x := -8.0 + i * 4.0
			var sweep := sin(t * 5.0 + i * 0.3) * 3.0
			for side in [-1.0, 1.0]:
				var root := Vector2(x, side * _half_beam(x / 15.0))
				var blade := Vector2(x + sweep, side * (_half_beam(x / 15.0) + 8.0))
				draw_line(root, blade, Style.INK, 0.9, true)
				draw_circle(blade, 0.9, Style.INK)
	# Mast and sail. When the chart is tilted they stand up off the deck (screen-up), with a shadow.
	var mast := Vector2(1.0, 0.0)
	var lift := Vector2(0, -tilt * 60.0).rotated(-(heading + roll)) / Vector2(bob, 1.0)
	if tilt > 0.01:
		draw_line(mast, mast + lift * 1.15, Style.INK, 1.4, true)
	if docked or ship_rowing:
		# Sail furled along the yard, yard lowered fore-and-aft
		draw_line(Vector2(-6, 0) + lift * 0.6, Vector2(8, 0) + lift * 0.6, Style.INK, 2.2, true)
		draw_line(Vector2(-6, 0) + lift * 0.6, Vector2(8, 0) + lift * 0.6, Style.ACCENT, 0.9, true)
	elif tilt <= 0.08:
		var wind_local := ship_wind.rotated(-(heading + roll))
		var strength := clampf(ship_wind.length() * 2.0, 0.35, 1.0)
		var push := wind_local.normalized() if wind_local.length() > 0.01 else Vector2.RIGHT
		# The yard swings square to the wind, but only so far (about 50° off the beam).
		var yard_dir := push.orthogonal()
		var ang := clampf(wrapf(yard_dir.angle() - PI / 2.0, -PI / 2.0, PI / 2.0), -0.87, 0.87)
		yard_dir = Vector2.DOWN.rotated(ang)
		var belly := (1.5 + 3.0 * strength) * (1.0 + sin(t * 7.0) * 0.06)
		var bulge := push * belly
		var sail := PackedVector2Array()
		for i in 9:
			var k := lerpf(-1.0, 1.0, i / 8.0)
			sail.append(mast + yard_dir * 7.5 * k)
		for i in range(8, -1, -1):
			var k := lerpf(-1.0, 1.0, i / 8.0)
			sail.append(mast + yard_dir * 7.0 * k + bulge * (1.0 - k * k))
		if tilt > 0.01:
			draw_colored_polygon(sail, Color(Style.INK, 0.12))   # shadow on the water
			for i in sail.size():
				sail[i] += lift
			mast += lift
		draw_colored_polygon(sail, Color(0.96, 0.93, 0.86))
		for stripe in [-0.5, 0.0, 0.5]:
			draw_line(mast + yard_dir * 7.0 * stripe, mast + yard_dir * 7.0 * stripe + bulge * (1.0 - stripe * stripe), Style.ACCENT, 1.1, true)
		var sail_ring := sail.duplicate()
		sail_ring.append(sail[0])
		draw_polyline(sail_ring, Style.INK, 1.0, true)
		draw_line(mast - yard_dir * 7.8, mast + yard_dir * 7.8, Style.INK, 1.6, true)
	draw_circle(mast, 1.3, Style.INK)
	if docked:
		# Anchor cable off the bow
		draw_line(Vector2(15, 0), Vector2(22, 1.5), Color(Style.INK, 0.5), 0.8, true)
		draw_circle(Vector2(22, 1.5), 1.0, Color(Style.INK, 0.6))
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
	if tilt > 0.08 and not docked and not ship_rowing:
		_upright_sail(heading + roll, s * SHIP_SCALE)


## In 2.5D the sail stands up and faces you: a billowing, striped trapezoid above the mast.
func _upright_sail(angle: float, sc: float) -> void:
	var t := Time.get_ticks_msec() / 1000.0
	var k := smoothstep(0.08, 0.3, tilt)
	var mast_w := ship_pos + Vector2(1.0, 0.0).rotated(angle) * sc
	var h := tilt * 60.0 * sc
	var top := mast_w + Vector2(0, -h * 1.05)
	var bottom := mast_w + Vector2(0, -h * 0.25)
	# How wide the sail looks depends on how the yard lies across your view.
	var yard_w := Vector2(0.0, 1.0).rotated(angle)
	var half := maxf(absf(yard_w.x), 0.35) * 8.0 * sc
	var fill := 1.0 + sin(t * 7.0) * 0.05
	var belly := Vector2(clampf(ship_wind.x, -1.0, 1.0) * 2.5 * sc, 0) * fill
	var sail := PackedVector2Array()
	for i in 7:
		var u := lerpf(-1.0, 1.0, i / 6.0)
		sail.append(top + Vector2(u * half, 0))
	for i in range(6, -1, -1):
		var u := lerpf(-1.0, 1.0, i / 6.0)
		sail.append(bottom + Vector2(u * half * 0.92, 0) + belly * (1.0 - u * u))
	draw_colored_polygon(sail, Color(0.96, 0.92, 0.84, k))
	for u in [-0.5, 0.0, 0.5]:
		draw_line(top + Vector2(u * half, 0), bottom + Vector2(u * half * 0.92, 0) + belly * (1.0 - u * u), Color(Style.ACCENT, k), 1.6 * sc / SHIP_SCALE, true)
	var ring := sail.duplicate()
	ring.append(sail[0])
	draw_polyline(ring, Color(Style.INK, k), 1.1 * sc / SHIP_SCALE, true)
	draw_line(top + Vector2(-half * 1.1, 0), top + Vector2(half * 1.1, 0), Color(Style.INK, k), 1.8 * sc / SHIP_SCALE, true)


func _half_beam(u: float) -> float:
	return 4.8 * pow(maxf(0.0, 1.0 - u * u), 0.75)


## A double-ended hull, 30 ship-units long, bow toward +x.
func _hull_points() -> PackedVector2Array:
	var pts := PackedVector2Array()
	for i in 17:
		var u := lerpf(-1.0, 1.0, i / 16.0)
		pts.append(Vector2(u * 15.0, _half_beam(u)))
	for i in range(15, 0, -1):
		var u := lerpf(-1.0, 1.0, i / 16.0)
		pts.append(Vector2(u * 15.0, -_half_beam(u)))
	return pts

## Waypoint handle (index) under p, or -1.
func handle_at(p: Vector2) -> int:
	if voyage == null or not editing:
		return -1
	for i in range(1, voyage.sea.size() - 1):
		if p.distance_to(voyage.sea[i]) < (HANDLE_R + 4.0) / zoom:
			return i
	return -1


## Sea segment whose midpoint handle is under p, or -1.
func mid_at(p: Vector2) -> int:
	if voyage == null or not editing:
		return -1
	for i in voyage.sea.size() - 1:
		if p.distance_to((voyage.sea[i] + voyage.sea[i + 1]) / 2.0) < (MID_R + 4.0) / zoom:
			return i
	return -1
