extends RefCounted
## Plant meshes, built from code in the same painterly style as the cog: a broadleaf tree
## (oak/alder, tinted per variant), a thorn bush, a grass tuft, reeds. Vertex colours are sRGB.
## Tree crowns use normals pointing out from the crown's centre, so they shade as soft masses
## instead of showing every facet.

static func tree(detail: int) -> ArrayMesh:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var rng := RandomNumberGenerator.new()
	rng.seed = 11
	# One broadleaf tree for every species: the scatter shader stretches it into a narrow alder
	# or a slender birch (and whitens a birch's bark). A broad, domed crown of clustered masses on
	# a short stout trunk, the way an oak grows in the open. detail 0 is the distant version:
	# the same silhouette from fewer, plainer masses.
	var bark := Color(0.3, 0.25, 0.19)
	_trunk(st, Vector3.ZERO, Vector3(0.15, 6.5, -0.05), 0.45, 0.28, bark, 6 if detail > 0 else 4)
	if detail > 0:
		_trunk(st, Vector3(0.1, 5.2, 0.0), Vector3(2.4, 8.0, 0.8), 0.2, 0.1, bark, 5)
		_trunk(st, Vector3(0.0, 5.6, 0.0), Vector3(-2.0, 8.4, -1.0), 0.18, 0.09, bark, 5)
		_trunk(st, Vector3(0.0, 6.0, 0.0), Vector3(0.4, 8.6, -2.3), 0.16, 0.08, bark, 4)
	var centre := Vector3(0, 9.4, 0)
	var blobs := [[Vector3(0, 10.6, 0), 3.4], [Vector3(2.7, 9.0, 0.9), 2.5], [Vector3(-2.6, 9.2, -1.0), 2.6],
		[Vector3(0.7, 8.8, -2.6), 2.4], [Vector3(-0.8, 8.6, 2.6), 2.4], [Vector3(1.8, 10.9, -1.4), 2.2],
		[Vector3(-1.7, 11.1, 1.2), 2.2], [Vector3(0.2, 12.4, 0.3), 2.0], [Vector3(2.0, 8.2, 2.3), 1.8]]
	if detail == 0:
		blobs = [[Vector3(0, 10.2, 0), 4.3], [Vector3(2.3, 9.0, 0.6), 2.9], [Vector3(-2.2, 9.1, -0.7), 3.0],
			[Vector3(0.3, 9.0, 2.2), 2.8], [Vector3(-0.2, 9.1, -2.3), 2.8]]
	for i in blobs.size():
		var b: Array = blobs[i]
		var shade := Color(0.25, 0.33, 0.17).lerp(Color(0.33, 0.4, 0.2), rng.randf())
		_blob(st, b[0], b[1], shade, centre, detail, rng)
	st.index()
	return st.commit()


## Alder: a taller, narrower tree of the wet ground, crown stacked into a blunt cone.
static func alder(detail: int) -> ArrayMesh:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var rng := RandomNumberGenerator.new()
	rng.seed = 23
	_trunk(st, Vector3.ZERO, Vector3(0.1, 12.5, 0.05), 0.3, 0.12, Color(0.3, 0.27, 0.24), 6)
	var centre := Vector3(0, 9.5, 0)
	var tiers := [[Vector3(0, 6.8, 0), 2.6], [Vector3(0.4, 8.9, -0.3), 2.3], [Vector3(-0.3, 10.9, 0.3), 1.9], [Vector3(0.1, 12.7, 0), 1.4]]
	var n := tiers.size() if detail > 0 else 2
	for i in n:
		var tr: Array = tiers[i]
		_blob(st, tr[0], tr[1], Color(0.24, 0.38, 0.19).lerp(Color(0.3, 0.44, 0.22), rng.randf()), centre, detail, rng)
	st.index()
	return st.commit()


