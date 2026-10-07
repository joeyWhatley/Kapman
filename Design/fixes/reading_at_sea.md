# Reading at sea: the empty hours go to the reader

7 October 2026. For the writing critic's fourth change (`Design/critique/review_words.md`, section 6, item 4: "Give
the crossing to the reader ... a crewman saying the next part of the song in plain English while she runs, a line
that turns, a riddle worked at the oar. Not a card to answer each evening"; worth half a star by his own count) and
the user's wish of 3 October ("a crewman reads part, you finish").

**How I worked.** Read: `Chest/Chest.cs`, `Chest/ChestData.cs`, `UI/Frame/FrameTongues.cs`, `FrameRoot.Floats.cs`
(lines 1 to 345), `FrameGame.cs`, `FramePanel.cs` (members), `Panels/ChestFramePanel.cs`, `CrewPanel.cs`,
`TonguesPanel.cs`, `UI/GameSpeed.cs`, `UI/PanelClock.cs`, `Self/Self.cs` (members), `Content/Tongues/TongueBook.cs`
(members), `Design/wiring_plan.md` sections 8 and 9, `Design/DECISIONS.md` items 13 to 26, `Design/QUEUE.md`,
`Design/research/next/crossing.md` sections 0, 4.1, 4.2, 4.5, and the heads of `fixes/brief_faces.md` and
`fixes/card_clock.md`. Counted with Python: `texts_793.json`, `riddles_793.json`, `story/tongues_793.json`,
`encounters/crew_793.json`. **The code below compiles**: the three new files and the four edited ones were built
outside Unity with the Roslyn compiler against the game's own references (`Kapman.Game.rsp`), 0 errors, 0 warnings
in the new files; the test file was built the same way against that result and the tests' references. **Nothing was
run**: no Unity, no test executed, no picture seen. Nothing under `Assets` was changed.

---

## 0. The short answer

1. **One window, three panels, no card.** A floating window with the id `reading` that holds no clock, asks for no
   answer and has no numbered choices. A section at the top of the **Chest** window, one in **Crew**, one in
   **Tongues**. One line in the log when something is first ready.
2. **Two halves, kept apart.** *What is due* goes by the game's hours alone, the same at every speed (ruling 26:
   nothing may be lost at speed 5). *What is shown* goes by the wall clock and the speed: at speeds 1 and 2, at sea,
   with nothing pressing, one window may open by itself in a quiet stretch. At speeds 3 to 5 nothing opens and
   nothing is logged twice: what is due is kept, one of a kind, for the next quiet stretch.
3. **A man gives part, you finish.** He says the first of it (a third, 14 lines at most, a line every 2.2 real
   seconds). Then "The rest is yours" and a Read on button that turns up 12 lines at a press. The leaf keeps the
   line. The next part of a set comes only when you took the last one up.
4. **The 43 evening-song cards stop.** They are the "card each evening" the critic names (`crew.song.fitt_01` to
   `fitt_43`, rate 0.6 an hour from 18:00). The code holds up their own flag (`crew_song_tonight`), so no card file
   is edited.
