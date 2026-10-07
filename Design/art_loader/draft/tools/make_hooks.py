# -*- coding: utf-8 -*-
r"""
THE HOOK LINES (7 Oct 2026): the few lines in the game's own files that bring the art loader in, in six steps.

Run plainly, it reads the project's files and never writes them. It makes hooked COPIES of the files it names in a
folder outside the project (to compile-check the lines), and writes the changes as one diff a step, beside this
script's folder:
    ../hooks_1_portraits.diff   ../hooks_2_card_pictures.diff   ../hooks_3_goods.diff
    ../hooks_4_harbour.diff     ../hooks_5_chart_marks.diff     ../hooks_6_frame_skin.diff
Each diff is made against the files with the steps before it already in, so they are applied in order. The diffs
and the copies keep each file's own line endings.
A hook whose old line is no longer in the file (another builder changed it) is reported as NOT FOUND and left out.
A hook that is in the file already is passed over, so this can be run again after some steps are in.

  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Design\art_loader\draft\tools\make_hooks.py [folder for the copies] [--upto N]
  ... make_hooks.py --apply N

Without a folder the copies go to the temp folder (kapman_art_hooks).
--upto N    the copies hold steps 1 to N only (the diffs are not written then).
--apply N   THE ONE WAY THIS WRITES INTO THE PROJECT: step N's lines are put into the game's own files, each file
            keeping its line endings. If any line of the step is not found, nothing is written. For the lead, one step
            at a time, in order, with a compile and a look after each.
"""
import difflib
import io
import os
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
P = os.path.normpath(os.path.join(HERE, "..", "..", "..", "..")).replace("\\", "/")
GAME = os.environ.get("KAPMAN_HOOKS_GAME") or P + "/Assets/Kapman/Code/Game"   # (the variable: a copy of the folder, to try --apply on)
NAMES = {1: "portraits", 2: "card_pictures", 3: "goods", 4: "harbour", 5: "chart_marks", 6: "frame_skin"}


def _args():
    out, upto, apply_step, argv = None, 99, 0, sys.argv[1:]
    i = 0
    while i < len(argv):
        if argv[i] == "--upto" and i + 1 < len(argv):
            upto = int(argv[i + 1])
            i += 2
        elif argv[i] == "--apply" and i + 1 < len(argv):
            apply_step = int(argv[i + 1])
            i += 2
        else:
            out = argv[i]
            i += 1
    return out or os.path.join(tempfile.gettempdir(), "kapman_art_hooks"), upto, apply_step


OUT, UPTO, APPLY = _args()

HOOKS = []          # (step, file, old, new)
STEP = [1]


def step(n):
    STEP[0] = n


def hook(rel, old, new):
    HOOKS.append((STEP[0], rel, old, new))


# ================================================================ 1  PORTRAITS BY THEIR SLOT, THROUGH ArtSlots
# (needs ArtFiles.cs and ArtSlots.cs in the project; after it FrameArt.cs has no user left and is deleted)
step(1)
hook("UI/Frame/FrameDrawn.cs",
     "        string who = \"\", slot = \"\";\n        Texture2D picture;\n",
     "        string who = \"\", slot = \"\";\n"
     "        /// <summary>The slot shown where this one has no picture: the stand-in of the person's role (ArtSlots.Role). Set it\n"
     "        /// before <see cref=\"Who\"/> or <see cref=\"Slot\"/>.</summary>\n"
     "        public string StandIn = \"\";\n")
hook("UI/Frame/FrameDrawn.cs",
     "                Slot = who == \"you\" ? FrameArt.Self(people != null ? people.people : \"frisian\", false) : \"\";\n",
     "                Slot = who == \"you\" ? ArtSlots.Self(people != null ? people.people : \"frisian\", false) : \"\";\n")
hook("UI/Frame/FrameDrawn.cs",
     "                slot = value ?? \"\";\n                picture = FrameArt.Load(slot);\n"
     "                style.backgroundImage = picture != null ? new StyleBackground(picture) : new StyleBackground(StyleKeyword.None);\n"
     "                MarkDirtyRepaint();\n",
     "                slot = value ?? \"\";\n"
     "                ArtSlots.Shared.Show(this, slot, StandIn);               // (the slot's picture, else the stand-in's: kept and followed while on screen)\n"
     "                MarkDirtyRepaint();\n")
