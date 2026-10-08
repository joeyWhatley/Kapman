# -*- coding: utf-8 -*-
"""THE ROAD TO RELEASE (the user, 8 Oct 2026, night: "create an extensive and indepth analysis on where the game is and
every single step from here to alpha to beta to full release in percentage, 100 percent being released").

One model, two outputs:
  Design/ROADMAP.md                      the record, kept in the repository
  <out>.html (argument; default below)   the page to read (written for the Artifact tool: no doctype, head or body)

THE MODEL. The whole job of the first release (your ruling of 7 Oct: one life, 793 to about 805, twelve ports) is 100
points, shared out over seven areas and their parts by judgment (WEIGHTS ARE MY ESTIMATES, NOT MEASUREMENTS). Each
part has a share done today, set from counts made in the project on 8 Oct 2026 (FACTS below). What is left of each part
is shared out among the steps that finish it. So the steps add up to exactly what is missing, and each step's running
percentage follows from the order they are listed in.

To bring it up to date: change a part's `done`, or strike a step and move its share into `done`, and run it again.
    python Bake/plan/roadmap.py [out.html]
"""
import base64, html, io, os, sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
OUT_HTML = sys.argv[1] if len(sys.argv) > 1 else "G:/kapman_unity/art_try/kapman_roadmap.html"
PICTURE = "G:/kapman_unity/art_try/roadmap_look.jpg"          # (a capture of the game, made small: see the foot of this file)
COUNTED = "8 October 2026"

# ------------------------------------------------------------------------------------------------ what was counted
FACTS = {
    "code_lines": 113382, "code_files": 457, "tests_edit": 460, "tests_play": 25, "tests_skipped": 2,
    "days": 12, "commits": 330,
    "ports": 50, "goods": 140, "ship_classes": 18, "cards": 1400, "chains": 43, "chain_cards": 595, "choices": 4484,
    "people": 478, "sites": 610, "readings": 267, "riddles": 137, "chronicle": 582, "book_pages": 2631,
    "script_entries": 18529, "script_words": 504184, "unmerged_texts": 5259, "unmerged_words": 133588,
    "cards_placeholder": 408, "scope_cards": 523, "scope_cards_placeholder": 66, "scope_choices": 1597, "scope_ends": 819,
    "scope_words": 130234, "scope_people": 78, "later_cards": 877,
    "art_slots": 4815, "art_final": 0, "art_standin": 661, "tier": {1: 123, 2: 49, 3: 104, 4: 583, 5: 3956},
    "sounds": 39,
}
TWELVE = "Dorestat, Traiectum, Walichrum, Witla, Medemelacha, Doccinga, Daventre, Quentovic, Lundenwic, Sarre, Fordwic and Ripa"

# hours to draw one picture of a kind: MY GUESSES for somebody learning pixel art, to be put right after tier 1
DRAW_HOURS = {"hud": 0.25, "ui": 1.5, "good": 0.4, "harbour": 3.0, "plate": 2.0, "port_mark": 0.2, "port_view": 4.0,
              "portrait": 1.0, "ship_mark": 0.5, "ship_side": 1.5, "scene": 2.5, "card": 1.5}
TIER_KINDS = {1: {"hud": 114, "ui": 9},
              2: {"good": 5, "harbour": 12, "plate": 2, "port_mark": 12, "port_view": 12, "portrait": 4, "ship_mark": 1, "ship_side": 1},
              3: {"card": 4, "portrait": 70, "scene": 30}}
TIER_HOURS = {t: round(sum(n * DRAW_HOURS[k] for k, n in kinds.items())) for t, kinds in TIER_KINDS.items()}

# ------------------------------------------------------------------------------------------------ the areas and parts
# (id, name, points, share done, what stands, what is missing)
AREAS = [
    ("S", "The game's machinery", "The rules that run the world and the voyage.", [
        ("S1", "The world that runs by itself", 3.5, 0.92, "138 other ships trade, carry news and reach the market first. Seasons, fairs, 582 dated events.", "A twelve-year run on many seeds. The ten-year test is skipped today."),
        ("S2", "Sailing on the chart", 3.0, 0.88, "A course by water to every port of the release, tides, grounding, night, fog that lifts, five speeds, the measure.", "Oars. Distances that jump while she sails."),
        ("S3", "Trade", 3.5, 0.72, "Markets, four coinages, the toll, the word on a good, the wares window.", "Deals by the lot. A price told against where she lies. A hold that adds up."),
        ("S4", "The debt", 1.5, 0.88, "Owed to Wursing, due at Michaelmas; his man, the day, the boat's work, a clean stick (built tonight).", "Your ruling on the missed day. Hard and easy terms. The count's old chain."),
        ("S5", "The card engine", 3.5, 0.85, "1,400 cards with chances and memory. Speakers under their own names and faces, written ends shown (both tonight).", "Cards that wait a week, pressing cards that hold her, the list of what the release lets in."),
        ("S6", "People and standing", 3.0, 0.75, "Feelings, standing, deeds that travel as news, tongues that turn into English, five stats.", "Events that move the stats. One memory for a person in port and on a card."),
        ("S7", "Meetings at sea", 1.5, 0.85, "Traders hailed, the king's men, raiders.", "The raid as an hour of its own."),
        ("S8", "Risk to the boat", 2.5, 0.05, "Nothing can happen to her on the chart. The plan is written (149 KB).", "All of it: hull, water in her, the sail, the loss and what follows."),
        ("S9", "The crew aboard", 1.5, 0.65, "Hired, paid and fed; health and heart; four named men on the cards and now on the quay.", "Their seats in the rules. Hiring after the first morning."),
        ("S10", "Orders", 1.0, 0.40, "Can be taken and delivered.", "Broken three ways by the critic's count. The fix is drafted and compiled."),
        ("S11", "Saving", 2.0, 0.80, "Save, load, a save at each mooring, the open card comes back as it was.", "A save that still loads after an update."),
        ("S12", "The year and the life", 4.0, 0.05, "Written as data only: 40 winter cards, the heir's chain.", "Winter, the years after the first, age, death, the heir. Nothing ends a year or a life today."),
        ("S13", "What silver becomes", 2.0, 0.10, "Written as data: 40 fittings, 20 ships, 39 works, 455 things to unlock.", "The yard, a bigger hull, works and plots. No screen shows them."),
        ("S14", "A boat sent, a changed line", 1.0, 0.05, "81 flags in the cards say history changed; nothing reads them.", "Goods in another keel, one boat of your own sent, one line of the chronicle yours."),
        ("S15", "The chest", 1.0, 0.80, "267 readings, 137 riddles, 40 rune inscriptions.", "Reading with the crew."),
        ("S16", "The town", 0.5, 0.40, "Alehouse, minster, rest. 53 acts written, some wired.", "The rest of the acts."),
    ]),
    ("U", "The screen", "What the player sees and presses.", [
        ("U1", "The frame", 3.5, 0.85, "The Byzantium frame: top bar, twelve buttons, one side window with tabs, talk in the foot, the log.", "Small things that cover each other; labels that are catalogue words."),
        ("U2", "The side windows", 2.0, 0.70, "22 windows.", "Several still list what is not built."),
        ("U3", "The opening and the first hour", 1.5, 0.85, "Loader, title, the lender's questions, the boat, Ravn, the list of first jobs, the landing slip.", "A stake with a day inside the first hour."),
        ("U4", "The chart", 1.5, 0.75, "The whole sailing world as placeholder tiles, tools, four ways to colour it.", "Names by zoom; the dim world that inks as you hear of it."),
        ("U5", "Menus and settings", 1.5, 0.25, "A main menu and saves.", "No settings window and no credits page."),
        ("U6", "The ship window", 2.0, 0.80, "Sea, sky and weather from the game, the jetty, the crew, three looks.", "Its own sounds; the oars shown when she rows."),
    ]),
    ("W", "The words", "Everything read on the screen.", [
        ("W1", "The release's cards, written", 3.5, 0.87, "523 cards can open in the release.", "66 of them show a placeholder where a line should be. The words exist and were never brought in."),
        ("W2", "What came of each choice", 2.0, 0.51, "819 of the release's 1,597 choices have a written end.", "778 tell only \u201cYou did X.\u201d"),
        ("W3", "The people of the twelve ports", 1.0, 0.95, "78 people with their lines.", "Each port checked before it is let in."),
        ("W4", "Your own hand through the words", 6.0, 0.03, "The opening is your script. Wursing and Ravn carry your rulings.", "About 130,000 words of draft on the release's cards wait for you."),
        ("W5", "The Book and the readings", 1.5, 0.85, "2,631 pages; 267 old texts in translations that may ship.", "A check against the names the game now uses."),
    ]),
    ("P", "The pictures", "Yours to draw. The game runs on placeholders until you do.", [
        ("P1", "Tier 1: the frame", 3.0, 0.0, "123 slots, each with a description and a sized template.", "All 123."),
        ("P2", "Tier 2: the first hour", 3.5, 0.0, "49 slots.", "All 49."),
        ("P3", "Tier 3: the stand-ins that cover many", 4.5, 0.0, "104 slots.", "All 104."),
        ("P4", "The chart's own picture", 1.5, 0.15, "A placeholder map made from real ground data, the same detail everywhere.", "Your own home waters."),
        ("P5", "The store's pictures and film", 1.5, 0.0, "Listed with sizes (to be checked on Steam).", "All of them."),
        ("P6", "The loader, the list, the templates", 1.0, 1.0, "The game shows your picture the moment its file exists, else a stand-in, else a placeholder.", ""),
    ]),
    ("A", "The sound", "Only recordings that are free for anyone to use.", [
        ("A1", "The soundscape", 1.5, 0.70, "39 free recordings, each with its source; the chart and the ship window sound by where she is.", "The ship window's own sounds."),
        ("A2", "Music or chant", 1.5, 0.10, "A brief for a chant is written.", "Nothing that plays today may ship: the tracks are your own stand-ins."),
    ]),
    ("Y", "The project built the usual way", "Your order of 7 October: scenes, screens and flows that can be opened and seen.", [
        ("Y1", "Scenes, screens as files, graphs, prefabs", 4.0, 0.45, "The project of its own; the systems as scene objects; the frame and menus as UI files; four rules and the opening as graphs.", "Three scenes, 22 windows as files, more graphs, prefabs, settings as assets, clearing out."),
    ]),
    ("R", "Getting it out", "A build, the tests by other people, the law, the shop.", [
        ("R1", "A build that runs on another machine", 2.0, 0.10, "It runs in the editor and in the tests.", "No build script. Never tried on a second PC."),
        ("R2", "Speed and steadiness", 2.0, 0.10, "The same seed ends the same slowly and at speed 5.", "Nothing measured: frame rate, memory, a long night's run."),
        ("R3", "Balance", 3.0, 0.30, "The debt has bitten since the money fix of 7 October.", "No soak since. 766 card rewards wait for their cap."),
        ("R4", "Play-tests and what they bring", 5.0, 0.05, "The F2 notes tool. One critic's walk through pictures: 4 of 10.", "You, then strangers."),
        ("R5", "Licences and credits", 1.5, 0.40, "Three audits written on 7 October.", "Two map sources, an asset pack and a font have no licence on record."),
        ("R6", "Steam", 2.0, 0.0, "The steps are written down.", "Nothing is started."),
        ("R7", "The demo", 1.5, 0.0, "", "Not cut."),
    ]),
]

