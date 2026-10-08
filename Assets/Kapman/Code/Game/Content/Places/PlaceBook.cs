// 7 Oct 2026: THE PLACES, as far as the frame needs them now (Design/wiring_plan.md, family (a)). Reads
// Game/Data/places/*.json through ContentFiles: each port's sites and people, with the years they are there, and the
// era's line for a year. The rest of the family in the plan (stories, works, ships offered, unlocks, sayings, history)
// is not read here yet: whoever builds the town adds it to this file in the shapes the plan gives.
using System;
using System.Collections.Generic;

namespace Kapman.Game
{
    /// <summary>A site of a port (the dam heads, the toll post, the church): what it is, what is seen there, the years
    /// it stands, and the "do" lines as written ("act:load", "person:dorestad.tollman", "market").</summary>
    public sealed class SiteDef
    {
        public string id = "", port = "", kind = "", name = "", see = "";
        public int from, to;
        public string[] doRaw = Array.Empty<string>();

        public bool Open(int year) => ContentJson.InYears(from, to, year);

        /// <summary>The ids of the people its "person:" lines name.</summary>
        public IEnumerable<string> People
        {
            get
            {
                foreach (string d in doRaw)
                    if (d != null && d.StartsWith("person:", StringComparison.Ordinal))
                        yield return d.Substring(7).Trim();
            }
        }
    }

    /// <summary>A person of a port: who they are, the years they are there, what is seen of them, and their lines by
    /// key ("greet" one line, "rumour" several).</summary>
    public sealed class PersonDef
    {
        public string id = "", port = "", name = "", role = "", people = "", faith = "", trait = "", looks = "", voice = "";
        public bool historical;
        public int born, died, from, to;
        /// <summary>The id of the story's cast this person is ("cast.thiadbod"), from the tie that says "the same
        /// person"; "" for most. Their portrait was issued under that id.</summary>
        public string same = "";
        public readonly Dictionary<string, string[]> lines = new Dictionary<string, string[]>(StringComparer.Ordinal);
        /// <summary>The line keys in the file's order.</summary>
        public readonly List<string> keys = new List<string>();

        /// <summary>There that year: within their years at the port, and not yet dead.</summary>
        public bool Here(int year) => ContentJson.InYears(from, to, year) && (died == 0 || died >= year);

        /// <summary>A line by its key ("" if they have none); `pick` goes round a key with several lines.</summary>
        public string Line(string key, int pick = 0)
        {
            if (key == null || !lines.TryGetValue(key, out var l) || l.Length == 0)
                return "";
            return l[((pick % l.Length) + l.Length) % l.Length];
        }

        public bool Has(string key) => key != null && lines.TryGetValue(key, out var l) && l.Length > 0;

        /// <summary>The role as it is said: "farm_wife" is "farm wife". A "skipper" of the files is said "shipmaster":
        /// the period's own word (Design/content/lore/period_words.md: not skipper, not captain).</summary>
        public string Role => (role ?? "").Replace('_', ' ').Replace("skipper", "shipmaster");
    }

    /// <summary>What a places file says of one port: a summary, its sites, its people, and a line for each era.</summary>
    public sealed class PlaceDef
    {
        public string port = "", file = "", summary = "";
        public readonly List<SiteDef> sites = new List<SiteDef>();
        public readonly List<PersonDef> people = new List<PersonDef>();
        public readonly List<(int from, int to, string text)> eras = new List<(int, int, string)>();
    }

    /// <summary>
    /// THE PLACES' BOOK: every port of every file under places/ (a file holds one port, or several under "ports"). A
    /// file that does not parse is skipped with ContentFiles' one warning and the others are read.
    /// </summary>
    public sealed class PlaceBook
    {
        public const string Folder = "places";

        readonly List<PlaceDef> places = new List<PlaceDef>();
        readonly Dictionary<string, PlaceDef> byPort = new Dictionary<string, PlaceDef>(StringComparer.Ordinal);
        readonly Dictionary<string, PersonDef> persons = new Dictionary<string, PersonDef>(StringComparer.Ordinal);
        readonly Dictionary<string, SiteDef> siteById = new Dictionary<string, SiteDef>(StringComparer.Ordinal);

        static readonly ContentCache<PlaceBook> cache = new ContentCache<PlaceBook>("the places", Load);
        public static PlaceBook Current => cache.Value;

        public IReadOnlyList<PlaceDef> Places => places;
        public PlaceDef Place(string port) => port != null && byPort.TryGetValue(port, out var p) ? p : null;
        public PersonDef Person(string id) => id != null && persons.TryGetValue(id, out var p) ? p : null;

        /// <summary>The offices of which a port has one holder at a time: a card that wants "the toll man here" means
        /// the port's own. (A priest, a skipper, a merchant are many: a card's is its own.)</summary>
        public static readonly HashSet<string> Offices = new HashSet<string>(StringComparer.Ordinal)
        {
            "tollman", "moneyer", "count", "bishop", "abbot", "abbess", "reeve", "harbour_master", "king",
        };

