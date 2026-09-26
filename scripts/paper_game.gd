extends Control
## Kāpman — the paper game, played on a desk.
## Same rules and numbers as design/paper_prototype; the computer rolls the die and keeps the books.

const D := preload("res://scripts/data.gd")
const GameState := preload("res://scripts/game_state.gd")
const Ink := preload("res://scripts/ui/ink.gd")
const Paper := preload("res://scripts/ui/paper.gd")
const MapSheet := preload("res://scripts/ui/map_sheet.gd")
const MarketSheet := preload("res://scripts/ui/market_sheet.gd")
const TrackerSheet := preload("res://scripts/ui/tracker_sheet.gd")
const BattleSheet := preload("res://scripts/ui/battle_sheet.gd")
const RulesSheet := preload("res://scripts/ui/rules_sheet.gd")
const CardView := preload("res://scripts/ui/card_view.gd")
const DieView := preload("res://scripts/ui/die_view.gd")
const NoteOverlay := preload("res://scripts/ui/note_overlay.gd")

const TABS := [["market", "Market"], ["tracker", "Tracker"], ["battle", "Battle"], ["rules", "Rules"]]

## For automated test runs: no animations, random choices.
var autopilot := false

var state := GameState.new()
var busy := false
var battle := {}
var _ending := false

var map_sheet
var market_sheet
var tracker_sheet
var battle_sheet
var rules_sheet
var season_card
var event_card
var die
var note
var log_box: RichTextLabel
var sheets := {}
var tab_buttons := {}


func _ready() -> void:
	set_anchors_preset(Control.PRESET_FULL_RECT)
	theme = Ink.make_theme()
	resized.connect(queue_redraw)
	_build()
	new_game()


func _build() -> void:
	map_sheet = _place(MapSheet.new(), Vector2(24, 26), Vector2(820, 846))
	map_sheet.port_clicked.connect(_on_port_clicked)

	for i in TABS.size():
		var b := Button.new()
		b.text = TABS[i][1]
		b.position = Vector2(868 + i * 106, 20)
		b.size = Vector2(100, 32)
		b.pressed.connect(show_tab.bind(TABS[i][0]))
		add_child(b)
		tab_buttons[TABS[i][0]] = b

	market_sheet = _place(MarketSheet.new(), Vector2(868, 56), Vector2(708, 598))
	market_sheet.buy.connect(_on_buy)
	market_sheet.sell.connect(_on_sell)
	tracker_sheet = _place(TrackerSheet.new(), Vector2(868, 56), Vector2(708, 598))
	tracker_sheet.action.connect(_on_tracker_action)
	battle_sheet = _place(BattleSheet.new(), Vector2(868, 56), Vector2(708, 598))
	rules_sheet = _place(RulesSheet.new(), Vector2(868, 56), Vector2(708, 598))
	sheets = {"market": market_sheet, "tracker": tracker_sheet, "battle": battle_sheet, "rules": rules_sheet}

	season_card = _place(CardView.new(), Vector2(868, 672), Vector2(150, 206))
	season_card.rotation = -0.02
	event_card = _place(CardView.new(), Vector2(1026, 672), Vector2(190, 206))
	event_card.rotation = 0.015
	die = _place(DieView.new(), Vector2(1224, 672), Vector2(92, 206))

	var log_paper = _place(Paper.new(), Vector2(1324, 672), Vector2(252, 206))
	log_paper.title = "Ship's log"
	log_box = RichTextLabel.new()
	log_box.position = Vector2(12, 50)
	log_box.size = Vector2(230, 148)
	log_box.scroll_following = true
	log_box.add_theme_font_override("normal_font", Ink.pencil())
	log_box.add_theme_font_size_override("normal_font_size", 11)
	log_box.add_theme_color_override("default_color", Ink.PENCIL)
	log_paper.add_child(log_box)

	note = NoteOverlay.new()
	add_child(note)