# ------------------------------------------------------------------------------------------------ the steps, in order
ME, YOU, BOTH = "me", "you", "both"
# who, size in words, my sittings (low, high), your hours (low, high), can start today, the parts it finishes (share of what is left)
STEPS = [
    # ---------------------------------------------------------------- phase 1: to the alpha
    (1, "", "Bring the five regions' words into the game", ME, "part of a sitting", (0.5, 0.5), (0, 0), True,
     "5,259 written texts (133,588 words) sit in five source files and were never put into the one file the game reads. So 408 of the 1,400 cards show \u201c[write: \u2026]\u201d where a line should be, 66 of them in the first release. They need the same passes the rest had: Dorestat, pennies, the bracket words.",
     [("W1", 1.0), ("W2", 0.03)]),
    (1, "", "Oars", ME, "one sitting", (1, 1), (0, 0), True,
     "Row 3 of your alpha list. An order to go under oars when the wind fails: slower, and it costs the men bread and strength. The ship window already shows the rowers.",
     [("S2", 0.5)]),
    (1, "", "The boat can be hurt and lost", ME, "5 to 7 sittings", (5, 7), (0, 0), True,
     "Row 4, and the widest gap before the alpha. The plan is written: hull, water in her and the sail; warned first, wounded before she is lost; the same gale dice as the other ships. For the alpha a loss shows \u201cShe is lost\u201d and the way back to a save. Your ruling: is that end enough for the alpha?",
     [("S8", 0.79), ("S5", 0.25)]),
    (1, "", "Deals by the lot, and the word told rightly", ME, "2 to 3 sittings", (2, 3), (0, 0), True,
     "Row 5. Buy and sell several at once with one settlement. A price is said against where she lies, by a named person, so many days old.",
     [("S3", 0.64)]),
    (1, "", "Cards that wait", ME, "2 sittings", (2, 2), (0, 0), True,
     "Row 7, your ruling of 7 October. An ordinary card waits seven game days, then takes one of its own options and says so in the log. A pressing card holds her where she is.",
     [("S5", 0.50)]),
    (1, "", "Orders that work", ME, "2 sittings", (2, 2), (0, 0), True,
     "Row 9. Taken or refused, brought in parts, paid like a sale, closed on their day. The code is drafted and was compiled outside Unity.",
     [("S10", 1.0)]),
    (1, "", "Events move the stats", ME, "one sitting", (1, 1), (0, 0), True,
     "Row 10, your ruling. A stat is a base plus marks that events put on and take off, each with a cause you can read and an end.",
     [("S6", 0.32)]),
    (1, "", "A stake inside the first hour, and a clean screen", ME, "2 sittings", (2, 2), (0, 0), True,
     "Row 17. A first order with a day: wine wanted at a named port. The first window stops listing things that are not built. Nothing covers anything. The top bar is no longer one run of words.",
     [("S4", 0.28), ("U1", 0.38), ("U2", 0.33), ("U3", 0.67)]),
    (1, "", "The play-through, a picture at each step", ME, "one sitting", (1, 1), (0, 0), False,
     "Row 18. The game played by the test's hands through the real buttons, from a new game to the Michaelmas reckoning. I look at every picture and write down what is wrong.",
     [("R4", 0.105)]),
    (1, "", "You play it", YOU, "3 to 5 hours", (0, 0), (3, 5), False,
     "From a new game to the reckoning, with F2 for your notes. The alpha is reached when you have looked and said so.",
     [("R4", 0.105)]),
    (1, "", "Mend what your notes say", ME, "1 to 2 sittings", (1, 2), (0, 0), False,
     "Your notes, worked through line by line.",
     [("R4", 0.05)]),
    # ---------------------------------------------------------------- phase 2: alpha to beta
    (2, "The world of the first release, closed", "Twelve ports open, the rest of the world dim", BOTH, "one sitting, and your nod", (1, 1), (0.5, 0.5), False,
     "Your ruling of 7 October, not yet carried out: today all 50 ports and all 1,400 cards are live. The release lets in the 523 cards that can open from 793 to 805 at the twelve ports, at sea and among the crew. The other 38 ports are heard of and dim.",
     [("S5", 0.08), ("W3", 1.0), ("U4", 0.27)]),
    (2, "The world of the first release, closed", "The nine chains that start and cannot end", BOTH, "one sitting, and your nod", (1, 1), (0.5, 0.5), False,
     "Nine story chains begin inside the release and need a port outside it to go on. The study of them says: leave five out, mend four so they end among the twelve.",
     [("S5", 0.17)]),
    (2, "The world of the first release, closed", "The other three homes", BOTH, "your ruling, then one sitting", (1, 1), (0.5, 0.5), False,
     "A Frank at Quentovic, an Englishman at Lundenwic and a Dane at Ripa can be chosen today. Each gets the Frisian's lender, day and coin under other names. Shut them for the first release, or give each its own.",
     [("S6", 0.08)]),
    (2, "A life, whole", "Winter is one board", ME, "4 to 6 sittings", (4, 6), (0, 0), False,
     "In the game's own book the sea closes at Martinmas, 11 November. One board reads the year back from the log, takes the winter keep and the tar, lets one thing be bought, and you name the year. 40 winter cards are written and no screen reads them.",
     [("S12", 0.24)]),
    (2, "A life, whole", "The years 794 to 805", ME, "2 to 3 sittings", (2, 3), (0, 0), False,
     "A second year has to be worth sailing. The dated events of those years reach her: the king's heavier penny in 794, the ports shut to the English until 796, the Saxon host, the exiles of 804. The world of traders is run for twelve years on many seeds and mended where it runs down.",
     [("S12", 0.13), ("S1", 0.28)]),
    (2, "A life, whole", "The house's own stories", BOTH, "2 to 3 sittings, and one ruling", (2, 3), (0.5, 0.5), False,
     "The rival, a marriage, a new ship and an heir: four chains written for these ports and years. Each is played through and mended. Your ruling: the count's old debt chain is written again for Wursing, or dropped.",
     [("S12", 0.10), ("S4", 0.36)]),
    (2, "A life, whole", "Hard and easy are the lender's terms", ME, "part of a sitting", (0.5, 0.5), (0, 0), False,
     "Your ruling of 4 October: difficulty is only what is owed and by when. Nothing sets it yet.",
     [("S4", 0.36)]),
    (2, "A life, whole", "Age, death and the heir", ME, "5 to 7 sittings", (5, 7), (0, 0), False,
     "The game's own book proposes: from 45 a draw each winter, warned a winter ahead; the heir takes the chart, the chest and the works, two thirds of the loose silver and half of each friend's feeling. It is the ending of the first release, and none of it is built.",
     [("S12", 0.42)]),
    (2, "A life, whole", "What follows a wreck", ME, "2 sittings", (2, 2), (0, 0), False,
     "Your ruling of 4 October: a wreck costs the boat, the cargo and maybe a man, and the house goes on. For the alpha a save is loaded. Here the ruling itself is built.",
     [("S8", 0.21), ("S12", 0.11)]),
    (2, "A life, whole", "The yard", ME, "4 to 6 sittings", (4, 6), (0, 0), False,
     "Mend her, fit her, buy a bigger hull. 40 fittings and 20 ships are written with their prices. Only the world's other ships read them.",
     [("S13", 0.5)]),
    (2, "A life, whole", "Works, plots and what they open", ME, "4 to 6 sittings", (4, 6), (0, 0), False,
     "Silver that sits earns nothing. A plot, a salt hearth, a share: 39 works and 455 things to unlock are written, and no screen shows them.",
     [("S13", 0.5), ("S16", 0.33)]),
    (2, "A life, whole", "A cask in another man's hold, then one boat sent", ME, "3 to 5 sittings", (3, 5), (0, 0), False,
     "The seed of the fellowship. Goods go by another keel and word of them comes back by ship. Then one boat of your own sails under an order. Many ships come after release.",
     [("S14", 0.68)]),
    (2, "A life, whole", "One changed line", ME, "one sitting", (1, 1), (0, 0), False,
     "81 flags in the cards say \u201chistory changed here\u201d and nothing reads them. One line a life goes into the chronicle in Byzantium.",
     [("S14", 0.32)]),
    (2, "Trade, the voyage and people", "The hold adds up, and money has one name", ME, "one sitting", (1, 1), (0, 0), False,
     "From the critic's walk: the hold's sums do not add up on the screen, and money has five names and rounds against the player.",
     [("S3", 0.36)]),
    (2, "Trade, the voyage and people", "The chart tells true", ME, "2 sittings", (2, 2), (0, 0), False,
     "Distances to ports jump while she sails. The bright line is the wrong one. Names come by zoom. A port inks when you hold a word from it.",
     [("S2", 0.5), ("U4", 0.73)]),
    (2, "Trade, the voyage and people", "The crew as four men in their seats", ME, "2 to 3 sittings", (2, 3), (0, 0), False,
     "Aldger, Brunger, Beornwine and Ravn hold their seats in the rules and not only on the cards: who is missed when a man is lost, hiring after the first morning, pay at Martinmas, what each says by the fire.",
     [("S9", 0.76), ("S6", 0.20)]),
    (2, "Trade, the voyage and people", "One memory for each person", ME, "one sitting", (1, 1), (0, 0), False,
     "Talk in port and a card about the same person keep two memories today.",
     [("S6", 0.24)]),
    (2, "Trade, the voyage and people", "The raid as the worst hour", ME, "one sitting", (1, 1), (0, 0), False,
     "Two ships and sixty men come up in the same small box as \u201cFish\u201d. A window of its own, a price on the tribute, odds on a fight.",
     [("S7", 1.0)]),
    (2, "Trade, the voyage and people", "Reading with the crew", ME, "2 to 3 sittings", (2, 3), (0, 0), False,
     "Your wish of 3 October: a crewman reads part of a rune stick or a verse and you finish it. Written up, not built.",
     [("S15", 1.0), ("S9", 0.24)]),
    (2, "Trade, the voyage and people", "The town's acts, whole", ME, "2 sittings", (2, 2), (0, 0), False,
     "53 acts are written: alms, the keeper, offerings, the fire. Some are wired. Board games wait until after release.",
     [("S16", 0.67), ("S6", 0.16)]),
    (2, "The screen, finished", "Every label a sailor's word", BOTH, "one sitting, and your eye", (1, 1), (1, 1), False,
     "Catalogue labels, rules that leak onto cards, things said twice, museum words and modern measures.",
     [("U1", 0.62)]),
    (2, "The screen, finished", "Each side window finished", ME, "2 to 3 sittings", (2, 3), (0, 0), False,
     "House, Ship, Chest, Crew, Journey and the rest hold what the release needs and nothing that says \u201clater\u201d.",
     [("U2", 0.67)]),
    (2, "The screen, finished", "Settings, and the credits", ME, "2 sittings", (2, 2), (0, 0), False,
     "There is no settings window: sound levels, the size of the text, window or full screen, what the clock stops for, the keys. And a page of credits.",
     [("U5", 1.0)]),
    (2, "The screen, finished", "The ship window finished", ME, "2 sittings", (2, 2), (0, 0), False,
     "Its own sounds (38 more free recordings are credited and wait outside the project), the oars shown when she rows, the look chosen in settings.",
     [("U6", 1.0), ("A1", 0.55)]),
    (2, "The screen, finished", "The opening's last pieces", ME, "part of a sitting", (0.5, 0.5), (0, 0), False,
     "The keys of the lender's questions, the feast on the first morning, the name as a drawn sign.",
     [("U3", 0.33)]),
    (2, "The words", "A written end for every choice of the release", ME, "5 to 7 sittings", (5, 7), (0, 0), False,
     "778 of the release's 1,597 choices tell only \u201cYou did X.\u201d I draft the ends as placeholders, about 35,000 words, in batches you can read. Dorestat's own cards and the toll man come first.",
     [("W2", 0.97)]),
    (2, "The words", "Your first reading: the first hour", YOU, "two evenings", (0, 0), (6, 8), True,
     "Read aloud and fix about sixty cards: the opening, the lender's man, the day, Ravn, the first ports, the crew. Nobody else can do this, and the critics hung a whole star on it.",
     [("W4", 0.12)]),
    (2, "The words", "Your second reading: the rest of the release", YOU, "45 to 60 hours", (0, 0), (45, 60), False,
     "523 cards and the lines of 78 people: about 130,000 words. Read aloud and fixed, that is 45 to 60 hours. Written again from nothing it would be several hundred.",
     [("W4", 0.76)]),
    (2, "The words", "Your voice set down as rules", BOTH, "part of a sitting", (0.5, 0.5), (1, 1), False,
     "After your first reading, what you cut becomes the rules for every draft I write after it.",
     [("W4", 0.12)]),
    (2, "The words", "The Book checked", ME, "one sitting", (1, 1), (0, 0), False,
     "The Book has 2,631 pages. Those of the release are checked against the names the game now uses, and each reading against its source.",
     [("W5", 1.0)]),
    (2, "The pictures", "Tier 1: the frame", YOU, "about %d hours" % TIER_HOURS[1], (0, 0), (TIER_HOURS[1], TIER_HOURS[1]), True,
     "123 pictures: 114 small signs of 16 pixels and 9 pieces of the frame. They change the look of every screen at once.",
     [("P1", 1.0)]),
    (2, "The pictures", "Tier 2: the first hour", YOU, "about %d hours" % TIER_HOURS[2], (0, 0), (TIER_HOURS[2], TIER_HOURS[2]), True,
     "49 pictures: twelve harbours, twelve views, twelve marks, four faces, five goods, the boat.",
     [("P2", 1.0)]),
    (2, "The pictures", "Tier 3: the stand-ins that cover many", YOU, "about %d hours" % TIER_HOURS[3], (0, 0), (TIER_HOURS[3], TIER_HOURS[3]), True,
     "104 pictures: a face for each of 70 trades, 30 kinds of place, four kinds of card. Each stands for many until its own is drawn.",
     [("P3", 1.0)]),
    (2, "The pictures", "Your own home waters", YOU, "not known yet", (0, 0), (0, 0), True,
     "The chart from the Rhine mouth to the Thames is a placeholder made from data. Yours replaces it tile by tile. It is the largest single picture in the game and I cannot guess its hours.",
     [("P4", 1.0)]),
    (2, "The pictures", "The store's pictures and a short film", BOTH, "about 35 hours, and one sitting for the takes", (1, 1), (35, 35), True,
     "Five capsules, four library pictures, three icons, five true screenshots, a film under a minute. The page cannot go up without them.",
     [("P5", 1.0)]),
    (2, "The sound", "Fill the gaps in the soundscape", ME, "one sitting", (1, 1), (0, 0), False,
     "Only free recordings, each with its source written down. 39 are in the game.",
     [("A1", 0.45)]),
    (2, "The sound", "Music: your ruling, then the work", YOU, "a ruling", (0, 0), (1, 1), True,
     "Nothing that plays today may ship: the tracks are your own stand-ins. Your ruling of 4 October was no music. Since then a brief for a chant was written. None, old recordings in the public domain, or a commission.",
     [("A2", 1.0)]),
    (2, "The project built the usual way", "Boot, Title and Game as scenes", ME, "2 sittings", (2, 2), (0, 0), False,
     "Three scenes you can open. The ship's stage as a prefab. The ports as objects.",
     [("Y1", 0.20)]),
    (2, "The project built the usual way", "The chart, the market and the windows as UI files", ME, "6 to 8 sittings", (6, 8), (0, 0), False,
     "The chart, the market, the notes and each of the 22 side windows become files you can open and move in UI Builder. 13 screens are files today.",
     [("Y1", 0.30)]),
    (2, "The project built the usual way", "More rules and flows as graphs", ME, "5 to 7 sittings", (5, 7), (0, 0), False,
     "Four rules and fifteen flows are graphs. Next: hand money, standing, what a card's choice does, the debt and the boat's work; the title, the places, the music's cues.",
     [("Y1", 0.25)]),
    (2, "The project built the usual way", "Prefabs, animation, settings as assets, clearing out", ME, "4 sittings", (4, 4), (0, 0), False,
     "The ship, a crewman, a port's mark, the rain. The rowers' motion as clips. What is tuned by hand as assets. Dead code out, and one page that says where everything is.",
     [("Y1", 0.25)]),
    (2, "Ready to ship", "A build that starts on a clean machine", BOTH, "2 sittings, and your second PC", (2, 2), (1, 1), False,
     "There is no build script, and no build has been tried on another machine. A script, a version stamp, a log of faults, and your name in place of \u201cDefaultCompany\u201d.",
     [("R1", 0.78)]),
    (2, "Ready to ship", "Saves that outlive a new version", ME, "one sitting", (1, 1), (0, 0), False,
     "A save made today must load after an update. A version step with its test, and the folder Steam's cloud will keep.",
     [("S11", 1.0)]),
    (2, "Ready to ship", "The money, tuned", BOTH, "3 sittings, and your eye on 72 sums", (3, 3), (2, 2), False,
     "A plain trader should pay the debt in 40 to 70 runs of 100. 200 seeded seasons are run. Card rewards get their cap: 766 sums, 72 of them marked for you.",
     [("R3", 0.67), ("S1", 0.72)]),
    (2, "Ready to ship", "Licences closed", BOTH, "2 sittings, and your receipts", (2, 2), (2, 2), False,
     "On record the licence of two map sources is unknown (Copernicus, GEBCO), one asset pack carries no licence note, the Hearth typeface has none written down, and the stand-in pictures from Wikimedia must not ship. Each is checked and written into one credits file.",
     [("R5", 0.83)]),
    (2, "Ready to ship", "Steam: the paperwork and the fee", YOU, "a few hours; a week or two to clear", (0, 0), (6, 6), True,
     "The Steamworks account, the tax interview, 100 dollars. A wait of 30 days runs from the fee, so this is worth starting now. (Steam's figures are from my note of 7 October and want checking on its own pages.)",
     [("R6", 0.24)]),
    (2, "Ready to ship", "Steam: the page, \u201cComing soon\u201d", BOTH, "about 10 hours, and one sitting for drafts", (1, 1), (10, 10), False,
     "The name typed without the long mark. The short and long text, the tags, the survey (the slave trade is in the game; an AI model wrote code and draft text; the art is yours). Wishlists gather from this day.",
     [("R6", 0.36)]),
    (2, "Ready to ship", "The demo", BOTH, "2 sittings, and an evening of yours", (2, 2), (3, 3), False,
     "The first hour to the Michaelmas reckoning, free, keeping its save. Steam's Next Fest is the best single chance a small game has to be seen.",
     [("R7", 1.0)]),
    (2, "Ready to ship", "A whole life played by the test's hands", ME, "2 sittings", (2, 2), (0, 0), False,
     "793 to 805 at speed, a picture at each turn of the year, every one looked at. The beta is reached when a whole life plays with nothing missing.",
     [("R4", 0.13)]),
    # ---------------------------------------------------------------- phase 3: beta to release
    (3, "", "Three strangers play it", BOTH, "2 to 3 weeks; you find them", (0.5, 0.5), (15, 15), False,
     "Each with a build and the F2 notes. The gate the game's own book sets: two of three begin a second year unasked.",
     [("R4", 0.32)]),
    (3, "", "Mend what they found", ME, "4 to 6 sittings", (4, 6), (0, 0), False,
     "Their notes, in order of how many of them hit the same thing.",
     [("R4", 0.24)]),
    (3, "", "Speed and steadiness", BOTH, "3 sittings, and your second PC", (3, 3), (2, 2), False,
     "Frame rate on a modest PC, memory, a whole life at speed 5 overnight with no fault, the size of a save, what the ship window costs.",
     [("R2", 1.0)]),
    (3, "", "The last tuning", ME, "one sitting", (1, 1), (0, 0), False,
     "The first hour's prices, the debt's bite and the cards' rewards, from what the strangers did.",
     [("R3", 0.33)]),
    (3, "", "The release build", ME, "one sitting", (1, 1), (0, 0), False,
     "The final build, with the credits and the licences in it.",
     [("R1", 0.22), ("R5", 0.17)]),
    (3, "", "Steam: the build reviewed, the day set", YOU, "a few hours; Valve's review takes up to a week", (0.5, 0.5), (4, 4), False,
     "Upload, Valve's review, the price, the day. The page must have been public for two weeks by then.",
     [("R6", 0.40)]),
    (3, "", "Your last look", YOU, "an evening", (0, 0), (3, 3), False,
     "From a new game, on the very build that will ship.",
     [("R4", 0.05)]),
]

