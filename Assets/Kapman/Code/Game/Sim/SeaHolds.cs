using System;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>What has hold of her at sea: a meeting, a chase or a search that is not over.</summary>
    public enum SeaHold { None, GuardBoat, PortSearch, TraderOffered, TraderClosing, TraderAlongside, RaidersComing, RaidersChasing, RaidersAlongside }

    /// <summary>
    /// THE WATCH ON WHAT HOLDS HER, as plain arithmetic a test can drive: how many game hours of running clock a hold
    /// has stood, and how many real seconds it has stood with nothing on screen to answer it by. Either can end it:
    /// its time is up (<see cref="Verdict.TimeUp"/>), or the talk it waits on is not there (<see cref="Verdict.Lost"/>).
    /// </summary>
    public sealed class HoldWatch
    {
        public enum Verdict { Wait, TimeUp, Lost }

        /// <summary>Real seconds a hold that needs an answer may stand with no talk on screen before it is let go.</summary>
        public const float LostSeconds = 6f;

        public SeaHold Kind { get; private set; }
        /// <summary>Game hours of running clock the hold has stood.</summary>
        public double Hours { get; private set; }
        /// <summary>Real seconds it has stood with nothing to answer.</summary>
        public float Unseen { get; private set; }

        /// <summary>Does this hold wait on a talk the player answers? (Closing with a ship does not: she sails to her.)</summary>
        public static bool NeedsTalk(SeaHold kind) => kind != SeaHold.None && kind != SeaHold.TraderClosing;

        /// <summary>One look: what holds her now, whether its talk is on screen and can be answered, and the game hours
        /// and real seconds since the last look.</summary>
        public Verdict Look(SeaHold now, bool answerable, double gameHours, float realSeconds, double limitHours)
        {
            if (now != Kind)
            {
                Kind = now;
                Hours = 0.0;
                Unseen = 0f;
            }
            if (now == SeaHold.None)
                return Verdict.Wait;
            Hours += Math.Max(0.0, gameHours);
            Unseen = NeedsTalk(now) && !answerable ? Unseen + Math.Max(0f, realSeconds) : 0f;
            if (Unseen >= LostSeconds)
                return Verdict.Lost;
            return Hours >= limitHours ? Verdict.TimeUp : Verdict.Wait;
        }

        public void Reset()
        {
            Kind = SeaHold.None;
            Hours = 0.0;
            Unseen = 0f;
        }
    }

    /// <summary>
    /// NOTHING HOLDS HER FOR EVER (7 Oct 2026). On the chart a ship met, a band of raiders and the king's men each hold
    /// the voyage in some way until they are answered: the planner lays her to for a search, sends her after a ship
    /// she closes with, runs her from a chase. Each is answered in a talk the graphs put up. Two things could leave
    /// her held with no way out: the talk not being there (the graph's flow ended, or something shut its panel), and
    /// the player letting the clock run and never answering. So each hold has a time of its own in game hours of
    /// RUNNING clock (none passes while the clock is held), after which it ends by its plainest way out and says so;
    /// and a hold whose talk is not on screen for a few real seconds is let go at once.
    ///
    /// The times here are a floor under the game, not its design. FOR THE BUILDER OF PRESSING CARDS (rulings 10 and 12
    /// of 7 October): the raider is to hold her where she is while the threat is unanswered (VoyagePlanner, at the
    /// comment "a threat, not yet answered"), with a time and an option the card names. Put that time in
    /// <see cref="LimitHours"/> (RaidersComing, RaidersAlongside) and the option in <see cref="End"/>; the graph's own
    /// wait of twenty game minutes (Encounter - raiders, "no answer: run") is what answers a threat today.
    /// </summary>
    public static class SeaHolds
    {
        static readonly HoldWatch watch = new HoldWatch();
        static double lastHours = -1.0;
        static float lastReal = -1f;

        /// <summary>What has hold of her now, from the three meetings' own stages.</summary>
        public static SeaHold Now
        {
            get
            {
                switch (Patrols.Stage)
                {
                    case PatrolStage.Signal: return SeaHold.GuardBoat;
                    case PatrolStage.Search: return SeaHold.PortSearch;
                }
                switch (RaiderEncounter.Stage)
                {
                    case RaidStage.Threat: return SeaHold.RaidersComing;
                    case RaidStage.Running: return SeaHold.RaidersChasing;
                    case RaidStage.Caught: return SeaHold.RaidersAlongside;
                }
                switch (Encounter.Stage)
                {
                    case EncounterStage.Offered: return SeaHold.TraderOffered;
                    case EncounterStage.Closing: return SeaHold.TraderClosing;
                    case EncounterStage.Alongside: return SeaHold.TraderAlongside;
                }
                return SeaHold.None;
            }
        }

        /// <summary>Game hours of running clock a hold may stand before it is ended for her.</summary>
        public static double LimitHours(SeaHold kind)
        {
            switch (kind)
            {
                case SeaHold.GuardBoat:
                case SeaHold.PortSearch: return Patrols.PressHours;        // (she is held: two hours, then the search)
                case SeaHold.TraderOffered: return 3.0;                     // (the graph lets her go after one)
                case SeaHold.TraderClosing: return 12.0;                    // (she could not come up with her)
                case SeaHold.TraderAlongside: return 24.0;                  // (the clock stands while they talk: a last resort)
                case SeaHold.RaidersComing: return 2.0;                     // (the graph has her run after twenty minutes)
                case SeaHold.RaidersChasing: return 48.0;                   // (a chase ends by night, by the murk, or they give up)
                case SeaHold.RaidersAlongside: return 24.0;                 // (a last resort: the pressing card will name less)
                default: return double.PositiveInfinity;
            }
        }

        /// <summary>Game hours left before what holds her now is ended for her (infinity: nothing holds her).</summary>
        public static double HoursLeft => watch.Kind == SeaHold.None ? double.PositiveInfinity : Math.Max(0.0, LimitHours(watch.Kind) - watch.Hours);

        static bool TalkUp(out TextPanel text)
        {
            text = TextPanel.Current;
            return text != null && text.Open && text.Choice < 0 && text.Key.StartsWith("encounter.", StringComparison.Ordinal);
        }

        /// <summary>Each frame on the chart (the voyage planner).</summary>
        public static void Tick()
        {
            var game = KapmanGame.Current;
            if (game == null)
                return;
            float real = Time.realtimeSinceStartup;
            double hours = game.Hours;
            // (a jump of the clock, a load, is not time she was held: at most an hour a look; a quarter second of the wall)
            double dh = lastHours < 0.0 ? 0.0 : Math.Max(0.0, Math.Min(1.0, hours - lastHours));
            float dr = lastReal < 0f ? 0f : Mathf.Clamp(real - lastReal, 0f, 0.25f);
            lastHours = hours;
            lastReal = real;
            var menu = MainMenu.Current;
            if (menu != null && (menu.Open || menu.OnTitle))
                return;                                                   // (the game is stopped: nothing is counted)
            var kind = Now;
            var verdict = watch.Look(kind, TalkUp(out _), dh, dr, LimitHours(kind));
            if (verdict != HoldWatch.Verdict.Wait)
                End(kind, verdict);
        }

        /// <summary>Ends what holds her by its plainest way out. With its talk on screen the choice is made in the talk,
        /// so the graph goes on as if the player had made it; with none, the hold is ended outright and any flow still
        /// waiting on the panel is given up.</summary>
        public static void End(SeaHold kind, HoldWatch.Verdict why)
        {
            bool shown = TalkUp(out var text);
            bool late = why == HoldWatch.Verdict.TimeUp;
            switch (kind)
            {
                case SeaHold.GuardBoat:
                case SeaHold.PortSearch:
                    if (late)
                        Voyage.Say(kind == SeaHold.GuardBoat ? "No answer was given to the guard boat. She hove to, and they came aboard."
                                                             : "No answer was given to the officers. They searched the ship.");
                    if (shown && text.Key == "encounter.officers")
                        text.Pick(0);                                     // (heave to, or let them search: the graph tells what came of it)
                    else
                    {
                        TextPanel.AbandonWaits();
                        Patrols.Answer(kind == SeaHold.GuardBoat ? "heave_to" : "search");
                    }
                    break;
                case SeaHold.TraderOffered:
                    if (shown && text.Key == "encounter.trader")
                        text.Pick(0);                                     // (hold your course)
                    else
                    {
                        TextPanel.AbandonWaits();
                        Encounter.Part();
                    }
                    break;
                case SeaHold.TraderClosing:
                    Voyage.Say("You could not come up with her. You hold your course.");
                    Encounter.Part();                                     // (the graph's wait sees the meeting over, and lets it go)
                    break;
                case SeaHold.TraderAlongside:
                    TextPanel.AbandonWaits();
                    Encounter.Part();
                    break;
                case SeaHold.RaidersComing:
                    if (shown && text.Key == "encounter.raiders")
                        text.Pick(0);                                     // (run)
                    else
                    {
                        TextPanel.AbandonWaits();
                        RaiderEncounter.Escaped();
                    }
                    break;
                case SeaHold.RaidersChasing:
                    if (shown && late)
                        RaiderEncounter.GiveUp();                         // (the graph's chase sees it ended, and tells it)
                    else
                    {
                        TextPanel.AbandonWaits();
                        RaiderEncounter.Escaped();
                    }
                    break;
                case SeaHold.RaidersAlongside:
                    if (late)
                        Voyage.Say("No answer was given to the raiders. They took what they wanted.");
                    if (!(shown && text.Key == "encounter.raiders.alongside" && text.PickKey("encounter.raiders.submit")))
                    {
                        TextPanel.AbandonWaits();
                        RaiderEncounter.Submit();
                    }
                    break;
            }
            if (!late && kind != SeaHold.None)
                Debug.LogWarning($"Kapman: {kind} held her with no talk on screen to answer it by: let go.");
            watch.Reset();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            watch.Reset();
            lastHours = -1.0;
            lastReal = -1f;
        }
    }
}