hook("UI/Frame/FrameDrawn.cs",
     "            if (picture != null || r.width < 4f || r.height < 4f)\n",
     "            if (ArtSlots.Shared.Shows(this) || r.width < 4f || r.height < 4f)\n")
hook("UI/Frame/FrameRoot.cs",
     "            SelfUpdate();\n            FloatsUpdate();\n",
     "            SelfUpdate();\n            FloatsUpdate();\n"
     "            ArtSlots.Shared.Pump();                                      // (a picture saved again on disk is seen; what is not shown is let go)\n")
hook("UI/Frame/FrameRoot.cs",
     "            public string name, line1, line2, line3, say, face;\n",
     "            public string name, line1, line2, line3, say, face;\n"
     "            /// <summary>The slot file of their portrait and of its stand-in, the role's (ArtSlots); null: none.</summary>\n"
     "            public string slot, standIn;\n")
hook("UI/Frame/FrameRoot.cs",
     "            face.Who = now.face ?? now.name ?? \"\";\n",
     "            face.StandIn = now.standIn ?? \"\";\n            face.Who = now.face ?? now.name ?? \"\";\n"
     "            if (!string.IsNullOrEmpty(now.slot) && face.Slot != now.slot)\n                face.Slot = now.slot;\n")
hook("UI/Frame/FrameRoot.Floats.cs",
     "        public string face;\n",
     "        public string face;\n"
     "        /// <summary>The slot files (ArtSlots) of the speaker's portrait and of its stand-in, the role's; and of the picture\n"
     "        /// across the top with its stand-in (a card's own, and its stage's). Null: none.</summary>\n"
     "        public string portrait, portraitAny, picture, pictureAny;\n")
hook("UI/Frame/FrameRoot.Floats.cs",
     "            if (spec.face != null)\n                top.Add(new FrameFace(\"frame-float-face\") { Who = spec.face });\n",
     "            if (spec.face != null)\n            {\n"
     "                var face = new FrameFace(\"frame-float-face\") { StandIn = spec.portraitAny ?? \"\", Who = spec.face };\n"
     "                if (!string.IsNullOrEmpty(spec.portrait))\n                    face.Slot = spec.portrait;\n"
     "                top.Add(face);\n            }\n")
hook("UI/Frame/FrameRoot.Floats.cs",
     "                spec.face = first.id;\n",
     "                spec.face = first.id;\n"
     "                // (their own portrait, else their role's stand-in; card_rules_793.json's \"portrait\" goes here when it is loaded)\n"
     "                spec.portrait = ArtSlots.Person(first.id);\n"
     "                spec.portraitAny = ArtSlots.Role(first.role);\n")
hook("UI/Frame/FrameRoot.Floats.cs",
     "say = say, face = s.face ?? s.speaker };\n",
     "say = say, face = s.face ?? s.speaker, slot = s.portrait, standIn = s.portraitAny };\n")
hook("UI/Frame/PersonTalk.cs",
     "                face = p.id, mustAnswer = false, holdsClock = false, shut = leave,\n",
     "                face = p.id, mustAnswer = false, holdsClock = false, shut = leave,\n"
     "                portrait = ArtSlots.Person(p.id), portraitAny = ArtSlots.Role(p.role),   // (their own portrait, else the stand-in of their role)\n")

# ================================================================ 2  A CARD'S PICTURE ACROSS THE TOP OF ITS WINDOW
step(2)
hook("UI/Frame/FrameRoot.Floats.cs",
     "            el.Add(top);\n",
     "            if (!string.IsNullOrEmpty(spec.picture) || !string.IsNullOrEmpty(spec.pictureAny))\n"
     "                el.Add(ArtSlots.Shared.Banner(spec.picture, spec.pictureAny, 2f));   // (192 x 108 shown at twice its size; no room taken when there is none)\n"
     "            el.Add(top);\n")
hook("UI/Frame/FrameRoot.Floats.cs",
     "            var spec = new FloatSpec { id = CardId, title = s.Title, holdsClock = !s.Chosen, mustAnswer = !s.Chosen };\n",
     "            var spec = new FloatSpec { id = CardId, title = s.Title, holdsClock = !s.Chosen, mustAnswer = !s.Chosen };\n"
     "            // (the card's own picture, else its stage's stand-in; card_rules_793.json's \"picture\" goes here when it is loaded)\n"
     "            spec.picture = ArtSlots.Card(s.card.id);\n"
     "            spec.pictureAny = ArtSlots.CardAny(s.card.stage.ToString());\n")

