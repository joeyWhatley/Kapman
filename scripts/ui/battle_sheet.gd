extends "res://scripts/ui/paper.gd"
## The battle sheet: you vs. Viking raiders. Pick a move; the raiders roll on their table.

signal move_chosen(move_id: String)

const D := preload("res://scripts/data.gd")
const RAIDER_TABLE := [
	"1  Jeer and circle: nothing.",
	"2  Arrow storm: your Crew −1.",
	"3  Close in (already close: as 5).",
	"4  Ram: CLOSE. Your Hull −2, theirs −1.",
	"5  Spears: at CLOSE your Crew −2; at FAR close in.",
	"6  Board! At FAR: close in + war cry, Morale −1.",
]

var state
var battle := {}          # empty = no battle
var lines: Array = []
var _buttons := {}


func _ready() -> void:
	title = "Battle"
	subtitle = "one enemy type: Viking raiders"
	for i in D.MOVES.size():
		var mv: Array = D.MOVES[i]
		var b := Button.new()
		b.text = "%s\n%s" % [mv[1], mv[3]]
		b.position = Vector2(14 + (i % 2) * 347, 172 + (i / 2) * 50)
		b.clip_text = true
		b.custom_minimum_size = Vector2(333, 44)
		b.size = Vector2(333, 44)
		b.add_theme_font_size_override("font_size", 12)
		b.alignment = HORIZONTAL_ALIGNMENT_LEFT
		var id: String = mv[0]
		b.pressed.connect(func(): move_chosen.emit(id))
		add_child(b)
		_buttons[id] = b


func valid_moves() -> Array:
	var out: Array = []
	if battle.is_empty():
		return out
	for mv in D.MOVES:
		if mv[2] and not battle["close"]:
			continue
		if mv[0] == "tribute" and state.silver < D.COSTS["tribute"] and state.hold.is_empty():
			continue
		out.append(mv[0])
	return out


func add_line(text: String) -> void:
	lines.append(text)
	if lines.size() > 60:
		lines.pop_front()
	queue_redraw()


func refresh() -> void:
	var ok := valid_moves()
	for id in _buttons:
		_buttons[id].visible = not battle.is_empty()
		_buttons[id].disabled = not id in ok or battle.get("waiting", false) == false
	queue_redraw()


func _stat_row(y: float, label: String, vals: Array, maxes: Array) -> void:
	draw_string(Ink.serif(), Vector2(14, y + 18), label, HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Ink.INK)
	for i in 3:
		var x := 170.0 + i * 180.0
		for k in maxes[i]:
			var r := Rect2(x + k * 15, y + 4, 12, 16)
			draw_rect(r, Ink.INK, false, 1.2)
			if k < vals[i]:
				draw_rect(r.grow(-2.5), Ink.PENCIL)
		draw_string(Ink.pencil(), Vector2(x + maxes[i] * 15 + 4, y + 20), str(vals[i]), HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Ink.RED)


func _draw() -> void:
	draw_paper()
	if state == null:
		return
	for i in 3:
		draw_string(Ink.bold(), Vector2(170 + i * 180, 70), ["Hull", "Crew", "Morale"][i], HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Ink.INK)
	if battle.is_empty():
		draw_string(Ink.serif(), Vector2(14, 110), "No raiders in sight.", HORIZONTAL_ALIGNMENT_LEFT, -1, 20, Ink.FAINT)
		var y := 150.0
		draw_string(Ink.bold(), Vector2(14, y), "Your moves", HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Ink.INK)
		for mv in D.MOVES:
			y += 20
			draw_string(Ink.sans(), Vector2(14, y), "%s — %s" % [mv[1], mv[3]], HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Ink.INK)
		y += 34
		draw_string(Ink.sans(), Vector2(14, y), "Raiders start: Hull %d · Crew %d · Morale %d. Battles start at FAR." % [D.RAIDERS["hull"], D.RAIDERS["crew"], D.RAIDERS["morale"]], HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Ink.INK)
		y += 20
		draw_string(Ink.sans(), Vector2(14, y), "Losing Crew in a round also costs 1 Morale. Morale 0 = that side breaks.", HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Ink.INK)
		_draw_table(Vector2(361, 420))
		return
	_stat_row(78, "You", [state.hull, state.crew, state.morale], [D.START["hull"], D.START["crew_max"], D.START["morale"]])
	_stat_row(110, "Viking raiders", [battle["hull"], battle["crew"], battle["morale"]], [D.RAIDERS["hull"], D.RAIDERS["crew"], D.RAIDERS["morale"]])
	var rng_txt := "Range:  " + ("☐ FAR   ☒ CLOSE" if battle["close"] else "☒ FAR   ☐ CLOSE")
	draw_string(Ink.bold(), Vector2(14, 160), rng_txt, HORIZONTAL_ALIGNMENT_LEFT, -1, 15, Ink.INK)
	if battle["shield"]:
		draw_string(Ink.pencil(), Vector2(260, 160), "shield wall up", HORIZONTAL_ALIGNMENT_LEFT, -1, 15, Ink.RED)
	# Log
	draw_line(Vector2(14, 380), Vector2(size.x - 14, 380), Ink.INK, 1.5)
	draw_string(Ink.bold(), Vector2(14, 400), "Battle log", HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Ink.INK)
	var shown := lines.slice(maxi(0, lines.size() - 9))
	for i in shown.size():
		draw_string(Ink.pencil(), Vector2(14, 422 + i * 19), shown[i], HORIZONTAL_ALIGNMENT_LEFT, 335, 12, Ink.PENCIL)
	_draw_table(Vector2(361, 400))


func _draw_table(pos: Vector2) -> void:
	draw_string(Ink.bold(), pos, "Raider table (d6, every round)", HORIZONTAL_ALIGNMENT_LEFT, -1, 13, Ink.INK)
	for i in RAIDER_TABLE.size():
		draw_string(Ink.sans(), pos + Vector2(0, 22 + i * 19), RAIDER_TABLE[i], HORIZONTAL_ALIGNMENT_LEFT, size.x - pos.x - 14, 12, Ink.INK)
