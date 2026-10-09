using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// VOYAGE - where she can go. The ports she has FOUND (seen where they lie), each with the days to it for this boat
    /// as a low and a high figure (the course planner's way by water, her own speeds: FramePassage): picking one shows
    /// its course on the chart and what is known of the place; "Set this course" charts it. It sets, it does not sail:
    /// casting off is the function button's above the windows (at sea she takes a new course up at once). The ways are
    /// reckoned a few at a time, and again when she has moved or moored.
    ///
    /// CLICK AND SAIL (the user, 9 Oct 2026, by the picker): a place pointed at on the chart, in known water or in the
    /// dark, stands at the head of the window and is charted the same way (<see cref="PickPlace"/>). The ports only
    /// HEARD OF stand under "By hearsay" with a rough bearing and no way: on the chart each is a wide ring, and she is
    /// sent to look by pointing at the water in it.
    /// </summary>
    public sealed class VoyagePanel : FramePanel
    {
        public override string Id => "voyage";
        public override string Title => "Voyage";
        public override int Order => 10;

        sealed class Way
        {
            public bool reckoned, ok;
            public double km;
        }

        readonly Dictionary<string, Way> ways = new Dictionary<string, Way>();
        readonly List<WorldXZ> scratch = new List<WorldXZ>();
        readonly List<WorldXZ> pickedLine = new List<WorldXZ>();
        WorldXZ waysFrom;
        string waysLying = "?";
        string picked;
        int waysVersion;
        /// <summary>A place pointed at on the chart: where; whether water she can sail leads towards it; its km; and whether
        /// the whole way lies in waters she knows.</summary>
        WorldXZ? place;
        bool placeOk, placeKnown;
        double placeKm;
        /// <summary>A port only heard of, picked in the list: the chart shows its ring.</summary>
        string hearsay;

        /// <summary>Is a place pointed at picked; does its whole way lie in waters she knows; the port picked under "By
        /// hearsay" ("" none).</summary>
        public bool PlacePicked => place.HasValue;
        public bool PlaceKnown => place.HasValue && placeKnown;
        public string HearsayPicked => hearsay ?? "";

        /// <summary>The port picked in the list ("" none).</summary>
        public string Picked => picked ?? "";

        /// <summary>Picks a port (from the list, or a click on its mark on the chart): its course is shown on the chart.</summary>
        public void Pick(string portId)
        {
            place = null;
            hearsay = null;
            picked = GameData.Port(portId) != null && portId != FrameGame.PortHere?.id ? portId : null;
            ShowPicked();
            // the chart shows the whole of the way picked; with none picked it goes back to her
            if (picked != null && pickedLine.Count > 1)
                MapView.Current?.ShowBoth(pickedLine[0], pickedLine[pickedLine.Count - 1]);
            else
                MapView.Current?.Follow();
            Redraw();
        }

        void ShowPicked()
        {
            pickedLine.Clear();
            var frame = Frame != null ? Frame : FrameRoot.Current;
            if (picked == null || !Course.Plan(picked, ChartWorld.ShipWorld(), Port.LyingId(), pickedLine, out _))
            {
                pickedLine.Clear();
                frame?.ShowPlanned(null);
                return;
            }
            frame?.ShowPlanned(pickedLine);
        }

        /// <summary>
        /// CLICK AND SAIL: a place pointed at on the chart is picked as a port is. Its way is shown where the water is
        /// known, and "Set this course" charts it. Pointed at again (within a km and a half): the course is set at once.
        /// </summary>
        public void PickPlace(WorldXZ at)
        {
            if (place.HasValue && placeOk && WorldXZ.Distance(place.Value, at) < 1500.0)
            {
                SetCourse();
                return;
            }
            picked = null;
            hearsay = null;
            place = at;
            ShowPlace();
            Redraw();
        }

        void ShowPlace()
        {
            var frame = Frame != null ? Frame : FrameRoot.Current;
            if (!place.HasValue)
                return;
            placeOk = Course.PlanToPlace(place.Value, ChartWorld.ShipWorld(), Port.LyingId(), pickedLine, null, null, out placeKm);
            placeKnown = placeOk && MapView.AllKnown(pickedLine);
            if (!placeOk)
                pickedLine.Clear();
            frame?.ShowPlanned(placeOk ? pickedLine : null, place);
        }

        /// <summary>A port only heard of, picked under "By hearsay" (again: put down): the chart shows her and its ring.</summary>
        public void PickHearsay(string portId)
        {
            var def = GameData.Port(portId);
            picked = null;
            place = null;
            pickedLine.Clear();
            (Frame != null ? Frame : FrameRoot.Current)?.ShowPlanned(null);
            hearsay = def != null && hearsay != portId ? portId : null;
            if (hearsay != null)
                MapView.Current?.ShowBoth(ChartWorld.ShipWorld(), Hearsay.RingMiddle(def));
            else
                MapView.Current?.Follow();
            Redraw();
        }

        protected override void OnShown()
        {
            if (picked != null)
                ShowPicked();
            else if (place.HasValue)
                ShowPlace();
        }

        protected override void OnLoaded()
        {
            place = null;
            hearsay = null;
            picked = null;                                               // (picked in the game before)
            pickedLine.Clear();
            ways.Clear();
            waysLying = "?";
            waysVersion++;
        }

        protected override void OnHidden()
        {
            (Frame != null ? Frame : FrameRoot.Current)?.ShowPlanned(null);
        }

        /// <summary>The ports she has found (seen where they lie): those a course can be charted to by name.</summary>
        static List<PortDef> FoundPorts()
        {
            var list = new List<PortDef>();
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            string lying = FrameGame.PortHere?.id;                        // (the port she is moored at is not somewhere to go)
            foreach (var def in GameData.Ports)
                if (def.Exists(year) && def.id != lying && Hearsay.Found(def))
                    list.Add(def);
            return list;
        }

        /// <summary>The ports she has heard of and not found: rings on the chart, and no way to them by name.</summary>
        static List<PortDef> HeardPorts()
        {
            var list = new List<PortDef>();
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            foreach (var def in GameData.Ports)
                if (def.Exists(year) && Hearsay.Ringed(def))
                    list.Add(def);
            return list;
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);

        /// <summary>The ways are from where she was when they were reckoned: thrown away when she has moved 5 km or has
        /// moored or cast off.</summary>
        void Fresh()
        {
            var at = ChartWorld.ShipWorld();
            string lying = Port.LyingId() ?? "";
            if (lying == waysLying && WorldXZ.Distance(at, waysFrom) < 5000.0)
                return;
            waysLying = lying;
            waysFrom = at;
            ways.Clear();
            waysVersion++;
            if (picked != null && picked == lying)
                picked = null;
            if (picked != null)
                ShowPicked();
            else if (place.HasValue)
                ShowPlace();
        }

        /// <summary>Reckons the ways not reckoned yet, three a look (each is a search over the lanes).</summary>
        void Reckon()
        {
            int done = 0;
            bool more = false;
            foreach (var def in FoundPorts())
            {
                if (ways.TryGetValue(def.id, out var w) && w.reckoned)
                    continue;
                if (done >= 3)
                {
                    more = true;
                    break;
                }
                w = new Way { reckoned = true };
                w.ok = Course.Plan(def.id, ChartWorld.ShipWorld(), Port.LyingId(), scratch, out w.km);
                ways[def.id] = w;
                done++;
            }
            // (7 Oct 2026: the window is built again when all the ways are known, and once a second meanwhile. It was
            // built again at every look while it reckoned, four times a second for some seconds after each mooring and
            // every five km at sea, and a click that fell across a rebuild was lost.)
            if (done > 0)
                waysShown = false;
            if (!waysShown && (!more || UnityEngine.Time.realtimeSinceStartup >= nextWaysShow))
            {
                waysShown = !more;
                nextWaysShow = UnityEngine.Time.realtimeSinceStartup + 1f;
                waysVersion++;
            }
        }

        bool waysShown = true;
        float nextWaysShow;

        protected override int Stamp()
        {
            if (KapmanGame.Current == null || KapmanGame.Current.ship == null)
                return 0;
            Fresh();
            Reckon();
            int h = FrameUi.Mix(17, waysVersion);
            h = FrameUi.Mix(h, Course.Destination);
            h = FrameUi.Mix(h, picked);
            h = FrameUi.Mix(h, FoundPorts().Count);
            h = FrameUi.Mix(h, HeardPorts().Count);
            h = FrameUi.Mix(h, hearsay);
            h = FrameUi.Mix(h, place.HasValue ? 1 + (int)(place.Value.x * 0.01) * 31 + (int)(place.Value.z * 0.01) : 0);
            h = FrameUi.Mix(h, placeOk);
            h = FrameUi.Mix(h, placeKnown);
            h = FrameUi.Mix(h, FrameGame.PortHere != null);
            h = FrameUi.Mix(h, FirstJobs.Version);
            h = FrameUi.Mix(h, Course.IsSet ? (int)(Course.KmLeft(ChartWorld.ShipWorld()) / 5.0) : -1);
            // (the oars: the order, the hours left in the men to the hour, and whether it can be given)
            var cls = FramePassage.HerClass;
            h = FrameUi.Mix(h, Oars.Version);
            h = FrameUi.Mix(h, (int)System.Math.Ceiling(Oars.Left(cls)));
            h = FrameUi.Mix(h, ChartBody.Current != null && ChartBody.Current.Sheltering);
            // (her set off her line, to the km; and a reckoning lost)
            h = FrameUi.Mix(h, Adrift.Version);
            h = FrameUi.Mix(h, (int)(Adrift.OffLine / 1000.0));
            return h;
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null)
            {
                body.Add(FrameUi.P("No ship yet."));
                return;
            }
            Fresh();
            var here = FrameGame.PortHere;
            if (Course.IsSet)
            {
                double left = Course.KmLeft(ChartWorld.ShipWorld());
                body.Add(FrameUi.H("Bound for " + Course.BoundName));
                // (a place pointed at in the dark: how far it is by water is not known until she has sailed it)
                if (Course.ToPlace && !MapView.AllKnown(Course.Points))
                    body.Add(FrameUi.Ln("Into waters you do not know", here != null ? "The course is charted. Cast off when you will." : "She finds the way as she goes."));
                else
                    body.Add(FrameUi.Ln($"{left:0} km to go by water", here != null ? "The course is charted. Cast off when you will." : "",
                                        FrameUi.Num(FramePassage.DaysWords(left))));
                // THE SEA MOVES HER (9 Oct 2026; by the picker: "you correct it by pointing again"): how far wind and tide
                // have set her off her line, and her course laid again from where she is. PLACEHOLDER WORDS.
                Button again = null;
                if (here == null && Adrift.OffLine > OffLineShownM && (Adrift.Helm == Helm.Reckoning || Adrift.Helm == Helm.Coasting))
                {
                    string airt = SeaPush.Sets(Adrift.Set.x, Adrift.Set.z);
                    body.Add(FrameUi.Ln("Off her line", $"Wind and tide have set her {System.Math.Max(1.0, System.Math.Round(Adrift.OffLine / 1000.0)):0} km" +
                                                        (airt.Length > 0 ? " to the " + airt : "") + ". The steersman keeps to his reckoning."));
                    again = FrameUi.Btn("Lay the course again", () => LayAgain(), "A new course to where she is bound, from where she truly is.");
                }
                var giveUp = FrameUi.Btn("Give up the course", () =>
                {
                    Course.Clear();
                    FrameLog.Add(here != null ? "The course is given up." : "The course is given up: she lies to.");
                    Redraw();
                });
                // THE OARS (the alpha's row 3; the user's ruling 8 of 7 Oct 2026: "Oars (out oars, or sail)"): under way,
                // the order stands beside the course. Out, a line says so and what the men have left in them.
                var cls = FramePassage.HerClass;
                Button oars = null;
                if (here == null && cls != null && cls.oarsKn > 0f)
                {
                    bool lying = ChartBody.Current != null && ChartBody.Current.Sheltering;
                    if (Oars.Out)
                    {
                        // (one run of words, no figure at the right: the window is narrow)
                        body.Add(FrameUi.Ln("Under oars", (cls.hasSail ? "The sail is down and the men row hard: " : "The men row hard: ") +
                                                          Oars.Span(Oars.Left(cls)) + " left in them."));
                        oars = FrameUi.Btn("Ship oars", () => OrderOars(false), cls.hasSail ? "The oars come in and the sail goes up." : "The men ease to their own stroke.");
                    }
                    else
                    {
                        bool can = Oars.Can(cls, false, lying, out string why);
                        oars = FrameUi.Btn("Out oars", () => OrderOars(true),
                                           $"The sail comes down and the men row hard: {cls.oarsKn:0.#} knots whatever the wind, for {Oars.Span(Oars.Left(cls)).Replace("about ", "")}. It eats bread and strength.");
                        oars.SetEnabled(can);
                        if (!can)
                            body.Add(FrameUi.P(why));
                    }
                }
                body.Add(FrameUi.Row(giveUp, oars));
                if (again != null)
                    body.Add(FrameUi.Row(again));
            }
            else if (here == null && Adrift.LostBound.Length > 0)
            {
                // the steersman has lost his reckoning: she lies to until she is given a course from where she is
                string bound = Adrift.LostBound == Course.PlaceId ? Course.PlaceWords : GameData.Port(Adrift.LostBound)?.name ?? Adrift.LostBound;
                body.Add(FrameUi.H("The reckoning is lost"));
                body.Add(FrameUi.P("She has run her distance for " + bound + " and the steersman cannot make it out. She lies to. The chart shows where she is."));
                body.Add(FrameUi.Go("Lay the course again", () => LayAgain()));
            }
            // ---- the place pointed at on the chart (click and sail)
            if (place.HasValue)
            {
                var me = ChartWorld.ShipWorld();
                body.Add(FrameUi.H(Cap(Course.PlaceWords)));
                string where = "To the " + SeaWatch.Airt(me.x, me.z, place.Value.x, place.Value.z);
                if (!placeOk)
                    body.Add(FrameUi.P("No water she can sail leads that way."));
                else
                {
                    if (placeKnown)
                        body.Add(FrameUi.Ln(where, $"{placeKm:0} km by water", FrameUi.Num(FramePassage.DaysWords(placeKm))));
                    else
                        body.Add(FrameUi.Ln(where, "Through waters you do not know. She finds the way as she goes."));
                    if (Course.ToPlace && WorldXZ.Distance(Course.Place, place.Value) < 1.0)
                        body.Add(FrameUi.P(here != null ? "This is your course. Cast off when you will." : "This is your course."));
                    else
                        body.Add(FrameUi.Go("Set this course", () => SetCourse()));
                }
            }
            body.Add(FrameUi.H("Where to"));
            var ports = FoundPorts();
            ports.Sort((a, b) =>
            {
                double ka = ways.TryGetValue(a.id, out var wa) && wa.ok ? wa.km : double.MaxValue;
                double kb = ways.TryGetValue(b.id, out var wb) && wb.ok ? wb.km : double.MaxValue;
                return ka != kb ? ka.CompareTo(kb) : string.CompareOrdinal(a.name, b.name);
            });
            if (ports.Count == 0)
                body.Add(FrameUi.P("You have found no other port yet. Point at the chart to sail there and look."));
            foreach (var def in ports)
            {
                string id = def.id;
                ways.TryGetValue(id, out var w);
                string small = w == null || !w.reckoned ? "reckoning the way" : !w.ok ? "no way there by water that she can sail" : $"{w.km:0} km by water";
                var days = w != null && w.reckoned && w.ok ? FrameUi.Num(FramePassage.DaysWords(w.km)) : null;
                if (Course.IsSet && Course.Destination == id)
                {
                    // (where she is bound: the course she is on, not a new reckoning from where she happens to be)
                    double left = Course.KmLeft(ChartWorld.ShipWorld());
                    small = $"your course: {left:0} km to go";
                    days = FrameUi.Num(FramePassage.DaysWords(left));
                }
                // (Ravn's word of the first morning: the port he named is marked until she first casts off)
                // (in the small print: as a tag beside the days it squeezed them into a column)
                if (FirstJobs.Listed && FirstJobs.TipPort == id)
                    small += ". Ravn's word";
                var line = FrameUi.LnButton(def.name, small, () => Pick(picked == id ? null : id), days);
                line.EnableInClassList("sel", id == picked);
                body.Add(line);
                if (id == picked)
                    Details(body, def, w, here != null);
            }
            // ---- the ports only heard of: no way by name; a ring on the chart, and a rough word of where
            var heard = HeardPorts();
            if (heard.Count > 0)
            {
                var me = ChartWorld.ShipWorld();
                heard.Sort((a, b) =>
                {
                    double ka = WorldXZ.Distance(me, Hearsay.RingMiddle(a)), kb = WorldXZ.Distance(me, Hearsay.RingMiddle(b));
                    return ka != kb ? ka.CompareTo(kb) : string.CompareOrdinal(a.name, b.name);
                });
                body.Add(FrameUi.H("By hearsay"));
                foreach (var def in heard)
                {
                    string id = def.id;
                    var middle = Hearsay.RingMiddle(def);
                    // (by its ring, not by where it lies: a rough bearing and a rough time, as the crow flies and half as much again)
                    string small = "to the " + SeaWatch.Airt(me.x, me.z, middle.x, middle.z) + ", " + FramePassage.DaysWords(WorldXZ.Distance(me, middle) / 1000.0 * 1.5) + " off, they say";
                    if (FirstJobs.Listed && FirstJobs.TipPort == id)
                        small += ". Ravn's word";
                    var line = FrameUi.LnButton(def.name, small, () => PickHearsay(id), null);
                    line.EnableInClassList("sel", id == hearsay);
                    body.Add(line);
                    if (id == hearsay)
                        body.Add(FrameUi.P("Heard of, not found. It lies somewhere in its ring on the chart: point at the water there, and set that course."));
                }
            }
        }

        /// <summary>Under the picked port: the days again in a sentence, today's wind for it, the word on what is aboard
        /// there, and setting the course.</summary>
        void Details(VisualElement body, PortDef def, Way w, bool inPort)
        {
            bool bound = Course.IsSet && Course.Destination == def.id;
            if (w != null && w.reckoned && w.ok)
            {
                string wind = FramePassage.WindWords(FramePassage.WindNow(pickedLine));
                body.Add(FrameUi.P($"About {FramePassage.DaysWords(w.km)} for your boat, by the wind" + (wind.Length > 0 ? ": " + wind : "") + ". Its course is drawn on the chart."));
            }
            var ask = Known.AskNow();
            int words = 0;
            if (Trade.Started)
                foreach (var (good, _) in Trade.Hold.Contents)
                {
                    if (words++ >= 3)
                        break;
                    body.Add(new WordLine(Known.Word(good, def, ask)));
                }
            if (bound)
                body.Add(FrameUi.P(inPort ? "This is your course. Cast off when you will." : "This is your course."));
            else if (w != null && w.reckoned && w.ok)
                body.Add(FrameUi.Go("Set this course", () => SetCourse()));
        }

        /// <summary>The order Out oars (true) or Ship oars, by the window's button: given, and said in the log.</summary>
        public void OrderOars(bool on)
        {
            bool moored = FrameGame.PortHere != null;
            bool lying = ChartBody.Current != null && ChartBody.Current.Sheltering;
            string said = Oars.Order(on, FramePassage.HerClass, moored, lying);
            if (said.Length > 0)
                FrameLog.Add(said);
            Redraw();
        }

        /// <summary>Real metres off her line from which the window says so and offers to lay the course again.</summary>
        public const double OffLineShownM = 1000.0;

        /// <summary>
        /// "Lay the course again": a new course to where she is bound (or was, when the steersman lost his reckoning),
        /// from where she truly is. False when no water she can sail leads there from here.
        /// </summary>
        public bool LayAgain()
        {
            string bound = Course.IsSet ? Course.Destination : Adrift.LostBound;
            if (string.IsNullOrEmpty(bound))
                return false;
            bool set = Course.SetTo(bound, ChartWorld.ShipWorld(), Port.LyingId());
            FrameLog.Add(set ? $"The course is laid again from where she is: {Course.Km:0} km to go for {Course.BoundName}."
                             : "No water she can sail leads there from here.");
            Redraw();
            return set;
        }

        /// <summary>Has the way to a port been reckoned (and is there one)?</summary>
        public bool Reckoned(string portId) => ways.TryGetValue(portId ?? "", out var w) && w.reckoned && w.ok;

        /// <summary>Charts the course to the picked port (the "Set this course" button). In port she stays moored until she is
        /// cast off; at sea she takes it up at once. False when no port is picked or the water does not reach it.</summary>
        public bool SetCourse()
        {
            bool inPort = FrameGame.PortHere != null;
            if (place.HasValue)
            {
                // a place pointed at on the chart
                bool there = Course.SetToPlace(place.Value, ChartWorld.ShipWorld(), Port.LyingId());
                if (there)
                {
                    FrameLog.Add((MapView.AllKnown(Course.Points)
                                      ? $"Course set for {Course.PlaceWords}: {Course.Km:0} km by water, about {FramePassage.DaysWords(Course.Km)}."
                                      : $"Course set for {Course.PlaceWords}, through waters you do not know.") + (inPort ? " Cast off when you will." : ""));
                    (Frame != null ? Frame : FrameRoot.Current)?.ShowPlanned(null);
                    MapView.Current?.Follow();
                    place = null;
                    pickedLine.Clear();
                }
                else
                    FrameLog.Add("No water she can sail leads that way.");
                Redraw();
                return there;
            }
            var def = GameData.Port(picked);
            if (def == null)
                return false;
            bool set = Course.SetTo(def.id, ChartWorld.ShipWorld(), Port.LyingId());
            if (set)
            {
                FrameLog.Add($"Course set for {def.name}: {Course.Km:0} km by water, about {FramePassage.DaysWords(Course.Km)}." +
                             (inPort ? " Cast off when you will." : ""));
                (Frame != null ? Frame : FrameRoot.Current)?.ShowPlanned(null);
                MapView.Current?.Follow();
                picked = null;
            }
            else
                FrameLog.Add("No way there by water that she can sail.");
            Redraw();
            return set;
        }
    }
}
