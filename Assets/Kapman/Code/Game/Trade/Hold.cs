using System;
using System.Collections.Generic;
using System.Linq;

namespace Kapman.Game
{
    /// <summary>A lot in the hold: so many units of a good, what they cost, and the day they came aboard.</summary>
    public sealed class Lot
    {
        public GoodDef good;
        public int count;
        /// <summary>Deniers paid for the whole lot (still aboard).</summary>
        public double cost;
        public double day;
        /// <summary>Where the goods came from when they were not bought or the merchant's own: "count" for the casks the
        /// lender put aboard at the start. They are the merchant's to sell and are owed for with the boat, so what they
        /// fetch is not told as gain. "": the merchant's own.</summary>
        public string whose = "";
    }

    /// <summary>
    /// THE HOLD: what the ship carries, lot by lot (the oldest goes first). It's full when the goods' space (kg x
    /// bulk) reaches the ship's cargo (ShipClass.cargoKg): light, bulky goods fill her before their weight does.
    /// </summary>
    public sealed class Hold
    {
        readonly List<Lot> lots = new List<Lot>();
        public float capacity = 900f;

        public IReadOnlyList<Lot> Lots => lots;
        public float Used => lots.Sum(l => l.count * l.good.Space);
        public float Free => Math.Max(0f, capacity - Used);
        public float Weight => lots.Sum(l => l.count * l.good.kg);

        public int Count(string good) => lots.Where(l => l.good.id == good).Sum(l => l.count);

        /// <summary>How many units of a good aboard are another's (the count's), not the merchant's own.</summary>
        public int Owed(string good) => lots.Where(l => l.good.id == good && l.whose.Length > 0).Sum(l => l.count);

        /// <summary>Of the units last taken out by <see cref="Remove"/>: how many had cost nothing (given or found, not
        /// bought), and how many were another's (the count's).</summary>
        public int LastFree { get; private set; }
        public int LastOwed { get; private set; }

        /// <summary>Empties it (a saved game loaded).</summary>
        public void Clear() => lots.Clear();

        /// <summary>A lot as it was (a saved game loaded): kept as its own lot.</summary>
        public void Put(GoodDef g, int n, double cost, double day, string whose = "")
        {
            if (g != null && n > 0)
                lots.Add(new Lot { good = g, count = n, cost = cost, day = day, whose = whose ?? "" });
        }

        /// <summary>The hold as plain data: [good, count, cost, day, whose] for each lot.</summary>
        public List<object> Capture() =>
            lots.Select(l => (object)new List<object> { l.good.id, l.count, l.cost, l.day, l.whose }).ToList();

        /// <summary>(A save from before lots had an owner has four values a lot: all the merchant's own.)</summary>
        public void Restore(List<object> data)
        {
            lots.Clear();
            foreach (var x in data ?? new List<object>())
                if (x is List<object> l && l.Count >= 4)
                    Put(GameData.Good(l[0] as string), (int)(double)l[1], (double)l[2], (double)l[3], l.Count >= 5 ? l[4] as string : "");
        }

        /// <summary>Deniers paid for one unit of what's aboard, on average (0 if none).</summary>
        public double CostEach(string good)
        {
            int n = Count(good);
            return n > 0 ? lots.Where(l => l.good.id == good).Sum(l => l.cost) / n : 0.0;
        }

        /// <summary>How many more units of a good there's room for.</summary>
        public int RoomFor(GoodDef g) => g == null ? 0 : (int)Math.Floor(Free / Math.Max(0.001f, g.Space) + 1e-6);

        /// <summary>Puts units aboard. Bought the same day as the last lot of the good they join it, but goods that were
        /// paid for never join goods that cost nothing or are another's: a cask bought beside the count's four would
        /// otherwise be five casks at the cost of one, and every sale after would read as gain.</summary>
        public void Add(GoodDef g, int n, double cost, double day, string whose = "")
        {
            if (g == null || n <= 0)
                return;
            whose = whose ?? "";
            var last = lots.LastOrDefault(l => l.good == g);
            if (last != null && Math.Abs(last.day - day) < 1.0 && last.whose == whose && (last.cost > 0.0) == (cost > 0.0))
            {
                last.count += n;                                     // bought the same day: one lot
                last.cost += cost;
            }
            else
                lots.Add(new Lot { good = g, count = n, cost = cost, day = day, whose = whose });
        }

        /// <summary>Takes units out, the oldest first; returns what they cost (deniers).</summary>
        public double Remove(GoodDef g, int n)
        {
            double cost = 0.0;
            LastFree = LastOwed = 0;
            foreach (var l in lots.Where(l => l.good == g).ToList())
            {
                if (n <= 0)
                    break;
                int take = Math.Min(n, l.count);
                double part = l.cost * take / l.count;
                if (l.cost <= 0.0)
                    LastFree += take;
                if (l.whose.Length > 0)
                    LastOwed += take;
                l.count -= take;
                l.cost -= part;
                cost += part;
                n -= take;
                if (l.count <= 0)
                    lots.Remove(l);
            }
            return cost;
        }

        /// <summary>The goods aboard (each once) and how many of each.</summary>
        public IEnumerable<(GoodDef good, int count)> Contents =>
            lots.GroupBy(l => l.good).Select(g => (g.Key, g.Sum(l => l.count)));

        /// <summary>The goods aboard that a law can bite on (arms, captives, sacred things, horses).</summary>
        public IEnumerable<GoodDef> UnderLaw => lots.Where(l => !string.IsNullOrEmpty(l.good.law)).Select(l => l.good).Distinct();
    }
}
