extends RefCounted
## Shared look for the paper game: colors, fonts, button theme.

const PAPER := Color(0.965, 0.945, 0.89)
const PAPER_DARK := Color(0.915, 0.885, 0.805)
const EDGE := Color(0.55, 0.5, 0.42)
const INK := Color(0.1, 0.09, 0.08)
const FAINT := Color(0.1, 0.09, 0.08, 0.45)
const GHOST := Color(0.1, 0.09, 0.08, 0.14)
const PENCIL := Color(0.3, 0.31, 0.36)
const RED := Color(0.64, 0.13, 0.1)
const WASH := Color(1.0, 0.84, 0.3, 0.33)
const DESK := Color(0.27, 0.2, 0.15)
const SHADOW := Color(0, 0, 0, 0.3)

static var _fonts := {}


static func _sys(key: String, names: Array, italic := false, weight := 400) -> Font:
	if not _fonts.has(key):
		var f := SystemFont.new()
		f.font_names = PackedStringArray(names)
		f.font_italic = italic
		f.font_weight = weight
		_fonts[key] = f
	return _fonts[key]


static func serif() -> Font:
	return _sys("serif", ["Georgia", "Times New Roman", "serif"], false, 700)


static func italic() -> Font:
	return _sys("italic", ["Georgia", "Times New Roman", "serif"], true)


static func sans() -> Font:
	return _sys("sans", ["Segoe UI", "Arial", "sans-serif"])


static func bold() -> Font:
	return _sys("bold", ["Segoe UI", "Arial", "sans-serif"], false, 700)


static func pencil() -> Font:
	return _sys("pencil", ["Segoe Print", "Comic Sans MS", "cursive"])


static func _box(bg: Color, border: Color, width := 2) -> StyleBoxFlat:
	var s := StyleBoxFlat.new()
	s.bg_color = bg
	s.border_color = border
	s.set_border_width_all(width)
	s.set_corner_radius_all(3)
	s.content_margin_left = 8
	s.content_margin_right = 8
	s.content_margin_top = 3
	s.content_margin_bottom = 3
	return s


static func make_theme() -> Theme:
	var t := Theme.new()
	t.default_font = sans()
	t.default_font_size = 14
	t.set_stylebox("normal", "Button", _box(PAPER_DARK, INK))
	t.set_stylebox("hover", "Button", _box(Color(1.0, 0.93, 0.66), INK))
	t.set_stylebox("pressed", "Button", _box(Color(0.85, 0.8, 0.66), INK))
	t.set_stylebox("disabled", "Button", _box(PAPER, GHOST, 1))
	t.set_stylebox("focus", "Button", StyleBoxEmpty.new())
	t.set_color("font_color", "Button", INK)
	t.set_color("font_hover_color", "Button", INK)
	t.set_color("font_pressed_color", "Button", INK)
	t.set_color("font_focus_color", "Button", INK)
	t.set_color("font_disabled_color", "Button", FAINT)
	t.set_color("font_color", "Label", INK)
	t.set_font("font", "Button", bold())
	return t