# ------------------------------------------------------------------------------------------------ done since the count
# A step done: its title, when, and a line on what came of it. Its points move into "now".
DONE = {
    "Bring the five regions' words into the game": ("8 Oct, night", "5,259 texts are in the game, after the same passes the rest had. 14 cards of one later chain (the Rhineland, 879 to 886) still have no lines: their words were never written."),
    "Events move the stats": ("8 Oct, night", "A stat is a base and the marks on it. Cards put them on with a verb, 18 kinds of known deed by a table, hunger and sickness by themselves. All marks together move a stat by no more than two. Yourself shows each with its cause and its end."),
    "Cards that wait": ("8 Oct, night", "A card left unanswered with the clock running waits a week, then takes one of its own options and the log says which. A pressing card (44 of them) waits hours, and 36 of those hold her where she is meanwhile. Left to wait, the card says under itself how long and what then. A harbour card goes its own way when she casts off."),
    "Oars": ("8 Oct, night", "The order Out oars stands beside her course in the Voyage window. The sail comes down and the men row hard: her full speed under oars whatever the wind, for the six hours that are in them. It eats bread and strength, and the strength comes back with rest. Left alone she goes as before."),
}
# What a part says once a step that finishes it is done: step title -> part -> (what stands, what is missing)
PART_AFTER = {
    "Bring the five regions' words into the game": {"W1": ("All 523 cards of the release have their words in the game.", "")},
    "Events move the stats": {"S6": ("Feelings, standing, deeds that travel as news, tongues that turn into English, five stats that events move.", "One memory for a person in port and on a card.")},
}
# A finding mended by a step: the finding's first words -> the step's title
MENDED = {
    "5,259 written texts never reached the game.": "Bring the five regions' words into the game",
}

