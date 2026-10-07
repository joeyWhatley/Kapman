# The wiki

The user, 7 Oct 2026: "I think an in-game wiki is needed."

The game is all in text and run from the frame. A player meets hundreds of ports, goods, people, feasts and old
words. The wiki is where any of them can be looked up without leaving the game. In the mock-up it is the nineteenth
panel (Design/ui_mock/template3.html, layout G: workspace 5, or F1).

## What it is

1. **A window like any other.** It tiles, it can sit beside Trade or the Chronicle, it has a search box and kinds
   (How to play, Ports, Trade, Peoples, Faith and days, Ship and sea, Words, Events, People).
2. **Everything links.** The user, 7 Oct: "I want everything to link with keywords and phrases of interest, they are a
   different colour and it could be anywhere, it all links together at the wiki, and if you hover over it it gives a
   brief overview, then if you hover over it and click it it will bring up the wiki UI on the page that you clicked."
   - A word or phrase of interest (a port, a good, a person, a people, a feast, a stat, a foreign word, a rule) is set
     in the link colour with a dotted line under it, wherever it stands: a window, a card, a talk, the log, the
     portrait block, the caption of the ship window, another wiki page. The link colour is one pale sea blue used for
     nothing else (gold stays "yours to spend").
   - **Hover**: a small box by the cursor with the name, its kind, one or two sentences, and how you know it.
   - **Click**: the wiki comes up on that page. If a Wiki window is tiled in this workspace it turns to the page;
     if not, the wiki opens as a floating window over the chart (the whole wiki: search, kinds, back), with Dock to
     tile it. Esc sends it away. Nothing else on the screen moves.
   - A thing not yet known is greyed, and its hover says only what little has been heard.
   - Words inside a button are not links (a click there must do what the button says).
3. **Every page has two parts, kept apart.**
   - *In the game*: what it is and what it does for you, in plain words, with your own figures (what you paid, what
     you saw).
   - *What is known*: what the record says, with its source. So the player can always tell the game from history.
4. **A page says how you know it.** "Your home port." "A Frisian shipmaster told you, last week. You have not seen it."
   This is the game's own rule (knowledge is the treasure and is as old as the ship that brought it) shown in the
   book.

## What it shows, and when (the user's ruling, 7 Oct: see DECISIONS.md)

- The rules (How to play) are all open from the first day.
- The world fills in as you learn it: a port when you see it or hear it named, a good when you see it traded, a
  person when you meet them or hear of them, a word when someone says it to you. A thing heard of but not named
  shows as a greyed line ("A market among the Danes: not yet known").
- The page of a dated event shows what really happened only once that day has passed in your game.

## Where the pages come from

Nothing new has to be written for most of it. The data already holds the facts and their sources.

| Kind | From | Pages |
|---|---|---|
| How to play | written by hand, one page a rule (days, the debt, the word, the stats, buttons and windows, the measure, tides, the toll, hailing, offerings, saving) | about 40 |
| Ports | Data/ports_793.json and Design/content/dossiers | 50 |
| Sites in ports | Data/places/*.json | 610 |
| People | Data/places/*.json, Design/content/spine.json, Data/story/weight_793.json | about 480 |
| Goods | Data/goods_793.json, Design/content/economy | 140 |
| Ships | Data/ship_classes_793.json, Data/story/ships_793.json | 18 |
| Peoples and faiths | Data/story/self_793.json, Design/content/peoples and realms | 16 |
| Faith and days | Data/story/calendar_793.json (feasts, fasts, fairs) | about 45 |
| Words | Data/story/tongues_793.json (words with where each is attested) | 318 |
| Events | Data/chronicle_793.json | 582 |
| Texts | Data/texts_793.json (the chest already holds them; the wiki links to them) | 267 |

About 2,500 pages in all, of which about 40 are written by hand.

## How it is built

1. **A baker** (Bake/content/make_wiki.py) reads the files above and writes one file the game loads,
   Data/story/wiki_793.json: for each page an id, a kind, a title, a hint (what it is called before it is known), the
   two texts, the source, the links, and the rule for when it is known (always; port seen or heard; good seen; person
   met; word heard; date passed). It also writes the list of linked words (each name and its other spellings).
2. **The Wiki panel** in the frame: search, kinds, a page, back, links.
3. **Linked words**: wherever the frame sets text (a window, a card, a talk, the log, the portrait block), the first
   use in that box of a word or phrase that has a page becomes a link, the longest phrase winning. One place in the
   code does this for all text, from the baker's list of names and their other spellings; the hover box and the click
   are handled there too. Cards and talks need no markup from their writers.
4. **How you know it**: the game already keeps who told you a price and when (Game/Known). The same record is kept for
   ports, people and words, and the page prints it.
5. The 40 rule pages are written once the rules they describe are in the game, so they are never ahead of it.

Order: the baker and the panel first (the rules and the ports of the first release), then linked words, then the
rest of the kinds.

## What it is not

- Not a tutorial: the first ten minutes still teach by doing. The wiki is for the second question, not the first.
- Not a spoiler: it never shows a card's outcomes, a chain's next step, or a day that has not come.
- Not a second chest: texts are read in the Chest. The wiki says what a text is and where it comes from.