func _place(node: Control, pos: Vector2, sz: Vector2) -> Control:
	node.position = pos
	node.size = sz
	if "state" in node:
		node.state = state
	add_child(node)
	return node


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, size), Ink.DESK)
	var grain := RandomNumberGenerator.new()
	grain.seed = 7
	for i in 80:
		var y := grain.randf() * size.y
		draw_line(Vector2(0, y), Vector2(size.x, y + grain.randf_range(-24, 24)), Color(0, 0, 0, 0.07), grain.randf_range(1.0, 3.0))


# --- Screen upkeep ---------------------------------------------------------

func show_tab(id: String) -> void:
	for k in sheets:
		sheets[k].visible = k == id
		tab_buttons[k].modulate = Color.WHITE if k == id else Color(1, 1, 1, 0.7)


func refresh() -> void:
	map_sheet.busy = busy
	market_sheet.busy = busy
	tracker_sheet.busy = busy
	battle_sheet.battle = battle
	map_sheet.queue_redraw()
	market_sheet.refresh()
	tracker_sheet.refresh()
	battle_sheet.refresh()
	var s: Dictionary = D.SEASONS[state.season]
	season_card.show_card("Season", s["name"], s["days"], s["flavor"], s["lines"])
	if state.event >= 0:
		var e: Dictionary = D.EVENTS[state.event]
		event_card.show_card("Event · this season", e["name"], e["year"], e["flavor"], e["lines"])
	tab_buttons["battle"].text = "Battle!" if not battle.is_empty() else "Battle"


func log_line(text: String) -> void:
	log_box.add_text(text + "\n")


func _blog(text: String) -> void:
	battle_sheet.add_line(text)
	log_line("  " + text)


func ask(title: String, text: String, options: Array) -> int:
	if autopilot:
		return 0
	note.ask(title, text, options)
	var i: int = await note.chosen
	return i


func roll(what: String) -> int:
	var r := state.roll()
	if autopilot:
		die.show_face(r, what)
	else:
		await die.animate(r, what)
	log_line("   d6 %s: %d" % [what, r])
	return r


func _move_ship(to: Vector2, seconds: float) -> void:
	if autopilot:
		map_sheet.ship_pos = to
		return
	var tw := create_tween()
	tw.tween_property(map_sheet, "ship_pos", to, seconds).set_trans(Tween.TRANS_SINE)
	await tw.finished


static func _upto(n: int) -> String:
	return "1" if n <= 1 else "1–%d" % n


# --- Game flow -------------------------------------------------------------

func new_game(seed_value: int = 0) -> void:
	state.new_game(seed_value)
	busy = true
	_ending = false
	battle = {}
	battle_sheet.lines.clear()
	log_box.clear()
	map_sheet.sailing_to = ""
	map_sheet.ship_pos = map_sheet.dock_point(D.HOME)
	show_tab("market")
	state.draw_event()
	refresh()
	log_line("Spring, day 1. You owe the reeve %ds." % state.debt)
	log_line("Event: %s." % D.EVENTS[state.event]["name"])
	await ask("Kāpman",
		"c. 790. Frisia lies under Frankish rule. You borrowed %ds from the Count's reeve at Dorestad to buy a small trading ship.\n\nPay him back in full, in Dorestad, by day %d, or lose the ship.\n\nClick a port joined to yours on the map to sail. Trade on the Market sheet. The die is rolled for you." % [state.debt, D.START["due"]],
		["Cast off"])
	if D.EVENTS[state.event]["burn"]:
		await _dorestad_burns()
	await _roll_mood()
	busy = false
	refresh()


func _on_port_clicked(p: String) -> void:
	if busy or state.over or state.port == "" or p == state.port:
		return
	var r: Dictionary = state.route(state.port, p)
	if r.is_empty():
		log_line("No direct route from %s to %s." % [state.port, p])
		return
	if state.is_closed(r):
		log_line("The %s–%s route is closed this season." % [state.port, p])
		return
	sail_to(p)


