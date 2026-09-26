extends Node2D
## The living map: gull flocks, distant sails going about their trade, currents as faint flowing
## ink, and port life (boats, chimney smoke, pennants). Port lights at night are drawn into a
## separate node (`lights`) that sits above the night tint. Only charted, on-screen things are drawn.

const Style := preload("res://route_map/style.gd")

const NPC_COUNT := 7
const NPC_SPEED := 9.0        # map units per real second
const GULL_FLOCKS := 5
const LIGHT := Color(1.0, 0.76, 0.38)

var rm                        # the route map
var lights: Node2D:
	set(v):
		lights = v
		if lights:
			lights.draw.connect(_draw_lights)
var night := 0.0
var zoom := 1.0

var _rng := RandomNumberGenerator.new()
var _npcs: Array = []         # {"path", "dist", "len", "speed"}
var _gulls: Array = []        # {"pos", "vel", "age", "life", "n", "seed"}
var _t := 0.0


func setup(route_map) -> void:
	rm = route_map
	_rng.seed = 7919
	var names: Array = rm.index.names()
	var tries := 0
	while _npcs.size() < NPC_COUNT and tries < 40 and names.size() > 1:
		tries += 1
		var a: String = names[_rng.randi_range(0, names.size() - 1)]
		var b: String = names[_rng.randi_range(0, names.size() - 1)]
		if a == b:
			continue
		var path: PackedVector2Array = rm.map.find_route(rm.index.anchorage(a), rm.index.anchorage(b))
		var l := 0.0
		for i in path.size() - 1:
			l += path[i].distance_to(path[i + 1])
		if l < 150.0:
			continue
		_npcs.append({"path": path, "dist": _rng.randf_range(0.0, l), "len": l, "speed": NPC_SPEED * _rng.randf_range(0.7, 1.2), "back": _rng.randf() < 0.5})


func _view() -> Rect2:
	var cam: Camera2D = get_viewport().get_camera_2d()
	var vs := get_viewport_rect().size / maxf(zoom, 0.01)
	return Rect2(cam.get_screen_center_position() - vs / 2.0, vs).grow(40.0 / maxf(zoom, 0.01))


func _process(delta: float) -> void:
	if rm == null:
		return
	_t += delta
	for n in _npcs:
		n["dist"] = fposmod(n["dist"] + n["speed"] * delta, n["len"])
	var view := _view()
	# Gulls live near the camera, in charted water.
	_gulls = _gulls.filter(func(g): return g["age"] < g["life"])
	for g in _gulls:
		g["age"] += delta
		g["pos"] += g["vel"] * delta
	if _gulls.size() < GULL_FLOCKS and zoom > 0.6:
		var p := Vector2(_rng.randf_range(view.position.x, view.end.x), _rng.randf_range(view.position.y, view.end.y))
		if not rm.map.is_land(p) and rm.fog.charted(p) > 0.5:
			_gulls.append({"pos": p, "vel": Vector2.from_angle(_rng.randf() * TAU) * _rng.randf_range(4.0, 9.0),
				"age": 0.0, "life": _rng.randf_range(8.0, 16.0), "n": _rng.randi_range(3, 6), "seed": _rng.randf() * 100.0})
	queue_redraw()
	if lights:
		lights.queue_redraw()


func _draw() -> void:
	if rm == null:
		return
	var view := _view()
	var s := 1.0 / maxf(zoom, 0.01)
	_draw_portages(view, s)
	_draw_currents(view, s)
	_draw_npcs(view, s)
	_draw_ports(view, s)
	_draw_gulls(s)


func _pos_on(path: PackedVector2Array, dist: float) -> Array:
	var acc := 0.0
	for i in path.size() - 1:
		var l := path[i].distance_to(path[i + 1])
		if dist <= acc + l:
			var d := (path[i + 1] - path[i]).normalized()
			return [path[i].lerp(path[i + 1], (dist - acc) / maxf(l, 0.001)), d]
		acc += l
	return [path[path.size() - 1], Vector2.RIGHT]


## Portages: dotted tracks where ships are hauled overland (only where charted).
func _draw_portages(view: Rect2, s: float) -> void:
	for pg in rm.map.portage_runs:
		var pts: PackedVector2Array = pg["pts"]
		for i in pts.size() - 1:
			var n := int(pts[i].distance_to(pts[i + 1]) / (5.0 * s)) + 1
			for k in n:
				var q := pts[i].lerp(pts[i + 1], float(k) / n)
				if view.has_point(q) and rm.fog.charted(q) > 0.4:
					draw_circle(q, 1.3 * s, Color(Style.INK, 0.7))


## Currents as faint ink dashes flowing downstream, only in charted water.
func _draw_currents(view: Rect2, s: float) -> void:
	if zoom < 0.5:
		return
	var col := Color(0.18, 0.32, 0.45, 0.28 * smoothstep(0.5, 0.9, zoom))
	for c in rm.currents.lines:
		if not c["bbox"].intersects(view):
			continue
		var pts: PackedVector2Array = c["pts"]
		for lane in [-0.35, 0.0, 0.35]:
			var acc := 0.0
			for i in pts.size() - 1:
				var a := pts[i]
				var b := pts[i + 1]
				var l := a.distance_to(b)
				var dir := (b - a) / maxf(l, 0.001)
				var reach: float = c["reach"]
				var off: Vector2 = dir.orthogonal() * reach * lane
				var strength: float = c["strength"]
				var phase := fposmod(_t * 14.0 * strength + lane * 17.0, 22.0)
				var d := -phase
				while d < l:
					var p0 := a + off + dir * maxf(d, 0.0)
					var p1 := a + off + dir * minf(d + 8.0, l)
					if d + 8.0 > 0.0 and view.has_point(p0) and rm.fog.charted(p0) > 0.5:
						draw_line(p0, p1, col, 1.0 * s)
					d += 22.0
				acc += l


