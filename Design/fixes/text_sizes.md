# Text sizes of the frame: the fix (7 Oct 2026)

Made from `Design/text_size_audit.md` and `Assets/Kapman/Game/UI/Frame.uss` (1077 lines). Unity was not run.
Nothing under Assets was changed. The proposed stylesheet is `Design/fixes/Frame.proposed.uss` (1081 lines:
the same file, 4 lines added, 50 lines changed, nothing taken out; made by a line-by-line swap and checked by
diff: 46 font sizes, all now a variable, no px font size left).

To put it in: copy `Design/fixes/Frame.proposed.uss` over `Assets/Kapman/Game/UI/Frame.uss`, then add the floor
(below). Line numbers below are those of today's Frame.uss; in the proposed file every line after 26 sits 4 lower.

## The four sizes (added after line 26, in `.frame-root`)

```css
    --t-quiet: 18px;
    --t-body: 22px;
    --t-name: 26px;
    --t-title: 30px;
```

## Each font size, one line each (line: selector, old -> new)

- 19: .frame-root, 16 -> var(--t-body) 22
- 62: Button, 15 -> body 22
- 98: .frame-top-date, 21 -> name 26
- 104: .frame-top-part, 16 -> quiet 18
- 116: .frame-top-figure .frame-num, 19 -> body 22
- 122: .frame-top-word, 16 -> body 22
- 131: .frame-top-state, 14 -> quiet 18
- 207: .frame-verb-name, 14 -> quiet 18
- 285: .frame-win-title, 18 -> body 22
- 297: .frame-win-name, 18 -> body 22
- 355: .frame-tab, 15 -> quiet 18
- 388: .frame-wm, 14 -> quiet 18
- 412: .frame-wm-key, 12 -> quiet 18
- 427: .frame-pick, 15 -> body 22
- 455: .frame-h, 18 -> body 22
- 461: .frame-p, 14 -> quiet 18
- 467: .frame-text, 16 -> body 22
- 508: .frame-ln-name, 16 -> body 22
- 513: .frame-ln-small, 13 -> quiet 18
- 519: .frame-num, 16 -> body 22
- 527: .frame-tag, 13 -> quiet 18
- 538: .frame-k, 14 -> quiet 18
- 577: .frame-k.go, 17 -> body 22
- 661: .word-text, 13 -> quiet 18
- 701: .frame-who-name, 20 -> name 26
- 707: .frame-who-line, 14 -> quiet 18
- 713: .frame-who-say, 15 -> body 22
- 750: .frame-ship-where, 15 -> body 22
- 756: .frame-ship-wind, 14 -> quiet 18
- 785: .frame-log-stamp, 14 -> quiet 18
- 795: .frame-log-text, 15 -> body 22
- 821: .frame-rule-text, 15 -> body 22
- 891: .frame-float-title, 21 -> title 30
- 897: .frame-float-who, 15 -> quiet 18
- 909: .frame-float-line, 18 -> body 22
- 917: .frame-float-line.unwritten, 15 -> quiet 18
- 921: .frame-float-small, 14 -> quiet 18
- 927: .frame-float-result, 17 -> body 22
- 946: .frame-option, 17 -> body 22
- 959: .frame-option-number, 16 -> body 22
- 972: .frame-option-label, 17 -> body 22
- 977: .frame-option-rule, 13 -> quiet 18
- 983: .frame-option-odds, 14 -> quiet 18
- 990: .frame-option-key, 13 -> quiet 18
- 1012: .frame-float-hint, 13 -> quiet 18
- 1074: .frame-self-line, 15 -> body 22

Count: body 22 selectors, quiet 21, name 2, title 1. Total 46. These are the audit's "New" column, unchanged.

## Fixed heights and widths loosened (4 lines)

- 79: .frame-top `height: 54px` -> `min-height: 54px` (the bar grows if its words wrap or the size goes up).
- 769: .frame-log `height: 114px` -> `height: 146px` (four whole lines of 22 px: 4 x 31 + 10 padding + 8 edge,
  with a little over; it stays a fixed height because it is pinned to the bottom and scrolls inside).
- 787: .frame-log-stamp `width: 118px` -> `min-width: 150px` (a long date at 18 bold no longer wraps).
- 961: .frame-option-number `width: 26px` -> `min-width: 36px` (two digits and a stop at 22 bold).

Left as they are, on purpose:

