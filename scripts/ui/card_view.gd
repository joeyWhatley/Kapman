extends "res://scripts/ui/paper.gd"
## A small card on the desk: the current season or the active event.

var kind := ""
var heading := ""
var sub := ""
var flavor := ""
var lines: Array = []


func show_card(k: String, h: String, s: String, f: String, l: Array) -> void:
	kind = k
	heading = h
	sub = s
	flavor = f
	lines = l
	queue_redraw()


func _draw() -> void:
	var r := Rect2(Vector2.ZERO, size)
	draw_rect(Rect2(Vector2(4, 5), size), Ink.SHADOW)
	draw_rect(r, Ink.PAPER)
	draw_rect(r.grow(-5), Ink.INK, false, 1.0)
	var w := size.x - 22
	draw_string(Ink.sans(), Vector2(11, 22), kind.to_upper(), HORIZONTAL_ALIGNMENT_LEFT, -1, 9, Ink.FAINT)
	var y := draw_wrapped(Ink.serif(), Vector2(11, 26), heading, w, 16, Ink.INK)
	if sub != "":
		y = draw_wrapped(Ink.sans(), Vector2(11, y), sub, w, 10, Ink.FAINT)
	if flavor != "":
		y = draw_wrapped(Ink.italic(), Vector2(11, y + 2), flavor, w, 10, Ink.INK)
	y += 3
	for line in lines:
		draw_string(Ink.bold(), Vector2(11, y + 11), "•", HORIZONTAL_ALIGNMENT_LEFT, -1, 11, Ink.INK)
		y = draw_wrapped(Ink.sans(), Vector2(21, y), line, w - 10, 11, Ink.INK) + 1
