@tool
class_name KapmanBorder
extends Node2D
## An interlace border around the whole chart: double rules with a woven band between, and knots
## at the corners. Drawn at map scale, like a printed frame. Resize with `rect`.

@export var rect := Rect2(0, 0, 4946, 3139):
	set(v):
		rect = v
		queue_redraw()
@export var band := 34.0
@export var ink := Color(0.15, 0.11, 0.08)
@export var fill := Color(0.9, 0.85, 0.74)
@export var accent := Color(0.66, 0.2, 0.1)


func _draw() -> void:
	var outer := rect.grow(band + 10.0)
	var inner := rect.grow(4.0)
	# Fill only the band around the chart (the chart itself stays visible underneath).
	draw_rect(Rect2(outer.position, Vector2(outer.size.x, rect.position.y - outer.position.y)), fill)
	draw_rect(Rect2(Vector2(outer.position.x, rect.end.y), Vector2(outer.size.x, outer.end.y - rect.end.y)), fill)
	draw_rect(Rect2(Vector2(outer.position.x, rect.position.y), Vector2(rect.position.x - outer.position.x, rect.size.y)), fill)
	draw_rect(Rect2(Vector2(rect.end.x, rect.position.y), Vector2(outer.end.x - rect.end.x, rect.size.y)), fill)
	draw_rect(outer, ink, false, 5.0)
	draw_rect(outer.grow(-5.0), ink, false, 1.2)
	draw_rect(inner, ink, false, 1.2)
	draw_rect(inner.grow(3.0), ink, false, 3.0)
	# The woven band along each edge
	var mid := rect.grow(band * 0.5 + 7.0)
	_weave(mid.position, Vector2(mid.end.x, mid.position.y))
	_weave(Vector2(mid.end.x, mid.position.y), mid.end)
	_weave(mid.end, Vector2(mid.position.x, mid.end.y))
	_weave(Vector2(mid.position.x, mid.end.y), mid.position)
	for c in [mid.position, Vector2(mid.end.x, mid.position.y), mid.end, Vector2(mid.position.x, mid.end.y)]:
		_knot(c)


## Two ribbons twisting around each other: an over-under interlace, as on Frisian and Norse metalwork.
func _weave(a: Vector2, b: Vector2) -> void:
	var l := a.distance_to(b)
	var dir := (b - a) / l
	var nrm := dir.orthogonal()
	var period := band * 1.6
	var n := int(l / period)
	var step := l / maxf(1, n)
	var amp := band * 0.3
	for strand in 2:
		var pts := PackedVector2Array()
		var samples := n * 16
		for i in samples + 1:
			var t := float(i) / samples
			var phase := t * n * TAU + strand * PI
			pts.append(a + dir * (t * l) + nrm * sin(phase) * amp)
		draw_polyline(pts, ink, 5.2)
		draw_polyline(pts, fill, 3.0)
	# Over-crossings: redraw one strand's short piece on top at every other crossing
	for k in n * 2:
		var t0 := (k + 0.5) / (n * 2.0)
		if k % 2 == 0:
			var piece := PackedVector2Array()
			for i in 7:
				var t := t0 + (i - 3) * 0.012 / maxf(1.0, n / 10.0)
				piece.append(a + dir * (t * l) + nrm * sin(t * n * TAU) * amp)
			draw_polyline(piece, ink, 5.2)
			draw_polyline(piece, fill, 3.0)
	draw_line(a + nrm * band * 0.5, b + nrm * band * 0.5, Color(ink, 0.5), 0.8)
	draw_line(a - nrm * band * 0.5, b - nrm * band * 0.5, Color(ink, 0.5), 0.8)


func _knot(c: Vector2) -> void:
	var r := band * 0.62
	draw_circle(c, r + 3.0, fill)
	for k in 3:
		var a := TAU * k / 3.0
		draw_arc(c + Vector2.from_angle(a) * r * 0.35, r * 0.62, 0, TAU, 40, ink, 4.0)
		draw_arc(c + Vector2.from_angle(a) * r * 0.35, r * 0.62, 0, TAU, 40, fill, 2.0)
	draw_arc(c, r + 2.0, 0, TAU, 48, ink, 2.0)
	draw_circle(c, 3.5, accent)
