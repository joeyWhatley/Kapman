extends PanelContainer
## Port details on demand: shown on hover or when a port is selected. Trading by hand when docked here.
## Layout lives in port_panel.tscn (edit it in the editor); _build() only runs if the nodes are missing.

signal buy(good: int)
signal sell(good: int)
signal closed
signal repair
signal hire

const Style := preload("res://route_map/style.gd")
const D := preload("res://scripts/data.gd")
const FACTIONS := {"F": "Frankish", "E": "English", "D": "Danish", "N": "Norse", "S": "Slavic", "B": "Baltic", "R": "Rus", "G": "Rhomaioi", "K": "Khazar & Bulgar"}

var state
var index
var port := ""
var pinned := false
var extra := ""        # e.g. "About 5 days from here"
var _title: Label
var _sub: Label
var _grid: GridContainer
var _foot: Label
var _close: Button
var _yard: HBoxContainer
var _yard_label: Label
var _repair: Button
var _hire: Button
var _tween: Tween


func _ready() -> void:
	if get_node_or_null("Body") == null:
		_build()
	_bind()
	mouse_filter = Control.MOUSE_FILTER_STOP
	modulate.a = 0.0
	visible = false


func _build() -> void:
	name = "PortPanel"
	custom_minimum_size = Vector2(340, 0)
	var v := VBoxContainer.new()
	v.name = "Body"
	v.add_theme_constant_override("separation", 8)
	add_child(v)
	var top := HBoxContainer.new()
	top.name = "Top"
	v.add_child(top)
	var title := _label("Port", 20, Style.INK, "SemiBold")
	title.name = "Title"
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	top.add_child(title)
	var close := Button.new()
	close.name = "Close"
	close.text = "✕"
	close.custom_minimum_size = Vector2(28, 26)
	top.add_child(close)
	var sub := _label("", 12, Style.INK_SOFT)
	sub.name = "Sub"
	sub.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	v.add_child(sub)
	var sep := HSeparator.new()
	sep.name = "Sep"
	v.add_child(sep)
	var grid := GridContainer.new()
	grid.name = "Prices"
	grid.columns = 5
	grid.add_theme_constant_override("h_separation", 12)
	grid.add_theme_constant_override("v_separation", 4)
	v.add_child(grid)
	var yard := HBoxContainer.new()
	yard.name = "Shipwright"
	yard.add_theme_constant_override("separation", 8)
	v.add_child(yard)
	var yl := _label("", 12, Style.INK_SOFT)
	yl.name = "ShipState"
	yl.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	yard.add_child(yl)
	for n in ["Repair", "Hire"]:
		var b := Button.new()
		b.name = n
		b.text = n
		b.add_theme_font_size_override("font_size", 12)
		yard.add_child(b)
	var foot := _label("", 12, Style.INK_SOFT)
	foot.name = "Foot"
	foot.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	v.add_child(foot)


func _bind() -> void:
	_title = $Body/Top/Title
	_close = $Body/Top/Close
	_sub = $Body/Sub
	_grid = $Body/Prices
	_yard = $Body/Shipwright
	_yard_label = $Body/Shipwright/ShipState
	_repair = $Body/Shipwright/Repair
	_hire = $Body/Shipwright/Hire
	_foot = $Body/Foot
	_close.pressed.connect(func(): closed.emit())
	_repair.pressed.connect(func(): repair.emit())
	_hire.pressed.connect(func(): hire.emit())


func _label(text: String, size := 13, color := Style.INK, weight := "Regular") -> Label:
	var l := Label.new()
	l.text = text
	l.add_theme_font_size_override("font_size", size)
	l.add_theme_color_override("font_color", color)
	l.add_theme_font_override("font", Style.font(weight))
	return l


func show_port(p: String, pin: bool) -> void:
	var changed := p != port or not visible
	port = p
	pinned = pin
	refresh()
	if changed:
		_fade(true)


func hide_panel() -> void:
	port = ""
	pinned = false
	_fade(false)


func _fade(show_it: bool) -> void:
	if _tween:
		_tween.kill()
	_tween = create_tween().set_parallel().set_trans(Tween.TRANS_CUBIC).set_ease(Tween.EASE_OUT)
	if show_it:
		visible = true
		offset_right = -8.0
		_tween.tween_property(self, "modulate:a", 1.0, 0.16)
		_tween.tween_property(self, "offset_right", -20.0, 0.18)
	else:
		_tween.tween_property(self, "modulate:a", 0.0, 0.14)
		_tween.chain().tween_callback(func(): visible = false)


func refresh() -> void:
	if port == "" or state == null or index == null:
		return
	var info: Dictionary = index.ports[port]
	var docked: bool = state.port == port
	_title.text = port
	_sub.text = "%s · %s\nApproach: %s" % [FACTIONS.get(info["fac"], info["fac"]), info["tag"], info["note"]]
	_close.visible = pinned
	for c in _grid.get_children():
		c.queue_free()
	for h in ["Goods", "Price", "Held", "", ""]:
		_grid.add_child(_label(h, 11, Style.INK_SOFT, "Medium"))
	for g in D.GOODS.size():
		var pr: int = state.price(port, g)
		_grid.add_child(_label(D.GOODS[g], 13, Style.ACCENT if g == D.SWORDS else Style.INK, "Medium"))
		if pr < 0:
			_grid.add_child(_label("—", 13, Style.INK_SOFT))
		else:
			var mood: int = state.mood.get(g, 0) if docked else 0
			_grid.add_child(_label("%d%s" % [pr, "  ↑" if mood > 0 else ("  ↓" if mood < 0 else "")], 13))
		var held: int = state.count_good(g)
		_grid.add_child(_label(str(held) if held > 0 else "", 13, Style.INK_SOFT))
		if docked:
			_grid.add_child(_trade_button("Buy", g, state.why_not_buy(g), buy))
			_grid.add_child(_trade_button("Sell", g, state.why_not_sell(g), sell))
		else:
			_grid.add_child(_label("", 13))
			_grid.add_child(_label("", 13))
	_yard.visible = docked
	if docked:
		_yard_label.text = "Hull %d/%d · Crew %d/%d" % [state.hull, D.START["hull"], state.crew, D.START["crew_max"]]
		_repair.text = "Repair +1 (%ds)" % D.COSTS["repair"]
		_repair.disabled = state.why_not_repair() != ""
		_repair.tooltip_text = state.why_not_repair()
		_hire.text = "Hire +1 (%ds)" % D.COSTS["hire"]
		_hire.disabled = state.why_not_hire() != ""
		_hire.tooltip_text = state.why_not_hire()
		_foot.text = "Docked. Hold %d/%d · %d silver. ↑↓ = today's market mood." % [state.hold.size(), D.START["hold"], state.silver]
	else:
		_foot.text = "Prices as last heard. " + extra


func _trade_button(text: String, g: int, why: String, sig: Signal) -> Button:
	var b := Button.new()
	b.text = text
	b.add_theme_font_size_override("font_size", 12)
	b.custom_minimum_size = Vector2(52, 24)
	b.disabled = why != ""
	b.tooltip_text = why
	b.pressed.connect(func(): sig.emit(g))
	return b
