# The loader that puts your sprites into the game

7 October 2026. Drafted outside Assets. Nothing in Assets, Bake or Art was changed. The lead brings it in.

Today a picture you draw reaches the game only as a portrait, and only at the next Play. After this plan every kind
of picture is read through one class, a picture saved while the game runs shows within a second, and the frame takes
your own pieces one by one as you draw them.

## 1. What is in this folder

| file | goes to | what it is |
|---|---|---|
| draft/ArtFiles.cs | Assets/Kapman/Code/Game/Art/ | Which file a slot is shown from. No Unity in it. |
| draft/ArtSlots.cs | Assets/Kapman/Code/Game/Art/ | The loader: texture or sprite for a slot, the cache, changes on disk, pictures on elements. |
| draft/FrameSkin.cs | Assets/Kapman/Code/Game/UI/Frame/ | Lays the frame's own pieces over its elements. |
| draft/ArtBuild.cs | Assets/Kapman/Editor/Game/ | Copies the pictures into a built game. The menu Kapman, Art. |
| draft/ArtSlotsTests.cs | Assets/Kapman/Tests/EditMode/ | 13 tests. |
| draft/FrameSkinTests.cs | Assets/Kapman/Tests/EditMode/ | 6 tests. |
| draft/hooks_1 to hooks_6 .diff | the game's own files | The lines that hook it in, one diff a step (section 4). |
| draft/hud_assign.json | (a draft of Art/hud_assign.json) | The live file with "uss" and "lay" added to all 130 pieces. |
| draft/uss_table.md | to read | The table: which element takes which piece, and what each still needs. |
| draft/artframe_places.py | to paste into Bake/art/artframe.py | The same table as the dict the baker should carry (section 6). |
| draft/tools/add_uss.py | stays here | Makes the three files above from the live Art/hud_assign.json. |
| draft/tools/make_hooks.py | stays here | Makes the six diffs from the game's files as they are now. Says which lines it no longer finds. With `--apply N` it puts step N into the project (section 4). |
| check/compile_check.sh | stays here | Compiles everything without Unity (section 2). |
| check/ArtCheck.cs, artcheck.csproj | stays here | Runs ArtFiles for real without Unity. |

Each draft is headed by the path it should have.

## 2. What compiled, what ran, what did not

Compiled with the editor's own compiler (DotNetSdk, Roslyn) and the references and defines of the project's response
files, all output in the temp folder. Last run 7 Oct 2026, 11:28. Every line below ended with exit 0 and no errors.

- Kapman.Core (57 files) and Kapman.Runtime (35 files) from the sources on disk, so the check matches the project as
  it is now and not as Unity last compiled it.
- Kapman.Game as on disk (135 files) with ArtFiles.cs, ArtSlots.cs and FrameSkin.cs added and nothing else changed.
  No warnings in the drafts.
- The same after each of the six steps of section 4, one after another. FrameArt.cs is left out from step 1.
- ArtBuild.cs against the Editor's references.
- The two test files against the tests' references.

Run it again with `bash Design/art_loader/check/compile_check.sh`.

Ran for real, without Unity: `check/ArtCheck.cs` on ArtFiles. 27 checks pass: the order of falling back both ways,
names that try to leave the art folders, the stamps of a file and of a folder on this disk, the drawing folder.

NOT run: the 19 NUnit tests (they need the editor), and anything on screen. Section 8 lists what that leaves open.

Other builders were changing the frame's files all morning. The hooks were found in the files as they stood at 11:28.
If a file has moved since, run `draft/tools/make_hooks.py` again: it writes the diffs afresh and names any line it no
longer finds.

## 3. The order of falling back

For a slot and its stand-in (a person and their role, a card and its stage, a site and its kind) the game shows the
first of these that is there:

1. your picture of the slot itself (Art/final)
2. your picture of the stand-in (Art/final)
3. the placeholder of the slot itself
4. the placeholder of the stand-in
5. nothing (a card needs no picture)

