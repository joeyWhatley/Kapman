using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// The small drawn signs of the function buttons (the mockup's, on a 24 by 24 grid): a boat under sail, a clock, a
    /// rule, an eye, a lead, a horn, a bed, a cross with a flame. Drawn in lines of the element's own text colour.
    /// In a UI file (UI Builder: Library, Project, Kapman.Game.Frame): FrameIcon with kind="CastOff".
    /// </summary>
    [UxmlElement]
    public sealed partial class FrameIcon : VisualElement
    {
        public enum Kind { CastOff, Wait, Measure, LookOut, Sound, Hail, Rest, Offer, Voyage, Crew, Hold, Trade, Alehouse, Wares, Journey, Errands, Book }

        Kind kind;

        /// <summary>Which sign it is (a button of the frame does what its sign says).</summary>
        [UxmlAttribute("kind")]
        public Kind Sign
        {
            get => kind;
            set
            {
                kind = value;
                MarkDirtyRepaint();
            }
        }

        public FrameIcon() : this(Kind.CastOff)
        {
        }

        public FrameIcon(Kind kind)
        {
            this.kind = kind;
            pickingMode = PickingMode.Ignore;
            AddToClassList("frame-icon");
            generateVisualContent += Paint;
        }

        void Paint(MeshGenerationContext ctx)
        {
            var r = contentRect;
            float s = Mathf.Min(r.width, r.height);
            if (s < 4f)
                return;
            var p = ctx.painter2D;
            p.strokeColor = resolvedStyle.color;
            p.lineWidth = Mathf.Max(1.4f, s * 1.7f / 24f);
            p.lineCap = LineCap.Round;
            p.lineJoin = LineJoin.Round;
            Vector2 o = new Vector2(r.x + (r.width - s) * 0.5f, r.y + (r.height - s) * 0.5f);
            float k = s / 24f;
            Vector2 At(float x, float y) => new Vector2(o.x + x * k, o.y + y * k);
            void Path(bool close, params float[] xy)
            {
                p.BeginPath();
                p.MoveTo(At(xy[0], xy[1]));
                for (int i = 2; i + 1 < xy.Length; i += 2)
                    p.LineTo(At(xy[i], xy[i + 1]));
                if (close)
                    p.ClosePath();
                p.Stroke();
            }
            void Circle(float x, float y, float radius)
            {
                p.BeginPath();
                p.Arc(At(x, y), radius * k, 0f, 360f);
                p.Stroke();
            }
            switch (kind)
            {
                case Kind.Voyage:                                         // a course laid from mark to mark
                    Path(false, 4, 19, 9, 12, 15, 14, 20, 5);
                    Circle(4, 19, 1.6f);
                    Circle(20, 5, 1.6f);
                    break;
                case Kind.Crew:                                           // two men, head and shoulders
                    Circle(8, 8, 3);
                    Circle(16.5f, 9, 2.6f);
                    Path(false, 2.5f, 20, 4, 14.5f, 12, 14.5f, 13.5f, 20);
                    Path(false, 14.5f, 15.5f, 20, 15.5f, 21.5f, 20);
                    break;
                case Kind.Hold:                                           // a chest with its lid line and its hasp
                    Path(true, 4, 8, 20, 8, 20, 20, 4, 20);
                    Path(false, 4, 12.5f, 20, 12.5f);
                    Path(false, 12, 11, 12, 15);
                    break;
                case Kind.Trade:                                          // a balance
                    Path(false, 12, 4, 12, 20);
                    Path(false, 5, 7, 19, 7);
                    Path(true, 5, 7, 2.5f, 14, 7.5f, 14);
                    Path(true, 19, 7, 16.5f, 14, 21.5f, 14);
                    Path(false, 8, 20, 16, 20);
                    break;
                case Kind.Alehouse:                                       // a cup with its handle
                    Path(true, 6, 7, 15, 7, 14, 20, 7, 20);
                    Path(false, 15, 10, 19, 10, 19, 16, 14.5f, 16);
                    Path(false, 7, 4, 8, 5.5f, 10, 4, 12, 5.5f, 14, 4);
                    break;
                case Kind.Wares:                                          // a tied sack
                    Path(true, 6, 9, 18, 9, 20, 20, 4, 20);
                    Path(false, 9, 9, 12, 4, 15, 9);
                    break;
                case Kind.Journey:                                        // a written leaf
                    Path(true, 5, 4, 19, 4, 19, 20, 5, 20);
                    Path(false, 8, 8, 16, 8);
                    Path(false, 8, 12, 16, 12);
                    Path(false, 8, 16, 13, 16);
                    break;
                case Kind.Errands:                                        // a pennon on its staff
                    Path(false, 6, 21, 6, 3);
                    Path(true, 6, 4, 19, 7.5f, 6, 11);
                    break;
                case Kind.Book:                                           // an open book
                    Path(false, 12, 6, 12, 20);
                    Path(false, 12, 6, 4, 4, 4, 18, 12, 20);
                    Path(false, 12, 6, 20, 4, 20, 18, 12, 20);
                    break;
                case Kind.CastOff:
                    Path(true, 3, 15, 21, 15, 18, 20, 6, 20);           // the hull
                    Path(false, 12, 3, 12, 15);                           // the mast
                    Path(false, 12, 4, 18, 13, 12, 13);                   // the sail
                    break;
                case Kind.Wait:
                    Circle(12, 12, 9);
                    Path(false, 12, 6, 12, 12, 16, 15);
                    break;
                case Kind.Measure:
                    Path(true, 3, 17, 17, 3, 21, 7, 7, 21);
                    Path(false, 8, 12, 10, 14);
                    Path(false, 11, 9, 13, 11);
                    Path(false, 14, 6, 16, 8);
                    break;
                case Kind.LookOut:
                    p.BeginPath();
                    FrameShipView.Start(p, At(2, 12));
                    p.QuadraticCurveTo(At(12, 1), At(22, 12));
                    p.QuadraticCurveTo(At(12, 23), At(2, 12));
                    p.Stroke();
                    Circle(12, 12, 3);
                    break;
                case Kind.Sound:
                    Path(false, 12, 2, 12, 15);
                    Path(true, 9, 15, 15, 15, 14, 21, 10, 21);
                    break;
                case Kind.Hail:
                    Path(true, 4, 9, 4, 15, 8, 15, 14, 19, 14, 5, 8, 9);
                    p.BeginPath();
                    FrameShipView.Start(p, At(17, 9));
                    p.QuadraticCurveTo(At(20, 12), At(17, 15));
                    p.Stroke();
                    break;
                case Kind.Rest:
                    Path(false, 3, 18, 21, 18);
                    p.BeginPath();
                    p.MoveTo(At(5, 18));
                    p.LineTo(At(5, 12));
                    p.LineTo(At(14, 12));
                    p.QuadraticCurveTo(At(18, 12), At(18, 16));
                    p.LineTo(At(18, 18));
                    p.Stroke();
                    Circle(8, 9, 2);
                    break;
                case Kind.Offer:
                    Path(false, 12, 21, 12, 8);
                    Path(false, 7, 12, 17, 12);
                    p.BeginPath();
                    FrameShipView.Start(p, At(12, 3));
                    p.QuadraticCurveTo(At(15, 5.5f), At(12, 8));
                    p.QuadraticCurveTo(At(9, 5.5f), At(12, 3));
                    p.Stroke();
                    break;
            }
        }
    }
}
