# The page and its strings: the fix (7 Oct 2026)

For the builders. It answers the writing critic's fifth change (`Design/critique/review_words.md`, section 6, point 5:
"Mend the page", half a star, "and for a reader with poor sight the whole score") and the newcomer's fifth
(`review_newcomer.md`, section 6, point 5: "One fixed screen for the first hour, swept clean", plus 3), and their
cons 12, 13, 15, 19 (words) and 10, 14 (newcomer).

Read, not run: Unity was not started, git was not used, nothing under Assets was changed. Files were read between
about 12:00 and 12:30 on 7 October, while six builders were changing the project (`FrameRoot.Floats.cs` was saved
at 12:28 as I read it). **Line numbers are of that half hour. Each row also gives the words, so search for the words.**

Six parts: 1 the rule and its test, 2 every string to change (with the words that should stand there), 3 line
length, 4 the text-size setting, 5 the chest, 6 captives. Then the order of work and what I could not check.

Where the new clock matters (ruling 26: pause and five speeds, a day in fifteen seconds at speed 4) the row says so.
Most of this fix does not rest on the clock. Three things do: the Rest button (2.2), the lines that name "1x, 2x,
4x" (2.9), and the tide wait's line in hours (2.4).

## 1. The rule, and a test that holds it

**The rule.** Nothing reaches the screen that was written for the makers. A window says what is so in the world. It
does not say what the game lacks, and it does not name a file, a key, a flag, a window or a rule, or put a number in
brackets after a word. A thing that is not built has no row and no button. Tooltips and the hint line at the foot of
a card may name a key ("Shut  [Esc]"): there the interface is speaking, and the player asked. In the log, the
windows' sentences, the cards and the talk the world is speaking, and it may not.

**The test.** One pure function says what is wrong with a string, and three callers use it. Put it in
`Assets/Kapman/Code/Game/UI/PageWords.cs` (new file, namespace `Kapman.Game`, beside `UiTyping.cs`). Not compiled.

```csharp
using System.Text.RegularExpressions;

namespace Kapman.Game
{
    /// <summary>THE PAGE'S RULE (7 Oct 2026, Design/fixes/page_and_strings.md): no text reaches the screen that was
    /// written for the makers. Fault() names what is wrong with a string, or null. Pure: a test holds it.</summary>
    public static class PageWords
    {
        static readonly (string name, Regex rx)[] Faults =
        {
            ("says what is to come",     new Regex(@"\bto come\)", RegexOptions.IgnoreCase)),
            ("says what the game lacks", new Regex(@"(?i:not (kept|counted|built|written|read|reckoned) yet|not in the game|no rule in the game|: not yet\b|not yet people)|^not yet$|^Not yet chosen$")),
            ("an unwritten key",         new Regex(@"\[write:|\(placeholder\)|\bTODO\b")),
            ("a number in brackets",     new Regex(@"\([^)]*\d[^)]*\)")),
            ("a word in brackets",       new Regex(@"\([a-z][a-z ]{0,20}\)")),
            ("a tag or key in brackets", new Regex(@"\[[A-Za-z0-9 \-]{1,24}\]")),
            ("a name in quote marks",    new Regex("\"[a-z0-9_.]+( [a-z0-9_.]+)*\"|'by [a-z_.]+'")),
            ("the sums joined",          new Regex(@"^Either: |; or: ")),
            ("an id",                    new Regex(@"\b[a-z]+(_[a-z0-9]+)+\b|\b(npc|hold|flag|text|me|port|crew)\.[a-z_.]+\b")),
            ("a file name",              new Regex(@"\.(json|cs|md|png|uss)\b")),
            ("a workshop word",          new Regex(@"\bpalette\b|\bpips?\b|\bflag\b|\bplaceholder\b|\b(panel|window)\b|\bA check\b|\bstats?\b", RegexOptions.IgnoreCase)),
            ("a sign of the sums",       new Regex(@"<=|>=|!=")),
        };

        /// <summary>What is wrong with a string the player would see, or null. `tooltip`: a key in square brackets
        /// and the words window and panel are allowed there (the interface speaking).</summary>
        public static string Fault(string text, bool tooltip = false)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            foreach (var (name, rx) in Faults)
            {
                if (tooltip && (name == "a tag or key in brackets" || name == "a workshop word"))
                    continue;
                if (rx.IsMatch(text))
                    return name;
            }
            return null;
        }
    }
}
```

**Tried, outside Unity.** The C# was not compiled. The same twelve patterns were run in Python over the strings of
this file: all 26 faulty lines quoted in part 2 are caught, and 12 clean lines pass, among them the nine choices of
`writing.json` that begin "Not yet" as world speech (lines 6550 to 9790: "Not yet: one more season"), which the
pattern lets through because it is bound to the frame's own two strings. Run over all 13,190 texts of
`writing.json` it found three things:

- **143 texts with a number in brackets**, 130 of them a choice with its price written into the words: "Give him
  something (12 d)" (line 66), "Buy a brace of duck (4 d) and ask him the way in over the Deben bar" (1014), "Sell
  him the gold mount for melting (160 d)" (1250). The other 13 are of the kind "(3 d for ale)" (38354).
- **195 choices that end in the odds as a word in brackets**: "Drop the sail and the mast and lie on the flats
  until dark (sure)" (3622), "(likely)" (3626, 3630). The card window already shows the odds beside the choice
  (`FrameRoot.Floats.cs:266-270`), so these read twice, and the written word can disagree with the reckoned one
  once a stat moves the odds.
- 30 texts with "window" in them, all holes in walls. So the workshop-word pattern is for the frame's strings and
  the data of caller 1 only, not for the cards' prose.

What to do with the first two is in 2.4.

The three callers:

1. **An EditMode test over the data the frame prints** (no scene): every `label` and `intention` of
   `wording_793.json` (cultures.*.menu, kinds.*.extra, ports.*.extra, market, outcomes, word); in
   `story/self_793.json` every people's `hard`, every faith's `asks` and `opens.what`, every past's `gift.what`,
   `chest.thing`, `chest.what`, `knows.why`, every stat's `what` and `levels`; in the twelve ports' files
   (`places/<port>.json`) every person's `role` and lines, every site's `name` and `see`, every era line.
2. **An EditMode test over every lock a card can show**: for each card of the first release, each option's
   `check.Describe(words)` (the call `Code/Core/Encounters/EncounterSession.cs:178` makes), and each fact an effect
   can say.
3. **In the frame's own play test** (the one that plays a new game to Lundenwic): after each step, walk
   `FrameRoot.Current.Root.Query<TextElement>().ToList()`, and fail on the first `Fault(el.text)` and on the first
   `Fault(el.tooltip, tooltip: true)`, naming the element's classes and the text. This is the one that catches
   strings built in code. Open every panel once in that test (Bonds, Crew, Ship, House, Faith, Tongues, Chest, The
   word, Chronicle, Tasks, The field, Wind and tide, Event), at sea and in port, so that each `Fill` runs.

## 2. Every string, with the words that should stand there

Columns: where, what stands there today, what should. "Goes" means the line is taken out and nothing stands in its
place. Paths are under `Assets/Kapman/` unless they start with `Design`. `Panels/` is
`Code/Game/UI/Frame/Panels/`; `Rack.cs` is `Code/Game/UI/Frame/FrameRoot.Rack.cs`.

### 2.1 "(to come)"

The newcomer's con 10: every row of the Port window ends in "(to come)", the one that works included. All 22 are in
the data, not the code: `Game/Data/wording_793.json`, the `intention` of a menu row, which `Panels/PortPanel.cs:54`
shows as the small line under the row. I checked each errand against `Code/Game/Ports/PortLife.cs:100-143`: six of
the nine are built, so their "(to come)" is false.