## Birch: slender white trunk, light open crown; on the sandy heath and at wood edges.
static func birch(detail: int) -> ArrayMesh:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var rng := RandomNumberGenerator.new()
	rng.seed = 31
	_trunk(st, Vector3.ZERO, Vector3(0.2, 10.5, 0.1), 0.2, 0.08, Color(0.86, 0.84, 0.78), 5)
	_trunk(st, Vector3(0, 0.0, 0), Vector3(0.05, 1.2, 0.02), 0.22, 0.2, Color(0.3, 0.28, 0.25), 5)
	var centre := Vector3(0, 8.5, 0)
	var blobs := [[Vector3(0.2, 9.8, 0), 1.9], [Vector3(-0.7, 8.2, 0.5), 1.6], [Vector3(0.9, 7.6, -0.4), 1.5], [Vector3(0.1, 6.6, 0.6), 1.3]]
	var n := blobs.size() if detail > 0 else 2
	for i in n:
		var b: Array = blobs[i]
		_blob(st, b[0], b[1], Color(0.46, 0.6, 0.27).lerp(Color(0.54, 0.66, 0.3), rng.randf()), centre, detail, rng)
	st.index()
	return st.commit()


static func bush() -> ArrayMesh:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var rng := RandomNumberGenerator.new()
	rng.seed = 5
	var centre := Vector3(0, 0.9, 0)
	_blob(st, Vector3(0, 1.0, 0), 1.3, Color(0.27, 0.34, 0.18), centre, 1, rng)
	_blob(st, Vector3(0.9, 0.7, 0.4), 0.95, Color(0.31, 0.37, 0.2), centre, 1, rng)
	_blob(st, Vector3(-0.7, 0.75, -0.5), 1.0, Color(0.25, 0.32, 0.17), centre, 1, rng)
	st.index()
	return st.commit()


## A tuft of blades, each a narrow curved strip; normals lean up so tufts light like a lawn.
static func grass(blades: int, height: float, lean: float) -> ArrayMesh:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var rng := RandomNumberGenerator.new()
	rng.seed = 3
	for b in blades:
		var a := rng.randf() * TAU
		var dir := Vector3(cos(a), 0, sin(a))
		var side := Vector3(-dir.z, 0, dir.x)
		var h := height * rng.randf_range(0.6, 1.1)
		var w := 0.035 * rng.randf_range(0.8, 1.3)
		var base := dir * rng.randf() * 0.12
		var pts: Array = []
		for s in 4:
			var t := s / 3.0
			pts.append(base + dir * lean * h * t * t + Vector3(0, h * t, 0))
		for s in 3:
			var t0 := s / 3.0
			var t1 := (s + 1) / 3.0
			var w0 := w * (1.0 - t0)
			var w1 := w * (1.0 - t1) + 0.002
			var c0 := Color(0.28, 0.37, 0.18).lerp(Color(0.56, 0.6, 0.36), t0)
			var c1 := Color(0.28, 0.37, 0.18).lerp(Color(0.56, 0.6, 0.36), t1)
			var q := [pts[s] - side * w0, pts[s] + side * w0, pts[s + 1] + side * w1, pts[s + 1] - side * w1]
			var cs := [c0, c0, c1, c1]
			var nrm := (Vector3.UP * 2.0 - dir).normalized()
			for k in [0, 1, 2, 0, 2, 3]:
				st.set_color(cs[k])
				st.set_normal(nrm)
				st.add_vertex(q[k])
	return st.commit()


static func reeds() -> ArrayMesh:
	var st := SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var rng := RandomNumberGenerator.new()
	rng.seed = 9
	for b in 11:
		var p := Vector3(rng.randf_range(-0.35, 0.35), 0, rng.randf_range(-0.35, 0.35))
		var h := rng.randf_range(1.5, 2.3)
		var lean := Vector3(rng.randf_range(-0.15, 0.15), 0, rng.randf_range(-0.15, 0.15))
		_trunk(st, p, p + lean + Vector3(0, h, 0), 0.018, 0.01, Color(0.55, 0.58, 0.33), 3)
		if rng.randf() < 0.6:
			_trunk(st, p + lean * 0.92 + Vector3(0, h * 0.92, 0), p + lean * 1.02 + Vector3(0, h * 1.05, 0), 0.05, 0.03, Color(0.45, 0.33, 0.22), 4)
	return st.commit()


