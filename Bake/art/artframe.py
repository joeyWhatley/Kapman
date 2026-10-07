# -*- coding: utf-8 -*-
r"""
THE FRAME'S PIECES (the user, 7 Oct 2026: "for the UI and really everything I'll need templates for and assigned
correctly and organised so I can make the sprites myself").

Every picture the Command frame needs (Design/ui_mock/template3.html, layout G: the thick rounded frame, the settings
bar, the eight function buttons, the tiled windows, the floating windows, the portrait block, the ship window), each as
a small placeholder drawn by rule at the size the real picture must be. make_art_list.py turns PIECES into slots of the
kinds "hud" and "shipview"; Art/hud_assign.json says for each where the game puts it and how it is cut.

All sizes are for the 640 x 360 screen (shown x3 on 1920 x 1080). A piece "cut in nine" has a corner size: the four
corners are kept as drawn, the sides stretch along, the middle fills.
"""
from artkit import *          # noqa: F401,F403  (the colours by name)
from artkit import Cv, glyph, placeholder_mark, label_strip

CUTS = {0: [], 1: [1], 2: [2, 1], 3: [3, 1, 1], 4: [4, 2, 1, 1], 5: [5, 3, 2, 1, 1]}
LOOK = ("The look of the frame: dark plum with a raised edge, thick, with rounded corners. The light comes from the top left in "
        "every piece, so a raised edge is light on top and left and dark at the bottom and right, and a sunk one the other way. "
        "Gold marks what is yours to press or to spend. The letters are set by the game: never draw words into a piece.")
SHIPLOOK = "The view is a small painting, not an icon: sky, water and shore in the game's 40 colours, hard pixels, no blur."


def _inside(x, y, x0, y0, x1, y1, r, round_bottom=True):
    if not (x0 <= x <= x1 and y0 <= y <= y1):
        return False
    cuts = CUTS.get(max(0, r), CUTS[5])
    top, bot, left, right = y - y0, y1 - y, x - x0, x1 - x
    for dy, rounded in ((top, True), (bot, round_bottom)):
        if rounded and dy < len(cuts) and (left < cuts[dy] or right < cuts[dy]):
            return False
    return True


def raised(cv, x0, y0, x1, y1, fill, light, dark, r=3, edge=2, round_bottom=True):
    """A rounded block with a bevel: light along the top and left, dark along the bottom and right."""
    for y in range(y0, y1 + 1):
        for x in range(x0, x1 + 1):
            if not _inside(x, y, x0, y0, x1, y1, r, round_bottom):
                continue
            if edge and not _inside(x, y, x0 + edge, y0 + edge, x1 - edge, y1 - (edge if round_bottom else 0), max(r - 1, 0), round_bottom):
                tl, br = min(y - y0, x - x0), min(y1 - y, x1 - x)
                cv.px(x, y, light if tl <= br else dark)
            elif fill:
                cv.px(x, y, fill)


