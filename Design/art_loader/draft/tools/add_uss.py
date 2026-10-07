# -*- coding: utf-8 -*-
r"""
WHICH ELEMENT OF THE FRAME TAKES WHICH PIECE (7 Oct 2026).

The table below gives every piece of Art/hud_assign.json the place it is laid in the Command frame, written as the
frame's stylesheet writes its elements (Assets/Kapman/Game/UI/Frame.uss), and how it is laid:
    fill   the element's background, cut in nine by the piece's "slice"
    over   a layer on top of the element (a hollow frame)
    icon   the picture in place of the element's own drawing
The game's FrameSkin reads the two fields "uss" and "lay" from Art/hud_assign.json.

This script only makes the DRAFT copies beside itself; it changes nothing in Art or Bake:
    ../hud_assign.json   Art/hud_assign.json with "uss" and "lay" added to every piece
    ../uss_table.md      the same table to read, with what each place still needs
    ../artframe_places.py  the table as the dict Bake/art/artframe.py should carry (see Design/art_loader/PLAN.md)

  "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" G:\kapman_unity\Kapman2\Design\art_loader\draft\tools\add_uss.py

state:  now    the place is in the frame today and needs no change of code
        hook   the place is there once the lines of ../hooks.diff are in the frame's code
        later  the frame has no such element yet; the place names the class its builder should give it
        code   not an element at all: drawn by code, which must ask ArtSlots itself
"""
import io
import json
import os

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.normpath(os.path.join(HERE, "..", "..", "..", ".."))
SRC = os.path.join(ROOT, "Art", "hud_assign.json")
OUT = os.path.normpath(os.path.join(HERE, ".."))

# the panels of the game by the art list's name for their icon (the game's id differs for one)
PANEL_ID = {"chron": "chronicle"}
PANELS_LATER = {"wiki"}                       # no such panel in the game yet

