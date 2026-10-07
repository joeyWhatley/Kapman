using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// TONGUES - the tongues you have, each as pips with a word for how well; and the words of other tongues you have
    /// met: those seen often enough to stay English from the start, and how many more you have heard. A word is learned
    /// by hearing it, never bought.
    /// </summary>
    public sealed class TonguesPanel : FramePanel
    {
        public override string Id => "tongues";
        public override string Title => "Tongues";
        public override int Order => 86;

        /// <summary>The tongues the panel looks at beside the ones held (those a tongue held may be kin to).</summary>
        static readonly string[] Listed = { "old_frisian", "old_english", "old_saxon", "old_frankish", "old_norse", "latin", "romance", "irish", "slavic", "baltic", "finnic" };

        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(17, Self.Version), Chest.KnowsLatin);

        /// <summary>How well, as a word: "your own", "well", "half known", "a few words".</summary>
        public static string How(double level, bool held) =>
            level >= 0.95 ? (held ? "Your own, or as good." : "As good as your own.") : level >= 0.7 ? "You follow it well."
            : level >= 0.4 ? "Half known: you get the gist and miss the catch." : "A few words.";

        /// <summary>A level of 0 to 1 as pips of five (anything held at all shows one).</summary>
        public static int PipsOf(double level) => level <= 0.0 ? 0 : System.Math.Max(1, System.Math.Min(5, (int)System.Math.Round(level * 5.0, System.MidpointRounding.AwayFromZero)));

        protected override void Fill(VisualElement body)
        {
            var seen = new HashSet<string>();
            var rows = new List<(string id, double level, bool held)>();
            foreach (var kv in Self.Tongues)
                if (seen.Add(kv.Key) && Self.TongueLevel(kv.Key) > 0.0)
                    rows.Add((kv.Key, Self.TongueLevel(kv.Key), true));
            foreach (string id in Listed)
                if (seen.Add(id) && Self.TongueLevel(id) >= 0.2)
                    rows.Add((id, Self.TongueLevel(id), Self.TongueHeld(id) > 0.0));
            rows.Sort((a, b) => b.level.CompareTo(a.level));
            foreach (var (id, level, held) in rows)
                body.Add(FrameUi.Ln(Self.TongueName(id), How(level, held) + (held || level >= 0.95 ? "" : " Through a tongue near to it."), FrameUi.Pips(PipsOf(level))));
            if (rows.Count == 0)
                body.Add(FrameUi.P("You have no tongue written down yet."));

            var learned = new List<string>();
            int heard = 0;
            foreach (var kv in Self.SeenAll)
            {
                if (kv.Value >= Self.StaysAfter)
                    learned.Add(kv.Key);
                else
                    heard++;
            }
            learned.Sort(System.StringComparer.Ordinal);
            body.Add(FrameUi.H("Words"));
            if (learned.Count > 0)
                body.Add(FrameUi.Text("Learned: " + string.Join(", ", learned) + "."));
            if (heard > 0)
                body.Add(FrameUi.P(heard == 1 ? "One more heard, not yet learned." : heard + " more heard, not yet learned."));
            if (learned.Count == 0 && heard == 0)
                body.Add(FrameUi.P("No words of other tongues met yet."));
            body.Add(FrameUi.P($"A word heard {Self.StaysAfter} times stays English from the start."));
        }
    }
}
