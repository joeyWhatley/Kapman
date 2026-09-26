extends Control
## Something happens at sea: a card with a few choices. Emits chosen(index). Time is paused while it's open.
## Layout lives in incident_card.tscn; _build() only runs if the nodes are missing.

signal chosen(index: int)

const Style := preload("res://route_map/style.gd")

var _panel: PanelContainer
var _kind: Label
var _title: Label
var _body: Label
var _options: VBoxContainer


func _ready() -> void:
	if get_node_or_null("Card") == null:
		_build()
	_bind()
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP
	visible = false


func _build() -> void:
	name = "IncidentCard"
	var panel := PanelContainer.new()
	panel.name = "Card"
	panel.add_theme_stylebox_override("panel", Style.panel_box(22))
	add_child(panel)
	var v := VBoxContainer.new()
	v.name = "Body"
	v.add_theme_constant_override("separation", 10)
	panel.add_child(v)
	var kind := Label.new()
	kind.name = "Kind"
	kind.add_theme_font_size_override("font_size", 11)
	kind.add_theme_color_override("font_color", Style.INK_SOFT)
	v.add_child(kind)
	var title := Label.new()
	title.name = "Title"
	title.add_theme_font_override("font", Style.font("SemiBold"))
	title.add_theme_font_size_override("font_size", 22)
	v.add_child(title)
	var body := Label.new()
	body.name = "Text"
	body.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	body.custom_minimum_size = Vector2(420, 0)
	v.add_child(body)
	var opts := VBoxContainer.new()
	opts.name = "Options"
	opts.add_theme_constant_override("separation", 6)
	v.add_child(opts)


func _bind() -> void:
	_panel = $Card
	_kind = $Card/Body/Kind
	_title = $Card/Body/Title
	_body = $Card/Body/Text
	_options = $Card/Body/Options


func open(kind: String, title: String, body: String, options: Array) -> void:
	_kind.text = kind.to_upper()
	_title.text = title
	_body.text = body
	for c in _options.get_children():
		c.queue_free()
	for i in options.size():
		var b := Button.new()
		b.text = options[i]
		b.custom_minimum_size = Vector2(420, 34)
		if i == 0:
			b.theme_type_variation = "PrimaryButton"
		b.pressed.connect(_pick.bind(i))
		_options.add_child(b)
	visible = true
	modulate.a = 0.0
	_panel.reset_size()
	await get_tree().process_frame
	_panel.position = (size - _panel.size) / 2.0
	create_tween().tween_property(self, "modulate:a", 1.0, 0.18)


func _pick(i: int) -> void:
	visible = false
	chosen.emit(i)


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, size), Color(0.1, 0.11, 0.13, 0.18))
