using System;
using System.Collections.Generic;
using System.Linq;

namespace Kapman.Game
{
    /// <summary>What a market asked and paid for a good, as noted (seen, or as told: a heard price can be a little off).</summary>
    public struct SeenPrice
    {
        public float asks, pays;
        public bool onOffer;
    }

    /// <summary>The prices at one port on one day, as the merchant noted them: seen at first hand, or told, by whom
    /// and how far the word had come (a carved price-stick in the chest).</summary>
    public sealed class PriceNote
    {
        public string port;
        /// <summary>The game day the prices were true (seen in the market), and the day they were learned.</summary>
        public double day, learned;
        /// <summary>Null: seen at first hand; else who told it ("heard from a ... of Dorestad").</summary>
        public string source;
        public How how = How.Seen;
        /// <summary>How many mouths the word passed through before the merchant's (0: the teller saw it).</summary>
        public int hops;
        /// <summary>Who told it, as a kind of person ("skipper": a ship's master; "quay": talk on a waterfront; null: not
        /// noted), and the port he is of (the master's home, the waterfront's port; null: not noted). The word on an
        /// item names him from these ("a Frisian skipper", "the toll man at Dorestat").</summary>
        public string teller, tellerOf;
        public readonly Dictionary<string, SeenPrice> prices = new Dictionary<string, SeenPrice>();
    }

    /// <summary>
    /// THE PRICE NOTES (the ledger): every price the merchant has seen or been told, port by port and day by day, kept
    /// as rune sticks in the chest (the user's choice, 2 October 2026). Nothing is thrown away: the history of each
    /// market is there for the charts, and what's believed now comes from the notes by how old and how sure they are
    /// (<see cref="Known.Price"/>). A price told is noted as told, so it may be a little off: more off the further the word
    /// travelled, most of all a rumour (Data/knowing_793.json). The error is fixed when it's heard, never drawn again.
    /// </summary>
    public sealed class Ledger
    {
        // each port's notes, oldest first
        readonly Dictionary<string, List<PriceNote>> byPort = new Dictionary<string, List<PriceNote>>();

        /// <summary>The newest note for each port.</summary>
        public IEnumerable<PriceNote> Notes => byPort.Values.Where(l => l.Count > 0).Select(l => l[l.Count - 1]);

        /// <summary>Every note for a port, oldest first.</summary>
        public IReadOnlyList<PriceNote> History(string port) =>
            port != null && byPort.TryGetValue(port, out var l) ? l : (IReadOnlyList<PriceNote>)Array.Empty<PriceNote>();

        /// <summary>The newest note for a port, or null.</summary>
        public PriceNote Latest(string port)
        {
            var h = History(port);
            return h.Count > 0 ? h[h.Count - 1] : null;
        }

        /// <summary>How many notes in all.</summary>
        public int Count => byPort.Values.Sum(l => l.Count);

        /// <summary>Notes down a market's prices today, seen at first hand (again on the same day: the note is renewed,
        /// as after a deal has moved them).</summary>
        public void Note(Market m, double day, int month)
        {
            var note = new PriceNote { port = m.port.id, day = day, learned = day, how = How.Seen };
            foreach (var g in m.Goods)
                note.prices[g.good.id] = new SeenPrice { asks = m.Asks(g, month), pays = m.Pays(g, month), onOffer = g.OnOffer };
            var list = ListOf(note.port);
            if (list.Count > 0 && list[list.Count - 1].how == How.Seen && Math.Abs(list[list.Count - 1].day - day) < 0.5)
                list[list.Count - 1] = note;
            else
                Insert(list, note);
        }

        /// <summary>Prices someone else saw, noted as heard; true if they're newer than anything noted for that port.</summary>
        public bool Hear(Kapman.Game.Sim.PriceSnapshot snap, string source) => Hear(snap, source, How.Talk, 1, snap != null ? snap.day : 0.0);

        /// <summary>Prices told: how (a witness, talk, a rumour...), through how many mouths, learned on which day. Each
        /// price is off by what the word's way allows (fixed now). Kept even if older than what's noted (it's history);
        /// true only if it's the newest for its port. `teller` and `tellerOf`: who told it and the port he is of
        /// (<see cref="PriceNote.teller"/>), where the caller knows.</summary>
        public bool Hear(Kapman.Game.Sim.PriceSnapshot snap, string source, How how, int hops, double learned, string teller = null, string tellerOf = null)
        {
            if (snap == null || snap.port < 0 || snap.port >= GameData.Ports.Count)
                return false;
            string port = GameData.Ports[snap.port].id;
            var list = ListOf(port);
            foreach (var n in list)
                if (Math.Abs(n.day - snap.day) < 1e-6)
                    return false;                                        // (that day's prices are noted already)
            var note = new PriceNote
            {
                port = port, day = snap.day, learned = learned, source = source, how = how, hops = Math.Max(0, hops), teller = teller, tellerOf = tellerOf,
            };
            float off = Known.ErrorShare(how, note.hops);
            for (int i = 0; i < snap.pays.Length && i < GameData.Goods.Count; i++)
            {
                if (float.IsNaN(snap.pays[i]))
                    continue;
                bool offer = !float.IsNaN(snap.asks[i]);
                float f = off > 0f ? 1f + off * (2f * Known.Hash01(port, snap.day, i, (int)how) - 1f) : 1f;
                note.prices[GameData.Goods[i].id] = new SeenPrice { asks = offer ? snap.asks[i] * f : 0f, pays = snap.pays[i] * f, onOffer = offer };
            }
            Insert(list, note);
            return list[list.Count - 1] == note;
        }

