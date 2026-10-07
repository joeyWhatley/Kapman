<!-- 7 Oct 2026. Written by the first of the wiring agents after reading the code and the data, and after three full
compile-and-test runs in Unity and one play of the chart. Everything under "1. What is laid down" is built, compiled, tested and committed.
Everything under "3. The families" is a plan: exact names and signatures for eight engineers who write without Unity.
(D) = a number or a rule I judged. -->

# The wiring plan: making the game read what the writers wrote

**The user, 7 Oct:** give "another 10 agents wiring it all together". The writers filled
`Assets/Kapman/Game/Data` with places, ships, town acts, works, games, unlocks, the winter, the ages, sayings,
settlements, a gazetteer and 43 chains. Only the cards and their words are live. This page is how the rest becomes
live: one foundation (built), eight loaders (planned here, written next, without Unity), then six HUD steps.

## 0. The data as it stands (measured on 7 Oct, 07:07, by `ContentReportTests`)

The report is written again on every EditMode run to `Logs/content_report.txt`. Read it before you start. The
writers are still at work, so the counts move: twenty minutes later there were 1,095 cards (still 0 faults) and
1,163 keys waiting for their words.

| What | State |
|---|---|
| Files | 28 at the root, 53 in `encounters`, 50 in `places`, 9 in `story`, 1 in `story/settlements`, 49 in `writing`: **all parse**. `story/wars`, `story/hinges`, `starts_793`, `endings_793`, `storyteller_793`, `war_templates_793` and `routes_*` are **not there yet** |
| Cards | 1,031 cards from 51 files load with **0 faults**; 43 chains among them. 493 writing keys have no entry in `writing.json` yet (430 of them are in `writing/rhine_frisia.json` and `writing/baltic_east.json`, waiting for `Bake/content/merge_writing.py`) |
| The winter's 40 cards (`story/winter_793.json` "cards") | Load beside the others with **0 new faults**. Their 389 words are in the same file under `writing.entries` and are in `writing.json` for none of them |
| `story/town_actions_793.json` | 53 acts. All 53 conditions, all 53 costs and all 41 effect lists are understood by the cards' engine as they are |
| `story/unlocks_793.json` | 455 rows. All 455 conditions and all 154 costs are understood |
| `story/settlements/rhine_frisia.json` | 10 towns. All 142 conditions, 50 costs and 58 effect lists are understood. Its `world` lists are a second language (SCHEMAS.md rule 8) that nothing reads yet |
| `story/winter_793.json` | 74 of 84 conditions are conditions. The other ten are plain prose for a person to read (`reckoning.years.*.when`, `reckoning.also[].when`); two costs are prose |
| `story/sayings_793.json` | 530 sayings. **`when` is prose** ("coming into a strange hall"); the condition, where there is one, is **`cond`** ("fog > 0.6"): 66 rows have one and all 66 parse |
| `gazetteer_793.json` | 5.2 MB: 15,447 places (tier 1: 50, tier 2: 2,915, tier 3: 12,482) and 3,239 stones. Read in 9 ms, parsed in 136 ms, about 40 MB while the parsed tree is held: **turn it into your own small records and let the tree go** |
| Places files | 50 files parse in 124 ms together. Every `when` in them is a condition |

No story file uses `flag <name> +n` yet, and none uses a verb or a variable the engine does not know.

## 1. What is laid down (built, compiled, tested, committed)