# ================================================================ 3  GOODS IN TRADE AND HOLD
step(3)
hook("UI/Frame/Panels/TradePanel.cs",
     "                    body.Add(FrameUi.Ln(FrameGame.Units(good, count), null));\n",
     "                    body.Add(ArtSlots.Shared.Before(FrameUi.Ln(FrameGame.Units(good, count), null), ArtSlots.Good(good.id)));\n")
hook("UI/Frame/Panels/TradePanel.cs",
     "                var line = FrameUi.Ln(good.name, asks + pays + unit, held > 0 ? FrameUi.Num(held.ToString()) : null, buy, sell);\n",
     "                var line = ArtSlots.Shared.Before(FrameUi.Ln(good.name, asks + pays + unit, held > 0 ? FrameUi.Num(held.ToString()) : null, buy, sell),\n"
     "                                                  ArtSlots.Good(good.id));\n")
hook("UI/Frame/Panels/HoldPanel.cs",
     "                body.Add(FrameUi.Ln(FrameGame.Units(good, count), small, FrameUi.Num($\"{count * good.kg:0} kg\")));\n",
     "                body.Add(ArtSlots.Shared.Before(FrameUi.Ln(FrameGame.Units(good, count), small, FrameUi.Num($\"{count * good.kg:0} kg\")), ArtSlots.Good(good.id)));\n")

# ================================================================ 4  THE HARBOUR VIEW AT THE TOP OF THE PORT WINDOW
step(4)
hook("UI/Frame/Panels/PortPanel.cs",
     "            body.Add(FrameUi.H(here.Title));\n",
     "            body.Add(ArtSlots.Shared.Banner(ArtSlots.Harbour(here.id), null, 1f));   // (the port from the water, shrunk to the window's width)\n"
     "            body.Add(FrameUi.H(here.Title));\n")

# ================================================================ 5  THE CHART: A PORT'S MARK, HER OWN MARK, A LANDMARK
step(5)
hook("Map/MapView.cs",
     "                    dot.AddToClassList(\"map-port-dot\");\n",
     "                    dot.AddToClassList(\"map-port-dot\");\n"
     "                    ArtSlots.Shared.ShowSized(dot, 2f, ArtSlots.PortMark(def.id));   // (the port's own mark, 16 x 16 shown at twice its size)\n")
hook("Map/MapView.cs",
     "            shipMark.AddToClassList(\"map-ship\");\n",
     "            shipMark.AddToClassList(\"map-ship\");\n"
     "            // (her class from above, 32 x 16, bow to the right: the turn below is already made for that)\n"
     "            ArtSlots.Shared.ShowSized(shipMark, 1f, ArtSlots.ShipMark(ChartBody.Current != null ? ChartBody.Current.classId : \"gislinge\"));\n")
hook("Map/MapView.cs",
     "            shipMark.style.left = (float)((w.x - viewWest) / mpp) - 9f;\n            shipMark.style.top = (float)((viewNorth - w.z) / mpp) - 4f;\n",
     "            // (centred on her place whatever the mark's size: 18 x 8 as the stylesheet has it, 32 x 16 as a picture)\n"
     "            float halfW = float.IsNaN(shipMark.resolvedStyle.width) ? 9f : shipMark.resolvedStyle.width * 0.5f;\n"
     "            float halfH = float.IsNaN(shipMark.resolvedStyle.height) ? 4f : shipMark.resolvedStyle.height * 0.5f;\n"
     "            shipMark.style.left = (float)((w.x - viewWest) / mpp) - halfW;\n            shipMark.style.top = (float)((viewNorth - w.z) / mpp) - halfH;\n")
hook("Map/MapView.cs",
     "                            mark.AddToClassList(\"map-landmark\");\n",
     "                            mark.AddToClassList(\"map-landmark\");\n"
     "                            ArtSlots.Shared.ShowFinalSized(mark, 2f, ArtSlots.Hud(\"landmark\"));   // (a piece of the frame: only once it is drawn)\n")

# ================================================================ 6  THE FRAME'S OWN PIECES (needs FrameSkin.cs in the project)
step(6)
hook("UI/Frame/FrameRoot.cs",
     "        FrameShipView shipView;\n",
     "        FrameShipView shipView;\n        FrameSkin skin;\n"
     "        /// <summary>The user's own pieces laid over the frame (null before the frame is built).</summary>\n"
     "        public FrameSkin Skin => skin;\n")
