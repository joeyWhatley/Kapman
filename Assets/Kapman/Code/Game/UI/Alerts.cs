using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>Something that needs the merchant while they're at the map: what, where, and what answering it does.</summary>
    public sealed class Alert
    {
        public string id, kind, text;
        /// <summary>Urgent ones stop the game till answered (raiders, a guard boat, aground); others slow it to 1x.</summary>
        public bool urgent;
        public double day;
        /// <summary>Where (real metres), or NaN.</summary>
        public double x = double.NaN, z = double.NaN;
        /// <summary>What answering it does (usually: back to the ship, where it then happens).</summary>
        public Action answer;
    }

    /// <summary>
    /// ALERTS - the map is where the merchant works while the ship sails (Plans/map_play.md), so what happens at sea mustn't
    /// be missed or slip by while they're at it (the user's choice, 2 October 2026: "slow + alert"). When a ship is made
    /// out, raiders close, a guard boat signals, she runs aground, or something happens aboard, while the map is open:
    /// an alert comes up on the map's top bar. Urgent ones stop the game; the others bring it back to 1x. Clicking one
    /// answers it: the map closes and it happens at sea, as it would have. Ignoring an urgent one isn't possible: going
    /// on answers it.
    /// </summary>
    public static class Alerts
    {
        public const string Hold = "alert";

        static readonly List<Alert> open = new List<Alert>();
        static SimShip retell;                                          // a ship made out: offered again once the map is shut

        public static IReadOnlyList<Alert> Open => open;
        public static event Action Changed;

        // (7 Oct 2026: "at the map table, away from the sea". On the chart the map is the sea: nothing is raised there,
        // what happens at sea happens at once, and Tick clears anything left)
        static bool MapOpen => MapView.AwayFromSea;

        /// <summary>Raises an alert (once: the same id again changes nothing).</summary>
        public static Alert Raise(string id, string kind, string text, bool urgent, Action answer, double x = double.NaN, double z = double.NaN)
        {
            foreach (var a in open)
                if (a.id == id)
                    return a;
            var game = KapmanGame.Current;
            var alert = new Alert { id = id, kind = kind, text = text, urgent = urgent, answer = answer, x = x, z = z, day = game != null ? game.Day : 0.0 };
            open.Add(alert);
            if (urgent)
                GameSpeed.Hold(Hold);
            else
                GameSpeed.Slow();
            Changed?.Invoke();
            return alert;
        }

        /// <summary>Answers an alert: it's done, and what it does is done.</summary>
        public static void Answer(Alert a)
        {
            if (a == null || !open.Remove(a))
                return;
            ReleaseIfNoneUrgent();
            Changed?.Invoke();
            a.answer?.Invoke();
        }

        /// <summary>Lets an alert go without answering it (it's over: the ship sailed out of sight).</summary>
        public static void Drop(string id)
        {
            int i = open.FindIndex(a => a.id == id);
            if (i < 0)
                return;
            open.RemoveAt(i);
            ReleaseIfNoneUrgent();
            Changed?.Invoke();
        }

        /// <summary>The game goes on while an urgent alert waits: it's answered (the first of them).</summary>
        public static void AnswerWaitingUrgent()
        {
            var a = open.Find(x => x.urgent);
            if (a != null)
                Answer(a);
        }

        static void ReleaseIfNoneUrgent()
        {
            if (!open.Exists(x => x.urgent))
                GameSpeed.Release(Hold);
        }

        /// <summary>Back to the ship (the map closes); what's waiting then happens at sea.</summary>
        public static void ToTheShip()
        {
            if (MapOpen)
                MapView.Current.Close();
        }

        // ------------------------------------------------------------------------------------------ what raises them

        /// <summary>Each frame (the game): alerts that are over go; a ship whose meeting was answered is offered again once
        /// the map is shut; nothing stays open once the map is.</summary>
        public static void Tick()
        {
            if (!MapOpen)
            {
                if (retell != null)
                {
                    var s = retell;
                    retell = null;
                    SeaWatch.Retell(s);                                    // (the graphs offer the meeting as usual)
                }
                if (open.Count > 0)
                {
                    open.Clear();                                          // (back at sea: everything happens there)
                    GameSpeed.Release(Hold);
                    Changed?.Invoke();
                }
            }
        }

        /// <summary>A ship made out while the map is open: a trader that could be met is an alert (hail her?).</summary>
        static void MadeOut(SimShip s)
        {
            if (s == null || s.raider || !MapOpen || !Encounter.CanOffer(s, ignoreMap: true))
                return;
            var game = KapmanGame.Current;
            var me = game != null ? MapWorld.ToWorld(game.ShipPosition) : default;   // ported 4 Oct: the level's place in the world
            double km = Math.Sqrt((s.pos.x - me.x) * (s.pos.x - me.x) + (s.pos.z - me.z) * (s.pos.z - me.z)) / 1000.0;
            string label = SeaWatch.Label(s);
            Raise("meet." + s.id, "ship", SeaWatch.MadeOutStart + $"{label}, {km:0} km to the " +
                  $"{SeaWatch.Airt(me.x, me.z, s.pos.x, s.pos.z)}. Go to the ship to hail her.", false,
                  () => { retell = s; ToTheShip(); }, s.pos.x, s.pos.z);
        }

        static void Lost(SimShip s)
        {
            if (s != null)
                Drop("meet." + s.id);
        }

        /// <summary>Raiders coming while the map is open (RaiderEncounter.Look): urgent.</summary>
        public static void RaidersComing(SimShip band, double km, string airt)
        {
            var seen = Known.SightingOf(band.id);
            string men = seen != null && seen.b > 0 ? $", about {seen.b:0} men" : "";
            Raise("raiders." + band.id, "raiders", $"Raiders made out, {km:0} km to the {airt}: {band.bandShips} " +
                  $"{(band.bandShips == 1 ? "ship" : "ships")}{men}. They're coming for you.", true, ToTheShip, band.pos.x, band.pos.z);
        }

        /// <summary>A guard boat signalling while the map is open (Patrols.LookAtSea): urgent.</summary>
        public static void GuardBoat() =>
            Raise("guard", "law", "A guard boat signals you to heave to.", true, ToTheShip);

        /// <summary>Stuck fast while the map is open (Aground): urgent.</summary>
        public static void Aground() =>
            Raise("aground", "aground", "She's aground.", true, ToTheShip);

        /// <summary>Something happening aboard while the map is open (GameEvents): it waits for you, and the game slows.</summary>
        public static void EventWaiting(EventDef e) =>
            Raise("event." + (e != null ? e.id : ""), "event", "Something needs you aboard.", false, ToTheShip);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            open.Clear();
            retell = null;
            Changed = null;
            SeaWatch.MadeOut -= MadeOut;
            SeaWatch.MadeOut += MadeOut;
            SeaWatch.Lost -= Lost;
            SeaWatch.Lost += Lost;
        }
    }
}
