using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Kapman.Game.Sim
{
    /// <summary>One passage: who, from where to where, when, what she carried, and what it came to (deniers).</summary>
    public sealed class VoyageRecord
    {
        public string ship, cls, from, to, cargo;
        public int year;
        public double departed, arrived = double.NaN, km, cost, revenue;
        public bool lost;
    }

    /// <summary>
    /// THE NUMBERS of a run: every voyage, every trader's year, and the daily prices of a dozen goods at every port - written
    /// as spreadsheets (CSV) and a short summary for the user to read (the user: "more excel spreadsheet game").
    /// </summary>
    public sealed class SimStats
    {
        public readonly List<VoyageRecord> voyages = new List<VoyageRecord>();
        readonly Dictionary<int, VoyageRecord> open = new Dictionary<int, VoyageRecord>();
        public readonly StringBuilder prices = new StringBuilder("day,date,port,good,asks,pays,stock\n");
        public int lost, deals, broke, newPartners;
        public double dealtDeniers;
        /// <summary>Seamanship: times a ship gave way, stood out to one side, pairs left too close (ship-hours), passages
        /// that waited for a fair wind (and the days waited), arrivals that anchored off a full landing (and the hours).</summary>
        public int giveWays, standOuts, closeCalls, waitedForWind, anchored;
        public double waitedDays, anchoredHours;
        /// <summary>Raiders: every raid on a port (done or not) and every ship taken at sea.</summary>
        public readonly List<RaidRecord> raids = new List<RaidRecord>();
        public int shipsTaken;
        /// <summary>The law: traders searched leaving a port, and searches that seized goods.</summary>
        public int searches, seizures;

        /// <summary>Days between the rows of prices_daily.csv (1: every day; a ten-year run keeps every 7th; 0: none, in the game).</summary>
        public int csvEvery = 1;
        /// <summary>Days between the price samples the report draws (report.html; 0: none, in the game).</summary>
        public int sampleEvery = 7;
        /// <summary>The report's price samples: what each port pays for each watched good ("port|good" -> (day, pays)).</summary>
        public readonly Dictionary<string, List<(int day, float pays)>> series = new Dictionary<string, List<(int, float)>>();
        /// <summary>Days a market that usually has a good had none ("port|good" -> days).</summary>
        public readonly Dictionary<string, int> stockOuts = new Dictionary<string, int>();

        /// <summary>The goods whose prices are kept every day.</summary>
        public string[] watch = { "wheat", "rye", "herring", "sea_salt", "rhenish_wine", "frisian_cloth", "wool_cloth", "bar_iron",
                                  "honey", "squirrel", "amber", "whetstones", "quernstones", "wax" };

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public void VoyageStarted(WorldSim w, SimShip s)
        {
            if (s.raider)
                return;                                                   // (raids.csv has the raiders)
            var r = new VoyageRecord
            {
                ship = s.id, cls = s.cls.id, from = GameData.Ports[s.at].id, to = GameData.Ports[s.dest].id, cargo = s.cargoNote, year = w.Date.year,
                departed = w.Day, km = s.passageKm, cost = s.voyageCost,
            };
            voyages.Add(r);
            open[s.index] = r;
        }

        public void Arrived(WorldSim w, SimShip s)
        {
            if (open.TryGetValue(s.index, out var r))
                r.arrived = w.Day;
        }

        public void VoyageEnded(WorldSim w, SimShip s)
        {
            if (open.TryGetValue(s.index, out var r))
            {
                r.revenue = s.voyageRevenue;
                open.Remove(s.index);
            }
            s.profit += s.voyageRevenue - s.voyageCost;
            s.voyageRevenue = 0.0;
            s.voyageCost = 0.0;
        }

        public void Lost(WorldSim w, SimShip s)
        {
            lost++;
            if (open.TryGetValue(s.index, out var r))
                r.lost = true;
        }

        public void Daily(WorldSim w)
        {
            var date = w.Date;
            int day = (int)Math.Floor(w.Day);
            foreach (var m in Markets.All)
            {
                foreach (var g in m.Goods)
                    if (g.usual >= 1f && g.stock < 1f)
                    {
                        string key = m.port.id + "|" + g.good.id;
                        stockOuts.TryGetValue(key, out int n);
                        stockOuts[key] = n + 1;
                    }
                foreach (string id in watch)
                {
                    var g = m.Get(id);
                    if (g == null)
                        continue;
                    if (sampleEvery > 0 && day % sampleEvery == 0)
                    {
                        string key = m.port.id + "|" + id;
                        if (!series.TryGetValue(key, out var list))
                            series[key] = list = new List<(int, float)>();
                        list.Add((day, m.Pays(g, date.month)));
                    }
                    if (csvEvery <= 0 || day % csvEvery != 0)
                        continue;
                    prices.Append(day).Append(',').Append(date.year).Append('-').Append(date.month.ToString("00")).Append('-').Append(date.day.ToString("00"))
                          .Append(',').Append(m.port.id).Append(',').Append(id).Append(',')
                          .Append(g.OnOffer ? m.Asks(g, date.month).ToString("0.##", Inv) : "").Append(',')
                          .Append(m.Pays(g, date.month).ToString("0.##", Inv)).Append(',')
                          .Append(g.stock.ToString("0.#", Inv)).Append('\n');
                }
            }
        }

        /// <summary>Writes voyages.csv, traders.csv, prices_daily.csv and summary.txt into a folder; returns the summary.</summary>
        public string Write(WorldSim w, string folder, double seconds)
        {
            Directory.CreateDirectory(folder);
            var v = new StringBuilder("ship,class,from,to,departed_day,arrived_day,days,km,cargo,cost_d,revenue_d,profit_d,lost\n");
            foreach (var r in voyages)
                v.Append(r.ship).Append(',').Append(r.cls).Append(',').Append(r.from).Append(',').Append(r.to).Append(',')
                 .Append(r.departed.ToString("0.0", Inv)).Append(',').Append(double.IsNaN(r.arrived) ? "" : r.arrived.ToString("0.0", Inv)).Append(',')
                 .Append(double.IsNaN(r.arrived) ? "" : (r.arrived - r.departed).ToString("0.0", Inv)).Append(',')
                 .Append(r.km.ToString("0", Inv)).Append(",\"").Append(r.cargo).Append("\",")
                 .Append(r.cost.ToString("0.0", Inv)).Append(',').Append(r.revenue.ToString("0.0", Inv)).Append(',')
                 .Append((r.revenue - r.cost).ToString("0.0", Inv)).Append(',').Append(r.lost ? "lost" : "").Append('\n');
            File.WriteAllText(Path.Combine(folder, "voyages.csv"), v.ToString());

            var rules = TradeRules.Current;
            var traders = w.ships.Where(s => !s.raider).ToList();
            var rc = new StringBuilder("day,date,band,origin,where,done,men,defence,loot_units,captives,what\n");
            foreach (var r in raids)
                rc.Append(r.day.ToString("0.0", Inv)).Append(',').Append(r.date).Append(',').Append(r.band).Append(',').Append(r.origin).Append(',')
                  .Append(r.port).Append(',').Append(r.done ? "yes" : "no").Append(',').Append(r.men.ToString("0", Inv)).Append(',')
                  .Append(r.defence.ToString("0", Inv)).Append(',').Append(r.loot).Append(',').Append(r.captives).Append(",\"").Append(r.what).Append("\"\n");
            File.WriteAllText(Path.Combine(folder, "raids.csv"), rc.ToString());
            var t = new StringBuilder("ship,class,home,state,at,voyages,start_silver_d,silver_d,cargo_worth_d,gain_d\n");
            foreach (var s in traders)
            {
                double silver = s.purse.Silver(rules), cargo = s.hold.Lots.Sum(l => l.cost);
                t.Append(s.id).Append(',').Append(s.cls.id).Append(',').Append(GameData.Ports[s.home].id).Append(',').Append(s.state).Append(',')
                 .Append(GameData.Ports[s.at].id).Append(',').Append(s.voyages).Append(',')
                 .Append(s.startWorth.ToString("0", Inv)).Append(',').Append(silver.ToString("0", Inv)).Append(',').Append(cargo.ToString("0", Inv)).Append(',')
                 .Append((silver + cargo - s.startWorth).ToString("0", Inv)).Append('\n');
            }
            File.WriteAllText(Path.Combine(folder, "traders.csv"), t.ToString());
            File.WriteAllText(Path.Combine(folder, "prices_daily.csv"), prices.ToString());

            var gains = traders.Select(s => s.purse.Silver(rules) + s.hold.Lots.Sum(l => l.cost) - s.startWorth).OrderBy(x => x).ToList();
            var done = voyages.Where(r => !double.IsNaN(r.arrived)).ToList();
            var busiest = voyages.GroupBy(r => string.CompareOrdinal(r.from, r.to) < 0 ? r.from + " - " + r.to : r.to + " - " + r.from)
                                 .OrderByDescending(g => g.Count()).Take(5).Select(g => $"{g.Key} ({g.Count()})");
            var sum = new StringBuilder();
            sum.AppendLine($"Kapman world simulation: seed {w.seed}, {w.Date} after {w.Day:0} days, {seconds:0.0} s to run.");
            sum.AppendLine($"Traders: {traders.Count} ({string.Join(", ", traders.GroupBy(s => s.cls.id).OrderByDescending(g => g.Count()).Select(g => $"{g.Count()} {g.Key}"))}).");
            sum.AppendLine($"Voyages: {voyages.Count} begun, {done.Count} arrived; {(done.Count > 0 ? done.Average(r => r.arrived - r.departed) : 0):0.0} days on average; {lost} ships lost to gales.");
            sum.AppendLine($"Voyages per trader: {(traders.Count > 0 ? (double)voyages.Count / traders.Count : 0):0.0}; {traders.Count(s => s.voyages >= 5)} traders made 5 or more.");
            sum.AppendLine($"Trader gains (silver + cargo at cost - start), deniers: median {(gains.Count > 0 ? gains[gains.Count / 2] : 0):0}, lowest {(gains.Count > 0 ? gains[0] : 0):0}, highest {(gains.Count > 0 ? gains[gains.Count - 1] : 0):0}; {gains.Count(x => x > 0)} of {gains.Count} gained.");
            sum.AppendLine($"Deals: {deals} by the world's traders, {dealtDeniers:0} deniers changing hands; crews' pay {w.ships.Sum(s => s.wages):0} deniers; {broke} times a ship couldn't pay her crew and lay up; {newPartners} times a new partner put silver into one laid up broke.");
            sum.AppendLine($"Busiest passages: {string.Join("; ", busiest)}.");
            sum.AppendLine($"Seamanship: {giveWays} times a ship gave way ({standOuts} by standing out to one side); {closeCalls} ship-hours two ships were left closer than their passing room; " +
                           $"{waitedForWind} sailings waited for a fair wind ({(waitedForWind > 0 ? waitedDays / waitedForWind : 0):0.0} days on average); " +
                           $"{anchored} arrivals anchored off a full landing ({(anchored > 0 ? anchoredHours / anchored : 0):0} hours on average).");
            int bands = w.ships.Count(s => s.raider), struck = raids.Count(r => r.done && !r.port.StartsWith("at sea"));
            sum.AppendLine(w.raiders == null ? "Raiders: none in this run." :
                $"Raiders: {bands} bands; {struck} places raided ({string.Join(", ", raids.Where(r => r.done && !r.port.StartsWith("at sea")).Select(r => r.port + " " + r.date))}); " +
                $"{raids.Count(r => !r.done)} times a place was too strong; {shipsTaken} ships taken at sea (raids.csv).");
            sum.AppendLine($"The law: {searches} traders searched leaving port; {seizures} had goods seized.");
            sum.AppendLine("Files: voyages.csv (every passage), traders.csv (every trader's year), prices_daily.csv (a dozen goods at every port, daily), raids.csv.");
            sum.AppendLine(w.routes is LaneRoutes ? "Routes: the sea lanes (Bake/lanes)." : "Routes are straight lines (no lanes.bin): distances and days are estimates.");
            File.WriteAllText(Path.Combine(folder, "summary.txt"), sum.ToString());
            return sum.ToString();
        }
    }
}
