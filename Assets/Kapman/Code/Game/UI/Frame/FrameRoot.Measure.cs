using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE MEASURE (the user, 7 Oct 2026: "a measuring thing for the map and it's in days"). A tool of the frame: with it
    /// in hand ([D], or its button), click two places on the chart and it draws the way between them by water (the
    /// lanes, as her own course would go: FramePassage.Between) and says the days it would take her, low and high. A
    /// third click begins again; a click on a port's mark takes the port's berth; dragging still moves the chart; Esc
    /// puts it down. It also draws a course a window wants shown (the Voyage window's picked port): <see cref="ShowPlanned"/>.
    /// With the measure down, a click on a port's mark picks that port in the Voyage window.
    /// </summary>
    public partial class FrameRoot
    {
        VisualElement ruleBox;
        Button ruleButton;
        Label ruleText, ruleFigure;
        ChartOverlay overlay;
        MapView overlayMap;
        readonly List<WorldXZ> measurePoints = new List<WorldXZ>();
        readonly List<WorldXZ> measureLine = new List<WorldXZ>();
        readonly List<WorldXZ> plannedLine = new List<WorldXZ>();
        double measureKm;
        bool measureFailed;

        /// <summary>The measure is in hand.</summary>
        public bool Measuring { get; private set; }
        /// <summary>The way last measured (empty until two places are clicked), its real km, and its days in words.</summary>
        public IReadOnlyList<WorldXZ> MeasuredLine => measureLine;
        public double MeasuredKm => measureKm;
        public string MeasuredWords => ruleFigure != null ? ruleFigure.text : "";

        /// <summary>Takes the measure up, or puts it down (what was measured goes with it).</summary>
        public void SetMeasuring(bool on)
        {
            if (Measuring == on)
                return;
            Measuring = on;
            measurePoints.Clear();
            measureLine.Clear();
            measureKm = 0.0;
            measureFailed = false;
            RuleWords();
            overlay?.MarkDirtyRepaint();
        }

        /// <summary>A place clicked with the measure in hand (real metres): the first, then the second; a third begins again.</summary>
        public void MeasureAt(WorldXZ place)
        {
            if (!Measuring)
                return;
            if (measurePoints.Count >= 2)
            {
                measurePoints.Clear();
                measureLine.Clear();
                measureFailed = false;
            }
            measurePoints.Add(place);
            if (measurePoints.Count == 2)
                measureFailed = !FramePassage.Between(measurePoints[0], measurePoints[1], measureLine, out measureKm);
            RuleWords();
            overlay?.MarkDirtyRepaint();
        }

        /// <summary>How many points the course drawn for a window has (0: none is drawn).</summary>
        public int PlannedCount => plannedLine.Count;

        /// <summary>A course drawn on the chart for a window (null or empty: none).</summary>
        public void ShowPlanned(IList<WorldXZ> line)
        {
            plannedLine.Clear();
            if (line != null)
                plannedLine.AddRange(line);
            overlay?.MarkDirtyRepaint();
        }

        void RuleWords()
        {
            if (ruleText == null)
                return;
            ruleButton.EnableInClassList("on", Measuring);
            ruleFigure.text = "";
            if (!Measuring)
                ruleText.text = "";
            else if (measurePoints.Count == 0)
                ruleText.text = "Click two places on the chart.";
            else if (measurePoints.Count == 1)
                ruleText.text = "And the second place.";
            else if (measureFailed || measureLine.Count < 2)
                ruleText.text = "No way between them by water that she can sail.";
            else
            {
                ruleFigure.text = "About " + FramePassage.DaysWords(measureKm);
                ruleText.text = $"for your boat, by the wind. {measureKm:0} km by water.";
            }
            ruleText.style.display = ruleText.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            ruleFigure.style.display = ruleFigure.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            ruleBox.style.display = Measuring ? DisplayStyle.Flex : DisplayStyle.None;   // (in hand: its words over the chart; down: out of the way)
        }

        // ------------------------------------------------------------------ the frame's hooks

        partial void MeasureTop(float y)
        {
            if (ruleBox != null)
                ruleBox.style.top = y;
        }

        partial void MeasureBuild()
        {
            // (it stands in the layout file, Frame.uxml, under "rule")
            ruleBox = Part<VisualElement>("rule");
            ruleButton = Part<Button>("rule-button");
            ruleButton.clicked += () => SetMeasuring(!Measuring);
            ruleFigure = Part<Label>("rule-figure");
            ruleText = Part<Label>("rule-text");
            Measuring = false;
            measurePoints.Clear();
            measureLine.Clear();
            RuleWords();
        }

        partial void MeasureDisable()
        {
            Unhook();
        }

        void Unhook()
        {
            if (overlayMap != null)
            {
                overlayMap.Clicked -= ChartClicked;
                overlayMap.PortClicked -= PortClicked;
            }
            overlay?.RemoveFromHierarchy();
            overlay = null;
            overlayMap = null;
        }

        partial void MeasureUpdate()
        {
            var map = MapView.Current;
            if (map != overlayMap || (overlay != null && overlay.parent == null))
            {
                // the frame's own drawing on the chart: over the map's tiles and marks, in the map's own place and scale
                Unhook();
                if (map != null && map.ChartOverlays != null)
                {
                    overlayMap = map;
                    overlay = new ChartOverlay(this, map);
                    map.ChartOverlays.Add(overlay);
                    map.Clicked += ChartClicked;
                    map.PortClicked += PortClicked;
                }
            }
            if (overlay != null && (measurePoints.Count > 0 || plannedLine.Count > 0))
                overlay.MarkDirtyRepaint();                               // (the chart moves under it)
        }

        partial void MeasureKeys(Keyboard kb, ref bool taken)
        {
            if (kb.dKey.wasPressedThisFrame)
            {
                taken = true;
                SetMeasuring(!Measuring);
            }
            else if (Measuring && kb.escapeKey.wasPressedThisFrame)
            {
                taken = true;
                SetMeasuring(false);
            }
        }

        void ChartClicked(WorldXZ place)
        {
            if (Measuring)
                MeasureAt(place);
        }

        void PortClicked(string id)
        {
            var def = GameData.Port(id);
            if (def == null)
                return;
            if (Measuring)
            {
                MeasureAt(Course.BerthOf(def));
                return;
            }
            // the measure is down: a port clicked on the chart is picked in the Voyage window
            if (FramePanels.Get("voyage") is VoyagePanel voyage)
            {
                Show("voyage");
                voyage.Pick(id);
            }
        }

        /// <summary>The frame's drawing on the chart: a planned course as gold dashes, and the measure (its two places and
        /// the way between them).</summary>
        sealed class ChartOverlay : VisualElement
        {
            readonly FrameRoot frame;
            readonly MapView map;

            public ChartOverlay(FrameRoot frame, MapView map)
            {
                this.frame = frame;
                this.map = map;
                pickingMode = PickingMode.Ignore;
                style.position = Position.Absolute;
                style.left = style.top = style.right = style.bottom = 0;
                generateVisualContent += Paint;
            }

            void Paint(MeshGenerationContext ctx)
            {
                var p = ctx.painter2D;
                p.lineJoin = LineJoin.Round;
                p.lineCap = LineCap.Round;
                Color gold = FrameColours.Gold, ink = new Color(0.08f, 0.05f, 0.1f, 0.9f);
                if (frame.plannedLine.Count > 1)
                {
                    Dashes(p, frame.plannedLine, ink, 5f, 12f, 8f);
                    Dashes(p, frame.plannedLine, gold, 2.5f, 12f, 8f);
                    Ring(p, map.ChartPx(frame.plannedLine[frame.plannedLine.Count - 1]), 7f, gold, ink);
                }
                if (frame.measureLine.Count > 1 && !frame.measureFailed)
                {
                    Line(p, frame.measureLine, ink, 6f);
                    Line(p, frame.measureLine, gold, 3f);
                }
                foreach (var place in frame.measurePoints)
                    Ring(p, map.ChartPx(place), 6f, gold, ink);
            }

            void Line(Painter2D p, List<WorldXZ> line, Color colour, float width)
            {
                p.strokeColor = colour;
                p.lineWidth = width;
                p.BeginPath();
                p.MoveTo(map.ChartPx(line[0]));
                for (int i = 1; i < line.Count; i++)
                    p.LineTo(map.ChartPx(line[i]));
                p.Stroke();
            }

            /// <summary>A line in dashes (the painter draws none of its own): so many pixels on, so many off, along it.</summary>
            void Dashes(Painter2D p, List<WorldXZ> line, Color colour, float width, float on, float off)
            {
                p.strokeColor = colour;
                p.lineWidth = width;
                float left = on;
                bool drawing = true;
                var at = map.ChartPx(line[0]);
                p.BeginPath();
                p.MoveTo(at);
                int budget = 4000;                                        // (far zoomed in, a long course is many dashes: enough is enough)
                for (int i = 1; i < line.Count && budget > 0; i++)
                {
                    var to = map.ChartPx(line[i]);
                    float seg = Vector2.Distance(at, to);
                    while (seg > 0.01f && budget-- > 0)
                    {
                        float step = Mathf.Min(seg, left);
                        at = Vector2.MoveTowards(at, to, step);
                        if (drawing)
                            p.LineTo(at);
                        else
                            p.MoveTo(at);
                        seg -= step;
                        left -= step;
                        if (left <= 0.01f)
                        {
                            drawing = !drawing;
                            left = drawing ? on : off;
                        }
                    }
                }
                p.Stroke();
            }

            static void Ring(Painter2D p, Vector2 at, float radius, Color colour, Color edge)
            {
                p.fillColor = edge;
                p.BeginPath();
                p.Arc(at, radius + 2f, 0f, 360f);
                p.Fill();
                p.fillColor = colour;
                p.BeginPath();
                p.Arc(at, radius, 0f, 360f);
                p.Fill();
            }
        }
    }
}
