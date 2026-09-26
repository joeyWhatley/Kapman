@tool
class_name KapmanLandPatch
extends Polygon2D
## A period-geography correction drawn over the modern coastline.
## CUT_WATER opens sea where there is land today (the Wantsum Channel); ADD_LAND fills in land
## where there is sea today. Both change where ships can sail and how the coast is drawn.

enum Mode { CUT_WATER, ADD_LAND }

@export var mode := Mode.CUT_WATER:
	set(v):
		mode = v
		_tint()
@export_multiline var note := ""

## Set by the route map at runtime.
var zoom := 1.0
var paper := Color(0.953, 0.945, 0.918)
var ink := Color(0.1, 0.11, 0.13)
var skip_edges: Array = []


func _ready() -> void:
	_tint()


func _tint() -> void:
	if Engine.is_editor_hint():
		color = Color(0.2, 0.5, 0.9, 0.45) if mode == Mode.CUT_WATER else Color(0.4, 0.7, 0.3, 0.45)
	else:
		color = Color(0, 0, 0, 0)   # drawn by _draw below instead


func world_polygon() -> PackedVector2Array:
	var out := PackedVector2Array()
	for p in polygon:
		out.append(to_global(p))
	return out


func _draw() -> void:
	if Engine.is_editor_hint() or polygon.size() < 3:
		return
	var w := 1.15 / zoom
	if mode == Mode.CUT_WATER:
		draw_colored_polygon(polygon, paper)
	# Only draw the edges that make new coastline (skip_edges is filled in by the route map).
	for i in polygon.size():
		if i in skip_edges:
			continue
		draw_line(polygon[i], polygon[(i + 1) % polygon.size()], ink, w, true)
