@tool
class_name KapmanCurrent
extends Line2D
## An ocean current. Draw the line in the direction the water flows (edit the points in the editor).
## Ships going with it gain speed, against it lose speed. Hidden while playing; shown as faint
## flowing ink where you've charted the sea.

## 0..1: how strong (0.5 ≈ a steady coastal current, 1 = a tidal race)
@export_range(0.0, 1.0) var strength := 0.4
## How far to each side the current is felt, in km
@export var width_km := 40.0
@export_multiline var note := ""

const KM_PER_UNIT := 1.112


func _ready() -> void:
	width = 3.0
	default_color = Color(0.15, 0.45, 0.75, 0.6)
	if not Engine.is_editor_hint():
		visible = false


func world_points() -> PackedVector2Array:
	var out := PackedVector2Array()
	for p in points:
		out.append(to_global(p))
	return out


func reach() -> float:
	return width_km / KM_PER_UNIT
