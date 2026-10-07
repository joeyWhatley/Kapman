# Steam store page: the pictures and words it needs

Written 7 Oct 2026 beside `Design/steam_release.md`. Like that file, this is from what I know of Steam's rules, not
from a fresh reading of Steamworks (no web was used). **Every size and limit here is marked (check): read it again on
partner.steamgames.com before anything is drawn to it.** Nothing here is in `Art/art_list.csv` yet: these are proposed
slots, for you to take up or change.

What every picture has to say: this is a game of words, run from a thick framed HUD over a 2D chart with a purple sea,
with a small view of a ship. Not a 3D sailing game. A capsule that promises open-sea action would sell the wrong game.

## 1. The pictures, as proposed art slots (kind "store")

The id is proposed in the manner of the art list's file names (`store/<id>.png`).

### Store capsules (needed before the page can go up)

| Id | Size, px (check) | Where Steam shows it | What it must show |
|---|---|---|---|
| `store.capsule.header` | 920 x 450 (check; the older size was 460 x 215) | top of the store page, lists, wishlist mails | The name "Kapman" large and readable, on the purple-sea chart. A corner of the thick frame, a small ship. The name must be readable when the picture is shrunk to a thumb's width. |
| `store.capsule.small` | 462 x 174 (check) | search results, lists | Almost only the name. The chart's purple behind it, perhaps the ship as a small mark. No small detail survives at this size. |
| `store.capsule.main` | 1232 x 706 (check) | the front page's large carousel, if the game is ever shown there | The widest picture: the chart with its coasts and the purple sea, the ship, the frame's edge, the name. |
| `store.capsule.vertical` | 748 x 896 (check) | seasonal sale pages, tall lists | The name at the top or middle; below it the chart running down a coast with the ship on it. |
| `store.page.background` | 1438 x 810 (check; optional) | faint, behind the store page | The chart alone, no words, no frame. Steam darkens and tints it, so plain shapes only. |

### Library pictures (needed before release; they are what an owner sees)

| Id | Size, px (check) | Where | What it must show |
|---|---|---|---|
| `store.library.capsule` | 600 x 900 (check) | the owner's library grid | Tall. The name, the ship, the purple sea of the chart. |
| `store.library.header` | 920 x 430 (check) | library, recent games | As the header capsule, with the name. |
| `store.library.hero` | 3840 x 1240 (check) | the wide band at the top of the game's library page | The chart, wide, **no name and no words** (the logo is laid over it). Keep the middle calm: the logo sits there or at the left. |
| `store.library.logo` | 1280 x 720, see-through PNG (check) | laid over the hero | The name alone, on nothing. The long mark of "Kāpman" may stay here (steam_release.md: the mark can stay in the title art). |

### Icons

| Id | Size (check) | Where | What it must show |
|---|---|---|---|
| `store.icon.community` | 184 x 184 JPG (check) | community hub, friends list | One sign: the ship, or a K, on purple. |
| `store.icon.client` | 32 x 32, ICO or TGA (check) | the library's list | The same sign, cut down to what 32 pixels hold. |
| `store.icon.shortcut` | a Windows .ico, up to 256 x 256 (check) | the desktop shortcut | The same sign. |

### Moving pictures and later ones

| Id | Size (check) | What it must show |
|---|---|---|
| `store.trailer.main` | 1920 x 1080, MP4, a high bitrate (check); not required | Real play only: the frame, a course set, a cast off, a card answered. Under about a minute (check). The first seconds must show the game itself, not a title. |
| `store.trailer.thumb` | Steam makes one from the film; an own one can be given (check the size) | A screenshot with the frame on it. |
| `store.event.cover` | 800 x 450 (check) | For news posts once the page is public. The chart and the name. |
| `store.event.header` | 1920 x 622 (check) | The wide band above a news post. The chart, no words. |
| `store.about.banner.*` | up to about 616 px wide, any height (check) | Optional pictures set inside the long description: section heads in the frame's own look. |

### Rules for these pictures, as I know them (check all)

- Capsules must carry the game's name or logo, and nothing else in words: no review quotes, no awards, no "out now", no prices.
- Capsules are art, screenshots are the game. A screenshot may not be concept art or a painted picture.
- PNG or JPG. Draw each capsule for its own size; a shrunk large one loses the name.
- A demo uses the same pictures; Steam puts its own "Demo" mark on them (check).

## 2. The five screenshots worth taking from the game as it is

At least five are asked (check), 1920 x 1080, 16:9 (check). All from `L_Chart` with the frame up (wiring_plan.md
section 8). `Assets/Kapman/Tests/PlayMode/ChartGameTests.cs` already plays the frame and saves eleven pictures to
`Captures/chart`: those are the place to start, though I have not looked at them for this note.

