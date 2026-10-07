# -*- coding: utf-8 -*-
# TO PASTE INTO Bake/art/artframe.py (after its USS table): where the game's FrameSkin lays each piece, and how.
# piece id as in PIECES (no "hud."): (uss, lay). A piece not named here is laid nowhere (uss "").
# artframe.USS (the field "today") stays as it is: that one is for reading, this one is read by the game.
# Made by Design/art_loader/draft/tools/add_uss.py on 7 Oct 2026.
PLACES = {
    "arrow_left":          (".frame-glyph.glyph-left", "icon"),   # hook
    "arrow_pick":          (".frame-glyph.glyph-down", "icon"),   # hook
    "arrow_right":         (".frame-glyph.glyph-right", "icon"),   # hook
    "bar_log":             (".frame-log", "fill"),
    "bar_top":             (".frame-top", "fill"),
    "bar_work":            (".frame-wmbar", "fill"),
    "chart.farther":       (".frame-tool.tool-farther .frame-icon", "icon"),   # later: The chart tools on the right (ruling 8) are not built.
    "chart.find_her":      (".frame-tool.tool-find_her .frame-icon", "icon"),   # later: The chart tools on the right (ruling 8) are not built.
    "chart.measure":       (".frame-verb.fn-measure .frame-icon, .frame-tool.tool-measure .frame-icon", "icon"),   # hook: On the rack's Measure button today; on the chart tool when ruling 8 is built.
    "chart.names":         (".frame-tool.tool-names .frame-icon", "icon"),   # later: The chart tools on the right (ruling 8) are not built.
    "chart.nearer":        (".frame-tool.tool-nearer .frame-icon", "icon"),   # later: The chart tools on the right (ruling 8) are not built.
    "chart.pins":          (".frame-tool.tool-pins .frame-icon", "icon"),   # later: The chart tools on the right (ruling 8) are not built.
    "chart.sails":         (".frame-tool.tool-sails .frame-icon", "icon"),   # later: The chart tools on the right (ruling 8) are not built.
    "chart.winds":         (".frame-tool.tool-winds .frame-icon", "icon"),   # later: The chart tools on the right (ruling 8) are not built.
    "chip":                (".frame-wm", "fill"),
    "chip_on":             (".frame-wm.on", "fill"),
    "fn.cast_off":         (".frame-verb.fn-cast_off .frame-icon", "icon"),   # hook
    "fn.hail":             (".frame-verb.fn-hail .frame-icon", "icon"),   # hook
    "fn.lie_to":           (".frame-verb.fn-lie_to .frame-icon", "icon"),   # hook
    "fn.look_out":         (".frame-verb.fn-look_out .frame-icon", "icon"),   # hook
    "fn.oars":             (".frame-verb.fn-oars .frame-icon", "icon"),   # later: The rack still has Measure where ruling 8 puts Oars; the class comes by itself when the kind Oars is added.
    "fn.offer":            (".frame-verb.fn-offer .frame-icon", "icon"),   # hook
    "fn.rest":             (".frame-verb.fn-rest .frame-icon", "icon"),   # hook
    "fn.sound":            (".frame-verb.fn-sound .frame-icon", "icon"),   # hook
    "fn.wait":             (".frame-verb.fn-wait .frame-icon", "icon"),   # hook
    "fn_button":           (".frame-verb", "fill"),
    "fn_button_down":      (".frame-verb.on, .frame-verb:active", "fill"),
    "fn_button_off":       (".frame-verb.off", "fill"),
    "frame_face":          (".frame-face, .frame-float-face", "over"),   # now: Stretched to the face's box; the box's own edge and round corners stand down.
    "frame_popup":         (".frame-float", "fill"),   # now: The front one of several has a gold edge in the stylesheet and no piece: once this is drawn the gold edge is gone.
    "frame_portrait":      (".frame-who", "fill"),
    "frame_screen":        (".frame-screen", "fill"),   # now: FrameSkin makes this element itself, under the frame's parts and over the chart.
    "frame_ship":          (".frame-ship-view", "over"),   # now: Hollow: laid over the view, the view's own sunk edge stands down.
    "frame_tip":           (".frame-tip", "fill"),   # later: The small box under the cursor for a linked word is not built.
    "frame_window":        (".frame-win, .frame-ship, .frame-rule", "fill"),   # now: Also the box of the ship window and the measure's words: plain slips with no piece of their own.
    "frame_window_focus":  (".frame-win.focus", "fill"),
    "lock":                (".frame-lock", "icon"),   # later: The wiki is not built.
    "main_button":         (".frame-k.go", "fill"),   # now: The one big button a panel is for (Cast off for ...).
    "option":              (".frame-option", "fill"),
    "option_hover":        (".frame-option:hover", "fill"),   # now: The cursor only: a choice picked by its number key has no class in the frame.
    "panel.bonds":         (".frame-win.panel-bonds .frame-win-icon", "icon"),   # hook
    "panel.chest":         (".frame-win.panel-chest .frame-win-icon", "icon"),   # hook
    "panel.chron":         (".frame-win.panel-chronicle .frame-win-icon", "icon"),   # hook
    "panel.crew":          (".frame-win.panel-crew .frame-win-icon", "icon"),   # hook
    "panel.event":         (".frame-win.panel-event .frame-win-icon", "icon"),   # hook
    "panel.faith":         (".frame-win.panel-faith .frame-win-icon", "icon"),   # hook
    "panel.field":         (".frame-win.panel-field .frame-win-icon", "icon"),   # hook
    "panel.hold":          (".frame-win.panel-hold .frame-win-icon", "icon"),   # hook
    "panel.house":         (".frame-win.panel-house .frame-win-icon", "icon"),   # hook
    "panel.people":        (".frame-win.panel-people .frame-win-icon", "icon"),   # hook
    "panel.self":          (".frame-win.panel-self .frame-win-icon", "icon"),   # hook
    "panel.ship":          (".frame-win.panel-ship .frame-win-icon", "icon"),   # hook
    "panel.sky":           (".frame-win.panel-sky .frame-win-icon", "icon"),   # hook
    "panel.tasks":         (".frame-win.panel-tasks .frame-win-icon", "icon"),   # hook
    "panel.tongues":       (".frame-win.panel-tongues .frame-win-icon", "icon"),   # hook
    "panel.trade":         (".frame-win.panel-trade .frame-win-icon", "icon"),   # hook
    "panel.voyage":        (".frame-win.panel-voyage .frame-win-icon", "icon"),   # hook
    "panel.wiki":          (".frame-win.panel-wiki .frame-win-icon", "icon"),   # later: The wiki panel is not built.
    "panel.word":          (".frame-win.panel-word .frame-win-icon", "icon"),   # hook
    "pip_empty":           (".frame-pip.off", "fill"),
    "pip_full":            (".frame-pip", "fill"),   # now: Stretched to the pip's box (10 x 10: twice the picture).
    "pressing":            (".frame-float.pressing .frame-pressing", "icon"),   # later: A pressing card (ruling 10) has no mark yet.
    "row":                 (".frame-ln", "fill"),   # now: A line that is a button lights up under the cursor in the stylesheet; there is no piece for that.
    "row_on":              (".frame-ln.sel", "fill"),
    "rule":                (".frame-sep", "fill"),   # now: Stretched along the line; the line is 1 px high in the stylesheet today.
    "scroll":              (".unity-scroller--vertical .unity-base-slider__dragger", "fill"),
    "scroll_track":        (".unity-scroller--vertical .unity-base-slider__tracker", "fill"),
    "small_button":        (".frame-k, .frame-pick", "fill"),   # now: Every small button, and the list a panel is picked from.
    "small_button_on":     (".frame-k.on, .frame-pick.on", "fill"),
    "stat.lore":           (".frame-ln.stat-lore .frame-stat-icon", "icon"),   # hook: The Yourself window; not yet before a choice in a talk.
    "stat.name":           (".frame-ln.stat-name .frame-stat-icon", "icon"),   # hook: The Yourself window; not yet before a choice in a talk.
    "stat.nerve":          (".frame-ln.stat-nerve .frame-stat-icon", "icon"),   # hook: The Yourself window; not yet before a choice in a talk.
    "stat.tongue":         (".frame-ln.stat-tongue .frame-stat-icon", "icon"),   # hook: The Yourself window; not yet before a choice in a talk.
    "stat.troth":          (".frame-ln.stat-troth .frame-stat-icon", "icon"),   # hook: The Yourself window; not yet before a choice in a talk.
    "swap":                (".frame-swap", "icon"),   # later: The button that changes the ship and the chart about (ruling 7) is not built.
    "tab":                 (".frame-tab", "fill"),
    "tab_on":              (".frame-tab.on", "fill"),
    "tag":                 (".frame-tag", "fill"),
    "tilt.down":           (".word-tilt.down", "icon"),   # hook: The same.
    "tilt.up":             (".word-tilt.up", "icon"),   # hook: In the frame's windows; the chart's own port card is not part of the frame.
    "title":               (".frame-win-head", "fill"),
    "tool_button":         (".frame-tool", "fill"),   # later: The chart tools on the right (ruling 8) are not built.
    "tool_button_on":      (".frame-tool.on", "fill"),   # later
    "wiki.events":         (".frame-wiki-icon.wiki-events", "icon"),   # later: The wiki is not built.
    "wiki.faith_and_days": (".frame-wiki-icon.wiki-faith_and_days", "icon"),   # later: The wiki is not built.
    "wiki.how_to_play":    (".frame-wiki-icon.wiki-how_to_play", "icon"),   # later: The wiki is not built.
    "wiki.people":         (".frame-wiki-icon.wiki-people", "icon"),   # later: The wiki is not built.
    "wiki.peoples":        (".frame-wiki-icon.wiki-peoples", "icon"),   # later: The wiki is not built.
    "wiki.ports":          (".frame-wiki-icon.wiki-ports", "icon"),   # later: The wiki is not built.
    "wiki.ship_and_sea":   (".frame-wiki-icon.wiki-ship_and_sea", "icon"),   # later: The wiki is not built.
    "wiki.trade":          (".frame-wiki-icon.wiki-trade", "icon"),   # later: The wiki is not built.
    "wiki.words":          (".frame-wiki-icon.wiki-words", "icon"),   # later: The wiki is not built.
    "win_add":             (".frame-glyph.glyph-plus", "icon"),   # hook
    "win_close":           (".frame-glyph.glyph-cross", "icon"),   # hook
    "win_open":            (".frame-glyph.glyph-open", "icon"),   # later: Ruling 9 (a panel opens into a full window) is not built; a FrameGlyph of the kind Open gets this class by the hook.
    "win_size":            (".frame-glyph.glyph-size", "icon"),   # later: The same, for the kind Size.
    "word.heard":          (".word-mark.heard", "icon"),   # hook: The same.
    "word.law":            (".word-mark.law", "icon"),   # hook: The same.
    "word.none":           (".word-mark.none", "icon"),   # hook: The same.
    "word.season":         (".word-mark.season", "icon"),   # hook: The same.
    "word.seen":           (".word-mark.seen", "icon"),   # hook: In the frame's windows; the chart's own port card is not part of the frame.
}
