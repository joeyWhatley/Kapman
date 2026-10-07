// PROJECT PATH (later): Assets/Kapman/Editor/Game/ShipViewSeating.cs      assembly: Kapman.Editor
// DRAFT, 7 Oct 2026. Compile-checked outside Unity (not run: it needs the editor).
using System.Collections.Generic;
using System.IO;
using Kapman.Game.ShipView;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE SHIP WINDOW'S SHIP, WITH HER CREW SEATED - an editor tool that makes one prefab from the user's pack
    /// (Assets/Stylized_Medieval_Vikings_Pack, left untouched): the pack's SMV_Ship, her own decorative oars and
    /// shields put away, and on the benches nearest amidships so many rowers, each with a seat, a thole on the rail and
    /// a working oar; a helmsman aft at the steering oar; and the RowingRig and ShipCrew that move them.
    /// It also makes the two sets of materials the ship window's styles swap between (the pack's own are URP's and draw
    /// pink in this project): HDRP/Lit ones and the console's, each reading the pack's own palette texture.
    ///
    /// Every figure here was measured from the pack's files without Unity (the prefabs are text; the models' vertices
    /// were read from the FBX files). The ship is 17.9 m long and 5.0 m in the beam; her rail's top amidships is 2.09 m
    /// above her keel line, her bench tops 1.745 m, her after deck 1.87 m; the pack's own demo floats her with still
    /// water at 1.2 m. See Design/ship_view/LOOK_AND_MOTION.md for what must be looked at in the editor first.
    /// Menu: Kapman ▸ Ship view ▸ Build the ship and her crew.   Batch: -executeMethod Kapman.EditorTools.ShipViewSeating.BuildBatch
    /// </summary>
    public static class ShipViewSeating
    {
        public const string Pack = "Assets/Stylized_Medieval_Vikings_Pack/";
        public const string OutDir = "Assets/Kapman/Models/ShipView";
        public const string PrefabPath = OutDir + "/ShipView_Ship.prefab";

        // the rail's top on the starboard side, station by station: z (m, + toward the bow), x, y. The port side mirrors.
        static readonly float[,] Rail =
        {
            { -5.25f, 1.57f, 2.47f }, { -4.25f, 1.87f, 2.30f }, { -2.21f, 2.22f, 2.15f }, { -1.00f, 2.41f, 2.11f }, { 0f, 2.48f, 2.09f },
            { 1.00f, 2.41f, 2.12f }, { 2.21f, 2.22f, 2.15f }, { 4.30f, 1.87f, 2.30f }, { 5.25f, 1.57f, 2.47f },
        };

        // the pack's oar (Oar_2): it runs along its own y, 0.764 m from its origin to the handle's end, 2.364 m to the
        // blade's tip, the middle of the blade 1.85 m out; the blade is 0.24 m wide
        const float OarInboard = 0.764f, OarBlade = 1.85f;

        // who sits where relative to his thole: the seat's middle so far inboard of it and so far toward the bow
        // (found by running the stroke on the pack's skeleton: Design/ship_view/check)
        const float SeatInboard = 0.56f, SeatAhead = 0.30f;

        static readonly string[] Men =
        {
            "SMV_Viking_Settler_1", "SMV_Viking_Settler_3", "SMV_Viking_Karl_2", "SMV_Viking_Settler_4",
            "SMV_Viking_Karl_3", "SMV_Viking_Karl_4", "SMV_Viking_Karl_1", "SMV_Viking_Karl_5",
        };

        [MenuItem("Kapman/Ship view/Build the ship and her crew (4 oars)")]
        public static void BuildFour() => Build(4, 1.5f, false);

        [MenuItem("Kapman/Ship view/Build the ship and her crew (8 oars)")]
        public static void BuildEight() => Build(8, 1.5f, false);

        public static void BuildBatch() => Build(4, 1.5f, false);

        /// <summary>
        /// Makes the prefab and returns it. rowers: how many oars are manned (half a side; the benches nearest amidships).
        /// oarLength: the pack's 3.1 m oar stretched by this much along its length (1.5: 4.7 m, which lets it lie flatter
        /// and brings the hands down to the chest). shields: keep the pack's row of shields along the rail.
        /// </summary>
        public static GameObject Build(int rowers, float oarLength, bool shields)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Pack + "Prefabs - Environment/SMV_Ship.prefab");
            if (source == null)
            {
                Debug.LogError("Kapman ship view: " + Pack + "Prefabs - Environment/SMV_Ship.prefab is not there.");
                return null;
            }
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), OutDir));
            AssetDatabase.Refresh();

            var root = new GameObject("ShipView_Ship");
            var ship = (GameObject)PrefabUtility.InstantiatePrefab(source, root.transform);
            ship.transform.localPosition = Vector3.zero;
            ship.transform.localRotation = Quaternion.identity;

            // what the pack dressed her with that the working crew replaces
            foreach (var t in ship.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Ship_Oars_L" || t.name == "Ship_Oars_R")
                    t.gameObject.SetActive(false);
                if (!shields && (t.name == "Ship_Shields_L" || t.name == "Ship_Shields_R" || t.name == "Ship_Shiled_Holder" || t.name.StartsWith("WeaponRack")))
                    t.gameObject.SetActive(false);
            }
            GameObject sailSet = Named(ship.transform, "Ship_Sail_Open"), sailFurled = Named(ship.transform, "Ship_Sail_Close");
            var steer = Named(ship.transform, "ShipSteerOar_1");

            // the benches: each one's plank, its top and its middle (the ship stands at the origin, unturned)
            var benches = new List<Vector3>();
            foreach (var t in ship.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("Ship_Bench"))
                    continue;
                Renderer plank = null;
                foreach (var r in t.GetComponentsInChildren<Renderer>(true))
                    if (plank == null || r.bounds.max.y > plank.bounds.max.y)
                        plank = r;
                if (plank != null)
                    benches.Add(new Vector3(plank.bounds.center.x, plank.bounds.max.y, plank.bounds.center.z));
            }
            if (benches.Count == 0)
                Debug.LogWarning("Kapman ship view: no bench was found under the ship (objects named Ship_Bench...); no rowers were seated.");

            var crewRoot = new GameObject("Crew").transform;
            crewRoot.SetParent(root.transform, false);
            var oarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Pack + "Prefabs - Environment/Oar_2.prefab");
            var oars = new List<RowingRig.Oar>();
            var men = new List<RowingRig.Rower>();
            var log = new System.Text.StringBuilder();
            int manIndex = 0;
            foreach (float side in new[] { 1f, -1f })
            {
                // this side's benches, the nearest to amidships first
                var mine = benches.FindAll(b => Mathf.Sign(b.x) == side);
                mine.Sort((a, b) => Mathf.Abs(a.z).CompareTo(Mathf.Abs(b.z)));
                int take = Mathf.Min(rowers / 2, mine.Count);
                for (int n = 0; n < take; n++)
                {
                    Vector3 bench = mine[n];
                    float tholeZ = bench.z - SeatAhead;
                    RailAt(tholeZ, out float railX, out float railY);
                    string tag = (side > 0 ? "starboard " : "port ") + (n + 1);

                    var thole = new GameObject("Thole " + tag).transform;
                    thole.SetParent(crewRoot, false);
                    thole.localPosition = new Vector3(side * (railX - 0.03f), railY + 0.02f, tholeZ);
                    thole.localRotation = side > 0 ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);   // x outboard
                    var pivot = new GameObject("Pivot").transform;
                    pivot.SetParent(thole, false);
                    Transform shaft = null;
                    if (oarPrefab != null)
                    {
                        shaft = ((GameObject)PrefabUtility.InstantiatePrefab(oarPrefab, pivot)).transform;
                        shaft.localScale = new Vector3(1f, oarLength, 1f);                                  // (its y is its length)
                    }
                    // the stretched oar slid outboard so 0.80 m of it is left inboard for the hands
                    float shift = OarInboard * oarLength - 0.80f;
                    oars.Add(new RowingRig.Oar
                    {
                        thole = thole, pivot = pivot, shaft = shaft, side = side,
                        bladeOut = OarBlade * oarLength + shift, shaftShift = shift, gripOuter = 0.40f, gripInner = 0.68f,
                    });

                    var seat = new GameObject("Seat " + tag).transform;
                    seat.SetParent(crewRoot, false);
                    seat.localPosition = new Vector3(side * (railX - 0.03f - SeatInboard), bench.y, bench.z);
                    seat.localRotation = Quaternion.Euler(0f, 180f, 0f);                                    // he faces the stern

                    var man = Man(Men[manIndex % Men.Length], crewRoot, "Rower " + tag);
                    manIndex++;
                    if (man != null)
                        men.Add(new RowingRig.Rower { man = man, seat = seat, oar = oars.Count - 1, seed = 1.3f + 2.1f * manIndex });
                    log.AppendLine($"  {tag}: bench top {bench.y:0.00} at z {bench.z:0.00}; thole on the rail at ({thole.localPosition.x:0.00}, {thole.localPosition.y:0.00}, {tholeZ:0.00}), {thole.localPosition.y - bench.y:0.00} above the seat");
                }
            }

            var rig = root.AddComponent<RowingRig>();
            rig.oars = oars.ToArray();
            rig.rowers = men.ToArray();
            rig.waterline = 1.2f;

            // the helmsman: on the after deck by the starboard quarter, his hand on the steering oar's shaft
            var crew = root.AddComponent<ShipCrew>();
            crew.rig = rig;
            crew.sailSet = sailSet;
            crew.sailFurled = sailFurled;
            var stand = new GameObject("Helm stand").transform;
            stand.SetParent(crewRoot, false);
            stand.localPosition = new Vector3(0.78f, 1.87f, -6.75f);
            crew.helmStand = stand;
            crew.helmsman = Man("SMV_Viking_Settler_2", crewRoot, "Helmsman");
            if (steer != null)
            {
                crew.steerOar = steer.transform;
                Vector3 onShaft = steer.transform.TransformPoint(new Vector3(0f, -0.32f, 0f));
                Vector3 toHim = (stand.position + Vector3.up * 1.3f - onShaft).normalized;
                var grip = new GameObject("Helm grip").transform;
                grip.SetParent(steer.transform, false);
                grip.position = onShaft + toHim * 0.11f;
                crew.helmGrip = grip;
                log.AppendLine($"  helmsman: stands at {stand.localPosition}; grips the steering oar at {root.transform.InverseTransformPoint(grip.position)}");
            }

            // her materials: the pack's own are URP's (pink here); put the HDRP ones on now, so the prefab is never pink
            // (ShipViewLook swaps them for the console's ones by style)
            var swaps = Materials();
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                    foreach (var sw in swaps)
                        if (mats[i] != null && mats[i].name == sw.source && sw.hdrp != null)
                        {
                            mats[i] = sw.hdrp;
                            changed = true;
                        }
                if (changed)
                    r.sharedMaterials = mats;
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log($"Kapman ship view: {PrefabPath} made with {men.Count} rowers of {benches.Count} benches, oars {3.128f * oarLength:0.0} m.\n{log}");
            return prefab;
        }

        /// <summary>The rail's top at a station (starboard: x is positive), between the measured stations.</summary>
        public static void RailAt(float z, out float x, out float y)
        {
            int last = Rail.GetLength(0) - 1;
            z = Mathf.Clamp(z, Rail[0, 0], Rail[last, 0]);
            for (int i = 0; i < last; i++)
                if (z <= Rail[i + 1, 0])
                {
                    float t = Mathf.InverseLerp(Rail[i, 0], Rail[i + 1, 0], z);
                    x = Mathf.Lerp(Rail[i, 1], Rail[i + 1, 1], t);
                    y = Mathf.Lerp(Rail[i, 2], Rail[i + 1, 2], t);
                    return;
                }
            x = Rail[last, 1];
            y = Rail[last, 2];
        }

        static GameObject Named(Transform under, string name)
        {
            foreach (var t in under.GetComponentsInChildren<Transform>(true))
                if (t.name == name)
                    return t.gameObject;
            return null;
        }

        /// <summary>One of the pack's men, his weapon and shield put away, his skinned parts kept drawn whatever his pose.</summary>
        static Transform Man(string prefabName, Transform parent, string name)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Pack + "Prefabs - Characters/" + prefabName + ".prefab");
            if (source == null)
            {
                Debug.LogWarning("Kapman ship view: " + prefabName + " is not in the pack's Prefabs - Characters.");
                return null;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            go.name = name;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
            {
                if (t.name != "hand.L" && t.name != "hand.R")
                    continue;
                for (int i = 0; i < t.childCount; i++)
                {
                    var held = t.GetChild(i);
                    if (held.GetComponentInChildren<Renderer>(true) != null)
                        held.gameObject.SetActive(false);                       // an axe, a sword, a shield
                }
            }
            foreach (var skin in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                skin.updateWhenOffscreen = true;                                // (its bounds are the standing pose's)
            return go.transform;
        }

        // ------------------------------------------------------------------ the materials

        static readonly (string source, string texture)[] PackMaterials =
        {
            ("SMV_BasePallete", "BasePallete.png"),                             // the ship, the men
            ("SMV_Environment", "SMV_EnvironmentPallete.png"),                  // barrels, planks, the pack's water
            ("SMV_Weapons", "UniversalWeaponsColorsVariant.png"),               // weapons, shields, and the oars
        };

        /// <summary>
        /// The materials the ship window swaps between, made once and kept in OutDir: for each of the pack's three, an
        /// HDRP/Lit one and a console one (Kapman/ShipView/PSX Model), both reading the pack's own palette texture.
        /// Give the result to ShipViewLook.swaps.
        /// </summary>
        [MenuItem("Kapman/Ship view/Make the materials")]
        public static void MaterialsMenu() => Materials();

        public static ShipViewLook.Swap[] Materials()
        {
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), OutDir));
            AssetDatabase.Refresh();
            var psx = Shader.Find("Kapman/ShipView/PSX Model");
            if (psx == null)
                Debug.LogError("Kapman ship view: the shader 'Kapman/ShipView/PSX Model' is missing (Assets/Kapman/Shaders/ShipView/PsxModel.shader): no console materials were made.");
            var swaps = new List<ShipViewLook.Swap>();
            foreach (var (source, texture) in PackMaterials)
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(Pack + "Materials/Textures/" + texture);
                if (tex == null)
                {
                    Debug.LogWarning("Kapman ship view: " + texture + " is not in the pack's Materials/Textures.");
                    continue;
                }
                string litPath = OutDir + "/SV_" + source + "_Lit.mat", psxPath = OutDir + "/SV_" + source + "_Psx.mat";
                var lit = AssetDatabase.LoadAssetAtPath<Material>(litPath);
                if (lit == null)
                {
                    lit = AssetLibrary.NewLit(tex, null, 0.25f, Color.white);
                    AssetDatabase.CreateAsset(lit, litPath);
                }
                var console = AssetDatabase.LoadAssetAtPath<Material>(psxPath);
                if (console == null && psx != null)
                {
                    console = new Material(psx);
                    console.SetTexture("_BaseMap", tex);
                    AssetDatabase.CreateAsset(console, psxPath);
                }
                swaps.Add(new ShipViewLook.Swap { source = source, hdrp = lit, psx = console });
            }
            AssetDatabase.SaveAssets();
            return swaps.ToArray();
        }
    }
}