5. **The data that does not exist** is one small file, `story/readers_793.json`: five seats and eight rows of who
   has what by heart or can read it. A first version is drafted in section 4, made from what the crew cards already
   say. Four more gaps are named there (plain openings, 35 texts with no English, two poems with no original, the
   tongue lines' `text_id` not read).
6. **Honest size.** The rows hand out 157 texts, 39,793 words of English: about two and three quarter hours of
   reading at 240 words a minute. Against a first life of about ten hours at speed 4 that is real. Against 230
   hours it is not a filler, and it is not meant as one: it is what the slow speeds are for.

---

## 1. What is there today (measured)

| What | Where | Fact |
|---|---|---|
| The texts | `Assets/Kapman/Game/Data/texts_793.json` | 267 texts: Old Norse 146, Old English 83, Latin 36, Old Saxon 1, Old Irish 1. 232 have English, 45,592 words. Beowulf is 43 fitts, 30,106 words, in Hall's verse of 1892 ("Waxed 'neath the welkin") |
| Riddle words | `riddles_793.json` | 137 riddles (18 want two or three answers), 12 verses to decode. 146 of the 267 texts are a riddle's or a verse's own words; **121 are free to read**, 39,158 words |
| No English | `texts_793.json` | 35 texts: all 33 Latin riddles (Aldhelm 12, Symphosius 12, Tatwine 5, Eusebius 4), `exeter_25_onion`, `adam` |
| No original | `texts_793.json` | `seafarer` and `deor`: the `original` is a note in square brackets ("not transcribed here yet") |
| Line for line | `texts_793.json` | 73 texts have as many lines of English as of the original (44 of Vafthrudnismal, 26 of Alvissmal, the vow, Aelfric, the Irish quatrain). Only these can turn piece by piece without a guess |
| Tongue lines | `story/tongues_793.json` | 111 real lines with pairs; 41 carry a `text_id`, 37 of those name a chest text. `TongueBook.cs` does not read `text_id` (0 hits) |
| Reading today | `Chest.cs`, about lines 130 to 185 | A switch turns a text into English: at once for the kin tongues and runes, for Latin after six lessons or `readToYou`. `ReaderAboard` is `false` (line 143). It is read in the old chest on [I], which hides the frame (`FrameRoot.Update`) |
| The Chest window | `Panels/ChestFramePanel.cs` | A list: title, from whom, a tag. Line 43: "To read a text or try a riddle, open the chest: press I." |
| The crew | `Voyage.Men` (a number), `Panels/CrewPanel.cs` line 39 | "The crew are not yet people with names". The crew cards speak through five fixed seats (`crew_793.json`, "THE SEATS"): steersman (a baptised Frisian), mate (a Saxon), lookout (a young Englishman schooled by monks, who has the song by heart), dane (keeps the old gods; aboard while `year < 816 or flag.crew_heathen_aboard`), hand (a Frisian) |
| The evening song | `encounters/crew_793.json` | 43 cards, `when`: `not moored and hour >= 18 and storm < 0.3 and not flag.crew_song_tonight and text.beowulf_NN-1 and not text.beowulf_NN`, rate 0.6 a game hour: in practice one card each evening at sea, 43 evenings running |
| Other crew cards that hand words out | `crew_793.json` | 53 `give_text` effects in all (43 are the song), 6 `give_riddle`, 5 `crew.tongue.*`, 2 `crew.read.*`. These are rare (rates 0.01 to 0.1) and stay cards |
| The turning line | `UI/Frame/FrameTongues.cs` | `TurningLine(FloatTurn)`: pieces go out and come back as English by the wall clock, by `Self.TongueLevel`. A piece seen ten times (`Self.StaysAfter`) is English from the start. "Nobody aboard interprets yet" (wiring plan 9.4) |
| The clock in code | `UI/GameSpeed.cs` line 23 | `Speeds = { 1, 2, 4 }`, the speed is `Time.timeScale`. The five speeds of ruling 26 are not in yet; `Design/fixes/clock_five_speeds.md` does not exist yet |
| A game hour | `KapmanGame.cs` lines 88 and 130 | `KapmanGame.HourPassed` fires once for every game hour whatever the speed (the tick ladder). This is what makes a speed-blind rule possible |

### What the crossing study assumed, and what the new clock changes

`Design/research/next/crossing.md` was written the same morning for a clock of 1x, 2x and 4x with "nothing faster"
(its 4.1, rule 2). Three of its parts on reading rest on that and do not hold now:

- **"One thing a watch": offers that light at each turn of the watch and go dark at the next** (4.5). A watch is
  four game hours: 60 real seconds at the new speed 2, 10 at speed 3, 2.5 at speed 4, less than one at speed 5.
  An offer that goes dark at the turn is lost at speed, which ruling 26 forbids. Here nothing goes dark: a thing is
  kept until it is taken up.
- **"2x is the speed a crossing is tuned for"** and its minute-by-minute crossing (section 5). The old 2x was two
  game minutes a second. The new speed 2 is four: its sums are those of the old 4x.
- **"At 4x a fitt lasts most of a night"** (4.5). True of the new speed 2. At speed 4 a fitt would last twelve game
  days, so no window opens there.

What it got right is kept: reading takes the player's minutes and not the ship's; caps go by the game day so no
speed earns more than another; a man tells the first third; one try a day at a riddle.

| Speed (ruling 26) | Game minutes a real second | A game day | A game hour | A fitt of 700 words (3 real minutes) costs | Dorestat to Lundenwic, fair wind (50 to 55 game hours) |
|---|---|---|---|---|---|
| 1 | 1 | 24 min | 60 s | 3 game hours | 50 to 55 min |
| 2 | 4 | 6 min | 15 s | 12 game hours | 13 to 14 min |
| 3 | about 24 | about 1 min | 2.5 s | 3 game days | about 2 min |
| 4 | about 96 | about 15 s | 0.6 s | 12 game days | about 33 s |
| 5 | about 360 | about 4 s | 0.17 s | 45 game days | about 9 s |

So the reading belongs to speeds 1 and 2, and the rule is by pace, not by level number: **quiet means four game
minutes a real second or fewer** (`Aloud.QuietPace`). Today's three speeds (1, 2, 4 times) all count as quiet, which
is right: they are the new speeds 1 and 2 and one between.

---

## 2. The design

### 2.1 What is due: by the game's hours, the same at every speed

`Aloud.Hour` runs on `KapmanGame.HourPassed`. It never looks at the speed or the wall clock and draws no random
number. While she is at sea (cast off and clear of a port's business):

- **A text.** From noon on, once a game day: if the last text a man began was taken up (or there is none), the
  next one is chosen and put in the chest quietly, from the man ("From the lookout."). If the last one was not
  taken up, it stays the one on offer and nothing new is given. So forty days at speed 5 put exactly one unread
  text in the chest, as forty days at speed 1 do.
  The choice, in order: (1) what the player asked for in the Chest window; (2) the next part of a set already
  begun; (3) a leaf lying unread in the chest that a man aboard can read; (4) something new a man has by heart, by
  the rows of the readers' file in their order (the song first).
- **A riddle.** No riddle is given by time. One that lies open in the chest (not answered, not resting) is put
  again every third game day, by a man who has its tongue if there is one. The seven `crew.riddle.*` cards and the
  ports go on being where riddles come from.
- **A word.** Once a game day each man whose own tongue the player follows at less than 0.9 names one thing in it:
  a word of kind ship, sea, food, trade or greeting from `tongues_793.json`. It is counted with `Self.See`, the
  same count the turning line uses. At ten hearings (`Self.StaysAfter`) it stays; his tongue rises by 0.02, up to
  0.6 by words alone; he takes the next word. Ten sea days a word, at any speed.

Each due thing is **kept**: a list of at most three (one text, one riddle, one word just learned). A newer one of a
kind takes the older one's place. Nothing piles up.

### 2.2 What is shown: by the wall clock and the speed

`FrameReading.Tick` asks `Aloud.Step` twice a real second.

| | Speeds 1 and 2 | Speeds 3, 4, 5, and the pause |
|---|---|---|
| A window opens by itself | Yes, when the stretch is quiet (below), one a stretch | Never |
| The log | One line when a window opens, or, if it cannot open just now, one line that the thing is ready; at most one word a game day ("The Dane, at his work: ...") | One line when a thing is first kept. Since the next text waits to be taken up, that is one line and then silence |
| The Chest window | Shows what is ready, with a button | The same. The player may open the window by hand at any speed |
| A window that opened by itself and was not touched | Folds itself 45 real seconds after the man has done | Folds at once when the speed goes up (it is kept) |
| Hearing a word | Counted | Counted the same |

**A quiet stretch** (`Aloud.Quiet`): the pace is four game minutes a second or less and the clock runs; she is at
sea; nothing presses (no card in play, no alert, no other floating window, no window that holds the clock, the old
chest and the menu shut, and `FrameReading.AlsoPressed`, a hook for the clock's builder: "the clock has just dropped
by itself"); the storm figure where she is is under 0.3 (the song cards' own bound). Lying to and waiting on the
water count as at sea: a night at anchor is the best hour there is for a tale.

### 2.3 The window

Title: the text's title, or "A riddle at the oar". Under it, who and how: "The lookout has it by heart. English.",
"The Dane reads what he can of it. Norse.", "The Dane puts it. Norse."

- **Its own words, turning.** Where the text runs line for line with its English (73 texts today), its first four
  lines stand in their own tongue and turn into English by the built `TurningLine`, at the player's skill in that
  tongue; the lines that turned are not said a second time under it. Where it does not run line for line (Beowulf,
  Havamal, the laments), nothing is paired by guess and the English begins at once. See gap 4 in section 4.3 for
  how Beowulf gets its ten real turning lines.
- **The man's part.** A third of the lines (the row's `share`), 14 at most, one every 2.2 real seconds. It goes by
  the wall clock, like the turning line, so it reads the same at speed 1 and speed 2.
- **"The rest is yours. 58 lines more."** Read on turns up 12 lines; the float's own scroll holds them. Let it lie
  shuts it. The cross and Esc do the same.
- **At the end:** "That is the end of it." and Keep it. A text had whole is `Aloud.Finished`.
- **A riddle:** its lines, "2 tries left. One try a day at the oar.", and a box: write two letters of a thing you
  know and up to eight of your known things that match are offered as buttons (`Chest.KnownThings`, so wrong
  answers are things too and nothing is given away). A press is the day's try (`Chest.Answer`). A riddle of two
  or three answers says "open the chest for it (I)".
- **Opened by hand** (from the Chest window) there is no waiting on the man: it opens at the line where it was
  left, at any speed, in port too. That is the player's own reading.

No numbered choices: the frame gives the number keys and Enter to the front window's `options`, and this window has
none, so a key meant for a workspace or a card never lands here.

### 2.4 What the player keeps

Only from what the player does, never from sitting at a slow speed:

- **Reading on for the first time in a game day**: the men's heart +0.02 (what "Listen" on the song card gave).
- **A text had whole**: Lore by use, through `Self.Used("lore", 1.0)` (a tenth of a pip divided by the pips held): a
  text of 150 words or more counts once, four shorter ones count once between them.
- **A riddle answered**: its own reward, as built.
- **A word that stays**: English from the start in every turning line, and a little of the tongue.
- `Aloud.Finished(id)` is there for the cards: crossing.md's "[Lore] Quote takes only texts you have finished"
  would read it. Not drafted here.

---

## 3. What it adds, to which panel and window

| Where | What is added | By |
|---|---|---|
| **A new floating window**, id `reading`, class `frame-float-reading` | Section 2.3. `holdsClock = false`, `mustAnswer = false`, no `options`, its own `content` | new `FrameReading.cs` |
| **Chest** (`ChestFramePanel`, id `chest`), at the top | Heading "Read aboard". A line that is a button for the text that is ready ("The lookout has it ready.", tag `ready`) and one for the riddle ("The Dane would put it again. A try is yours today."). "Ask for:" and a row of buttons, one for each set a man aboard has by heart, plus "Their choice". A switch: "The men begin by themselves: yes / no". When they hold back: "You let it lie twice, so they wait to be asked. Read anything and they begin again." Then the heading "In the chest" and the list as before | edit, section 7.4 |
| **Chest**, the list | A text with English is now a line that is a button: it opens in the reading window at the line where it was left. Its tag says `unread`, `line 24` or `read`. The last line changes to: "Click a text to read it here. For the runes, your notes and a riddle of more than one answer, open the chest: press I." | the same edit |
| **Chest**, for the title | `ChestFramePanel.LampWords`: "2 things kept for a quiet hour." or null. The frame has no lamp on a window's title yet; the builder of the brief faces (`fixes/brief_faces.md`) owns the title and the brief face, and this is the string for both | the same edit |
| **Crew** (`CrewPanel`, id `crew`) | Heading "What they have". One line a seat that is filled: "The lookout. English, and letters. By heart: the song of Beowulf; by heart: the English laments; reads: a Latin leaf, slowly; reads: an English leaf." with, at the right, the word he is naming these days. The last line becomes "They are seats, not yet people with names of their own: that comes with hiring." | edit, section 7.4 |
| **Tongues** (`TonguesPanel`, id `tongues`) | Heading "From the men". One line a man who is naming a word: the word, "The Dane: a sail (say: SAY-gl). One hearing a day at sea.", and "3 of 10" | edit, section 7.4 |
| **The log** | "The lookout has Beowulf, fitt II ready for a quiet hour. It will keep: Chest." once a thing; "The lookout begins ..." when a window opens by itself; at a quiet speed one word a game day; "You have the Dane's word for it now: ..." | `FrameReading.Tell` |
| **The frame's update** | One line: `FrameReading.Tick(this);` after `FloatsUpdate();` (`FrameRoot.cs` line 377) | edit, section 7.4 |
| **The saved game** | A part `aloud` through `ContentSave.Register`: the place in every text, what is finished, what is kept, the counters, the switch | new `Aloud.cs` |
| **The stylesheet** | Six rules, section 7.5 | `Frame.uss` |

**Not touched:** the eight function buttons (Hail already hails a sail: `FrameRoot.Rack.cs` line 240; the reading
is not a verb and needs no button); the old chest on [I] (runes, notes, pins, verses, riddles of several answers
stay there); `Chest.ReaderAboard` (if it read "the lookout is aboard", every Latin text would open whole at once,
which is not "slowly and in part"); the card files.

---

## 4. Who can read or recite what

### 4.1 The rule

A man gives a text when a row of the readers' file names the text and fits the man:

- **By heart** (`how: recite`): by his seat, his people and his faith. No leaf is needed: the text goes into the
  chest as he begins it. The lookout has the song because the data says so (all 43 fitts: `acquire.who`, "a crewman
  who has the song by heart"). A Dane has the sayings of the High One whatever he believes; the questions of the
  gods only if he keeps the old gods or is prime-signed.
- **Read** (`how: read`): the leaf must be in the chest, and he must have **letters** (the schooled lookout: Latin
  script, so Latin and his own English written in it) or **runes** (the Dane). He gives less of it: a quarter of
  a Latin leaf, half of a stave.
- **By tongue:** he says it in its own tongue. The turning line turns by the *player's* skill in that tongue, as
  built. The English that follows is the man giving it in the boat's speech: this is the first use of "somebody
  aboard interprets" (wiring plan 9.4), and it is why a player with no Norse still gets the sense.
- **By year:** `not_before` holds a text back (Maldon before 991).
- A man who has by heart goes before a man who reads.
- A text that is a riddle's or a verse's own words is never handed out as reading (146 of 267): the chest would
  hold the riddle twice.

### 4.2 The data that does not exist: `Assets/Kapman/Game/Data/story/readers_793.json`

First version. Every row names what it rests on; a (?) is my judgement and the user's to rule. With this file the
rows hand out: the song 43 texts (30,106 words), the English laments 3 (3,142), the sayings 27 (843), the old
questions of the gods 37 (1,034), the vow 1 (67), rune staves 40 copies of stones (1,041, and only once one is in
the chest), a Latin leaf 1 (244), an English leaf 5 (3,316). In all 157 texts, 39,793 words. Three free texts no
row covers: `landnama`, `alcuin_793` (the data marks it `check`), `irish_viking_terror` (nobody aboard has Irish).

Without the file the code uses a smaller table of its own (five seats, five rows): the game works before the data
step is done.

```json
{
  "about": "WHO ABOARD KNOWS WHAT (Design/fixes/reading_at_sea.md), 7 Oct 2026. The seats are the five parts the crew cards speak through (encounters/crew_793.json, 'THE SEATS'); each row of 'knows' says which texts of texts_793.json a kind of man has by heart ('recite') or can read from a leaf that is in the chest ('read'), and how much of one he gives before the rest is the player's ('share'). 'why' names the card or the data the row rests on; (?) marks a row that rests on the designer's judgement and is the user's to rule. A text that is the words of a riddle or of a verse to decode is never handed out by these rows (the code leaves them out). 'not_before' is the year before which a text is not said aloud as a living thing; 'plain' is for a text's opening in plain English as the man says it, one string a line: it is empty, and the words are the user's to write.",
  "seats": [
    { "id": "steersman", "name": "the steersman", "people": "frisian", "faith": "christian", "first_tongue": "old_frisian", "from_men": 1, "why": "crew_793.json about: 'steersman' (a baptised Frisian)" },
    { "id": "mate", "name": "the mate", "people": "saxon", "faith": "christian", "first_tongue": "old_saxon", "from_men": 2, "why": "crew_793.json about: 'mate' (a Saxon who speaks for the oars)" },
    { "id": "lookout", "name": "the lookout", "people": "english", "faith": "christian", "first_tongue": "old_english", "letters": 1, "from_men": 3, "why": "crew_793.json about: 'lookout' (a young Englishman schooled by monks, who has the song by heart)" },
    { "id": "dane", "name": "the Dane", "people": "danish", "faith": "old_gods", "first_tongue": "old_norse", "runes": 1, "heathen_seat": 1, "from_men": 4, "why": "crew_793.json about: 'dane' (a Dane or Northman who keeps the old gods); aboard while year < 816 or flag.crew_heathen_aboard, as the cards ask" },
    { "id": "hand", "name": "the hand", "people": "frisian", "faith": "christian", "first_tongue": "old_frisian", "from_men": 5, "why": "crew_793.json about: 'hand' (the ordinary Frisian hand)" }
  ],
  "knows": [
    { "id": "song", "title": "the song of Beowulf", "how": "recite", "texts": ["beowulf_*"], "seats": ["lookout"], "share": 0.33,
      "why": "texts_793.json, acquire.who of all 43 fitts: 'a crewman who has the song by heart: one fitt at a time'; crew.song.fitt_01 to 43 give them through the lookout" },
    { "id": "english_laments", "title": "the English laments", "how": "recite", "texts": ["seafarer", "wanderer", "deor"], "peoples": ["english"], "share": 0.33,
      "why": "the cold card has the lookout say the opening of The Seafarer and give it; the low-heart card has him tell Deor. The Wanderer by likeness (?)" },
    { "id": "sayings", "title": "the sayings of the High One", "how": "recite", "texts": ["havamal_*"], "peoples": ["danish", "norse", "svea", "gute"], "share": 1.0,
      "why": "the Dane says Havamal 3, 21, 36, 41 and 47 in the crew cards. A stanza is two to four lines: he says the whole" },
    { "id": "gods_lore", "title": "the old questions of the gods", "how": "recite", "texts": ["t-vafth-*", "t-alv-*", "t-gest-*"], "peoples": ["danish", "norse", "svea", "gute"], "faiths": ["old_gods", "prime_signed"], "share": 1.0,
      "why": "(?) held to a man who keeps the old gods or is only prime-signed: the 37 stanzas of Vafthrudnismal, Alvissmal and Gestumblindi that are not a riddle's own words" },
    { "id": "vow", "title": "the vow at the font", "how": "recite", "texts": ["saxon_vow"], "peoples": ["saxon", "frisian"], "faiths": ["christian"], "share": 1.0, "not_after": 830,
      "why": "crew.faith.vow: the mate says the Old Saxon vow. The year is the card's own (year < 830)" },
    { "id": "staves", "title": "rune staves", "how": "read", "texts": ["stone:*"], "runes": 1, "share": 0.5,
      "why": "crew.read.rune_stick: 'the Dane reads a rune stick in part and the merchant finishes it'" },
    { "id": "latin_leaf", "title": "a Latin leaf, slowly", "how": "read", "texts": ["dicuil"], "letters": 1, "share": 0.25,
      "why": "crew.read.latin_leaf: 'the lookout, schooled as a boy by monks, reads a Latin leaf slowly and in part'" },
    { "id": "english_leaf", "title": "an English leaf", "how": "read", "texts": ["chronicle_793", "aelfric_merchant", "ohthere", "wulfstan", "maldon_991"], "peoples": ["english"], "letters": 1, "share": 0.5,
      "why": "(?) a man who reads Latin letters reads his own tongue written in them" }
  ],
  "not_before": {
    "maldon_991": 991,
    "ohthere": 890,
    "wulfstan": 890,
    "dicuil": 825,
    "adam": 1075,
    "landnama": 1100,
    "aelfric_merchant": 995,
    "chronicle_793": 794,
    "alcuin_793": 794
  },
  "not_before_notes": "maldon_991: the battle was in August 991 (the text's own id). The rest are from the designer's memory and want a check against each text's 'writtenDown' (?): Ohthere and Wulfstan in the Old English Orosius, about 890; Dicuil, 825; Adam of Bremen, about 1075; Landnamabok, 12th century; Aelfric's Colloquy, about 995; the annal for 793 and Alcuin's letter after it, said from 794 on. texts_793.json marks alcuin_793 'check': TongueBook gives nothing for a line so marked, and this file should not hand that text out until it is checked.",
  "plain": {}
}
```

### 4.3 Four more gaps, said plainly

1. **The plain openings.** The critic met "Beowulf in a translation of 1892 ... forty-three evenings long" and asks
   for "plain English". The man's part should be his own plain telling, and those words do not exist. The file has
   a place for them (`plain`: a text id, then one string a line); where it is filled, the man says those lines and
   the old translation follows from the line after his part. **To write: 43 openings for the song (about ten
   lines each) and three for the laments.** By the user's rule the words are the user's; until then the man says
   the first lines of the 1892 verse, and the window is honest about it by showing the same words the chest holds.
2. **35 texts have no English** (section 1). All 33 Latin riddles are among them. At the oar such a riddle shows
   its Latin and "There is no English of this one yet: only its own words." They want the game's own rendering
   (`ownTranslation`, as three texts already have).
3. **`seafarer` and `deor` have no original.** The code never shows an `original` that begins with a square bracket.
4. **The tongue lines' `text_id` is not read.** Ten real lines of Beowulf, thirteen of Havamal and three each of
   The Seafarer and The Wanderer already have hand-made pairs in `tongues_793.json`. One field on `TongueLine`
   (`textId`), one line in `TongueBook.Load` and a lookup by text would let those open the reading and turn. Not
   drafted: `TongueBook.cs` belongs to the tongues' builder.

And one that is not this task's: **the crew are seats, not people.** When they become people (the book's step 9),
a `ReaderSeat` becomes a view of a crewman: the same five fields come from him, and `from_men` and `heathen_seat`
go. Until then, if a man is lost (`Voyage.LoseMen`) it is the last seat that empties, which is arbitrary.

---

## 5. The rules that keep it from being a card every evening

| # | Rule | Number | Where |
|---|---|---|---|
| 1 | It never holds the clock and never must be answered. No numbered choices | | `FrameReading.Show` |
| 2 | It opens by itself only at speeds 1 and 2 | pace of 4 or less | `Aloud.QuietPace` |
| 3 | Only at sea, with nothing pressing and no storm | storm under 0.3 | `Aloud.Quiet`, `FrameReading.Pressed` |
| 4 | Not the moment the speed drops: the stretch must have been quiet a while | 20 real seconds | `Aloud.DwellSeconds` |
| 5 | One window in a quiet stretch | 1 | `Aloud.Step` |
| 6 | A floor in real time between two windows that open by themselves | 8 real minutes | `Aloud.GapSeconds` |
| 7 | One text begun a game day, from noon on; the next only when the last was taken up | 1 | `Aloud.Hour` |
| 8 | A riddle put again every third game day; one try a game day | 3, 1 | `Aloud.RiddleEveryDays`, `Aloud.CanTry` |
| 9 | A word never opens a window: a log line a game day at most, at quiet speeds | 1 | `FrameReading.Tell` |
| 10 | Let lie once: the men wait two game days. Twice running: they stop beginning by themselves until the player reads anything | 2 | `Aloud.LetLie`, `Aloud.StopAfter` |
| 11 | It gives way at once: a card, an alert, a sail, a faster speed or the pause folds an untouched window. It is still kept | | `Aloud.MustFold` |
| 12 | An untouched window folds itself after the man has done | 45 real seconds | `FrameReading.LingerSeconds` |
| 13 | The player can switch it off: "The men begin by themselves: no" | | `Aloud.MenBegin` |
| 14 | The 43 song cards are shut while this is in | flag `crew_song_tonight` | `Aloud.OnHour` |

**What the caps come to.** At speed 1 a game day is 24 real minutes: one text a day and a riddle every third day
is three or four windows at most in a crossing of 52 minutes. At speed 2 a day is 6 minutes and the 8-minute floor
is the tighter rule: two windows at most in a fair crossing of 13 minutes, four in a foul one of half an hour. Cards
at sea are held four game hours apart (`chartSeaGapHours`), which at speed 2 is one a real minute at most: the
reading comes an eighth as often as a card may, and unlike a card it can be ignored without a cost.

**Why this is not a card.** A card stops the clock, wants a choice and pays or costs by it. This stops nothing,
wants nothing, and what it gives is the text itself. The one thing it has in common with a card is that it appears,
and rules 4, 5, 6, 10 and 13 are all about that.

---

## 6. One crossing at speed 2, as it would go

Dorestat to Lundenwic, fair wind, 13 real minutes. A Frisian player; the four of the first crew. Not played: this
is the rules of section 5 worked through by hand.

| Real minute | Game time | What happens |
|---|---|---|
| 0:00 | day 1, 06:00 | Cast off. No text: it is before noon |
| 0:15 | day 1, 07:00 | The first hour at sea. The mate, the lookout and the Dane each name a word of their own tongue: counted, not shown. Once the stretch is quiet the log has one of them: "The Dane, at his work: ..." (one such line a game day) |
| 1:30 | day 1, 12:00 | The song's first part is due and goes into the chest. The stretch has been quiet long enough, so the window opens. Log: "The lookout begins Beowulf, fitt I ...". His 14 lines take 31 real seconds: two game hours |
| 2:01 | day 1, 14:00 | "The rest is yours. 58 lines more." The player reads on, 12 lines a press, or lets it lie |
| 3:00 | day 1, 18:00 | A card (`sea.*`). An untouched window folds here; a window the player is reading stays, behind the card |
| 7:30 | day 2, 12:00 | If part one was taken up, part two is due. It does not open: the last window opened at 1:30 and the floor is 8 minutes. Log: "The lookout has Beowulf, fitt II ... ready for a quiet hour. It will keep: Chest." |
| 9:30 | day 2, 20:00 | The floor is past. If the stretch is quiet the lookout begins part two |
| 13:00 | day 3, 10:00 | Nearing port: the clock's own drop, and nothing opens. What is unread is in the chest at its line |

At speed 4 the same crossing is 33 seconds. One log line at noon of the first day, and no more. When the player
next slows down at sea, twenty seconds later the lookout begins.

---

## 7. The code

Four new files, four edited ones, six stylesheet rules, one data file. All paths are from `G:/kapman_unity/Kapman2`.

### 7.1 New: `Assets/Kapman/Code/Game/Chest/ReaderBook.cs`

```csharp
using System;
using System.Collections.Generic;

namespace Kapman.Game
{
    /// <summary>One seat of the boat, as the crew cards speak through it (encounters/crew_793.json, "THE SEATS"):
    /// who sits there by people and faith, his own tongue, and whether he has letters or runes.</summary>
    public sealed class ReaderSeat
    {
        public string id = "", name = "", people = "", faith = "", tongue = "";
        /// <summary>He was schooled: he reads the Latin letters (and so English written in them).</summary>
        public bool letters;
        /// <summary>He reads runes.</summary>
        public bool runes;
        /// <summary>He is aboard only while the boat has a heathen (the cards' own rule: a year before 816, or the
        /// flag crew_heathen_aboard).</summary>
        public bool heathenSeat;
        /// <summary>The boat must have this many men for the seat to be filled.</summary>
        public int fromMen = 1;
    }

    /// <summary>A row of who knows what: a set of texts, how they are had (by heart, or read from a leaf that is in
    /// the chest), and the men it holds for (by seat, people, faith, letters, runes).</summary>
    public sealed class ReaderRule
    {
        public string id = "", title = "", how = "recite", why = "";
        public string[] texts = Array.Empty<string>();
        public string[] seats = Array.Empty<string>(), peoples = Array.Empty<string>(), faiths = Array.Empty<string>();
        public bool letters, runes;
        /// <summary>How much of a text he gives before the rest is the player's (0 to 1).</summary>
        public double share = 1.0 / 3.0;
        /// <summary>The years it is said as a living thing (0: no limit).</summary>
        public int notBefore, notAfter;

        public bool ByHeart => how != "read";

        public static bool Matches(string pattern, string textId) =>
            pattern == textId || (pattern.EndsWith("*", StringComparison.Ordinal) && textId.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.Ordinal));

        public bool Covers(string textId)
        {
            if (string.IsNullOrEmpty(textId))
                return false;
            foreach (string t in texts)
                if (Matches(t, textId))
                    return true;
            return false;
        }

        public bool Fits(ReaderSeat s, int year)
        {
            if (s == null || (notBefore != 0 && year < notBefore) || (notAfter != 0 && year > notAfter))
                return false;
            if (seats.Length > 0 && Array.IndexOf(seats, s.id) < 0)
                return false;
            if (peoples.Length > 0 && Array.IndexOf(peoples, s.people) < 0)
                return false;
            if (faiths.Length > 0 && Array.IndexOf(faiths, s.faith) < 0)
                return false;
            return (!letters || s.letters) && (!runes || s.runes);
        }
    }

    /// <summary>
    /// WHO ABOARD KNOWS WHAT (Design/fixes/reading_at_sea.md): the seats of the boat and the texts each can say by
    /// heart or read from a leaf, by his people, his faith and his letters. Read from story/readers_793.json; with no
    /// file, a small table of its own that says only what the crew cards already say (the lookout has the song; the
    /// Dane has the sayings and reads the staves; the lookout reads a Latin leaf in part).
    /// Until the crew are people with names (the book's step 9), a seat is all there is to a man.
    /// </summary>
    public sealed class ReaderBook
    {
        public const string File = "story/readers_793.json";

        public readonly List<ReaderSeat> seats = new List<ReaderSeat>();
        public readonly List<ReaderRule> rules = new List<ReaderRule>();
        /// <summary>The year before which a text is not said aloud as a living thing (Maldon before 991).</summary>
        public readonly Dictionary<string, int> notBefore = new Dictionary<string, int>(StringComparer.Ordinal);
        /// <summary>A text's opening in plain English, as the man says it (the writers' to fill; empty today).</summary>
        public readonly Dictionary<string, string[]> plain = new Dictionary<string, string[]>(StringComparer.Ordinal);
        public bool FromFile { get; private set; }

        static readonly ContentCache<ReaderBook> cache = new ContentCache<ReaderBook>("who aboard knows what", Load);
        public static ReaderBook Current => cache.Value;

        public ReaderSeat Seat(string id) => seats.Find(s => s.id == id);

        /// <summary>The seats that are filled: by how many men she has, and whether a heathen is aboard.</summary>
        public List<ReaderSeat> Aboard(int men, bool heathen)
        {
            var list = new List<ReaderSeat>();
            foreach (var s in seats)
                if (men >= s.fromMen && (!s.heathenSeat || heathen))
                    list.Add(s);
            return list;
        }

        /// <summary>May this text be said aloud in this year at all?</summary>
        public bool Living(string textId, int year)
        {
            if (string.IsNullOrEmpty(textId))
                return false;
            foreach (var kv in notBefore)
                if (year < kv.Value && ReaderRule.Matches(kv.Key, textId))
                    return false;
            return true;
        }

        /// <summary>Who of these men gives this text, and by which row: one who has it by heart before one who reads
        /// it. A text that is read must be in the chest (`inChest`). Nobody aboard: (null, null).</summary>
        public (ReaderSeat seat, ReaderRule rule) Who(string textId, IReadOnlyList<ReaderSeat> aboard, int year, bool inChest)
        {
            if (aboard == null || !Living(textId, year))
                return (null, null);
            for (int pass = 0; pass < 2; pass++)
                foreach (var r in rules)
                {
                    if (r.ByHeart != (pass == 0) || !r.Covers(textId) || (!r.ByHeart && !inChest))
                        continue;
                    foreach (var s in aboard)
                        if (r.Fits(s, year))
                            return (s, r);
                }
            return (null, null);
        }

        /// <summary>The rows that hold for a man in a year.</summary>
        public List<ReaderRule> RowsOf(ReaderSeat s, int year)
        {
            var list = new List<ReaderRule>();
            foreach (var r in rules)
                if (r.Fits(s, year))
                    list.Add(r);
            return list;
        }

        // ------------------------------------------------------------------ reading

        static ReaderBook Load()
        {
            var book = new ReaderBook();
            var root = ContentFiles.Exists(File) ? ContentFiles.ParseObject(File) : null;
            if (root == null)
            {
                book.Own();
                return book;
            }
            foreach (var x in ContentJson.Rows(root, "seats"))
            {
                var s = new ReaderSeat
                {
                    id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), people = ContentJson.Text(x, "people"),
                    faith = ContentJson.Text(x, "faith"), tongue = ContentJson.Text(x, "first_tongue"),
                    letters = ContentJson.Num(x, "letters") > 0.0, runes = ContentJson.Num(x, "runes") > 0.0,
                    heathenSeat = ContentJson.Num(x, "heathen_seat") > 0.0, fromMen = Math.Max(1, ContentJson.Int(x, "from_men", 1)),
                };
                if (s.id.Length > 0)
                    book.seats.Add(s);
            }
            foreach (var x in ContentJson.Rows(root, "knows"))
            {
                var r = new ReaderRule
                {
                    id = ContentJson.Text(x, "id"), title = ContentJson.Text(x, "title"), how = ContentJson.Text(x, "how", "recite"),
                    why = ContentJson.Text(x, "why"),
                    texts = ContentJson.Strings(x, "texts"), seats = ContentJson.Strings(x, "seats"),
                    peoples = ContentJson.Strings(x, "peoples"), faiths = ContentJson.Strings(x, "faiths"),
                    letters = ContentJson.Num(x, "letters") > 0.0, runes = ContentJson.Num(x, "runes") > 0.0,
                    share = Math.Max(0.05, Math.Min(1.0, ContentJson.Num(x, "share", 1.0 / 3.0))),
                    notBefore = ContentJson.Int(x, "not_before"), notAfter = ContentJson.Int(x, "not_after"),
                };
                if (r.id.Length > 0 && r.texts.Length > 0)
                    book.rules.Add(r);
            }
            foreach (var kv in ContentJson.Pairs(root, "not_before"))
                if (kv.Value is double year)
                    book.notBefore[kv.Key] = (int)year;
            foreach (var kv in ContentJson.Pairs(root, "plain"))
                if (kv.Value is List<object> lines)
                    book.plain[kv.Key] = lines.ConvertAll(l => l as string ?? "").ToArray();
            book.FromFile = book.seats.Count > 0 && book.rules.Count > 0;
            if (!book.FromFile)
                book.Own();
            return book;
        }

        /// <summary>The table with no file, as a book of its own (a test).</summary>
        public static ReaderBook OwnTable()
        {
            var book = new ReaderBook();
            book.Own();
            return book;
        }

        /// <summary>The table with no file: only what the crew cards say already.</summary>
        void Own()
        {
            seats.Clear();
            rules.Clear();
            seats.Add(new ReaderSeat { id = "steersman", name = "the steersman", people = "frisian", faith = "christian", tongue = "old_frisian", fromMen = 1 });
            seats.Add(new ReaderSeat { id = "mate", name = "the mate", people = "saxon", faith = "christian", tongue = "old_saxon", fromMen = 2 });
            seats.Add(new ReaderSeat { id = "lookout", name = "the lookout", people = "english", faith = "christian", tongue = "old_english", letters = true, fromMen = 3 });
            seats.Add(new ReaderSeat { id = "dane", name = "the Dane", people = "danish", faith = "old_gods", tongue = "old_norse", runes = true, heathenSeat = true, fromMen = 4 });
            seats.Add(new ReaderSeat { id = "hand", name = "the hand", people = "frisian", faith = "christian", tongue = "old_frisian", fromMen = 5 });
            rules.Add(new ReaderRule { id = "song", title = "the song of Beowulf", texts = new[] { "beowulf_*" }, seats = new[] { "lookout" } });
            rules.Add(new ReaderRule { id = "english_verse", title = "the English laments", texts = new[] { "seafarer", "wanderer", "deor" }, peoples = new[] { "english" } });
            rules.Add(new ReaderRule { id = "sayings", title = "the sayings of the High One", texts = new[] { "havamal_*" }, peoples = new[] { "danish", "norse", "svea", "gute" } });
            rules.Add(new ReaderRule { id = "staves", title = "rune staves", how = "read", texts = new[] { "t-gest-*", "t-vafth-*", "t-alv-*", "stone:*" }, runes = true, share = 0.5 });
            rules.Add(new ReaderRule { id = "latin_leaf", title = "a Latin leaf, slowly", how = "read", texts = new[] { "aldhelm_*", "symphosius_*", "tatwine_*", "eusebius_*", "alcuin_793", "dicuil" }, letters = true, share = 0.25 });
            notBefore["maldon_991"] = 991;
        }
    }
}
```

### 7.2 New: `Assets/Kapman/Code/Game/Chest/Aloud.cs`

The rules and what is kept. `Hour` and everything it calls is speed-blind; `Step`, `Quiet` and `MustFold` are the
only members that read the pace or the wall clock, and they change nothing that is saved.

```csharp
using System;
using System.Collections.Generic;

namespace Kapman.Game
{
    /// <summary>The moment, as the rules of the reading need it. The game fills it (<see cref="Aloud.NowOf"/>); a test
    /// fills it by hand.</summary>
    public struct AloudNow
    {
        /// <summary>Game days since the start, with the hours; the hour of the day; the year.</summary>
        public double day, hour;
        public int year;
        /// <summary>Game minutes that pass in one real second (0: the clock stands). Speed 1 of the ruling is 1,
        /// speed 2 is 4, speed 3 about 24, speed 4 about 96, speed 5 about 360.</summary>
        public float pace;
        /// <summary>The wall clock, in seconds.</summary>
        public float real;
        /// <summary>Cast off and clear of a port's business (under way, lying to, or waiting on the water).</summary>
        public bool atSea;
        /// <summary>Something needs the player: a card, an alert, another floating window, the chest, the menu.</summary>
        public bool pressed;
        public double storm;
        public int men;
        public bool heathen;
    }

    public enum AloudKind { Text, Riddle, Word }

    /// <summary>A thing a man of the crew has ready: the next part of a text, a riddle put again, a word you now
    /// have. It is kept until a quiet stretch, and one of a kind at most is ever kept.</summary>
    public sealed class AloudOffer
    {
        public AloudKind kind;
        /// <summary>The text's id, the riddle's id, or the word; and the seat of the man who gives it.</summary>
        public string id = "", seat = "";
        public double day;
        /// <summary>The log has said it once.</summary>
        public bool told;
    }

    /// <summary>
    /// THE READING AT SEA (Design/fixes/reading_at_sea.md; the user, 3 Oct 2026: "a crewman reads part, you finish").
    /// The empty hours of a crossing are the hours the game reads to you: a man of the crew begins a text he has by
    /// heart or can read, in a window that asks nothing and never holds the clock; the rest is the player's to read,
    /// now or on another day; a riddle that is in the chest is put again and tried once a game day; a word of a
    /// man's own tongue is heard each day at sea until it stays.
    ///
    /// TWO HALVES, kept apart on purpose (ruling 26: nothing may be lost at speed 5 that would have happened at 1):
    ///   - WHAT IS DUE goes by the game's hours alone (<see cref="Hour"/>, on KapmanGame.HourPassed): the same at
    ///     every speed, with no random numbers and nothing read from the wall clock.
    ///   - WHAT IS SHOWN goes by the wall clock and the speed (<see cref="Step"/>): at speeds 1 and 2, at sea, with
    ///     nothing pressing, one window may open by itself in a quiet stretch. At speeds 3 to 5 nothing opens: what
    ///     is due is kept for the next quiet stretch. Showing a thing changes nothing in the world.
    /// What the player gains comes only from what the player does (reading on, a try at a riddle), never from sitting
    /// at a slow speed.
    /// </summary>
    public static class Aloud
    {
        public const string Key = "aloud";
        /// <summary>The fastest pace at which a man begins by himself: speed 2 of the ruling (four game minutes a
        /// real second).</summary>
        public const float QuietPace = 4.01f;
        /// <summary>Real seconds a stretch must have been quiet before anything opens (so stepping down through
        /// speed 2 opens nothing).</summary>
        public const float DwellSeconds = 20f;
        /// <summary>Real seconds between two windows that open by themselves (eight minutes: a third of a day at
        /// speed 1, a day and a third at speed 2).</summary>
        public const float GapSeconds = 480f;
        public const double StormMax = 0.3;
        /// <summary>A text is begun from this hour of the day on (the idle afternoon and the evening).</summary>
        public const double FromHour = 12.0;
        /// <summary>Let lie this many times running, the men stop beginning by themselves until you read again.</summary>
        public const int StopAfter = 2;
        /// <summary>Game days between two times a riddle is put again by itself.</summary>
        public const int RiddleEveryDays = 3;
        /// <summary>A text under this many words of English counts as a quarter of one for Lore.</summary>
        public const int ShortWords = 150;
        /// <summary>The most lines a man says before the rest is the player's.</summary>
        public const int SaidMax = 14;
        /// <summary>What a word learned from a man adds to his tongue, and how far words alone can take it.</summary>
        public const double WordLift = 0.02, WordCap = 0.6;
        /// <summary>The kinds of word a man names aboard (the kinds of story/tongues_793.json).</summary>
        public static readonly string[] WordKinds = { "ship", "sea", "food", "trade", "greeting" };
        const int Never = -1000000;

        // ------------------------------------------------------------------ what is kept in the saved game

        /// <summary>The setting: the men may begin by themselves (off: the lamp on the Chest window only).</summary>
        public static bool MenBegin = true;
        /// <summary>The text the player has put in hand (a text's id, or a set as "beowulf_*"; "": the men choose).</summary>
        public static string InHand { get; private set; } = "";
        /// <summary>The text a man has begun that the player has not taken up ("": none).</summary>
        public static string Pending { get; private set; } = "";
        /// <summary>Times running a window that opened by itself was let lie.</summary>
        public static int LetLieRun { get; private set; }

        static readonly Dictionary<string, int> place = new Dictionary<string, int>(StringComparer.Ordinal);
        static readonly HashSet<string> finished = new HashSet<string>(StringComparer.Ordinal);
        static readonly HashSet<string> takenUp = new HashSet<string>(StringComparer.Ordinal);
        static readonly List<AloudOffer> kept = new List<AloudOffer>();
        static readonly Dictionary<string, string> wordOf = new Dictionary<string, string>(StringComparer.Ordinal);
        static int textDay = Never, riddleDay = Never, tryDay = Never, wordDay = Never, heartDay = Never, shortRead;
        static double lieUntil;

        // (not saved: the wall clock's side)
        static float quietSince = -1f, lastOpen = -1e9f;
        static bool openedThisStretch;

        /// <summary>Counts up with every change (a panel's stamp).</summary>
        public static int Version { get; private set; }
        public static IReadOnlyList<AloudOffer> Kept => kept;
        public static AloudOffer KeptOf(AloudKind kind) => kept.Find(o => o.kind == kind);

        // ------------------------------------------------------------------ what is due: the game's hours only

        /// <summary>One game hour has passed (KapmanGame.HourPassed: once an hour at every speed). Works out what the
        /// men have ready and keeps it. Nothing here looks at the speed or the wall clock.</summary>
        public static void Hour(in AloudNow now, ReaderBook book)
        {
            if (book == null || !now.atSea || now.men <= 0)
                return;
            int d = (int)Math.Floor(now.day);
            var aboard = book.Aboard(now.men, now.heathen);

            // the text: one begun a game day, from noon on; the next only when the last was taken up
            if (now.hour >= FromHour && d > textDay)
            {
                textDay = d;
                if (Pending.Length > 0 && (takenUp.Contains(Pending) || finished.Contains(Pending)))
                    Pending = "";
                if (Pending.Length == 0)
                {
                    var (id, seat) = NextText(book, aboard, now.year);
                    if (id != null)
                    {
                        Pending = id;
                        if (!Chest.Has(id))
                            Chest.Give("text", id, seat.name, null, true);
                        Keep(AloudKind.Text, id, seat.id, now.day);
                    }
                }
                else if (KeptOf(AloudKind.Text) == null)
                {
                    var who = book.Who(Pending, aboard, now.year, Chest.Has(Pending));
                    if (who.seat != null)
                        Keep(AloudKind.Text, Pending, who.seat.id, now.day);
                }
            }

            // the riddle: one that lies open in the chest is put again, every third day at most
            if (d >= riddleDay + RiddleEveryDays && KeptOf(AloudKind.Riddle) == null)
            {
                var item = OpenRiddle(book, aboard, now.year, out var seat);
                if (item != null)
                {
                    riddleDay = d;
                    Keep(AloudKind.Riddle, item.def, seat != null ? seat.id : "", now.day);
                }
            }

            // the word: each man whose tongue is not yours names one thing a day; heard often enough, it stays
            if (d > wordDay)
            {
                wordDay = d;
                foreach (var s in aboard)
                    Hear(s, now.day);
            }
        }

        /// <summary>Keeps a thing for the next quiet stretch (one of a kind: a newer one takes the place of an older).</summary>
        public static void Keep(AloudKind kind, string id, string seat, double day)
        {
            kept.RemoveAll(o => o.kind == kind);
            kept.Add(new AloudOffer { kind = kind, id = id, seat = seat, day = day });
            Version++;
        }

        /// <summary>The text a man begins next, and who: what the player put in hand; else the next part of a set
        /// already begun; else a leaf lying unread in the chest that a man can read; else something new a man has by
        /// heart (the rows of the readers' file, in their order). (null, null): nothing.</summary>
        public static (string id, ReaderSeat seat) NextText(ReaderBook book, IReadOnlyList<ReaderSeat> aboard, int year)
        {
            // 1. in hand
            if (InHand.Length > 0)
            {
                string id = FirstUnread(InHand);
                var who = id != null ? book.Who(id, aboard, year, Chest.Has(id)) : (null, null);
                if (who.seat != null)
                    return (id, who.seat);
            }
            // 2. a set already begun (a text of a series that is in the chest and finished or taken up: its next part)
            foreach (var r in book.rules)
            {
                if (!r.ByHeart)
                    continue;
                string next = null;
                bool begun = false;
                foreach (string id in TextIds(r))
                {
                    if (Chest.Has(id))
                        begun = true;
                    else if (next == null)
                        next = id;
                }
                if (begun && next != null)
                {
                    var who = book.Who(next, aboard, year, false);
                    if (who.seat != null)
                        return (next, who.seat);
                }
            }
            // 3. a leaf in the chest that has not been read and that a man aboard can give
            foreach (var item in Chest.Items)
            {
                if (item.kind != "text" || item.Text == null || finished.Contains(item.def) || takenUp.Contains(item.def))
                    continue;
                var who = book.Who(item.def, aboard, year, true);
                if (who.seat != null)
                    return (item.def, who.seat);
            }
            // 4. something new by heart
            foreach (var r in book.rules)
            {
                if (!r.ByHeart)
                    continue;
                foreach (string id in TextIds(r))
                {
                    if (Chest.Has(id))
                        continue;
                    var who = book.Who(id, aboard, year, false);
                    if (who.seat != null)
                        return (id, who.seat);
                    break;                                               // (nobody for the first of the row: nobody for the row)
                }
            }
            return (null, null);
        }

        /// <summary>The ids of the chest's texts a row names, in the order of the texts' file.</summary>
        static IEnumerable<string> TextIds(ReaderRule r)
        {
            foreach (var kv in ChestData.Texts)
                if (r.Covers(kv.Key) && !string.IsNullOrEmpty(kv.Value.translation) && !RiddleWords(kv.Key))
                    yield return kv.Key;
        }

        static string FirstUnread(string pattern)
        {
            foreach (var kv in ChestData.Texts)
                if (ReaderRule.Matches(pattern, kv.Key) && !finished.Contains(kv.Key) && !string.IsNullOrEmpty(kv.Value.translation) && !RiddleWords(kv.Key))
                    return kv.Key;
            return null;
        }

        /// <summary>The words of a riddle or of a verse to decode (146 of the 267 texts): never handed out as a text
        /// to read, or the chest would hold the riddle twice.</summary>
        static bool RiddleWords(string textId)
        {
            foreach (var r in ChestData.Riddles.Values)
                if (r.textId == textId)
                    return true;
            foreach (var v in ChestData.Verses.Values)
                if (v.textId == textId)
                    return true;
            return false;
        }

        /// <summary>A riddle in the chest that is open (not answered, not resting): first one a man aboard can say.</summary>
        static ChestItem OpenRiddle(ReaderBook book, IReadOnlyList<ReaderSeat> aboard, int year, out ReaderSeat seat)
        {
            seat = null;
            ChestItem any = null;
            foreach (var item in Chest.Items)
            {
                if (item.kind != "riddle" || item.solved || item.resting || item.Riddle == null)
                    continue;
                any = any ?? item;
                string tongue = TongueOf(item.Text);
                foreach (var s in aboard)
                    if (s.tongue == tongue || (tongue == "latin" && s.letters))
                    {
                        seat = s;
                        return item;
                    }
            }
            return any;
        }

        /// <summary>A man names one thing in his own tongue. Counted once a game day at sea, whatever the speed.</summary>
        static void Hear(ReaderSeat s, double day)
        {
            if (s == null || s.tongue.Length == 0 || Self.TongueLevel(s.tongue) >= TurnPlan.KnownFrom)
                return;
            if (!wordOf.TryGetValue(s.id, out string word) || Self.Seen(word) >= Self.StaysAfter)
            {
                word = null;
                foreach (var w in TongueBook.Current.Words)
                    if (w.tongue == s.tongue && Array.IndexOf(WordKinds, w.kind) >= 0 && Self.Seen(w.word) < Self.StaysAfter)
                    {
                        word = w.word;
                        break;
                    }
                if (word == null)
                {
                    wordOf.Remove(s.id);
                    return;
                }
                wordOf[s.id] = word;
            }
            Self.See(word);
            if (Self.Seen(word) >= Self.StaysAfter)
            {
                // (it stays: his tongue is a little more yours, and the next quiet stretch says so once)
                double has = Math.Max(Self.TongueHeld(s.tongue), Self.TongueLevel(s.tongue));
                if (has < WordCap)
                    Self.SetTongue(s.tongue, Math.Min(WordCap, has + WordLift));
                Keep(AloudKind.Word, word, s.id, day);
            }
            Version++;
        }

        /// <summary>The word a man is naming these days ("": none: his tongue is yours, or he has no more).</summary>
        public static string WordOf(string seat) => seat != null && wordOf.TryGetValue(seat, out string w) ? w : "";

        // ------------------------------------------------------------------ what is shown: the wall clock and the speed

        /// <summary>A quiet stretch: a slow speed, the clock running, at sea, nothing pressing, no storm.</summary>
        public static bool Quiet(in AloudNow now) =>
            now.pace > 0f && now.pace <= QuietPace && now.atSea && !now.pressed && now.storm < StormMax;

        /// <summary>Asked a few times a second by the frame: the offer whose window may open by itself now, or null.
        /// One a stretch, none within <see cref="GapSeconds"/> of the last, none while the men hold back.</summary>
        public static AloudOffer Step(in AloudNow now)
        {
            if (!Quiet(now))
            {
                quietSince = -1f;
                openedThisStretch = false;
                return null;
            }
            if (quietSince < 0f)
                quietSince = now.real;
            if (!MenBegin || openedThisStretch || LetLieRun >= StopAfter || now.day < lieUntil)
                return null;
            if (now.real - quietSince < DwellSeconds || now.real - lastOpen < GapSeconds)
                return null;
            var offer = KeptOf(AloudKind.Text) ?? KeptOf(AloudKind.Riddle);     // (a word never opens a window)
            if (offer == null)
                return null;
            openedThisStretch = true;
            lastOpen = now.real;
            return offer;
        }

        /// <summary>A window that opened by itself and was not taken up folds at once when the stretch is quiet no
        /// more (a card, a sail, a faster speed, the pause).</summary>
        public static bool MustFold(in AloudNow now) => !Quiet(now);

        /// <summary>A window that opened by itself was shut, or folded itself, with nothing read. Once: the men wait
        /// two game days. Twice running: they stop beginning by themselves until the player reads again.</summary>
        public static void LetLie(double day)
        {
            LetLieRun++;
            lieUntil = day + 2.0 * LetLieRun;
            Version++;
        }

        /// <summary>Do the men hold back now (the Chest window says so)?</summary>
        public static bool HoldingBack => LetLieRun >= StopAfter;

        // ------------------------------------------------------------------ what the player does

        public static int Place(string textId) => textId != null && place.TryGetValue(textId, out int n) ? n : 0;
        public static bool Finished(string textId) => textId != null && finished.Contains(textId);
        public static bool TakenUp(string textId) => textId != null && takenUp.Contains(textId);

        /// <summary>The player put a text in hand (the Chest window), or took it out ("").</summary>
        public static void Put(string textOrSet)
        {
            InHand = textOrSet ?? "";
            Version++;
        }

        /// <summary>The player read on: the leaf keeps the place. The first time for a text, it is taken up: the men
        /// are heartened once a game day, and they begin by themselves again.</summary>
        public static void ReadTo(string textId, int line, int total, double day)
        {
            var t = ChestData.Text(textId);
            if (t == null)
                return;
            place[textId] = Math.Max(Place(textId), Math.Min(line, total));
            if (takenUp.Add(textId))
            {
                int d = (int)Math.Floor(day);
                if (d > heartDay)
                {
                    heartDay = d;
                    Voyage.Affect(0.0, 0.02);                            // (as "Listen" on the evening song's card did)
                }
            }
            LetLieRun = 0;
            lieUntil = 0.0;
            kept.RemoveAll(o => o.kind == AloudKind.Text && o.id == textId);
            if (line >= total && finished.Add(textId))
            {
                // a text had whole: Lore by use, a long text a full share, four short ones a share between them
                int words = (t.translation ?? "").Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (words >= ShortWords || ++shortRead % 4 == 0)
                    Self.Used("lore", 1.0);
            }
            Version++;
        }

        /// <summary>May a riddle be tried today (one try a game day at the oar)?</summary>
        public static bool CanTry(double day) => (int)Math.Floor(day) > tryDay;

        /// <summary>A try at a riddle with one answer: Chest.Answer judges it. One a game day.</summary>
        public static string Try(ChestItem item, string thingId, double day)
        {
            if (item == null || item.Riddle == null)
                return "";
            if (item.Riddle.SlotCount > 1)
                return "This one wants more than one answer: open the chest for it.";
            if (!CanTry(day))
                return "One try a day at the oar. Sleep on it.";
            tryDay = (int)Math.Floor(day);
            LetLieRun = 0;
            lieUntil = 0.0;
            string fact = Chest.Answer(item, thingId);
            if (item.solved || item.resting)
                kept.RemoveAll(o => o.kind == AloudKind.Riddle && o.id == item.def);
            Version++;
            return fact;
        }

        /// <summary>A kept thing has been shown and is done with (a word told; a riddle looked at).</summary>
        public static void Done(AloudOffer offer)
        {
            if (offer != null && kept.Remove(offer))
                Version++;
        }

        // ------------------------------------------------------------------ the words of a text

        /// <summary>A text's tongue as the tongues' file names it ("Old Norse" is old_norse, "Old Irish" irish).</summary>
        public static string TongueOf(TextDef t)
        {
            string l = (t != null ? t.language ?? "" : "").Trim().ToLowerInvariant().Replace(' ', '_');
            return l == "old_irish" ? "irish" : l;
        }

        /// <summary>The lines of a text as they are read: its own line breaks; prose in one piece goes by sentences.</summary>
        public static List<string> Lines(string text)
        {
            var list = new List<string>();
            foreach (string raw in (text ?? "").Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0)
                    continue;
                if (line.Length <= 240)
                {
                    list.Add(line);
                    continue;
                }
                int from = 0;
                for (int i = 0; i < line.Length - 1; i++)
                    if ((line[i] == '.' || line[i] == ';' || line[i] == '?' || line[i] == '!') && line[i + 1] == ' ' && i - from >= 40)
                    {
                        list.Add(line.Substring(from, i + 1 - from).Trim());
                        from = i + 1;
                    }
                if (from < line.Length)
                    list.Add(line.Substring(from).Trim());
            }
            return list;
        }

        /// <summary>The English a text is read in: the plain opening where the readers' file has one (the man's own
        /// words), then the translation.</summary>
        public static List<string> English(TextDef t) => Lines(t != null ? t.translation : null);

        /// <summary>How many lines the man gives before the rest is the player's.</summary>
        public static int SaidLines(int total, double share) =>
            total <= 0 ? 0 : Math.Max(1, Math.Min(Math.Min(SaidMax, total), (int)Math.Ceiling(total * share)));

        /// <summary>The first lines of a text in its own tongue beside their English, where the two run line for line
        /// (73 of the 267 today: the wisdom contests, the vow). Else none: nothing is paired by guess.</summary>
        public static List<(string original, string english)> Opening(TextDef t, int max)
        {
            var pairs = new List<(string, string)>();
            if (t == null || string.IsNullOrEmpty(t.translation) || (t.original ?? "").TrimStart().StartsWith("[", StringComparison.Ordinal))
                return pairs;
            var a = Lines(t.original);
            var b = Lines(t.translation);
            if (a.Count != b.Count || a.Count < 2)
                return pairs;
            for (int i = 0; i < a.Count && i < max; i++)
                pairs.Add((a[i], b[i]));
            return pairs;
        }

        // ------------------------------------------------------------------ the game's side

        /// <summary>The moment as the game has it. `pace` and `pressed` are the frame's to give (the frame knows the
        /// speed and what is on screen): here they are 0 and false.</summary>
        public static bool NowOf(out AloudNow now)
        {
            now = default;
            var game = KapmanGame.Current;
            if (game == null || game.ship == null || !Voyage.Started)
                return false;
            now.day = game.Day;
            now.hour = game.clock != null ? game.clock.HourOfDay : 12.0;
            now.year = game.Year;
            now.men = Voyage.Men;
            now.atSea = Frame.FrameGame.PortHere == null;
            now.storm = ChartWorld.TryWeather(out var w) ? w.storm : 0.0;
            var director = EncounterRunner.Director;
            now.heathen = game.Year < 816 || (director != null && director.state.Flag("crew_heathen_aboard", game.Day) > 0.0);
            return true;
        }

        /// <summary>The flag the 43 evening-song cards wait on (crew.song.fitt_NN: "not flag.crew_song_tonight"), and
        /// the one the card of the song's end waits for (crew.song.paid: "flag.crew_song_whole").</summary>
        public const string SongFlag = "crew_song_tonight", SongWholeFlag = "crew_song_whole";

        static void OnHour(double hour)
        {
            var game = KapmanGame.Current;
            var director = EncounterRunner.Director;
            if (game != null && director != null)
            {
                // the song is read here now, not dealt as a card each evening: the cards' own flag is held up for good
                if (director.state.Flag(SongFlag, game.Day) <= 0.0)
                    director.state.SetFlag(SongFlag, 1.0, game.Day, 0.0);
                if (Finished("beowulf_43") && director.state.Flag(SongWholeFlag, game.Day) <= 0.0)
                    director.state.SetFlag(SongWholeFlag, 1.0, game.Day, 0.0);
            }
            if (NowOf(out var now))
                Hour(now, ReaderBook.Current);
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnPlay()
        {
            // (after SubsystemRegistration, where KapmanGame empties its events)
            KapmanGame.HourPassed -= OnHour;
            KapmanGame.HourPassed += OnHour;
        }

        // ------------------------------------------------------------------ saving

        public static object Capture()
        {
            var offers = new List<object>();
            foreach (var o in kept)
                offers.Add(new Dictionary<string, object> { ["kind"] = o.kind.ToString(), ["id"] = o.id, ["seat"] = o.seat, ["day"] = o.day, ["told"] = o.told });
            var places = new Dictionary<string, object>();
            foreach (var kv in place)
                places[kv.Key] = (double)kv.Value;
            var words = new Dictionary<string, object>();
            foreach (var kv in wordOf)
                words[kv.Key] = kv.Value;
            return new Dictionary<string, object>
            {
                ["men_begin"] = MenBegin, ["in_hand"] = InHand, ["pending"] = Pending, ["let_lie"] = (double)LetLieRun, ["lie_until"] = lieUntil,
                ["text_day"] = (double)textDay, ["riddle_day"] = (double)riddleDay, ["try_day"] = (double)tryDay, ["word_day"] = (double)wordDay,
                ["heart_day"] = (double)heartDay, ["short_read"] = (double)shortRead,
                ["place"] = places, ["finished"] = new List<object>(finished), ["taken_up"] = new List<object>(takenUp),
                ["kept"] = offers, ["word_of"] = words,
            };
        }

        /// <summary>Null (a save from before the reading was kept): the plain start.</summary>
        public static void Restore(object saved)
        {
            Clear();
            if (!(saved is Dictionary<string, object> d))
                return;
            MenBegin = !d.TryGetValue("men_begin", out object mb) || !(mb is bool b) || b;
            InHand = Json.Str(d, "in_hand");
            Pending = Json.Str(d, "pending");
            LetLieRun = (int)Json.Num(d, "let_lie");
            lieUntil = Json.Num(d, "lie_until");
            textDay = (int)Json.Num(d, "text_day", Never);
            riddleDay = (int)Json.Num(d, "riddle_day", Never);
            tryDay = (int)Json.Num(d, "try_day", Never);
            wordDay = (int)Json.Num(d, "word_day", Never);
            heartDay = (int)Json.Num(d, "heart_day", Never);
            shortRead = (int)Json.Num(d, "short_read");
            if (Json.Obj(d, "place") is Dictionary<string, object> places)
                foreach (var kv in places)
                    if (kv.Value is double n)
                        place[kv.Key] = (int)n;
            foreach (object o in Json.Arr(d, "finished") ?? new List<object>())
                if (o is string s)
                    finished.Add(s);
            foreach (object o in Json.Arr(d, "taken_up") ?? new List<object>())
                if (o is string s)
                    takenUp.Add(s);
            foreach (object o in Json.Arr(d, "kept") ?? new List<object>())
                if (Enum.TryParse(Json.Str(o, "kind"), out AloudKind kind) && Json.Str(o, "id").Length > 0)
                    kept.Add(new AloudOffer { kind = kind, id = Json.Str(o, "id"), seat = Json.Str(o, "seat"), day = Json.Num(o, "day"), told = Json.Bool(o, "told") });
            if (Json.Obj(d, "word_of") is Dictionary<string, object> words)
                foreach (var kv in words)
                    if (kv.Value is string w)
                        wordOf[kv.Key] = w;
            Version++;
        }

        /// <summary>Back to the plain start (a new game; a test).</summary>
        public static void Clear()
        {
            MenBegin = true;
            InHand = "";
            Pending = "";
            LetLieRun = 0;
            lieUntil = 0.0;
            textDay = riddleDay = tryDay = wordDay = heartDay = Never;
            shortRead = 0;
            place.Clear();
            finished.Clear();
            takenUp.Clear();
            kept.Clear();
            wordOf.Clear();
            quietSince = -1f;
            lastOpen = -1e9f;
            openedThisStretch = false;
            Version++;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Clear();
            ContentSave.Register(Key, Capture, Restore);
        }
    }
}
```

### 7.3 New: `Assets/Kapman/Code/Game/UI/Frame/FrameReading.cs`

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE READING AT SEA, ON SCREEN (Design/fixes/reading_at_sea.md). One floating window, id "reading": it holds
    /// no clock, must not be answered, and has no numbered choices, so no key meant for something else lands on it.
    /// It opens by itself only as <see cref="Aloud.Step"/> allows (a slow speed, at sea, nothing pressing, one in a
    /// quiet stretch), and folds itself the moment the stretch is quiet no more. The Chest window opens it by hand at
    /// any time and any speed. What the rules keep, this shows; it decides nothing.
    /// </summary>
    public static class FrameReading
    {
        public const string FloatId = "reading";
        /// <summary>Real seconds between two lines of the man's part; lines the player turns up at a press; real
        /// seconds a window that opened by itself stays after the man has done, if nothing is pressed.</summary>
        public const float LineSeconds = 2.2f, LingerSeconds = 45f;
        public const int Page = 12;

        /// <summary>TO THE BUILDER OF THE CLOCK (ruling 26): set this to say "the clock has just dropped by itself"
        /// (a sail close, nearing port, the debt's day). While it is true nothing opens here.</summary>
        public static Func<bool> AlsoPressed;

        static ReadingLeaf leaf;
        static bool bySelf;
        static float nextLook;
        static int wordLineDay = int.MinValue;

        /// <summary>Game minutes that pass in a real second. TO THE BUILDER OF THE CLOCK: today the speed is the
        /// time scale (GameSpeed.Factor: 1, 2, 4). If speeds 3 to 5 are made another way, give the true pace here:
        /// speed 2 must read 4 or less, speed 3 more than 4.</summary>
        public static float Pace() => GameSpeed.Factor;

        /// <summary>The window is up.</summary>
        public static bool Open => leaf != null && FrameRoot.Current != null && FrameRoot.Current.FloatOpen(FloatId);

        /// <summary>Each frame, from FrameRoot.Update.</summary>
        public static void Tick(FrameRoot frame)
        {
            if (frame == null)
                return;
            float real = Time.realtimeSinceStartup;
            if (leaf != null && !frame.FloatOpen(FloatId))
                leaf = null;
            leaf?.Step(real);
            if (real < nextLook)
                return;
            nextLook = real + 0.5f;
            if (!Aloud.NowOf(out var now))
                return;
            now.real = real;
            now.pace = Pace();
            now.pressed = Pressed(frame);

            if (leaf != null && bySelf && !leaf.Taken)
            {
                if (Aloud.MustFold(now))
                    Close(frame);                                        // (it gives way at once, and costs nothing: it is kept)
                else if (leaf.Lingered(real))
                {
                    Aloud.LetLie(now.day);
                    Close(frame);
                }
            }
            if (leaf == null)
            {
                var offer = Aloud.Step(now);
                if (offer != null)
                    Show(frame, offer, true);
            }
            Tell(now);
        }

        static bool Pressed(FrameRoot frame)
        {
            var director = EncounterRunner.Director;
            if ((director != null && director.Current != null) || Alerts.Open.Count > 0 || FrameRoot.FloatHoldsClock)
                return true;
            foreach (string id in frame.FloatIds)
                if (id != FloatId)
                    return true;
            var map = MapView.Current;
            var menu = MainMenu.Current;
            return (map != null && map.ChestOpen) || (menu != null && menu.Open) || (AlsoPressed != null && AlsoPressed());
        }

        /// <summary>The log's part: one line when a thing is first kept (at any speed, once), and at a quiet speed
        /// one word a game day from the men.</summary>
        static void Tell(in AloudNow now)
        {
            var book = ReaderBook.Current;
            foreach (var o in Aloud.Kept)
            {
                if (o.told)
                    continue;
                o.told = true;
                string who = Cap(book.Seat(o.seat)?.name ?? "one of the men");
                if (o.kind == AloudKind.Text)
                    FrameLog.Add($"{who} has {Title(o.id)} ready for a quiet hour. It will keep: Chest.");
                else if (o.kind == AloudKind.Riddle)
                    FrameLog.Add($"{who} would put the riddle to you again. It will keep: Chest.");
                else
                {
                    var w = Word(book.Seat(o.seat), o.id);
                    FrameLog.Add($"You have {book.Seat(o.seat)?.name ?? "his"} word for it now: {o.id}{(w != null ? ", " + w.means : "")}.");
                    Aloud.Done(o);
                    return;                                              // (the list changed under the loop)
                }
            }
            int d = (int)Math.Floor(now.day);
            if (d == wordLineDay || !Aloud.Quiet(now))
                return;
            wordLineDay = d;
            var aboard = book.Aboard(now.men, now.heathen);
            for (int i = 0; i < aboard.Count; i++)
            {
                var s = aboard[(i + d) % aboard.Count];                  // (a different man each day)
                var w = Word(s, Aloud.WordOf(s.id));
                if (w == null)
                    continue;
                FrameLog.Add($"{Cap(s.name)}, at his work: \"{w.word}\"{(w.say.Length > 0 ? " (say: " + w.say + ")" : "")}. {Cap(w.means)}. Heard {Self.Seen(w.word)} of {Self.StaysAfter}.");
                break;
            }
        }

        static TongueWord Word(ReaderSeat s, string word) =>
            s == null || string.IsNullOrEmpty(word) ? null : TongueBook.Current.Word(s.tongue, word);

        public static string Cap(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);

        public static string Title(string textId)
        {
            var t = ChestData.Text(textId);
            return t != null && !string.IsNullOrEmpty(t.title) ? t.title : "a text";
        }

        /// <summary>Opens the window on a kept thing by hand (the Chest window's button): any speed, any place.</summary>
        public static void ShowByHand(AloudOffer offer)
        {
            if (FrameRoot.Current != null && offer != null)
                Show(FrameRoot.Current, offer, false);
        }

        /// <summary>Opens the window on a text of the chest by hand, where the leaf was left.</summary>
        public static void ShowText(string textId)
        {
            if (FrameRoot.Current != null && ChestData.Text(textId) != null)
                Show(FrameRoot.Current, new AloudOffer { kind = AloudKind.Text, id = textId, seat = "", day = KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0, told = true }, false);
        }

        static void Show(FrameRoot frame, AloudOffer offer, bool self)
        {
            var book = ReaderBook.Current;
            var seat = book.Seat(offer.seat);
            leaf = new ReadingLeaf(offer, seat, self);
            bySelf = self;
            string who = seat != null ? Cap(seat.name) : "";
            var spec = new FloatSpec
            {
                id = FloatId,
                title = offer.kind == AloudKind.Riddle ? "A riddle at the oar" : Title(offer.id),
                who = leaf.WhoLine,
                look = "frame-float-reading",
                content = leaf,
                mustAnswer = false,
                holdsClock = false,                                      // (never: the ruling is that only a card stops the clock)
            };
            spec.shut = () =>
            {
                // (shut by the player before anything was read: it was let lie. The thing itself is still kept)
                var game = KapmanGame.Current;
                if (bySelf && leaf != null && !leaf.Taken && game != null)
                    Aloud.LetLie(game.Day);
                Close(frame);
            };
            frame.OpenFloat(spec);
            offer.told = true;                                           // (the window says it: the log need not say "ready" as well)
            if (self && who.Length > 0)
                FrameLog.Add(offer.kind == AloudKind.Riddle ? $"{who} puts the riddle again." : $"{who} begins {Title(offer.id)}.");
        }

        static void Close(FrameRoot frame)
        {
            frame.CloseFloat(FloatId);
            leaf = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            leaf = null;
            bySelf = false;
            nextLook = 0f;
            wordLineDay = int.MinValue;
            AlsoPressed = null;
        }
    }

    /// <summary>
    /// THE LEAF in the reading window. A text: who gives it and how; its first lines in its own tongue, turning into
    /// English where the two run line for line; then the man's part, a line at a time by the wall clock; then "the
    /// rest is yours" and a button that turns up a page at a press. The place is kept (Aloud.ReadTo), so it is read
    /// on another day from where it was left. A riddle: its words, the tries left, and a box to answer in with a
    /// thing you know, one try a game day.
    /// </summary>
    public sealed class ReadingLeaf : VisualElement
    {
        readonly AloudOffer offer;
        readonly ReaderSeat seat;
        readonly TextDef text;
        readonly ChestItem riddle;
        readonly List<string> english;
        readonly VisualElement lines, foot;
        readonly int said, turned;
        int shownTo;
        float startedAt = -1f, doneAt = -1f;

        /// <summary>The player has pressed something in it: it is theirs now, and does not fold itself.</summary>
        public bool Taken { get; private set; }
        public string WhoLine { get; }

        public ReadingLeaf(AloudOffer offer, ReaderSeat seat, bool bySelf)
        {
            this.offer = offer;
            this.seat = seat;
            AddToClassList("frame-read");
            riddle = offer.kind == AloudKind.Riddle ? FindRiddle(offer.id) : null;
            text = riddle != null ? riddle.Text : ChestData.Text(offer.id);
            var book = ReaderBook.Current;
            english = Plain(book, text);
            var game = KapmanGame.Current;
            var who = text != null && seat != null ? book.Who(text.id, new[] { seat }, game != null ? game.Year : 793, true) : (null, null);
            double share = who.rule != null ? who.rule.share : 1.0 / 3.0;
            string name = seat != null ? FrameReading.Cap(seat.name) : "";
            string tongue = Self.TongueName(Aloud.TongueOf(text));
            WhoLine = seat == null ? tongue
                : riddle != null ? $"{name} puts it. {tongue}."
                : who.rule != null && !who.rule.ByHeart ? $"{name} reads what he can of it. {tongue}."
                : $"{name} has it by heart. {tongue}.";

            // its own words first, turning, where the file lets them be paired without a guess
            var pairs = Aloud.Opening(text, 4);
            // (the lines that turn are not said a second time under it; a plain telling of the man's own begins at its start)
            turned = text != null && book.plain.ContainsKey(text.id) ? 0 : pairs.Count;
            if (pairs.Count > 0)
            {
                var turn = new FloatTurn { tongue = Aloud.TongueOf(text), where = text.title ?? "", english = string.Join(" ", english.GetRange(0, Math.Min(4, english.Count))), skill = Self.TongueLevel(Aloud.TongueOf(text)) };
                turn.pairs.AddRange(pairs);
                Add(new TurningLine(turn));
            }
            lines = new VisualElement();
            lines.AddToClassList("frame-read-lines");
            Add(lines);
            foot = new VisualElement();
            foot.AddToClassList("frame-read-foot");
            Add(foot);

            if (riddle != null)
            {
                said = 0;
                shownTo = english.Count;
                Put(0, english.Count, false);
                if (english.Count == 0 && text != null)
                {
                    // (33 Latin riddles have no free English in texts_793.json: its own words, and it says so)
                    foreach (string l in Aloud.Lines(text.original))
                        lines.Add(Line(l, false));
                    lines.Add(FrameUi.P("There is no English of this one yet: only its own words."));
                }
                RiddleFoot("");
                return;
            }
            int place = Math.Min(Aloud.Place(offer.id), english.Count);
            // (opened by hand, or read before: no waiting on the man. By itself: his part comes line by line)
            said = bySelf && place == 0 && seat != null ? Aloud.SaidLines(english.Count, share) : 0;
            shownTo = said > 0 ? Math.Min(turned, said) : Math.Max(place, Math.Min(Math.Max(turned, FrameReading.Page), english.Count));
            Put(0, Math.Min(place, shownTo), false);
            Put(Math.Min(place, shownTo), shownTo, true);
            if (said == 0)
            {
                Taken = !bySelf;
                TextFoot();
            }
            else if (shownTo >= said)
            {
                // (a stanza short enough to have turned whole: the man has done already)
                doneAt = Time.realtimeSinceStartup;
                TextFoot();
            }
        }

        /// <summary>Adds the English lines from one place to another. The first of them may stand in the turning line
        /// already: those are not said twice.</summary>
        void Put(int from, int to, bool fresh)
        {
            for (int i = Math.Max(from, turned); i < to && i < english.Count; i++)
                lines.Add(Line(english[i], fresh));
        }

        static ChestItem FindRiddle(string def)
        {
            foreach (var item in Chest.Items)
                if (item.kind == "riddle" && item.def == def)
                    return item;
            return null;
        }

        /// <summary>The English it is read in: the plain opening of the readers' file where there is one, then the
        /// translation from the line after it.</summary>
        static List<string> Plain(ReaderBook book, TextDef t)
        {
            var list = Aloud.English(t);
            if (t != null && book.plain.TryGetValue(t.id, out string[] plain) && plain.Length > 0)
            {
                var both = new List<string>(plain);
                int skip = Math.Min(list.Count, Aloud.SaidLines(list.Count, 1.0 / 3.0));
                both.AddRange(list.GetRange(skip, list.Count - skip));
                return both;
            }
            return list;
        }

        static Label Line(string words, bool fresh)
        {
            var l = new Label(words);
            l.AddToClassList("frame-read-line");
            l.EnableInClassList("fresh", fresh);
            return l;
        }

        /// <summary>Each frame: the man's part comes a line at a time by the wall clock.</summary>
        public void Step(float real)
        {
            if (said == 0 || shownTo >= said)
                return;
            if (startedAt < 0f)
                startedAt = real;
            int due = Math.Min(said, turned + 1 + (int)((real - startedAt) / FrameReading.LineSeconds));
            if (due <= shownTo)
                return;
            Put(shownTo, due, true);
            shownTo = due;
            if (shownTo >= said)
            {
                doneAt = real;
                TextFoot();
            }
        }

        /// <summary>The man has done and nothing was pressed for a while: the window may fold itself.</summary>
        public bool Lingered(float real) => !Taken && doneAt >= 0f && real - doneAt > FrameReading.LingerSeconds;

        void TextFoot()
        {
            foot.Clear();
            int total = english.Count;
            if (shownTo >= total)
            {
                foot.Add(FrameUi.P(Aloud.Finished(offer.id) ? "You have it whole." : "That is the end of it."));
                if (!Aloud.Finished(offer.id))
                    foot.Add(FrameUi.Row(FrameUi.Go("Keep it", ReadOn)));
                return;
            }
            foot.Add(FrameUi.P(said > 0 && shownTo <= said
                ? $"The rest is yours. {total - shownTo} lines more."
                : $"{shownTo} of {total} lines."));
            foot.Add(FrameUi.Row(FrameUi.Go("Read on", ReadOn), FrameUi.Btn("Let it lie", LetLie, "It stays in the chest, at this line.")));
        }

        void ReadOn()
        {
            Taken = true;
            var game = KapmanGame.Current;
            int total = english.Count;
            int to = Math.Min(total, shownTo + FrameReading.Page);
            Put(shownTo, to, true);
            shownTo = to;
            Aloud.ReadTo(offer.id, shownTo, total, game != null ? game.Day : 0.0);
            TextFoot();
        }

        void LetLie()
        {
            // (the cross does the same: the float's own shut)
            var frame = FrameRoot.Current;
            var game = KapmanGame.Current;
            if (!Taken && game != null)
                Aloud.LetLie(game.Day);
            Taken = true;                                                // (so the frame's shut does not count it twice)
            frame?.CloseFloat(FrameReading.FloatId);
        }

        // ------------------------------------------------------------------ a riddle

        void RiddleFoot(string fact)
        {
            foot.Clear();
            var game = KapmanGame.Current;
            double day = game != null ? game.Day : 0.0;
            if (riddle == null)
                return;
            if (fact.Length > 0)
                foot.Add(FrameUi.Text(fact));
            if (riddle.solved)
            {
                foot.Add(FrameUi.P("Answered."));
                return;
            }
            if (riddle.resting)
            {
                foot.Add(FrameUi.P("It rests until you learn something new that bears on it."));
                return;
            }
            foot.Add(FrameUi.P($"{riddle.tries} {(riddle.tries == 1 ? "try" : "tries")} left. One try a day at the oar."));
            if (riddle.Riddle.SlotCount > 1)
            {
                foot.Add(FrameUi.P("It wants more than one answer: open the chest for it (I)."));
                return;
            }
            if (!Aloud.CanTry(day))
            {
                foot.Add(FrameUi.P("You have tried today. Sleep on it."));
                return;
            }
            var box = new TextField { value = "" };
            box.AddToClassList("frame-read-answer");
            var picks = new VisualElement();
            picks.AddToClassList("frame-read-picks");
            box.RegisterValueChangedCallback(e => Picks(picks, e.newValue));
            foot.Add(FrameUi.P("Answer with a thing you know: write the first letters of it."));
            foot.Add(box);
            foot.Add(picks);
        }

        void Picks(VisualElement picks, string typed)
        {
            picks.Clear();
            typed = (typed ?? "").Trim();
            if (typed.Length < 2)
                return;
            int n = 0;
            foreach (var thing in Chest.KnownThings())
            {
                if (thing.name.IndexOf(typed, StringComparison.OrdinalIgnoreCase) < 0 || riddle.tried.Contains(thing.id))
                    continue;
                string id = thing.id;
                picks.Add(FrameUi.Btn(thing.name, () => Answer(id)));
                if (++n >= 8)
                    break;
            }
        }

        void Answer(string thingId)
        {
            Taken = true;
            var game = KapmanGame.Current;
            RiddleFoot(Aloud.Try(riddle, thingId, game != null ? game.Day : 0.0));
        }
    }
}
```

### 7.4 Edits to four files that exist

As unified diffs against the files as they stood on 7 October at about 12:40 (the lines round each change are the
diff's own context). If a builder has changed one of these since, place the change by its context, not its number.

`Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs` (one line, in `Update`):

```diff
--- a/Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs
+++ b/Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs
@@ -375,6 +375,7 @@
             SelfUpdate();
             EventsUpdate();
             FloatsUpdate();
+            FrameReading.Tick(this);                                     // (the reading at sea: Design/fixes/reading_at_sea.md)
         }
 
         /// <summary>The frame's edge and accent follow the world's palette (the player's people).</summary>
