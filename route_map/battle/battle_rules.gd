extends RefCounted
## Ship-vs-ship battle rules (same as the paper prototype): Hull / Crew / Morale, range FAR or CLOSE,
## seven moves for you, a d6 table for the raiders. Pure logic: the screen calls these and shows the log.

const MOVES := [
	{"id": "arrows", "name": "Loose Arrows", "close": false, "tip": "4–5: their crew −1 · 6: −2"},
	{"id": "spears", "name": "Spear Volley", "close": true, "tip": "Close only · 3–4: their crew −1 · 5–6: −2"},
	{"id": "board", "name": "Board", "close": true, "tip": "Close only · d6 + crew each side; win by 4+ and they break"},
	{"id": "shield", "name": "Shield Wall", "close": false, "tip": "Their hits on your crew −1, +2 to your boarding, until your next move"},
	{"id": "rally", "name": "Rally", "close": false, "tip": "1–2: morale +1 · 3–6: +2"},
	{"id": "row", "name": "Row Hard", "close": false, "tip": "Flee · escape on 3+ at range, 5+ up close (−1 if crew under 4)"},
	{"id": "tribute", "name": "Pay Tribute", "close": false, "tip": "15 silver, or your two best lots · ends the fight"},
]

# Raider strength by hazard severity.
const RAIDERS := {1: {"hull": 5, "crew": 5, "morale": 4}, 2: {"hull": 6, "crew": 7, "morale": 5}, 3: {"hull": 8, "crew": 9, "morale": 6}}
const MAX_MORALE := 5
## Your crew gives up at this many men or fewer (the raiders break at 2 too).
const SURRENDER_AT := 2
## Morale lost per crewman lost, capped per round. 0 = the prototype rule (1 per round, however many fall).
const MORALE_PER_MAN_CAP := 0
const TRIBUTE := 15

var state            # game_state.gd (your ship's hull, crew, morale, silver, hold)
var them := {}
var close := false
var shield := false
var outcome := ""    # "", won, sank_them, surrender, captured, sunk, fled, tribute
var lines: Array = []
var last_roll := 0
var _lost_me := false
var _lost_them := false


func setup(game_state, severity: int, start_close := false) -> void:
	state = game_state
	them = RAIDERS[clampi(severity, 1, 3)].duplicate()
	them["max_hull"] = them["hull"]
	them["max_crew"] = them["crew"]
	them["max_morale"] = them["morale"]
	close = start_close
	state.morale = MAX_MORALE
	lines.clear()
	outcome = ""


func _roll() -> int:
	last_roll = state.roll()
	return last_roll


func valid(id: String) -> bool:
	for m in MOVES:
		if m["id"] == id:
			if m["close"] and not close:
				return false
			if id == "tribute" and state.silver < TRIBUTE and state.hold.is_empty():
				return false
			return outcome == ""
	return false


func _say(t: String) -> void:
	lines.append(t)


func _hurt_me(n: int) -> void:
	if n <= 0:
		return
	state.crew = maxi(0, state.crew - n)
	state.morale = maxi(0, state.morale - _morale_hit(n, _lost_me))
	_lost_me = true


func _hurt_them(n: int) -> void:
	if n <= 0:
		return
	them["crew"] = maxi(0, them["crew"] - n)
	them["morale"] = maxi(0, them["morale"] - _morale_hit(n, _lost_them))
	_lost_them = true


func _morale_hit(n: int, already: bool) -> int:
	if MORALE_PER_MAN_CAP <= 0:
		return 0 if already else 1
	return mini(n, MORALE_PER_MAN_CAP) if not already else 0


## One full round: your move, then theirs. `pay_with_cargo` only matters for tribute.
func play_round(id: String, pay_with_cargo := false) -> void:
	if not valid(id):
		return
	_lost_me = false
	_lost_them = false
	shield = false
	_player(id, pay_with_cargo)
	if outcome == "":
		outcome = _check()
	if outcome == "":
		_raiders()
		outcome = _check()
	if outcome != "":
		_say(_ending())


