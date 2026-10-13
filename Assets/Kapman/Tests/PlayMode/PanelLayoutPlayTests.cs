using System.Collections;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE TEXT PANEL, THE NOTICES AND THE PORT'S PICTURE AS PLAYED ARE THE FILES' (14 Oct 2026, KAPMAN_PLAN step 2d).
    /// A new game: the text panel's root on screen is its file's and the port's menu it holds is drawn by the frame as
    /// before; a notice shown stands in the holder as a copy of the notice file; the port's picture, its ground and its
    /// button are the frame's own parts, not made by code. A picture is taken (Captures/panels).
    /// </summary>
    public class PanelLayoutPlayTests
    {
        [UnityTest, Timeout(600000)]
        public IEnumerator ThePanelsOnScreenAreTheFilesOwn()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_panels_saves", p => play = p, "panels");
            try
            {
                // the text panel: its root is the file's, filled once, and holds the port's menu the frame draws
                var text = TextPanel.Current;
                Assert.IsNotNull(text, "no text panel");
                var textDoc = text.GetComponent<UIDocument>().rootVisualElement;
                var textRoots = textDoc.Query<VisualElement>(TextPanel.RootName).ToList();
                Assert.AreEqual(1, textRoots.Count, "not one root named \"" + TextPanel.RootName + "\" in the game's document");
                Assert.AreSame(text, textRoots[0].userData, "the text panel's root on screen is not the one the panel filled in");
                Assert.IsTrue(text.Open, "the port's menu is not up on the text panel");
                Assert.Greater(text.Options.Count, 1, "the port's menu has not its options");
                var options = textRoots[0].Q<VisualElement>("options").Children().ToList();
                Assert.AreEqual(text.Options.Count, options.Count, "the options on the panel are not the options it holds");
                Assert.IsTrue(options.All(o => o.name == "option" && o is Button), "an option is not a copy of the option file");
                Assert.IsFalse(ChartPlay.Shown(textRoots[0]), "the text panel's own box is shown while the frame draws it");

                // the notices: a notice is a copy of the notice file, in the holder of the notices file
                var notices = Notices.Current;
                Assert.IsNotNull(notices, "no notices");
                var holder = notices.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>(Notices.RootName);
                Assert.IsNotNull(holder, "no holder named \"" + Notices.RootName + "\" in the notices' document");
                Assert.AreSame(notices, holder.userData, "the notices' holder on screen is not the one the code took");
                notices.Show("The water casks are full, for the test.");
                yield return ChartPlay.Frames(3);
                var shown = holder.Children().OfType<Label>().ToList();
                Assert.AreEqual(1, shown.Count, "not one notice in the holder");
                Assert.AreEqual("notice", shown[0].name, "the notice is not a copy of the notice file");
                Assert.AreEqual("The water casks are full, for the test.", shown[0].text);
                Assert.IsTrue(ChartPlay.Shown(shown[0]), "the notice is not shown");

                // the port's picture: the frame's own parts, in their places
                var view = PortView.Current;
                var frame = FrameRoot.Current;
                Assert.IsNotNull(view, "no port view");
                Assert.IsNotNull(frame, "no frame");
                Assert.IsNotNull(view.Picture, "the port view has no picture");
                Assert.AreEqual("port-view", view.Picture.name);
                Assert.AreSame(frame.Root, view.Picture.parent, "the port's picture is not under the frame's root");
                // (behind the frame's panes; the 3D view's picture and its helm mark are put in front of it by code and
                // are never up at the same time as the port's picture)
                int at = frame.Root.IndexOf(view.Picture);
                Assert.Less(at, frame.Root.IndexOf(frame.Root.Q<VisualElement>("pane-left")), "the port's picture does not lie behind the frame's panes");
                Assert.AreEqual("place-ground", frame.Root.ElementAt(at + 1).name, "the place's ground is not right after the port's picture");
                Assert.IsNotNull(view.Flip, "the port view has no button");
                Assert.AreEqual("port-flip", view.Flip.name);
                Assert.AreSame(frame.ViewButtons, view.Flip.parent, "the port's button is not among the view buttons");
                Assert.AreEqual(1, frame.Root.Query<Image>("port-view").ToList().Count, "more than one port picture in the frame");
                Assert.AreEqual(1, frame.Root.Query<Button>("port-flip").ToList().Count, "more than one port button in the frame");
                yield return play.Shot("panels_0_the_files_own");
                Assert.AreEqual(0, play.faults.Count, string.Join("\n", play.faults));
            }
            finally
            {
                play.End();
            }
        }
    }
}
