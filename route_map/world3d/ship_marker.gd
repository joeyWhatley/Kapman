@tool
extends Control
## Keeps your ship findable at every zoom. When the cog is too small to make out, an ink ship glyph
## (pointing the way she sails) with a ring and her name is drawn over her; when she is off-screen,
## an arrow at the screen edge points to her with the distance. Drawn in the chart's ink, so it reads
## the same over the 3D sea and on the 2D chart (which lines up with the 3D camera).

const INK := Color(0.15, 0.11, 0.08)
const PAPER := Color(0.95, 0.91, 0.83)
const ACCENT := Color(0.66, 0.2, 0.1)
const MARGIN := 34.0
## Keep the edge arrow out of the top-left status text.
@export var top_margin := 120.0

var cam: Camera3D
var ship: Node3D
var ship_name := "Aldgisl"
var length_m := 18.0

var _t := 0.0


func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_IGNORE


func _process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
	_t += delta
	queue_redraw()


func _draw() -> void:
	if cam == null or ship == null or not ship.is_inside_tree():
		return
	var size_px := get_viewport_rect().size
	var pos := ship.global_position
	var fwd3 := -ship.global_transform.basis.z
	fwd3.y = 0.0
	fwd3 = fwd3.normalized()
	var behind := cam.is_position_behind(pos)
	var sp := cam.unproject_position(pos)
	if behind:
		sp = size_px - sp           # mirrored: points the right way round when she is behind you
	var on_screen := not behind and Rect2(Vector2.ZERO, size_px).grow(-8.0).has_point(sp)
	if on_screen:
		var bow := cam.unproject_position(pos + fwd3 * length_m * 0.5)
		var stern := cam.unproject_position(pos - fwd3 * length_m * 0.5)
		var px_len := bow.distance_to(stern)
		var show := 1.0 - smoothstep(22.0, 48.0, px_len)
		if show <= 0.01:
			return
		var dir := (bow - stern).normalized() if px_len > 0.5 else _screen_dir(pos, fwd3)
		_glyph(sp, dir, show)
	else:
		_edge_arrow(sp, size_px)


func _screen_dir(pos: Vector3, fwd3: Vector3) -> Vector2:
	var a := cam.unproject_position(pos)
	var b := cam.unproject_position(pos + fwd3 * 1000.0)
	var d := b - a
	return d.normalized() if d.length() > 0.001 else Vector2.UP


## Ship glyph: a round-bellied cog seen from above, sail across, ring around, name beneath.
func _glyph(c: Vector2, dir: Vector2, alpha: float) -> void:
	var ink := Color(INK, alpha)
	var paper := Color(PAPER, alpha * 0.92)
	var pulse := 0.5 + 0.5 * sin(_t * 2.4)
	draw_arc(c, 17.0 + pulse * 3.0, 0.0, TAU, 48, Color(ACCENT, alpha * (0.35 + 0.25 * (1.0 - pulse))), 1.5, true)
	draw_circle(c, 14.0, paper)
	draw_arc(c, 14.0, 0.0, TAU, 40, ink, 1.6, true)
	var ang := dir.angle() + PI / 2.0         # glyph drawn bow-up
	var hull := PackedVector2Array([Vector2(0, -10), Vector2(3.2, -5.5), Vector2(4.2, 0), Vector2(3.6, 5.5),
		Vector2(1.8, 8.2), Vector2(-1.8, 8.2), Vector2(-3.6, 5.5), Vector2(-4.2, 0), Vector2(-3.2, -5.5)])
	for i in hull.size():
		hull[i] = c + hull[i].rotated(ang)
	draw_colored_polygon(hull, ink)
	# yard and sail across the hull
	var a := c + Vector2(-6.5, -1.0).rotated(ang)
	var b := c + Vector2(6.5, -1.0).rotated(ang)
	draw_line(a, b, Color(ACCENT, alpha), 2.4, true)
	draw_circle(c + Vector2(0, -1.0).rotated(ang), 1.4, paper)
	var font := get_theme_default_font()
	var fs := 13
	var w := font.get_string_size(ship_name, HORIZONTAL_ALIGNMENT_LEFT, -1, fs).x
	var tp := c + Vector2(-w / 2.0, 32.0)
	draw_string_outline(font, tp, ship_name, HORIZONTAL_ALIGNMENT_LEFT, -1, fs, 4, paper)
	draw_string(font, tp, ship_name, HORIZONTAL_ALIGNMENT_LEFT, -1, fs, ink)


## Off-screen: an arrow on the screen edge toward the ship, with how far she is.
func _edge_arrow(sp: Vector2, size_px: Vector2) -> void:
	var box := Rect2(Vector2(MARGIN, top_margin), size_px - Vector2(MARGIN * 2.0, top_margin + MARGIN))
	var centre := box.get_center()
	var d := sp - centre
	if d.length() < 0.001:
		d = Vector2.UP
	var half := box.size / 2.0
	var k := minf(absf(half.x / d.x) if absf(d.x) > 0.001 else INF, absf(half.y / d.y) if absf(d.y) > 0.001 else INF)
	var at := centre + d * k
	var dir := d.normalized()
	var ang := dir.angle()
	var tip := at + dir * 12.0
	var tri := PackedVector2Array([tip, at + Vector2(-8, -9).rotated(ang), at + Vector2(-8, 9).rotated(ang)])
	draw_circle(at, 17.0, Color(PAPER, 0.92))
	draw_arc(at, 17.0, 0.0, TAU, 40, INK, 1.6, true)
	draw_colored_polygon(tri, ACCENT)
	var metres := cam.global_position.distance_to(ship.global_position)
	var txt := "%s · %s" % [ship_name, ("%.0f m" % metres) if metres < 1000.0 else ("%.1f km" % (metres / 1000.0))]
	var font := get_theme_default_font()
	var fs := 12
	var w := font.get_string_size(txt, HORIZONTAL_ALIGNMENT_LEFT, -1, fs).x
	var tp := at - dir * 30.0 + Vector2(-w / 2.0, 4.0)
	tp.x = clampf(tp.x, 6.0, size_px.x - w - 6.0)
	tp.y = clampf(tp.y, 16.0, size_px.y - 6.0)
	draw_string_outline(font, tp, txt, HORIZONTAL_ALIGNMENT_LEFT, -1, fs, 4, Color(PAPER, 0.92))
	draw_string(font, tp, txt, HORIZONTAL_ALIGNMENT_LEFT, -1, fs, INK)
