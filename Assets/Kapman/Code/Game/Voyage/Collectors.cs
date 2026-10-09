using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>One sending of the lender's men (Game/Data/collectors_793.json).</summary>
    public sealed class Sending
    {
        /// <summary>The game day (from the game's first day) they set out, how many men, and how many days they stay out.</summary>
        public double day, daysOut = 40.0;
        public int men = 2;
    }

    /// <summary>A way round them: which of her stats carries it (its odds before that stat's say are the card's own).</summary>
    public sealed class Trick
    {
        public string id = "", stat = "nerve";
    }

    /// <summary>The lender's men, as a table (Game/Data/collectors_793.json; every number a STARTING GUESS TO TUNE).</summary>
    public sealed class CollectorRules
    {
        public bool on = true;
        /// <summary>The port they are sent from and go home to.</summary>
        public string home = "dorestad";
        /// <summary>A sending is not made if she has paid this much on the stick since the last one (the lender has had
        /// something of his own lately).</summary>
        public double paidLatelyD = 240.0;
        /// <summary>Their speed between ports, and when they have her in sight (knots).</summary>
        public double speedKn = 3.0, chaseKn = 3.8;
        /// <summary>Hours they ask round a quay before they go on; days they wait there when nobody knows where she went.</summary>
        public double askHours = 6.0, waitDays = 3.0;
        /// <summary>How far they see her at sea by day and by night (km); how far off she is lost again; how near is alongside (m).</summary>
        public double sightKm = 6.0, sightNightKm = 1.5, loseKm = 12.0, alongsideM = 300.0;
        /// <summary>How far off she sees their boat and knows it (km).</summary>
        public double seenKm = 10.0;
        /// <summary>What they come for each time (deniers), and how much more of it for each step of the lender's temper over 2.</summary>
        public double instalmentD = 240.0, moreByTemper = 0.5;
        /// <summary>What cargo is counted at on the stick when it is given or taken (a share of its value), and the share
        /// of what they take by force that they keep for their trouble.</summary>
        public double cargoPrice = 0.6, troubleShare = 0.25;
        /// <summary>The lender's temper: where it starts, the most it gets; up to which they only talk; from which a man
        /// is beaten, and from which a man is killed, when they are given little or made fools of.</summary>
        public double temperStart = 1.0, temperMost = 6.0, warnUpTo = 1.0, beatFrom = 3.0, killFrom = 4.0;
        /// <summary>The chance a man is killed one step of temper short of that, when they got nothing and were lied to.</summary>
        public double killEarly = 0.3;
        /// <summary>Drunk they sleep this many hours; shaken off at sea they are blind to her this many hours.</summary>
        public double sleepHours = 30.0, blindHours = 12.0;
        /// <summary>A way round them is this much harder (on the stat's own scale) for each time it has fooled them before
        /// ("he told us to look under the stones this time"), and for each sending after the first.</summary>
        public double toldPenalty = 5.0, laterPenalty = 1.0;
        /// <summary>THE THIRD TIME SHE DOES NOT PAY THEY KILL RAVN (the user, 9 Oct 2026: "Maybe it you don't pay up for
        /// the 3rd time they kill Ravn?"): the count of visits that ended with nothing paid of her own will at which
        /// they do it when next she gives them nothing (0: never), and who he is in the standing cast.</summary>
        public double ravnAtRefusal = 3.0;
        public string ravnCast = "cast.crew.ravn";
        public readonly List<Sending> sendings = new List<Sending>();
        public readonly Dictionary<string, Trick> tricks = new Dictionary<string, Trick>(StringComparer.Ordinal);

        public static CollectorRules Read(Dictionary<string, object> o)
        {
            var r = new CollectorRules();
            if (o != null)
            {
                foreach (var f in typeof(CollectorRules).GetFields())
                {
                    if (f.IsStatic)
                        continue;
                    if (f.FieldType == typeof(double))
                        f.SetValue(r, Json.Num(o, f.Name, (double)f.GetValue(r)));
                    else if (f.FieldType == typeof(bool))
                        f.SetValue(r, Json.Bool(o, f.Name, (bool)f.GetValue(r)));
                    else if (f.FieldType == typeof(string))
                        f.SetValue(r, Json.Str(o, f.Name, (string)f.GetValue(r)));
                }
                foreach (object s in Json.Arr(o, "sendings") ?? new List<object>())
                    r.sendings.Add(new Sending { day = Json.Num(s, "day"), men = (int)Json.Num(s, "men", 2), daysOut = Json.Num(s, "days_out", 40.0) });
                foreach (var kv in Json.Obj(o, "tricks") ?? new Dictionary<string, object>())
                    r.tricks[kv.Key] = new Trick { id = kv.Key, stat = Json.Str(kv.Value, "stat", "nerve") };
            }
            r.sendings.Sort((a, b) => a.day.CompareTo(b.day));
            return r;
        }
    }

    /// <summary>Where the lender's men are in a sending.</summary>
    public enum GoonState
    {
        /// <summary>Nobody is out.</summary>
        Home,
        /// <summary>On their way to the port they last had word of her at.</summary>
        Sailing,
        /// <summary>At a quay, asking after her.</summary>
        Asking,
        /// <summary>They have her in sight at sea and are making for her.</summary>
        Chasing,
        /// <summary>They have found her made fast: they stand on the quay by her berth.</summary>
        Quay,
        /// <summary>They are alongside her at sea.</summary>
        Alongside,
        /// <summary>Drunk under her thwarts, or on the quay.</summary>
        Sleeping,
        /// <summary>Sent the wrong way, to a port she is not at.</summary>
        Astray,
        /// <summary>Going home to the lender, with what they got or with nothing.</summary>
        Homing,
    }

    /// <summary>What the lender's men did when they were given nothing, or made fools of.</summary>
    public struct Teeth
    {
        /// <summary>They only talked.</summary>
        public bool warned;
        /// <summary>Deniers of silver they mean to take, and of cargo (by its value) if the silver falls short.</summary>
        public double want;
        public bool beat, kill;
        /// <summary>It is Ravn they kill.</summary>
        public bool ravn;
    }

    /// <summary>
    /// THE LENDER SENDS HIS MEN TO COLLECT (the user, 9 Oct 2026: "I want the debt mechanic to be more of a constant cat
    /// and mouse game, you CAN just play the game and try and pay the debt asap", "Or you could idk try different methods
    /// and comedic ways around it"; by the picker: "Sends his goons to collect", "A handful of times", "Ports and at
    /// sea", "Fools first, sharper later"; and of what they do to her: "Dependant", then "It'll be dynamic you know",
    /// "Maybe you can pay them off in installments, maybe you can give them cargo maybe they kill a crew member ect",
    /// "It'll be comedic but also shockingly brutal at times", "Just clueless brutes").
    ///
    /// A HANDFUL OF TIMES in the life of the debt (Game/Data/collectors_793.json: four sendings) the lender sends a
    /// boat of his men out from home, unless she has paid a pound on the stick since the last time. THEY FOLLOW THE
    /// WORD OF HER: they sail to the port she last made, ask round the quay, and go on to the next she made after it,
    /// or to where she said she was bound; if she lingers they find her at her berth; if they see her at sea in
    /// daylight they row for her. They give up when their days are out.
    /// CAUGHT, a card opens (the quay's, or alongside at sea). She may pay a pound, or more, or all of it; give cargo
    /// off the hold at their price; or try a way round them (hide the silver, send them the wrong way, drink them
    /// under the thwarts, plead, run). A way round them is carried by one of her stats and is harder each time it has
    /// fooled them before, and for each later sending.
    /// GIVEN NOTHING, WHAT THEY DO DEPENDS (<see cref="Bite"/>): on the lender's temper, on what there is aboard to
    /// take, and on whether she has just made fools of them. They talk; or turn out the purse and the hold; or beat a
    /// man; or kill one. His temper falls when she pays and rises when they come home with nothing or fooled.
    /// The day itself (the lender's own cards) is as it was. Plain state, saved whole; nothing here is random but one
    /// seeded throw for a killing a step early.
    /// </summary>
    public static class Collectors
    {
        public const string File = "collectors_793.json";
        public const string QuayCard = "harbour.goons.quay", SeaCard = "sea.goons.alongside";
        /// <summary>The flag of a game that has the lender in it (the opening sets it).</summary>
        public const string OpeningFlag = Trade.LenderFlag;

        static readonly ContentCache<CollectorRules> cache =
            new ContentCache<CollectorRules>("the lender's men", () => CollectorRules.Read(ContentFiles.Exists(File) ? ContentFiles.ParseObject(File) : null));
        static CollectorRules own;

        public static CollectorRules Rules => own ?? cache.Value;

        /// <summary>Puts a table in the game's place (a test); null gives the game's back.</summary>
        public static void Use(CollectorRules rules) => own = rules;

        // ------------------------------------------------------------------ what is so (saved whole)

        public static GoonState State { get; private set; } = GoonState.Home;
        /// <summary>The sendings made or let pass so far, and which of them is out now (1 the first; 0 none).</summary>
        public static int Sent { get; private set; }
        public static int Visit { get; private set; }
        /// <summary>The lender's temper, 0 easy to <see cref="CollectorRules.temperMost"/>.</summary>
        public static double Temper { get; private set; } = 1.0;
        /// <summary>Where their boat is (real metres), the port they are making for or lie at, and the port they lie at ("" at sea).</summary>
        public static WorldXZ At { get; private set; }
        public static string Target { get; private set; } = "";
        public static string InPort { get; private set; } = "";
        /// <summary>She has met them: she knows their boat when she sees it.</summary>
        public static bool Met { get; private set; }
        /// <summary>How many times she has made fools of them, all told.</summary>
        public static int Fooled { get; private set; }
        /// <summary>The visits that ended with nothing paid of her own will, less one for each time she has paid since.</summary>
        public static int Unpaid { get; private set; }
        /// <summary>Is Ravn dead (the flag the cards' engine keeps for one of the standing cast who is gone)?</summary>
        public static bool RavnGone => ContentFlags.Ready && ContentFlags.Has(EncounterState.GoneFlag(Rules.ravnCast));
        /// <summary>Which way their boat heads, degrees.</summary>
        public static double Heading { get; private set; }
        /// <summary>She has their boat in sight now.</summary>
        public static bool InSight { get; private set; }
        /// <summary>She knows a boat of them is out after her: a quay has said so, or she has seen them.</summary>
        public static bool KnownOut => Out && known;
        /// <summary>The lender's temper in a word. PLACEHOLDER WORDS, the user's to write.</summary>
        public static string TemperWord => Temper <= 1.0 ? "patient" : Temper <= 2.0 ? "short with you" : Temper <= 3.0 ? "angry" : "past patience";
        /// <summary>Is it Ravn the next time she gives them nothing (and has she met them, to know it)?</summary>
        public static bool RavnNext => Met && Var("goons.ravn_next") > 0.0;
        /// <summary>Counts up when what a window or the chart shows of them has changed.</summary>
        public static int Version { get; private set; }

        static readonly Dictionary<string, int> used = new Dictionary<string, int>(StringComparer.Ordinal);
        static readonly List<WorldXZ> line = new List<WorldXZ>();
        /// <summary>Where they have asked after her in this sending: the port, the day, and where they went on to.</summary>
        static readonly List<(string port, double day, string on)> asked = new List<(string, double, string)>();
        static readonly HashSet<string> toldAt = new HashSet<string>(StringComparer.Ordinal);
        static int next;
        static double outSince, until, debtMark = double.NaN, blindUntil, unseenHours;
        static bool offered, sightTold, sendingTold, known;
        static string lastPort = "", bound = "";

        /// <summary>How many times a way round them has fooled them.</summary>
        public static int Used(string trick) => trick != null && used.TryGetValue(trick, out int n) ? n : 0;

        /// <summary>The men in the boat that is out (0: none out).</summary>
        public static int Men => Visit >= 1 && Visit <= Rules.sendings.Count ? Rules.sendings[Visit - 1].men : 0;

        public static bool Out => State != GoonState.Home;
        public static bool Caught => State == GoonState.Quay || State == GoonState.Alongside;

        // ------------------------------------------------------------------ the reckoning (no game needed)

        /// <summary>
        /// HOW EASY A WAY ROUND THEM IS, on her stat's own scale (a card's chance takes it as its "by": the odds are the
        /// trick's own chance and a twentieth for each point of this over 3). Her stat as it stands; less for each
        /// time that way has fooled them before; less for each sending after the first.
        /// </summary>
        public static double Ease(CollectorRules r, double stat, int usedBefore, int visit) =>
            GameRules.TrickEase(stat, usedBefore, visit, r.toldPenalty, r.laterPenalty);   // (the graph "Rule - a way round the lender's men")

        /// <summary>
        /// WHAT THEY DO WHEN THEY ARE GIVEN NOTHING: it depends. `temper`: the lender's. `fooled`: she has just tried a
        /// way round them and it failed. `aboard`: the silver and the cargo (by its value) there is to take. `men`:
        /// her crew. `throwOf`: a throw of 0 to 1 for the one thing left to chance.
        /// Up to <see cref="CollectorRules.warnUpTo"/> they only talk. Over that they mean to take an instalment (more
        /// as his temper climbs), silver first and cargo for the rest. When there is little to take, or she has just
        /// made fools of them, a man is beaten from <see cref="CollectorRules.beatFrom"/> and killed from
        /// <see cref="CollectorRules.killFrom"/>; a step short of that, with nothing aboard and a lie just told, one
        /// time in three. Never the last man aboard.
        /// </summary>
        public static Teeth Bite(CollectorRules r, double temper, bool fooled, double aboard, int men, double throwOf, int unpaid = 0, bool ravnAlive = false)
        {
            // THE TWO RULES ARE GRAPHS (9 Oct 2026; the user's ruling of 7 Oct: "Rules as graphs too"): what they come for
            // ("Rule - what the lender's men come for") and what they do ("Rule - what the lender's men do": 0 they only
            // talk, 1 they take, 2 they take and beat a man, 3 they take and kill a man, 4 they take and kill Ravn).
            // GameRules keeps the same sums in code beside them: the fallback, and what the test holds the graphs to.
            double want = GameRules.Instalment(temper + (fooled ? 1.0 : 0.0), r.instalmentD, r.moreByTemper);
            int does = (int)Math.Round(GameRules.GoonsDo(temper, fooled, aboard, men, throwOf, unpaid, ravnAlive,
                                                         r.warnUpTo, r.beatFrom, r.killFrom, r.killEarly, r.ravnAtRefusal, want));
            var bite = new Teeth();
            if (does <= 0)
            {
                bite.warned = true;
                return bite;
            }
            bite.want = want;
            bite.beat = does == 2;
            bite.kill = does == 3;
            bite.ravn = does >= 4;
            return bite;
        }

        /// <summary>Is a sending due: the day has come for the next of them, and nobody is out.</summary>
        public static bool Due(CollectorRules r, double day, int sent, bool anyOut) =>
            !anyOut && sent < r.sendings.Count && day >= r.sendings[sent].day;

        // ------------------------------------------------------------------ the live game

        static bool Owing => Trade.Started && Trade.Debt > 0.01 && ContentFlags.Ready && ContentFlags.Has(OpeningFlag);

        static WorldXZ PortPlace(string id)
        {
            var def = GameData.Port(id);
            if (def == null)
                return default;
            var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
            return new WorldXZ(x, z);
        }

        static string PortName(string id) => GameData.Port(id)?.name ?? id;

        static bool Night(WorldXZ here)
        {
            var (lat, lon) = WorldFrame.ToLatLon(here.x, here.z);
            return Kapman.Game.ShipView.StageSky.At(SimStage.JulianDay, lat, lon).sunAltitude < Rest.DarkBelow;
        }

        /// <summary>The port the waterfronts last had her at: where she lies, or the last she made.</summary>
        static string LastWord()
        {
            string lying = Port.LyingId();
            if (!string.IsNullOrEmpty(lying))
                return lying;
            return lastPort;
        }

        /// <summary>One game minute of them (KapmanGame.StepMinute). `hours`: the game hours of the step.</summary>
        public static void Step(double hours)
        {
            var game = KapmanGame.Current;
            var r = Rules;
            if (game == null || game.ship == null || !r.on)
                return;
            double day = game.Day, now = game.Hours;
            var her = ChartWorld.ShipWorld();
            string lying = Port.LyingId() ?? "";
            // her own trail: the port she last made, and where she said she was bound when she left it
            if (lying.Length > 0)
            {
                lastPort = lying;
                bound = "";
            }
            else if (bound.Length == 0 && Course.IsSet && !Course.ToPlace)
                bound = Course.Destination ?? "";
            if (!Owing)
            {
                // nothing owed (or a game with no lender): nobody is out, and nobody is sent
                if (State != GoonState.Home)
                    Recall();
                return;
            }
            if (double.IsNaN(debtMark))
                debtMark = Trade.Debt;
            // a sending falls due
            if (Due(r, day, Sent, Out))
            {
                Sent++;
                bool paid = debtMark - Trade.Debt >= r.paidLatelyD - 0.5;
                debtMark = Trade.Debt;
                if (paid)
                    Cool(1.0);                                           // (he has had something of his own: he sends nobody)
                else
                    SetOut(day);
            }
            PortTalk(lying, day);
            if (State == GoonState.Home)
            {
                InSight = false;
                return;
            }
            // their days are out: home with nothing
            if (State != GoonState.Homing && !Caught && Visit >= 1 && Visit <= r.sendings.Count && day - outSince > r.sendings[Visit - 1].daysOut)
            {
                Heat(1.0);
                GoHome();
            }
            switch (State)
            {
                case GoonState.Sailing:
                case GoonState.Astray:
                case GoonState.Homing:
                    if (State == GoonState.Sailing && Sees(r, her, lying, now))
                    {
                        State = GoonState.Chasing;
                        unseenHours = 0.0;
                        Version++;
                        break;
                    }
                    if (Along(r.speedKn, hours))
                        Arrived(lying, day, now);
                    break;
                case GoonState.Asking:
                    if (lying.Length > 0 && lying == InPort)
                        Catch(GoonState.Quay);
                    else if (now >= until)
                        AskedRound(day, now);
                    break;
                case GoonState.Chasing:
                    Chase(r, her, lying, hours, now);
                    break;
                case GoonState.Quay:
                    if (lying != InPort && !CardUp())
                        Follow(now);                                      // (she slipped her lines before they had their say)
                    else
                        Offer();
                    break;
                case GoonState.Alongside:
                    At = her;
                    if (lying.Length > 0 && !CardUp())
                    {
                        InPort = lying;
                        State = GoonState.Quay;
                        offered = false;
                    }
                    Offer();
                    break;
                case GoonState.Sleeping:
                    if (now >= until)
                    {
                        // they wake, and she is where she is: at the same quay still, or gone
                        if (InPort.Length > 0 && lying == InPort)
                            Catch(GoonState.Quay);
                        else
                        {
                            State = GoonState.Asking;
                            until = now + r.askHours;
                            Version++;
                        }
                    }
                    break;
            }
            Sighting(r, her, lying);
        }

        static void SetOut(double day)
        {
            var r = Rules;
            Visit = Sent;
            outSince = day;
            asked.Clear();
            toldAt.Clear();
            offered = sightTold = sendingTold = known = false;
            blindUntil = 0.0;
            InPort = r.home;
            At = PortPlace(r.home);
            Target = "";
            State = GoonState.Asking;
            until = (KapmanGame.Current != null ? KapmanGame.Current.Hours : 0.0) + 1.0;      // (they ask at home first: an hour)
            Version++;
        }

        /// <summary>A course for their boat to a port, from where it is. False: no water leads there.</summary>
        static bool Lay(string port)
        {
            line.Clear();
            next = 1;
            if (string.IsNullOrEmpty(port) || GameData.Port(port) == null)
                return false;
            bool ok = Course.Plan(port, At, InPort.Length > 0 ? InPort : null, line, out _);
            if (!ok || line.Count < 2)
            {
                line.Clear();
                return false;
            }
            Target = port;
            InPort = "";
            return true;
        }

        /// <summary>They move along their course at so many knots; true when they have come to its end.</summary>
        static bool Along(double knots, double hours)
        {
            if (line.Count < 2)
                return true;
            double go = knots * SimUnits.KnotMs * hours * 3600.0;
            while (go > 0.0 && next < line.Count)
            {
                double d = WorldXZ.Distance(At, line[next]);
                if (d <= go)
                {
                    At = line[next];
                    go -= d;
                    next++;
                }
                else
                {
                    Heading = (Math.Atan2(line[next].x - At.x, line[next].z - At.z) * 180.0 / Math.PI + 360.0) % 360.0;
                    At = new WorldXZ(At.x + (line[next].x - At.x) * go / d, At.z + (line[next].z - At.z) * go / d);
                    go = 0.0;
                }
            }
            return next >= line.Count;
        }

        static void Arrived(string lying, double day, double now)
        {
            var r = Rules;
            InPort = Target;
            line.Clear();
            if (State == GoonState.Homing)
            {
                State = GoonState.Home;
                Visit = 0;
                InPort = "";
                Version++;
                return;
            }
            if (State == GoonState.Astray)
            {
                // nobody there has seen her: they know they were sent the wrong way, and go home to say so
                asked.Add((InPort, day, r.home));
                Heat(1.0);
                GoHome();
                return;
            }
            if (lying.Length > 0 && lying == InPort)
            {
                Catch(GoonState.Quay);
                return;
            }
            State = GoonState.Asking;
            until = now + r.askHours;
            Version++;
        }

        /// <summary>They have asked round the quay: on to where the word of her leads, or they wait, or go home.</summary>
        static void AskedRound(double day, double now)
        {
            var r = Rules;
            string word = LastWord(), on = "";
            if (word.Length > 0 && word != InPort)
                on = word;                                               // (she has made another port since: there)
            else if (bound.Length > 0 && bound != InPort && Port.LyingId() == null)
                on = bound;                                              // (she left here saying where she was bound)
            string here = InPort;
            if (on.Length > 0 && Lay(on))
            {
                asked.Add((here, day, on));                              // (what that quay will tell her when she comes back to it)
                State = GoonState.Sailing;
                Version++;
                return;
            }
            // nobody knows: they wait a few days for word of her, then go home
            if (now - until < r.waitDays * 24.0)
                return;
            Heat(1.0);
            GoHome();
        }

        /// <summary>Their card is taken out of the line (they are gone before it could open: she has killed one of
        /// them, or the fight went against her and they did what they do: Voyage/Feud.cs).</summary>
        static void Unqueue()
        {
            var cards = EncounterRunner.Director;
            if (cards != null && cards.state != null)
                cards.state.queue.RemoveAll(id => id == QuayCard || id == SeaCard);
        }

        static void GoHome()
        {
            var r = Rules;
            offered = false;
            Unqueue();
            if (State == GoonState.Home || Visit < 1)
            {
                // (nobody is out: their card was answered all the same, put in line by a test or left from a game before)
                State = GoonState.Home;
                Visit = 0;
                InPort = "";
                line.Clear();
                return;
            }
            if (InPort == r.home)
            {
                State = GoonState.Home;
                Visit = 0;
                InPort = "";
                Version++;
                return;
            }
            if (InPort.Length > 0 && KapmanGame.Current != null)
                asked.Add((InPort, KapmanGame.Current.Day, r.home));
            if (!Lay(r.home))
            {
                State = GoonState.Home;                                  // (no water home from here: they are simply home)
                Visit = 0;
                InPort = "";
                Version++;
                return;
            }
            State = GoonState.Homing;
            Version++;
        }

        /// <summary>The sending is called off (the stick is clean, or a game with no lender): nobody is out.</summary>
        static void Recall()
        {
            Unqueue();
            State = GoonState.Home;
            Visit = 0;
            InPort = "";
            InSight = false;
            line.Clear();
            offered = false;
            Version++;
        }

        static bool Sees(CollectorRules r, WorldXZ her, string lying, double now)
        {
            if (lying.Length > 0 || now < blindUntil || ChartWorld.OnRiver())
                return false;
            bool thick = ChartWorld.TryWeather(out var air) && air.fog > 0.5f;
            double sight = (Night(At) ? r.sightNightKm : r.sightKm) * 1000.0 * (thick ? 0.25 : 1.0);
            return WorldXZ.Distance(At, her) <= sight && Course.OpenWater(At, her);
        }

        static void Chase(CollectorRules r, WorldXZ her, string lying, double hours, double now)
        {
            double d = WorldXZ.Distance(At, her);
            if (lying.Length > 0)
            {
                // she has run into a port: they come in after her
                if (Lay(lying))
                    State = GoonState.Sailing;
                else
                    Follow(now);
                Version++;
                return;
            }
            if (d <= r.alongsideM)
            {
                Catch(GoonState.Alongside);
                return;
            }
            if (Sees(r, her, lying, now))
                unseenHours = 0.0;
            else
                unseenHours += hours;
            if (d > r.loseKm * 1000.0 || unseenHours >= 1.0)
            {
                Follow(now);                                              // (lost her: back to asking where she goes)
                return;
            }
            double go = Math.Min(d, r.chaseKn * SimUnits.KnotMs * hours * 3600.0);
            var to = new WorldXZ(At.x + (her.x - At.x) * go / Math.Max(1.0, d), At.z + (her.z - At.z) * go / Math.Max(1.0, d));
            if (Course.KnowsWater && !Course.IsWater(to.x, to.z))
            {
                Follow(now);                                              // (the land lies between: they cannot come at her)
                return;
            }
            Heading = (Math.Atan2(her.x - At.x, her.z - At.z) * 180.0 / Math.PI + 360.0) % 360.0;
            At = to;
        }

        /// <summary>She is gone from them: they make for the port the word of her leads to, or go home if there is none.</summary>
        static void Follow(double now)
        {
            offered = false;
            string word = LastWord();
            if (word.Length == 0)
                word = bound;
            if (word.Length > 0 && Lay(word))
            {
                State = GoonState.Sailing;
                Version++;
                return;
            }
            GoHome();
        }

        static void Catch(GoonState how)
        {
            State = how;
            offered = false;
            Met = true;
            known = true;
            line.Clear();
            Version++;
            Offer();
        }

        static bool CardUp()
        {
            var cards = EncounterRunner.Director;
            return cards != null && cards.Current != null && (cards.Current.card.id == QuayCard || cards.Current.card.id == SeaCard);
        }

        /// <summary>Their card is put first in line (asked again each minute until it can come: another card may be up).</summary>
        static void Offer()
        {
            var game = KapmanGame.Current;
            double now = game != null ? game.Hours : 0.0;
            // (put in line and never come: the line was cleared under it. Asked again after three hours)
            if (offered && !CardUp() && now - offeredAt > 3.0)
                offered = false;
            if (offered)
                return;
            var cards = EncounterRunner.Director;
            if (cards == null)
                return;
            if (cards.Offer(State == GoonState.Alongside ? SeaCard : QuayCard))
            {
                offered = true;
                offeredAt = now;
                ClockDrops.Raise(ClockReason.Goons);
            }
        }

        static double offeredAt;

        // PLACEHOLDER WORDS, every one: the user's to write
        static void Sighting(CollectorRules r, WorldXZ her, string lying)
        {
            bool was = InSight;
            InSight = State != GoonState.Home && State != GoonState.Quay && State != GoonState.Sleeping && InPort.Length == 0 && lying.Length == 0
                      && WorldXZ.Distance(At, her) <= r.seenKm * 1000.0 * (Night(her) ? 0.3 : 1.0);
            if (InSight && !was && !sightTold && State != GoonState.Homing && State != GoonState.Astray)
            {
                sightTold = true;
                known = true;
                string airt = SeaWatch.Airt(her.x, her.z, At.x, At.z);
                Voyage.Say(Met ? $"There is a boat to the {airt}, rowing hard after you, and it is the lender's men." : $"There is a boat to the {airt}, rowing hard on your own course.");
                ClockDrops.Raise(ClockReason.Goons);
            }
            if (!InSight && was)
                sightTold = false;
            if (InSight != was)
                Version++;
        }

        /// <summary>What the quay tells her when she makes fast: that they are out after her, and where they have asked.</summary>
        static void PortTalk(string lying, double day)
        {
            if (lying.Length == 0 || State == GoonState.Home || toldAt.Contains(lying))
                return;
            if (State == GoonState.Quay && InPort == lying)
                return;                                                   // (they are here themselves: they will say it)
            foreach (var a in asked)
                if (a.port == lying)
                {
                    toldAt.Add(lying);
                    known = true;
                    double since = Math.Max(0.0, Math.Round(day - a.day));
                    Voyage.Say($"The quay says {Men} men with a rope were here asking for you{(since < 1.0 ? " today" : since < 2.0 ? " yesterday" : $", {since:0} days since")}." +
                               (a.on.Length > 0 && a.on != lying ? $" They went on to {PortName(a.on)}." : ""));
                    return;
                }
            if (!sendingTold && day - outSince >= 3.0)
            {
                sendingTold = true;
                known = true;
                toldAt.Add(lying);
                Voyage.Say("The word on the quay is that the lender has sent men out after you.");
            }
        }

        static void Heat(double by)
        {
            Temper = Math.Max(0.0, Math.Min(Rules.temperMost, Temper + by));
            Version++;
        }

        static void Cool(double by) => Heat(-by);

        /// <summary>The lender is past patience (she came for him in his own house, and was thrown out: Voyage/Feud.cs).</summary>
        public static void Rage()
        {
            Temper = Rules.temperMost;
            Version++;
        }

        // ------------------------------------------------------------------ what a card can ask of them

        /// <summary>A number for a card's conditions and odds: goons.caught, goons.sea, goons.visit, goons.men,
        /// goons.temper, goons.used_&lt;trick&gt;, goons.ease_&lt;trick&gt;, goons.drink (is there a cask of drink aboard),
        /// goons.cargo (can cargo be given for an instalment), goons.fooled, goons.unpaid, goons.ravn_next (the next
        /// time she gives them nothing, it is Ravn).</summary>
        public static double Var(string name)
        {
            var r = Rules;
            switch (name)
            {
                case "goons.caught": return Caught ? 1.0 : 0.0;
                case "goons.sea": return State == GoonState.Alongside ? 1.0 : 0.0;
                case "goons.visit": return Visit;
                case "goons.men": return Men;
                case "goons.temper": return Temper;
                case "goons.fooled": return Fooled;
                case "goons.unpaid": return Unpaid;
                case "goons.ravn_next": return r.ravnAtRefusal >= 1.0 && Unpaid + 1 >= r.ravnAtRefusal && !RavnGone && Voyage.Men > 1 ? 1.0 : 0.0;
                case "goons.drink": return Drink() != null ? 1.0 : 0.0;
                case "goons.cargo": return CargoValue() * r.cargoPrice >= r.instalmentD - 0.5 ? 1.0 : 0.0;
                case "goons.out": return Out ? 1.0 : 0.0;
            }
            if (name.StartsWith("goons.used_", StringComparison.Ordinal))
                return Used(name.Substring("goons.used_".Length));
            if (name.StartsWith("goons.ease_", StringComparison.Ordinal))
            {
                string trick = name.Substring("goons.ease_".Length);
                string stat = r.tricks.TryGetValue(trick, out var t) ? t.stat : "nerve";
                return Ease(r, Self.IsStat(stat) ? Self.StatNow(stat) : 3.0, Used(trick), Math.Max(1, Visit));
            }
            return 0.0;
        }

        static double CargoValue()
        {
            if (!Trade.Started)
                return 0.0;
            double v = 0.0;
            foreach (var (good, count) in Trade.Hold.Contents)
                v += count * (double)good.value;
            return v;
        }

        /// <summary>The cheapest cask of drink aboard (null: none).</summary>
        static GoodDef Drink()
        {
            if (!Trade.Started)
                return null;
            GoodDef best = null;
            foreach (var (good, count) in Trade.Hold.Contents)
                if (count > 0 && (good.cat == "Drink" || good.id == "ale") && (best == null || good.value < best.value))
                    best = good;
            return best;
        }

        /// <summary>Deniers as the port she lies at reckons them; at sea, in deniers.</summary>
        static string Show(double deniers)
        {
            var port = GameData.Port(Port.LyingId());
            var zone = port != null && TradeRules.Current != null ? TradeRules.Current.ZoneOf(port.region) : null;
            return zone != null ? zone.Show(deniers) : deniers.ToString("0.#", CultureInfo.InvariantCulture) + " d";
        }

        /// <summary>Cargo taken off the hold to a value: the dearest lots first. Says what went, and its value.</summary>
        static string TakeCargo(double value, out double taken)
        {
            taken = 0.0;
            var gone = new List<string>();
            var lots = new List<(GoodDef good, int count)>(Trade.Hold.Contents);
            lots.Sort((a, b) => ((double)b.good.value).CompareTo((double)a.good.value));
            foreach (var (good, count) in lots)
            {
                if (taken >= value - 0.5 || count <= 0 || good.value <= 0)
                    continue;
                int n = Math.Min(count, (int)Math.Ceiling((value - taken) / (double)good.value));
                if (n <= 0)
                    continue;
                Trade.Hold.Remove(good, n);
                taken += n * (double)good.value;
                gone.Add(Trade.UnitsNamed(good, n));
            }
            return gone.Count == 0 ? "" : string.Join(", ", gone);
        }

        /// <summary>
        /// WHAT A CARD ORDERS OF THEM ("law goons_paid" and the rest: Encounters/GameEffects.Law hands every kind that
        /// begins "goons_" here). Returns what happened as a plain fact for the card's end, or null.
        ///   goons_paid        she paid on the stick of her own will: they go home, and his temper cools
        ///   goons_paid_well   she paid two pounds or more: it cools twice
        ///   goons_done        the stick is clean: they go home for good
        ///   goons_cargo       she gives cargo for an instalment, at their price
        ///   goons_fooled_X    the way round them X worked (hide, plead): they go home with nothing, and it will be
        ///                     harder next time
        ///   goons_astray      sent the wrong way: they sail for a port far from her
        ///   goons_drunk       they sleep; a cask of her drink is gone
        ///   goons_poured      the cask is gone and they are not drunk (the way round them failed)
        ///   goons_shaken      she has run from them at sea: they lose her for some hours
        ///   goons_routed      she has killed the one who killed Ravn: the rest go home (Voyage/Feud.cs)
        ///   goons_teeth       she gave nothing: what they do depends (Bite)
        ///   goons_teeth_angry the same, after a way round them has just failed
        /// PLACEHOLDER WORDS, every one: the user's to write.
        /// </summary>
        public static string Law(string kind)
        {
            var r = Rules;
            var game = KapmanGame.Current;
            double now = game != null ? game.Hours : 0.0;
            offered = false;
            switch (kind)
            {
                case "goons_paid":
                    Cool(1.0);
                    Unpaid = Math.Max(0, Unpaid - 1);
                    debtMark = Trade.Debt;
                    GoHome();
                    return null;
                case "goons_paid_well":
                    Cool(2.0);
                    Unpaid = Math.Max(0, Unpaid - 2);
                    debtMark = Trade.Debt;
                    GoHome();
                    return null;
                case "goons_done":
                    Temper = 0.0;
                    Unpaid = 0;
                    Recall();
                    return null;
                case "goons_cargo":
                {
                    string what = TakeCargo(r.instalmentD / Math.Max(0.05, r.cargoPrice), out double taken);
                    double counted = Math.Min(Trade.Debt, Math.Round(taken * r.cargoPrice));
                    Trade.Debt = Math.Max(0.0, Trade.Debt - counted);
                    debtMark = Trade.Debt;
                    if (counted > 0.5)
                        Unpaid = Math.Max(0, Unpaid - 1);
                    GoHome();
                    return what.Length == 0 ? "There was nothing in the hold they would carry." : $"They carry off {what}, which goes on the stick as {Show(counted)}. Still owed: {Show(Trade.Debt)}.";
                }
                case "goons_astray":
                {
                    Count("lie");
                    if (!Out)
                        return null;
                    string far = FarPort();
                    if (far.Length > 0 && Lay(far))
                    {
                        State = GoonState.Astray;
                        Version++;
                        return $"They row off for {PortName(far)}.";
                    }
                    Heat(1.0);
                    GoHome();
                    return "They go off to look for him.";
                }
                case "goons_drunk":
                case "goons_poured":
                {
                    var cask = Drink();
                    string gone = "";
                    if (cask != null)
                    {
                        Trade.Hold.Remove(cask, 1);
                        gone = $"{Cap(Trade.UnitsNamed(cask, 1))} is gone. ";
                    }
                    if (kind == "goons_poured")
                        return gone.Trim();
                    Count("drink");
                    if (!Out)
                        return gone.Trim();
                    if (State == GoonState.Alongside)
                        InPort = "";
                    State = GoonState.Sleeping;
                    until = now + r.sleepHours;
                    Version++;
                    return (gone + "They sleep.").Trim();
                }
                case "goons_shaken":
                    Count("run");
                    if (!Out)
                        return null;
                    blindUntil = now + r.blindHours;
                    Follow(now);
                    return null;
                case "goons_routed":
                    // (she has killed the one of them who killed Ravn: the rest go home to say so, and he is the angrier)
                    Heat(1.0);
                    GoHome();
                    return null;
                case "goons_teeth":
                case "goons_teeth_angry":
                    return Bitten(kind == "goons_teeth_angry");
            }
            if (kind.StartsWith("goons_fooled_", StringComparison.Ordinal))
            {
                Count(kind.Substring("goons_fooled_".Length));
                Heat(1.0);
                GoHome();
                return null;
            }
            return null;
        }

        /// <summary>A way round them has worked: it is counted (harder next time), and she has made fools of them.</summary>
        static void Count(string trick)
        {
            used[trick] = Used(trick) + 1;
            Fooled++;
            Unpaid++;                                                    // (and they went away with nothing)
            Version++;
        }

        /// <summary>A port far from her that the lanes lead to from where they are: the one furthest from her of those
        /// between 150 and 700 km off (the game's own ports, by its tables).</summary>
        static string FarPort()
        {
            var her = ChartWorld.ShipWorld();
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            string best = "";
            double most = 0.0;
            foreach (var def in GameData.Ports)
            {
                if (def == null || !def.Exists(year) || def.id == InPort || def.id == Rules.home)
                    continue;
                double d = WorldXZ.Distance(her, PortPlace(def.id));
                if (d < 150000.0 || d > 700000.0 || d <= most)
                    continue;
                most = d;
                best = def.id;
            }
            return best;
        }

        /// <summary>She gave them nothing (or made fools of them and was found out): what they do, done.</summary>
        static string Bitten(bool fooled)
        {
            var r = Rules;
            var rules = TradeRules.Current;
            var game = KapmanGame.Current;
            double silver = Trade.Started ? Trade.Purse.Silver(rules) : 0.0, cargo = CargoValue();
            int seed = game != null && game.World != null ? game.World.seed : 793;
            Unpaid++;
            var bite = Bite(r, Temper, fooled, silver + cargo, Voyage.Men, SimRandom.For(seed, "goons.kill." + Sent).Next(), Unpaid, !RavnGone);
            var said = new List<string>();
            if (bite.warned)
            {
                Heat(1.0);
                GoHome();
                return "They tell you what he told them to say, as far as they can remember it, and then they stand about, not sure what comes next, until Bubo leads them away.";
            }
            // silver first, then cargo for the rest; a share of it is for their trouble
            double got = 0.0, counted = 0.0;
            double s = Trade.Started ? Trade.Purse.PayUpTo(Math.Min(silver, bite.want), rules) : 0.0;
            if (s > 0.0)
            {
                got += s;
                double on = Math.Round(s * (1.0 - r.troubleShare));
                counted += on;
                said.Add($"They turn out the purse and take {Show(s)}, of which {Show(on)} goes on the stick and the rest is for their trouble.");
            }
            if (got < bite.want - 0.5)
            {
                string what = TakeCargo(bite.want - got, out double taken);
                if (what.Length > 0)
                {
                    got += taken;
                    double on = Math.Round(taken * r.cargoPrice * (1.0 - r.troubleShare));
                    counted += on;
                    said.Add($"They carry off {what}, and count it as {Show(on)} on the stick.");
                }
            }
            counted = Math.Min(Trade.Debt, counted);
            Trade.Debt = Math.Max(0.0, Trade.Debt - counted);
            if (got <= 0.5)
                said.Add("They turn out the purse and the hold, and there is nothing in either worth carrying.");
            // (what is still owed is said before what they do to a man: the card's end closes on him)
            if (counted > 0.5)
                said.Add($"Still owed: {Show(Trade.Debt)}.");
            if (bite.ravn)
            {
                // (the cards' engine keeps who of the standing cast is dead: he speaks no more, and a card that leans
                // on him does not come)
                ContentFlags.Set(EncounterState.GoneFlag(r.ravnCast));
                Voyage.LoseMen(1);
                // HIS DEATH HAS A SCENE OF ITS OWN (the user, 9 Oct 2026: "Ravn needs an impactful death scene as genuine
                // punishment for not repaying the debt"): this card's end only turns to him, and the scene is played
                // when the card is shut (Voyage/Feud.cs). With that rule off he dies in a sentence, as any man does.
                string lying = Port.LyingId();
                if (Feud.RavnKilled(lying == null, lying ?? ""))
                    said.Add("Then the one with the rope looks at Ravn.");
                else
                {
                    Voyage.Affect(0.0, -0.2);
                    said.Add("Then one of them puts his knife in Ravn, as carelessly as a man kicks a dog on his way out, and Ravn is dead before he reaches the planks.");
                }
            }
            else if (bite.kill)
            {
                int lost = Voyage.LoseMen(1);
                if (lost > 0)
                {
                    Voyage.Affect(0.0, -0.15);
                    said.Add("Then one of them puts his knife in the nearest of your men, as carelessly as a man kicks a dog on his way out, and the man is dead before he reaches the planks.");
                }
            }
            else if (bite.beat)
            {
                Voyage.Affect(-0.12, -0.08);
                said.Add("One of them beats the nearest of your men with the rope's end until the other tells him that is enough.");
            }
            debtMark = Trade.Debt;
            if (got <= 0.5)
                Heat(1.0);
            GoHome();
            return string.Join(" ", said);
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        // ------------------------------------------------------------------ a test's hands

        /// <summary>A test's: a boat of them is out now, at a place, as the n-th sending, with the lender's temper so.</summary>
        public static void PutOut(int visit, WorldXZ at, double temper, string inPort = "")
        {
            var game = KapmanGame.Current;
            Sent = Math.Max(Sent, visit);
            Visit = visit;
            outSince = game != null ? game.Day : 0.0;
            Temper = temper;
            At = at;
            InPort = inPort ?? "";
            Target = InPort;
            line.Clear();
            asked.Clear();
            toldAt.Clear();
            offered = sightTold = false;
            blindUntil = 0.0;
            State = GoonState.Asking;
            until = (game != null ? game.Hours : 0.0) + (InPort.Length > 0 ? Rules.askHours : 0.0);
            if (InPort.Length == 0)
                Follow(game != null ? game.Hours : 0.0);
            Version++;
        }

        /// <summary>A test's: so many visits have ended with nothing paid of her own will.</summary>
        public static void Refused(int times)
        {
            Unpaid = Math.Max(0, times);
            Version++;
        }

        // ------------------------------------------------------------------ a saved game

        static object Capture()
        {
            var points = new List<object>();
            foreach (var p in line)
            {
                points.Add(p.x);
                points.Add(p.z);
            }
            var trail = new List<object>();
            foreach (var a in asked)
                trail.Add(new Dictionary<string, object> { ["port"] = a.port, ["day"] = a.day, ["on"] = a.on });
            var ways = new Dictionary<string, object>();
            foreach (var kv in used)
                ways[kv.Key] = (double)kv.Value;
            return new Dictionary<string, object>
            {
                ["state"] = State.ToString(), ["sent"] = (double)Sent, ["visit"] = (double)Visit, ["temper"] = Temper, ["x"] = At.x, ["z"] = At.z,
                ["target"] = Target, ["in_port"] = InPort, ["met"] = Met, ["fooled"] = (double)Fooled, ["unpaid"] = (double)Unpaid, ["line"] = points, ["next"] = (double)next,
                ["out_since"] = outSince, ["until"] = until, ["debt_mark"] = double.IsNaN(debtMark) ? -1.0 : debtMark, ["blind_until"] = blindUntil,
                ["asked"] = trail, ["told_at"] = toldAt.OrderBy(s => s, StringComparer.Ordinal).Cast<object>().ToList(), ["used"] = ways,
                ["last_port"] = lastPort, ["bound"] = bound, ["sending_told"] = sendingTold, ["known"] = known,
            };
        }

        static void Restore(object saved)
        {
            Forget();
            if (saved == null)
                return;
            State = Enum.TryParse(Json.Str(saved, "state"), out GoonState s) ? s : GoonState.Home;
            Sent = (int)Json.Num(saved, "sent");
            Visit = (int)Json.Num(saved, "visit");
            Temper = Json.Num(saved, "temper", Rules.temperStart);
            At = new WorldXZ(Json.Num(saved, "x"), Json.Num(saved, "z"));
            Target = Json.Str(saved, "target");
            InPort = Json.Str(saved, "in_port");
            Met = Json.Bool(saved, "met");
            Fooled = (int)Json.Num(saved, "fooled");
            Unpaid = (int)Json.Num(saved, "unpaid");
            var points = Json.Arr(saved, "line") ?? new List<object>();
            for (int i = 0; i + 1 < points.Count; i += 2)
                if (points[i] is double x && points[i + 1] is double z)
                    line.Add(new WorldXZ(x, z));
            next = (int)Json.Num(saved, "next", 1);
            outSince = Json.Num(saved, "out_since");
            until = Json.Num(saved, "until");
            double mark = Json.Num(saved, "debt_mark", -1.0);
            debtMark = mark < 0.0 ? double.NaN : mark;
            blindUntil = Json.Num(saved, "blind_until");
            foreach (object o in Json.Arr(saved, "asked") ?? new List<object>())
                asked.Add((Json.Str(o, "port"), Json.Num(o, "day"), Json.Str(o, "on")));
            foreach (object o in Json.Arr(saved, "told_at") ?? new List<object>())
                if (o is string id)
                    toldAt.Add(id);
            foreach (var kv in Json.Obj(saved, "used") ?? new Dictionary<string, object>())
                if (kv.Value is double n)
                    used[kv.Key] = (int)n;
            lastPort = Json.Str(saved, "last_port");
            bound = Json.Str(saved, "bound");
            sendingTold = Json.Bool(saved, "sending_told");
            known = Json.Bool(saved, "known");
            if (Visit < 1 || Visit > Rules.sendings.Count)
            {
                State = GoonState.Home;                                   // (a table with fewer sendings than the save had)
                Visit = 0;
            }
            Version++;
        }

        /// <summary>As a new game has it: nobody out, nobody sent, the lender's temper where it starts.</summary>
        public static void Forget()
        {
            State = GoonState.Home;
            Sent = Visit = Fooled = Unpaid = 0;
            Heading = 0.0;
            Temper = Rules.temperStart;
            At = default;
            Target = InPort = lastPort = bound = "";
            offeredAt = 0.0;
            Met = InSight = offered = sightTold = sendingTold = known = false;
            used.Clear();
            line.Clear();
            asked.Clear();
            toldAt.Clear();
            next = 1;
            outSince = until = blindUntil = unseenHours = 0.0;
            debtMark = double.NaN;
            Version++;
        }

        // Enter Play Mode runs without a domain reload, and a new game runs this too (MainMenu.ClearAllState)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            own = null;
            Forget();
            ContentSave.Register("collectors", Capture, Restore);
        }
    }
}
