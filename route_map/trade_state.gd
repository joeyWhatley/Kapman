extends RefCounted
## Route map trade state: silver, debt, ship, hold and market rules.
## Forked from scripts/game_state.gd (the paper prototype stays frozen). Port data comes from the
## Port nodes in the route map scene: ports[name] = {"fac", "prices", "swords"}.

const D := preload("res://scripts/data.gd")

var rng := RandomNumberGenerator.new()
var ports := {}           # set by the route map from its Port nodes
var home := "Dorestad"

var day := 1
var silver := 0
var debt := 0
var hull := 0
var crew := 0
var morale := 0
var port := ""            # "" while at sea
var hold: Array = []      # {"good": int, "paid": int, "from": String}
var marks := {}           # port -> Array of 6 ints (pencil marks)
var season := 0
var event := -1
var deck: Array = []
var mood := {}            # good -> +/- amount, current port, this visit only
var swords_bought := 0    # this visit
var graced := false

var over := false
var result := ""          # "win", "bitter", "lose"
var result_text := ""
var stats := {}


func new_game(seed_value: int = 0) -> void:
	if seed_value != 0:
		rng.seed = seed_value
	else:
		rng.randomize()
	day = 1
	silver = D.START["silver"]
	debt = D.START["debt"]
	hull = D.START["hull"]
	crew = D.START["crew"]
	morale = D.START["morale"]
	port = home
	hold.clear()
	marks.clear()
	for p in ports:
		marks[p] = [0, 0, 0, 0, 0, 0]
	season = 0
	event = -1
	mood.clear()
	swords_bought = 0
	graced = false
	over = false
	result = ""
	result_text = ""
	stats = {"won": 0, "fled": 0, "paid": 0, "lost": 0, "checks_ok": 0, "caught": 0}
	deck = []
	for i in D.EVENTS.size():
		deck.append(i)
	_shuffle(deck)


func roll() -> int:
	return rng.randi_range(1, 6)


func _shuffle(a: Array) -> void:
	for i in range(a.size() - 1, 0, -1):
		var j := rng.randi_range(0, i)
		var t = a[i]
		a[i] = a[j]
		a[j] = t


# --- Calendar & debt -------------------------------------------------------

func season_for(d: int) -> int:
	return mini(3, floori(float(d - 1) / D.SEASON_LENGTH))


func deadline() -> int:
	return D.START["grace_due"] if graced else D.START["due"]


func new_season_due() -> bool:
	return season_for(day) != season


## Starts the season the day marker is now in. Returns the interest added.
func begin_season() -> int:
	season = season_for(day)
	var added := ceili(debt * D.INTEREST)
	debt += added
	for p in marks:
		marks[p] = [0, 0, 0, 0, 0, 0]
	draw_event()
	return added


func draw_event() -> int:
	if deck.is_empty():
		for i in D.EVENTS.size():
			if i != event:
				deck.append(i)
		_shuffle(deck)
	event = deck.pop_back()
	return event


func peek_event() -> int:
	if deck.is_empty():
		return -1
	return int(deck.back())


func next_interest() -> int:
	return ceili(debt * D.INTEREST)


## Pays from silver; any shortfall goes on the debt with a penalty. Returns a note ("" if paid in full).
func charge(amount: int) -> String:
	if silver >= amount:
		silver -= amount
		return ""
	var short := amount - silver
	silver = 0
	debt += short + D.COSTS["short_penalty"]
	return "Short %ds: %ds added to the debt." % [short, short + D.COSTS["short_penalty"]]


func pay_debt(amount: int) -> int:
	if port != home or over:
		return 0
	var paid := mini(amount, mini(silver, debt))
	silver -= paid
	debt -= paid
	if debt == 0:
		if graced:
			finish("bitter", "You clear the debt during the reeve's grace. The ship is yours, barely.")
		else:
			finish("win", "You lay the last silver on the reeve's table. The ship is yours, free and clear.")
	return paid


func grace_half() -> int:
	return ceili(debt / 2.0)


func take_grace() -> void:
	var half := grace_half()
	silver -= half
	debt = ceili((debt - half) * 1.5)
	graced = true


func finish(kind: String, text: String) -> void:
	if over:
		return
	over = true
	result = kind
	result_text = text


# --- Prices & trade --------------------------------------------------------

func _applies(where: String, p: String) -> bool:
	return where == "*" or where == p or where == ports[p]["fac"]


func modifiers(p: String, g: int) -> int:
	var total := 0
	var lists: Array = [D.SEASONS[season]["prices"]]
	if event >= 0:
		lists.append(D.EVENTS[event]["prices"])
	for list in lists:
		for m in list:
			if _applies(m[0], p) and g in m[1]:
				total += int(m[2])
	return total


## Current price of one lot, or -1 if not traded at that port.
func price(p: String, g: int) -> int:
	var base: int = ports[p]["prices"][g]
	if base < 0:
		return -1
	var v: int = base + int(marks[p][g]) + modifiers(p, g)
	if p == port:
		v += int(mood.get(g, 0))
	return maxi(1, v)


func count_good(g: int) -> int:
	var n := 0
	for lot in hold:
		if lot["good"] == g:
			n += 1
	return n


func has_swords() -> bool:
	return count_good(D.SWORDS) > 0


func goods_in_hold() -> Array:
	var out: Array = []
	for lot in hold:
		if not lot["good"] in out:
			out.append(lot["good"])
	return out


