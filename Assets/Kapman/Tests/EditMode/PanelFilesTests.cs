using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE SIDE WINDOWS' OWN LAYOUTS (KAPMAN_PLAN step 2d, 14 Oct 2026). Every file of Game/UI/Panels is named for a
    /// panel of the frame, holds the element "panel" with the sections that panel's code asks for, and is named in
    /// Resources/FrameParts so the window finds it.
    /// </summary>
    public class PanelFilesTests
    {
        const string Folder = "Assets/Kapman/Game/UI/Panels";
        const string PartsAsset = "Assets/Kapman/Game/Resources/FrameParts.asset";

        /// <summary>The sections each panel's Fill puts its lines into (Section("...") in its code).</summary>
        static readonly Dictionary<string, string[]> Sections = new Dictionary<string, string[]>
        {
            { "hold", new[] { "room", "goods", "room-left", "stores" } },
            { "crew", new[] { "men", "state", "pay", "sea" } },
            { "faith", new[] { "faith", "days" } },
            { "house", new[] { "silver", "boats", "home", "your-name" } },
            { "port", new[] { "place", "words", "choices", "note" } },
        };

        static string Guid(string asset) => Regex.Match(File.ReadAllText(asset + ".meta"), "guid: ([0-9a-f]{32})").Groups[1].Value;

        [Test]
        public void EveryWindowFileIsAPanelsAndHoldsItsSections()
        {
            Assert.IsTrue(Directory.Exists(Folder), Folder + " is missing");
            var files = Directory.GetFiles(Folder, "*.uxml").Select(f => f.Replace('\\', '/')).OrderBy(f => f).ToList();
            Assert.IsNotEmpty(files, "no window files in " + Folder);
            string parts = File.ReadAllText(PartsAsset);
            foreach (string file in files)
            {
                string id = Path.GetFileNameWithoutExtension(file);
                Assert.IsNotNull(FramePanels.Get(id), file + " is named for no panel of the frame (the file's name must be a panel's id)");
                string uxml = File.ReadAllText(file);
                StringAssert.Contains("name=\"panel\"", uxml, file + " has no element named \"panel\"");
                if (Sections.TryGetValue(id, out var wanted))
                {
                    var missing = wanted.Where(w => !uxml.Contains("name=\"" + w + "\"")).ToList();
                    Assert.IsEmpty(missing, file + " lacks the sections its code fills: " + string.Join(", ", missing));
                }
                StringAssert.Contains("guid: " + Guid(file), parts, PartsAsset + " does not name " + file + " (Kapman > Make physical > 2b)");
            }
            foreach (string id in Sections.Keys)
                Assert.IsTrue(files.Any(f => Path.GetFileNameWithoutExtension(f) == id), "no window file for the panel \"" + id + "\"");
        }

        [Test]
        public void TheWindowFilesInstantiateAndArePickedByTheirId()
        {
#if UNITY_EDITOR
            var parts = UnityEditor.AssetDatabase.LoadAssetAtPath<FrameParts>(PartsAsset);
            Assert.IsNotNull(parts, PartsAsset + " is missing");
            Assert.GreaterOrEqual(parts.panels.Length, Sections.Count, PartsAsset + " names fewer window files than there are");
            foreach (var (id, wanted) in Sections.Select(kv => (kv.Key, kv.Value)))
            {
                var file = parts.panels.FirstOrDefault(v => v != null && v.name == id);
                Assert.IsNotNull(file, PartsAsset + " names no window file for \"" + id + "\"");
                var panel = file.Instantiate().Q<VisualElement>("panel");
                Assert.IsNotNull(panel, id + ".uxml has no element named \"panel\" once instantiated");
                foreach (string w in wanted)
                    Assert.IsNotNull(panel.Q<VisualElement>(w), id + ".uxml has no section named \"" + w + "\" once instantiated");
                CollectionAssert.AreEqual(wanted, panel.Children().Select(c => c.name).ToList(), id + ".uxml: the sections are not in the order the code fills them (fine to reorder, then change this test)");
            }
#endif
        }
    }
}
