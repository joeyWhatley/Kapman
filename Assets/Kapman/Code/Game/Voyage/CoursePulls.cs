using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE COURSE IS PULLED ON THE CHART: A FLOW (the user, 13 Oct 2026: "you should want to be able to drag and correct
    /// the course on the map with your mouse"; by the picker: pull the line anywhere, and the course goes through where
    /// you let go and on to the same end; by the lanes through your point, a point let go on land to the nearest water;
    /// a point may be let go in the dark, as click and sail). The chart (MapView) raises a moment when a knot is let go
    /// or taken off; the graph "Chart - the course is pulled" (on the '--- GAME ---' object) runs the parts, and the code
    /// runs the same parts in the same order where no graph listens (the user's rule of 7 Oct 2026: flows are graphs).
    /// The parts: the point is taken to the water (<see cref="TakeToWater"/>), her course is laid again through her
    /// knots (<see cref="LayThroughKnots"/>), the log is told (<see cref="Told"/>). Nothing here waits.
    /// </summary>
    public static class CoursePulls
    {
        /// <summary>Set by the graphs' side (Kapman.Graphs): runs the graph's flow for the moment that is on and returns
        /// true; false where no graph listens for it.</summary>
        public static Func<bool> Flow;
        /// <summary>How many moments a graph has run since the game began, and how many the code ran itself.</summary>
        public static int ByGraph { get; private set; }
        public static int ByCode { get; private set; }

        /// <summary>THE MOMENT THAT IS ON (the graph's event reads these). What: "pulled" (a new knot, let go), "moved"
        /// (a knot pulled again), "off" (a knot taken off, with a right click).</summary>
        public static string What { get; private set; } = "";
        /// <summary>Where the knot was let go (real metres, the world frame). The part <see cref="TakeToWater"/> moves
        /// it to the water where it lay on land.</summary>
        public static WorldXZ Where { get; private set; }
        /// <summary>Which knot, counted from the first still ahead of her; -1 for a new one.</summary>
        public static int Knot { get; private set; } = -1;
        /// <summary>For a new knot: the point of her line the press was nearest. The knot goes among the others by it.</summary>
        public static int Leg { get; private set; }
        /// <summary>The point let go lies on dry land or the sands (the world bake says).</summary>
        public static bool OnLand { get; private set; }
        /// <summary>Where she is, and the port she lies at (null at sea): the course is laid again from there.</summary>
        public static WorldXZ From { get; private set; }
        public static string FromPort { get; private set; }
        /// <summary>Set by the part <see cref="LayThroughKnots"/>: the course was laid again, and its length.</summary>
        public static bool Laid { get; private set; }
        public static double Km { get; private set; }
        /// <summary>What the log is told at the end, and its short form (PLACEHOLDER WORDS, the user's to write).</summary>
        public static string Words { get; private set; } = "";
        public static string Short { get; private set; } = "";
        public static string Figure { get; private set; } = "";

        public const string PulledWords = "Her course is laid again through your knot: {0} km, about {1} days.";
        public const string MovedWords = "Your knot is moved, and her course laid again through it: {0} km, about {1} days.";
        public const string OffWords = "Your knot is taken off, and her course laid again: {0} km, about {1} days.";
        public const string ToWaterWords = " The point you let go of lay on the land: the knot is on the water nearest it.";
        public const string NoWayWords = "No water she can sail leads through that point: her course stands as it was.";
        public const string NoCourseWords = "She has no course to pull.";
        public const string PulledShort = "Course pulled", MovedShort = "Knot moved", OffShort = "Knot taken off", NoWayShort = "No way through there";

        /// <summary>A knot is let go, pulled again, or taken off: the graph's flow where one listens, else the code's.</summary>
        public static void Raise(string what, WorldXZ where, int knot, int leg, WorldXZ from, string fromPort)
        {
            What = what ?? "";
            Where = where;
            Knot = knot;
            Leg = leg;
            From = from;
            FromPort = fromPort;
            OnLand = What != "off" && Course.KnowsWater && !Course.IsWater(where.x, where.z);
            Laid = false;
            Km = 0.0;
            Words = Short = Figure = "";
            toWater = false;
            bool byGraph = false;
            try
            {
                byGraph = Flow != null && Flow();
            }
            catch (Exception e)
            {
                Debug.LogError("Kapman, the chart: the graph's flow for the course pulled (\"" + What + "\") failed (" + e.Message + ").");
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
            if (OnLand)
                TakeToWater();
            LayThroughKnots();
            Told(Words);
        }

        static bool toWater;

        /// <summary>A PART: the point let go lay on land, so the knot goes to the water nearest it (within
        /// <see cref="Course.ToWaterM"/>; where none is found it stays, and the lanes bring her as near as they come).</summary>
        public static void TakeToWater()
        {
            if (!OnLand)
                return;
            if (Course.NearestWater(Where, Course.ToWaterM, out var water))
            {
                Where = water;
                toWater = true;
            }
        }

        /// <summary>A PART: her course is laid again from where she is, through her knots with this one let go, moved or
        /// taken off among them, and on to the same end. True when it was; the words for the log are set either way.</summary>
        public static bool LayThroughKnots()
        {
            Laid = false;
            if (!Course.IsSet)
            {
                Words = NoCourseWords;
                Short = NoWayShort;
                return false;
            }
            var knots = new List<WorldXZ>(Course.Knots);
            if (What == "off")
            {
                if (Knot >= 0 && Knot < knots.Count)
                    knots.RemoveAt(Knot);
            }
            else if (What == "moved" && Knot >= 0 && Knot < knots.Count)
                knots[Knot] = Where;
            else
            {
                // (a new knot goes among the others by the point of the line it was pulled from)
                int at = knots.Count;
                for (int i = 0; i < Course.KnotAt.Count && i < knots.Count; i++)
                    if (Course.KnotAt[i] > Leg)
                    {
                        at = i;
                        break;
                    }
                knots.Insert(at, Where);
            }
            Laid = Course.SetThrough(knots, From, FromPort);
            if (!Laid)
            {
                Words = NoWayWords;
                Short = NoWayShort;
                Figure = "";
                return false;
            }
            Km = Course.Km;
            string days = Course.Days(Km).ToString("0.#");
            Words = string.Format(What == "off" ? OffWords : What == "moved" ? MovedWords : PulledWords, Km.ToString("0"), days) + (toWater ? ToWaterWords : "");
            Short = What == "off" ? OffShort : What == "moved" ? MovedShort : PulledShort;
            Figure = Km.ToString("0") + " km";
            return true;
        }

        /// <summary>A PART: the log is told. The moment's own words go in their short form, the whole of them in the
        /// line's box (10 Oct 2026); any other words as they are.</summary>
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
            What = Words = Short = Figure = "";
            Knot = -1;
            Laid = OnLand = toWater = false;
        }
    }
}
