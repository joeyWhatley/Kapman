@tool
class_name KapmanPort
extends Node2D
## A port on the route map. Drag it in the editor to move it; edit its market in the Inspector.
## Its child Line2D "Approach" is the river/inlet path: first point = sea anchorage, last point = the port.

enum Swords { BUY, SELL, NONE }

@export var display_name := "Port"
@export_range(1, 3) var tier := 2
## F Frankish (contraband checks), E English, D Danish, N Norse, S Slavic, B Baltic, R Rus,
## G Rhomaioi (Byzantine), K Khazar & Volga Bulgar
@export_enum("F", "E", "D", "N", "S", "B", "R", "G", "K") var faction := "D"
@export var tag := ""
@export var approach_note := ""
## Base price per lot: Grain, Cloth, Wine, Furs, Amber, Swords (-1 = not traded)
@export var prices := PackedInt32Array([8, 12, 14, 15, 20, 24])
@export var swords := Swords.SELL
@export var home_port := false


func _ready() -> void:
	if not Engine.is_editor_hint():
		visible = false


func approach_points() -> PackedVector2Array:
	var out := PackedVector2Array()
	var line := get_node_or_null("Approach") as Line2D
	if line == null or line.points.size() < 2:
		out.append(global_position + Vector2(-10, 0))
		out.append(global_position)
		return out
	for p in line.points:
		out.append(line.to_global(p))
	return out


func swords_mode() -> String:
	return ["buy", "sell", "none"][swords]


func _draw() -> void:
	if not Engine.is_editor_hint():
		return
	var r: float = [0.0, 9.0, 7.0, 5.0][tier]
	draw_circle(Vector2.ZERO, r, Color(0.1, 0.11, 0.13))
	draw_arc(Vector2.ZERO, r + 3.0, 0, TAU, 32, Color(0.7, 0.2, 0.12), 1.5)
	draw_string(ThemeDB.fallback_font, Vector2(r + 6, 5), display_name, HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Color(0.1, 0.11, 0.13))


func _process(_d: float) -> void:
	if Engine.is_editor_hint():
		queue_redraw()
