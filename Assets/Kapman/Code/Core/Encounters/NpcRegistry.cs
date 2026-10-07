using System;
using System.Collections.Generic;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Sim;

namespace Kapman.Encounters
{
    /// <summary>A person the merchant can meet: forged from a role at a port, or a named person of the data.</summary>
    public sealed class Npc
    {
        public string id = "", name = "", people = "", faith = "", tongue = "", role = "", homePort = "", sex = "m";
        public int bornYear, diesYear;
        /// <summary>0 for the first holder of a role at a port, 1 for his heir, ...</summary>
        public int generation;
        public bool alive = true;
        public double lastSeenDay;
        public string lastSeenPort = "";

        public bool AliveIn(int year) => alive && year <= diesYear;
    }

    /// <summary>One thing a person remembers of the merchant: when, on which card, what he chose, how it moved them, a word
    /// (the effect's) and a note (the card's memory line for that choice).</summary>
    public sealed class MemoryEntry
    {
        public double day;
        public string card = "", option = "", word = "", note = "";
        public double delta;
        /// <summary>The merchant's generation when it happened (a son is told what his father did).</summary>
        public int generation;
    }

    /// <summary>What one person thinks of one merchant (keyed by both, so many merchants can share a world).</summary>
    public sealed class Relationship
    {
        public string npc = "", merchant = "";
        /// <summary>-100 .. 100 (shown as a word: WordTable.Feeling).</summary>
        public double feeling;
        /// <summary>Silver owed: + the merchant owes them, - they owe him.</summary>
        public double owed;
        public bool met;
        public readonly List<MemoryEntry> memory = new List<MemoryEntry>();
    }

    /// <summary>The people, faith and tongue of a port, as a forge reads them.</summary>
    public sealed class PortFacts
    {
        public string id = "", realm = "", faith = "";
        /// <summary>The peoples a person of this port is drawn from (the first the main one).</summary>
        public List<string> peoples = new List<string>();
        public bool hasMint;
        public string People => peoples.Count > 0 ? peoples[0] : "";
    }

    /// <summary>
    /// THE ROLES (Data/encounters/npc_roles_793.json): what kind of person fills a role (the toll man: a man of 30 to 60
    /// of the port's people and faith), the peoples' tongues and faiths, the realms' and the ports' peoples, and the
    /// feeling words. Read once; the registry and the vars read it.
    /// </summary>
    public sealed class RoleTable
    {
        public sealed class RoleDef
        {
            public string id = "", title = "", sex = "m";
            public int ageMin = 25, ageMax = 55;
            public bool fixedRole;
        }

        public readonly Dictionary<string, RoleDef> roles = new Dictionary<string, RoleDef>();
        readonly Dictionary<string, (string tongue, string faith)> peoples = new Dictionary<string, (string, string)>();
        readonly Dictionary<string, (List<string> peoples, string faith)> realms = new Dictionary<string, (List<string>, string)>();
        readonly Dictionary<string, (List<string> peoples, string faith, bool mint)> ports = new Dictionary<string, (List<string>, string, bool)>();

        public RoleDef Role(string id) => id != null && roles.TryGetValue(id, out var r) ? r : null;

        public string TongueOf(string people) => people != null && peoples.TryGetValue(people, out var p) ? p.tongue : "";

        public static RoleTable Load(string json, List<string> errors)
        {
            var t = new RoleTable();
            object o;
            try
            {
                o = Json.Parse(json ?? "");
            }
            catch (FormatException e)
            {
                errors?.Add("npc_roles_793.json: " + e.Message);
                return t;
            }
            if (o == null)
            {
                errors?.Add("npc_roles_793.json: missing.");
                return t;
            }
            foreach (var kv in Json.Obj(o, "roles") ?? new Dictionary<string, object>())
            {
                var age = Json.Arr(kv.Value, "age");
                var r = new RoleDef
                {
                    id = kv.Key,
                    title = Json.Str(kv.Value, "title", kv.Key),
                    sex = Json.Str(kv.Value, "sex", "m"),
                    fixedRole = Json.Bool(kv.Value, "fixed"),
                };
                if (age != null && age.Count == 2 && age[0] is double a0 && age[1] is double a1)
                {
                    r.ageMin = (int)a0;
                    r.ageMax = (int)a1;
                }
                t.roles[kv.Key] = r;
            }
            foreach (var kv in Json.Obj(o, "peoples") ?? new Dictionary<string, object>())
                t.peoples[kv.Key] = (Json.Str(kv.Value, "tongue"), Json.Str(kv.Value, "faith"));
            foreach (var kv in Json.Obj(o, "realms") ?? new Dictionary<string, object>())
                t.realms[kv.Key] = (Strings(Json.Arr(kv.Value, "peoples")), Json.Str(kv.Value, "faith"));
            foreach (var kv in Json.Obj(o, "ports") ?? new Dictionary<string, object>())
                t.ports[kv.Key] = (Strings(Json.Arr(kv.Value, "peoples")), Json.Str(kv.Value, "faith"), Json.Bool(kv.Value, "has_mint"));
            if (t.roles.Count == 0)
                errors?.Add("npc_roles_793.json: no roles.");
            foreach (var kv in t.realms)
                foreach (var p in kv.Value.peoples)
                    if (!t.peoples.ContainsKey(p))
                        errors?.Add($"npc_roles_793.json: the realm {kv.Key} names a people {p} not in peoples.");
            foreach (var kv in t.ports)
                foreach (var p in kv.Value.peoples)
                    if (!t.peoples.ContainsKey(p))
                        errors?.Add($"npc_roles_793.json: the port {kv.Key} names a people {p} not in peoples.");
            return t;
        }

