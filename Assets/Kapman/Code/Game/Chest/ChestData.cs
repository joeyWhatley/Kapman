using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    // ported 4 Oct: unchanged from the first build. The tables are read from TextAssets (texts_793, riddles_793,
    // runes_793, voyages_793 in Assets/Kapman/Game/Data), handed in by KapmanGame; Json is the Core's (Kapman.Game.Json).

    /// <summary>A real text of the time (Data/texts_793.json): in its own tongue, with a free translation where one
    /// passes the rule (the translator dead 70 years and more, published before 1931), and where it comes from.</summary>
    public sealed class TextDef
    {
        public string id, title, people, form, language, script, original, translation, translator, credit, where, writtenDown;
        /// <summary>Runes: the words as spoken (normalized Old Norse), for reading them yourself (RuneReading).</summary>
        public string normalized;
        /// <summary>A text of sailing directions: its legs are in <see cref="ChestData.Legs"/> (laid out on the map).</summary>
        public bool voyage;
        /// <summary>Where and how the text itself is had (bought, given), if it's had on its own.</summary>
        public Acquire acquire;
        /// <summary>The things (lore names) it teaches once read.</summary>
        public string[] teaches = new string[0];
        public int translated, translatorDied;
        /// <summary>Written down after 1066 (Snorri, the sagas): told later.</summary>
        public bool toldLater;

        /// <summary>The merchant must learn this tongue (or have it read) before the switch turns it into English.</summary>
        public bool NeedsLearning => Chest.NeedsLearning(language);

        /// <summary>"rune stick", "wax tablet", "parchment leaf", "book".</summary>
        public string FormName => form == "rune_stick" ? "rune stick" : form == "wax_tablet" ? "wax tablet" : form == "parchment" ? "parchment leaf"
                                : form == "stone" ? "copy of a rune stone" : form ?? "";
    }

    /// <summary>A thing the merchant can know and drag onto a riddle as its answer: a good, a place, an animal, a god...
    /// (Data/riddles_793.json "palette"). Wrong answers are things too.</summary>
    public sealed class ThingDef
    {
        public string id, kind, name, gameId;
        /// <summary>How it comes to be known: start, seen_at_sea, seen_in_port, market, place, text, talk.</summary>
        public string known;
        /// <summary>Where it's come to know: port ids (seen in port, talk), or the text ids that teach it (text).</summary>
        public string[] at = new string[0];
        /// <summary>A wrong answer (it answers nothing), there so the things don't give the answers away.</summary>
        public bool decoy;
    }

    /// <summary>Where and how a riddle is to be had (bought, a reward, given, found), by whom, in which years.</summary>
    public sealed class Acquire
    {
        public string how, who, why, needs;
        public string[] where = new string[0];
        public int from, to;
        public double price;
        /// <summary>Each place it's had, with its own years (0: no limit).</summary>
        public readonly List<(string port, int from, int to)> places = new List<(string, int, int)>();

        public bool OpenIn(int year) => (from == 0 || year >= from) && (to == 0 || year <= to);

        /// <summary>Is it had at this port (or "sea") in this year?</summary>
        public bool OpenAt(string port, int year)
        {
            if (!OpenIn(year))
                return false;
            foreach (var p in places)
                if (p.port == port && (p.from == 0 || year >= p.from) && (p.to == 0 || year <= p.to))
                    return true;
            return false;
        }
    }

    /// <summary>A riddle (Data/riddles_793.json): its text, its answers (things), how hard, how many tries, what wakes it
    /// when it rests, where it's got and what it wins.</summary>
    public sealed class RiddleDef
    {
        public string id, textId, answer, answerSource, answerWords, answerKind, reward, notes;
        /// <summary>The things that answer it (any one).</summary>
        public string[] answers = new string[0];
        /// <summary>A riddle with two or three answers: one list per answer (any thing in a list fills it); all must be
        /// given at once.</summary>
        public readonly List<string[]> slots = new List<string[]>();
        public int SlotCount => Math.Max(1, slots.Count);
        public string[] clues = new string[0];
        public int difficulty = 3, tries = 3;
        /// <summary>What, newly learned, wakes it when it rests: (kind, id) - market (a good seen), port (visited), text
        /// (read), thing (come to know), news (heard).</summary>
        public readonly List<(string kind, string id)> wakeOn = new List<(string, string)>();
        public Acquire acquire;
        /// <summary>The set it belongs to (parts or a chain), if any.</summary>
        public string set;
    }

    /// <summary>A reward: silver, standing, toll freedom at a port, a price tip, a text, lodging, or the big ones (a hoard, a
    /// ship, a place: their own steps).</summary>
    public sealed class RewardDef
    {
        public string id, kind, what, where, size, text, gameId;
        public double silver, standing;
        public int years;
        /// <summary>The years it can be had, and the port that gives it once its own has closed.</summary>
        public int from, to, laterFrom;
        public string laterWhere;
        public double lat = double.NaN, lon = double.NaN;
    }

    /// <summary>Riddles that go together: parts of one (all needed) or a chain (each answer leads to the next).</summary>
    public sealed class RiddleSet
    {
        public string id, kind, reward;
        public string[] riddles = new string[0];
    }

    /// <summary>One leg of an old text's sailing directions: from where, the words, the bearing and days as the text has
    /// them, the landmarks, and where scholars place its end (lat/lon; a port where there is one).</summary>
    public sealed class LegDef
    {
        public string text, voyage, original, translation, bearing, days, landmarks, scholars;
        public int order;
        public string fromName, fromPort, toName, toPort;
        public double fromLat = double.NaN, fromLon = double.NaN, toLat = double.NaN, toLon = double.NaN;
        public string Key => text + ":" + order;
    }

    /// <summary>A kenning in a verse: its words, what it means, the thing it's answered by.</summary>
    public sealed class VerseSlot
    {
        public string original, means, literal, thing;
    }

    /// <summary>A real verse dense with kennings (Data/riddles_793.json "verses"): decoded by giving each kenning its plain
    /// meaning; the question it answers when decoded.</summary>
    public sealed class VerseDef
    {
        public string id, textId, hint, answer;
        public readonly List<VerseSlot> slots = new List<VerseSlot>();
        public Acquire acquire;
    }

    /// <summary>The Younger Futhark rune: its sign, name and the sounds it stood for.</summary>
    public sealed class RuneDef
    {
        public string rune, name;
        public string[] sounds = new string[0];
    }

    /// <summary>A real rune inscription (Data/runes_793.json): where, when, the runes, how it reads.</summary>
    public sealed class InscriptionDef
    {
        public string id, site, port, date, runes, transliteration, normalized, translation, gloss, credit;
        /// <summary>The first year it can have stood, from <see cref="date"/> (0: not known). It is not there to be read before.</summary>
        public int carved;

        /// <summary>"c. 940-958": 940; "brooch c. 700; runes added c. 900-950": 900; "10th century": 900; else 0.</summary>
        public static int CarvedFrom(string date)
        {
            string s = date ?? "";
            int added = s.IndexOf("added", System.StringComparison.OrdinalIgnoreCase);
            if (added >= 0)
                s = s.Substring(added);
            var year = System.Text.RegularExpressions.Regex.Match(s, @"(\d{3,4})");
            if (year.Success)
                return int.Parse(year.Groups[1].Value);
            var century = System.Text.RegularExpressions.Regex.Match(s, @"(\d{1,2})(st|nd|rd|th) century");
            return century.Success ? (int.Parse(century.Groups[1].Value) - 1) * 100 : 0;
        }

        /// <summary>The stone's name as the page says it: "Gorm's stone", without the scholars' "(Jelling 1)".</summary>
        public static string Plain(string site)
        {
            int cut = (site ?? "").IndexOf(" (", System.StringComparison.Ordinal);
            return cut > 0 ? site.Substring(0, cut) : site ?? "";
        }
    }

    /// <summary>
    /// THE CHEST'S TABLES - the real texts, the riddles and their answers, the runes (Data/texts_793.json,
    /// riddles_793.json, runes_793.json), loaded once. Every text is a real one of the time or told of it; the words round
    /// them (who gives a riddle, what's said) are yours to write.
    /// </summary>
    public static class ChestData
    {
        public static readonly Dictionary<string, TextDef> Texts = new Dictionary<string, TextDef>();
        public static readonly Dictionary<string, RiddleDef> Riddles = new Dictionary<string, RiddleDef>();
        public static readonly Dictionary<string, ThingDef> Things = new Dictionary<string, ThingDef>();
        public static readonly Dictionary<string, RewardDef> Rewards = new Dictionary<string, RewardDef>();
        public static readonly Dictionary<string, RiddleSet> Sets = new Dictionary<string, RiddleSet>();
        public static readonly List<RuneDef> Futhark = new List<RuneDef>();
        public static readonly List<InscriptionDef> Inscriptions = new List<InscriptionDef>();
        public static readonly List<LegDef> Legs = new List<LegDef>();
        public static readonly Dictionary<string, VerseDef> Verses = new Dictionary<string, VerseDef>();
        /// <summary>The riddle the merchant starts with (Ravn's), if the table names one.</summary>
        public static string StartRiddle;

        public static void Load(TextAsset texts, TextAsset riddles, TextAsset runes, TextAsset voyages = null)
        {
            Legs.Clear();
            Verses.Clear();
            Texts.Clear();
            Riddles.Clear();
            Things.Clear();
            Rewards.Clear();
            Sets.Clear();
            Futhark.Clear();
            Inscriptions.Clear();
            StartRiddle = null;
            if (texts != null)
                foreach (var o in Json.Arr(Json.Parse(texts.text), "texts") ?? new List<object>())
                {
                    var t = new TextDef
                    {
                        id = Json.Str(o, "id"), title = Json.Str(o, "title"), people = Json.Str(o, "people"), form = Json.Str(o, "form", "parchment"),
                        language = Json.Str(o, "language"), script = Json.Str(o, "script", "latin"), original = Json.Str(o, "original"),
                        translation = Json.Str(o, "translation", null), translator = Json.Str(o, "translator"), credit = Json.Str(o, "credit"),
                        where = Json.Str(o, "where"), writtenDown = Json.Str(o, "writtenDown"),
                        translated = (int)Json.Num(o, "translated"), translatorDied = (int)Json.Num(o, "translatorDied"), toldLater = Json.Bool(o, "toldLater"),
                        normalized = Json.Str(o, "normalized", null), voyage = Json.Bool(o, "voyage"), acquire = ReadAcquire(Json.Obj(o, "acquire")),
                        teaches = Strings(Json.Arr(o, "teaches")),
                    };
                    if (!string.IsNullOrEmpty(t.id))
                        Texts[t.id] = t;
                }
            if (riddles != null)
            {
                var root = Json.Parse(riddles.text);
                StartRiddle = Json.Str(root, "start", null);
                foreach (var o in Json.Arr(root, "palette") ?? new List<object>())
                {
                    var th = new ThingDef { id = Json.Str(o, "id"), kind = Json.Str(o, "kind"), name = Json.Str(o, "name"), gameId = Json.Str(o, "gameId", null), known = Json.Str(o, "known", "start"),
                                            decoy = Json.Bool(o, "decoy") };
                    var at = Json.Arr(o, "at");
                    if (at != null)
                        th.at = Strings(at);
                    else if (Json.Str(o, "at", null) is string one)
                        th.at = new[] { one };
                    if (!string.IsNullOrEmpty(th.id))
                        Things[th.id] = th;
                }
                foreach (var o in Json.Arr(root, "riddles") ?? new List<object>())
                {
                    var r = new RiddleDef
                    {
                        id = Json.Str(o, "id"), textId = Json.Str(o, "text"), answer = Json.Str(o, "answer"), answerSource = Json.Str(o, "answerSource"),
                        answerWords = Json.Str(o, "answerWords"), answerKind = Json.Str(o, "answerKind"), reward = Json.Str(o, "reward", null),
                        notes = Json.Str(o, "notes"), difficulty = (int)Json.Num(o, "difficulty", 3), tries = (int)Json.Num(o, "tries", 0),
                        answers = Strings(Json.Arr(o, "answers")), clues = Strings(Json.Arr(o, "clues")), set = Json.Str(o, "set", null),
                    };
                    if (r.tries <= 0)
                        r.tries = r.difficulty >= 4 ? 2 : 3;                    // (the hardest give fewer tries)
                    foreach (var w in Json.Arr(o, "wakeOn") ?? new List<object>())
                        r.wakeOn.Add((Json.Str(w, "kind"), Json.Str(w, "id")));
                    foreach (var s in Json.Arr(o, "slots") ?? new List<object>())
                        if (s is List<object> l)
                            r.slots.Add(Strings(l));
                    r.acquire = ReadAcquire(Json.Obj(o, "acquire"));
                    if (!string.IsNullOrEmpty(r.id))
                        Riddles[r.id] = r;
                }
                foreach (var o in Json.Arr(root, "rewards") ?? new List<object>())
                {
                    var w = new RewardDef
                    {
                        id = Json.Str(o, "id"), kind = Json.Str(o, "kind"), what = Json.Str(o, "what"), where = Json.Str(o, "where", null),
                        size = Json.Str(o, "size", "small"), text = Json.Str(o, "text", null), silver = Json.Num(o, "silver"),
                        standing = Json.Num(o, "standing"), years = (int)Json.Num(o, "years"), gameId = Json.Str(o, "gameId", null),
                        from = (int)Json.Num(Json.Obj(o, "window"), "from"), to = (int)Json.Num(Json.Obj(o, "window"), "to"),
                        laterFrom = (int)Json.Num(Json.Obj(o, "later"), "from"), laterWhere = Json.Str(Json.Obj(o, "later"), "where", null),
                    };
                    var place = Json.Obj(o, "place");
                    if (place != null)
                    {
                        w.lat = Json.Num(place, "lat", double.NaN);
                        w.lon = Json.Num(place, "lon", double.NaN);
                    }
                    if (!string.IsNullOrEmpty(w.id))
                        Rewards[w.id] = w;
                }
                foreach (var o in Json.Arr(root, "verses") ?? new List<object>())
                {
                    var v = new VerseDef { id = Json.Str(o, "id"), textId = Json.Str(o, "text"), hint = Json.Str(o, "hint"), answer = Json.Str(o, "answer"), acquire = ReadAcquire(Json.Obj(o, "acquire")) };
                    foreach (var s in Json.Arr(o, "slots") ?? new List<object>())
                        v.slots.Add(new VerseSlot { original = Json.Str(s, "original"), means = Json.Str(s, "means"), literal = Json.Str(s, "literal"), thing = Json.Str(s, "thing") });
                    if (!string.IsNullOrEmpty(v.id))
                        Verses[v.id] = v;
                }
                foreach (var o in Json.Arr(root, "sets") ?? new List<object>())
                {
                    var s = new RiddleSet { id = Json.Str(o, "id"), kind = Json.Str(o, "kind", "parts"), reward = Json.Str(o, "reward", null), riddles = Strings(Json.Arr(o, "riddles")) };
                    if (!string.IsNullOrEmpty(s.id))
                        Sets[s.id] = s;
                }
            }
            if (voyages != null)
                foreach (var o in Json.Arr(Json.Parse(voyages.text), "legs") ?? new List<object>())
                {
                    var l = new LegDef
                    {
                        text = Json.Str(o, "text"), voyage = Json.Str(o, "voyage"), order = (int)Json.Num(o, "order"), original = Json.Str(o, "original"),
                        translation = Json.Str(o, "translation", null), bearing = Json.Str(o, "bearing"), landmarks = Json.Str(o, "landmarks"),
                        scholars = Json.Str(o, "scholars"),
                    };
                    var days = o is Dictionary<string, object> od && od.TryGetValue("days", out var dv) ? dv : null;
                    l.days = days is double dd ? $"{dd:0.#} days" : days as string ?? "";
                    ReadPlace(Json.Obj(o, "from"), out l.fromName, out l.fromPort, out l.fromLat, out l.fromLon);
                    ReadPlace(Json.Obj(o, "to"), out l.toName, out l.toPort, out l.toLat, out l.toLon);
                    Legs.Add(l);
                }
            if (runes != null)
            {
                var root = Json.Parse(runes.text);
                foreach (var o in Json.Arr(root, "futhark") ?? new List<object>())
                    Futhark.Add(new RuneDef { rune = Json.Str(o, "rune"), name = Json.Str(o, "name"), sounds = Strings(Json.Arr(o, "sounds")) });
                foreach (var o in Json.Arr(root, "inscriptions") ?? new List<object>())
                {
                    var ins = new InscriptionDef
                    {
                        id = Json.Str(o, "id"), site = Json.Str(o, "site"), port = Json.Str(o, "port", null), date = Json.Str(o, "date"),
                        runes = Json.Str(o, "runes"), transliteration = Json.Str(o, "transliteration"), normalized = Json.Str(o, "normalized"),
                        translation = Json.Str(o, "translation", null), gloss = Json.Str(o, "gloss", null), credit = Json.Str(o, "credit"),
                    };
                    ins.carved = InscriptionDef.CarvedFrom(ins.date);
                    Inscriptions.Add(ins);
                    // each stone can be copied into the chest, to read yourself
                    Texts["stone:" + ins.id] = new TextDef
                    {
                        id = "stone:" + ins.id, title = InscriptionDef.Plain(ins.site), people = "norse", form = "stone", language = "Old Norse", script = "runic",
                        original = ins.runes, translation = ins.translation ?? ins.gloss, credit = ins.credit, where = ins.date, normalized = ins.normalized,
                        translator = ins.translation != null ? "" : null,
                    };
                }
            }
        }

        public static TextDef Text(string id) => id != null && Texts.TryGetValue(id, out var t) ? t : null;
        public static VerseDef Verse(string id) => id != null && Verses.TryGetValue(id, out var v) ? v : null;

        /// <summary>The legs of a voyage text, in order.</summary>
        public static List<LegDef> LegsOf(string text) => Legs.FindAll(l => l.text == text);

        static Acquire ReadAcquire(Dictionary<string, object> a)
        {
            if (a == null)
                return null;
            var q = new Acquire
            {
                how = Json.Str(a, "how"), who = Json.Str(a, "who"), why = Json.Str(a, "why"), needs = Json.Str(a, "needs", null),
                from = (int)Json.Num(a, "from"), to = (int)Json.Num(a, "to"), price = Json.Num(a, "price"),
            };
            // where: plain port ids, or {port, from, to} each with its own years
            var ports = new List<string>();
            foreach (var w in Json.Arr(a, "where") ?? new List<object>())
            {
                if (w is string s)
                {
                    ports.Add(s);
                    q.places.Add((s, 0, 0));
                }
                else if (Json.Str(w, "port", null) is string p)
                {
                    ports.Add(p);
                    q.places.Add((p, (int)Json.Num(w, "from"), (int)Json.Num(w, "to")));
                }
            }
            q.where = ports.ToArray();
            return q;
        }

        static void ReadPlace(object o, out string name, out string port, out double lat, out double lon)
        {
            name = Json.Str(o, "name");
            port = Json.Str(o, "port", null);
            lat = o is Dictionary<string, object> d && d.TryGetValue("lat", out var la) && la is double x ? x : double.NaN;
            lon = o is Dictionary<string, object> e && e.TryGetValue("lon", out var lo) && lo is double y ? y : double.NaN;
        }
        public static RiddleDef Riddle(string id) => id != null && Riddles.TryGetValue(id, out var r) ? r : null;
        public static ThingDef Thing(string id) => id != null && Things.TryGetValue(id, out var t) ? t : null;
        public static RewardDef Reward(string id) => id != null && Rewards.TryGetValue(id, out var r) ? r : null;

        static string[] Strings(List<object> list) => list == null ? new string[0] : list.ConvertAll(s => s as string ?? "").ToArray();
    }
}