| Where | Today | Should stand |
|---|---|---|
| wording_793.json:66, 119, 172, 225, 286, 339, 392, 445 (one a culture; key `port.news`) | "News, prices elsewhere, the weather (to come)." | "News, and what other places are paying." (PortLife.cs:40-86 gives news, a fair, orders wanted and prices. It gives no weather, so that word goes too.) |
| :492 (kind emporium), :589 (Sarre), :598 (Fordwic); key `port.tollhouse` | "Tolls and permission to trade (to come)." | "What the toll is here, and what the law says of your cargo." (PortLife.cs:112-123 gives that, and the debt.) |
| :502 (emporium; `port.moneyer`) | "Change foreign coin into local money, at a loss (to come)." | "Change foreign coin for the coin of the place, at a loss." |
| :512 (market), :522 (town), :577 (village); `port.host` | "A place to stay ashore for you and the crew (to come)." | "A night ashore for you and the crew: a penny a man." (PortLife.cs:127) |
| :527 (town; `port.church`) | "Give silver to the church (to come)." | "Give alms at the church door." (PortLife.cs:130-136: two in the hundred of your silver, never under 2 d.) |
| :537 (monastery; `port.guesthouse`) | "The monastery's guest house (to come)." | "A night at the guest house: a halfpenny a man." (not a kind of the twelve ports) |
| :542 (monastery; `port.minster`) | "Buy its surplus, sell what it needs (to come)." | "Buy what it has over, sell what it lacks." (not among the twelve) |
| :552 `port.hall`, :557 `port.reeve` (royal seat), :567 `port.fort` (fort) | "Ask to be heard by the lord (to come)." and two more | Not built: PortLife.cs:141-142 answers "Not built yet: it comes with its system." The rows must not show (2.2, the last row). None of the twelve ports is a royal seat or a fort, so the first release never meets them. |
| Code/Game/Sim/Patrols.cs:341 | "... You are taken before the count for judgement (to come)." | "... You are taken before the count." |
| Code/Game/Map/MapTable.cs:61 (the chest button's tooltip; the frame hides that bar, MapTable.cs:147) | "Everything written you keep: your price notes, and (to come) texts, riddles and your own notes." | "Everything written that you keep: price notes, texts, riddles and your own notes." |
| Game/Data/trade_rules.json:20, :26 (a zone's `note`) | "...take their due in kind (to come)." / "Gold nomismata and silver miliaresia (to come)..." | Not shown by the frame as far as I read. Clean them anyway: strike " (to come)". |

The twelve ports by kind (`ports_793.json` lines 4-37), so the builder can see which rows a first-release player
meets: emporium (Dorestat, Walichrum, Witla, Quentovic, Lundenwic, Ripa: toll house, exchange), town (Traiectum,
Daventre: lodging, alms), market (Medemelacha, Serrae, Fordwic: lodging; the two Kentish ones also the toll house),
village (Doccinga: lodging). All of those are built.

### 2.2 "Not yet", "not kept", "not in the game"

The systems critic counted five windows and three buttons that say what is missing. The rule: the line goes, and a
thing that does nothing has no button.

| Where | Today | Should stand |
|---|---|---|
| Rack.cs:252 (tooltip), :323 (log) | "All hands rest for a watch. Its rule is not in the game yet." / "Rest: not yet. A watch below has no rule in the game yet, and no hours are skipped." | The **Rest** button goes from the rack. It was written for the old clock (resting ran the clock at 30 times). Under ruling 26 a rest is a wait at the speed the player picks, and that is already the Wait button's "Till first light" (Rack.cs:371). Nothing is lost. |
| Rack.cs:256 (tooltip), :326 (log) | "An offering, or a vow, by your faith. Its rule is not in the game yet." / "Offer: not yet. Offerings and vows come with the faiths." | The **Offer** button goes until an offering does something (the words critic's third change is where it comes back). |
| Rack.cs:144-145 (the list of eight) | Cast off, Wait, Measure, Look out, Sound, Hail, Rest, Offer | Ruling 8 already moves Measure to the right and puts Oars in. With Rest and Offer out the rack has six that all work. If eight places are wanted for the look, two stand as empty slips with no word and no click. For the builder of QUEUE part 3. |
| Panels/BondsPanel.cs:80 | "How the people of the towns feel about you is not kept yet: only that you have spoken." | Goes. |
| Panels/CrewPanel.cs:39 | "The crew are not yet people with names, tongues and faiths of their own: that comes with hiring." | Goes. (Four names are the words critic's con 18: another fix.) |
| Panels/CrewPanel.cs:36 | "At sea" with the figure "not yet" | "At sea" with "still in port" |
| Panels/FaithPanel.cs:58 | "Keeping these days is not counted yet: nothing stops a sale on a feast, and Troth does not move by them." | Goes. (Both critics quote it. When Troth moves, a line saying what moved it may stand here.) |
| Panels/FaithPanel.cs:41 | "The year's days are not written yet." | "No days are reckoned for this year." |
| Panels/HousePanel.cs:53 | "Plots, stores, other boats and an heir are not in the game yet." | Goes. |
| Panels/ShipPanel.cs:50 | "Her wear, her fittings and a yard to mend her at are not in the game yet." | Goes. |
| Panels/SelfPanel.cs:22-23 | "Not yet chosen" / "A new game asks who you are: your people, your faith, your past." | "Nobody yet." and the second line goes. (Seen only before the who-are-you window is answered.) |
| Panels/EventPanel.cs:36 | "The cards are not read yet." | "Nothing yet." |
| Panels/PeoplePanel.cs:42, :48 | "Nothing is written of the people of this place yet." / "Nobody here is written of for this year." | "You know nobody here." / "Nobody you know is here this year." |
| Panels/TonguesPanel.cs:44 | "You have no tongue written down yet." | "No tongue is known." |
| Panels/SkyPanel.cs:48, :70 | "There is no weather to read yet." / "No tide is reckoned on the chart yet." | "No weather to read." / the second goes (no row for a tide that is not there). |
| Code/Game/Chest/Chest.cs:166 | "There's no free English of this text: only its own words." ("free" is the licence talking) | "Nobody has put this into English." (With part 5 it is rarely seen.) |
| Code/Game/Map/ChestPanel.cs:353 | "This writing is missing from the tables." | "The leaf is blank." and a warning to the console with the id. |
| Code/Game/UI/Frame/FramePanel.cs:81 | "This window could not be drawn: " and the exception's message | "This window is empty." and `Debug.LogException(e)`. A C# error must not be read out to the player. |
| Code/Game/Ports/PortLife.cs:142 | "Not built yet: it comes with its system." | Never shown: the Port window leaves out a row whose errand is not built. Draft below. |

`Panels/PortPanel.cs`, in the loop at lines 49-56, the test at 52-53 becomes:

```csharp
                if (key == "port.town" || key == "port.leave" || !PortLife.Built(key))
                    continue;                                             // (the Trade window; the Cast off button; an errand not built has no row)
```

and `Code/Game/Ports/PortLife.cs` gains, above `Errand` (line 100):

```csharp
        /// <summary>Is this row of the port's menu something the game does? (The hall, the reeve and the fort are not.)</summary>
        public static bool Built(string key) => key != "port.hall" && key != "port.reeve" && key != "port.fort";
```

### 2.3 Ids and file names shown as text

**The root of most of them is one function.** `Code/Core/Encounters/Cond.cs:402-408`, `Human(name)`, turns
"npc.tollman.feeling" into "tollman feeling" and "charter_toll_dorestad" into "charter toll dorestad". Every line
under a choice that cannot be taken is built from it (`EncounterSession.cs:178`, shown by
`FrameRoot.Floats.cs:259-264` and `Panels/EventPanel.cs:60`). Both critics quote two of them from the captures.

| Where (Cond.cs) | Today | Should stand |
|---|---|---|
| :252 | `Only with the writing "charter toll dorestad" in your chest.` | "Only with the toll charter of Dorestat in your chest." The id becomes the text's title; no quote marks. |
| :254, :262 | "Only after: bk carried." / "Not after: bk carried." (a flag's name) | "Not now." A flag is the makers' name for a thing that happened; it is never shown. |
| :256, :333, :264, :335 | "Only with captives aboard." / "Not with weapons aboard." / "Only with no captives aboard." | "Only if you carry captives." / "Not while you carry weapons." / "Only if you carry no captives." (the good's own name, lower case) |
| :257, :265 | "Only when crew seamanship best." / "Not when ..." | "Not now." |
| :234 | "Either: Only with weapons aboard; or: Only with captives aboard." | "Only if you carry weapons, or captives." Join the parts with ", or " and drop a repeated "Only if you carry". Where the parts do not share a start: the first sentence, then "Or: " and the second. |
| :187 | "Not when: " and the inner rule | "Not now." unless the inner rule is one of the named kinds above. |
| :320, :322 | "The tollman must be at least easy with you." | "The toll man must be at least easy with you." (the role's title from `npc_roles_793.json`, as `EncounterSession.cs:132` reads it, not the role's id) |
| :345, :347 | "Your Tongue must be 3 or more." | "Your Tongue must be good or better." (the level's word, `SelfBook.Stat(id).Level(n)`, as the Yourself window shows it; no bare number) |
| :349, :398 | "Only when hold value >= 200." (the sign of the sum is printed, Cond.cs:284-295) | "Not now." |
| :367 | "Only at dorestad." (the port's id; the town is spelt Dorestat on screen) | "Only at Dorestat." |
| :368 | "Only if your people is frisian." | "Only for a Frisian." (the people's name from `self_793.json`) |
| :369 | "Only where port kind is emporium." | "Not now." |

How to do it without `Kapman.Encounters` (the Core assembly) knowing the game: Cond.cs gets one hook, and the game
fills it. Draft, not compiled:

```csharp
        // Code/Core/Encounters/Cond.cs, above Human (line 401)
        /// <summary>The game's own name for a thing a rule names ("text.charter_toll_dorestad" gives "the toll charter
        /// of Dorestat"; "hold.captives" gives "captives"; "port:dorestad" gives "Dorestat"; "role:tollman" gives "the
        /// toll man"), or null when it has none that a player may read. Set by the game (ContentRules).</summary>
        public static Func<string, string> NameOf;

        /// <summary>The name to show for a thing, or null: never the id.</summary>
        static string Shown(string key) => NameOf != null ? NameOf(key) : null;
```

Each `Describe` above asks `Shown(...)` first and falls back to "Not now." when it gives null; `Human` is then used
only in warnings to the console. The game sets the hook where it sets its word table
(`Code/Game/Content/ContentRules.cs`, by `Why` at line 206): `text.<id>` from `ChestData.Text(id)?.title`,
`hold.<id>` from `GameData.Good(id)?.name` in lower case (and "weapons", "captives" for the two sums),
`port:<id>` from `GameData.Port(id)?.name`, `role:<id>` from the roles table, `me.<stat>` from `SelfBook`.

**Better still, a written line for each lock.** The newcomer's mend. An option may carry its own words for why it is
shut: a writing key `<card>.<option>.locked` in `writing.json`, read in `EncounterSession.cs:178` before
`Describe`. The made line above is then only the fallback. How many locks the first-release cards have I did not
count; the content pass (stat-tagged choices) is adding more.

The rest of the ids:

| Where | Today | Should stand |
|---|---|---|
| Code/Game/UI/Frame/PersonTalk.cs:50 | `"Ask about " + key.Replace('_', ' ')` for every line key not in the list of four at :18-23 | See below: this one is large. |
| Game/Data/story/self_793.json:399 (a past's gift) | "A second tongue, learned on the bench. For a Dane it is old_frisian. ..." | "... For a Dane it is Frisian. ..." |
| self_793.json:300 (a faith's `asks`) | "Keep Passover, the Day of Atonement and the other appointed days: calendar_793.json gives them for each year." | "Keep Passover, the Day of Atonement and the other appointed days." (Shown only if that faith is offered.) |
| Game/Data/places/doccinga.json:45 (a person's `role`) | `lords_man`, shown as "lords man" (`PlaceBook.cs:57` swaps the underline for a space) | "the lord's man": write the role as it should be read. The other underlined roles of the twelve ports read well enough (farm wife, raider chief, guest master, harbour master). |
| Panels/FieldPanel.cs:59 | `Article(s.cls.name)`: "A Scandinavian small clinker boat (faering, six-oar boat, lake and river boat) of Ripa" (the catalogue's name, brackets and all) | `Article(SeaWatch.ShortName(s.cls))` (SeaWatch.cs:107 already cuts the brackets, and the lookout uses it). Better: a short `call` name a class in `ship_classes_793.json` ("a six-oared boat"): 12 and more names to write. |

**The talk offers the makers' keys as questions.** `PersonTalk.AskKeys` (PersonTalk.cs:53-65) offers every line a
person has, bar the greetings and the farewell, and labels any key outside its list of four as "Ask about" and the
key. In the twelve ports' files there are 66 kinds of key (my count); 58 of them are not in the list. Most are not
topics at all. They are the states in which a line is said:

- `to_heir` (26 people): what they would say to your heir. Offered as "Ask about to heir", and the line itself is
  said to the first merchant.
- `friend` (9), `trusted` (9), `warm` (8), `trust` (8), `greet_old`, `later`, `returned`, `warned`, `win`, `lose`:
  lines for when they know or trust you. Offered as "Ask about trusted".
- `after_796`, `after_fire`, `before_882`, `after_882`, `after`: lines for other years. Offered as "Ask about
  after 882" in 793. (This is how the priest tells of a raid that has not happened: the newcomer's con 9.)
- `greet_witla`, `farewell_witla`, `saying_witla`: one man's lines for another port.
- True topics, about 30, one person each: `york`, `dirham`, `leaf`, `law`, `hunger`, `feast`, `bell`, `grave`,
  `wine`, `stone`, `tally`, `gold`, `father`, `daughter`, `husband`, `cousin`, `sunday`, `lent`, `winter`, `earth`,
  `eyes`, `age`, `wage`, `boat`, `crew`, `search`, `gift`, `heir`, `low_water`, `teaches`, `trade`, `ask`.

What should stand: a key is offered only when it has a label written for it. A topic's label is written beside the
line in the port's file (`"ask": "Ask about the leaf"`), and a key with no label is never offered: the game picks
state lines by the state (the years, the heir, whether you have met), which is the content pass's work on dates and
on lines that fit who you are. Until that data is there, the smallest safe change is in code:

```csharp
        // Code/Game/UI/Frame/PersonTalk.cs, replacing lines 61-63 of AskKeys
            foreach (string k in p.keys)
                if (!IsGreeting(k) && k != "farewell" && !keys.Contains(k) && IsTopic(k))
                    keys.Add(k);
```

```csharp
        /// <summary>A key that names a thing to ask about, not a state a line is said in (to an heir, to a friend, in
        /// other years, at another port).</summary>
        static bool IsTopic(string key) =>
            key.IndexOf('_') < 0 && key != "friend" && key != "trusted" && key != "warm" && key != "trust" && key != "later"
            && key != "returned" && key != "warned" && key != "win" && key != "lose" && key != "after" && key != "age"
            && key != "ask" && key != "teaches";
```

That leaves "Ask about york", "Ask about dirham" and the like: plain, if lower case. `low_water` is lost by the
underline test and is the one topic to add by hand. The cost: until the state lines are picked by the state, they
are not heard at all. That is right for `to_heir` and the dated ones, and a loss for `friend` and `trusted` (26
lines), which is why the data fix should follow soon.

### 2.4 Numbers in brackets

| Where | Today | Should stand |
|---|---|---|
| Code/Game/Reputation.cs:190 (the Port window's second line and the House window, `PortPanel.cs:39`, `HousePanel.cs:48`) | "Your standing here: known (+25). In the region: known (+10)." | "Your standing here: known. In the region: known." |
| Code/Game/Map/MapView.cs:645-648 (a port's card on the chart) | "Your standing here when you were last here (12 days ago): known. In the region then: known." / "Your standing here: not known (you haven't been here)." | "When you last lay here, 12 days ago, you were known. In the region: known." / "Nobody here knows you." |
| Code/Core/Encounters/Effects.cs:156 (a fact after a choice) | "Liudbrand thinks better of you (easy)." | "Liudbrand thinks better of you." and, only when the word changed: "He is easy with you now." |
| Code/Core/Encounters/EncounterSession.cs:135 (who speaks, on a card) | "Liudbrand, the toll man · easy" (reads as how hard he is) | "Liudbrand, the toll man" and the feeling as words of its own in the quiet line under the name: "Easy with you." |
| Code/Game/Chest/Chest.cs:169-170 | "You can't read Latin yet. A monk could read it to you at a monastery, or you could learn it there (2 of 6 lessons)." | "You cannot read Latin. A monk would read it to you at a monastery, or teach you: you have had 2 lessons of the 6." |
| Chest.cs:586 | "... (no toll on your sales at Lundenwic until 796)." | "... No toll on what you sell at Lundenwic until 796." |
| Code/Game/Voyage/TideWait.cs:267-268 | "The flats ahead have too little water for her (0.4 m; she wants 0.9). She lies to and waits for the tide: about 5.5 h." | "The flats ahead have too little water for her. She lies to for the tide: about five hours." At speed 4 of the new clock five hours pass in three seconds, so this line is the only sign of the wait: keep it, in words. |
| Code/Game/Ports/PortLife.cs:59 (Talk to people) | "<the news> (3 days ago; brought by a skipper of Ripa)." | "<the news> A skipper of Ripa brought it, 3 days ago." |
| PortLife.cs:81, :84-85 | "Prices heard for 14 places, now in your ledger: Traiectum (3 days ago), Witla (today) and 6 more." | "You hear what 14 places were paying: Traiectum 3 days ago, Witla today, and 6 more." ("your ledger" is the interface.) |
| PortLife.cs:170 | "... (the moneyer keeps 3.2 deniers' worth)." | "... The moneyer keeps 3 d of it." |
| Code/Game/UI/Frame/FrameRoot.cs:807, :813 (the log) | "Word comes: <the news> (a skipper of Ripa, told at Dorestat)" | "Word comes from a skipper of Ripa, told at Dorestat: <the news>" |
| Panels/WordPanel.cs:61 | "Dorestat (here)" | "Dorestat, here" |
| Code/Game/Map/ChestPanel.cs:419 | "Translated by John Lesslie Hall (1892). ..." | Behind the press (part 5), as "Put into English by John Lesslie Hall, 1892." |
| ChestPanel.cs:477, :638 | "(The runes and the spoken words don't line up word for word here: read it as a whole.)" / "<title> (a riddle)" | The same without the brackets / "<title>, a riddle" |
| Panels/ChroniclePanel.cs:62 (a tooltip) | "(uncertain)" | "Not certain." |
| FrameRoot.Floats.cs:258, Panels/EventPanel.cs:60, EncounterSession.cs:51 | "[Tongue] Ask him to wait till Lammas": the choice's tag in square brackets | The tag as a small word of its own before the label, no brackets. Draft below. The content pass is adding stat-tagged choices, so there will be more of these. |

The tag, in `FrameRoot.Floats.cs`, lines 256-259 (inside the loop over `spec.options`) become:

```csharp
                    var w = Box("frame-option-words");
                    if (!string.IsNullOrEmpty(o.tag))
                    {
                        var tag = Text("frame-option-tag");
                        tag.text = o.tag;
                        w.Add(tag);
                    }
                    var label = Text("frame-option-label");
                    label.text = o.label;
                    w.Add(label);
```

and Frame.uss gains, after `.frame-root .frame-option-label` (line 971 today):

```css
.frame-root .frame-option-tag {
    font-size: var(--t-quiet);
    color: var(--frame-accent);
    -unity-font: url("Fonts/GentiumBookPlus-Bold.ttf");
}
```

(`EventPanel.cs:60` and the chosen line at `FrameRoot.Floats.cs:657`, which uses `o.Tagged`, take the same change:
tag, a colon, the label: "Tongue: ask him to wait till Lammas".)

Keys in square brackets in tooltips stay ("Save, load, a new game, quit  [Esc]", FrameRoot.cs:185): see the rule.

**The cards' own choices carry the price and the odds in brackets** (found by the test's patterns, part 1; these
are in `Game/Data/writing.json`, the cards' words, which the content pass is rewriting now):

| What | How many | Should stand |
|---|---|---|
| A choice with its price in its words: "Give him something (12 d)" (writing.json:66) | 130, and 13 of the kind "(3 d for ale)" | The words without the bracket: "Give him something". The price is shown by the window from the choice's own `cost`, as a gold figure at the right of the row where the odds stand: "12 d". Then the words and the charge cannot disagree, which matters this week: the pay-cap rewrite changes 766 amounts (`Design/QUEUE.md`, "Left for the user"). |
| A choice that ends in its odds: "... until dark (sure)" (writing.json:3622) | 195 | The words without the bracket. The window already shows the odds from the choice's chance. |

For the builder: until the 338 lines are cleaned in the data, strip both in code where the label is split from its
tag (`Code/Core/Encounters/EncounterSession.cs:56-71`, `SplitTag`): a trailing `(sure)`, `(likely)` or
`(doubtful)`, and a trailing or inner `(<number> d)`. The price figure needs one more string on `ShownOption`
(`EncounterSession.cs:47`) and on `FloatOption` (`FrameRoot.Floats.cs:14`), filled from the option's cost the way
`ContentRules.Lacks` reads it (`Code/Game/Content/ContentRules.cs:210-240`: the verb `silver` with a minus). To
the six agents of the content pass: do not write a price or the odds into a choice's words.

### 2.5 Names of rules in quote marks

Two, both listed above: `Cond.cs:252` (the writing's id in quote marks) and `self_793.json:372` (2.6, first row).
The test's "a name in quote marks" keeps a third from arriving.

### 2.6 Workshop words

| Where | Today | Should stand |
|---|---|---|
| Code/Game/UI/Frame/FrameRoot.Self.cs:130-136 (the who-are-you window, under Your people) | "You start at Dorestat, and the world has the colours of the Tide palette." | "You start at Dorestat." Lines 130-133 and 135-136 go. The chart changes colour when the chip is pressed: the player sees it. |
| self_793.json:372 (the steersman's child, `gift.what`) | "You read water. A check 'by crew.seamanship_best' counts one pip better while you are aboard." | "You read water. While you are aboard she is handled as if by a better steersman." |
| self_793.json:412 (the toll-keeper's clerk) | "You know every toll between the sea and Colonia by heart: the toll panel shows the due before you land." | "You know every toll between the sea and Colonia by heart, and what is due before you land." |
| self_793.json:426 (the hall child, `chest.what`) | "A cloak of the long old cut and a gilded brooch: worn, they are a pip of Name on a first meeting." | "A cloak of the long old cut and a gilded brooch: worn, they make a stranger think more of your name at a first meeting." |
| self_793.json:262 (a faith's `opens.what`) | "Baptism at the last, which washes everything: a card that comes once." | "Baptism at the last, which washes everything. It can be asked for once." |
| self_793.json:35 (the Frisian's `hard`; its first sentence is shown, FrameRoot.Self.cs:140) | "Nothing more than the count's day: this is the plain start, and the others are measured against it." | "Nothing more than the count's day." |
| self_793.json:93 (the Dane's `hard`) | "Among the twelve ports only Ripa is yours, and it is full only in summer." ("the twelve ports" is the release, not the world) | "Of the ports you know only Ripa is yours, and it is full only in summer." |
| Panels/SelfPanel.cs:46 | "A stat rises by use, never by spending: each check you win by it adds a little, more against someone who resisted." | "These grow by use, never by silver: each time one carries the day it grows a little, and more when someone stood against you." |
| Panels/TonguesPanel.cs:63 | "A word heard 3 times stays English from the start." | "A word you have heard 3 times you know for good." |
| Panels/HoldPanel.cs:39 | "Your own from the start." (the newcomer's con 13: the plans call the casks the count's) | "On the count's tally." if the debt card says they are his (`Design/fixes/debt_day.md` decides); else "Yours." |

### 2.7 English that explains the interface instead of the world

A window names another window, or tells the player where to click. In the first hour's fixed screen (the newcomer's
fifth change) the thing named is in sight, so the sentence can go.

| Where | Today | Should stand |
|---|---|---|
| Panels/PortPanel.cs:60 | "The market is the Trade window. Casting off is the button above the windows." | Goes. |
| Panels/PortPanel.cs:36, Panels/PeoplePanel.cs:33 | "She lies at no port. When she moors, what can be done there shows here." / "... the people of the place show here." | "She lies at no port." |
| Panels/ChestFramePanel.cs:43 | "To read a text or try a riddle, open the chest: press I." | A button, not a sentence: `body.Add(FrameUi.Go("Open the chest", ...))`, calling `MapTable.OpenChest(tab)` (Code/Game/Map/MapTable.cs:290, public). How the frame reaches the chart's table I did not trace. |
| Panels/BondsPanel.cs:76 | "You know nobody yet. The People here window has those you can talk to in a port." | "You know nobody yet." |
| Panels/FieldPanel.cs:105 | "The field is the other ships bound where you are bound. Set a course in the Voyage window." | "No course is set, so nobody is ahead of you or behind." |
| Panels/TasksPanel.cs:74 | "None. Orders are posted in the ports: see below when she lies in one." | "None taken." |
| Panels/EventPanel.cs:42-43 | "She lies at Dorestat. Who comes down to the boat, and what the day brings, shows here and over the chart." / "At sea. What the watch brings shows here and over the chart." | "She lies at Dorestat. Nobody has come down to the boat." / "At sea. The watch has nothing to tell." |
| Rack.cs:198 (a tooltip) | "No course is set. Pick a port in the Voyage window and set its course first." | "No course is set." (A tooltip may name a window; this one need not.) |
| Rack.cs:242 (a tooltip) | "Hailing is for the sea. In port, speak with the people in the People here window." | "There is nobody to hail in port." |
| Rack.cs:480 (the log) | "The lookout has 3 things to say: press Look out." | "The lookout calls down: three things." |
| Rack.cs:305 (the log) | "The measure is in your hand: click two places on the chart." | Nothing in the log: the measure's own slip says it (`FrameRoot.Measure.cs:91`). |
| FrameRoot.cs:815 (the log) | "You hear the talk of the waterfront: 5 things new to you. The Chronicle has them." | "You hear the talk of the waterfront: five things new to you." |
| Panels/WordPanel.cs:39 | "You have no word of any market's prices yet. Look at a market, or talk to people in port." | "You have no word of any market's prices." |

### 2.8 Keys named in the world's voice

`FrameRoot.cs:108` hangs the log on `Voyage.Notice`, so every `Voyage.Say` of the older game reaches the frame's
log, with the keys of the older game in it.

| Where | Today | Should stand |
|---|---|---|
| Code/Game/Voyage/Rest.cs:53 | "Night falls. [Z] to rest till dawn." (the newcomer's con 10; and Z rested at 30 times the clock, which rulings 5 and 26 end) | "Night falls." |
| Code/Game/Voyage/VoyagePlanner.cs:224, :229 | "Choose where to sail: pick a port on the map." / "No course set: [M] for the map, then pick a port." | "No course is set." for both. |

### 2.9 Lines written for the old clock

| Where | Today | Should stand |
|---|---|---|
| FrameRoot.cs:202-204 (the clock's buttons and tooltips) | "Pause", "1x", "2x", "4x"; "A game minute a second  [-]", "4 times as fast  [+]" | For the builder of ruling 26. Five buttons numbered 1 to 5, and tooltips that say what passes, not a multiple: "A game minute each second", "A watch in a minute", "A day in a minute", "A day in a quarter minute", "A day in four seconds"; then the key. |
| FrameRoot.cs:720-721 (the state word by the clock) | "paused", "waits for your answer", "held" | Add one for the clock dropping back by itself (ruling 26), naming the cause in two or three words: "slowed: a sail", "slowed: port near", "slowed: the debt's day". The log says the same thing as the world would: "A sail, close to the south." |
| Rack.cs:205, :224, :366 | "She stops and lies to. The clock runs on." / "... The clock runs while you wait, and bread is eaten." | Still true at five speeds. Keep. |
| FrameRoot.Floats.cs:302 | "It waits for your answer. The clock is held: Space lets it run." | Still true. Keep. |

### 2.10 Seen on the way, not on the critics' list

Not asked for, so not counted in the work below. A reader of history sees them in the first minute.

- **Modern measures.** `Panels/SkyPanel.cs:60` shows the wind as "7 m/s"; `Panels/ShipPanel.cs:36-46` "m" and
  "knots"; `Panels/HoldPanel.cs:32-41` and `Panels/TradePanel.cs:126` "kg"; `HoldPanel.cs:46` "litres";
  `Panels/VoyagePanel.cs:158, :181, :187, :237` and `Panels/FieldPanel.cs:65, :110` "km"; `Rack.cs:416, :449` "5 km
  off". The Voyage window already gives days ("About two days for your boat"), which is the measure a skipper had.
  The wind wants a word (`FramePassage.cs:91-94` has four).
- **Money has three names** (the newcomer's con 14): "34 d" in rows, "deniers" from the purse
  (`Purse.Amount`, in the sale's line), "deniers' worth" (`FrameRoot.cs:194`, `PortLife.cs:131`), "g silver" at Ripa.
  One word: "d" after a figure, "pennies" in a sentence, and silver by weight only where the place weighs it.
- **The narrator in two centuries** (the words critic's con 10): the era line under "These years"
  (`Panels/PeoplePanel.cs:63-66`) prints the whole era, next year's law included (`places/dorestad.json:39`). The
  content pass on dates has it.
- **The older panels.** `Code/Game/UI/MarketPanel.cs`, `TextPanel.cs` and `Code/Game/Encounters/EncounterPanel.cs`
  are not drawn under the frame (`TextPanel.External`, FrameRoot.cs:100; the Port window skips `port.town`,
  PortPanel.cs:52). I did not sweep them. `EncounterPanel.cs:209-222` has "[Enter]" and "(likely)" if it is ever
  shown again.

## 3. Where a line runs wider than seventy letters, and how to hold it

**The measure.** Gentium Book Plus Regular averages 0.4146 of its size a letter, spaces counted (my sum over the
10,035 texts of `writing.json` longer than 60 letters, from the font file's own widths). So a letter is 7.5 px at 18
and 9.1 px at 22, and seventy letters are 522 px at 18 and 638 px at 22: **29 times the size**. Widths below are at
1920 x 1080, from `Game/UI/Frame.uss` and `Game/UI/MapView.uss`. Sums, not seen.

| What is read | Rule (line in Frame.uss today) | Width of the text | Letters a line today | With the four sizes of text_sizes.md |
|---|---|---|---|---|
| A card's or a talk's speech | .frame-float-line (909), 18 px | 662 px (the float is 42%, 806 px, less 8 of edge, 32 of padding, 104 of face) | **89** | 73 at 22 |
| A card's small print | .frame-float-small (921), 14 px | 662 | **114** | **89** at 18 |
| A choice | .frame-option-label (972), 17 px | about 730 | **104** | **80** at 22 |
| The lock under a choice | .frame-option-rule (977), 13 px | about 730 | **135** | **98** at 18 |
| Who are you (the float is 54%) | .frame-self-line (1074), 15 px | 893 | **144** | **98** at 22 |
| The log | .frame-log-text (795), 15 px | 676 | **109** | 71 at 22 (the stamp grows to 150) |
| Left column, a sentence | .frame-text (467), 16 px | about 520 | 78 | 57 |
| Left column, quiet print | .frame-p (461), 14 px | about 520 | **90** | 70 |
| Left column when wide (46%) with one window | .frame-left.wide (228, the rule) | about 846 | **128** | **93** |
| The chest: a text | MapView.uss .chest-word (657), 15 px, one label a word | 665 | about **96** | not in that fix |
| The chest: a note | MapView.uss .map-chest-note (494), 13 px | 695 | **129** | not in that fix |
| The chest: the credit | MapView.uss .chest-credit (686), **11 px** | 695 | **152** | not in that fix |

The words critic measured about ninety and about a hundred and ten on capture 03: the first two rows. The chest is
the worst page in the game and the one meant for long reading. It was in neither the size audit nor the size fix
(`text_size_audit.md`: "MapView.uss ... Not audited"), and its credit line at 11 px is the smallest text there is.

**So the size fix does most of it, and does not finish it.** At 22 px a card's speech is 73 letters. Five rows stay
over seventy (in bold in the last column), and the chest is untouched.

**How to hold it.** USS has no unit that follows the letter (no `ch`, no `em`), so the column is a width in px tied
to the size by a variable: two more variables beside the four sizes, and `max-width` on the things that are read.
Add to `.frame-root` (after the four `--t-` lines that `Design/fixes/text_sizes.md` adds after line 26):

```css
    --t-col-body: 638px;      /* seventy letters of body text: 29 x the size */
    --t-col-quiet: 522px;     /* seventy letters of quiet text */
```

and at the end of Frame.uss:

```css
/* ---------------------------------------------------------------- the page: no line of reading over seventy letters
   (Design/fixes/page_and_strings.md, part 3). A width in px, because USS has no unit that follows the letter:
   29 x the size, which the size classes below keep in step. */
.frame-root .frame-float-line,
.frame-root .frame-float-result,
.frame-root .frame-float-sprinkle,
.frame-root .frame-turn,
.frame-root .frame-self-line,
.frame-root .frame-text,
.frame-root .frame-log-text,
.frame-root .frame-option-words {
    max-width: var(--t-col-body);
}

.frame-root .frame-float-small,
.frame-root .frame-float-who,
.frame-root .frame-option-rule,
.frame-root .frame-turn-note,
.frame-root .frame-p,
.frame-root .frame-ln-small,
.frame-root .word-text {
    max-width: var(--t-col-quiet);
}

/* the odds keep to the right edge when the words beside them are held narrower */
.frame-root .frame-option-odds {
    margin-left: auto;
    padding-left: 12px;
}
```

Notes for the builder:

- The windows keep their widths. Only the text is held, so a wide window has air on its right, as a page has a
  margin. The who-are-you window is the one where it shows most (893 px wide, text held to 638).
- `.frame-float-result` has 12 px of padding each side inside its width, so its text is 614 px: 67 letters.
- `.frame-option-odds` has `margin-left: 12px` today (Frame.uss:985); the rule above takes its place. That the
  layout engine takes `margin-left: auto` in a row I believe and did not see.
- **Do not put this in before the sizes.** With today's px sizes the two variables would be wrong (18 px speech
  wants 522, 14 px print 406). If it must go first, use those two numbers and change them with the sizes.
- **The chest** (`MapView.uss`): raise `.chest-word` (657) from 15 to `var(--t-body)`; `.map-chest-note` (494, 13
  px), `.chest-state` (674, 14), `.chest-tried` (681, 13), `.chest-credit` (687, 11) and the buttons under a note
  (788, 12) to `var(--t-quiet)`, with `max-width: var(--t-col-quiet)` on the first four. At 22 px a word's label is
  about 46 px with 7 beside it, so seventy letters are about 665 px, which is the column the chest already has:
  raising the size is the whole fix there, and `.chest-text` (638) needs only `max-width: 695px` against a wider
  chest later. The chest is not under `.frame-root`, so `.map-chest` (456) must define the six variables itself
  (the same six lines) and take the size classes of part 4.
- **A warning about `Design/fixes/Frame.proposed.uss`.** It was made from a Frame.uss of 1077 lines. Today's is
  1146: the tongues' styles were added at the end since (`.frame-turn`, `.frame-turn-piece`, `.frame-words`, lines
  1079 to 1146; the proposed file has none of them). **Copying the proposed file over today's would delete the
  turning lines' look.** Make the 46 swaps as edits, and add four the audit never saw: 1098 `.frame-turn-piece` 19
  to `var(--t-body)`; 1112 `.frame-turn-note` 13 to `var(--t-quiet)`; 1128 `.frame-words-w` 16 to `var(--t-body)`;
  1134 `.frame-float-sprinkle .frame-words-w` 18 to `var(--t-body)`.

Not in this fix: the words critic also asks that a card over 150 words be split where the speaker changes (con 19:
a card holds 130 words at the middle, 208 at the ninetieth in a hundred, 402 at most). That is the cards' own
shape and belongs with the content pass.

## 4. A text-size setting of three steps

**Three steps:** Normal, Large, Largest. They are the upper three of the four the art study planned
(`Design/research/next/art.md` C4: 18, 22, 28, 36). Its "Small" is body text at 18, under the audit's own body
size, so it is left out, which makes the critic's three.

| Step | quiet | body | name | title | Letters a line in a card (662 px) | In the left column (520 px) |
|---|---|---|---|---|---|---|
| Normal | 18 | 22 | 26 | 30 | 70 (held; 73 unheld) | 57 |
| Large | 22 | 28 | 32 | 36 | 57 | 45 |
| Largest | 28 | 36 | 40 | 44 | 44 | 35 |

Panel px at 1080p. The first row is the four sizes of `text_size_audit.md`. Body and quiet grow by 1.27 and 1.64, as
the art study has them; name and title grow less, because they are already large.

**What it scales.** The four variables, and so every one of the 46 font sizes of Frame.uss (and the four of the
tongues, part 3), the two column widths, and the chest once it takes the variables. It does not scale: the frame's
edges, icons, faces and the ship view (pictures); the windows' widths; the main menu (`MainMenu.uss` has its own px
sizes: 80 lines, not read closely); the chart's own labels. At Largest two things break that the stylesheet cannot
mend, and both are already named in `text_sizes.md`: the top bar (a 40 px line is taller than the bar's 54 px, so
the three `top: 68px` must follow the bar from code), and the left column at 35 letters (the art study widens it
from 29% to 30%). Ship Normal and Large first if those two are not done.

**How it meets the floor.** `text_sizes.md` holds text at 18 true px on a small screen with nine classes
(`t-floor-20` to `t-floor-36`) that reset the variables. A size setting done the same way would fight them: with
both on the root, `t-floor-28` would pull Large's 32 px name down to 28. So one mechanism serves both. The code
reckons each of the four sizes as the larger of the step's size and the floor, and puts four classes on the root,
one a size. **This takes the place of the nine `t-floor` classes and of `FloorClassFor` in text_sizes.md; its
variables, its `PanelScale()` and its two hooks in FrameRoot stand.** On a 1080p screen at Normal the result is the
same as that fix; at 720p it gives the same numbers as its `t-floor-28`.

The classes, at the end of Frame.uss and again at the end of MapView.uss (for the chest). No `.frame-root` before
them, so the chest's root can wear them too:

```css
/* ---------------------------------------------------------------- the sizes of text: one class a size, put on the
   root by TextSize.Apply (the player's step and the 18 px floor together). The column is 29 x the size. */
.tq-18 { --t-quiet: 18px; --t-col-quiet: 522px; }   .tb-18 { --t-body: 18px; --t-col-body: 522px; }   .tn-18 { --t-name: 18px; }   .tt-18 { --t-title: 18px; }
.tq-20 { --t-quiet: 20px; --t-col-quiet: 580px; }   .tb-20 { --t-body: 20px; --t-col-body: 580px; }   .tn-20 { --t-name: 20px; }   .tt-20 { --t-title: 20px; }
.tq-22 { --t-quiet: 22px; --t-col-quiet: 638px; }   .tb-22 { --t-body: 22px; --t-col-body: 638px; }   .tn-22 { --t-name: 22px; }   .tt-22 { --t-title: 22px; }
.tq-24 { --t-quiet: 24px; --t-col-quiet: 696px; }   .tb-24 { --t-body: 24px; --t-col-body: 696px; }   .tn-24 { --t-name: 24px; }   .tt-24 { --t-title: 24px; }
.tq-26 { --t-quiet: 26px; --t-col-quiet: 755px; }   .tb-26 { --t-body: 26px; --t-col-body: 755px; }   .tn-26 { --t-name: 26px; }   .tt-26 { --t-title: 26px; }
.tq-28 { --t-quiet: 28px; --t-col-quiet: 813px; }   .tb-28 { --t-body: 28px; --t-col-body: 813px; }   .tn-28 { --t-name: 28px; }   .tt-28 { --t-title: 28px; }
.tq-30 { --t-quiet: 30px; --t-col-quiet: 871px; }   .tb-30 { --t-body: 30px; --t-col-body: 871px; }   .tn-30 { --t-name: 30px; }   .tt-30 { --t-title: 30px; }
.tq-32 { --t-quiet: 32px; --t-col-quiet: 929px; }   .tb-32 { --t-body: 32px; --t-col-body: 929px; }   .tn-32 { --t-name: 32px; }   .tt-32 { --t-title: 32px; }
.tq-34 { --t-quiet: 34px; --t-col-quiet: 987px; }   .tb-34 { --t-body: 34px; --t-col-body: 987px; }   .tn-34 { --t-name: 34px; }   .tt-34 { --t-title: 34px; }
.tq-36 { --t-quiet: 36px; --t-col-quiet: 1045px; }   .tb-36 { --t-body: 36px; --t-col-body: 1045px; }   .tn-36 { --t-name: 36px; }   .tt-36 { --t-title: 36px; }
.tq-38 { --t-quiet: 38px; --t-col-quiet: 1103px; }   .tb-38 { --t-body: 38px; --t-col-body: 1103px; }   .tn-38 { --t-name: 38px; }   .tt-38 { --t-title: 38px; }
.tq-40 { --t-quiet: 40px; --t-col-quiet: 1161px; }   .tb-40 { --t-body: 40px; --t-col-body: 1161px; }   .tn-40 { --t-name: 40px; }   .tt-40 { --t-title: 40px; }
.tq-42 { --t-quiet: 42px; --t-col-quiet: 1219px; }   .tb-42 { --t-body: 42px; --t-col-body: 1219px; }   .tn-42 { --t-name: 42px; }   .tt-42 { --t-title: 42px; }
.tq-44 { --t-quiet: 44px; --t-col-quiet: 1277px; }   .tb-44 { --t-body: 44px; --t-col-body: 1277px; }   .tn-44 { --t-name: 44px; }   .tt-44 { --t-title: 44px; }
```

(56 rules on 14 lines. `.frame-root` keeps its own six defaults, so a root with no class reads as Normal at 1080p.)

**The code.** `Assets/Kapman/Code/Game/UI/TextSize.cs`, new, namespace `Kapman.Game`. Not compiled.

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE SIZE OF TEXT (7 Oct 2026, Design/fixes/page_and_strings.md part 4, on Design/fixes/text_sizes.md): three steps
    /// the player picks (Normal, Large, Largest), kept on the machine, and the floor of 18 true px on a small screen,
    /// as one sum. Four sizes come out (quiet, body, name, title, in panel px), each put on a root as a class
    /// (tq-22, tb-28, tn-32, tt-36) whose rule in the stylesheet sets the variable and its column.
    /// </summary>
    public static class TextSize
    {
        public const string Pref = "kapman.text.size";
        public const float LeastPx = 18f;
        public static readonly string[] Names = { "Normal", "Large", "Largest" };

        // quiet, body, name, title, in panel px at 1080p
        static readonly int[][] Steps = { new[] { 18, 22, 26, 30 }, new[] { 22, 28, 32, 36 }, new[] { 28, 36, 40, 44 } };
        static readonly string[] Family = { "tq-", "tb-", "tn-", "tt-" };
        const int Least = 18, Most = 44;

        static int step = -1;

        /// <summary>The player's step: 0 Normal, 1 Large, 2 Largest. Kept in the settings (PlayerPrefs).</summary>
        public static int Step
        {
            get
            {
                if (step < 0)
                    step = Mathf.Clamp(PlayerPrefs.GetInt(Pref, 0), 0, Steps.Length - 1);
                return step;
            }
            set
            {
                value = Mathf.Clamp(value, 0, Steps.Length - 1);
                if (value == Step)
                    return;
                step = value;
                PlayerPrefs.SetInt(Pref, value);
                PlayerPrefs.Save();
            }
        }

        /// <summary>The four sizes for a step and a panel scale (true px for one panel px): each the larger of the
        /// step's size and what makes 18 true px, made even, never over 44. Pure, so a test holds it.</summary>
        public static int[] Px(int step, float scale)
        {
            var s = Steps[Mathf.Clamp(step, 0, Steps.Length - 1)];
            int floor = scale > 0f && scale < 1f ? Mathf.CeilToInt(LeastPx / scale - 0.01f) : Least;
            var px = new int[4];
            for (int i = 0; i < 4; i++)
            {
                int v = Mathf.Max(s[i], floor);
                v += v & 1;
                px[i] = Mathf.Clamp(v, Least, Most);
            }
            return px;
        }

        /// <summary>Puts the four size classes on a root (the frame's, the chest's), taking the old ones off.</summary>
        public static void Apply(VisualElement root, float scale)
        {
            if (root == null)
                return;
            var px = Px(Step, scale);
            for (int f = 0; f < 4; f++)
            {
                for (int v = Least; v <= Most; v += 2)
                    root.RemoveFromClassList(Family[f] + v);
                root.AddToClassList(Family[f] + px[f]);
            }
        }

        // Enter Play Mode without a domain reload: read the setting again
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay() => step = -1;
    }
}
```

`Assets/Kapman/Code/Game/UI/Frame/FrameRoot.TextFloor.cs`, in place of the draft of that name in `text_sizes.md`
(the two hooks that fix adds to `FrameRoot.cs`, in `Build()` and as the first line of `Update()`, stay as written
there, less its line that clears `floorClass`):

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    public partial class FrameRoot
    {
        int sizedW, sizedH, sizedStep = -1;

        /// <summary>The panel's scale as it is now: screen px over panel px.</summary>
        float PanelScale()
        {
            var tree = root?.panel?.visualTree;
            if (tree == null)
                return 1f;
            float h = tree.layout.height;
            return float.IsNaN(h) || h < 1f ? 1f : Screen.height / h;
        }

        /// <summary>The four sizes on the root, by the player's step and the screen's floor. Cheap: three compares a
        /// frame, and work only when the screen or the step changed.</summary>
        void TextFloor(bool force = false)
        {
            if (root == null || root.panel == null)
                return;
            if (!force && Screen.width == sizedW && Screen.height == sizedH && TextSize.Step == sizedStep)
                return;
            if (float.IsNaN(root.panel.visualTree.layout.height))
                return;                                                   // (not laid out yet: the next frame)
            sizedW = Screen.width;
            sizedH = Screen.height;
            sizedStep = TextSize.Step;
            TextSize.Apply(root, PanelScale());
        }
    }
}
```

The chest takes the same call when it opens: in `Code/Game/Map/MapTable.cs`, in `OpenChest` (line 290) and in
`ToggleChest` (line 298) where it opens, `TextSize.Apply(chest, scale)` with the same scale (the chest's own panel:
whether it is the frame's document or the chart's I did not check).

**Where settings live.** Today there is no settings screen. Two things are kept on the machine in PlayerPrefs:
the windows' arrangement (`kapman.frame.windows`, `FrameRoot.cs:44`) and the palette (`MapPalette.cs:61, 82`). The
size is a third (`kapman.text.size`), on the machine and not in a saved game: it belongs to the eyes, not to the
merchant. The menu is `Code/Game/UI/MainMenu.cs`: a title list (lines 105-117) and a pause list (138-150), each a
row of `Add(label, action)`. Add a Settings row to both and one small list:

```csharp
            // MainMenu.cs, TitleList, before  Add("Quit", Quit);  (line 116)
            Add("Settings", () => SettingsList(TitleList));
            // MainMenu.cs, PauseList, before  Add("New game", ...);  (line 147)
            Add("Settings", () => SettingsList(PauseList));
```

```csharp
        // ---------------------------------------------------------------- settings (kept on this machine, not in a saved game)

        void SettingsList(Action from)
        {
            Fill("Settings", "", "");
            Add("Text: " + TextSize.Names[TextSize.Step], () =>
            {
                TextSize.Step = (TextSize.Step + 1) % TextSize.Names.Length;
                SettingsList(from);                                       // (the frame behind the menu changes at once)
            });
            Add("Back", from);
            back = from;
        }
```

The same list is where the other settings already promised go: the sound slider (`Design/fixes/chart_sound.md`,
line 387) and the switches for the clock dropping back by itself (ruling 26).

**A test** (EditMode, no scene):

```csharp
        [Test]
        public void TextSize_StepsAndFloor()
        {
            CollectionAssert.AreEqual(new[] { 18, 22, 26, 30 }, TextSize.Px(0, 1f));
            CollectionAssert.AreEqual(new[] { 22, 28, 32, 36 }, TextSize.Px(1, 1f));
            CollectionAssert.AreEqual(new[] { 28, 36, 40, 44 }, TextSize.Px(2, 2f));
            CollectionAssert.AreEqual(new[] { 28, 28, 28, 30 }, TextSize.Px(0, 720f / 1080f));   // text_sizes.md's t-floor-28
            CollectionAssert.AreEqual(new[] { 22, 22, 26, 30 }, TextSize.Px(0, 900f / 1080f));   // its t-floor-22
            CollectionAssert.AreEqual(new[] { 28, 28, 32, 36 }, TextSize.Px(1, 720f / 1080f));   // Large keeps its name and title
            for (int step = 0; step < 3; step++)
                for (float s = 0.5f; s < 1f; s += 0.01f)
                    Assert.GreaterOrEqual(TextSize.Px(step, s)[0] * s, 17.99f, "step " + step + ", scale " + s);
        }
```

Not done here: a Steam Deck should start at Large (art.md, line 625); the sample card beside the setting that the
art study asks for (the frame behind the menu stands in for it); Largest's two breaks named above.

## 5. The chest shows a text plainly first

**Today** (`Code/Game/Map/ChestPanel.cs:348-423`). A text opens in its own tongue (`bool english` at line 24 starts
false). A switch of two buttons, the tongue's name and "English" (lines 371-380), turns it to the old translation:
Beowulf in the English of 1892, the Edda in that of 1923. Above it stands a note of where it came from; below, at
11 px, "Translated by John Lesslie Hall (1892)." and the credit. So a reader meets Old English first, then "Waxed
'neath the welkin", and never the voice the cards speak in.

**The size of it** (my counts over `Game/Data/texts_793.json`):

- 267 texts. 232 have an English, 45,592 words of it. 35 have none ("no free English": Chest.cs:166).
- By translator: Bellows 1923 (97 texts), Hall's Beowulf 1892 (43 parts, **30,106 words**, two thirds of all the
  English in the chest), Vigfusson and Powell 1883 (43), Thorpe 1842 (31), eight others (one to three each).
- 57 texts have three or more words of the old kind (thee, thou, hath, 'neath, welkin and the like: a list of 40),
  and they hold 33,508 of the 45,592 words.
- Three texts already carry the game's own English, marked `ownTranslation` (`aelfric_merchant`, `saxon_vow`,
  `alcuin_793`). So the way is already open; it has only been walked three times.
- **For the first release: 24 texts, 968 words.** That is what can be had on its own, or as a riddle's text, at the
  twelve ports in 793 to 805 by the chest's own tables (`acquire` in texts_793.json and riddles_793.json). The
  cards hand out more (99 kinds of text and 88 of riddle in all card files); which of those a first-release card
  gives I did not sift.

**The data.** Each text gains two fields in `texts_793.json`:

```json
   "plain": "We have heard of the Spear-Danes in days long gone, of the kings of that people and their glory ...",
   "plainBy": "the game's own, from the Old English"
```

`plain` is the game's own plain English, in the voice of the cards, with no word a reader must look up. Like all
the game's words it is a draft for the user to write over. It is the game's own work (made from the original, or
reworded from a translation that is free to use), so it ships; the credits audit should add one line for it. For
the three `ownTranslation` texts `plain` is their `translation` and there is no older English to show.

**The page**, top to bottom:

1. The title.
2. One quiet line of who gave it: "From Ravn, at Dorestat, three days ago." The form and the tongue move to 5.
3. **The words, plainly**: `plain`, at body size, held to seventy letters (part 3).
4. Under the words a row of quiet presses, each of which changes what stands at 3:
   **"In Old English"** (the tongue's own name: the original, and for runes the rune reading under it);
   **"The English of 1892"** (the old translation, named by its year; no press when there is none);
   **"Plainly"** (back).
5. **"About this writing"**, a fourth press that opens a note under the row and shuts it again: what it is written
   on and in what tongue, when it was written down ("Told of the time, written down later"), who put it into
   English and when, the credit line, and where it is found, which today is a tooltip nobody will find
   (ChestPanel.cs:421).

When the merchant cannot read the tongue (Latin, Irish: `Chest.CanRead`, Chest.cs:145), the page shows what the eye
sees: the original, and the line of 2.4 ("You cannot read Latin. A monk would read it to you ..."). Once it has
been read out, the plain words stand first. When a text has no `plain` yet: the old English first if there is one,
else the original. A riddle's clue marks (a click on a word) work on whichever is up.

**The code** (drafts, not compiled):

`Code/Game/Chest/ChestData.cs:14` gains `plain, plainBy` among the strings of `TextDef`, and the loader at line 190
gains `plain = Json.Str(o, "plain", null), plainBy = Json.Str(o, "plainBy", null),`.

`Code/Game/Chest/Chest.cs:151` and `:165`: a text with a `plain` and no old translation is still read. Both tests
`string.IsNullOrEmpty(t.translation)` become `string.IsNullOrEmpty(t.translation) && string.IsNullOrEmpty(t.plain)`.

`Code/Game/Map/ChestPanel.cs:24`, `bool english;` becomes:

```csharp
        string face = "plain";                                        // what a text shows: plain (the game's own English), old (the old translation), own (its own tongue)
        bool about;                                                   // the note about the writing is open
```

Line 100 (`bool eng = english && Chest.CanRead(item);`, for the sailing directions) becomes
`bool eng = face != "own" && Chest.CanRead(item);`. In `TextView`, lines 359-390 (from `double ago` to
`int baseIndex`) become:

```csharp
            double ago = KapmanGame.Current != null ? KapmanGame.Current.Day - item.got : 0.0;
            string when = ago < 1.0 ? "today" : ago < 2.0 ? "yesterday" : $"{ago:0} days ago";
            if (!string.IsNullOrEmpty(item.from))
                into.Add(Note($"From {item.from}{(item.at != null ? ", at " + (GameData.Port(item.at)?.name ?? item.at) : "")}, {when}."));
            // which words are up: plainly first, wherever the merchant can read it at all
            string why = Chest.WhyNotRead(item);
            bool canRead = why == null;
            bool hasPlain = !string.IsNullOrEmpty(t.plain);
            bool hasOld = !string.IsNullOrEmpty(t.translation) && t.translation != t.plain;
            string show = !canRead || face == "own" ? "own" : face == "old" && hasOld ? "old" : hasPlain ? "plain" : hasOld ? "old" : "own";
            bool showEnglish = show != "own";
            if (!canRead)
                into.Add(Note(why));
            string words = show == "plain" ? t.plain : show == "old" ? t.translation : t.original;
            int baseIndex = show == "plain" ? 200000 : show == "old" ? 100000 : 0;       // (a riddle's marks are kept by which words they were made on)
```

Lines 391-417 (the words, the marking, the runes) stand as they are. The credit at lines 419-422 becomes the row
of presses and the note:

```csharp
            var bar = new VisualElement();
            bar.AddToClassList("chest-switch");
            void Press(string label, bool on, System.Action act)
            {
                var b = new Button(() => { act(); Redraw(); }) { text = label };
                b.AddToClassList("map-button");
                b.EnableInClassList("on", on);
                bar.Add(b);
            }
            if (canRead && hasPlain)
                Press("Plainly", show == "plain", () => face = "plain");
            if (canRead && hasOld)
                Press(t.translated > 0 ? "The English of " + t.translated : "An older English", show == "old", () => face = "old");
            Press("In " + t.language, show == "own", () => face = "own");
            Press("About this writing", about, () => about = !about);
            into.Add(bar);
            if (about)
            {
                into.Add(Note($"On a {t.FormName}, in {t.language}." + (string.IsNullOrEmpty(t.writtenDown) ? "" : " Written down: " + t.writtenDown + ".") +
                              (t.toldLater ? " Told of the time, written down later." : "")));
                if (hasOld && !string.IsNullOrEmpty(t.translator))
                    into.Add(Note($"Put into English by {t.translator}{(t.translated > 0 ? ", " + t.translated : "")}."));
                if (hasPlain && !string.IsNullOrEmpty(t.plainBy))
                    into.Add(Note("The plain English is " + t.plainBy + "."));
                into.Add(Note(t.credit));
                if (!string.IsNullOrEmpty(t.where))
                    into.Add(Note(t.where));
            }
```

One thing to look at when it runs: `t.where` is a scholar's note and for some texts names the makers' own files
("scratchpad/cpb1.txt lines 15679-15683" in `t-gest-01`; a search of texts_793.json for "scratchpad/" finds three lines). It is a tooltip today. Cut
the file names out of the data before it is shown as a line, or leave `where` out of the note: the test of part 1
("a file name") will say which.

**A test:** every text that can be had at the twelve ports in 793 to 805 has a `plain`; no `plain` trips
`PageWords.Fault`; none holds a word of the old list.

**Who writes the 24, and Beowulf.** The standing rule is that the user writes the game's words and the game drafts.
The 24 are an evening's drafting. Beowulf is 30,106 words; if a crewman is to say it at the oar (the words critic's
fourth change) it is `plain` he says, so that change waits on this field, and a plain Beowulf is its largest cost.

## 6. Captives: off the market row, onto a card

The ruling (DECISIONS.md, line 65, item 4 of the book's questions): "The slave trade: in the world and in the
cards. The player may carry or refuse; word of it travels as news does, and the heirs bear it." The words critic's
con 13: "no row. A person is bought or sold only on a card or in a talk with the dealer, as so many men and so many
women."

### Where they stand as a row among goods today

| Where | What it does |
|---|---|
| Game/Data/goods_793.json:188 | The good `captives`: name "Captives", unit "person", 60 kg, value 48 d, with the note "Shown as a lot in the market like any other." |
| Code/Game/Trade/Market.cs:93-103, :31 | Every market holds every traded good; one is on offer when its stock is 1 or more. |
| Panels/TradePanel.cs:29-33 (`Shown`) | A row for every good on offer, wanted here, **or aboard**. So with captives in the hold, every market shows the row, at Dorestat too. |
| Panels/TradePanel.cs:105-116 | The row: "Captives. They ask 48 d, 12 here. They pay 41 d. By the person." with a Buy and a Sell button, and the tooltip "6 aboard, cost you 48 d each." |
| Panels/TradePanel.cs:69-74 | At sea: the hold's list with the word on each ("They say Ripa pays well for captives."). |
| Panels/HoldPanel.cs:34-43 | "6 persons of Captives. Cost you 288 d." and "360 kg". |
| Code/Game/Trade/Trade.cs:186-194 (`Units`) | "6 persons of captives", used by the deal's line and by a card's fact (`Code/Game/Encounters/GameEffects.cs:82, :88`: "6 persons of captives taken aboard." / "... gone from the hold."). |
| Code/Game/Trade/Trade.cs:138 | The sale's line ends "12 d more than they cost." The critic's sentence. |
| Code/Game/Trade/Trade.cs:103-104, :131-132 | A deed is recorded (`bought_captives`, `sold_captives`): this is right, and is how word of it travels. Keep. |
| Code/Game/UI/MarketPanel.cs:45 | The older market's list of kinds has "Captives". Not drawn under the frame. |

**Where a first-release player meets the row: Ripa.** `Game/Data/raiders_793.json:19` and `:31` name Ripa among the
markets where two peoples' bands sell; `Code/Game/Sim/RaiderBrain.cs:364-371` gives a band 6 to 20 captives from a
raid on a monastery, a village or a market, and `:322-331` sells its whole hold into the market it comes to. Then
Ripa's stock is over 1 and the row is there with its Buy button. None of the other eleven ports makes or wants them
(goods_793.json:188: made in Wendland, Ireland, Prussia, Curonia, Ladoga, Northumbria, Dal Riata). I did not run a
game to see in which month of 793 to 805 a band first sells at Ripa.

### How they should be met

- **The world keeps them.** The markets' stock, the raiders' sales, the prices, the laws (`trade_rules.json:36`:
  no export from the Frankish and English lands) and the deeds stay as they are. One economy.
- **The player's windows do not list them.** No row in Trade, no Buy and no Sell, no weight, no "cost you", no
  "more than they cost", no word-on-the-item line.
- **A dealer stands where they are sold.** Where captives are on offer, the People here window has one more
  person, a dealer in people, at the strand or the market's edge. The first time she moors at such a place the
  card comes of itself, once, as what is seen from the boat (as `harbour.strand_of_captives.duiblinn` does at
  Duiblinn). After that it is met by going to him.
- **It is a card, not a talk.** It asks for an answer and holds the clock (ruling 5). Its choices, in the engine's
  own verbs (as `chain_silver_road_793.json:530-537` uses them); the words are the user's:

| Choice | Cost | Effects | Note |
|---|---|---|---|
| Walk on | none | none | Always there. The one the card takes by itself after a week (ruling 12). |
| Tell him you carry no people | none | a flag (`me_carries_no_people`); the dealer cooler toward you; a deed the church hears of | A refusal that is remembered and costs something north of the Eider. Dealers stop calling to you. |
| Buy, as so many men and so many women | silver, each at what this market asks | `goods captives n`, `deed bought_captives` | Two or three sizes of lot, written out ("two men"), not a counter. Shut, with the law's own words, where the law forbids it. |
| Buy one free | the price of one | `deed freed_captive`; the person goes ashore | Only promise a passage home when `passenger` does something (GameEffects.cs:214 answers nothing today). |
| With captives aboard: sell him the lot | `goods captives n` | silver at what this market pays, `deed sold_captives` | The written outcome says what was done, not what was gained. |
| With captives aboard: set them ashore free | `goods captives n` | `deed freed_captives`; the crew's heart falls (their share is gone) | |

- **The price is the market's.** A card's effects take fixed numbers today ("silver +1700"). So that the world's
  stock and price move when the player buys or sells on the card, give the engine two verbs, `buy <good> <n>` and
  `sell <good> <n>`, which make the deal through `Trade.Buy` and `Trade.Sell` at the market where she lies (the
  same toll, law, deed and stock) and say nothing themselves: the card's written outcome speaks. About twenty
  lines in `Code/Core/Encounters/Effects.cs` (the switch at lines 114-132) and `GameEffects.cs`. Without it the
  card has a fixed price and the market does not know the player bought: a small lie, acceptable for a first card.
- **Aboard they are people.** The Hold window does not list them among goods. One line, after the goods: "Six
  captives aboard." with the small line "They take the room of cargo." Whether they eat (the good's note says each
  needs food and water like a crewman) I did not find in `Voyage`; the line must not say so unless they do.
- **Word of it.** The deeds already feed standing and news. That the heirs bear it rests on standing passing to the
  heir, which I did not check.
- **A question for the lead, on the pay cap** (ruling 14: no card pays over 60 d, no chain over 240 d). Six sold at
  the market's price is about 250 d. It is the sale of a cargo, not a card's gift, and capping it would make the
  world's price a lie. But `chain_silver_road_793.json:531` and `:537` pay 1,700 d and 1,450 d for six, thirty
  times the market's price: the pay-cap pass should bring those to the market's.
- The words critic's con 14 (only the captive who is like you may speak; one card where the one who calls cannot be
  bought) is writing, and the user's.

### The code (drafts, not compiled)

`EncounterGameVars.IsCaptive` (Code/Game/Encounters/EncounterGameVars.cs:26, namespace `Kapman.Game`) already says
which good is a person.

`Panels/TradePanel.cs:29-33`:

```csharp
        static IEnumerable<MarketGood> Shown(Market m) =>
            m.Goods.Where(g => !EncounterGameVars.IsCaptive(g.good))      // (people are met on a card, never as a row: ruling 4 of 7 Oct)
             .Where(g => g.OnOffer || g.wanted > 0f || Trade.Hold.Count(g.good.id) > 0)
             .OrderByDescending(g => Trade.Hold.Count(g.good.id) > 0)
             .ThenByDescending(g => g.OnOffer)
             .ThenBy(g => g.good.name);
```

`Panels/TradePanel.cs:69` (the hold's list at sea), as the loop's first lines:

```csharp
                    if (EncounterGameVars.IsCaptive(good))
                        continue;
```

`Panels/TradePanel.cs:130-131` (`DealOne`), after the test for no market: a person is not dealt in by a button, even if
a test or a key calls it:

```csharp
            if (EncounterGameVars.IsCaptive(GameData.Good(good)))
                return null;
```

`Panels/HoldPanel.cs:34-43`, the loop skips them and a line follows it:

```csharp
            int kinds = 0, people = 0;
            foreach (var (good, count) in hold.Contents)
            {
                if (EncounterGameVars.IsCaptive(good))
                {
                    people += count;
                    continue;
                }
                kinds++;
                double cost = hold.CostEach(good.id) * count;
                string small = cost > 0.5 ? $"Cost you {cost:0} d." : "Your own from the start.";
                body.Add(FrameUi.Ln(FrameGame.Units(good, count), small, FrameUi.Num($"{count * good.kg:0} kg")));
            }
            if (people > 0)
                body.Add(FrameUi.Ln(people == 1 ? "One captive aboard" : people + " captives aboard", "They take the room of cargo."));
            if (kinds == 0 && people == 0)
                body.Add(FrameUi.P("She is empty."));
```

`Code/Game/Trade/Trade.cs:186` (`Units`), as its first lines, so that no line anywhere says "persons of":

```csharp
            if (g != null && g.cat == "Captives")
                return n == 1 ? "one captive" : n + " captives";
```

`Code/Game/Trade/Trade.cs:138`: `(d.cost > 0 ? ...` becomes `(d.cost > 0 && good.cat != "Captives" ? ...`.

`Code/Game/Encounters/GameEffects.cs:82, :88`: for a captive, "are brought aboard." and "leave the boat." in place
of "taken aboard." and "gone from the hold."

`Game/Data/goods_793.json:188`, the note: "People: each needs food and water like a crewman. Never a row in a
market window: met on a card or with the dealer (Design/fixes/page_and_strings.md, part 6)."

The dealer in People here (`Panels/PeoplePanel.cs`, after the people of the place, line 55): a row "A dealer in
people" with the small line of where he stands and a Talk button that opens the card, shown when
`Markets.Of(here, game.Year, game.Day).Get("captives")?.OnOffer == true` or captives are aboard. The call that
opens a card by its id from outside a card I did not trace (inside one it is `world.state.Enqueue(id, true)`,
`Code/Core/Encounters/EncounterSession.cs:239`).

**A test:** a market given a stock of 10 captives, a hold given 2: `TradePanel` has no row named Captives and
`DealOne("captives", true)` returns null with the purse as it was; the Hold window has no row that holds both
"aptive" and "kg"; `Trade.Units` of 6 is "6 captives"; the People here window has the dealer.

## The order of work

1. **The strings (part 2).** Data: 22 lines of `wording_793.json`, 7 of `self_793.json`, 1 of `doccinga.json`.
   Code: about 70 lines in 22 files, each a string or a line to take out. Half a day with the tests run. It
   depends on nothing. Mind the others' hands: `FrameRoot.Rack.cs`, `PersonTalk.cs`, `FrameRoot.cs`, `FrameSave.cs`
   and `FrameRoot.Floats.cs` were all saved between 12:14 and 12:28 today. The 338 bracketed choices of
   `writing.json` (2.4) go to the content pass, which has that file open; the strip in code covers them meanwhile.
2. **The test (part 1)**, with it or straight after: it finds what I missed.
3. **The locks (2.3):** the hook in Cond.cs and its filling, two hours; written locks after, with the content pass.
4. **The sizes, then the column, then the setting (parts 3 and 4):** `text_sizes.md` as edits (not the copy: see
   the warning), the four tongues' sizes, the two column variables and the rules, `TextSize`, the menu's list, the
   chest's sizes. Half a day, and it needs a look in Unity at Normal and at Large.
5. **The chest (part 5):** two hours of code; then 24 plain texts for the first release.
6. **Captives (part 6):** the rows out and the hold's line, an hour, at once; the dealer and the card when the card
   is written.

By the critics' own reckoning: the words critic gives the whole of "Mend the page" half a star; the newcomer gives
the swept screen plus 3 of 100, of which the strings here are the larger half and the fixed first-hour layout (not
in this file) the rest.

## What I could not check

- Nothing was run or seen. No draft here was compiled. Every letters-a-line figure is a sum from the stylesheet
  and the font's widths at 1920 x 1080.
- Line numbers moved under me: four of the frame's files were saved while I read. Search by the words.
- Whether the chest opens in the frame's document or the chart's (it decides where the six variables and the size
  classes must be defined). Whether `margin-left: auto` and `var()` in `max-width` behave in this Unity.
- Which culture each of the eight `port.news` lines of `wording_793.json` belongs to (all eight change alike).
- How many locks the first-release cards show, and their words: I read the code that makes them, not each card.
- The word table that gives "easy" as a feeling (2.4): I found where it is printed, not where it is written.
- The cards' own prose (`writing.json`, 13,190 texts) was put through the test's patterns and not read: the 338
  choices with a bracket (part 1, 2.4) are what that found. The content pass has the cards.
- The older panels (`MarketPanel.cs`, `TextPanel.cs`, `EncounterPanel.cs`, `Alerts.cs`, `Notices.cs`,
  `HoverCards.cs`) and the chart's own port card (`MapView.cs`, bar the one line in 2.4) were not swept.
- Whether a raiders' band sells captives at Ripa in a new game's first years; whether captives aboard eat; whether
  standing passes to an heir.
- Which of the 99 texts and 88 riddles the cards hand out belong to the first release (part 5's 24 are the chest's
  own tables only).
- The first hour's fixed screen, the half row cut by a window's edge and the three names for a penny are in the
  critics' same points and are not in this file.
