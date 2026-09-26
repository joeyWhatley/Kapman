@tool
class_name KapmanMarginalia
extends Node2D
## Ink drawings for the empty ocean: a sea serpent, a whale spouting, a ship under sail. Pure
## decoration at chart scale; they fade out as you zoom in and gently bob.

enum Kind { SERPENT, WHALE, SHIP }

@export var kind := Kind.SERPENT:
	set(v):
		kind = v
		queue_redraw()
@export var size := 1.0
@export var ink := Color(0.15, 0.11, 0.08)
@export var fill := Color(0.93, 0.89, 0.8)
@export var accent := Color(0.66, 0.2, 0.1)

var zoom := 1.0


func _ready() -> void:
	add_to_group("zoom_aware")


func _process(_d: float) -> void:
	queue_redraw()


func _draw() -> void:
	var a := 1.0 - smoothstep(1.0, 1.7, zoom)
	if a <= 0.0 and not Engine.is_editor_hint():
		return
	if Engine.is_editor_hint():
		a = 1.0
	var t := Time.get_ticks_msec() / 1000.0
	draw_set_transform(Vector2(0, sin(t * 0.8) * 3.0), 0.0, Vector2.ONE * size)
	var col := Color(ink, a)
	var fc := Color(fill, a)
	match kind:
		Kind.SERPENT:
			_serpent(col, fc, t)
		Kind.WHALE:
			_whale(col, fc, t)
		Kind.SHIP:
			_ship(col, fc)
	draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _waves(col: Color, w: float) -> void:
	for i in 5:
		var x := -w / 2.0 + i * w / 4.0
		draw_arc(Vector2(x, 8), 12.0, PI * 1.1, PI * 1.9, 10, col, 2.0)


func _serpent(col: Color, fc: Color, t: float) -> void:
	# Three humps breaking the water, a curled tail and a crested head.
	for i in 3:
		var c := Vector2(-60 + i * 42, 0)
		var hump := PackedVector2Array()
		for k in 13:
			var ang := PI + PI * k / 12.0
			hump.append(c + Vector2(cos(ang) * 16, sin(ang) * (16 + sin(t + i) * 1.5)))
		draw_colored_polygon(hump, fc)
		draw_polyline(hump, col, 3.0)
		for k in 3:
			draw_line(c + Vector2(-8 + k * 8, -13), c + Vector2(-5 + k * 8, -20), col, 2.0)
	draw_arc(Vector2(-92, -4), 12.0, PI * 0.2, PI * 1.9, 20, col, 3.0)
	var head := PackedVector2Array([Vector2(62, 0), Vector2(66, -34), Vector2(90, -40), Vector2(98, -30), Vector2(80, -26), Vector2(76, 0)])
	draw_colored_polygon(head, fc)
	head.append(head[0])
	draw_polyline(head, col, 3.0)
	draw_circle(Vector2(86, -34), 2.5, col)
	draw_line(Vector2(66, -34), Vector2(58, -48), col, 2.0)
	draw_line(Vector2(72, -38), Vector2(68, -52), col, 2.0)
	draw_line(Vector2(98, -30), Vector2(108, -26), Color(accent, col.a), 2.0)
	_waves(col, 220)


func _whale(col: Color, fc: Color, t: float) -> void:
	var body := PackedVector2Array()
	for k in 25:
		var ang := PI + PI * k / 24.0
		body.append(Vector2(cos(ang) * 70, sin(ang) * 26))
	body.append(Vector2(70, 0))
	draw_colored_polygon(body, fc)
	draw_polyline(body, col, 3.0)
	draw_circle(Vector2(-44, -8), 2.5, col)
	# Tail flukes and a spout
	draw_polyline(PackedVector2Array([Vector2(70, 0), Vector2(92, -18), Vector2(84, 0), Vector2(98, 8)]), col, 3.0)
	for k in 5:
		var ang := -PI / 2.0 + (k - 2) * 0.35
		var h := 30.0 + sin(t * 3.0 + k) * 4.0
		draw_line(Vector2(-30, -26), Vector2(-30, -26) + Vector2.from_angle(ang) * h, col, 1.6)
	_waves(col, 200)


func _ship(col: Color, fc: Color) -> void:
	var hull := PackedVector2Array([Vector2(-70, -6), Vector2(-50, 12), Vector2(50, 12), Vector2(72, -8)])
	draw_polyline(hull, col, 3.0)
	draw_line(Vector2(-70, -6), Vector2(72, -8), col, 2.0)
	draw_line(Vector2(0, -6), Vector2(0, -92), col, 3.0)
	var sail := PackedVector2Array([Vector2(-36, -86), Vector2(38, -86), Vector2(33, -24), Vector2(-31, -24)])
	draw_colored_polygon(sail, fc)
	for k in 4:
		var x := -30.0 + k * 18.0
		draw_line(Vector2(x + 2, -86), Vector2(x + 1, -24), Color(accent, col.a), 5.0)
	sail.append(sail[0])
	draw_polyline(sail, col, 2.4)
	for k in 7:
		draw_arc(Vector2(-45 + k * 15, -2), 5.0, 0, TAU, 12, col, 1.6)
	_waves(col, 180)