Each one should have the whole frame on it: top bar with the date, the tiled windows, the portrait, the ship window,
the log, the eight function buttons. Use the purple-sea palette on all five.

| Id | Screen | What must be on it |
|---|---|---|
| `store.shot.1.home` | In port at Dorestat, 1 May 793, a new game. Trade and Hold windows. | The date in the top bar. Goods with the word on each (the `WordLine`), prices, the figures in gold. The chart behind with Dorestat's mark. This is the "game of words and trade" picture and should come first. |
| `store.shot.2.course` | In port, the Voyage window with a course set to Lundenwic. | The course drawn on the chart (`ShowPlanned`), both places in view (`ShowBoth`), the days of the passage low and high, Cast off ready among the function buttons. |
| `store.shot.3.atsea` | Under way at sea. | The ship on the chart on the purple sea, the ship window, the log with several true lines in it, perhaps the measure in hand with its slip at the chart's top left. |
| `store.shot.4.card` | A card as a floating window, the clock held. | A card whose words are all written, with its choices, their odds, and one choice greyed with its reason. The speaker standing in the portrait's place. |
| `store.shot.5.news` | The third workspace (called News for now): The word, Chronicle, Event. | A dated line of the chronicle, the word heard of other ports. Best taken at a second port (Lundenwic) so the page shows the player got somewhere. |

Things that must NOT be on a screenshot, all of them in the game today (section 8.8):

- `[write: key]` on a card (4,426 writing keys have no words yet). Pick the card for shot 4 by hand.
- A log line saying "not yet" (Hail, Rest and Offer). Do not press them before the shot.
- An empty face shape: the player's portrait and a card's speaker need their pictures first.
- The drawn stand-in ship window, if the real one or the 3D view (ruling 11) exists by then.
- A card where the toll man has the wrong name (the card says "Liudbrand", the art list "Aldgrim").
- Rulings 7 to 12 of 7 October are not in the frame yet (boat and chart change places, Oars in, the measure out of the
  eight). Screenshots taken before them will be out of date after them: take the store's five after those rulings are in.
- steam_release.md already says it: the mock-up's small text is under the size a PC game needs. Fix that first.

## 3. The words the page needs

| Id | Limit (check) | What it is |
|---|---|---|
| `store.text.name` | | "Kapman" without the long mark for the page's address (steam_release.md). |
| `store.text.short` | about 300 characters (check) | The short description. A draft is below. |
| `store.text.about` | long; simple formatting and pictures allowed (check) | "About this game". Yours to write. |
| `store.text.tags` | up to about 20 (check) | Chosen in Steam's tag tool. |
| `store.text.genre` | | Genres and categories from Steam's lists (single-player and so on). |
| `store.text.sysreq` | | Least and recommended machine, Windows. Not measured yet. |
| `store.text.languages` | | The table of languages: interface, sound, subtitles. |
| `store.text.mature` | | The mature content note. steam_release.md: the slave trade and raiding are in the world and the cards; say so plainly. |
| `store.text.ai` | | The AI disclosure, printed on the page. steam_release.md: yes for the code and much of the card text, the art is yours; in your own words. |
| `store.text.earlyaccess` | | If Early Access: Steam's set of questions (why, how long, what is in now, what will change, the price). (check) |
| `store.text.legal` | | The legal line and credits: map data, fonts, asset packs, chest texts, sounds. The audit is not done (steam_release.md). |
| `store.text.price` | | The price, and Steam's suggested prices for other moneys. |
| `store.text.release` | | The "Coming soon" date text. |
| `store.text.links` | | Website and other links, the support address. |

### The short description: A DRAFT FOR YOU TO REWRITE

**This is a first draft in plain words, not the text. You write the game's words; this only shows the length and what
fits in it.** It is about 230 characters (counted by hand), under the 300 (check) Steam allows.

> A merchant-sailing game of words. In 793 you take a ship out of Dorestat with a debt to pay. Read the chart, hear the word in each port, buy and sell, and choose what to do when a card turns up. One life, 793 to 805, twelve ports.

Only things the design files hold are in it: the year and the home port, the debt (steam_release.md: "the money tuned
so the debt bites"), the chart, the word on a good, the cards, one life 793 to 805 on twelve ports.

## 4. What I do not know

- Every size above is from memory and Steam has changed them before (the header capsule most lately). (check)
- `Art/art_list.csv` has the columns id, kind, name, where used, size, file, description, source data file, draw when
  (its ids run like `person.ahus.alle`, kind `portrait`, file `portrait/person.ahus.alle.png`). The slots above fit
  that shape (id `store.capsule.header`, kind `store`, file `store/store.capsule.header.png`) but none has been added
  to the list: that file was only read.
- Which of the eleven pictures in `Captures/chart` are already near to the five above.