**This needs your word.** Ruling 13 can be read two ways. Every slot has a placeholder of its own, so if "a person's
own portrait" means "final or placeholder", a stand-in you draw would never show for anyone who is on the list. The
DRAW FIRST page says the opposite: "One picture the game shows for every person of a role ... that has none of its
own. With these drawn, nothing in the first release is bare." I followed that page. The other reading (1, 3, 2, 4) is
one switch: `ArtSlots.Shared.Files.StandInBeforePlaceholder = false`. Both are tested.

The frame's pieces are different: only step 1 counts. A piece that is not in Art/final is not used, and the element
keeps its stylesheet look.

## 4. The order to bring it in

Each step compiles on its own and gives one thing to look at. The steps are made in this order, so bring them in
in this order. Two ways:

- `draft/tools/make_hooks.py --apply N` puts step N's lines into the game's own files. If one line of the step is
  not found it writes nothing and says which. A step that is in already is left alone. I tried this on a copy of
  the game's folder in the temp folder, never on the project: steps 1 to 6 in order gave, byte for byte, the files
  that compiled in section 2; step 6 before the others wrote nothing; step 3 twice changed nothing.
- or `git apply Design/art_loader/draft/hooks_N_....diff`. I could not try git here.

**Step 0. The loader alone.** Copy ArtFiles.cs, ArtSlots.cs and ArtSlotsTests.cs in. Compile, run the 13 tests.
Nothing on screen changes.

**Step 1. Portraits (hooks_1, 12 lines in 4 files).** FrameFace asks ArtSlots. A talk and a card name the speaker's
portrait and the stand-in of their role. `ArtSlots.Shared.Pump()` goes into the frame's Update. Delete FrameArt.cs and
its .meta: nothing uses it any more.
Look: your own portrait top right, as before. Talk to the toll man at Dorestad: his placeholder shows in the window
and top right. A card's speaker shows the stand-in of their role, where today the place is an empty shape.
Then, with the game running, save any picture over `Art/placeholder/portrait/self.frisian.man.png`: it should change
within a second.

**Step 2. A card's picture (hooks_2, 2 lines).** Across the top of a card's window: the card's own picture, else its
stage's stand-in, 192 x 108 shown at twice its size. Every card has a placeholder, so every card grows by 216 pixels.
Look at a long card before keeping this step.

**Step 3. Goods (hooks_3, 3 lines).** A 32 x 32 icon before each good in Trade and Hold.

**Step 4. The harbour view (hooks_4, 1 line).** At the top of the Port window. It is 640 wide and the window is
narrower, so it is shrunk and its pixels are uneven. It belongs in the full window of ruling 9.

**Step 5. The chart (hooks_5, 4 lines).** A port's own mark in place of the dot. Your ship's mark by her class. A
landmark's mark once you have drawn it. The other sails are not in this step: see section 5.

**Step 6. The frame's own pieces (hooks_6, 17 lines in 8 files).** Copy FrameSkin.cs and FrameSkinTests.cs in. Run
the 6 tests. Nothing on screen changes yet: the live Art/hud_assign.json has no "uss", and nothing is drawn.

**Step 7. The table into the baker (section 6).** Run make_art_list.py. Still nothing changes on screen.
To see it work: copy `Art/placeholder/hud/hud.fn_button.png` to `Art/final/hud/` while the game runs. The eight
buttons take it within a second. Delete it: they go back.

**Step 8. The built game.** Copy ArtBuild.cs in. See section 7.

## 5. Where each kind is shown, and the line that hooks it in

