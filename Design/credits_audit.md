# Credits and licence audit (7 Oct 2026, from disk only, no web)

What someone else made, and what the project itself records about its licence.
"Record" means a file in the project. UNKNOWN = the project holds no record.
Not checked on the web. Not a legal opinion.

## 1. Asset packs (top folders of Assets that are not Kapman)

| What | Where | Publisher | Licence as recorded | May ship in a sold game (per record) | Credit owed |
|---|---|---|---|---|---|
| Stylized Pack: Meadow Environment + Common | Assets/ANGRY MESH/ | ANGRY MESH (folder name only) | UNKNOWN. The Readme.txt files are how-to only, no licence text. | UNKNOWN on record (no EULA note or receipt on disk) | none recorded |
| Meadow Environment Dynamic Nature, Foliage/Object Shaders, NM Wind | Assets/NatureManufacture Assets/ | NatureManufacture (folder name) | "Asset is governed by the Asset Store EULA" (Third-Party Notices.txt line 1, Third-Party Notices 2.txt line 1); parts under the Unity Companion License (NMWind.cginc/.hlsl, NM_Wind.cs, ShaderWindSettings.cs, 3DNoise.psd, GustNoise.psd, the Lightweight grass shaders) | Yes per that record (Asset Store EULA; the Unity Companion parts only inside Unity) | none recorded |
| Stylized Medieval Vikings Pack | Assets/Stylized_Medieval_Vikings_Pack/ | UNKNOWN (README.txt names no author) | UNKNOWN. README.txt says "Thank you for purchasing" and "leave a review on the Asset Store"; no licence text | UNKNOWN on record | none recorded |
| Primitive Characters Pack (Primitive_01..05, Scenes) | Assets/Primitive_Characters_Pack/ | UNKNOWN | UNKNOWN. No read-me, no licence file at all | UNKNOWN | UNKNOWN |
| HDRPDefaultResources, Settings, Scenes, Unity.VisualScripting.Generated, InputSystem_Actions | Assets/ | Unity (made by the editor) | Unity's own | Yes | none |

None of the four packs is named in Assets/Kapman/ThirdParty/CREDITS.md.

## 2. Third-party things inside Assets/Kapman

| What | Where | Licence as recorded | May ship | Credit owed |
|---|---|---|---|---|
| Poly Haven textures and models, 36 folders | Assets/Kapman/ThirdParty/PolyHaven/ | CC0 1.0 (ThirdParty/CREDITS.md lines 7-10) | Yes | none required |
| ...of which 16 folders are NOT in the CREDITS.md list: aerial_mud_1, coast_sand_01, damp_sand, farm_soil, forest_leaves_02, gray_rocks, marble_cliff_01, mud_forest, old_planks_02, raw_plank_wall, riet_01, rough_linen, sand_01, shrub_02, stone_01, wood_trunk_wall | same folder | Recorded only by the folder they sit in (PolyHaven = CC0) | Yes if they are what the folder says | none; add to the list |
| ambientCG textures, 7 folders: Grass001, Grass002, Grass004, Grass008, Moss002, Wicker002, Wicker003 | Assets/Kapman/ThirdParty/AmbientCG/ | NOT in ThirdParty/CREDITS.md. Design notes say ambientCG is CC0 1.0 | Yes (CC0), but the record is missing from CREDITS.md | none required; suggested line in the Design notes: "Created using [asset] from ambientCG.com, licensed under CC0 1.0." |
| "Gislinge Viking Boat" model | Assets/Kapman/Models/Ships/Gislinge/ (CREDITS.md there) | CC BY 4.0 | Yes, with the credit | "Gislinge Viking Boat" by Opus Poly, licensed under CC BY 4.0, after the Viking Ship Museum's Gislinge Boat Open Source Project. (Changes: materials rebuilt for HDRP.) |
| Star cube Stars_930.exr | Assets/Kapman/Sky/ | Baked from the Yale Bright Star Catalogue, "public domain" (ThirdParty/CREDITS.md line 19) | Yes | none required |
| Kuwahara filter (KapmanLook.shader, mode 2) | Assets/Kapman/Shaders/ | MIT, Copyright (c) 2022 Garrett Gunnell; full text in ThirdParty/CREDITS.md | Yes; the MIT notice must go with the game | the MIT notice, "Garrett Gunnell, Post-Processing" |
| BlueNoise128.png | Assets/Kapman/Look/ | UNKNOWN (no record: home-made or a free blue-noise texture) | UNKNOWN | UNKNOWN |
| WaterLine_Gradient.png, Utrecht_boat_hull_Lid, Planking_Lid | Assets/Kapman/Water/ | no record; look project-made | likely own | none |
| HUD frame art | made by Bake/art/artframe.py (placeholders drawn by rule; the user draws the real ones) | own | Yes | none |

