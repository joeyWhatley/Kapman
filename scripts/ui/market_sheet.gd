extends "res://scripts/ui/paper.gd"
## The market sheet: printed base prices, pencil marks, and buy/sell for the port you're in.

signal buy(good: int)
signal sell(good: int)

const D := preload("res://scripts/data.gd")
const GX := 14.0
const GY := 54.0
const PORT_W := 112.0
const CELL_W := 95.0
const HEAD_H := 38.0
const ROW_H := 42.0
const STRIP_Y := 450.0

var state
var busy := false
var _buy_buttons: Array = []
var _sell_buttons: Array = []


func _ready() -> void:
	title = "Market"
	subtitle = "price per lot · base + marks + season + event + mood"
	for g in D.GOODS.size():
		var x := GX + PORT_W + g * CELL_W + 6
		var b := Button.new()
		b.text = "Buy"
		b.position = Vector2(x, STRIP_Y + 40)
		b.size = Vector2(40, 26)
		b.add_theme_font_size_override("font_size", 12)
		b.pressed.connect(func(): buy.emit(g))
		add_child(b)
		_buy_buttons.append(b)
		var s := Button.new()
		s.text = "Sell"
		s.position = Vector2(x + 44, STRIP_Y + 40)
		s.size = Vector2(40, 26)
		s.add_theme_font_size_override("font_size", 12)
		s.pressed.connect(func(): sell.emit(g))
		add_child(s)
		_sell_buttons.append(s)


func refresh() -> void:
	for g in D.GOODS.size():
		var why_b: String = "Busy." if busy else state.why_not_buy(g)
		var why_s: String = "Busy." if busy else state.why_not_sell(g)
		_buy_buttons[g].disabled = why_b != ""
		_buy_buttons[g].tooltip_text = why_b
		_sell_buttons[g].disabled = why_s != ""
		_sell_buttons[g].tooltip_text = why_s
		var tradable: bool = state.port != "" and state.price(state.port, g) >= 0
		_buy_buttons[g].visible = tradable
		_sell_buttons[g].visible = tradable
	queue_redraw()