# piece id (without "hud."): (uss, lay, state, note)
USS = {
    # ---- frames
    "frame_screen": (".frame-screen", "fill", "now", "FrameSkin makes this element itself, under the frame's parts and over the chart."),
    "frame_window": (".frame-win, .frame-ship, .frame-rule", "fill", "now",
                     "Also the box of the ship window and the measure's words: plain slips with no piece of their own."),
    "frame_window_focus": (".frame-win.focus", "fill", "now", ""),
    "frame_popup": (".frame-float", "fill", "now", "The front one of several has a gold edge in the stylesheet and no piece: once this is drawn the gold edge is gone."),
    "frame_ship": (".frame-ship-view", "over", "now", "Hollow: laid over the view, the view's own sunk edge stands down."),
    "frame_portrait": (".frame-who", "fill", "now", ""),
    "frame_face": (".frame-face, .frame-float-face", "over", "now", "Stretched to the face's box; the box's own edge and round corners stand down."),
    "bar_top": (".frame-top", "fill", "now", ""),
    "bar_log": (".frame-log", "fill", "now", ""),
    "bar_work": (".frame-wmbar", "fill", "now", ""),
    "frame_tip": (".frame-tip", "fill", "later", "The small box under the cursor for a linked word is not built."),
    # ---- buttons
    "fn_button": (".frame-verb", "fill", "now", ""),
    "fn_button_down": (".frame-verb.on, .frame-verb:active", "fill", "now", ""),
    "fn_button_off": (".frame-verb.off", "fill", "now", ""),
    "small_button": (".frame-k, .frame-pick", "fill", "now", "Every small button, and the list a panel is picked from."),
    "small_button_on": (".frame-k.on, .frame-pick.on", "fill", "now", ""),
    "main_button": (".frame-k.go", "fill", "now", "The one big button a panel is for (Cast off for ...)."),
    "field": ("", "fill", "later", "A place to type (the wiki's search, a note in the chest) is not in the frame yet."),
    "tab": (".frame-tab", "fill", "now", ""),
    "tab_on": (".frame-tab.on", "fill", "now", ""),
    "chip": (".frame-wm", "fill", "now", ""),
    "chip_on": (".frame-wm.on", "fill", "now", ""),
    "option": (".frame-option", "fill", "now", ""),
    "option_hover": (".frame-option:hover", "fill", "now", "The cursor only: a choice picked by its number key has no class in the frame."),
    "row": (".frame-ln", "fill", "now", "A line that is a button lights up under the cursor in the stylesheet; there is no piece for that."),
    "row_on": (".frame-ln.sel", "fill", "now", ""),
    "tag": (".frame-tag", "fill", "now", ""),
    "title": (".frame-win-head", "fill", "now", ""),
    "tool_button": (".frame-tool", "fill", "later", "The chart tools on the right (ruling 8) are not built."),
    "tool_button_on": (".frame-tool.on", "fill", "later", ""),
    # ---- marks
    "arrow_left": (".frame-glyph.glyph-left", "icon", "hook", ""),
    "arrow_right": (".frame-glyph.glyph-right", "icon", "hook", ""),
    "arrow_pick": (".frame-glyph.glyph-down", "icon", "hook", ""),
    "win_close": (".frame-glyph.glyph-cross", "icon", "hook", ""),
    "win_add": (".frame-glyph.glyph-plus", "icon", "hook", ""),
    "lock": (".frame-lock", "icon", "later", "The wiki is not built."),
    "pip_full": (".frame-pip", "fill", "now", "Stretched to the pip's box (10 x 10: twice the picture)."),
    "pip_empty": (".frame-pip.off", "fill", "now", ""),
    "scroll": (".unity-scroller--vertical .unity-base-slider__dragger", "fill", "now", ""),
    "scroll_track": (".unity-scroller--vertical .unity-base-slider__tracker", "fill", "now", ""),
    "rule": (".frame-sep", "fill", "now", "Stretched along the line; the line is 1 px high in the stylesheet today."),
    "landmark": ("", "icon", "code", "On the chart, which is not part of the frame: MapView.cs lays it itself (one line, in hooks.diff)."),
    "tilt.up": (".word-tilt.up", "icon", "hook", "In the frame's windows; the chart's own port card is not part of the frame."),
    "tilt.down": (".word-tilt.down", "icon", "hook", "The same."),
    "word.seen": (".word-mark.seen", "icon", "hook", "In the frame's windows; the chart's own port card is not part of the frame."),
    "word.heard": (".word-mark.heard", "icon", "hook", "The same."),
    "word.season": (".word-mark.season", "icon", "hook", "The same."),
    "word.law": (".word-mark.law", "icon", "hook", "The same."),
    "word.none": (".word-mark.none", "icon", "hook", "The same."),
    "clock.pause": ("", "icon", "later", "The clock's four buttons are words today (Pause, 1x, 2x, 4x)."),
    "clock.x1": ("", "icon", "later", "The same."),
    "clock.x2": ("", "icon", "later", "The same."),
    "clock.x4": ("", "icon", "later", "The same."),
    "measure_point": ("", "fill", "code", "The measure is drawn by code on the chart (FrameRoot.Measure.cs, the Paint of its layer)."),
    "measure_dash": ("", "fill", "code", "The same."),
    "lay_rows": ("", "icon", "later", "The three layouts are words on their buttons today (Rows, Columns, Tabs)."),
    "lay_cols": ("", "icon", "later", "The same."),
    "lay_tabs": ("", "icon", "later", "The same."),
    "menu": ("", "icon", "later", "The Menu button is a word today."),
    "swap": (".frame-swap", "icon", "later", "The button that changes the ship and the chart about (ruling 7) is not built."),
    "win_open": (".frame-glyph.glyph-open", "icon", "later", "Ruling 9 (a panel opens into a full window) is not built; a FrameGlyph of the kind Open gets this class by the hook."),
    "win_size": (".frame-glyph.glyph-size", "icon", "later", "The same, for the kind Size."),
    "pressing": (".frame-float.pressing .frame-pressing", "icon", "later", "A pressing card (ruling 10) has no mark yet."),
    "mini_her": ("", "icon", "code", "The small chart (ruling 7) is not built; her mark on it will be drawn by code."),
}
for _id in ("cast_off", "lie_to", "wait", "look_out", "sound", "hail", "rest", "offer"):
    USS["fn." + _id] = (".frame-verb.fn-%s .frame-icon" % _id, "icon", "hook", "")
USS["fn.oars"] = (".frame-verb.fn-oars .frame-icon", "icon", "later", "The rack still has Measure where ruling 8 puts Oars; the class comes by itself when the kind Oars is added.")
for _id in ("voyage", "trade", "hold", "people", "crew", "tasks", "event", "self", "bonds", "word", "faith", "chron", "chest", "house", "field", "sky", "tongues", "ship", "wiki"):
    USS["panel." + _id] = (".frame-win.panel-%s .frame-win-icon" % PANEL_ID.get(_id, _id), "icon", "later" if _id in PANELS_LATER else "hook",
                           "The wiki panel is not built." if _id in PANELS_LATER else "")
for _id in ("tongue", "name", "nerve", "lore", "troth"):
    USS["stat." + _id] = (".frame-ln.stat-%s .frame-stat-icon" % _id, "icon", "hook", "The Yourself window; not yet before a choice in a talk.")
USS["chart.measure"] = (".frame-verb.fn-measure .frame-icon, .frame-tool.tool-measure .frame-icon", "icon", "hook",
                        "On the rack's Measure button today; on the chart tool when ruling 8 is built.")
