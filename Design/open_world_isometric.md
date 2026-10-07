# The open world at 1:80

The user's direction of 4 October, afternoon (DECISIONS.md, bottom). One open world of the North Sea, Britain and
Brittany, Norway, the Baltic and the road to Kiev, Iceland, Greenland and Vinland. Its lengths and distances are at about
1:80; the ship, the buildings and the rivers' widths are true size ("the river width in scale with the boat, that's the
important part"). Only the water and a waterfront band of about 60 m are built; nothing inland. Everything built so far
(the weather, the sound, the economy, the beats, the encounters, the ship's body and sail, the water records) carries over.

The first map of it: https://claude.ai/artifact/8weqvazJtcsiMWw6svbbMA (Bake/world/make_world_map.py -> world_map.json).

## The numbers

- **The frame:** a Lambert conformal projection over the North Atlantic (standard parallels 50 and 68 N, centred 58 N
  5 W), divided by 80: about 100 by 54 km of game world, nearly all of it open sea.
- **Times** (straight line at about 5 knots, 1x and 4x): Dorestad to Lundenwic 32 and 8 minutes; Hedeby to Birka 1 hour
  and 15 minutes; Hólmgarðr to Kiev 1.2 hours and 19 minutes; Reykjavík to Vinland 3.3 hours and 49 minutes.
- **The coast** at 1:80: a 1 km headland is about 12 m in the game, so Natural Earth's 10 m coastline (public domain)
  holds all the detail the game can show.
- **Rivers** at their true width beside the true-size boat (the Kromme Rijn about 100 m, the Lek about 350 m capped), up
  to about 300 m; estuaries, broad lower rivers and firths narrow to about 300 m and open out to the sea at their mouths.
  The course is the real one shortened 80 times, its bends eased so no bend is tighter than about three times the width.
- **The waterfront band:** about 60 m of land beside all water: banks, beaches, cliffs, marsh, reeds, the first row of
  trees and houses; a soft haze beyond it.
- **Ports:** where the real port is, with a waterfront by importance: an emporium like Dorestad about 300 m of jetties and
  storehouses, a market town about 150 m, a hamlet or monastery about 50 m; the buildings true size.

## c. 800, not today

The coast and rivers are today's from the data, put back to c. 800 where the research knows: the Netherlands from the Vos
reconstruction (Lake Almere, the Rhine mouth at Katwijk, the open Wadden), the Wantsum channel open (Thanet an island),
the Flemish coast, the Baltic shores raised by Scandinavia's land uplift since 800 (NKG2016LU: Mälaren a bay of the
Baltic, Birka on the water), Iceland, Greenland and Vinland as the sagas and the digs give them.

## Building it

1. **The world baker and the North Sea core.** An editor baker cuts the world into 1 km tiles: the coast band (its
   ground and its kind: cliff, dune, marsh, rock, from the real elevation and land cover at that coast), the sea's depth
   (GEBCO and EMODnet, true depth), and an index of what each tile holds. A persistent world scene (the clock, the
   weather, the sound, the game, the ship) and tile streaming around the ship with a floating origin. Done when she sails
   from the Rhine mouth along the Holland coast to the Thames mouth with the coast coming and going and no hitch.
2. **The Rhine to Dorestad.** The river baker: the Rhine's branches at true width along their shortened courses, banks
   and bands, the stream from the water records; Dorestad rebuilt as a 300 m emporium on the new band. Done when she
   sails from Dorestad's berth to the open sea in one go.
3. **The view.** A small test scene with a simulated sea, wind and waves and several camera angles and distances to
   choose from (the user's ask); then the chosen view near land, the free and cinematic camera on the open sea.
4. **Outward:** the Thames and the English rivers, the Scheldt, the Canche and the Seine; then Jutland and the Baltic,
   Norway, the road to Kiev, and the Atlantic islands; each port at its importance.
