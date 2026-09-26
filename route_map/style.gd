extends RefCounted
## One look for the route map: ink on paper, one typeface (Inter), quiet UI.

## The old Frisian chart: vellum, iron-gall ink, vermilion. (UI panels keep the clean theme.tres.)
const PAPER := Color(0.937, 0.905, 0.83)
const PAPER_OUTSIDE := Color(0.24, 0.19, 0.15)     # the leather the chart lies on
const FOG := Color(0.74, 0.69, 0.6)
const INK := Color(0.15, 0.11, 0.08)
const INK_SOFT := Color(0.15, 0.11, 0.08, 0.6)
const INK_FAINT := Color(0.15, 0.11, 0.08, 0.22)
const ROUGH := Color(0.15, 0.11, 0.08, 0.42)
const RIVER := Color(0.15, 0.11, 0.08, 0.45)
const ACCENT := Color(0.66, 0.2, 0.1)
const GOLD := Color(0.7, 0.55, 0.28)
const PANEL := Color(0.985, 0.98, 0.968, 0.97)

static var _fonts := {}


static func font(weight := "Regular") -> Font:
	if not _fonts.has(weight):
		_fonts[weight] = load("res://route_map/fonts/Inter-%s.ttf" % weight)
	return _fonts[weight]


static func panel_box(margin := 14) -> StyleBoxFlat:
	var s := StyleBoxFlat.new()
	s.bg_color = PANEL
	s.border_color = INK_FAINT
	s.set_border_width_all(1)
	s.set_corner_radius_all(6)
	s.shadow_color = Color(0, 0, 0, 0.1)
	s.shadow_size = 10
	s.shadow_offset = Vector2(0, 3)
	s.content_margin_left = margin
	s.content_margin_right = margin
	s.content_margin_top = margin * 0.7
	s.content_margin_bottom = margin * 0.7
	return s


static func _button_box(bg: Color, border: Color) -> StyleBoxFlat:
	var s := StyleBoxFlat.new()
	s.bg_color = bg
	s.border_color = border
	s.set_border_width_all(1)
	s.set_corner_radius_all(4)
	s.content_margin_left = 10
	s.content_margin_right = 10
	s.content_margin_top = 4
	s.content_margin_bottom = 4
	return s


static func make_theme() -> Theme:
	var t := Theme.new()
	t.default_font = font("Regular")
	t.default_font_size = 14
	t.set_stylebox("normal", "Button", _button_box(Color(1, 1, 1, 0), INK_FAINT))
	t.set_stylebox("hover", "Button", _button_box(Color(0.1, 0.11, 0.13, 0.06), INK_SOFT))
	t.set_stylebox("pressed", "Button", _button_box(Color(0.1, 0.11, 0.13, 0.12), INK))
	t.set_stylebox("disabled", "Button", _button_box(Color(1, 1, 1, 0), Color(0.1, 0.11, 0.13, 0.08)))
	t.set_stylebox("focus", "Button", StyleBoxEmpty.new())
	t.set_color("font_color", "Button", INK)
	t.set_color("font_hover_color", "Button", INK)
	t.set_color("font_pressed_color", "Button", INK)
	t.set_color("font_focus_color", "Button", INK)
	t.set_color("font_disabled_color", "Button", Color(0.1, 0.11, 0.13, 0.3))
	t.set_font("font", "Button", font("Medium"))
	t.set_color("font_color", "Label", INK)
	t.set_stylebox("panel", "PanelContainer", panel_box())
	t.set_type_variation("PrimaryButton", "Button")
	t.set_stylebox("normal", "PrimaryButton", _button_box(INK, INK))
	t.set_stylebox("hover", "PrimaryButton", _button_box(Color(0.22, 0.23, 0.26), INK))
	t.set_stylebox("pressed", "PrimaryButton", _button_box(Color(0.0, 0.0, 0.0), INK))
	t.set_color("font_color", "PrimaryButton", PANEL)
	t.set_color("font_hover_color", "PrimaryButton", PANEL)
	t.set_color("font_pressed_color", "PrimaryButton", PANEL)
	t.set_color("font_focus_color", "PrimaryButton", PANEL)
	return t