func _on_buy(g: int) -> void:
	if busy or state.why_not_buy(g) != "":
		return
	var pr: int = state.buy(g)
	log_line("Bought 1 %s for %ds." % [D.GOODS[g], pr])
	refresh()


func _on_sell(g: int) -> void:
	if busy or state.why_not_sell(g) != "":
		return
	var res: Array = state.sell(g)
	log_line("Sold 1 %s for %ds (paid %d, %+d)." % [D.GOODS[g], res[0], res[1], res[0] - res[1]])
	refresh()


func _on_tracker_action(id: String) -> void:
	if busy or state.over:
		return
	match id:
		"pay10", "payall":
			var paid: int = state.pay_debt(10 if id == "pay10" else state.silver)
			log_line("Paid the reeve %ds. Debt now %ds." % [paid, state.debt])
		"repair":
			if state.why_not_repair() == "":
				state.repair()
				log_line("Repaired the hull (3s). Hull %d." % state.hull)
		"hire":
			if state.why_not_hire() == "":
				state.hire()
				log_line("Hired a crewman (5s). Crew %d." % state.crew)
		"wait":
			await wait_day()
	refresh()
	if state.over:
		_game_over()


func wait_day() -> void:
	if not await _deadline_ok(1, state.port):
		return
	busy = true
	refresh()
	state.day += 1
	var short: String = state.charge(D.COSTS["wait"])
	log_line("Day %d: you wait a day in %s (1s)." % [state.day, state.port])
	if short != "":
		log_line(short)
	await _season_check()
	await _roll_mood()
	busy = false
	refresh()


## Checks the reeve's deadline before anything that moves the day marker. False = don't go.
func _deadline_ok(days: int, dest: String) -> bool:
	if state.debt <= 0:
		return true
	var dl: int = state.deadline()
	var arrive: int = state.day + days
	if state.port == D.HOME and not state.graced and arrive > dl:
		var half: int = state.grace_half()
		var text := "The reeve's day has come: day %d, and you still owe %ds.\n\n" % [dl, state.debt]
		var opts: Array = []
		if state.silver >= half:
			text += "Pay half now (%ds) and he grants grace: the rest ×1.5 is due by day %d." % [half, D.START["grace_due"]]
			opts = ["Pay half and take the grace", "Not yet: keep trading here", "Give up the ship"]
		else:
			text += "You can't pay even half (%ds). Sell cargo first, or give up the ship." % half
			opts = ["Not yet: keep trading here", "Give up the ship"]
		var i: int = await ask("The reeve", text, opts)
		var pick: String = opts[i]
		if pick.begins_with("Pay half"):
			state.take_grace()
			log_line("Grace: you pay %ds. The reeve writes %ds against you, due day %d." % [half, state.debt, D.START["grace_due"]])
			refresh()
			return arrive <= state.deadline()
		if pick == "Give up the ship":
			state.finish("lose", "You hand the reeve your ship. You'll work off the rest in his fields.")
			_game_over()
		return false
	if arrive > dl or (arrive >= dl and dest != D.HOME):
		var why := "That would bring you to day %d. The reeve expects you in Dorestad with his silver by the end of day %d." % [arrive, dl]
		var j: int = await ask("Past the reeve's day", why, ["Stay here", "Give up the ship"])
		if j == 1:
			state.finish("lose", "You never go back to Dorestad. The reeve takes your ship in your absence.")
			_game_over()
		return false
	return true


