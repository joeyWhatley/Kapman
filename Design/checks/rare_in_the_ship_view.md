# The rare events in the ship view

Check of 7 Oct 2026 against `Assets/Kapman/Game/Data/story/rare_793.json` (forty events). Nothing here is invented: "seen" is the data's own `seen.window` line, "log" its `seen.log`. The "to draw or build" column and the names are proposals. Names follow the art list's kind.id form, but the kinds used here (sky, sea, shore, deck, ship, sound, card, beast) were not checked against the kinds the art loader already knows: rename to fit.

Counts: 40 events. 15 can be shown from the ship in the first life (793 to 806), 10 more belong to later lives, 15 are only heard of or are port scenes with nothing in the sky or on the water.

Dates are Julian. "fire" is the day the data puts a dated card on the scheduler. "chance" is once-a-life.

## What the task asked about, and what the data holds

| Asked for | In the data? |
|---|---|
| Comet | Yes, later lives only: comet_837, comet_912, comet_989, star_1066. None in 793 to 805. |
| Eclipse of the moon | Yes, first life: moon_796, moons_800_802, signs_806, plus four reckoned ones (796-09-21, 798-02-05, 803-11-03, 805-09-12). |
| Eclipse of the sun | Later lives only: signs_807, sun_810, sun_840, sun_1030. The file says: no eclipse of the sun could be seen from the North Sea between 793 and 805, do not invent one. |
| Northern lights | Not as an event of its own. sky_signs_793 has "sheets of light over Northumbria" by word, and one optional red light low in the north, at night in May 793. |
| Ring round the sun or moon | Yes: signs_806 (a ring round the sun, 806-08-30; a cross in the moon, 806-06-04). |
| Red sky | red_sun_939 (later): a blood-coloured sun and a sky like dirty wool. Nothing in the first life. |
| Whale | Only stranded, on the sand, seen from a moored ship: stranded_whale. No whale at sea. |
| Ice | ice_in_june (later, north and west of the Faroes). First life: frost_800, frost on the ship in July. |
| Glowing sea | Not in the data. |
| Waterspout | Not in the data ("whirlwinds" are in the 793 news, by word only). |
| Wreck | empty_ship: a drifting hull with nobody aboard. |

## A. Seen from the ship, first life (15)

### In the sky

| id | date or chance | where | what the data says is seen | to draw or build | proposed name |
|---|---|---|---|---|---|
| moon_796 | 796-03-28, between cock-crow and dawn; fire 796-03-28; recorded | lundenwic, sarre, fordwic, quentovic, dorestad; any water; year 796, month 3 | "The moon going down the western sky with a bite out of it, then the colour of old copper." | A moon disc that can take a shadow bite and then a copper tint, low in the west before dawn. One effect serves every darkened moon (the card rare.moon_dark is shared, shown at most twice in a life). | sky.moon_dark |
| (same event, reckoned only) | 796-09-21 evening, total; 798-02-05 evening, partial; 803-11-03 about midnight, total; 805-09-12 at moonrise, partial; not recorded by any annal | as moon_796 | no window line of their own | The same effect with a partial and a total setting. No chronicle line may name them. | sky.moon_dark (partial, total) |
| moons_800_802 | 800-01-15 evening (partial); 802-05-21 before dawn (total); fire 800-01-15 and 802-05-21; recorded | lundenwic, sarre, fordwic, dorestad, quentovic, ripa; any water | "January: a bitten moon over the frozen hard, in the winter port. May: a red moon going down into the sea before sunrise." | The same moon effect; January wants the winter hard with frost or ice at the mooring, May a red moon setting into the sea horizon. | sky.moon_dark; shore.hard_frozen |
| signs_806 | 806-06-04 at dawn (cross in the moon); 806-08-30 (ring round the sun); 806-09-01 at night (moon darkened); fire 806-06-04; recorded | lundenwic, sarre, fordwic; any water; year 806, months 6 to 9 | "June: a pale cross through the setting moon. August: a ring wide as a hand at arm's length round the sun. September: the moon red at midnight." | Two new sky effects: a pale cross through the moon disc, and a halo ring round the sun. The September one is the moon effect again. Note: 806 is one year past the 793 to 805 release. | sky.moon_cross; sky.sun_ring; sky.moon_dark |
| sky_signs_793 | 793, before 8 June, no day; fire 793-06-22; recorded; life "word" | lundenwic, dorestad, quentovic (the news); the thing itself north of the Humber, outside the twelve | "Nothing, in the first release: it is told with the news of the holy island. If the game begins on 1 May, once at night in May, low in the north, a red light that moves." Log: "sheets of light over Northumbria, and whirlwinds, and fiery dragons flying in the air." | Optional only: one low red glow on the northern horizon that drifts, once, at night in May 793. Not a full aurora; the dragons and whirlwinds are told, never drawn. | sky.red_light_north |
| frost_800 | 800-07-06 and 800-07-09; fire 800-07-06; recorded | dorestad, traiectum, daventre, walichrum, witla, medemelacha, quentovic; the Frankish shore; year 800, month 7 | "White on the thwarts and the mooring ropes at dawn, in July." | A frost layer on the ship itself (thwarts, ropes) at dawn, in summer light. The file names this as the first life's stand-in for sea ice. | deck.frost |

