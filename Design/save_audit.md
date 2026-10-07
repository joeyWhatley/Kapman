# Save audit (7 Oct 2026)

Read, not run. Paths are under Assets/Kapman/Code. "Game/" = Assets/Kapman/Code/Game.

## How a save is made

- One file a slot: persistentDataPath/saves/<slot>.kapman.json.gz (gzipped JSON) + <slot>.kapman.info (date, where).
- SaveGame.Save (Game/SaveGame.cs:75) writes: version, date, saved, hours, level, ship, course, voyage, trade,
  markets, shocks, lessons, world, seen, deeds, known, chest, hoards, standing, orders, encounters, content.
- SaveGame.Load (SaveGame.cs:136) reads them back over the running game, clock first.
- ContentSave (Game/Content/ContentSave.cs:34 Register, :54 Capture, :73 Restore): a part names itself by a key.
  Only ONE part is registered today: "self" (Game/Self/Self.cs:337).
- Who calls it: F5/F9 (KapmanGame.cs:476 SaveKeys, called every Update at :492, not gated), the pause menu
  (UI/MainMenu.cs:144 save, :184 LoadSlot), the old map (Map/MapView.cs:246), the graphs (Graphs/PortNodes.cs:324).
  The frame has no save code of its own: its Menu button opens MainMenu (UI/Frame/FrameRoot.cs:159).
- SaveGame.Busy (SaveGame.cs:53) refuses a save or a load: during a ship meeting / raid / patrol stop, while aground,
  and while TextPanel is open away from a port. It does NOT look at an open card.
- No autosave anywhere (no other caller of SaveGame.Save).

## The table

