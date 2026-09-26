extends RefCounted
## All ocean currents, read from the KapmanCurrent lines in the scene. current_at(p) gives the flow
## (direction the water moves, length 0..1) at any map position.

var lines: Array = []   # {"pts": PackedVector2Array, "strength", "reach", "bbox"}


func build(container: Node) -> void:
	lines.clear()
	if container == null:
		return
	for n in container.get_children():
		if not n is KapmanCurrent:
			continue
		var pts: PackedVector2Array = n.world_points()
		if pts.size() < 2:
			continue
		var box := Rect2(pts[0], Vector2.ZERO)
		for p in pts:
			box = box.expand(p)
		lines.append({"pts": pts, "strength": n.strength, "reach": n.reach(), "bbox": box.grow(n.reach())})


func current_at(p: Vector2) -> Vector2:
	var out := Vector2.ZERO
	for c in lines:
		if not c["bbox"].has_point(p):
			continue
		var pts: PackedVector2Array = c["pts"]
		var best := INF
		var dir := Vector2.ZERO
		for i in pts.size() - 1:
			var q := Geometry2D.get_closest_point_to_segment(p, pts[i], pts[i + 1])
			var d := q.distance_to(p)
			if d < best:
				best = d
				dir = (pts[i + 1] - pts[i]).normalized()
		var reach: float = c["reach"]
		if best < reach:
			out += dir * c["strength"] * (1.0 - smoothstep(reach * 0.3, reach, best))
	return out
