extends Control
## Side-vs-side battle screen in ink: your ship on the left, the raiders on the right.
## Emits finished(outcome) when the fight is over and the player presses Continue.

signal finished(outcome: String)

const Style := preload("res://route_map/style.gd")
const Rules := preload("res://route_map/battle/battle_rules.gd")

var rules
var ship_name := "Seamew"
var _panel: PanelContainer
var _stage: Control
var _log: Label
var _moves: GridContainer
var _buttons := {}
var _end_box: HBoxContainer
var _sep := 0.44          # distance between ships as a share of stage width
var _shake_me := 0.0
var _shake_them := 0.0


func _ready() -> void:
	if get_node_or_null("Card") == null:
		_build()
	_bind()
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP
	visible = false


## Layout lives in battle_screen.tscn; this only runs if the nodes are missing.
func _build() -> void:
	name = "BattleScreen"
	var panel := PanelContainer.new()
	panel.name = "Card"
	var box := Style.panel_box(22)
	box.bg_color = Color(Style.PANEL, 1.0)
	panel.add_theme_stylebox_override("panel", box)
	add_child(panel)
	var v := VBoxContainer.new()
	v.name = "Body"
	v.add_theme_constant_override("separation", 12)
	panel.add_child(v)
	var stage := Control.new()
	stage.name = "Stage"
	stage.custom_minimum_size = Vector2(900, 215)
	v.add_child(stage)
	var log_label := Label.new()
	log_label.name = "Log"
	log_label.custom_minimum_size = Vector2(900, 84)
	log_label.add_theme_font_size_override("font_size", 13)
	log_label.add_theme_color_override("font_color", Style.INK_SOFT)
	log_label.vertical_alignment = VERTICAL_ALIGNMENT_BOTTOM
	v.add_child(log_label)
	var moves := GridContainer.new()
	moves.name = "Moves"
	moves.columns = 4
	moves.add_theme_constant_override("h_separation", 8)
	moves.add_theme_constant_override("v_separation", 8)
	v.add_child(moves)
	for m in Rules.MOVES:
		var b := Button.new()
		b.name = m["id"]
		b.text = m["name"]
		b.tooltip_text = m["tip"]
		b.custom_minimum_size = Vector2(219, 38)
		moves.add_child(b)
	var end_box := HBoxContainer.new()
	end_box.name = "End"
	end_box.alignment = BoxContainer.ALIGNMENT_CENTER
	v.add_child(end_box)
	var cont := Button.new()
	cont.name = "Continue"
	cont.text = "Continue"
	cont.theme_type_variation = "PrimaryButton"
	cont.custom_minimum_size = Vector2(220, 38)
	end_box.add_child(cont)


func _bind() -> void:
	_panel = $Card
	_stage = $Card/Body/Stage
	_log = $Card/Body/Log
	_moves = $Card/Body/Moves
	_end_box = $Card/Body/End
	_stage.draw.connect(_draw_stage)
	for m in Rules.MOVES:
		var id: String = m["id"]
		var b: Button = _moves.get_node(id)
		b.pressed.connect(func(): _play(id))
		_buttons[id] = b
	$Card/Body/End/Continue.pressed.connect(func(): visible = false; finished.emit(rules.outcome))


func start(r, ship: String) -> void:
	rules = r
	ship_name = ship
	_sep = 0.24 if rules.close else 0.44
	_refresh()
	visible = true
	modulate.a = 0.0
	_panel.reset_size()
	await get_tree().process_frame
	_panel.position = (size - _panel.size) / 2.0
	create_tween().tween_property(self, "modulate:a", 1.0, 0.2)


func _play(id: String) -> void:
	var hull_me: int = rules.state.hull
	var crew_me: int = rules.state.crew
	var crew_them: int = rules.them["crew"]
	var hull_them: int = rules.them["hull"]
	rules.play_round(id)
	if rules.state.hull < hull_me or rules.state.crew < crew_me:
		_shake("_shake_me")
	if rules.them["crew"] < crew_them or rules.them["hull"] < hull_them:
		_shake("_shake_them")
	var target := 0.24 if rules.close else 0.44
	if target != _sep:
		create_tween().set_trans(Tween.TRANS_SINE).tween_method(func(x): _sep = x; _stage.queue_redraw(), _sep, target, 0.5)
	_refresh()