func sail_to(dest: String) -> void:
	var r: Dictionary = state.route(state.port, dest)
	var days: int = state.voyage_days(r)
	if not await _deadline_ok(days, dest):
		return
	busy = true
	refresh()
	var from: String = state.port
	if state.has_swords() and D.PORTS[from]["fac"] == "F":
		await _contraband("Leaving %s" % from)
	log_line("Day %d: sail %s → %s, %d days." % [state.day, from, dest, days])
	state.port = ""
	state.mood.clear()
	state.swords_bought = 0
	map_sheet.sailing_to = dest
	refresh()
	var a: Vector2 = map_sheet.port_point(from)
	var b: Vector2 = map_sheet.port_point(dest)
	await _move_ship(a.lerp(b, 0.5), 0.7)
	state.day += days
	refresh()
	var extra := 0
	var dg: int = state.danger(r)
	if dg > 0:
		var e: int = await roll("Encounter? (%s)" % _upto(dg))
		if e <= dg:
			extra = await _encounter()
		else:
			log_line("Fair winds. Nothing sighted.")
	if not state.over:
		await _move_ship(map_sheet.dock_point(dest), 0.7)
		map_sheet.sailing_to = ""
		await _arrive(dest, days + extra, r)
	busy = false
	refresh()
	if state.over:
		_game_over()


func _arrive(dest: String, days: int, r: Dictionary) -> void:
	state.port = dest
	var cost: int = state.upkeep(days, r)
	var short: String = state.charge(cost)
	log_line("Day %d: arrive at %s. Upkeep %ds." % [state.day, dest, cost])
	if short != "":
		log_line(short)
	var dl: int = state.deadline()
	if state.debt > 0 and (state.day > dl or (state.day >= dl and dest != D.HOME)):
		if dest == D.HOME:
			state.finish("lose", "You reach Dorestad on day %d, too late. The reeve's day was %d, and he has already taken your ship." % [state.day, dl])
		else:
			state.finish("lose", "Day %d passes with you in %s and the reeve unpaid. He calls it flight and takes your ship." % [dl, dest])
		return
	if state.has_swords() and D.PORTS[dest]["fac"] == "F":
		await _contraband("Entering %s" % dest)
	await _season_check()
	state.morale = D.START["morale"]
	await _roll_mood()


func _season_check() -> void:
	if not state.new_season_due():
		return
	var added: int = state.begin_season()
	var s: Dictionary = D.SEASONS[state.season]
	var e: Dictionary = D.EVENTS[state.event]
	log_line("— %s begins. Interest +%ds, debt %ds. Marks erased." % [s["name"], added, state.debt])
	log_line("Event: %s (%s)." % [e["name"], e["year"]])
	refresh()
	await ask(s["name"],
		"%s\n\nThe reeve adds his interest: +%ds (debt now %ds). The market marks are erased.\n\nNew event: %s (%s). %s" % [s["flavor"], added, state.debt, e["name"], e["year"], e["flavor"]],
		["Go on"])
	if e["burn"] and state.port == D.HOME:
		await _dorestad_burns()


func _dorestad_burns() -> void:
	var r: int = await roll("Dorestad burns: 1–3 lose a lot")
	if r > 3:
		log_line("The fire spares your cargo.")
		return
	if state.hold.is_empty():
		log_line("Fire in the warehouses, but your hold is empty.")
		return
	var goods: Array = state.goods_in_hold()
	var i: int = await ask("Dorestad burns", "Fire in the warehouses. Choose one lot to lose.", goods.map(func(g): return D.GOODS[g]))
	state.remove_lot(goods[i])
	log_line("Lost 1 %s to the fire." % D.GOODS[goods[i]])
	refresh()


func _roll_mood() -> void:
	var r1: int = await roll("Mood: in demand")
	var r2: int = await roll("Mood: glut")
	state.roll_mood(r1, r2)
	if r1 == r2:
		log_line("Market mood in %s: steady." % state.port)
	else:
		log_line("Market mood in %s: %s +%d, %s −%d." % [state.port, D.GOODS[r1 - 1], D.MOOD_SWING, D.GOODS[r2 - 1], D.MOOD_SWING])
	refresh()


