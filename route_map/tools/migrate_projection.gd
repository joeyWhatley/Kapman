extends SceneTree
## Moves every placed node in route_map.tscn from the OLD map projection (North Sea–Baltic chart,
## 26°W–44°E, 43.5°N–71.5°N) to the current one in geo.gd, through real latitude/longitude, so your
## edited ports, hazards, currents, corrections and decorations stay where they belong on the Earth.
## Run once after changing geo.gd:
##   godot --headless --path <project> --script res://route_map/tools/migrate_projection.gd

const Geo := preload("res://route_map/geo.gd")
const SCENE := "res://route_map/route_map.tscn"


## The previous projection, frozen here (forward and inverse).
class OldGeo:
	const LON_MIN := -26.0
	const LON_MAX := 44.0
	const LAT_MIN := 43.5
	const LAT_MAX := 71.5
	const LON0 := 12.0
	const LAT1 := 50.0
	const LAT2 := 66.0
	const LAT0 := 57.5
	const K := 100.0
	var n := 0.0
	var f := 0.0
	var rho0 := 0.0
	var mn := Vector2(INF, INF)
	var mx := Vector2(-INF, -INF)

	func _init() -> void:
		var p1 := deg_to_rad(LAT1)
		var p2 := deg_to_rad(LAT2)
		n = log(cos(p1) / cos(p2)) / log(tan(PI / 4 + p2 / 2) / tan(PI / 4 + p1 / 2))
		f = cos(p1) * pow(tan(PI / 4 + p1 / 2), n) / n
		rho0 = f / pow(tan(PI / 4 + deg_to_rad(LAT0) / 2), n)
		var lat := LAT_MIN
		while lat <= LAT_MAX + 0.001:
			for lon in [LON_MIN, LON_MAX]:
				_grow(raw(lat, lon))
			lat += 0.5
		var lon2 := LON_MIN
		while lon2 <= LON_MAX + 0.001:
			for lat2 in [LAT_MIN, LAT_MAX]:
				_grow(raw(lat2, lon2))
			lon2 += 0.5

	func _grow(p: Vector2) -> void:
		mn = mn.min(p)
		mx = mx.max(p)

	func raw(lat: float, lon: float) -> Vector2:
		var r := K * 180.0 / PI
		var rho := f / pow(tan(PI / 4 + deg_to_rad(lat) / 2), n)
		var theta := n * deg_to_rad(lon - LON0)
		return Vector2(rho * sin(theta), rho0 - rho * cos(theta)) * r

	## Map position -> [lat, lon]
	func inverse(p: Vector2) -> Vector2:
		var r := K * 180.0 / PI
		var x := (p.x + mn.x) / r
		var y := rho0 - (mx.y - p.y) / r
		var rho := signf(n) * sqrt(x * x + y * y)
		var theta := atan2(x, y)
		var lon := LON0 + rad_to_deg(theta / n)
		var lat := rad_to_deg(2.0 * atan(pow(f / rho, 1.0 / n)) - PI / 2.0)
		return Vector2(lat, lon)


var old := OldGeo.new()


func to_new(p: Vector2) -> Vector2:
	var ll := old.inverse(p)
	return Geo.project(ll.x, ll.y)


func _initialize() -> void:
	# Self-check: the inverse must round-trip.
	var probe := old.raw(52.0, 5.3)
	var back := old.inverse(Vector2(probe.x - old.mn.x, old.mx.y - probe.y))
	print("round-trip Dorestad: ", back, " (expect 52.0, 5.3)")
	var root: Node = (load(SCENE) as PackedScene).instantiate()
	var map: Node = root.get_node("Map")
	# Pass 1: record old world positions of every node and every vertex.
	var jobs: Array = []
	for node in _all(map):
		if not node is Node2D or node.get_parent() == null:
			continue
		var job := {"node": node, "pos": (node as Node2D).global_position}
		if node is Line2D:
			var pts: Array = []
			for q in node.points:
				pts.append(node.to_global(q))
			job["line"] = pts
		elif node is Polygon2D:
			var pts2: Array = []
			for q in node.polygon:
				pts2.append(node.to_global(q))
			job["poly"] = pts2
		jobs.append(job)
	# Pass 2: set new positions (parents first: _all is depth-first pre-order), then vertices.
	var moved := 0
	for job in jobs:
		var node: Node2D = job["node"]
		if node.name in ["Paper", "FogWash", "Sea", "CoastRough", "CoastDetail", "Lakes", "Rivers", "HazardMarks", "Ambient", "CloudShadows", "PortGlyphs", "Route", "Border"]:
			continue
		node.global_position = to_new(job["pos"])
		moved += 1
	for job in jobs:
		var node: Node2D = job["node"]
		if job.has("line"):
			var np := PackedVector2Array()
			for q in job["line"]:
				np.append(node.to_local(to_new(q)))
			node.points = np
		if job.has("poly"):
			var pp := PackedVector2Array()
			for q in job["poly"]:
				pp.append(node.to_local(to_new(q)))
			node.polygon = pp
	var border = map.get_node_or_null("Border")
	if border:
		border.rect = Rect2(Vector2.ZERO, Geo.size())
	var paper = map.get_node_or_null("Paper")
	if paper:
		paper.rect = Rect2(Vector2.ZERO, Geo.size())
		if paper.material:
			paper.material.set_shader_parameter("chart", Vector4(0, 0, Geo.size().x, Geo.size().y))
	var wash = map.get_node_or_null("FogWash")
	if wash:
		wash.rect = Rect2(Vector2.ZERO, Geo.size())
	var out := PackedScene.new()
	out.pack(root)
	ResourceSaver.save(out, SCENE)
	root.free()
	print("migrated %d nodes to the new projection; map is now %s" % [moved, Geo.size()])
	quit()


func _all(n: Node) -> Array:
	var out: Array = []
	for c in n.get_children():
		out.append(c)
		out.append_array(_all(c))
	return out
