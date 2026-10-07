using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE REPORT of a test run (Plans/ai_fleets.md, phase 6): one page, report.html, to open in a browser, drawn with
    /// inline SVG (nothing to install, nothing fetched):
    ///   the summary and a year-by-year table;
    ///   prices: what the busiest ports pay for each watched good, week by week;
    ///   gain by kind of ship;
    ///   raids on a timeline;
    ///   the passages sailed, as a map (lines between ports, thicker the more ships sailed them);
    ///   stock-outs: markets that usually have a good and had none, the longest first;
    ///   health: ships that stopped sailing, prices that drifted far from where they began.
    /// </summary>
    public static class SimReport
    {
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
        static readonly string[] Colours = { "#1f5f8b", "#b03a2e", "#2e7d4f", "#7d3c98", "#b9770e", "#566573" };

        /// <summary>Writes the report; returns its path.</summary>
        public static string Write(WorldSim w, string summary, string folder)
        {
            var st = w.stats;
            var h = new StringBuilder();
            string from = Calendar(w, 0.0), to = w.Date.ToString();
            h.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>Kapman: the world ").Append(Esc(from)).Append(" to ").Append(Esc(to)).Append("</title>");
            h.Append("<style>body{font:14px/1.45 Georgia,'Times New Roman',serif;color:#1d1d1b;background:#f7f6f2;margin:0;padding:24px 32px;max-width:1180px}")
             .Append("h1{font-size:26px;margin:0 0 4px}h2{font-size:19px;margin:28px 0 6px;border-bottom:1px solid #000;padding-bottom:3px}")
             .Append("p.note{color:#555;margin:2px 0 10px}table{border-collapse:collapse;margin:6px 0}td,th{border:1px solid #bbb;padding:3px 8px;text-align:right}")
             .Append("th{background:#e9e7e0}td.l,th.l{text-align:left}.grid{display:flex;flex-wrap:wrap;gap:14px}.card{background:#fff;border:1px solid #000;padding:6px 8px}")
             .Append(".key span{display:inline-block;margin-right:12px;font-size:12px}.key i{display:inline-block;width:14px;height:3px;margin-right:4px;vertical-align:middle}")
             .Append("svg text{font:11px Georgia,serif;fill:#333}.warn{color:#b03a2e}.ok{color:#2e7d4f}pre{white-space:pre-wrap;background:#fff;border:1px solid #000;padding:8px}</style></head><body>");
            h.Append("<h1>The world, ").Append(Esc(from)).Append(" to ").Append(Esc(to)).Append("</h1>");
            h.Append("<p class=\"note\">Seed ").Append(w.seed).Append(". A test run of the world's ships with nobody watching (Kapman ▸ Sim). Money in deniers of silver.</p>");
            h.Append("<pre>").Append(Esc(summary)).Append("</pre>");

            Years(h, w, st);
            Prices(h, w, st);
            Gains(h, w);
            Raids(h, w, st);
            Passages(h, st);
            StockOuts(h, w, st);
            h.Append("<h2>Health</h2><ul>");
            foreach (var line in Health(w, out _, out _))
                h.Append("<li>").Append(line).Append("</li>");
            h.Append("</ul></body></html>");
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, "report.html");
            File.WriteAllText(path, h.ToString(), new UTF8Encoding(false));
            return path;
        }

        // ------------------------------------------------------------------ year by year

        static void Years(StringBuilder h, WorldSim w, SimStats st)
        {
            var years = st.voyages.Select(v => v.year).Concat(new[] { w.startYear, w.Date.year }).Distinct().OrderBy(y => y).ToList();
            h.Append("<h2>Year by year</h2><table><tr><th class=\"l\">Year</th><th>Voyages begun</th><th>Arrived</th><th>Lost at sea</th>")
             .Append("<th>Median gain a voyage</th><th>Places raided</th><th>Raids beaten off</th></tr>");
            foreach (int y in years)
            {
                var vs = st.voyages.Where(v => v.year == y).ToList();
                var gains = vs.Where(v => !double.IsNaN(v.arrived)).Select(v => v.revenue - v.cost).OrderBy(x => x).ToList();
                var raids = st.raids.Where(r => YearOf(w, r.day) == y).ToList();
                h.Append("<tr><td class=\"l\">").Append(y).Append("</td><td>").Append(vs.Count).Append("</td><td>").Append(vs.Count(v => !double.IsNaN(v.arrived)))
                 .Append("</td><td>").Append(vs.Count(v => v.lost)).Append("</td><td>").Append(gains.Count > 0 ? gains[gains.Count / 2].ToString("0", Inv) : "")
                 .Append("</td><td>").Append(raids.Count(r => r.done && !r.port.StartsWith("at sea"))).Append("</td><td>").Append(raids.Count(r => !r.done)).Append("</td></tr>");
            }
            h.Append("</table>");
        }

        // ------------------------------------------------------------------ prices

        /// <summary>The busiest ports: the most arrivals.</summary>
        static List<string> Busiest(SimStats st, int n) =>
            st.voyages.Where(v => !double.IsNaN(v.arrived)).GroupBy(v => v.to).OrderByDescending(g => g.Count()).Take(n).Select(g => g.Key).ToList();

        static void Prices(StringBuilder h, WorldSim w, SimStats st)
        {
            var ports = Busiest(st, Colours.Length);
            h.Append("<h2>Prices</h2><p class=\"note\">What the busiest ports pay for each watched good, sampled every ").Append(st.sampleEvery)
             .Append(" days (the most arrivals first).</p><div class=\"key\">");
            for (int i = 0; i < ports.Count; i++)
                h.Append("<span><i style=\"background:").Append(Colours[i]).Append("\"></i>").Append(Esc(PortName(ports[i]))).Append("</span>");
            h.Append("</div><div class=\"grid\">");
            const float W = 360f, H = 150f, L = 38f, B = 18f;
            foreach (string good in st.watch)
            {
                var lines = ports.Select(p => st.series.TryGetValue(p + "|" + good, out var s) ? s : null).ToList();
                if (lines.All(l => l == null || l.Count < 2))
                    continue;
                float minDay = lines.Where(l => l != null).Min(l => l[0].day), maxDay = lines.Where(l => l != null).Max(l => l[l.Count - 1].day);
                float top = Math.Max(0.5f, lines.Where(l => l != null).Max(l => l.Max(p => p.pays)) * 1.1f);
                h.Append("<div class=\"card\"><b>").Append(Esc(GameData.Good(good)?.name ?? good)).Append("</b><br><svg width=\"").Append(W).Append("\" height=\"").Append(H).Append("\">");
                Axes(h, L, B, W, H, top, minDay, maxDay, w);
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i] == null)
                        continue;
                    h.Append("<polyline fill=\"none\" stroke-width=\"1.4\" stroke=\"").Append(Colours[i]).Append("\" points=\"");
                    foreach (var (day, pays) in lines[i])
                        h.Append(F(L + (day - minDay) / Math.Max(1f, maxDay - minDay) * (W - L - 6f))).Append(',').Append(F(H - B - pays / top * (H - B - 8f))).Append(' ');
                    h.Append("\"/>");
                }
                h.Append("</svg></div>");
            }
            h.Append("</div>");
        }

        static void Axes(StringBuilder h, float L, float B, float W, float H, float top, float minDay, float maxDay, WorldSim w)
        {
            h.Append("<line x1=\"").Append(F(L)).Append("\" y1=\"").Append(F(H - B)).Append("\" x2=\"").Append(F(W - 4f)).Append("\" y2=\"").Append(F(H - B)).Append("\" stroke=\"#000\"/>");
            h.Append("<line x1=\"").Append(F(L)).Append("\" y1=\"4\" x2=\"").Append(F(L)).Append("\" y2=\"").Append(F(H - B)).Append("\" stroke=\"#000\"/>");
            for (int k = 0; k <= 2; k++)
            {
                float v = top * k / 2f, y = H - B - v / top * (H - B - 8f);
                h.Append("<text x=\"").Append(F(L - 4f)).Append("\" y=\"").Append(F(y + 4f)).Append("\" text-anchor=\"end\">").Append(v.ToString(v < 10f ? "0.#" : "0", Inv)).Append("</text>");
            }
            h.Append("<text x=\"").Append(F(L)).Append("\" y=\"").Append(F(H - 4f)).Append("\">").Append(Esc(Calendar(w, minDay))).Append("</text>");
            h.Append("<text x=\"").Append(F(W - 4f)).Append("\" y=\"").Append(F(H - 4f)).Append("\" text-anchor=\"end\">").Append(Esc(Calendar(w, maxDay))).Append("</text>");
        }

        // ------------------------------------------------------------------ gain by kind of ship

        static void Gains(StringBuilder h, WorldSim w)
        {
            var rules = TradeRules.Current;
            var byClass = w.ships.Where(s => !s.raider).GroupBy(s => s.cls.id)
                .Select(g =>
                {
                    var gains = g.Select(s => s.purse.Silver(rules) + s.hold.Lots.Sum(l => l.cost) - s.startWorth).OrderBy(x => x).ToList();
                    return (cls: g.First().cls.name ?? g.Key, ships: g.Count(), median: gains[gains.Count / 2], voyages: g.Average(s => (double)s.voyages));
                })
                .OrderByDescending(c => c.median).ToList();
            if (byClass.Count == 0)
                return;
            double most = Math.Max(1.0, byClass.Max(c => Math.Abs(c.median)));
            const float W = 900f, Row = 20f, Mid = 620f, Half = 250f;
            h.Append("<h2>Gain by kind of ship</h2><p class=\"note\">The median trader's gain over the run (silver and cargo at cost, less what she started with); her kind, how many, voyages each.</p>");
            h.Append("<svg width=\"").Append(W + 220f).Append("\" height=\"").Append(F(byClass.Count * Row + 10f)).Append("\">");
            for (int i = 0; i < byClass.Count; i++)
            {
                var c = byClass[i];
                float y = 4f + i * Row, len = (float)(Math.Abs(c.median) / most * Half), x = c.median >= 0 ? Mid : Mid - len;
                h.Append("<text x=\"").Append(F(Mid - Half - 8f)).Append("\" y=\"").Append(F(y + 13f)).Append("\" text-anchor=\"end\">").Append(Esc(Short(c.cls))).Append(" (").Append(c.ships).Append(")</text>");
                h.Append("<rect x=\"").Append(F(x)).Append("\" y=\"").Append(F(y + 2f)).Append("\" width=\"").Append(F(Math.Max(1f, len))).Append("\" height=\"").Append(F(Row - 6f))
                 .Append("\" fill=\"").Append(c.median >= 0 ? "#2e7d4f" : "#b03a2e").Append("\"/>");
                h.Append("<text x=\"").Append(F(Mid + Half + 8f)).Append("\" y=\"").Append(F(y + 13f)).Append("\">").Append(c.median.ToString("0", Inv)).Append(" d, ")
                 .Append(c.voyages.ToString("0.0", Inv)).Append(" voyages</text>");
            }
            h.Append("<line x1=\"").Append(F(Mid)).Append("\" y1=\"0\" x2=\"").Append(F(Mid)).Append("\" y2=\"").Append(F(byClass.Count * Row + 6f)).Append("\" stroke=\"#000\"/></svg>");
        }

        // ------------------------------------------------------------------ raids

        static void Raids(StringBuilder h, WorldSim w, SimStats st)
        {
            h.Append("<h2>Raids</h2>");
            if (st.raids.Count == 0)
            {
                h.Append("<p class=\"note\">No raids in this run.</p>");
                return;
            }
            double minDay = 0.0, maxDay = Math.Max(1.0, w.Day);
            const float W = 1100f, L = 10f, Rows = 4f, RowH = 16f;
            float H = 40f + Rows * RowH;
            h.Append("<p class=\"note\">Each raid on the run's timeline: filled, the place was struck; hollow, it was too strong for them. Ships taken at sea are left out.</p>");
            h.Append("<svg width=\"").Append(W).Append("\" height=\"").Append(F(H)).Append("\"><line x1=\"").Append(F(L)).Append("\" y1=\"20\" x2=\"").Append(F(W - 10f)).Append("\" y2=\"20\" stroke=\"#000\"/>");
            // the years
            for (int y = w.startYear; y <= w.Date.year; y++)
            {
                double d = DayOf(w, y);
                if (d < minDay || d > maxDay)
                    continue;
                float x = L + (float)((d - minDay) / (maxDay - minDay)) * (W - L - 20f);
                h.Append("<line x1=\"").Append(F(x)).Append("\" y1=\"14\" x2=\"").Append(F(x)).Append("\" y2=\"26\" stroke=\"#000\"/><text x=\"").Append(F(x + 2f)).Append("\" y=\"11\">").Append(y).Append("</text>");
            }
            int n = 0;
            foreach (var r in st.raids.Where(r => !r.port.StartsWith("at sea")).OrderBy(r => r.day))
            {
                float x = L + (float)((r.day - minDay) / (maxDay - minDay)) * (W - L - 20f);
                h.Append("<circle cx=\"").Append(F(x)).Append("\" cy=\"20\" r=\"4\" stroke=\"#b03a2e\" stroke-width=\"1.4\" fill=\"").Append(r.done ? "#b03a2e" : "#fff").Append("\"><title>")
                 .Append(Esc($"{r.date}: {r.band} at {PortName(r.port)}{(r.done ? "" : " (too strong)")}")).Append("</title></circle>");
                if (r.done)
                    h.Append("<text x=\"").Append(F(x)).Append("\" y=\"").Append(F(40f + (n++ % (int)Rows) * RowH)).Append("\" text-anchor=\"middle\">").Append(Esc(PortName(r.port))).Append("</text>");
            }
            h.Append("</svg><table><tr><th class=\"l\">Date</th><th class=\"l\">Band</th><th class=\"l\">Where</th><th class=\"l\">Struck</th><th>Men</th><th>Defence</th><th>Loot (units)</th><th>Captives</th></tr>");
            foreach (var r in st.raids.OrderBy(r => r.day))
                h.Append("<tr><td class=\"l\">").Append(Esc(r.date)).Append("</td><td class=\"l\">").Append(Esc(r.band)).Append("</td><td class=\"l\">").Append(Esc(PortName(r.port)))
                 .Append("</td><td class=\"l\">").Append(r.done ? "yes" : "no").Append("</td><td>").Append(r.men.ToString("0", Inv)).Append("</td><td>").Append(r.defence.ToString("0", Inv))
                 .Append("</td><td>").Append(r.loot).Append("</td><td>").Append(r.captives).Append("</td></tr>");
            h.Append("</table>");
        }

        // ------------------------------------------------------------------ the passages sailed

        static void Passages(StringBuilder h, SimStats st)
        {
            var legs = st.voyages.GroupBy(v => string.CompareOrdinal(v.from, v.to) < 0 ? v.from + "|" + v.to : v.to + "|" + v.from)
                                 .Select(g => (a: g.Key.Split('|')[0], b: g.Key.Split('|')[1], n: g.Count())).ToList();
            var calls = new Dictionary<string, int>();
            foreach (var v in st.voyages)
            {
                calls.TryGetValue(v.to, out int c);
                calls[v.to] = c + 1;
                if (!calls.ContainsKey(v.from))
                    calls[v.from] = 0;
            }
            var ports = calls.Keys.Select(GameData.Port).Where(p => p != null).ToList();
            if (ports.Count < 2)
                return;
            double lat0 = ports.Min(p => p.lat), lat1 = ports.Max(p => p.lat), lon0 = ports.Min(p => p.lon), lon1 = ports.Max(p => p.lon);
            double k = Math.Cos((lat0 + lat1) * 0.5 * Math.PI / 180.0);
            const float W = 1100f, Pad = 30f;
            double wide = Math.Max(0.01, (lon1 - lon0) * k), tall = Math.Max(0.01, lat1 - lat0);
            float scale = (float)((W - 2f * Pad) / wide), H = (float)(tall * scale) + 2f * Pad;
            if (H > 900f)
            {
                scale *= 900f / H;
                H = 900f;
            }
            float X(PortDef p) => Pad + (float)((p.lon - lon0) * k * scale);
            float Y(PortDef p) => H - Pad - (float)((p.lat - lat0) * scale);
            int most = Math.Max(1, legs.Count > 0 ? legs.Max(l => l.n) : 1);
            h.Append("<h2>The passages sailed</h2><p class=\"note\">Every passage begun, drawn port to port (straight, not along the lanes): the thicker, the more ships. ")
             .Append("Dots: ports, by how many ships came in. The 30 busiest are named.</p>");
            h.Append("<svg width=\"").Append(W).Append("\" height=\"").Append(F(H)).Append("\" style=\"background:#fff;border:1px solid #000\">");
            foreach (var (a, b, n) in legs.OrderBy(l => l.n))
            {
                var pa = GameData.Port(a);
                var pb = GameData.Port(b);
                if (pa == null || pb == null)
                    continue;
                h.Append("<line x1=\"").Append(F(X(pa))).Append("\" y1=\"").Append(F(Y(pa))).Append("\" x2=\"").Append(F(X(pb))).Append("\" y2=\"").Append(F(Y(pb)))
                 .Append("\" stroke=\"#1f5f8b\" stroke-opacity=\"0.45\" stroke-width=\"").Append(F(0.4f + 7f * n / most)).Append("\"><title>")
                 .Append(Esc($"{pa.name} - {pb.name}: {n}")).Append("</title></line>");
            }
            int maxCalls = Math.Max(1, calls.Values.Max());
            var named = new HashSet<string>(calls.OrderByDescending(c => c.Value).Take(30).Select(c => c.Key));
            foreach (var p in ports)
            {
                h.Append("<circle cx=\"").Append(F(X(p))).Append("\" cy=\"").Append(F(Y(p))).Append("\" r=\"").Append(F(1.5f + 5f * (float)Math.Sqrt(calls[p.id] / (double)maxCalls)))
                 .Append("\" fill=\"#1d1d1b\"><title>").Append(Esc($"{p.name}: {calls[p.id]} ships came in")).Append("</title></circle>");
                if (named.Contains(p.id))
                    h.Append("<text x=\"").Append(F(X(p) + 6f)).Append("\" y=\"").Append(F(Y(p) - 4f)).Append("\">").Append(Esc(p.name)).Append("</text>");
            }
            h.Append("</svg>");
        }

        // ------------------------------------------------------------------ stock-outs

        static void StockOuts(StringBuilder h, WorldSim w, SimStats st)
        {
            double days = Math.Max(1.0, w.Day);
            var top = st.stockOuts.OrderByDescending(kv => kv.Value).Take(25).ToList();
            h.Append("<h2>Stock-outs</h2>");
            if (top.Count == 0)
            {
                h.Append("<p class=\"note\">No market that makes a good ever ran out of it.</p>");
                return;
            }
            h.Append("<p class=\"note\">Markets that usually have a good and had none (the longest first): ").Append(st.stockOuts.Count)
             .Append(" goods at some port ran out at some time.</p><table><tr><th class=\"l\">Port</th><th class=\"l\">Good</th><th>Days out</th><th>Share of the run</th><th class=\"l\"></th></tr>");
            foreach (var kv in top)
            {
                var parts = kv.Key.Split('|');
                double share = kv.Value / days;
                h.Append("<tr><td class=\"l\">").Append(Esc(PortName(parts[0]))).Append("</td><td class=\"l\">").Append(Esc(GameData.Good(parts[1])?.name ?? parts[1]))
                 .Append("</td><td>").Append(kv.Value).Append("</td><td>").Append((share * 100.0).ToString("0", Inv)).Append("%</td><td class=\"l\"><svg width=\"160\" height=\"10\"><rect width=\"")
                 .Append(F((float)(160.0 * Math.Min(1.0, share)))).Append("\" height=\"10\" fill=\"#b9770e\"/></svg></td></tr>");
            }
            h.Append("</table>");
        }

        // ------------------------------------------------------------------ health

        /// <summary>The run's health, as lines: traders stuck (no voyage begun in the last year of the run, or ever in a
        /// shorter one, yet not laid up: in port, waiting or at sea all that time - a fault); traders laid up all that time
        /// for want of trade worth the voyage (the economy's numbers, for the tuning pass); and goods whose price moved more
        /// than twofold between the run's first and last 90 days.</summary>
        public static List<string> Health(WorldSim w, out int stuck, out int drifted)
        {
            var st = w.stats;
            var lines = new List<string>();
            double end = w.Day, window = Math.Min(365.0, end);
            var lastSailed = st.voyages.GroupBy(v => v.ship).ToDictionary(g => g.Key, g => g.Max(v => v.departed));
            var idle = w.ships.Where(s => !s.raider && s.state != ShipState.Lost
                                          && (!lastSailed.TryGetValue(s.id, out double d) || end - d > window)).ToList();
            var stuckShips = idle.Where(s => s.state != ShipState.LaidUp).ToList();
            var laidUp = idle.Where(s => s.state == ShipState.LaidUp).ToList();
            stuck = stuckShips.Count;
            string Ships(List<SimShip> list) =>
                Esc(string.Join(", ", list.Take(20).Select(s => $"{s.id} ({s.state} at {PortName(GameData.Ports[s.at].id)}, " +
                                                                 $"{s.purse.Silver(TradeRules.Current):0} d, {w.Reach(s).Count() - 1} ports in reach)"))) + (list.Count > 20 ? "..." : "");
            lines.Add(stuck == 0
                ? $"<span class=\"ok\">No trader is stuck: every one afloat sailed in the last {window:0} days, or lay up for want of trade.</span>"
                : $"<span class=\"warn\">{stuck} traders are stuck (no voyage begun in the last {window:0} days, and not laid up):</span> " + Ships(stuckShips));
            if (laidUp.Count > 0)
                lines.Add($"{laidUp.Count} traders lay up the last {window:0} days for want of trade worth the voyage (for the tuning pass: " +
                          $"their reach, their purse, their home market's prices): " + Ships(laidUp));
            drifted = 0;
            var drift = new List<string>();
            foreach (string good in st.watch)
            {
                var first = new List<float>();
                var last = new List<float>();
                foreach (var kv in st.series)
                {
                    if (!kv.Key.EndsWith("|" + good))
                        continue;
                    foreach (var (day, pays) in kv.Value)
                    {
                        if (day <= 90)
                            first.Add(pays);
                        if (day >= end - 90.0)
                            last.Add(pays);
                    }
                }
                if (first.Count == 0 || last.Count == 0)
                    continue;
                first.Sort();
                last.Sort();
                double ratio = last[last.Count / 2] / Math.Max(0.01, first[first.Count / 2]);
                if (ratio > 2.0 || ratio < 0.5)
                {
                    drifted++;
                    drift.Add($"{GameData.Good(good)?.name ?? good} x{ratio:0.00}");
                }
            }
            lines.Add(drifted == 0
                ? "<span class=\"ok\">Every watched good's median price stayed within twofold of where it began.</span>"
                : $"<span class=\"warn\">{drifted} goods' median price moved more than twofold:</span> " + Esc(string.Join(", ", drift)));
            int atSea = w.ships.Count(s => !s.raider && s.state == ShipState.Sailing), afloat = w.ships.Count(s => !s.raider && s.state != ShipState.Lost);
            lines.Add($"At the end: {atSea} of {afloat} traders afloat were at sea; {w.ships.Count(s => !s.raider && s.state == ShipState.Lost)} lost in all.");
            return lines;
        }

        // ------------------------------------------------------------------ helpers

        static string Calendar(WorldSim w, double day) => Kapman.World.Calendar.At(w.startYear, w.startDay, day * 24.0).ToString();
        static int YearOf(WorldSim w, double day) => Kapman.World.Calendar.At(w.startYear, w.startDay, day * 24.0).year;

        /// <summary>The run's day on which a year begins (1 January).</summary>
        static double DayOf(WorldSim w, int year)
        {
            double days = 0.0;
            for (int y = w.startYear; y < year; y++)
                days += y == w.startYear ? Kapman.World.Calendar.DaysIn(y) - (w.startDay - 1) : Kapman.World.Calendar.DaysIn(y);
            return days;
        }

        static string PortName(string id) => GameData.Port(id)?.name ?? id;
        static string Short(string name) => name.Contains(" (") ? name.Substring(0, name.IndexOf(" (", StringComparison.Ordinal)) : name;
        static string F(float v) => v.ToString("0.#", Inv);
        static string Esc(string s) => (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
    }
}
