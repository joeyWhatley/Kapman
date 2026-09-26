extends RefCounted
## Hazards of the sea, read from the KapmanHazard polygons in the scene. Hidden under the fog until
## you learn of them. knowledge: 0 = unknown, 1 = rumoured (vague warning), 2 = charted (named, drawn).

# Chance per day spent inside a zone that something happens, by kind (x severity).
const DAILY := {"shoals": 0.18, "storm": 0.16, "fog": 0.3, "raiders": 0.14, "cursed": 0.22}

# Prevailing wind by season: direction the wind blows TOWARD (map coords: +x east, +y south), strength.
const WINDS := [
	{"dir": Vector2(-0.95, 0.3), "strength": 0.18, "from": "the east"},
	{"dir": Vector2(0.9, -0.2), "strength": 0.1, "from": "the west"},
	{"dir": Vector2(0.7, -0.7), "strength": 0.25, "from": "the south-west"},
	{"dir": Vector2(0.6, 0.8), "strength": 0.3, "from": "the north-west"},
]

var zones: Array = []      # {"id","kind","name","sev","seasons","rumor","known_text","poly","center","bbox"}
var knowledge := {}        # id -> 0/1/2


func build(container: Node) -> void:
	zones.clear()
	knowledge.clear()
	for n in container.get_children():
		if not n is KapmanHazard:
			continue
		var poly: PackedVector2Array = n.world_polygon()
		if poly.size() < 3:
			continue
		var c := Vector2.ZERO
		var box := Rect2(poly[0], Vector2.ZERO)
		for p in poly:
			c += p
			box = box.expand(p)
		zones.append({
			"id": String(n.name), "kind": n.kind_name(), "name": n.display_name, "sev": n.severity,
			"seasons": n.season_list(), "rumor": n.rumor, "known_text": n.known_text,
			"poly": poly, "center": c / poly.size(), "bbox": box,
		})
		knowledge[String(n.name)] = n.known_at_start


func contains(z: Dictionary, p: Vector2) -> bool:
	return z["bbox"].has_point(p) and Geometry2D.is_point_in_polygon(p, z["poly"])


func active(z: Dictionary, season: int) -> bool:
	return season in z["seasons"]


func zone(id: String) -> Dictionary:
	for z in zones:
		if z["id"] == id:
			return z
	return {}


## Raise knowledge from what the fog shows: charted over the centre = known, charted over an edge = rumoured.
func learn_from_fog(fog) -> Array:
	var newly: Array = []
	for z in zones:
		var id: String = z["id"]
		var before: int = knowledge[id]
		if before >= 2:
			continue
		var level := 0
		if contains(z, z["center"]) and fog.charted(z["center"]) > 0.6:
			level = 2
		else:
			var poly: PackedVector2Array = z["poly"]
			var step := maxi(1, poly.size() / 12)
			for i in range(0, poly.size(), step):
				var inner: Vector2 = poly[i].lerp(z["center"], 0.2)
				if fog.charted(inner) > 0.5:
					level = 1
					break
		if level > before:
			knowledge[id] = level
			newly.append(id)
	return newly


## Warning text for a zone, depending on how much you know.
func hint(z: Dictionary, _season: int) -> String:
	var k: int = knowledge[z["id"]]
	if k >= 2:
		var when := "" if z["seasons"].size() == 4 else " (%s)" % ", ".join(z["seasons"].map(func(s): return ["spring", "summer", "autumn", "winter"][s]))
		return z["known_text"] + when
	return z["rumor"]


func wind(season: int) -> Dictionary:
	return WINDS[season]