```

`Assets/Kapman/Code/Game/UI/Frame/Panels/ChestFramePanel.cs`:

```diff
--- a/Assets/Kapman/Code/Game/UI/Frame/Panels/ChestFramePanel.cs
+++ b/Assets/Kapman/Code/Game/UI/Frame/Panels/ChestFramePanel.cs
@@ -1,3 +1,4 @@
+using System.Collections.Generic;
 using UnityEngine.UIElements;
 
 namespace Kapman.Game.Frame
@@ -13,11 +14,12 @@
         public override string Title => "Chest";
         public override int Order => 94;
 
-        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(17, Chest.Version), Chest.Items.Count), Self.Version);
+        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(17, Chest.Version), Chest.Items.Count), Self.Version), Aloud.Version);
 
         protected override void Fill(VisualElement body)
         {
             int shown = 0;
+            ReadAboard(body);
             var past = Self.Chosen ? SelfBook.Current.Past(Self.Past) : null;
             if (past != null && past.chestThing.Length > 0)
             {
@@ -30,7 +32,16 @@
                 string title = text != null && !string.IsNullOrEmpty(text.title) ? text.title : item.kind == "riddle" ? "A riddle" : "A " + item.kind;
                 string from = string.IsNullOrEmpty(item.from) ? "" : "From " + item.from + ".";
                 string tag = item.kind == "riddle" ? (item.solved ? "solved" : item.resting ? "resting" : "open") : item.kind;
-                body.Add(FrameUi.Ln(title, from.Length > 0 ? from : null, FrameUi.Tag(tag)));
+                if (item.kind == "text" && text != null && !string.IsNullOrEmpty(text.translation))
+                {
+                    // (a text is read here, in the reading window, from the line where it was left)
+                    int at = Aloud.Place(item.def);
+                    tag = Aloud.Finished(item.def) ? "read" : at > 0 ? "line " + at : "unread";
+                    string def = item.def;
+                    body.Add(FrameUi.LnButton(title, from.Length > 0 ? from : null, () => FrameReading.ShowText(def), FrameUi.Tag(tag)));
+                }
+                else
+                    body.Add(FrameUi.Ln(title, from.Length > 0 ? from : null, FrameUi.Tag(tag)));
                 shown++;
             }
             if (Chest.Notes.Count > 0)
@@ -40,7 +51,59 @@
             }
             if (shown == 0)
                 body.Add(FrameUi.P("The chest is empty."));