        /// <summary>The port's own holder of an office in a year (the first of the file who is there then), or null.</summary>
        public PersonDef Holder(string port, string role, int year)
        {
            if (string.IsNullOrEmpty(port) || role == null || !Offices.Contains(role))
                return null;
            foreach (var p in People(port, year))
                if (p.role == role && p.name.Length > 0)
                    return p;
            return null;
        }
        public SiteDef Site(string id) => id != null && siteById.TryGetValue(id, out var s) ? s : null;

        /// <summary>The sites of a port that stand that year, in the file's order.</summary>
        public List<SiteDef> Sites(string port, int year)
        {
            var list = new List<SiteDef>();
            var place = Place(port);
            if (place != null)
                foreach (var s in place.sites)
                    if (s.Open(year))
                        list.Add(s);
            return list;
        }

        /// <summary>The people of a port who are there that year, in the file's order.</summary>
        public List<PersonDef> People(string port, int year)
        {
            var list = new List<PersonDef>();
            var place = Place(port);
            if (place != null)
                foreach (var p in place.people)
                    if (p.Here(year))
                        list.Add(p);
            return list;
        }

        /// <summary>The site a person is found at that year: the first standing site whose "person:" lines name them
        /// (null: about the town).</summary>
        public SiteDef SiteOf(PersonDef person, int year)
        {
            var place = person != null ? Place(person.port) : null;
            if (place == null)
                return null;
            foreach (var s in place.sites)
            {
                if (!s.Open(year))
                    continue;
                foreach (string id in s.People)
                    if (id == person.id)
                        return s;
            }
            return null;
        }

        /// <summary>The era's line for a port that year ("" for none).</summary>
        public string Era(string port, int year)
        {
            var place = Place(port);
            if (place != null)
                foreach (var (from, to, text) in place.eras)
                    if (ContentJson.InYears(from, to, year))
                        return text;
            return "";
        }

        static PlaceBook Load()
        {
            var book = new PlaceBook();
            foreach (var kv in ContentFiles.ParseAll(Folder))
            {
                var root = kv.Value;
                if (root == null)
                    continue;
                bool many = false;
                foreach (var port in ContentJson.Rows(root, "ports"))
                {
                    many = true;
                    book.Add(port, kv.Key);
                }
                if (!many && ContentJson.Has(root, "port"))
                    book.Add(root, kv.Key);                               // (a file that is one port itself)
            }
            return book;
        }

        void Add(Dictionary<string, object> x, string file)
        {
            string port = ContentJson.Text(x, "port");
            if (port.Length == 0 || byPort.ContainsKey(port))
                return;
            var place = new PlaceDef { port = port, file = file, summary = ContentJson.Text(x, "summary") };
            foreach (var s in ContentJson.Rows(x, "sites"))
            {
                var site = new SiteDef
                {
                    id = ContentJson.Text(s, "id"), port = port, kind = ContentJson.Text(s, "kind"), name = ContentJson.Text(s, "name"),
                    see = ContentJson.Text(s, "see"), from = ContentJson.Int(s, "from"), to = ContentJson.Int(s, "to"),
                    doRaw = ContentJson.Strings(s, "do"),
                };
                if (site.id.Length == 0)
                    continue;
                place.sites.Add(site);
                if (!siteById.ContainsKey(site.id))
                    siteById[site.id] = site;
            }
            foreach (var p in ContentJson.Rows(x, "people"))
            {
                var person = new PersonDef
                {
                    id = ContentJson.Text(p, "id"), port = port, name = ContentJson.Text(p, "name"), role = ContentJson.Text(p, "role"),
                    people = ContentJson.Text(p, "people"), faith = ContentJson.Text(p, "faith"), trait = ContentJson.Text(p, "trait"),
                    looks = ContentJson.Text(p, "looks"), voice = ContentJson.Text(p, "voice"), historical = Json.Bool(p, "historical"),
                    born = ContentJson.Int(p, "born"), died = ContentJson.Int(p, "died"), from = ContentJson.Int(p, "from"), to = ContentJson.Int(p, "to"),
                };
                if (person.id.Length == 0)
                    continue;
                foreach (var tie in ContentJson.Rows(p, "ties"))
                {
                    string to = ContentJson.Text(tie, "to"), why = ContentJson.Text(tie, "note");
                    if (person.same.Length == 0 && to.StartsWith("cast.", StringComparison.Ordinal) && why.StartsWith("the same", StringComparison.OrdinalIgnoreCase))
                        person.same = to;
                }
                foreach (var kv in ContentJson.Pairs(p, "lines"))
                {
                    string[] said = ContentJson.Strings(p["lines"], kv.Key);
                    if (said.Length == 0)
                        continue;
                    person.lines[kv.Key] = said;
                    person.keys.Add(kv.Key);
                }
                place.people.Add(person);
                if (!persons.ContainsKey(person.id))
                    persons[person.id] = person;
            }
            foreach (var e in ContentJson.Rows(x, "eras"))
                place.eras.Add((ContentJson.Int(e, "from"), ContentJson.Int(e, "to"), ContentJson.Text(e, "text")));
            places.Add(place);
            byPort[port] = place;
        }
    }
}
