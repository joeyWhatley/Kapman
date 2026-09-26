extends "res://scripts/ui/paper.gd"
## The tracker sheet: day track, debt, ship, hold, and the port-side jobs.

signal action(name: String)

const D := preload("res://scripts/data.gd")
const GX := 14.0
const GY := 54.0
const LAB_W := 110.0
const DAY_W := 38.0
const DAY_H := 26.0
const ROWS := [["Spring", 1], ["Summer", 16], ["Autumn", 31], ["Winter", 46], ["Reeve's grace", 61]]
const RIGHT_X := 372.0

var state
var busy := false
var _buttons := {}


func _ready() -> void:
	title = "Tracker"
	subtitle = "days · debt · ship · hold"
	_add("pay10", "Pay 10s", Vector2(GX, 300), 110)
	_add("payall", "Pay all you can", Vector2(GX + 118, 300), 150)
	_add("repair", "Repair Hull +1 (3s)", Vector2(GX, 470), 165)
	_add("hire", "Hire Crew +1 (5s)", Vector2(GX + 173, 470), 160)
	_add("wait", "Wait a day (1s, new mood)", Vector2(GX, 506), 333)


func _add(id: String, text: String, pos: Vector2, w: float) -> void:
	var b := Button.new()
	b.text = text
	b.position = pos
	b.size = Vector2(w, 30)
	b.pressed.connect(func(): action.emit(id))
	add_child(b)
	_buttons[id] = b


func refresh() -> void:
	var in_home: bool = state.port == D.HOME and not state.over
	var why_pay := "" if in_home and state.silver > 0 and state.debt > 0 else "Pay the reeve in Dorestad."
	_gate("pay10", why_pay)
	_gate("payall", why_pay)
	_gate("repair", state.why_not_repair())
	_gate("hire", state.why_not_hire())
	_gate("wait", "" if state.port != "" and not state.over else "Only in port.")
	queue_redraw()


func _gate(id: String, why: String) -> void:
	if busy:
		why = "Busy."
	_buttons[id].disabled = why != ""
	_buttons[id].tooltip_text = why


func _boxes(pos: Vector2, count: int, filled: int, label: String) -> void:
	draw_string(Ink.bold(), pos + Vector2(0, 17), label, HORIZONTAL_ALIGNMENT_LEFT, -1, 15, Ink.INK)
	for i in count:
		var r := Rect2(pos + Vector2(72 + i * 30, 0), Vector2(24, 24))
		draw_rect(r, Ink.INK, false, 1.5)
		if i < filled:
			for k in range(4, 24, 5):
				draw_line(r.position + Vector2(k, 22), r.position + Vector2(k + 6, 2), Ink.PENCIL, 2.0)
	draw_string(Ink.pencil(), pos + Vector2(72 + count * 30 + 6, 18), "%d" % filled, HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Ink.PENCIL)