-            body.Add(FrameUi.P("To read a text or try a riddle, open the chest: press I."));
+            body.Add(FrameUi.P("Click a text to read it here. For the runes, your notes and a riddle of more than one answer, open the chest: press I."));
+        }
+        /// <summary>What the window's title may carry as a lamp (the brief faces' builder shows it): what is kept
+        /// for a quiet hour, or null.</summary>
+        public static string LampWords => Aloud.Kept.Count == 0 ? null : Aloud.Kept.Count == 1 ? "One thing kept for a quiet hour." : Aloud.Kept.Count + " things kept for a quiet hour.";
+
+        /// <summary>The reading at sea (Design/fixes/reading_at_sea.md): what the men have ready, what you may ask
+        /// them for, and whether they begin by themselves.</summary>
+        void ReadAboard(VisualElement body)
+        {
+            if (!Voyage.Started || !Aloud.NowOf(out var now))
+                return;
+            var book = ReaderBook.Current;
+            var aboard = book.Aboard(now.men, now.heathen);
+            body.Add(FrameUi.H("Read aboard"));
+            var ready = Aloud.KeptOf(AloudKind.Text);
+            if (ready != null)
+            {
+                string who = FrameReading.Cap(book.Seat(ready.seat)?.name ?? "one of the men");
+                body.Add(FrameUi.LnButton(FrameReading.Title(ready.id), who + " has it ready.", () => FrameReading.ShowByHand(ready), FrameUi.Tag("ready")));
+            }
+            var put = Aloud.KeptOf(AloudKind.Riddle);
+            if (put != null)
+            {
+                string who = FrameReading.Cap(book.Seat(put.seat)?.name ?? "one of the men");
+                body.Add(FrameUi.LnButton("A riddle at the oar", who + " would put it again. " + (Aloud.CanTry(now.day) ? "A try is yours today." : "You have tried today."),
+                    () => FrameReading.ShowByHand(put), FrameUi.Tag("open")));
+            }
+            if (ready == null && put == null)
+                body.Add(FrameUi.P(now.atSea ? "Nothing is ready yet. A man begins something on a quiet afternoon at sea." : "In port the men are ashore. They read at sea."));
+
+            // what to ask for next: the sets a man aboard has by heart
+            var asks = new List<VisualElement>();
+            var seen = new HashSet<string>();
+            foreach (var s in aboard)
+                foreach (var r in book.RowsOf(s, now.year))
+                    if (r.ByHeart && r.texts.Length > 0 && seen.Add(r.id))
+                    {
+                        string set = r.texts[0];
+                        string name = r.title.Length > 0 ? r.title : r.id;
+                        asks.Add(FrameUi.Btn(Aloud.InHand == set ? name + " (asked for)" : name, () => { Aloud.Put(set); Redraw(); }, FrameReading.Cap(s.name) + " has it by heart."));
+                    }
+            if (asks.Count > 0)
+            {
+                asks.Add(FrameUi.Btn(Aloud.InHand.Length == 0 ? "Their choice (now)" : "Their choice", () => { Aloud.Put(""); Redraw(); }));
+                body.Add(FrameUi.P("Ask for:"));
+                body.Add(FrameUi.Row(asks.ToArray()));
+            }
+            body.Add(FrameUi.Row(FrameUi.Btn(Aloud.MenBegin ? "The men begin by themselves: yes" : "The men begin by themselves: no",
+                () => { Aloud.MenBegin = !Aloud.MenBegin; Redraw(); }, "No: nothing opens by itself. What they have ready waits here.")));
+            if (Aloud.MenBegin && Aloud.HoldingBack)
+                body.Add(FrameUi.P("You let it lie twice, so they wait to be asked. Read anything and they begin again."));
+            body.Add(FrameUi.H("In the chest"));
         }
     }
 }
