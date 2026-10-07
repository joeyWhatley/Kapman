# Save the frame: the log, the layout, sizes, open full windows (7 Oct 2026)

Read, not run, not compiled. Paths are under Assets/Kapman/Code/Game unless said. Nothing under Assets was changed.

## What is wrong today (checked in the code)

| Thing | Where | State |
|---|---|---|
| The log | `FrameLog.Lines`, UI/Frame/FrameGame.cs:126 (static, keeps 200) | Not saved. Not cleared on a load: after F9 it shows the abandoned future. Cleared only at `SubsystemRegistration` (:152-157). |
| The layout | `FrameWindows.Capture` (UI/Frame/FrameWindows.cs:242) -> PlayerPrefs `kapman.frame.windows` | Read in `FrameRoot.OnEnable` (FrameRoot.cs:78-79), written ONLY in `OnDisable` (:111-112). A crash loses it. Per machine, not per game. |
| Panel sizes | nowhere | `FrameWorkspace` (FrameWindows.cs:11-17) has name, layout, slots, focus. No size field exists. Ruling 9 (DECISIONS.md:124-129) asks small / medium / large. |
| Full windows | nowhere | Not built: wiring_plan.md:1323-1324 says the full window is to be `OpenFloat` with the panel as content. `FrameRoot.Floats.cs` keeps `floats` (:75) in memory only; a float's place is set from its slot (:174-175), there is no dragging and no capture. |

## How a part gets into the saved game (the one to copy: "self")

1. `Self.OnPlay` (Self/Self.cs:332-338) runs at `SubsystemRegistration` and calls `ContentSave.Register("self", Capture, Restore)`.
2. `SaveGame.Save` (SaveGame.cs:111) writes `["content"] = ContentSave.Capture()`: every registered part under its key (Content/ContentSave.cs:54).
3. `SaveGame.Load` (SaveGame.cs:186) calls `ContentSave.Restore(Json.Obj(data, "content"))`: each part gets its piece, or NULL when the save has none (an older save): it must then put itself as at the start (ContentSave.cs:17-19, :73).
4. Capture returns plain data: `Dictionary<string, object>`, `List<object>`, string, double, bool, null. Numbers come back as double.

So: ONE new part, key `"frame"`. SaveGame.cs is NOT edited. The save's version stays 2. An old save loads (the part gets null).

## The saved shape

```json
"content": { "frame": {
  "log":     [ { "text": "...", "day": 12.375, "date": "3 July", "part": "morning" } ],
  "windows": "0;Sea|0|0|voyage,hold|voyage:2;Port|0|1|trade,port,people|;Self|2|0|self,bonds,faith,tongues|;House|2|0|house,tasks,word,chronicle|",
  "full":    [ { "panel": "crew", "left": 52.5, "top": 47.0 } ]
} }
```

`windows` is the same one line `FrameWindows.Capture` makes today, with a FIFTH field a workspace: the sizes, `panel:size` (0 small, 1 medium, 2 large; a panel not named is medium).

## Change 1: FrameLog can be captured, restored, cleared

UI/Frame/FrameGame.cs, inside `FrameLog`, after `Add` (after line 150), before `Reset` (:152):

```csharp
        /// <summary>The log for the saved game: every line kept (200 at most).</summary>
        public static List<object> Capture()
        {
            var saved = new List<object>(Lines.Count);
            foreach (var l in Lines)
                saved.Add(new Dictionary<string, object> { ["text"] = l.text, ["day"] = l.day, ["date"] = l.date ?? "", ["part"] = l.part ?? "" });
            return saved;
        }

        /// <summary>The log of a loaded game. Null (a save from before the log was kept): empty, never the lines of the
        /// game that was running.</summary>
        public static void Restore(List<object> saved)
        {
            Lines.Clear();
            if (saved != null)
                foreach (object o in saved)
                {
                    if (!(o is Dictionary<string, object> d))
                        continue;
                    string text = Json.Str(d, "text");
                    if (text.Length > 0)
                        Lines.Add(new Line { text = text, day = Json.Num(d, "day"), date = Json.Str(d, "date"), part = Json.Str(d, "part") });
                }
            if (Lines.Count > Keep)
                Lines.RemoveRange(0, Lines.Count - Keep);
            Version++;                                                    // (the frame draws the log again: FrameRoot.FillLog)
        }

        /// <summary>A new game: nothing said yet.</summary>
        public static void Clear() => Restore(null);
```

