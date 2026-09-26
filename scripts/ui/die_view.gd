extends Control
## The d6 on the desk. Tumbles briefly, then shows the result and what it was for.

const Ink := preload("res://scripts/ui/ink.gd")
const PIPS := {
	1: [Vector2(0, 0)],
	2: [Vector2(-1, -1), Vector2(1, 1)],
	3: [Vector2(-1, -1), Vector2(0, 0), Vector2(1, 1)],
	4: [Vector2(-1, -1), Vector2(1, -1), Vector2(-1, 1), Vector2(1, 1)],
	5: [Vector2(-1, -1), Vector2(1, -1), Vector2(0, 0), Vector2(-1, 1), Vector2(1, 1)],
	6: [Vector2(-1, -1), Vector2(1, -1), Vector2(-1, 0), Vector2(1, 0), Vector2(-1, 1), Vector2(1, 1)],
}
const FACE := 76.0

var face := 6
var label := "the die"
var tilt := 0.0


func animate(result: int, what: String) -> void:
	label = what
	for i in 7:
		face = randi_range(1, 6)
		tilt = randf_range(-0.35, 0.35)
		queue_redraw()
		await get_tree().create_timer(0.045).timeout
	face = result
	tilt = randf_range(-0.08, 0.08)
	queue_redraw()
	await get_tree().create_timer(0.3).timeout


func show_face(result: int, what: String) -> void:
	face = result
	label = what
	queue_redraw()


func _draw() -> void:
	var c := Vector2(size.x / 2.0, FACE / 2.0 + 6)
	draw_set_transform(c, tilt, Vector2.ONE)
	var r := Rect2(Vector2(-FACE / 2.0, -FACE / 2.0), Vector2(FACE, FACE))
	draw_rect(Rect2(r.position + Vector2(4, 5), r.size), Ink.SHADOW)
	draw_rect(r, Color(0.98, 0.97, 0.93))
	draw_rect(r, Ink.INK, false, 2.5)
	for p in PIPS[face]:
		draw_circle(p * 21.0, 6.5, Ink.RED if face == 1 else Ink.INK)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)
	var font := Ink.pencil()
	draw_multiline_string(font, Vector2(0, FACE + 30), label, HORIZONTAL_ALIGNMENT_CENTER, size.x, 12, 4, Color(0.93, 0.9, 0.82))