## "" if allowed, otherwise the reason.
func why_not_buy(g: int) -> String:
	if over:
		return "The game is over."
	if port == "":
		return "At sea."
	var pr := price(port, g)
	if pr < 0:
		return "Not traded here."
	if g == D.SWORDS and ports[port]["swords"] != "buy":
		return "Swords can't be bought here."
	if g == D.SWORDS and swords_bought >= D.SWORD_LIMIT:
		return "Max %d lots of swords per visit." % D.SWORD_LIMIT
	if hold.size() >= D.START["hold"]:
		return "Hold is full."
	if silver < pr:
		return "Not enough silver."
	return ""


func why_not_sell(g: int) -> String:
	if over:
		return "The game is over."
	if port == "":
		return "At sea."
	if price(port, g) < 0:
		return "Not traded here."
	if g == D.SWORDS and ports[port]["swords"] != "sell":
		return "Swords can't be sold at a Frankish port."
	if count_good(g) == 0:
		return "None in your hold."
	return ""


func buy(g: int) -> int:
	var pr := price(port, g)
	silver -= pr
	hold.append({"good": g, "paid": pr, "from": port})
	marks[port][g] += 1
	if g == D.SWORDS:
		swords_bought += 1
	return pr


## Sells the cheapest-bought lot of that good. Returns [price, paid].
func sell(g: int) -> Array:
	var pr := price(port, g)
	var best := -1
	for i in hold.size():
		if hold[i]["good"] == g and (best < 0 or hold[i]["paid"] < hold[best]["paid"]):
			best = i
	var paid: int = hold[best]["paid"]
	hold.remove_at(best)
	silver += pr
	marks[port][g] -= 1
	return [pr, paid]


func remove_lot(g: int) -> void:
	for i in hold.size():
		if hold[i]["good"] == g:
			hold.remove_at(i)
			return


func swap_lot(give: int, take: int) -> void:
	for lot in hold:
		if lot["good"] == give:
			lot["good"] = take
			lot["from"] = "a Frisian trader"
			return


func roll_mood(r1: int, r2: int) -> void:
	mood.clear()
	if r1 != r2:
		mood[r1 - 1] = D.MOOD_SWING
		mood[r2 - 1] = -D.MOOD_SWING


## Most valuable lots first (by what you paid).
func lots_by_value() -> Array:
	var lots := hold.duplicate()
	lots.sort_custom(func(x, y): return x["paid"] > y["paid"])
	return lots


# --- Ship ------------------------------------------------------------------

func why_not_repair() -> String:
	if port == "" or over:
		return "Only in port."
	if hull >= D.START["hull"]:
		return "Hull is sound."
	if silver < D.COSTS["repair"]:
		return "Not enough silver."
	return ""


func why_not_hire() -> String:
	if port == "" or over:
		return "Only in port."
	if crew >= D.START["crew_max"]:
		return "No room for more crew."
	if silver < D.COSTS["hire"]:
		return "Not enough silver."
	return ""


func repair() -> void:
	silver -= D.COSTS["repair"]
	hull += 1


func hire() -> void:
	silver -= D.COSTS["hire"]
	crew += 1


# --- Routes & voyages ------------------------------------------------------

func route(a: String, b: String) -> Dictionary:
	for r in D.ROUTES:
		if (r["a"] == a and r["b"] == b) or (r["a"] == b and r["b"] == a):
			return r
	return {}


func _same(r: Dictionary, a: String, b: String) -> bool:
	return (r["a"] == a and r["b"] == b) or (r["a"] == b and r["b"] == a)


func is_closed(r: Dictionary) -> bool:
	if event < 0:
		return false
	for pair in D.EVENTS[event]["close"]:
		if _same(r, pair[0], pair[1]):
			return true
	return false


## Highest encounter roll that triggers an encounter on this route (0 = none).
func danger(r: Dictionary) -> int:
	if r["type"] == "land":
		return 0
	var d := 1 if r["type"] == "coastal" else 2
	if season == 3 and r["type"] == "open":
		d = 3
	if season == 1:
		d += 1
	if event >= 0:
		var e: Dictionary = D.EVENTS[event]
		for x in e["danger"]:
			if _same(r, x[0], x[1]):
				d += int(x[2])
		if e["danger_port"] != "" and (r["a"] == e["danger_port"] or r["b"] == e["danger_port"]):
			d += 1
	return mini(d, 6)


func voyage_days(r: Dictionary) -> int:
	var d: int = r["days"]
	if crew < 4:
		d += 1
	if season == 3 and r["type"] != "land":
		d += 1
	return d


func upkeep(days: int, r: Dictionary) -> int:
	var cost: int = days * D.COSTS["upkeep_day"] + maxi(0, crew - 6) * D.COSTS["extra_crew"]
	if r.get("type", "") == "land":
		cost += hold.size() * D.COSTS["cart"]
	return cost


func caught_on(bribe: bool) -> int:
	var n: int = D.CAUGHT_ON
	if event >= 0:
		n += int(D.EVENTS[event]["caught_bonus"])
	if bribe:
		n -= 1
	return maxi(1, n)


## Swords seized and the fine paid (or added to the debt). Returns lots lost.
func confiscate() -> int:
	var n := count_good(D.SWORDS)
	hold = hold.filter(func(lot): return lot["good"] != D.SWORDS)
	if silver >= D.COSTS["fine"]:
		silver -= D.COSTS["fine"]
	else:
		debt += D.COSTS["fine"]
	return n
