extends "res://scripts/ui/paper.gd"
## The rules sheet, short version. The computer rolls the die and keeps the books.

const RULES := [
	["Goal", "You owe the Count's reeve 100s for your ship. Clear the debt in Dorestad by the end of day 60. Interest: +10% of the debt at the start of Summer, Autumn and Winter."],
	["Sail", "Click a port joined to yours on the map. The box on each line is days at sea. Upkeep: 1s per day, +1s per voyage for each crew above 6. Overland carts: 1s per lot."],
	["Trade", "Market sheet, in port. Each lot you buy pushes that price up 1 here; each lot you sell pushes it down 1. The marks wash out each new season. On arrival the die sets the mood: one good +3, one −3, this visit only. Hold: 6 lots."],
	["Encounters", "Coastal routes on a 1, open sea on 1–2 (Summer +1). Then: 1–3 Viking raiders, 4 storm (Hull −1, +1 day), 5 Frankish patrol, 6 Frisian trader (a rumour and a swap)."],
	["Swords", "Contraband. Buy at Dorestad or Quentovic (max 3 per visit); sell only at English and Danish ports. Entering or leaving a Frankish port with swords: caught on 1–2 (bribe 5s: only on 1). Caught = swords seized, 10s fine."],
	["Seasons", "Every 15 days: interest, marks erased, a new event card. Winter: no raiders, but storms and slower voyages."],
	["Ship", "Hull 0: you sink. Crew under 4: voyages +1 day. Repair 3s, hire 5s, on the Tracker sheet."],
	["Grace", "In Dorestad on day 60 but short? Pay half, and the rest ×1.5 is due by day 75 (a bitter win). Otherwise you lose the ship."],
]


func _ready() -> void:
	title = "Rules"
	subtitle = "the short version"


func _draw() -> void:
	draw_paper()
	var y := 58.0
	for item in RULES:
		draw_string(Ink.bold(), Vector2(14, y + 14), item[0], HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Ink.INK)
		y = draw_wrapped(Ink.sans(), Vector2(110, y), item[1], size.x - 124, 13, Ink.INK) + 10
