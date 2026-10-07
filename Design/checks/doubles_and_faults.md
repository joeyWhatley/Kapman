# Doubles and faults: places/*.json and spine.json

Checked 2026-10-07 by script, read only. Nothing invented: every line is from the data.

Counted: 50 place files, 477 people (117 in the twelve ports), 610 sites (149 in the twelve), 267 stories, 96 spine ids.

The twelve ports: dorestad, traiectum, walichrum, lundenwic, witla, medemelacha, doccinga, daventre, quentovic, sarre, fordwic, ripa.

## 1. Ids that are not unique (places: sites, people, stories, works, unlocks)

Twelve ports: **1**. All fifty ports: **2**.

- `witla.work.beacon` x2: witla/works, witla/unlocks

Other ports (1), first 25:

- `birka.second_sister` x2: birka/stories, birka/unlocks

Spine.json ids used more than once: **0**


Ids that are in both spine.json and a places file: **0** (may be meant as a reference, not a double)


## 2a. Same name AND (same port or same birth year) under two ids - likely the same person

Twelve ports: **1**. All fifty ports: **3**.

- **Dudda**: `gyrwe.fisherman` (gyrwe, fisherman, born 760, here 793-818); `sarre.ferryman` (sarre, ferryman, born 760, here 793-825)

Other ports (2), first 25:

- **Baldwin**: `ganda.baldwin` (ganda, count, born 865, here 883-918); `ganda.baldwin_v` (ganda, count, born 1012, here 1035-1066)
- **Liudger**: `diusburg.liudger` (diusburg, priest, born 742, here 799-809); `emuthon.liudger` (emuthon, priest, born 742, here 794-809)

## 2b. Same name in two ports, different birth year - a shared name or a double; to be judged by hand

Twelve ports: **14**. All fifty ports: **33**.

- **Alli**: `hleidr.alli` (hleidr, steward, born 935, here 975-1000); `ripa.carter` (ripa, carter, born 760, here 793-820)
- **Bernrad**: `wijnaldum.bernrad` (wijnaldum, smith, born 760, here 793-822); `witla.bernrad` (witla, priest, born 756, here 793-822)
- **Ceolmund**: `gyrwe.schoolmaster` (gyrwe, schoolmaster, born 722, here 793-801); `lundenwic.ceolmund` (lundenwic, host, born 805, here 835-857)
- **Eadhun**: `fordwic.scribe` (fordwic, scribe, born 765, here 793-831); `hamwic.eadhun` (hamwic, pilot, born 755, here 793-815)
- **Gerbrand**: `doccinga.gerbrand` (doccinga, moneyer, born 1005, here 1038-1057); `dorestad.gerbrand` (dorestad, shipwright, born 760, here 793-833); `wijnaldum.gerbrand` (wijnaldum, craftsman, born 758, here 793-815); `witla.gerbrand` (witla, pilot, born 785, here 800-836)
- **Gunnulf**: `eoforwic.shipwright` (eoforwic, shipwright, born 1000, here 1025-1066); `medemelacha.gunnulf` (medemelacha, tollman, born 818, here 850-873)
- **Hroald**: `hleidr.hroald` (hleidr, setter, born 725, here 793-808); `ripa.hroald` (ripa, host, born 748, here 793-818)
- **Hunfrith**: `lindisfarena.hunfrith` (lindisfarena, cellarer, born 752, here 793-815); `lundenwic.hunfrith` (lundenwic, craftsman, born 755, here 793-818)
- **Leofstan**: `hamwic.leofstan` (hamwic, merchant, born 757, here 793-820); `lindisfarena.leofstan` (lindisfarena, priest, born 1005, here 1035-1066); `quentovic.leofstan` (quentovic, skipper, born 758, here 793-812)
- **Radward**: `medemelacha.radward` (medemelacha, tollman, born 748, here 793-812); `witla.radward` (witla, skipper, born 758, here 793-819)
- **Soti**: `hleidr.soti` (hleidr, goldsmith, born 860, here 885-915); `ripa.goldsmith` (ripa, goldsmith, born 752, here 793-822)
- **Thiadlef**: `daventre.thiadlef` (daventre, merchant, born 832, here 863-895); `dorestad.thiadlef` (dorestad, skipper, born 787, here 825-852)
- **Toki**: `gipeswic.toki` (gipeswic, merchant, born 852, here 880-912); `hleidr.toki` (hleidr, herdsman, born 765, here 793-825); `reric.toki` (reric, shipwright, born 738, here 793-806); `ripa.bead_maker` (ripa, craftsman, born 745, here 793-812)
- **Tola**: `hleidr.tola` (hleidr, alewife, born 748, here 793-822); `ripa.tola` (ripa, alewife, born 755, here 793-826)