EditMode: **299 total, 297 passed, 0 failed, 2 skipped** (276 before; 23 new). The chart play test passes
(`ChartGameTests`: she lies at Dorestad, sails to Traiectum, the port's menu opens; the screenshots were looked at).

### 1.1 `Assets/Kapman/Code/Game/Content/ContentFiles.cs`: where the data is, and what a bad file costs

Namespace `Kapman.Game`, assembly `Kapman.Game`. Nothing in it throws, and nothing logs an error.

```csharp
public static class ContentFiles
{
    public const string ProjectFolder = "Kapman/Game/Data";    // under Assets: the editor and the tests
    public const string BuiltFolder   = "Data";                // under StreamingAssets: a built game
    public static int Version { get; }                         // counts up on Reset and SetRoot: "read again"
    public static string DefaultRoot { get; }                  // forward slashes, no slash at the end
    public static string Root { get; }
    public static bool RootIsDefault { get; }
    public static void SetRoot(string folder);                 // a test's own folder; null puts it back. Calls Reset
    public static void Reset();                                // forget the warnings, Version++ (also on every Play)
    public static string PathOf(string relativePath);          // full path; null (one warning) if it leaves the folder
    public static bool Exists(string relativePath);            // no warning either way
    public static bool FolderExists(string folder);            // no warning either way
    public static string Read(string relativePath);            // text; null + one warning if missing or unreadable
    public static string ReadDefault(string relativePath);     // the same, always from DefaultRoot (the shared tables)
    public static string[] List(string folder, string pattern = "*.json");   // "places/ahus.json", ordinal order
    public static object Parse(string relativePath);           // Json tree; null + one warning
    public static Dictionary<string, object> ParseObject(string relativePath);   // null + one warning if not { }
    public static object ParseText(string json, string name);  // null + one warning
    public static IEnumerable<KeyValuePair<string, Dictionary<string, object>>> ParseAll(string folder, string pattern = "*.json");
    public static void Warn(string key, string message);       // ONE "Kapman content: ..." warning per key per run
    public static IReadOnlyList<string> Warnings { get; }
}

public sealed class ContentCache<T> where T : class, new()     // what a loader keeps
{
    public ContentCache(string name, Func<T> load);
    public T Value { get; }       // loads on first use, and again when ContentFiles.Version has moved; never null
    public bool Loaded { get; }
    public void Clear();
}

public static class ContentJson                                // forgiving readers, beside Json.Str/Num/Bool/Obj/Arr
{
    public static IEnumerable<Dictionary<string, object>> Rows(object o, string key);   // the objects of a list
    public static IEnumerable<KeyValuePair<string, object>> Pairs(object o, string key); // an object used as a map
    public static string[] Strings(object o, string key);      // a list of text, or one text; never null
    public static bool Has(object o, string key);
    public static double Num(object o, string key, double fallback = 0.0);   // a number, or text that is one
    public static int Int(object o, string key, int fallback = 0);
    public static string Text(object o, string key, string fallback = "");   // text; a number or bool as text
    public static bool Range(object o, string key, out double low, out double high);   // [low, high] or one number
    public static bool Date(object o, string key, out int year, out int month, out int day);
    public static bool Date(string text, out int year, out int month, out int day);
    public static int DayKey(int year, int month, int day);    // year*400 + month*32 + day, as ChronicleEntry.Key
    public static bool InYears(int from, int to, int year);    // 0 = open, as in ports_793.json
}
```

Rules that are tested: paths are relative, with forward slashes (a backslash is taken too); `List` never returns
`.meta`, hidden, `.tmp`, `.part` or `~` files and never looks into subfolders; a folder that is not there lists
nothing with one warning (ask `FolderExists` first for a folder that may not be written yet); a file a writer has
open is still read; a half-written file gives null and one warning however often it is asked; `Date` reads
`"793-06-21"`, `793`, `"863"` (month and day 0) and `"03-25"` (year 0).

### 1.2 The three doors to the game

**`Content/ContentFlags.cs`: the flags.** Everything the content remembers as a number is a flag of the encounter
layer, which is already saved. `Core/Encounters/EncounterState.cs` got `Flags(day)` (every flag that holds, in
name order), `FlagVersion` (counts up on every change) and `AddFlag(name, by, day)`.

```csharp
public static class ContentFlags
{
    public static void Use(EncounterState state, Func<double> day = null);   // a test's own flags; null: the game's
    public static EncounterState State { get; }     // null: no game is running
    public static double Day { get; }
    public static bool Ready { get; }
    public static int Version { get; }              // changes when any flag may have changed: remake your list
    public static double Value(string name);        // 0: never set, lapsed, or no game
    public static bool Has(string name);            // what "flag.<name>" means in a condition
    public static bool Set(string name, double value = 1.0, double days = 0.0);   // false: no game, or a bad name
    public static bool Add(string name, double by);
    public static bool Clear(string name);
    public static List<KeyValuePair<string, double>> All();
    public static List<KeyValuePair<string, double>> WithPrefix(string prefix);   // "unlock_", "ch_black_stones_"
    public static bool ValidName(string name);      // letters, digits and underscores
}
```

**`Content/ContentRules.cs`: conditions, costs and effects.** The story files write them in the grammar of
`Cond.cs` and the verbs of `Effects.cs`. This parses them when a file loads and runs them on the live game.

```csharp
public sealed class ContentResult { public bool ok; public string why = ""; public readonly List<string> facts; public string Text { get; } }

public static class ContentRules
{
    public static void Use(EncounterDirector director);        // a test's own director; null: the live game's
    public static EncounterDirector Director { get; }          // null: no game (or the cards have faults)
    public static bool Ready { get; }
    public static WordTable Words { get; }                     // the game's words; read from the tables with no game
    public static Cond Never { get; }
    public static Cond ParseWhen(string text, string where);   // "" = always. A fault: one warning, and NEVER holds
    public static bool TryParseWhen(string text, out Cond cond);   // no warning: "is this text a condition at all?"
    public static bool ParseEffects(object node, string where, out List<Effect> effects);   // false if any did not parse
    public static bool ParseCost(object node, string where, out List<Effect> cost);   // "", "none" = free. FALSE for prose
    public static IVars Vars { get; }
    public static bool Holds(Cond when);                       // null holds
    public static string Why(Cond when);                       // the rule in plain words
    public static string Lacks(List<Effect> cost);             // "You need 24 d of silver." or null
    public static ContentResult Run(string id, Cond when, List<Effect> cost, List<Effect> effects);
}
```

`Run` checks the condition and the cost, pays, runs the effects through `EncounterDirector.RunEffects` (new), and
gives back the facts ("Paid 8 d.", "The crew's heart rises."). A role an effect names (`npc shipwright feeling +2`)
is **the holder of that role at the port she lies at**, the same person a card there would bring. At sea nobody is
there and such an effect does nothing. **A cost that `ParseCost` does not understand must keep the thing shut**:
never give it for nothing. `where` is the row's name for the warning: `"story/town_actions_793.json act repair"`.

Also new on `EncounterDirector`: `VarsNow()`, `RunEffects(id, cost, effects)`, `CanOffer(cardId)` and
`Offer(cardId)` (the player asks for a card at a site: it goes first in line if its `when` holds and it is past its
cool-down). New in `Effects.cs`: `flag <name> +n` and `flag <name> -n` (SCHEMAS.md rule 7): a lapsing flag keeps
its day.

**`Content/ContentSave.cs`: the saved game.** What is not a flag is saved by the class that owns it:

```csharp
public static class ContentSave
{
    public static void Register(string key, Func<object> capture, Action<object> restore);
    public static void Unregister(string key);
    public static bool Registered(string key);
    public static IEnumerable<string> Keys { get; }
    public static Dictionary<string, object> Capture();        // SaveGame.Save writes it under "content"
    public static void Restore(Dictionary<string, object> saved);   // SaveGame.Load, after the encounter layer
}
```

A part registers itself on every Play, in its own file, so nobody edits `SaveGame.cs`:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
static void OnPlay() { /* clear my own state */ ContentSave.Register("settlements", Capture, Restore); }
```

`capture` returns what `Json.Write` takes (`Dictionary<string, object>`, `List<object>`, text, `double`, `bool`,
null). `restore` gets what `Json.Parse` made of it, **or null** for a save without that part: then start afresh.

### 1.3 The tests' kit: `Assets/Kapman/Tests/EditMode/Content/ContentTestKit.cs`

Namespace `Kapman.Tests`. What every family's tests need, written once:

```csharp
public static class ContentTestKit
{
    public static string TempFolder();                             // a new empty folder of the test's own
    public static void Put(string folder, string relativePath, string text);   // writes a fixture file (makes its folders)
    public static string UseFolder();                              // TempFolder() + ContentFiles.SetRoot(it); returns it
    public static void Done(string folder);                        // ContentRules.Use(null), ContentFlags.Use(null), SetRoot(null), deletes the folder (null: no folder)
    public static EncounterSources Tables();                       // the real roles, names and words; no cards
    public static EncounterBook EmptyBook();                       // the cards' engine with those tables and no cards
    public static (EncounterDirector d, FakeGame game) AtPort(string port = "dorestad", double day = 10.0, int year = 793, int month = 6);
    public static (EncounterDirector d, FakeGame game) AtSea(double day = 10.0, int year = 793, int month = 6);
    public static void LoadPorts();                                // GameData.LoadPorts from the project's ports_793.json
    public static void LoadGoods();
}
```

`AtPort` gives a director moored at the port (realm `franks`, people `frisian`), already handed to
`ContentRules.Use`, with a fake game that writes down every call (`game.calls`, `game.silver`, 100 d at the start).
The hour is 10 in the morning and the crew is four: set any other number a condition reads with
`d.vars.nums["hold.lead"] = 2;` (`silver` follows `game.silver`). Call `ContentTestKit.Done(folder)` in `[TearDown]`.

### 1.4 Other things done on the way

- `Sim/SimData.cs`: `public static ShipClassDef ClassFrom(object row)`, taken out of `Load` unchanged, so a row
  of `story/ships_793.json` becomes the same `ShipClassDef` the world's ships sail by.
- `Data/GameFiles.cs` and `Encounters/EncounterRunner.cs` read through `ContentFiles` where they read the disk, so
  a built game finds the 22 tables and every card file.
- `Editor/Game/ContentBuild.cs`: see section 6.
- `Bake/content/compile_check.py`: **compiles the game's own C# without starting Unity**, in about five seconds.
  See section 2.

## 2. Rules for the eight loaders

1. **Where.** Runtime classes in namespace `Kapman.Game`, in `Assets/Kapman/Code/Game/Content/<Family>/`. Tests in
   `Assets/Kapman/Tests/EditMode/Content/<Family>Tests.cs`, namespace `Kapman.Tests`. Touch no other file. Never
   edit anything under `Assets/Kapman/Game/Data`. Write no `.meta` file: the next agent that runs Unity commits the
   ones Unity makes.
2. **No family uses another family's classes.** The eight are written at the same time and nobody compiles
   against the others. Use the foundation and the code that exists today. Where two families meet (a site's acts,
   a port's ships), the HUD step joins them (section 5). The signatures below already keep them apart.
3. **The shape of a loader.** A plain class with a public constructor that makes an empty book, a static `Load()`
   that reads through `ContentFiles`, and

   ```csharp
   static readonly ContentCache<PlaceBook> cache = new ContentCache<PlaceBook>("the places", Load);
   public static PlaceBook Current => cache.Value;
   ```

   Nothing is read until somebody asks. `KapmanGame.Awake` needs no new line.
4. **Take what is there.** A missing file or folder: an empty book, one warning (`ContentFiles` gives it). A row
   without an id: skipped. A second row with an id already seen: the first wins, one warning. A field that is
   missing: "" for text, 0 for a number, an empty array for a list; never null. Never `Debug.LogError`, never an
   exception out of a loader: an error fails the play tests, and the content is allowed to be unfinished.
5. **Asked for something that is not there:** a lookup by id returns **null**; a list is **empty, never null**; a
   number is **0**; text is **""**. Each family's table below says so again where it matters.
6. **Names.** A field is the JSON name in camelCase (`cost_d` is `costD`, `built_at` is `builtAt`). Where the JSON
   name is a C# word or would mislead: `class` is `cls`; `do` is `does`; `is` is `isA`; `new` is `news`; `fixed`
   is `fixedRows`; `with` is `withRoles`; `where` is `whereSites` (or what the list holds); `ref` is `reference`;
   `check` (a to-do mark in sayings) is `toCheck`. A condition keeps both: `whenText` (as written) and `when`
   (the parsed `Cond`). A cost keeps `costText`, `cost` (the parsed list) and `costUnderstood`.
7. **Years** are `from` and `to`, 0 meaning open: use `ContentJson.InYears`. **Dates** go through
   `ContentJson.Date` and are compared by `ContentJson.DayKey`.
8. **Words the player reads** are the data's own text fields, shown as they are. Add no label of your own beyond a
   plain fact ("8 d. 4 hours."). No period labels. Never a sound.
9. **C# 9** (`-langversion:9.0`): no file-scoped namespaces, no global usings, no raw string literals, no
   `required`, no collection expressions. Only `Json` reads JSON (no System.Text.Json, no JsonUtility for maps).
10. **Tests.** Use `ContentTestKit` (1.3). Logic is tested on a folder of the test's own:
    `dir = ContentTestKit.UseFolder(); ContentTestKit.Put(dir, "story/ships_793.json", "{ ... }");` and
    `ContentTestKit.Done(dir)` in `[TearDown]`. The shared tables a fixture does not have (the words of standing
    and feeling) are still found: `ContentRules.Words` reads them from the project's folder. A test on the real
    data (`ContentFiles.SetRoot(null)`) must pass while writers are at work: assert what cannot change (the file
    parses, Dorestad is in it, a count is at least so many), and `Assume.That(ContentFiles.Exists(...))` for a
    file that may not be written. A test that needs the ports calls `ContentTestKit.LoadPorts()`. A test that
    needs the engine (a condition asked, a row run) calls `ContentTestKit.AtPort()`. Do not use
    `EncounterData.Book()` (every card of the game): a card file a writer is in the middle of would fail you.
11. **Check your compile without Unity**, as often as you like:

    ```
    "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Bake\content\compile_check.py
    ```

    It compiles Core, Game, Graphs, Editor and both test assemblies with the compiler Unity ships, from every `.cs`
    file on the disk now, into a temp folder, and prints `COMPILES` or the errors. It runs no test. If another
    engineer's half-written file is in the way, it will say whose file: wait, or say so in your report.
12. **No state of your own** beyond the cache. A book is what the files say; what the game has done is in the
    flags. If a class must keep a static field besides its `ContentCache` (an event, a list made from the flags),
    clear it on Play, because this project enters Play without reloading the code:
    `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void OnPlay() { ... }`.
13. **Report** what you built as the plan names it, every place where you had to differ from this page and why,
    and anything in the data that no row of this page covers.

### 2.1 The flags, by family: who sets them, so two parts never use one name

| Flags | Set by | Read by |
|---|---|---|
| `ch_<chain>_<n>`, `ch_<chain>_end_<name>`, a chain's side flags | the chains' cards | the chain journal (e), later cards |
| `unlock_<kind>_<what>` | cards, town acts, `UnlockRegistry.Win` | the unlock registry (d), everything that asks it |
| `changed_<event>` | chains and hinges | cards, endings, the chain journal |
| `act_<what>` (`act_clean_hull`, `act_talks_used`, `act_talks_call` ...) | the town acts (c) | the acts' own conditions |
| `house_<what>`, `wintering`, `crew_<what>`, `boat_<what>`, `vow_<what>`, `memorial_<what>` | the winter's cards and screen | the winter, the endings |
| `age` | `YearBook.SyncFlags` (h) | cards (`flag.age >= 3`) |
| `st_<port>_<what>`, `here_<what>`, `war_<id>...`, `news_<port>_days`, `word_band_<port>`, `promises`, `house_keels` and the rest of SCHEMAS.md rule 9 | the living world's ledgers, later (not the loaders) | cards, jobs, hinges |
| `open_<port>_<what>`, `st_end_<port>_<name>`, `war_<id>_end_<name>`, `ending_<id>`, `start_<id>` | the living world, later | cards, the house book |
| `hud_<button>` | the frame (HUD step 1) | the frame |
| the spine's own (`bs_`, `lf_`, `yk_`, `hy_`, `bk_`, `en_`, `rw_`, `db_` ...) | the regions' cards | the regions' cards |

A loader sets no flag when it loads. A name is letters, digits and underscores; `ContentFlags.Set` refuses any other.

## 3. The families

For each: the file, the classes with their public members exactly, what comes back for missing data, and which
existing class will call it. Fields are listed JSON name first where the C# name differs.

### (a) Places: `Content/Places/PlaceBook.cs`

Reads `places/*.json`. A file holds one port or several under `"ports"` (BRIEF.md, BRIEF_2.md); take every port of
every file.

```csharp
public sealed class SiteDo        { public string raw = "", kind = "", id = ""; }
    // "market" -> kind "market", id "";  "act:repair" -> "act", "repair";  "person:dorestad.wright" -> "person", ...;
    // "card:harbour.x.y" -> "card", ...;  anything else -> kind "" (kept, shown nowhere, one entry in Faults)
public sealed class SiteDef       { public string id = "", port = "", kind = "", name = "", see = "", source = "";
                                    public int from, to; public SiteDo[] does = new SiteDo[0];
                                    public string[] doRaw = new string[0];        // the "do" strings as written
                                    public bool Open(int year); }
public sealed class PersonTie     { public string to = "", tie = "", note = ""; }
public sealed class PersonDef     { public string id = "", port = "", name = "", role = "", people = "", faith = "", sex = "", trait = "",
                                    want = "", fear = "", voice = "", looks = "", source = "";
                                    public bool historical; public int born, died, from, to;
                                    public string[] knows = new string[0]; public PersonTie[] ties = new PersonTie[0];
                                    public Dictionary<string, string[]> lines;    // "greet" -> one line; "rumour" -> several
                                    public bool Here(int year);                   // InYears(from, to) and not dead
                                    public string Line(string key, int pick = 0); }   // "" if he has no such line
public sealed class StoryBeat     { public string at = "", whenText = "", text = "", card = ""; public Cond when; }
public sealed class StoryEnding   { public string id = "", how = "", text = "", reward = ""; }
public sealed class StoryDef      { public string id = "", port = "", title = "", thread = "", summary = "", picture = "", reward = "";
                                    public StoryBeat[] beats; public string[] links; public StoryEnding[] endings; }
public sealed class PlaceWorkDef  { public string id = "", port = "", name = "", what = "", needs = "", gives = "", picture = "", source = "";
                                    public int costD, days, from, to;
                                    public string Kind { get; } }                 // "dorestad.work.jetty" -> "jetty"
public sealed class ShipOffer     { public string port = "", cls = "", how = "", note = "", picture = "", source = ""; public int from, to; }
public sealed class PlaceNote     { public string text = "", where = "", source = ""; public int year, from, to; }
    // one shape for history (year, text, source), eras (from, to, text), sayings and easter_eggs (text, where, source)
public sealed class PlaceUnlock   { public string id = "", what = "", how = "", gives = ""; }
public sealed class PlaceDef      { public string port = "", file = "", region = "", summary = "", seasons = "";
                                    public PlaceNote[] history, eras, sayings, easterEggs;
                                    public SiteDef[] sites; public PersonDef[] people; public StoryDef[] stories;
                                    public PlaceWorkDef[] works; public ShipOffer[] ships; public PlaceUnlock[] unlocks;
                                    public string[] sources; }

public sealed class PlaceBook
{
    public PlaceBook();
    public static PlaceBook Current { get; }
    public static PlaceBook Load();
    public IReadOnlyList<PlaceDef> Places { get; }                 // in the files' order
    public PlaceDef Place(string port);                            // null: no file has that port
    public SiteDef Site(string siteId);                            // null
    public PersonDef Person(string personId);                      // null
    public StoryDef Story(string storyId);                         // null
    public PlaceWorkDef Work(string workId);                       // null
    public List<SiteDef> Sites(string port, int year);             // those there that year, in the file's order
    public List<PersonDef> People(string port, int year);
    public List<PersonDef> PeopleAt(string siteId, int year);      // the people its "person:" lines name
    public List<PlaceWorkDef> Works(string port, int year);
    public List<ShipOffer> Ships(string port, int year);
    public string Era(string port, int year);                      // the era's text for that year; ""
    public IReadOnlyList<string> Faults { get; }                   // "dorestad.quay: person:dorestad.x is not in the file"
}
```

- `people[].lines` values are one string or a list of strings: keep both as `string[]`.
- A beat's `when` is a condition in every file today: `ContentRules.ParseWhen(text, file + " story " + id)`.
- `Faults` collects, never throws: a site's person who is not in the file, two sites with one id, a `do` string of
  no known kind. Do **not** check acts, cards or ship classes here (other families' tables): the HUD step does.
- **Called by:** the town panel that replaces the port menu of `Code/Graphs/PortNodes.cs` and
  `MapView.ExploreTown` (HUD step 3); the portrait (a person's `looks` and `lines`); `PortLife.Talk` for a rumour.
- **Tests:** a fixture with two ports in one file and one in another; a site closed by its years; a person with a
  line list; a half-written second file skipped. On the real data: at least 40 ports, `Place("dorestad")` has at
  least 8 sites and a person `dorestad.ravn`.

### (b) Ships and upgrades: `Content/Ships/ShipBook.cs`

Reads `story/ships_793.json` ("ships": 20 rows, each a full class row plus the house's fields) and
`story/ship_upgrades_793.json` ("upgrades": 40 rows; "fit_groups").

```csharp
public sealed class ShipDef
{
    public string id = "", name = "", periodName = "", look = "", howHad = "", unlockFlag = "", whatItIs = "", whyWantIt = "",
                  builtAtNote = "", canGoNote = "", source = "";
    public Kapman.Game.Sim.ShipClassDef cls;        // never null: SimData.ClassFrom(row)
    public bool inClassTable;
    public int rung, from, to, costD, buildDays, crewMin, crewFull, upkeepDAYear, lifeYears;
    public float cargoKg;
    public string[] builtAt = new string[0];        // port ids
    public string canRiver = "", canFlats = "", canCoast = "", canOpenSea = "";   // "yes" | "limited" | "no"
    public Dictionary<string, double> numbers;      // every numeric field of the row by its JSON name
    public bool Offered(int year);                  // year >= from and year <= to
    public bool BuiltAt(string port);
    public bool RewardOnly { get; }                 // howHad == "reward"
}
public sealed class UpgradeChange { public string field = ""; public double by; public double times = 1.0; public bool isTimes; }
public sealed class UpgradeDef
{
    public string id = "", group = "", name = "", whatItIs = "", evidence = "", goods = "", upkeep = "", lostWhen = "", howHad = "",
                  rewardNote = "", source = "", plain = "";      // plain = effect.plain
    public int from, to, costD; public double days; public bool sizeFree;
    public string[] whereSites = new string[0], wherePorts = new string[0];   // where.sites (site kinds), where.ports
    public string[] fits = new string[0];           // ship ids; { "all" } when the row says "all"
    public UpgradeChange[] changes = new UpgradeChange[0];
    public string Flag { get; }                     // "unlock_upgrade_" + id
    public bool Fits(string shipId);
    public bool OfferedAt(string siteKind, string port, int year);   // empty wherePorts: any port
}
public sealed class FittedShip { public ShipDef ship; public Kapman.Game.Sim.ShipClassDef cls; public Dictionary<string, double> numbers; }

public sealed class ShipBook
{
    public ShipBook();
    public static ShipBook Current { get; }
    public static ShipBook Load();
    public IReadOnlyList<ShipDef> Ships { get; }
    public ShipDef Ship(string id);                                // null
    public IReadOnlyList<UpgradeDef> Upgrades { get; }
    public UpgradeDef Upgrade(string id);                          // null
    public List<ShipDef> Roster(int year);                         // offered that year: by rung, then by cost
    public List<ShipDef> RosterAt(string port, int year);          // of those, the ones whose builtAt has the port
    public List<UpgradeDef> UpgradesFor(string shipId, string siteKind, string port, int year);
    public int UpgradeCost(UpgradeDef upgrade, ShipDef ship);      // 0 if either is null
    public double UpgradeDays(UpgradeDef upgrade, ShipDef ship);
    public FittedShip Fitted(ShipDef ship, IEnumerable<string> upgradeIds);   // null if ship is null
}
```

- `ShipDef.cls` is made by `SimData.ClassFrom(row)`. Note `from`/`to` are real years here (793, 1066), where the
  class table writes 0.
- **The size rule** (`ship_upgrades_793.json` "rules"): `costD` and `days` are for a hull under 9 m; times 2 for
  9 to 12 m, times 4 for 13 to 17 m, times 8 above; `sizeFree` rows cost the same on any hull.
- **`Fitted`** copies the class and applies each fitting's changes: `by` is added, `times` multiplies. Fields the
  class has: `polar_factor` (polar), `closest_to_wind_deg` (closestToWind), `gale_fragility` (galeFragility),
  `speed_sail_kn` (sailKn), `speed_max_kn` (maxKn), `speed_oars_kn` (oarsKn), `cargo_t` (cargoT), `draught_m`
  (draught), `hull_top_m` (hullTop), `sail_top_m` (sailTop), `lookout_eye_m` (lookoutEye). The others the data
  changes (`oars`, `freeboard_m`, `towed_upstream_km_day`) live only in `numbers`. Then the caps of the rules: no
  ship is bettered by fittings by more than a fifth in any speed, 8 degrees toward the wind, or 0.25 in
  `gale_fragility`. An unknown fitting id is skipped.
- **Called by:** `KapmanGame.MakeWorld` (a wiring edit, section 4: the roster's classes join `SimData.classes`, so
  `WorldSim.ClassOf("knarr")` answers); the town acts `build_ship`, `buy_ship`, `fit_upgrade` (HUD step 3); the
  ship window (HUD step 5); `Trade.Begin` for the hold of the hull she sails.
- **Tests:** a fixture of two ships and three upgrades for `Roster`, the size rule, `Fitted` with a cap reached, a
  fitting that does not fit. On the real data: 20 ships, `Ship("gislinge").cls.length` is 7.7, `Ship("knarr")` is
  not offered in 810 and is in 950, at least 30 upgrades.

### (c) The town: `Content/Town/TownBook.cs` and `Content/Town/Town.cs`

Reads `story/town_actions_793.json` ("acts", "rules", "flags"), `story/works_793.json` ("works", "ladder") and
`story/town_games_793.json` ("games"). Design: `Design/town_interactivity.md`.

```csharp
public sealed class ActDef
{
    public string id = "", name = "", what = "", evidence = "", who = "", costText = "", whenText = "", gives = "", changes = "",
                  offer = "", done = "", refused = "", source = "";    // offer, done, refused = words.offer ...
    public string[] at = new string[0];             // the site kinds it belongs to
    public double hours; public bool talk;
    public Cond when = Cond.True;                    // ContentRules.ParseWhen
    public List<Effect> cost = new List<Effect>(), effects = new List<Effect>();
    public bool costUnderstood = true, effectsUnderstood = true;
    public bool shutOnSunday, openAtNight, moreOnFeast, shutOnFast;   // from rules.sunday.shut, night.open, feast.more, fast.shut
}
public sealed class TownRules { public int talksPerCall = 3, talkBackAtDawn = 1, workFrom = 6, workTo = 18, eveningTo = 23; }
public sealed class WorkKindDef
{
    public string id = "", name = "", kind = "", gotBy = "", what = "", evidence = "", needs = "", gives = "", source = "";
    public int rung, from, to, costD, days, upkeepDAYear;
    public string[] whereSites = new string[0], canBeLost = new string[0], aliases = new string[0];
    public string Flag(string port);                 // "unlock_work_" + port + "_" + id
}
public sealed class WorkRung { public int rung; public string name = "", means = ""; public string[] works = new string[0]; }
public sealed class TownGameDef
{
    public string id = "", name = "", act = "", screen = "", takes = "", where = "", cardRun = "", oddsWords = "", stakes = "",
                  whatItGives = "", picture = "", source = "", notKnown = "";
    public string[] withRoles = new string[0], peoples = new string[0], whereSites = new string[0], wherePorts = new string[0],
                    howItPlays = new string[0], inVerbs = new string[0], evidence = new string[0];
    public int from, to;
}

public sealed class TownBook
{
    public TownBook();
    public static TownBook Current { get; }
    public static TownBook Load();
    public TownRules Rules { get; }                                // the defaults when the file has none
    public IReadOnlyList<ActDef> Acts { get; }
    public ActDef Act(string id);                                  // null
    public IReadOnlyList<WorkKindDef> Works { get; }
    public WorkKindDef Work(string idOrAlias);                     // null
    public IReadOnlyList<WorkRung> Ladder { get; }
    public IReadOnlyList<TownGameDef> Games { get; }
    public TownGameDef Game(string id);                            // null
    public List<TownGameDef> GamesFor(string act, string siteKind, string port, string people, int year);
    public IReadOnlyDictionary<string, string> ActFlags { get; }   // the file's "flags": name -> what it means
}

public sealed class SiteChoice                 // one line of a site's menu, ready for a panel
{
    public string kind = "", id = "";          // "market" | "act" | "person" | "card", and the act, person or card id
    public string label = "", detail = "";     // an act: its name, and "8 d. 4 hours."; a person or card: the id (the panel names them)
    public bool open; public string why = "";  // shut: why, in plain words
    public ActDef act;                         // for an act; else null
}

public static class Town                       // the live side: needs a game (ContentRules.Ready)
{
    public static List<SiteChoice> Menu(string port, string siteKind, IEnumerable<string> does);   // the site's "do" strings, in order
    public static string Shut(ActDef act);                         // null: open now. Else why
    public static ContentResult Do(ActDef act);                    // pays, moves the clock, runs the effects
    public static int TalksLeft { get; }                           // 0 with no call in progress
    public static bool UseTalk();
    public static event Action<ActDef, ContentResult> Done;
}
```

- `who`: "none" in the data becomes "". `cost`: `ContentRules.ParseCost` ("none" is free).
- **`Shut(act)`**, in this order: no game: "The game is not ready."; not moored (`ContentFlags.State.callPort`
  empty): "She is not in port."; `shutOnSunday` at a Christian port on a Sunday: `act.refused`; `talk` and no
  talk left: `act.refused`; `!costUnderstood`: "Its price is not settled yet."; the `when` does not hold:
  `act.refused` if it has one, else `ContentRules.Why(act.when)`; the cost cannot be paid: `ContentRules.Lacks`.
  The port's faith: `ContentRules.Director.Facts(port).faith`. Sunday: `Kapman.World.Calendar.Weekday` of the
  level clock's Julian day is 0. The acts' own `when` already holds the hours and the feasts.
- **`Do(act)`**: `Shut` first (not ok, `why`). Then `ContentRules.Run("act." + act.id, act.when, act.cost,
  act.effects)`; if ok, move the clock as `Hoards.Dig` does (`LevelClock.Current.GameHours += act.hours`, so the
  hourly draws still run), count a talk if `act.talk`, put `act.done` first among the facts, raise `Done`.
- In an EditMode test there is no clock (`Kapman.Levels.LevelClock.Current` is null): then it is never Sunday,
  and `Do` moves no clock. Take today's whole days from `ContentFlags.Day`.
- **Talks** are kept in flags, so they are saved: `act_talks_used` (how many this call) and `act_talks_call` (the
  day this call began, `State.callStartDay`). A different call day means none used. Left = the smaller of
  `talksPerCall` and `talksPerCall + (whole dawns since the call began) x talkBackAtDawn - used` (D).
- **`Menu`** takes strings, not another family's classes: `"market"` is one choice, open; `"act:<id>"` is the act
  (`Shut` gives `open` and `why`; an act the table does not have is left out with one warning); `"person:<id>"`
  and `"card:<id>"` are passed through with `open` true for a person and `Director.CanOffer(id)` for a card.
  `detail` for an act is its price and its time as plain facts, in that order (depth_design 1.6: silver, then
  hours): from the parsed cost, silver as "8 d.", goods as "2 lead." (the good's `name` from `GameData.Good`, else
  the id with spaces), men as "1 man."; then the hours: nothing for 0, "Half an hour." for 0.5, "An hour." for
  1, "4 hours." otherwise. So: "8 d. 4 hours.".
- **Called by:** the town panel (HUD step 3), which gets a site's `doRaw` from `PlaceBook` and passes it here; the
  day stick (hours); the market site opens `MarketPanel` as today.
- **Tests:** with a fixture and `ContentSeamTests.AtDorestad()`'s director: an act runs and pays; its `when`
  refuses it at sea; a prose cost keeps it shut; three talks and no fourth; a Sunday at a Christian port. On the
  real data: all 53 acts of BRIEF_2.md's list load, every one with `costUnderstood` and `effectsUnderstood`.

### (d) Unlocks: `Content/Unlocks/UnlockRegistry.cs`

Reads `story/unlocks_793.json` ("kinds": 17, "unlocks": 455). What the house **has** is the flags.

```csharp
public sealed class UnlockKind { public string id = "", prefix = "", rule = ""; public bool inherited; public bool isFlag; }
    // isFlag: the prefix begins with "unlock_". The others (tongue_, learned_, memorial_, text., rw_) are not flags:
    // the game holds them elsewhere (tongue.<name>, learned.<x>, the chest). They are listed, never asked through Has.
public sealed class UnlockDef
{
    public string id = "", kind = "", name = "", whatItIs = "", whatItGives = "", source = "";
    public string howPlain = "", howBy = "", whenText = "", costText = "";     // how_won.plain, .by, .when, .cost
    public string[] where = new string[0];          // how_won.where: port ids; empty: anywhere
    public int days, from, to; public bool inherited;
    public Cond when = Cond.True; public List<Effect> cost = new List<Effect>(); public bool costUnderstood = true;
    public string shipClass = "", worksKind = "";
    public string[] alsoSetBy = new string[0], fromChain = new string[0];
    public string What { get; }                     // the id without its kind's prefix: "knarr", "dorestad", "dorestad_jetty"
}

public sealed class UnlockRegistry
{
    public UnlockRegistry();
    public static UnlockRegistry Current { get; }
    public static UnlockRegistry Load();
    public IReadOnlyList<UnlockKind> Kinds { get; }                // the file's; the twelve of CHAINS.md if there is no file
    public UnlockKind Kind(string id);                             // null
    public UnlockKind KindOf(string flag);                         // by the longest prefix; null
    public IReadOnlyList<UnlockDef> Defs { get; }
    public UnlockDef Def(string flag);                             // null: the table has no row for it
    public List<UnlockDef> OfKind(string kind);
    public string NameOf(string flag);                             // the row's name; else the flag in plain words

    // what the house has: the flags, through ContentFlags (false and empty with no game)
    public bool Has(string flag);
    public bool Has(string kind, string what);                     // Has(prefix + what)
    public bool HasShip(string classId);                           // unlock_ship_<class>
    public bool HasCharter(string port);                           // unlock_charter_<port>
    public bool HasPlot(string port);
    public bool HasFactor(string port);
    public bool HasPilot(string water);
    public bool HasUpgrade(string upgradeId);
    public bool HasWork(string port, string workKind);             // unlock_work_<port>_<kind>
    public bool HasTitle(string name);
    public List<string> Held();                                    // every unlock_ flag set, in name order
    public List<string> Held(string kind);                         // "what" parts: Held("plot") -> { "dorestad", "lundenwic" }
    public List<UnlockDef> Offered(string port, int year);         // rows that can be had there now: see below
    public ContentResult Win(UnlockDef def);                       // the price paid and the flag set
    public List<string> NotInherited();                            // held flags an heir does not keep
}
```

- No file: `Kinds` are the twelve flag kinds of CHAINS.md (ship, charter, plot, factor, partner, pilot, upgrade,
  work, title, relic, heirloom, kin) with their `unlock_<kind>_` prefixes, so every `Has...` still answers.
- `Offered(port, year)`: rows of a flag kind, in their years, not held, whose `where` is empty or has the port,
  whose `howBy` begins with `act:` and whose cost was understood. It does not ask `when` (the caller greys with
  `ContentRules.Why`). `Win(def)`: `ContentRules.Run(def.id, def.when, def.cost, <"flag " + def.id>)`.
- A flag set by a chain that the table has no row for is still held: `Held()` lists it, `NameOf` gives it plain
  words ("unlock_pilot_vlie" is "pilot: vlie").
- Keep nothing: ask `ContentFlags` each time (it is a dictionary lookup). If you do keep a list, remake it when
  `ContentFlags.Version` differs.
- **Called by:** `Deal.Sell` for the toll (a wiring edit: a charter at this port waives it); the town's acts
  `buy_plot`, `fit_upgrade`, `build_ship`, `fund_work`; the House and Hold panels; `ShipBook` users asking
  `HasShip` before a reward-only hull is offered; the winter's inheritance (`NotInherited`).
- **Tests:** flags through `ContentFlags.Use(new EncounterState(), () => day)`; a flag with no row; `KindOf` with
  `unlock_work_dorestad_jetty` against `unlock_work_`; `Offered` and `Win` with the director of `ContentSeamTests`.
  On the real data: 17 kinds, at least 400 rows, every row's id begins with its kind's prefix.

### (e) The chain journal: `Content/Chains/ChainJournal.cs`

Reads `encounters/chain_*_793.json` itself (the raw files, so it works in a test and when a card has a fault) and
the flags. A chain's cards are `<stage>.chain_<id>.<step>`; a step is `s03_reeve` (number 3) or `end_<name>`
(an ending's card). Its flags are `ch_<id>_<n>` with an optional letter (`ch_black_stones_3h` is step 3),
`ch_<id>_end_<name>`, and side flags (`ch_black_stones_trust`) that are neither.

```csharp
public sealed class ChainStepDef { public string card = "", step = "", titleKey = "", title = "", intentionKey = "", intention = "", stage = "";
                                   public int number; public bool ending; public string endName = ""; }
public sealed class ChainDef
{
    public string id = "", file = "", about = "", name = "";       // name: the id in plain words ("black stones") until a page names it
    public ChainStepDef[] steps = new ChainStepDef[0];             // its cards, in the file's order
    public string[] flags = new string[0];                         // every ch_<id>_ flag its cards set
    public string[] endings = new string[0];                       // the <name> of every ch_<id>_end_<name>
    public string[] changed = new string[0], unlocks = new string[0];   // the changed_ and unlock_ flags its cards set
    public string FlagPrefix { get; }                              // "ch_" + id + "_"
    public ChainStepDef Step(int number);                          // the first card of that number; null
    public ChainStepDef EndingCard(string endName);                // null
}
public enum ChainState { NotBegun, Afoot, Ended }
public sealed class ChainEntry
{
    public ChainDef def; public ChainState state; public int step;         // the highest step number among the flags set; 0
    public ChainStepDef reached;                                           // that step's card; null
    public string line = "";                                               // one plain line to show
    public string nextCard = "", nextPort = ""; public double nextDay = -1.0;   // its earliest follow-up card in line, if any
    public string[] endingsReached = new string[0], endingLines = new string[0];
    public bool changedHistory;
}

public sealed class ChainJournal
{
    public ChainJournal();
    public static ChainJournal Current { get; }
    public static ChainJournal Load();
    public IReadOnlyList<ChainDef> Chains { get; }
    public ChainDef Chain(string id);                              // null
    public ChainDef ChainOfCard(string cardId);                    // null: not a chain's card
    public ChainDef ChainOfFlag(string flag);                      // the chain with the longest id that fits; null
    public ChainEntry Entry(string id);                            // as the flags stand now; null for an unknown chain
    public List<ChainEntry> Afoot();                               // begun, not ended: the one with the nearest next day first
    public List<ChainEntry> Ended();
    public List<ChainEntry> All();
    public static string Words(string key, string plain);          // EncounterRunner.Book's words for a key, else the plain text
}
```

- The flags a chain's cards set: every string in an option's `effects` (and inside a chance object's `success`
  and `failure`) that begins with `flag `; the name is the second word.
- **State:** `Ended` if any `ch_<id>_end_*` flag is set; else `Afoot` if any flag with the chain's prefix is set;
  else `NotBegun`. With no game every chain is `NotBegun`.
- **`line`:** the reached step's title (`Words(titleKey, title)`); if a follow-up card of this chain waits in
  `ContentFlags.State.scheduled`, add where and when in plain words: " Next: at Lundenwic, in 2 days." (the port's
  name from `GameData.Port(id)?.name`, else the id). For an ended chain: the ending card's title.
- `nextDay` is `EncounterState.Scheduled.day`; days from now is `nextDay - ContentFlags.Day`.
- **Called by:** the Tasks panel and the Chest's Years (HUD step 4); the log, when a step's flag is first set
  (the panel compares `ContentFlags.Version`).
- **Tests:** a fixture chain of four cards with a branch (`_2a`, `_2b`), two endings and a `schedule`; flags
  through `ContentFlags.Use`. On the real data: at least 40 chains; `Chain("black_stones")` has an ending "peace";
  `ChainOfFlag("ch_kentish_revolt_796_3")` is `kentish_revolt_796`.

### (f) The gazetteer and the routes: `Content/Gazetteer/Gazetteer.cs` and `Content/Gazetteer/Routes.cs`

`gazetteer_793.json`: "places" (15,447) and "stones" (3,239), each with `id`, `qid`, `name`, `name_local`, `lat`,
`lon`, `x`, `z`, `kind`, `from`, `to` (0 = not known), `tier` (1 the 50 ports, 2 a place a merchant would know by
name, 3 the rest), `source`, `country`. `x` and `z` are the world frame's metres (x east, z north, the same
projection as `Kapman.World.WorldFrame.ToWorld`): what `MapView` draws in.

```csharp
public sealed class GazPlace { public string id = "", qid = "", name = "", nameLocal = "", kind = "", country = "";
                               public string[] also = new string[0];   // its other kinds ("also": ["roman_town"]); 330 rows have it
                               public double lat, lon, x, z; public int from, to, tier; public bool stone;
                               public bool In(int year); }         // InYears: an unknown date is always there

public sealed class Gazetteer
{
    public Gazetteer();
    public static Gazetteer Current { get; }
    public static Gazetteer Load();
    public string Credit { get; }                                  // the file's "credit": it goes in the credits
    public IReadOnlyList<GazPlace> Places { get; }
    public IReadOnlyList<GazPlace> Stones { get; }
    public GazPlace Place(string id);                              // null
    public int InView(double west, double south, double east, double north, int maxTier, int year,
                      List<GazPlace> into, int limit = 400, bool stones = false);
    public GazPlace Nearest(double x, double z, double within, int maxTier = 3, int year = 0);   // null
    public List<GazPlace> Near(double x, double z, double within, int maxTier, int year);
    public List<GazPlace> Find(string text, int limit = 20);       // by either name: begins-with first, then contains
    public static int TierFor(double metresPerPixel);              // 1 above 2,500 m a pixel, 2 above 300, else 3 (D)
}
```

- `InView` appends to `into` (it does not clear it) the places inside the box whose tier is at most `maxTier` and
  that are there in `year` (0: any year), **the better tier first**, and stops at `limit`. It returns how many it
  added. `stones: true` adds the stones after the places. An empty gazetteer adds nothing.
- **Index:** a grid of 20 km cells (D), one dictionary a tier, from `(cellX, cellZ)` to the places in it, so a
  wide view that asks for tiers 1 and 2 never walks the 12,482 of tier 3. Build it in `Load`. Keep `GazPlace`
  records only: do not keep the parsed tree (40 MB).
- A row without `x` and `z` gets them from `WorldFrame.ToWorld(lat, lon)`. A tier-1 row has the port's own id: the
  chart already draws the ports, so its caller skips ids that `GameData.Port` knows.
- **Routes.** No routes file exists yet and no brief fixes its shape, so read forgivingly: every file that
  `ContentFiles.List("", "routes_*.json")` and `List("story", "routes_*.json")` find.

```csharp
public sealed class RouteDef { public string id = "", kind = "", name = "", note = "", source = "", file = "";
                               public int from, to; public string[] via = new string[0];
                               public double[] xs = new double[0], zs = new double[0];   // the line, world metres
                               public double km; public double west, south, east, north; }
public sealed class Routes
{
    public Routes();
    public static Routes Current { get; }
    public static Routes Load();
    public IReadOnlyList<RouteDef> All { get; }
    public RouteDef Route(string id);                              // null
    public List<RouteDef> OfKind(string kind, int year = 0);       // "water", "land", or as the files write it
    public int InView(double west, double south, double east, double north, int year, List<RouteDef> into);
    public List<RouteDef> Through(string placeOrPortId);
}
```

  A file's routes are the list under `"routes"`; or lists under `"water"` and `"land"` (the key is the kind); the
  kind is else the row's `kind`, else the word between `routes_` and the next `_` in the file's name. A row's line
  is `points` (or `line`, or `path`): a list of `[a, b]` pairs or of objects with `lat` and `lon` or `x` and `z`.
  A pair is latitude and longitude when both are within 90 and 180, else metres. With no line, its `via` (or
  `places`, `stops`, `ports`) ids are looked up: `GameData.Port(id)` first (its `lat`, `lon`), then
  `Gazetteer.Current.Place(id)`; an id found nowhere is skipped with one warning for the route. A route with
  fewer than two points is kept with an empty line. `km` is the line's length.
- **Called by:** `MapView.Draw` for names and signs by zoom (HUD step 6: it knows its own box as
  `cx`, `cz`, `mpp` and the panel's size); the dusk rule's "a known landing within 8 km" (`Near`); `HoverCards`.
- **Tests:** a fixture of twelve places over three tiers and two cells: the box, the tier, the year, the limit,
  the order; `Nearest`; `Find`. Routes in the three shapes above. On the real data (`Assume` it is there): at
  least 15,000 places, `Place("dorestad")` is tier 1, a box round Dorestad at tier 2 holds Utrecht or Tiel, and
  `Load` takes under two seconds.

### (g) The living world's data: `Content/Living/`

Loaders only, by `Design/content/SCHEMAS.md`. The ledgers, the wars' clock and the teller are
`Design/living_world.md` section 10, steps 2 to 16, and are **not** this work. Today only
`story/settlements/rhine_frisia.json` exists: write every loader against the worked examples of SCHEMAS.md (copy
them into your fixtures) and take what is there.

One file a subject, each with its book:

| File | Book | Reads |
|---|---|---|
| `Settlements.cs` | `Settlements` | `story/settlements/*.json` |
| `Wars.cs` | `Wars` | `story/wars/*.json`, `story/war_templates_793.json` |
| `Hinges.cs` | `Hinges` | `story/hinges/*.json` |
| `Campaign.cs` | `Campaign` | `story/starts_793.json`, `story/endings_793.json` |
| `Storyteller.cs` | `Storyteller` | `story/storyteller_793.json` |
| `WorldEffects.cs` | `WorldEffect` | the `world` lists of all of them (SCHEMAS.md rule 8) |

```csharp
// ---- WorldEffects.cs: the second language, parsed only (the ledgers apply it later)
public sealed class WorldEffect
{
    public string raw = "", verb = "", port = "", word = "";      // port may be "here"; word: a state, a realm, a work kind, a template or job id, an entry id
    public double n, days; public bool share, captives;           // "-3%": share; "+captives": captives
    public int toYear;                                            // "hold <port> <realm> to <year>"
    public static bool Parse(string text, out WorldEffect effect);
    public static WorldEffect[] ParseList(object node, string where);   // a fault: one warning, left out
    public static readonly string[] Verbs;    // people mark store heal state hold tilt war job open close work drop tell
}

// ---- Settlements.cs
public sealed class PeoplePoint  { public string age = "", source = "", note = ""; public int year; public double low, high, best; }
public sealed class GoodFlow     { public string good = "", season = "", why = "", source = ""; public double aYear; }
public sealed class StoreRow     { public string good = "", where = "", note = "", source = ""; public double months; }
public sealed class CauseRow     { public string cause = "", note = "", source = ""; public double by; public int[] years = new int[0]; public bool turns; }
public sealed class FixedRow     { public string what = "", text = "", why = "", says = "", source = ""; public int from, to; }
public sealed class HelpRow      { public string id = "", by = "", what = "", then = "", card = "", job = "", work = "", act = "", whenText = "", costText = "";
                                   public Cond when = Cond.True; public List<Effect> cost, effects; public bool costUnderstood = true, effectsUnderstood = true;
                                   public WorldEffect[] world = new WorldEffect[0]; public Dictionary<string, double> gift; }
public sealed class AskRow       { public string job = "", whenText = ""; public Cond when = Cond.True; public string[] goods = new string[0];
                                   public double unitsLow, unitsHigh, premiumLow, premiumHigh, daysLow, daysHigh; }
public sealed class OpenRow      { public string id = "", what = "", whenText = "", text = "", lostText = "", source = ""; public Cond when = Cond.True;
                                   public string[] opens = new string[0], closes = new string[0]; public bool keeps;
                                   public string Flag(string port); }      // "open_" + port + "_" + what
public sealed class PlaceEnding  { public string id = "", whenText = "", text = "", picture = ""; public int at; public bool real;
                                   public Cond when = Cond.True; public List<Effect> effects; public WorldEffect[] world = new WorldEffect[0]; }
public sealed class SettlementDef
{
    public string port = "", region = "", file = "";
    public PeoplePoint[] people = new PeoplePoint[0];
    public double householdSize = 6.0, householdLow, householdHigh, byKeel, afterHarvest;
    public string[] eats = new string[0];
    public GoodFlow[] makes, needs; public StoreRow[] stores; public CauseRow[] grows, fails; public FixedRow[] fixedRows;
    public HelpRow[] help; public AskRow[] asks; public OpenRow[] opens; public PlaceEnding[] endings;
    public double Line(double year);          // history's own figure: straight between rows, the first before, the last after; 0 with no rows
    public double Households(double heads);   // heads / householdSize
}
public sealed class Settlements
{
    public Settlements();
    public static Settlements Current { get; }
    public static Settlements Load();
    public IReadOnlyList<SettlementDef> All { get; }
    public SettlementDef Of(string port);                          // null: no row
    public double Line(string port, double year);                  // 0 with no row
    public List<string> PortsWithout();                            // ports of GameData.Ports with no row
}

// ---- Wars.cs
public sealed class RealmDef   { public string id = "", name = "", under = "", people = "", faith = "", toll = "", law = "", note = "", source = ""; public int from, to; }
public sealed class HoldRow    { public string port = "", realm = "", lord = "", entry = "", note = "", source = ""; public bool historical;
                                 public int fromKey, toKey; }      // ContentJson.DayKey; toKey 0: until the next row
public sealed class WarSide    { public string side = "", realm = "", who = ""; public string[] bandsFrom = new string[0]; }
public sealed class WarLevers  { public double bandsLow, bandsHigh, dearX = 1.0, patrolsX = 1.0; public string[] dearGoods = new string[0]; public bool levy; }
public sealed class WarPhase   { public int n, tide, onKey, untilKey; public string kind = "", entry = "", text = "", source = "";
                                 public string[] ports = new string[0], jobs = new string[0]; }
public sealed class WarEnding  { public string id = "", whenText = "", text = ""; public bool real; public Cond when = Cond.True;
                                 public List<Effect> effects; public WorldEffect[] world = new WorldEffect[0]; }
public sealed class WarDef     { public string id = "", name = "", file = "", cause = "", whatItDidToTrade = ""; public bool real; public int fromKey, toKey;
                                 public WarSide a, b; public WarLevers levers; public WarPhase[] phases; public WarEnding[] endings;
                                 public string[] theatre, plays, chains, hinges, sources;
                                 public bool Runs(int dayKey); public WarPhase PhaseOn(int dayKey); }   // the last phase begun by then; null
public sealed class WarTemplate { public string id = "", name = "", what = "", precedent = "", source = ""; public int from, to; public int[] months;
                                  public Dictionary<string, object> raw; }   // sides, theatre, levers, phases, endings as parsed: the teller's own code reads them
public sealed class GroundDef  { public string id = "", a = "", b = "", why = "", source = ""; public int from, to; public bool mayChangeHands;
                                 public string[] templates = new string[0], ports = new string[0]; }
public sealed class Wars
{
    public Wars();
    public static Wars Current { get; }
    public static Wars Load();
    public IReadOnlyList<RealmDef> Realms { get; }
    public RealmDef Realm(string id);                              // null
    public IReadOnlyList<HoldRow> Holds { get; }                   // by port, then by day
    public string HolderOf(string port, int year, int month, int day);   // see below; "" for an unknown port
    public IReadOnlyList<WarDef> All { get; }
    public WarDef War(string id);                                  // null
    public List<WarDef> Running(int dayKey);
    public List<WarDef> At(string port, int dayKey);               // those running whose theatre has the port
    public IReadOnlyList<WarTemplate> Templates { get; }
    public IReadOnlyList<GroundDef> Grounds { get; }
}

// ---- Hinges.cs
public sealed class HingeHerald { public int daysBefore; public string card = "", news = ""; }
public sealed class HingeSoften { public string entry = "", whenText = "", text = ""; public Cond when = Cond.True; public double woundX = 1.0; }
public sealed class HingeLine   { public string port = "", note = ""; public PeoplePoint[] people = new PeoplePoint[0]; public int to, sizeCap; }
public sealed class HingePays   { public string tier = "", by = ""; public double silverLow, silverHigh; public string[] grand = new string[0]; }
public sealed class HingeDef
{
    public string id = "", name = "", cls = "", entry = "", really = "", couldAMerchant = "", turnedText = "", turnedWords = "",
                  chain = "", war = "", picture = "", note = "", file = "", age = "";
    public int onKey; public Cond turned;                           // ContentRules.ParseWhen: a fault never turns
    public HingeHerald herald; public HingeSoften[] softens; public HingeLine line; public HingePays pays;
    public List<Dictionary<string, object>> instead;                // chronicle rows as written: Chronicle's own reader takes them later
    public string[] drops = new string[0]; public HoldRow[] holds = new HoldRow[0];
    public List<Effect> effects; public WorldEffect[] world = new WorldEffect[0];
    public string Flag { get; }                                     // "changed_" + id
}
public sealed class Hinges
{
    public Hinges();
    public static Hinges Current { get; }
    public static Hinges Load();
    public IReadOnlyList<HingeDef> All { get; }                     // by day
    public HingeDef Hinge(string id);                               // null
    public HingeDef OfEntry(string chronicleEntryId);               // null
    public List<HingeDef> InAge(string ageId);
    public IReadOnlyList<FixedRow> Fixed { get; }                   // what cannot be turned (what, why, says, from, to)
}

// ---- Campaign.cs
public sealed class StartDef
{
    public string id = "", name = "", port = "", burden = "", age = "", firstCard = "", picture = "", source = "";
    public int year, month, day; public double hour;
    public string people = "", faith = "", isA = "";                // who.people, who.faith, who.is
    public Dictionary<string, double> tongues, coins, standingPorts, standingRegions;
    public string shipClass = "", shipOwnedNote = "", shipNameFrom = ""; public bool shipOwned; public double shipState = 1.0;
    public int crewMen; public string[] cast = new string[0];
    public double debtD; public string debtTo = "", debtChain = ""; public int debtDueKey;
    public KeyValuePair<string, int>[] hold;                        // good id, units
    public double storesDays, newsReachKm;
    public string[] knownPorts = new string[0], texts = new string[0], flags = new string[0];
    public string opensWhenText = "", opensWords = "", readyNote = ""; public Cond opensWhen = Cond.True;
    public bool readyPlaces, readyChronicle, readyRaiders;
    public string Flag { get; }                                     // "start_" + id
}
public sealed class HouseEnding { public string id = "", name = "", whenText = "", text = "", text2 = "", text3 = "", picture = "", source = "", bookPage = "";
                                  public int rank; public Cond when = Cond.True; public List<Effect> effects; public string[] bookOpens = new string[0]; }
public sealed class WorldEnding { public string id = "", whenText = "", text = "", source = ""; public bool real; public Cond when = Cond.True; }
public sealed class Campaign
{
    public Campaign();
    public static Campaign Current { get; }
    public static Campaign Load();
    public IReadOnlyList<StartDef> Starts { get; }
    public StartDef Start(string id);                               // null
    public List<StartDef> Offered();                                // those with readyPlaces
    public IReadOnlyList<HouseEnding> HouseEndings { get; }         // the highest rank first
    public IReadOnlyList<WorldEnding> WorldEndings { get; }         // in the file's order
    public HouseEnding WeighHouse();                                // the first, by rank, whose when holds now; null with no file
    public WorldEnding WeighWorld();                                // the first whose when holds; null with no file
    public int LastDayKey { get; }                                  // when_weighed.last_day; DayKey(1066, 12, 25) with no file
    public Cond Early { get; }                                      // when_weighed.early; Never with no file
    public IReadOnlyDictionary<string, string> Still { get; }       // places.still: thriving, steady, thinning -> a line
}

// ---- Storyteller.cs
public sealed class JobLine    { public string whenText = "", text = ""; public Cond when = Cond.True; }
public sealed class JobOutcome { public List<Effect> effects = new List<Effect>(); public WorldEffect[] world = new WorldEffect[0]; }
public sealed class JobDef
{
    public string id = "", name = "", shape = "", cause = "", lane = "", asker = "", whenText = "", who = "", to = "", about = "", source = "";
    public int heat, openAtAPort; public Cond when = Cond.True; public string[] goods = new string[0];
    public double unitsLow, unitsHigh, unitsA100Heads, premiumLow, premiumHigh, daysLow, daysHigh, payLow, payHigh, freshDays, repeatDays;
    public JobOutcome kept, late, broken, unfilled;
    public JobLine[] offer = new JobLine[0], keptLines = new JobLine[0], brokenLines = new JobLine[0];
    public static string Say(JobLine[] lines);                      // the first whose when holds now; ""
}
public sealed class TellerRow  { public string id = "", lane = "", what = "", reference = "", whenText = "", note = "", source = "";
                                 public Cond when = Cond.True; public double weight, cooldownDays; public int heat; public string[] ages = new string[0]; }
public sealed class Storyteller
{
    public Storyteller();
    public static Storyteller Current { get; }
    public static Storyteller Load();
    public int HeatOf(string cardId);                               // the first heat rule that matches ("*" is any run); the default (1)
    public IReadOnlyList<JobDef> Jobs { get; }
    public JobDef Job(string id);                                   // null
    public IReadOnlyList<TellerRow> Table { get; }
    public double Number(string block, string key, double fallback);   // "leg", "after_card_hours": the teller's numbers
    public Dictionary<string, object> Block(string block);          // "stages", "pulse", "season", "lanes", "patience", "caps" as parsed; null
}
```

- **`Line(year)`**: rows are `(year, best)`; between two rows a straight line; before the first, the first; after
  the last, the last.
- **`HolderOf(port, y, m, d)`**: the realm of the last `holds` row for the port whose `fromKey` is at or before
  the day and whose `toKey` is 0 or after it; with none, the port's `realm` in `ports_793.json`
  (`GameData.Port(port)?.realm`); "" for a port nobody knows. A `from` that is a year alone is 1 January; a `to`
  that is a year alone is 31 December.
- A settlement file has `"region"` at the top and may have `"region"` on each row too (the real file does): the
  row's wins.
- A job row that is only `{ "id": ..., "see": ... }` (the worked example's stand-in) is skipped without a warning.
- `effects` everywhere go through `ContentRules.ParseEffects`; `world` through `WorldEffect.ParseList`; every
  `when`, `turned` and `check` through `ContentRules.ParseWhen`. A `turned` or `when` with `{war}` or `{port}` in
  it (the war templates) is **not** parsed here: templates keep their rows raw.
- **Called by (all later, in the living world's own steps):** `Market` for a town's size and a hungry price;
  `EncounterRunner.PortInfo` for `port.realm` (a wiring edit: `Wars.HolderOf`); `Commissions` for jobs;
  `Chronicle.Advance` for the hinges' gate; `KapmanGame.Awake` for a start; `Scheduler` for the teller's heat. For
  the HUD: the town panel's line of people ("About 2,400 souls"), the World panel (who holds what, each war's
  phase), the Chronicle's "Otherwise" heading.
- **Tests:** the SCHEMAS.md examples as fixtures, one per book: Dorestad's line at 793, between 834 and 838, and
  after 1042; `HolderOf("dorestad", 851, 1, 1)` is `rorik` and `franks` in 849; `delta_834.PhaseOn` on a day
  between two phases; a hinge's `turned` with flags set and not; `WeighHouse` picks the highest rank that holds;
  `HeatOf("sea.raider.x")` is 3; every world verb of rule 8 parses and "people dorestad -3%" is a share. On the
  real data: `Settlements.Of("dorestad")` is there and its line in 793 is between 2,000 and 3,000.

### (h) The year: `Content/Year/YearBook.cs`

Reads `story/ages_793.json` ("ages": 7), `story/sayings_793.json` ("sayings": 530, "kinds": 8) and
`story/winter_793.json` ("reckoning", "one_thing", "cards", "writing", "flags", "inheritance", "remembered").

```csharp
public sealed class AgeRuler  { public string where = "", realm = "", who = "", years = ""; }
public sealed class AgeChange { public int year; public string kind = "", what = "", source = ""; }
public sealed class AgeStory  { public string id = "", title = "", chronicle = "", text = "", source = ""; public int year; public string[] where = new string[0]; }
public sealed class AgeDef
{
    public string id = "", name = "", world = "", trade = "", danger = "";
    public int number, from, to;                                    // number: 1 to 7, its place in the file
    public AgeRuler[] rulers; public AgeChange[] news, gone; public AgeStory[] stories;      // news = "new"
    public string[] goodsMore, goodsLess, portsRise, portsFall, portsLacking, sources;
    public bool Has(int year);
}
public sealed class SayingDef
{
    public string id = "", kind = "", text = "", answer = "", who = "", whenText = "", condText = "", source = "", textId = "";
    public string[] people = new string[0];                         // empty: anybody says it
    public int from, to;                                            // the years it is said in (71 rows have them); 0: any
    public bool real, toldLater, toCheck;
    public Cond cond = Cond.True;                                   // from "cond" (66 rows have one); none: always
    public bool SaidBy(string people);
    public bool SaidIn(int year);
}
public sealed class WinterChoice   { public string id = "", name = "", sets = "", ready = "", costText = ""; public List<Effect> cost; public bool costUnderstood = true; }
public sealed class WinterYear     { public string id = "", whenText = ""; public string[] lines = new string[0]; }   // good, middling, bad
public sealed class WinterAlso     { public string whenText = "", line = ""; }
public sealed class RememberedLife { public int rank; public string id = "", name = "", whenText = "", also = "", entry = ""; public Cond when; }
public sealed class InheritRow     { public string what = "", rule = "", d = "", source = ""; }
public sealed class HeirRank       { public int rank; public string heir = "", whenText = "", note = ""; public Cond when; }

public sealed class YearBook
{
    public YearBook();
    public static YearBook Current { get; }
    public static YearBook Load();

    // the ages
    public IReadOnlyList<AgeDef> Ages { get; }
    public AgeDef Age(string id);                                   // null
    public AgeDef AgeOf(int year);                                  // before the first: the first; after the last: the last; null only with no file
    public int AgeNumber(int year);                                 // 1 to 7; 0 with no file
    public bool Turned(int fromYear, int toYear, out AgeDef entered);   // did an age begin after fromYear, at or before toYear
    public static AgeDef Now { get; }                               // AgeOf(the game's year); null with no game or no file
    public static void SyncFlags();                                 // sets the flag "age" to the age's number when it differs

    // sayings
    public IReadOnlyList<SayingDef> Sayings { get; }
    public IReadOnlyList<string> SayingKinds { get; }
    public SayingDef Saying(string id);                             // null
    public List<SayingDef> SayingsFor(string people, string kind = null);   // said by that people (or by anybody), of that kind (null: any), in the game's year, whose cond holds now
    public SayingDef SayingFor(string people, string kind = null, int seed = 0);   // one of them, the same for the same seed; null if none

    // the winter
    public IReadOnlyList<WinterChoice> OneThing { get; }
    public WinterYear Reckoning(string id);                         // "good" | "middling" | "bad"; null
    public string[] DebtLines(string id);                           // "paid" | "part" | "unpaid"; empty
    public IReadOnlyList<WinterAlso> Also { get; }
    public IReadOnlyList<RememberedLife> Lives { get; }             // by rank
    public RememberedLife RememberedAs();                           // the first, by rank, whose when holds now; null
    public IReadOnlyList<HeirRank> Heirs { get; }
    public IReadOnlyList<InheritRow> Keeps { get; }
    public IReadOnlyList<InheritRow> Pays { get; }
    public IReadOnlyList<InheritRow> Loses { get; }
    public IReadOnlyList<string> FlagsCleared { get; }              // inheritance.flags_cleared.flags
    public IReadOnlyList<string> InheritLines { get; }
    public bool HasWinterCards { get; }
    public const string WinterFile = "story/winter_793.json";       // its "cards" are in the cards' own format
    public IReadOnlyDictionary<string, string> WinterWriting { get; }   // writing.entries: key -> text
}
```

- **`cond`, not `when`:** a saying's `when` is prose for a person; `cond` is the condition. `SayingsFor` with no
  game treats a saying with a `cond` as not fitting (nothing can be asked), and one without as fitting.
- The winter's prose conditions (`reckoning.years.*.when`, `also[].when`, some `remembered` rows): use
  `ContentRules.TryParseWhen`; where it is false keep the text and set `when` to `ContentRules.Never`. The
  reckoning's three years are chosen by the winter board's own sums, not by these lines.
- A pick by seed: `Kapman.Game.Sim.SimRandom.For(seed, "saying/" + people + "/" + kind)` over the list, so a
  saved game says the same line again.
- **Called by:** the day stick and the Chronicle panel (the age's name and what is new in it); `PortLife.Talk`
  and the speech place (a saying that fits the speaker's people and the weather); `KapmanGame.YearPassed`
  (`SyncFlags`, a wiring edit); the winter board (steps 16 and 20 of `depth_design.md`); `EncounterRunner.LoadBook`
  for the winter's cards (a wiring edit: section 4, W1).
- **Tests:** fixtures for the ages' edges (792, 793, 833, 834, 1066, 1070), `Turned(833, 834)`, a saying by
  people, kind and `cond` with a director's variables, the same seed twice. On the real data: 7 ages, age 1 is
  793 to 833, at least 500 sayings, at least 60 with a `cond` and every `cond` parses, 40 winter cards, 10 things
  to build over a winter.

## 4. Wiring edits to code that exists (only an agent that runs Unity makes these)

The loaders add files and change none. These are the edits that make the game call them. Each is small, each needs
a compile and the tests, so each belongs to a HUD step or to the integrator before it. In order of use:

| # | Edit | Where | Needs | Who |
|---|---|---|---|---|
| W1 | The winter's cards load with the others: add `(YearBook.WinterFile, text)` to the card sources when the file is there, and put `YearBook.Current.WinterWriting` into `book.writing` for keys it lacks. The same two lines in the tests' `EncounterData.Sources()` | `EncounterRunner.LoadBook`, `Tests/EditMode/EncounterData.cs` | (h) | integrator. Safe today: 0 new faults. They never come until a winter screen sets `flag wintering` |
| W2 | The roster's classes join the world's: for every `ShipBook.Current.Ships`, if `SimData.classes` has no such id, add `ship.cls` | `KapmanGame.MakeWorld`, after `SimData.Load` | (b) | integrator |
| W3 | `flag age` follows the year: `YearBook.SyncFlags()` on `KapmanGame.YearPassed` and once after the director is made | `EncounterRunner.EnsureDirector` | (h) | integrator |
| W4 | A charter waives the toll: where `Deal.Sell` multiplies by `EncounterRunner.TollFactor(port, day)`, take 0 when `UnlockRegistry.Current.HasCharter(port)` | `Trade/Deal.cs` | (d) | HUD step 3 |
| W5 | The town panel replaces the port menu's "Explore the town" and "Talk to people" | `Code/Graphs/PortNodes.cs`, `MapView.ExploreTown` | (a), (c) | HUD step 3 |
| W6 | The chart names the world: `Gazetteer.Current.InView` in `MapView.Draw`, tier by `Gazetteer.TierFor(mpp)`, skipping the ports it draws itself | `Map/MapView.cs` | (f) | HUD step 6 |
| W7 | `port.realm` reads the holds | `EncounterRunner.PortInfo` | (g), and wars files written | the living world's step 9 |
| W8 | New condition names the town acts wait for (`sunday`, `dark`, `talks_left`, `port.size`, `port.zone`, `fair`), and live answers for `port.people` and `port.faith` | `Core/Encounters/Cond.cs` (`VarNames`), `EncounterGameVars` | none | HUD step 3, when an act's `when` first uses one. Today no file does |
| W9 | The words of 493 keys: run `Bake/content/merge_writing.py` when the writers are done | data | none | the user, or the integrator on the user's word |

Nothing here edits `SaveGame.cs`: a part that keeps state registers with `ContentSave`.

## 5. The HUD steps, in order

Design: `Design/depth_design.md`. One agent at a time, each running Unity, each leaving `ChartGameTests` and the
EditMode suite passing. **All six:** do not change a signature of section 1 or 3 (add beside it, and say so);
do not edit `Assets/Kapman/Game/Data`, `Packages` or `ProjectSettings`; do not change the cards' grammar; do not
print a number the design says is a word; no sound that is not a recording on disk with its credit line.

| # | Step | Reads from the content | Must not touch |
|---|---|---|---|
| 1 | **The frame.** `FrameRoot` with its named places, the settings block, the button rack, the dock, in place of `MapTable`'s bar and outliner; `MarketPanel`, `ChestPanel`, `EncounterPanel`, `TextPanel` moved into its places | Buttons that have arrived are flags (`hud_hold`, `hud_tasks` ...) through `ContentFlags`, so they are saved and an heir keeps them. `YearBook.Now` for the date's tooltip | How the chart is drawn (`MapRenderer`, `MapArt`): only where it may draw (`ChartSafeRect`). The panels' insides. Any loader |
| 2 | **Voyage and measure.** The day stick, the course's days on it, the bread line, the word that sets a course (depth_design 2.3); the chart's measures that exist (dividers, pin, string: `MapView.Tools.cs`) given their place in the frame | `ShipBook.Fitted` for her speeds; `Gazetteer.Near` for "a known landing within 8 km"; `YearBook` for feasts' and ages' names; the debt's day from `Trade` | `WorldSim`'s own sailing rules. `Market`. The frame's places (add signs to the stick, not new places) |
| 3 | **Trade and people.** The town panel (sites as lines), the site panel (who is there, what can be done), the portrait and the speech place, the market in the dock | `PlaceBook.Sites`, `PeopleAt`, `Era`; `Town.Menu(port, site.kind, site.doRaw)`, `Town.Do`; `ShipBook.RosterAt` joined with `PlaceBook.Ships` for a yard; `UnlockRegistry.Offered` and `Win`; `Settlements.Of(port).Line(year)` for the line of people; `YearBook.SayingFor(person.people)`; `Director.Offer(cardId)` for a site's card. Wiring edits W4, W5, W8 | The cards' engine. `Market`'s prices (named buyers and the haggle are depth_design steps 7 and after). The loaders' files |
| 4 | **Tasks and events.** The Tasks panel, the Chronicle panel, the log and the hint line (`NoticeBus`), cards at sea in the chart scene | `ChainJournal.Afoot()` and `Ended()`; `Commissions.All`; `Storyteller.Jobs` when jobs are posted; `Chronicle`; `YearBook.Ages`; `Hinges` for the "Otherwise" heading; `ContentFlags.Version` to know when to refresh | `EncounterState`'s meaning of a flag. `Scheduler`'s draws. The town panel |
| 5 | **The ship window.** Her picture on its small stage, the caption | `ShipBook.Ship(id)`, `Fitted` with `UnlockRegistry.Held("upgrade")` for what is seen on her; the hold's load from `Trade.Hold` | The 3D levels' cameras and water. Budget: stop and say so above 2 ms a frame |
| 6 | **The whole-world map.** Names and signs by zoom, the routes, who holds what | `Gazetteer.InView` and `TierFor`; `Routes.InView`; `Wars.HolderOf` and `Wars.Running`; `Settlements` for a town's size sign; `Exploration` for the fog (a place under fog is not named). Wiring edit W6 | The ports' own marks and cards on the chart. `MapArt`'s tiles. The fog's rules |

What each later step may assume of the earlier: after step 1 every panel opens in the dock and holds the clock
(`PanelClock`); after step 3 a port call is played through sites; after step 4 a card can open at sea.

## 6. How a built game gets the data

`ContentFiles` reads `StreamingAssets/Data` outside the editor. `Assets/Kapman/Editor/Game/ContentBuild.cs` is an
`IPreprocessBuildWithReport`: before every build it copies the whole of `Assets/Kapman/Game/Data` (every file but
`.meta`, hidden and half-saved ones) to `Assets/StreamingAssets/Data`, and as an `IPostprocessBuildWithReport` it
takes the copy away again, so the project never keeps two copies of the writers' files. If a build fails part-way
the copy stays: menu **Kapman > Game > Remove the content copy**. By hand: **Kapman > Game > Copy the content
for a build**. `GameFiles.Find` (the 22 tables) and the cards' folder fall back to the same place, so a build needs
nothing assigned on `KapmanGame`. Not tried in a real player build yet: the copy is tested, the build is not.

## 7. Known gaps, said plainly

- **No routes file exists** and no brief fixes one: (f) reads three likely shapes. When a routes writer starts,
  give him section (f)'s shapes.
- **Wars, hinges, starts, endings and the storyteller have no file yet.** (g) is written from SCHEMAS.md's worked
  examples; expect small differences when the real files land, and one more pass.
- **`world` effects are parsed and not applied.** Applying them is the living world's step 6.
- **The acts' numbers are not all in the engine's verbs**: 12 of the 53 acts do their work in words under
  `gives` (mending a hull, a store by port, the law). `Town.Do` runs what the verbs say and no more; the rest is
  `town_interactivity.md` section 8, steps 6 to 11.
- **`unlocks_793.json` and `ships_793.json` disagree** on four prices and three years (`ships_793.json` "rules"
  lists them). `ShipBook` reads the roster's; `UnlockRegistry` reads the unlocks'. The yard should show the
  roster's. One of the two files should give way: the user's call.
- **A flag that lapses by its day** does not move `ContentFlags.Version` until the next hourly draw drops it. A
  list kept from the flags can be an hour stale for such a flag.
- **Cards with a fault stop all cards** (as before): `EncounterRunner` reports them and no card opens. The content
  outside the cards still loads, and `ContentRules.Words` still answers; `ContentRules.Run` cannot run.

## A ruling from the user, 7 Oct, after the builders were started: READ THIS IF YOU ARE BUILDING THE FRAME

"The panels must be separate function from the 8 buttons above them."

So the left side has three things, top to bottom, and they are three different things:

1. **The settings bar** (menu, date, silver, bread, the clock).
2. **Eight function buttons**, in a rack of four by two under it. Each DOES one thing when pressed. They are verbs. They
   do not open, close or switch the windows below. In the mockup (Design/ui_mock/template3.html, layout G, the array FX
   and the function doFx): Cast off (Lie to when at sea), Wait (till first light, the flood, a fair wind), Measure (the
   way in days: click two places on the chart), Look out, Sound (the lead), Hail, Rest, Offer. A press either acts at
   once and writes a line in the log, or opens a small floating window with the choices. A button that cannot be used
   now is greyed, with a reason on hover.
3. **The tiled windows** (workspaces; rows, columns or tabs; any window holds any panel). These show and set things.
   Their own titles, arrows and the bar under them are the only way to change which panels are shown.

If the frame is already built without the rack, add it: it is one small element and eight calls into code that mostly
exists (ShipMotion and VoyagePlanner for cast off and lie to; TideWait and Rest for waiting; the measure; the lookout
in Game/Sim; the rest may write an honest "not yet" line to the log until their rules exist).

## 8. The command frame as built on 7 October (first builder): what is there, and how to add to it

Layout G of `Design/ui_mock/template3.html` is in the game, in `L_Chart` only: the object `--- GAME --- / Frame`, added
by `Assets/Kapman/Editor/Game/ChartBuilder.cs` (after changing the builder, rebuild the scene with
`-executeMethod Kapman.EditorTools.ChartBuilder.BuildBatch -quit`). `Assets/Kapman/Tests/PlayMode/ChartGameTests.cs`
plays the whole of it from the frame and saves eleven pictures to `Captures/chart`. Open them after every change: they
are what the player sees.

### 8.1 Where things are

| What | File |
|---|---|
| The frame: top bar, tiled windows, portrait, ship window, log, keys | `Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs` |
| Floating windows (cards, talk, urgent alerts, anything else) | `Frame/FrameRoot.Floats.cs` |
| The measure, and a course drawn on the chart for a window | `Frame/FrameRoot.Measure.cs` |
| The eight function buttons, and waiting | `Frame/FrameRoot.Rack.cs` |
| The windows' state (workspaces, layouts, focus): plain C#, tested | `Frame/FrameWindows.cs` |
| A panel's base class, the registry, the parts panels are built from | `Frame/FramePanel.cs` (`FramePanel`, `FramePanels`, `FrameUi`) |
| What the frame reads of the game, in plain words; the log | `Frame/FrameGame.cs` (`FrameGame`, `FrameLog`) |
| Days of a passage, low and high; the way by water between two places | `Frame/FramePassage.cs` |
| The two colours that follow the world's palette | `Frame/FrameColours.cs` |
| A picture by its slot (`Art/final`, else `Art/placeholder`) | `Frame/FrameArt.cs` |
| The drawn stand-ins: face place, ship window, small signs | `Frame/FrameDrawn.cs`, `Frame/FrameIcon.cs` |
| The panels: Voyage, Trade, Port, Hold, Tasks, The word, Chronicle, Event | `Frame/Panels/*.cs` |
| The look | `Assets/Kapman/Game/UI/Frame.uss` |

### 8.2 To add a panel: one file

Put a class in `Assets/Kapman/Code/Game/UI/Frame/Panels/`. Nothing else has to be touched. `FramePanels` finds every
class that inherits `FramePanel` and has a public constructor that takes nothing; it then turns up in every window's
list of panels and can be stepped to.

```csharp
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    public sealed class CrewPanel : FramePanel
    {
        public override string Id => "crew";        // never changes: the saved layout keeps it
        public override string Title => "Crew";     // what the window's title says
        public override int Order => 35;            // its place in the list (the first eight are 10 to 70)

        // a number that changes when what the panel shows has changed. The frame asks four times a second and
        // calls Fill again only when it has moved, so a click is never lost to a redraw
        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(17, Voyage.Men), (int)(Voyage.Morale * 20));

        protected override void Fill(VisualElement body)
        {
            body.Add(FrameUi.H("Four men"));
            body.Add(FrameUi.Ln("Aldgrim", "steersman, of Wijnaldum", FrameUi.Pips(3)));
            body.Add(FrameUi.Ln("Pay owed", null, FrameUi.Num($"{Voyage.PayOwed:0} d")));
            body.Add(FrameUi.Row(FrameUi.Btn("Rations: full", () => { /* set it */ Redraw(); })));
        }
    }
}
```

- `FrameUi` has the parts: `H` a heading, `P` small print, `Text` reading text, `Ln(name, small, right...)` a line,
  `LnButton` a line that is a button, `Num` a figure in gold (what is the player's to spend or count), `Tag`, `Btn`,
  `Go` (the one button a panel is for), `Row`, `Pips`. `new WordLine(Known.Word(good, port, ask))` is the word on a good.
- One object is made of each panel and lives for the play, so it may keep what the player picked (see `VoyagePanel`).
- `Live()` is called between redraws for a figure that runs all the time; `OnShown()` and `OnHidden()` when a window
  takes the panel up or lets it go; `Frame` is the frame (`Frame.OpenFloat`, `Frame.ShowPlanned`, `Frame.Show`).
- Do not name a method of a panel after a class it uses (`Deal`, `How`): the method hides the class and it will not compile.
- To have a workspace show it from the start, name its id in `FrameWindows.Defaults` (four workspaces: name, layout,
  panels). A workspace may name a panel that is not built yet: it is left out until it is. The third workspace is
  called News for now (The word, Chronicle, Event); the mockup's is Self, for when those panels exist.
- The ruling of 7 October holds: a window SHOWS and SETS. A thing that is DONE (cast off, wait, hail) is a function
  button's (8.4), not a window's. So the Voyage window sets the course and does not cast off.

### 8.3 To open a floating window

```csharp
FrameRoot.Current.OpenFloat(new FloatSpec
{
    id = "priest",                        // one window an id: open it again to change it in place
    title = "Liudger, the priest", who = "At the church, has a letter from York",
    speaker = "Liudger", speakerLine = "The priest of Dorestat",   // he stands in the portrait's place while he speaks
    face = "liudger",                     // a face place (null: none)
    lines = { ("Qui descendunt mare in navibus.", true) },
    small = "Latin. Psalm 106.",
    options =
    {
        new FloatOption { label = "Ask his blessing on the crossing", odds = "sure", pick = () => { /* ... */ } },
        new FloatOption { label = "Answer with the next verse", tag = "Lore", odds = "doubtful", enabled = false, rule = "You have not the Latin." },
    },
    mustAnswer = false,                   // true: no cross, Esc is refused
    holdsClock = false,                   // true: the clock waits until it is answered, or until the player goes on
});
```

`content` takes any element of your own in place of the lines: the line that turns from a tongue into English belongs
there. Cards, talk and urgent alerts open by themselves (`CardFloat`, `TalkFloat`, `AlertFloats`): a talk is whatever the
graphs put in `TextPanel` that is not the port's own menu. Several windows may be up; each new one stands a little
down and right of the last; a click brings one forward; 1 to 9 choose on the front one, Enter takes its marked choice,
Esc shuts it if it may be shut.

### 8.4 The eight function buttons (`FrameRoot.Rack.cs`)

Cast off (in port, with a course set; at sea it is Lie to, then Sail on), Wait (till first light, till a fair wind:
`FrameWait`), Measure, Look out and Sound are real. **Hail, Rest and Offer write "not yet" in the log**: their rules are
not in the game. To make one real, change its case in `FrameRoot.Look` (its name, whether it can be pressed, the reason
on hover) and in `FrameRoot.Do` (what a press does). A button that cannot be used now is greyed but not switched off:
it says why on hover and in the log when pressed.

### 8.5 The clock (the rulings of 7 October in `Design/DECISIONS.md`)

- **Only a card stops the clock.** With the frame up, `PanelClock.OnlyCards` is set: no tiled window, and not the
  port's menu, holds time. In port the clock runs. What holds it is a floating window with `holdsClock` (a card until
  it is answered; a talk with no way back).
- **"It stops the clock but you can override it immediately."** Space or a speed button with a card up sets the clock
  running and the card waits, still open (`PanelClock.GoOn`, called by `GameSpeed.TogglePause` and `SetLevel`). The
  Pause button pauses.
- **No hours are skipped.** `Rest.RateCap` and `TideWait.RateCap` are set, so resting ([Z] at night) and the wait for a
  tide keep the clock's own pace at the chosen speed. The clock's dev keys (T, Y, U, Tab) are off on the chart.
- A course set in port waits for the Cast off button (`VoyagePlanner.Moored`). Before, the planner's order to steer
  un-moored her at the berth, and with the clock running in port she would have sailed.

### 8.6 Other things a builder will want

- A line in the log: `FrameLog.Add("...")`. `Voyage.Say` goes there too.
- "Is she in port?": `FrameGame.PortHere` (a `PortDef`, or null at sea). Do not ask `Port.LyingId()` alone: lying to
  off a port she has cast off from she is still "anchored" inside its reach.
- A course drawn on the chart for a window: `Frame.ShowPlanned(line)`; `null` takes it away. `MapView.ShowBoth(a, b)`
  brings two places into the chart's free middle; `MapView.Follow()` goes back to her.
- Days for a way of so many km: `FramePassage.DaysWords(km)`, or `FramePassage.Days(km)` for the two figures.
- A click on a port's mark picks that port in the Voyage window (`MapView.PortClicked`); with the measure in hand it is
  one of the measure's two places. `MapView.Clicked` is a click on the chart itself.
- A picture by its slot: `FrameArt.Load("portrait/person.dorestad.tollman.png")` (the file names of `Art/art_list.csv`).
  `new FrameFace("frame-face") { Slot = ... }` shows it; with no picture it is an empty shape.
- The palette: `FrameColours.EdgeNow` and `AccentNow` in code; `var(--frame-edge)` and `var(--frame-accent)` in the
  stylesheet. A new palette in `MapPalette.All` needs its class in `Frame.uss` (`FrameWindowsTests` says so if it is
  missing).
- Every rule in `Frame.uss` begins `.frame-root .frame-...`: it has to, to outrank the plain reset of labels and
  buttons at the top of the file. The panels are solid, not glass: blended in linear light a dark panel that is a
  little see-through lets the bright chart show as a ghost.
- In batch mode `Time.unscaledDeltaTime` stands still while the game's clock is held. The frame's timers use
  `Time.realtimeSinceStartup`.
- With Painter2D, a path that has curves is closed with a line back to its start, not with `ClosePath`
  (`FrameShipView.Start`): the plain way drew a wedge from the element's corner.

### 8.7 Cards on the chart (`EncounterRunner`)

A mooring at every port; a call begins about two real seconds after the port's graph has laid her to; a new game's call
at home is in progress and quiet (`BeginCallQuietly`). The pace: one drawn card a call beside the arrival set, and at
sea none within four game hours of the last (`chartSeaGapHours` on the runner). A card file with faults is shut alone
(`EncounterRunner.ShutFiles` names it, and the log has an error line). `wind`, `storm` and `fog` come from
`WorldSim.WeatherAt`; `level_kind` is `river_place` or `ocean` by the water she is on; `ice` still reads 0 on the chart.

### 8.8 What is not done, said plainly

- **The Wiki panel and the links to it** (rulings 3 and 6 of 7 October) are not built. Register it as `wiki`.
- **Faces.** The player's portrait comes from its slot (`portrait/self.<people>.man.png`: the game does not know yet
  whether the player is a man or a woman). A card's speaker has no slot: the cards' people (`NpcRegistry`, named from
  the name pool) are not joined to the places files' people, so the toll man at Dorestat is "Liudbrand" on the card
  and "Aldgrim" in the art list. Until they are joined a speaker's face is an empty shape.
- **The frame's own pieces** (`Art/placeholder/hud`, `Art/hud_assign.json`) are not used: the frame is drawn by the
  stylesheet, and the function buttons' signs are drawn in code (`FrameIcon`). Skinning is a later step, as ruled.
- **The old market's Writings tab** (riddles and texts to buy, a monk to read to you, lessons in letters) has no place
  in the frame yet. `MarketPanel` is whole and unused on the chart.
- **Meetings at sea, raiders, the king's guard boats and the events of `events_793.json` do not happen on the chart.**
  Each still waits for "the map to be closed" (`MapView.IsOpen`), and on the chart it never is. This was so before the
  frame. They would now show as floating windows by themselves (they speak through `TextPanel`); what is missing is
  the switch at five places (`GameEvents.Look`, `Encounter.CanOffer`, `Patrols.LookAtSea`, `RaiderEncounter.Look` twice)
  and a pace for the Rhine, where a sail is made out every few minutes. Hail would be the hand-made way in.
- **The chest** opens on [I] as before, over the chart, and the frame steps aside for it. It is not a panel.
- **The ship window** is a drawn stand-in, as ruled. `Art/placeholder/shipview` is not used yet.
- **Passengers**: the game carries none. The Tasks window says so.
- **`ice`** reads 0 on the chart, and the 4,426 writing keys with no words still show as `[write: key]` on a card.

### 8.9 Rulings 7 to 12 of 7 October came after this was built: none of them is in yet

They are in `Design/DECISIONS.md`. Where each would go in what is here:

- **7, the boat and the chart change places.** The ship window is `FrameShipView` in the box `frame-ship`
  (`FrameRoot.Build`). There is no button on it and no small chart.
- **8, chart things on the right, the measure out of the eight buttons, Oars in.** The eight are the list in
  `FrameRoot.BuildRack`, with their cases in `Look` and `Do`; the measure's own slip is `ruleBox` in
  `FrameRoot.Measure.cs` (it stands at the chart's top left while the measure is in hand). The measure itself
  (`SetMeasuring`, `MeasureAt`) does not care where its button is.
- **9, every panel a brief face and a full window.** `FramePanel.Fill` is the full face and is all there is. A brief
  face would be a second method on `FramePanel`; the full window is `OpenFloat` with the panel's body as `content`.
- **10 and 12, a card left unanswered answers itself** (an ordinary one after seven days of running clock, a pressing
  one sooner, holding the ship). A card whose hold the player let go is marked in `FrameRoot.Floats.cs` (`Float.released`,
  set by `PanelClock.LetGo`): the days would be counted from there. The cards' files have no urgency and name no option
  to take, so the cards' grammar comes first.
- **11, the 3D view of the ship** is the lead's.

### 8.10 The first builder of the queue (7 October, afternoon): made steady, the sea, the saves

`ChartGameTests` now plays through the screen itself and saves about eighty pictures to `Captures/chart`. EditMode:
383 tests (381 pass, 2 skipped). What 8.8 says of meetings at sea and of the Hail button is no longer so.

| What | Where |
|---|---|
| The player's hands and eyes for a play test: a click is a pointer pressed where the thing is (and fails if it is covered), a key goes through the input system, every word on screen is read at each picture | `Tests/PlayMode/ChartPlay.cs` |
| Four ports reached, the Lek out by the Maas mouth: twelve edges made river after the lanes load | `Code/Game/Sim/LaneOpenings.cs`, called in `SimRoutes.Graph` (`Sim/Travel.cs`) |
| "Away from the sea": the one switch for what waits while the player is at the map table (never on the chart) | `MapView.AwayFromSea` |
| A happening of the events table as a window of the frame's own | `Frame/FrameRoot.Events.cs`; `GameEvents.Show` |
| Nothing holds her for ever: a time for each meeting, chase and search, and a hold with no talk let go | `Code/Game/Sim/SeaHolds.cs` (`HoldWatch` is the arithmetic) |
| The game saves itself: on mooring, on quit, the last three kept | `Code/Game/Autosave.cs`; hooks in `EncounterRunner.Update`, `KapmanGame.OnApplicationQuit`, `MainMenu.Quit` |
| What the frame keeps in a saved game: the log, the windows and sizes, what was asked, what she waits for | `Frame/FrameSave.cs` (ContentSave part `frame`) |

**The sea.**
- A ship met, raiders and the king's men still speak through the graphs and `TextPanel`; the frame shows each as the
  `talk` window. A happening of the events table does not: it is the frame's own window (`event`), so it may come over
  the port's menu.
- A graph's wait "for a choice, or so many seconds" counts the game's seconds on the chart
  (`TextPanel.WaitsByGameClock`): none while the clock is held. Sixty seconds is an hour of the game.
- While a talk at sea is up she does not arrive at a port (`TextPanel.TalkAtSea`, read by the `Ship: Speed` node, whose
  one use is the arrival test). Without it the port's menu came up over the talk and both flows took the same choice.
- Meetings offered by themselves come no oftener than every 12 game hours (`Encounter.ChartOfferGapHours`: my number,
  not ruled). The Hail button hails the nearest sail made out, whatever the pace.
- Times in `SeaHolds.LimitHours`, in game hours of running clock: the king's men 2 (then she heaves to, or lets them
  search); a meeting offered 3; closing with a ship 12; raiders coming 2; a chase 48; raiders alongside 24 (then she
  gives up). A hold whose talk is not on screen is let go after 6 real seconds. These are a floor, not the design.
- **FOR THE BUILDER OF PRESSING CARDS (rulings 10 and 12):** the raider is not yet a pressing card. The threat is
  answered by the graph's own wait (twenty game minutes with the clock running, then she runs), and she sails on
  meanwhile. Where to hold her: `VoyagePlanner.Update`, at the comment "a threat, not yet answered". Where its time
  and its option go: `SeaHolds.LimitHours` and `SeaHolds.End`. The events' window is an ordinary card with no week
  yet: `FrameRoot.Events.cs`.
- The chart's wind and haze now come from the world's weather (`LevelStage.Wind`, `HazeKm`; `GameEvents.Holds`).
  Before, the chase and the events read a south-westerly 5 or 6 m/s for ever, so a calm, a fog and a gale never came.

**The saves.**
- `SaveGame.FolderOverride` names another folder (the play tests use one of their own; a headless run never writes
  among the player's saves).
- `TextPanel.AbandonWaits()` ends every flow of the graphs that waits on the panel. A load and a new game laid at
  another port call it. Before, the old port's menu went on waiting behind the new one and every choice was taken
  twice (`TextPanel.Waiters` counts them: more than one is a fault).
- A load sets `ChartWorld.SaveLoaded`, ends a wait and a held autosave, and tells each panel (`FramePanel.OnLoaded`).
- An open card is saved under `encounters/open` with its cast and its hour; a course with its `kinds`.
- Saving no longer thins the price notes: the day's turn does (`KapmanGame`, the daily tick).
- A window's size is state only (`FrameWindows.SetSize`, `SizeOf`; the class `size-0/1/2` on the window). No button
  sets it and no style reads it: the brief faces' builder's. `FrameSave.OpenFull` is the hook for full windows.

**Faults found by playing it, and mended.** A port clicked on the chart picked nothing (the chart's layer took the
pointer: `MapView.OnDown`). Esc could not shut the pause menu and "1. Resume" also changed the workspace
(`FrameRoot.Keys`). A new game kept the course of the game before (`Course.ClearState`). A talk with someone of a
port stayed up after she sailed (`FrameRoot.PeopleLeftBehind`). The Voyage window built itself again four times a
second while it reckoned. The log did not show its newest line. A Dane began with fog over her own coast and with
Dorestat's prices "seen this morning". The creation window lay over the left column and the portrait.

**Not done, and seen.**
- A full hold at the start: she begins with 6 kg of room, so nothing can be bought until something is sold.
- The Voyage window lists every port she knows of , not only the twelve of the first release.
- The ports' menu still offers "Visit the toll house" and "Exchange" with "(to come)" in their small print.
- `skipper` is said `shipmaster` for a person of the places files (`PersonDef.Role`), but `wording_793.json` still has
  "a {people} skipper" for who told a price (a data file, and `KnownWordTests` pins it).
- Cards drawn at sea may show unwritten lines; the play test lists any it met in the log, and fails on none.
- Clock speed and pause are not saved. Weather is not saved (it follows the date and the seed).
- `lanes.bin` is read from `G:/kapman_unity/data/baked/world`: a built game will not carry it.
- The player's own arrangement of windows is written to the settings only when `keepLayout` is on and the run is not
  a test (`FrameRoot.FreshLayout`).
- The lookout counts a band's men by eye ("about 65 men" in the log) and the raiders' window gives the band's own
  number ("about 60 men"): two figures for one band on one screen.
- Some choices have their odds written into their own words by the writers ("... (sure)"); the window drops them only
  when they repeat the odds word it shows at the line's end.
- The other copies of the project (`Kapman2_ship`, `Kapman2_content`) share one temporary folder with this one. The
  play tests keep their saved games in a folder of their own for each copy (`ChartGameTests.TestSaves`); before, a
  test run in another copy emptied this one's saves in the middle of a test.
- The self is not asked "man or woman" yet, so the portrait is always `portrait/self.<people>.man.png`.

### 8.11 The second builder of the queue (7 October, afternoon): the numbers and the look

Six things: the palette, the money, the text sizes, sound on the chart, the clock's five speeds, and the lines of
the log this part wrote. EditMode: 417 tests (415 pass, 2 skipped). `ChartGameTests`: 6 of 6, 111 pictures in
`Captures/chart`, 26 of them the same screens at 1280x720 (`*_720.png`).

| What | Where |
|---|---|
| **The palette mend** (ruling 15): the seventeen moves, and Byzantium (#702963) exactly once in every palette | `Code/Game/Map/MapPalette.cs`, the `pal-` classes of `Game/UI/Frame.uss`, `Bake/world/recolour_map.py`, `Design/palettes/*.gpl`; test `FrameWindowsTests.EveryPaletteHoldsByzantiumExactlyOnce` |
| **The debt is 1,920 d, due on a day** (rulings 14 and 30): Michaelmas, 29 September 793, shown as the day and the days left; the Tasks and House windows say it is a Sunday | `Game/Data/trade_rules.json` (`start`: `debt`, `due_month`, `due_day`, `due_name`, `world_from`), `Trade/TradeRules.cs`, `Trade/Trade.cs` (`DueDayOf`, `DebtDaysLeft`, `DebtDueName`), `Frame/FrameGame.cs` (`DebtLine`, `DebtDay`, `DebtDayWarning`) |
| **The world's traders at work from 1 April** before the first morning; the first morning is kept on disk so a new game does not wait for it | `KapmanGame.MakeWorld` and `BuildWorld`, `Sim/WorldSim.RunTo`, `Sim/FirstMorning.cs` (kept in the temporary cache, keyed by the tables, the seed, the rules and the code) |
| The traders' least margin is 0.25; famine raises grain only; the English toll is a tenth | `Sim/WorldSim.cs` (`minMargin`), `Game/Data/shocks_793.json`, `trade_rules.json` |
| **The count's four casks are his**: a lot has an owner, a sale of his cask is his stake and not your gain | `Trade/Hold.cs` (`Lot.whose`, `Owed`), `Trade.SaleReckoned`, `Chart/ChartWorld.cs` (`CountsGood`, `CountsOwn`), `Panels/HoldPanel.cs` |
| "40 d or 86 d": both were true. The purse is 85.5 d of silver in four kinds; at Dorestat it buys 81 d's worth. The bar and the Trade window now say what it buys here. A cask asks about 46 d: she can buy one once she has room | `FrameGame.SilverHere`, `SilverWords`, `CostWords`; `Panels/TradePanel.cs`; `MoneyTests` (15 tests) |
| **Four sizes of text**, 18, 22, 26 and 30 at 1080p, as variables; on a smaller screen every size is raised so nothing is under 18 true pixels | `Game/UI/Frame.uss` (`--t-quiet`, `--t-body`, `--t-name`, `--t-title`), `MainMenu.uss`, `MapView.uss`, `Code/Game/UI/TextFloor.cs` (the `t-floor-NN` classes, by the panel's scale); `TextSizeTests`; `ChartPlay.SmallShot` |
| **Sound on the chart**: the 3D levels' soundscape, fed by where she is (port, river, coast, sea), the weather and the hour; quiet beside reading; a line in the menus to turn it down or off | `Code/Game/Chart/ChartSound.cs`, `Runtime/Audio/LevelSoundscape2.cs` (`Feed`, `FeedWind`, `Shut`), `MainMenu.SoundLine`, `Editor/Game/ChartBuilder.cs`; `ChartSoundTests`; only recordings already in `Audio`, each with its credit |
| **The clock: a pause and five speeds** (ruling 26): 60, 240, 1440, 5760 and 21600 game seconds a real second. `+`, `-`, Space | `Code/Game/UI/GameSpeed.cs` (`Rates`, `Faster`, `Slower`, `DropTo`, `Before`, `Cap`) |
| **The game steps its own clock on the chart**, a whole game minute at a time, in one order at every speed. The time scale stays 1 | `Code/Core/Sim/ClockStepper.cs`; `KapmanGame.RunClock`, `StepMinute`, `Sign`, `OneAFrame`; each thing that counts time has a `Step`: `ChartBody`, `VoyagePlanner`, `SeaWatch`, `EncounterRunner`, `FrameWait` |
| **The clock drops back by itself**, and says why; `+` goes back to the speed she had. Eleven reasons, each with a switch in the pause menu, "The clock" | `Code/Game/UI/ClockDrops.cs` (`ClockReason`, `ClockSettings`, `ClockDrops`), `MainMenu.ClockList`, the bar in `FrameRoot.FillTop`, `.frame-k.was` and `.capped` in `Frame.uss` |
| The proof ruling 26 asks for | `ClockStepperTests` (7), `ClockWorldTests` (the world), and in play `ChartGameTests.ThenTheSameSeedEndsTheSameSlowAndAtSpeedFive`: three game days toward Lundenwic, slowly and at speed 5, end the same to the minute |
| A band of raiders that has had its answer leaves her be for twelve game hours | `Sim/RaiderEncounter.cs` (`LeaveBeHours`) |
| A drawn card at sea every 36 game hours at most (it was 4) | `EncounterRunner.chartSeaGapHours`, and the scene |

**How to use the clock from code.** Anything that must count time counts game minutes in a `Step` called from
`KapmanGame.StepMinute`, never real seconds and never `Time.deltaTime`. Anything that puts up a window from a step
must change `KapmanGame.Sign` (a card, a talk, a happening, a hold of the sea already do): the clock then stands two
frames so the window is up before another minute passes. To drop the clock: `ClockDrops.Raise(ClockReason.X)`. A
test in a hurry sets `ClockDrops.Quiet = true`; a test of the switches sets `ClockSettings.AsNew = true`; a test that
runs the clock itself sets `KapmanGame.ClockByHand = true` and calls `RunClock(seconds)`. What 8.5 says of resting
and of the tide keeping "the clock's own pace" is over on the chart: every wait runs at the chosen speed and the
clock drops back when it is over.

**What is not done, said plainly.**

- **The debt chain's cards still say 480 d, "two pounds" and "forty dozen".** The count's tally card comes down to the
  boat at Dorestat and says so, under a bar that says 1,920 d. The chain's ways of paying do not lower
  `Trade.Debt` either. `Bake/economy/apply_debt_sum_cards.py` lists the 56 lines to change and changes them with
  `--apply`. It was not applied: the branch `content` is rewriting those files. Run it after that merge.
- The cap on what a card pays was not applied (as told).
- The explicit test `SimTests.TestRunTenYears` fails at a margin of 0.25 (three traders stuck after ten years) and
  passes at 0.12. It is one of the two skipped tests. The year's run passes.
- Rulings 32 and 33 (the payment at Saint John's, the creditor's man), hard terms, a creditor for each people (every
  start owes "the count"), and the shut market on a Sunday are not built. The warning is only words.
- She begins with a full hold (36 kg free, and a cask is 115 kg), so the first cask can be bought only after one is
  sold.
- The play test at two speeds met two cards and two ships in its three days. No raiders, no guard boat and no port
  call fell in them, so those are proved the same at every speed only by the step's own tests and by reasoning.
- A band of raiders came on again half a game minute after it had its answer. It is now kept off for twelve game
  hours, which is a floor and not a design: the builder of pressing cards will want to rule it.
- The clock: the cards' own waiting time and "a pressing card" are the card clock's (`Design/fixes/card_clock.md`):
  the hooks are named in `EncounterRunner.Step` and `ClockDrops.PressingCard`. Opening the chest does not drop the
  clock. "Any sail made out" is off from the start, because on the Rhine it is every few minutes. In a 3D level
  speeds 3 to 5 are all four times. Her mark on the chart moves once a game minute and is not smoothed between.
- The log at speed 4 and 5 is not folded by the day, bread is still bought by hand, and wages are still told at
  every call (`Design/fixes/clock_five_speeds.md`, section 7).
- Text: the title's own name is 44 px (the one exception). The chest and the old map-table panels keep their old
  sizes. At 1280x720 the three columns are tight, port names on the chart overlap, and the list of the clock's
  switches stands a little over the top bar.
- Sound was never heard: a batch run plays nothing. The levels are set by reckoning (`ChartSound.level = 0.35`).
  There is no recording of wind at sea in the project.
- The first new game after the tables or the code change takes about thirteen seconds to make the first morning;
  after that it is read in a tenth of a second. The pre-run also runs in a 3D level (81 days there).
- `ProjectSettings/EditorBuildSettings.asset` changes when the scene is rebuilt, and was left uncommitted.

## 9. The self, the stats in talk, eleven more panels and the turning line (second builder, 7 October)

`ChartGameTests` now saves twenty-two pictures; 12 to 22 are these. EditMode tests: `SelfTests`, `StatsInTalkTests`,
`MorePanelsTests`, `TongueTests`.

### 9.1 The self

| What | File |
|---|---|
| The record: people, faith, past, five stats, tongues held, words seen, people met; saved under `content/self` | `Code/Game/Self/Self.cs` |
| The tables from `story/self_793.json`, with a small table of its own when the file cannot be read | `Code/Game/Self/SelfBook.cs` |
| The creation window (one page, three choices, a line each) | `Code/Game/UI/Frame/FrameRoot.Self.cs` |
| A new game's people start at their own port | `ChartWorld.StartAt`, `SaveGame.LaidAt` |

- A new game on the chart asks while `Self.Chosen` is false. A loaded game brings its self; a save from before the
  self was kept is the plain start (a Frisian of Dorestat, Christian, no past) and is not asked.
- Offered: the peoples of the first release whose home port is in `ports_793.json` (Frisian, Frank, English, Dane), and
  for each the faiths the data lists as usual for it and of the first release. A click on a people recolours the world
  at once (`MapPalette.SetPeople`).
- Only the place is moved by a start elsewhere. The boat, the first cargo, the debt (480 d, due in 365 days by
  `trade_rules.json`), what she knows of markets, and her standing are the same at every home. The data has a debt,
  a creditor and a first chain for each people (`PeopleDef.debt`, `debtTo`, `debtDue` are read and not yet used).
- The game begins on 1 May 793 (`ChartBuilder`: `startDay = 121`). The chronicle fires Lindisfarne on 8 June and the
  word of it travels by the world's ships as any news does. The debt is still due 365 days on: the money has not
  been tuned to Michaelmas.
- A past's thing for the chest is shown in the Yourself and Chest windows; it is not an item of the chest. A past's
  gift flag (`me_sea_born`, `me_knows_tolls`, `me_church_kin`) is set; nothing reads it yet.

### 9.2 The stats in the cards

- Conditions: `me.tongue`, `me.name`, `me.nerve`, `me.lore`, `me.troth` (numbers, 1 to 5), `me.people`, `me.faith`,
  `me.past` (text). `"check": "me.lore >= 3"`, `"check": "me.people = frisian"`. A check that fails says "Your Lore
  must be 3 or more." or "Only if your people is frisian."
- A chance: `{"chance": 0.55, "by": "me.tongue", "success": [...], "failure": [...]}`. The odds are as before:
  the chance plus 0.05 for each pip over 3, less for each under, shown as a word.
- A tag: a label that begins `[Tongue] ` (any words in square brackets) shows the tag before the words and the odds
  word after them. `ShownOption.tag`, `ShownOption.Tagged`.
- A check by one of the five that is won raises the stat (`EncounterDirector.StatUsed`, `Self.Used`): a third of a
  pip against one who resisted (a written chance of 0.55 or less), a tenth otherwise, divided by the pips it already
  has. When a pip fills the outcome says "Your Tongue has grown: 2 of 5." Troth is not raised this way.
- `tongue.<id>` now reads how well the self follows a tongue (0 to 1, with what follows through its kin), and the
  verb `tongue <id> = f` teaches it. Before, both read and did nothing. The ids are those of `tongues_793.json`;
  `norse`, `english`, `frisian`, `frankish`, `saxon` are taken as the `old_` ones.
- No verb changes a stat. The data's `"stats": {"name": 1}` beside an effects list (self_793, hours_793) is not read.
- Ruling 18 of 7 October (events move the stats: marks with a size, a cause and an end, capped at 2;
  `Design/fixes/stats_events.md`) came while this was being built and is NOT in. What is here is its base: a stat is
  one number, raised by use. `Self.Stat(id)` is the one place a stat is read (the cards, the odds, the panels), so the
  marks and their cap go there; `Self.StatRaw` is the base with the part of a pip gathered.

### 9.3 The panels

`self` Yourself, `crew` Crew, `people` People here, `bonds` Bonds, `faith` Faith and days, `field` The field, `sky` Wind
and tide, `ship` Ship, `house` House, `chest` Chest (the class is `ChestFramePanel`), `tongues` Tongues: one file each
in `Frame/Panels`. The default workspaces are now Sea (voyage, hold), Port (trade, port, people), Self (self, bonds,
faith, tongues, as tabs), House (house, tasks, word, chronicle, as tabs); Event is in no workspace by default.

| Reads | File |
|---|---|
| `places/*.json`: sites and people with their years, the era's line (the rest of family (a) is not read yet) | `Code/Game/Content/Places/PlaceBook.cs` |
| `story/calendar_793.json`: the days ahead for a faith | `Code/Game/Content/Year/FaithCalendar.cs` |
| A talk with a person of a place, as a floating window | `Code/Game/UI/Frame/PersonTalk.cs` |

What each says is not there yet, in its own words on screen: the crew are not people with names; nothing enforces a
feast or a fast and Troth does not move by them; how the people of the towns feel is not kept (only that you have
spoken, `Self.MarkMet`); no tide is reckoned on the chart; the ship has no wear, fittings or yard; the house has no
plots, stores or heir; the chest is read on [I] as before.

### 9.4 The tongues and the turning line

| What | File |
|---|---|
| `story/tongues_793.json`: tongues and kin, words, real lines in pieces; how a line turns (`TurnPlan`); who says what (`TalkTongues`) | `Code/Game/Content/Tongues/TongueBook.cs` |
| Who is given which word and line (a file of the builder's own: only ids of the tongues file) | `Game/Data/story/talk_tongues_793.json` |
| The line on screen (`TurningLine`), a line with words set apart (`SprinkledText`), what a window carries (`FloatTurn`) | `Code/Game/UI/Frame/FrameTongues.cs` |

- To have a window show a line that turns: `spec.turns.Add(FloatTurn.Of(TongueBook.Current.Line(id, year)))`.
  `Line` gives nothing for an id the file lacks, a line it marks `check` (quoted from memory), or one not yet spoken
  that year (`not_before`).
- A card's line may name one: `{"who": "host", "key": "...", "turns": "on_hav_003"}`. The real line comes up over the
  card's own lines and turns.
- How it turns follows `Self.TongueLevel(tongue)`: 0.9 and up, every piece, the whole in about a second; 0.2 to 0.9,
  slower and the longest pieces stay; under 0.2 nothing turns, and the small print gives the sense. A piece seen ten
  times (`Self.StaysAfter`) is English from the start; each showing counts once.
- A word in a line is set apart, with its meaning on hover, when the window names it (`spec.marks`) or when it is a
  word of the speaker's tongue (`spec.tongue`) with a letter English does not have. A word spelled as an English one
  (Old English `wine`, a friend) is never picked out of running text.
- Nobody aboard interprets yet: the design's "it turns in his words, coloured by what he wants you to hear" waits for
  a crew of people.