```

`Assets/Kapman/Code/Game/UI/Frame/Panels/CrewPanel.cs`:

```diff
--- a/Assets/Kapman/Code/Game/UI/Frame/Panels/CrewPanel.cs
+++ b/Assets/Kapman/Code/Game/UI/Frame/Panels/CrewPanel.cs
@@ -19,6 +19,7 @@
             h = FrameUi.Mix(h, (int)(Voyage.Health * 20));
             h = FrameUi.Mix(h, (int)(Voyage.Morale * 20));
             h = FrameUi.Mix(h, (int)Voyage.PayOwed);
+            h = FrameUi.Mix(h, Aloud.Version);
             return FrameUi.Mix(h, Voyage.DaysAtSea);
         }
 
@@ -36,7 +37,25 @@
             body.Add(FrameUi.Ln("At sea", null, FrameUi.Num(Voyage.DaysAtSea <= 0 ? "not yet" : Voyage.DaysAtSea == 1 ? "1 day" : Voyage.DaysAtSea + " days")));
             body.Add(FrameUi.Ln("Bread and other food", null, FrameUi.Num(Voyage.FoodDays < 0.25 ? "none" : FrameUi.Days(Voyage.FoodDays))));
             body.Add(FrameUi.Ln("Ale and water", null, FrameUi.Num(Voyage.DrinkDays < 0.25 ? "none" : FrameUi.Days(Voyage.DrinkDays))));
