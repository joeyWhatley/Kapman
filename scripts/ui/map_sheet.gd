extends "res://scripts/ui/paper.gd"
## The map sheet: route network, ports, your ship. Click a joined port to sail.

signal port_clicked(port_name: String)

const D := preload("res://scripts/data.gd")
const MAP_ORIGIN := Vector2(10, 50)
const DOCK_OFFSET := Vector2(-22, -20)
const REGIONS := [["NORTH SEA", 380, 230, 26], ["ENGLAND", 40, 440, 16], ["FRANCIA", 560, 640, 16],
	["FRISIA", 560, 420, 14], ["JUTLAND", 690, 300, 14], ["THE NARROW SEA", 110, 720, 12]]

var state
var busy := false
var hover := ""
var sailing_to := ""
var ship_pos := Vector2.ZERO:
	set(v):
		if v != ship_pos:
			heading = (v - ship_pos).normalized()
		ship_pos = v
		queue_redraw()
var heading := Vector2.UP


func _ready() -> void:
	title = "KĀPMAN"
	mouse_filter = Control.MOUSE_FILTER_STOP


func port_point(p: String) -> Vector2:
	return MAP_ORIGIN + D.PORTS[p]["pos"]


func dock_point(p: String) -> Vector2:
	return port_point(p) + DOCK_OFFSET


func _port_at(pos: Vector2) -> String:
	for p in D.PORTS:
		if pos.distance_to(port_point(p)) < 24.0:
			return p
	return ""


func _gui_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		var h := _port_at(event.position)
		if h != hover:
			hover = h
			mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND if _reachable(h) else Control.CURSOR_ARROW
			queue_redraw()
	elif event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var p := _port_at(event.position)
		if p != "":
			port_clicked.emit(p)


func _reachable(p: String) -> bool:
	if p == "" or state == null or state.port == "" or p == state.port:
		return false
	var r: Dictionary = state.route(state.port, p)
	return not r.is_empty() and not state.is_closed(r)


func _dotted(a: Vector2, b: Vector2, color: Color) -> void:
	var n := int(a.distance_to(b) / 10.0)
	for i in n + 1:
		draw_circle(a.lerp(b, float(i) / max(1, n)), 2.2, color)


