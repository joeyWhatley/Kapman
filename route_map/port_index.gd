extends RefCounted
## All ports on the map, read from the KapmanPort nodes in the scene.

var ports := {}       # name -> {"node", "pos", "approach", "anchorage", "tier", "fac", "tag", "note"}
var home := ""


func build(container: Node) -> void:
	ports.clear()
	for n in container.get_children():
		if not n is KapmanPort:
			continue
		var a: PackedVector2Array = n.approach_points()
		ports[n.display_name] = {
			"node": n, "pos": a[a.size() - 1], "approach": a, "anchorage": a[0],
			"tier": n.tier, "fac": n.faction, "tag": n.tag, "note": n.approach_note,
		}
		if n.home_port and home == "":
			home = n.display_name
	if home == "" and not ports.is_empty():
		home = ports.keys()[0]


func names() -> Array:
	return ports.keys()


func approach(p: String) -> PackedVector2Array:
	return ports[p]["approach"]


func position(p: String) -> Vector2:
	return ports[p]["pos"]


func anchorage(p: String) -> Vector2:
	return ports[p]["anchorage"]


## Market data in the shape trade_state.gd expects.
func trade_data() -> Dictionary:
	var out := {}
	for p in ports:
		var n: KapmanPort = ports[p]["node"]
		var pr: Array = []
		for v in n.prices:
			pr.append(v)
		while pr.size() < 6:
			pr.append(-1)
		out[p] = {"fac": n.faction, "prices": pr, "swords": n.swords_mode(), "tag": n.tag}
	return out
