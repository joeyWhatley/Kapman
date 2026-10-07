using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// WAITING, by the Wait button: the clock runs at its own pace (1x, 2x, 4x; nothing is skipped) until first light, or
    /// until the wind is fair for her course. At sea she lies to meanwhile and takes her course up again by herself; in
    /// port she stays moored and the clock, which a port holds, is let run (PanelClock asks <see cref="On"/>).
    /// </summary>
    public static class FrameWait
    {
        public enum For { Nothing, FirstLight, FairWind }

        public static For Until { get; private set; }
        public static bool On => Until != For.Nothing;
        /// <summary>The course she lay to on, to take up again ("" none).</summary>
        static string resume = "";
        static readonly List<WorldXZ> line = new List<WorldXZ>();

        public static void Begin(For what)
        {
            if (what == For.Nothing)
                return;
            Until = what;
            resume = "";
            if (FrameGame.PortHere == null && Course.IsSet)
            {
                // at sea: she lies to, and takes this course up again when the wait is over
                resume = Course.Destination;
                Course.Clear();
            }
            FrameLog.Add(what == For.FirstLight ? "You wait for first light." : "You wait for a fair wind.");
        }

        /// <summary>Stops waiting. `say`: what is said of it (null: nothing).</summary>
        public static void End(string say)
        {
            if (!On)
                return;
            Until = For.Nothing;
            if (resume.Length > 0 && FrameGame.PortHere == null && !Course.IsSet)
                Course.SetTo(resume, ChartWorld.ShipWorld(), null);
            resume = "";
            if (!string.IsNullOrEmpty(say))
                FrameLog.Add(say);
        }

        /// <summary>What she waited for has come: the wait ends, and the clock drops back (the player set it fast to
        /// wait, and would not want the days to go on running in port).</summary>
        static void Over(string say)
        {
            End(say);
            ClockDrops.Raise(ClockReason.WaitOver);
        }

        /// <summary>Is it dark enough to wait for the light (the Sun well under the horizon)?</summary>
        public static bool Dark => GameSpeed.Stepped ? SimStage.SunElevationAt(ChartWorld.ShipWorld()) < -6.0 : LevelClock.SunAltitude < -6f;

        /// <summary>The port her course (set, or laid by while she waits) is for; null with none.</summary>
        public static string Bound => Course.IsSet ? Course.Destination : resume.Length > 0 ? resume : null;

        /// <summary>Is the wind fair for the course she is bound on now (not foul, no gale)?</summary>
        public static bool WindFair()
        {
            string to = Bound;
            if (to == null || !Course.Plan(to, ChartWorld.ShipWorld(), Port.LyingId(), line, out _))
                return false;
            var wind = FramePassage.WindNow(line);
            return wind == FramePassage.Wind.Fair || wind == FramePassage.Wind.Working;
        }

        static float nextLook;

        /// <summary>Each frame (the frame): the wait is over when what it waits for has come.</summary>
        public static void Tick()
        {
            if (GameSpeed.Stepped)
                return;                                                   // (the game's clock steps the wait: Step)
            if (!On || Time.realtimeSinceStartup < nextLook)
                return;
            nextLook = Time.realtimeSinceStartup + 0.5f;
            if (Until == For.FirstLight && !Dark)
                Over("First light.");
            else if (Until == For.FairWind && WindFair())
                Over("The wind has come fair.");
        }

        /// <summary>One step of the game's clock (the game minute it ended on): first light is looked for every minute,
        /// a fair wind every tenth (it plans her course to ask).</summary>
        public static void Step(long minute)
        {
            if (!On)
                return;
            if (Until == For.FirstLight)
            {
                if (!Dark)
                    Over("First light.");
            }
            else if (Until == For.FairWind && minute % 10 == 0 && WindFair())
                Over("The wind has come fair.");
        }

        /// <summary>The wait for the saved game: what she waits for, and the course laid by to take up again.</summary>
        public static Dictionary<string, object> Capture() =>
            new Dictionary<string, object> { ["until"] = Until.ToString(), ["resume"] = resume };

        /// <summary>The wait of a loaded game (null, an older save: none). Before, a wait of the game that was running
        /// went on under the loaded one: the clock ran in port, and she took up a course that was not hers.</summary>
        public static void Restore(object saved)
        {
            Until = For.Nothing;
            resume = "";
            nextLook = 0f;
            if (saved == null)
                return;
            if (Enum.TryParse(Json.Str(saved, "until"), out For what))
                Until = what;
            resume = Until != For.Nothing ? Json.Str(saved, "resume") : "";
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Until = For.Nothing;
            resume = "";
            nextLook = 0f;
        }
    }

    /// <summary>
    /// THE EIGHT FUNCTION BUTTONS (the user's ruling of 7 Oct 2026, in Design/wiring_plan.md: "The panels must be
    /// separate function from the 8 buttons above them"). A rack of four by two between the top bar and the windows.
    /// Each DOES one thing: they are verbs, and they never open, close or switch a window. A press acts at once and writes
    /// a line in the log, or opens a small floating window with the choices; one that cannot be used now is greyed, with
    /// the reason on hover.
    ///   CAST OFF (in port, with a course set; at sea it is LIE TO, and SAIL ON when she lies to by it) · WAIT (till first
    ///   light, till a fair wind: FrameWait) · MEASURE (takes the measure up or puts it down) · LOOK OUT (what is in sight
    ///   from the masthead: the lookout's own sightings, the wind, the sky) · SOUND (the lead: the water on this stretch of
    ///   her course, from the lanes' depths) · HAIL (the nearest sail the lookout has made out: she may heave to and
    ///   speak; Encounter.Hail) · REST, OFFER: their rules are not in the game yet, and each says so in the log rather
    ///   than pretend.
    /// </summary>
    public partial class FrameRoot
    {
        sealed class Verb
        {
            public Button button;
            public Label label;
            public FrameIcon.Kind kind;
            /// <summary>"" always there; "port" only while she lies at a port; "sea" only while she does not.</summary>
            public string group = "";
        }

        readonly List<Verb> rack = new List<Verb>();
        string lieTo = "";                                             // the course she lies to on, by the Lie to button
        float nextRack;
        int seaLogSeen = -1;

        partial void BuildRack(VisualElement column)
        {
            rack.Clear();
            // THREE ROWS OF FOUR (the user's ruling of 7 Oct 2026, late, by the picker, after "what even is happening up
            // there"): the ship (Cast off, Voyage, Crew, Hold); where she is (in port: Trade, Alehouse, Minster or
            // Grove, Rest; at sea that row is Look out, Sound, Hail, Rest); what is known (Wares, Journey, Errands,
            // Book). A button that names a window brings that window up; one that names a place goes into it
            // (FramePlace); the orders of the sea act at once, as before. Measure went down to the chart's own tools.
            // THE BUTTONS STAND IN THE LAYOUT FILE (Frame.uxml, under "rack"): each does what the sign in it says (its
            // FrameIcon's kind), and the class at-port or at-sea shows it only there. So a button can be moved, taken
            // out or added in UI Builder with no change here.
            var box = Part<VisualElement>("rack");
            box.Query<Button>(className: "frame-verb").ForEach(b =>
            {
                var icon = b.Q<FrameIcon>();
                if (icon == null)
                {
                    Debug.LogError("Kapman frame: the button \"" + b.name + "\" of the rack (Frame.uxml) has no FrameIcon in it: its sign says what it does.");
                    return;
                }
                var k = icon.Sign;
                var label = b.Q<Label>(className: "frame-verb-name");
                if (label == null)
                {
                    label = new Label { pickingMode = PickingMode.Ignore };
                    label.AddToClassList("frame-verb-name");
                    b.Add(label);
                }
                b.clicked += () => Do(k);
                string group = b.ClassListContains("at-port") ? "port" : b.ClassListContains("at-sea") ? "sea" : "";
                rack.Add(new Verb { button = b, label = label, kind = k, group = group });
            });
            nextRack = 0f;
            PanelClock.Waiting = () => FrameWait.On;                      // (waiting runs the clock though she lies in port)
            VoyagePlanner.Moored = () => FrameGame.PortHere != null;      // (a course set in port waits for the Cast off button)
        }

        partial void RackUpdate()
        {
            FrameWait.Tick();
            FramePlace.Tick(this);
            if (Time.realtimeSinceStartup < nextRack)
                return;
            nextRack = Time.realtimeSinceStartup + 0.25f;
            LookoutLog();
            if (FrameGame.PortHere != null || Course.IsSet)
                lieTo = "";                                               // (moored, or under way again: the lying to is over)
            bool seaNow = FrameGame.PortHere == null;
            foreach (var v in rack)
            {
                // (the middle row is the port's or the sea's)
                v.button.style.display = v.group.Length == 0 || (v.group == "sea") == seaNow ? DisplayStyle.Flex : DisplayStyle.None;
                Look(v.kind, out string name, out string why, out bool can, out bool on);
                v.label.text = name;
                v.button.tooltip = why;
                // (a button that cannot be used now is greyed, not switched off: it still says why, on hover and when pressed)
                v.button.EnableInClassList("off", !can);
                v.button.EnableInClassList("on", on);
            }
        }

        /// <summary>A function button as things stand now: what it is called, what it does (or why it cannot), whether it
        /// can be pressed, and whether it shows as switched on.</summary>
        public void Look(FrameIcon.Kind kind, out string name, out string why, out bool can, out bool on)
        {
            can = true;
            on = false;
            why = "";
            bool atSea = FrameGame.PortHere == null;
            switch (kind)
            {
                case FrameIcon.Kind.CastOff:
                    if (!atSea)
                    {
                        name = "Cast off";
                        if (FirstJobs.Blocks(out string notYet)) { can = false; why = notYet; }
                        else if (!Course.IsSet) { can = false; why = "No course is set. Pick a port in the Voyage window and set its course first."; }
                        else if (!FrameGame.PortMenuOpen) { can = false; why = "Finish what is in front of you first."; }
                        else why = "Cast off for " + (GameData.Port(Course.Destination)?.name ?? "sea") + ".";
                    }
                    else if (Course.IsSet)
                    {
                        name = "Lie to";
                        why = "She stops and lies to. The clock runs on.";
                    }
                    else if (lieTo.Length > 0)
                    {
                        name = "Sail on";
                        on = true;
                        why = "Take up the course for " + (GameData.Port(lieTo)?.name ?? lieTo) + " again.";
                    }
                    else
                    {
                        name = "Lie to";
                        can = false;
                        why = "She lies to already, for want of a course. Set one in the Voyage window.";
                    }
                    break;
                case FrameIcon.Kind.Wait:
                    on = FrameWait.On;
                    name = on ? "Stop waiting" : "Wait";
                    why = on ? (FrameWait.Until == FrameWait.For.FirstLight ? "Waiting for first light." : "Waiting for a fair wind.") + " Press to stop."
                             : "Wait for the light, or for a fair wind. The clock runs while you wait, and bread is eaten.";
                    break;
                case FrameIcon.Kind.Measure:
                    name = "Measure";
                    on = Measuring;
                    why = "The way between two places on the chart, in days: click two places.  [D]";
                    break;
                case FrameIcon.Kind.LookOut:
                    name = "Look out";
                    why = "What can be seen from the masthead now.";
                    break;
                case FrameIcon.Kind.Sound:
                    name = "Sound";
                    if (!Course.IsSet) { can = false; why = "The lead is read along a charted course, and none is set."; }
                    else why = "Heave the lead: the water on this stretch of her course.";
                    break;
                case FrameIcon.Kind.Hail:
                    name = "Hail";
                    if (!atSea) { can = false; why = "Hailing is for the sea. In port, speak with the people in the People here window."; }
                    else if (Encounter.Stage != EncounterStage.None) { can = false; why = "You have a ship to answer already."; }
                    else
                    {
                        var sail = SailToHail();
                        if (sail == null) { can = false; why = "No sail is near enough to hail."; }
                        else why = "Hail " + SeaWatch.Label(sail) + ": she may heave to and speak with you.";
                    }
                    break;
                case FrameIcon.Kind.Voyage:
                    Window("voyage", "Voyage", "Where she can go from here, and her course.", out name, out why, out on);
                    break;
                case FrameIcon.Kind.Crew:
                    Window("crew", "Crew", "The men aboard: how they are, and what they cost.", out name, out why, out on);
                    break;
                case FrameIcon.Kind.Hold:
                    Window("hold", "Hold", "What she carries, and the room left.", out name, out why, out on);
                    break;
                case FrameIcon.Kind.Trade:
                    Window("trade", "Trade", "Buy and sell at this market.", out name, out why, out on);
                    break;
                case FrameIcon.Kind.Wares:
                    Window("wares", "Wares", "All you know of every ware: where it is wanted, where it is sold.", out name, out why, out on);
                    break;
                case FrameIcon.Kind.Journey:
                    Window("journey", "Journey", "What you have done and what you have learned.", out name, out why, out on);
                    break;
                case FrameIcon.Kind.Errands:
                    Window("tasks", "Errands", "What you have undertaken, and what is still to do.", out name, out why, out on);
                    break;
                case FrameIcon.Kind.Book:
                    Window("book", "Book", "The book of what you know: the rules, and the world as you learn it.", out name, out why, out on);
                    break;
                case FrameIcon.Kind.Alehouse:
                {
                    var ale = FramePlace.SiteHere(PlaceKind.Alehouse);
                    name = "Alehouse";
                    on = FramePlace.At == PlaceKind.Alehouse;
                    if (ale == null) { can = false; why = "No house here sells ale to strangers."; }
                    else why = ale.name + ": shipmen for hire, the other masters, the talk.";
                    break;
                }
                case FrameIcon.Kind.Rest:
                    on = FramePlace.At == PlaceKind.Rest || FrameWait.On;
                    name = FrameWait.On ? "Stop waiting" : "Rest";
                    why = FrameWait.On ? (FrameWait.Until == FrameWait.For.FirstLight ? "Waiting for first light." : "Waiting for a fair wind.") + " Press to stop."
                                       : atSea ? "A watch below: talk with the men, sleep till first light, wait for a wind."
                                               : "The fire on the bank: talk with the men, sleep till first light, wait for a wind.";
                    break;
                default:
                {
                    var holy = FramePlace.SiteHere(PlaceKind.Holy);
                    name = FramePlace.HolyWord(holy);
                    on = FramePlace.At == PlaceKind.Holy;
                    if (holy == null) { can = false; why = "No holy place is written of here."; }
                    else why = holy.name + ".";
                    break;
                }
            }
        }

        /// <summary>A button that names a window: lit while that window has the focus.</summary>
        void Window(string id, string called, string does, out string name, out string why, out bool on)
        {
            name = called;
            why = does;
            on = Windows != null && Windows.Focused == id;
        }

        /// <summary>A function button pressed. One that cannot be used now says why in the log, and does nothing.</summary>
        public void Do(FrameIcon.Kind kind)
        {
            Look(kind, out string called, out string reason, out bool can, out _);
            nextRack = 0f;
            if (!can)
            {
                FrameLog.Add(reason);
                return;
            }
            var here = FrameGame.PortHere;
            switch (kind)
            {
                case FrameIcon.Kind.CastOff:
                    if (here != null)
                    {
                        string to = GameData.Port(Course.Destination)?.name;
                        if (Course.IsSet && FrameGame.CastOff())
                            FrameLog.Add($"You cast off from {here.name}" + (to != null ? " for " + to : "") + ".");
                    }
                    else if (Course.IsSet)
                    {
                        lieTo = Course.Destination;
                        if (FrameWait.On)
                            FrameWait.End(null);
                        Course.Clear();
                        FrameLog.Add("She lies to. The clock runs on.");
                    }
                    else if (lieTo.Length > 0)
                    {
                        string to = lieTo;
                        lieTo = "";
                        if (Course.SetTo(to, ChartWorld.ShipWorld(), null))
                            FrameLog.Add("She sails on for " + (GameData.Port(to)?.name ?? to) + ".");
                    }
                    break;
                case FrameIcon.Kind.Wait:
                    if (FrameWait.On)
                        FrameWait.End("You stop waiting.");
                    else
                        WaitFloat();
                    break;
                case FrameIcon.Kind.Measure:
                    SetMeasuring(!Measuring);
                    FrameLog.Add(Measuring ? "The measure is in your hand: click two places on the chart." : "You put the measure down.");
                    break;
                case FrameIcon.Kind.LookOut:
                    LookOutFloat();
                    break;
                case FrameIcon.Kind.Sound:
                    FrameLog.Add(Sounding());
                    break;
                case FrameIcon.Kind.Hail:
                {
                    var sail = SailToHail();
                    if (sail == null)
                        break;
                    string who = SeaWatch.Label(sail);
                    FrameLog.Add(Encounter.Hail(sail) ? "You hail " + who + "." : "You hail " + who + ". She does not answer.");
                    break;
                }
                case FrameIcon.Kind.Voyage:
                    Windows.Show("voyage");
                    break;
                case FrameIcon.Kind.Crew:
                    Windows.Show("crew");
                    break;
                case FrameIcon.Kind.Hold:
                    Windows.Show("hold");
                    break;
                case FrameIcon.Kind.Trade:
                    Windows.Show("trade");
                    break;
                case FrameIcon.Kind.Wares:
                    Windows.Show("wares");
                    break;
                case FrameIcon.Kind.Journey:
                    Windows.Show("journey");
                    break;
                case FrameIcon.Kind.Errands:
                    Windows.Show("tasks");
                    break;
                case FrameIcon.Kind.Book:
                    Windows.Show("book");
                    break;
                case FrameIcon.Kind.Alehouse:
                    if (FramePlace.At == PlaceKind.Alehouse) FramePlace.Leave(this); else FramePlace.Enter(this, PlaceKind.Alehouse);
                    break;
                case FrameIcon.Kind.Rest:
                    if (FrameWait.On)
                        FrameWait.End("You stop waiting.");
                    else if (FramePlace.At == PlaceKind.Rest)
                        FramePlace.Leave(this);
                    else
                        FramePlace.Enter(this, PlaceKind.Rest);
                    break;
                case FrameIcon.Kind.Offer:
                    if (FramePlace.At == PlaceKind.Holy) FramePlace.Leave(this); else FramePlace.Enter(this, PlaceKind.Holy);
                    break;
            }
            nextRack = 0f;
        }

        // ------------------------------------------------------------------ Hail

        /// <summary>The sail to hail: the nearest trader the lookout has made out that could be met now (not a raider, under
        /// way, nothing else on, she herself clear of the ports); null with none.</summary>
        public static SimShip SailToHail()
        {
            var game = KapmanGame.Current;
            var watch = SeaWatch.Current;
            var world = game != null ? game.World : null;
            if (watch == null || world == null)
                return null;
            var me = ChartWorld.ShipWorld();
            SimShip best = null;
            double bestM = double.MaxValue;
            foreach (var s in world.ships)
            {
                if (s.raider || watch.Of(s) != SeaWatch.Seen.MadeOut)
                    continue;
                double m = WorldXZ.Distance(me, s.pos);
                if (m < bestM && Encounter.CanOffer(s))
                {
                    bestM = m;
                    best = s;
                }
            }
            return best;
        }

        // ------------------------------------------------------------------ Wait

        void WaitFloat()
        {
            const string id = "wait";
            var spec = new FloatSpec { id = id, title = "Wait" };
            spec.lines.Add(("For what? The clock runs while you wait, and bread is eaten.", true));
            string bound = FrameWait.Bound;
            string to = bound != null ? GameData.Port(bound)?.name ?? bound : null;
            spec.options.Add(new FloatOption
            {
                label = "Till first light", enabled = FrameWait.Dark, rule = "It is light already.",
                pick = () => { CloseFloat(id); FrameWait.Begin(FrameWait.For.FirstLight); },
            });
            spec.options.Add(new FloatOption
            {
                label = to != null ? "Till the wind is fair for " + to : "Till the wind is fair",
                enabled = to != null && !FrameWait.WindFair(),
                rule = to == null ? "No course is set: fair for where?" : "It is fair enough now.",
                pick = () => { CloseFloat(id); FrameWait.Begin(FrameWait.For.FairWind); },
            });
            spec.options.Add(new FloatOption { label = "Never mind", pick = () => CloseFloat(id) });
            spec.small = "A tide she must wait for before the flats on her course is waited for by itself.";
            OpenFloat(spec);
        }

        // ------------------------------------------------------------------ Look out

        void LookOutFloat()
        {
            const string id = "lookout";
            var spec = new FloatSpec { id = id, title = "The lookout" };
            var game = KapmanGame.Current;
            var me = ChartWorld.ShipWorld();
            string first = "";
            // the air and the light
            if (ChartWorld.TryWeather(out var air))
            {
                string sky = air.fog > 0.6f ? "Thick fog." : air.fog > 0.3f ? "Mist." : air.rain > 0.5f ? "Rain." : air.storm > 0.5f ? "Thunder cloud." :
                             air.layered > 0.7f ? "Grey overcast." : air.cumulus + air.layered > 0.6f ? "Broken cloud." : "A clear sky.";
                string light = LevelClock.Night > 0.9f ? " Night." : LevelClock.Night > 0.1f ? " Half light." : "";
                first = Cap(FrameGame.WindWords(air.windSpeed, air.windFrom)) + ". " + sky + light;
                spec.lines.Add((first, true));
            }
            // the sails in sight, nearest first
            var seen = new List<(double km, string text)>();
            var watch = SeaWatch.Current;
            var world = game != null ? game.World : null;
            if (watch != null && world != null)
                foreach (var s in world.ships)
                {
                    var what = watch.Of(s);
                    if (what == SeaWatch.Seen.None)
                        continue;
                    double km = WorldXZ.Distance(me, s.pos) / 1000.0;
                    string airt = SeaWatch.Airt(me.x, me.z, s.pos.x, s.pos.z);
                    string far = km < 1.5 ? "close by" : $"{km:0} km off";
                    seen.Add((km, what == SeaWatch.Seen.MadeOut
                        ? $"{Cap(SeaWatch.Label(s))}, to the {airt}, {far}" + (s.raider ? ": raiders." : s.state == ShipState.Anchored ? ", at anchor." : ".")
                        : $"A sail to the {airt}, {far}: too far to make out."));
                }
            seen.Sort((a, b) => a.km.CompareTo(b.km));
            if (seen.Count == 0)
                spec.lines.Add(("No sail in sight.", true));
            for (int i = 0; i < seen.Count && i < 6; i++)
                spec.lines.Add((seen[i].text, true));
            if (seen.Count > 6)
                spec.lines.Add(($"And {seen.Count - 6} more, farther off.", true));
            // the nearest port she knows
            PortDef near = null;
            double nearKm = double.MaxValue;
            int year = game != null ? game.Year : 793;
            foreach (var def in GameData.Ports)
            {
                if (!def.Exists(year))
                    continue;
                var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
                if (Exploration.Seen(x, z) <= 0.35f)
                    continue;
                double km = WorldXZ.Distance(me, new WorldXZ(x, z)) / 1000.0;
                if (km < nearKm)
                {
                    nearKm = km;
                    near = def;
                }
            }
            if (near != null && nearKm < 60.0)
            {
                var (x, z) = WorldFrame.ToWorld(near.lat, near.lon);
                spec.lines.Add((nearKm < 2.5 ? $"{near.name} lies under her bows." : $"{near.name} lies {nearKm:0} km to the {SeaWatch.Airt(me.x, me.z, x, z)}.", true));
            }
            spec.options.Add(new FloatOption { label = "Go on", isDefault = true, pick = () => CloseFloat(id) });
            OpenFloat(spec);
            FrameLog.Add((seen.Count == 0 ? "The lookout sees no sail." : seen.Count == 1 ? "The lookout has one sail in sight." : "The lookout has " + seen.Count + " sails in sight.") +
                         (first.Length > 0 ? " " + first : ""));
        }

        /// <summary>What the game's own lookout writes in the sea log (a sail sighted, made out, lost) goes in the frame's
        /// log too: one line as it is, more at once as a count.</summary>
        void LookoutLog()
        {
            int n = SeaWatch.Log.Count;
            if (seaLogSeen < 0 || n < seaLogSeen)
            {
                seaLogSeen = n;                                           // (what was written before the frame came up is not told again)
                return;
            }
            // (and nothing while she lies in a port: the comings and goings of the harbour she is in are no news)
            if (FrameGame.PortHere != null)
            {
                seaLogSeen = n;
                return;
            }
            // (a ship made out is a notice of the game's own already, and so in the log: only the rest is told here)
            int fresh = 0;
            string one = "";
            for (int i = seaLogSeen; i < n; i++)
                if (!SeaWatch.Log[i].text.StartsWith(SeaWatch.MadeOutStart, StringComparison.Ordinal))
                {
                    fresh++;
                    one = SeaWatch.Log[i].text;
                }
            seaLogSeen = n;
            if (fresh == 1 && one.Length > 0)
                FrameLog.Add(one);
            else if (fresh > 1)
                FrameLog.Add($"The lookout has {fresh} things to tell you. Press Look out to hear them.");
        }

        // ------------------------------------------------------------------ Sound

        /// <summary>The lead, as far as the game knows the water: the shallowest of the lane the leg she is on runs over
        /// (the lanes' own depths, real metres under mean sea level), said in fathoms.</summary>
        static string Sounding()
        {
            if (!Course.IsSet || Course.Points.Count < 2)
                return "The lead: no course is charted to sound along.";
            int leg = Mathf.Min(Course.Next, Course.Points.Count - 1);
            if (Course.LegIsRiver(leg))
                return "The lead: the river's own channel, deep enough for her.";
            float water = Course.LegWater(leg);
            if (float.IsInfinity(water))
                return "The lead finds deep water: nothing to fear on this stretch.";
            if (water < 0.5f)
                return "The lead: this stretch dries at low water. She must cross it on the flood.";
            double fathoms = water / 1.83;
            string depth = fathoms < 1.25 ? "barely a fathom" : fathoms < 1.75 ? "a fathom and a half" : $"{Math.Round(fathoms):0} fathoms";
            return $"The lead: {depth} at the shallowest of this stretch." + (water < Course.DeepWaterM ? " Little enough under her." : "");
        }
    }
}
