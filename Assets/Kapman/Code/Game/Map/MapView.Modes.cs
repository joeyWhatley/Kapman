using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    public enum MapMode { Land, Realms, Faith, Peoples }

    /// <summary>
    /// THE CHART'S OTHER READINGS (the user, 7 Oct 2026, late: "add alt map modes like borders, religion, culture maps").
    /// Land is the chart as it is drawn. In Realms, Faith and Peoples each port's dot is the colour of whose it is, and
    /// so is its name when the pointer brings it up (its realm from the ports' file; its faith and its people as the
    /// encounters' book has them for the port), and
    /// a key at the chart's corner names the colours that are in sight. It is a reading of the ports, not of the land:
    /// the land has no borders drawn on it yet (there is no file of them), and where one comes these colours are its.
    /// </summary>
    public partial class MapView
    {
        public MapMode Mode { get; private set; }

        static readonly Color32[] ModeColours =
        {
            new Color32(112, 41, 99, 255), new Color32(41, 112, 54, 255), new Color32(176, 96, 32, 255), new Color32(40, 88, 140, 255),
            new Color32(150, 40, 46, 255), new Color32(96, 82, 30, 255), new Color32(30, 110, 112, 255), new Color32(120, 70, 150, 255),
            new Color32(70, 70, 78, 255), new Color32(160, 70, 110, 255), new Color32(60, 120, 80, 255), new Color32(130, 100, 60, 255),
        };

        readonly Dictionary<string, string> modeOf = new Dictionary<string, string>(StringComparer.Ordinal);       // port -> whose
        readonly Dictionary<string, int> modeIndex = new Dictionary<string, int>(StringComparer.Ordinal);          // whose -> its colour
        readonly List<string> modeShown = new List<string>();
        VisualElement modeKey;
        string modeKeyFor = "";

        public void SetMode(MapMode mode)
        {
            if (Mode == mode)
                return;
            Mode = mode;
            modeOf.Clear();
            modeIndex.Clear();
            modeKeyFor = "";
        }

        string ModeOf(PortDef def)
        {
            if (modeOf.TryGetValue(def.id, out string known))
                return known;
            string whose = "";
            if (Mode == MapMode.Realms)
                whose = def.realm ?? "";
            else
            {
                var director = EncounterRunner.Director;
                if (director != null)
                {
                    var facts = director.Facts(def.id);
                    whose = Mode == MapMode.Faith ? facts.faith ?? "" : facts.People ?? "";
                }
            }
            modeOf[def.id] = whose;
            return whose;
        }

        /// <summary>"old_gods" is "Old gods"; a people or a faith of the book of selves is called by its name there.</summary>
        string ModeName(string whose)
        {
            if (Mode == MapMode.Peoples && SelfBook.Current.People(whose) != null)
                return SelfBook.Current.People(whose).name;
            if (Mode == MapMode.Faith && SelfBook.Current.Faith(whose) != null)
                return SelfBook.Current.Faith(whose).name;
            string s = (whose ?? "").Replace('_', ' ');
            return s.Length == 0 ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        /// <summary>A port's dot, and its name when the pointer brings it up, in the colour of whose it is (each frame, for
        /// the ports in sight). The dot since 9 Oct 2026: the names are hidden until the pointer is on them.</summary>
        void ModePaint(PortDef def, Label name, VisualElement dot)
        {
            string whose = Mode == MapMode.Land ? "" : ModeOf(def);
            if (whose.Length == 0)
            {
                name.style.backgroundColor = StyleKeyword.Null;
                name.style.color = StyleKeyword.Null;
                dot.style.backgroundColor = StyleKeyword.Null;
                return;
            }
            if (!modeIndex.TryGetValue(whose, out int i))
                modeIndex[whose] = i = modeIndex.Count;
            name.style.backgroundColor = (Color)ModeColours[i % ModeColours.Length];
            name.style.color = Color.white;
            dot.style.backgroundColor = (Color)ModeColours[i % ModeColours.Length];
            if (!modeShown.Contains(whose))
                modeShown.Add(whose);
        }

        /// <summary>The key: the colours in sight, each with whose it is. Built again only when they change.</summary>
        void ModeKeyUpdate()
        {
            modeShown.Sort(StringComparer.Ordinal);
            string now = Mode + ":" + string.Join(",", modeShown);
            if (now != modeKeyFor)
            {
                modeKeyFor = now;
                if (modeKey == null && root != null)
                {
                    modeKey = new VisualElement { pickingMode = PickingMode.Ignore };
                    modeKey.AddToClassList("map-mode-key");
                    root.Add(modeKey);
                }
                if (modeKey != null)
                {
                    modeKey.Clear();
                    modeKey.style.display = Mode != MapMode.Land && modeShown.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    if (Mode != MapMode.Land)
                    {
                        var head = new Label(Mode == MapMode.Realms ? "Whose the ports are" : Mode == MapMode.Faith ? "The ports' faith" : "The ports' people") { pickingMode = PickingMode.Ignore };
                        head.AddToClassList("map-mode-head");
                        modeKey.Add(head);
                        foreach (string whose in modeShown)
                        {
                            var row = new VisualElement { pickingMode = PickingMode.Ignore };
                            row.AddToClassList("map-mode-row");
                            var swatch = new VisualElement { pickingMode = PickingMode.Ignore };
                            swatch.AddToClassList("map-mode-swatch");
                            swatch.style.backgroundColor = (Color)ModeColours[modeIndex[whose] % ModeColours.Length];
                            var label = new Label(ModeName(whose)) { pickingMode = PickingMode.Ignore };
                            label.AddToClassList("map-mode-name");
                            row.Add(swatch);
                            row.Add(label);
                            modeKey.Add(row);
                        }
                    }
                }
            }
            modeShown.Clear();
        }
    }
}
