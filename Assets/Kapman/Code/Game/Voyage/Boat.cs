using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>The cloth: whole; weak (a seam has let go: she is not driven, and it may split in a breeze); split (half a
    /// sail on a jury rig until it is sewn or bought).</summary>
    public enum SailCloth { Whole, Weak, Split }
    /// <summary>How it is set: all of it, shortened, or down (she goes by the oars).</summary>
    public enum SailSet { Full, Reefed, Down }
    /// <summary>The standing order for a gale: strike the sail and hold her head to it under oars (what the world's ships
    /// do); run on under the shortened sail when the wind is abaft the beam; or carry all of it.</summary>
    public enum GaleOrder { LieTo, RunReefed, CarrySail }

    /// <summary>The boat's table (Data/boat_rules.json; every number a STARTING GUESS TO TUNE).</summary>
    public sealed class BoatRules
    {
        // her speed
        public double reefedSpeed = 0.7, weakSailCap = 0.8, splitSpeed = 0.5, wearyOars = 0.5, waterSlow = 0.3, runAbaftDeg = 100.0;
        // the hull: wear, and where a wound or a mend leaves her
        public double wearPerDay = 0.0005, untarredWear = 2.0, galeWearPerHour = 0.004, reefedWear = 2.0, carryWear = 5.0;
        public double strakeHull = 0.55, patchHull = 0.75, strikeHull = 0.3, tarLastsDays = 300.0;
        // water: litres an hour in, and what the men lift out
        public double soundLeak = 2.0, openLeak = 6000.0, workFromWind = 8.0;
        public double freshSeas = 60.0, seasLieTo = 300.0, seasReefed = 1200.0, seasCarry = 4000.0, freshWind = 13.0, scoopAnHour = 10.0;
        public double manBails = 1500.0, burstBails = 2400.0, oneManFrom = 100.0, twoMenFrom = 400.0, allHandsFrom = 1200.0;
        public double swampPerM2 = 180.0, spoilFrom = 400.0;
        // weariness, a share an hour
        public double wearySailing = 0.004, wearyNight = 0.008, wearyOarsHour = 0.012, wearyHardOars = 0.03, wearyBailing = 0.02, restMoored = 0.04, restLyingUp = 0.03;
        public double wearyHeartFrom = 0.8, wearyHeartPerHour = 0.005;
        // the dice: what multiplies the chance every hull has
        public double diceReefed = 2.5, diceCarry = 8.0, diceStrained = 2.0, diceOpened = 5.0, diceHeavy = 2.0, diceShortHanded = 1.5,
                      diceLashed = 0.8, diceRiver = 0.1;
        public double strainedBelow = 0.7, openedBelow = 0.5;
        // the sail's own dice, a chance an hour
        public double weakSplits = 0.12, weakSplitsWind = 9.0, carryTears = 0.15;
        // the signs: hours ahead the weather is read, and when each sign comes
        public double lookAhead = 12.0, secondSign = 6.0, lastSign = 1.0, fairAfterHours = 1.0;
        // shares and bundles
        public double shareOfGain = 0.1, wageOnShares = 0.5, bundleKgPerMan = 20.0, noGainHeart = -0.1, gainHeart = 0.1;
        public double martinmasMonth = 11.0, martinmasDay = 11.0;
        // a yard's prices, deniers
        public double hullMendBase = 8.0, hullMendFull = 20.0, sailSewn = 3.0, sailNew = 12.0;

        /// <summary>The table, by the fields' own names ("reefedSpeed": 0.7); a name not in the file keeps its number here.</summary>
        public static BoatRules Load(TextAsset json)
        {
            var r = new BoatRules();
            if (json == null)
                return r;
            var o = Json.Parse(json.text);
            foreach (var f in typeof(BoatRules).GetFields())
                if (f.FieldType == typeof(double))
                    f.SetValue(r, Json.Num(o, f.Name, (double)f.GetValue(r)));
            return r;
        }
    }

    /// <summary>What one game hour is like where she is: handed to <see cref="Boat.Hour"/> by the live game (BoatWatch) or
    /// by a test.</summary>
    public struct BoatHour
    {
        /// <summary>The hour's count since the start (the tick ladder's).</summary>
        public long hour;
        /// <summary>The wind where she is, m/s, and how far off it her course lies (0 into it, 180 before it).</summary>
        public double wind, offWind;
        public bool underWay, moored, river, night;
        /// <summary>The oars are out by the player's order (Voyage/Oars.cs): the men row hard this hour.</summary>
        public bool hardOars;
        /// <summary>Hours to the first hour of wind over the gale line at her place, within the look-ahead (0: it blows
        /// now; infinity: none in sight).</summary>
        public double galeIn;
        /// <summary>Men aboard, and the hands her class wants.</summary>
        public int men, hands;
        public double length, beam, fragility, lossPerHour;
        public int seed;
        /// <summary>How rough the water she is in is, beyond what the wind makes it (Voyage/RoughWaters.cs: a tide race,
        /// a bar, a shoal, an open head): 0 none, 1 as a man bailing and the hull working as in a gale.</summary>
        public double rough;
    }

    /// <summary>
    /// THE BOAT SHE TRULY IS, on the chart (the strategy critic, 7 Oct 2026: "the deck sails a second boat that the
    /// simulation never meets"; his third change: "the same gale dice for her as for the other ships (warned first), and
    /// sail, leak and shares kept in the simulation instead of in card flags"). One state, written by the cards and by the
    /// weather, read by the simulation:
    ///   her hull's soundness (1 sound, 0 open), the water in her (litres), the sail's cloth and how it is set, the
    ///   order for a gale, the men's weariness (0 to 1), whether they sail on shares.
    /// Each game hour (<see cref="Hour"/>): the signs of a gale within the look-ahead; the hull works; water comes in by
    /// the laps and over the gunwale and the men bail it; the men tire or rest; a weak sail may split; and in a gale she
    /// has been warned of, the sea's dice are thrown for her as for every hull (GaleDice), times the state she is in.
    /// A strike wounds before it kills: a started strake first, then the sail, then the boat. She is also lost if she
    /// fills. Lost is the end of the game (the user, 2 October: a wreck is game over; the game can be saved at any time).
    /// Her speed (<see cref="Speed"/>) is the world's ships' own (Travel.Speed) through her sail, her water and her men.
    /// Plain data: saved and restored whole; no dice of its own to save (GaleDice.Roll has no memory).
    /// </summary>
    public static class Boat
    {
        public static BoatRules R { get; private set; } = new BoatRules();

        public static double Hull { get; private set; } = 1.0;
        public static double Water { get; private set; }
        public static SailCloth Cloth { get; private set; }
        public static SailSet Set { get; private set; }
        public static GaleOrder Order { get; private set; }
        public static double Weary { get; private set; }
        public static bool OnShares { get; private set; }
        public static bool Bundles { get; private set; }
        public static bool SpareSpar { get; private set; }

        /// <summary>The sign of the gale coming: 0 none, 1 the swell, 2 the sky, 3 within the hour.</summary>
        public static int Sign { get; private set; }
        /// <summary>Hours to the gale at her place when last read (infinity: none in sight; 0: it blows).</summary>
        public static double GaleIn { get; private set; } = double.PositiveInfinity;
        public static bool InGale { get; private set; }
        /// <summary>The men bailing this hour, and whether all hands are at it.</summary>
        public static int Bailers { get; private set; }
        public static bool AllHands { get; private set; }
        public static bool IsLost { get; private set; }
        /// <summary>How she was lost, as a plain sentence ("" while she swims).</summary>
        public static string LostHow { get; private set; } = "";
        /// <summary>What she holds before she is swamped, litres (from her class's length and beam).</summary>
        public static double Swamp { get; private set; } = 3000.0;
        /// <summary>The chance the sea had of striking her in the last gale hour (0 outside one): for the Ship panel.</summary>
        public static double LastChance { get; private set; }

        static long now, signAt = long.MinValue, lastGaleHour = long.MinValue, lieToUntil = long.MinValue;
        static long reefUntil = long.MaxValue, graceUntil = long.MinValue;
        static double lashedUntilDay = double.NegativeInfinity, tarredDay = double.NegativeInfinity;
        static double seasonBase = double.NaN;
        static int paidYear = int.MinValue;
        static int men = 4, hands = 4;

        /// <summary>A sign of weather (1, 2, 3), with the hours to the gale: the live game drops the clock and opens the card.</summary>
        public static event Action<int, double> SignCame;
        /// <summary>A plain fact about the boat ("The sail splits."): for the log.</summary>
        public static event Action<string> Said;
        /// <summary>She is lost (how): the end of the game.</summary>
        public static event Action<string> Lost;

        public static void Begin(BoatRules rules)
        {
            R = rules ?? new BoatRules();
            Hull = 1.0;
            Water = 0.0;
            Cloth = SailCloth.Whole;
            Set = SailSet.Full;
            Order = GaleOrder.LieTo;
            Weary = 0.0;
            OnShares = Bundles = SpareSpar = false;
            Sign = 0;
            GaleIn = double.PositiveInfinity;
            InGale = AllHands = IsLost = false;
            Bailers = 0;
            LostHow = "";
            LastChance = 0.0;
            now = 0;
            signAt = lastGaleHour = lieToUntil = long.MinValue;
            reefUntil = long.MaxValue;
            graceUntil = long.MinValue;
            lashedUntilDay = double.NegativeInfinity;
            tarredDay = -120.0;                                            // (a new game's boat was tarred in the winter before it)
            seasonBase = double.NaN;
            paidYear = int.MinValue;
            men = hands = 4;
        }

        // ------------------------------------------------------------------ the orders (the panel, the cards)

        /// <summary>The sail set (shortened for some hours, when a card says so; else until it is changed).</summary>
        public static void SetSail(SailSet set, double forHours = 0.0)
        {
            Set = set;
            reefUntil = set == SailSet.Reefed && forHours > 0.0 ? now + (long)Math.Ceiling(forHours) : long.MaxValue;
        }

        public static void SetOrder(GaleOrder order) => Order = order;

        /// <summary>She lies to for some hours (a card: the sail sewn, the cargo restowed, a patch from over the side).</summary>
        public static void LieTo(double hours) => lieToUntil = Math.Max(lieToUntil, now + (long)Math.Ceiling(Math.Max(0.0, hours)));

        public static bool LyingTo => lieToUntil > now;
        public static bool Lashed(double day) => day < lashedUntilDay;
        public static bool Tarred(double day) => day - tarredDay < R.tarLastsDays;
        /// <summary>Hours since the last hour of a gale she was out in (infinity: none yet).</summary>
        public static double HoursSinceGale => lastGaleHour == long.MinValue ? double.PositiveInfinity : now - lastGaleHour;
        /// <summary>She has had a sign of this gale long enough ago to have acted on it: only then are the dice thrown.</summary>
        public static bool FairlyWarned => signAt != long.MinValue && now - signAt >= R.fairAfterHours && now >= graceUntil;

        /// <summary>No dice before this hour (the clock was jumped: hours nobody could answer are not held against her).</summary>
        public static void Grace(long untilHour) => graceUntil = Math.Max(graceUntil, untilHour);

        // ------------------------------------------------------------------ wounds and mends (the cards, a yard)

        /// <summary>The hull changed by a share, either way (a grounding, a patch), kept within 0 to 1.</summary>
        public static void HullBy(double share) => Hull = Math.Max(0.0, Math.Min(1.0, Hull + share));

        /// <summary>A started strake: the hull no better than "started" (if it was already, a little worse).</summary>
        public static void Strake() => Hull = Hull > R.strakeHull ? R.strakeHull : Math.Max(0.0, Hull - 0.1);

        /// <summary>Patched at sea: no worse than "working". A patch is not a yard's mend.</summary>
        public static void Patch() => Hull = Math.Max(Hull, R.patchHull);

        /// <summary>Hauled out and tarred (the winter's card, a yard): sound, and tarred from this day.</summary>
        public static void Tar(double day)
        {
            Hull = 1.0;
            tarredDay = day;
        }

        public static void WaterBy(double litres) => Water = Math.Max(0.0, Math.Min(Swamp, Water + litres));
        public static void SetCloth(SailCloth cloth) => Cloth = cloth;
        public static void WearyBy(double share) => Weary = Math.Max(0.0, Math.Min(1.0, Weary + share));
        public static void Lash(double day, double days) => lashedUntilDay = days > 0.0 ? day + days : double.NegativeInfinity;
        public static void Spar(bool aboard) => SpareSpar = aboard;
        public static void SetBundles(bool on) => Bundles = on;

        /// <summary>What a yard asks to make her hull sound, and her sail whole (deniers; 0: nothing to do).</summary>
        public static double HullMendCost => Hull >= 0.999 ? 0.0 : Math.Round(R.hullMendBase + R.hullMendFull * (1.0 - Hull));
        /// <summary>Her seams want a yard: she is no longer "sound" (a day's wear is not work for one).</summary>
        public static bool WantsSeams => Hull < 0.9;
        public static double SailMendCost => Cloth == SailCloth.Whole ? 0.0 : Cloth == SailCloth.Weak ? R.sailSewn : R.sailNew;

        // ------------------------------------------------------------------ shares

        /// <summary>The men go on shares (or back on the wage). worthNow: the house's worth now (silver and cargo, less the
        /// debt), from which the season's gain is counted.</summary>
        public static void Shares(bool on, double worthNow)
        {
            if (on && !OnShares)
                seasonBase = worthNow;
            OnShares = on;
        }

        /// <summary>The share of the day's wage that is owed (the whole of it on a wage; part of it on shares).</summary>
        public static double WageFactor => OnShares ? R.wageOnShares : 1.0;

        /// <summary>Martinmas (or the first day after it in a year not yet reckoned): what the men are owed of the
        /// season's gain, deniers (0 when not on shares, when it is not the day, or when there was no gain; `due` says
        /// whether this was the reckoning).</summary>
        public static double Martinmas(int year, int month, int day, double worthNow, out bool due)
        {
            due = OnShares && year != paidYear && (month > R.martinmasMonth || (month == (int)R.martinmasMonth && day >= R.martinmasDay));
            if (!due)
                return 0.0;
            paidYear = year;
            double gain = double.IsNaN(seasonBase) ? 0.0 : worthNow - seasonBase;
            seasonBase = worthNow;
            return gain > 0.0 ? Math.Round(gain * R.shareOfGain, 1) : 0.0;
        }

        /// <summary>Room in the hold taken by what is not cargo: the water in her, and the men's own bundles (kg).</summary>
        public static double TakenKg => Water + (Bundles ? R.bundleKgPerMan * men : 0.0);

        // ------------------------------------------------------------------ what the simulation reads

        static double OffWind(double courseDeg, double windFrom) => Math.Abs(((courseDeg - windFrom) % 360.0 + 540.0) % 360.0 - 180.0);

        /// <summary>The share of her speed the water in her leaves (dry: all of it; half swamped or more: 1 - waterSlow).</summary>
        public static double WaterFactor => 1.0 - R.waterSlow * Math.Min(1.0, Water / Math.Max(1.0, Swamp * 0.5));

        /// <summary>The share of her speed under oars her men leave: weary, short-handed, some of them bailing.</summary>
        public static double OarFactor =>
            (1.0 - R.wearyOars * Weary) * Math.Max(0.0, Math.Min(1.0, (men - Bailers) / (double)Math.Max(1, hands)));

        /// <summary>
        /// Her speed made good along her course (m/s) and whether she lies to: Travel.Speed, the world's ships' own
        /// rule, through her state. Shortened sail is <c>reefedSpeed</c> of it; a weak sail is not driven past
        /// <c>weakSailCap</c> of her class's speed; a split one gives <c>splitSpeed</c>; the sail down, or the oars
        /// better, she rows, as well as her men can; water in her slows her. In a gale she lies to, unless her order
        /// is to run and the wind is abaft the beam.
        /// </summary>
        public static double Speed(ShipClassDef c, double courseDeg, double windFrom, double windSpeed, out bool lying)
        {
            lying = false;
            if (c == null || IsLost)
            {
                lying = IsLost;
                return 0.0;
            }
            if (LyingTo)
            {
                lying = true;
                return 0.0;
            }
            double oars = c.oarsKn * Math.Min(1.0, c.rowingHours / 12.0) * SimUnits.KnotMs;
            if (windSpeed > GaleDice.GaleWind)
            {
                bool run = Order != GaleOrder.LieTo && c.hasSail && Cloth != SailCloth.Split && OffWind(courseDeg, windFrom) >= R.runAbaftDeg;
                lying = !run;
                return run ? c.maxKn * SimUnits.KnotMs * (Order == GaleOrder.RunReefed ? R.reefedSpeed : 1.0) * WaterFactor : 0.0;
            }
            double made = Travel.Speed(c, courseDeg, windFrom, windSpeed, out _);
            double sail = made > oars + 1e-9 ? made : 0.0;                  // (Travel takes the better of the two)
            if (EffectiveSet == SailSet.Down)
                sail = 0.0;
            else if (sail > 0.0)
            {
                if (EffectiveSet == SailSet.Reefed)
                    sail *= R.reefedSpeed;
                if (Cloth == SailCloth.Split)
                    sail *= R.splitSpeed;
                else if (Cloth == SailCloth.Weak)
                    sail = Math.Min(sail, R.weakSailCap * c.sailKn * SimUnits.KnotMs);
            }
            return Math.Max(sail, oars * OarFactor) * WaterFactor;
        }

        /// <summary>How the sail is set now (a reef taken for some hours shakes itself out).</summary>
        public static SailSet EffectiveSet => Set == SailSet.Reefed && now >= reefUntil ? SailSet.Full : Set;

        /// <summary>What her state multiplies the sea's chance by, in a gale: 1 for a sound boat lying to with a full
        /// crew (what the world's ships are).</summary>
        public static double DiceFactor(bool river, double day)
        {
            double f = Order == GaleOrder.CarrySail ? R.diceCarry : Order == GaleOrder.RunReefed ? R.diceReefed : 1.0;
            f *= Hull < R.openedBelow ? R.diceOpened : Hull < R.strainedBelow ? R.diceStrained : 1.0;
            if (Water >= Swamp * 0.5)
                f *= R.diceHeavy;
            f *= 1.0 + Weary;
            if (men < hands)
                f *= R.diceShortHanded;
            if (Lashed(day))
                f *= R.diceLashed;
            if (river)
                f *= R.diceRiver;
            return f;
        }

        /// <summary>Litres an hour through the laps: a sound hull's weep, and far more as she opens and as she works in a sea.</summary>
        public static double Leak(double wind)
        {
            double open = 1.0 - Hull, work = Math.Max(0.0, Math.Min(1.0, (wind - R.workFromWind) / Math.Max(1.0, GaleDice.GaleWind - R.workFromWind)));
            return R.soundLeak + open * open * R.openLeak * (1.0 + work);
        }

        // ------------------------------------------------------------------ the hour

        /// <summary>One game hour of the boat. Returns the facts of the hour, in order (also raised through Said).</summary>
        public static List<string> Hour(BoatHour h)
        {
            var facts = new List<string>();
            if (IsLost)
                return facts;
            now = h.hour;
            men = Math.Max(1, h.men);
            hands = Math.Max(1, h.hands);
            Swamp = Math.Max(500.0, R.swampPerM2 * Math.Max(1.0, h.length) * Math.Max(1.0, h.beam));
            if (Set == SailSet.Reefed && now >= reefUntil)
                SetSail(SailSet.Full);
            double day = h.hour / 24.0;
            bool gale = h.wind > GaleDice.GaleWind && !h.moored;

            // the signs: read ahead, and told once each for one gale (a gale that has blown itself out is done with:
            // the next one has its own signs, and its own hour of warning before any dice)
            if (InGale && !gale)
            {
                Sign = 0;
                signAt = long.MinValue;
            }
            GaleIn = h.moored ? double.PositiveInfinity : gale ? 0.0 : h.galeIn;
            if (!double.IsInfinity(GaleIn) && !double.IsNaN(GaleIn) && GaleIn <= R.lookAhead)
            {
                int sign = GaleIn <= R.lastSign ? 3 : GaleIn <= R.secondSign ? 2 : 1;
                if (sign > Sign)
                {
                    if (Sign == 0)
                        signAt = now;
                    Sign = sign;
                    SignCame?.Invoke(sign, GaleIn);
                }
            }
            else if (Sign != 0)
            {
                Sign = 0;                                                  // (it passed her by, or it is over, or she is made fast)
                signAt = long.MinValue;
            }

            // the stance in a gale, from the order: the sail comes down, is shortened, or is carried
            bool wasInGale = InGale;
            InGale = gale;
            bool running = gale && Order != GaleOrder.LieTo && Cloth != SailCloth.Split && h.offWind >= R.runAbaftDeg;
            bool carrying = running && Order == GaleOrder.CarrySail;
            if (gale)
                lastGaleHour = now;

            // the hull works
            if (gale)
                HullBy(-R.galeWearPerHour * (carrying ? R.carryWear : running ? R.reefedWear : 1.0) * (Tarred(day) ? 1.0 : R.untarredWear));
            // (9 Oct 2026: and in rough water, a race or a bar or a shoal, whatever the wind)
            if (h.rough > 0.0 && !h.moored)
                HullBy(-SeaPush.Rules.roughWear * h.rough * (Tarred(day) ? 1.0 : R.untarredWear));

            // the sail's own dice
            bool sailUp = gale ? running : EffectiveSet != SailSet.Down;
            if (sailUp && h.underWay && Cloth == SailCloth.Weak && h.wind >= R.weakSplitsWind && GaleDice.Roll(h.seed, "player.sail", h.hour) < R.weakSplits)
            {
                Cloth = SailCloth.Split;
                Tell(facts, "The sail splits from the foot to the yard.");
            }
            else if (carrying && Cloth != SailCloth.Split && GaleDice.Roll(h.seed, "player.sail", h.hour) < R.carryTears)
            {
                Cloth = Cloth == SailCloth.Whole ? SailCloth.Weak : SailCloth.Split;
                Tell(facts, Cloth == SailCloth.Weak ? "A seam of the sail lets go." : "The sail splits from the foot to the yard.");
            }

            // water: in by the laps and over the gunwale, out by the men
            if (!h.moored)
            {
                double before = Water;
                bool wasAllHands = AllHands;
                double seas = gale ? (carrying ? R.seasCarry : running ? R.seasReefed : R.seasLieTo) : h.wind >= R.freshWind && !h.river ? R.freshSeas : 0.0;
                if (h.rough > 0.0)
                    seas += SeaPush.Rules.roughSeas * h.rough;             // (rough water comes over the gunwale too)
                double into = Leak(h.wind) + seas, after = Water + into;
                // the fewest men that keep it down this hour: none, one, two, then all hands at a pace they cannot hold;
                // weary men lift less
                double tired = 1.0 - R.wearyOars * Weary, one = R.manBails * tired, two = Math.Min(2, men) * one;
                AllHands = false;
                if (after < R.oneManFrom)
                    Bailers = 0;
                else if (after - one < R.twoMenFrom)
                    Bailers = 1;
                else if (after - two < R.allHandsFrom)
                    Bailers = Math.Min(2, men);
                else
                {
                    Bailers = men;
                    AllHands = true;
                }
                double lifted = AllHands ? men * R.burstBails * tired : Bailers * one;
                // (a tight boat's weep is scooped out as it comes, by whoever sits nearest: no man is set to it, and she
                // is dry. The play-through of 9 Oct 2026: left to stand, it took a hundred kg of her room every two days)
                if (Bailers == 0 && !gale)
                    lifted = Math.Min(after, R.scoopAnHour);
                Water = Math.Max(0.0, Math.Min(Swamp, after - lifted));
                if (before < R.spoilFrom && Water >= R.spoilFrom)
                    Tell(facts, "The water is over the floor timbers: the lowest of the cargo stands in it.");
                if (AllHands && !wasAllHands)
                    Tell(facts, "All hands to the bailing.");
            }
            else
            {
                Water = Math.Max(0.0, Water - men * R.manBails);           // (made fast: bailed dry at leisure)
                Bailers = 0;
                AllHands = false;
            }

            // the men tire, or rest
            bool rowing = h.underWay && (h.hardOars || (gale ? !running : EffectiveSet == SailSet.Down || h.wind < 1.5));
            bool hard = h.underWay && (h.hardOars || (gale && !running));
            if (h.moored)
                WearyBy(-R.restMoored);
            else if (!h.underWay && !gale)
                WearyBy(-R.restLyingUp);
            else
                // (rowing tires them: little at their own stroke, in a calm or with the sail down; much at the hard pull
                // by the player's order, or holding her head to a gale)
                WearyBy((h.night ? R.wearyNight : R.wearySailing) + (hard ? R.wearyHardOars : rowing ? R.wearyOarsHour : 0.0)
                        + R.wearyBailing * Bailers / men * (AllHands ? 2.0 : 1.0));

            // the sea's dice: the same as for every hull, times her state, and only in a gale she was warned of
            LastChance = 0.0;
            if (gale)
            {
                LastChance = GaleDice.Chance(h.lossPerHour, h.fragility, DiceFactor(h.river, day));
                if (!wasInGale)
                    Tell(facts, running ? "The gale is on her: she runs before it." : "The gale is on her: she lies to.");
                if (FairlyWarned && GaleDice.Roll(h.seed, "player", h.hour) < LastChance)
                    Strike(facts, running);
            }

            if (!IsLost && Water >= Swamp)
                Lose(facts, "She filled, and the sea came over the gunwale.");
            if (!IsLost && Hull <= 0.0)
                Lose(facts, "Her strakes opened and she went down under them.");
            return facts;
        }

        /// <summary>The sea strikes her: a wound before a death. A boat still tight takes a started strake; one already
        /// started loses her sail, if it is up; after that it is the boat.</summary>
        static void Strike(List<string> facts, bool sailUp)
        {
            if (Hull >= R.strainedBelow)
            {
                Hull = Math.Min(Hull - R.strikeHull, R.strakeHull);
                WaterBy(300.0);
                Tell(facts, "A sea breaks aboard. A strake has started below the waterline.");
            }
            else if (sailUp && Cloth != SailCloth.Split)
            {
                Cloth = SailCloth.Split;
                WaterBy(300.0);
                Tell(facts, "A sea lays her over. The sail splits, and she comes back up without it.");
            }
            else
                Lose(facts, "A sea broke over her that she could not lift to.");
        }

        static void Lose(List<string> facts, string how)
        {
            IsLost = true;
            LostHow = how;
            Tell(facts, how);
            Lost?.Invoke(how);
        }

        static void Tell(List<string> facts, string fact)
        {
            facts.Add(fact);
            Said?.Invoke(fact);
        }

        /// <summary>A day of the boat: the hull's slow wear (twice it untarred).</summary>
        public static void Day(double day) => HullBy(-R.wearPerDay * (Tarred(day) ? 1.0 : R.untarredWear));

        // ------------------------------------------------------------------ the cards' flags are this boat

        /// <summary>A card set one of the boat's flags: the boat takes it. False for a flag that is not the boat's.</summary>
        public static bool FlagSet(string name, double value, double days, double day, double worthNow)
        {
            bool on = value != 0.0;
            switch (name)
            {
                case "sea_leak":
                    if (on) Strake();
                    else Patch();
                    return true;
                case "sea_sail_weak":
                    if (on && Cloth == SailCloth.Whole) Cloth = SailCloth.Weak;
                    else if (!on && Cloth == SailCloth.Weak) Cloth = SailCloth.Whole;      // (sewn; a split sail takes "boat sail whole")
                    return true;
                case "sea_reefed":
                    SetSail(on ? SailSet.Reefed : SailSet.Full, on ? days * 24.0 : 0.0);
                    return true;
                case "sea_lashed":
                    Lash(day, on ? Math.Max(days, 1.0) : 0.0);
                    return true;
                case "sea_spar_aboard":
                    SpareSpar = on;
                    return true;
                case "sea_after_gale":
                    return true;                                             // (the boat knows: nothing to set)
                case "crew_on_shares":
                    Shares(on, worthNow);
                    return true;
                case "crew_own_bundles":
                    Bundles = on;
                    return true;
                case "crew_boat_tarred":
                    if (on) Tar(day);
                    return true;
            }
            return false;
        }

        /// <summary>What a card reads for one of the boat's flags (null: not the boat's; the deck's own memory answers).</summary>
        public static double? FlagView(string name, double day)
        {
            switch (name)
            {
                case "sea_leak": return Hull < R.strainedBelow ? 1.0 : 0.0;
                case "sea_sail_weak": return Cloth == SailCloth.Weak ? 1.0 : 0.0;
                case "sea_reefed": return EffectiveSet == SailSet.Reefed ? 1.0 : 0.0;
                case "sea_lashed": return Lashed(day) ? 1.0 : 0.0;
                case "sea_spar_aboard": return SpareSpar ? 1.0 : 0.0;
                case "sea_after_gale": return !InGale && HoursSinceGale <= 48.0 ? 1.0 : 0.0;
                case "crew_on_shares": return OnShares ? 1.0 : 0.0;
                case "crew_own_bundles": return Bundles ? 1.0 : 0.0;
                case "crew_boat_tarred": return Tarred(day) ? 1.0 : 0.0;
            }
            return null;
        }

        // ------------------------------------------------------------------ words for the panels

        public static string HullWord => Hull >= 0.9 ? "sound" : Hull >= R.strainedBelow ? "working" : Hull >= R.openedBelow ? "a strake started" : Hull >= 0.3 ? "opening" : "going";
        public static string WaterWord => Water < 20.0 ? "dry" : Water < R.oneManFrom ? "wet underfoot" : Water < R.twoMenFrom ? "a hand deep"
                                        : Water < R.allHandsFrom ? "over the floor timbers" : Water < Swamp * 0.75 ? "to the thwarts" : "awash";
        public static string ClothWord => Cloth == SailCloth.Whole ? "whole" : Cloth == SailCloth.Weak ? "a seam gone" : "split: half a sail";
        public static string SetWord => EffectiveSet == SailSet.Full ? "all of it" : EffectiveSet == SailSet.Reefed ? "shortened" : "down";
        public static string OrderWord => Order == GaleOrder.LieTo ? "lie to" : Order == GaleOrder.RunReefed ? "run under shortened sail" : "carry sail";
        public static string WearyWord => Weary < 0.25 ? "fresh" : Weary < 0.5 ? "tired" : Weary < 0.8 ? "weary" : "spent";

        /// <summary>The chance of a strike in a night of it (twelve hours) at this chance an hour.</summary>
        public static double NightChance(double chancePerHour) => 1.0 - Math.Pow(1.0 - Math.Max(0.0, Math.Min(1.0, chancePerHour)), 12.0);

        /// <summary>How she would fare in a gale as she is, from the chance an hour.</summary>
        public static string GaleWord(double chancePerHour)
        {
            double night = NightChance(chancePerHour);
            return night < 0.005 ? "she would ride it" : night < 0.02 ? "she would be pressed" : night < 0.08 ? "she would be in danger" : "she would not live long in it";
        }

        // ------------------------------------------------------------------ the save

        public static Dictionary<string, object> Capture() => new Dictionary<string, object>
        {
            ["hull"] = Hull, ["water"] = Water, ["cloth"] = (double)(int)Cloth, ["set"] = (double)(int)Set, ["order"] = (double)(int)Order,
            ["weary"] = Weary, ["shares"] = OnShares, ["bundles"] = Bundles, ["spar"] = SpareSpar, ["sign"] = (double)Sign,
            ["now"] = (double)now, ["sign_at"] = Stamp(signAt), ["last_gale"] = Stamp(lastGaleHour),
            ["lie_to"] = Stamp(lieToUntil), ["reef_until"] = reefUntil == long.MaxValue ? -1.0 : reefUntil, ["in_gale"] = InGale,
            ["lashed"] = double.IsInfinity(lashedUntilDay) ? -1.0 : lashedUntilDay, ["tarred"] = double.IsInfinity(tarredDay) ? -1.0e9 : tarredDay,
            ["base"] = double.IsNaN(seasonBase) ? -1.0e18 : seasonBase, ["paid_year"] = (double)paidYear, ["lost"] = IsLost, ["lost_how"] = LostHow,
        };

        static double Stamp(long hour) => hour == long.MinValue ? -1.0 : hour;
        static long Unstamp(double v) => v < 0.0 ? long.MinValue : (long)v;

        /// <summary>A saved game's boat; a save from before the boat was kept (null) is a sound boat on a wage.</summary>
        public static void Restore(Dictionary<string, object> d)
        {
            Begin(R);
            if (d == null)
                return;
            Hull = Json.Num(d, "hull", 1.0);
            Water = Json.Num(d, "water");
            Cloth = (SailCloth)(int)Json.Num(d, "cloth");
            Set = (SailSet)(int)Json.Num(d, "set");
            Order = (GaleOrder)(int)Json.Num(d, "order");
            Weary = Json.Num(d, "weary");
            OnShares = Json.Bool(d, "shares");
            Bundles = Json.Bool(d, "bundles");
            SpareSpar = Json.Bool(d, "spar");
            Sign = (int)Json.Num(d, "sign");
            now = (long)Json.Num(d, "now");
            signAt = Unstamp(Json.Num(d, "sign_at", -1.0));
            lastGaleHour = Unstamp(Json.Num(d, "last_gale", -1.0));
            lieToUntil = Unstamp(Json.Num(d, "lie_to", -1.0));
            double reef = Json.Num(d, "reef_until", -1.0);
            reefUntil = reef < 0.0 ? long.MaxValue : (long)reef;
            InGale = Json.Bool(d, "in_gale");
            double lashed = Json.Num(d, "lashed", -1.0), tarred = Json.Num(d, "tarred", -1.0e9), b = Json.Num(d, "base", -1.0e18);
            lashedUntilDay = lashed < 0.0 ? double.NegativeInfinity : lashed;
            tarredDay = tarred <= -1.0e8 ? double.NegativeInfinity : tarred;
            seasonBase = b <= -1.0e17 ? double.NaN : b;
            paidYear = (int)Json.Num(d, "paid_year", int.MinValue);
            IsLost = Json.Bool(d, "lost");
            LostHow = Json.Str(d, "lost_how");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            SignCame = null;
            Said = null;
            Lost = null;
            Begin(null);
        }
    }
}
