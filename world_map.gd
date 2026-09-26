extends Node2D
## Kāpman — milestone 1: ports on a map, click one to sail there.
## Placeholder positions only. Swap in real coordinates once the traced map exists.

const PORTS := {
	"Kaupang": Vector2(700, 80),
	"Eoforwic": Vector2(300, 230),
	"Ribe": Vector2(680, 250),
	"Hedeby": Vector2(760, 300),
	"Lundenwic": Vector2(340, 440),
	"Dorestad": Vector2(600, 470),
	"Quentovic": Vector2(420, 540),
}
const HOME_PORT := "Dorestad"
const START_YEAR := 793

const SHIP_SPEED := 120.0      # map pixels per real second
const PIXELS_PER_DAY := 120.0  # how far the ship sails in one in-game day
const DAYS_PER_SEASON := 90
const SEASONS := ["Spring", "Summer", "Autumn", "Winter"]

const PAPER := Color(0.96, 0.94, 0.89)
const INK := Color(0.1, 0.1, 0.1)
const FAINT_INK := Color(0.1, 0.1, 0.1, 0.35)

var ship_pos: Vector2
var current_port := HOME_PORT
var target_port := ""
var day := 0.0

var status := Label.new()


func _ready() -> void:
	ship_pos = PORTS[HOME_PORT]
	add_child(status)
	status.position = Vector2(16, 12)
	status.add_theme_color_override("font_color", INK)
	_update_status()


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		var clicked := _port_at(get_global_mouse_position())
		if clicked != "" and clicked != current_port and target_port == "":
			target_port = clicked
			current_port = ""


func _process(delta: float) -> void:
	if target_port != "":
		var goal: Vector2 = PORTS[target_port]
		var step := SHIP_SPEED * delta
		var remaining := ship_pos.distance_to(goal)
		if remaining <= step:
			day += remaining / PIXELS_PER_DAY
			ship_pos = goal
			current_port = target_port
			target_port = ""
		else:
			ship_pos = ship_pos.move_toward(goal, step)
			day += step / PIXELS_PER_DAY
		_update_status()
	queue_redraw()


func _draw() -> void:
	draw_rect(get_viewport_rect(), PAPER)

	# Route line while sailing
	if target_port != "":
		draw_line(ship_pos, PORTS[target_port], FAINT_INK, 1.5)

	# Ports
	var font := ThemeDB.fallback_font
	for port_name in PORTS:
		var p: Vector2 = PORTS[port_name]
		draw_circle(p, 6.0, INK)
		draw_circle(p, 3.5, PAPER)
		draw_string(font, p + Vector2(10, -8), port_name, HORIZONTAL_ALIGNMENT_LEFT, -1, 16, INK)

	# Ship: a small triangle pointing where it's heading
	var heading := Vector2.UP
	if target_port != "":
		heading = ship_pos.direction_to(PORTS[target_port])
	var side := heading.orthogonal()
	var ship := PackedVector2Array([
		ship_pos + heading * 10.0,
		ship_pos - heading * 6.0 + side * 6.0,
		ship_pos - heading * 6.0 - side * 6.0,
	])
	draw_colored_polygon(ship, INK)


func _port_at(pos: Vector2) -> String:
	for port_name in PORTS:
		if pos.distance_to(PORTS[port_name]) < 16.0:
			return port_name
	return ""


func _update_status() -> void:
	var whole_days := int(day)
	var year := START_YEAR + whole_days / (DAYS_PER_SEASON * 4)
	var season: String = SEASONS[(whole_days / DAYS_PER_SEASON) % 4]
	var where := current_port if current_port != "" else "At sea → " + target_port
	status.text = "%s, %d — Day %d\n%s" % [season, year, whole_days + 1, where]