def block(fill, light, dark, r=3, edge=2, shadow=0, round_bottom=True, hollow=False, name=None):
    def fn(cv):
        w, h = cv.w, cv.h
        if shadow:
            raised(cv, shadow, shadow, w - 1, h - 1, NIGHT, NIGHT, NIGHT, r, 0, round_bottom)
        raised(cv, 0, 0, w - 1 - shadow, h - 1 - shadow, None if hollow else fill, light, dark, r, edge, round_bottom)
        if name and w >= 24:
            cv.text_mid((w - shadow) // 2, (h - shadow) // 2 - 2, name, MAUVE)
    return fn


def sign(name):
    def fn(cv):
        cv.blit(glyph(name), (cv.w - 16) // 2, (cv.h - 16) // 2)
    fn.glyph = name
    return fn


def _wait(cv):
    cv.ell(3, 3, 12, 12, None, PGOLD); cv.line(8, 8, 8, 5, PSAND); cv.line(8, 8, 10, 9, PSAND)


def _measure(cv):
    cv.line(2, 12, 12, 2, PGOLD); cv.line(3, 13, 13, 3, PGOLD)
    for i in (4, 7, 10):
        cv.px(i + 1, 15 - i, PSAND)
    cv.px(2, 13, PSAND); cv.px(13, 2, PSAND)


def _sound(cv):
    cv.line(8, 1, 8, 9, PSAND); cv.rect(6, 10, 10, 13, PGOLD); cv.line(7, 14, 9, 14, GOLD); cv.line(3, 6, 5, 6, WAVE); cv.line(11, 6, 13, 6, WAVE)


def _menu(cv):
    for y in (2, 5, 8):
        cv.line(2, y, 9, y, PGOLD)


def _arrow_left(cv):
    for y in range(7):
        n = 3 - abs(3 - y)
        cv.line(4 - n, y, 4, y, PGOLD)


def _arrow_right(cv):
    for y in range(7):
        n = 3 - abs(3 - y)
        cv.line(0, y, n, y, PGOLD)


def _arrow_pick(cv):
    for y in range(4):
        cv.line(y, y, 6 - y, y, PGOLD)


def _close(cv):
    cv.line(0, 0, 6, 6, STONE); cv.line(6, 0, 0, 6, STONE)


def _add(cv):
    cv.line(3, 0, 3, 6, PGOLD); cv.line(0, 3, 6, 3, PGOLD)


def _lock(cv):
    cv.box(1, 0, 5, 5, STONE); cv.rect(0, 4, 6, 8, SLATE); cv.px(3, 6, NIGHT)


def _pip(full):
    def fn(cv):
        cv.ell(0, 0, 4, 4, PGOLD if full else None, GOLD if full else STONE)
    return fn


def _scroll(cv):
    cv.rect(0, 0, 3, 7, MAUVE); cv.line(0, 0, 3, 0, LILAC); cv.line(0, 7, 3, 7, PLUM)


def _point(cv):
    cv.ell(0, 0, 4, 4, PGOLD, NIGHT)


def _dash(cv):
    cv.rect(0, 0, 4, 1, PGOLD)


def _lay(kind):
    def fn(cv):
        if kind == "rows":
            for y in (0, 3, 6):
                cv.line(0, y, 8, y, PGOLD)
        elif kind == "cols":
            for x in (0, 4, 8):
                cv.line(x, 0, x, 6, PGOLD)
        else:
            cv.box(0, 2, 8, 6, PGOLD); cv.line(0, 0, 3, 0, PGOLD); cv.line(5, 0, 8, 0, STONE)
    return fn


# ------------------------------------------------------------------------------------------------ the ship window
SKY_BANDS = {"day": [SKY, SKY, FROST], "dusk": [DPURP, PLUM, DROSE, PEACH], "night": [NIGHT, NIGHT, DPURP]}
SEA_BANDS = {"day": [SHOAL, SEA, DEEP], "dusk": [SEA, DEEP, ABYSS], "night": [DEEP, ABYSS, ABYSS]}
PLACES = [("river", "On the river", "Both banks close: reeds and willows near, the far bank low and wooded. Flat water."),
          ("coast", "Off the coast", "One low shore along the horizon on the left: dunes or marsh, no cliffs unless the port has them. A light swell."),
          ("sea", "Open sea", "No land at all. The horizon, the swell, the sky. This is the one she spends longest in: make the sky carry it."),
          ("port", "In port", "A waterfront seen from the boat: a timber quay or a beach with posts, low houses with thatch, smoke. No stone quays, no chimneys.")]
HOURS = [("day", "by day"), ("dusk", "at dusk or dawn"), ("night", "at night")]
HORIZON = 58


def backdrop(place, hour):
    def fn(cv):
        w, h = cv.w, cv.h
        sky, sea = SKY_BANDS[hour], SEA_BANDS[hour]
        for i, c in enumerate(sky):
            cv.rect(0, i * HORIZON // len(sky), w - 1, (i + 1) * HORIZON // len(sky) - 1, c)
        for i, c in enumerate(sea):
            cv.rect(0, HORIZON + i * (h - HORIZON) // len(sea), w - 1, HORIZON + (i + 1) * (h - HORIZON) // len(sea) - 1, c)
        land, low = (WSHADE, PEAT) if hour == "night" else (FOREST, GRASS)
        if place == "river":
            cv.rect(0, HORIZON - 7, w - 1, HORIZON - 1, land); cv.rect(0, HORIZON - 2, w - 1, HORIZON - 1, low)
            cv.poly([(0, h - 1), (0, h - 26), (14, h - 18), (30, h - 1)], low, land)
            for x in range(2, 24, 4):
                cv.line(x, h - 20 + x // 3, x + 1, h - 30 + x // 3, REED if hour != "night" else PEAT)
        elif place == "coast":
            cv.poly([(0, HORIZON - 1), (0, HORIZON - 6), (40, HORIZON - 9), (72, HORIZON - 4), (96, HORIZON - 1)], SAND if hour == "day" else DEARTH, land)
        elif place == "port":
            cv.rect(0, HORIZON - 3, w - 1, HORIZON + 1, TIMBER)
            for i, x in enumerate(range(6, w - 20, 22)):
                hh = 9 + (i * 5) % 7
                cv.rect(x, HORIZON - 3 - hh, x + 14, HORIZON - 4, DEARTH)
                cv.poly([(x - 2, HORIZON - 3 - hh), (x + 7, HORIZON - 9 - hh), (x + 16, HORIZON - 3 - hh)], PEAT if hour != "day" else SAND)
            for x in range(10, w, 26):
                cv.line(x, HORIZON + 1, x, HORIZON + 8, PEAT)
        else:
            for x in range(8, w, 23):
                cv.line(x, HORIZON + 6 + (x * 7) % 25, x + 5, HORIZON + 6 + (x * 7) % 25, WAVE if hour == "day" else SHOAL)
        if hour == "day":
            cv.ell(126, 9, 134, 17, MGOLD)
        elif hour == "dusk":
            cv.ell(120, HORIZON - 9, 132, HORIZON + 3, PEACH)
            cv.rect(0, HORIZON, w - 1, HORIZON + 3, sea[0])
        else:
            cv.ell(124, 8, 132, 16, CWHITE); cv.ell(127, 7, 134, 14, NIGHT)
        label_strip(cv, "%s %s" % (place, hour))
        placeholder_mark(cv)
    return fn


def _fog(cv):
    cv.dither(0, 24, cv.w - 1, cv.h - 1, CWHITE, 0, 2); cv.dither(0, 44, cv.w - 1, cv.h - 1, FROST, 1, 2)


def _rain(cv):
    for i in range(0, cv.w + cv.h, 9):
        for k in range(0, cv.h, 14):
            cv.line(i - k, k + (i * 5) % 9, i - k - 3, k + (i * 5) % 9 + 6, FROST)


def _sun(cv):
    cv.ell(0, 0, 8, 8, MGOLD, PGOLD)


def _moon(cv):
    cv.ell(0, 0, 8, 8, CWHITE); cv.d.ellipse([3, -1, 10, 7], fill=(0, 0, 0, 0))


# ------------------------------------------------------------------------------------------------ the list
# id, group, name, w, h, corner (0 = not cut), drawer, where the game shows it, what it must show
WIN = (DPURP, MAUVE, NIGHT)
PIECES = [
    # frames
    ("frame_screen", "frames", "The frame round the whole screen", 48, 48, 12, block(None, MAUVE, NIGHT, 5, 4, hollow=True),
     "Round the whole screen, over the edge of the chart.",
     "The thick outer frame, hollow in the middle (clear). The one piece the player sees all the time: carve it, knot it or keep it plain."),
    ("frame_window", "frames", "A window", 24, 24, 8, block(*WIN),
     "Round every panel on the left (Voyage, Trade, Yourself and the rest) and round the block of chart tools on the right.",
     "A window that is not in focus: plum fill, raised edge 2 px, rounded corners."),
    ("frame_window_focus", "frames", "The window in focus", 24, 24, 8, block(DPURP, PGOLD, GOLD),
     "Round the one tiled window the keys now act on.", "The same window with a gold edge."),
    ("frame_popup", "frames", "A floating window", 32, 32, 10, block(DPURP, MAUVE, NIGHT, 4, 2, shadow=2),
     "Round everything that floats over the middle of the chart: a card, a talk, the full window of a panel, the wiki.",
     "As a window, a little grander, with a hard dark shadow 2 px down and right (part of the piece)."),
    ("frame_ship", "frames", "The frame of the ship window", 24, 24, 8, block(None, NIGHT, MAUVE, 3, 2, hollow=True),
     "Round the live view of your ship, bottom right.", "A sunk frame (dark top and left, light bottom and right), hollow: the view shows through."),
    ("frame_portrait", "frames", "The portrait block", 24, 24, 8, block(*WIN),
     "Round the portrait top right: yours, or whoever is speaking, with the name and two lines under it.", "A window frame for the portrait block."),
    ("frame_face", "frames", "The frame of a face", 68, 84, 0, block(None, NIGHT, MAUVE, 3, 2, hollow=True),
     "Round a 64 x 80 portrait, in a floating window and in the portrait block.",
     "A sunk frame 2 px wide round a 64 x 80 hole (clear). Not stretched: it is the portrait size plus 2 px each side."),
    ("bar_top", "frames", "The settings bar", 48, 16, 6, block(DPURP, MAUVE, NIGHT, 3, 2),
     "Along the top: menu, date, hour, silver, days of bread, the clock.", "A long low bar.; the game sets the words."),
    ("bar_log", "frames", "The log", 24, 24, 8, block(DPURP, MAUVE, NIGHT, 3, 2),
     "Under the chart: the last lines of what happened.", "A window frame for the log: the bottom edge may be gold where the newest line is."),
    ("bar_work", "frames", "The workspace bar", 24, 12, 5, block(NIGHT, PLUM, NIGHT, 3, 1),
     "Under the tiled windows: the workspaces (1 Sea, 2 Port and so on) and the layouts.", "A dark low strip with a thin edge."),
    # buttons
    ("fn_button", "buttons", "A function button, at rest", 36, 24, 6, block(PLUM, MAUVE, NIGHT, 4, 2),
     "The rack of eight under the settings bar: cast off or lie to, wait, oars, look out, sound, hail, rest, offer.",
     "A raised button. Its icon (16 x 16, one of hud.fn.*) sits in the upper middle and the game sets the word under it."),
    ("fn_button_down", "buttons", "A function button, pressed", 36, 24, 6, block(DPURP, NIGHT, MAUVE, 4, 2),
     "The same button while it is held, or while the thing is on (the oars are out).", "The same button sunk: the edge colours change sides."),
    ("fn_button_off", "buttons", "A function button that cannot be used now", 36, 24, 6, block(BASALT, SLATE, NIGHT, 4, 2),
     "The same button when the thing cannot be done here (no hailing with no one in sight).", "The same button greyed and flat."),
    ("small_button", "buttons", "A small button", 16, 10, 4, block(PLUM, MAUVE, NIGHT, 3, 1),
     "Inside a window: Buy, Sell, Talk, Gift, Read, Show. In the settings bar: under each clock mark. In the list you pick a panel from.",
     "A small pill. Words by the game."),
    ("small_button_on", "buttons", "A small button, chosen", 16, 10, 4, block(BYZ, LILAC, DPURP, 3, 1),
     "A small button that is on: the way of sailing you chose, the speed the clock runs at, the panel a window now holds.", "The same pill in byzantium."),
    ("tab", "buttons", "A tab", 24, 10, 5, block(NIGHT, PLUM, NIGHT, 3, 1, round_bottom=False),
     "Above a window laid as tabs: one for each window not shown.", "A tab with a rounded top and a flat foot, dark."),
    ("tab_on", "buttons", "The tab that is open", 24, 10, 5, block(BYZ, LILAC, DPURP, 3, 1, round_bottom=False),
     "The tab of the window now shown.", "The same tab in byzantium."),
    ("chip", "buttons", "A chip", 16, 9, 4, block(None, NIGHT, NIGHT, 3, 0),
     "In the workspace bar: a workspace or a layout that is not chosen.", "Clear, or the faintest edge: the word alone shows."),
    ("chip_on", "buttons", "The chip that is chosen", 16, 9, 4, block(BYZ, BYZ, BYZ, 3, 1),
     "In the workspace bar: the workspace you are in, the layout in use.", "A byzantium pill."),
    ("option", "buttons", "A choice", 24, 12, 4, block(DPURP, PLUM, NIGHT, 0, 1),
     "In a floating window: one row for each thing you may answer, with its odds at the right.", "A flat row with a thin line above."),
    ("option_hover", "buttons", "A choice under the cursor", 24, 12, 4, block(PLUM, GOLD, NIGHT, 0, 1),
     "The choice the cursor is over, or the one the number key picked.", "The same row a shade lighter with a gold line."),
    ("row", "buttons", "A line in a list", 16, 12, 4, block(NIGHT, NIGHT, NIGHT, 2, 0),
     "Inside a window: one line of a list (a port, a good, a man of the crew).", "A dark rounded strip, no edge."),
    ("row_on", "buttons", "The line that is chosen", 16, 12, 4, block(NIGHT, PGOLD, GOLD, 2, 1),
     "The line you chose (the port you are bound for).", "The same strip with a thin gold edge."),
    ("tag", "buttons", "A tag", 16, 8, 4, block(None, PLUM, PLUM, 3, 1, hollow=True),
     "At the end of a line: one or two words (a day ahead, trusted, worn).", "A hollow pill with a thin plum edge."),
    ("title", "buttons", "A window's title strip", 24, 10, 4, block(DPURP, DPURP, PLUM, 0, 1),
     "Along the top of each window, under its frame: the arrows, the name, the cross.", "A strip with a thin line under it."),
    # marks
    ("arrow_left", "marks", "The arrow to the panel before", 5, 7, 0, _arrow_left, "Left in a window's title strip.", "A small solid arrow pointing left."),
    ("arrow_right", "marks", "The arrow to the next panel", 5, 7, 0, _arrow_right, "Right in a window's title strip.", "A small solid arrow pointing right."),
    ("arrow_pick", "marks", "The mark that a name opens a list", 7, 5, 0, _arrow_pick, "After a window's name.", "A small solid arrow pointing down."),
    ("win_close", "marks", "Shut a window", 7, 7, 0, _close, "Top right of a window and of a floating window.", "A small cross, soft grey; gold when the cursor is on it is done by the game."),
    ("win_add", "marks", "Open a new window", 7, 7, 0, _add, "At the right end of the workspace bar.", "A small plus."),
    ("lock", "marks", "Not yet known", 7, 9, 0, _lock, "In the wiki: before a page your skipper has not heard of.", "A small lock or a closed eye."),
    ("pip_full", "marks", "A pip, filled", 5, 5, 0, _pip(True), "Stats (Tongue, Name, Nerve, Lore, Troth), the mood of the crew, how well you know a tongue.", "A gold dot."),
    ("pip_empty", "marks", "A pip, empty", 5, 5, 0, _pip(False), "The same rows: the levels not yet reached.", "A hollow dot."),
    ("scroll", "marks", "The scroll thumb", 4, 8, 2, _scroll, "At the right of a window whose list is longer than the window.", "A small bar."),
    ("measure_point", "marks", "An end of the measure", 5, 5, 0, _point, "On the chart: where you clicked with the measure.", "A gold dot with a dark eye, so it reads on sea and land."),
    ("measure_dash", "marks", "The line of the measure", 8, 2, 0, _dash, "On the chart, repeated along the way by water between the two ends.", "One dash and its gap: 5 px gold, 3 px clear."),
    ("lay_rows", "marks", "Layout: rows", 9, 7, 0, _lay("rows"), "In the workspace bar.", "Three lines one above the other."),
    ("lay_cols", "marks", "Layout: columns", 9, 7, 0, _lay("cols"), "In the workspace bar.", "Three lines side by side."),
    ("lay_tabs", "marks", "Layout: tabs", 9, 7, 0, _lay("tabs"), "In the workspace bar.", "A box with tabs on top."),
    ("menu", "marks", "The menu button", 12, 12, 0, _menu, "Top left of the settings bar.", "Three short lines, or a small knot: it opens save, load, the settings."),
]
for _pid, _name, _draw, _what in [
        ("cast_off", "Cast off", sign("sail"), "A sail going up, or a mooring line let go."),
        ("lie_to", "Lie to", sign("hull"), "A hull with the sail down: she stops and drifts."),
        ("wait", "Wait", _wait, "The sun on a stick, an hour-candle, or another plain sign of time passing: no clock face in 793."),
        ("oars", "Oars", sign("oar"), "An oar, or an oar across a furled sail: out oars, or back to the sail."),
        ("look_out", "Look out", sign("eye"), "An eye, or a hand shading the eyes."),
        ("sound", "Sound", _sound, "A weight on a line going down into water."),
        ("hail", "Hail", sign("horn"), "A horn, or a hand at the mouth."),
        ("rest", "Rest", sign("bed"), "A man asleep under a cloak or a hide: there are no bunks in an open boat."),
        ("offer", "Offer", sign("candle"), "A candle, or an open hand with a coin: it must serve every faith, so no cross and no hammer.")]:
    PIECES.append(("fn." + _pid, "function icons", "Function: " + _name, 16, 16, 0, _draw,
                   "On its function button in the rack of eight.", _what + " One or two colours, clear background, reads at a glance."))
for _pid, _name, _g, _what in [
        ("voyage", "Voyage", "sail", "Where to and how."), ("trade", "Trade", "scales", "The market where she lies."), ("hold", "Hold", "cask", "What is stowed."),
        ("people", "People here", "speech", "Who is in this port."), ("crew", "Crew", "oar", "Your men."), ("tasks", "Tasks", "tally", "What you have taken on."),
        ("event", "Event", "bell", "The card in front of you."), ("self", "Yourself", "heart", "Who you are and your five stats."),
        ("bonds", "Bonds", "ring", "Who you are tied to."), ("word", "The word", "coin", "What you know of prices, and how old it is."),
        ("faith", "Faith and days", "fire", "Feasts, fasts and vows: draw a sign no one faith owns (a flame, a day-mark)."),
        ("chron", "Chronicle", "book", "The news of the year."), ("chest", "Chest", "chest", "Texts, riddles, your notes."), ("house", "House", "house", "The debt, your boats, your heir."),
        ("field", "The field", "flag", "The other sails bound where you are bound."), ("sky", "Wind and tide", "wind", "Weather, tide and signs."),
        ("tongues", "Tongues", "quill", "The tongues you know."), ("ship", "Ship", "hull", "Hull, sail and what is fitted."), ("wiki", "Wiki", "leaf", "The book of what is known.")]:
    PIECES.append(("panel." + _pid, "panel icons", "Panel: " + _name, 16, 16, 0, sign(_g),
                   "Before the name of the %s window, and in the list you pick a panel from." % _name, _what + " A small sign, clear background."))
for _pid, _name, _g, _what in [
        ("tongue", "Tongue", "speech", "Talking a thing through."), ("name", "Name", "circlet", "How you look and who vouches for you."),
        ("nerve", "Nerve", "shield", "Holding firm."), ("lore", "Lore", "book", "What you can quote."), ("troth", "Troth", "candle", "Keeping your own faith, whichever it is.")]:
    PIECES.append(("stat." + _pid, "stat icons", "Stat: " + _name, 16, 16, 0, sign(_g),
                   "In the Yourself window, and before a choice in a talk that the stat opens.", _what + " A small sign, clear background."))
for _place, _pname, _pwhat in PLACES:
    for _hour, _hname in HOURS:
        PIECES.append(("back.%s.%s" % (_place, _hour), "ship window", "%s, %s" % (_pname, _hname), 160, 96, 0, backdrop(_place, _hour),
                       "The ship window, bottom right, behind your ship when she is %s %s." % (_pname.lower(), _hname),
                       "%s The horizon is %d px from the top, the same in all twelve so they change without a jump. Keep the middle of the water empty: "
                       "her own picture (ship_side, 160 x 96, the same size) is laid over this." % (_pwhat, HORIZON)))
PIECES += [
    ("over.fog", "ship window", "Fog, laid over the view", 160, 96, 0, _fog, "The ship window in fog, over the backdrop and the ship.",
     "Banks of pale fog on a clear ground, thicker toward the water: the game lays it over any backdrop."),
    ("over.rain", "ship window", "Rain and spray, laid over the view", 160, 96, 0, _rain, "The ship window in a gale, over the backdrop and the ship.",
     "Slanting rain and spray on a clear ground."),
    ("sun", "ship window", "The sun", 9, 9, 0, _sun, "The ship window: the game moves it across the sky by the hour.", "A small disc."),
    ("moon", "ship window", "The moon", 9, 9, 0, _moon, "The ship window at night: the game moves it by the hour.", "A small crescent; the game may swap in other phases later."),
]


# ---- added after the rulings of 7 October: chart tools on the right, panels that open, the boat and the chart changing places
def _swap(cv):
    cv.rect(0, 0, 11, 11, NIGHT)
    cv.line(2, 4, 9, 4, PGOLD); cv.px(8, 3, PGOLD); cv.px(8, 5, PGOLD)
    cv.line(2, 7, 9, 7, PGOLD); cv.px(3, 6, PGOLD); cv.px(3, 8, PGOLD)


def _open(cv):
    cv.line(1, 5, 5, 1, PGOLD); cv.line(3, 1, 5, 1, PGOLD); cv.line(5, 1, 5, 3, PGOLD); cv.line(0, 6, 2, 6, STONE); cv.line(0, 4, 0, 6, STONE)


def _size(cv):
    cv.line(0, 1, 6, 1, PGOLD); cv.line(0, 3, 4, 3, PGOLD); cv.line(0, 5, 2, 5, PGOLD)


def _pressing(cv):
    cv.poly([(4, 0), (7, 5), (6, 8), (2, 8), (1, 5)], RUST, BLOOD); cv.poly([(4, 3), (5, 6), (4, 7), (3, 6)], PGOLD)


def _ring(mark):
    def fn(cv):
        cv.ell(2, 2, 13, 13, None, PGOLD)
        cv.line(5, 8, 10, 8, PSAND)
        if mark == "+":
            cv.line(8, 5, 8, 10, PSAND)
    return fn


PIECES += [
    ("swap", "marks", "Change places: the ship and the chart", 12, 12, 0, _swap, "On the top left corner of the view of the ship.",
     "Two arrows passing each other. It sits on sky, sea or chart, so it carries a dark ground of its own."),
    ("win_open", "marks", "Open the full window", 7, 7, 0, _open, "In the title strip of a panel on the left.", "A small arrow leaving a corner."),
    ("win_size", "marks", "Change the size of a panel", 7, 7, 0, _size, "In the title strip of a panel; the game may write S, M or L in its place.", "Three bars, long to short."),
    ("pressing", "marks", "A pressing card", 9, 9, 0, _pressing, "On a floating window that must be answered: it holds the ship where she is until it is.",
     "A small sign of haste that is of the period: a flame, a horn, a raised hand. Not an exclamation mark in a triangle."),
    ("mini_her", "marks", "Her mark on the small chart", 5, 5, 0, _point, "On the chart when it is shown small in the window bottom right.", "A gold dot with a dark edge."),
    ("frame_tip", "frames", "The brief overview under the cursor", 16, 16, 5, block(NIGHT, MAUVE, NIGHT, 3, 1),
     "Round the small box that shows when the cursor rests on a linked word.", "A small dark box with a thin raised edge."),
    ("tool_button", "buttons", "A chart tool, at rest", 24, 12, 4, block(PLUM, MAUVE, NIGHT, 3, 1),
     "The chart tools on the right: measure, nearer, farther, find her, names, winds, sails, pins.",
     "A small raised button. Its icon (one of hud.chart.*) and its word are set by the game."),
    ("tool_button_on", "buttons", "A chart tool that is on", 24, 12, 4, block(BYZ, NIGHT, LILAC, 3, 1),
     "A chart tool that is switched on (names shown, the measure in your hand).", "The same button sunk, in byzantium."),
]
for _pid, _name, _draw, _what in [
        ("measure", "Measure", _measure, "A notched stick or a knotted cord: the way between two places, in days."),
        ("nearer", "Nearer", _ring("+"), "The chart drawn nearer: a ring with a plus, or an eye close to."),
        ("farther", "Farther", _ring("-"), "The chart drawn back: a ring with a bar."),
        ("find_her", "Find her", sign("hull"), "The chart centred on your own boat: a hull in a ring."),
        ("names", "Names", sign("quill"), "The names of places shown or hidden."),
        ("winds", "Winds", sign("wind"), "The wind drawn on the chart."),
        ("sails", "Sails", sign("sail"), "The other sails you know of, shown or hidden."),
        ("pins", "Pins", sign("flag"), "Your own marks on the chart.")]:
    PIECES.append(("chart." + _pid, "chart tool icons", "Chart tool: " + _name, 16, 16, 0, _draw,
                   "On its button among the chart tools, right of the screen under the portrait.", _what + " One or two colours, clear background."))
for _pid, _name, _g, _what in [
        ("how_to_play", "How to play", "key", "The rules."), ("ports", "Ports", "house", "Ports and their sites."), ("trade", "Trade", "scales", "Goods, coin, tolls."),
        ("peoples", "Peoples", "speech", "The peoples of the northern seas."), ("faith_and_days", "Faith and days", "candle", "Feasts, fasts and faiths: a sign no one faith owns."),
        ("ship_and_sea", "Ship and sea", "hull", "Boats, tides, the lead."), ("words", "Words", "quill", "Words of the old tongues."), ("events", "Events", "bell", "What has happened, by its day."),
        ("people", "People", "circlet", "The people you have met or heard of.")]:
    PIECES.append(("wiki." + _pid, "wiki icons", "Wiki: " + _name, 16, 16, 0, sign(_g),
                   "In the wiki: beside the kind of a page, and in the small box under the cursor.", _what + " A small sign, clear background."))


# ---- added 7 October, after the frame was checked against the mock-up and the game: the clock, the word marks,
#      a place to type, the main button of a full window, the groove of a scroll bar, a dividing line, a landmark
def _clock(kind):
    def fn(cv):
        if kind == "pause":
            cv.rect(2, 1, 3, 5, PGOLD); cv.rect(5, 1, 6, 5, PGOLD)
            return
        n = {"x1": 1, "x2": 2, "x4": 3}[kind]
        step = 4 if n == 2 else 3
        x0 = (9 - ((n - 1) * step + 3)) // 2
        for i in range(n):
            for y in range(1, 6):
                d = 2 - abs(3 - y)
                cv.rect(x0 + i * step + (d if n == 3 else 0), y, x0 + i * step + d, y, PGOLD)
    return fn


def _word(kind):
    def fn(cv):
        if kind == "seen":
            cv.line(0, 3, 3, 0, PGOLD); cv.line(3, 0, 6, 3, PGOLD); cv.line(0, 3, 3, 6, PGOLD); cv.line(3, 6, 6, 3, PGOLD); cv.px(3, 3, PSAND)
        elif kind == "heard":
            cv.line(2, 0, 4, 0, PGOLD); cv.line(1, 1, 1, 3, PGOLD); cv.line(5, 1, 5, 3, PGOLD); cv.px(4, 4, PGOLD); cv.line(3, 5, 3, 6, PGOLD); cv.px(3, 2, PSAND)
        elif kind == "season":
            cv.ell(2, 2, 4, 4, PGOLD)
            for x, y in ((3, 0), (3, 6), (0, 3), (6, 3)):
                cv.px(x, y, PGOLD)
        elif kind == "law":
            cv.ell(1, 0, 5, 4, RUST, BLOOD); cv.line(2, 5, 2, 6, RUST); cv.line(4, 5, 4, 6, RUST)
        else:
            cv.line(1, 3, 5, 3, STONE)
    return fn


def _tilt(up):
    def fn(cv):
        for i in range(3):
            y = (1 + i) if up else (3 - i)
            cv.rect(2 - i, y, 2 + i, y, PGOLD if up else STONE)
    return fn


def _landmark(cv):
    cv.rect(0, 0, 4, 4, PSAND); cv.box(0, 0, 4, 4, PEAT)


def _track(cv):
    cv.rect(0, 0, 3, 7, NIGHT); cv.line(0, 0, 0, 7, BASALT)


def _rule(cv):
    cv.line(0, 0, 7, 0, PLUM)


PIECES += [
    ("field", "buttons", "A place to type", 16, 10, 4, block(NIGHT, NIGHT, MAUVE, 3, 1),
     "In the wiki: the line you type a word into to find its page. In the chest: a note of your own.",
     "A sunk box: dark on top and left, light at the bottom and right, a near-black middle. The game writes what is typed."),
    ("main_button", "buttons", "The main button of a window", 24, 14, 5, block(BYZ, LILAC, DPURP, 4, 2),
     "At the foot of a full window: the one thing the window is for ('Cast off for Traiectum').",
     "A raised button in byzantium, larger than a small button. The game sets the words in pale gold."),
    ("scroll_track", "marks", "The groove of the scroll bar", 4, 8, 2, _track,
     "At the right of a window whose list is longer than the window, under the scroll thumb.",
     "A dark groove, a shade darker than the window's fill."),
    ("rule", "marks", "A dividing line", 8, 1, 0, _rule,
     "Inside a window, between two parts (under a heading, above the foot of a wiki page). The game pulls it out to the window's width.",
     "One line of one colour, 1 px high. Every pixel the same, since it is stretched."),
    ("landmark", "marks", "A landmark on the chart", 5, 5, 0, _landmark,
     "On the chart, at a named landmark (a headland, a tower, a bar), with its name beside it by the game.",
     "A small square or lozenge: a pale middle and a dark edge, so it reads on sea and on land."),
    ("tilt.up", "marks", "The price leans up there", 5, 5, 0, _tilt(True),
     "In the Trade window and The word window, after the mark of how you know a price: it is worth carrying there.",
     "A small wedge pointing up."),
    ("tilt.down", "marks", "The price leans down there", 5, 5, 0, _tilt(False),
     "The same place: the price there leans down.", "The same wedge pointing down, in a dull colour."),
]
for _pid, _name, _what in [
        ("pause", "The clock: held", "Two upright bars."),
        ("x1", "The clock: a game minute a second", "One arrowhead pointing right."),
        ("x2", "The clock: twice as fast", "Two arrowheads pointing right."),
        ("x4", "The clock: four times as fast", "Three arrowheads pointing right, or four if they stay apart at this size.")]:
    PIECES.append(("clock." + _pid, "marks", _name, 9, 7, 0, _clock(_pid),
                   "At the right end of the settings bar, on a small button: the four ways the clock can stand. The one that is on is lit by its button.",
                   _what + " One colour, clear ground."))
for _pid, _name, _what in [
        ("seen", "You saw it yourself", "An eye."), ("heard", "You were told it", "An ear."),
        ("season", "It is the season's doing", "A sun: a ring with rays."), ("law", "The law is on it", "A seal on its two cords."),
        ("none", "Nothing is known", "A short dash.")]:
    PIECES.append(("word." + _pid, "marks", "How you know a price: " + _name.lower(), 7, 7, 0, _word(_pid),
                   "In the Trade window, The word window and a port's lines on the chart: before each line of what you know of a good.",
                   _what + " One colour (the law may be rust), clear ground."))

# ------------------------------------------------------------------------------------------------ notes for the artist
# pid -> (what it goes with and must line up with, what must read at its size and what to leave out)
NOTES = {
    "frame_screen": (
        "Everything else stands inside it: the settings bar along the top, the panels down the left, the portrait block, the chart tools and "
        "the ship window down the right, the log along the foot. Its inner edge is what they all sit against, so end it inward with one "
        "dark line (night). Over dark water its outer edge is lost unless the outermost line is warm white, one pixel wide "
        "(Design/research/next/art.md, B5).",
        "It is on screen all evening, so quiet is better than rich. Keep any carving or knot inside the four corners: a knot on a side is "
        "pulled out of shape when the side stretches."),
    "frame_window": (
        "Draw it with hud.frame_window_focus, hud.frame_portrait and hud.bar_log as one set: the same rounding of the corners, the same "
        "2 px edge, the same fill, so a column of them reads as one hand. hud.title lies along the inside of its top and has the same fill.",
        "It is a ground for words. The fill is one flat dark colour (dark purple): no texture, no dither, or the words on it are hard to read."),
    "frame_window_focus": (
        "The same shape as hud.frame_window, pixel for pixel: the game swaps one for the other, so a pixel that moved will jump. "
        "Only the edge changes, from mauve and night to pale gold and gold.",
        "The gold edge must say at one glance which panel the keys act on."),
    "frame_popup": (
        "Of the family of hud.frame_window, a little heavier, since it lies over the chart and over other windows. It holds hud.title along "
        "its top, hud.win_close in its top right corner, hud.frame_face round a portrait at its top left, the rows hud.option at its foot, "
        "and hud.pressing when the card is a pressing one.",
        "The shadow is the bottom 2 rows and the right 2 columns, in night, hard, no blur. It is part of the 10 px corner: 8 px of frame "
        "and 2 px of shadow. The top and the left have no shadow."),
    "frame_ship": (
        "Stands in the ship block, bottom right, round a view of 160 x 96 px. hud.swap sits on its top left corner. hud.frame_face is its "
        "small sister with the same sunk edge, so the two pictures on the right of the screen look set into the frame in the same way.",
        "Sunk, not raised: dark on top and left, light at the bottom and right. The middle is clear."),
    "frame_portrait": (
        "The top block of the right-hand column, above the chart tools and the ship window, and as wide as they are: the same edge and "
        "fill as hud.frame_window. hud.frame_face with a 64 x 80 portrait stands at its left; the name and two lines of words at its "
        "right; under them a row of the crew's marks (ui.crew_head).",
        "Flat dark fill, as for a window."),
    "frame_face": (
        "Laid over a portrait: the hole is exactly 64 x 80 px and starts 2 px in from the left and from the top. Match the sunk edge of "
        "hud.frame_ship.",
        "Only 2 px wide all round: an edge and its shade, nothing more. The hole's corners are square, so no pixel of the portrait is hidden."),
    "bar_top": (
        "Runs along the top, inside hud.frame_screen. From the left it holds hud.menu, the date, the hour, the silver, the days of bread and "
        "the debt; at its right end the four clock marks hud.clock.* on small buttons. The same edge and fill as hud.frame_window.",
        "16 px high: 2 px of edge above and below leave 12 px, the height of hud.menu. Flat fill: figures are read off it all the time."),
    "bar_log": (
        "Under the chart, along the foot of the screen, between the panels on the left and the ship window on the right. Of the set of "
        "hud.frame_window.",
        "Flat dark fill: three or four lines of small words stand on it, the newest lowest."),
    "bar_work": (
        "Stands straight under the panels on the left and is as wide as they are. It holds hud.chip and hud.chip_on (9 px high) for the "
        "workspaces and the layouts, the marks hud.lay_rows, hud.lay_cols, hud.lay_tabs, and hud.win_add at its right end.",
        "Lower and darker than a window, so it reads as a foot and not as one more panel: 12 px high with a 1 px edge."),
    "frame_tip": (
        "The small sister of hud.frame_popup. It floats at the cursor, over anything. One of hud.wiki.* (16 x 16) may stand at its top left.",
        "It lies over chart, windows and pictures alike: a near-black fill and a clear pale edge part it from all of them."),
    "field": (
        "Stands inside a window above the lines hud.row, and is as high as hud.small_button, so the two can share a line.",
        "It must look hollow, the opposite of a button: that is all that tells the player 'type here'."),
    "fn_button": (
        "Three pictures of one button: hud.fn_button, hud.fn_button_down, hud.fn_button_off. The outline is the same in all three, pixel "
        "for pixel; only the fill and the edge colours change. Eight stand in two rows of four under the settings bar, 1 or 2 px apart, so "
        "judge it beside a twin. hud.tool_button is its small cousin on the right of the screen.",
        "The raised edge is what says 'press me': 2 px, light (mauve) on top and left, dark (night) at the bottom and right."),
    "fn_button_down": (
        "The same outline as hud.fn_button, pixel for pixel.",
        "Sunk: the light and the dark of the edge change sides, and the fill goes one step darker. Nothing else moves."),
    "fn_button_off": (
        "The same outline as hud.fn_button, pixel for pixel.",
        "Grey and flat (basalt, slate): no light on the edge. It must still be seen as a button, only one that will not answer."),
    "small_button": (
        "The commonest button in the game, with its twin hud.small_button_on (the same outline). hud.main_button is its big brother.",
        "10 px high with a 1 px edge: room for one line of small words. A pill: the ends are fully round."),
    "small_button_on": (
        "The same outline as hud.small_button, pixel for pixel.",
        "Byzantium fill, so 'on' is told from 'off' by more than a line: the words on it turn pale gold by the game."),
    "main_button": (
        "At the foot of a full window (inside hud.frame_popup), as wide as the window. A window has only one.",
        "Larger and brighter than hud.small_button, so the eye finds it first. Raised, as hud.fn_button is."),
    "tool_button": (
        "Eight stand in two rows of four in the block of chart tools on the right, under the portrait. A small cousin of hud.fn_button, "
        "so the rack on the left and the rack on the right are kin. Twin: hud.tool_button_on, the same outline.",
        "A thin raised edge of 1 px."),
    "tool_button_on": (
        "The same outline as hud.tool_button, pixel for pixel.",
        "Sunk, in byzantium: the tool is switched on (names shown, the measure in your hand)."),
    "tab": (
        "A row of them stands on top of a panel when the workspace is laid as tabs. The flat foot meets the panel's top edge with no gap, "
        "so the bottom row has no edge line of its own. Twin: hud.tab_on.",
        "Dark and low: it is the tab that is not open."),
    "tab_on": (
        "The same outline as hud.tab, pixel for pixel.",
        "Byzantium: it must look joined to the panel under it."),
    "chip": (
        "In hud.bar_work, beside hud.chip_on (the same outline). 9 px high: 1 px less than a small button, so the bar stays low.",
        "Almost nothing: clear, or the faintest edge. The word alone shows."),
    "chip_on": (
        "The same outline as hud.chip.",
        "A byzantium pill: the workspace you are in."),
    "option": (
        "Rows of it stand one under another at the foot of a floating window (hud.frame_popup), each as wide as the window. Twin: "
        "hud.option_hover. A stat icon (hud.stat.*) may stand at its left, its odds as a word at its right.",
        "Flat, with one thin line along the top to part it from the row above. No edge at the sides: it runs into the frame."),
    "option_hover": (
        "The same outline as hud.option.",
        "One shade lighter with a gold line: the choice the cursor is on, or the one a number key picked."),
    "row": (
        "Stacked in a window, 1 px apart, each as wide as the window. Twin: hud.row_on. hud.tag may stand at its right end and "
        "hud.small_button beside that.",
        "Darker than the window's fill and without an edge, so a list reads as lines and not as boxes."),
    "row_on": (
        "The same outline as hud.row.",
        "A thin gold edge: the one line you chose."),
    "tag": (
        "At the right end of hud.row, 8 px high inside the 12 px row.",
        "A hollow pill: a 1 px edge and a clear middle. One or two words stand in it."),
    "title": (
        "Along the inside of the top of hud.frame_window and hud.frame_popup: the same fill as theirs. From the left it holds "
        "hud.arrow_left, the panel's own icon (hud.panel.*), its name, hud.arrow_pick, hud.arrow_right, then hud.win_size, hud.win_open "
        "and hud.win_close at the right.",
        "10 px high: the marks on it are 7 px. One thin line along its foot parts it from the window's body."),
    "arrow_left": ("The mirror of hud.arrow_right, pixel for pixel. On hud.title.", "Solid: at 5 x 7 an outline arrow is only noise."),
    "arrow_right": ("The mirror of hud.arrow_left, pixel for pixel. On hud.title.", "Solid."),
    "arrow_pick": ("Of the same weight as hud.arrow_left and hud.arrow_right, turned to point down. It stands straight after a panel's name.", "Solid."),
    "win_close": ("One of four marks 7 x 7 on hud.title: hud.win_close, hud.win_open, hud.win_size, hud.win_add. Draw them with one "
                  "weight of line (1 px) so they sit as a set.", "A plain cross. Soft grey, since it is the least wanted of the four."),
    "win_add": ("Of the set of hud.win_close. It stands at the right end of hud.bar_work.", "A plain plus, 1 px."),
    "win_open": ("Of the set of hud.win_close.", "An arrow leaving a corner: 'open this out'."),
    "win_size": ("Of the set of hud.win_close.", "Three bars, long to short: small, medium, large."),
    "lock": ("In the wiki, at the left of a line hud.row whose page is not yet known.", "A lock or a closed eye; soft grey, it is not a warning."),
    "pip_full": ("With hud.pip_empty, the same outline. Five stand in a row, 1 px apart, at the right end of hud.row.",
                 "A gold dot with a darker gold edge."),
    "pip_empty": ("The same outline as hud.pip_full.", "A ring only: the middle is clear."),
    "scroll": ("Runs in hud.scroll_track, the same width.", "A plain bar, lighter than its groove. It is pulled out up and down, so every row of its middle is the same."),
    "scroll_track": ("Under hud.scroll, the same width, at the right inside edge of a window.", "Darker than the window's fill: a groove."),
    "rule": ("Inside a window, on its fill (dark purple).", "It only parts two things: plum on dark purple is enough. Not gold."),
    "measure_point": ("With hud.measure_dash and hud.mini_her: the marks that lie on the chart itself.",
                      "Two tones, as every mark on the chart: a light middle and a dark edge, so it reads on dark sea and on pale sand."),
    "measure_dash": ("The game lays it end to end between two hud.measure_point and turns it to the way, so it must join to itself: gold "
                     "at the left end, clear at the right.",
                     "5 px of gold and 3 px clear. If the lower row is night and the upper gold, it reads on pale ground as well as on dark."),
    "mini_her": ("As hud.measure_point: a mark on the chart.", "Gold with a dark edge."),
    "landmark": ("A mark on the chart, smaller than a port's mark (16 x 16) so it is never taken for a port.", "Pale middle, dark edge."),
    "lay_rows": ("Three marks 9 x 7 in hud.bar_work: hud.lay_rows, hud.lay_cols, hud.lay_tabs. One weight of line.", "Three lines one above the other."),
    "lay_cols": ("Of the set of hud.lay_rows.", "Three lines side by side."),
    "lay_tabs": ("Of the set of hud.lay_rows.", "A box with two small tabs on top."),
    "menu": ("At the left end of hud.bar_top: 12 px high, the bar's own inner height.", "Three short lines or a small knot."),
    "swap": ("On the top left corner of hud.frame_ship, over the view.",
             "It lies on sky, sea or chart, so it has a dark ground of its own (night) under two gold arrows that pass each other."),
    "pressing": ("In hud.title of a floating window, before the name.",
                 "A sign of haste that is of the time: a flame, a horn, a raised hand. Rust or gold. Not an exclamation mark in a triangle."),
    "tilt.up": ("With hud.tilt.down, its mirror. It stands after one of hud.word.*, which is 7 px high.", "Solid, gold."),
    "tilt.down": ("The mirror of hud.tilt.up.", "Solid, dull (stone): down is told from up by its shape first and its colour second."),
}
for _k in ("pause", "x1", "x2", "x4"):
    NOTES["clock." + _k] = ("Four marks of one set, 9 x 7 each: hud.clock.pause, hud.clock.x1, hud.clock.x2, hud.clock.x4. Each stands on "
                            "hud.small_button; the one that is on stands on hud.small_button_on.",
                            "Solid shapes. No clock face, no numbers: the arrowheads count the speed.")
for _k in ("seen", "heard", "season", "law", "none"):
    NOTES["word." + _k] = ("Five marks of one set, 7 x 7 each: hud.word.seen, hud.word.heard, hud.word.season, hud.word.law, hud.word.none. "
                           "One stands at the left of a line of words; hud.tilt.up or hud.tilt.down may follow it.",
                           "Told apart by shape alone, since some players do not see red against green or gold. 1 px lines.")

GROUP_NOTES = {
    "marks": "A mark is one or two colours on a clear ground, with no edge line round it: at 5 to 9 px the shape is the whole picture. "
             "Try it on dark purple (a window) and on night (the workspace bar).",
    "function icons": "Nine pictures for eight buttons (cast off and lie to share one). They stand side by side in the rack, so draw the nine "
                      "on one sheet: one weight of line, the same two or three colours of the gold ramp, each about 12 x 12 px inside the "
                      "16 x 16 with 2 px clear all round, all standing on one base line. Fill each with black: if you still know which is "
                      "which, they are right. Leave out: a ground, a frame, shading.",
    "panel icons": "Nineteen, one for each panel. One stands at the left of a panel's title and all nineteen stand together in the list a "
                   "panel is picked from, so each must be told from the other eighteen by its outline. One or two colours, 2 px clear all "
                   "round, the same weight of line as the function icons.",
    "stat icons": "Five, seen as a set in the Yourself window and one at a time before a choice that a stat opens. They must differ in "
                  "outline, since a choice shows one alone.",
    "chart tool icons": "Eight, in two rows of four on the right of the screen. The first four do a thing (measure, nearer, farther, find "
                        "her); the last four switch a thing on and off (names, winds, sails, pins). One weight of line, the same colours "
                        "as the function icons.",
    "wiki icons": "Nine, one for each kind of page. One stands beside a page's kind in the wiki and in the small box under the cursor. "
                  "Quieter than the function icons: they label, they are not pressed.",
    "ship window": "Twelve views: four places at three hours. Draw the day view of a place first and make dusk and night from it by "
                   "changing colours only, so the shore does not jump when the hour turns. Leave out boats, people and birds: her own "
                   "picture is laid over the view. These views are a stand-in: the ship window is to become a live view of the ship "
                   "in 3D (Design/DECISIONS.md, 7 October, ruling 11), so they are not among the pictures to draw first.",
}

# the stylesheet class or the drawn sign that shows each piece in the game today (Assets/Kapman/Game/UI/Frame.uss, the Frame code)
USS = {
    "frame_window": ".frame-win", "frame_window_focus": ".frame-win.focus", "frame_popup": ".frame-float", "frame_ship": ".frame-ship-view",
    "frame_portrait": ".frame-who", "frame_face": ".frame-face, .frame-float-face", "bar_top": ".frame-top", "bar_log": ".frame-log",
    "bar_work": ".frame-wmbar", "fn_button": ".frame-verb", "fn_button_down": ".frame-verb.on", "fn_button_off": ".frame-verb.off",
    "small_button": ".frame-k, .frame-pick", "small_button_on": ".frame-k.on, .frame-pick.on", "main_button": ".frame-k.go",
    "tab": ".frame-tab", "tab_on": ".frame-tab.on", "chip": ".frame-wm", "chip_on": ".frame-wm.on", "option": ".frame-option",
    "option_hover": ".frame-option:hover", "row": ".frame-ln", "row_on": ".frame-ln.sel", "tag": ".frame-tag", "title": ".frame-win-head",
    "arrow_left": "FrameGlyph.Kind.Left", "arrow_right": "FrameGlyph.Kind.Right", "arrow_pick": "FrameGlyph.Kind.Down",
    "win_close": "FrameGlyph.Kind.Cross", "win_add": "FrameGlyph.Kind.Plus", "pip_full": ".frame-pip", "pip_empty": ".frame-pip.off",
    "scroll": ".unity-base-slider__dragger", "scroll_track": ".unity-base-slider__tracker", "rule": ".frame-sep",
    "landmark": ".map-landmark (MapView.uss)", "tilt.up": ".word-tilt.up", "tilt.down": ".word-tilt.down",
    "word.seen": ".word-mark.seen", "word.heard": ".word-mark.heard", "word.season": ".word-mark.season", "word.law": ".word-mark.law",
    "word.none": ".word-mark.none", "fn.cast_off": "FrameIcon.Kind.CastOff", "fn.wait": "FrameIcon.Kind.Wait",
    "fn.look_out": "FrameIcon.Kind.LookOut", "fn.sound": "FrameIcon.Kind.Sound", "fn.hail": "FrameIcon.Kind.Hail",
    "fn.rest": "FrameIcon.Kind.Rest", "fn.offer": "FrameIcon.Kind.Offer", "chart.measure": "FrameIcon.Kind.Measure",
}


def cut_words(w, h, c):
    """What a piece cut in nine keeps and what the game pulls out, in its own numbers."""
    mw, mh = w - 2 * c, h - 2 * c
    t = "Cut in nine: the four corners, %d x %d px each, stay as you draw them." % (c, c)
    if mw > 0:
        t += (" The top and bottom sides between them are %d px long in the file and the game pulls them out to any length, so each "
              "column of a side must be the same as the one beside it: straight bands, no knot, no dither." % mw)
    else:
        t += " The left and right corners meet, so it is always %d px wide and only grows up and down." % w
    if mh > 0:
        t += " The left and right sides, %d px long in the file, are pulled out to any length, so each row of them must be the same as the next." % mh
    else:
        t += " The top and bottom corners meet, so it is always %d px high and only grows sideways." % h
    if mw > 0 and mh > 0:
        t += " The middle, %d x %d px, is one flat colour (clear in a hollow frame)." % (mw, mh)
    return t


SIGN_WORDS = {"speech": "a sign of speech", "tally": "a tally stick", "fire": "a flame", "wind": "a gust of wind", "scales": "a pair of scales"}


def sign_words(piece):
    """For an icon whose words say what the thing is and not what to draw: the sign the placeholder took, as a first thought."""
    g = getattr(piece[6], "glyph", None)
    if not g or piece[1] == "function icons":
        return ""
    w = SIGN_WORDS.get(g) or (("an " if g[:1] in "aeiou" else "a ") + g)
    return "What to draw: one plain sign for that. The placeholder's is %s; take another if it says it better." % w


def twins(piece):
    """Which other placeholders carry the same sign: the artist must know which may share a picture and which must not."""
    g = getattr(piece[6], "glyph", None)
    if not g:
        return ""
    same = [("shipview." if p[1] == "ship window" else "hud.") + p[0] for p in PIECES if p is not piece and getattr(p[6], "glyph", None) == g]
    if not same:
        return ""
    return ("The placeholder's sign (%s) is also on %s. Where the two mean one thing you may draw one picture and save it under both "
            "names; where they mean different things your two pictures must be told apart." % (g, ", ".join(same)))


def draw(piece):
    pid, group, name, w, h, corner, fn, where, what = piece
    cv = Cv(w, h)
    fn(cv)
    return cv


def kind_of(piece):
    return "shipview" if piece[1] == "ship window" else "hud"


def describe(piece):
    pid, group, name, w, h, corner, fn, where, what = piece
    parts = ["%s. %s" % (name, what), sign_words(piece)]
    if corner:
        parts.append(cut_words(w, h, corner))
    goes, reads = NOTES.get(pid, ("", ""))
    if goes:
        parts.append("Goes with: " + goes)
    if reads:
        parts.append("At this size: " + reads)
    if group in GROUP_NOTES:
        parts.append(GROUP_NOTES[group])
    parts.append(twins(piece))
    parts.append(LOOK if kind_of(piece) == "hud" else SHIPLOOK)
    parts.append("In the mock-up: Design/ui_mock/template3.html, layout G (Command frame).")
    return " ".join(x for x in parts if x)


def slice_guide(piece, k=8):
    """The placeholder x8 with gold lines where a piece cut in nine is cut: to look at, never to draw on."""
    pid, group, name, w, h, corner, fn, where, what = piece
    big = draw(piece).scaled(k)
    out = Cv(w * k, h * k, SLATE)
    out.blit(big, 0, 0)
    c = min(corner, w // 2, h // 2)
    for x in (c * k, (w - c) * k - 1):
        out.line(x, 0, x, h * k - 1, PGOLD)
    for y in (c * k, (h - c) * k - 1):
        out.line(0, y, w * k - 1, y, PGOLD)
    return out


def kind_guide(kind, w, h):
    """Guide lines at the real size for a kind of picture: lay it under your drawing as a layer, never save over a slot with it."""
    cv = Cv(w, h)
    for x in range(0, w, 2):
        cv.px(x, 0, MAUVE); cv.px(x, h - 1, MAUVE)
    for y in range(0, h, 2):
        cv.px(0, y, MAUVE); cv.px(w - 1, y, MAUVE)

    def hline(y, c):
        for x in range(1, w - 1, 3):
            cv.px(x, y, c)

    def vline(x, c):
        for y in range(1, h - 1, 3):
            cv.px(x, y, c)
    if kind == "portrait":
        hline(int(h * 0.12), PLUM); hline(int(h * 0.40), PGOLD); hline(int(h * 0.62), PLUM); hline(int(h * 0.80), PGOLD); vline(w // 2, PLUM)
    elif kind in ("scene", "harbour", "card", "story", "ship_offer", "ending", "plate"):
        hline(int(h * 0.60), PGOLD); vline(w // 3, PLUM); vline(2 * w // 3, PLUM); hline(h // 3, PLUM)
    elif kind == "shipview":
        hline(HORIZON, PGOLD); vline(w // 2, PLUM)
    elif kind in ("ship_side",):
        hline(int(h * 0.78), PGOLD); vline(w // 2, PLUM)
    else:
        m = 2 if min(w, h) >= 24 else 1
        cv.box(m, m, w - 1 - m, h - 1 - m, PLUM)
        cv.px(w // 2, h // 2, PGOLD)
    return cv


GUIDE_WORDS = {
    "portrait": "gold lines: the eyes (40% down) and the shoulders (80% down); plum: the top of the head, the chin, the middle",
    "scene": "gold line: the horizon or the eye line (60% down); plum: thirds",
    "shipview": "gold line: the horizon, %d px down, the same in every backdrop; plum: the middle, where her mast stands" % HORIZON,
    "ship_side": "gold line: the waterline; plum: the middle",
}