        static List<string> Strings(List<object> list)
        {
            var all = new List<string>();
            foreach (var x in list ?? new List<object>())
                if (x is string s && s.Length > 0)
                    all.Add(s);
            return all;
        }

        /// <summary>
        /// A port's people and faith: the port's own row, else its realm's, else the people the game names (its culture);
        /// the faith falls back to the people's.
        /// </summary>
        public PortFacts Facts(string portId, string realm, string gamePeople)
        {
            var f = new PortFacts { id = portId ?? "", realm = realm ?? "" };
            if (portId != null && ports.TryGetValue(portId, out var p))
            {
                f.peoples.AddRange(p.peoples);
                f.faith = p.faith;
                f.hasMint = p.mint;
            }
            if (f.peoples.Count == 0 && realm != null && realms.TryGetValue(realm, out var r))
                f.peoples.AddRange(r.peoples);
            if (string.IsNullOrEmpty(f.faith) && realm != null && realms.TryGetValue(realm, out var r2))
                f.faith = r2.faith;
            if (f.peoples.Count == 0 && !string.IsNullOrEmpty(gamePeople))
                f.peoples.Add(gamePeople);
            if (string.IsNullOrEmpty(f.faith) && f.peoples.Count > 0 && peoples.TryGetValue(f.peoples[0], out var pp))
                f.faith = pp.faith;
            return f;
        }
    }

    /// <summary>
    /// THE NAMES (night one: Data/encounters/names_placeholder.json, twenty names of the time made from period name parts,
    /// by people and sex). The name forge of night two replaces it.
    /// </summary>
    public sealed class NamePool
    {
        readonly List<(string name, string people, string sex)> names = new List<(string, string, string)>();

        public int Count => names.Count;

        public static NamePool Load(string json, List<string> errors)
        {
            var pool = new NamePool();
            object o;
            try
            {
                o = Json.Parse(json ?? "");
            }
            catch (FormatException e)
            {
                errors?.Add("names_placeholder.json: " + e.Message);
                return pool;
            }
            foreach (var n in Json.Arr(o, "names") ?? new List<object>())
            {
                string name = Json.Str(n, "name");
                if (name.Length > 0)
                    pool.names.Add((name, Json.Str(n, "people"), Json.Str(n, "sex", "m")));
            }
            if (pool.names.Count == 0)
                errors?.Add("names_placeholder.json: no names.");
            return pool;
        }

        /// <summary>A name for a person of a people and sex ("any" for either), drawn with the forge's dice: from that
        /// people if it has any, else from anyone of that sex, else from anyone.</summary>
        public string Pick(string people, string sex, ref SimRandom rnd)
        {
            bool AnySex(string s) => sex == "any" || string.IsNullOrEmpty(sex) || s == sex;
            var from = names.Where(n => n.people == people && AnySex(n.sex)).ToList();
            if (from.Count == 0)
                from = names.Where(n => AnySex(n.sex)).ToList();
            if (from.Count == 0)
                from = names;
            return from.Count == 0 ? "" : from[rnd.Range(0, from.Count)].name;
        }
    }

