using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE WORD ON THE ITEM, drawn (Design/research_trade_seasons_and_hints.md 6.2): a small mark for how it came (an eye:
    /// seen; an ear: heard; a sun: the season; a seal: the law; a dash: nothing known), a small wedge for the tilt (up:
    /// the price there leans up, worth carrying there; down: it leans down), and the line itself in italics. The marks are
    /// drawn, not typed, so they need no font. The look is in MarketPanel.uss (and MapView.uss for the port's card):
    /// word-line, word-mark, word-tilt, word-text.
    /// </summary>
    public sealed class WordLine : VisualElement
    {
        public readonly GoodWord word;

        public WordLine(GoodWord word)
        {
            this.word = word;
            AddToClassList("word-line");
            AddToClassList(Class(word.mark));
            Add(new Mark(word.mark, 0) { tooltip = Wording.WordText(GameData.Port(word.port), "mark." + Class(word.mark)) });
            if (word.tilt != 0)
                Add(new Mark(WordMark.None, word.tilt));
            var text = new Label(word.Text);
            text.AddToClassList("word-text");
            Add(text);
        }

        /// <summary>The mark's name as a USS class and a wording key: seen, heard, season, law, none.</summary>
        public static string Class(WordMark mark)
        {
            switch (mark)
            {
                case WordMark.Seen: return "seen";
                case WordMark.Heard: return "heard";
                case WordMark.Season: return "season";
                case WordMark.Law: return "law";
                default: return "none";
            }
        }

        /// <summary>One small drawn mark: how the word came, or (tilt not 0) the wedge up or down. Drawn in the element's
        /// own text colour, so the stylesheet sets it.</summary>
        sealed class Mark : VisualElement
        {
            readonly WordMark mark;
            readonly int tilt;

            public Mark(WordMark mark, int tilt)
            {
                this.mark = mark;
                this.tilt = tilt;
                AddToClassList(tilt != 0 ? "word-tilt" : "word-mark");
                AddToClassList(tilt > 0 ? "up" : tilt < 0 ? "down" : Class(mark));
                generateVisualContent += Paint;
            }

            void Paint(MeshGenerationContext ctx)
            {
                var r = contentRect;
                float s = Mathf.Min(r.width, r.height);
                if (s < 2f)
                    return;
                var p = ctx.painter2D;
                var colour = resolvedStyle.color;
                p.strokeColor = colour;
                p.fillColor = colour;
                p.lineWidth = Mathf.Max(1f, s * 0.1f);
                p.lineJoin = LineJoin.Round;
                p.lineCap = LineCap.Round;
                Vector2 o = new Vector2(r.x + (r.width - s) * 0.5f, r.y + (r.height - s) * 0.5f);
                Vector2 At(float x, float y) => new Vector2(o.x + x * s, o.y + y * s);
                if (tilt != 0)
                {
                    // a wedge: up or down
                    float tip = tilt > 0 ? 0.2f : 0.8f, foot = tilt > 0 ? 0.75f : 0.25f;
                    p.BeginPath();
                    p.MoveTo(At(0.5f, tip));
                    p.LineTo(At(0.9f, foot));
                    p.LineTo(At(0.1f, foot));
                    p.ClosePath();
                    p.Fill();
                    return;
                }
                switch (mark)
                {
                    case WordMark.Seen:
                        // an eye: two lids and the pupil
                        p.BeginPath();
                        p.MoveTo(At(0.04f, 0.5f));
                        p.QuadraticCurveTo(At(0.5f, 0.02f), At(0.96f, 0.5f));
                        p.QuadraticCurveTo(At(0.5f, 0.98f), At(0.04f, 0.5f));
                        p.Stroke();
                        p.BeginPath();
                        p.Arc(At(0.5f, 0.5f), s * 0.14f, 0f, 360f);
                        p.Fill();
                        break;
                    case WordMark.Heard:
                        // an ear: the rim open toward the cheek, and the hollow in it
                        p.BeginPath();
                        p.Arc(At(0.5f, 0.42f), s * 0.34f, 150f, 420f);
                        p.LineTo(At(0.52f, 0.94f));
                        p.Stroke();
                        p.BeginPath();
                        p.Arc(At(0.5f, 0.44f), s * 0.13f, 180f, 380f);
                        p.Stroke();
                        break;
                    case WordMark.Season:
                        // the sun: a ring and four rays
                        p.BeginPath();
                        p.Arc(At(0.5f, 0.5f), s * 0.2f, 0f, 360f);
                        p.Stroke();
                        Ray(p, At(0.5f, 0.04f), At(0.5f, 0.16f));
                        Ray(p, At(0.5f, 0.84f), At(0.5f, 0.96f));
                        Ray(p, At(0.04f, 0.5f), At(0.16f, 0.5f));
                        Ray(p, At(0.84f, 0.5f), At(0.96f, 0.5f));
                        break;
                    case WordMark.Law:
                        // a seal: a square with its bar
                        p.BeginPath();
                        p.MoveTo(At(0.16f, 0.16f));
                        p.LineTo(At(0.84f, 0.16f));
                        p.LineTo(At(0.84f, 0.84f));
                        p.LineTo(At(0.16f, 0.84f));
                        p.ClosePath();
                        p.Stroke();
                        Ray(p, At(0.3f, 0.5f), At(0.7f, 0.5f));
                        break;
                    default:
                        Ray(p, At(0.25f, 0.5f), At(0.75f, 0.5f));
                        break;
                }
            }

            static void Ray(Painter2D p, Vector2 a, Vector2 b)
            {
                p.BeginPath();
                p.MoveTo(a);
                p.LineTo(b);
                p.Stroke();
            }
        }
    }
}
