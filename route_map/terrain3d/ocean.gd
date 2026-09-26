extends MeshInstance3D
## The open sea: one faceted water sheet that follows the view (snapped to its own grid, so the
## facets never swim). Colour comes from the real depth, foam and ripples from the coast distance.

const SIZE := 8000.0            # reaches past the streamed land, into the haze
const FACET := 20.0

var world: Node3D


func _ready() -> void:
	var pm := PlaneMesh.new()
	pm.size = Vector2(SIZE, SIZE)
	pm.subdivide_width = int(SIZE / FACET) - 1
	pm.subdivide_depth = int(SIZE / FACET) - 1
	mesh = pm
	cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	var mat := ShaderMaterial.new()
	mat.shader = load("res://route_map/terrain3d/ocean.gdshader")
	mat.set_shader_parameter("height_tex", world.height_tex)
	mat.set_shader_parameter("map_size", world.map_size)
	mat.set_shader_parameter("dist_tex", load("res://route_map/data/sea_distance.png"))
	mat.set_shader_parameter("dist_size", preload("res://route_map/geo.gd").size())
	material_override = mat


func _process(_delta: float) -> void:
	var f: Vector2 = world.focus
	position = Vector3(snappedf(f.x, FACET), 0.0, snappedf(f.y, FACET))
