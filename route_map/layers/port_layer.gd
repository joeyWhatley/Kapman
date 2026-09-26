extends Node2D
## Port glyphs and labels, sized by importance. Ports under fog are drawn faint and vague.

const Style := preload("res://route_map/style.gd")

var index             # port_index.gd
var fog
var zoom := 1.0
var here := ""
var hover := ""
var selected := ""
var destination := ""
var tilt := 0.0      # 2.5D: little houses stand up around the port when the chart tilts


func _draw() -> void:
	if index == null:
		return
	for p in index.names():
		var pos: Vector2 = index.position(p)
		var tier: int = index.ports[p]["tier"]
		var charted: float = fog.charted(pos) if fog else 1.0
		var ink := Style.INK.lerp(Style.ROUGH, 1.0 - charted)
		draw_set_transform(pos, 0.0, Vector2.ONE / zoom)
		var r: float = [0.0, 6.5, 5.0, 3.5][tier]
		if p == hover or p == selected or p == destination:
			draw_circle(Vector2.ZERO, r + 7.0, Color(Style.INK, 0.07))
		if p == here:
			draw_arc(Vector2.ZERO, r + 5.0, 0, TAU, 40, Style.ACCENT, 1.5, true)
		match tier:
			1:
				draw_circle(Vector2.ZERO, r, ink)
				draw_arc(Vector2.ZERO, r + 2.5, 0, TAU, 40, ink, 1.0, true)
			2:
				draw_circle(Vector2.ZERO, r, Style.PAPER)
				draw_arc(Vector2.ZERO, r, 0, TAU, 32, ink, 1.6, true)
			_:
				draw_circle(Vector2.ZERO, r, ink)
		var font := Style.font("SemiBold" if tier == 1 else "Medium" if tier == 2 else "Regular")
		var size: int = [0, 15, 13, 12][tier]
		draw_string(font, Vector2(r + 7, 4), p, HORIZONTAL_ALIGNMENT_LEFT, -1, size, ink)
		if tilt > 0.02 and charted > 0.5:
			_houses(p, tier, ink)
		draw_set_transform(Vector2.ZERO, 0.0, Vector2.ONE)


func _houses(p: String, tier: int, ink: Color) -> void:
	var k := tilt / 0.42
	var n: int = [0, 7, 5, 3][tier]
	var seed := p.hash()
	for i in n:
		var ox := -12.0 - float((seed >> (i * 3)) & 7) * 2.2 - (i % 3) * 7.0
		var oy := 6.0 + float((seed >> (i * 2 + 5)) & 3) * 3.0 + (i / 3) * 7.0
		var w := 5.0 + float((seed >> i) & 1) * 2.0
		var wall := 4.0 * k + 1.0
		var roof := 4.0 * k
		var base := Vector2(ox, oy)
		var front := PackedVector2Array([base, base + Vector2(w, 0), base + Vector2(w, -wall), base + Vector2(0, -wall)])
		draw_colored_polygon(front, Style.PAPER)
		front.append(front[0])
		draw_polyline(front, ink, 0.9, true)
		var gable := PackedVector2Array([base + Vector2(-0.8, -wall), base + Vector2(w / 2.0, -wall - roof), base + Vector2(w + 0.8, -wall)])
		draw_colored_polygon(gable, Color(Style.INK, 0.55 * ink.a))


## Port under a map position, within `radius_px` on screen.
func port_at(p: Vector2, radius_px := 14.0) -> String:
	var best := ""
	var best_d := radius_px / zoom
	for pn in index.names():
		var d := p.distance_to(index.position(pn))
		if d < best_d:
			best_d = d
			best = pn
	return best
