extends RefCounted
## A planned voyage: river approach out of the origin, an editable sea course, river approach into
## the destination. Plus the sailing model: how fast a square-rigged ship goes at each angle to the
## wind, when the crew has to row, and how currents help or hinder.

const Geo := preload("res://route_map/geo.gd")

const SEA_SPEED := 90.0      # map units per day, beam reach in a moderate breeze
const RIVER_SPEED := 70.0    # rowing, towing, waiting on tides
const ROW_SPEED := 0.5       # fraction of SEA_SPEED when rowing (full crew of 6)
const CURRENT_SPEED := 30.0  # map units per day added by a current of strength 1 dead astern
const MIN_SPEED := 12.0
const PORTAGE_SPEED := 14.0   # hauling the ship overland on rollers
const FLOW_EFFECT := 0.4     # rivers: downstream up to +40%, upstream down to -40%
enum { SEA, RIVER, PORTAGE }

var origin := ""
var dest := ""
var sea := PackedVector2Array()   # [origin anchorage, ...waypoints..., dest anchorage]
var aground: Array = []           # indices of sea segments that cross land
var head := PackedVector2Array()  # river approach out of the origin (port -> anchorage)
var tail := PackedVector2Array()  # river approach into the destination (anchorage -> port)

var _path := PackedVector2Array()
var _seg_days := PackedFloat32Array()
var _seg_len := PackedFloat32Array()
var _seg_kind := PackedByteArray()
var _seg_rowed := PackedByteArray()
var total_days := 0.0             # estimate, using the season's prevailing wind
var total_len := 0.0
var sea_units := 0.0
var river_units := 0.0
var row_units := 0.0              # sea distance you'll have to row (estimate)
var portage_units := 0.0
var _map = null
var wind_dir := Vector2.ZERO      # direction the wind blows toward
var wind_strength := 0.0
var wind_score := 0.0             # -1 dead against .. +1 dead behind, averaged over the sea course
var crew := 6
var current_fn := Callable()      # func(p: Vector2) -> Vector2


func setup(from: String, to: String, course: PackedVector2Array, origin_approach: PackedVector2Array, dest_approach: PackedVector2Array) -> void:
	origin = from
	dest = to
	sea = course
	head = origin_approach.duplicate()
	head.reverse()
	tail = dest_approach


func harbors() -> Array:
	return [sea[0], sea[sea.size() - 1]]


## Point of sail for a square-rigged ship. `wind` points where the wind blows, its length is the
## strength. Returns [speed factor (x SEA_SPEED), rowing?]. Within ~55° of the wind you must row.
static func sail_factor(dir: Vector2, wind: Vector2, crew_count: int) -> Array:
	var row := ROW_SPEED * clampf(crew_count / 6.0, 0.3, 1.34)
	var s := wind.length()
	if s < 0.03:
		return [row, true]                         # becalmed
	var theta := rad_to_deg(absf(dir.angle_to(wind)))   # 0 = wind dead astern
	var f := 0.0
	if theta <= 35.0:
		f = 1.0                                    # running before the wind
	elif theta <= 80.0:
		f = 1.12                                   # broad reach: fastest for a square sail
	elif theta <= 105.0:
		f = 1.0                                    # beam reach
	elif theta <= 130.0:
		f = 0.8                                    # close-hauled: ~50° off the wind, as close as a square sail goes
	else:
		return [row, true]
	f *= 0.8 + minf(s, 1.0) * 1.5
	return [f, false] if f >= row else [row, true]


## Recomputes length, days and grounding with the season's prevailing wind. Call after any edit.
func evaluate(map, wind := Vector2.ZERO, strength := 0.0, crew_count := 6, currents := Callable()) -> void:
	wind_dir = wind
	wind_strength = strength
	crew = crew_count
	current_fn = currents
	_map = map
	aground.clear()
	for i in sea.size() - 1:
		if not map.line_clear(sea[i], sea[i + 1], harbors()):
			aground.append(i)
	_path = PackedVector2Array()
	_seg_days = PackedFloat32Array()
	_seg_len = PackedFloat32Array()
	_seg_kind = PackedByteArray()
	_seg_rowed = PackedByteArray()
	sea_units = 0.0
	river_units = 0.0
	row_units = 0.0
	portage_units = 0.0
	_add_run(head, true)
	var before_sea := sea_units
	_add_run(sea, false)
	wind_score = _wind_score() if sea_units > before_sea else 0.0
	_add_run(tail, true)
	total_days = 0.0
	total_len = 0.0
	for i in _seg_days.size():
		total_days += _seg_days[i]
		total_len += _seg_len[i]


## What kind of water is at p: SEA, RIVER (rivers and lakes) or PORTAGE.
func kind_at(p: Vector2) -> int:
	if _map == null:
		return SEA
	match _map.kind_at(p):
		2:
			return RIVER
		3:
			return PORTAGE
	return SEA


