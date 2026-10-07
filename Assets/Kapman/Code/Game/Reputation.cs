using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// STANDING - what each place and each region thinks of the merchant (the user's spec: "Reputation: a big one. Slavery
    /// etc. affects moral standing with villages and regions independently"). A deed (Sim.Deeds) counts fully where it was
    /// done, partly across its region, a little among the same people elsewhere; each people weighs each kind of deed by
    /// its own values (Data/reputation_793.json). Standing moves his prices there, how hard the king's men search him, and
    /// whether a place will deal with him at all. -100 to +100.
    /// </summary>
    public static class Reputation
    {
        static readonly Dictionary<string, double> ports = new Dictionary<string, double>();
        static readonly Dictionary<string, double> regions = new Dictionary<string, double>();
        static readonly Dictionary<string, Dictionary<string, double>> deedValues = new Dictionary<string, Dictionary<string, double>>();
        static readonly List<(double below, string word)> words = new List<(double, string)>();
        static double spreadPort = 1.0, spreadRegion = 0.4, spreadPeople = 0.1;
        static double priceAt100 = 0.1, searchAtMinus100 = 0.4, refuseBelow = -60.0;
        static readonly Dictionary<string, double> lastDealt = new Dictionary<string, double>();
        /// <summary>A standing changed (a port's id or a region's).</summary>
        public static event Action Changed;

        public static void Load(TextAsset json)
        {
            ports.Clear();
            regions.Clear();
            deedValues.Clear();
            words.Clear();
            lastDealt.Clear();
            travelling.Clear();
            if (json == null)
                return;
            var o = Json.Parse(json.text);
            var news = Json.Obj(o, "news");
            newsMinPoints = Json.Num(news, "min_points", 2.0);
            newsForgottenDays = Json.Num(news, "forgotten_days", 400.0);
            var spread = Json.Obj(o, "spread");
            spreadPort = Json.Num(spread, "port", spreadPort);
            spreadRegion = Json.Num(spread, "region", spreadRegion);
            spreadPeople = Json.Num(spread, "people_elsewhere", spreadPeople);
            foreach (var kv in Json.Obj(o, "deeds") ?? new Dictionary<string, object>())
            {
                var values = new Dictionary<string, double>();
                foreach (var v in kv.Value as Dictionary<string, object> ?? new Dictionary<string, object>())
                    if (v.Value is double d)
                        values[v.Key] = d;
                deedValues[kv.Key] = values;
            }
            var fx = Json.Obj(o, "effects");
            priceAt100 = Json.Num(fx, "price_at_100", priceAt100);
            searchAtMinus100 = Json.Num(fx, "search_at_minus_100", searchAtMinus100);
            refuseBelow = Json.Num(fx, "refuse_below", refuseBelow);
            foreach (var w in Json.Arr(o, "words") ?? new List<object>())
                if (w is List<object> l && l.Count >= 2 && l[0] is double below)
                    words.Add((below, l[1] as string));
            foreach (var kv in Json.Obj(o, "start") ?? new Dictionary<string, object>())
                if (kv.Value is double d)
                {
                    if (GameData.Port(kv.Key) != null)
                        ports[kv.Key] = d;
                    else
                        regions[kv.Key] = d;
                }
        }

        public static double OfPort(string id) => id != null && ports.TryGetValue(id, out double v) ? v : 0.0;
        public static double OfRegion(string id) => id != null && regions.TryGetValue(id, out double v) ? v : 0.0;

        /// <summary>His standing as a place sees it: its own, and half its region's (-100 to +100).</summary>
        public static double At(PortDef p) => p == null ? 0.0 : Clamp(OfPort(p.id) + 0.5 * OfRegion(p.region));

        /// <summary>
        /// A deed of a kind done at a place: the standing it earns or costs there and across its region at once (word of
        /// mouth), and among the same people in other regions when the news of it gets there - it's posted at the place and
        /// travels with the ships like any news (a deed too small to be news never goes that far). With no world running
        /// (the tests), the far share counts at once. `what`: the deed in plain words, for the news. Returns the points at
        /// the place.
        /// </summary>
        public static double Deed(string kind, PortDef at, string what = null)
        {
            if (at == null || string.IsNullOrEmpty(kind) || !deedValues.TryGetValue(kind, out var values))
                return 0.0;
            string people = Wording.CultureOf(at);
            if (people == null || !values.TryGetValue(people, out double points) || points == 0.0)
                return 0.0;
            ports[at.id] = Clamp(OfPort(at.id) + points * spreadPort);
            regions[at.region] = Clamp(OfRegion(at.region) + points * spreadRegion);
            var elsewhere = new Dictionary<string, double>();
            foreach (var p in GameData.Ports)
                if (p.region != at.region && Wording.CultureOf(p) == people)
                    elsewhere[p.region] = points * spreadPeople;
            var world = World;
            if (world == null)
                foreach (var kv in elsewhere)
                    regions[kv.Key] = Clamp(OfRegion(kv.Key) + kv.Value);
            else if (Math.Abs(points) >= newsMinPoints && elsewhere.Count > 0)
            {
                var item = world.Post("deed", $"News of you at {at.name}: {(string.IsNullOrEmpty(what) ? kind.Replace('_', ' ') : what)}.", at.index);
                travelling.Add(new Travelling { news = item.id, day = world.Day, regions = elsewhere });
            }
            Changed?.Invoke();
            return points * spreadPort;
        }

        /// <summary>A deed's news on its way: the news item, the day, and the standing still to come in each region.</summary>
        sealed class Travelling
        {
            public string news;
            public double day;
            public Dictionary<string, double> regions;
        }

        static readonly List<Travelling> travelling = new List<Travelling>();
        static Sim.WorldSim worldOverride;

        /// <summary>The world the news travels in: the game's, or one a test run sets.</summary>
        public static Sim.WorldSim World
        {
            get => worldOverride ?? (KapmanGame.Current != null ? KapmanGame.Current.World : null);
            set => worldOverride = value;
        }
        static double newsMinPoints = 2.0, newsForgottenDays = 400.0;

        /// <summary>Deeds whose news is still on its way to some region.</summary>
        public static int TravellingCount => travelling.Count;

        /// <summary>Once a game day: where a deed's news has reached a port of a region still waiting for it, the standing
        /// it earns or costs there counts now. News that never got there is let go after a year or so.</summary>
        public static void Daily()
        {
            var world = World;
            if (world == null || travelling.Count == 0)
                return;
            bool changed = false;
            for (int i = travelling.Count - 1; i >= 0; i--)
            {
                var t = travelling[i];
                foreach (string r in new List<string>(t.regions.Keys))
                    foreach (var p in GameData.Ports)
                        if (p.region == r && world.Board(p.index).Knows(t.news))
                        {
                            regions[r] = Clamp(OfRegion(r) + t.regions[r]);
                            t.regions.Remove(r);
                            changed = true;
                            break;
                        }
                if (t.regions.Count == 0 || world.Day - t.day > newsForgottenDays)
                    travelling.RemoveAt(i);
            }
            if (changed)
                Changed?.Invoke();
        }

        /// <summary>He dealt in a market today: known as a trader who comes back (once a day a place).</summary>
        public static void Dealt(PortDef at, double day)
        {
            if (at == null || (lastDealt.TryGetValue(at.id, out double last) && day - last < 1.0))
                return;
            lastDealt[at.id] = day;
            Deed("fair_dealing", at);
        }

        /// <summary>The share his prices move there: + he buys cheaper and sells dearer.</summary>
        public static double PriceFavour(PortDef p) => At(p) / 100.0 * priceAt100;

        /// <summary>How much more often the king's men search him there (1 = as anyone).</summary>
        public static double SearchFactor(PortDef p) => 1.0 + Math.Max(0.0, -At(p)) / 100.0 * searchAtMinus100;

        /// <summary>Will this place not deal with him at all?</summary>
        public static bool Refuses(PortDef p) => At(p) < refuseBelow;

        /// <summary>The word for a standing ("a stranger", "well regarded"...).</summary>
        public static string Word(double s)
        {
            foreach (var (below, word) in words)
                if (s < below)
                    return word;
            return words.Count > 0 ? words[words.Count - 1].word : s.ToString("0");
        }

        /// <summary>"Here: known (+24). In Frisia: a stranger (+4)."</summary>
        public static string Line(PortDef p)
        {
            if (p == null)
                return "";
            double here = At(p), region = OfRegion(p.region);
            return $"Your standing here: {Word(here)} ({here:+0;-0;0}). In the region: {Word(region)} ({region:+0;-0;0}).";
        }

        static double Clamp(double v) => Math.Max(-100.0, Math.Min(100.0, v));

        public static Dictionary<string, object> Capture()
        {
            var p = new Dictionary<string, object>();
            foreach (var kv in ports)
                p[kv.Key] = kv.Value;
            var r = new Dictionary<string, object>();
            foreach (var kv in regions)
                r[kv.Key] = kv.Value;
            var going = new List<object>();
            foreach (var x in travelling)
            {
                var left = new Dictionary<string, object>();
                foreach (var kv in x.regions)
                    left[kv.Key] = kv.Value;
                going.Add(new Dictionary<string, object> { ["news"] = x.news, ["day"] = x.day, ["regions"] = left });
            }
            return new Dictionary<string, object> { ["ports"] = p, ["regions"] = r, ["travelling"] = going };
        }

        public static void Restore(Dictionary<string, object> d)
        {
            if (d == null)
                return;
            ports.Clear();
            regions.Clear();
            foreach (var kv in Json.Obj(d, "ports") ?? new Dictionary<string, object>())
                if (kv.Value is double v)
                    ports[kv.Key] = v;
            foreach (var kv in Json.Obj(d, "regions") ?? new Dictionary<string, object>())
                if (kv.Value is double v)
                    regions[kv.Key] = v;
            travelling.Clear();
            foreach (var x in Json.Arr(d, "travelling") ?? new List<object>())
            {
                var left = new Dictionary<string, double>();
                foreach (var kv in Json.Obj(x, "regions") ?? new Dictionary<string, object>())
                    if (kv.Value is double v)
                        left[kv.Key] = v;
                travelling.Add(new Travelling { news = Json.Str(x, "news"), day = Json.Num(x, "day"), regions = left });
            }
            Changed?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            ports.Clear();
            regions.Clear();
            lastDealt.Clear();
            travelling.Clear();
            worldOverride = null;
        }
    }
}