func _contraband(reason: String) -> void:
	var bribe := false
	if state.silver >= D.COSTS["bribe"]:
		var text := "%s with swords aboard. Caught on %s.\n\nBribe the inspector %ds to be caught only on %s?" % [reason, _upto(state.caught_on(false)), D.COSTS["bribe"], _upto(state.caught_on(true))]
		var i: int = await ask("Contraband check", text, ["Bribe (%ds)" % D.COSTS["bribe"], "Take your chances"])
		if i == 0:
			bribe = true
			state.silver -= D.COSTS["bribe"]
			log_line("You slip the inspector %ds." % D.COSTS["bribe"])
	var mx: int = state.caught_on(bribe)
	var r: int = await roll("Contraband (caught on %s)" % _upto(mx))
	if r <= mx:
		var n: int = state.confiscate()
		state.stats["caught"] += 1
		log_line("Caught! %d lots of swords seized and a %ds fine." % [n, D.COSTS["fine"]])
		refresh()
		await ask("Caught!", "%s: the swords are found. %d lots seized, and a %ds fine." % [reason, n, D.COSTS["fine"]], ["Curse the Franks"])
	else:
		state.stats["checks_ok"] += 1
		log_line("The inspector looks the other way.")


## Resolves an encounter. Returns extra days lost.
func _encounter() -> int:
	var e: int = await roll("Encounter table")
	var winter: bool = state.season == 3
	if e <= 3 and winter:
		log_line("Sails? No, storm clouds. The raiders stay home in winter.")
		e = 4
	if e <= 3:
		return await _battle()
	if e == 4:
		var dmg := 2 if winter else 1
		state.hull = maxi(0, state.hull - dmg)
		state.day += 1
		log_line("Storm! Hull −%d, and a day lost." % dmg)
		refresh()
		if state.hull <= 0:
			state.finish("lose", "The storm breaks your ship apart. The sea keeps you.")
		else:
			await ask("Storm", "The sea rises. Hull −%d, and a day lost riding it out." % dmg, ["Bail and pray"])
		return 1
	if e == 5:
		if state.has_swords():
			await _contraband("A Frankish patrol boards you")
		else:
			log_line("A Frankish patrol looks you over and waves you on.")
		return 0
	await _trader()
	return 0


func _trader() -> void:
	var nxt: int = state.peek_event()
	var rumour := "They have no news worth the telling."
	if nxt >= 0:
		rumour = "They swear the next season brings: %s (%s)." % [D.EVENTS[nxt]["name"], D.EVENTS[nxt]["year"]]
	log_line("A Frisian trader hails you. " + rumour)
	if state.hold.is_empty():
		await ask("A Frisian trader", rumour, ["Sail on"])
		return
	var i: int = await ask("A Frisian trader", rumour + "\n\nThey'll swap one lot of your cargo for one of theirs.", ["Swap a lot", "Sail on"])
	if i != 0:
		return
	var mine: Array = state.goods_in_hold()
	var j: int = await ask("Swap which lot?", "", mine.map(func(g): return D.GOODS[g]))
	var give: int = mine[j]
	var others: Array = []
	for g in D.GOODS.size():
		if g != give:
			others.append(g)
	var k: int = await ask("Take which good?", "", others.map(func(g): return D.GOODS[g]))
	state.swap_lot(give, others[k])
	log_line("Swapped 1 %s for 1 %s." % [D.GOODS[give], D.GOODS[others[k]]])
	refresh()


# --- Battle ----------------------------------------------------------------