Other ports (19), first 25:

- **Ceolhelm**: `hamwic.ceolhelm` (hamwic, priest, born 745, here 793-812); `lindisfarena.ceolhelm` (lindisfarena, scribe, born 760, here 793-828)
- **Eadhelm**: `legacaestir.eadhelm` (legacaestir, priest, born 745, here 793-814); `namnetis.eadhelm` (namnetis, monk, born 765, here 796-826)
- **Eadstan**: `gipeswic.eadstan` (gipeswic, reeve, born 748, here 793-812); `lindisfarena.eadstan` (lindisfarena, pilot, born 745, here 793-812)
- **Einold**: `ganda.einold` (ganda, scribe, born 752, here 793-818); `rotomagus.einold` (rotomagus, clerk, born 816, here 841-879)
- **Ermentrud**: `andoverpis.ermentrud` (andoverpis, alewife, born 752, here 793-826); `parisius.ermentrud` (parisius, host, born 745, here 793-822)
- **Godwine**: `dofras.godwine` (dofras, count, born 1001, here 1042-1053); `sliesthorp.moneyer` (sliesthorp, moneyer, born 955, here 985-1005)
- **Gunnhild**: `ogvaldsnes.gunnhild` (ogvaldsnes, farm_wife, born 766, here 793-828); `sliesthorp.gunnhild` (sliesthorp, alewife, born 752, here 793-822)
- **Hallmund**: `legacaestir.moneyer` (legacaestir, moneyer, born 955, here 990-1016); `skiringssal.hallmund` (skiringssal, lawman, born 855, here 885-936)
- **Hallstein**: `aldeigjuborg.skald` (aldeigjuborg, host, born 845, here 881-906); `eoforwic.skald` (eoforwic, skald, born 875, here 903-940)
- **Hereberht**: `dofras.reeve` (dofras, reeve, born 748, here 793-798); `gipeswic.hereberht` (gipeswic, priest, born 750, here 793-816)
- **Hildulf**: `andoverpis.hildulf` (andoverpis, priest, born 755, here 793-818); `hamwic.hildulf` (hamwic, factor, born 760, here 793-805)
- **Hunwine**: `dommoc.fisherman` (dommoc, fisherman, born 852, here 880-915); `streonaeshalch.cellarer` (streonaeshalch, cellarer, born 748, here 793-816)
- **Ketil**: `ganda.ketil` (ganda, raider, born 840, here 879-881); `streonaeshalch.ketil` (streonaeshalch, farmer, born 850, here 877-914); `truso.ketil` (truso, host, born 752, here 793-824)
- **Robert**: `parisius.robert` (parisius, count, born 866, here 888-922); `rotomagus.robert` (rotomagus, bishop, born 965, here 989-1037)
- **Skardi**: `hleidr.skardi` (hleidr, lords_man, born 758, here 793-820); `reric.skardi` (reric, tollman, born 752, here 793-808)
- **Vigmund**: `aldeigjuborg.fur_buyer` (aldeigjuborg, chief, born 748, here 793-815); `birka.vigmund` (birka, warrior, born 905, here 930-970)
- **Wido**: `colonia.wido` (colonia, steward, born 750, here 793-812); `namnetis.wido` (namnetis, count, born 0, here 799-818)
- **Wulfheard**: `eoforwic.wulfheard` (eoforwic, host, born 748, here 793-818); `hamwic.wulfheard` (hamwic, commander, born 0, here 825-840)
- **Óengus**: `duiblinn.oengus` (duiblinn, monk, born 750, here 793-824); `hi.oengus` (hi, guest_master, born 748, here 793-806)

