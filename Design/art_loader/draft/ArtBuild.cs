// PROJECT PATH: Assets/Kapman/Editor/Game/ArtBuild.cs
// (a draft of 7 Oct 2026, written outside Assets: Design/art_loader/PLAN.md says how to bring it in)
using System;
using System.IO;
using Kapman.Game;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE PICTURES IN A BUILT GAME (7 Oct 2026). The game reads its pictures as files (ArtSlots): in the editor straight
    /// from Kapman2/Art beside Assets, in a built game from StreamingAssets/Art. This copies them there just before
    /// every build and takes the copy away again when the build is done, as ContentBuild does for the data:
    ///   Art/final/**/*.png         the user's pictures, all of them
    ///   Art/placeholder/**/*.png   the placeholders, but not one whose slot the user has drawn (it would never show)
    ///   Art/hud_assign.json        where each piece of the frame goes
    /// The chart's tiles (the kind "map") are left out: the chart has its own folder (MapArt). If a build fails part-way
    /// the copy stays: the second menu line removes it.
    /// Menu: Kapman ▸ Art ▸ Copy the pictures for a build; Remove the pictures' copy; Show my pictures straight from the
    /// Desktop folder (a tick: while it is on, the game shows a picture as soon as it is saved in the Desktop folder
    /// "Kapman Art", before pull_art.py has brought it to Art/final).
    /// </summary>
    public sealed class ArtBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>The copy's folder in the project.</summary>
        public const string Target = "Assets/StreamingAssets/Art";
        const string StreamingAssets = "Assets/StreamingAssets";
        const string ParentWasThereKey = "Kapman.ArtBuild.streamingAssetsWasThere";
        const string ContentMadeParentKey = "Kapman.ContentBuild.madeStreamingAssets";   // (ContentBuild's own note: it runs first)
        const string DrawPref = "Kapman.Art.fromTheDesktopFolder";
        const string DrawMenu = "Kapman/Art/Show my pictures straight from the Desktop folder";

        public int callbackOrder => 1;                                    // (after ContentBuild)

        public void OnPreprocessBuild(BuildReport report)
        {
            int n = Copy(out long bytes);
            Debug.Log($"Kapman art: {n} pictures ({bytes / 1048576.0:0.0} MB) copied to {Target} for the build.");
        }

        public void OnPostprocessBuild(BuildReport report) => Clean();

        [MenuItem("Kapman/Art/Copy the pictures for a build", priority = 70)]
        static void CopyMenu()
        {
            int n = Copy(out long bytes);
            Debug.Log($"Kapman art: {n} pictures ({bytes / 1048576.0:0.0} MB) copied to {Target}.");
        }

        [MenuItem("Kapman/Art/Remove the pictures' copy", priority = 71)]
        static void CleanMenu() => Clean();

        /// <summary>Copies the pictures into the project's StreamingAssets/Art (replacing an older copy) and tells Unity.
        /// Returns how many files were copied.</summary>
        public static int Copy(out long bytes)
        {
            // (ours to take away again if it ends up empty: it was not there, or ContentBuild made it a moment ago)
            SessionState.SetBool(ParentWasThereKey, AssetDatabase.IsValidFolder(StreamingAssets) && !SessionState.GetBool(ContentMadeParentKey, false));
            string target = Path.Combine(Application.dataPath, "StreamingAssets", "Art");
            if (Directory.Exists(target))
                Directory.Delete(target, true);
            int n = CopyTo(Path.Combine(Application.dataPath, "..", "Art"), target, out bytes);
            AssetDatabase.Refresh();
            return n;
        }

        /// <summary>
        /// Copies what a built game needs of an Art folder into another folder (made if need be). Returns how many
        /// files; `bytes` their size together. A file that cannot be read at that moment is left out with a warning.
        /// </summary>
        public static int CopyTo(string art, string folder, out long bytes)
        {
            bytes = 0;
            int n = 0;
            if (!Directory.Exists(art))
            {
                Debug.LogWarning("Kapman art: no Art folder at " + art + ": nothing copied.");
                return 0;
            }
            string final = Path.Combine(art, "final"), holder = Path.Combine(art, "placeholder");
            n += CopyKinds(final, Path.Combine(folder, "final"), null, ref bytes);
            n += CopyKinds(holder, Path.Combine(folder, "placeholder"), final, ref bytes);
            string assign = Path.Combine(art, "hud_assign.json");
            if (File.Exists(assign) && CopyOne(assign, Path.Combine(folder, "hud_assign.json"), ref bytes))
                n++;
            return n;
        }

        /// <summary>The pictures of every kind's folder but the chart's; `unless`: a folder laid out the same way whose
        /// pictures take these ones' place (a placeholder is left out when its final is there).</summary>
        static int CopyKinds(string from, string to, string unless, ref long bytes)
        {
            if (!Directory.Exists(from))
                return 0;
            int n = 0;
            foreach (string dir in Directory.GetDirectories(from))
            {
                string kind = Path.GetFileName(dir);
                if (kind.Length == 0 || kind[0] == '.' || kind[0] == '_' || kind == "map")
                    continue;
                foreach (string path in Directory.GetFiles(dir, "*.png"))
                {
                    string name = Path.GetFileName(path);
                    if (name.Length == 0 || name[0] == '.' || name[0] == '_')
                        continue;
                    if (unless != null && File.Exists(Path.Combine(unless, kind, name)))
                        continue;
                    Directory.CreateDirectory(Path.Combine(to, kind));
                    if (CopyOne(path, Path.Combine(to, kind, name), ref bytes))
                        n++;
                }
            }
            return n;
        }

        static bool CopyOne(string from, string to, ref long bytes)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(to));
                File.Copy(from, to, true);
                bytes += new FileInfo(to).Length;
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                Debug.LogWarning("Kapman art: " + Path.GetFileName(from) + " could not be copied (" + e.Message + "): left out.");
                return false;
            }
        }

        /// <summary>Takes the copy out of the project again (and the StreamingAssets folder, if it was not there before
        /// the build and nothing else is in it).</summary>
        public static void Clean()
        {
            if (AssetDatabase.IsValidFolder(Target))
                AssetDatabase.DeleteAsset(Target);
            string parent = Path.Combine(Application.dataPath, "StreamingAssets");
            if (!SessionState.GetBool(ParentWasThereKey, true) && Directory.Exists(parent) && Directory.GetFileSystemEntries(parent).Length == 0)
                AssetDatabase.DeleteAsset(StreamingAssets);
            SessionState.EraseBool(ParentWasThereKey);
        }

        // ------------------------------------------------------------------ drawing with the game open

        /// <summary>The folder the user draws in: KAPMAN_ART_DESKTOP if it is set (as pull_art.py reads it), else
        /// "Kapman Art" on the Desktop.</summary>
        public static string DesktopFolder
        {
            get
            {
                string env = Environment.GetEnvironmentVariable("KAPMAN_ART_DESKTOP");
                return !string.IsNullOrEmpty(env) ? env : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Kapman Art");
            }
        }

        [InitializeOnLoadMethod]
        static void DrawFolderAtLoad() => SetDrawFolder(EditorPrefs.GetBool(DrawPref, false), false);

        [MenuItem(DrawMenu, priority = 90)]
        static void DrawFolderMenu() => SetDrawFolder(!EditorPrefs.GetBool(DrawPref, false), true);

        [MenuItem(DrawMenu, true)]
        static bool DrawFolderTick()
        {
            Menu.SetChecked(DrawMenu, EditorPrefs.GetBool(DrawPref, false));
            return true;
        }

        static void SetDrawFolder(bool on, bool say)
        {
            EditorPrefs.SetBool(DrawPref, on);
            string folder = DesktopFolder;
            bool there = Directory.Exists(folder);
            ArtSlots.DrawFolder = on && there ? folder : null;
            if (!say)
                return;
            if (on && !there)
                Debug.LogWarning("Kapman art: the Desktop folder is not at " + folder + " (run Bake/art/make_art_list.py, or set KAPMAN_ART_DESKTOP).");
            else
                Debug.Log(on ? "Kapman art: pictures are shown straight from " + folder + " as you save them. Run pull_art.py when they are finished."
                             : "Kapman art: pictures are shown from Art/final only (what pull_art.py has brought over).");
        }
    }
}
