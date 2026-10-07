# Text size audit of the frame (7 Oct 2026)

Read, not run: `Assets/Kapman/Game/UI/Frame.uss` (1077 lines), `Assets/Kapman/Code/Game/UI/Frame/*.cs`,
`Assets/Kapman/Game/UI/GamePanel.asset`, `Design/research/next/art.md` C2. Unity was not run. Nothing was changed.

## The short of it

- **46 font-size lines, 10 different sizes**: 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 px. The rule allows four.
- **Body text is 16 px** (Frame.uss:19, :467). The rule asks 22 at 1080p. No text in the frame is 22.
- **27 of the 46 are under 16 px** (the floor). **38 of 46 are under 18** (the least at 1080p). Only 8 pass.
- **Sizes follow the screen, with no floor.** So at 1280 x 720 every size is two thirds: body 10.7 px, the
  smallest 8 px. The rule asks 18 and 16 there.
- **No pixel font.** Signs (verbs, window names, date) are in Gentium. All sizes are in the stylesheet; the
  code sets none.
- Contrast: all normal text passes 4.5. What fails is switched-off text (2.5 to 4.5) and one likely case of
  mute on the lit colour.

## Do sizes follow the screen height?

Yes, by the panel, not by the stylesheet.

- `GamePanel.asset`:21 `m_ScaleMode: 2` (scale with screen size), :27 reference 1920 x 1080, :28 match mode 0
  (width or height), :29 `m_Match: 0.5`.
- So scale = square root of (width / 1920 x height / 1080). On 16:9 that is height / 1080.
- Every px in Frame.uss is a px at 1080p. Nothing in the code reads `Screen.height` or sets `fontSize`
  (grep of the Frame folder: no hit).

| Screen | Scale | Body 16 becomes | Smallest 12 becomes | Rule asks body / least |
|---|---|---|---|---|
| 1280 x 720 | 0.667 | 10.7 px | 8.0 px | 18 / 16 |
| 1280 x 800 (Deck) | 0.703 | 11.2 px | 8.4 px | 18 / 16 (and start at Large) |
| 1920 x 1080 | 1.0 | 16 px | 12 px | 22 / 18 |
| 2560 x 1440 | 1.333 | 21.3 px | 16 px | 29.3 / 24 |
| 3840 x 2160 | 2.0 | 32 px | 24 px | 44 / 36 |

The rule is "the larger of 18 px and 22 x height / 1080". The panel gives only the second half. Under 884 px
of height the 18 px floor must win, and it does not: there is no floor anywhere.

Not checked: that the frame's UIDocument in L_Chart uses GamePanel (it is the only panel settings file in
`Game/UI`), and that the `style` field of FrameRoot (FrameRoot.cs:31, added at :68-69) holds Frame.uss.

## Which stylesheets the frame loads

- One: the `style` field, FrameRoot.cs:31, added to the document root at FrameRoot.cs:68-69. Frame.uss by its
  own header and by FramePanel.cs:193.
- The panel's theme (GamePanel.asset:15) lies under it: Unity's default. Frame.uss:53-70 strips the Button and
  Label defaults.
- MapView.uss, MarketPanel.uss, TextPanel.uss, MainMenu.uss are not loaded by the Frame code. Not audited.
- No size is set in C#: no `fontSize`, no `<size=` rich text, no drawn text in FrameDrawn.cs or
  FrameRoot.Measure.cs.

## Every font size in use (Frame.uss)

"Breaks" is against 1080p: body 22, quiet 18, nothing under 18 (16 at 720p). New = proposed value, see below.

