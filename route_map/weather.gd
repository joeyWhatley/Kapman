extends RefCounted
## Simulated weather. Low-pressure systems form out in the Atlantic, drift east across the map as
## game days pass, strengthen and die. Wind anywhere = the season's prevailing wind + the swirl of
## any system nearby (anticlockwise, as in the northern hemisphere). Storms bring rough sea.
## Advances only with game time (it is still while you're in port).

const Hazards := preload("res://route_map/hazards.gd")

# Chance per day that a new system forms, and how strong it can get, by season.
const SPAWN_PER_DAY := [0.18, 0.12, 0.3, 0.38]
const MAX_STRENGTH := [0.55, 0.4, 0.8, 1.0]
const MAX_SYSTEMS := 4

var systems: Array = []      # {"pos", "vel", "radius", "peak", "age", "life"}
var map_size := Vector2.ZERO
var rng := RandomNumberGenerator.new()


func setup(size: Vector2, seed_value: int) -> void:
	map_size = size
	rng.seed = seed_value
	# Start with one system already on the map so the sea isn't dead calm on day one.
	_spawn(0, rng.randf_range(0.2, 0.5))


func _spawn(season: int, x_frac := 0.0) -> void:
	var peak: float = rng.randf_range(0.35, 1.0) * MAX_STRENGTH[season]
	systems.append({
		"pos": Vector2(map_size.x * x_frac - 200.0, rng.randf_range(0.15, 0.7) * map_size.y),
		"vel": Vector2(rng.randf_range(90.0, 160.0), rng.randf_range(-25.0, 25.0)),   # map units per day
		"radius": rng.randf_range(260.0, 480.0),
		"peak": peak,
		"age": 0.0,
		"life": rng.randf_range(7.0, 14.0),
	})


## Advance the weather by `days` of game time.
func step(days: float, season: int) -> void:
	if days <= 0.0:
		return
	for s in systems:
		s["pos"] += s["vel"] * days
		s["age"] += days
	systems = systems.filter(func(s): return s["age"] < s["life"] and s["pos"].x < map_size.x + 600.0)
	if systems.size() < MAX_SYSTEMS and rng.randf() < SPAWN_PER_DAY[season] * days:
		_spawn(season)


func _strength(s: Dictionary) -> float:
	var k: float = s["age"] / s["life"]
	return s["peak"] * sin(clampf(k, 0.0, 1.0) * PI)


## Wind at a point: direction it blows toward, length = strength (0 .. ~1).
func wind_at(p: Vector2, season: int) -> Vector2:
	var base: Dictionary = Hazards.WINDS[season]
	var w: Vector2 = base["dir"].normalized() * base["strength"]
	for s in systems:
		var r: Vector2 = p - s["pos"]
		var dist := r.length()
		var rad: float = s["radius"]
		if dist > rad * 1.6 or dist < 1.0:
			continue
		var fall := 1.0 - smoothstep(rad * 0.25, rad * 1.6, dist)
		var swirl := Vector2(r.y, -r.x) / dist            # anticlockwise on the map (y down)
		var inflow := -r / dist * 0.35
		w += (swirl + inflow).normalized() * _strength(s) * fall * 0.8
	return w


## 0 (calm) .. 1 (full storm) at a point.
func storm_at(p: Vector2) -> float:
	var out := 0.0
	for s in systems:
		var rad: float = s["radius"]
		var fall := 1.0 - smoothstep(rad * 0.3, rad, p.distance_to(s["pos"]))
		out = maxf(out, _strength(s) * fall)
	return out


## For the shaders: [Vector4(x, y, radius, strength)] per system.
func shader_storms() -> Array:
	var out: Array = []
	for s in systems:
		out.append(Vector4(s["pos"].x, s["pos"].y, s["radius"], _strength(s)))
	return out