hook("UI/Frame/FrameRoot.cs",
     "            MeasureBuild();\n            FloatsBuild();\n",
     "            MeasureBuild();\n            FloatsBuild();\n            skin?.Dispose();\n"
     "            skin = FrameSkin.ForFrame(root);                             // (Art/hud_assign.json: a piece is used once it is drawn)\n")
hook("UI/Frame/FrameRoot.cs",
     "            MeasureDisable();\n            FloatsDisable();\n",
     "            MeasureDisable();\n            FloatsDisable();\n            skin?.Dispose();\n            skin = null;\n")
hook("UI/Frame/FrameRoot.cs",
     "            ArtSlots.Shared.Pump();                                      // (a picture saved again on disk is seen; what is not shown is let go)\n",
     "            ArtSlots.Shared.Pump();                                      // (a picture saved again on disk is seen; what is not shown is let go)\n"
     "            skin?.Tick();\n")
hook("UI/Frame/FrameRoot.cs",
     "                wmbar.Add(add);\n            }\n        }\n",
     "                wmbar.Add(add);\n            }\n            skin?.Apply(left);\n        }\n")
hook("UI/Frame/FrameRoot.cs",
     "            win.userData = index;\n",
     "            win.userData = index;\n            win.AddToClassList(\"panel-\" + id);                           // (so the panel's own icon finds its window)\n")
hook("UI/Frame/FrameRoot.cs",
     "            title.Add(name);\n",
     "            title.Add(FrameSkin.Place(\"frame-win-icon\"));               // (no room taken until hud.panel.<id> is drawn)\n            title.Add(name);\n")
hook("UI/Frame/FramePanel.cs",
     "                Fill(Body);\n",
     "                Fill(Body);\n                Frame?.Skin?.Apply(Body);                                // (the user's own pieces on the new lines at once)\n")
hook("UI/Frame/FrameRoot.Floats.cs",
     "            floatLayer.Insert(Mathf.Min(floats.IndexOf(f), floatLayer.childCount), f.el);\n",
     "            floatLayer.Insert(Mathf.Min(floats.IndexOf(f), floatLayer.childCount), f.el);\n            skin?.Apply(f.el);\n")
hook("UI/Frame/FrameRoot.Rack.cs",
     "            public FrameIcon.Kind kind;\n",
     "            public FrameIcon.Kind kind;\n            public string kindName;\n")
hook("UI/Frame/FrameRoot.Rack.cs",
     "                rack.Add(new Verb { button = b, label = label, kind = kind });\n",
     "                rack.Add(new Verb { button = b, label = label, kind = kind, kindName = kind.ToString() });\n")
hook("UI/Frame/FrameRoot.Rack.cs",
     "                v.label.text = name;\n",
     "                v.label.text = name;\n"
     "                FrameSkin.Verb(v.button, v.kindName, name);               // (fn-cast_off, fn-lie_to ...: the icon's own piece finds the button)\n")
hook("UI/Frame/FrameIcon.cs",
     "        void Paint(MeshGenerationContext ctx)\n        {\n            var r = contentRect;\n",
     "        void Paint(MeshGenerationContext ctx)\n        {\n            if (FrameSkin.Skinned(this))\n"
     "                return;                                                 // (the user's own picture of this sign is laid on: FrameSkin)\n"
     "            var r = contentRect;\n")
hook("UI/Frame/FrameDrawn.cs",
     "            AddToClassList(\"frame-glyph\");\n",
     "            AddToClassList(\"frame-glyph\");\n"
     "            AddToClassList(\"glyph-\" + kind.ToString().ToLowerInvariant());   // (so its piece of the frame finds it: FrameSkin)\n")
hook("UI/Frame/FrameDrawn.cs",
     "            var r = contentRect;\n            float s = Mathf.Min(r.width, r.height);\n            if (s < 2f)\n",
     "            if (FrameSkin.Skinned(this))\n                return;                                                 // (the user's own picture of this mark is laid on)\n"
     "            var r = contentRect;\n            float s = Mathf.Min(r.width, r.height);\n            if (s < 2f)\n")