| Line | Selector | Now | What text | Breaks | New |
|---|---|---|---|---|---|
| 19 | .frame-root | 16 | the base every label takes | body under 22 | 22 |
| 62 | Button | 15 | every button with no class of its own | under 16 | 22 |
| 98 | .frame-top-date | 21 bold | the date on the top bar | a sign, no pixel font; odd size | 26 |
| 104 | .frame-top-part | 16 | part of the day or season beside the date | under 18 | 18 |
| 116 | .frame-top-figure .frame-num | 19 bold | purse and counts on the top bar | odd size | 22 |
| 122 | .frame-top-word | 16 | words on the top bar (place, state) | under 18 | 22 |
| 131 | .frame-top-state | 14 | the state word on the top bar | under 16 | 18 |
| 207 | .frame-verb-name | 14 | names of the eight verbs | under 16; a sign | 18 |
| 285 | .frame-win-title | 18 bold | a window's title button | passes; a sign | 22 |
| 297 | .frame-win-name | 18 bold | a window's name | passes; a sign | 22 |
| 355 | .frame-tab | 15 | tabs in a window | under 16 | 18 |
| 388 | .frame-wm | 14 | workspace and layout buttons under the windows | under 16 | 18 |
| 412 | .frame-wm-key | 12 | the key beside them | under 16; smallest in the frame | 18 |
| 427 | .frame-pick | 15 | the list of panels to pick | under 16 | 22 |
| 455 | .frame-h | 18 bold | a heading in a panel | passes | 22 |
| 461 | .frame-p | 14 | a quiet paragraph in a panel | under 16 | 18 |
| 467 | .frame-text | 16 | a sentence in a panel: body | under 22 | 22 |
| 508 | .frame-ln-name | 16 | the name in a row | under 22 | 22 |
| 513 | .frame-ln-small | 13 | the small line under a row's name | under 16 | 18 |
| 519 | .frame-num | 16 bold | numbers in rows | under 22 | 22 |
| 527 | .frame-tag | 13 | a tag in a row | under 16 | 18 |
| 538 | .frame-k | 14 | small buttons in rows | under 16 | 18 |
| 577 | .frame-k.go | 17 bold | the go button | odd size | 22 |
| 661 | .word-text | 13 italic | a word heard (news), with its age | under 16; italic likely over two lines | 18 |
| 701 | .frame-who-name | 20 bold | the name by the portrait | odd size | 26 |
| 707 | .frame-who-line | 14 | lines under the name | under 16 | 18 |
| 713 | .frame-who-say | 15 italic | what the person says | under 16; body; italic for plain speech | 22 |
| 750 | .frame-ship-where | 15 | where the ship is | under 16 | 22 |
| 756 | .frame-ship-wind | 14 | the wind | under 16 | 18 |
| 785 | .frame-log-stamp | 14 bold | the date of a log line | under 16 | 18 |
| 795 | .frame-log-text | 15 | the message log: sentences, body | under 16 | 22 |
| 821 | .frame-rule-text | 15 | the measure over the chart | under 16 | 22 |
| 891 | .frame-float-title | 21 bold | the title of a card or talk | odd size | 30 |
| 897 | .frame-float-who | 15 | who speaks | under 16 | 18 |
| 909 | .frame-float-line | 18 | card and talk text: the main reading | body under 22 | 22 |
| 917 | .frame-float-line.unwritten | 15 italic | text not yet written | under 16 | 18 |
| 921 | .frame-float-small | 14 | small print on a card | under 16 | 18 |
| 927 | .frame-float-result | 17 | the outcome of a choice | body under 22 | 22 |
| 946 | .frame-option | 17 | a choice (the button) | under 22 | 22 |
| 959 | .frame-option-number | 16 bold | the number of a choice | under 18 | 22 |
| 972 | .frame-option-label | 17 | the words of a choice | body under 22 | 22 |
| 977 | .frame-option-rule | 13 | the rule under a choice | under 16 | 18 |
| 983 | .frame-option-odds | 14 bold | the odds | under 16 | 18 |
| 990 | .frame-option-key | 13 | the key of a choice | under 16 | 18 |
| 1012 | .frame-float-hint | 13 | the hint at the foot of a card | under 16 | 18 |
| 1074 | .frame-self-line | 15 | lines in the window about yourself | under 16 | 22 |

Count by size: 12 px x 1, 13 x 6, 14 x 10, 15 x 10, 16 x 7, 17 x 4, 18 x 4, 19 x 1, 20 x 1, 21 x 2. Total 46.

## The proposed four sizes

At 1080p, as panel px in Frame.uss. The first two are the study's (C2). The upper two are my proposal: the
study gives signs to the pixel font, which is not built.