-            body.Add(FrameUi.P("The crew are not yet people with names, tongues and faiths of their own: that comes with hiring."));
+            // what each seat has by heart or can read (story/readers_793.json; Design/fixes/reading_at_sea.md)
+            if (Aloud.NowOf(out var now))
+            {
+                var book = ReaderBook.Current;
+                body.Add(FrameUi.H("What they have"));
+                foreach (var s in book.Aboard(now.men, now.heathen))
+                {
+                    var titles = new System.Collections.Generic.List<string>();
+                    foreach (var r in book.RowsOf(s, now.year))
+                        titles.Add((r.ByHeart ? "by heart: " : "reads: ") + (r.title.Length > 0 ? r.title : r.id));
+                    string own = Self.TongueName(s.tongue) + (s.letters ? ", and letters" : "") + (s.runes ? ", and runes" : "");
+                    string word = Aloud.WordOf(s.id);
+                    var line = FrameUi.Ln(FrameReading.Cap(s.name), own + ". " + (titles.Count > 0 ? FrameReading.Cap(string.Join("; ", titles)) + "." : "No song that he owns to."));
+                    if (word.Length > 0)
+                        line.Add(FrameUi.Tag(word));
+                    body.Add(line);
+                }
+            }
+            body.Add(FrameUi.P("They are seats, not yet people with names of their own: that comes with hiring."));
         }
     }
 }