| State | Saved? | Where (class.method) | What the player sees lost after a load |
|---|---|---|---|
| Clock and date | YES | SaveGame.Save "hours" (:89); Load sets clock.GameHours (:165), game.ResyncHours (:188) | Nothing. Hour-exact. |
| Clock speed / pause | NO | - | The rate is whatever it was at the moment of loading. Small. |
| Her place, heading, made fast or not | YES | SaveGame "ship" x, z, heading, anchored, lying_at (:91-95); Load Teleport (:198) | Nothing, IF the level is the same. Another level: she stays where she is, a log warning only (:201). Her speed through the water is not saved: she starts from rest. |
| Course (destination, points, next point, km, depths) | YES | Course.Capture / Restore (Voyage/Course.cs:201, :214) | Course.Kinds (river or sea per leg, Course.cs:35) is cleared on load (:219) and never written or rebuilt: after a load Course.LegIsRiver (:177) says "not river" for every leg, so whatever reads it (river pace, river rules) is wrong until a new course is set. |
| Lying to for the night or tide, and "bound for" after it | NO | FrameRoot.Rack statics: Until, resume (UI/Frame/FrameRoot.Rack.cs:21-23) | If saved while lying to with the course put by, the port she was bound for is forgotten; set the course again. (Not traced to the end.) |
| Tide wait, resting | NO (ended on load) | TideWait.Loaded (Voyage/TideWait.cs:342), Rest statics (Voyage/Rest.cs:19) | The wait or the rest stops; she waits again by herself if still short of water. |
| Aground | Cannot save | SaveGame.Busy (:61) | Save refused while aground. |
| Silver (purse, every coin kind) | YES | Trade.Capture "purse" -> Purse.Capture (Trade/Trade.cs:57, Trade/Purse.cs:27) | Nothing. |
| Hold (goods, cost, day bought, capacity) | YES | Trade.Capture "hold", "capacity" -> Hold.Capture (Trade/Hold.cs:44) | Nothing. |
| Debt (sum, creditor, due day) | YES | Trade.Capture "debt", "creditor", "due" (Trade.cs:59-60) | Nothing. |
| Crew: number, health, morale, water, pay owed, days at sea, seasick, open sack and cask | YES | Voyage.Capture / Restore (Voyage/Voyage.cs:111, :117) | Nothing. The crew is a number and four meters; there are no named hands in the code to lose (only UI/Frame/Panels/CrewPanel.cs). |
| The self: people, faith, past, chosen | YES | Self.Capture / Restore via ContentSave "self" (Self/Self.cs:269, :292) | Nothing. An old save with no self loads as the plain start, taken as chosen. |
| The five stats, tongues, what was seen, who was met | YES | Self.Capture "stats", "tongues", "seen", "met" (Self.cs:271-283) | Nothing. Stats are clamped to min/max on load. |
| Prices known and when (the price notes) | YES | Trade.Capture "ledger" -> PriceNotes.Capture (Known/PriceNotes.cs:169, :193). Thinned first: Trade.Ledger.Thin (SaveGame.cs:83) | Nothing recent. SAVING itself thins old notes (keepAllDays, seasonAfterYears), so the act of saving changes the live game too. |
| News heard, ships sighted and spoken to, places heard of, standing known | YES | Known.Capture / Restore (Known/Known.cs:449, :471) | Nothing. Version-1 saves lose most of it. |
| The spoken word of the news (which line is said) | NO, rebuilt | Word.recentNews cache (Known/Word.cs:93) | Nothing: worked out again from Known. |
| Flags and chain progress (ch_*, unlock_*, changed_*, house_*, act_*, st_*) | YES | EncounterState.Capture "flags" value + until (Core/Encounters/EncounterState.cs:209); reached by SaveGame "encounters" -> EncounterRunner.Capture (:83) -> EncounterDirector.Capture (:515) | Nothing. All content flags live here (Content/ContentFlags.cs:1-6). |
| Cards done, last shown (cooldowns), visits per port, waived and eased tolls | YES | EncounterState.Capture "done", "shown", "visits", "waived", "eased" (:209-237) | Nothing. |
| Scheduled cards (card, day, port) | YES | EncounterState.Capture "scheduled" (:228-230) | Nothing. |
| Cards waiting in line | YES | EncounterState.Capture "queue" (:236) | Nothing. |
| The call in progress (port, visits before, start day), watch armed | YES | EncounterState "call_port", "call_before", "call_start"; EncounterDirector "armed" (:522) | Nothing. Only Armed of the mooring watch is kept, not what else the watch holds (not read). |
| An OPEN card, not yet answered | PARTLY | EncounterDirector.Capture puts its id first in the queue (:517-519); Restore sets Current = null (:528) | The card closes and opens again from its first page. Its people are drawn again (cardActors are not saved), so a rolled name or face may differ if the draw is not seeded the same. Not checked. |
| An open card AFTER the choice (the outcome page still up) | NO | Capture skips it when Current.Chosen (:518) | The outcome text is gone; the effects were already applied, so nothing of substance. |
| A card's waiting time | Nothing to save | - | There is no timer on an open card in the code: a card stops the clock (PanelClock.OnlyCards, FrameRoot.cs:91). Queue and "last_hour" are saved. |
| The dice of the cards | Nothing to save | EncounterDirector :581-584: seed + card + game hour | Same card at the same hour rolls the same. Loading and choosing an hour later rolls anew. |
| People: who they are, who holds which role, alive or dead | YES | NpcRegistry.Capture "npcs", "holders", "counts", "generations" (Core/Encounters/NpcRegistry.cs:405) | Nothing. |
| People's feelings, what is owed, met, their memory of each deed | YES | NpcRegistry.Capture "relationships": feeling, owed, met, memory (:423-432) | Nothing. |
| Which line a person has said in a talk (the ask picks) | NO | PersonTalk.picks static (UI/Frame/PersonTalk.cs:26) | A person may say again a rumour, saying or riddle already heard. Also NOT cleared by a load: picks of the game before carry over. |
| Reputation / standing with ports | YES | Reputation.Capture / Restore (Game/Reputation.cs:195, :214) | Nothing. |
| Orders taken and on offer | YES | Commissions.Capture / Restore (Ports/Commissions.cs:139, :145) | Nothing. |
| The world's other ships | YES, whole | WorldSim.Capture "ships" (Sim/WorldSim.cs:977-1047): place, path, leg, purse, hold, what each knows, raider band, and each ship's own dice (rng state + inc, :1031) | Nothing: the world goes on as it would have. |
| The waterfront boards (what each port knows), raids | YES | WorldSim.Capture "boards", "snapshots", "news", "raided", "raid_year" (:1048-1053) | Nothing. |
| Markets (stock, glut, dearness, closing, ruin) | YES | Markets.Capture -> Market.Capture (Trade/Market.cs:447, :120) | Nothing. |
| Shocks on the markets, lessons, deeds | YES | Shocks.Capture (Trade/Shocks.cs:212), Lessons (Beats/Beats.cs:285), Sim.Deeds (Sim/Patrols.cs:99) | Nothing. |
| The chart seen (fog) | YES | Exploration.Capture / Restore (Map/Exploration.cs:125) | Nothing. |
| The chest (items, tries, solved, marks, notes, threads, Latin, toll-free, promised) | YES | Chest.Capture / Restore (Chest/Chest.cs:848, :882); Hoards.Capture (Chest/Hoards.cs:227) | Nothing. |
| The log of what was said (the frame's notices) | NO | FrameLog.Lines static, keeps 200 (UI/Frame/FrameGame.cs:126) | After a load the log still shows the lines of the game BEFORE the load; after a restart it is empty. The player loses the written trail of the voyage. |
| The chronicle's place | Rebuilt | Chronicle.StartAt(clock date) (SaveGame.cs:187) | Nothing, if it only depends on the date. |
| The frame's layout (workspaces, layout kind, focus, which panel in which slot) | YES, but not in the save | PlayerPrefs "kapman.frame.windows": FrameWindows.Capture (UI/Frame/FrameWindows.cs:242), written only in FrameRoot.OnDisable (FrameRoot.cs:112), read in OnEnable (:79) | Per machine, not per saved game. Lost if the game crashes before the frame is disabled. |
| Panel sizes, floating windows' places | NOT FOUND | FrameWindows.Capture writes name, layout, focus, slots only (:247) | No sizes in the kept string. FrameRoot.Floats.cs has no PlayerPrefs and no capture: floating windows and their places are lost. |
| Chart colours | YES, PlayerPrefs | MapPalette (Map/MapPalette.cs:61, :82); set again from the self on load (Self.cs:331) | Nothing. |
| The wiki's known pages | NO SUCH CODE | No "wiki" in any game code under Game/ | Nothing exists to save. When the wiki is built it needs a ContentSave part (or to read Known + Self.seen, which are saved). |
| Weather | NOT FOUND | No Capture found for weather | Not checked whether weather follows from the date and seed. If not, the wind and sky change on a load. |

## The five losses that would hurt most

1. No autosave, and no save on quit. One file a slot, written only when the player asks (F5 or the menu).
   A crash or a closed window loses everything since the last asked save. In a one-life game this is the worst.
2. The frame's log of what was said is not saved (FrameLog.Lines, FrameGame.cs:126), and is not cleared on a load
   either: after F9 the log shows the abandoned future. In a game of words the log is the player's memory.
3. A save taken in one level loads only in that level (SaveGame.cs:193-202). Loaded elsewhere, everything is
   restored except her place, with no word to the player (a console warning). Check that L_Chart always has the
   same levelId, and say it on screen if not.
4. An open card is put back in line but not kept as it was (EncounterDirector.cs:517-528): it reopens from the
   start, with its people drawn again. A long card half read is read again; a face may change. And ChartWorld.SaveLoaded
   (Chart/ChartWorld.cs:85) is never set true anywhere in the code, though three places read it (ChartWorld.cs:64, :71;
   EncounterRunner.cs:180; FrameRoot.Self.cs:28): today that is harmless because a load runs over a game already
   started, but any future "load before Start" path would get the new-game cargo, the home berth and the who-are-you ask.
5. Small frame memory is lost or carried over wrongly: which lines a person has already said (PersonTalk.picks, :26,
   not saved and not cleared), the port she was bound for while lying to (FrameRoot.Rack resume, :23), panel sizes,
   and the layout itself (PlayerPrefs, written only on a clean OnDisable).

Also worth a line: saving thins the price notes of the live game (SaveGame.cs:83). Saving should not change the game.

## Does a game saved at any moment load back to the same moment?

- Lying in port, nothing open: YES. Clock, place, purse, hold, debt, crew, self, known, flags, people, world: all back.
- Under way at sea, nothing open: YES in substance. Place, heading, course and next point are back; the other ships
  are back with their own dice. She starts from rest (speed not saved); a tide wait or a rest is ended and begins
  again by itself; lying-to's "bound for" may be forgotten.
- With a card open, unanswered: NEARLY. Busy does not refuse it. The card comes back first in line and opens again
  from its first page; nothing was applied yet, so nothing is lost or doubled. Its cast may be drawn again.
- With a card open, answered, outcome showing: YES in substance (effects kept), the outcome page is gone.
- During a ship meeting, a raid, a patrol stop, or aground: NO SAVE. Busy refuses (SaveGame.cs:58-62).
- With the old TextPanel open at sea: NO SAVE (SaveGame.cs:63). In the frame TextPanel.External is true
  (FrameRoot.cs:86); whether TextPanel.Current.Open can still be true there was not checked. If it can, F5 at sea
  is refused with "close the panel first" and no panel to close.
- Loaded in another level than it was saved in: NO. Her place is not restored.
- Loaded with a newer game version: version must be 2 or 1 (SaveGame.cs:157). No migration beyond that; a new
  ContentSave part gets null and starts afresh, which is right.

## Not checked (out of time)

- Checked late: Hold.Restore clears first (Trade/Hold.cs:49), good. FrameLog is never cleared anywhere. PersonTalk.picks is cleared only at play start (:156).
- The mooring watch beyond Armed; weather; FrameRoot.Floats window places;
  whether L_Chart sets LevelInfo.levelId; Hold.Restore clearing before filling; Game/Content/Places and Year
  (no static lists found there, so probably flags only).