| Name | px at 1080p | For | How many selectors |
|---|---|---|---|
| quiet | 18 | hints, ages, sources, tags, keys, small buttons, tabs, verb names | 21 |
| body | 22 | all that is read: sentences, rows, numbers, choices, log, window names and headings (bold) | 22 |
| name | 26 | the date, the name by the portrait | 2 |
| title | 30 | the title of a card | 1 |

Do it as four variables on `.frame-root` (`--t-quiet: 18px` and so on) and let every selector take one.
Then the size setting of art.md C4 (18, 22, 28, 36) is four classes that reset the variables, not 46 edits.
Check first that this Unity takes `font-size: var(...)`; it should.

The floor, which the stylesheet cannot do alone. Two ways:
1. Code: on a change of screen size, if scale x 22 is under 18, put a class on the root whose variables are
   18 / scale and 16 / scale. At 720p: body 27 panel px, quiet 24.
2. Or follow art.md C2 and stop scaling read text with the panel: set sizes in true screen px from code.
   More work; it is what C5 asks.

At 720p the study's figures are quiet 16, body 18. Name and title there: 21 and 24.

## Will it fit? (sums, not seen)

- Left column: 29% of 1920 = 557 px, about 520 inside the edges. Body goes 16 to 22: a line holds about 57
  letters where it now holds about 78. Every panel grows about 1.4 times in height.
- At the 720p floor (27 panel px) the same column holds about 46 letters: over the least of 36. Fine.
- A row has a name (22), a number (22) and small buttons (18): the name wraps sooner.
  `.frame-ln-left` min-width 42% (Frame.uss:504) holds.
- Verb buttons are 23.6% of the column, about 121 px inside. A 16-letter verb at 18 px is about 135 px: it
  wraps to two lines (labels wrap, Frame.uss:69). It is not cut.

## Text cut off by a fixed height or width

None is certain at the sizes of today; these bite when sizes go up. Fixed px scale with the panel, so they
bite at every screen alike.

| Line | Where | Fixed | What happens |
|---|---|---|---|
| 79 | .frame-top | height 54 | One line only. A 21 px line is about 27 px high: fits. At 26: about 34, fits. At the Largest setting (26 x 36 / 22 = 43 px letters, a line of about 55): cut. If the bar's words wrap on a narrow screen the second line falls outside the bar. Make it min-height |
| 769 | .frame-log | height 114 | About 96 px inside. Now 4 lines of 15 px. At 22 px: 3 lines, and the fourth shows half. It scrolls (FrameRoot.cs:227), so nothing is lost, but the newest line can sit half hidden. Give it a height in lines, or min-height |
| 788 | .frame-log-stamp | width 118 | Bold 14 now. At 18 bold a long date wraps to a second line. Widen to about 150 or let it size itself |
| 961 | .frame-option-number | width 26 | Bold 16 now. At 22 bold a two-digit number with a stop wraps. Widen to 36 |
| 244-252 | .frame-win | min-height 0, basis 0, overflow hidden | Windows share the column height. The body scrolls (.frame-win-scroll :332), but with three or more windows and bigger text the head and tabs take most of a window and the rest is clipped. The tiler should refuse a split by height too, not only by width |
| 221, 808 | .frame-left, .frame-rule | top 68 | Tied to the 54 px top bar. If the bar grows, these must move with it |
| 848, 1059 | .frame-float | max-height 78% / 90% | Text scrolls (.frame-float-scroll :902). Not checked: whether the choices (.frame-float-options :935) are inside the scroll. If not, many choices at 22 px push the card over its max and are cut |
| 679, 869 | .frame-face, .frame-float-face | height 150 / 110 | Pictures. Not checked: whether the words beside them (name 26, lines 18, say 22) grow taller than the picture, and whether the box then grows |
| 304, 1001 | .frame-head-button, .frame-float-shut | 26 / 28 | Icons only, no text. But 26 px is a small thing to hit |

## Contrast under 4.5 to 1