hook("UI/WordLine.cs",
     "            void Paint(MeshGenerationContext ctx)\n            {\n                var r = contentRect;\n",
     "            void Paint(MeshGenerationContext ctx)\n            {\n"
     "                if (Kapman.Game.Frame.FrameSkin.Skinned(this))\n"
     "                    return;                                             // (the user's own picture of this mark is laid on: FrameSkin)\n"
     "                var r = contentRect;\n")
hook("UI/Frame/Panels/SelfPanel.cs",
     "                body.Add(FrameUi.Ln(name, stat != null ? stat.what : null, level.Length > 0 ? FrameUi.Tag(level) : null, FrameUi.Pips(pips)));\n",
     "                body.Add(FrameSkin.Mark(FrameUi.Ln(name, stat != null ? stat.what : null, level.Length > 0 ? FrameUi.Tag(level) : null, FrameUi.Pips(pips)),\n"
     "                                        \"stat-\" + id, \"frame-stat-icon\"));   // (a place for hud.stat.<id>: no room taken until it is drawn)\n")


def own(rel, text, crlf):
    """A file's text with the line endings the file itself has."""
    return text.replace("\n", "\r\n") if crlf[rel] else text


def do_step(st, texts, crlf):
    """Puts one step's hooks into `texts` (read from the project when first needed). Returns the files it touched,
    how many hooks were not found, how many were in already, and the step's diff."""
    files, bad, already, diff = [], 0, 0, []
    for s, rel, old, new in HOOKS:
        if s == st and rel not in files:
            files.append(rel)
    for rel in files:
        if rel not in texts:
            with io.open(GAME + "/" + rel, "r", encoding="utf-8", newline="") as f:
                raw = f.read()
            crlf[rel] = "\r\n" in raw
            texts[rel] = raw.replace("\r\n", "\n")
        before = work = texts[rel]
        for s, r, old, new in HOOKS:
            if s != st or r != rel:
                continue
            if new in work:
                already += 1                                  # (brought in before: left as it is)
                continue
            n = work.count(old)
            if n != 1:
                print("NOT FOUND (found %d) step %d in %s:\n   %s" % (n, st, rel, old.strip().split("\n")[0][:110]))
                bad += 1
                continue
            work = work.replace(old, new)
        texts[rel] = work
        # (the diff has the file's own line endings, so it applies to the file as it is)
        diff.extend(difflib.unified_diff(own(rel, before, crlf).splitlines(True), own(rel, work, crlf).splitlines(True),
                                         "a/Assets/Kapman/Code/Game/" + rel, "b/Assets/Kapman/Code/Game/" + rel, n=3, lineterm="\n"))
    return files, bad, already, diff


def main():
    texts, crlf, bad = {}, {}, 0
    if APPLY:
        # one step, written into the project's own files (the lead's doing, never this script's by itself)
        if APPLY not in NAMES:
            print("--apply takes a step from 1 to %d" % max(NAMES))
            return 1
        files, bad, already, _ = do_step(APPLY, texts, crlf)
        if bad:
            print("step %d: %d lines not found. NOTHING WAS WRITTEN. Are the steps before it in?" % (APPLY, bad))
            return 1
        for rel in files:
            with io.open(GAME + "/" + rel, "w", encoding="utf-8", newline="") as f:
                f.write(own(rel, texts[rel], crlf))
        print("step %d %s: written into %d files of the project (%d lines were in already)" % (APPLY, NAMES[APPLY], len(files), already))
        return 0
    for st in sorted(NAMES):
        if st > UPTO:
            break
        files, missing, already, diff = do_step(st, texts, crlf)
        bad += missing
        if UPTO == 99:
            with io.open(os.path.normpath(os.path.join(HERE, "..", "hooks_%d_%s.diff" % (st, NAMES[st]))), "w", encoding="utf-8", newline="") as f:
                f.write("".join(diff))
        print("step %d %-14s %2d hooks in %d files%s" % (st, NAMES[st], sum(1 for h in HOOKS if h[0] == st), len(files),
                                                        (", %d in already" % already) if already else ""))
    for rel, work in texts.items():
        dst = OUT + "/" + rel
        os.makedirs(os.path.dirname(dst), exist_ok=True)
        with io.open(dst, "w", encoding="utf-8", newline="") as f:
            f.write(own(rel, work, crlf))
    print("%d files copied with their hooks to %s; %d hooks not found" % (len(texts), OUT, bad))
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