func _draw() -> void:
	draw_paper()
	if state == null:
		return
	var s := "%s · Day %d of %d · Silver %ds · Debt %ds" % [D.SEASONS[state.season]["name"], state.day, state.deadline(), state.silver, state.debt]
	draw_string(Ink.pencil(), Vector2(16, 34), s, HORIZONTAL_ALIGNMENT_RIGHT, size.x - 32, 17, Ink.PENCIL)

	for reg in REGIONS:
		draw_string(Ink.bold(), MAP_ORIGIN + Vector2(reg[1], reg[2]), reg[0], HORIZONTAL_ALIGNMENT_LEFT, -1, reg[3], Ink.GHOST)

	# Routes
	for r in D.ROUTES:
		var a := port_point(r["a"])
		var b := port_point(r["b"])
		var closed: bool = state.is_closed(r)
		var from_here: bool = state.port != "" and (r["a"] == state.port or r["b"] == state.port)
		var col := Ink.FAINT if closed else Ink.INK
		var w := 4.0 if from_here and not closed else 2.5
		match r["type"]:
			"open":
				draw_dashed_line(a, b, col, w, 12.0)
			"land":
				_dotted(a, b, col)
			_:
				draw_line(a, b, col, w)
		var m := (a + b) / 2.0
		var box := Rect2(m - Vector2(18, 12), Vector2(36, 24))
		draw_rect(box, Ink.PAPER)
		draw_rect(box, col, false, 1.5)
		draw_string(Ink.bold(), m + Vector2(-18, 6), "%dd" % r["days"], HORIZONTAL_ALIGNMENT_CENTER, 36, 15, col)
		if closed:
			draw_line(m + Vector2(-16, -16), m + Vector2(16, 16), Ink.RED, 3.0)
			draw_line(m + Vector2(-16, 16), m + Vector2(16, -16), Ink.RED, 3.0)
			draw_string(Ink.pencil(), m + Vector2(-30, 32), "closed", HORIZONTAL_ALIGNMENT_CENTER, 60, 13, Ink.RED)

	# Voyage line
	if sailing_to != "":
		draw_line(ship_pos, port_point(sailing_to), Ink.RED, 1.5)

	# Ports
	for p in D.PORTS:
		var c := port_point(p)
		var info: Dictionary = D.PORTS[p]
		if p == state.port:
			draw_circle(c, 24.0, Ink.WASH)
		if _reachable(p):
			draw_arc(c, 19.0, 0, TAU, 32, Ink.PENCIL, 1.5)
			if p == hover:
				draw_circle(c, 22.0, Ink.WASH)
		var rad := 13.0 if p == D.HOME else 10.0
		draw_circle(c, rad, Ink.PAPER)
		draw_arc(c, rad, 0, TAU, 32, Ink.INK, 3.0)
		draw_string(Ink.bold(), c + Vector2(-8, 4.5), info["fac"], HORIZONTAL_ALIGNMENT_CENTER, 16, 11, Ink.INK)
		var tp := c + Vector2(18, -4)
		var align := HORIZONTAL_ALIGNMENT_LEFT
		var lw := 180.0
		match info["lab"]:
			"left":
				tp = c + Vector2(-24 - lw, -8)
				align = HORIZONTAL_ALIGNMENT_RIGHT
			"below":
				tp = c + Vector2(-lw / 2.0, 30)
				align = HORIZONTAL_ALIGNMENT_CENTER
			"above":
				tp = c + Vector2(-lw / 2.0, -34)
				align = HORIZONTAL_ALIGNMENT_CENTER
		draw_string(Ink.bold(), tp, p, align, lw, 17, Ink.INK)
		draw_string(Ink.sans(), tp + Vector2(0, 15), info["tag"], align, lw, 12, Ink.FAINT)

	_draw_ship()

	# Footer: hover info + legend
	var foot := "Click a port joined to yours to sail there."
	if busy:
		foot = "Under way…"
	elif state.over:
		foot = "The voyage is over."
	elif hover != "" and hover != state.port:
		var r: Dictionary = state.route(state.port, hover) if state.port != "" else {}
		if r.is_empty():
			foot = "%s: no direct route from %s." % [hover, state.port]
		elif state.is_closed(r):
			foot = "%s: route closed this season." % hover
		else:
			var dg: int = state.danger(r)
			var enc := "no encounters" if dg == 0 else ("encounter on 1" if dg == 1 else "encounter on 1–%d" % dg)
			var days: int = state.voyage_days(r)
			foot = "→ %s: %d days, %s, %s. Upkeep about %ds." % [hover, days, r["type"], enc, state.upkeep(days, r)]
	draw_string(Ink.pencil(), Vector2(16, size.y - 34), foot, HORIZONTAL_ALIGNMENT_LEFT, size.x - 32, 15, Ink.PENCIL)
	var lg := "———  coastal: encounter on 1      – – –  open sea: 1–2      · · ·  overland: none, 1s per lot      F Frankish · E English · D Danish"
	draw_string(Ink.sans(), Vector2(16, size.y - 12), lg, HORIZONTAL_ALIGNMENT_LEFT, size.x - 32, 12, Ink.FAINT)


func _draw_ship() -> void:
	var h := heading if heading != Vector2.ZERO else Vector2.UP
	if sailing_to == "":
		h = Vector2.UP
	var side := h.orthogonal()
	var p := ship_pos
	var hull := PackedVector2Array([p + h * 13.0, p - h * 8.0 + side * 7.0, p - h * 8.0 - side * 7.0])
	draw_colored_polygon(hull, Ink.RED)
	draw_polyline(PackedVector2Array([hull[0], hull[1], hull[2], hull[0]]), Ink.INK, 1.5)
