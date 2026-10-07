using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>What the helm is doing beyond holding a course (ship physics 2.5): going about through the wind, wearing
    /// round away from it, or lying in irons (stopped head to wind in the middle of a tack).</summary>
    public enum Manoeuvre { None, Tacking, Wearing, InIrons }

    /// <summary>
    /// THE HELM'S JUDGEMENT (ship physics 2.5 and 3): what course the helmsman is given, from the course the master wants.
    /// Under oars, or with no wind, she is steered straight for it. Under sail she cannot be driven closer than her closest
    /// point to the true wind (the class's closest_to_wind_deg; the design's 60): a course inside it is refused (said
    /// once: "She won't lie closer...", placeholder wording, the user writes the real lines) and she sails full and by,
    /// 2 degrees off her closest, on the board she is on, beating up to it: she goes about when the goal has crossed well
    /// over to the other side of the wind (half her close-hauled angle: short boards near the goal, long ones far off,
    /// never a board under 30 s), or at once when the crew call shoal water ahead. A course that can be laid but lies on
    /// the other side of the wind's eye is a going-about too. Going about is a tack (through the wind) by default, or a
    /// wear (away from it, round through the run, losing ground to leeward but safe) when the wind is above 13 m/s, the sea
    /// above 0.4 of the class's limit, she has under 1 m/s of way, or her last tack failed (the design's default: both,
    /// the crew wearing in a big sea; the thresholds are guesses). A tack keeps the oars in until her head is through the
    /// wind; if she loses her way (under 0.5 m/s) before that she is in irons: after the crew's delay (5 s for a good
    /// crew, 15 for a poor one) they put the oars out and pull her head round; with no oars, or if that fails within 45 s,
    /// she falls off on the old tack and the next going-about is a wear. (On the Gislinge's numbers a tack in a moderate
    /// breeze usually ends in irons and the oars: her steering oar loses its grip as she slows and the stern sweeps
    /// across, which is how square-riggers tacked.) Call <see cref="Steer"/> each step or frame before the steps; it
    /// sets the sim's heading order and whether the oars may turn her.
    /// </summary>
    public sealed class Helm
    {
        /// <summary>The crew's seamanship (0-1; the crew design will give it): how soon they put the oars out in irons.</summary>
        public float skill = 0.7f;
        /// <summary>Above this wind (m/s at 10 m) the crew wear instead of tacking.</summary>
        public float wearAboveWind = 13f;
        /// <summary>Above this share of the class's Hs limit the crew wear instead of tacking.</summary>
        public float wearAboveSea = 0.4f;
        /// <summary>Under this way (m/s) she cannot shoot through the wind: the crew wear.</summary>
        public float tackMinSpeed = 1f;
        /// <summary>The shortest board (s) before she goes about for the goal's sake.</summary>
        public float minBoard = 30f;

        /// <summary>Full and by: how far off her closest point she is sailed close-hauled (degrees).</summary>
        public const float FullAndBy = 2f;
        /// <summary>How far past the wind's line the goal's bearing goes (as a share of the close-hauled angle) before she tacks.</summary>
        public const float TackCone = 0.5f;

        int fromSide, wearPhase;
        float target, boardTime, doingTime, ironsTime;
        bool wearNext, passedEye;

        /// <summary>How long the crew take to put the oars out when she is in irons (s): 5 for a good crew, 15 for a poor one.</summary>
        public float CrewDelay => 15f - 10f * Mathf.Clamp01(skill);

        /// <summary>What she is doing; whether the course wanted lies inside her closest point (she is beating up to it).</summary>
        public Manoeuvre Doing { get; private set; }
        public bool Beating { get; private set; }
        /// <summary>The heading the helmsman was given at the last Steer (degrees from north).</summary>
        public float Steered { get; private set; }
        /// <summary>The helm's latest line (placeholder wording) and how many it has said: show a line when the count moves.</summary>
        public string Line { get; private set; } = "";
        public int Said { get; private set; }
        /// <summary>Counts: tacks begun, wears begun, times in irons.</summary>
        public int Tacks { get; private set; }
        public int Wears { get; private set; }
        public int Irons { get; private set; }

        /// <summary>Forgets the board and any manoeuvre (a new order, or she was moved).</summary>
        public void Reset()
        {
            Doing = Manoeuvre.None;
            Beating = false;
            boardTime = doingTime = ironsTime = 0f;
            wearPhase = 0;
            wearNext = passedEye = false;
        }

        void Say(string line)
        {
            Line = line;
            Said++;
        }

        /// <summary>
        /// Sets the sim's heading order for the course wanted (degrees from north), under sail or not, and whether the oars
        /// may turn her; <paramref name="dt"/> is the game time since the last call; <paramref name="shoalAhead"/> the
        /// crew's call that she is running out of water on this board.
        /// </summary>
        public void Steer(ShipSim sim, float wanted, bool underSail, float dt, bool shoalAhead = false)
        {
            sim.HasHeadingOrder = true;
            Vector3 tw = sim.TrueWind;
            tw.y = 0f;
            if (!underSail || tw.sqrMagnitude < 1f || sim.Class.sailArea <= 0f)
            {
                Doing = Manoeuvre.None;
                Beating = false;
                sim.OarsMayTurn = true;
                sim.HeadingOrder = Steered = wanted;
                return;
            }
            float h = Rot.Heading(sim.State.rotation);
            float wind = Mathf.Atan2(-tw.x, -tw.z) * Mathf.Rad2Deg;          // where the wind comes from
            float theta = sim.Class.closestToWind + FullAndBy;
            float a = Mathf.DeltaAngle(wind, h);                              // her head against the wind: + clockwise of it
            int sideNow = a >= 0f ? 1 : -1;
            float g = Mathf.DeltaAngle(wind, wanted);
            float u = sim.Forces.AheadSpeed;
            boardTime += dt;

            switch (Doing)
            {
                case Manoeuvre.Tacking:
                {
                    doingTime += dt;
                    bool through = sideNow == -fromSide && Mathf.Abs(a) > 3f;
                    passedEye |= through;
                    if (!passedEye && u < 0.5f && doingTime > 2f)
                    {
                        Doing = Manoeuvre.InIrons;
                        ironsTime = 0f;
                        Irons++;
                        Say(sim.Class.oars > 0 ? "In irons: out oars and pull her head round." : "In irons.");
                    }
                    else if ((passedEye && Mathf.Abs(Mathf.DeltaAngle(h, target)) < 15f) || doingTime > 90f)
                        Finish();
                    break;
                }
                case Manoeuvre.InIrons:
                {
                    ironsTime += dt;
                    bool through = sideNow == -fromSide && Mathf.Abs(a) > 3f;
                    if (through)
                    {
                        Doing = Manoeuvre.Tacking;
                        passedEye = true;
                        doingTime = 0f;
                    }
                    else if (ironsTime > 45f || (sim.Class.oars <= 0 && ironsTime > 15f))
                    {
                        // she will not come: fall off on the old tack, and wear the next time
                        target = wind + fromSide * theta;
                        wearNext = true;
                        Finish();
                        Say("She won't come about: back on the old tack.");
                    }
                    break;
                }
                case Manoeuvre.Wearing:
                {
                    doingTime += dt;
                    if (wearPhase == 0 && Mathf.Abs(Mathf.DeltaAngle(h, wind + 180f)) < 25f)
                        wearPhase = 1;
                    if ((wearPhase == 1 && Mathf.Abs(Mathf.DeltaAngle(h, target)) < 15f) || doingTime > 120f)
                        Finish();
                    break;
                }
            }

            if (Doing == Manoeuvre.None)
            {
                if (Mathf.Abs(g) >= theta)
                {
                    // she can lay it: straight there, going about first if it lies across the wind's eye
                    Beating = false;
                    if (CrossesWind(h, wanted, wind))
                        GoAbout(sim, wanted, sideNow, wind);
                    else
                        target = wanted;
                }
                else
                {
                    if (!Beating)
                    {
                        Beating = true;
                        Say($"She won't lie closer than {sim.Class.closestToWind:0} degrees to the wind: beating up to it.");
                    }
                    int s = sideNow;
                    if (Mathf.Abs(a) < 0.5f * theta && Mathf.Abs(g) > 1f)
                        s = g >= 0f ? 1 : -1;                                  // head to wind now: bear away towards the goal's side
                    target = wind + s * theta;
                    bool crossed = s * g < -TackCone * theta;
                    if ((crossed && boardTime > minBoard) || (shoalAhead && boardTime > 5f))
                        GoAbout(sim, wind - s * theta, s, wind);
                }
            }

            float order = Doing == Manoeuvre.Wearing && wearPhase == 0 ? wind + 180f : target;
            sim.HeadingOrder = Steered = Mathf.Repeat(order, 360f);
            sim.OarsMayTurn = Doing == Manoeuvre.Tacking ? passedEye
                            : Doing == Manoeuvre.InIrons ? ironsTime > CrewDelay
                            : true;
        }

        /// <summary>Does the shortest turn from one heading to another pass through the wind's eye?</summary>
        static bool CrossesWind(float from, float to, float wind)
        {
            float turn = Mathf.DeltaAngle(from, to);
            float eye = Mathf.DeltaAngle(from, wind);
            return Mathf.Abs(turn) > 5f && Mathf.Sign(turn) == Mathf.Sign(eye) && Mathf.Abs(eye) < Mathf.Abs(turn);
        }

        void GoAbout(ShipSim sim, float newHeading, int side, float wind)
        {
            Vector3 tw = sim.TrueWind;
            tw.y = 0f;
            bool wear = wearNext || tw.magnitude > wearAboveWind || sim.SeaHs > wearAboveSea * sim.Class.hsLimit
                        || sim.Forces.AheadSpeed < tackMinSpeed;
            target = newHeading;
            fromSide = side;
            doingTime = 0f;
            boardTime = 0f;
            passedEye = false;
            if (wear)
            {
                Doing = Manoeuvre.Wearing;
                wearPhase = 0;
                wearNext = false;
                Wears++;
                Say("Wearing round.");
            }
            else
            {
                Doing = Manoeuvre.Tacking;
                Tacks++;
                Say("Going about.");
            }
        }

        void Finish()
        {
            Doing = Manoeuvre.None;
            boardTime = 0f;
            wearPhase = 0;
        }
    }
}