PHASES = {1: ("To the alpha", "Your own list of 7 October: the eighteen basics built and joined, in one build played from a new game to the first reckoning, with nothing in it that says \u201cnot yet\u201d, and you have looked at the play-through."),
          2: ("From the alpha to the beta", "My definition, for you to change: everything the first release holds is in the game and can be played from the first morning to the heir. The twelve ports are closed off, every choice has its end, your words and your pictures are in, there is a settings window, a build runs on another machine, the store page is up and the demo is cut. What is left is other people playing it."),
          3: ("From the beta to the release", "The first release is out on Steam: one life, 793 to about 805, on twelve ports, with the whole world dim on the chart. That is your ruling of 7 October, and it is the 100 on this scale.")}

ALPHA_ROWS = [
    (1, "A new game: people, faith, past; home by people", "built"), (2, "The clock: a pause and five speeds", "built"),
    (3, "Sailing: a course, cast off, sail or oars, weather that matters", "built but for oars"), (4, "The boat can be hurt and lost", "not built"),
    (5, "Trading by the lot, a price told rightly and by whom, the toll", "in part"), (6, "A debt with a day and an end", "built tonight; one ruling open"),
    (7, "Cards: they wait a week, pressing ones hold her, a written end, a name and a face", "in part: ends, names and faces tonight"), (8, "Meetings at sea", "built"),
    (9, "Orders taken, carried in parts, paid fairly", "broken; fix drafted"), (10, "People: talk, the old tongue, stats that events move", "built but for events"),
    (11, "The frame as ruled", "built"), (12, "The wiki", "built"), (13, "The 3D view of the ship", "built"), (14, "Saving", "built"),
    (15, "Sound on the chart", "built"), (16, "Your pictures show, with stand-ins", "built"), (17, "The first hour: a stake inside it, a clean screen", "in part"),
    (18, "It holds together: tests pass; a play-through you have looked at", "tests pass; play-through to do"),
]

FOUND = [
    ("5,259 written texts never reached the game.", "133,588 words for the Baltic, the Danes and Norway, south-east England, Northumbria and part of the home rivers sit in five source files. The file the game reads never got them. 408 cards show \u201c[write: \u2026]\u201d where a line should be. It is the first step below and takes part of a sitting."),
    ("Not one final picture exists.", "The art list holds 4,815 slots; the game shows 661 stand-ins and drawn placeholders. Your ruling made the list a catalogue: 276 pictures are wanted for the first release, 583 more are optional, 3,956 are for later."),
    ("Half the release's choices have no written end.", "Since tonight a card answered tells what came of it. For 778 of the release's 1,597 choices nothing is written, so they still say only what you did."),
    ("Nothing ends a year or a life.", "No winter, no ageing, no heir. The first release ends with the heir. 40 winter cards and the heir's chain are written and read by no code."),
    ("Nothing can happen to the boat.", "On the chart she cannot be hurt, fill or be lost. It is row 4 of the alpha and the largest piece of building before it."),
    ("The first release's world is not closed.", "You ruled twelve ports. All 50 ports and all 1,400 cards are live, and nine chains can begin and never end."),
    ("A good deal is written and waits for a screen.", "40 fittings and 20 ships for the yard, 39 works, 455 things to unlock, 10 town games, 530 sayings, 40 rare events, 17 hours of history. No code reads most of them."),
    ("The game has never left the editor.", "No build script, no settings window, no credits page, and the maker's name in the build is still \u201cDefaultCompany\u201d."),
    ("The later game is far ahead of the first.", "877 of the 1,400 cards, 38 more ports with their people and the chains that run to 1066 are drafted already. They are not in the 100, and they are a temptation to widen it."),
    ("The machinery is well guarded.", "Of 485 tests, 483 pass and two are skipped. The same seed ends the same slowly and at speed 5. A save comes back into the very card that was open."),
]

RULINGS = [
    "What follows a missed Michaelmas. I chose \u201cshe works for him\u201d tonight. Keep it or change it.",
    "The name of the lender's man. \u201cBubo\u201d is a placeholder.",
    "The count's old debt chain: written again for Wursing, or dropped.",
    "For the alpha, is \u201cShe is lost\u201d and a save loaded enough? The heir after a wreck comes later.",
    "The other three homes: shut for the first release, or each made its own.",
    "Music: none, old recordings in the public domain, or a commission.",
    "Early Access or a plain release, and the price.",
    "The name the build carries in place of \u201cDefaultCompany\u201d.",
    "How far into the optional tier 4 (583 pictures) you want to go before release.",
]

RISKS = [
    ("Whether a stranger wants a second hour.", "The last critic gave 4 of 10 on 8 October. Five of his changes are in since. If the first hour is not good with real players, steps come back."),
    ("Your drawing pace.", "The hours for the pictures are my guesses. After tier 1 we shall know."),
    ("The money.", "The debt has bitten since 7 October, and there has been no soak since."),
    ("Licences.", "If one of the two unknown map sources may not ship, the map has to be made again without it."),
    ("Widening.", "The later ages are drafted and tempting. Every card let in before release is one more to read."),
    ("My allowance.", "About 130 sittings of building. This week ran at three to five a day. A week's allowance went in two days on 7 October."),
]

