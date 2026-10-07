using System;
using Kapman.Levels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>A small drawn mark for a button (an arrow, a cross, a plus): drawn, not typed, so it needs no font. It takes
    /// the text colour of its place, so the stylesheet sets it.</summary>
    public sealed class FrameGlyph : VisualElement
    {
        public enum Kind { Left, Right, Down, Cross, Plus }

        readonly Kind kind;

        public FrameGlyph(Kind kind)
        {
            this.kind = kind;
            pickingMode = PickingMode.Ignore;
            AddToClassList("frame-glyph");
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
            p.fillColor = colour;
            p.strokeColor = colour;
            p.lineWidth = Mathf.Max(1.5f, s * 0.16f);
            p.lineCap = LineCap.Round;
            Vector2 o = new Vector2(r.x + (r.width - s) * 0.5f, r.y + (r.height - s) * 0.5f);
            Vector2 At(float x, float y) => new Vector2(o.x + x * s, o.y + y * s);
            switch (kind)
            {
                case Kind.Left:
                    Triangle(p, At(0.15f, 0.5f), At(0.8f, 0.1f), At(0.8f, 0.9f));
                    break;
                case Kind.Right:
                    Triangle(p, At(0.85f, 0.5f), At(0.2f, 0.1f), At(0.2f, 0.9f));
                    break;
                case Kind.Down:
                    Triangle(p, At(0.5f, 0.85f), At(0.08f, 0.25f), At(0.92f, 0.25f));
                    break;
                case Kind.Cross:
                    Line(p, At(0.15f, 0.15f), At(0.85f, 0.85f));
                    Line(p, At(0.85f, 0.15f), At(0.15f, 0.85f));
                    break;
                case Kind.Plus:
                    Line(p, At(0.5f, 0.1f), At(0.5f, 0.9f));
                    Line(p, At(0.1f, 0.5f), At(0.9f, 0.5f));
                    break;
            }
        }

        static void Triangle(Painter2D p, Vector2 a, Vector2 b, Vector2 c)
        {
            p.BeginPath();
            p.MoveTo(a);
            p.LineTo(b);
            p.LineTo(c);
            p.ClosePath();
            p.Fill();
        }

        static void Line(Painter2D p, Vector2 a, Vector2 b)
        {
            p.BeginPath();
            p.MoveTo(a);
            p.LineTo(b);
            p.Stroke();
        }
    }

    /// <summary>Drawing a quad whose colour runs from its top to its bottom (the sky, the sea): Painter2D fills flat.</summary>
    static class FrameMesh
    {
        static readonly ushort[] Indices = { 0, 1, 2, 2, 3, 0 };

        public static void Quad(MeshGenerationContext ctx, Rect r, Color top, Color bottom)
        {
            if (r.width <= 0f || r.height <= 0f)
                return;
            var mesh = ctx.Allocate(4, 6);
            mesh.SetNextVertex(new Vertex { position = new Vector3(r.xMin, r.yMin, Vertex.nearZ), tint = top });
            mesh.SetNextVertex(new Vertex { position = new Vector3(r.xMax, r.yMin, Vertex.nearZ), tint = top });
            mesh.SetNextVertex(new Vertex { position = new Vector3(r.xMax, r.yMax, Vertex.nearZ), tint = bottom });
            mesh.SetNextVertex(new Vertex { position = new Vector3(r.xMin, r.yMax, Vertex.nearZ), tint = bottom });
            mesh.SetAllIndices(Indices);
        }
    }

    /// <summary>
    /// A FACE PLACE: where a portrait stands (the player's top right; the speaker's in a floating window). The picture is
    /// the slot's (FrameArt: Art/final, else Art/placeholder; the user makes the art, the game makes up no faces). A
    /// face with no slot yet (a card's speaker: the cards' people are not joined to the places files' people) is a plain
    /// empty shape of a head and shoulders, the same for everyone, that says only "a face goes here".
    /// </summary>
    public sealed class FrameFace : VisualElement
    {
        string who = "", slot = "";
        Texture2D picture;

        public FrameFace(string cls)
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(cls);
            style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(BackgroundSizeType.Cover));
            style.backgroundPositionX = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
            style.backgroundPositionY = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Center));
            style.backgroundRepeat = new StyleBackgroundRepeat(new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat));
            generateVisualContent += Paint;
        }

        /// <summary>Whose place it is: "you" for the player (the portrait of the player's people), else a name or an id
        /// with no picture of its own yet. Set <see cref="Slot"/> instead for a person who has one.</summary>
        public string Who
        {
            get => who;
            set
            {
                string v = value ?? "";
                var people = MapPalette.Current;
                // (the player's own: the face chosen at the start if there is one, else the drawn portrait of the people)
                string want = v == "you" ? (Self.Face.Length > 0 ? Self.Face : FrameArt.Self(people != null ? people.people : "frisian", false)) : FrameArt.Named(v);
                if (who == v && (v != "you" || want == slot))
                    return;
                who = v;
                Slot = want;
            }
        }

        /// <summary>The picture's slot ("portrait/person.dorestad.tollman.png"; "" for none: the empty shape).</summary>
        public string Slot
        {
            get => slot;
            set
            {
                slot = value ?? "";
                picture = FrameArt.Load(slot);
                style.backgroundImage = picture != null ? new StyleBackground(picture) : new StyleBackground(StyleKeyword.None);
                MarkDirtyRepaint();
            }
        }

        void Paint(MeshGenerationContext ctx)
        {
            var r = contentRect;
            if (picture != null || r.width < 4f || r.height < 4f)
                return;
            // no picture: an empty shape in the panel's own tones (no face is made up)
            var p = ctx.painter2D;
            float cx = r.x + r.width * 0.5f, w = r.width, ht = r.height;
            p.fillColor = new Color32(74, 54, 80, 255);
            p.BeginPath();
            p.Arc(new Vector2(cx, r.yMax + w * 0.16f), w * 0.52f, 180f, 360f);
            p.ClosePath();
            p.Fill();
            float head = Mathf.Min(w, ht) * 0.23f;
            var at = new Vector2(cx, r.y + ht * 0.42f);
            p.BeginPath();
            p.Arc(at, head, 0f, 360f);
            p.Fill();
            p.BeginPath();
            p.MoveTo(new Vector2(cx - head * 0.42f, at.y + head * 0.7f));
            p.LineTo(new Vector2(cx + head * 0.42f, at.y + head * 0.7f));
            p.LineTo(new Vector2(cx + head * 0.5f, at.y + head * 1.5f));
            p.LineTo(new Vector2(cx - head * 0.5f, at.y + head * 1.5f));
            p.ClosePath();
            p.Fill();
        }
    }

    /// <summary>
    /// THE SHIP WINDOW, bottom right (the user: "an immersive 3d view of the trader ship"). The 3D scene comes later; this
    /// keeps its place with a plain drawn stand-in that follows what is so: the sky by the sun (the level clock's night
    /// and low sun), the wind in the sail, the waves and the pennant, the land she lies under (a river's bank, a port's
    /// roofs, nothing at sea), her sail furled in port, and fog and rain over it all. Set <see cref="State"/> and it
    /// draws; it moves a little all the time (FrameRoot asks for a repaint some times a second).
    /// </summary>
    public sealed class FrameShipView : VisualElement
    {
        public enum Place { Sea, River, Port }

        /// <summary>What the window shows now.</summary>
        public struct Look
        {
            public Place place;
            /// <summary>0 by day .. 1 at full night; and the warm low sun, 0 .. 1.</summary>
            public float night, golden;
            /// <summary>The sun's height in degrees (under the horizon: below 0), and the hour for where along the sky it stands.</summary>
            public float sunAltitude, hour;
            /// <summary>The wind in m/s, and where it comes from relative to her bow in degrees (+ from starboard).</summary>
            public float wind, windOffBow;
            public float fog, rain;
            /// <summary>Moored or lying to: the sail is down.</summary>
            public bool lying;
        }

        public Look State;

        public FrameShipView()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("frame-ship-view");
            generateVisualContent += Paint;
        }

        static Color Mix(Color a, Color b, float t) => Color.Lerp(a, b, Mathf.Clamp01(t));
        static Color Hex(int rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);

        void Paint(MeshGenerationContext ctx)
        {
            var r = contentRect;
            if (r.width < 8f || r.height < 8f)
                return;
            var s = State;
            float t = Time.realtimeSinceStartup;
            float gale = Mathf.InverseLerp(10f, 18f, s.wind);
            float horizon = r.y + r.height * 0.62f;

            // the sky: day, the low sun's warmth, night; greyer in a gale
            Color top = Mix(Mix(Hex(0x7fb0d6), Hex(0x3a2a55), s.golden * 0.8f), Hex(0x0b0f24), s.night);
            Color low = Mix(Mix(Hex(0xcfe3ea), Hex(0xd98a5a), s.golden), Hex(0x1b2340), s.night);
            top = Mix(top, Mix(Hex(0x3b4250), Hex(0x10131f), s.night), gale * 0.85f);
            low = Mix(low, Mix(Hex(0x7a8590), Hex(0x1b2340), s.night), gale * 0.85f);
            FrameMesh.Quad(ctx, new Rect(r.x, r.y, r.width, horizon - r.y), top, low);

            var p = ctx.painter2D;
            // the sun by day, the moon by night
            float veil = Mathf.Clamp01(1f - gale * 0.7f - s.fog * 0.7f - s.rain * 0.6f);
            if (s.night < 0.85f && s.sunAltitude > -4f)
            {
                float along = Mathf.InverseLerp(4f, 21f, s.hour);
                var sun = new Vector2(r.x + r.width * Mathf.Lerp(0.12f, 0.88f, along), horizon - Mathf.Clamp(s.sunAltitude, -4f, 62f) / 62f * (horizon - r.y) * 0.86f);
                Color c = Mix(Hex(0xf6e7a8), Hex(0xffd08a), s.golden);
                c.a = veil;
                p.fillColor = c;
                p.BeginPath();
                p.Arc(sun, r.height * 0.055f, 0f, 360f);
                p.Fill();
            }
            if (s.night > 0.5f)
            {
                Color c = Hex(0xe8eef0);
                c.a = veil * Mathf.InverseLerp(0.5f, 1f, s.night);
                p.fillColor = c;
                p.BeginPath();
                p.Arc(new Vector2(r.x + r.width * 0.74f, r.y + r.height * 0.16f), r.height * 0.045f, 0f, 360f);
                p.Fill();
                // a few stars
                c.a *= 0.8f;
                p.fillColor = c;
                for (int i = 0; i < 9; i++)
                {
                    float sx = r.x + r.width * (0.07f + 0.105f * i + 0.03f * Mathf.Sin(i * 7.3f));
                    float sy = r.y + r.height * (0.07f + 0.2f * Mathf.Abs(Mathf.Sin(i * 3.1f)));
                    p.BeginPath();
                    p.Arc(new Vector2(sx, sy), 1.3f, 0f, 360f);
                    p.Fill();
                }
            }

            // the land she lies under
            float dark = 1f - s.night * 0.72f;
            if (s.place == Place.River)
                Land(p, r, horizon, 0.2f, Hex(0x2f4a2a) * dark,           // a river's bank: a line of trees
                     0f, 0.6f, 0.08f, 0.3f, 0.16f, 0.55f, 0.27f, 0.2f, 0.38f, 0.5f, 0.52f, 0.25f, 0.63f, 0.55f, 0.74f, 0.15f, 0.86f, 0.5f, 1f, 0.3f);
            else if (s.place == Place.Port)
                Land(p, r, horizon, 0.2f, Hex(0x3a2a22) * dark,           // a port: a hall's gable, low roofs, a post
                     0f, 0.7f, 0.1f, 0.7f, 0.1f, 0.35f, 0.17f, 0.12f, 0.24f, 0.35f, 0.24f, 0.7f, 0.4f, 0.7f, 0.4f, 0.45f, 0.52f, 0.45f, 0.52f, 0.7f,
                     0.66f, 0.7f, 0.69f, 0.3f, 0.72f, 0.7f, 0.84f, 0.7f, 0.84f, 0.5f, 0.93f, 0.5f, 0.93f, 0.7f, 1f, 0.7f);

            // the sea, darker in the deep and at night; the river green-grey
            Color sea = s.place == Place.Sea ? Mix(Hex(0x2f5a86), Hex(0x2a3a52), gale) : Hex(0x4f7f86);
            sea = Mix(sea, Hex(0x141c3a), s.night * 0.9f);
            FrameMesh.Quad(ctx, new Rect(r.x, horizon, r.width, r.yMax - horizon), sea, Mix(sea, Hex(0x10131f), 0.8f));

            // the waves: lines that drift with the wind, livelier as it rises
            float drift = t * (6f + s.wind * 2.2f);
            float lively = s.place == Place.Sea ? 1f : 0.45f;
            p.lineWidth = Mathf.Max(1.5f, r.height * 0.012f);
            p.lineCap = LineCap.Round;
            for (int row = 0; row < 3; row++)
            {
                float y = horizon + (r.yMax - horizon) * (0.2f + 0.3f * row);
                float gap = r.width * (0.16f + 0.05f * row), dash = gap * 0.42f;
                float off = (drift * (0.6f + 0.35f * row)) % gap;
                p.strokeColor = new Color(1f, 1f, 1f, (0.2f - 0.04f * row) * (1f - s.night * 0.5f));
                for (float x = r.x - gap + off; x < r.xMax; x += gap)
                {
                    float lift = Mathf.Sin((x + drift) * 0.05f + row) * r.height * 0.008f * (1f + s.wind * 0.25f) * lively;
                    p.BeginPath();
                    p.MoveTo(new Vector2(Mathf.Max(r.x, x), y + lift));
                    p.LineTo(new Vector2(Mathf.Min(r.xMax, x + dash), y - lift));
                    p.Stroke();
                }
            }

            Boat(p, r, horizon, s, t);

            // the weather over it all
            if (s.rain > 0.05f || gale > 0.4f)
            {
                p.strokeColor = new Color(1f, 1f, 1f, 0.22f);
                p.lineWidth = 1.2f;
                float fall = (t * 420f) % 40f;
                for (float x = r.x - r.height * 0.3f; x < r.xMax; x += 22f)
                    for (float y = r.y - 40f + fall; y < r.yMax; y += 40f)
                    {
                        p.BeginPath();
                        p.MoveTo(new Vector2(x + (y - r.y) * 0.27f, y));
                        p.LineTo(new Vector2(x + (y - r.y) * 0.27f - 4f, y + 15f));
                        p.Stroke();
                    }
            }
            if (s.fog > 0.05f)
                FrameMesh.Quad(ctx, r, new Color(0.86f, 0.88f, 0.9f, 0.5f * s.fog), new Color(0.86f, 0.88f, 0.9f, 0.8f * s.fog));
        }

        /// <summary>A skyline standing on the horizon: its outline as pairs, how far across (0 .. 1) and how far down from
        /// its top (0 its top .. 1 the horizon).</summary>
        static void Land(Painter2D p, Rect r, float horizon, float tall, Color colour, params float[] line)
        {
            colour.a = 1f;
            float h = r.height * tall;
            p.fillColor = colour;
            p.BeginPath();
            p.MoveTo(new Vector2(r.x, horizon + 1f));
            for (int i = 0; i + 1 < line.Length; i += 2)
                p.LineTo(new Vector2(r.x + r.width * line[i], horizon - h + h * line[i + 1]));
            p.LineTo(new Vector2(r.xMax, horizon + 1f));
            p.ClosePath();
            p.Fill();
        }

        /// <summary>Begins a path that has curves in it: with a line of no length to its own first point, and such a path is
        /// closed by hand with a line back to that point, not with ClosePath. (A hull drawn the plain way had a wedge
        /// running from the window's corner to her stern.)</summary>
        internal static void Start(Painter2D p, Vector2 at)
        {
            p.MoveTo(at);
            p.LineTo(at);
        }

        /// <summary>The boat: a clinker hull with its stems, the mast, and the sail set and drawing (or the yard lowered in
        /// port), riding the swell. Drawn on a 108 by 80 grid, as the mockup's.</summary>
        static void Boat(Painter2D p, Rect r, float horizon, Look s, float t)
        {
            float scale = r.width * 0.42f / 108f;
            float pace = s.wind > 12f ? 3.8f : s.place == Place.Sea ? 1.75f : 1.15f;
            float bob = Mathf.Sin(t * pace) * r.height * (s.place == Place.Sea ? 0.014f : 0.006f) * (1f + s.wind * 0.08f);
            float tilt = Mathf.Sin(t * pace + 0.8f) * (1.5f + Mathf.Min(6f, s.wind * 0.3f)) * Mathf.Deg2Rad;
            var origin = new Vector2(r.x + r.width * 0.5f - 54f * scale, horizon + (r.yMax - horizon) * 0.42f - 70f * scale + bob);
            var pivot = new Vector2(54f, 66f);
            float cos = Mathf.Cos(tilt), sin = Mathf.Sin(tilt);
            Vector2 At(float x, float y)
            {
                float dx = x - pivot.x, dy = y - pivot.y;
                return origin + new Vector2(pivot.x + dx * cos - dy * sin, pivot.y + dx * sin + dy * cos) * scale;
            }
            float dark = 1f - s.night * 0.6f;
            Color wood = Hex(0x6e4b31) * dark, line = Hex(0x2a1a2e), cloth = Hex(0xe3cb9a) * dark, stripe = (Color)FrameColours.EdgeNow * Mathf.Lerp(1f, 0.6f, s.night);
            wood.a = cloth.a = stripe.a = 1f;
            // the mast
            p.fillColor = wood;
            Poly(p, At(52f, s.lying ? 12f : 8f), At(56f, s.lying ? 12f : 8f), At(56f, 64f), At(52f, 64f));
            if (s.lying)
                Poly(p, cloth, At(32f, 16f), At(76f, 16f), At(76f, 22f), At(32f, 22f));   // the sail on its yard, lowered
            else
            {
                // the sail: bellied to leeward (the wind on the other side of her), fuller as it freshens
                float belly = Mathf.Clamp(s.wind, 0f, 14f) * 0.55f * (s.windOffBow >= 0f ? -1f : 1f);
                p.fillColor = cloth;
                p.strokeColor = line;
                p.lineWidth = Mathf.Max(1f, 1.5f * scale);
                p.BeginPath();
                p.MoveTo(At(26f, 12f));
                p.LineTo(At(82f, 12f));
                p.QuadraticCurveTo(At(80f + belly, 32f), At(76f, 52f));
                p.LineTo(At(32f, 52f));
                p.QuadraticCurveTo(At(28f + belly, 32f), At(26f, 12f));
                p.Fill();
                p.Stroke();
                p.strokeColor = stripe;
                p.lineWidth = Mathf.Max(1.5f, 3f * scale);
                foreach (float x in new[] { 40f, 54f, 68f })
                {
                    p.BeginPath();
                    Start(p, At(x, 13f));
                    p.QuadraticCurveTo(At(x + belly * 0.9f, 32f), At(x + (x - 54f) * -0.2f, 51f));
                    p.Stroke();
                }
            }
            // the pennant at the masthead: it streams down the wind
            float fly = s.windOffBow >= 0f ? -1f : 1f, reach = 6f + Mathf.Min(12f, s.wind);
            float droop = s.wind < 2f ? 9f : 2f + Mathf.Sin(t * 5f) * 1.5f;
            p.fillColor = FrameColours.Gold;
            Poly(p, At(54f, s.lying ? 12f : 8f), At(54f + fly * reach, (s.lying ? 12f : 8f) + droop), At(54f, (s.lying ? 12f : 8f) + 5f));
            // the hull and her stems
            p.fillColor = wood;
            p.strokeColor = line;
            p.lineWidth = Mathf.Max(1.2f, 2f * scale);
            p.BeginPath();
            Start(p, At(4f, 58f));
            p.QuadraticCurveTo(At(14f, 76f), At(34f, 76f));
            p.LineTo(At(78f, 76f));
            p.QuadraticCurveTo(At(98f, 76f), At(106f, 56f));
            p.LineTo(At(98f, 60f));
            p.QuadraticCurveTo(At(92f, 68f), At(78f, 68f));
            p.LineTo(At(32f, 68f));
            p.QuadraticCurveTo(At(16f, 68f), At(12f, 58f));
            p.LineTo(At(4f, 58f));                                        // (closed by hand: see Start)
            p.Fill();
            p.Stroke();
            p.lineWidth = Mathf.Max(1.5f, 2.5f * scale);
            p.BeginPath();
            p.MoveTo(At(6f, 58f));
            p.LineTo(At(2f, 44f));
            p.Stroke();
            p.BeginPath();
            p.MoveTo(At(104f, 56f));
            p.LineTo(At(107f, 42f));
            p.Stroke();
        }

        static void Poly(Painter2D p, params Vector2[] pts)
        {
            p.BeginPath();
            p.MoveTo(pts[0]);
            for (int i = 1; i < pts.Length; i++)
                p.LineTo(pts[i]);
            p.ClosePath();
            p.Fill();
        }

        static void Poly(Painter2D p, Color fill, params Vector2[] pts)
        {
            p.fillColor = fill;
            Poly(p, pts);
        }
    }
}
