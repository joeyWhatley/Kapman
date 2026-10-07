using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE LOOKOUT - what the merchant's crew see of the world's ships (Plans/ai_fleets.md, phase 2). Every few game
    /// minutes each ship under way or at anchor is checked against how far the lookout sees her (Sight: her sail over
    /// the horizon, the weather's visibility, the night): first a sail SIGHTED, then, closer, MADE OUT (her kind, her
    /// home, where she's bound), and LOST when she's out of sight again. Each is told to the graphs ("On ship sighted",
    /// "On ship made out", "On ship lost") and written in the sea log, as facts.
    /// </summary>
    public class SeaWatch : MonoBehaviour
    {
        public enum Seen : byte { None, Sighted, MadeOut }

        [Tooltip("Metres above the water of the merchant's lookout.")]
        public float lookoutEye = 1.8f;
        [Tooltip("Real km within which a ship in sight is made out: her kind, her home, where she's bound.")]
        public float madeOutKm = 10f;
        [Tooltip("Game minutes between looks round.")]
        public float everyGameMinutes = 10f;
        [Tooltip("The most entries the sea log keeps.")]
        public int logLength = 200;

        public static SeaWatch Current { get; private set; }
        public static event Action<SimShip> Sighted, MadeOut, Lost;
        /// <summary>The sea log, newest last: (date and time, fact).</summary>
        public static readonly List<(string when, string text)> Log = new List<(string, string)>();

        Seen[] seen = new Seen[0];
        double nextLook = double.MinValue;

        void Awake() => Current = this;

        /// <summary>What the lookout has of a ship now.</summary>
        public Seen Of(SimShip s) => s != null && s.index < seen.Length ? seen[s.index] : Seen.None;

        void Update()
        {
            if (GameSpeed.Stepped)
                return;                                                 // (the game's clock steps the lookout: Step)
            var game = KapmanGame.Current;
            if (game == null || game.Hours < nextLook)
                return;
            nextLook = game.Hours + everyGameMinutes / 60.0;
            Look();
        }

        /// <summary>One step of the game's clock (the game minute it ended on): the lookout looks round every
        /// <see cref="everyGameMinutes"/>, on the same minutes at every speed.</summary>
        public void Step(long minute)
        {
            if (minute % Math.Max(1L, (long)Math.Round(everyGameMinutes)) == 0)
                Look();
        }

        void Look()
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            if (world == null || game.ship == null)
                return;
            if (seen.Length != world.ships.Count)
                Array.Resize(ref seen, world.ships.Count);
            // ported 4 Oct: the merchant's place in the world, the air and the clock come from the stage (SimStage), not
            // from FloatingOrigin, LocalWeather and GameClock
            var me = SimStage.ToWorld(game.ShipPosition);
            double haze = SimStage.HazeKm;
            double sun = SimStage.SunElevationAt(me);
            double moon = Sight.MoonLit(SimStage.JulianDay);
            double t = Math.Max(0.0, Math.Min(1.0, game.Hours - (world.Hours - 1.0)));
            foreach (var s in world.ships)
            {
                bool out_ = s.state == ShipState.Sailing || s.state == ShipState.Anchored;
                double x = s.prevPos.x + (s.pos.x - s.prevPos.x) * t, z = s.prevPos.z + (s.pos.z - s.prevPos.z) * t;
                double km = Math.Sqrt((x - me.x) * (x - me.x) + (z - me.z) * (z - me.z)) / 1000.0;
                double range = out_ ? Sight.RangeKm(lookoutEye, s.cls.SeenHeight, haze, sun, moon) : -1.0;
                if (km <= range)                                               // (in range of clear air: now the air between)
                    range = Sight.RangeKm(lookoutEye, s.cls.SeenHeight, SimStage.VisibilityKmTo(new WorldXZ(x, z)), sun, moon);
                var was = seen[s.index];
                if (km <= range)
                {
                    if (was == Seen.None)
                    {
                        seen[s.index] = Seen.Sighted;
                        Known.Sighted(s, false, x, z, game.Day);
                        Write(game, $"The lookout sights a sail, {Off(km, Airt(me.x, me.z, x, z))}.");
                        Sighted?.Invoke(s);
                    }
                    if (seen[s.index] == Seen.Sighted && km <= Math.Min(range, madeOutKm))
                    {
                        seen[s.index] = Seen.MadeOut;
                        var fact = Known.Sighted(s, true, x, z, game.Day);
                        Write(game, MadeOutStart + $"{Label(s)}, {Off(km, Airt(me.x, me.z, x, z))}" + Bound(s, fact) + ".", notice: true);
                        MadeOut?.Invoke(s);
                    }
                }
                else if (was != Seen.None)
                {
                    seen[s.index] = Seen.None;
                    if (out_)
                    {
                        Write(game, $"The lookout has lost sight of {(was == Seen.MadeOut ? Label(s) : "the sail")}.");
                        Lost?.Invoke(s);
                    }
                }
            }
        }

        /// <summary>How the line for a ship made out begins (the frame leaves those lines to the game's own notice).</summary>
        public const string MadeOutStart = "The lookout makes out ";

        void Write(KapmanGame game, string text, bool notice = false)
        {
            Log.Add((game.Date, text));
            if (Log.Count > logLength)
                Log.RemoveAt(0);
            // (not while she lies in a port: the ships of the harbour she is in are no news. 7 Oct 2026: moored at
            // Dorestat the log filled with sails "made out, 0 km to the east")
            if (notice && Port.LyingId() == null)
                Voyage.Say(text);                                        // (sightings and losses go to the log only)
        }

        /// <summary>How far off and which way, as it is said: "6 km to the west"; under a kilometre, "close by".</summary>
        public static string Off(double km, string airt) => km < 0.95 ? "close by" : $"{km:0} km to the {airt}";

        // the peoples a kind of craft may be named for: such a name keeps its capital after "a"
        static readonly string[] ProperFirst = { "Scandinavian", "Gaelic", "English", "Southern", "Norse", "Frisian", "Frankish", "Gislinge", "Pictish", "Baltic" };

        /// <summary>"a Frisian ... merchant ship of Dorestad", "an expanded or double logboat river freighter of Dorestat"
        /// (her kind's name without its description in brackets, with the article that fits it).</summary>
        public static string Label(SimShip s) => $"{A(ShortName(s.cls))} of {GameData.Ports[s.home].name}";

        /// <summary>A kind's name with "a" or "an" before it; its first letter small unless it is a people's name.</summary>
        public static string A(string kind)
        {
            kind = (kind ?? "").Trim();
            if (kind.Length == 0)
                return "a ship";
            bool proper = false;
            foreach (string p in ProperFirst)
                proper |= kind.StartsWith(p, StringComparison.Ordinal);
            if (!proper)
                kind = char.ToLowerInvariant(kind[0]) + kind.Substring(1);
            return ("aeiouAEIOU".IndexOf(kind[0]) >= 0 ? "an " : "a ") + kind;
        }

        /// <summary>A kind of craft's name without the description in brackets ("Coaster ... ship (beach-landing ...)").</summary>
        public static string ShortName(ShipClassDef c)
        {
            string n = c.name ?? c.id;
            int open = n.IndexOf(" (", StringComparison.Ordinal);
            return open > 0 ? n.Substring(0, open).Trim() : n;
        }

        // what the lookout can tell from afar: a band's ships and men (counted by eye), a ship at anchor off a port. Where a
        // ship is bound is known only once she's spoken to (Encounter).
        static string Bound(SimShip s, Fact seen) =>
            s.raider ? $", {s.bandShips} {(s.bandShips == 1 ? "ship" : "ships")}, about {(seen != null && seen.b > 0 ? seen.b : s.men):0} men"
            : s.state == ShipState.Anchored && s.dest >= 0 ? $", at anchor off {GameData.Ports[s.dest].name}" : "";

        /// <summary>Tells the graphs again that a ship is made out (an alert answered: the meeting can be offered now).</summary>
        public static void Retell(SimShip s)
        {
            if (s != null && Current != null && Current.Of(s) == Seen.MadeOut)
                MadeOut?.Invoke(s);
        }

        static readonly string[] Airts = { "north", "north-east", "east", "south-east", "south", "south-west", "west", "north-west" };

        /// <summary>The airt (one of eight) from one point to another.</summary>
        public static string Airt(double fromX, double fromZ, double toX, double toZ)
        {
            double deg = (Math.Atan2(toX - fromX, toZ - fromZ) * 180.0 / Math.PI + 360.0) % 360.0;
            return Airts[(int)Math.Round(deg / 45.0) % 8];
        }

        // (the events aren't cleared here: the graphs' bridge subscribes at the same moment, -= then +=, so it never doubles)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearLog() => Log.Clear();                       // (not "Reset": that's a Unity message on components)
    }
}