LATER = [
    "The second life and the age of 834, when a port can burn.",
    "The other 38 ports as water you can sail: 877 cards and their people are drafted, and some ports still wait for their rivers.",
    "Six keels: the fellowship, and the game as a fleet manager.",
    "Wars the world makes, and the hinges of history turned by news brought in time.",
    "Other starts, in other ports and ages.",
    "The road to 1066: the whole span in about 230 hours at the five speeds.",
    "The other 3,956 pictures, and the 583 optional ones.",
    "Board games with the crew, and more peoples and faiths.",
]

# ------------------------------------------------------------------------------------------------ the sums
parts = {}
for aid, aname, ablurb, items in AREAS:
    for pid, name, pts, done, stands, missing in items:
        parts[pid] = dict(area=aid, name=name, pts=pts, done=done, left=pts * (1.0 - done), stands=stands, missing=missing)
assert abs(sum(p["pts"] for p in parts.values()) - 100.0) < 1e-9, sum(p["pts"] for p in parts.values())
claimed = {pid: 0.0 for pid in parts}
for s in STEPS:
    for pid, share in s[9]:
        claimed[pid] += share
for pid, c in claimed.items():
    if parts[pid]["left"] > 1e-9:
        assert abs(c - 1.0) < 1e-6, (pid, c)
    else:
        assert c == 0.0, (pid, c)

COUNT = sum(p["pts"] * p["done"] for p in parts.values())          # (as it stood on the day of the count)
for title in DONE:
    assert any(st[2] == title for st in STEPS), "no such step: " + title
for p_ in parts.values():
    p_["since"] = 0.0
all_steps = []
for i, (phase, group, title, who, size, sit, hrs, today, text, claims) in enumerate(STEPS, 1):
    pts = sum(parts[pid]["left"] * share for pid, share in claims)
    done = title in DONE
    if done:
        for pid, share in claims:
            parts[pid]["since"] += parts[pid]["left"] * share
            if title in PART_AFTER and pid in PART_AFTER[title]:
                parts[pid]["stands"], parts[pid]["missing"] = PART_AFTER[title][pid]
    all_steps.append(dict(n=i, phase=phase, group=group, title=title, who=who, size=size, sit=sit, hrs=hrs, today=today, text=text, pts=pts, done=done,
                          areas=sorted({parts[pid]["area"] for pid, _ in claims})))
NOW = COUNT + sum(s_["pts"] for s_ in all_steps if s_["done"])
run = NOW
for s_ in all_steps:
    if not s_["done"]:
        run += s_["pts"]
    s_["run"] = run
assert abs(run - 100.0) < 1e-6, run
steps = [s_ for s_ in all_steps if not s_["done"]]
done_steps = [s_ for s_ in all_steps if s_["done"]]
ALPHA = max([s_["run"] for s_ in all_steps if s_["phase"] == 1])
BETA = max([s_["run"] for s_ in all_steps if s_["phase"] == 2])
for p_ in parts.values():
    p_["share"] = (p_["pts"] * p_["done"] + p_["since"]) / p_["pts"]
area_sum = {aid: (sum(parts[p[0]]["pts"] for p in items), sum(parts[p[0]]["pts"] * parts[p[0]]["share"] for p in items)) for aid, _, _, items in AREAS}


def phase_sum(ph):
    ss = [s for s in steps if s["phase"] == ph]
    return dict(n=len(ss), pts=sum(s["pts"] for s in ss), sit=(sum(s["sit"][0] for s in ss), sum(s["sit"][1] for s in ss)),
                hrs=(sum(s["hrs"][0] for s in ss), sum(s["hrs"][1] for s in ss)))


PS = {ph: phase_sum(ph) for ph in (1, 2, 3)}
SIT = (sum(PS[p]["sit"][0] for p in PS), sum(PS[p]["sit"][1] for p in PS))
HRS = (sum(PS[p]["hrs"][0] for p in PS), sum(PS[p]["hrs"][1] for p in PS))
YOURS = sum(s["pts"] for s in steps if s["who"] == YOU)
MINE = sum(s["pts"] for s in steps if s["who"] == ME)
OURS = sum(s["pts"] for s in steps if s["who"] == BOTH)
WHO = {ME: "Mine", YOU: "Yours", BOTH: "Both of us"}


def pct(x, d=0):
    return ("%." + str(d) + "f") % x


def rng(a, b, unit):
    a, b = (int(a) if float(a).is_integer() else a), (int(b) if float(b).is_integer() else b)
    return ("%s %s" % (a, unit)) if a == b else ("%s to %s %s" % (a, b, unit))


def num(n):
    return "{:,}".format(n)


# ------------------------------------------------------------------------------------------------ the record (markdown)
def markdown():
    F = FACTS
    o = []
    o.append("# The road to release")
    o.append("")
    o.append("The user, %s, night: \"can you create an extensive and indepth analysis on where the game is and every single step from here to alpha to beta to full release in percentage 100 percent being released\"." % COUNTED)
    o.append("")
    o.append("Made by `Bake/plan/roadmap.py` from counts taken in the project that night. To bring it up to date, change the model in that file and run it again. **The weights are my estimates, not measurements**, and a point is a share of the work, not of the calendar.")
    o.append("")
    o.append("## The short of it")
    o.append("")
    o.append("| | Percent |")
    o.append("|---|---|")
    o.append("| **Now** | **%s** |" % pct(NOW, 1 if done_steps else 0))
    o.append("| The alpha | %s |" % pct(ALPHA))
    o.append("| The beta | %s |" % pct(BETA))
    o.append("| Released | 100 |")
    o.append("")
    o.append("- **100 is the first release**, as you ruled on 7 October: one life, 793 to about 805, on twelve ports (%s), with the whole world dim on the chart. The later ages are not in it." % TWELVE)
    o.append("- %d steps are left: %d to the alpha, %d more to the beta, %d to the release." % (len(steps), PS[1]["n"], PS[2]["n"], PS[3]["n"]))
    o.append("- Of the %s points still to do, %s are mine to build, %s are yours alone and %s need both of us." % (pct(100 - NOW), pct(MINE), pct(YOURS), pct(OURS)))
    o.append("- My part is about %s. Yours is about %s, and the hours of your own home waters on top. **The release date is set by your drawing and your reading, not by the building.**" % (rng(round(SIT[0]), round(SIT[1]), "sittings"), rng(round(HRS[0]), round(HRS[1]), "hours")))
    o.append("")
    if done_steps:
        o.append("## Done since the count of %s" % COUNTED)
        o.append("")
        o.append("The count stood at %s. %d of the %d steps are done since, worth %s points." % (pct(COUNT, 1), len(done_steps), len(all_steps), pct(NOW - COUNT, 1)))
        o.append("")
        o.append("| No. | Step | When | What came of it | Points |")
        o.append("|---|---|---|---|---|")
        for s in done_steps:
            when, note = DONE[s["title"]]
            o.append("| %d | %s | %s | %s | +%s |" % (s["n"], s["title"], when, note, pct(s["pts"], 2)))
        o.append("")
    o.append("## Where the game is, part by part")
    o.append("")
    for aid, aname, ablurb, items in AREAS:
        tot, done = area_sum[aid]
        o.append("### %s: %s of %s points (%s%%)" % (aname, pct(done, 1), pct(tot, 0), pct(100 * done / tot)))
        o.append("")
        o.append(ablurb)
        o.append("")
        o.append("| Part | Points | Done | What stands | What is missing |")
        o.append("|---|---|---|---|---|")
        for pid, name, pts, d, stands, missing in items:
            o.append("| %s | %s | %s%% | %s | %s |" % (name, pct(pts, 1), pct(100 * parts[pid]["share"]), parts[pid]["stands"] or "", parts[pid]["missing"] or ""))
        o.append("")
    o.append("## What the counting turned up")
    o.append("")
    for i, (head, body) in enumerate(FOUND, 1):
        mended = " **Mended since (%s).**" % DONE[MENDED[head]][0] if MENDED.get(head) in DONE else ""
        o.append("%d. **%s** %s%s" % (i, head, body, mended))
    o.append("")
    o.append("## Your alpha list, row by row")
    o.append("")
    o.append("| Row | Basic | Today |")
    o.append("|---|---|---|")
    for n, name, state in ALPHA_ROWS:
        o.append("| %d | %s | %s |" % (n, name, state))
    o.append("")
    o.append("## Every step")
    o.append("")
    o.append("A sitting is one piece of work built, tested by the full suites, looked at in pictures from the game and committed. Tonight had five. The groups inside a phase can run side by side: your pictures and the Steam paperwork can begin today.")
    o.append("")
    for ph in (1, 2, 3):
        name, what = PHASES[ph]
        start = NOW if ph == 1 else ALPHA if ph == 2 else BETA
        end = ALPHA if ph == 1 else BETA if ph == 2 else 100.0
        o.append("### %s: %s to %s" % (name, pct(start, 1), pct(end, 1)))
        o.append("")
        o.append(what)
        o.append("")
        o.append("%d steps, %s points. Mine: %s. Yours: %s." % (PS[ph]["n"], pct(PS[ph]["pts"], 1), rng(round(PS[ph]["sit"][0]), round(PS[ph]["sit"][1]), "sittings"), rng(round(PS[ph]["hrs"][0]), round(PS[ph]["hrs"][1]), "hours")))
        o.append("")
        o.append("| No. | Step | Who | Size | Adds | Then at |")
        o.append("|---|---|---|---|---|---|")
        group = None
        for s in steps:
            if s["phase"] != ph:
                continue
            if s["group"] and s["group"] != group:
                group = s["group"]
                o.append("| | **%s** | | | | |" % group)
            o.append("| %d | **%s.** %s | %s | %s | +%s | %s |" % (s["n"], s["title"], s["text"], WHO[s["who"]], s["size"], pct(s["pts"], 2), pct(s["run"], 1)))
        o.append("")
    o.append("## What only you can do")
    o.append("")
    o.append("**Rulings wanted**")
    o.append("")
    for i, r in enumerate(RULINGS, 1):
        o.append("%d. %s" % (i, r))
    o.append("")
    o.append("**The pictures of the first release** (hours are my guesses for somebody learning pixel art)")
    o.append("")
    o.append("| Tier | Pictures | Hours |")
    o.append("|---|---|---|")
    for t, label in ((1, "1. The frame"), (2, "2. The first hour"), (3, "3. Stand-ins that cover many")):
        o.append("| %s | %d | about %d |" % (label, F["tier"][t], TIER_HOURS[t]))
    o.append("| 4. One by one (optional) | %d | not in the 100 |" % F["tier"][4])
    o.append("| 5. Later | %s | not in the 100 |" % num(F["tier"][5]))
    o.append("")
    o.append("## What sets the pace")
    o.append("")
    o.append("- **My part:** about %s. This week ran at three to five a day while you were here. That is weeks." % rng(round(SIT[0]), round(SIT[1]), "sittings"))
    o.append("- **Your part:** about %s, and your home waters on top. At ten hours a week that is most of a year. At twenty it is five to six months." % rng(round(HRS[0]), round(HRS[1]), "hours"))
    o.append("- **Steam's own waits:** 30 days after the fee, two weeks of \u201cComing soon\u201d, two reviews. Six weeks at the least, even for a finished game.")
    o.append("- The note on Steam of 7 October said spring to summer 2027 for the first release. This count agrees with it.")
    o.append("")
    o.append("## What could move these numbers")
    o.append("")
    for head, body in RISKS:
        o.append("- **%s** %s" % (head, body))
    o.append("")
    o.append("## After the release (not in the 100)")
    o.append("")
    for l in LATER:
        o.append("- %s" % l)
    o.append("")
    o.append("## How the numbers were made")
    o.append("")
    o.append("The whole job of the first release is 100 points. I shared them over seven areas and %d parts by judgment. Each part has a share done, set from what I counted: %s lines of code in %d files, %d tests, %s cards, %s script entries, %s art slots, and the design papers. What is left of each part is shared among the steps that finish it, so the steps add up to exactly what is missing. Read every figure as give or take five." % (len(parts), num(F["code_lines"]), F["code_files"], F["tests_edit"] + F["tests_play"], num(F["cards"]), num(F["script_entries"]), num(F["art_slots"])))
    o.append("")
    return "\n".join(o) + "\n"


