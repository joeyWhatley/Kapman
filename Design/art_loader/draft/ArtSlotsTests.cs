// PROJECT PATH: Assets/Kapman/Tests/EditMode/ArtSlotsTests.cs
// (a draft of 7 Oct 2026, written outside Assets: Design/art_loader/PLAN.md says how to bring it in)
using System;
using System.IO;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// The pictures by their slots (ArtSlots, ArtFiles), on a folder of small pictures made for the test: the order of
    /// falling back (ruling 13 of 7 Oct 2026), pixel art read as it is, a picture that changes on disk seen while the
    /// game runs, what nothing shows let go, and the slots' file names as make_art_list.py writes them.
    /// </summary>
    public class ArtSlotsTests
    {
        static readonly Color32 Red = new Color32(200, 30, 30, 255), Green = new Color32(30, 180, 60, 255), Blue = new Color32(40, 60, 200, 255),
                                Gold = new Color32(240, 210, 122, 255), Plum = new Color32(112, 41, 99, 255);

        const string Own = "portrait/person.testport.tollman.png", RoleFile = "portrait/role.tollman.png";

        string root, final, holder;
        ArtSlots art;
        float clock;

        [SetUp]
        public void MakeAFolder()
        {
            root = Path.Combine(Path.GetTempPath(), "kapman_art_" + Guid.NewGuid().ToString("N"));
            final = Path.Combine(root, "final");                           // (not made yet: as before the first pull_art.py)
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

        /// <summary>Writes a small picture of one colour, a little later than anything written before it.</summary>
        string Png(string folder, string file, Color32 colour, int w = 4, int h = 4)
        {
            string path = Path.Combine(folder, file);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++)
                px[i] = colour;
            t.SetPixels32(px);
            t.Apply();
            File.WriteAllBytes(path, t.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(t);
            stamp = stamp.AddSeconds(3);
            File.SetLastWriteTimeUtc(path, stamp);
            Directory.SetLastWriteTimeUtc(Path.GetDirectoryName(path), stamp);   // (its folder too: two pictures made in the same moment are still told apart)
            return path;
        }

        DateTime stamp = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc);

        static void Same(Color32 want, Texture2D tex, string what)
        {
            Assert.IsNotNull(tex, what + ": no picture");
            var got = tex.GetPixels32()[0];
            Assert.IsTrue(got.r == want.r && got.g == want.g && got.b == want.b, $"{what}: the picture is {got}, not {want}");
        }

        // ------------------------------------------------------------------ the order of falling back

        [Test]
        public void TheOrder_TheUsersOwn_ThenTheUsersStandIn_ThenThePlaceholders()
        {
            // nothing at all: no picture (a card with none still works)
            Assert.IsNull(art.Picture(Own, RoleFile));
            Assert.IsNull(art.Source(Own, RoleFile));
            Assert.IsNull(art.CardPicture("no.such.card", "sea"));

            // only the role's placeholder: everyone of the role shows it
            Png(holder, RoleFile, Plum);
            art.Look();
            Same(Plum, art.Picture(Own, RoleFile), "the role's placeholder");

            // the person's own placeholder comes before the role's
            Png(holder, Own, Red);
            art.Look();
            Same(Red, art.Picture(Own, RoleFile), "the person's own placeholder");
            StringAssert.Contains("placeholder", art.Source(Own, RoleFile));

            // the user draws the stand-in: it covers everyone who has no drawn portrait of their own
            Png(final, RoleFile, Gold);
            art.Look();
            Same(Gold, art.Picture(Own, RoleFile), "the user's stand-in, before the person's own placeholder");
            Assert.IsFalse(art.Drawn(Own));
            Assert.IsTrue(art.Drawn(RoleFile));

            // the user draws the person: their own picture wins
            Png(final, Own, Green);
            art.Look();
            Same(Green, art.Picture(Own, RoleFile), "the user's own picture of the person");
            Assert.IsTrue(art.Drawn(Own));

            // asked for alone, a slot is its final, else its placeholder
            Same(Green, art.Picture(Own), "final wins over the placeholder of the same name");
            Same(Gold, art.Portrait("portrait/person.nobody.png", "tollman"), "a person with no slot: the role");
            Same(Gold, art.Final(RoleFile), "only the user's own");
            Assert.IsNull(art.Final("portrait/role.smith.png"));
        }

        [Test]
        public void TheOtherReading_ASlotsOwnPlaceholderBeforeAnyStandIn()
        {
            art.Files.StandInBeforePlaceholder = false;
            Png(holder, Own, Red);
            Png(holder, RoleFile, Plum);
            Png(final, RoleFile, Gold);
            Same(Red, art.Picture(Own, RoleFile), "the person's own placeholder first");
            Same(Gold, art.Picture("portrait/person.nobody.png", RoleFile), "then the stand-in, the user's before its placeholder");
        }

        [Test]
        public void ACard_ItsOwnPicture_ElseItsStage_ASite_ItsOwnScene_ElseItsKind()
        {
            Png(holder, ArtSlots.CardAny("sea"), Blue, 8, 4);
            Png(holder, ArtSlots.Card("sea.fog.bank"), Red, 8, 4);
            Png(holder, ArtSlots.SceneAny("tavern"), Plum, 8, 4);
            Same(Red, art.CardPicture("sea.fog.bank", "sea"), "a card's own picture");
            Same(Blue, art.CardPicture("sea.another", "Sea"), "a card with none: its stage's (the code's Stage.Sea reads the same)");
            Assert.IsNull(art.CardPicture("crew.card", "crew"), "no picture of its own and none for its stage: none");
            Same(Plum, art.Scene("testport.alehouse", "tavern"), "a site with no scene: its kind's");
        }

        [Test]
        public void PixelArt_IsReadAsItIs()
        {
            Png(holder, Own, Red, 64, 80);
            var tex = art.Picture(Own);
            Assert.IsNotNull(tex);
            Assert.AreEqual(64, tex.width);
            Assert.AreEqual(80, tex.height);
            Assert.AreEqual(FilterMode.Point, tex.filterMode, "never smoothed");
            Assert.AreEqual(1, tex.mipmapCount, "no mipmaps");
            Assert.AreEqual(TextureWrapMode.Clamp, tex.wrapMode);
        }

        // ------------------------------------------------------------------ changes on disk

        [Test]
        public void APictureSavedAgain_IsSeen_InTheSameTexture()
        {
            Png(holder, Own, Red);
            var tex = art.Picture(Own);
            Same(Red, tex, "at first");
            int version = art.Version, told = 0;
            art.Changed += () => told++;

            art.Look();
            Assert.AreEqual(version, art.Version, "nothing changed: nothing said");

            Png(holder, Own, Green);                                       // (the user saves it again)
            art.Look();
            Assert.AreSame(tex, art.Picture(Own), "the same texture, so whatever shows it shows the new picture");
            Same(Green, tex, "after it was saved again");
            Assert.AreEqual(version + 1, art.Version);
            Assert.AreEqual(1, told);
        }

        [Test]
        public void AHalfSavedPicture_LeavesWhatWasThere_UntilItIsWhole()
        {
            string path = Png(holder, Own, Red);
            var tex = art.Picture(Own);
            byte[] whole = File.ReadAllBytes(Png(Path.Combine(root, "elsewhere"), Own, Green));
            var half = new byte[whole.Length / 2];
            Array.Copy(whole, half, half.Length);
            File.WriteAllBytes(path, half);
            File.SetLastWriteTimeUtc(path, stamp.AddSeconds(10));
            art.Look();
            Same(Red, tex, "half a file: the old picture stays");
            File.WriteAllBytes(path, whole);
            File.SetLastWriteTimeUtc(path, stamp.AddSeconds(20));
            art.Look();
            Same(Green, tex, "the whole file: the new picture");
            Assert.IsFalse(ArtSlots.WholePng(half));
            Assert.IsTrue(ArtSlots.WholePng(whole));
        }

        [Test]
        public void ANewFinal_TakesThePlaceholdersPlace_OnTheElementThatShowsIt()
        {
            Png(holder, Own, Red);
            var el = new VisualElement();
            Assert.IsTrue(art.Show(el, Own, RoleFile));
            Assert.IsTrue(art.Shows(el));
            Same(Red, el.style.backgroundImage.value.texture, "the placeholder on the element");

            Png(final, Own, Blue);                                         // (pull_art.py brings the user's picture)
            art.Look();
            Same(Blue, el.style.backgroundImage.value.texture, "the user's picture on the element, without asking again");

            File.Delete(Path.Combine(final, Own));                         // (and it is taken away again)
            art.Look();
            Same(Red, el.style.backgroundImage.value.texture, "back to the placeholder");

            art.Clear(el);
            Assert.IsFalse(art.Shows(el));
            Assert.AreEqual(StyleKeyword.Null, el.style.backgroundImage.keyword);
        }

        [Test]
        public void AnElementThatBecomesThePicture_TakesItsSize()
        {
            Png(holder, ArtSlots.PortMark("testport"), Gold, 16, 16);
            var dot = new VisualElement();
            Assert.IsTrue(art.ShowSized(dot, 2f, ArtSlots.PortMark("testport")));
            Assert.AreEqual(32f, dot.style.width.value.value, 1e-4);
            Assert.AreEqual(32f, dot.style.height.value.value, 1e-4);
            Assert.AreEqual(0f, dot.style.borderLeftWidth.value, 1e-4);
            // no picture: as its stylesheet has it
            var bare = new VisualElement();
            Assert.IsFalse(art.ShowSized(bare, 2f, ArtSlots.PortMark("nowhere")));
            Assert.AreEqual(StyleKeyword.Null, bare.style.width.keyword);
            // a made element with no picture takes no room
            Assert.AreEqual(DisplayStyle.None, art.Element(ArtSlots.Good("nothing")).style.display.value);
        }

        // ------------------------------------------------------------------ the cache

        [Test]
        public void WhatNothingShows_IsLetGo_AndWhatIsKeptStays()
        {
            Png(holder, Own, Red);
            Png(holder, RoleFile, Plum);
            var seen = art.Picture(Own);
            var kept = art.Keep(RoleFile);
            Assert.AreEqual(2, art.Held);

            clock += art.KeepSeconds * 0.5f;
            art.Look();
            Assert.AreEqual(2, art.Held, "asked for a moment ago: still held");

            clock += art.KeepSeconds + 1f;
            art.Look();
            Assert.AreEqual(1, art.Held, "nothing shows the first any more: let go");
            Assert.IsTrue(seen == null, "its texture is destroyed");
            Assert.IsTrue(kept != null, "the kept one stays");

            art.Release(RoleFile);
            clock += art.KeepSeconds + 1f;
            art.Look();
            Assert.AreEqual(0, art.Held);
            // asked for again, it is read again
            Same(Red, art.Picture(Own), "read again");
        }

        // ------------------------------------------------------------------ sprites and cuts

        [Test]
        public void ASprite_CarriesTheCuts_AsItsBorder()
        {
            Png(holder, ArtSlots.Hud("fn_button"), Plum, 36, 24);
            // hud_assign.json: left, top, right, bottom from the picture's top left; a sprite's border: left, bottom, right, top
            var sprite = art.SpriteOf(ArtSlots.Hud("fn_button"), new[] { 1, 2, 3, 4 });
            Assert.IsNotNull(sprite);
            Assert.AreEqual(new Vector4(1f, 4f, 3f, 2f), sprite.border);
            Assert.AreEqual(new Rect(0f, 0f, 36f, 24f), sprite.rect);
            Assert.AreSame(sprite, art.SpriteOf(ArtSlots.Hud("fn_button"), new[] { 1, 2, 3, 4 }), "made once");
            Assert.AreEqual(Vector4.zero, art.SpriteOf(ArtSlots.Hud("fn_button")).border, "not cut");
            Assert.AreEqual(Vector4.zero, ArtSlots.Border(new[] { 20, 0, 20, 0 }, 36, 24), "cuts that do not fit the picture: none");
            Assert.AreEqual(new Vector4(6f, 6f, 6f, 6f), ArtSlots.Border(new[] { 6, 6, 6, 6 }, 36, 24));
        }

        // ------------------------------------------------------------------ drawing with the game open

        [Test]
        public void TheDrawingFolder_CountsOnlyWhatTheUserHasDrawn()
        {
            string desk = Path.Combine(root, "desk");
            string place = Png(holder, Own, Red, 8, 10);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(desk, Own)));
            File.Copy(place, Path.Combine(desk, Own));                     // (the placeholder as make_art_list.py puts it on the Desktop)
            art.Files.DrawFolder = desk;
            Assert.IsFalse(art.Files.Drawn(Own), "still the placeholder: not the user's");

            Png(desk, Own, Green, 4, 4);
            Assert.IsFalse(art.Files.Drawn(Own), "another size than the slot's: not taken");

            string mine = Png(desk, Own, Green, 8, 10);
            Assert.IsTrue(art.Files.Drawn(Own), "the slot's size and no longer the placeholder: the user's");
            Assert.AreEqual(Path.GetFullPath(mine), Path.GetFullPath(art.Files.Final(Own)));

            Png(final, Own, Blue, 8, 10);
            Assert.AreEqual(Path.GetFullPath(mine), Path.GetFullPath(art.Files.Final(Own)), "what is being drawn comes before what was pulled");
            Assert.IsTrue(ArtFiles.PngSize(mine, out int w, out int h) && w == 8 && h == 10);
        }

        // ------------------------------------------------------------------ the names

        [Test]
        public void TheNames_AreTheArtListsOwn()
        {
            Assert.AreEqual("portrait/person.dorestad.tollman.png", ArtSlots.Person("dorestad.tollman"));
            Assert.AreEqual("portrait/weight.offa.png", ArtSlots.Weight("offa"));
            Assert.AreEqual("portrait/self.frisian.man.png", ArtSlots.Self("", false));
            Assert.AreEqual("portrait/self.danish.woman.png", ArtSlots.Self("danish", true));
            Assert.AreEqual("portrait/role.tollman.png", ArtSlots.Role("tollman"));
            Assert.IsNull(ArtSlots.Role(""));
            Assert.AreEqual("card/card.any.harbour.png", ArtSlots.CardAny("Harbour"));
            Assert.AreEqual("card/card.crew.bored.calm.png", ArtSlots.Card("crew.bored.calm"));
            Assert.AreEqual("scene/site.dorestad.market.png", ArtSlots.Site("dorestad.market"));
            Assert.AreEqual("scene/scene.any.tavern.png", ArtSlots.SceneAny("tavern"));
            Assert.AreEqual("hud/hud.fn.cast_off.png", ArtSlots.Hud("fn.cast_off"));
            Assert.AreEqual("shipview/shipview.back.sea.day.png", ArtSlots.ShipBack("sea", "day"));
            // make_art_list.py's safe_id: a run of anything else is one underscore, no dot or underscore at the ends
            Assert.AreEqual("a_b_c", ArtSlots.Safe("a b/&c"));
            Assert.AreEqual("x.y", ArtSlots.Safe("..x.y__"));
            Assert.AreEqual("x", ArtSlots.Safe(""));
            // what a data file may say: an id of the kind, or the file itself
            Assert.AreEqual("portrait/person.x.png", ArtSlots.FileOf("portrait", "person.x"));
            Assert.AreEqual("portrait/role.monk.png", ArtSlots.FileOf("card", "portrait/role.monk.png"));
            Assert.IsNull(ArtSlots.FileOf("card", ""));
            // nothing leaves the art folders
            Assert.IsFalse(ArtFiles.Valid("../secret.png"));
            Assert.IsFalse(ArtFiles.Valid("C:/x.png"));
            Assert.IsTrue(ArtFiles.Valid("good/good.wax.png"));
        }

        /// <summary>The names above against the placeholders the project really has (Art/placeholder beside Assets).</summary>
        [Test]
        public void TheProjectsOwnPlaceholders_AreFoundByTheseNames()
        {
            string folder = Path.Combine(Application.dataPath, "..", "Art", "placeholder");
            if (!Directory.Exists(folder))
                Assert.Ignore("no Art/placeholder on this machine (Bake/art/make_art_list.py)");
            var files = new ArtFiles(Path.Combine(folder, "..", "final"), folder);
            foreach (string file in new[]
                     {
                         ArtSlots.Self("frisian", false), ArtSlots.Self("frisian", true), ArtSlots.Role("tollman"), ArtSlots.Person("dorestad.tollman"),
                         ArtSlots.Weight("offa"), ArtSlots.CardAny("harbour"), ArtSlots.CardAny("sea"), ArtSlots.CardAny("crew"), ArtSlots.CardAny("long"),
                         ArtSlots.Site("dorestad.market"), ArtSlots.SceneAny("market"), ArtSlots.Good("wax"), ArtSlots.PortMark("dorestad"),
                         ArtSlots.Harbour("dorestad"), ArtSlots.ShipMark("knarr"), ArtSlots.ShipSide("knarr"), ArtSlots.Hud("fn_button"),
                         ArtSlots.Hud("fn.cast_off"), ArtSlots.Hud("panel.voyage"), ArtSlots.ShipBack("sea", "day"),
                     })
                Assert.IsNotNull(files.Placeholder(file), file + " has no placeholder: the name the game asks for is not the art list's");
            foreach (string port in new[] { "dorestad", "traiectum", "walichrum", "lundenwic", "witla", "medemelacha", "doccinga", "daventre", "quentovic", "sarre", "fordwic", "ripa" })
            {
                Assert.IsNotNull(files.Placeholder(ArtSlots.PortMark(port)), "no mark for " + port);
                Assert.IsNotNull(files.Placeholder(ArtSlots.Harbour(port)), "no harbour view for " + port);
            }
        }
    }
}