Spine people whose name is also a person in a places file: **44**

- spine `cast.crew.ravn` **Ravn** (/cast, born None) ~ places `dorestad.ravn`
- spine `cast.crew.aldger` **Aldger** (/cast, born None) ~ places `doccinga.aldger`
- spine `cast.crew.brunger` **Brunger** (/cast, born None) ~ places `daventre.brunger`
- spine `cast.crew.beornwine` **Beornwine** (/cast, born None) ~ places `gyrwe.beornwine`
- spine `cast.folcbert` **Folcbert** (/cast, born None) ~ places `dorestad.folcbert`
- spine `cast.hartwin` **Hartwin** (/cast, born None) ~ places `dorestad.hartwin`
- spine `cast.thiadbod` **Thiadbod** (/cast, born None) ~ places `dorestad.thiadbod`
- spine `cast.thiadburg` **Thiadburg** (/cast, born None) ~ places `dorestad.thiadburg`
- spine `cast.sigimar` **Sigimar** (/cast, born None) ~ places `dorestad.sigimar`
- spine `cast.gerlef` **Gerlef** (/cast, born None) ~ places `witla.gerlef`
- spine `cast.notburg` **Notburg** (/cast, born None) ~ places `emuthon.notburg`
- spine `cast.sigewine` **Sigewine** (/cast, born None) ~ places `eoforwic.sigewine`
- spine `cast.wighelm` **Wighelm** (/cast, born None) ~ places `lundenwic.wighelm`
- spine `cast.ecgwulf` **Ecgwulf** (/cast, born None) ~ places `eoforwic.ecgwulf`
- spine `cast.tidwine` **Tidwine** (/cast, born None) ~ places `lindisfarena.tidwine`
- spine `cast.asulf` **Asulf** (/cast, born None) ~ places `sliesthorp.asulf`
- spine `cast.thorleif` **Thorleif** (/cast, born None) ~ places `ogvaldsnes.thorleif`
- spine `cast.dragomir` **Dragomir** (/cast, born None) ~ places `reric.dragomir`
- spine `cast.holmfast` **Holmfast** (/cast, born None) ~ places `birka.holmfast`
- spine `cast.eystein` **Eystein** (/cast, born None) ~ places `dorestad.eystein`
- spine `cast.gervold` **Gervold** (/cast, born None) ~ places `quentovic.gervold`
- spine `cast.alcuin` **Alcuin** (/cast, born None) ~ places `eoforwic.alcuin`
- spine `cast.higbald` **Higbald** (/cast, born None) ~ places `lindisfarena.higbald`
- spine `cast.liudger` **Liudger** (/cast, born None) ~ places `diusburg.liudger`, `emuthon.liudger`
- spine `cast.bernlef` **Bernlef** (/cast, born None) ~ places `emuthon.bernlef`
- spine `cast.landric` **Landric** (/cast, born None) ~ places `emuthon.landric`
- spine `cast.hamacar` **Hamacar** (/cast, born None) ~ places `traiectum.hamacar`
- spine `cast.eanbald` **Eanbald** (/cast, born None) ~ places `eoforwic.eanbald`
- spine `cast.aethelheard` **Æthelheard** (/cast, born None) ~ places `fordwic.aethelheard`
- spine `cast.godfred` **Godfred** (/cast, born None) ~ places `sliesthorp.godfred`
- spine `cast.thrasco` **Thrasco** (/cast, born None) ~ places `reric.thrasco`
- spine `cast.bresal` **Bresal** (/cast, born None) ~ places `hi.bresal`
- spine `cast.cellach` **Cellach** (/cast, born None) ~ places `hi.cellach`
- spine `cast.anskar` **Anskar** (/cast, born None) ~ places `sliesthorp.anskar`
- spine `cast.herigar` **Herigar** (/cast, born None) ~ places `birka.herigar`
- spine `cast.frideburg` **Frideburg** (/cast, born None) ~ places `birka.frideburg`
- spine `cast.catla` **Catla** (/cast, born None) ~ places `birka.catla`
- spine `cast.rorik` **Rorik** (/cast, born None) ~ places `dorestad.rorik`
- spine `cast.horik` **Horik** (/cast, born None) ~ places `sliesthorp.horik`
- spine `cast.ohthere` **Ohthere** (/cast, born None) ~ places `skiringssal.ohthere`
- spine `cast.wulfstan` **Wulfstan** (/cast, born None) ~ places `sliesthorp.wulfstan`
- spine `cast.rollo` **Rollo** (/cast, born None) ~ places `rotomagus.rollo`
- spine `cast.cnut` **Cnut** (/cast, born None) ~ places `lundenwic.cnut`
- spine `cast.thorkell` **Thorkell** (/cast, born None) ~ places `lundenwic.thorkell`

