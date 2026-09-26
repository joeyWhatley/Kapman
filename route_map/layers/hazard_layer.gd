extends Node2D
## Charted hazards, drawn faintly so they never compete with the coastline.
## Only zones you know (knowledge 2) appear; rumours show up on your course line instead.

const Style := preload("res://route_map/style.gd")

var hazards
var season := 0
var _band := 9999
var zoom := 1.0:
	set(v):
		zoom = v
		var b := int(round(log(maxf(v, 0.01)) / log(1.25)))
		if b != _band:
			_band = b
			queue_redraw()


func _draw() -> void:
	if hazards == null:
		return
	var w := 1.0 / zoom
	for z in hazards.zones:
		if hazards.knowledge[z["id"]] < 2:
			continue
		var on: bool = hazards.active(z, season)
		var col := Color(Style.INK, 0.28 if on else 0.12)
		var poly: PackedVector2Array = z["poly"]
		# Dotted outline
		for i in poly.size():
			var a := poly[i]
			var b := poly[(i + 1) % poly.size()]
			var n := int(a.distance_to(b) / 6.0) + 1
			for k in n:
				draw_circle(a.lerp(b, float(k) / n), 0.9 * w, col)
		# A light texture that says what kind of trouble it is
		var box: Rect2 = z["bbox"]
		var step := maxf(9.0, (box.size.x + box.size.y) / 18.0)
		var y := box.position.y + step * 0.5
		var row := 0
		while y < box.end.y:
			var x := box.position.x + (step * 0.5 if row % 2 else 0.0)
			while x < box.end.x:
				var p := Vector2(x, y)
				if Geometry2D.is_point_in_polygon(p, poly) and Geometry2D.is_point_in_polygon(p.lerp(z["center"], -0.12), poly):
					_mark(p, z["kind"], col, w)
				x += step
			y += step
			row += 1
		if zoom >= 0.9:
			draw_set_transform(z["center"], 0.0, Vector2.ONE / zoom)
			draw_string(Style.font("Regular"), Vector2(-70, 0), z["name"], HORIZONTAL_ALIGNMENT_CENTER, 140, 11, Color(Style.INK, 0.45 if on else 0.25))
			draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _mark(p: Vector2, kind: String, col: Color, w: float) -> void:
	match kind:
		"shoals":
			draw_circle(p, 0.8 * w, col)
			draw_circle(p + Vector2(2.2, 1.0) * w, 0.6 * w, col)
		"storm":
			draw_arc(p, 2.4 * w, PI * 1.1, PI * 1.9, 6, col, 0.8 * w, true)
			draw_arc(p + Vector2(4.6, 0) * w, 2.4 * w, PI * 0.1, PI * 0.9, 6, col, 0.8 * w, true)
		"fog":
			draw_line(p - Vector2(3.5, 0) * w, p + Vector2(3.5, 0) * w, col, 0.8 * w)
		"raiders":
			draw_line(p - Vector2(2, 2) * w, p + Vector2(2, 2) * w, Color(Style.ACCENT, col.a), 0.9 * w)
		"cursed":
			draw_arc(p, 2.0 * w, 0, TAU, 10, col, 0.7 * w, true)