| kind | where it shows | the line | state |
|---|---|---|---|
| your portrait | the portrait block, top right | `ArtSlots.Shared.Show(this, slot, StandIn)` in FrameFace.Slot | step 1 |
| a person of a places file | a talk's window, and top right while they speak | `portrait = ArtSlots.Person(p.id), portraitAny = ArtSlots.Role(p.role)` in PersonTalk.Spec | step 1 |
| a card's speaker | the card's window, and top right | the same two fields in FrameRoot.CardSpec | step 1 |
| a person of weight | nowhere yet | `ArtSlots.Weight(id)` where they are shown | no hook |
| a card's picture | across the top of the card's window | `spec.picture = ArtSlots.Card(s.card.id); spec.pictureAny = ArtSlots.CardAny(stage)` | step 2 |
| goods | each line of Trade and Hold | `ArtSlots.Shared.Before(FrameUi.Ln(...), ArtSlots.Good(good.id))` | step 3 |
| harbour view | top of the Port window | `body.Add(ArtSlots.Shared.Banner(ArtSlots.Harbour(here.id), null, 1f))` | step 4 |
| port mark | the chart, MapView.cs, where the dot is made | `ArtSlots.Shared.ShowSized(dot, 2f, ArtSlots.PortMark(def.id))` | step 5 |
| your ship's mark | the chart, MapView.cs | `ArtSlots.Shared.ShowSized(shipMark, 1f, ArtSlots.ShipMark(classId))` | step 5 |
| other sails' marks | the chart, MapView.SimShipMarks | see below | not hooked |
| a site's scene | nowhere yet: the People here window only lists the sites | `ArtSlots.Shared.Banner(ArtSlots.Site(s.id), ArtSlots.SceneAny(s.kind), 1f)` when a site is opened | no hook |
| the ship window's backdrops | FrameShipView in FrameDrawn.cs draws the view by code | `ArtSlots.Shared.ShowFinal(this, ArtSlots.ShipBack(place, hour))`, and Paint draws no sky or shore while it shows | not hooked |

**card_rules_793.json** was not written yet, and neither was its script. I used the field names "portrait" and
"picture". When its loader is there, two lines in FrameRoot.CardSpec change:
`spec.portrait = ArtSlots.FileOf("portrait", rule.portrait) ?? ArtSlots.Person(first.id);` and
`spec.picture = ArtSlots.FileOf("card", rule.picture) ?? ArtSlots.Card(s.card.id);`
FileOf takes a slot id ("person.dorestad.tollman") or the file itself ("portrait/person.dorestad.tollman.png").

**The other sails.** Not in the diffs, for a reason of the game's own rules. A sail that is only sighted must not
show her class, or the mark tells what the lookout does not know. So in SimShipMarks, when a mark takes a ship or her
state of being seen changes: made out, `ArtSlots.Shared.ShowSized(m, 1f, ArtSlots.ShipMark(s.cls.id))`; only sighted,
`"ui/ui.sail.png"`. Do it on the change, not every frame. The offsets there (minus 7, minus 3) must become half the
mark's size, as step 5 does for your own ship.

**The ship window.** Design/ship_view plans a 3D view. The twelve backdrops are for the drawn stand-in that is there
today. If the 3D view wants one behind the boat, it asks the same call from its sky. I did not touch either.

## 6. The frame's pieces: the table, and what the baker must write

`draft/uss_table.md` has all 130 pieces. In short:

- **now, 31 pieces.** The place is in the frame today: the windows, the bars, the buttons, tabs, chips, choices,
  lines, tags, pips, the scroll bar, the screen's own frame.
- **hook, 44 pieces.** The place is there once hooks_6 is in: the arrows and crosses, the function icons, the panel
  icons, the five stat icons, the marks of how a price is known.
- **later, 35 pieces.** The frame has no such element yet (the chart tools, the wiki, the clock's signs, the swap
  button, a pressing card's mark). Where the class was plain to name I named it, so the piece is found the day its
  builder uses that class.
- **code, 20 pieces.** Drawn by code, not an element: the ship window's 16, the measure's two, a landmark, her mark
  on the small chart.

How a piece is laid ("lay"): **fill** is the element's background, cut in nine; **over** is a layer on top (the
frame of a face, the frame of the ship's view); **icon** takes the place of the element's own drawing.

**What must change in the baker.** Bake/art/artframe.py already has a table called USS that make_art_list.py writes
as "today". It is for reading: it holds things like `FrameGlyph.Kind.Left` and `.map-landmark (MapView.uss)`, which
the game cannot use. Leave it. Add beside it:

1. In `Bake/art/artframe.py`, after USS: paste the dict `PLACES` from `draft/artframe_places.py`.
2. In `Bake/art/make_art_list.py`, in `collect_hud`, where `"today": artframe.USS.get(pid, "")` is written, add
   `"uss": artframe.PLACES.get(pid, ("", "fill"))[0], "lay": artframe.PLACES.get(pid, ("", "fill"))[1]`.
3. In the same file, the "about" line of hud_assign.json says the game takes the placeholder when there is no
   final. For the frame that is no longer so. Say: a piece is laid only when it is in Art/final.

Then Art/hud_assign.json equals draft/hud_assign.json. Until then FrameSkin finds no places, says so once in the
console, and does nothing.

## 7. In a built game, and drawing with the game open

**A built game.** The pictures are plain files, so they travel in StreamingAssets, as the data does. ArtBuild.cs
does what ContentBuild.cs does: before every build it copies Art/final, Art/placeholder and hud_assign.json to
Assets/StreamingAssets/Art, and takes the copy away when the build is done. It leaves out the chart's tiles (the
chart has its own folder) and any placeholder whose slot you have drawn. About 17 MB today. ArtSlots reads
StreamingAssets/Art in a built game and does not watch the files there.