        List<PriceNote> ListOf(string port)
        {
            if (!byPort.TryGetValue(port, out var l))
                byPort[port] = l = new List<PriceNote>();
            return l;
        }

        static void Insert(List<PriceNote> list, PriceNote note)
        {
            int i = list.Count;
            while (i > 0 && list[i - 1].day > note.day)
                i--;
            list.Insert(i, note);
        }

        /// <summary>Everywhere a good's price has been noted: (port, day, price) from each port's newest note that has it,
        /// the most recent first.</summary>
        public IEnumerable<(string port, double day, SeenPrice price)> Seen(string good)
        {
            var found = new List<(string, double, SeenPrice)>();
            foreach (var l in byPort.Values)
                for (int i = l.Count - 1; i >= 0; i--)
                    if (l[i].prices.TryGetValue(good, out var p))
                    {
                        found.Add((l[i].port, l[i].day, p));
                        break;
                    }
            return found.OrderByDescending(x => x.Item2);
        }

        /// <summary>
        /// Thins old notes so a long game's saves stay small: everything of the last year is kept; older, one note a
        /// month for each port (a seen one first), and after `seasonAfterYears` one a season.
        /// </summary>
        public void Thin(double today, double keepAllDays = 365.0, double seasonAfterYears = 10.0)
        {
            foreach (var l in byPort.Values)
            {
                var keep = new List<PriceNote>();
                var bucketBest = new Dictionary<long, PriceNote>();
                foreach (var n in l)
                {
                    double age = today - n.day;
                    if (age <= keepAllDays)
                    {
                        keep.Add(n);
                        continue;
                    }
                    long bucket = age > seasonAfterYears * 365.0 ? (long)Math.Floor(n.day / 91.0) * 2 + 1 : (long)Math.Floor(n.day / 30.0) * 2;
                    if (!bucketBest.TryGetValue(bucket, out var best) || (n.how == How.Seen && best.how != How.Seen))
                        bucketBest[bucket] = n;
                }
                keep.AddRange(bucketBest.Values);
                keep.Sort((a, b) => a.day.CompareTo(b.day));
                l.Clear();
                l.AddRange(keep);
            }
        }

        /// <summary>The notes as plain data (a saved game).</summary>
        public List<object> Capture()
        {
            var list = new List<object>();
            foreach (var l in byPort.Values)
                foreach (var n in l)
                {
                    var prices = new Dictionary<string, object>();
                    foreach (var kv in n.prices)
                        prices[kv.Key] = new List<object> { kv.Value.asks, kv.Value.pays, kv.Value.onOffer };
                    var d = new Dictionary<string, object>
                    {
                        ["port"] = n.port, ["day"] = n.day, ["learned"] = n.learned, ["source"] = n.source, ["how"] = (int)n.how,
                        ["hops"] = n.hops, ["prices"] = prices,
                    };
                    if (!string.IsNullOrEmpty(n.teller))
                        d["teller"] = n.teller;
                    if (!string.IsNullOrEmpty(n.tellerOf))
                        d["teller_of"] = n.tellerOf;
                    list.Add(d);
                }
            return list;
        }

        /// <summary>The notes as saved (an older save has one note per port, without how it was learned).</summary>
        public void Restore(List<object> data)
        {
            byPort.Clear();
            foreach (var x in data ?? new List<object>())
            {
                string source = Json.Str(x, "source", null);
                var note = new PriceNote
                {
                    port = Json.Str(x, "port"), day = Json.Num(x, "day"), source = source,
                    how = (How)(int)Json.Num(x, "how", source == null ? (double)How.Seen : (double)How.Talk),
                    hops = (int)Json.Num(x, "hops", source == null ? 0 : 1),
                    teller = Json.Str(x, "teller", null), tellerOf = Json.Str(x, "teller_of", null),
                };
                note.learned = Json.Num(x, "learned", note.day);
                foreach (var kv in Json.Obj(x, "prices") ?? new Dictionary<string, object>())
                    if (kv.Value is List<object> p && p.Count >= 3)
                        note.prices[kv.Key] = new SeenPrice { asks = (float)(double)p[0], pays = (float)(double)p[1], onOffer = p[2] is bool b && b };
                if (!string.IsNullOrEmpty(note.port))
                    Insert(ListOf(note.port), note);
            }
        }
    }
}
