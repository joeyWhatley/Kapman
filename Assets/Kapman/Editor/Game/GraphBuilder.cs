using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// GRAPH BUILDER - writes a visual scripting graph from code, laid out for reading: nodes placed left to right in
    /// labelled groups, with sticky notes saying what each part does. The game's graphs are made with it once; after
    /// that they're yours: <see cref="Save"/> never overwrites a graph that exists (delete it to have it made again).
    /// </summary>
    public sealed class GraphBuilder
    {
        public readonly FlowGraph graph = new FlowGraph();

        public GraphBuilder(string title, string summary)
        {
            graph.title = title;
            graph.summary = summary;
        }

        /// <summary>Adds a node at (x, y) in the graph's space (x to the right, y down).</summary>
        public T Add<T>(T unit, float x, float y) where T : IUnit
        {
            unit.position = new Vector2(x, y);
            graph.units.Add(unit);
            return unit;
        }

        /// <summary>Connects flow: from an exit to an entry.</summary>
        public void Flow(ControlOutput from, ControlInput to) => graph.controlConnections.Add(new ControlConnection(from, to));

        /// <summary>Connects a value: from an output to an input.</summary>
        public void Data(ValueOutput from, ValueInput to) => graph.valueConnections.Add(new ValueConnection(from, to));

        /// <summary>Sets an input's own value (shown on the node, editable there). Only for inputs that have one of their
        /// own (most do); a variable setter's value needs a node: <see cref="Value"/>.</summary>
        public void Set(ValueInput input, object value) => input.unit.defaultValues[input.key] = value;

        /// <summary>Feeds an input from a small value node beside it (true/false, a number, a word, or nothing).</summary>
        public void Value(ValueInput input, object value, float x, float y)
        {
            if (value == null)
            {
                var nothing = Add(new Null(), x, y);
                Data(nothing.@null, input);
                return;
            }
            var literal = Add(new Literal(value.GetType(), value), x, y);
            Data(literal.output, input);
        }

        /// <summary>
        /// When true, <see cref="Save"/> writes over a graph that exists (into the same asset, so what uses it keeps
        /// it). Only for remaking graphs nobody has edited yet; off by default.
        /// </summary>
        public static bool ReplaceExisting;

        /// <summary>A sticky note: what this part of the graph does, in words.</summary>
        public void Note(float x, float y, float width, float height, string title, string body,
                         StickyNote.ColorEnum color = StickyNote.ColorEnum.Classic)
        {
            graph.sticky.Add(new StickyNote { position = new Rect(x, y, width, height), title = title, body = body, colorTheme = color });
        }

        /// <summary>A labelled box round a part of the graph.</summary>
        public void Group(float x, float y, float width, float height, string label, string comment, Color color)
        {
            graph.groups.Add(new GraphGroup { position = new Rect(x, y, width, height), label = label, comment = comment, color = color });
        }

        /// <summary>
        /// Saves the graph as an asset, unless one is already there (then that one is returned as it is: the user's).
        /// </summary>
        public ScriptGraphAsset Save(string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(path);
            if (existing != null)
            {
                if (!ReplaceExisting)
                    return existing;
                existing.graph = graph;
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
                return existing;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var asset = ScriptableObject.CreateInstance<ScriptGraphAsset>();
            asset.graph = graph;
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }

        // colours for the groups, by kind of work
        public static readonly Color Watch = new Color(0.25f, 0.42f, 0.62f);     // watching the world
        public static readonly Color Talk = new Color(0.62f, 0.45f, 0.22f);      // the text panel
        public static readonly Color Change = new Color(0.32f, 0.55f, 0.32f);    // changing the game's state
        public static readonly Color Info = new Color(0.4f, 0.4f, 0.4f);         // notes and maps
    }
}