**Drawing with the game open.** The game looks at the files once a second. A picture written again is read into the
same texture. A picture added or taken away changes what each slot falls back to. So: draw, save, run pull_art.py,
and it is on screen.
There is also a tick in the menu, off until you want it: Kapman, Art, "Show my pictures straight from the Desktop
folder". With it on, a picture shows as soon as you save it in "Kapman Art", before pull_art.py. It counts only if
it has the slot's size and is no longer the placeholder: the two checks pull_art.py makes. I left it off because
ruling 4 names Art/final as the place the game reads. It is your choice.

## 8. What I could not know without running the game

- Whether the nine cuts come out right. `unitySliceScale` is set to 3 (one pixel of a piece is three of the
  screen's). It compiles. I have not seen it.
- Whether :hover and :active reach the skin. It listens on the frame's root for the pointer going over, out, down
  and up. A button takes hold of the pointer when pressed; I assumed the root still hears it let go.
- Whether an element comes on screen and leaves it in the order I assumed. The cache counts on that to know what is
  shown.
- How it looks. The sizes of the icons, the card's picture, the goods and the port marks are my guesses. Each is
  one number.
- Where the screen's own frame should lie. I put it under the frame's parts and over the chart.
- The cost of walking the frame four times a second. 108 places against a few hundred elements. It should be
  small.
- A built game. The build step compiles. No build was made.

## 9. What I found wrong on the way

1. **The portrait boxes are not a whole multiple of 64 x 80.** Frame.uss has the face top right at 120 x 150 (line
   677) and the face in a floating window at 88 x 110 (line 867). A portrait read without smoothing will show
   uneven pixels there. 128 x 160 and 64 x 80 (or 128 x 160 again) would be even.
2. **Three elements are too small for corners at three times.** A tag, a chip of the workspace bar and the scroll
   bar's thumb, as Frame.uss sizes them, cannot hold two corners at 3x. FrameSkin shows those corners at 2x or 1x
   so nothing overlaps. That mixes pixel sizes until the stylesheet gives them room.
3. **States with a look in the stylesheet and no piece.** The front one of several floating windows (gold edge). A
   button or a line under the cursor (it lights up). Once the plain piece is drawn, these looks are gone. A choice
   picked by its number key has no class at all, so hud.option_hover follows the cursor only.
4. **No piece for two boxes.** The box round the ship's view and its caption, and the measure's words over the
   chart. I gave both hud.frame_window so a full set looks whole.
5. **The Port window has no icon in the art list.** There is hud.panel.people and no hud.panel.port. And
   hud.panel.wiki has no panel yet.
6. **A panel's icon is 16 pixels high and the title strip it sits in is 10** (hud.title is 24 x 10). At one size of
   pixel the icon cannot fit the strip. I show it at twice its size in today's strip.
7. **The face is always the man's.** FrameFace asks for self.<people>.man. Self has no choice of man or woman yet,
   and the list has both.
8. **A slot id that clashes gets four letters added by make_art_list.py** (its slot function). The game cannot guess
   such a name. None of the names I checked is one.
9. **The chart's own port card is not part of the frame.** The marks of how a price is known (hud.word.*) will show
   in the frame's windows and not there.
10. **Art/hud_assign.json's "today" for hud.fn_button_down** names `.frame-verb.on` and not the button while it is
    held. The "uss" I wrote has both.
