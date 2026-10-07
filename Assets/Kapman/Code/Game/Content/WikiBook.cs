using System;
using System.Collections.Generic;
using Kapman.Levels;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>One page of the book: what a thing is in the game, what the record says of it, and when it opens.</summary>
    public sealed class WikiPage
    {
        public string id = "", kind = "", title = "", hint = "", brief = "", game = "", known = "", source = "", opens = "always";
        public string[] links = Array.Empty<string>();
        /// <summary>A How to play page of a rule that is ruled but not in the game yet is held back.</summary>
        public bool built = true;
    }

    /// <summary>
    /// THE BOOK (Design/wiki.md; the user, 7 Oct 2026: "I think an in-game wiki is needed"). The pages are baked from the
    /// game's own data by Bake/content/make_wiki.py into Data/story/wiki_793.json (about 2,600 of them: the rules, the
    /// ports and their sites and people, the goods, ships, peoples, faiths, days, words, events and texts); this reads
    /// them once and answers for them: a page by its id, the pages of a kind, the names that link to pages, and whether
    /// a page is open to the player yet.
    ///
    /// WHAT IS OPEN (the user's ruling of 7 Oct: the rules from the first day, the world as you learn it): a page's own
    /// rule (always; a port heard of or seen; a good seen; a person met; a day that has passed), or the page has been
    /// come upon in play: its name stood on the screen as a link, or it was opened (<see cref="Meet"/>: kept with the
    /// saved game). A rule this cannot check yet (a word heard, a text held) is closed until the page is come upon.
    /// </summary>
    public sealed class WikiBook
    {
        public const string File = "story/wiki_793.json";
        /// <summary>Pages written by hand beside the baked ones (words the game itself uses that no data file holds): read
        /// after the baked book; a page of the same id takes the baked one's place, and its names are looked for first.</summary>
        public const string More = "story/wiki_more_793.json";
        public static readonly string[] Kinds = { "how_to_play", "port", "site", "person", "good", "ship", "people", "faith", "day", "word", "event", "text" };
        public static readonly string[] KindNames = { "How to play", "Ports", "Places in ports", "People", "Wares", "Ships", "Peoples", "Faiths", "Feasts and days", "Words", "What happened", "Texts" };

        readonly List<WikiPage> pages = new List<WikiPage>();
        readonly Dictionary<string, WikiPage> byId = new Dictionary<string, WikiPage>(StringComparer.Ordinal);
        readonly Dictionary<string, List<WikiPage>> byKind = new Dictionary<string, List<WikiPage>>(StringComparer.Ordinal);
        readonly List<(string name, string page)> names = new List<(string, string)>();
        string[] shields = Array.Empty<string>();
        /// <summary>Phrases that hold a name that is not that page's ("New York" is not York): never cut into.</summary>
        public string[] Shields => shields;

        static readonly ContentCache<WikiBook> cache = new ContentCache<WikiBook>("the book", Load);
        public static WikiBook Current => cache.Value;

        public IReadOnlyList<WikiPage> Pages => pages;
        /// <summary>Every name that links to a page, the longest first (as baked).</summary>
        public IReadOnlyList<(string name, string page)> Names => names;
        public WikiPage Page(string id) => id != null && byId.TryGetValue(id, out var p) ? p : null;
        public IReadOnlyList<WikiPage> OfKind(string kind) => kind != null && byKind.TryGetValue(kind, out var l) ? l : (IReadOnlyList<WikiPage>)Array.Empty<WikiPage>();
        public static string KindName(string kind)
        {
            int i = Array.IndexOf(Kinds, kind);
            return i >= 0 ? KindNames[i] : kind ?? "";
        }

        static WikiBook Load()
        {
            var book = new WikiBook();
            if (!ContentFiles.Exists(File))
                return book;
            var root = ContentFiles.ParseObject(File);
            if (root == null)
                return book;
            book.Read(root, false);
            book.shields = ContentJson.Strings(root, "shields");
            if (ContentFiles.Exists(More))
            {
                var more = ContentFiles.ParseObject(More);
                if (more != null)
                    book.Read(more, true);
            }
            foreach (var list in book.byKind.Values)
                list.Sort((x, y) => string.Compare(x.title, y.title, StringComparison.OrdinalIgnoreCase));
            return book;
        }

        void Read(Dictionary<string, object> root, bool byHand)
        {
            foreach (var row in ContentJson.Rows(root, "pages"))
            {
                var p = new WikiPage
                {
                    id = ContentJson.Text(row, "id"), kind = ContentJson.Text(row, "kind"), title = ContentJson.Text(row, "title"),
                    hint = ContentJson.Text(row, "hint"), brief = ContentJson.Text(row, "brief"), game = ContentJson.Text(row, "game"),
                    known = ContentJson.Text(row, "known"), source = ContentJson.Text(row, "source"), opens = ContentJson.Text(row, "opens", "always"),
                    links = ContentJson.Strings(row, "links"),
                    built = !(row.TryGetValue("built", out object built) && built is bool yes && !yes),
                };
                if (p.id.Length == 0)
                    continue;
                if (byId.TryGetValue(p.id, out var old))
                {
                    if (!byHand)
                        continue;
                    pages.Remove(old);
                    if (byKind.TryGetValue(old.kind, out var was))
                        was.Remove(old);
                }
                pages.Add(p);
                byId[p.id] = p;
                if (!byKind.TryGetValue(p.kind, out var list))
                    byKind[p.kind] = list = new List<WikiPage>();
                list.Add(p);
            }
            int front = 0;
            foreach (var row in ContentJson.Rows(root, "names"))
            {
                string name = ContentJson.Text(row, "name"), page = ContentJson.Text(row, "page");
                if (name.Length < 2 || !byId.ContainsKey(page))
                    continue;
                if (byHand)
                    names.Insert(front++, (name, page));
                else
                    names.Add((name, page));
            }
        }

        // ------------------------------------------------------------------ what the player has come upon

        static readonly HashSet<string> met = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>Counts up when a page is come upon for the first time.</summary>
        public static int Version { get; private set; }

        /// <summary>The player has come upon this page (its name stood on the screen, or it was opened).</summary>
        public static void Meet(string pageId)
        {
            if (!string.IsNullOrEmpty(pageId) && met.Add(pageId))
                Version++;
        }

        public static bool Met(string pageId) => pageId != null && met.Contains(pageId);

        /// <summary>Is the page open to the player now?</summary>
        public bool Open(WikiPage p)
        {
            if (p == null || !p.built)
                return false;
            if (met.Contains(p.id))
                return true;
            foreach (string rule in p.opens.Split('&'))
                if (!Holds(rule.Trim()))
                    return false;
            return true;
        }

        static bool Holds(string rule)
        {
            if (rule.Length == 0 || rule == "always")
                return true;
            int cut = rule.IndexOf(':');
            string what = cut > 0 ? rule.Substring(0, cut) : rule, arg = cut > 0 ? rule.Substring(cut + 1) : "";
            switch (what)
            {
                case "port_heard":
                case "port_seen":
                {
                    var port = GameData.Port(arg);
                    if (port == null)
                        return false;
                    if (Port.LyingId() == arg || (ChartWorld.Current != null && ChartWorld.Current.homePort == arg))
                        return true;
                    var note = Trade.Started ? Trade.Ledger.Latest(arg) : null;
                    if (what == "port_seen")
                        return note != null && note.how == How.Seen;
                    return note != null || Known.IsHeardOf(port);
                }
                case "good_seen":
                {
                    if (!Trade.Started)
                        return false;
                    foreach (var (good, _) in Trade.Hold.Contents)
                        if (good.id == arg)
                            return true;
                    foreach (var note in Trade.Ledger.Notes)
                        if (note.prices.ContainsKey(arg))
                            return true;
                    return false;
                }
                case "person_met":
                    return Self.Met(arg) != null;
                case "date":
                {
                    var clock = LevelClock.Current;
                    string[] ymd = arg.Split('-');
                    if (clock == null || ymd.Length != 3 || !int.TryParse(ymd[0], out int y) || !int.TryParse(ymd[1], out int m) || !int.TryParse(ymd[2], out int d))
                        return false;
                    var now = clock.Date;
                    return now.year > y || (now.year == y && (now.month > m || (now.month == m && now.day > d)));
                }
                default:
                    return false;                                         // (a word heard, a text held: open when come upon)
            }
        }

        static object Capture() => new Dictionary<string, object> { ["met"] = new List<object>(met) };

        static void Restore(object saved)
        {
            met.Clear();
            foreach (object o in Json.Arr(saved, "met") ?? new List<object>())
                if (o is string s && s.Length > 0)
                    met.Add(s);
            Version++;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            met.Clear();
            Version = 0;
            ContentSave.Register("book", Capture, Restore);
        }
    }
}