- 222, 671, 808: `top: 68px` of .frame-left, .frame-who, .frame-rule. They are tied to the 54 px bar. With
  the four sizes the bar stays 54 (a 26 px line is about 34 high), so 68 holds. If the bar ever grows past 54
  (the Largest setting, or the floor on a very small screen) these three must move with it: that needs code
  (set `style.top` from the bar's `resolvedStyle.height` + 14), not a stylesheet.
- 148 .frame-verb `width: 23.6%`, 425 .frame-pick `width: 46%`: labels wrap (line 69), they are not cut.
- 134 .frame-top-state `min-width: 108px`: a least width, cuts nothing.
- 244-252 .frame-win (`overflow: hidden`, basis 0): the body scrolls; with three or more windows the head and
  tabs eat more of each window. Not a stylesheet matter: the tiler should refuse a split by height.
- 679, 869 faces (150 / 110 high), 731 ship view (262), 201-202, 304, 317, 1001 icons: pictures, no text.
- 848, 1059 .frame-float `max-height`: the text scrolls; whether the choices scroll too is still unchecked.

## Not sure of (to see in Unity, once)

- That this Unity takes `font-size: var(--t-body)`. USS variables are allowed in any property, so it should.
  If it does not, the fallback is to swap each `var(...)` for its number (the list above gives all 46).
- The log is 32 px taller, so it covers 32 px more of the chart's foot. If `FrameRoot.Measure.cs` or the chart
  reckons with a log 114 high, that number must become 146 too. A grep of FrameRoot.Measure.cs for 114 and
  logBox found nothing; the other Frame files were not searched.
- Every panel is about 1.4 times taller (the audit's sum). No screen was seen.

## The floor: no text under 18 px on a small screen

### What the panel does (read from the asset)

`Assets/Kapman/Game/UI/GamePanel.asset` (the only panel settings in Game/UI; guid e543386db7bf5294a9161504b2d4b63c):

- line 21 `m_ScaleMode: 2` (scale with screen size)
- line 24 `m_Scale: 1`
- line 27 `m_ReferenceResolution: {x: 1920, y: 1080}`
- line 28 `m_ScreenMatchMode: 0` (match width or height)
- line 29 `m_Match: 0.5`

So scale = sqrt((width / 1920) x (height / 1080)); on 16:9 it is height / 1080. A size of 18 in the stylesheet
is 18 x scale true px: 12 at 720p. A stylesheet cannot read the screen and C# cannot set a USS variable on an
element, so the floor is: classes that reset the four variables, and a few lines of code that pick the class.

Rule used: each size is the larger of itself and 18 true px, so panel px = max(size, 18 / scale). This is the
audit's rule ("the larger of 18 px and 22 x height / 1080") applied to all four. NOTE: the audit's study lets
quiet text go to 16 at 720p; the task here says nothing under 18, so quiet and body meet at 18 true px on small
screens (bold and colour still tell them apart). If 16 is wanted for quiet, lower the quiet values below by 2/18.

### Stylesheet: add at the end of Frame.uss (NOT in Frame.proposed.uss, which holds only the size change)

The class name is the least panel px, rounded up to the next step. Steps of 2 keep the true size between 18
and about 19.5 px.

```css
/* ---------------------------------------------------------------- the floor: no text under 18 true px.
   FrameRoot.TextFloor() puts one of these on the root when the panel's scale is under 1 */
.frame-root.t-floor-20 { --t-quiet: 20px; }
.frame-root.t-floor-22 { --t-quiet: 22px; }
.frame-root.t-floor-24 { --t-quiet: 24px; --t-body: 24px; }
.frame-root.t-floor-26 { --t-quiet: 26px; --t-body: 26px; }
.frame-root.t-floor-28 { --t-quiet: 28px; --t-body: 28px; --t-name: 28px; }
.frame-root.t-floor-30 { --t-quiet: 30px; --t-body: 30px; --t-name: 30px; }
.frame-root.t-floor-32 { --t-quiet: 32px; --t-body: 32px; --t-name: 32px; --t-title: 32px; }
.frame-root.t-floor-34 { --t-quiet: 34px; --t-body: 34px; --t-name: 34px; --t-title: 34px; }
.frame-root.t-floor-36 { --t-quiet: 36px; --t-body: 36px; --t-name: 36px; --t-title: 36px; }
```

These must come after `.frame-root { ... }` (same weight of selector plus a class, so they win anyway).

| Screen | Scale | 18 / scale | Class | Quiet / body in true px |
|---|---|---|---|---|
| 1920 x 1080 and over | 1.0 or more | 18 or less | none | 18 / 22 and up |
| 1600 x 900 | 0.833 | 21.6 | t-floor-22 | 18.3 / 18.3 |
| 1366 x 768 | 0.711 | 25.3 | t-floor-26 | 18.5 / 18.5 |
| 1280 x 800 (Deck) | 0.703 | 25.6 | t-floor-26 | 18.3 / 18.3 |
| 1280 x 720 | 0.667 | 27.0 | t-floor-28 | 18.7 / 18.7 |
| 960 x 540 | 0.5 | 36.0 | t-floor-36 | 18 / 18 |

Under scale 0.5 the class stays t-floor-36 and text does fall under 18: such a screen cannot hold the frame.

### Code: a new partial file, `Assets/Kapman/Code/Game/UI/Frame/FrameRoot.TextFloor.cs` (draft, not compiled)

FrameRoot is `public partial class FrameRoot : MonoBehaviour` (FrameRoot.cs:29); `root` is its field
(FrameRoot.cs:46), made in `Build()` (FrameRoot.cs:152-154) and given the class `frame-root`. Take the
namespace is `Kapman.Game.Frame` (FrameRoot.cs:7).

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame   // as FrameRoot.cs:7
{
    public partial class FrameRoot
    {
        /// <summary>The least true size of any text, in screen px.</summary>
        public const float LeastTextPx = 18f;

        static readonly int[] FloorSteps = { 20, 22, 24, 26, 28, 30, 32, 34, 36 };
        string floorClass;
        int floorScreenW, floorScreenH;

        /// <summary>The floor class for a panel scale (true px per panel px), or null when none is needed.
        /// Pure, so a test can hold it.</summary>
        public static string FloorClassFor(float scale)
        {
            if (scale <= 0f || scale >= 1f) return null;
            float need = LeastTextPx / scale;              // panel px that make 18 true px
            if (need <= 18.01f) return null;
            foreach (int step in FloorSteps)
                if (need <= step + 0.01f) return "t-floor-" + step;
            return "t-floor-" + FloorSteps[FloorSteps.Length - 1];
        }

        /// <summary>The panel's scale as it is now: screen px over panel px. Read from the laid-out panel,
        /// so it holds whatever GamePanel's scale mode, reference size or match are set to.</summary>
        float PanelScale()
        {
            var tree = root?.panel?.visualTree;
            if (tree == null) return 1f;
            float panelH = tree.layout.height;
            if (float.IsNaN(panelH) || panelH < 1f) return 1f;
            return Screen.height / panelH;
        }

        /// <summary>Put the right floor class on the root. Cheap: does nothing unless the screen changed.</summary>
        void TextFloor(bool force = false)
        {
            if (root == null) return;
            if (!force && Screen.width == floorScreenW && Screen.height == floorScreenH) return;
            float scale = PanelScale();
            if (root.panel == null || float.IsNaN(root.panel.visualTree.layout.height)) return; // not laid out yet: try next frame
            floorScreenW = Screen.width;
            floorScreenH = Screen.height;
            string want = FloorClassFor(scale);
            if (want == floorClass) return;
            if (floorClass != null) root.RemoveFromClassList(floorClass);
            if (want != null) root.AddToClassList(want);
            floorClass = want;
        }
    }
}
```

Two lines to add in FrameRoot.cs:

1. In `Build()`, right after `root.AddToClassList("frame-root");` (FrameRoot.cs:154):

```csharp
            floorClass = null; floorScreenW = floorScreenH = 0;   // a new root has no floor class yet
            root.RegisterCallback<GeometryChangedEvent>(_ => TextFloor(true));
```

2. In `Update()` (FrameRoot.cs:297), as its first line:

```csharp
            TextFloor();   // two int compares a frame; catches a window resize or a change of resolution
```

Why both: the geometry event fires when the panel is first laid out and when its panel size changes; the
Update check catches a resolution change that leaves the panel size the same (16:9 to 16:9, where the panel
stays 1920 x 1080 and only the scale moves).

A test to add beside the frame tests (EditMode, no scene needed):

```csharp
        [Test]
        public void TextFloor_HoldsEighteenPx()
        {
            Assert.IsNull(FrameRoot.FloorClassFor(1f));
            Assert.IsNull(FrameRoot.FloorClassFor(2f));
            Assert.AreEqual("t-floor-22", FrameRoot.FloorClassFor(900f / 1080f));
            Assert.AreEqual("t-floor-28", FrameRoot.FloorClassFor(720f / 1080f));
            Assert.AreEqual("t-floor-36", FrameRoot.FloorClassFor(0.5f));
            Assert.AreEqual("t-floor-36", FrameRoot.FloorClassFor(0.3f));
            // every step keeps 18 true px for every scale from 0.5 up
            for (float s = 0.5f; s < 1f; s += 0.01f)
            {
                string c = FrameRoot.FloorClassFor(s);
                float px = c == null ? 18f : int.Parse(c.Substring("t-floor-".Length));
                Assert.GreaterOrEqual(px * s, 17.99f, "scale " + s);
            }
        }
```

### What the floor costs (sums, not seen)

- At 720p text is 28 panel px in a column about 520 wide: about 44 letters a line (the audit's least is 36).
- The top bar: a 28 px line is about 36 high, inside the 54. At t-floor-36 a line is about 47: still inside.
- The log (146 high, about 128 inside): 3 whole lines at 28 px, where 1080p shows 4. It scrolls.
- Verb names (about 121 px wide) wrap to two or three lines at 28 px. Not cut, but the rack grows tall.

### The other way, not taken

Set GamePanel to a constant pixel size with scale = max(1, the sum above), so nothing shrinks under 1080p.
One line of code, no classes; but every edge, icon and picture stays big too, and at 720p the left column is
371 px wide with 22 px text: about 38 letters. The classes keep the layout and lift only the letters.

## Not done

- Unity not run; nothing compiled; no screen seen.
- FrameRoot.cs was grepped, not read whole: the namespace, the usings and the exact shape of Build() and
  Update() must be checked against the two insert points.
- Not checked: that L_Chart's UIDocument uses GamePanel, and that FrameRoot's `style` field holds Frame.uss
  (the audit left these open too).
- MapView.uss, MarketPanel.uss, TextPanel.uss, MainMenu.uss: not touched, not read.
- Contrast, the true italic file, line heights and the pixel font for signs (audit, later sections): not in
  this fix.
