using System;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// YOUR HAND ON THE HELM (the user, 13 Oct 2026: "need a way to control while in 3d mode"; by the picker: "Steer her
    /// yourself in 3D"; this overturns the rulings of 26 Sep and 10 Oct that her bow is not turned by hand). In the 3D
    /// view the keys A and D, or the mouse held to either side of the picture, put the helm over; she answers it at
    /// her class's rate (the rule graph "how fast she answers the helm"), and let go she holds her heading. While your
    /// hand is on it her charted course waits: the planner gives no orders. H, or the swap back to the chart, gives the
    /// helm back: the steersman lays her course again from where she is (the rule graph "does the steersman lay her
    /// course again when the helm is given back"), or she lies to. Run aground by your hand, the helm is let go and
    /// the getting off is as it always was.
    /// THE FLOW is the graph "Voyage - the helm is yours" (on the '--- GAME ---' object): its moments are raised here,
    /// and the code runs the same parts in the same order where no graph listens (the user's rule of 7 Oct 2026).
    /// The turning itself is the chart body's (ChartBody.Move); the keys are the 3D view's (ShipStageHook).
    /// </summary>
    public static class HandHelm
    {
        /// <summary>Set by the graphs' side (Kapman.Graphs): runs the graph's flow for the moment that is on and returns
        /// true; false where no graph listens for it.</summary>
        public static Func<bool> Flow;
        public static int ByGraph { get; private set; }
        public static int ByCode { get; private set; }

        /// <summary>Your hand is on the helm now.</summary>
        public static bool Held { get; private set; }
        /// <summary>The helm over: -1 hard to port, 1 hard to starboard, 0 amidships (she holds her heading).</summary>
        public static float Rudder { get; private set; }

        /// <summary>THE MOMENT THAT IS ON (the graph's event reads these): "taken" (your hand on the helm), "let_go"
        /// (the helm amidships again: she holds her heading), "given_back" (the helm the steersman's again), "aground"
        /// (run aground by your hand).</summary>
        public static string What { get; private set; } = "";
        public static bool GivenBack => What == "given_back";
        /// <summary>Where she is, the port she lies at (null at sea) and whether the air is thick, when the helm is given
        /// back: her course is laid again from there, or not.</summary>
        public static WorldXZ From { get; private set; }
        public static string FromPort { get; private set; }
        public static bool Thick { get; private set; }
        /// <summary>Set by the part <see cref="LayAgainFromHere"/>: her course was laid again, and its length.</summary>
        public static bool Laid { get; private set; }
        public static double Km { get; private set; }
        /// <summary>What the log is told, and its short form (PLACEHOLDER WORDS, the user's to write).</summary>
        public static string Words { get; private set; } = "";
        public static string Short { get; private set; } = "";
        public static string Figure { get; private set; } = "";

        public const string TakenWords = "Your hand is on the helm: she answers it, and her charted course waits.";
        public const string LetGoWords = "";                             // (the helm amidships: nothing is said)
        public const string GivenBackWords = "The helm is the steersman's again: her course is laid again from where she is, {0} km, about {1} days.";
        public const string GivenBackNoCourseWords = "The helm is the steersman's again: with no course set, she lies to.";
        public const string GivenBackThickWords = "The helm is the steersman's again, but in this thick air he cannot see to lay her course: she lies to.";
        public const string GivenBackNoWayWords = "The helm is the steersman's again, but no water he knows leads on from here: she lies to.";
        public const string AgroundWords = "Your hand has run her aground.";
        public const string TakenShort = "Helm taken", GivenBackShort = "Helm given back", AgroundShort = "Aground by your hand";

        /// <summary>May the helm be taken now: at sea (not made fast in a port), not held by a card, not stuck fast.
        /// With no game running (a test without the scene) it may.</summary>
        public static bool CanTake =>
            !Held && Aground.Stage == AgroundStage.None && !EncounterRunner.ShipHeld
            && (KapmanGame.Current == null || (KapmanGame.Current.ship != null && Kapman.Game.Frame.FrameGame.PortHere == null));

        /// <summary>Your hand takes the helm. False where it may not be taken now.</summary>
        public static bool Take()
        {
            if (!CanTake)
                return false;
            Held = true;
            Rudder = 0f;
            Raise("taken");
            return true;
        }

        /// <summary>The helm as your hand has it now (-1..1). Put amidships after being over, the moment "let_go".</summary>
        public static void Steer(float rudder)
        {
            if (!Held)
                return;
            rudder = Mathf.Clamp(rudder, -1f, 1f);
            bool wasOver = Mathf.Abs(Rudder) > 0.001f;
            Rudder = rudder;
            if (wasOver && Mathf.Abs(rudder) <= 0.001f)
                Raise("let_go");
        }

        /// <summary>The helm is the steersman's again: from where she is (`here`, lying at `fromPort` or null), he lays
        /// her course again or she lies to (the part <see cref="LayAgainFromHere"/>). False with no hand on it.</summary>
        public static bool GiveBack(WorldXZ here, string fromPort, bool thick)
        {
            if (!Held)
                return false;
            Held = false;
            Rudder = 0f;
            From = here;
            FromPort = fromPort;
            Thick = thick;
            Raise("given_back");
            return true;
        }

        /// <summary>Stuck fast under your hand: the helm is let go, and the getting off is as it always was (Aground).</summary>
        public static void RanAground()
        {
            if (!Held)
                return;
            Held = false;
            Rudder = 0f;
            Raise("aground");
        }

        static void Raise(string what)
        {
            What = what ?? "";
            Laid = false;
            Km = 0.0;
            Figure = "";
            switch (What)
            {
                case "taken": Words = TakenWords; Short = TakenShort; break;
                case "aground": Words = AgroundWords; Short = AgroundShort; break;
                case "given_back": Words = ""; Short = GivenBackShort; break;             // (the laying sets the words)
                default: Words = LetGoWords; Short = ""; break;
            }
            bool byGraph = false;
            try
            {
                byGraph = Flow != null && Flow();
            }
            catch (Exception e)
            {
                Debug.LogError("Kapman, the helm: the graph's flow for the moment \"" + What + "\" failed (" + e.Message + ").");
                byGraph = true;
            }
            if (byGraph)
            {
                ByGraph++;
                return;
            }
            ByCode++;
            InCode();
        }

        /// <summary>The parts in their order, as the graph has them.</summary>
        static void InCode()
        {
            if (GivenBack)
                LayAgainFromHere();
            Told(Words);
        }

        /// <summary>A PART: the helm given back, the steersman lays her course again from where she is (through her
        /// knots, on to the same end), where the rule says he does and the water lets him; else she lies to. True when
        /// it was laid; the words for the log are set either way.</summary>
        public static bool LayAgainFromHere()
        {
            Laid = false;
            if (!GivenBack)
                return false;
            bool lays = GameRules.LaysAgainOnGiveBack(Course.IsSet, Thick);
            if (!lays || !Course.IsSet)
            {
                Words = !Course.IsSet ? GivenBackNoCourseWords : GivenBackThickWords;
                return false;
            }
            Laid = Adrift.LayAgain(From, FromPort);
            if (!Laid)
            {
                Course.Clear();                                          // (no way on from here: she lies to, as with no course)
                Words = GivenBackNoWayWords;
                return false;
            }
            Km = Course.Km;
            Words = string.Format(GivenBackWords, Km.ToString("0"), Course.Days(Km).ToString("0.#"));
            Figure = Km.ToString("0") + " km";
            return true;
        }

        /// <summary>A PART: the log is told. The moment's own words go in their short form, the whole of them in the
        /// line's box (10 Oct 2026); any other words as they are; none, nothing.</summary>
        public static void Told(string words)
        {
            if (string.IsNullOrEmpty(words))
                return;
            if (Short.Length > 0 && words == Words)
                Voyage.Say(words, Short, Figure, new LogBox().SayEach(words));
            else
                Voyage.Say(words);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            ByGraph = ByCode = 0;
            Held = Laid = Thick = false;
            Rudder = 0f;
            What = Words = Short = Figure = "";
            FromPort = null;
        }

        /// <summary>A new game, or one loaded: no hand on the helm.</summary>
        public static void Clear()
        {
            Held = false;
            Rudder = 0f;
        }
    }
}
