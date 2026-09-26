extends SceneTree
## One-off repair: the projection migration also moved the container nodes (Ports, Hazards, …) away
## from the map origin. This puts every container back at (0, 0) while keeping each child where it
## is on the Earth. Nodes added after the migration (listed below) were placed assuming the origin,
## so they keep their local position instead.

const SCENE := "res://route_map/route_map.tscn"
const ADDED_AFTER := ["Miklagard", "Cherson", "Bolghar", "Atil", "Parisius", "Rotomagus", "Colonia", "Hammaburg", "Bosporus", "Dardanelles"]


func _initialize() -> void:
	var root: Node = (load(SCENE) as PackedScene).instantiate()
	var map: Node = root.get_node("Map")
	var report: Array = []
	for path in ["Ports", "Hazards", "Corrections", "Currents", "Decor", "Decor/RegionNames", "Decor/Marginalia"]:
		var c = map.get_node_or_null(path)
		if c == null or not c is Node2D:
			continue
		var off: Vector2 = (c as Node2D).position
		if off == Vector2.ZERO:
			continue
		c.position = Vector2.ZERO
		for child in c.get_children():
			if child is Node2D and not String(child.name) in ADDED_AFTER:
				child.position += off
		report.append("%s (%s)" % [path, off])
	var out := PackedScene.new()
	out.pack(root)
	ResourceSaver.save(out, SCENE)
	root.free()
	print("containers reset: ", report)
	quit()
