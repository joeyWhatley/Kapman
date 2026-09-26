@tool
extends Node2D
## Ink linework at a constant on-screen width. `source` picks what it draws from the coast data;
## it also draws in the editor so you can see the map while you place ports and hazards.
## `sketch` draws a second, slightly offset pass so the fog coast reads as vague and hand-guessed.
##
## Performance: lines are split into tiles (child canvas items) so off-screen parts are culled, and
## tiles only redraw when the zoom crosses a band (every ~25%), not on every zoom step.

@export_enum("detail", "rough", "lakes", "custom") var source := "detail":
	set(v):
		source = v
		if Engine.is_editor_hint() and is_inside_tree():
			_load_for_editor()
@export var color := Color(0.1, 0.11, 0.13):
	set(v):
		color = v
		_redraw_tiles()
@export var width_px := 1.25:
	set(v):
		width_px = v
		_redraw_tiles()
@export var sketch := false

## Below LOD_ZOOM the detail layer draws the generalised coast; below FAR_ZOOM the calmest one.
const LOD_ZOOM := 0.9
const FAR_ZOOM := 0.5
const WATER_ALPHA := [0.34, 0.2, 0.11]
const TILE := 450.0
const MAX_RUN := 160          # long coastlines are cut into pieces so tiles cull well
const BAND := 1.25
const SMALL := 45.0           # lakes smaller than this (map units across) hide at far zoom

var runs: Array = []:
	set(v):
		runs = v
		_rebuild()
var medium: Array = []:       # detail layer only: generalised coast
	set(v):
		medium = v
		_rebuild()
var far: Array = []:          # detail layer only: calmest coast, whole-map view
	set(v):
		far = v
		_rebuild()
var water: Array = []:        # detail layer only: [level][run] water-lines
	set(v):
		water = v
		_rebuild()
var zoom := 1.0:
	set(v):
		zoom = v
		var b := int(round(log(maxf(v, 0.01)) / log(BAND)))
		if b != _band:
			_band = b
			_redraw_tiles()

var _band := 9999
var _pending: Array = []        # off-screen tiles waiting to redraw, a few per frame
const PER_FRAME := 3
var _tiles := {}              # Vector2i -> Tile
var _sets := {}               # set name -> {Vector2i: [runs]}

static var _editor_data = null


class Tile extends Node2D:
	var layer
	var key := Vector2i.ZERO

	func _draw() -> void:
		layer._draw_tile(self, key)


func _ready() -> void:
	if Engine.is_editor_hint() and runs.is_empty() and source != "custom":
		_load_for_editor()


func band_zoom() -> float:
	return pow(BAND, _band) if _band != 9999 else zoom


func _rebuild() -> void:
	_sets.clear()
	_bucket("detail", runs)
	var big: Array = []
	for r in runs:
		if _extent(r) >= SMALL:
			big.append(r)
	_bucket("big", big)
	_bucket("medium", medium)
	_bucket("far", far)
	for i in water.size():
		_bucket("water%d" % i, water[i])
	var keys := {}
	for s in _sets.values():
		for k in s:
			keys[k] = true
	for k in _tiles.keys():
		if not keys.has(k):
			_tiles[k].queue_free()
			_tiles.erase(k)
	for k in keys:
		if not _tiles.has(k):
			var t := Tile.new()
			t.layer = self
			t.key = k
			t.use_parent_material = true
			add_child(t)
			_tiles[k] = t
	_redraw_tiles()


func _bucket(set_name: String, list: Array) -> void:
	var d := {}
	for run in list:
		var r: PackedVector2Array = run
		var i := 0
		while i < r.size() - 1:
			var j := mini(i + MAX_RUN, r.size() - 1)
			var piece := r.slice(i, j + 1)
			var k := Vector2i(floori(piece[0].x / TILE), floori(piece[0].y / TILE))
			if not d.has(k):
				d[k] = []
			d[k].append(piece)
			i = j
	_sets[set_name] = d


func _extent(run: PackedVector2Array) -> float:
	var b := Rect2(run[0], Vector2.ZERO)
	for p in run:
		b = b.expand(p)
	return b.size.length()


## On-screen tiles redraw now; off-screen ones trickle in over the next frames (no stutter).
func _redraw_tiles() -> void:
	var cam: Camera2D = get_viewport().get_camera_2d() if is_inside_tree() and not Engine.is_editor_hint() else null
	if cam == null:
		for t in _tiles.values():
			t.queue_redraw()
		return
	var vs := get_viewport_rect().size / maxf(zoom, 0.01)
	var view := Rect2(cam.get_screen_center_position() - vs / 2.0, vs).grow(TILE * 0.6)
	_pending.clear()
	for k in _tiles:
		if Rect2(Vector2(k) * TILE, Vector2(TILE, TILE)).intersects(view):
			_tiles[k].queue_redraw()
		else:
			_pending.append(_tiles[k])
	set_process(not _pending.is_empty())


func _process(_delta: float) -> void:
	if Engine.is_editor_hint():
		set_process(false)
		return
	for i in PER_FRAME:
		if _pending.is_empty():
			break
		var t = _pending.pop_back()
		if is_instance_valid(t):
			t.queue_redraw()
	if _pending.is_empty():
		set_process(false)


func _draw_tile(tile: Node2D, key: Vector2i) -> void:
	var z := band_zoom()
	var w := width_px / z
	# Water-lining fades in as you come closer; the whole-map view stays clean.
	var fade := clampf((z - 0.45) / 0.55, 0.0, 1.0)
	if fade > 0.0:
		for level in water.size():
			var col := Color(color, color.a * WATER_ALPHA[mini(level, 2)] * fade)
			for run in _sets.get("water%d" % level, {}).get(key, []):
				tile.draw_polyline(run, col, -1.0)   # hairline: hardware line, no triangulation
	var set_name := "detail"
	if z < FAR_ZOOM and not far.is_empty():
		set_name = "far"
	elif z < LOD_ZOOM and not medium.is_empty():
		set_name = "medium"
	elif z < FAR_ZOOM and source == "lakes":
		set_name = "big"
	var lines: Array = _sets.get(set_name, {}).get(key, [])
	for run in lines:
		tile.draw_polyline(run, color, w)
	if sketch:
		var off := Vector2(1.6, 1.1)
		var faint := Color(color, color.a * 0.45)
		for run in lines:
			var moved := PackedVector2Array()
			for p in run:
				moved.append(p + off)
			tile.draw_polyline(moved, faint, w)


func _load_for_editor() -> void:
	if _editor_data == null:
		_editor_data = load("res://route_map/map_data.gd").new()
		_editor_data.load_data("res://route_map/data/coast.json", false)
	match source:
		"detail":
			water = _editor_data.water_runs
			runs = _editor_data.detail_runs
		"rough":
			runs = _editor_data.rough_runs
		"lakes":
			runs = _editor_data.lake_runs
