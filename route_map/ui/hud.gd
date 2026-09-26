extends CanvasLayer
## The route map's HUD: status strip, time controls, action bar, toast, hover tip, plus the port
## panel, music player, incident card and battle screen (each its own scene, instanced here).
## Layout lives in hud.tscn; _build() only runs if the nodes are missing.

const Style := preload("res://route_map/style.gd")

var strip_label: Label
var time_label: Label
var btn_play: Button
var btn_pause: Button
var btn_fast: Button
var action_label: Label
var action_hint: Label
var action_box: HBoxContainer
var toast: Label
var tip: PanelContainer
var tip_label: Label
var port_panel
var music
var incident
var battle_screen


static func pin(c: Control, ax: float, ay: float, ox: float, oy: float) -> void:
	c.anchor_left = ax
	c.anchor_right = ax
	c.anchor_top = ay
	c.anchor_bottom = ay
	c.grow_horizontal = Control.GROW_DIRECTION_END if ax == 0.0 else (Control.GROW_DIRECTION_BEGIN if ax == 1.0 else Control.GROW_DIRECTION_BOTH)
	c.grow_vertical = Control.GROW_DIRECTION_END if ay == 0.0 else Control.GROW_DIRECTION_BEGIN
	c.offset_left = ox
	c.offset_right = ox
	c.offset_top = oy
	c.offset_bottom = oy


func _ready() -> void:
	if get_node_or_null("Root") == null:
		_build({})
	_bind()


## `scenes` maps "PortPanel"/"MusicPlayer"/"IncidentCard"/"BattleScreen" to PackedScenes (the builder
## passes them so the HUD saves instances); with no scenes the scripts build themselves.
func _build(scenes: Dictionary) -> void:
	name = "HUD"
	var root := Control.new()
	root.name = "Root"
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter = Control.MOUSE_FILTER_IGNORE
	if ResourceLoader.exists("res://route_map/ui/theme.tres"):
		root.theme = load("res://route_map/ui/theme.tres")
	else:
		root.theme = Style.make_theme()
	add_child(root)

	var strip := PanelContainer.new()
	strip.name = "Strip"
	root.add_child(strip)
	pin(strip, 0.0, 0.0, 20, 18)
	var sl := Label.new()
	sl.name = "Status"
	sl.add_theme_font_override("font", Style.font("Medium"))
	strip.add_child(sl)

	var tc := PanelContainer.new()
	tc.name = "TimeControls"
	root.add_child(tc)
	pin(tc, 1.0, 0.0, -20, 18)
	var th := HBoxContainer.new()
	th.name = "Row"
	th.add_theme_constant_override("separation", 4)
	tc.add_child(th)
	for spec in [["Play", "▶", "Start / resume (Space)"], ["Pause", "❚❚", "Pause (Space)"], ["Fast", "▶▶", "Fast forward"]]:
		var b := Button.new()
		b.name = spec[0]
		b.text = spec[1]
		b.tooltip_text = spec[2]
		b.custom_minimum_size = Vector2(38, 28)
		th.add_child(b)
	var tl := Label.new()
	tl.name = "Speed"
	tl.custom_minimum_size = Vector2(70, 0)
	tl.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	tl.add_theme_font_size_override("font_size", 12)
	tl.add_theme_color_override("font_color", Style.INK_SOFT)
	tl.size_flags_vertical = Control.SIZE_SHRINK_CENTER
	th.add_child(tl)

	var bar := PanelContainer.new()
	bar.name = "ActionBar"
	root.add_child(bar)
	pin(bar, 0.5, 1.0, 0, -20)
	var bv := VBoxContainer.new()
	bv.name = "Body"
	bv.add_theme_constant_override("separation", 8)
	bar.add_child(bv)
	var al := Label.new()
	al.name = "Label"
	al.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	al.add_theme_font_override("font", Style.font("Medium"))
	bv.add_child(al)
	var ah := Label.new()
	ah.name = "Hint"
	ah.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	ah.add_theme_font_size_override("font_size", 12)
	ah.add_theme_color_override("font_color", Style.INK_SOFT)
	bv.add_child(ah)
	var ab := HBoxContainer.new()
	ab.name = "Buttons"
	ab.alignment = BoxContainer.ALIGNMENT_CENTER
	ab.add_theme_constant_override("separation", 8)
	bv.add_child(ab)

	var pp: Control = scenes["PortPanel"].instantiate() if scenes.has("PortPanel") else load("res://route_map/ui/port_panel.gd").new()
	pp.name = "PortPanel"
	root.add_child(pp)
	pin(pp, 1.0, 0.0, -20, 70)

	var mp: Control = scenes["MusicPlayer"].instantiate() if scenes.has("MusicPlayer") else load("res://route_map/ui/music_player.gd").new()
	mp.name = "MusicPlayer"
	root.add_child(mp)
	pin(mp, 1.0, 1.0, -20, -18)

	var t := Label.new()
	t.name = "Toast"
	t.add_theme_font_override("font", Style.font("Medium"))
	t.add_theme_font_size_override("font_size", 14)
	t.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.add_child(t)
	pin(t, 0.5, 0.0, 0, 70)

	var tp := PanelContainer.new()
	tp.name = "Tip"
	tp.add_theme_stylebox_override("panel", Style.panel_box(10))
	tp.mouse_filter = Control.MOUSE_FILTER_IGNORE
	root.add_child(tp)
	var tpl := Label.new()
	tpl.name = "Text"
	tpl.add_theme_font_size_override("font_size", 12)
	tpl.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	tpl.custom_minimum_size = Vector2(260, 0)
	tp.add_child(tpl)

	var ic: Control = scenes["IncidentCard"].instantiate() if scenes.has("IncidentCard") else load("res://route_map/ui/incident_card.gd").new()
	ic.name = "IncidentCard"
	root.add_child(ic)
	var bs: Control = scenes["BattleScreen"].instantiate() if scenes.has("BattleScreen") else load("res://route_map/battle/battle_screen.gd").new()
	bs.name = "BattleScreen"
	root.add_child(bs)


func _bind() -> void:
	strip_label = $Root/Strip/Status
	time_label = $Root/TimeControls/Row/Speed
	btn_play = $Root/TimeControls/Row/Play
	btn_pause = $Root/TimeControls/Row/Pause
	btn_fast = $Root/TimeControls/Row/Fast
	action_label = $Root/ActionBar/Body/Label
	action_hint = $Root/ActionBar/Body/Hint
	action_box = $Root/ActionBar/Body/Buttons
	toast = $Root/Toast
	tip = $Root/Tip
	tip_label = $Root/Tip/Text
	port_panel = $Root/PortPanel
	music = $Root/MusicPlayer
	incident = $Root/IncidentCard
	battle_screen = $Root/BattleScreen
	toast.modulate.a = 0.0
	tip.visible = false