for _id in ("nearer", "farther", "find_her", "names", "winds", "sails", "pins"):
    USS["chart." + _id] = (".frame-tool.tool-%s .frame-icon" % _id, "icon", "later", "The chart tools on the right (ruling 8) are not built.")
for _id in ("how_to_play", "ports", "trade", "peoples", "faith_and_days", "ship_and_sea", "words", "events", "people"):
    USS["wiki." + _id] = (".frame-wiki-icon.wiki-%s" % _id, "icon", "later", "The wiki is not built.")

SHIPVIEW_NOTE = "The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view)."


def main():
    with io.open(SRC, "r", encoding="utf-8") as f:
        data = json.load(f)
    pieces = data["pieces"]
    rows, unknown = [], []
    count = {"now": 0, "hook": 0, "later": 0, "code": 0}
    for pid, p in pieces.items():
        if pid.startswith("shipview."):
            uss, lay, state, note = "", "fill", "code", SHIPVIEW_NOTE
        else:
            key = pid[4:] if pid.startswith("hud.") else pid
            if key not in USS:
                unknown.append(pid)
                uss, lay, state, note = "", "fill", "later", "NOT IN THE TABLE: a piece added to artframe.py after 7 Oct 2026."
            else:
                uss, lay, state, note = USS[key]
        p["uss"] = uss
        p["lay"] = lay
        count[state] += 1
        rows.append((pid, p["file"], "%dx%d" % (p["w"], p["h"]), "-" if not p.get("slice") else " ".join(str(x) for x in p["slice"]), uss, lay, state, note))
    stale = [k for k in USS if "hud." + k not in pieces]
    data["about"] = (data.get("about", "") + " uss = the elements of the frame that take the piece, as Frame.uss writes them (classes, :hover and :active; "
                     "several with commas; empty: no element takes it). lay = fill (the background, cut in nine), over (a layer on top), icon (in place "
                     "of the element's own drawing). A piece is laid only when it is in Art/final.")
    with io.open(os.path.join(OUT, "hud_assign.json"), "w", encoding="utf-8", newline="\n") as f:
        f.write(json.dumps(data, indent=1, ensure_ascii=False))
    with io.open(os.path.join(OUT, "uss_table.md"), "w", encoding="utf-8", newline="\n") as f:
        f.write("# Which element of the frame takes which piece\n\n")
        f.write("Made by tools/add_uss.py from Art/hud_assign.json (%d pieces). The game reads the columns uss and lay from hud_assign.json.\n\n" % len(pieces))
        f.write("- now (%d): the place is in the frame today.\n- hook (%d): the place is there once the lines of hooks.diff are in.\n"
                "- later (%d): the frame has no such element yet; the place names the class its builder should give it (empty: its builder must choose).\n"
                "- code (%d): drawn by code, not an element.\n\n" % (count["now"], count["hook"], count["later"], count["code"]))
        f.write("| piece | size | cuts | lay | uss | state | note |\n|---|---|---|---|---|---|---|\n")
        for pid, fn, size, cut, uss, lay, state, note in rows:
            f.write("| %s | %s | %s | %s | %s | %s | %s |\n" % (pid, size, cut, lay, ("`%s`" % uss) if uss else "", state, note))
    with io.open(os.path.join(OUT, "artframe_places.py"), "w", encoding="utf-8", newline="\n") as f:
        f.write("# -*- coding: utf-8 -*-\n# TO PASTE INTO Bake/art/artframe.py (after its USS table): where the game's FrameSkin lays each piece, and how.\n"
                "# piece id as in PIECES (no \"hud.\"): (uss, lay). A piece not named here is laid nowhere (uss \"\").\n"
                "# artframe.USS (the field \"today\") stays as it is: that one is for reading, this one is read by the game.\n"
                "# Made by Design/art_loader/draft/tools/add_uss.py on 7 Oct 2026.\nPLACES = {\n")
        for key in sorted(USS):
            uss, lay, state, note = USS[key]
            if uss:
                f.write("    %-22s (%s, %s),%s\n" % (json.dumps(key) + ":", json.dumps(uss), json.dumps(lay), ("   # " + state + (": " + note if note else "")) if state != "now" or note else ""))
        f.write("}\n")
    print("pieces %d: now %d, hook %d, later %d, code %d" % (len(pieces), count["now"], count["hook"], count["later"], count["code"]))
    print("with a place (uss not empty): %d" % sum(1 for r in rows if r[4]))
    if unknown:
        print("NOT IN THE TABLE: " + ", ".join(unknown))
    if stale:
        print("IN THE TABLE BUT NOT A PIECE: " + ", ".join(stale))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
