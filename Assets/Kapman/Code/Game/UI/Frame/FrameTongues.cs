using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// A LINE THAT TURNS, as a floating window carries it: the tongue it is in, its pieces ([their words, the English
    /// for them]), the player's skill in that tongue, and small print for under it (the tongue and where the line
    /// stands). It keeps when it began, so a window that is built again (a choice made in it) goes on with the turn
    /// where it was instead of starting it over; and that its pieces have been counted as seen, once.
    /// </summary>
    public sealed class FloatTurn
    {
        public string tongue = "", where = "", english = "";
        public readonly List<(string original, string english)> pairs = new List<(string, string)>();
        public double skill;
        /// <summary>The wall clock when it began to show (under 0: not yet).</summary>
        public float startedAt = -1f;
        public bool counted;
        TurnPlan plan;

        /// <summary>How it turns (reckoned once, from the skill and what has been seen, when it first shows).</summary>
        public TurnPlan Plan => plan ?? (plan = TurnPlan.Make(pairs, skill, Self.Seen));

        /// <summary>A turn for a real line of the tongues file, at the player's skill in its tongue now.</summary>
        public static FloatTurn Of(TongueLine line)
        {
            if (line == null)
                return null;
            var turn = new FloatTurn { tongue = line.tongue, where = line.Where, english = line.english, skill = Self.TongueLevel(line.tongue) };
            turn.pairs.AddRange(line.pairs);
            return turn;
        }

        /// <summary>The small print under it: the tongue, where the line stands, and what the player made of it.</summary>
        public string Note()
        {
            var p = Plan;
            string s = Self.TongueName(tongue) + (where.Length > 0 ? ". " + where + "." : ".");
            if (p.Turning == 0 && p.FromStart == 0)
                return s + " You do not have the tongue. The sense, as it is given you: " + english;
            if (p.Staying > 0)
                return s + " You half follow it: some of it is lost on you.";
            return s;
        }
    }

    /// <summary>
    /// THE TURNING LINE (the user, 7 Oct 2026: "you only see their original language and it does a cool animation
    /// translation as it appears and the foreign language disappears as English takes its place"; "make it change
    /// quicker"). The line stands whole in the speaker's tongue, set apart, and its pieces go out one after another and
    /// come back as English, by the plan (TurnPlan): quickly for a tongue you know, slower and not all of it for one
    /// you half know, not at all for one you do not have. Under it, small, the tongue and where the line stands.
    /// It goes by the wall clock, so it turns while a card holds the game's clock. Every line on screen is moved on by
    /// the frame each frame (<see cref="TickAll"/>).
    /// </summary>
    public sealed class TurningLine : VisualElement
    {
        static readonly List<TurningLine> live = new List<TurningLine>();

        readonly FloatTurn turn;
        readonly Label[] pieces;
        readonly Label note;
        float held = -1f;

        public FloatTurn Turn => turn;
        /// <summary>Seconds since it began to show (or where a test holds it).</summary>
        public float Elapsed => held >= 0f ? held : turn.startedAt < 0f ? 0f : Time.realtimeSinceStartup - turn.startedAt;
        /// <summary>Every piece that turns has turned.</summary>
        public bool Done => Elapsed >= turn.Plan.Duration;

        public TurningLine(FloatTurn turn)
        {
            this.turn = turn;
            AddToClassList("frame-turn");
            var row = new VisualElement();
            row.AddToClassList("frame-turn-row");
            var plan = turn.Plan;
            pieces = new Label[plan.pieces.Length];
            for (int i = 0; i < pieces.Length; i++)
            {
                pieces[i] = new Label();
                pieces[i].AddToClassList("frame-turn-piece");
                row.Add(pieces[i]);
            }
            Add(row);
            note = new Label(turn.Note());
            note.AddToClassList("frame-turn-note");
            Add(note);
            RegisterCallback<AttachToPanelEvent>(_ => Begin());
            RegisterCallback<DetachFromPanelEvent>(_ => live.Remove(this));
            Draw();
        }

        void Begin()
        {
            if (turn.startedAt < 0f)
                turn.startedAt = Time.realtimeSinceStartup;
            if (!turn.counted)
            {
                // (each piece heard is a little learned: seen often enough it stays English from the start)
                turn.counted = true;
                foreach (var p in turn.Plan.pieces)
                    Self.See(p.original);
            }
            if (!live.Contains(this))
                live.Add(this);
            Draw();
        }

        /// <summary>Holds the line as it stands `seconds` into its turn (a test, a picture of it in mid-turn); under 0
        /// lets it run again.</summary>
        public void HoldAt(float seconds)
        {
            held = seconds;
            Draw();
        }

        /// <summary>The line as it reads now.</summary>
        public string Reads => turn.Plan.ReadAt(Elapsed);

        void Draw()
        {
            var plan = turn.Plan;
            float t = Elapsed;
            for (int i = 0; i < pieces.Length; i++)
            {
                float phase = plan.Phase(i, t);
                string text = plan.TextAt(i, t);
                if (pieces[i].text != text)
                    pieces[i].text = text;
                pieces[i].EnableInClassList("eng", phase >= 0.5f);
                pieces[i].EnableInClassList("stays", plan.pieces[i].at < 0f && plan.Turning > 0);
                pieces[i].style.opacity = plan.Opacity(i, t);
            }
            // (what was made of it is said when the turning is over, or at once where nothing turns)
            note.style.visibility = plan.Turning == 0 || t >= plan.Duration ? Visibility.Visible : Visibility.Hidden;
            settled = held < 0f && turn.startedAt >= 0f && t >= plan.Duration + 0.5f;
        }

        /// <summary>It has turned and stands as it will stay: nothing more to draw each frame.</summary>
        bool settled;

        /// <summary>Moves every turning line on screen on (the frame calls it each frame).</summary>
        public static void TickAll()
        {
            for (int i = live.Count - 1; i >= 0; i--)
            {
                var line = live[i];
                if (line.panel == null)
                {
                    live.RemoveAt(i);
                    continue;
                }
                if (!line.settled)
                    line.Draw();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => live.Clear();
    }

    /// <summary>
    /// A LINE OF ENGLISH WITH WORDS OF ANOTHER TONGUE IN IT (layer two of the tongues: "a Dane says heill when he greets
    /// you"). The words run on and wrap as text does; a word of the other tongue is set apart, and resting the pointer
    /// on it gives what it means, how it is said and the tongue it is from (the game's hover card).
    /// </summary>
    public sealed class SprinkledText : VisualElement
    {
        /// <summary>The words of other tongues in it, as they were marked.</summary>
        public readonly List<TongueWord> Marked = new List<TongueWord>();

        public SprinkledText(IEnumerable<TongueRun> runs, string cls = "frame-words")
        {
            AddToClassList(cls);
            if (runs == null)
                return;
            foreach (var run in runs)
            {
                if (run.word != null)
                {
                    var w = new Label(run.text) { tooltip = TongueBook.Current.Tip(run.word) };
                    w.AddToClassList("frame-words-w");
                    w.AddToClassList("word");
                    Add(w);
                    Marked.Add(run.word);
                    continue;
                }
                // plain words, one label a word so the line wraps as text does (what follows a word without a space,
                // a full stop or a comma, stays on it)
                string text = run.text ?? "";
                bool first = childCount > 0 && text.Length > 0 && text[0] != ' ';
                foreach (string part in text.Split(' '))
                {
                    if (part.Length == 0)
                        continue;
                    var l = new Label(part);
                    l.AddToClassList("frame-words-w");
                    l.EnableInClassList("glue", first && !char.IsLetterOrDigit(part[0]));
                    first = false;
                    Add(l);
                }
            }
        }
    }
}
