@tool
class_name KapmanHazard
extends Polygon2D
## A hazard zone. Reshape the polygon in the editor (select the node, then drag its points).
## Hidden while playing; the player only learns of it through the fog, rumours or bad luck.

enum Kind { SHOALS, STORM, FOG, RAIDERS, CURSED }
const KIND_NAMES := ["shoals", "storm", "fog", "raiders", "cursed"]
const KIND_COLORS := [Color(0.85, 0.7, 0.3, 0.35), Color(0.3, 0.4, 0.8, 0.3), Color(0.6, 0.6, 0.6, 0.35), Color(0.8, 0.2, 0.15, 0.3), Color(0.5, 0.2, 0.6, 0.3)]

@export var display_name := "Hazard"
@export var kind := Kind.SHOALS:
	set(v):
		kind = v
		color = KIND_COLORS[v]
@export_range(1, 3) var severity := 2
@export_flags("Spring", "Summer", "Autumn", "Winter") var seasons := 15
## 0 unknown, 1 rumoured, 2 charted — at the start of a game
@export_range(0, 2) var known_at_start := 0
@export_multiline var rumor := ""
@export_multiline var known_text := ""


func _ready() -> void:
	color = KIND_COLORS[kind]
	if not Engine.is_editor_hint():
		visible = false


func kind_name() -> String:
	return KIND_NAMES[kind]


func season_list() -> Array:
	var out: Array = []
	for s in 4:
		if seasons & (1 << s):
			out.append(s)
	return out


func world_polygon() -> PackedVector2Array:
	var out := PackedVector2Array()
	for p in polygon:
		out.append(to_global(p))
	return out