func _mini_ship(p: Vector2, dir: Vector2, s: float, alpha: float) -> void:
	draw_set_transform(p, dir.angle(), Vector2(s, s))
	var hull := PackedVector2Array([Vector2(8, 0), Vector2(3, 2.4), Vector2(-5, 2.4), Vector2(-8, 0), Vector2(-5, -2.4), Vector2(3, -2.4)])
	draw_colored_polygon(hull, Color(Style.PAPER, alpha))
	hull.append(hull[0])
	draw_polyline(hull, Color(Style.INK, alpha), 1.0, true)
	var sway := sin(_t * 1.7 + p.x) * 0.3
	draw_line(Vector2(0.5, -4.5).rotated(sway), Vector2(0.5, 4.5).rotated(sway), Color(Style.INK, alpha), 1.4, true)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _draw_npcs(view: Rect2, s: float) -> void:
	for n in _npcs:
		var d: float = n["len"] - n["dist"] if n["back"] else n["dist"]
		var pd: Array = _pos_on(n["path"], d)
		var p: Vector2 = pd[0]
		if not view.has_point(p):
			continue
		var c: float = rm.fog.charted(p)
		if c < 0.3:
			continue
		var dir: Vector2 = -pd[1] if n["back"] else pd[1]
		_mini_ship(p, dir, s, 0.55 * c)


func _draw_ports(view: Rect2, s: float) -> void:
	if zoom < 0.55:
		return
	for pn in rm.index.names():
		var p: Vector2 = rm.index.position(pn)
		if not view.has_point(p):
			continue
		var c: float = rm.fog.charted(p)
		if c < 0.4:
			continue
		var tier: int = rm.index.ports[pn]["tier"]
		var seed := float(pn.hash() % 1000)
		# Boats working around the harbour.
		for k in (4 - tier):
			var ang := _t * (0.25 + 0.07 * k) + seed + k * 2.1
			var r := (11.0 + k * 5.0) * s
			var bp := p + Vector2(cos(ang) * r * 1.3, sin(ang) * r)
			var bd := Vector2(-sin(ang) * 1.3, cos(ang)).normalized()
			draw_set_transform(bp, bd.angle(), Vector2(s, s))
			draw_polyline(PackedVector2Array([Vector2(3, 0), Vector2(-2.5, 1.2), Vector2(-2.5, -1.2), Vector2(3, 0)]), Color(Style.INK, 0.6 * c), 0.9, true)
			draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
		# Chimney smoke curling up and drifting.
		for k in 2:
			var base := p + Vector2(5 + k * 5, -5) * s
			var wisp := PackedVector2Array()
			for i in 9:
				var h := i * 3.2
				wisp.append(base + Vector2(sin(_t * 1.4 + i * 0.7 + seed + k) * (1.0 + i * 0.35) + i * 0.6, -h) * s)
			draw_polyline(wisp, Color(Style.INK, 0.16 * c), 1.0 * s, true)
		# Pennant on a pole.
		var pole := p + Vector2(-9, -3) * s
		draw_line(pole, pole + Vector2(0, -12) * s, Color(Style.INK, 0.7 * c), 1.0 * s)
		var wave := sin(_t * 6.0 + seed) * 1.2
		draw_colored_polygon(PackedVector2Array([pole + Vector2(0, -12) * s, pole + Vector2(8, -10.5 + wave) * s, pole + Vector2(0, -8.5) * s]), Color(Style.ACCENT, 0.85 * c))


func _draw_gulls(s: float) -> void:
	for g in _gulls:
		var life: float = g["life"]
		var a := minf(1.0, minf(g["age"], life - g["age"]) / 1.5) * 0.6
		for i in g["n"]:
			var off := Vector2(sin(g["seed"] + i * 1.7), cos(g["seed"] * 0.7 + i * 2.3)) * 9.0 * s
			var p: Vector2 = g["pos"] + off + Vector2(sin(_t * 0.8 + i), cos(_t * 0.6 + i)) * 2.0 * s
			var flap := sin(_t * 8.0 + i * 1.3) * 1.6
			var w := 3.2 * s
			draw_polyline(PackedVector2Array([p + Vector2(-w, -flap * s), p, p + Vector2(w, -flap * s)]), Color(Style.INK, a), 0.9 * s, true)


## Warm port lights and the ship's lantern, drawn above the night tint.
func _draw_lights() -> void:
	if rm == null or night < 0.03:
		return
	var view := _view()
	var s := 1.0 / maxf(zoom, 0.01)
	for pn in rm.index.names():
		var p: Vector2 = rm.index.position(pn)
		if not view.has_point(p) or rm.fog.charted(p) < 0.4:
			continue
		var tier: int = rm.index.ports[pn]["tier"]
		var flicker := 0.85 + 0.15 * sin(_t * 9.0 + float(pn.hash() % 97))
		for r in [14.0, 8.0, 4.0]:
			lights.draw_circle(p, r * s * (1.3 if tier == 1 else 1.0), Color(LIGHT, 0.12 * night * flicker))
		for k in (5 - tier):
			var q := p + Vector2(sin(k * 2.4 + pn.length()), cos(k * 1.7)) * 6.0 * s
			lights.draw_circle(q, 1.2 * s, Color(LIGHT, 0.9 * night))
	var ship: Vector2 = rm.route_layer.ship_pos
	if view.has_point(ship):
		lights.draw_circle(ship, 6.0 * s, Color(LIGHT, 0.15 * night))
		lights.draw_circle(ship, 1.4 * s, Color(LIGHT, 0.9 * night))