# ------------------------------------------------------------------------------------------------ the page (html)
CSS = """
/* Layout: one reading column; a wide rail of progress under the heading; the steps as a ledger whose left margin
   carries the running percentage. Colours and the pixel display face are the game's own (Byzantium on sage). */
:root {
  --ground: #eef2e6; --panel: #dfe6d2; --field: #cbd6bd; --line: #aab89c;
  --ink: #2a1230; --soft: #5c4763;
  --byz: #702963; --plum: #4a1c4a;
  --gold: #8a5d12; --gold-fill: #d9a441;
  --grass: #3d6a2a;
  --rail-done: #702963; --rail-alpha: #a2569a; --rail-beta: #cfa9c6; --rail-rest: #e4d6e0;
  --who-me: #702963; --who-you: #9a6a12; --who-both: #3d6a2a;
  --display: "Pixelify Sans", "Lucida Console", "Courier New", monospace;
  --body: "Atkinson Hyperlegible", "Segoe UI", system-ui, sans-serif;
}
@media (prefers-color-scheme: dark) {
  :root:not([data-theme="light"]) {
    --ground: #140c18; --panel: #20142a; --field: #2d1d38; --line: #4d3859;
    --ink: #eee6ef; --soft: #c2b0c7;
    --byz: #d39ac6; --plum: #702963;
    --gold: #e6bb62; --gold-fill: #d9a441;
    --grass: #a9cc88;
    --rail-done: #d39ac6; --rail-alpha: #9a4a8a; --rail-beta: #5d2f58; --rail-rest: #36203d;
    --who-me: #d39ac6; --who-you: #e6bb62; --who-both: #a9cc88;
    color-scheme: dark;
  }
}
:root[data-theme="dark"] {
  --ground: #140c18; --panel: #20142a; --field: #2d1d38; --line: #4d3859;
  --ink: #eee6ef; --soft: #c2b0c7;
  --byz: #d39ac6; --plum: #702963;
  --gold: #e6bb62; --gold-fill: #d9a441;
  --grass: #a9cc88;
  --rail-done: #d39ac6; --rail-alpha: #9a4a8a; --rail-beta: #5d2f58; --rail-rest: #36203d;
  --who-me: #d39ac6; --who-you: #e6bb62; --who-both: #a9cc88;
  color-scheme: dark;
}
* { box-sizing: border-box; }
body { margin: 0; background: var(--ground); color: var(--ink); font: 17px/1.55 var(--body); }
.page { max-width: 47rem; margin: 0 auto; padding-inline: 16px; padding-block: 28px 72px; display: flex; flex-direction: column; gap: 44px; }
section { display: flex; flex-direction: column; gap: 16px; min-width: 0; }
h1, h2, h3 { font-family: var(--display); font-weight: 700; text-wrap: balance; }
.num, .area-pct, .p-pct, .run, .span { font-family: var(--body); font-weight: 700; font-variant-numeric: tabular-nums; }
h1 { font-size: clamp(2rem, 7vw, 3rem); line-height: 1.05; margin: 0; color: var(--byz); letter-spacing: .01em; }
h2 { font-size: 1.55rem; line-height: 1.15; margin: 0; color: var(--byz); padding-bottom: 6px; border-bottom: 3px solid var(--byz); }
h3 { font-size: 1.15rem; line-height: 1.2; margin: 0; }
p { margin: 0; max-width: 65ch; }
.lede { font-size: 1.12rem; }
.quiet { color: var(--soft); font-size: .92rem; }
.eyebrow { font-family: var(--display); font-weight: 500; text-transform: uppercase; letter-spacing: .12em; font-size: .8rem; color: var(--gold); }
strong { font-weight: 700; }
a { color: var(--byz); }
:focus-visible { outline: 3px solid var(--gold-fill); outline-offset: 2px; }

/* the rail */
.rail { display: flex; flex-direction: column; gap: 14px; }
.bar { position: relative; height: 34px; background: var(--rail-rest); border: 3px solid var(--plum); display: flex; }
.bar span { display: block; height: 100%; }
.bar .done { background: var(--rail-done); }
.bar .alpha { background: var(--rail-alpha); }
.bar .beta { background: var(--rail-beta); }
.marks { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 10px; }
.mark { background: var(--panel); border-left: 8px solid var(--rail-rest); padding: 8px 10px 9px; min-width: 0; }
.mark.now { border-left-color: var(--rail-done); }
.mark.alpha { border-left-color: var(--rail-alpha); }
.mark.beta { border-left-color: var(--rail-beta); }
.mark.out { border-left-color: var(--gold-fill); }
.mark .num { font-size: clamp(1.8rem, 7vw, 2.6rem); line-height: 1; }
.mark .what { font-size: .88rem; color: var(--soft); }
@media (max-width: 560px) { .marks { grid-template-columns: repeat(2, minmax(0, 1fr)); } }

figure { margin: 0; display: flex; flex-direction: column; gap: 6px; }
figure img { display: block; max-width: 100%; height: auto; border: 3px solid var(--plum); image-rendering: auto; }
figcaption { color: var(--soft); font-size: .9rem; }

ul.plain, ol.plain { margin: 0; padding-left: 1.3rem; display: flex; flex-direction: column; gap: 8px; max-width: 65ch; }

/* areas */
.areas { display: flex; flex-direction: column; gap: 6px; }
details.area { background: var(--panel); min-width: 0; }
details.area > summary { cursor: pointer; list-style: none; display: grid; grid-template-columns: minmax(0, 1fr) auto; gap: 4px 14px; align-items: center; padding: 11px 12px; }
details.area > summary::-webkit-details-marker { display: none; }
.area-name { font-family: var(--display); font-weight: 700; font-size: 1.08rem; }
.area-name::before { content: "+ "; color: var(--gold); }
details[open] > summary .area-name::before { content: "\\2212  "; }
.area-pct { font-size: 1.25rem; }
.area-blurb { grid-column: 1 / -1; color: var(--soft); font-size: .9rem; }
.meter { grid-column: 1 / -1; height: 12px; background: var(--rail-rest); border: 2px solid var(--plum); }
.meter i { display: block; height: 100%; background: var(--rail-done); }
.parts { display: flex; flex-direction: column; padding: 0 12px 12px; }
.part { display: grid; grid-template-columns: minmax(0, 1fr) 3.4rem; gap: 2px 12px; padding: 9px 0; border-top: 1px solid var(--line); }
.part b { font-weight: 700; }
.part .p-pct { text-align: right; }
.part .p-note { grid-column: 1 / -1; font-size: .92rem; }
.part .miss { color: var(--soft); }
.part .tiny { grid-column: 1 / -1; height: 6px; background: var(--rail-rest); }
.part .tiny i { display: block; height: 100%; background: var(--grass); }

/* found */
ol.found { margin: 0; padding: 0; list-style: none; counter-reset: f; display: flex; flex-direction: column; }
ol.found li { counter-increment: f; display: grid; grid-template-columns: 2.4rem minmax(0, 1fr); gap: 0 10px; padding: 11px 0; border-top: 1px solid var(--line); }
ol.found li::before { content: counter(f); font-family: var(--body); font-weight: 700; font-size: 1.4rem; line-height: 1.15; color: var(--gold); }

/* tables */
.tbl { overflow-x: auto; }
table { border-collapse: collapse; width: 100%; font-size: .93rem; }
th, td { text-align: left; vertical-align: top; padding: 7px 10px 7px 0; border-top: 1px solid var(--line); }
th { color: var(--soft); font-weight: 400; font-size: .8rem; text-transform: uppercase; letter-spacing: .08em; border-top: 0; }
td.n, th.n { text-align: right; font-variant-numeric: tabular-nums; white-space: nowrap; padding-right: 0; }
td.state-built { color: var(--grass); }
td.state-open { color: var(--gold); }

/* steps */
.filter { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; }
.filter button { font: 500 .95rem var(--display); letter-spacing: .04em; color: var(--ink); background: var(--panel); border: 2px solid var(--plum); padding: 6px 12px; cursor: pointer; }
.filter button[aria-pressed="true"] { background: var(--plum); color: #f4eaf2; }
.phase { display: flex; flex-direction: column; gap: 10px; }
.phase-head { background: var(--panel); border-left: 8px solid var(--gold-fill); padding: 12px 14px; display: flex; flex-direction: column; gap: 6px; }
.phase-head .span { font-size: 1.3rem; color: var(--byz); }
.group { font-family: var(--display); font-weight: 500; text-transform: uppercase; letter-spacing: .1em; font-size: .85rem; color: var(--gold); margin-top: 14px; }
.steps { display: flex; flex-direction: column; }
.step { display: grid; grid-template-columns: 4.3rem minmax(0, 1fr); gap: 2px 14px; padding: 13px 0; border-top: 1px solid var(--line); }
.step .run { font-size: 1.45rem; line-height: 1.1; color: var(--byz); }
.step .adds { font-size: .82rem; color: var(--soft); font-variant-numeric: tabular-nums; }
.step .left { display: flex; flex-direction: column; gap: 2px; }
.step .body { display: flex; flex-direction: column; gap: 5px; min-width: 0; }
.step h4 { margin: 0; font: 700 1.04rem var(--body); }
.step h4 .no { color: var(--soft); font-weight: 400; }
.step p { font-size: .96rem; }
.chips { display: flex; flex-wrap: wrap; gap: 6px 14px; font-size: .84rem; color: var(--soft); }
.chip::before { content: ""; display: inline-block; width: 9px; height: 9px; margin-right: 6px; background: var(--soft); }
.chip.me::before { background: var(--who-me); } .chip.you::before { background: var(--who-you); } .chip.both::before { background: var(--who-both); }
.chip.who { color: var(--ink); font-weight: 700; }
.chip.today::before { display: none; }
.chip.today { color: var(--gold); font-weight: 700; }
.two { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 18px 28px; }
@media (max-width: 620px) { .two { grid-template-columns: minmax(0, 1fr); } .step { grid-template-columns: 3.6rem minmax(0, 1fr); gap: 2px 10px; } .step .run { font-size: 1.25rem; } }
.note { background: var(--panel); padding: 12px 14px; border-left: 8px solid var(--rail-alpha); }
.donestep .run { color: var(--grass); }
.mended { color: var(--grass); }
"""

