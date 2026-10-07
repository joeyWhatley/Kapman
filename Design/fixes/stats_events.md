# Stats that events can move

The user, 7 Oct 2026, about 11:30: "Stats can be effected by in game events with caps and dynamic decisions and what not
like all the other games that have implemented such mechanics."

Until now a stat (Tongue, Name, Nerve, Lore, Troth; 1 to 5) rose only by use. This adds what the games the user means
do (Crusader Kings' modifiers and traits, Kingdom Come's wear and drink, Disco Elysium's clothes and thoughts, Pentiment's
past): a stat is a base plus what lies on it, events put things on it and take them off, and every such thing has a
cap and an end.

## The rule

- **Base**: 1 to 5. Set at the start by people, faith and past. Moves slowly: up by use (as built), and up or down by a
  few rare events that change the person for good (a winter with the monks, a hand lost, an oath broken before
  witnesses).
- **What lies on it** (a *mark*): a named thing with a size, a cause the player can read, and an end.
  - size: +1 or -1 (rarely 2); a mark never moves a stat by more than 2;
  - end: a number of days, or "until" something (until you are shriven, until the debt is paid, until you sleep
    ashore, until Ubbi forgives you), or "for good" (then it is a change of the base, not a mark).
- **What you have now** = base + the marks, never under 1 and never over 5 in talk. All marks together may not move a
  stat by more than 2 either way: the third good thing does nothing until one ends. That is the cap, and it is shown.
- **The same mark twice** does not stack: it starts its time again.
- **Shown**: in Yourself each stat is its pips, and under it each mark in a line: "Nerve 2 (+1 the crew saw you stand
  at the Walichrum bar, 12 days left; -1 no sleep, until you rest ashore)". A choice in a talk that a stat opens shows
  the stat as it is now, marks and all.

## Where marks come from

| From | For instance | Stat | How long |
|---|---|---|---|
| A card's outcome | you faced down the toll man; you ran from a sail | Nerve +1; Nerve -1 | 30 days |
| A deed that is known | you paid a dead partner's widow; you sold a Christian out of the realm | Name +1 in that realm; Name -1, Troth -1 | a year; until atoned |
| Body and boat | three days without sleep; the fever; hunger; fresh bread and a night ashore | Nerve -1; Tongue -1; all -1 at the worst; clears them | until rested, healed, fed |
| Dress and gifts | a good cloak at a hall; rags after a wreck | Name +1 there; Name -1 | while worn |
| Faith kept or broken | a fast kept on a hard crossing; meat on a Friday before the crew; a vow paid | Troth +1; Troth -1; Troth +1 | 40 days; until shriven; a year |
| Company | a monk aboard; a known liar in your crew | Lore +1 while he is aboard; Name -1 | while aboard |
| Drink | ale before a bargain | Tongue +1, Nerve +1, Lore -1 | that evening |
| A season | the first week in a strange tongue's land | Tongue -1 there | until you have 25 words of it |
| The past | the ghost of your past met again | as that past says | by its three beats |

## Decisions that are dynamic

A choice may ask for a stat, **offer** a mark, or **cost** one, and say so before it is taken:

- "[Nerve 3] Stand on and show no fear" asks.
- "Swear it on the relic (Troth +1 for a year if kept; Troth -1 until shriven if broken)" offers and warns.
- "Drink with him first (Tongue +1 tonight, Lore -1)" trades one for another.
- A choice that needs more than you have is greyed with what would raise it: "Nerve 3: you have 2. Sleep ashore, or
  have Aldgrim beside you."

## In the engine (for the builder)

- New effect verbs beside `silver`, `morale`, `flag` (Assets/Kapman/Code/Core/Encounters/Effects.cs):
  `mark <stat> <+n|-n> <id> for <days>` ; `mark <stat> <+n|-n> <id> until <flag>` ; `unmark <id>` ; `stat <stat> <+1|-1>`
  (the base, rare).
- New condition names (Cond.cs): `me.<stat>` already reads the stat as it is now; add `me.<stat>.base` and
  `mark.<id>` (is this mark on me).
- The self (Game/.../Self.cs, saved already): a list of marks {id, stat, size, cause words key, ends day or flag}.
  Each game day drops the ended ones. The caps are applied when a stat is read, not when a mark is put on, so a mark
  that is capped out today counts again tomorrow if another ends.
- Yourself panel: the lines above; the brief face shows any stat that is under or over its base in colour.
- Data: Assets/Kapman/Game/Data/story/self_793.json already has, for each stat, "raises", "lowers" and "limits" in
  words: turn those into marks. A lint in Bake/content/check_content.py: a mark's size is 1 or 2; it has an end; its
  words exist.
- Tests: the cap of 2; the same mark twice restarts; a mark ends on its day and on its flag; a stat never reads under 1
  or over 5; saved and loaded.
