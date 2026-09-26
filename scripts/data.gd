extends RefCounted
## Kāpman paper game: every tunable number lives here.
## v0.1 — same values as design/paper_prototype/kapman_paper_prototype.html.

const GOODS := ["Grain", "Cloth", "Wine", "Furs", "Amber", "Swords"]
const GOOD_NOTES := ["staple", "Frisian cloth", "Rhenish · seasonal", "luxury · winter", "luxury", "CONTRABAND"]
const SWORDS := 5

# fac: F = Frankish (contraband checks, swords can't be sold), E = English, D = Danish.
# prices: base price per lot for each good in GOODS order; -1 = not traded here.
# swords: "buy" (buy only), "sell" (sell only), "none".
const PORTS := {
	"Dorestad":  {"fac": "F", "pos": Vector2(479, 492), "lab": "right", "tag": "HOME · the reeve", "prices": [6, 10, 8, 20, 24, 12], "swords": "buy"},
	"Medemblik": {"fac": "F", "pos": Vector2(468, 396), "lab": "right", "tag": "Frisian cloth",    "prices": [5, 7, 10, 19, 23, -1],  "swords": "none"},
	"Quentovic": {"fac": "F", "pos": Vector2(292, 672), "lab": "right", "tag": "Frankish mint",    "prices": [5, 10, 9, 20, 25, 13],  "swords": "buy"},
	"Lundenwic": {"fac": "E", "pos": Vector2(198, 552), "lab": "left",  "tag": "Mercia",           "prices": [8, 13, 14, 17, 27, 22], "swords": "sell"},
	"Hamwic":    {"fac": "E", "pos": Vector2(130, 624), "lab": "below", "tag": "Wessex",           "prices": [7, 13, 14, 18, 27, 22], "swords": "sell"},
	"Eoforwic":  {"fac": "E", "pos": Vector2(146, 252), "lab": "left",  "tag": "Northumbria",      "prices": [9, 15, 17, 15, 26, 24], "swords": "sell"},
	"Ribe":      {"fac": "D", "pos": Vector2(661, 96),  "lab": "above", "tag": "Danish",           "prices": [9, 13, 15, 13, 19, 28], "swords": "sell"},
	"Hedeby":    {"fac": "D", "pos": Vector2(703, 192), "lab": "below", "tag": "Danish · Baltic",  "prices": [11, 15, 17, 10, 14, 30], "swords": "sell"},
}
const PORT_ORDER := ["Dorestad", "Medemblik", "Quentovic", "Lundenwic", "Hamwic", "Eoforwic", "Ribe", "Hedeby"]
const HOME := "Dorestad"

# type: coastal (encounter on 1), open (1-2), land (no encounters, cart hire per lot)
const ROUTES := [
	{"a": "Dorestad",  "b": "Medemblik", "days": 2, "type": "coastal"},
	{"a": "Dorestad",  "b": "Quentovic", "days": 3, "type": "coastal"},
	{"a": "Dorestad",  "b": "Lundenwic", "days": 4, "type": "open"},
	{"a": "Medemblik", "b": "Ribe",      "days": 4, "type": "coastal"},
	{"a": "Medemblik", "b": "Eoforwic",  "days": 5, "type": "open"},
	{"a": "Quentovic", "b": "Lundenwic", "days": 2, "type": "open"},
	{"a": "Quentovic", "b": "Hamwic",    "days": 3, "type": "open"},
	{"a": "Lundenwic", "b": "Hamwic",    "days": 3, "type": "coastal"},
	{"a": "Lundenwic", "b": "Eoforwic",  "days": 4, "type": "coastal"},
	{"a": "Eoforwic",  "b": "Ribe",      "days": 6, "type": "open"},
	{"a": "Ribe",      "b": "Hedeby",    "days": 2, "type": "land"},
]

const START := {"silver": 30, "debt": 100, "hull": 8, "crew": 6, "crew_max": 8, "morale": 5, "hold": 6, "due": 60, "grace_due": 75}
const COSTS := {"upkeep_day": 1, "extra_crew": 1, "repair": 3, "hire": 5, "wait": 1, "bribe": 5, "fine": 10, "tribute": 15, "cart": 1, "short_penalty": 2}
const RAIDERS := {"hull": 6, "crew": 7, "morale": 5}
const SEASON_LENGTH := 15
const INTEREST := 0.10
const MOOD_SWING := 3
const SWORD_LIMIT := 3
const CAUGHT_ON := 2