`Version++` is enough for the redraw: `FillLog` (FrameRoot.cs:704) compares `logDrawn` with `FrameLog.Version`.
`Json` (Core/Data/Json.cs:26-38) is used unqualified in Self.cs; if it does not resolve in FrameGame.cs, copy the using from Self/Self.cs.

## Change 2: sizes in the windows' state

UI/Frame/FrameWindows.cs.

a) `FrameWorkspace` (line 11-17), add one field. By panel id, not by slot index, so Add / Close / Step / Set / the swap need no change:

```csharp
        /// <summary>How big each panel's window is in this workspace (0 small, 1 medium, 2 large). Not named: medium.</summary>
        public readonly Dictionary<string, int> sizes = new Dictionary<string, int>();
```

b) In `FrameWindows`, beside `SetLayout` (about line 119):

```csharp
        public const int Small = 0, Medium = 1, Large = 2;

        public int SizeOf(int index)
        {
            var w = Space;
            return index >= 0 && index < w.slots.Count && w.sizes.TryGetValue(w.slots[index], out int s) ? s : Medium;
        }

        public void SetSize(int index, int size)
        {
            var w = Space;
            if (index < 0 || index >= w.slots.Count)
                return;
            size = Math.Max(Small, Math.Min(Large, size));
            if (SizeOf(index) == size)
                return;
            w.sizes[w.slots[index]] = size;
            Version++;
        }
```

c) `Capture` (line 242-249): replace the one `b.Append(...)` line in the loop (:247) with

```csharp
            {
                b.Append(';').Append(w.name).Append('|').Append((int)w.layout).Append('|').Append(w.focus).Append('|').Append(string.Join(",", w.slots)).Append('|');
                bool first = true;
                foreach (var kv in w.sizes)
                {
                    if (kv.Value == Medium)
                        continue;
                    b.Append(first ? "" : ",").Append(kv.Key).Append(':').Append(kv.Value);
                    first = false;
                }
            }
```

d) `Restore` (line 253-282): the check at :264 `f.Length != 4` becomes `(f.Length != 4 && f.Length != 5)` (a string kept by today's build still reads), and after the slots loop (:267-269), before `read.Add(w)`:

```csharp
                if (f.Length == 5)
                    foreach (string pair in f[4].Split(','))
                    {
                        int colon = pair.LastIndexOf(':');
                        if (colon > 0 && int.TryParse(pair.Substring(colon + 1), out int size))
                            w.sizes[pair.Substring(0, colon)] = Math.Max(Small, Math.Min(Large, size));
                    }
```

e) FrameRoot.cs `WindowsKey` (:426-431): add the sizes so a change of size draws the windows again, e.g. append
`+ "/" + string.Join(",", System.Linq.Enumerable.Select(w.slots, (s, i) => Windows.SizeOf(i)))`; and in `Window` (:497) `win.AddToClassList("size-" + Windows.SizeOf(index));`. The size buttons and the three USS classes are the builder of ruling 9's work; the state and the save are ready for them.