## Speed (map units/day) heading `dir` at `p` on water of `kind`, for a given wind vector.
## Returns [speed, rowing?]. Rivers: rowing, faster downstream. Lakes: sail as at sea.
func speed_at(p: Vector2, dir: Vector2, kind: int, wind: Vector2) -> Array:
	var crew_f := clampf(crew / 6.0, 0.5, 1.25)
	if kind == PORTAGE:
		return [PORTAGE_SPEED * crew_f, true]
	if kind == RIVER:
		var fl: Vector2 = _map.flow_at(p) if _map else Vector2.ZERO
		if fl != Vector2.ZERO:
			return [maxf(RIVER_SPEED * crew_f * (1.0 + FLOW_EFFECT * dir.dot(fl)), MIN_SPEED), true]
		if _map == null:
			return [RIVER_SPEED * crew_f, true]
	var sf: Array = sail_factor(dir, wind, crew)
	var sp: float = SEA_SPEED * sf[0]
	if current_fn.is_valid():
		sp += current_fn.call(p).dot(dir) * CURRENT_SPEED
	return [maxf(sp, MIN_SPEED), sf[1]]


## Adds a run of points. The sea course is split wherever the kind of water changes, so rivers,
## lakes and portages along it each get their own speed. Port approaches are river throughout.
func _add_run(pts: PackedVector2Array, approach: bool) -> void:
	for i in pts.size():
		if _path.is_empty():
			_path.append(pts[i])
			continue
		if pts[i].is_equal_approx(_path[_path.size() - 1]):
			continue
		var a := _path[_path.size() - 1]
		var b := pts[i]
		if approach:
			_add_seg(a, b, RIVER)
			continue
		var steps := int(a.distance_to(b) / 2.5) + 1
		var start := a
		var cur := kind_at(a.lerp(b, 0.5 / steps))
		for s in range(1, steps + 1):
			var q := a.lerp(b, float(s) / steps)
			var k := kind_at(a.lerp(b, minf((s + 0.5) / steps, 1.0)))
			if s == steps:
				_add_seg(start, q, cur)
			elif k != cur:
				_add_seg(start, q, cur)
				start = q
				cur = k


func _add_seg(a: Vector2, b: Vector2, kind: int) -> void:
	var seg_len := a.distance_to(b)
	if seg_len < 0.001:
		return
	var dir := (b - a) / seg_len
	var sp: Array = speed_at(a.lerp(b, 0.5), dir, kind, wind_dir.normalized() * wind_strength)
	_seg_days.append(seg_len / sp[0])
	_seg_len.append(seg_len)
	_seg_kind.append(kind)
	_seg_rowed.append(1 if (sp[1] and kind == SEA) else 0)
	_path.append(b)
	match kind:
		RIVER:
			river_units += seg_len
		PORTAGE:
			portage_units += seg_len
		_:
			sea_units += seg_len
			if sp[1]:
				row_units += seg_len


func _wind_score() -> float:
	var total := 0.0
	var weighted := 0.0
	for i in sea.size() - 1:
		var l := sea[i].distance_to(sea[i + 1])
		total += l
		weighted += l * (sea[i + 1] - sea[i]).normalized().dot(wind_dir)
	return weighted / maxf(total, 0.001)


## True if the given full-path segment is (by the estimate) rowed.
func seg_rowed(i: int) -> bool:
	return i >= 0 and i < _seg_rowed.size() and _seg_rowed[i] == 1


## Evenly spaced points along the whole path: [position, direction] every step map units.
func samples(step: float) -> Array:
	var out: Array = []
	var carry := 0.0
	for i in _path.size() - 1:
		var a := _path[i]
		var b := _path[i + 1]
		var l := a.distance_to(b)
		var d := carry
		while d < l:
			out.append([a.lerp(b, d / l), (b - a).normalized()])
			d += step
		carry = d - l
	return out


func ok() -> bool:
	return aground.is_empty() and origin != dest


func full_path() -> PackedVector2Array:
	return _path


## Position and heading after t days under way (using the estimate's timing).
func pose_at(t: float) -> Array:
	var acc := 0.0
	for i in _seg_days.size():
		var d := _seg_days[i]
		if t <= acc + d or i == _seg_days.size() - 1:
			var f := clampf((t - acc) / maxf(d, 0.0001), 0.0, 1.0)
			var a := _path[i]
			var b := _path[i + 1]
			return [a.lerp(b, f), (b - a).normalized(), i]
		acc += d
	return [_path[_path.size() - 1], Vector2.UP, _seg_days.size() - 1]


## Position, heading, segment index and river flag at a distance along the path (live sailing).
func at_distance(dist: float) -> Array:
	var acc := 0.0
	for i in _seg_len.size():
		var l := _seg_len[i]
		if dist <= acc + l or i == _seg_len.size() - 1:
			var f := clampf((dist - acc) / maxf(l, 0.0001), 0.0, 1.0)
			var a := _path[i]
			var b := _path[i + 1]
			return [a.lerp(b, f), (b - a).normalized(), i, _seg_kind[i]]
		acc += l
	return [_path[_path.size() - 1], Vector2.UP, _seg_len.size() - 1, RIVER]


## Planned-days equivalent of a distance (for incidents and the "day x of y" label).
func days_at_distance(dist: float) -> float:
	var acc := 0.0
	var t := 0.0
	for i in _seg_len.size():
		var l := _seg_len[i]
		if dist <= acc + l:
			return t + _seg_days[i] * clampf((dist - acc) / maxf(l, 0.0001), 0.0, 1.0)
		acc += l
		t += _seg_days[i]
	return total_days


static func km(units: float) -> int:
	return int(round(units * Geo.KM_PER_UNIT))
