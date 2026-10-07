// ported 4 Oct: WHERE THE TABLES ARE. In the first build the 22 json tables were TextAssets dragged onto the KapmanGame
// component in the scene. The rebuild's level scenes are built by editor tools, so KapmanGame fills any table left empty
// from Assets/Kapman/Game/Data by name: from Resources if the folder is ever moved under one, else (in the editor) the
// asset itself, else the file read off the disk (the editor and development builds run beside the project). A player
// build reads them from StreamingAssets/Data, where Editor/Game/ContentBuild.cs copies the whole folder before a build
// (7 Oct: ContentFiles); tables assigned on the component, or a Data folder under Resources, still come first.
using System.IO;
using UnityEngine;

namespace Kapman.Game
{
    public static class GameFiles
    {
        /// <summary>The tables' folder in the project.</summary>
        public const string Folder = "Assets/Kapman/Game/Data";

        /// <summary>A table by its file name without the extension ("ports_793"), or null with a warning.</summary>
        public static TextAsset Find(string name, string extension = ".json")
        {
            var r = Resources.Load<TextAsset>("Data/" + name);
            if (r != null)
                return r;
#if UNITY_EDITOR
            var a = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(Folder + "/" + name + extension);
            if (a != null)
                return a;
#endif
            try
            {
                string path = Path.Combine(Application.dataPath, "Kapman/Game/Data", name + extension);
                if (File.Exists(path))
                {
                    var t = new TextAsset(File.ReadAllText(path));
                    t.name = name;
                    return t;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Kapman: the table " + name + " couldn't be read: " + e.Message);
            }
            // 7 Oct: a built game has the whole data folder under StreamingAssets/Data (Editor/Game/ContentBuild.cs copies
            // it there before a build; ContentFiles knows where). In the editor this is the same folder as above.
            if (ContentFiles.Exists(name + extension))
            {
                string text = ContentFiles.Read(name + extension);
                if (text != null)
                {
                    var t = new TextAsset(text);
                    t.name = name;
                    return t;
                }
            }
            Debug.LogWarning("Kapman: no table " + name + extension + " (assign it on KapmanGame, or put the Data folder under Resources).");
            return null;
        }
    }
}