    /// <summary>
    /// THE PEOPLE THE MERCHANT MEETS (npc_text_encounters.md 2.5): fixed roles at ports (the same toll man meets you each
    /// time; when he dies his heir holds the role, with half his feelings), what each person feels toward each merchant as a
    /// word, and what they remember (12 things, the oldest dropped). Forging is seeded by the world's seed, the port, the
    /// role and the decade, so every game of the same seed meets the same Dorestad toll man in the 790s. Night one: no
    /// decay of feeling over time yet and no news between people (night two).
    /// </summary>
    public sealed class NpcRegistry
    {
        public const int MemoryCap = 12;
        public const int Cap = 2000;

        readonly int seed;
        readonly RoleTable roles;
        readonly NamePool names;
        readonly Dictionary<string, Npc> npcs = new Dictionary<string, Npc>(StringComparer.Ordinal);
        readonly Dictionary<string, string> holders = new Dictionary<string, string>(StringComparer.Ordinal);   // "port/role" -> npc id
        readonly Dictionary<string, int> heldCount = new Dictionary<string, int>(StringComparer.Ordinal);       // "port/role" -> holders so far
        readonly Dictionary<string, Relationship> rels = new Dictionary<string, Relationship>(StringComparer.Ordinal);
        readonly Dictionary<string, int> merchantGeneration = new Dictionary<string, int>(StringComparer.Ordinal);

        public NpcRegistry(int seed, RoleTable roles, NamePool names)
        {
            this.seed = seed;
            this.roles = roles ?? new RoleTable();
            this.names = names ?? new NamePool();
        }

        public int Count => npcs.Count;
        public Npc Get(string id) => id != null && npcs.TryGetValue(id, out var n) ? n : null;
        public IEnumerable<Npc> All => npcs.Values;

        /// <summary>The merchant's generation (0 the first merchant; his son 1).</summary>
        public int Generation(string merchant) => merchant != null && merchantGeneration.TryGetValue(merchant, out int g) ? g : 0;

        /// <summary>
        /// The person who holds a fixed role at a port this year: the living holder, or (none yet, or he has died) a new one
        /// forged for it; an heir takes on half his predecessor's feelings and keeps his memories, tagged.
        /// </summary>
        public Npc Holder(RoleSpec spec, PortFacts port, int year, double day)
        {
            string key = port.id + "/" + spec.role;
            Npc was = holders.TryGetValue(key, out var id) ? Get(id) : null;
            if (was != null && was.AliveIn(year))
                return Seen(was, port.id, day);
            if (was != null)
                was.alive = false;
            var npc = Forge(spec, port, year, was);
            holders[key] = npc.id;
            if (was != null)
                Inherit(was, npc);
            return Seen(npc, port.id, day);
        }

        Npc Seen(Npc n, string port, double day)
        {
            n.lastSeenDay = day;
            n.lastSeenPort = port ?? "";
            return n;
        }

        /// <summary>A new person for a role at a port, seeded by (seed, port, role, decade, holder number).</summary>
        public Npc Forge(RoleSpec spec, PortFacts port, int year, Npc predecessor = null)
        {
            var def = roles.Role(spec.role) ?? new RoleTable.RoleDef { id = spec.role, title = spec.role };
            string key = port.id + "/" + spec.role;
            int number = heldCount.TryGetValue(key, out int k) ? k : 0;
            heldCount[key] = number + 1;
            var rnd = SimRandom.For(seed, $"npc/{key}/{year / 10}/{number}");
            var candidates = spec.people == "port" || string.IsNullOrEmpty(spec.people) ? port.peoples : new List<string> { spec.people };
            string people = candidates.Count == 0 ? "" : candidates[rnd.Range(0, candidates.Count)];
            string sex = !string.IsNullOrEmpty(spec.sex) ? spec.sex : def.sex;
            int ageMin = spec.ageMin > 0 ? spec.ageMin : def.ageMin, ageMax = spec.ageMax > 0 ? spec.ageMax : def.ageMax;
            int age = rnd.Range(ageMin, Math.Max(ageMin, ageMax) + 1);
            var npc = new Npc
            {
                id = $"{port.id}.{spec.role}.{number}",
                role = spec.role,
                homePort = port.id,
                people = people,
                faith = spec.faith == "port" || string.IsNullOrEmpty(spec.faith) ? port.faith : spec.faith,
                tongue = spec.tongue == "port" || string.IsNullOrEmpty(spec.tongue) ? roles.TongueOf(people) : spec.tongue,
                sex = sex == "f" ? "f" : "m",
                bornYear = year - age,
                generation = predecessor != null ? predecessor.generation + 1 : 0,
            };
            npc.name = names.Pick(people, npc.sex, ref rnd);
            // a life of 45 to 75 years (guess: an adult's, once past childhood), never already over when forged
            var life = SimRandom.For(seed, "life/" + npc.id);
            npc.diesYear = npc.bornYear + Math.Max(life.Range(45, 76), age + 1);
            npcs[npc.id] = npc;
            Trim();
            return npc;
        }