```

`Assets/Kapman/Code/Game/UI/Frame/Panels/TonguesPanel.cs`:

```diff
--- a/Assets/Kapman/Code/Game/UI/Frame/Panels/TonguesPanel.cs
+++ b/Assets/Kapman/Code/Game/UI/Frame/Panels/TonguesPanel.cs
@@ -17,7 +17,7 @@
         /// <summary>The tongues the panel looks at beside the ones held (those a tongue held may be kin to).</summary>
         static readonly string[] Listed = { "old_frisian", "old_english", "old_saxon", "old_frankish", "old_norse", "latin", "romance", "irish", "slavic", "baltic", "finnic" };
 
-        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(17, Self.Version), Chest.KnowsLatin);
+        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(17, Self.Version), Chest.KnowsLatin), Aloud.Version);
 
         /// <summary>How well, as a word: "your own", "well", "half known", "a few words".</summary>
         public static string How(double level, bool held) =>
@@ -61,6 +61,22 @@
             if (learned.Count == 0 && heard == 0)
                 body.Add(FrameUi.P("No words of other tongues met yet."));
             body.Add(FrameUi.P($"A word heard {Self.StaysAfter} times stays English from the start."));
+            // the word each man is naming these days at sea (Design/fixes/reading_at_sea.md)
+            if (Voyage.Started && Aloud.NowOf(out var now))
+            {
+                bool head = false;
+                foreach (var s in ReaderBook.Current.Aboard(now.men, now.heathen))
+                {
+                    var w = TongueBook.Current.Word(s.tongue, Aloud.WordOf(s.id));
+                    if (w == null)
+                        continue;
+                    if (!head)
+                        body.Add(FrameUi.H("From the men"));
+                    head = true;
+                    body.Add(FrameUi.Ln(w.word, FrameReading.Cap(s.name) + ": " + w.means + (w.say.Length > 0 ? " (say: " + w.say + ")" : "") + ". One hearing a day at sea.",
+                        FrameUi.Num(Self.Seen(w.word) + " of " + Self.StaysAfter)));
+                }
+            }
         }
     }
 }
```

### 7.5 The look: `Assets/Kapman/Game/UI/Frame.uss`

After the rule `.frame-root .frame-turn-note` (the turning line's block, about line 1111). Sizes are from the four
of `Design/text_size_audit.md` (18, 22, 26, 30 at 1080p). Colours are left to the float's own rules. **Not seen on
screen**: the width is reckoned for a column of about 70 letters at 22 px, which is the critic's fifth change; the
window's place is the float's own (the middle of the chart), and whether it should stand lower or to one side wants
a look at a capture. `FrameRoot.Build` sets `left` and `top` in code, so a place of its own needs a line there.

```css
/* the reading at sea: a column a reader can hold, about 70 letters */
.frame-root .frame-float.frame-float-reading {
    width: 36%;
    max-width: 780px;
    max-height: 66%;
}

.frame-root .frame-read-lines {
    margin-top: 8px;
}

.frame-root .frame-read-line {
    font-size: 22px;
    white-space: normal;
    margin-bottom: 3px;
}

.frame-root .frame-read-foot {
    margin-top: 12px;
}

.frame-root .frame-read-answer {
    font-size: 22px;
    margin-top: 6px;
}

