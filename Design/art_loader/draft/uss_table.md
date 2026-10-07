# Which element of the frame takes which piece

Made by tools/add_uss.py from Art/hud_assign.json (130 pieces). The game reads the columns uss and lay from hud_assign.json.

- now (31): the place is in the frame today.
- hook (44): the place is there once the lines of hooks.diff are in.
- later (35): the frame has no such element yet; the place names the class its builder should give it (empty: its builder must choose).
- code (20): drawn by code, not an element.

| piece | size | cuts | lay | uss | state | note |
|---|---|---|---|---|---|---|
| hud.frame_screen | 48x48 | 12 12 12 12 | fill | `.frame-screen` | now | FrameSkin makes this element itself, under the frame's parts and over the chart. |
| hud.frame_window | 24x24 | 8 8 8 8 | fill | `.frame-win, .frame-ship, .frame-rule` | now | Also the box of the ship window and the measure's words: plain slips with no piece of their own. |
| hud.frame_window_focus | 24x24 | 8 8 8 8 | fill | `.frame-win.focus` | now |  |
| hud.frame_popup | 32x32 | 10 10 10 10 | fill | `.frame-float` | now | The front one of several has a gold edge in the stylesheet and no piece: once this is drawn the gold edge is gone. |
| hud.frame_ship | 24x24 | 8 8 8 8 | over | `.frame-ship-view` | now | Hollow: laid over the view, the view's own sunk edge stands down. |
| hud.frame_portrait | 24x24 | 8 8 8 8 | fill | `.frame-who` | now |  |
| hud.frame_face | 68x84 | - | over | `.frame-face, .frame-float-face` | now | Stretched to the face's box; the box's own edge and round corners stand down. |
| hud.bar_top | 48x16 | 6 6 6 6 | fill | `.frame-top` | now |  |
| hud.bar_log | 24x24 | 8 8 8 8 | fill | `.frame-log` | now |  |
| hud.bar_work | 24x12 | 5 5 5 5 | fill | `.frame-wmbar` | now |  |
| hud.fn_button | 36x24 | 6 6 6 6 | fill | `.frame-verb` | now |  |
| hud.fn_button_down | 36x24 | 6 6 6 6 | fill | `.frame-verb.on, .frame-verb:active` | now |  |
| hud.fn_button_off | 36x24 | 6 6 6 6 | fill | `.frame-verb.off` | now |  |
| hud.small_button | 16x10 | 4 4 4 4 | fill | `.frame-k, .frame-pick` | now | Every small button, and the list a panel is picked from. |
| hud.small_button_on | 16x10 | 4 4 4 4 | fill | `.frame-k.on, .frame-pick.on` | now |  |
| hud.tab | 24x10 | 5 5 5 5 | fill | `.frame-tab` | now |  |
| hud.tab_on | 24x10 | 5 5 5 5 | fill | `.frame-tab.on` | now |  |
| hud.chip | 16x9 | 4 4 4 4 | fill | `.frame-wm` | now |  |
| hud.chip_on | 16x9 | 4 4 4 4 | fill | `.frame-wm.on` | now |  |
| hud.option | 24x12 | 4 4 4 4 | fill | `.frame-option` | now |  |
| hud.option_hover | 24x12 | 4 4 4 4 | fill | `.frame-option:hover` | now | The cursor only: a choice picked by its number key has no class in the frame. |
| hud.row | 16x12 | 4 4 4 4 | fill | `.frame-ln` | now | A line that is a button lights up under the cursor in the stylesheet; there is no piece for that. |
| hud.row_on | 16x12 | 4 4 4 4 | fill | `.frame-ln.sel` | now |  |
| hud.tag | 16x8 | 4 4 4 4 | fill | `.frame-tag` | now |  |
| hud.title | 24x10 | 4 4 4 4 | fill | `.frame-win-head` | now |  |
| hud.arrow_left | 5x7 | - | icon | `.frame-glyph.glyph-left` | hook |  |
| hud.arrow_right | 5x7 | - | icon | `.frame-glyph.glyph-right` | hook |  |
| hud.arrow_pick | 7x5 | - | icon | `.frame-glyph.glyph-down` | hook |  |
| hud.win_close | 7x7 | - | icon | `.frame-glyph.glyph-cross` | hook |  |
| hud.win_add | 7x7 | - | icon | `.frame-glyph.glyph-plus` | hook |  |
| hud.lock | 7x9 | - | icon | `.frame-lock` | later | The wiki is not built. |
| hud.pip_full | 5x5 | - | fill | `.frame-pip` | now | Stretched to the pip's box (10 x 10: twice the picture). |
| hud.pip_empty | 5x5 | - | fill | `.frame-pip.off` | now |  |
| hud.scroll | 4x8 | 2 2 2 2 | fill | `.unity-scroller--vertical .unity-base-slider__dragger` | now |  |
| hud.measure_point | 5x5 | - | fill |  | code | The measure is drawn by code on the chart (FrameRoot.Measure.cs, the Paint of its layer). |
| hud.measure_dash | 8x2 | - | fill |  | code | The same. |
| hud.lay_rows | 9x7 | - | icon |  | later | The three layouts are words on their buttons today (Rows, Columns, Tabs). |
| hud.lay_cols | 9x7 | - | icon |  | later | The same. |
| hud.lay_tabs | 9x7 | - | icon |  | later | The same. |
| hud.menu | 12x12 | - | icon |  | later | The Menu button is a word today. |
| hud.fn.cast_off | 16x16 | - | icon | `.frame-verb.fn-cast_off .frame-icon` | hook |  |
| hud.fn.lie_to | 16x16 | - | icon | `.frame-verb.fn-lie_to .frame-icon` | hook |  |
| hud.fn.wait | 16x16 | - | icon | `.frame-verb.fn-wait .frame-icon` | hook |  |
| hud.fn.oars | 16x16 | - | icon | `.frame-verb.fn-oars .frame-icon` | later | The rack still has Measure where ruling 8 puts Oars; the class comes by itself when the kind Oars is added. |
| hud.fn.look_out | 16x16 | - | icon | `.frame-verb.fn-look_out .frame-icon` | hook |  |
| hud.fn.sound | 16x16 | - | icon | `.frame-verb.fn-sound .frame-icon` | hook |  |
| hud.fn.hail | 16x16 | - | icon | `.frame-verb.fn-hail .frame-icon` | hook |  |
| hud.fn.rest | 16x16 | - | icon | `.frame-verb.fn-rest .frame-icon` | hook |  |
| hud.fn.offer | 16x16 | - | icon | `.frame-verb.fn-offer .frame-icon` | hook |  |
| hud.panel.voyage | 16x16 | - | icon | `.frame-win.panel-voyage .frame-win-icon` | hook |  |
| hud.panel.trade | 16x16 | - | icon | `.frame-win.panel-trade .frame-win-icon` | hook |  |
| hud.panel.hold | 16x16 | - | icon | `.frame-win.panel-hold .frame-win-icon` | hook |  |
| hud.panel.people | 16x16 | - | icon | `.frame-win.panel-people .frame-win-icon` | hook |  |
| hud.panel.crew | 16x16 | - | icon | `.frame-win.panel-crew .frame-win-icon` | hook |  |
| hud.panel.tasks | 16x16 | - | icon | `.frame-win.panel-tasks .frame-win-icon` | hook |  |
| hud.panel.event | 16x16 | - | icon | `.frame-win.panel-event .frame-win-icon` | hook |  |
| hud.panel.self | 16x16 | - | icon | `.frame-win.panel-self .frame-win-icon` | hook |  |
| hud.panel.bonds | 16x16 | - | icon | `.frame-win.panel-bonds .frame-win-icon` | hook |  |
| hud.panel.word | 16x16 | - | icon | `.frame-win.panel-word .frame-win-icon` | hook |  |
| hud.panel.faith | 16x16 | - | icon | `.frame-win.panel-faith .frame-win-icon` | hook |  |
| hud.panel.chron | 16x16 | - | icon | `.frame-win.panel-chronicle .frame-win-icon` | hook |  |
| hud.panel.chest | 16x16 | - | icon | `.frame-win.panel-chest .frame-win-icon` | hook |  |
| hud.panel.house | 16x16 | - | icon | `.frame-win.panel-house .frame-win-icon` | hook |  |
| hud.panel.field | 16x16 | - | icon | `.frame-win.panel-field .frame-win-icon` | hook |  |
| hud.panel.sky | 16x16 | - | icon | `.frame-win.panel-sky .frame-win-icon` | hook |  |
| hud.panel.tongues | 16x16 | - | icon | `.frame-win.panel-tongues .frame-win-icon` | hook |  |
| hud.panel.ship | 16x16 | - | icon | `.frame-win.panel-ship .frame-win-icon` | hook |  |
| hud.panel.wiki | 16x16 | - | icon | `.frame-win.panel-wiki .frame-win-icon` | later | The wiki panel is not built. |
| hud.stat.tongue | 16x16 | - | icon | `.frame-ln.stat-tongue .frame-stat-icon` | hook | The Yourself window; not yet before a choice in a talk. |
| hud.stat.name | 16x16 | - | icon | `.frame-ln.stat-name .frame-stat-icon` | hook | The Yourself window; not yet before a choice in a talk. |
| hud.stat.nerve | 16x16 | - | icon | `.frame-ln.stat-nerve .frame-stat-icon` | hook | The Yourself window; not yet before a choice in a talk. |
| hud.stat.lore | 16x16 | - | icon | `.frame-ln.stat-lore .frame-stat-icon` | hook | The Yourself window; not yet before a choice in a talk. |
| hud.stat.troth | 16x16 | - | icon | `.frame-ln.stat-troth .frame-stat-icon` | hook | The Yourself window; not yet before a choice in a talk. |
| shipview.back.river.day | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.river.dusk | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.river.night | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.coast.day | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.coast.dusk | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.coast.night | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.sea.day | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.sea.dusk | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.sea.night | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.port.day | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.port.dusk | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.back.port.night | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.over.fog | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.over.rain | 160x96 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.sun | 9x9 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| shipview.moon | 9x9 | - | fill |  | code | The ship window is drawn by code (FrameShipView in FrameDrawn.cs today; Design/ship_view plans the 3D view). |
| hud.swap | 12x12 | - | icon | `.frame-swap` | later | The button that changes the ship and the chart about (ruling 7) is not built. |
| hud.win_open | 7x7 | - | icon | `.frame-glyph.glyph-open` | later | Ruling 9 (a panel opens into a full window) is not built; a FrameGlyph of the kind Open gets this class by the hook. |
| hud.win_size | 7x7 | - | icon | `.frame-glyph.glyph-size` | later | The same, for the kind Size. |
| hud.pressing | 9x9 | - | icon | `.frame-float.pressing .frame-pressing` | later | A pressing card (ruling 10) has no mark yet. |
| hud.mini_her | 5x5 | - | icon |  | code | The small chart (ruling 7) is not built; her mark on it will be drawn by code. |
| hud.frame_tip | 16x16 | 5 5 5 5 | fill | `.frame-tip` | later | The small box under the cursor for a linked word is not built. |
| hud.tool_button | 24x12 | 4 4 4 4 | fill | `.frame-tool` | later | The chart tools on the right (ruling 8) are not built. |
| hud.tool_button_on | 24x12 | 4 4 4 4 | fill | `.frame-tool.on` | later |  |
| hud.chart.measure | 16x16 | - | icon | `.frame-verb.fn-measure .frame-icon, .frame-tool.tool-measure .frame-icon` | hook | On the rack's Measure button today; on the chart tool when ruling 8 is built. |
| hud.chart.nearer | 16x16 | - | icon | `.frame-tool.tool-nearer .frame-icon` | later | The chart tools on the right (ruling 8) are not built. |
| hud.chart.farther | 16x16 | - | icon | `.frame-tool.tool-farther .frame-icon` | later | The chart tools on the right (ruling 8) are not built. |
| hud.chart.find_her | 16x16 | - | icon | `.frame-tool.tool-find_her .frame-icon` | later | The chart tools on the right (ruling 8) are not built. |
| hud.chart.names | 16x16 | - | icon | `.frame-tool.tool-names .frame-icon` | later | The chart tools on the right (ruling 8) are not built. |
| hud.chart.winds | 16x16 | - | icon | `.frame-tool.tool-winds .frame-icon` | later | The chart tools on the right (ruling 8) are not built. |
| hud.chart.sails | 16x16 | - | icon | `.frame-tool.tool-sails .frame-icon` | later | The chart tools on the right (ruling 8) are not built. |
| hud.chart.pins | 16x16 | - | icon | `.frame-tool.tool-pins .frame-icon` | later | The chart tools on the right (ruling 8) are not built. |
| hud.wiki.how_to_play | 16x16 | - | icon | `.frame-wiki-icon.wiki-how_to_play` | later | The wiki is not built. |
| hud.wiki.ports | 16x16 | - | icon | `.frame-wiki-icon.wiki-ports` | later | The wiki is not built. |
| hud.wiki.trade | 16x16 | - | icon | `.frame-wiki-icon.wiki-trade` | later | The wiki is not built. |
| hud.wiki.peoples | 16x16 | - | icon | `.frame-wiki-icon.wiki-peoples` | later | The wiki is not built. |
| hud.wiki.faith_and_days | 16x16 | - | icon | `.frame-wiki-icon.wiki-faith_and_days` | later | The wiki is not built. |
| hud.wiki.ship_and_sea | 16x16 | - | icon | `.frame-wiki-icon.wiki-ship_and_sea` | later | The wiki is not built. |
| hud.wiki.words | 16x16 | - | icon | `.frame-wiki-icon.wiki-words` | later | The wiki is not built. |
| hud.wiki.events | 16x16 | - | icon | `.frame-wiki-icon.wiki-events` | later | The wiki is not built. |
| hud.wiki.people | 16x16 | - | icon | `.frame-wiki-icon.wiki-people` | later | The wiki is not built. |
| hud.field | 16x10 | 4 4 4 4 | fill |  | later | A place to type (the wiki's search, a note in the chest) is not in the frame yet. |
| hud.main_button | 24x14 | 5 5 5 5 | fill | `.frame-k.go` | now | The one big button a panel is for (Cast off for ...). |
| hud.scroll_track | 4x8 | 2 2 2 2 | fill | `.unity-scroller--vertical .unity-base-slider__tracker` | now |  |
| hud.rule | 8x1 | - | fill | `.frame-sep` | now | Stretched along the line; the line is 1 px high in the stylesheet today. |
| hud.landmark | 5x5 | - | icon |  | code | On the chart, which is not part of the frame: MapView.cs lays it itself (one line, in hooks.diff). |
| hud.tilt.up | 5x5 | - | icon | `.word-tilt.up` | hook | In the frame's windows; the chart's own port card is not part of the frame. |
| hud.tilt.down | 5x5 | - | icon | `.word-tilt.down` | hook | The same. |
| hud.clock.pause | 9x7 | - | icon |  | later | The clock's four buttons are words today (Pause, 1x, 2x, 4x). |
| hud.clock.x1 | 9x7 | - | icon |  | later | The same. |
| hud.clock.x2 | 9x7 | - | icon |  | later | The same. |
| hud.clock.x4 | 9x7 | - | icon |  | later | The same. |
| hud.word.seen | 7x7 | - | icon | `.word-mark.seen` | hook | In the frame's windows; the chart's own port card is not part of the frame. |
| hud.word.heard | 7x7 | - | icon | `.word-mark.heard` | hook | The same. |
| hud.word.season | 7x7 | - | icon | `.word-mark.season` | hook | The same. |
| hud.word.law | 7x7 | - | icon | `.word-mark.law` | hook | The same. |
| hud.word.none | 7x7 | - | icon | `.word-mark.none` | hook | The same. |