# prices: [where, goods, amount]; where = "*", a faction letter, or a port name.
const SEASONS := [
	{"name": "Spring", "days": "Days 1–15", "flavor": "The sea opens. Granaries are nearly empty.",
		"lines": ["Grain +2 at every port.", "Encounters: normal danger."],
		"prices": [["*", [0], 2]]},
	{"name": "Summer", "days": "Days 16–30", "flavor": "Raiding season. The dragon-ships are out.",
		"lines": ["Interest +10% of debt.", "Danger +1 on every sea route.", "Furs −2 at every port."],
		"prices": [["*", [3], -2]]},
	{"name": "Autumn", "days": "Days 31–45", "flavor": "Harvest and vintage. Barns full, casks new.",
		"lines": ["Interest +10% of debt.", "Grain −2 at every port.", "Wine −3 at Frankish ports."],
		"prices": [["*", [0], -2], ["F", [2], -3]]},
	{"name": "Winter", "days": "Days 46–60 · DEBT DUE", "flavor": "Storms and short days. The raiders have gone home to their halls.",
		"lines": ["Interest +10% of debt.", "Sea voyages +1 day.", "Open sea danger 1–3; raiders become storms (Hull −2).", "Grain, Wine, Furs +4 at English & Danish ports."],
		"prices": [["E", [0, 2, 3], 4], ["D", [0, 2, 3], 4]]},
]

# close: route pairs closed. danger: [a, b, amount]. danger_port: +1 on every route touching it.
const EVENTS := [
	{"name": "Charlemagne's Embargo", "year": "c. 790", "flavor": "Charles quarrels with King Offa and closes his ports to English ships.",
		"lines": ["CLOSED: Dorestad–Lundenwic, Quentovic–Lundenwic, Quentovic–Hamwic.", "Cloth +4 at Lundenwic and Hamwic."],
		"close": [["Dorestad", "Lundenwic"], ["Quentovic", "Lundenwic"], ["Quentovic", "Hamwic"]],
		"prices": [["Lundenwic", [1], 4], ["Hamwic", [1], 4]], "danger": [], "danger_port": "", "caught_bonus": 0, "burn": false},
	{"name": "Famine in Francia", "year": "792–793", "flavor": "The harvest fails across the Frankish realm.",
		"lines": ["Grain +5 at Dorestad, Medemblik and Quentovic."],
		"close": [], "prices": [["F", [0], 5]], "danger": [], "danger_port": "", "caught_bonus": 0, "burn": false},
	{"name": "Lindisfarne", "year": "8 June 793", "flavor": "Northmen fall on the holy island. Every English coast fears the sea.",
		"lines": ["CLOSED: Eoforwic–Ribe.", "Danger +1 on Lundenwic–Eoforwic and Medemblik–Eoforwic.", "Swords +5 at English ports."],
		"close": [["Eoforwic", "Ribe"]], "prices": [["E", [5], 5]],
		"danger": [["Lundenwic", "Eoforwic", 1], ["Medemblik", "Eoforwic", 1]], "danger_port": "", "caught_bonus": 0, "burn": false},
	{"name": "Capitulary of Thionville", "year": "805", "flavor": "Charlemagne forbids merchants to carry weapons and mail beyond the frontier.",
		"lines": ["Contraband checks catch you on 1–3 (bribe: 1–2).", "Swords +4 at Ribe and Hedeby."],
		"close": [], "prices": [["D", [5], 4]], "danger": [], "danger_port": "", "caught_bonus": 1, "burn": false},
	{"name": "Godfred Moves the Merchants", "year": "808", "flavor": "King Godfred razes Reric and moves its traders to Hedeby.",
		"lines": ["At Hedeby: Furs and Amber −3.", "At Hedeby: Cloth and Wine +3."],
		"close": [], "prices": [["Hedeby", [3, 4], -3], ["Hedeby", [1, 2], 3]], "danger": [], "danger_port": "", "caught_bonus": 0, "burn": false},
	{"name": "Dorestad Burns", "year": "834", "flavor": "Danes sail up the Rhine and burn the lower town.",
		"lines": ["If you're at Dorestad now: roll, 1–3 lose 1 lot.", "Grain, Cloth, Wine +4 at Dorestad.", "Danger +1 on all routes to or from Dorestad."],
		"close": [], "prices": [["Dorestad", [0, 1, 2], 4]], "danger": [], "danger_port": "Dorestad", "caught_bonus": 0, "burn": true},
]

# id, name, needs CLOSE range, reminder text
const MOVES := [
	["arrows", "Loose Arrows", false, "4–5: raider Crew −1 · 6: −2"],
	["spears", "Spear Volley", true, "CLOSE · 3–4: Crew −1 · 5–6: −2"],
	["board", "Board", true, "CLOSE · boarding fight; win by 4+ ends it"],
	["shield", "Shield Wall", false, "their Crew hits −1, +2 to your boarding"],
	["rally", "Rally", false, "1–2: Morale +1 · 3–6: +2"],
	["row", "Row Hard (flee)", false, "escape FAR 3+ · CLOSE 5+ · +1 day"],
	["tribute", "Pay Tribute", false, "15s or your 2 best lots · ends it"],
]
