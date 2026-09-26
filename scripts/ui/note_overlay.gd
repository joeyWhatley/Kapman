extends Control
## A note slid across the desk: a question with a few answers. Emits chosen(index).

signal chosen(index: int)

const Ink := preload("res://scripts/ui/ink.gd")

var _panel: PanelContainer
var _title: Label
var _text: Label
var _options: VBoxContainer


func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_STOP
	visible = false
	_panel = PanelContainer.new()
	var box := StyleBoxFlat.new()
	box.bg_color = Ink.PAPER
	box.border_color = Ink.INK
	box.set_border_width_all(2)
	box.shadow_color = Ink.SHADOW
	box.shadow_size = 10
	box.shadow_offset = Vector2(6, 8)
	box.set_content_margin_all(22)
	_panel.add_theme_stylebox_override("panel", box)
	add_child(_panel)
	var v := VBoxContainer.new()
	v.add_theme_constant_override("separation", 10)
	_panel.add_child(v)
	_title = Label.new()
	_title.add_theme_font_override("font", Ink.serif())
	_title.add_theme_font_size_override("font_size", 24)
	v.add_child(_title)
	_text = Label.new()
	_text.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	_text.custom_minimum_size = Vector2(440, 0)
	_text.add_theme_font_size_override("font_size", 15)
	v.add_child(_text)
	_options = VBoxContainer.new()
	_options.add_theme_constant_override("separation", 6)
	v.add_child(_options)


func ask(title: String, text: String, options: Array) -> void:
	_title.text = title
	_text.text = text
	_text.visible = text != ""
	for c in _options.get_children():
		c.queue_free()
	for i in options.size():
		var b := Button.new()
		b.text = options[i]
		b.custom_minimum_size = Vector2(440, 36)
		b.pressed.connect(_pick.bind(i))
		_options.add_child(b)
	visible = true
	queue_redraw()
	_panel.reset_size()
	await get_tree().process_frame
	_panel.position = (size - _panel.size) / 2.0


func _pick(i: int) -> void:
	visible = false
	chosen.emit(i)


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, size), Color(0, 0, 0, 0.35))
