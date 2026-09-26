@tool
extends RefCounted
## Map projection for the route map: Lambert conformal conic, standard parallels 45°N and 65°N,
## centred on 5°E. Keeps shapes true from Greenland to the Volga. Map units are "pixels at zoom 1";
## 100 units ≈ one degree of latitude (111 km).
## The map covers 50°W–52°E, 39.5°N–71.5°N: Greenland to the Volga, Lofoten to Constantinople.

const LON_MIN := -50.0
const LON_MAX := 52.0
const LAT_MIN := 39.5
const LAT_MAX := 71.5
const LON0 := 5.0
const LAT1 := 45.0
const LAT2 := 65.0
const LAT0 := 56.0
const K := 100.0                   # map units per degree of latitude
const CELL := 5.0                  # land/sea grid cell size, map units
const KM_PER_UNIT := 111.2 / K

static var _n := 0.0
static var _f := 0.0
static var _rho0 := 0.0
static var _min := Vector2.ZERO
static var _max := Vector2.ZERO


static func _static_init() -> void:
	var p1 := deg_to_rad(LAT1)
	var p2 := deg_to_rad(LAT2)
	_n = log(cos(p1) / cos(p2)) / log(tan(PI / 4 + p2 / 2) / tan(PI / 4 + p1 / 2))
	_f = cos(p1) * pow(tan(PI / 4 + p1 / 2), _n) / _n
	_rho0 = _f / pow(tan(PI / 4 + deg_to_rad(LAT0) / 2), _n)
	_min = Vector2(INF, INF)
	_max = Vector2(-INF, -INF)
	var lat := LAT_MIN
	while lat <= LAT_MAX + 0.001:
		for lon in [LON_MIN, LON_MAX]:
			_grow(_raw(lat, lon))
		lat += 0.5
	var lon2 := LON_MIN
	while lon2 <= LON_MAX + 0.001:
		for lat2 in [LAT_MIN, LAT_MAX]:
			_grow(_raw(lat2, lon2))
		lon2 += 0.5


static func _grow(p: Vector2) -> void:
	_min = _min.min(p)
	_max = _max.max(p)


## Projected coordinates before flipping into map space (y up).
static func _raw(lat: float, lon: float) -> Vector2:
	var r := K * 180.0 / PI
	var rho := _f / pow(tan(PI / 4 + deg_to_rad(lat) / 2), _n)
	var theta := _n * deg_to_rad(lon - LON0)
	return Vector2(rho * sin(theta), _rho0 - rho * cos(theta)) * r


static func project(lat: float, lon: float) -> Vector2:
	var p := _raw(lat, lon)
	return Vector2(p.x - _min.x, _max.y - p.y)


static func size() -> Vector2:
	return _max - _min


## Map position -> Vector2(lat, lon).
static func unproject(p: Vector2) -> Vector2:
	var r := K * 180.0 / PI
	var x := (p.x + _min.x) / r
	var y := _rho0 - (_max.y - p.y) / r
	var rho := signf(_n) * sqrt(x * x + y * y)
	var theta := atan2(x, y)
	var lon := LON0 + rad_to_deg(theta / _n)
	var lat := rad_to_deg(2.0 * atan(pow(_f / rho, 1.0 / _n)) - PI / 2.0)
	return Vector2(lat, lon)
