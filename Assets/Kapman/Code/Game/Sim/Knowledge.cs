using System.Collections.Generic;

namespace Kapman.Game.Sim
{
    /// <summary>What one market asked and paid, good by good, on the day someone saw it (deniers; NaN: not dealt in).</summary>
    public sealed class PriceSnapshot
    {
        public int port;
        public double day;
        public float[] asks, pays;
        /// <summary>The ship that saw it in the market (her id; null: not known, e.g. what a waterfront knew at the start).
        /// A ship telling prices she saw herself tells them as a witness; others' are talk.</summary>
        public string seenBy;

        /// <summary>Seen now, in a market.</summary>
        public static PriceSnapshot Of(Market m, int month, double day)
        {
            int n = GameData.Goods.Count;
            var s = new PriceSnapshot { port = m.port.index, day = day, asks = new float[n], pays = new float[n] };
            for (int i = 0; i < n; i++)
                s.asks[i] = s.pays[i] = float.NaN;
            foreach (var g in m.Goods)
            {
                s.pays[g.good.index] = m.Pays(g, month);
                if (g.OnOffer)
                    s.asks[g.good.index] = m.Asks(g, month);
            }
            return s;
        }
    }

    /// <summary>A piece of news: what happened, where, on which game day (raids, ships taken, the chronicle's events,
    /// fairs). Shared: every ship and waterfront that knows it holds the same item.</summary>
    public sealed class NewsItem
    {
        public string id, kind, text;
        public int port = -1;
        public double day;
        /// <summary>The chronicle entry it tells of (its id), if any.</summary>
        public string refId;
        /// <summary>Where it happened (real metres) when that's not a port (a ship taken at sea); NaN if not known.</summary>
        public double x = double.NaN, z = double.NaN;
    }

    /// <summary>
    /// What a ship (or a port's waterfront) knows of the markets: the latest prices seen or heard at each, with the day.
    /// News travels with ships: a ship takes the board's news on arriving and leaves its own (the newer wins), so
    /// prices far away are known late and second-hand, as they were. And the news of the world the same way: each item
    /// with the day it was learned here and who brought it.
    /// </summary>
    public sealed class Knowledge
    {
        readonly Dictionary<int, PriceSnapshot> byPort = new Dictionary<int, PriceSnapshot>();
        readonly Dictionary<string, (NewsItem item, double learned, string from)> news = new Dictionary<string, (NewsItem, double, string)>();
        /// <summary>The most news items kept (the oldest forgotten first).</summary>
        public const int NewsKept = 40;

        /// <summary>The news known here, newest first: the item, the game day it was learned, who brought it.</summary>
        public IEnumerable<(NewsItem item, double learned, string from)> News
        {
            get
            {
                var list = new List<(NewsItem, double, string)>(news.Values);
                list.Sort((a, b) => b.Item1.day.CompareTo(a.Item1.day));
                return list;
            }
        }

        /// <summary>Forgets everything (a saved game loaded).</summary>
        public void Clear()
        {
            byPort.Clear();
            news.Clear();
        }

        /// <summary>Is this piece of news known here?</summary>
        public bool Knows(string id) => id != null && news.ContainsKey(id);

        /// <summary>Learns a piece of news (if not already known), on a day, from someone.</summary>
        public void Hear(NewsItem item, double day, string from)
        {
            if (item == null || news.ContainsKey(item.id))
                return;
            news[item.id] = (item, day, from);
            if (news.Count > NewsKept)
            {
                string oldest = null;
                double d = double.MaxValue;
                foreach (var kv in news)
                    if (kv.Value.item.day < d)
                    {
                        d = kv.Value.item.day;
                        oldest = kv.Key;
                    }
                news.Remove(oldest);
            }
        }

        /// <summary>Each learns the other's news it hasn't (a ship at a waterfront, two ships meeting).</summary>
        public static void SwapNews(Knowledge a, Knowledge b, double day, string aName, string bName)
        {
            var fromA = new List<NewsItem>();
            foreach (var v in a.news.Values)
                fromA.Add(v.item);
            foreach (var v in b.news.Values)
                a.Hear(v.item, day, bName);
            foreach (var item in fromA)
                b.Hear(item, day, aName);
        }

        public IEnumerable<PriceSnapshot> All => byPort.Values;
        public int Count => byPort.Count;

        public PriceSnapshot Of(int port) => byPort.TryGetValue(port, out var s) ? s : null;

        /// <summary>Keeps the newer of what it knows and this.</summary>
        public void Learn(PriceSnapshot s)
        {
            if (s == null)
                return;
            if (!byPort.TryGetValue(s.port, out var old) || old.day < s.day)
                byPort[s.port] = s;
        }

        /// <summary>Each learns what the other knows newer (a ship at a port's waterfront, or two ships meeting).</summary>
        public static void Swap(Knowledge a, Knowledge b)
        {
            var fromA = new List<PriceSnapshot>(a.byPort.Values);
            foreach (var s in b.byPort.Values)
                a.Learn(s);
            foreach (var s in fromA)
                b.Learn(s);
        }
    }
}