Panel ids must not hold `; | , :` (today's ids are plain words: voyage, hold, trade, port, people, self, bonds, faith, tongues, house, tasks, word, chronicle).

## Change 3: the part itself (a new file)

New file UI/Frame/FrameSave.cs:

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// WHAT THE FRAME KEEPS IN A SAVED GAME (ContentSave part "frame"): the log of what was said, the arrangement of the
    /// windows with their sizes, and which full windows stand open and where. Static, so a save made while the frame is
    /// not up (the old map, a graph) still writes what the frame last had, and a load before the frame is up is held
    /// for it.
    /// </summary>
    public static class FrameSave
    {
        public const string Key = "frame";

        /// <summary>A full window that stands open: its panel, and its place in percent of the frame (below 0: where a new one goes).</summary>
        public struct FullWindow
        {
            public string panel;
            public float left, top;
        }

        /// <summary>The arrangement last known (FrameWindows.Capture's line), "" for none.</summary>
        public static string Layout = "";
        /// <summary>The full windows last known, back to front.</summary>
        public static readonly List<FullWindow> Full = new List<FullWindow>();
        /// <summary>Counts up when a load hands the frame a new state (the frame takes it in its Update).</summary>
        public static int Loads { get; private set; }

        /// <summary>Opens a panel's full window and gives its element back (null: could not). Set by whoever builds the
        /// full windows (ruling 9). While it is null the saved list is kept as it is and written out again, so nothing is lost.</summary>
        public static Func<string, VisualElement> OpenFull;

        public static object Capture()
        {
            FrameRoot.Current?.SaveState();                              // (the frame up: its state as it is now)
            var full = new List<object>();
            foreach (var w in Full)
                full.Add(new Dictionary<string, object> { ["panel"] = w.panel, ["left"] = (double)w.left, ["top"] = (double)w.top });
            return new Dictionary<string, object> { ["log"] = FrameLog.Capture(), ["windows"] = Layout, ["full"] = full };
        }

        /// <summary>Null (an older save): the log is emptied, the arrangement on screen stays as the player has it.</summary>
        public static void Restore(object saved)
        {
            var d = saved as Dictionary<string, object>;
            FrameLog.Restore(d != null ? Json.Arr(d, "log") : null);
            Full.Clear();
            if (d == null)
                return;
            string windows = Json.Str(d, "windows");
            if (windows.Length > 0)
                Layout = windows;
            if (Json.Arr(d, "full") is List<object> full)
                foreach (object o in full)
                    if (o is Dictionary<string, object> w && Json.Str(w, "panel").Length > 0)
                        Full.Add(new FullWindow { panel = Json.Str(w, "panel"), left = (float)Json.Num(w, "left", -1.0), top = (float)Json.Num(w, "top", -1.0) });
            Loads++;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            Layout = "";
            Full.Clear();
            Loads = 0;
            OpenFull = null;
            ContentSave.Register(Key, Capture, Restore);
        }
    }
}
```

`ContentSave.Register` takes `Func<object>`: `Capture` returns `object` for that reason.

## Change 4: FrameRoot gives and takes its state, and writes the settings at every change

UI/Frame/FrameRoot.cs.

a) Fields, after line 61 (`string heardLast;`):

```csharp
        int loadsTaken, keptVersion = -1;
        float nextKeep;
```

b) `OnEnable`, lines 78-79 become (a loaded game's arrangement wins over the settings; a new game takes the settings):

```csharp
            if (keepLayout && !FreshLayout && !Windows.Restore(FrameSave.Layout))
                Windows.Restore(PlayerPrefs.GetString(Pref, ""));
            loadsTaken = FrameSave.Loads;
            keptVersion = Windows.Version;
```

c) `OnDisable`, lines 111-112 become:

```csharp
            if (Windows != null)
                SaveState();                                             // (before FloatsDisable: see below)
            if (keepLayout && !FreshLayout && Windows != null)
            {
                PlayerPrefs.SetString(Pref, Windows.Capture());
                PlayerPrefs.Save();
            }
```

and MOVE that block up to stand before `FloatsDisable();` (line 107), which clears `floats`.

d) New methods (after `Show`, about line 570):

```csharp
        // ------------------------------------------------------------------ the saved game (FrameSave)

        partial void FloatsCapture();
        partial void FloatsRestore();

        /// <summary>The frame's state as it is now, for the saved game.</summary>
        public void SaveState()
        {
            if (Windows != null)
                FrameSave.Layout = Windows.Capture();
            FloatsCapture();
        }

        /// <summary>A loaded game's arrangement over the one on screen.</summary>
        void LoadState()
        {
            loadsTaken = FrameSave.Loads;
            if (!FreshLayout && Windows.Restore(FrameSave.Layout))
            {
                pick = -1;
                drawnKey = null;
            }
            FloatsRestore();
            nextFill = 0f;
        }

        /// <summary>The settings follow every change of the windows (not only a clean close), written to disk a few seconds after.</summary>
        void KeepLayout()
        {
            if (!keepLayout || FreshLayout || Windows.Version == keptVersion)
                return;
            keptVersion = Windows.Version;
            PlayerPrefs.SetString(Pref, Windows.Capture());
            nextKeep = Time.realtimeSinceStartup + 3f;
        }

        void OnApplicationQuit()
        {
            if (keepLayout && !FreshLayout && Windows != null)
            {
                PlayerPrefs.SetString(Pref, Windows.Capture());
                PlayerPrefs.Save();
            }
        }
```

e) `Update`, right after `Workspace();` (line 313):

```csharp
            if (loadsTaken != FrameSave.Loads)
                LoadState();
            KeepLayout();
            if (nextKeep > 0f && Time.realtimeSinceStartup >= nextKeep)
            {
                nextKeep = 0f;
                PlayerPrefs.Save();
            }
