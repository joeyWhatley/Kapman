using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Kapman.Levels;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE LANDMARKS a crew steers by (playability 3.2), written per level to Assets/Kapman/Data/Levels/landmarks_&lt;level&gt;.json
    /// from the built scene's named objects (the builders start from an empty scene, so hand-placed markers would be wiped):
    /// each object's place is the middle of its renderers, its height their top above the water. The builders call it
    /// before saving; Kapman > Levels > Write Dorestad's landmarks does it on the saved scene.
    /// </summary>
    public static class LandmarkExport
    {
        public const string Dir = "Assets/Kapman/Data/Levels";

        /// <summary>What to look for: the start of an object's name, the landmark's id (a number is added for several), its
        /// name as the crew says it (placeholder wording), its kind, and how far off it is noticed (m).</summary>
        static readonly (string prefix, string id, string name, string kind, float seen)[] DorestadRules =
        {
            ("The fort across the Lek", "fort", "the fort across the Lek", "fort", 900f),
            ("Cemeteries and the church", "church", "the church by the cemeteries", "church", 700f),
            ("Fish weir", "weir", "a fish weir", "weir", 220f),
        };

        public static void Write(string level, Scene scene, float waterLevel, (string prefix, string id, string name, string kind, float seen)[] rules, List<Vector2> channel = null)
        {
            var found = new List<(string id, string name, string kind, Vector3 at, float height, float seen)>();
            var count = new Dictionary<string, int>();
            if (channel != null && channel.Count > 1)
                foreach (var m in RouteMarks(scene, channel, waterLevel))
                {
                    found.Add(m);
                    count[m.id] = 1;
                }
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    foreach (var r in rules)
                    {
                        if (!t.name.StartsWith(r.prefix))
                            continue;
                        var rends = t.GetComponentsInChildren<Renderer>(true);
                        if (rends.Length == 0)
                            continue;
                        var b = rends[0].bounds;
                        foreach (var x in rends)
                            b.Encapsulate(x.bounds);
                        count.TryGetValue(r.id, out int n);
                        count[r.id] = ++n;
                        found.Add((r.id, r.name, r.kind, b.center, Mathf.Max(1f, b.max.y - waterLevel), r.seen));
                        break;
                    }
            var sb = new StringBuilder();
            sb.Append("{\n  \"level\": \"").Append(level).Append("\",\n");
            sb.Append("  \"units\": \"metres in the level (x east, z north); height above the water; seen_m: how far off the crew notices it\",\n");
            sb.Append("  \"about\": \"Written by LandmarkExport from the built scene; a rebuild rewrites it. The names are placeholders.\",\n");
            sb.Append("  \"landmarks\": [\n");
            var seenIds = new Dictionary<string, int>();
            for (int i = 0; i < found.Count; i++)
            {
                var f = found[i];
                seenIds.TryGetValue(f.id, out int k);
                seenIds[f.id] = ++k;
                string id = count[f.id] > 1 ? f.id + "_" + k : f.id;
                sb.Append("    { \"id\": \"").Append(id).Append("\", \"name\": \"").Append(f.name).Append("\", \"kind\": \"").Append(f.kind)
                  .Append("\", \"x\": ").Append(F(f.at.x)).Append(", \"z\": ").Append(F(f.at.z)).Append(", \"height\": ").Append(F(f.height))
                  .Append(", \"seen_m\": ").Append(F(f.seen)).Append(", \"from\": 0, \"to\": 0 }").Append(i < found.Count - 1 ? ",\n" : "\n");
            }
            sb.Append("  ]\n}\n");
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), Dir));
            string path = $"{Dir}/landmarks_{level}.json";
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), path), sb.ToString());
            AssetDatabase.ImportAsset(path);
            Debug.Log($"Kapman landmarks: {found.Count} written to {path}.");
        }

        /// <summary>A look along Dorestad's route: every named group with renderers whose middle is within 160 m of the
        /// channel from the berth to the exit, by distance along it (to choose the route's landmarks).</summary>
        public static void DiagRoute()
        {
            var scene = EditorSceneManager.OpenScene(DorestadBuilder.ScenePath, OpenSceneMode.Single);
            var lvl = Kapman.Chain.ChainData.Load(Kapman.Chain.ChainData.ProjectPath).Level("dorestad");
            var line = new List<Vector2>();
            foreach (var p in lvl.channel)
                line.Add(new Vector2((float)p.x, (float)p.z));
            var seen = new HashSet<string>();
            var rows = new List<(float along, string text)>();
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.childCount == 0 && t.GetComponent<Renderer>() == null)
                        continue;
                    string key = System.Text.RegularExpressions.Regex.Replace(t.name, @"[0-9]+", "#");
                    var rends = t.GetComponentsInChildren<Renderer>(true);
                    if (rends.Length == 0 || rends.Length > 400)
                        continue;
                    var b = rends[0].bounds;
                    foreach (var x in rends)
                        b.Encapsulate(x.bounds);
                    var c = new Vector2(b.center.x, b.center.z);
                    float best = float.MaxValue, along = 0f, run = 0f, bestAlong = 0f;
                    for (int i = 0; i + 1 < line.Count; i++)
                    {
                        Vector2 a = line[i], d = line[i + 1] - a;
                        float u = Mathf.Clamp01(Vector2.Dot(c - a, d) / d.sqrMagnitude);
                        float dist = Vector2.Distance(c, a + d * u);
                        if (dist < best) { best = dist; bestAlong = run + u * d.magnitude; }
                        run += d.magnitude;
                    }
                    if (best > 160f || b.size.magnitude > 600f)
                        continue;
                    if (!seen.Add(key + Mathf.RoundToInt(bestAlong / 50f)))
                        continue;
                    rows.Add((bestAlong, $"{bestAlong,6:0} m along, {best,4:0} m off, top {b.max.y - 2.44f,4:0.0} m: {t.name} (in {(t.parent != null ? t.parent.name : "-")})"));
                }
            rows.Sort((x, y) => x.along.CompareTo(y.along));
            var sb = new StringBuilder("Kapman route: named things along the channel\n");
            foreach (var r in rows)
                sb.AppendLine(r.text);
            Debug.Log(sb.ToString());
        }

        /// <summary>Distance along a polyline to the point nearest c, and how far off it c lies.</summary>
        static (float along, float off) Along(List<Vector2> line, Vector2 c)
        {
            float best = float.MaxValue, run = 0f, bestAlong = 0f;
            for (int i = 0; i + 1 < line.Count; i++)
            {
                Vector2 a = line[i], d = line[i + 1] - a;
                float u = Mathf.Clamp01(Vector2.Dot(c - a, d) / d.sqrMagnitude);
                float dist = Vector2.Distance(c, a + d * u);
                if (dist < best) { best = dist; bestAlong = run + u * d.magnitude; }
                run += d.magnitude;
            }
            return (bestAlong, best);
        }

        static Vector2 At(List<Vector2> line, float s)
        {
            for (int i = 0; i + 1 < line.Count; i++)
            {
                float len = Vector2.Distance(line[i], line[i + 1]);
                if (s <= len)
                    return Vector2.Lerp(line[i], line[i + 1], s / Mathf.Max(len, 1e-3f));
                s -= len;
            }
            return line[line.Count - 1];
        }

        /// <summary>
        /// The river's own marks along a level's channel, added to the named ones: the boats lying off the dams close to the
        /// fairway (keep clear), the last dam of the town, and the bends (where the channel turns more than 35 degrees within
        /// 200 m), each with its distance along the channel.
        /// </summary>
        public static List<(string id, string name, string kind, Vector3 at, float height, float seen)> RouteMarks(Scene scene, List<Vector2> line, float waterLevel)
        {
            var marks = new List<(string, string, string, Vector3, float, float)>();
            float lastDam = -1f;
            Vector3 lastDamAt = default;
            int boats = 0;
            foreach (var root in scene.GetRootGameObjects())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    bool boat = t.name.StartsWith("Utrecht boat at dam") || (t.name.StartsWith("Ship_") && t.parent != null && t.parent.name == "Moored boats");
                    bool dam = System.Text.RegularExpressions.Regex.IsMatch(t.name, @"^Dam [0-9]+a$");
                    if (!boat && !dam)
                        continue;
                    var rends = t.GetComponentsInChildren<Renderer>(true);
                    if (rends.Length == 0)
                        continue;
                    var b = rends[0].bounds;
                    foreach (var x in rends)
                        b.Encapsulate(x.bounds);
                    var (along, off) = Along(line, new Vector2(b.center.x, b.center.z));
                    if (boat && off < 25f)
                        marks.Add(($"moored_{++boats}", "a boat lying off the dams", "moored", b.center, Mathf.Max(1f, b.max.y - waterLevel), 160f));
                    if (dam && off < 200f && along > lastDam)
                    {
                        lastDam = along;
                        lastDamAt = b.center;
                    }
                }
            if (lastDam >= 0f)
                marks.Add(("town_end", "the last dam of the town", "town_end", lastDamAt, 2f, 300f));
            // the bends: the turn between the 250 m before and after each point, its local greatest above 20 degrees, 400 m
            // apart at least (the Kromme Rijn turns in long curves, 70 to 100 degrees over a kilometre)
            float total = 0f;
            for (int i = 0; i + 1 < line.Count; i++)
                total += Vector2.Distance(line[i], line[i + 1]);
            var turn = new List<(float s, float deg)>();
            for (float s = 250f; s <= total - 250f; s += 20f)
            {
                Vector2 before = At(line, s) - At(line, s - 250f), after = At(line, s + 250f) - At(line, s);
                turn.Add((s, Mathf.Abs(Vector2.SignedAngle(before, after))));
            }
            int bends = 0;
            float lastBend = -1e9f;
            for (int i = 1; i + 1 < turn.Count; i++)
                if (turn[i].deg > 20f && turn[i].deg >= turn[i - 1].deg && turn[i].deg > turn[i + 1].deg && turn[i].s - lastBend >= 400f)
                {
                    lastBend = turn[i].s;
                    var p = At(line, turn[i].s);
                    marks.Add(($"bend_{++bends}", "the bend", "bend", new Vector3(p.x, waterLevel, p.y), 1f, 220f));
                }
            return marks;
        }

        static string F(float v) => v.ToString("0.#", CultureInfo.InvariantCulture);

        public static void WriteDorestad(Scene scene, float waterLevel)
        {
            var lvl = Kapman.Chain.ChainData.Load(Kapman.Chain.ChainData.ProjectPath).Level("dorestad");
            var line = new List<Vector2>();
            if (lvl != null)
                foreach (var p in lvl.channel)
                    line.Add(new Vector2((float)p.x, (float)p.z));
            Write("dorestad", scene, waterLevel, DorestadRules, line);
        }

        [MenuItem("Kapman/Levels/Write Dorestad's landmarks")]
        public static void FromSavedDorestad()
        {
            var scene = EditorSceneManager.OpenScene(DorestadBuilder.ScenePath, OpenSceneMode.Single);
            var info = Object.FindFirstObjectByType<LevelInfo>();
            WriteDorestad(scene, info != null ? info.WaterLevel : 2.44f);
        }
    }
}
