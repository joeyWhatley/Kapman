using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE TEXT PANEL, THE NOTICES AND THE PORT'S PICTURE ARE LAID OUT IN FILES (KAPMAN_PLAN step 2d, 14 Oct 2026).
    /// Game/UI/TextPanel.uxml and Game/UI/Notices.uxml are the Source Assets of their documents in the Kapman scene,
    /// their repeated parts (an option, a notice) are named in Resources/FrameParts, and the port's picture, its ground
    /// and its button are parts of Frame.uxml, the frame's own file.
    /// </summary>
    public class PanelLayoutTests
    {
        const string TextLayout = "Assets/Kapman/Game/UI/TextPanel.uxml";
        const string OptionLayout = "Assets/Kapman/Game/UI/Parts/TextOption.uxml";
        const string NoticesLayout = "Assets/Kapman/Game/UI/Notices.uxml";
        const string NoticeLayout = "Assets/Kapman/Game/UI/Parts/Notice.uxml";
        const string FrameLayout = "Assets/Kapman/Game/UI/Frame.uxml";
        const string PickerLayout = "Assets/Kapman/Game/UI/Parts/NotePicker.uxml";
        const string SlipLayout = "Assets/Kapman/Game/UI/Parts/NoteSlip.uxml";
        const string Scene = "Assets/Kapman/Scenes/Kapman.unity";
        const string PartsAsset = "Assets/Kapman/Game/Resources/FrameParts.asset";

        static readonly string[] TextParts = { "text-panel", "box", "title", "story", "intention", "options", "outcome" };
        static readonly string[] PortParts = { "port-view", "place-ground", "port-flip" };
        static readonly string[] ShipParts = { "ship-big", "ship-small", "ship-swap", "ship-look" };

        static string Guid(string asset) => Regex.Match(File.ReadAllText(asset + ".meta"), "guid: ([0-9a-f]{32})").Groups[1].Value;

        static void Holds(string file, params string[] parts)
        {
            Assert.IsTrue(File.Exists(file), file + " is missing");
            string uxml = File.ReadAllText(file);
            var missing = parts.Where(p => !uxml.Contains("name=\"" + p + "\"")).ToList();
            Assert.IsEmpty(missing, "parts not in " + file + ": " + string.Join(", ", missing));
        }

        [Test]
        public void TheFilesHoldEveryPartAndTheSceneNamesThem()
        {
            Holds(TextLayout, TextParts);
            Holds(OptionLayout, "option");
            Holds(NoticesLayout, "notices");
            Holds(NoticeLayout, "notice");
            Holds(FrameLayout, PortParts);
            Holds(FrameLayout, ShipParts);
            Holds(PickerLayout, "note-veil", "note-mark", "note-hint");
            Holds(SlipLayout, "note-slip", "note-head", "note-about", "note-ask", "note-box", "note-words-ask", "note-words", "note-row", "note-leave", "note-save", "note-foot");
            Assert.AreEqual(TextPanel.RootName, "text-panel");
            Assert.AreEqual(Notices.RootName, "notices");
            string scene = File.ReadAllText(Scene);
            StringAssert.Contains("guid: " + Guid(TextLayout), scene, "the scene's text panel document has not " + TextLayout + " as its Source Asset (Kapman > Make physical > 2f)");
            StringAssert.Contains("guid: " + Guid(NoticesLayout), scene, "the scene's notices document has not " + NoticesLayout + " as its Source Asset (Kapman > Make physical > 2f)");
            string parts = File.ReadAllText(PartsAsset);
            StringAssert.Contains("guid: " + Guid(OptionLayout), parts, PartsAsset + " does not name " + OptionLayout);
            StringAssert.Contains("guid: " + Guid(NoticeLayout), parts, PartsAsset + " does not name " + NoticeLayout);
            StringAssert.Contains("guid: " + Guid(PickerLayout), parts, PartsAsset + " does not name " + PickerLayout);
            StringAssert.Contains("guid: " + Guid(SlipLayout), parts, PartsAsset + " does not name " + SlipLayout);
        }

        [Test]
        public void TheFilesInstantiateWithTheirPartsInTheirOrder()
        {
#if UNITY_EDITOR
            var text = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TextLayout);
            Assert.IsNotNull(text, TextLayout + " did not import");
            var root = text.Instantiate().Q<VisualElement>(TextPanel.RootName);
            Assert.IsNotNull(root, "no element named \"" + TextPanel.RootName + "\"");
            Assert.AreEqual(PickingMode.Ignore, root.pickingMode, "the text panel's root takes the pointer while it is shut");
            CollectionAssert.AreEqual(new[] { "title", "story", "intention", "options", "outcome" },
                                      root.Q<VisualElement>("box").Children().Select(c => c.name).ToList(), "the text panel's parts are not in their order");
            var parts = UnityEditor.AssetDatabase.LoadAssetAtPath<FrameParts>(PartsAsset);
            Assert.IsNotNull(parts, PartsAsset + " is missing");
            Assert.IsNotNull(parts.textOption, PartsAsset + " names no option of the text panel");
            Assert.IsNotNull(parts.notice, PartsAsset + " names no notice");
            var option = FrameParts.Make<Button>(parts.textOption, "option");
            Assert.IsNull(option.parent, "the option's copy is still in the file's preview");
            Assert.IsTrue(option.ClassListContains("text-option"), "the option has not the look of one");
            var notice = FrameParts.Make<Label>(parts.notice, "notice");
            Assert.IsNull(notice.parent, "the notice's copy is still in the file's preview");
            Assert.AreEqual(PickingMode.Ignore, notice.pickingMode, "a notice takes the pointer");
            Assert.IsNotNull(parts.notePicker, PartsAsset + " names no picker of the notes");
            Assert.IsNotNull(parts.noteSlip, PartsAsset + " names no slip of a note");
            var veil = FrameParts.Make<VisualElement>(parts.notePicker, "note-veil");
            Assert.IsNull(veil.parent, "the veil's copy is still in the file's preview");
            Assert.IsTrue(veil.ClassListContains("frame-note-veil"), "the veil has not the look of one");
            var slip = FrameParts.Make<VisualElement>(parts.noteSlip, "note-slip");
            Assert.IsNull(slip.parent, "the slip's copy is still in the file's preview");
            CollectionAssert.AreEqual(new[] { "note-head", "note-about", "note-ask", "note-box", "note-words-ask", "note-words", "note-row", "note-foot" },
                                      slip.Children().Select(c => c.name).ToList(), "the slip's parts are not in their order");
            Assert.IsInstanceOf<TextField>(slip.Q<VisualElement>("note-box"));
            Assert.IsInstanceOf<Button>(slip.Q<VisualElement>("note-save"));

            var notices = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(NoticesLayout);
            Assert.IsNotNull(notices, NoticesLayout + " did not import");
            var holder = notices.Instantiate().Q<VisualElement>(Notices.RootName);
            Assert.IsNotNull(holder, "no element named \"" + Notices.RootName + "\"");
            Assert.AreEqual(PickingMode.Ignore, holder.pickingMode, "the notices' holder takes the pointer");

            var frame = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(FrameLayout);
            Assert.IsNotNull(frame, FrameLayout + " did not import");
            var frameRoot = frame.Instantiate().Q<VisualElement>("frame");
            Assert.IsNotNull(frameRoot, "no element named \"frame\"");
            Assert.IsInstanceOf<Image>(frameRoot.Q<VisualElement>("port-view"));
            Assert.IsInstanceOf<Image>(frameRoot.Q<VisualElement>("ship-big"));
            Assert.IsInstanceOf<Image>(frameRoot.Q<VisualElement>("ship-small"));
            Assert.AreEqual("ship-big", frameRoot.ElementAt(0).name, "the ship's big picture is not the first thing under the frame");
            Assert.AreEqual("port-view", frameRoot.ElementAt(1).name, "the port's picture is not right after the ship's big picture (both lie behind everything)");
            Assert.AreEqual("place-ground", frameRoot.ElementAt(2).name, "the place's ground is not right after the port's picture");
            Assert.AreEqual("ship-view", frameRoot.Q<VisualElement>("ship-small").parent.name, "the ship's small picture is not in her window");
            Assert.AreEqual("views", frameRoot.Q<Button>("ship-swap").parent.name, "Swap is not among the view buttons of the top bar");
            Assert.AreEqual("views", frameRoot.Q<Button>("ship-look").parent.name, "Look is not among the view buttons of the top bar");
            var flip = frameRoot.Q<Button>("port-flip");
            Assert.IsNotNull(flip, "no button named \"port-flip\"");
            Assert.AreEqual("views", flip.parent.name, "the port's button is not among the view buttons of the top bar");
#endif
        }
    }
}