func _draw() -> void:
	draw_paper()
	if state == null:
		return
	# Day track
	for row in ROWS.size():
		var y := GY + row * DAY_H
		var start: int = ROWS[row][1]
		var grace_row := start == 61
		if grace_row:
			draw_rect(Rect2(GX, y, LAB_W + DAY_W * 15, DAY_H), Ink.GHOST)
		if row == state.season and not grace_row and state.day <= 60:
			draw_rect(Rect2(GX, y, LAB_W, DAY_H), Ink.WASH)
		draw_string(Ink.bold(), Vector2(GX + 5, y + 18), ROWS[row][0], HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Ink.INK)
		for i in 15:
			var d := start + i
			var r := Rect2(GX + LAB_W + i * DAY_W, y, DAY_W, DAY_H)
			var due: bool = d == D.START["due"] or d == D.START["grace_due"]
			if due:
				draw_rect(r, Ink.INK)
			draw_string(Ink.sans(), r.position + Vector2(3, 12), str(d), HORIZONTAL_ALIGNMENT_LEFT, -1, 10, Ink.PAPER if due else Ink.FAINT)
			if d < state.day:
				draw_line(r.position + Vector2(6, 5), r.end - Vector2(6, 4), Ink.PENCIL if not due else Ink.PAPER, 2.0)
				draw_line(Vector2(r.end.x - 6, r.position.y + 5), Vector2(r.position.x + 6, r.end.y - 4), Ink.PENCIL if not due else Ink.PAPER, 2.0)
			elif d == state.day:
				draw_arc(r.get_center(), 11.0, 0, TAU, 24, Ink.RED, 2.5)
	var grid_r := Rect2(GX, GY, LAB_W + DAY_W * 15, DAY_H * ROWS.size())
	draw_rect(grid_r, Ink.INK, false, 1.5)
	for row in ROWS.size():
		draw_line(Vector2(GX, GY + row * DAY_H), Vector2(grid_r.end.x, GY + row * DAY_H), Ink.INK, 1.0)
	for i in 16:
		var x := GX + LAB_W + i * DAY_W
		draw_line(Vector2(x, GY), Vector2(x, grid_r.end.y), Ink.INK, 1.0)

	# Debt & purse
	var y0 := 206.0
	draw_string(Ink.serif(), Vector2(GX, y0 + 20), "Debt", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Ink.INK)
	draw_string(Ink.pencil(), Vector2(GX + 70, y0 + 22), "%ds" % state.debt, HORIZONTAL_ALIGNMENT_LEFT, -1, 26, Ink.RED)
	draw_string(Ink.serif(), Vector2(GX + 180, y0 + 20), "Silver", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Ink.INK)
	draw_string(Ink.pencil(), Vector2(GX + 250, y0 + 22), "%ds" % state.silver, HORIZONTAL_ALIGNMENT_LEFT, -1, 26, Ink.PENCIL)
	var note := "Next season's interest: +%ds." % state.next_interest() if state.season < 3 else "No more interest this year."
	draw_string(Ink.sans(), Vector2(GX, y0 + 50), note, HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Ink.FAINT)
	draw_string(Ink.sans(), Vector2(GX, y0 + 68), "Pay the reeve in Dorestad only.", HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Ink.FAINT)

	# Ship
	draw_string(Ink.serif(), Vector2(GX, 362), "Ship", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Ink.INK)
	_boxes(Vector2(GX, 372), D.START["hull"], state.hull, "Hull")
	_boxes(Vector2(GX, 402), D.START["crew_max"], state.crew, "Crew")
	_boxes(Vector2(GX, 432), D.START["morale"], state.morale, "Morale")
	draw_string(Ink.sans(), Vector2(GX, 556), "Upkeep 1s/day at sea, +1s per voyage per crew above 6.", HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Ink.FAINT)
	draw_string(Ink.sans(), Vector2(GX, 572), "Crew under 4: voyages +1 day. Morale resets in port.", HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Ink.FAINT)

	# Hold
	var hx := RIGHT_X
	var hw := size.x - 14 - hx
	draw_string(Ink.serif(), Vector2(hx, y0 + 20), "Hold", HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Ink.INK)
	draw_string(Ink.sans(), Vector2(hx + 50, y0 + 20), "%d / %d lots" % [state.hold.size(), D.START["hold"]], HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Ink.FAINT)
	var hy := y0 + 32
	for i in D.START["hold"]:
		var r := Rect2(hx, hy + i * 30, hw, 30)
		draw_rect(r, Ink.INK, false, 1.0)
		draw_string(Ink.sans(), r.position + Vector2(6, 20), str(i + 1), HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Ink.FAINT)
		if i < state.hold.size():
			var lot: Dictionary = state.hold[i]
			var col := Ink.RED if lot["good"] == D.SWORDS else Ink.PENCIL
			draw_string(Ink.pencil(), r.position + Vector2(26, 21), D.GOODS[lot["good"]], HORIZONTAL_ALIGNMENT_LEFT, -1, 15, col)
			draw_string(Ink.pencil(), r.position + Vector2(110, 21), "paid %d · %s" % [lot["paid"], lot["from"]], HORIZONTAL_ALIGNMENT_LEFT, hw - 116, 12, Ink.PENCIL)

	# The reeve's terms
	var ty := hy + 6 * 30 + 16
	draw_rect(Rect2(hx, ty, hw, size.y - ty - 14), Ink.INK, false, 1.5)
	var terms := "Due: in Dorestad with the debt cleared by the end of day %d.\n\nReeve's grace: in Dorestad on day 60 but short? Pay at least half; the rest ×1.5 is due by day 75.\n\nOtherwise the reeve takes your ship. You lose." % D.START["due"]
	if state.graced:
		terms = "GRACE GRANTED.\n\nClear the rest in Dorestad by the end of day %d, or the reeve takes your ship." % D.START["grace_due"]
	draw_wrapped(Ink.sans(), Vector2(hx + 8, ty + 6), terms, hw - 16, 12, Ink.INK)