func _shake(prop: String) -> void:
	var t := create_tween()
	for k in 4:
		t.tween_method(func(x): set(prop, x); _stage.queue_redraw(), 0.0, 5.0 if k % 2 == 0 else -5.0, 0.05)
	t.tween_method(func(x): set(prop, x); _stage.queue_redraw(), 0.0, 0.0, 0.05)


func _refresh() -> void:
	var shown: Array = rules.lines.slice(maxi(0, rules.lines.size() - 4))
	_log.text = "\n".join(shown) if not shown.is_empty() else "They're coming about. Choose your move."
	var over: bool = rules.outcome != ""
	for id in _buttons:
		_buttons[id].disabled = not rules.valid(id)
	_moves.visible = not over
	_end_box.visible = over
	_stage.queue_redraw()


# --- Drawing ---------------------------------------------------------------

func _draw_stage() -> void:
	var s := _stage.size
	var water := s.y * 0.8
	# Sea: one line and a few quiet ticks.
	_stage.draw_line(Vector2(0, water), Vector2(s.x, water), Style.INK_FAINT, 1.0, true)
	for i in 18:
		var x := 25.0 + i * (s.x - 50.0) / 17.0
		_stage.draw_line(Vector2(x, water + 8 + (i % 3) * 5), Vector2(x + 14, water + 8 + (i % 3) * 5), Color(Style.INK, 0.08), 1.0)
	var me_x := s.x * (0.5 - _sep) + _shake_me
	var them_x := s.x * (0.5 + _sep) + _shake_them
	_draw_trader(Vector2(me_x, water))
	_draw_longship(Vector2(them_x, water))
	var range_txt := "CLOSE" if rules.close else "AT RANGE"
	_stage.draw_string(Style.font("Medium"), Vector2(0, 26), range_txt, HORIZONTAL_ALIGNMENT_CENTER, s.x, 12, Style.INK_SOFT)
	if rules.shield:
		_stage.draw_string(Style.font("Medium"), Vector2(0, 44), "shield wall up", HORIZONTAL_ALIGNMENT_CENTER, s.x, 12, Style.ACCENT)
	_draw_stats(Vector2(20, 18), ship_name, [["Hull", rules.state.hull, 8], ["Crew", rules.state.crew, 8], ["Morale", rules.state.morale, Rules.MAX_MORALE]], false)
	_draw_stats(Vector2(s.x - 20, 18), "Raiders", [["Hull", rules.them["hull"], rules.them["max_hull"]], ["Crew", rules.them["crew"], rules.them["max_crew"]], ["Morale", rules.them["morale"], rules.them["max_morale"]]], true)
	if rules.last_roll > 0:
		_stage.draw_string(Style.font("Regular"), Vector2(0, s.y - 8), "last roll: %d" % rules.last_roll, HORIZONTAL_ALIGNMENT_CENTER, s.x, 11, Color(Style.INK, 0.35))


func _draw_stats(pos: Vector2, title: String, rows: Array, right: bool) -> void:
	var f := Style.font("SemiBold")
	var align := HORIZONTAL_ALIGNMENT_RIGHT if right else HORIZONTAL_ALIGNMENT_LEFT
	var x0 := pos.x - 240 if right else pos.x
	_stage.draw_string(f, Vector2(x0, pos.y + 4), title, align, 240, 15, Style.INK)
	for i in rows.size():
		var y := pos.y + 26 + i * 20
		var label: String = rows[i][0]
		var val: int = rows[i][1]
		var mx: int = rows[i][2]
		var pip_w := 11.0
		var total := mx * (pip_w + 3)
		var start := (pos.x - total) if right else (pos.x + 58)
		_stage.draw_string(Style.font("Regular"), Vector2((start - 58) if not right else (start - 60), y + 9), label, HORIZONTAL_ALIGNMENT_RIGHT if right else HORIZONTAL_ALIGNMENT_LEFT, 54, 11, Style.INK_SOFT)
		for k in mx:
			var r := Rect2(start + k * (pip_w + 3), y, pip_w, 10)
			if k < val:
				_stage.draw_rect(r, Style.INK)
			else:
				_stage.draw_rect(r, Color(Style.INK, 0.12))


