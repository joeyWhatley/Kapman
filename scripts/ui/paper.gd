extends Control
## A sheet of paper lying on the desk. Subclasses call draw_paper() at the top of _draw().

const Ink := preload("res://scripts/ui/ink.gd")

var title := ""
var subtitle := ""


func draw_paper() -> void:
	var r := Rect2(Vector2.ZERO, size)
	draw_rect(Rect2(r.position + Vector2(5, 7), r.size), Ink.SHADOW)
	draw_rect(r, Ink.PAPER)
	draw_rect(r, Ink.EDGE, false, 1.0)
	if title != "":
		draw_string(Ink.serif(), Vector2(16, 34), title, HORIZONTAL_ALIGNMENT_LEFT, -1, 24, Ink.INK)
		if subtitle != "":
			draw_string(Ink.sans(), Vector2(16, 32), subtitle, HORIZONTAL_ALIGNMENT_RIGHT, size.x - 32, 12, Ink.FAINT)
		draw_line(Vector2(14, 44), Vector2(size.x - 14, 44), Ink.INK, 2.0)


## Word-wrapped text; returns the y just below it.
func draw_wrapped(font: Font, pos: Vector2, text: String, width: float, font_size: int, color: Color) -> float:
	draw_multiline_string(font, pos + Vector2(0, font.get_ascent(font_size)), text, HORIZONTAL_ALIGNMENT_LEFT, width, font_size, -1, color)
	var h := font.get_multiline_string_size(text, HORIZONTAL_ALIGNMENT_LEFT, width, font_size).y
	return pos.y + h


func _draw() -> void:
	draw_paper()