        /// <summary>The heir: half the feelings, the memories kept (each still tagged with the generation it happened in).</summary>
        void Inherit(Npc from, Npc to)
        {
            foreach (var r in rels.Values.Where(r => r.npc == from.id).ToList())
            {
                var heir = Rel(to.id, r.merchant);
                heir.feeling = r.feeling * 0.5;
                heir.owed = r.owed;
                foreach (var m in r.memory)
                    heir.memory.Add(new MemoryEntry { day = m.day, card = m.card, option = m.option, word = m.word, note = m.note, delta = m.delta, generation = m.generation });
                rels.Remove(Key(from.id, r.merchant));
            }
        }

        /// <summary>Over the cap: the people longest unseen go first (never a role's holder of the moment).</summary>
        void Trim()
        {
            if (npcs.Count <= Cap)
                return;
            var held = new HashSet<string>(holders.Values);
            foreach (var n in npcs.Values.Where(n => !held.Contains(n.id)).OrderBy(n => n.lastSeenDay).Take(npcs.Count - Cap).ToList())
            {
                npcs.Remove(n.id);
                foreach (var key in rels.Keys.Where(k => k.StartsWith(n.id + "|", StringComparison.Ordinal)).ToList())
                    rels.Remove(key);
            }
        }

        static string Key(string npc, string merchant) => npc + "|" + merchant;

        /// <summary>What a person thinks of a merchant (made at nothing, unmet, when asked first).</summary>
        public Relationship Rel(string npc, string merchant)
        {
            string key = Key(npc, merchant);
            if (!rels.TryGetValue(key, out var r))
                rels[key] = r = new Relationship { npc = npc, merchant = merchant };
            return r;
        }

        public double Feeling(string npc, string merchant) => rels.TryGetValue(Key(npc, merchant), out var r) ? r.feeling : 0.0;

        /// <summary>
        /// A person's feeling toward a merchant moves (kept within -100..100), and they remember it: the card, the option
        /// (with its branch: "hide.failure"), the change and a word. Twelve memories at most; the oldest goes.
        /// </summary>
        public MemoryEntry Feel(string npc, string merchant, double delta, double day, string card, string option, string word)
        {
            var r = Rel(npc, merchant);
            r.feeling = Math.Max(-100.0, Math.Min(100.0, r.feeling + delta));
            return Remember(r, day, card, option, delta, word, Generation(merchant));
        }

        /// <summary>The card's memory line for a choice: it goes on the memory this choice already made with that person, or
        /// is remembered on its own (no change of feeling).</summary>
        public void Note(string npc, string merchant, double day, string card, string option, string note)
        {
            if (string.IsNullOrEmpty(note))
                return;
            var r = Rel(npc, merchant);
            var last = r.memory.Count > 0 ? r.memory[r.memory.Count - 1] : null;
            if (last != null && last.day == day && last.card == (card ?? "") && last.option == (option ?? ""))
                last.note = note;
            else
                Remember(r, day, card, option, 0.0, "", Generation(merchant)).note = note;
        }

        public static MemoryEntry Remember(Relationship r, double day, string card, string option, double delta, string word, int generation)
        {
            var m = new MemoryEntry { day = day, card = card ?? "", option = option ?? "", delta = delta, word = word ?? "", generation = generation };
            r.memory.Add(m);
            while (r.memory.Count > MemoryCap)
                r.memory.RemoveAt(0);
            return m;
        }

        /// <summary>The option the merchant chose last with this person ("declare", "hide.failure"; "" if none remembered).</summary>
        public string LastOption(string npc, string merchant) =>
            rels.TryGetValue(Key(npc, merchant), out var r) && r.memory.Count > 0 ? r.memory[r.memory.Count - 1].option : "";

        /// <summary>Have they met before?</summary>
        public bool Met(string npc, string merchant) => rels.TryGetValue(Key(npc, merchant), out var r) && r.met;

        public void MarkMet(string npc, string merchant) => Rel(npc, merchant).met = true;

        /// <summary>The highest feeling toward the merchant of anyone at this port he has met (-100 if he knows nobody).</summary>
        public double BestFeelingAt(string port, string merchant)
        {
            double best = -100.0;
            foreach (var r in rels.Values)
                if (r.merchant == merchant && r.met && npcs.TryGetValue(r.npc, out var n) && n.alive && n.homePort == port)
                    best = Math.Max(best, r.feeling);
            return best;
        }