## 3. People whose years cannot be

Checked: from before born; to or died more than ninety years after born; died before born; to after died; to before from; no birth year.

Twelve ports: **9**. All fifty ports: **38**.

- `doccinga.adelsuit` Adelsuit (doccinga): still here in 836 at age 92
- `doccinga.adelsuit` Adelsuit (doccinga): dies in 836 at age 92
- `quentovic.grippo` Grippo (quentovic): no birth year (born=None)
- `sarre.cwenthryth` Cwenthryth (sarre): no birth year (born=None)
- `sarre.ealhhere` Ealhhere (sarre): no birth year (born=None)
- `traiectum.hunger` Hunger (traiectum): no birth year (born=None)
- `walichrum.eggihard` Eggihard (walichrum): no birth year (born=None)
- `walichrum.hemming` Hemming (walichrum): no birth year (born=None)
- `walichrum.harald` Harald (walichrum): no birth year (born=None)

Other ports (29), first 25:

- `aldeigjuborg.ragnvald` Ragnvald Ulfsson (aldeigjuborg): no birth year (born=None)
- `bebbanburg.waltheof` Waltheof (bebbanburg): no birth year (born=None)
- `bebbanburg.uhtred` Uhtred (bebbanburg): no birth year (born=None)
- `colonia.hildebold` Hildebold (colonia): no birth year (born=None)
- `colonia.hadebald` Hadebald (colonia): no birth year (born=None)
- `diusburg.henry` Henry (diusburg): no birth year (born=None)
- `hamwic.wulfheard` Wulfheard (hamwic): no birth year (born=None)
- `herio.hilbod` Hilbod (herio): no birth year (born=None)
- `herio.ermentarius` Ermentarius (herio): no birth year (born=None)
- `legacaestir.ingimund` Hingamund (legacaestir): no birth year (born=None)
- `legacaestir.irfara` Irfara (legacaestir): no birth year (born=None)
- `mogontia.riculf` Riculf (mogontia): no birth year (born=None)
- `mogontia.liutefred` Liutefred (mogontia): no birth year (born=None)
- `mogontia.otgar` Otgar (mogontia): no birth year (born=None)
- `mogontia.liutbert` Liutbert (mogontia): no birth year (born=None)
- `namnetis.odilard` Odilard (namnetis): no birth year (born=None)
- `namnetis.wido` Wido (namnetis): no birth year (born=None)
- `namnetis.gunhard` Gunhard (namnetis): no birth year (born=None)
- `namnetis.alan` Alan Twisted-beard (namnetis): no birth year (born=None)
- `namnetis.rainald` Rainald (namnetis): no birth year (born=None)
- `noviomagus.eberhard` Eberhard (noviomagus): no birth year (born=None)
- `ogvaldsnes.thorir_sel` Thorir Sel (ogvaldsnes): no birth year (born=None)
- `parisius.aimoin` Aimoin (parisius): no birth year (born=None)
- `paviken.vivil` Vivil (paviken): no birth year (born=None)
- `paviken.ailikn` Ailikn (paviken): no birth year (born=None)

