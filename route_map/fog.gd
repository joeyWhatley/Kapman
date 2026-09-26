extends RefCounted
## What you've explored. One byte per fog cell: 0 = fog, 255 = charted. Soft-edged reveals.
## Coarser than the land grid (the fog is soft anyway), and uploaded to the GPU at most ~12x a second.

const FOG_CELL := 10.0
const MIN_UPLOAD_INTERVAL := 0.08

var image: Image
var texture: ImageTexture
var cols := 0
var rows := 0
var _dirty := false
var _last_upload := -1.0


func setup(map_size: Vector2) -> void:
	cols = int(ceil(map_size.x / FOG_CELL))
	rows = int(ceil(map_size.y / FOG_CELL))
	image = Image.create(cols, rows, false, Image.FORMAT_L8)
	image.fill(Color.BLACK)
	texture = ImageTexture.create_from_image(image)


## The area the fog texture covers, in map units (for the shader).
func size_units() -> Vector2:
	return Vector2(cols, rows) * FOG_CELL


## Reveals a soft circle around p (map units).
func reveal(p: Vector2, radius: float) -> void:
	var c := p / FOG_CELL
	var r := radius / FOG_CELL
	for y in range(maxi(0, int(c.y - r - 1)), mini(rows, int(c.y + r + 2))):
		for x in range(maxi(0, int(c.x - r - 1)), mini(cols, int(c.x + r + 2))):
			var d := Vector2(x + 0.5, y + 0.5).distance_to(c)
			if d > r:
				continue
			var v := clampf((r - d) / (r * 0.4), 0.0, 1.0)
			if v > image.get_pixel(x, y).r:
				image.set_pixel(x, y, Color(v, v, v))
				_dirty = true


func reveal_path(pts: PackedVector2Array, radius: float) -> void:
	for i in pts.size() - 1:
		var steps := int(pts[i].distance_to(pts[i + 1]) / (radius * 0.5)) + 1
		for s in steps + 1:
			reveal(pts[i].lerp(pts[i + 1], float(s) / steps), radius)


## Uploads pending changes. `force` skips the rate limit (use after big reveals).
func flush(force := false) -> void:
	if not _dirty:
		return
	var now := Time.get_ticks_msec() / 1000.0
	if not force and now - _last_upload < MIN_UPLOAD_INTERVAL:
		return
	texture.update(image)
	_dirty = false
	_last_upload = now


## 0 (fog) .. 1 (charted) at a map position.
func charted(p: Vector2) -> float:
	var x := clampi(int(p.x / FOG_CELL), 0, cols - 1)
	var y := clampi(int(p.y / FOG_CELL), 0, rows - 1)
	return image.get_pixel(x, y).r
