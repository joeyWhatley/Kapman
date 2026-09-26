extends SceneTree
## Gives sea_view.tscn the cel look (all other edits kept):
##  - hull_material.tres becomes a cel-shaded vertex-colour material (toon_vcol.gdshader);
##  - Camera/Outlines: the ink-outline pass (outline_material.tres, outline.gdshader).
##   godot --headless --path <project> --script res://route_map/tools/apply_cel_look.gd

const DIR := "res://route_map/world3d/"


func _initialize() -> void:
	var hull := ShaderMaterial.new()
	hull.shader = load(DIR + "toon_vcol.gdshader")
	ResourceSaver.save(hull, DIR + "hull_material.tres")
	var outline := ShaderMaterial.new()
	outline.shader = load(DIR + "outline.gdshader")
	outline.render_priority = 100
	ResourceSaver.save(outline, DIR + "outline_material.tres")
	outline = ResourceLoader.load(DIR + "outline_material.tres", "", ResourceLoader.CACHE_MODE_REPLACE)
	ResourceLoader.load(DIR + "hull_material.tres", "", ResourceLoader.CACHE_MODE_REPLACE)
	var root: Node = (load(DIR + "sea_view.tscn") as PackedScene).instantiate()
	var cam: Node = root.get_node("Camera")
	if cam.get_node_or_null("Outlines") == null:
		var q := MeshInstance3D.new()
		q.name = "Outlines"
		var qm := QuadMesh.new()
		qm.size = Vector2(2, 2)
		q.mesh = qm
		q.material_override = outline
		q.extra_cull_margin = 16384.0
		q.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		q.position = Vector3(0, 0, -1)
		cam.add_child(q)
		q.owner = root
		print("added Camera/Outlines")
	var ps := PackedScene.new()
	ps.pack(root)
	ResourceSaver.save(ps, DIR + "sea_view.tscn")
	root.free()
	print("saved")
	quit()
