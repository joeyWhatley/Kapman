using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// PICTURES TAKEN OF THE GAME'S OWN MODELS, for the frame (the user, 8 Oct 2026: "replace the bread with a pic of a
    /// barrel from the assets we have"). The barrel of the Vikings pack is drawn by the editor's own preview, on a
    /// plain ground, into Bake/art/out/icons/barrel.png; Bake/art/standin_barrel.py then cuts it from its ground and
    /// puts it in the game's tones (Art/standin/hud/hud.stores.png).
    /// Menu: Kapman > Pictures > The barrel.   Batch: -executeMethod Kapman.EditorTools.KapmanPictures.BarrelBatch
    /// </summary>
    public static class KapmanPictures
    {
        public const string Barrel = "Assets/Stylized_Medieval_Vikings_Pack/Prefabs - Environment/Barrel_1.prefab";
        public const string BarrelPicture = "Bake/art/out/icons/barrel.png";

        [MenuItem("Kapman/Pictures/The barrel")]
        public static void BarrelNow()
        {
            if (!Draw())
                Debug.LogError("Kapman pictures: the barrel could not be drawn.");
        }

        public static void BarrelBatch()
        {
            bool ok = false;
            try
            {
                ok = Draw();
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
            EditorApplication.Exit(ok ? 0 : 1);
        }

        static bool Draw()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Barrel);
            if (prefab == null)
            {
                Debug.LogError("Kapman pictures: " + Barrel + " is not there.");
                return false;
            }
            const int size = 512;
            var util = new PreviewRenderUtility();
            try
            {
                var thing = util.InstantiatePrefabInScene(prefab);
                var bounds = new Bounds(thing.transform.position, Vector3.zero);
                bool any = false;
                foreach (var r in thing.GetComponentsInChildren<Renderer>())
                {
                    if (!any)
                        bounds = r.bounds;
                    else
                        bounds.Encapsulate(r.bounds);
                    any = true;
                }
                // (the pack's own materials are not of this project's render pipeline: the barrel is drawn by the plain old
                // renderer, in a plain lit material that carries the pack's own painted picture)
                var plain = Shader.Find("Legacy Shaders/Diffuse");
                foreach (var r in thing.GetComponentsInChildren<Renderer>())
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        Texture picture = null;
                        if (mats[i] != null)
                            foreach (string name in new[] { "_BaseMap", "_MainTex", "_BaseColorMap", "_Albedo", "_BaseTexture" })
                                if (picture == null && mats[i].HasProperty(name))
                                    picture = mats[i].GetTexture(name);
                        var m = new Material(plain) { hideFlags = HideFlags.HideAndDontSave };
                        m.mainTexture = picture;
                        m.color = Color.white;
                        mats[i] = m;
                    }
                    r.sharedMaterials = mats;
                }
                float reach = Mathf.Max(0.05f, bounds.extents.magnitude);
                var cam = util.camera;
                cam.fieldOfView = 26f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.1f, 0.85f, 0.25f, 1f);       // (a plain ground no barrel is the colour of)
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = reach * 40f;
                Vector3 from = Quaternion.Euler(18f, -32f, 0f) * Vector3.back;
                cam.transform.position = bounds.center + from * (reach / Mathf.Tan(13f * Mathf.Deg2Rad)) * 1.05f;
                cam.transform.LookAt(bounds.center);
                util.lights[0].intensity = 1.3f;
                util.lights[0].transform.rotation = Quaternion.Euler(40f, -60f, 0f);
                util.lights[1].intensity = 0.7f;
                util.ambientColor = new Color(0.5f, 0.5f, 0.5f);
                util.BeginStaticPreview(new Rect(0, 0, size, size));
                util.Render(false);
                var tex = util.EndStaticPreview();
                if (tex == null)
                    return false;
                string path = Path.Combine(Application.dataPath, "..", BarrelPicture);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                Debug.Log("Kapman pictures: the barrel is drawn to " + BarrelPicture + ".");
                return true;
            }
            finally
            {
                util.Cleanup();
            }
        }
    }
}
