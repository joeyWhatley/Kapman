extends RefCounted
## Streams the baked terrain tiles (G:/kapman_data/tiles, made by G:/kapman_data/baker/Baker.exe)
## around the camera. For each of the 9 levels (30 m, 60 m ... 7.7 km per pixel) a window of 4 x 4
## tiles (1024 px) centred on the camera sits in one layer of a Texture2DArray; when the camera
## crosses a tile, that level's window is rebuilt on a worker thread and swapped in.
## Pixels: R height (m), G water surface (m), B distance to the shore (m). Tiles that were not baked
## (fine levels over open sea) are filled with a marker the shaders skip, falling back to a coarser level.

const LEVELS := 9
const TILE := 256
const SPAN_TILES := 4
const WIN := TILE * SPAN_TILES
const M := 1112.0
const MISSING := -60000.0

var dir := "G:/kapman_data/tiles"
var tex: Texture2DArray
var images: Array[Image] = []
var win_origin: Array[Vector2i] = []      # window's top-left tile, per level
var ready_levels := 0

var _cache := {}                           # "L/z/x" -> Image
var _cache_order: Array[String] = []
var _cache_mutex := Mutex.new()
var _missing: Image
var _pending := {}                         # level -> task id
var _results := {}                         # level -> [Image, Vector2i]
var _mutex := Mutex.new()


static func px(level: int) -> float:
	return 30.0 * float(1 << level)


static func span(level: int) -> float:
	return px(level) * TILE


func _init(tiles_dir: String = "") -> void:
	if tiles_dir != "":
		dir = tiles_dir
	_missing = Image.create_empty(TILE, TILE, false, Image.FORMAT_RGBH)
	_missing.fill(Color(MISSING, 0.0, 2550.0))
	for l in LEVELS:
		images.append(null)
		win_origin.append(Vector2i(-99999, -99999))


func available() -> bool:
	return DirAccess.dir_exists_absolute(dir + "/L8")


## Build every level's window around a point (absolute map metres), blocking. Call once at start.
func prime(ax: float, az: float) -> void:
	var imgs: Array[Image] = []
	for l in LEVELS:
		var o := _want(l, ax, az)
		images[l] = _compose(l, o)
		win_origin[l] = o
		imgs.append(images[l])
	if tex == null:
		tex = Texture2DArray.new()
		tex.create_from_images(imgs)
	else:
		for l in LEVELS:
			tex.update_layer(imgs[l], l)
	ready_levels = LEVELS


## Keep the windows centred on the camera; swaps in finished rebuilds. Call every frame.
func update(ax: float, az: float) -> void:
	_mutex.lock()
	var done := _results.duplicate()
	_results.clear()
	_mutex.unlock()
	for l in done:
		WorkerThreadPool.wait_for_task_completion(_pending[l])
		_pending.erase(l)
		images[l] = done[l][0]
		win_origin[l] = done[l][1]
		tex.update_layer(images[l], l)
	for l in LEVELS:
		var o := _want(l, ax, az)
		if o != win_origin[l] and not _pending.has(l):
			_pending[l] = WorkerThreadPool.add_task(_rebuild.bind(l, o))


## Shader uniform: per level (window top-left in local metres, window size, 1).
func rects(origin_x: float, origin_z: float) -> PackedVector4Array:
	var out := PackedVector4Array()
	for l in LEVELS:
		var s := span(l)
		out.append(Vector4(win_origin[l].x * s - origin_x, win_origin[l].y * s - origin_z, s * SPAN_TILES, 1.0))
	return out


## Height, water level and shore distance at absolute metres, from the finest window that has data.
func sample(ax: float, az: float) -> Vector3:
	for l in LEVELS:
		var img := images[l]
		if img == null:
			continue
		var p := px(l)
		var fx := ax / p - win_origin[l].x * TILE - 0.5
		var fz := az / p - win_origin[l].y * TILE - 0.5
		if fx < 0.0 or fz < 0.0 or fx >= WIN - 1 or fz >= WIN - 1:
			continue
		var c := img.get_pixel(int(fx), int(fz))
		if c.r < MISSING * 0.5:
			continue
		# bilinear
		var ix := int(fx)
		var iz := int(fz)
		var tx := fx - ix
		var tz := fz - iz
		var c10 := img.get_pixel(ix + 1, iz)
		var c01 := img.get_pixel(ix, iz + 1)
		var c11 := img.get_pixel(ix + 1, iz + 1)
		if c10.r < MISSING * 0.5 or c01.r < MISSING * 0.5 or c11.r < MISSING * 0.5:
			return Vector3(c.r, c.g, c.b)
		var top := Vector3(c.r, c.g, c.b).lerp(Vector3(c10.r, c10.g, c10.b), tx)
		var bot := Vector3(c01.r, c01.g, c01.b).lerp(Vector3(c11.r, c11.g, c11.b), tx)
		return top.lerp(bot, tz)
	return Vector3(0.0, 0.0, 2550.0)


func _want(l: int, ax: float, az: float) -> Vector2i:
	var s := span(l)
	return Vector2i(int(round(ax / s)) - SPAN_TILES / 2, int(round(az / s)) - SPAN_TILES / 2)


func _rebuild(l: int, o: Vector2i) -> void:
	var img := _compose(l, o)
	_mutex.lock()
	_results[l] = [img, o]
	_mutex.unlock()


func _compose(l: int, o: Vector2i) -> Image:
	var img := Image.create_empty(WIN, WIN, false, Image.FORMAT_RGBH)
	for j in SPAN_TILES:
		for i in SPAN_TILES:
			var t := _tile(l, o.x + i, o.y + j)
			img.blit_rect(t, Rect2i(0, 0, TILE, TILE), Vector2i(i * TILE, j * TILE))
	return img


func _tile(l: int, x: int, z: int) -> Image:
	var key := "%d/%d/%d" % [l, z, x]
	_cache_mutex.lock()
	var hit: Image = _cache.get(key)
	_cache_mutex.unlock()
	if hit:
		return hit
	var img := _missing
	var path := "%s/L%d/%d/%d.bin" % [dir, l, z, x]
	if x >= 0 and z >= 0 and FileAccess.file_exists(path):
		var raw := FileAccess.get_file_as_bytes(path).decompress(TILE * TILE * 6, FileAccess.COMPRESSION_DEFLATE)
		if raw.size() == TILE * TILE * 6:
			img = Image.create_from_data(TILE, TILE, false, Image.FORMAT_RGBH, raw)
	_cache_mutex.lock()
	_cache[key] = img
	_cache_order.append(key)
	if _cache_order.size() > 600:
		_cache.erase(_cache_order.pop_front())
	_cache_mutex.unlock()
	return img
