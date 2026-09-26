@tool
class_name KapmanCartouche
extends Node2D
## The chart's title cartouche with a scale bar. Move it in the editor; edit the text in the Inspector.

@export var title := "KĀPMAN"
@export var subtitle := "Mare Frisicum et Oceanus Occidentalis"
@export var dedication := "anno Domini DCCXCIII · Dorestadi descripta"
@export var width := 520.0
@export var scale_km := 200.0
@export var ink := Color(0.15, 0.11, 0.08)
@export var fill := Color(0.93, 0.89, 0.8)
@export var accent := Color(0.66, 0.2, 0.1)

const KM_PER_UNIT := 1.112


func _font(weight: String) -> Font:
	return load("res://route_map/fonts/Inter-%s.ttf" % weight)


func _draw() -> void:
	var h := width * 0.46
	var box := Rect2(-width / 2.0, -h / 2.0, width, h)
	# Scroll ends
	for side in [-1.0, 1.0]:
		var c := Vector2(side * width / 2.0, 0)
		draw_circle(c + Vector2(side * 10, -h * 0.35), 16.0, fill)
		draw_arc(c + Vector2(side * 10, -h * 0.35), 16.0, 0, TAU, 32, ink, 2.0)
		draw_arc(c + Vector2(side * 10, -h * 0.35), 8.0, 0, TAU, 24, ink, 1.4)
		draw_circle(c + Vector2(side * 10, h * 0.35), 16.0, fill)
		draw_arc(c + Vector2(side * 10, h * 0.35), 16.0, 0, TAU, 32, ink, 2.0)
		draw_arc(c + Vector2(side * 10, h * 0.35), 8.0, 0, TAU, 24, ink, 1.4)
	draw_rect(box, fill)
	draw_rect(box, ink, false, 3.0)
	draw_rect(box.grow(-9.0), ink, false, 1.0)
	# Title
	var f := _font("SemiBold")
	draw_string(f, Vector2(-width / 2.0, -h * 0.12), title, HORIZONTAL_ALIGNMENT_CENTER, width, int(h * 0.3), ink)
	draw_line(Vector2(-width * 0.3, -h * 0.02), Vector2(width * 0.3, -h * 0.02), accent, 2.5)
	draw_circle(Vector2(0, -h * 0.02), 5.0, accent)
	draw_string(_font("Medium"), Vector2(-width / 2.0, h * 0.14), subtitle.to_upper(), HORIZONTAL_ALIGNMENT_CENTER, width, int(h * 0.075), ink)
	draw_string(_font("Regular"), Vector2(-width / 2.0, h * 0.25), dedication, HORIZONTAL_ALIGNMENT_CENTER, width, int(h * 0.06), Color(ink, 0.75))
	# Scale bar: alternating ink and paper blocks
	var bar_len := scale_km / KM_PER_UNIT
	var y := h * 0.34
	var x0 := -bar_len / 2.0
	for i in 4:
		var seg := Rect2(x0 + i * bar_len / 4.0, y, bar_len / 4.0, 7.0)
		draw_rect(seg, ink if i % 2 == 0 else fill)
		draw_rect(seg, ink, false, 1.2)
	draw_string(_font("Regular"), Vector2(x0 - 60, y + 8), "0", HORIZONTAL_ALIGNMENT_RIGHT, 55, 13, ink)
	draw_string(_font("Regular"), Vector2(x0 + bar_len + 6, y + 8), "%d km" % int(scale_km), HORIZONTAL_ALIGNMENT_LEFT, 120, 13, ink)
