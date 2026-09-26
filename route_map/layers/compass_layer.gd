@tool
extends Node2D
## A portolan-style compass rose with faint rhumb lines running across the sea.
## Move the node in the editor to place the rose; the lines radiate from it.

@export var radius := 70.0
@export var rhumb_length := 6000.0
@export var ink := Color(0.1, 0.11, 0.13)
@export var paper := Color(0.953, 0.945, 0.918)
@export var accent := Color(0.7, 0.2, 0.12)

var _band := 9999
var zoom := 1.0:
	set(v):
		zoom = v
		var b := int(round(log(maxf(v, 0.01)) / log(1.25)))
		if b != _band:
			_band = b
			queue_redraw()


func _draw() -> void:
	var w := 1.0 / maxf(zoom, 0.05)
	# 32 rhumb lines; the eight winds a little darker than the half- and quarter-winds.
	for i in 32:
		var a := TAU * i / 32.0 - PI / 2.0
		var strength := 0.075 if i % 4 == 0 else (0.05 if i % 2 == 0 else 0.03)
		draw_line(Vector2.ZERO, Vector2(cos(a), sin(a)) * rhumb_length, Color(ink, strength), w)
	# Rings
	draw_circle(Vector2.ZERO, radius * 1.08, Color(paper, 0.92))
	draw_arc(Vector2.ZERO, radius * 1.08, 0, TAU, 96, Color(ink, 0.8), 1.2 * w, true)
	draw_arc(Vector2.ZERO, radius * 1.0, 0, TAU, 96, Color(ink, 0.5), 0.8 * w, true)
	for i in 64:
		var a := TAU * i / 64.0
		var r0 := radius * (0.97 if i % 4 else 0.93)
		draw_line(Vector2(cos(a), sin(a)) * r0, Vector2(cos(a), sin(a)) * radius, Color(ink, 0.6), 0.8 * w)
	# Star: 8 short points, 4 intercardinals, 4 cardinals on top. Each point is split into a light
	# and a dark half, the classic engraved look.
	_star(8, radius * 0.42, radius * 0.07, PI / 8.0, w)
	_star(4, radius * 0.66, radius * 0.1, PI / 4.0, w)
	_star(4, radius * 0.95, radius * 0.13, 0.0, w)
	draw_circle(Vector2.ZERO, radius * 0.05, accent)
	# North marker
	var n := Vector2(0, -radius * 1.2)
	draw_colored_polygon(PackedVector2Array([n + Vector2(0, -radius * 0.14), n + Vector2(-radius * 0.07, radius * 0.04), n + Vector2(radius * 0.07, radius * 0.04)]), accent)


func _star(points: int, length: float, half_width: float, offset: float, w: float) -> void:
	for i in points:
		var a := TAU * i / points - PI / 2.0 + offset
		var tip := Vector2(cos(a), sin(a)) * length
		var side := Vector2(cos(a + PI / 2.0), sin(a + PI / 2.0)) * half_width
		draw_colored_polygon(PackedVector2Array([Vector2.ZERO, side, tip]), paper)
		draw_colored_polygon(PackedVector2Array([Vector2.ZERO, tip, -side]), ink)
		draw_polyline(PackedVector2Array([side, tip, -side]), ink, 0.8 * w, true)