JS = """
(function () {
  var buttons = document.querySelectorAll('.filter button');
  var rows = document.querySelectorAll('.step:not(.donestep)');
  var groups = document.querySelectorAll('.group');
  function show(who) {
    rows.forEach(function (r) { r.hidden = !(who === 'all' || r.getAttribute('data-who') === who); });
    groups.forEach(function (g) {
      var any = false, el = g.nextElementSibling;
      while (el && el.classList.contains('step')) { if (!el.hidden) any = true; el = el.nextElementSibling; }
      g.hidden = !any;
    });
    buttons.forEach(function (b) { b.setAttribute('aria-pressed', b.getAttribute('data-who') === who ? 'true' : 'false'); });
  }
  buttons.forEach(function (b) { b.addEventListener('click', function () { show(b.getAttribute('data-who')); }); });
})();
"""


def page():
    F = FACTS
    e = html.escape
    o = []
    o.append("<title>K\u0101pman road to release</title>")
    o.append('<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Atkinson+Hyperlegible:ital,wght@0,400;0,700;1,400&family=Pixelify+Sans:wght@500;700&display=swap">')
    o.append("<style>" + CSS + "</style>")
    o.append('<div class="page">')

    # ---- the head and the rail
    o.append("<section>")
    o.append('<div class="eyebrow">K\u0101pman \u00b7 counted on %s%s</div>' % (e(COUNTED), (" \u00b7 %d steps done since" % len(done_steps)) if done_steps else ""))
    o.append("<h1>The road to release</h1>")
    o.append('<p class="lede">The game is about <strong>%s per cent</strong> of the way to its first release. The alpha is %s points off. Most of what lies between the alpha and the release is yours: the pictures and the reading.</p>' % (pct(NOW, 1 if done_steps else 0), pct(ALPHA - NOW, 1 if done_steps else 0)))
    o.append('<div class="rail" role="img" aria-label="Progress: now %s, alpha %s, beta %s, released 100">' % (pct(NOW), pct(ALPHA), pct(BETA)))
    o.append('<div class="bar"><span class="done" style="width:%.2f%%"></span><span class="alpha" style="width:%.2f%%"></span><span class="beta" style="width:%.2f%%"></span></div>' % (NOW, ALPHA - NOW, BETA - ALPHA))
    o.append('<div class="marks">')
    o.append('<div class="mark now"><div class="num">%s</div><div class="what">Now</div></div>' % pct(NOW, 1 if done_steps else 0))
    o.append('<div class="mark alpha"><div class="num">%s</div><div class="what">The alpha</div></div>' % pct(ALPHA))
    o.append('<div class="mark beta"><div class="num">%s</div><div class="what">The beta</div></div>' % pct(BETA))
    o.append('<div class="mark out"><div class="num">100</div><div class="what">Released</div></div>')
    o.append("</div></div>")
    o.append('<ul class="plain">')
    o.append("<li><strong>100 is the first release</strong>, as you ruled on 7 October: one life, 793 to about 805, on twelve ports, with the whole world dim on the chart. The later ages are not in it.</li>")
    o.append("<li>%d steps are left: %d to the alpha, %d more to the beta, %d to the release.</li>" % (len(steps), PS[1]["n"], PS[2]["n"], PS[3]["n"]))
    o.append("<li>Of the %s points still to do, %s are mine to build, %s are yours alone and %s need both of us.</li>" % (pct(100 - NOW), pct(MINE), pct(YOURS), pct(OURS)))
    o.append("<li>A point is a share of the work, not of the calendar. A point of yours takes far longer than one of mine.</li>")
    o.append("</ul>")
    if os.path.exists(PICTURE):
        b64 = base64.b64encode(open(PICTURE, "rb").read()).decode("ascii")
        o.append('<figure><img alt="The game tonight: the Byzantium frame round the chart of the Rhine mouth, a course drawn from Dorestat to Witla, the Voyage and Hold windows, the log and the small view of the boat." width="1200" height="675" src="data:image/jpeg;base64,%s"><figcaption>The game tonight, from the play test: a course set from Dorestat, the hold, the log, the boat at her berth.</figcaption></figure>' % b64)
    o.append("</section>")

    # ---- done since the count
    if done_steps:
        o.append("<section>")
        o.append("<h2>Done since the count</h2>")
        o.append("<p>The count of %s stood at %s. %d of the %d steps are done since, worth %s points.</p>" % (e(COUNTED), pct(COUNT, 1), len(done_steps), len(all_steps), pct(NOW - COUNT, 1)))
        o.append('<div class="steps">')
        for s in done_steps:
            when, note = DONE[s["title"]]
            o.append('<div class="step donestep"><div class="left"><span class="run">+%s</span><span class="adds">%s</span></div><div class="body"><h4><span class="no">%d.</span> %s</h4><p>%s</p></div></div>' % (pct(s["pts"], 2), e(when), s["n"], e(s["title"]), e(note)))
        o.append("</div>")
        o.append("</section>")

    # ---- where it is
    o.append("<section>")
    o.append("<h2>Where the game is</h2>")
    o.append("<p>Seven areas share the 100 points. Open one to see its parts, what stands and what is missing.</p>")
    o.append('<div class="areas">')
    for aid, aname, ablurb, items in AREAS:
        tot, done = area_sum[aid]
        o.append('<details class="area"><summary><span class="area-name">%s</span><span class="area-pct">%s%%</span><span class="area-blurb">%s %s of %s points.</span><span class="meter"><i style="width:%.1f%%"></i></span></summary>' % (e(aname), pct(100 * done / tot), e(ablurb), pct(done, 1), pct(tot), 100 * done / tot))
        o.append('<div class="parts">')
        for pid, name, pts, d, stands, missing in items:
            d, stands, missing = parts[pid]["share"], parts[pid]["stands"], parts[pid]["missing"]
            o.append('<div class="part"><b>%s</b><span class="p-pct">%s%%</span><span class="tiny"><i style="width:%.0f%%"></i></span>' % (e(name), pct(100 * d), 100 * d))
            if stands:
                o.append('<span class="p-note">%s</span>' % e(stands))
            if missing:
                o.append('<span class="p-note miss">Missing: %s</span>' % e(missing))
            o.append("</div>")
        o.append("</div></details>")
    o.append("</div>")
    o.append('<p class="quiet">Counted: %s lines of code in %d files, %d tests (all passing, %d skipped), %s cards with %s choices, %d ports with %d people, %s script entries (%s words), %s art slots. Twelve days of building in three projects, %d commits.</p>' % (num(F["code_lines"]), F["code_files"], F["tests_edit"] + F["tests_play"], F["tests_skipped"], num(F["cards"]), num(F["choices"]), F["ports"], F["people"], num(F["script_entries"]), num(F["script_words"]), num(F["art_slots"]), F["commits"]))
    o.append("</section>")

    # ---- found
    o.append("<section>")
    o.append("<h2>What the counting turned up</h2>")
    o.append('<ol class="found">')
    for head, body in FOUND:
        mended = ' <strong class="mended">Mended since (%s).</strong>' % e(DONE[MENDED[head]][0]) if MENDED.get(head) in DONE else ""
        o.append("<li><p><strong>%s</strong> %s%s</p></li>" % (e(head), e(body), mended))
    o.append("</ol>")
    o.append("</section>")

    # ---- the steps
    o.append("<section>")
    o.append("<h2>Every step from here</h2>")
    o.append("<p>The number on the left is where the game stands once that step is done. A sitting is one piece of work built, tested by the full suites, looked at in pictures from the game and committed. Tonight had five.</p>")
    if done_steps:
        o.append('<p class="quiet">The steps keep the numbers they had at the count. Those done are listed above.</p>')
    o.append('<div class="filter" role="group" aria-label="Whose steps to show"><button type="button" id="f-all" data-who="all" aria-pressed="true">All %d</button><button type="button" id="f-me" data-who="me" aria-pressed="false">Mine %d</button><button type="button" id="f-you" data-who="you" aria-pressed="false">Yours %d</button><button type="button" id="f-both" data-who="both" aria-pressed="false">Both %d</button></div>' % (len(steps), sum(1 for s in steps if s["who"] == ME), sum(1 for s in steps if s["who"] == YOU), sum(1 for s in steps if s["who"] == BOTH)))
    for ph in (1, 2, 3):
        name, what = PHASES[ph]
        start = NOW if ph == 1 else ALPHA if ph == 2 else BETA
        end = ALPHA if ph == 1 else BETA if ph == 2 else 100.0
        o.append('<div class="phase">')
        o.append('<div class="phase-head"><h3>%s</h3><div class="span">%s \u2192 %s</div><p>%s</p><p class="quiet">%d steps, %s points. Mine: %s. Yours: %s.</p></div>' % (e(name), pct(start, 1), pct(end, 1), e(what), PS[ph]["n"], pct(PS[ph]["pts"], 1), rng(round(PS[ph]["sit"][0]), round(PS[ph]["sit"][1]), "sittings"), rng(round(PS[ph]["hrs"][0]), round(PS[ph]["hrs"][1]), "hours")))
        if ph == 1:
            o.append('<div class="tbl"><table><thead><tr><th>Row</th><th>Your alpha list</th><th>Today</th></tr></thead><tbody>')
            for n, rname, state in ALPHA_ROWS:
                cls = "state-built" if state == "built" else "state-open"
                o.append('<tr><td class="n">%d</td><td>%s</td><td class="%s">%s</td></tr>' % (n, e(rname), cls, e(state)))
            o.append("</tbody></table></div>")
        o.append('<div class="steps">')
        group = None
        for s in steps:
            if s["phase"] != ph:
                continue
            if s["group"] and s["group"] != group:
                group = s["group"]
                o.append('<div class="group">%s</div>' % e(group))
            chips = '<span class="chip who %s">%s</span><span class="chip">%s</span>' % (s["who"], WHO[s["who"]], e(s["size"]))
            if s["today"] and s["who"] != ME:
                chips += '<span class="chip today">Can start today</span>'
            o.append('<div class="step" data-who="%s"><div class="left"><span class="run">%s</span><span class="adds">+%s</span></div><div class="body"><h4><span class="no">%d.</span> %s</h4><p>%s</p><div class="chips">%s</div></div></div>' % (s["who"], pct(s["run"], 1), pct(s["pts"], 2), s["n"], e(s["title"]), e(s["text"]), chips))
        o.append("</div></div>")
    o.append("</section>")

    # ---- yours alone
    o.append("<section>")
    o.append("<h2>What only you can do</h2>")
    o.append('<div class="two">')
    o.append("<div><h3>Rulings wanted</h3><ol class=\"plain\">" + "".join("<li>%s</li>" % e(r) for r in RULINGS) + "</ol></div>")
    o.append('<div><h3>The pictures of the first release</h3><div class="tbl"><table><thead><tr><th>Tier</th><th class="n">Pictures</th><th class="n">Hours</th></tr></thead><tbody>')
    for t, label in ((1, "1. The frame"), (2, "2. The first hour"), (3, "3. Stand-ins that cover many")):
        o.append('<tr><td>%s</td><td class="n">%d</td><td class="n">about %d</td></tr>' % (label, F["tier"][t], TIER_HOURS[t]))
    o.append('<tr><td>4. One by one, optional</td><td class="n">%d</td><td class="n">not in the 100</td></tr>' % F["tier"][4])
    o.append('<tr><td>5. Later</td><td class="n">%s</td><td class="n">not in the 100</td></tr>' % num(F["tier"][5]))
    o.append("</tbody></table></div>")
    o.append('<p class="quiet">The hours are my guesses for somebody learning pixel art: a quarter of an hour for a 16-pixel sign, an hour for a face, two and a half for a scene, four for a port\u2019s view. After tier 1 we shall know your real pace.</p></div>')
    o.append("</div>")
    o.append("</section>")

    # ---- the pace
    o.append("<section>")
    o.append("<h2>What sets the pace</h2>")
    o.append('<ul class="plain">')
    o.append("<li><strong>My part:</strong> about %s. This week ran at three to five a day while you were here. That is weeks.</li>" % rng(round(SIT[0]), round(SIT[1]), "sittings"))
    o.append("<li><strong>Your part:</strong> about %s, and your own home waters on top. At ten hours a week that is most of a year. At twenty it is five to six months.</li>" % rng(round(HRS[0]), round(HRS[1]), "hours"))
    o.append("<li><strong>Steam\u2019s own waits:</strong> 30 days after the fee, two weeks of \u201cComing soon\u201d, two reviews. Six weeks at the least, even for a finished game.</li>")
    o.append("</ul>")
    o.append('<p class="note">The release date is set by your drawing and your reading, not by the building. The note on Steam of 7 October said spring to summer 2027 for the first release, and this count agrees with it.</p>')
    o.append("<h3>What could move these numbers</h3>")
    o.append('<ul class="plain">' + "".join("<li><strong>%s</strong> %s</li>" % (e(h), e(b)) for h, b in RISKS) + "</ul>")
    o.append("</section>")

    # ---- after
    o.append("<section>")
    o.append("<h2>After the release</h2>")
    o.append("<p>None of this is in the 100. You ruled that the later ages come as later releases.</p>")
    o.append('<ul class="plain">' + "".join("<li>%s</li>" % e(l) for l in LATER) + "</ul>")
    o.append("</section>")

    # ---- the method
    o.append("<section>")
    o.append("<h2>How the numbers were made</h2>")
    o.append("<p>The whole job of the first release is 100 points. I shared them over seven areas and %d parts by judgment: the weights are my estimates, not measurements. Each part has a share done, set from what I counted in the project. What is left of each part is shared among the steps that finish it, so the steps add up to exactly what is missing.</p>" % len(parts))
    o.append('<div class="tbl"><table><thead><tr><th>Area</th><th class="n">Points</th><th class="n">Done</th><th class="n">Left</th></tr></thead><tbody>')
    for aid, aname, ablurb, items in AREAS:
        tot, done = area_sum[aid]
        o.append('<tr><td>%s</td><td class="n">%s</td><td class="n">%s</td><td class="n">%s</td></tr>' % (e(aname), pct(tot), pct(done, 1), pct(tot - done, 1)))
    o.append('<tr><td><strong>All</strong></td><td class="n"><strong>100</strong></td><td class="n"><strong>%s</strong></td><td class="n"><strong>%s</strong></td></tr>' % (pct(NOW, 1), pct(100 - NOW, 1)))
    o.append("</tbody></table></div>")
    o.append('<p class="quiet">Read every figure as give or take five. The model is the file Bake/plan/roadmap.py in the project, and the same analysis is kept as Design/ROADMAP.md. Change a part\u2019s share done, or strike a step, and both are made again.</p>')
    o.append("</section>")

    o.append("</div>")
    o.append("<script>" + JS + "</script>")
    return "\n".join(o) + "\n"