func _draw() -> void:
	draw_paper()
	if state == null:
		return
	var right := GX + PORT_W + CELL_W * D.GOODS.size()
	# Header
	draw_rect(Rect2(GX, GY, right - GX, HEAD_H), Ink.PAPER_DARK)
	draw_string(Ink.bold(), Vector2(GX + 6, GY + 24), "Port", HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Ink.INK)
	for g in D.GOODS.size():
		var x := GX + PORT_W + g * CELL_W
		draw_string(Ink.bold(), Vector2(x, GY + 17), "%d. %s" % [g + 1, D.GOODS[g]], HORIZONTAL_ALIGNMENT_CENTER, CELL_W, 14, Ink.INK)
		var note_col := Ink.RED if g == D.SWORDS else Ink.FAINT
		draw_string(Ink.sans(), Vector2(x, GY + 32), D.GOOD_NOTES[g], HORIZONTAL_ALIGNMENT_CENTER, CELL_W, 10, note_col)
	# Rows
	for i in D.PORT_ORDER.size():
		var p: String = D.PORT_ORDER[i]
		var y := GY + HEAD_H + i * ROW_H
		var here: bool = p == state.port
		if here:
			draw_rect(Rect2(GX, y, right - GX, ROW_H), Ink.WASH)
		draw_string(Ink.serif(), Vector2(GX + 6, y + 20), p, HORIZONTAL_ALIGNMENT_LEFT, -1, 15, Ink.INK)
		var fac: String = {"F": "Frankish", "E": "English", "D": "Danish"}[D.PORTS[p]["fac"]]
		draw_string(Ink.sans(), Vector2(GX + 6, y + 35), fac + ("  ← you" if here else ""), HORIZONTAL_ALIGNMENT_LEFT, -1, 11, Ink.FAINT)
		for g in D.GOODS.size():
			var x := GX + PORT_W + g * CELL_W
			var base: int = D.PORTS[p]["prices"][g]
			if base < 0:
				draw_rect(Rect2(x, y, CELL_W, ROW_H), Ink.GHOST)
				draw_string(Ink.sans(), Vector2(x, y + 26), "not traded", HORIZONTAL_ALIGNMENT_CENTER, CELL_W, 11, Ink.FAINT)
				continue
			draw_string(Ink.serif(), Vector2(x + 6, y + 24), str(base), HORIZONTAL_ALIGNMENT_LEFT, -1, 19, Ink.INK)
			var m: int = state.marks[p][g]
			if m != 0:
				draw_string(Ink.pencil(), Vector2(x + 6, y + 38), "marks %+d" % m, HORIZONTAL_ALIGNMENT_LEFT, -1, 10, Ink.PENCIL)
			var now: int = state.price(p, g)
			if now != base or here:
				draw_string(Ink.pencil(), Vector2(x, y + 24), "=%d" % now, HORIZONTAL_ALIGNMENT_RIGHT, CELL_W - 8, 17, Ink.RED if here else Ink.PENCIL)
			if g == D.SWORDS:
				var sm: String = D.PORTS[p]["swords"]
				draw_string(Ink.sans(), Vector2(x, y + 38), "buy only" if sm == "buy" else "sell only", HORIZONTAL_ALIGNMENT_RIGHT, CELL_W - 6, 9, Ink.FAINT)
	# Grid lines
	var bottom := GY + HEAD_H + ROW_H * D.PORT_ORDER.size()
	draw_rect(Rect2(GX, GY, right - GX, bottom - GY), Ink.INK, false, 1.5)
	for i in D.PORT_ORDER.size() + 1:
		var y := GY + HEAD_H + i * ROW_H
		draw_line(Vector2(GX, y), Vector2(right, y), Ink.INK, 1.0)
	for g in D.GOODS.size() + 1:
		var x := GX + PORT_W + g * CELL_W
		draw_line(Vector2(x, GY), Vector2(x, bottom), Ink.INK, 1.0)

	# Trading strip
	draw_line(Vector2(GX, STRIP_Y), Vector2(right, STRIP_Y), Ink.INK, 2.0)
	if state.port == "":
		draw_string(Ink.serif(), Vector2(GX, STRIP_Y + 30), "At sea — no trading.", HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Ink.INK)
	else:
		draw_string(Ink.serif(), Vector2(GX, STRIP_Y + 24), "Trading at", HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Ink.INK)
		draw_string(Ink.serif(), Vector2(GX, STRIP_Y + 44), state.port, HORIZONTAL_ALIGNMENT_LEFT, -1, 17, Ink.INK)
		for g in D.GOODS.size():
			var x := GX + PORT_W + g * CELL_W
			var pr: int = state.price(state.port, g)
			if pr < 0:
				continue
			var mood: int = state.mood.get(g, 0)
			draw_string(Ink.pencil(), Vector2(x + 6, STRIP_Y + 22), "%ds" % pr, HORIZONTAL_ALIGNMENT_LEFT, CELL_W - 8, 15, Ink.RED if mood != 0 else Ink.PENCIL)
			if mood != 0:
				draw_string(Ink.sans(), Vector2(x + 6, STRIP_Y + 36), "mood %+d" % mood, HORIZONTAL_ALIGNMENT_LEFT, CELL_W - 8, 10, Ink.RED)
	var hold_line := "Hold %d/%d lots · Silver %ds" % [state.hold.size(), D.START["hold"], state.silver]
	draw_string(Ink.pencil(), Vector2(GX, STRIP_Y + 100), hold_line, HORIZONTAL_ALIGNMENT_LEFT, -1, 16, Ink.PENCIL)
	var rule := "Each lot bought here: price +1. Each lot sold: −1. Marks wash out each new season. Swords: buy at Frankish ports (max 3), sell at English & Danish."
	draw_wrapped(Ink.sans(), Vector2(GX, STRIP_Y + 110), rule, right - GX, 11, Ink.FAINT)
