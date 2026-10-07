using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>An option of a port's menu, in the words of its people: what the button says, and what choosing it means.</summary>
    public sealed class PortOption
    {
        public string key, label, intention;
    }

    /// <summary>
    /// THE WORDS OF 793 - how each people names what you do in port and in the market (Data/wording_793.json): the
    /// port's menu, the market's labels and the outcomes' wording, by culture. A port's culture comes from its trade
    /// region (a few ports differ: Noviomagus is the king's palace, Grobina a Northmen's settlement among the Curonians).
    /// Its kind of place adds errands (the procurator, the moneyer, the guest-house...), and single ports can change any
    /// word (the king's tenth is taken only at Dorestad and Quentovic). Labels and intentions only: no story.
    /// </summary>
    public static class Wording
    {
        static Dictionary<string, object> root;
        static readonly Dictionary<string, string> regionCulture = new Dictionary<string, string>();
        static readonly Dictionary<string, string> portCulture = new Dictionary<string, string>();
        static readonly Dictionary<string, string> zoneCulture = new Dictionary<string, string>();

        public static bool Loaded => root != null;

        public static void Load(TextAsset json)
        {
            root = null;
            regionCulture.Clear();
            portCulture.Clear();
            zoneCulture.Clear();
            if (json == null)
                return;
            root = Json.Parse(json.text) as Dictionary<string, object>;
            Copy(Json.Obj(root, "culture_of_region"), regionCulture);
            Copy(Json.Obj(root, "culture_of_port"), portCulture);
            Copy(Json.Obj(root, "zone_culture"), zoneCulture);
        }

        static void Copy(Dictionary<string, object> from, Dictionary<string, string> to)
        {
            if (from == null)
                return;
            foreach (var kv in from)
                to[kv.Key] = kv.Value as string ?? "";
        }

        /// <summary>The culture whose words a port uses (frankish, frisian, english, norse, slavic, baltic, finnic_rus, gaelic).</summary>
        public static string CultureOf(PortDef port)
        {
            if (port == null)
                return "frankish";
            if (portCulture.TryGetValue(port.id ?? "", out var c) || regionCulture.TryGetValue(port.region ?? "", out c))
                return c;
            var zone = TradeRules.Current?.ZoneOf(port.region);
            return zone != null && zoneCulture.TryGetValue(zone.id, out c) ? c : "frankish";
        }

        static Dictionary<string, object> Culture(PortDef port) => Json.Obj(Json.Obj(root, "cultures"), CultureOf(port));
        static Dictionary<string, object> PortWords(PortDef port) => port != null ? Json.Obj(Json.Obj(root, "ports"), port.id) : null;
        static Dictionary<string, object> Kind(PortDef port) => port != null ? Json.Obj(Json.Obj(root, "kinds"), port.kind) : null;

        /// <summary>A port's menu option in its people's words (null if the table hasn't it).</summary>
        public static PortOption Option(PortDef port, string key)
        {
            foreach (var o in Json.Arr(Culture(port), "menu") ?? new List<object>())
                if (Json.Str(o, "key") == key)
                    return new PortOption { key = key, label = Json.Str(o, "label"), intention = Json.Str(o, "intention") };
            return null;
        }

        /// <summary>What lying in port here means (the port menu's intention).</summary>
        public static string Arrive(PortDef port) => Json.Str(Culture(port), "arrive_intention", "You lie to at the berth");

        /// <summary>The errands this kind of place adds (the procurator, the moneyer, the guest-house...), and the port's own.</summary>
        public static List<PortOption> Errands(PortDef port)
        {
            var list = new List<PortOption>();
            if (!Loaded || port == null)
                return list;
            string culture = CultureOf(port);
            foreach (var e in Json.Arr(Kind(port), "extra") ?? new List<object>())
            {
                if (Json.Arr(e, "hide_at") is List<object> hide && hide.Contains(port.id))
                    continue;
                string label = Json.Str(Json.Obj(e, "label_at"), port.id, null)
                               ?? Json.Str(Json.Obj(e, "label_by_culture"), culture, null) ?? Json.Str(e, "label");
                string intention = Json.Str(Json.Obj(e, "intention_by_culture"), culture, null) ?? Json.Str(e, "intention");
                list.Add(new PortOption { key = Json.Str(e, "key"), label = label, intention = intention });
            }
            foreach (var e in Json.Arr(PortWords(port), "extra") ?? new List<object>())
                if (!list.Exists(o => o.key == Json.Str(e, "key")))
                    list.Add(new PortOption { key = Json.Str(e, "key"), label = Json.Str(e, "label"), intention = Json.Str(e, "intention") });
            return list;
        }

        /// <summary>The market's name here ("The emporium at Dorestat").</summary>
        public static string MarketTitle(PortDef port)
        {
            string t = Json.Str(PortWords(port), "market_title", null)
                       ?? Json.Str(Json.Obj(Kind(port), "market_title"), CultureOf(port), null)
                       ?? Json.Str(Culture(port), "market_title", "The market at {port}");
            return t.Replace("{port}", port != null ? port.name : "");
        }

        /// <summary>One of the market's labels here (leave, asks, pays, here, aboard, buy, sell, sell_all, purse, hold,
        /// worth_here, owed, toll_line, seen_elsewhere, market_intention), or the fallback.</summary>
        public static string Market(PortDef port, string name, string fallback) =>
            Json.Str(Json.Obj(PortWords(port), "market"), name, null) ?? Json.Str(Json.Obj(Culture(port), "market"), name, fallback);

        /// <summary>An outcome's words here, its {placeholders} filled ({goods}, {good}, {price}, {coins}, {back}, {toll_paid}).</summary>
        public static string Outcome(PortDef port, string key, string fallback, params (string name, string value)[] fill)
        {
            string t = Json.Str(Json.Obj(PortWords(port), "outcomes"), key, null)
                       ?? Json.Str(Json.Obj(Culture(port), "outcomes"), key, fallback);
            foreach (var (name, value) in fill)
                t = t.Replace("{" + name + "}", value ?? "");
            return t;
        }

        // ------------------------------------------------------------------------------------------ the word on the item

        // The words the game falls back on when wording_793.json has no "word" entry for a key (the table has them all:
        // the user rewrites them there). A key with several words is one of them, the same one each time for the same thing.
        static readonly Dictionary<string, string[]> wordDefaults = new Dictionary<string, string[]>
        {
            // rule 1: hard news
            ["news"] = new[] { "{news}" },
            ["news.shut"] = new[] { "{news} Nobody there is buying." },
            ["news.dear"] = new[] { "{news} {Good} is dear at {port} now." },
            ["news.cheap"] = new[] { "{news} {Good} goes cheap at {port} now." },
            // rule 2: a price seen (the only line with a number)
            ["seen"] = new[] { "You saw {good} pay {price} at {port}, {age}." },
            // rule 3: a price told
            ["told"] = new[] { "{Person} said {port} was paying {pays} for {good}. {Age}." },
            // rule 4: talk and rumour
            ["talk"] = new[] { "They say {port} pays {pays} for {good}. {Age}." },
            ["talk.at"] = new[] { "They say at {place} that {port} pays {pays} for {good}. {Age}." },
            ["talk.fair"] = new[] { "They say there is a fair on at {port}: plenty of goods, and many buyers." },
            ["rumour"] = new[] { "There is a rumour that {port} pays {pays} for {good}." },
            ["saw"] = new[] { "You saw {port} paying {pays} for {good}, {age}. It may have changed." },
            // rule 5: the season
            ["season.wanted"] = new[] { "{port} wants {good} in {month}." },
            ["season.comes"] = new[] { "{Good} comes to market at {port} in {month}: plenty, and cheap." },
            ["season.dear"] = new[] { "{Good} is dear in {month}." },
            ["season.cheap"] = new[] { "{Good} is cheap in {month}." },
            // rule 6: who sailed ahead
            ["ahead"] = new[] { "{Ships} left for {port} before you with {good} aboard." },
            ["ships.1"] = new[] { "a ship" },
            ["ships.2"] = new[] { "two ships" },
            ["ships.3"] = new[] { "three ships" },
            ["ships.4"] = new[] { "four ships" },
            ["ships.many"] = new[] { "several ships" },
            // rule 7: the law
            ["law"] = new[] { "The law is on {good} at {port}." },
            ["law.no_export"] = new[] { "{Good} may not be carried out of {realm}. They search ships at {port}." },
            ["law.death"] = new[] { "Selling {good} to the Northmen costs a man his life in {realm}." },
            ["law.no_sale"] = new[] { "{Good} may not be bought or sold in {realm}: the Church forbids it." },
            // rule 8: nothing known
            ["none"] = new[] { "No word on {good} at {port}." },
            ["none.since"] = new[] { "No word on {good} at {port} {age}." },
            ["none.old"] = new[] { "Only old talk of {good} at {port}." },
            ["none.anywhere"] = new[] { "No word on {good} from anywhere." },
            // the age, in words
            ["age.this_morning"] = new[] { "this morning" },
            ["age.yesterday"] = new[] { "yesterday" },
            ["age.two_days"] = new[] { "two days back" },
            ["age.few_days"] = new[] { "a few days back" },
            ["age.last_week"] = new[] { "last week" },
            ["age.fortnight"] = new[] { "a fortnight ago" },
            ["age.last_month"] = new[] { "last month" },
            ["age.since_spring"] = new[] { "since spring" },
            ["age.since_summer"] = new[] { "since summer" },
            ["age.since_autumn"] = new[] { "since autumn" },
            ["age.since_winter"] = new[] { "since winter" },
            ["age.old_talk"] = new[] { "old talk" },
            // a told price, as a word
            ["pays.well"] = new[] { "well" },
            ["pays.fair"] = new[] { "the usual" },
            ["pays.poorly"] = new[] { "poorly" },
            // who told it
            ["person.skipper"] = new[] { "a {people} skipper" },
            ["person.skipper.plain"] = new[] { "a skipper" },
            ["person.quay"] = new[] { "the toll man at {place}", "a boatman at {place}", "a porter at {place}", "an ale-wife at {place}" },
            ["person.witness"] = new[] { "a man who saw it" },
            ["person.letter"] = new[] { "a letter" },
            ["person.someone"] = new[] { "a man on the quay" },
            ["people.frankish"] = new[] { "Frankish" },
            ["people.frisian"] = new[] { "Frisian" },
            ["people.english"] = new[] { "English" },
            ["people.norse"] = new[] { "Norse" },
            ["people.slavic"] = new[] { "Wendish" },
            ["people.baltic"] = new[] { "Baltic" },
            ["people.finnic_rus"] = new[] { "Rus" },
            ["people.gaelic"] = new[] { "Irish" },
            // the small mark beside the line, said
            ["mark.seen"] = new[] { "seen" },
            ["mark.heard"] = new[] { "heard" },
            ["mark.season"] = new[] { "the season" },
            ["mark.law"] = new[] { "the law" },
            ["mark.none"] = new[] { "nothing known" },
        };

        /// <summary>Every key of the word on the item that the game can ask for (wording_793.json's "word" has them all).</summary>
        public static IEnumerable<string> WordKeys => wordDefaults.Keys;

        /// <summary>Does the table itself (not the built-in words) have this key of the word on the item?</summary>
        public static bool HasWord(string key) => Json.Obj(root, "word") is Dictionary<string, object> d && key != null && d.ContainsKey(key);

        static string Chosen(Dictionary<string, object> table, string key, float pick)
        {
            if (table == null || !table.TryGetValue(key, out var v))
                return null;
            if (v is string s)
                return s;
            if (v is List<object> list && list.Count > 0)
                return list[Mathf.Clamp((int)(pick * list.Count), 0, list.Count - 1)] as string;
            return null;
        }

        /// <summary>
        /// The words for a key of the word on the item ("told", "age.last_week", "person.quay"): the port's own
        /// (ports.&lt;id&gt;.word), its people's (cultures.&lt;culture&gt;.word), the table's ("word"), then the built-in ones.
        /// A key nothing has falls back to its parent ("law.no_export" to "law"); with nothing at all, "". Where a key
        /// has several words, `pick` (0 to 1) chooses one.
        /// </summary>
        public static string WordText(PortDef port, string key, float pick = 0f)
        {
            for (string k = key; !string.IsNullOrEmpty(k);)
            {
                string t = Chosen(Json.Obj(PortWords(port), "word"), k, pick)
                           ?? (port != null ? Chosen(Json.Obj(Culture(port), "word"), k, pick) : null)
                           ?? Chosen(Json.Obj(root, "word"), k, pick);
                if (t == null && wordDefaults.TryGetValue(k, out var d))
                    t = d[Mathf.Clamp((int)(pick * d.Length), 0, d.Length - 1)];
                if (t != null)
                    return t;
                int dot = k.LastIndexOf('.');
                k = dot > 0 ? k.Substring(0, dot) : null;
            }
            return "";
        }

        /// <summary>Fills a line's {slots}: "{name}" with the value as given, "{Name}" with its first letter made large.</summary>
        public static string Fill(string template, params (string name, string value)[] slots)
        {
            string t = template ?? "";
            if (slots == null || t.IndexOf('{') < 0)
                return t;
            foreach (var (name, value) in slots)
            {
                if (string.IsNullOrEmpty(name))
                    continue;
                string v = value ?? "";
                t = t.Replace("{" + name + "}", v).Replace("{" + Capital(name) + "}", Capital(v));
            }
            return t;
        }

        /// <summary>The word on an item as a line: its key's words (of the port it is about), the slots filled, a large
        /// first letter.</summary>
        public static string Word(GoodWord w)
        {
            if (string.IsNullOrEmpty(w.key))
                return "";
            string t = Fill(WordText(GameData.Port(w.port), w.key), w.slots).Trim();
            while (t.Contains("  "))
                t = t.Replace("  ", " ");
            return Capital(t);
        }

        static string Capital(string s) => string.IsNullOrEmpty(s) ? s ?? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
