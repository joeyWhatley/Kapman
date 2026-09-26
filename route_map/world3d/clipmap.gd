@tool
extends Node3D
## Nested square grids around the camera (a geometry clipmap): level 0 is a full grid of `cells`
## x `cells` squares of `base` metres; each next level doubles the square size and has a hole
## where the finer level sits. Every level snaps to twice its own square size, so vertices never
## swim as the camera moves. The shader displaces the flat grid (terrain height or waves) and
## morphs each level's outer rim onto the coarser grid so there are no cracks.
##
## The hole is a little smaller than the finer level, so neighbours overlap by a thin ring;
## coarser levels are nudged down (`level_bias`) so the finer one always wins there.

## Size of one square in the finest ring (metres).
@export var base := 2.0
## Squares across each ring.
@export var cells := 64
## Number of rings; each doubles the square size (13 rings of 2 m reach ~260 km).
@export var levels := 14
## The shader material (terrain_material.tres / sea_material.tres). The world copies it at start
## and fills in the map data, so edit its look here; the data textures are added at run time.
@export var material: ShaderMaterial
@export var cast_shadows := true

var _levels: Array[MeshInstance3D] = []


func _ready() -> void:
	_build()


func _build() -> void:
	for mi in _levels:
		mi.queue_free()
	_levels.clear()
	for l in levels:
		var mi := MeshInstance3D.new()
		mi.mesh = build_grid(cells, 1.0, l > 0)
		mi.scale = Vector3.ONE * base * pow(2.0, l)
		mi.material_override = material
		mi.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_ON if cast_shadows else GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		mi.extra_cull_margin = 2000.0      # vertices move in the shader; never cull a level by its flat box
		mi.set_instance_shader_parameter("cell", base * pow(2.0, l))
		mi.set_instance_shader_parameter("half_extent", base * pow(2.0, l) * cells * 0.5)
		mi.set_instance_shader_parameter("level_bias", 0.0 if l == 0 else -0.004 * base * pow(2.0, l))
		add_child(mi)
		_levels.append(mi)


## Draw with this material instead of `material` (a copy carrying the map data).
func use_material(m: ShaderMaterial) -> void:
	for mi in _levels:
		mi.material_override = m


## Follow a point on the ground (local metres).
func follow(p: Vector3) -> void:
	for l in _levels.size():
		var snap := base * pow(2.0, l) * 2.0
		_levels[l].position = Vector3(snappedf(p.x, snap), 0.0, snappedf(p.z, snap))


## A grid of n x n unit squares centred on the origin; with a hole, the middle (n/2 - 2) squares
## each way are left out (the finer level covers them, with an overlap).
static func build_grid(n: int, s: float, hole: bool) -> ArrayMesh:
	var half := n / 2
	var h0 := -(half / 2 - 2)
	var h1 := half / 2 - 2
	var verts := PackedVector3Array()
	var idx := PackedInt32Array()
	for j in n + 1:
		for i in n + 1:
			verts.append(Vector3((i - half) * s, 0.0, (j - half) * s))
	for j in n:
		for i in n:
			var ci := i - half
			var cj := j - half
			if hole and ci >= h0 and ci < h1 and cj >= h0 and cj < h1:
				continue
			var a := j * (n + 1) + i
			var b := a + 1
			var c := a + n + 1
			var d := c + 1
			# alternate the diagonal so the grid reads less regular
			if (i + j) % 2 == 0:
				idx.append_array([a, b, d, a, d, c])
			else:
				idx.append_array([a, b, c, b, d, c])
	var arr := []
	arr.resize(Mesh.ARRAY_MAX)
	arr[Mesh.ARRAY_VERTEX] = verts
	arr[Mesh.ARRAY_INDEX] = idx
	var mesh := ArrayMesh.new()
	mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arr)
	return mesh