func _player(id: String, cargo: bool) -> void:
	match id:
		"arrows":
			var r := _roll()
			var n := 2 if r == 6 else (1 if r >= 4 else 0)
			_hurt_them(n)
			_say("You loose arrows (%d): %s." % [r, "their crew −%d" % n if n > 0 else "no hits"])
		"spears":
			var r := _roll()
			var n := 2 if r >= 5 else (1 if r >= 3 else 0)
			_hurt_them(n)
			_say("Spear volley (%d): %s." % [r, "their crew −%d" % n if n > 0 else "no hits"])
		"board":
			_say("You board them!")
			if _boarding() >= 4:
				_say("They break before your charge.")
				outcome = "won"
		"shield":
			shield = true
			_say("Shields up, rims overlapping.")
		"rally":
			var r := _roll()
			var add := 1 if r <= 2 else 2
			state.morale = mini(MAX_MORALE, state.morale + add)
			_say("You rally the crew (%d): morale +%d." % [r, add])
		"row":
			var r := _roll()
			var need := 5 if close else 3
			var mod := -1 if state.crew < 4 else 0
			if r + mod >= need:
				_say("You row hard (%d) and pull away." % r)
				outcome = "fled"
			else:
				_say("You row hard (%d), but they keep pace." % r)
		"tribute":
			if not cargo and state.silver >= TRIBUTE:
				state.silver -= TRIBUTE
				_say("You pay %d silver." % TRIBUTE)
			else:
				var lots: Array = state.lots_by_value()
				for k in mini(2, lots.size()):
					state.hold.erase(lots[k])
				_say("You hand over your best cargo.")
			outcome = "tribute"


## Returns your total minus theirs.
func _boarding() -> int:
	var mine := _roll()
	var theirs: int = state.roll()
	var a: int = mine + state.crew + (2 if shield else 0)
	var b: int = theirs + them["crew"]
	if a > b:
		_hurt_them(mini(3, a - b))
		_say("Boarding %d vs %d: they lose %d crew." % [a, b, mini(3, a - b)])
	elif b > a:
		_hurt_me(mini(3, b - a))
		_say("Boarding %d vs %d: you lose %d crew." % [a, b, mini(3, b - a)])
	else:
		_hurt_me(1)
		_hurt_them(1)
		_say("Boarding %d vs %d: both sides lose a man." % [a, b])
	return a - b


func _raiders() -> void:
	var r: int = state.roll()
	var soak := 1 if shield else 0
	match r:
		1:
			_say("They jeer and circle.")
		2:
			var n := maxi(0, 1 - soak)
			_hurt_me(n)
			_say("Arrow storm: your crew −%d." % n)
		3, 5:
			if close:
				var n := maxi(0, 2 - soak)
				_hurt_me(n)
				_say("Their spears fly: your crew −%d." % n)
			else:
				close = true
				_say("They close in.")
		4:
			close = true
			state.hull = maxi(0, state.hull - 2)
			them["hull"] = maxi(0, them["hull"] - 1)
			_say("They ram you! Your hull −2, theirs −1.")
		6:
			if close:
				_say("They board you!")
				_boarding()
			else:
				close = true
				state.morale = maxi(0, state.morale - 1)
				_say("They close in with a war cry: your morale −1.")


func _check() -> String:
	if state.hull <= 0:
		return "sunk"
	if state.crew <= 0:
		return "captured"
	if state.crew <= SURRENDER_AT:
		return "surrender"
	if them["hull"] <= 0:
		return "sank_them"
	if them["morale"] <= 0 or them["crew"] <= 2:
		return "won"
	if state.morale <= 0:
		return "surrender"
	return ""


func _ending() -> String:
	match outcome:
		"won":
			return "The raiders break off and flee."
		"sank_them":
			return "Their ship founders and goes down."
		"surrender":
			return "Your crew throws down its weapons."
		"captured":
			return "Your last man falls."
		"sunk":
			return "Your ship goes down under their ram."
		"fled":
			return "You lose them in the haze."
		"tribute":
			return "They take their price and row away laughing."
	return ""
