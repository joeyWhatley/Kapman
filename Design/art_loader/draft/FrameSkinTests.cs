// PROJECT PATH: Assets/Kapman/Tests/EditMode/FrameSkinTests.cs
// (a draft of 7 Oct 2026, written outside Assets: Design/art_loader/PLAN.md says how to bring it in)
using System;
using System.Collections.Generic;
using System.IO;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// The frame's own pieces (FrameSkin), without the scene: the cuts of Art/hud_assign.json are read rightly, a piece
    /// finds its elements by the places its "uss" names, and THE RULE holds: a piece is laid only when the user has
    /// drawn it, and until then the element keeps its stylesheet look.
    /// </summary>
    public class FrameSkinTests
    {
        const string Json = @"{
 ""about"": ""a test's own"",
 ""pieces"": {
  ""hud.fn_button"": { ""file"": ""hud/hud.fn_button.png"", ""w"": 36, ""h"": 24, ""slice"": [6, 5, 4, 3], ""uss"": "".frame-verb"", ""lay"": ""fill"" },
  ""hud.fn_button_down"": { ""file"": ""hud/hud.fn_button_down.png"", ""w"": 36, ""h"": 24, ""slice"": [6, 6, 6, 6], ""uss"": "".frame-verb.on, .frame-verb:active"" },
  ""hud.fn_button_off"": { ""file"": ""hud/hud.fn_button_off.png"", ""w"": 36, ""h"": 24, ""slice"": [6, 6, 6, 6], ""uss"": "".frame-verb.off"" },
  ""hud.frame_face"": { ""file"": ""hud/hud.frame_face.png"", ""w"": 68, ""h"": 84, ""slice"": null, ""uss"": "".frame-face"", ""lay"": ""over"" },
  ""hud.fn.cast_off"": { ""file"": ""hud/hud.fn.cast_off.png"", ""w"": 16, ""h"": 16, ""slice"": null, ""uss"": "".frame-verb.fn-cast_off .frame-icon"", ""lay"": ""icon"" },
  ""hud.fn.lie_to"": { ""file"": ""hud/hud.fn.lie_to.png"", ""w"": 16, ""h"": 16, ""slice"": null, ""uss"": "".frame-verb.fn-lie_to .frame-icon"", ""lay"": ""icon"" },
  ""hud.panel.voyage"": { ""file"": ""hud/hud.panel.voyage.png"", ""w"": 16, ""h"": 16, ""slice"": null, ""uss"": "".frame-win.panel-voyage .frame-win-icon"", ""lay"": ""icon"" },
  ""hud.lock"": { ""file"": ""hud/hud.lock.png"", ""w"": 7, ""h"": 9, ""slice"": null, ""uss"": """" },
  ""hud.odd"": { ""file"": ""hud/hud.odd.png"", ""w"": 8, ""h"": 8, ""slice"": null, ""uss"": ""Button.frame-k, #menu"" }
 }
}";

        string root, final, holder;
        ArtSlots art;
        float clock;
        DateTime stamp = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public void MakeAFolder()
        {
            root = Path.Combine(Path.GetTempPath(), "kapman_skin_" + Guid.NewGuid().ToString("N"));
            final = Path.Combine(root, "final");
            holder = Path.Combine(root, "placeholder");
            Directory.CreateDirectory(holder);
            clock = 100f;
            art = new ArtSlots(final, holder) { Watch = true, Now = () => clock };
        }

        [TearDown]
        public void TakeItAway()
        {
            art.Reset();
            try
            {
                Directory.Delete(root, true);
            }
            catch (IOException)
            {
            }
        }

        string Png(string folder, string file, int w, int h)
        {
            string path = Path.Combine(folder, file);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            File.WriteAllBytes(path, t.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(t);
            stamp = stamp.AddSeconds(3);
            File.SetLastWriteTimeUtc(path, stamp);
            Directory.SetLastWriteTimeUtc(Path.GetDirectoryName(path), stamp);   // (its folder too: two pictures made in the same moment are still told apart)
            return path;
        }

        static VisualElement El(VisualElement parent, params string[] classes)
        {
            var e = new VisualElement();
            foreach (string c in classes)
                e.AddToClassList(c);
            parent?.Add(e);
            return e;
        }

        static FrameSkin.Piece Find(List<FrameSkin.Piece> pieces, string id) => pieces.Find(p => p.id == id);

        // ------------------------------------------------------------------ reading hud_assign.json

        [Test]
        public void TheCuts_AreReadRightly()
        {
            var pieces = FrameSkin.Read(Json);
            Assert.AreEqual(9, pieces.Count);
            var button = Find(pieces, "hud.fn_button");
            Assert.AreEqual("hud/hud.fn_button.png", button.file);
            Assert.AreEqual(36, button.w);
            Assert.AreEqual(24, button.h);
            CollectionAssert.AreEqual(new[] { 6, 5, 4, 3 }, button.slice, "left, top, right, bottom");
            Assert.AreEqual(".frame-verb", button.uss);
            Assert.AreEqual(FrameSkin.Fill, button.lay);
            Assert.AreEqual(FrameSkin.Fill, Find(pieces, "hud.fn_button_down").lay, "no \"lay\": fill");
            var face = Find(pieces, "hud.frame_face");
            Assert.IsNull(face.slice, "null: not cut");
            Assert.AreEqual(FrameSkin.Over, face.lay);
            Assert.AreEqual(0, FrameSkin.Read("not json at all {").Count, "a file that does not parse gives no pieces and no error");
            Assert.AreEqual(0, FrameSkin.Read("{}").Count);
        }

        /// <summary>The project's own Art/hud_assign.json, as Bake/art/make_art_list.py writes it.</summary>
        [Test]
        public void TheProjectsOwnFile_IsRead_AndEveryPlaceInItIsUnderstood()
        {
            string path = Path.Combine(Application.dataPath, "..", "Art", "hud_assign.json");
            if (!File.Exists(path))
                Assert.Ignore("no Art/hud_assign.json on this machine (Bake/art/make_art_list.py)");
            var pieces = FrameSkin.ReadFile(path);
            Assert.GreaterOrEqual(pieces.Count, 100, "the frame has more than a hundred pieces");
            var button = Find(pieces, "hud.fn_button");
            Assert.IsNotNull(button);
            Assert.AreEqual("hud/hud.fn_button.png", button.file);
            Assert.AreEqual(36, button.w);
            Assert.AreEqual(24, button.h);
            CollectionAssert.AreEqual(new[] { 6, 6, 6, 6 }, button.slice);
            CollectionAssert.AreEqual(new[] { 12, 12, 12, 12 }, Find(pieces, "hud.frame_screen").slice);
            Assert.IsNull(Find(pieces, "hud.frame_face").slice, "the frame of a face is not stretched");
            foreach (var p in pieces)
            {
                Assert.IsTrue(p.file.StartsWith("hud/", StringComparison.Ordinal) || p.file.StartsWith("shipview/", StringComparison.Ordinal), p.id + ": " + p.file);
                Assert.Greater(p.w, 0, p.id);
                Assert.Greater(p.h, 0, p.id);
                if (p.slice != null)
                    Assert.IsTrue(p.slice[0] + p.slice[2] < p.w && p.slice[1] + p.slice[3] < p.h, p.id + ": the cuts leave no middle");
            }
            var skin = new FrameSkin(art, pieces);
            if (skin.RuleCount == 0)
                Assert.Ignore("Art/hud_assign.json has no \"uss\" fields yet (Bake/art/artframe.py does not write the places)");
            CollectionAssert.IsEmpty(skin.Skipped, "a place that is not classes, :hover and :active");
        }

        // ------------------------------------------------------------------ THE RULE

        [Test]
        public void APieceIsUsed_OnlyWhenTheUserHasDrawnIt()
        {
            // every piece has its placeholder; the user has drawn the button at rest alone
            foreach (var p in FrameSkin.Read(Json))
                Png(holder, p.file, p.w, p.h);
            Png(final, "hud/hud.fn_button.png", 36, 24);
            var frame = El(null, "frame-root");
            var rest = El(frame, "frame-verb");
            var on = El(frame, "frame-verb", "on");
            var off = El(frame, "frame-verb", "off");
            var skin = new FrameSkin(art, FrameSkin.Read(Json));
            Assert.AreEqual(8, skin.RuleCount, "eight places read (one piece has two, one has none, one has two that are not understood)");
            CollectionAssert.AreEquivalent(new[] { "Button.frame-k", "#menu" }, skin.Skipped);
            skin.Attach(frame);

            // the drawn piece is laid, cut in nine, each of its pixels three of the screen's
            Assert.IsNotNull(rest.style.backgroundImage.value.texture, "the drawn piece is laid");
            Assert.AreEqual(36, rest.style.backgroundImage.value.texture.width);
            Assert.AreEqual(6, rest.style.unitySliceLeft.value);
            Assert.AreEqual(5, rest.style.unitySliceTop.value);
            Assert.AreEqual(4, rest.style.unitySliceRight.value);
            Assert.AreEqual(3, rest.style.unitySliceBottom.value);
            Assert.AreEqual(3f, rest.style.unitySliceScale.value, 1e-4);
            Assert.AreEqual(Color.clear, rest.style.backgroundColor.value, "the stylesheet's ground stands down");
            Assert.AreEqual("hud.fn_button", skin.PieceFor(rest).id);

            // a button that is on has a piece of its own, not drawn yet: it keeps its stylesheet look (never the placeholder,
            // and not the plain button's picture either, or on and off could not be told apart)
            Assert.AreEqual("hud.fn_button_down", skin.PlaceFor(on).id);
            Assert.IsNull(skin.PieceFor(on));
            Assert.AreEqual(StyleKeyword.Null, on.style.backgroundImage.keyword);
            Assert.AreEqual(StyleKeyword.Null, on.style.backgroundColor.keyword);
            Assert.AreEqual(StyleKeyword.Null, off.style.backgroundImage.keyword);

            // the user draws it: seen while the game runs
            Png(final, "hud/hud.fn_button_down.png", 36, 24);
            art.Look();
            skin.Apply();
            Assert.IsNotNull(on.style.backgroundImage.value.texture, "drawn now: laid");
            Assert.AreEqual(6, on.style.unitySliceTop.value);

            // the button is switched off again: it takes the plain piece
            on.RemoveFromClassList("on");
            skin.Apply();
            Assert.AreEqual("hud.fn_button", skin.PieceFor(on).id);
            Assert.AreEqual(5, on.style.unitySliceTop.value);

            // the user's picture is taken away: the stylesheet look comes back
            File.Delete(Path.Combine(final, "hud/hud.fn_button.png"));
            art.Look();
            skin.Apply();
            Assert.AreEqual(StyleKeyword.Null, rest.style.backgroundImage.keyword);
            Assert.AreEqual(StyleKeyword.Null, rest.style.unitySliceLeft.keyword);
            Assert.AreEqual(StyleKeyword.Null, rest.style.backgroundColor.keyword);

            skin.Dispose();
        }

        // ------------------------------------------------------------------ which element takes which piece

        [Test]
        public void AnIcon_FindsItsButton_ByTheFunctionItIsNow()
        {
            Png(final, "hud/hud.fn.cast_off.png", 16, 16);
            Png(final, "hud/hud.fn.lie_to.png", 16, 16);
            var frame = El(null, "frame-root");
            var button = El(frame, "frame-verb");
            var icon = El(button, "frame-icon");
            var skin = new FrameSkin(art, FrameSkin.Read(Json));
            skin.Attach(frame);
            Assert.IsNull(skin.PlaceFor(icon), "the button has not said which function it is");
            Assert.IsFalse(FrameSkin.Skinned(icon));

            FrameSkin.Verb(button, "CastOff", "Cast off");
            Assert.IsTrue(button.ClassListContains("fn-cast_off"));
            skin.Apply();
            Assert.AreEqual("hud.fn.cast_off", skin.PieceFor(icon).id);
            Assert.IsTrue(FrameSkin.Skinned(icon), "its own drawing stands down");
            Assert.IsNotNull(icon.style.backgroundImage.value.texture);

            // at sea the same button is Lie to: another class, another piece
            FrameSkin.Verb(button, "CastOff", "Lie to");
            Assert.IsTrue(button.ClassListContains("fn-lie_to"));
            Assert.IsFalse(button.ClassListContains("fn-cast_off"));
            skin.Apply();
            Assert.AreEqual("hud.fn.lie_to", skin.PieceFor(icon).id);

            Assert.AreEqual("look_out", FrameSkin.Slug("LookOut"));
            Assert.AreEqual("oars", FrameSkin.Slug("Oars"));

            skin.Detach();
            Assert.IsFalse(FrameSkin.Skinned(icon), "let go: every element as its stylesheet has it");
            Assert.AreEqual(StyleKeyword.Null, icon.style.backgroundImage.keyword);
        }

        [Test]
        public void APlaceForAnIcon_TakesNoRoom_UntilItsPieceIsDrawn()
        {
            var frame = El(null, "frame-root");
            var win = El(frame, "frame-win", "panel-voyage");
            var place = FrameSkin.Place("frame-win-icon");
            win.Add(place);
            var other = El(frame, "frame-win", "panel-trade");
            var otherPlace = FrameSkin.Place("frame-win-icon");
            other.Add(otherPlace);
            var skin = new FrameSkin(art, FrameSkin.Read(Json));
            skin.Attach(frame);
            Assert.AreEqual(DisplayStyle.None, place.style.display.value, "not drawn: no room taken");

            Png(final, "hud/hud.panel.voyage.png", 16, 16);
            art.Look();
            skin.Apply();
            Assert.AreEqual(DisplayStyle.Flex, place.style.display.value);
            Assert.AreEqual(16f * skin.PlaceScale, place.style.width.value.value, 1e-4);
            Assert.AreEqual(DisplayStyle.None, otherPlace.style.display.value, "the Trade window's icon is not drawn");
            Assert.IsNull(skin.PlaceFor(otherPlace));
        }

        [Test]
        public void AFrameLaidOver_IsALayerOnTop_AndGoesAgain()
        {
            Png(final, "hud/hud.frame_face.png", 68, 84);
            var frame = El(null, "frame-root");
            var face = El(frame, "frame-face");
            var skin = new FrameSkin(art, FrameSkin.Read(Json));
            skin.Attach(frame);
            Assert.AreEqual(1, face.hierarchy.childCount, "a layer over the face");
            Assert.IsNotNull(face.hierarchy[0].style.backgroundImage.value.texture);
            Assert.AreEqual(StyleKeyword.Null, face.style.backgroundImage.keyword, "the face's own picture is not touched");
            skin.Apply();
            skin.Apply();
            Assert.AreEqual(1, face.hierarchy.childCount, "laid once, however often the frame is walked");

            File.Delete(Path.Combine(final, "hud/hud.frame_face.png"));
            art.Look();
            skin.Apply();
            Assert.AreEqual(0, face.hierarchy.childCount, "taken off again");
        }
    }
}