Spine.json people with year faults (missing birth year not counted): **0**


## 4. People with a role npc_roles_793.json does not know

Roles the file knows (70): abbess, abbot, alewife, beggar, bishop, captive, carter, cellarer, chief, child, clerk, commander, count, craftsman, crewman, envoy, factor, farm_wife, farmer, ferryman, fisherman, fowler, goldsmith, guest_master, harbour_master, herdsman, host, hostage, hunter, interpreter, king, kings_man, lawman, leech, lookout, lords_man, master, mate, merchant, moneyer, monk, nun, owner, pilgrim, pilot, porter, priest, raider, raider_chief, reeve, rune_carver, sailor, salter, schoolmaster, scribe, setter, shipwright, singer, skald, skipper, slave_dealer, smith, steersman, steward, survivor, tollman, warrior, weaver, widow, wise_woman

Twelve ports: **0**. All fifty ports: **0**.

- none in the twelve ports

Unknown roles by count, all ports: 


Spine.json people with an unknown role: **0**


## 5. Sites with no kind, or a kind used only once

Twelve ports: **0**. All fifty ports: **0**.

- none in the twelve ports

Kinds used only once within the twelve ports but more often elsewhere: **3**

- `daventre.smithy` (daventre): kind `smithy` once in the twelve (x16 in all ports)
- `doccinga.clergy_house` (doccinga): kind `thing` once in the twelve (x8 in all ports)
- `lundenwic.bridge` (lundenwic): kind `bridge` once in the twelve (x3 in all ports)

Site kinds, all ports: market x49, workshop x44, hall x38, church x38, beach x34, shipyard x31, tavern x30, graveyard x29, quay x27, field x26, lodging x26, mint x23, shrine x21, storehouse x21, mound x18, toll x17, abbey x17, smithy x16, fishery x15, wall x15, ferry x14, minster x14, gate x11, well x8, thing x8, grove x6, ford x6, saltern x3, bridge x3, quarry x2


## 6. Sites whose "do" names a person or story that does not exist

Method: every dotted id inside a site's do list that starts with a port name is looked up among all place ids.

Twelve ports: **0**. All fifty ports: **0**.

- none in the twelve ports

## 6b. The same check by prefix (do entries are strings like act:..., card:..., talk:...)

Prefixes in all site do lists: act x3299, card x889, person x606, (no prefix) x68. Entries that are not strings: 0.

Every entry that is not act: or card: was looked up (person ids for talk/person, story ids for story, any place id for the rest). act: and card: were not checked here.

Twelve ports: **0**. All fifty ports: **0**.

- none in the twelve ports

## 7. Spine cast against the places files (the 44 name matches in section 2 are these links, not doubles)

Spine cast: 44. Cast whose place_id is missing or points at nobody: **0**.

- none

Fields that differ between a cast entry and the person it points at (name, role, people, faith, from, to, historical): **7**

- `cast.dragomir` / `reric.dragomir`: to is 820 in spine, 808 in places
- `cast.liudger` / `emuthon.liudger`: from is 793 in spine, 794 in places
- `cast.frideburg` / `birka.frideburg`: to is 853 in spine, 852 in places
- `cast.rorik` / `dorestad.rorik`: from is 841 in spine, 850 in places
- `cast.wulfstan` / `sliesthorp.wulfstan`: from is 870 in spine, 880 in places
- `cast.wulfstan` / `sliesthorp.wulfstan`: to is 900 in spine, 899 in places
- `cast.cnut` / `lundenwic.cnut`: from is 1014 in spine, 1016 in places