static func _trunk(st: SurfaceTool, a: Vector3, b: Vector3, r0: float, r1: float, col: Color, sides: int) -> void:
	var axis := (b - a).normalized()
	var u := axis.cross(Vector3.RIGHT if absf(axis.x) < 0.9 else Vector3.FORWARD).normalized()
	var v := axis.cross(u)
	for i in sides:
		var t0 := TAU * i / sides
		var t1 := TAU * (i + 1) / sides
		var o0 := u * cos(t0) + v * sin(t0)
		var o1 := u * cos(t1) + v * sin(t1)
		var q := [a + o0 * r0, a + o1 * r0, b + o1 * r1, b + o0 * r1]
		var ns := [o0, o1, o1, o0]
		for k in [0, 2, 1, 0, 3, 2]:
			st.set_color(col)
			st.set_normal(ns[k])
			st.add_vertex(q[k])


## A lumpy sphere (subdivided icosahedron) for a mass of leaves.
static func _blob(st: SurfaceTool, c: Vector3, r: float, col: Color, crown_centre: Vector3, detail: int, rng: RandomNumberGenerator) -> void:
	var t := (1.0 + sqrt(5.0)) / 2.0
	var verts: Array = [Vector3(-1, t, 0), Vector3(1, t, 0), Vector3(-1, -t, 0), Vector3(1, -t, 0),
		Vector3(0, -1, t), Vector3(0, 1, t), Vector3(0, -1, -t), Vector3(0, 1, -t),
		Vector3(t, 0, -1), Vector3(t, 0, 1), Vector3(-t, 0, -1), Vector3(-t, 0, 1)]
	var faces: Array = [[0, 11, 5], [0, 5, 1], [0, 1, 7], [0, 7, 10], [0, 10, 11], [1, 5, 9], [5, 11, 4], [11, 10, 2],
		[10, 7, 6], [7, 1, 8], [3, 9, 4], [3, 4, 2], [3, 2, 6], [3, 6, 8], [3, 8, 9], [4, 9, 5], [2, 4, 11],
		[6, 2, 10], [8, 6, 7], [9, 8, 1]]
	for i in verts.size():
		verts[i] = (verts[i] as Vector3).normalized()
	for s in detail:
		var nf: Array = []
		var mids := {}
		for f in faces:
			var m := []
			for e in 3:
				var a: int = f[e]
				var b: int = f[(e + 1) % 3]
				var key := mini(a, b) * 10000 + maxi(a, b)
				if not mids.has(key):
					mids[key] = verts.size()
					verts.append(((verts[a] as Vector3) + (verts[b] as Vector3)).normalized())
				m.append(mids[key])
			nf.append([f[0], m[0], m[2]])
			nf.append([f[1], m[1], m[0]])
			nf.append([f[2], m[2], m[1]])
			nf.append([m[0], m[1], m[2]])
		faces = nf
	var lump: Array = []
	for i in verts.size():
		lump.append(1.0 + rng.randf_range(-0.14, 0.14))
	for f in faces:
		for k in [0, 2, 1]:
			var dirv: Vector3 = verts[f[k]]
			var pos: Vector3 = c + dirv * r * lump[f[k]]
			var light := clampf(0.8 + dirv.y * 0.25, 0.6, 1.1)       # darker underneath
			st.set_color(Color(col.r * light, col.g * light, col.b * light))
			st.set_normal((pos - crown_centre).normalized().lerp(dirv, 0.35).normalized())
			st.add_vertex(pos)