## 3. Fonts (only three font files in Assets and Design)

| Font | Where | Licence file | Licence | May ship | Credit owed |
|---|---|---|---|---|---|
| Gentium Book Plus, Regular + Bold | Assets/Kapman/Game/UI/Fonts/GentiumBookPlus-*.ttf | GentiumBookPlus-OFL.txt | SIL Open Font License 1.1. "Copyright (c) 2003-2022 SIL International, with Reserved Font Names Gentium and SIL" | Yes. The OFL text must ship with the font; the font may not be sold by itself; a changed font may not keep the name | the copyright line + the OFL text |
| Noto Sans Runic Regular | same folder, NotoSansRunic-Regular.ttf | NotoSansRunic-OFL.txt | SIL OFL 1.1. "Copyright 2022 The Noto Project Authors (https://github.com/notofonts/runic)" | Yes, same terms | the copyright line + the OFL text |

No .otf anywhere. No font files under Design. The fonts are not named in ThirdParty/CREDITS.md (the OFL files are the record).

## 4. Map and terrain data (the bake scripts name these)

Main list: Bake/world/make_map_pyramid.py lines 112-125 (CREDITS, written into the pyramid's index).
Also Bake/world/make_tiles.py lines 989-1003 (SOURCES), Bake/world/README.md lines 165-175,
Bake/world/make_map_art.py lines 693-697, Bake/levels/make_place.py lines 287-290 and 355-359.

| Source | Licence as recorded | May ship | Credit line as recorded |
|---|---|---|---|
| Copernicus DEM GLO-30 and GLO-90 | No licence named. The credit line itself ends "all rights reserved" (make_map_pyramid.py:113) | UNKNOWN on record: the wording does not say commercial use is allowed. The worst map item | "Copernicus DEM GLO-30 and GLO-90 (c) DLR e.V. 2010-2014 and (c) Airbus Defence and Space GmbH 2014-2018, provided under COPERNICUS by the European Union and ESA; all rights reserved" |
| GEBCO 2025 Grid | UNKNOWN (no licence word at make_map_pyramid.py:116) | UNKNOWN on record | "GEBCO Compilation Group (2025) GEBCO 2025 Grid" |
| HydroRIVERS v1.0 | Three wordings: "HydroSHEDS licence" (pyramid:120, map_art:695); "CC BY 4.0" (make_tiles.py:995); README.md:174 quotes "free for non-commercial and commercial use" from the TechDoc in the zip | Yes per the README; make the three lines agree | "Lehner, B., Grill G. (2013): Global river hydrography and network routing: baseline data and new approaches to study the world's large river systems. Hydrological Processes, 27(15): 2171-2186. Data available at www.hydrosheds.org" |
| Natural Earth 10 m | public domain (make_tiles.py:990) | Yes | "Made with Natural Earth. Free vector and raster map data @ naturalearthdata.com." |
| Paleogeografische kaarten van Nederland, 800 n.Chr. (Vos & De Vries) | CC BY 4.0 (make_tiles.py:992; README.md:173 cites data.overheid.nl dataset 62601, recorded in Research/region_textures.md) | Yes with credit | "Paleogeografische kaarten van Nederland, P.C. Vos & S. de Vries (Deltares/TNO), Rijksdienst voor het Cultureel Erfgoed, CC BY 4.0" |
| EMODnet Digital Bathymetry DTM 2024 | CC BY 4.0 | Yes with credit | "EMODnet Bathymetry Consortium (2024): EMODnet Digital Bathymetry (DTM 2024), emodnet.ec.europa.eu, CC BY 4.0" |
| DeltaDTM v1.1 | CC BY 4.0 (pyramid:114) | Yes with credit | "DeltaDTM v1.1 (Pronk et al. 2024), Deltares, CC BY 4.0" |
| AHN4 lidar (Netherlands) | CC0 1.0 (pyramid:118, make_place.py:288) | Yes | "AHN4 (Rijkswaterstaat and the water boards), CC0 1.0" |
| EA LIDAR Composite DTM 2022 (Thames) | Open Government Licence v3.0 (pyramid:119, make_place.py:356) | Yes with the line | "Contains Environment Agency information (c) Environment Agency and/or database right 2022. Licensed under the Open Government Licence v3.0." |
| GRWL river centre lines | CC BY 4.0 (pyramid:121) | Yes with credit | "Allen, G. H. & Pavelsky, T. M. (2018), Global extent of rivers and streams (GRWL), Science 361, CC BY 4.0" |
| Cohen et al. 2012, Rhine-Meuse delta channel belts | CC BY 4.0 (make_place.py:290, make_dorestad.py:451). NOT in the pyramid's CREDITS list | Yes with credit | "Cohen et al. 2012, Rhine-Meuse delta channel belts (CC BY 4.0)" (short; the full citation is not on record) |
| KK10 land cover (Kaplan & Krumhardt 2011) | CC BY 3.0 (pyramid:122) | Yes with credit | "Kaplan, J.O. and Krumhardt, K.M. (2011): The KK10 Anthropogenic Land Cover Change scenario, PANGAEA, CC BY 3.0" |
| ESA WorldCover 2021 | CC BY 4.0 (pyramid:123) | Yes with credit | "(c) ESA WorldCover project 2021 / Contains modified Copernicus Sentinel data (2021) processed by ESA WorldCover consortium, CC BY 4.0" |
| Wikidata (gazetteer: places, names, dates) | CC0 1.0 (Bake/gazetteer/make_gazetteer.py:16 and :76) | Yes | "Places, names, coordinates and dates: Wikidata (wikidata.org), CC0 1.0 Universal public domain dedication." |
| KNMI station series 330, 235, 310, 260 (weather normals 1991-2020) | "KNMI open data, free reuse with attribution" (Bake/weather/make_normals.py:24 and :389) | Yes with credit | "Source: KNMI" |
| Sea temperatures in the weather normals | not from KNMI; "marked as a guess" (make_normals.py:25) | own guess | none |

The gazetteer script states that no OpenStreetMap, Wikipedia text, Pleiades or OpenDomesday data was used (make_gazetteer.py:1709).

## 5. The old texts (Assets/Kapman/Game/Data/texts_793.json)

267 texts. Every one has a non-empty "credit" field.
The file's own rule: the translator died in 1955 or before, and the translation was published before 1931.

- Translator NAMED, with a real death year before 1956 written in: **214**.
  (Bellows 97, J. L. Hall 43, Vigfusson + York Powell 36, Thorpe 31, Bosworth 2, and one each: Ellwood, Hickey, Giles, Kuno Meyer, Joseph Anderson.)
- NOT so: **53**, in five kinds:
  - 3 with a named translator but the death year written as 0 (not recorded): seafarer, Lola La Motte Iddings, 1902 (line 4741); deor, Francis A. Blackburn, 1902 (line 4784); maldon_991, H. W. Lumsden, 1902 (line 4806). Published before 1931, but the death years are UNKNOWN in the file.
  - 12 where the translator field holds a code, not a name, and there is no death year: "cpb1883" x7 (ids verse:v01_hofudlausn... onward, about line 3271), "anderson1880" x3, "laing1844" x2. The works are old (1883, 1880, 1844), so very likely fine, but the name and the death year are not written in the entry.
  - 3 that are the game's own drafts (translated 2026, translatorDied 0): aelfric_merchant (line 5368), saxon_vow (5389), alcuin_793 (5410). Own work, shippable. alcuin_793 has check=true (the original quoted from memory, to be checked).
  - 34 Latin riddles with no translation and no translator: exeter_25_onion, aldhelm x12, symphosius x12, tatwine x5, eusebius x4. Original only; the credit names the editor, for example "Aldhelm, Enigmata, ed. Rudolf Ehwald (1919). Public domain."
  - 1 more with translator null and translation null: adam (line 3200, "Adam of Bremen's sailing times", Latin from the Hannover 1876 edition).
- Editors of the originals: 52 entries carry an "editor" field. The editors' death years are NOT recorded (an edited text can carry a right of its own in some countries). UNKNOWN.
- May ship: yes by the file's rule for the 214; the 3 with year 0 and the 12 coded ones need the name and the year written in; the editions are the open point.
- Credit owed: none by law for public domain; each entry's "credit" line is ready to print.
- On the count: 7 entries carry a death year of 0 (the 3 above, the 3 own drafts, adam); the 214 leaves all of them out.

## 6. Sounds

- 65 .mp3 files in Assets/Kapman/Audio/Sounds/. No .wav or .ogg anywhere in Assets.
- Record: Assets/Kapman/Audio/CREDITS_sounds.md (125 lines, 103 credit lines). All "public domain (CC0 1.0) from freesound.org", each with title, author, URL and file name.
- All 65 files in the folder are named in CREDITS_sounds.md (checked by file name). The other 38 lines are for files kept outside the project (G:\kapman_unity\data\sounds_cc0).
- Small error in the record: line 3 says the files are in "Audio/Resources/Sounds"; they are in Audio/Sounds.
- May ship: yes (CC0). Credit owed: none required; the file's lines are ready, in the form: "<title>" by <author>, CC0, freesound.org.
- Not checked: that each freesound page really says CC0 (no web).

## 7. Unity packages (Packages/manifest.json)

All 17 named packages are com.unity.* (Unity's own), plus Unity's built-in modules.
No third-party package, no git entry, no scoped registry.
Unity's own, but worth a look before release:
- com.unity.ai.assistant 2.20.0-pre.1 and com.unity.pipeline 0.8.0-exp.1 are pre-release or experimental;
- com.unity.ai.inference, com.unity.recorder, com.unity.collab-proxy, the two IDE packages and test-framework are editor or tool packages, not needed in the shipped game.
No credit line recorded or needed beyond Unity's usual terms.

## 8. The unknowns, worst first

1. **Copernicus DEM GLO-30/GLO-90**: the project's own credit line says "all rights reserved" and names no licence. It is the base land height of the whole map. Record the licence text that allows commercial use, or the map stands on an unproven source.
2. **Primitive_Characters_Pack**: no publisher, no licence, no read-me. Nothing at all on record.
3. **Stylized_Medieval_Vikings_Pack**: bought on the Asset Store by its read-me, but no author and no licence on record.
4. **ANGRY MESH Stylized Pack**: publisher known from the folder name only; no licence or purchase on record.
5. **GEBCO 2025 Grid**: a credit line only, no licence named.
6. **HydroRIVERS**: three different licence wordings in three files (HydroSHEDS licence / CC BY 4.0 / "free for non-commercial and commercial use"). Pick the true one.
7. **Editors of the old texts' originals** (52 entries with an editor; Ehwald 1919 and others): death years not recorded.
8. **15 texts without a translator's death year**: 3 named with year 0 (Iddings, Blackburn, Lumsden: seafarer, deor, maldon_991) and 12 with a code for a name (cpb1883, anderson1880, laing1844).
9. **ambientCG, 7 texture sets**: in the project, missing from ThirdParty/CREDITS.md.
10. **16 Poly Haven folders** missing from the list in ThirdParty/CREDITS.md.
11. **BlueNoise128.png** (Assets/Kapman/Look): no source on record.
12. **Cohen et al. 2012**: used for Dorestad, missing from the pyramid's CREDITS list; only a short citation on record.
13. **ThirdParty/CREDITS.md is not complete**: it says "All of these may ship in a paid game" but leaves out the four packs, the fonts, ambientCG and all the map data.
14. **Paleogeographic maps (Vos)**: CC BY 4.0 is recorded, but the proof is a note in Research/region_textures.md, not a licence file with the data.
15. **alcuin_793**: the original is "quoted from memory, to be checked" (check=true).
16. CREDITS_sounds.md names the wrong folder (Audio/Resources/Sounds).

## Not done

- Nothing checked on the web: every licence above is what the project says, not what the source says today.
- Did not open every pack sub-folder for a hidden licence file beyond a name search (licen*, readme*, credit*, .txt, .pdf).
- Did not look at G:\kapman_unity\data (the raw data and its own licence files) or read Research/*.md.
- Did not check the pictures under Design (mock-ups) for borrowed images.
- The count of 34 untranslated riddles is by id prefix; they were not read one by one.

## A note from the lead, 7 Oct 2026, on the worst of the unknowns (from memory, to be checked at the source)

- **Copernicus DEM (GLO-30, GLO-90).** The line that ends "all rights reserved" is not a bar on use: it is the credit
  notice the Copernicus DEM licence itself asks to be shown ("(c) DLR e.V. 2010-2014 and (c) Airbus Defence and Space
  GmbH 2014-2018 provided under COPERNICUS by the European Union and ESA; all rights reserved"). As I recall that
  licence, the public GLO-30 and GLO-90 data may be used, changed and shipped in a product that is sold, free of charge,
  so long as that notice is shown. What is missing in the project is the licence text itself and a note of the day it
  was read: fetch it from the Copernicus data space and put it beside the data.
- **GEBCO grid.** As I recall, the GEBCO grids are placed in the public domain and ask only for the credit line
  ("GEBCO Compilation Group (year) GEBCO year Grid"). Record the terms page.
- **HydroRIVERS (HydroSHEDS).** As I recall, the HydroSHEDS licence allows commercial use with attribution. The project
  quotes it three different ways: keep one, taken from the licence document itself.
- **The asset packs.** Things bought on the Unity Asset Store are covered by the store's own licence, which allows
  shipping them inside a game. What the project lacks is the proof of purchase for each pack in one place (the order
  page saved as a file) and the publisher's name for the credits.