WCAG sum, worked out for 23 pairs and all nine palettes (script in the session scratchpad, not kept).
Grounds: slip 38,26,44 (:43); row 29,20,33 (a slip under black 0.24, :477); verb 52,36,57 (:159); tab off
42,28,46 (:365); workspace bar 24,16,30 (:380); small button 59,42,63 (:548); lit = `--frame-edge`.
Mute is paper at 0.62 (:26), so its colour moves with its ground.

Under 4.5:

| Ratio | Text on ground | Line | Note |
|---|---|---|---|
| 2.54 | .frame-option-rule (mute) in a choice that is off (opacity 0.5), on a slip | 953, 977 | the line that says why you cannot choose it: the one you most need to read |
| 2.78 | .frame-verb.off .frame-verb-name: paper at 0.35 on the verb ground | 196-198 | a verb you cannot use now; also 14 px |
| 3.52 | .frame-k:disabled: opacity 0.45, paper on 59,42,63 over a slip | 564-566 | |
| 4.01 | .frame-pick.used: opacity 0.5 | 447-449 | |
| 4.46 | .frame-option:disabled label: paper at 0.5 on a slip | 953-955 | |
| 3.22 to 4.42 | mute on the lit colour: Danish 3.22, Irish 3.35, Frisian / English / Wendish / Painted 4.09, Norse 4.20, Svea 4.31, Frankish 4.42 | 411-415 with 402-405 | only if .frame-wm-key sits inside a lit .frame-wm. Not checked in code. Every other lit thing sets gold |

Switched-off text is let off by WCAG. In a game of words the reason a thing is off must still be read:
keep the rule line at full mute and grey only the label.

Close to the line (pass):

| Ratio | Text on ground | Line |
|---|---|---|
| 4.56 | gold on the lit colour, Danish 41,99,112 (lit tab, verb, button) | 32, 369-372 |
| 4.79 | gold on the lit colour, Irish 47,93,122 | 36 |
| 5.08 | mute on a hovered ground (gold 0.12 over a slip) | 292, 950 |

Against the study's own harder rule (C2: body 7, quiet 4.5):

| Ratio | Text | Line | Note |
|---|---|---|---|
| 6.11 | mute on a slip | 26 | fine for quiet text. But .frame-log-text (:795) is sentences in mute: body under 7. Only the last line is paper (:799) |
| 6.00 | mute on a tab that is off | 366 | a sign: needs 4.5, passes |
| 6.50 | gold on the lit colour 112,41,99 | 371 | a sign, passes |

The study says mute on a slip is 6.7; on the slip as built (38,26,44) it is 6.11.

All that pass well: paper on a slip 13.89, on a row 15.00, on a verb 12.07, on a small button 11.07; gold on a
slip 11.22, on a row 12.11; the accent on a slip 8.14 (Norse) to 10.11 (Danish); the warn red 226,132,110 on a
slip 6.13; the date 240,239,234 on a slip 14.40.

Not worked out: text over the chart with no slip under it (none found: .frame-rule is a slip,
FrameRoot.Measure.cs:110). The marks .word-mark.none (paper 0.35, :642) and .word-tilt.down (paper 0.5, :654)
are icons and need 3; at 0.35 the mark will be near 2.9 (a guess from the verb sum, not worked out).
The verb ground (alpha 0.97) and the tab ground (0.96) were taken as solid.

## Other breaks of the reading rule

- No pixel font anywhere: only GentiumBookPlus Regular and Bold are named (Frame.uss:18, :61, :97 and on).
  Signs are in Gentium. The rule's "pixel font for signs" is not built.
- Italic by `-unity-font-style: italic` on the Regular face (:662, :715, :916): Unity slants it. The study
  (C5.3) asks for the true italic file, and italic only for words in another tongue. .word-text and
  .frame-who-say are plain speech in italic.
- Centred text (:210 verbs, :354 tabs, :395, :550 buttons): all signs, none a sentence. Within the rule.
- No line height is set anywhere. The study asks 1.45 for sentences and 1.25 for rows.

## Not done

- Unity not run: no size was seen on a screen, no cut-off was seen, only reckoned.
- The other four stylesheets were not read.
- Which panel settings and stylesheet the scene binds: taken from the files, not from L_Chart.
- Whether .frame-wm-key is a child of .frame-wm, and whether choices scroll with the card text.