.frame-root .frame-read-picks {
    flex-direction: row;
    flex-wrap: wrap;
}
```

### 7.6 Tests: `Assets/Kapman/Tests/EditMode/AloudTests.cs`

Ten tests of the rules, with no scene. The one that matters most for ruling 26 is
`WhatIsDue_IsTheSameAtSpeedOneAndSpeedFive`: twelve game days are run twice, once asked as the frame asks at speed 1
and once as at speed 5, and what is saved, what is in the chest and what has been heard must be equal to the
letter. It compiles; it has not been run. `ReaderBook.Current` reads the data folder, so the tests use
`ReaderBook.OwnTable()` and do not change when the data file arrives. `Hear` reads the real `tongues_793.json`.

```csharp
using System.Collections.Generic;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The reading at sea (Design/fixes/reading_at_sea.md): what is due goes by the game's hours and is the
    /// same at every speed; what opens by itself opens only in a quiet stretch at a slow speed, once, and stops when
    /// it is let lie twice; the next part comes only when the last was taken up; a riddle has one try a game day.</summary>
    public class AloudTests
    {
        const string Texts = @"{ ""texts"": [
            { ""id"": ""beowulf_01"", ""title"": ""The song, part one"", ""form"": ""book"", ""language"": ""Old English"", ""script"": ""latin"",
              ""original"": ""a\nb\nc"", ""translation"": ""one\ntwo\nthree\nfour\nfive\nsix\nseven\neight\nnine"", ""translator"": ""X"", ""credit"": ""test"" },
            { ""id"": ""beowulf_02"", ""title"": ""The song, part two"", ""form"": ""book"", ""language"": ""Old English"", ""script"": ""latin"",
              ""original"": ""a\nb"", ""translation"": ""ten\neleven\ntwelve"", ""translator"": ""X"", ""credit"": ""test"" },
            { ""id"": ""t_fog"", ""title"": ""A Norse riddle"", ""form"": ""rune_stick"", ""language"": ""Old Norse"", ""script"": ""latin"",
              ""original"": ""Hvat er þat\nundra"", ""translation"": ""What is that\nwonder"", ""translator"": ""X"", ""credit"": ""test"" },
            { ""id"": ""aldhelm_test"", ""title"": ""A Latin leaf"", ""form"": ""parchment"", ""language"": ""Latin"", ""script"": ""latin"",
              ""original"": ""Quid est"", ""translation"": ""What is it"", ""translator"": ""Y"", ""credit"": ""test"" } ] }";

        const string Riddles = @"{ ""palette"": [
              { ""id"": ""fog"", ""kind"": ""weather"", ""name"": ""fog"", ""known"": ""start"" },
              { ""id"": ""anchor"", ""kind"": ""thing"", ""name"": ""an anchor"", ""known"": ""start"" },
              { ""id"": ""swan"", ""kind"": ""animal"", ""name"": ""a swan"", ""known"": ""start"" } ],
            ""riddles"": [ { ""id"": ""r_fog"", ""text"": ""t_fog"", ""answers"": [""fog""], ""difficulty"": 2, ""tries"": 3 } ] }";

        ReaderBook book;

        [SetUp]
        public void Load()
        {
            ChestData.Load(new TextAsset(Texts), new TextAsset(Riddles), null);
            Chest.Clear();
            Self.Reset();
            Aloud.Clear();
            book = ReaderBook.OwnTable();
        }

        static AloudNow At(double day, float pace = 1f, float real = 0f, bool atSea = true, bool pressed = false, double storm = 0.0) =>
            new AloudNow { day = day, hour = (day - System.Math.Floor(day)) * 24.0, year = 793, pace = pace, real = real, atSea = atSea, pressed = pressed, storm = storm, men = 4, heathen = true };

        [Test]
        public void Quiet_IsASlowSpeedAtSeaWithNothingPressing()
        {
            Assert.IsTrue(Aloud.Quiet(At(1.5, 1f)), "speed 1");
            Assert.IsTrue(Aloud.Quiet(At(1.5, 4f)), "speed 2");
            Assert.IsFalse(Aloud.Quiet(At(1.5, 24f)), "speed 3");
            Assert.IsFalse(Aloud.Quiet(At(1.5, 96f)), "speed 4");
            Assert.IsFalse(Aloud.Quiet(At(1.5, 360f)), "speed 5");
            Assert.IsFalse(Aloud.Quiet(At(1.5, 0f)), "paused");
            Assert.IsFalse(Aloud.Quiet(At(1.5, 1f, pressed: true)), "a card is up");
            Assert.IsFalse(Aloud.Quiet(At(1.5, 1f, atSea: false)), "in port");
            Assert.IsFalse(Aloud.Quiet(At(1.5, 1f, storm: 0.5)), "a storm");
        }

        [Test]
        public void Step_WaitsOutTheDwell_OpensOnceAStretch_AndKeepsTheGap()
        {
            Aloud.Keep(AloudKind.Text, "beowulf_01", "lookout", 1.5);
            Assert.IsNull(Aloud.Step(At(1.5, 1f, 100f)), "the stretch has only begun");
            Assert.IsNull(Aloud.Step(At(1.5, 1f, 100f + Aloud.DwellSeconds - 1f)));
            Assert.IsNotNull(Aloud.Step(At(1.5, 1f, 100f + Aloud.DwellSeconds + 1f)), "after the dwell it opens");
            Assert.IsNull(Aloud.Step(At(1.5, 1f, 200f)), "one a stretch");
            Assert.IsNull(Aloud.Step(At(1.5, 1f, 201f, pressed: true)), "the stretch ends");
            Assert.IsNull(Aloud.Step(At(1.5, 1f, 202f)));
            Assert.IsNull(Aloud.Step(At(1.5, 1f, 260f)), "a new stretch, but inside the gap");
            float later = 100f + Aloud.DwellSeconds + 1f + Aloud.GapSeconds + 1f;
            Assert.IsNotNull(Aloud.Step(At(1.6, 1f, later)), "after the gap");
        }

        [Test]
        public void Step_OpensNothingAtAFastSpeed_AndTheThingIsKept()
        {
            Aloud.Keep(AloudKind.Text, "beowulf_01", "lookout", 1.5);
            for (int i = 0; i < 200; i++)
                Assert.IsNull(Aloud.Step(At(1.5 + i, 360f, i * 4f)));
            Assert.IsNotNull(Aloud.KeptOf(AloudKind.Text), "kept for the next quiet stretch");
        }

        [Test]
        public void LetLieTwice_TheMenStop_ReadingStartsThemAgain()
        {
            Aloud.Keep(AloudKind.Text, "beowulf_01", "lookout", 1.5);
            Aloud.LetLie(1.5);
            Assert.IsNull(Aloud.Step(At(2.0, 1f, 0f)));
            Assert.IsNull(Aloud.Step(At(2.0, 1f, 1000f)), "they wait two game days after the first");
            Aloud.LetLie(4.0);
            Assert.IsTrue(Aloud.HoldingBack);
            Assert.IsNull(Aloud.Step(At(30.0, 1f, 5000f)), "they stop until asked");
            Assert.IsNotNull(Aloud.KeptOf(AloudKind.Text), "the thing itself is still kept");
            Aloud.ReadTo("beowulf_01", 3, 9, 30.0);
            Assert.IsFalse(Aloud.HoldingBack);
        }

        [Test]
        public void TheNextPartComesOnlyWhenTheLastWasTakenUp()
        {
            Aloud.Hour(At(0.4), book);
            Assert.IsFalse(Chest.Has("beowulf_01"), "not before noon");
            Aloud.Hour(At(0.6), book);
            Assert.IsTrue(Chest.Has("beowulf_01"));
            Assert.AreEqual("beowulf_01", Aloud.KeptOf(AloudKind.Text).id);
            Assert.AreEqual("lookout", Aloud.KeptOf(AloudKind.Text).seat);
            for (int d = 1; d < 40; d++)
                Aloud.Hour(At(d + 0.6), book);
            Assert.IsFalse(Chest.Has("beowulf_02"), "forty days at any speed give no second part unread");
            Aloud.ReadTo("beowulf_01", 9, 9, 40.7);
            Assert.IsTrue(Aloud.Finished("beowulf_01"));
            Aloud.Hour(At(41.6), book);
            Assert.IsTrue(Chest.Has("beowulf_02"));
        }

        [Test]
        public void WhatIsDue_IsTheSameAtSpeedOneAndSpeedFive()
        {
            string Run(float pace)
            {
                ChestData.Load(new TextAsset(Texts), new TextAsset(Riddles), null);
                Chest.Clear();
                Self.Reset();
                Aloud.Clear();
                Chest.Give("riddle", "r_fog", "a skipper", null, true);
                float real = 0f;
                for (int h = 0; h < 24 * 12; h++)
                {
                    Aloud.Hour(At(h / 24.0, pace, real), book);
                    // (the frame asks twice a real second: many times an hour at speed 1, less than once at speed 5)
                    int asks = pace < 5f ? 120 : 1;
                    for (int i = 0; i < asks; i++)
                    {
                        real += 0.5f;
                        Aloud.Step(At(h / 24.0, pace, real));
                    }
                }
                return Json.Write(Aloud.Capture()) + "|" + Chest.Items.Count + "|" + Self.SeenCount;
            }
            Assert.AreEqual(Run(1f), Run(360f));
        }

        [Test]
        public void WhoKnowsWhat_GoesByPeopleFaithAndLetters()
        {
            var four = book.Aboard(4, true);
            Assert.AreEqual("lookout", book.Who("beowulf_01", four, 793, false).seat.id, "the Englishman has the song by heart");
            Assert.AreEqual("dane", book.Who("t-gest-05", four, 793, true).seat.id, "the Dane reads the staves");
            Assert.IsNull(book.Who("t-gest-05", four, 793, false).seat, "a stave that is not aboard cannot be read");
            Assert.AreEqual("lookout", book.Who("aldhelm_test", four, 793, true).seat.id, "the schooled man reads a Latin leaf");
            var noHeathen = book.Aboard(4, false);
            Assert.IsNull(book.Who("havamal_001", noHeathen, 820, false).seat, "no Dane aboard, no sayings");
            Assert.IsNull(book.Who("maldon_991", four, 793, true).seat, "not before its year");
            Assert.AreEqual(3, book.Aboard(3, true).Count, "three men fill three seats");
        }

        [Test]
        public void ARiddleAtTheOar_HasOneTryAGameDay()
        {
            var item = Chest.Give("riddle", "r_fog", "a skipper", null, true);
            Aloud.Hour(At(0.2), book);
            Assert.AreEqual("r_fog", Aloud.KeptOf(AloudKind.Riddle).id);
            Assert.AreEqual("dane", Aloud.KeptOf(AloudKind.Riddle).seat, "it is Norse: the Dane puts it");
            StringAssert.StartsWith("Not", Aloud.Try(item, "swan", 0.3));
            Assert.AreEqual(2, item.tries);
            StringAssert.StartsWith("One try a day", Aloud.Try(item, "fog", 0.9));
            Assert.IsFalse(item.solved);
            Aloud.Try(item, "fog", 1.1);
            Assert.IsTrue(item.solved);
            Assert.IsNull(Aloud.KeptOf(AloudKind.Riddle));
        }

        [Test]
        public void Lines_AndTheMansPart()
        {
            Assert.AreEqual(3, Aloud.Lines("a\n\n b \nc").Count);
            string prose = new string('x', 120) + ". " + new string('y', 120) + ". " + new string('z', 60) + ".";
            Assert.AreEqual(3, Aloud.Lines(prose).Count, "long prose goes by sentences");
            Assert.AreEqual(3, Aloud.SaidLines(9, 1.0 / 3.0));
            Assert.AreEqual(Aloud.SaidMax, Aloud.SaidLines(98, 1.0 / 3.0), "never more than the cap");
            Assert.AreEqual(1, Aloud.SaidLines(2, 0.25));
            Assert.AreEqual(2, Aloud.Opening(ChestData.Text("t_fog"), 4).Count, "line for line: it can turn");
            Assert.AreEqual(0, Aloud.Opening(ChestData.Text("beowulf_01"), 4).Count, "not line for line: nothing is paired by guess");
        }

        [Test]
        public void SavesAndLoads()
        {
            Aloud.Hour(At(0.6), book);
            Aloud.ReadTo("beowulf_01", 4, 9, 0.7);
            Aloud.MenBegin = false;
            Aloud.Put("beowulf_*");
            object saved = Json.Parse(Json.Write(Aloud.Capture()));
            Aloud.Clear();
            Aloud.Restore(saved);
            Assert.AreEqual(4, Aloud.Place("beowulf_01"));
            Assert.IsTrue(Aloud.TakenUp("beowulf_01"));
            Assert.IsFalse(Aloud.MenBegin);
            Assert.AreEqual("beowulf_*", Aloud.InHand);
            Aloud.Restore(null);
            Assert.IsTrue(Aloud.MenBegin, "a save from before the reading: the plain start");
        }
    }
}
```

---

## 8. To build, in order

Each step compiles and can be committed alone. One Unity at a time, as the queue says.

| Step | What | Touches | Check |
|---|---|---|---|
| 1 | `ReaderBook.cs`, `Aloud.cs`, `AloudTests.cs` | three new files | the ten tests |
| 2 | `FrameReading.cs`, the line in `FrameRoot.Update`, the six rules in `Frame.uss` | one new file, two edits | `ChartGameTests`: add a picture of the window (keep an offer with `Aloud.Keep`, open it with `FrameReading.ShowByHand`) and look at it |
| 3 | The three panels | three edits | pictures of Chest, Crew, Tongues at sea |
| 4 | `story/readers_793.json` into `Game/Data/story` | data | `ContentReportTests` sees it; the Crew window names eight rows, not five |
| 5 | Play one crossing at speed 1 and one at speed 2 from cast off to the mooring; then the same at the fastest speed there is | | The window opens once or twice, never over a card; at the fast speed one log line and no window |

**Who else is in these files.** The brief faces (`fixes/brief_faces.md`) drafts a `Brief` for Crew and an
`OpenFull` for the Chest in the same two panel files: the reading's sections are in `Fill` only, so the two fit, but
one hand should merge them. The builder of the clock (queue part 2, ruling 26) owns `GameSpeed`: this needs two
things of that builder, both one line. `FrameReading.Pace()` must give game minutes a real second if the fast speeds
are not the time scale; and where the clock drops by itself, set `FrameReading.AlsoPressed` so nothing opens in that
moment. The cards' content pass wrote an outcome line for each choice of the 43 song cards: with the cards shut those
lines are not shown, and the first line of each (`crew.song.NN.lead`, the lookout leading in) is the natural thing
to put in `plain` as the first line of his part.

**To take it out again:** remove the line in `FrameRoot.Update` and nothing opens; drop the flag hold in
`Aloud.OnHour` and the song cards come back. The chest, the save and the cards are otherwise as they were.

---

## 9. Yours to rule

1. **The words.** The man's plain opening for each of the 43 parts of the song and the three laments (4.3, gap 1).
   Until they are written he says the verse of 1892.
2. **Who has what** (4.2): the two rows marked (?). Should a christened Dane have the questions of the gods? Should
   the lookout read English leaves as well as Latin?
3. **Should the song cards go for good?** Here they are shut by a flag and can come back. If the reading is kept,
   the 43 cards and their written outcomes can be dropped from the first release.
4. **Lore.** A long text read whole counts as one use of Lore, four short ones as one. Ruling 18 (events move the
   stats) may want a different measure.
5. **The numbers of section 5**: 20 seconds, 8 minutes, one a day, twice let lie. They are first guesses for
   speeds nobody has played yet.
6. **A night at anchor** counts as a quiet stretch here. If a tale should belong to the fire only, or to the open
   sea only, it is one clause in `Aloud.NowOf`.

---

## 10. Not checked

- **Nothing was run.** The code compiles outside Unity; no test was executed, no window was seen, and the
  stylesheet rules are unseen guesses.
- **The five-speed clock is not in the code yet** (`GameSpeed.Speeds` is `{ 1, 2, 4 }`). The paces in section 1 are
  ruling 26's words turned into numbers; the builder's real numbers may differ, and `QuietPace` should then sit
  between his speed 2 and his speed 3.
- **`KapmanGame.HourPassed` at high speed.** I read that it fires through a tick ladder (lines 88, 130, 520) and
  took it that every hour is fired when many pass in one frame. I did not read the ladder itself. If it skips hours
  at speed 5, `Aloud.Hour` still does its day's work on the first hour it gets past noon, and the day is the same;
  the hour written on the chest item may differ.
- **`FrameGame.PortHere`** is what "at sea" rests on. It reads the text panel; with no frame up (a headless run) I
  did not check what it gives.
- **`Self.Reset` and the tongues** in `AloudTests`: the test of the two speeds hears real words of the tongues file
  for a self that is the plain start. Both runs do the same, so the test holds either way, but I did not check
  which tongues the plain start has.
- **The scroll.** Lines added to the leaf after the window is built should grow the float's scroll view; I did not
  see it do so, nor whether it follows to the newest line.
- **The text field in the riddle's foot** relies on `UiTyping.Watch(root)` to keep the game's keys quiet while the
  player writes. Read, not tried.
- **`not_before` years** other than Maldon's are from memory (4.2, the notes in the file).
- **The crossing of section 6** is the rules worked by hand, not a play.
- **The content pass on the cards** (six agents, today) may be changing `crew_793.json` now. The count of 43 song
  cards and their `when` clause are as read at about 12:20.
- I ran one read-only `git log -1` by mistake while collecting line numbers. Nothing was changed by it.