## A stubby Frisian cargo ship, bow to the right.
func _draw_trader(p: Vector2) -> void:
	var outline := PackedVector2Array([p + Vector2(-70, -18), p + Vector2(-58, 0), p + Vector2(55, 0), p + Vector2(78, -22)])
	_stage.draw_polyline(outline, Style.INK, 2.0, true)
	_stage.draw_line(p + Vector2(-70, -18), p + Vector2(78, -22), Style.INK, 1.2, true)
	_stage.draw_line(p + Vector2(0, -20), p + Vector2(0, -120), Style.INK, 2.0, true)
	var sail := PackedVector2Array([p + Vector2(-34, -112), p + Vector2(38, -112), p + Vector2(34, -44), p + Vector2(-30, -44)])
	_stage.draw_colored_polygon(sail, Color(Style.PAPER, 1.0))
	sail.append(sail[0])
	_stage.draw_polyline(sail, Style.INK, 1.4, true)
	_stage.draw_line(p + Vector2(-34, -112), p + Vector2(38, -112), Style.INK, 2.0)
	_stage.draw_line(p + Vector2(0, -120), p + Vector2(76, -24), Style.INK_FAINT, 1.0, true)


## A long, low raiding ship with a dragon prow, bow to the left.
func _draw_longship(p: Vector2) -> void:
	var keel := PackedVector2Array([p + Vector2(-92, -34), p + Vector2(-80, -8), p + Vector2(-60, 0), p + Vector2(60, 0), p + Vector2(82, -8), p + Vector2(92, -32)])
	_stage.draw_polyline(keel, Style.INK, 2.0, true)
	_stage.draw_line(p + Vector2(-80, -14), p + Vector2(84, -14), Style.INK, 1.2, true)
	# dragon head and tail curl
	_stage.draw_arc(p + Vector2(-98, -38), 7.0, -PI * 0.2, PI * 1.3, 12, Style.INK, 1.8, true)
	_stage.draw_arc(p + Vector2(98, -36), 6.0, PI * 0.8, PI * 2.2, 12, Style.INK, 1.6, true)
	# shields and oars
	for i in 9:
		var x := -64.0 + i * 16.0
		_stage.draw_arc(p + Vector2(x, -18), 5.0, 0, TAU, 14, Style.INK, 1.2, true)
		_stage.draw_line(p + Vector2(x, -10), p + Vector2(x - 10, 16), Style.INK_SOFT, 1.0, true)
	_stage.draw_line(p + Vector2(0, -22), p + Vector2(0, -108), Style.INK, 2.0, true)
	var sail := PackedVector2Array([p + Vector2(-36, -100), p + Vector2(36, -100), p + Vector2(32, -40), p + Vector2(-32, -40)])
	_stage.draw_colored_polygon(sail, Style.PAPER)
	for k in 4:
		var x0 := -36.0 + k * 18.0 + 4.0
		_stage.draw_colored_polygon(PackedVector2Array([p + Vector2(x0, -100), p + Vector2(x0 + 9, -100), p + Vector2(x0 + 8, -40), p + Vector2(x0 - 1, -40)]), Color(Style.ACCENT, 0.75))
	sail.append(sail[0])
	_stage.draw_polyline(sail, Style.INK, 1.4, true)
	_stage.draw_line(p + Vector2(-36, -100), p + Vector2(36, -100), Style.INK, 2.0)


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, size), Color(0.1, 0.11, 0.13, 0.22))