if __name__ == "__main__":
    md = markdown()
    io.open(os.path.join(ROOT, "Design", "ROADMAP.md"), "w", encoding="utf-8", newline="\n").write(md)
    pg = page()
    os.makedirs(os.path.dirname(OUT_HTML), exist_ok=True)
    io.open(OUT_HTML, "w", encoding="utf-8", newline="\n").write(pg)
    print("count %.1f | now %.1f | alpha %.1f | beta %.1f | steps left %d (%d, %d, %d) | done %d" % (COUNT, NOW, ALPHA, BETA, len(steps), PS[1]["n"], PS[2]["n"], PS[3]["n"], len(done_steps)))
    print("mine %.1f points, yours %.1f, both %.1f | sittings %.0f to %.0f | your hours %.0f to %.0f" % (MINE, YOURS, OURS, SIT[0], SIT[1], HRS[0], HRS[1]))
    for aid, aname, _, _ in AREAS:
        print("   %-34s %5.2f of %4.1f" % (aname, area_sum[aid][1], area_sum[aid][0]))
    for ph in (1, 2, 3):
        print("   phase %d: %d steps, %.2f points, sittings %s, hours %s" % (ph, PS[ph]["n"], PS[ph]["pts"], PS[ph]["sit"], PS[ph]["hrs"]))
    print("tier hours:", TIER_HOURS)
    print("wrote Design/ROADMAP.md (%d B) and %s (%d B)" % (len(md.encode("utf-8")), OUT_HTML, len(pg.encode("utf-8"))))

# The picture: Captures/chart/19_port_clicked_on_the_chart.png of the play test, made 1200 wide as a JPEG of quality 82
# (PIL: Image.open(...).convert("RGB").resize((1200, 675), Image.LANCZOS).save(PICTURE, quality=82)).