        // ------------------------------------------------------------------ the save (plain data; Json.Write writes it)

        public Dictionary<string, object> Capture()
        {
            var people = new List<object>();
            foreach (var n in npcs.Values.OrderBy(n => n.id, StringComparer.Ordinal))
                people.Add(new Dictionary<string, object>
                {
                    ["id"] = n.id, ["name"] = n.name, ["people"] = n.people, ["faith"] = n.faith, ["tongue"] = n.tongue,
                    ["role"] = n.role, ["home"] = n.homePort, ["sex"] = n.sex, ["born"] = n.bornYear, ["dies"] = n.diesYear,
                    ["generation"] = n.generation, ["alive"] = n.alive, ["seen"] = n.lastSeenDay, ["seen_at"] = n.lastSeenPort,
                });
            var held = new Dictionary<string, object>();
            foreach (var kv in holders.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                held[kv.Key] = kv.Value;
            var counts = new Dictionary<string, object>();
            foreach (var kv in heldCount.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                counts[kv.Key] = kv.Value;
            var all = new List<object>();
            foreach (var r in rels.Values.OrderBy(r => Key(r.npc, r.merchant), StringComparer.Ordinal))
            {
                var mem = new List<object>();
                foreach (var m in r.memory)
                    mem.Add(new List<object> { m.day, m.card, m.option, m.delta, m.word, m.generation, m.note });
                all.Add(new Dictionary<string, object>
                {
                    ["npc"] = r.npc, ["merchant"] = r.merchant, ["feeling"] = r.feeling, ["owed"] = r.owed, ["met"] = r.met, ["memory"] = mem,
                });
            }
            var gens = new Dictionary<string, object>();
            foreach (var kv in merchantGeneration.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                gens[kv.Key] = kv.Value;
            return new Dictionary<string, object> { ["npcs"] = people, ["holders"] = held, ["counts"] = counts, ["relationships"] = all, ["generations"] = gens };
        }

        public void Restore(Dictionary<string, object> d)
        {
            npcs.Clear();
            holders.Clear();
            heldCount.Clear();
            rels.Clear();
            merchantGeneration.Clear();
            if (d == null)
                return;
            foreach (var o in Json.Arr(d, "npcs") ?? new List<object>())
            {
                var n = new Npc
                {
                    id = Json.Str(o, "id"), name = Json.Str(o, "name"), people = Json.Str(o, "people"), faith = Json.Str(o, "faith"),
                    tongue = Json.Str(o, "tongue"), role = Json.Str(o, "role"), homePort = Json.Str(o, "home"), sex = Json.Str(o, "sex", "m"),
                    bornYear = (int)Json.Num(o, "born"), diesYear = (int)Json.Num(o, "dies"), generation = (int)Json.Num(o, "generation"),
                    alive = Json.Bool(o, "alive", true), lastSeenDay = Json.Num(o, "seen"), lastSeenPort = Json.Str(o, "seen_at"),
                };
                if (n.id.Length > 0)
                    npcs[n.id] = n;
            }
            foreach (var kv in Json.Obj(d, "holders") ?? new Dictionary<string, object>())
                if (kv.Value is string s)
                    holders[kv.Key] = s;
            foreach (var kv in Json.Obj(d, "counts") ?? new Dictionary<string, object>())
                if (kv.Value is double c)
                    heldCount[kv.Key] = (int)c;
            foreach (var o in Json.Arr(d, "relationships") ?? new List<object>())
            {
                var r = new Relationship
                {
                    npc = Json.Str(o, "npc"), merchant = Json.Str(o, "merchant"), feeling = Json.Num(o, "feeling"),
                    owed = Json.Num(o, "owed"), met = Json.Bool(o, "met"),
                };
                foreach (var m in Json.Arr(o, "memory") ?? new List<object>())
                    if (m is List<object> l && l.Count >= 6)
                        r.memory.Add(new MemoryEntry
                        {
                            day = l[0] is double day ? day : 0.0, card = l[1] as string ?? "", option = l[2] as string ?? "",
                            delta = l[3] is double delta ? delta : 0.0, word = l[4] as string ?? "", generation = l[5] is double g ? (int)g : 0,
                            note = l.Count > 6 ? l[6] as string ?? "" : "",
                        });
                rels[Key(r.npc, r.merchant)] = r;
            }
            foreach (var kv in Json.Obj(d, "generations") ?? new Dictionary<string, object>())
                if (kv.Value is double g)
                    merchantGeneration[kv.Key] = (int)g;
        }
    }
}