## Fights Viking raiders. Returns extra days lost.
func _battle() -> int:
	battle = {"hull": D.RAIDERS["hull"], "crew": D.RAIDERS["crew"], "morale": D.RAIDERS["morale"], "close": false, "shield": false, "waiting": false}
	battle_sheet.lines.clear()
	show_tab("battle")
	refresh()
	_blog("Viking raiders! A dragon-prow runs down on you.")
	await ask("Viking raiders!", "A dragon-prow comes out of the haze, oars flashing. Choose your moves on the Battle sheet.", ["To arms"])
	var outcome := ""
	var rounds := 0
	while outcome == "":
		rounds += 1
		var lost := {"me": false, "them": false}
		var mv: String = await _choose_move()
		battle["shield"] = false
		outcome = await _player_move(mv, lost)
		if outcome == "":
			outcome = _battle_end()
		if outcome == "":
			await _raider_move(lost)
			outcome = _battle_end()
		refresh()
		if outcome == "" and rounds >= 40:
			outcome = "fled"
	var extra := 0
	var summary := ""
	match outcome:
		"won":
			var loot: int = (await roll("Loot (×3)")) * 3
			state.silver += loot
			state.stats["won"] += 1
			summary = "The raiders break off and flee. You take %ds in loot." % loot
		"sank_them":
			state.stats["won"] += 1
			summary = "Their ship founders and goes down. No loot, but no raiders."
		"surrender":
			state.hold.clear()
			state.stats["lost"] += 1
			summary = "Your crew throws down its weapons. The raiders strip your hold and let you go."
		"captured":
			state.finish("lose", "Your last crewman falls. The raiders take you north, to be sold as a slave.")
			summary = "Captured."
		"sunk":
			state.finish("lose", "Your ship goes down under the raiders' ram.")
			summary = "Sunk."
		"fled":
			state.day += 1
			extra = 1
			state.stats["fled"] += 1
			summary = "You row until your hands bleed and lose them in the haze. A day lost."
		"tribute":
			state.stats["paid"] += 1
			summary = "The raiders take their price and row away laughing."
	_blog(summary)
	refresh()
	if not state.over:
		await ask("Battle over", summary, ["Sail on"])
	battle = {}
	show_tab("market")
	refresh()
	return extra


func _choose_move() -> String:
	var ok: Array = battle_sheet.valid_moves()
	if autopilot:
		# Test policy: flee while far, fight up close, pay when about to break.
		if not battle["close"]:
			return "row"
		if state.morale <= 1 and "tribute" in ok:
			return "tribute"
		return "spears"
	battle["waiting"] = true
	refresh()
	var mv: String = await battle_sheet.move_chosen
	battle["waiting"] = false
	refresh()
	return mv


func _hurt_me(n: int, lost: Dictionary) -> void:
	if n <= 0:
		return
	state.crew = maxi(0, state.crew - n)
	if not lost["me"]:
		lost["me"] = true
		state.morale = maxi(0, state.morale - 1)


func _hurt_them(n: int, lost: Dictionary) -> void:
	if n <= 0:
		return
	battle["crew"] = maxi(0, battle["crew"] - n)
	if not lost["them"]:
		lost["them"] = true
		battle["morale"] = maxi(0, battle["morale"] - 1)


func _player_move(mv: String, lost: Dictionary) -> String:
	match mv:
		"arrows":
			var r: int = await roll("Loose Arrows")
			var n := 2 if r == 6 else (1 if r >= 4 else 0)
			_hurt_them(n, lost)
			_blog("You loose arrows: %s." % ("raider Crew −%d" % n if n > 0 else "no hits"))
		"spears":
			var r: int = await roll("Spear Volley")
			var n := 2 if r >= 5 else (1 if r >= 3 else 0)
			_hurt_them(n, lost)
			_blog("Spear volley: %s." % ("raider Crew −%d" % n if n > 0 else "no hits"))
		"board":
			_blog("You board them!")
			var diff: int = await _boarding(lost)
			if diff >= 4:
				_blog("They break before your charge!")
				return "won"
		"shield":
			battle["shield"] = true
			_blog("Shields up, rims overlapping.")
		"rally":
			var r: int = await roll("Rally")
			var add := 1 if r <= 2 else 2
			state.morale = mini(D.START["morale"], state.morale + add)
			_blog("You rally the crew: Morale +%d." % add)
		"row":
			var r: int = await roll("Row Hard")
			var need := 5 if battle["close"] else 3
			var mod := -1 if state.crew < 4 else 0
			if r + mod >= need:
				return "fled"
			_blog("You row hard, but they keep pace.")
		"tribute":
			var opts: Array = []
			if state.silver >= D.COSTS["tribute"]:
				opts.append("Pay %ds" % D.COSTS["tribute"])
			if not state.hold.is_empty():
				opts.append("Hand over your 2 most valuable lots")
			var i: int = await ask("Pay tribute", "The raiders' chief names his price.", opts)
			if opts[i].begins_with("Pay"):
				state.silver -= D.COSTS["tribute"]
				_blog("You pay %ds." % D.COSTS["tribute"])
			else:
				var lots: Array = state.lots_by_value()
				for k in mini(2, lots.size()):
					state.hold.erase(lots[k])
					_blog("They take your %s." % D.GOODS[lots[k]["good"]])
			return "tribute"
	return ""


