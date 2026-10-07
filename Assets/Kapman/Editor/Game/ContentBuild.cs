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
    /// THE CONTENT IN A BUILT GAME (7 Oct 2026). The game reads its data as files (ContentFiles): in the editor straight
    /// from Assets/Kapman/Game/Data, in a built game from StreamingAssets/Data. This copies the whole data folder there
    /// just before every build (every file but Unity's .meta files and half-saved ones), and takes the copy away again
    /// when the build is done, so the project never keeps two copies of the writers' files. If a build fails part-way the
    /// copy stays: the second menu line removes it.
    /// Menu: Kapman ▸ Game ▸ Copy the content for a build, and Remove the content copy.
    /// </summary>
    public sealed class ContentBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        /// <summary>The copy's folder in the project.</summary>
        public const string Target = "Assets/StreamingAssets/" + ContentFiles.BuiltFolder;
        const string StreamingAssets = "Assets/StreamingAssets";
        const string MadeParentKey = "Kapman.ContentBuild.madeStreamingAssets";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            int n = Copy();
            Debug.Log($"Kapman content: {n} data files copied to {Target} for the build.");
        }

        public void OnPostprocessBuild(BuildReport report) => Clean();

        [MenuItem("Kapman/Game/Copy the content for a build", priority = 60)]
        static void CopyMenu() => Debug.Log($"Kapman content: {Copy()} data files copied to {Target}.");

        [MenuItem("Kapman/Game/Remove the content copy", priority = 61)]
        static void CleanMenu() => Clean();

        /// <summary>Copies the data folder into the project's StreamingAssets/Data (replacing an older copy) and tells
        /// Unity. Returns how many files were copied.</summary>
        public static int Copy()
        {
            if (!AssetDatabase.IsValidFolder(StreamingAssets))
                SessionState.SetBool(MadeParentKey, true);                 // (ours to take away again, if it ends up empty)
            string target = Path.Combine(Application.dataPath, "StreamingAssets", ContentFiles.BuiltFolder);
            if (Directory.Exists(target))
                Directory.Delete(target, true);
            int n = CopyTo(target);
            AssetDatabase.Refresh();
            return n;
        }

        /// <summary>
        /// Copies every content file of the project's data folder, with its subfolders, into a folder (made if need be):
        /// all but .meta files, hidden files and half-saved ones (.tmp, .part, a name ending in ~). Returns how many.
        /// A file that cannot be read at that moment (a writer has it) is skipped with a warning.
        /// </summary>
        public static int CopyTo(string folder)
        {
            string source = Path.Combine(Application.dataPath, ContentFiles.ProjectFolder);
            if (!Directory.Exists(source))
            {
                Debug.LogWarning("Kapman content: no data folder at " + source + ": nothing copied.");
                return 0;
            }
            return CopyFolder(source, folder);
        }

        static int CopyFolder(string from, string to)
        {
            Directory.CreateDirectory(to);
            int n = 0;
            foreach (string path in Directory.GetFiles(from))
            {
                string name = Path.GetFileName(path);
                if (name.Length == 0 || name[0] == '.' || name[name.Length - 1] == '~')
                    continue;
                if (name.EndsWith(".meta", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
                    name.EndsWith(".part", StringComparison.OrdinalIgnoreCase))
                    continue;
                try
                {
                    File.Copy(path, Path.Combine(to, name), true);
                    n++;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Kapman content: " + name + " could not be copied (" + e.Message + "): left out.");
                }
            }
            foreach (string dir in Directory.GetDirectories(from))
            {
                string name = Path.GetFileName(dir);
                if (name.Length == 0 || name[0] == '.' || name[name.Length - 1] == '~')
                    continue;
                n += CopyFolder(dir, Path.Combine(to, name));
            }
            return n;
        }

        /// <summary>Takes the copy out of the project again (and the StreamingAssets folder, if this made it and nothing
        /// else is in it).</summary>
        public static void Clean()
        {
            if (AssetDatabase.IsValidFolder(Target))
                AssetDatabase.DeleteAsset(Target);
            string parent = Path.Combine(Application.dataPath, "StreamingAssets");
            if (SessionState.GetBool(MadeParentKey, false) && Directory.Exists(parent) && Directory.GetFileSystemEntries(parent).Length == 0)
                AssetDatabase.DeleteAsset(StreamingAssets);
            SessionState.EraseBool(MadeParentKey);
        }
    }
}
