@tool
class_name KapmanRegionLabel
extends Node2D
## A widely spaced region name in the chart's hand, e.g. MARE FRESICUM. Rotate the node to follow
## a coast. Fades out when you zoom in close (it's a chart-scale label).

@export var text := "MARE FRESICUM":
	set(v):
		text = v
		queue_redraw()
@export var size := 60.0
@export var spacing := 0.55          # extra space between letters, as a share of the size
@export var ink := Color(0.15, 0.11, 0.08, 0.34)
@export var fade_from_zoom := 1.1    # starts fading here
@export var sea := true              # sea names are set a little lighter

var zoom := 1.0:
	set(v):
		zoom = v
		var a := _alpha()
		if absf(a - _drawn_alpha) > 0.02:
			queue_redraw()
var _drawn_alpha := -1.0


func _ready() -> void:
	add_to_group("zoom_aware")


func _alpha() -> float:
	return 1.0 - smoothstep(fade_from_zoom, fade_from_zoom * 1.8, zoom)


func _draw() -> void:
	_drawn_alpha = _alpha()
	if _drawn_alpha <= 0.0:
		return
	var font: Font = load("res://route_map/fonts/Inter-%s.ttf" % ("Regular" if sea else "Medium"))
	var fs := int(size)
	var adv := 0.0
	var widths: Array = []
	for ch in text:
		var w := font.get_string_size(ch, HORIZONTAL_ALIGNMENT_LEFT, -1, fs).x + size * spacing
		widths.append(w)
		adv += w
	var x := -adv / 2.0
	var col := Color(ink, ink.a * _drawn_alpha)
	for i in text.length():
		draw_string(font, Vector2(x, size * 0.35), text[i], HORIZONTAL_ALIGNMENT_LEFT, -1, fs, col)
		x += widths[i]