## Boarding fight. Returns your total minus theirs.
func _boarding(lost: Dictionary) -> int:
	var mine: int = await roll("Boarding: you")
	var theirs: int = await roll("Boarding: raiders")
	var a: int = mine + state.crew + (2 if battle["shield"] else 0)
	var b: int = theirs + battle["crew"]
	if a > b:
		_hurt_them(mini(3, a - b), lost)
		_blog("Boarding %d vs %d: they lose %d Crew." % [a, b, mini(3, a - b)])
	elif b > a:
		_hurt_me(mini(3, b - a), lost)
		_blog("Boarding %d vs %d: you lose %d Crew." % [a, b, mini(3, b - a)])
	else:
		_hurt_me(1, lost)
		_hurt_them(1, lost)
		_blog("Boarding %d vs %d: both sides lose 1 Crew." % [a, b])
	return a - b


func _raider_spears(lost: Dictionary) -> void:
	var n := maxi(0, 2 - (1 if battle["shield"] else 0))
	_hurt_me(n, lost)
	_blog("Their spears fly: your Crew −%d." % n)


func _raider_move(lost: Dictionary) -> void:
	var r: int = await roll("Raiders act")
	match r:
		1:
			_blog("The raiders jeer and circle.")
		2:
			var n := maxi(0, 1 - (1 if battle["shield"] else 0))
			_hurt_me(n, lost)
			_blog("Arrow storm: your Crew −%d." % n)
		3:
			if battle["close"]:
				_raider_spears(lost)
			else:
				battle["close"] = true
				_blog("They close in.")
		4:
			battle["close"] = true
			state.hull = maxi(0, state.hull - 2)
			battle["hull"] = maxi(0, battle["hull"] - 1)
			_blog("They ram you! Your Hull −2, theirs −1.")
		5:
			if battle["close"]:
				_raider_spears(lost)
			else:
				battle["close"] = true
				_blog("They close in.")
		6:
			if battle["close"]:
				_blog("They board you!")
				await _boarding(lost)
			else:
				battle["close"] = true
				state.morale = maxi(0, state.morale - 1)
				_blog("They close in with a war cry: your Morale −1.")


func _battle_end() -> String:
	if state.hull <= 0:
		return "sunk"
	if state.crew <= 0:
		return "captured"
	if battle["hull"] <= 0:
		return "sank_them"
	if battle["morale"] <= 0 or battle["crew"] <= 2:
		return "won"
	if state.morale <= 0:
		return "surrender"
	return ""


# --- The end ---------------------------------------------------------------

func _game_over() -> void:
	if _ending:
		return
	_ending = true
	busy = true
	refresh()
	var titles := {"win": "Free and clear!", "bitter": "Free, barely.", "lose": "The voyage ends."}
	var s: Dictionary = state.stats
	var text := "%s\n\nDay %d · Silver left %ds · Debt %ds\nBattles: won %d, fled %d, paid tribute %d, lost %d\nContraband checks: passed %d, caught %d" % [
		state.result_text, state.day, state.silver, state.debt, s["won"], s["fled"], s["paid"], s["lost"], s["checks_ok"], s["caught"]]
	log_line("=== %s ===" % titles[state.result])
	if autopilot:
		return
	await ask(titles[state.result], text, ["New voyage"])
	new_game()