### On the water and from the water

| id | date or chance | where | what the data says is seen | to draw or build | proposed name |
|---|---|---|---|---|---|
| storm_799 | 799, no day or month (the game's month: autumn); fire 799-10-05; recorded | sarre, quentovic, walichrum, witla; the strait and the southern North Sea; months 9 to 10, under way | "The sea goes the colour of slate, then white. Other sails, then fewer." | A storm state heavier than the ordinary one: slate water turning white, and other ships in view whose number falls. | sea.great_storm; ship.sails_far |
| mast_fire | chance 0.3 | any water, at night, as a storm passes; under way, storm >= 1, hour 21 to 4 | "A pale flame standing on the mast-head, and a second on the yard. It does not burn the wood." | Two pale glow sprites on the player's own ship: mast-head and yard. The file says: do not call it Saint Elmo's fire. | ship.mast_fire |
| empty_ship | chance 0.25 | any open water, likelier after a great storm; under way; after the 799 storm flags or from 795 | "A hull lying beam-on to the swell, sail in rags, nobody at the helm." | A drifting ship: a hull with a torn sail and no crew, lying across the swell. This is the data's only wreck. | ship.empty |
| bell_in_fog | chance 0.3 | off the dunes of Walichrum, in fog; under way, fog >= 1 | "Nothing but grey. A bell, slow, somewhere to the south. Then from the north." | Nothing new to draw: the fog that exists. It is a sound (a bell, from two sides): a recording is needed, public domain or CC0, never made up. | sound.bell_in_fog |
| hermit | chance 0.3 | a sandbank island or the outer dunes near walichrum, sarre, doccinga; not a river, not moored | "A hut of driftwood and turf on a bar that covers at spring tides, and a thread of smoke." | A small prop on a sandbank: a driftwood and turf hut with a thin smoke. | shore.hermit_hut |
| stranded_whale | chance 0.35 | witla, walichrum, sarre, medemelacha; moored | "A black hill on the sand at low water, with gulls on it and every soul of the place running." | A whale body on the sand at low water, gulls on it, people running. Seen from the moored ship; the only whale in the data. | shore.whale_stranded |
| false_grain_793 | 793, spring to harvest; fire 793-05-15; recorded | dorestad, traiectum, daventre; the river between them; months 5 to 7, Frankish realm | "Corn standing thick and pale along the bank, in a year when there is none." | A pale corn field along the river bank, unreaped. (A new game begins 1 May 793 at Dorestat, so this is the first rare thing a player can see.) | shore.false_grain |
| earth_shakes_801 | 801-04-30 in Italy; the Rhine lands the same year, no day; fire 801-05-10; recorded | dorestad, traiectum, daventre; year 801, month 5 on, moored, Frankish realm | "The river shivers flat, with no wind. A bell rings once with nobody at the rope." | A short water effect: the river surface shivers flat with no wind. And one bell stroke (a recording). | sea.river_shiver; sound.bell_once |
| gale_800 | 800-12-24, Christmas Eve; fire 800-12-24; recorded | lundenwic, sarre, fordwic, walichrum, witla, dorestad, medemelacha, doccinga, quentovic; month 12, moored | "The winter hard at night: thatch in the air, the tide over the bank and still coming." | The winter hard at night in a gale: thatch flying, the water level risen over the bank. | shore.gale_flood |
| fire_798 | 798, no day (the game's month: a dry one; again in 801); fire 798-08-20; recorded | lundenwic; the Thames below it; year 798, moored | "From the river at night: the whole north bank lit from underneath, and the hard black with people." | A burning town seen from the river at night: glow under the smoke on the north bank, a crowd as dark shapes on the hard. | shore.lundenwic_burning |

## B. Seen from the ship, later lives only (10)

These need the same kinds of effect but cannot fire in 793 to 805. Build nothing for them for the first release.

| id | date | where | what the data says is seen | to draw or build | proposed name |
|---|---|---|---|---|---|
| signs_807 | 807-01-31 (Jupiter seemed to pass through the moon); 807-02-11 noon (sun darkened); 807-02-26 (moon darkened, battle lines in the sky); 807-03-17 and eight days (a small dark spot on the sun); 807-08-22 (moon darkened); fire 807-02-11 | any water, year 807 | "A year in which the sky will not keep still." | Sun-dark and moon-dark effects again; a bright star at the moon's edge; a spot on the sun. The sun eclipse of 11 Feb was not checked against the catalog (?). | sky.sun_dark; sky.moon_dark; sky.star_in_moon; sky.sun_spot |
| sun_810 | 809-07-16 (England); 810-11-30 (Francia); fire 810-11-30 | any water | "The light goes thin and cold in the middle of the day; shadows have two edges." | A partial eclipse of the sun: dimmed, cold daylight and a bitten sun (deep partial on the Rhine, not total). | sky.sun_dark (partial) |
| comet_837 | Easter week 837; nearest 10 to 11 April; fire 837-04-03 | any water, months 3 to 5 | "A star with a tail a third of the sky long, moving a hand's breadth a night." | A comet with a very long tail that moves a little each night. | sky.comet |
| sun_840 | 840-05-05; fire 840-05-05 | any water | "Full night at mid-afternoon, and stars." | A total eclipse of the sun: night sky and stars by day. | sky.sun_dark (total) |
| comet_912 | July 912; fire 912-07-10 | any water | "A hairy star low in the north-west after sunset." | The comet, small, low in the north-west at dusk. | sky.comet |
| red_sun_939 | 939, for a day and a half; dim suns to 942; fire 939-06-01 | any water, 939 to 940 | "A sun the colour of blood that can be looked at, and a sky like dirty wool." | A blood-red, dim sun disc and a grey-brown haze over the whole sky. The data's only red sky. | sky.red_sun |
| comet_989 | August to September 989; fire 989-08-20; not recorded (reckoned) | any water | "A hairy star before dawn." | The comet before dawn. | sky.comet |
| sun_1030 | 1030-08-31; fire 1030-08-31 | ogvaldsnes, skiringssal; the coast of Norway | "The day goes dark over the fjord." | The sun-dark effect, over a fjord. | sky.sun_dark |
| star_1066 | from 1066-04-24, seven nights; fire 1066-04-24 | any water, months 4 to 5 | "The long-haired star, every night for a week." | The comet at its largest, seven nights running. | sky.comet |
| ice_in_june | chance 0.3 | north and west of the Faroes; under way, month 6, ice >= 1 | "A white line on the northern horizon that is not cloud; then floes, with a seal on one." | An ice edge on the horizon, drifting floes, a seal on a floe. The file says the North Sea of 793 to 805 has no summer ice. | sea.ice_edge; sea.ice_floes; beast.seal_on_ice |

## C. Only heard of, or a port scene with nothing in the sky or on the water (15)

| id | date or chance | where | what the data says | why not in the ship view |
|---|---|---|---|---|
| mars_797 | 797-07 to 798-07; fire 797-10-01; first life | traiectum, quentovic; moored | "Nothing: that is the point." A clerk asks whether the red star was seen at sea; nobody has. | Nothing to draw. If the night sky ever shows the red star, it must be absent in these months. |
| whirlpool | chance 0.4; life "word" | quentovic, dorestad, traiectum; moored | "In the first life, nothing: a tale told ashore." | Told only. The water itself is in the far north-west, in later lives. |
| elephant_802 | 802-07-20 at Aachen; fire 802-07-20; life "word" | colonia, the Rhine above the twelve | "In the first release: nothing, only talk." If Colonia is open: a grey beast as high as a hay barn at the water's edge, drinking. | Talk only in the first release. A later picture could be beast.elephant. |
| water_clock_807 | 807, at Aachen; later | none | "By word: a clock of brass, turned by water..." | By word. |
| organ_826 | 826, at Aachen; later | none | "By word: a chest of pipes that sings when bellows are trodden." | By word. |
| white_bear | chance 0.2; later | ogvaldsnes, skiringssal, ripa; moored, from 900 | "A white bear on a chain on the foredeck of a small ship, and a thin man feeding it his own dinner." | A harbour scene, later lives. It is on another ship's deck, so it could be shown in the harbour: beast.white_bear. |
| sea_unicorn | chance 0.2; later | ripa, skiringssal, lundenwic; moored, from 990 | "A twisted white horn as long as a man, in a linen wrap, shown under a cloak." | A thing shown in the hand. A card picture: card.sea_unicorn. |
| white_falcon | chance 0.25; first life | ripa; moored, months 6 to 8 | "A hooded bird on a block, nearly white, larger than any hawk you know." | A market scene. A card picture: card.white_falcon. |
| hunnish_sword_796 | 796, spring to summer; fire 796-05-01; first life | quentovic, lundenwic; moored | "A chest open on the toll house table: a belt with gold plates, a curved sword in a strange scabbard, two bolts of silk the colour of wine." | Indoors. A card picture: card.hunnish_sword. |
| helgo_figure | chance 0.5; later | helgo; moored | "On a shelf in a caster's booth, among moulds: a small bronze man sitting cross-legged on a flower, with a mark between his eyes." | Indoors, outside the twelve. card.bronze_man. |
| staff_woman | chance 0.3; first life | ripa; moored | "An old woman with an iron staff, seated apart at the edge of the market." | A market scene. card.staff_woman. |
| king_unknown | 878, 6 January to Easter; fire 878-02-15; later | the Somerset marshes | "A thin man in a herdsman's cloak at a fire in the reeds, who asks sharper questions than a herdsman would." | A meeting ashore, later life. card.king_unknown. |
| singer_verse | chance 0.35; first life | doccinga, lundenwic, medemelacha; moored, Name >= 2 | "A blind man by the fire with a small harp across his knees, and a boy to lead him." | Indoors. card.singer_verse. |
| name_first_said | chance 1.0, once; first life | the first port never visited after Name reaches three | no window line; log: "A man I never saw before knew whose boat this was." | Words only. |
| first_grey | chance 1.0, once; first life | moored, December, from 800 | no window line; log: "Grey in the comb this winter." | Words only (the portrait may change). |

## What to build for the first release, shortest list

1. sky.moon_dark: one moon effect with a partial and a total (copper) setting. Serves moon_796, moons_800_802, the four reckoned moons and signs_806: up to eight nights.
2. ship.empty: the drifting hull with a torn sail (chance 0.25).
3. ship.mast_fire: two pale flames on the player's ship (chance 0.3).
4. deck.frost: white on thwarts and ropes (July 800); the same layer could serve the frozen hard of January 800.
5. sea.great_storm with ship.sails_far (autumn 799).
6. Shore pieces seen from the ship: shore.false_grain (May to July 793, the first one a new player can meet), shore.whale_stranded, shore.hermit_hut, shore.gale_flood, shore.lundenwic_burning.
7. Small: sea.river_shiver (801), sky.red_light_north (optional, May 793), and for 806 sky.moon_cross and sky.sun_ring.

Two are sound, not picture: sound.bell_in_fog and sound.bell_once. Recordings only.

## Not in the data: do not draw

In 793 to 805: a comet, an eclipse of the sun, a red sky, sea ice, a whale at sea. In any year: a glowing sea, a waterspout, a full aurora.

## Not checked

- The file's `also_in_other_files` and `sources` blocks were not read.
- The proposed names were not compared with the kinds in Design/art_guide.md or the art loader.
- The card options, effects and stats of each event were not read here: only id, date, chance, where and seen.