```

Why in Update and not inside `FrameSave.Restore`: ContentSave.Restore runs in the middle of SaveGame.Load (SaveGame.cs:186), before the ship is put back and before `Loaded` fires (:207). The frame takes the state on its next Update, when the game is whole.

`Workspace()` (FrameRoot.cs:361-376) switches to Port or Sea only when "lying in port" CHANGES. So the saved workspace stands after a load unless the load itself moved her between port and sea, in which case Port / Sea comes up, as it does in play.

## Change 5: the full windows that stand open

UI/Frame/FrameRoot.Floats.cs, inside `partial class FrameRoot` (after `FloatsDisable`, about line 362):

```csharp
        /// <summary>A panel's full window is the float "full.<panel id>" (ruling 9; wiring_plan 8, item 9).</summary>
        public const string FullPrefix = "full.";

        partial void FloatsCapture()
        {
            if (FrameSave.OpenFull == null)
                return;                                                   // (full windows not built: keep what the save had)
            FrameSave.Full.Clear();
            foreach (var f in floats)                                     // back to front
                if (f.spec.id != null && f.spec.id.StartsWith(FullPrefix, System.StringComparison.Ordinal))
                    FrameSave.Full.Add(new FrameSave.FullWindow
                    {
                        panel = f.spec.id.Substring(FullPrefix.Length),
                        left = f.el.style.left.value.value, top = f.el.style.top.value.value,   // percent, as Build sets them (:174-175)
                    });
        }

        partial void FloatsRestore()
        {
            if (FrameSave.OpenFull == null)
                return;
            // (the full windows of the game that was: shut. USE THE FILE'S OWN CLOSE HERE; its name was not read.)
            foreach (var w in FrameSave.Full.ToArray())
            {
                if (FramePanels.Get(w.panel) == null)
                    continue;                                             // (a panel that is no longer there)
                var el = FrameSave.OpenFull(w.panel);
                if (el == null || w.left < 0f || w.top < 0f)
                    continue;
                el.style.left = Length.Percent(w.left);
                el.style.top = Length.Percent(w.top);
            }
        }
```

Cards, talks and alerts are NOT in this list on purpose: an open card comes back from the encounter queue (EncounterDirector.Capture :517), a talk from the port's graph. Only "full.*" floats are the frame's own to keep.

When dragging or resizing a float is built, write the place as percent into `el.style.left/top` (and width/height the same way, two more numbers in `FullWindow`): the capture above then keeps it with no further change.

## What this does and does not keep

- KEPT in the save: all 200 log lines with their day, date and part of day; workspace on screen; each workspace's layout, focus, panels, sizes; open full windows and their places.
- KEPT on the machine: the same arrangement in PlayerPrefs at every change (a crash loses about 3 seconds at most), for a new game.
- An old save (no "frame"): the log is emptied (right: it was never that game's), the arrangement on screen stays.
- A load over a running game no longer shows the abandoned future.

## Not built yet, so only the save side is ready

- Size buttons and the three size classes (ruling 9): `SetSize` / `SizeOf` are there to call.
- Full windows: nothing opens one today. The builder sets `FrameSave.OpenFull` and uses ids `"full." + panelId`. Until then a saved "full" list is carried through untouched.
- The place in Change 5 that shuts the old full windows needs the float file's close method (not read).

## Not checked

- Whether a NEW GAME from the menu reloads the domain. `FrameLog.Reset` and `FrameSave.OnPlay` run only at `SubsystemRegistration`; if a new game is a scene reload, the old game's log carries into it. If so call `FrameLog.Clear()` where the new game starts (UI/MainMenu.cs; PersonTalk.picks has the same fault, save_audit.md:51).
- FrameWindows.cs lines 60-225 (Add, Close, Set, Step, Mend) were not read; sizes by panel id were chosen so they need no change. `Mend` may leave a size for a dropped panel: harmless.
- Whether `Windows.Focus` bumps `Version` (if not, a change of focus alone is written at the next other change or on quit).
- That `.value.value` of a style set with `Length.Percent` gives the percent number back: by the API, not run.
- Line numbers in Self.cs and SaveGame.cs are worked out from a partial read: a line or two either way.

## Tests to add (EditMode, beside the FrameWindows tests)

1. `FrameWindows`: SetSize, Capture, a new FrameWindows, Restore: sizes equal; a four-field line from today's build still restores.
2. `FrameLog`: Add three lines, Capture, Clear, Restore(Json.Parse(Json.Write(captured))): same text, day, date, part; Version moved.
3. `FrameSave.Restore(null)`: log empty, `Layout` unchanged.
4. `ContentSave.Registered("frame")` after play start.
