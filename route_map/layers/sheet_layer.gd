@tool
extends Node2D
## A filled rectangle over the whole map: the paper itself, or the fog wash (with the fog shader).

@export var rect := Rect2()
@export var color := Color.WHITE
@export var margin := 0.0
@export var outside := Color.TRANSPARENT


func _draw() -> void:
	if outside.a > 0.0:
		draw_rect(rect.grow(4000), outside)
	draw_rect(rect.grow(margin), color)
