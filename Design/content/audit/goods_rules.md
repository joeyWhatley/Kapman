# Audit: goods_793.json and trade_rules.json against the sources

Written 6 Oct 2026. The two tables were read whole and **not changed**. This page judges every entry.

**The verdicts.** `right` = the facts in the row stand. `wrong` = a fact in the row is against the sources; the
correction and its source follow. `doubtful` = no source carries it, or the scholars disagree, or it is true only
for part of 793 to 1066; the reason follows.

**The marks on a source.** `[W]` = opened and checked today on Wikipedia (the page is named). `[eco: file §n]` = the
project's own research note in `Design/content/economy/`, written today from sources it lists; I read its findings
and agree unless I say otherwise. `[mem]` = from my memory of the named source, not opened today. `(?)` = I could
not check it; treat it as a guess.

**What was not judged.** `value`, `bulk`, the `made_factor` and `wanted_factor` numbers and the `market` block are
game tuning (the files say "a starting guess to tune"). I judge a value only where a period price exists to hold it
against. Almost no good of the time has a known price; the few that do are named.

**The one fault that runs through the whole goods table.** A good has one `from`, one `to`, one `made` and one
`wanted` for 273 years. Many rows are right for one century and wrong for the next (Iceland before 874, Greenland
before 985, Shetland before 850, the Harz before 968, Flanders before 1000). Where a row is marked doubtful "by
years", that is the reason. Change 3 at the foot of the page mends it.

Count: 140 goods, 21 sources, 4 coins, 5 zones, 5 laws, the market block, the start block, the two factor tables.
Some entries of the head and the rules share a row (the eight sound books; the tuning numbers).

---

## 1. goods_793.json: the goods

### Food, drink and provisions

| id | Verdict | Finding |
|---|---|---|
| wheat | doubtful | The 794 price (4 d the modius) is right: Synod of Frankfurt 794, c. 4 [mem; the same figures in eco: silver_coin §8]. Value 6 for a modius and a half is right. Doubtful: `wanted` norway_west 3, svealand 2, ladoga 2. The north wanted barley, malt and rye; wheat was a lord's luxury there. Set norway_west 1 [eco: food §10.1 no. 8]. `dear` [3,4,5,6] is early: the hungry gap is May to July. Set `dear` [5,6,7] [eco: food §10.1 no. 5]. The 794 figures are a ceiling for a dear year, not a market price. |
| rye | right | 3 d the modius in 794 (Frankfurt c. 4). Saxony and the sandy lands as makers is sound. `dear` months as wheat. |
| barley | wrong | The note gives 2 d the modius; a sack of a modius and a half is then 3 d, and `value` is 4. Set `value` 3. Source: Frankfurt 794 c. 4 [eco: food §10.1 no. 6]. |
| oats | right | 1 d the modius in 794; 2 for the sack is near enough (1.5). |
| malt | doubtful | Plausible, no source in the row. The north's want of malt is sound (it should stand above wheat there). |
| flour | doubtful | Flour was ground where it was eaten; no text shows it shipped. Harmless as a small good. Source lacking for the trade itself; Verhulst covers only the mills. |
| ships_bread | wrong | "About a day's food for 10 men a sack" of 20 kg is 2 kg a man. The usual ration is about 1 kg: say "20 man-days a sack" [eco: food §10.1 no. 10]. The loaf price (12 wheat loaves of 2 pounds a penny) is right: Frankfurt 794 c. 4. "Twice-baked" bread in the north of 793 has no source I know (?); "hard-baked" is safer. |
| rhenish_wine | doubtful | The trade is right and central: Rhine wine through Dorestad [W: Dorestad: "wine from the vineyards south of Mainz", old wine barrels dug up there]. Doubtful: (1) the "cask of about 100 litres": the casks found in the wells of Dorestad are five to eight times that; call it a keg or small cask [eco: wine_salt §16 no. 1]; (2) `keeps` 540 is long for wine in wood; 330 to 365 [same, no. 2]; (3) `wanted` 3 in norway_west has no evidence for 793; good for Hedeby and Birka, fair for Ribe and Kaupang; saxony, danish_isles and the delta itself are missing [same, no. 6]; (4) `cheap` [10,11,12]: new wine was sold from Martinmas; [11,12,1] is truer [same, no. 4]. Value 60: no period price for a cask on the coast; a guess. |
| frankish_wine | doubtful | The Saint-Denis fair (from 9 October) and its English and Frisian buyers are right: judgment of 753 for Saint-Denis [mem]. Doubtful: the row joins the Seine and the Loire and sends both to Ireland at 3. Seine wine went to Hamwic, London and Quentovic; take Ireland off, or split the row. Frisia at 1 is low for the buyers the charter names [eco: wine_salt §16 no. 7]. |
| aquitaine_wine | doubtful | No text of 793 to 1066 shows wine shipped from Bordeaux; the great Bordeaux trade is of the 12th century on. `wanted` dalriata 2 rests on a trade (E ware) that ended about 700 (?). Mark the row (?) and lower every strength to 1 [eco: wine_salt §16 no. 8]. |
| mead | right | The note is true of the Prussians: Wulfstan's voyage in the Old English Orosius (c. 890) says the poor and the slaves of the Ests drink mead and no ale is brewed there [mem]. The row has no `src`: add the Orosius (it is in the list as `ohthere`; Wulfstan is the same book). For the Wends it is a fair guess (?). |
| ale | right | Brewed everywhere, soon sour; hopped beer later. Value 10 is high against barley at 2 d the modius; 5 or 6 [eco: food §10.1 no. 12]. |
| sea_salt | wrong | (1) The name "Bay salt" belongs to the trade of the 14th and 15th centuries. Name it "Sea salt" or "Grey salt of the Loire". (2) `wanted` norway_west 3, vestfold 3, svealand 2, ladoga 2, prussia 2, jutland 2, gotland 2: nothing in the period shows Loire salt going to any of them. That northern salt trade is Hanseatic. The texts have it going up the Loire and to the Bretons. Set `wanted` loire 1, neustria 2, ireland 1 (?), and drop the rest. (3) `zeeland` 1 as a maker of this salt is wrong: Zeeland and Frisia made peat salt, a different, darker good (Lorsch held salt pans in Zeeland from 776 [mem]). (4) `cheap` [9,10,11] and `dear` [4,5,6] are backwards: salt is dearest at the autumn slaughter and the herring. Set `cheap` [7,8,9], `dear` [10,11]. All four: [eco: wine_salt §16 nos. 12 to 15]. |
| brine_salt | doubtful | Droitwich is right (the Mercian kings' Saltwic, charters from 716) [mem]. Doubtful: `wanted` northumbria 2 and ireland 2. Droitwich salt went by wagon and pack-horse to the west midlands and the Thames valley, not by sea [eco: wine_salt §16 no. 18]. |
| rock_salt | wrong | Three faults. Reichenhall salt was boiled from brine springs, not mined. Mining at Hallein belongs to the end of the 12th century (?). It went east down the Danube (Raffelstetten is a Danube toll), so `made` rhineland 0.5 and `wanted` saxony 1 have no road between them. The Rhineland's real inland salt was from Vic, Marsal and Moyenvic on the Seille (Prüm held pans there). Replace the row with "Spring salt (the Seille)", made rhineland 2, wanted rhineland, meuse, rhine_delta; and, from 956, Lüneburg salt made in saxony [eco: wine_salt §16 no. 19]. |
| herring | wrong | "Salted in barrels", unit "barrel", `keeps` 270: the barrelled herring that keeps most of a year is of the 13th to 14th centuries. In period it was dry-salted or smoked, counted by the thousand, and kept weeks to a few months. Set unit "thousand (dry-salted, in baskets)", `keeps` 90 to 120. Sea fishing for the market grows sharply only about 1000 (Barrett, Locker and Roberts 2004, Antiquity 78: the "fish event horizon") [mem], so `made` england_east 2 and `wanted` rhineland 2 are too strong before 1000. `made` lacks frisia, flanders, schleswig; `wanted` lacks saxony. Sources for the row: Domesday herring renders, Aelfric's Colloquy, IV Aethelred [eco: food §10.1 nos. 3, 4]. The note on the Scanian fairs coming later is right. |
| stockfish | doubtful | `from` 1000 follows Barrett 2008 for England and the Rhine and is right there. But Arctic cod was eaten at Hedeby in the 9th to 11th centuries (Star and others 2017, PNAS: ancient DNA of the Haithabu cod) [mem], and Egil's Saga has a chief shipping stockfish to England about 875 [W: Stockfish], though that is a 13th-century telling. Set `from` about 875 with `wanted` schleswig, jutland, vestfold; keep the southern markets from 1000 (needs change 3). |
| dried_fish | doubtful | Plausible, no source. Fine as ship's food. |
| eels | right | Fen rents in eels by the thousand are in Domesday; Bede derives Ely's name from its eels (HE IV.19) [mem]. The period unit is the stick of 25, not the barrel [eco: food §10.1 no. 11]. |
| salmon | doubtful | Plausible, no source and no note. |
| whale_meat | doubtful | Ohthere speaks of whale hunting, which carries Norway. "A stranding was shared out by law" rests on later law books (Gulathing, Gragas), not on 793 (?). `wanted` is empty, though the men of Rouen sold blubber at London about 1000 (IV Aethelred 2): add made neustria 1, wanted england_south 1 [eco: food §10.1 no. 14]. Frisia 1 as a maker has no source (?). |
| cheese | right | Frisian dairy on the salt marsh is sound (Lebecq 1983; the abbeys' Frisian rents) [mem]. |
| butter | right | Sound; no source in the row. |
| salt_pork | right | Martinmas killing and pannage pigs are sound. No source in the row. |
| honey | right | Sound. No source in the row: Capitulare Saxonicum 797 (honey as a reckoning good), Ine 70.1, Wulfstan. `made` lacks wales, mercia_west, ireland [eco: food §10.1 no. 16]. |
| olive_oil | right | Oil for lamps and chrism, scarce in the north. The charter of 716 for Corbie gives 10,000 pounds of oil from the store at Fos [mem]. |

### Spices and southern foods

The best single source for this whole block is the charter of Chilperic II for Corbie (716), which lists what the
abbey drew each year at Fos: 30 pounds of pepper, 150 of cumin, 2 of cloves, 1 of cinnamon, 50 of dates, 100 of
figs, 100 of almonds, 20 of rice, 10 of orpiment, 50 quires of papyrus, 10,000 pounds of oil [mem]. It should be
in the `sources` list and on these rows.

| id | Verdict | Finding |
|---|---|---|
| pepper | doubtful | Right as a good (Corbie 716; Bede left pepper and incense at his death, 735, Cuthbert's letter [mem]). Doubtful: `made` ladoga 0.5. Nothing shows pepper coming north by the Rus rivers; Mainz (965) and London (c. 1000, the Emperor's men pay 10 pounds of pepper) put it on the Rhine. Drop ladoga [eco: pots_glass §16.1 no. 9]. The pound of "about 400 g" is the Carolingian pound (408 g): right. |
| cinnamon | right | Corbie 716 (1 pound). Empty note. |
| cloves | right | Corbie 716 (2 pounds); the Moluccas are right. |
| ginger | doubtful | Known in the west (Anthimus, 6th century) [mem]; no source in the row, no note. Fair. |
| cumin | right | Corbie 716 (150 pounds): cheap beside pepper is exactly what the charter shows. Add the source. |
| sugar | wrong | Against BRIEF rule 2 (no sugar as a common thing) and against the evidence. No witness for sugar north of the Alps before the First Crusade (?). Cane in al-Andalus is of the 10th century (Calendar of Cordoba, 961) [mem], so `made` spain 2 is wrong in 793. Take away `made` neustria 0.5 and the `wanted` 2 in neustria and rhineland, and set `from` 1000 at the earliest [eco: pots_glass §16.1 no. 10]. |
| dates_figs | right | Corbie 716 (dates 50, figs 100). |
| almonds | doubtful | Corbie 716 (100 pounds) carries the good. The note "almond milk for fast days in monastic kitchens" is a habit of the 12th century on (?): change it to "from the south, for an abbot's table". |
| rice | doubtful | A curiosity in the north is right, and attested (Corbie 716: 20 pounds). The note is wrong by years: Sicily is under the Emirs only after 827 and rice in al-Andalus is of the 10th century (?). In 793 it came from Egypt and the east. A sack of 30 kg is too large for a rarity; sell it by the pound. |
| hazelnuts | right | Sound and harmless. |
| hops | wrong | "Corvey's statutes of 822" is wrong. The statutes are Abbot Adalhard's for **Corbie** on the Somme [W: History of beer: "In 822 the Carolingian Abbot Adalard of Corbie wrote about the use of hops in beer"]. Corvey on the Weser was only founded in 822. Earlier still: hop gardens left to Saint-Denis by Pippin in 768 [W: Hops], so `from` may be 768 (that is, 0). The Graveney boat (Kent, c. 950) carried hops: add made neustria, wanted england_south [mem; eco: food §10.1 no. 1]. |
| mustard_seed | right | In the Capitulare de villis plant list (sinape) [mem]. Harmless. |

### Livestock

| id | Verdict | Finding |
|---|---|---|
| cattle | doubtful | Value 24 is the ox of the Ripuarian law (2 shillings; the cow is 1 shilling, or 3 in some copies) [mem, Lex Ribuaria 36 (40), 11; I could not open the text today (?)]. So 24 fits an ox and is dear for a cow. Weight 300 kg leans modern; the beasts of the time were about 200 to 250 kg (?) [eco: food §10.1 no. 9]. |
| sheep | doubtful | Plausible; no source. 40 kg is heavy for the time (25 to 35) (?). |
| pigs | doubtful | Plausible; no source. 80 kg is heavy (50 to 70) (?). |
| horses | doubtful | The Ripuarian law gives a sound stallion 7 shillings (84 d; 12 in some copies) and a mare 3 (36 d) [mem (?)]. So "worth a sword or more" is right by the law (a sword with scabbard is also 7), but `value` 120 matches neither figure: 84. The law: the Pîtres ban (864) is right [W: Edict of Pistres: death for selling horses to the Vikings]. An earlier ban is missing: the capitulary of Mantua (781), c. 7, forbids selling stallions out of the realm [mem (?)]. And England: no horse over sea except as a gift (II Aethelstan 18, c. 930) [eco: food §10.4 no. 1 (?)]. `wanted` svealand 2 and norway_west 2 have no source. |

### Textiles, fibres and dyes

| id | Verdict | Finding |
|---|---|---|
| frisian_cloth | doubtful | The name and the gift are right: *pallia Fresonica*, white, grey, red and blue, sent to Harun, in Notker, Gesta Karoli II.9 (written in the 880s, so a story about Charlemagne, not a record) [mem]. The note is honest that the place of weaving is argued. Doubtful: `wanted` caliphate 2 rests on that one gift; svealand 3 and ladoga 3 rest on a disputed reading of the Birka twills (2 is safer); Flanders as a maker is of about 1050 on; the name drops out of the texts after the 880s. The unit "bolt" is out of period: cloth was sold by the cloak and the ell [eco: cloth_wool §13.1 nos. 7 to 9, 11]. |
| wool_cloth | wrong | (1) It misses the best-attested cloth trade of the start year: English cloaks sent to Francia, the subject of Charlemagne's letter to Offa of 796 [W: Offa of Mercia: "cloaks (or possibly cloths), traded from England to the Franks"]. Add `wanted` neustria 2, rhineland 1, rhine_delta 1. (2) Value 18 for 8 kg is the price of the raw wool in it once wool is priced by the sources (see raw_wool); cloth is three to five times its wool: 35 to 45 [eco: cloth_wool §13.1 no. 3]. |
| wadmal | doubtful | Wadmal as money in Iceland is right, but only from the 10th to 11th centuries. Iceland is empty before 874 and has no surplus before about 930; the row has it making from year 0 (by years). |
| linen | doubtful | Sound in kind; no source, no note. Frisia, the Danish isles and the Rhineland also made it [eco: cloth_wool §13.1 no. 14]. |
| fine_linen | doubtful | Plausible (Egyptian linen to the churches, McCormick); *sindon* is a fair word. No price evidence. |
| silk | doubtful | Right as a good: silk strips in the Birka graves and in the Oseberg burial (834) [mem]. Doubtful: `made` lacks italy (Pavia and Venice were the main western door); ladoga 0.5 should end about 970; and the richest good in the table has no law: Byzantium forbade the export of purple and the best silks (Book of the Eparch, c. 912; Liudprand's silks seized, 968) and the treaty of 944 capped a Rus buyer at 50 gold coins a piece [mem] [eco: pots_glass §16.1 no. 8, §16.3 no. 5]. |
| silk_trim | right | Cut strips sewn on clothes: Birka, Oseberg, Dublin. No source in the row; add one. |
| cotton | doubtful | The note says "almost unknown in the north" and that is right; then `wanted` neustria 1 should go. Cotton in Spain is of the 10th century (?). Harmless: no northern market sells it. |
| raw_wool | wrong | (1) Value: 8 d for 30 kg is a quarter of a penny a kilo. Ine's fleece (2 pence) and Edgar's wey (120 pence) both give 1 to 2 d a kilo: a 30 kg sack is 35 to 45 d [eco: cloth_wool §13.1 no. 1]. (2) `wanted` flanders 3 from the first year is 250 years early (the note itself says "will come to"). In 793 the Flemish marshes have their own sheep. Weak or nothing until about 1000, strong from about 1050 [same, no. 6] (needs change 3). |
| fleeces | wrong | Ten fleeces of the time weigh 6 to 10 kg, not 25, and by Ine are worth about 14 d, not 8. The row also doubles raw_wool. Drop it or turn it into woolfells (skins with the wool on) [eco: cloth_wool §13.1 no. 2]. Iceland by years. |
| flax | doubtful | Plausible; no source, no note. |
| hemp | doubtful | Plausible; no source. |
| woad | doubtful | Right as the north's blue: *waisdo* in the Capitulare de villis c. 43; woad at Coppergate in York [mem]. The Old Norse word given, "vaitr", I cannot attest (?): leave it out. |
| madder | wrong | `made` zeeland 1: Zeeland's madder is of the 12th to 14th centuries. For 793 to 1066 the source is Francia (*warentia* in the Capitulare de villis c. 43): neustria 2, rhineland or meuse 1. `wanted` should add northumbria (York's dyers, from about 870) [eco: cloth_wool §13.1 no. 5]. |
| kermes | doubtful | The dye is real (*vermiculum* in the Capitulare de villis c. 43) [mem]. But in the northern finds kermes turns up only on silk dyed far away; `wanted` as a dye in neustria, rhineland and england_south is not shown. Keep it very rare [eco: cloth_wool §13.1 no. 12]. |
| alum | wrong | `wanted` frisia 2 and flanders 2 has no evidence in the period. Northern dyers fixed their dyes with clubmoss, as the York (Coppergate) finds show; alum as a northern trade good comes with the Italian trade after 1100. Byzantium as a maker is of the 13th century (Phocaea); in period it is Egypt. Lower to a rarity or give it a late `from` [eco: cloth_wool §13.1 no. 4]. |

### Furs, hides and northern goods

| id | Verdict | Finding |
|---|---|---|
| squirrel | wrong | (1) "(vair)" is a French word of the 12th century: drop it. (2) "The commonest fur of the trade, by the tens of thousands" is true from the 11th century; the 9th- and 10th-century writers name sable, black fox, beaver and marten. (3) `src` cites Ibn Khurradadhbih, who names beaver and black fox, not squirrel: cite Ibn Rusta and the Primary Chronicle instead [eco: furs §10.1 nos. 2 to 4]. The "timber of 40" is attested by 1086 (Domesday, Chester, for marten) [mem]; earlier (?). |
| marten | right | Ohthere: the richest Sami pay fifteen marten skins [mem]. The empty note should carry that. |
| sable | right | Ibn Fadlan (922) on the Rus and their sable is right; the Bulgars selling on is right. `made` finns 2 is a guess (?). |
| beaver | right | Ibn Khurradadhbih: the Rus bring beaver skins, black fox skins and swords [mem]. A winter pelt, but the row has no season. |
| castoreum | doubtful | Real; no source. It is a drug, not a fur: wrong category [eco: furs §10.1 no. 5]. |
| fox | doubtful | The fox the sources name is the **black** fox of the east (Ibn Khurradadhbih), which is missing; the row gives red and arctic fox from Norway with no source. Add black fox (made rus_volga 3, wanted caliphate 3) or re-aim the row [eco: furs §10.2 no. 12]. Iceland by years. |
| otter | right | Ohthere's tribute includes a coat of bear or otter skin [mem]: add norway_north and the source. |
| ermine | doubtful | Plausible; no source, no note. "Hermelin" is a later word (?). |
| wolf_skins | doubtful | Plausible; no source. |
| bear_skins | doubtful | Ohthere's tribute has a bear skin (add the source). The white bear as a king's gift is right but late: Audun's bear and Bishop Isleif's bear for the Emperor are both of the 1050s [mem]. Greenland 3 by years (after 985, and a trickle). |
| reindeer_hides | right | Ohthere: five reindeer skins in the tribute. |
| seal_skins | right | Ohthere: a ship-rope of seal hide. Greenland by years. |
| cattle_hides | doubtful | Sound in kind (Ireland's cattle wealth); no source, no season. Hides are a Martinmas glut: `cheap` [11,12,1] [eco: food §10.1 no. 15]. |
| leather | doubtful | Plausible; no source. York (Coppergate) is the best-known tannery and is missing. |
| walrus_rope | right | Ohthere, exactly: two ship-ropes, each sixty ells long, one of walrus hide and one of seal [mem]. The unit "coil (60 ells)" is the source's own. Greenland by years. |
| walrus_ivory | doubtful | Ohthere's tusks for King Alfred are right. Wrong word: "hrosshvalr tönn". Ohthere's word is Old English *horshwael*; the Old Norse beast is *rostungr* (hrosshvalr in the sagas is a monster whale) (?). Doubtful by years: greenland 3 equal to norway_north 3 (the DNA puts the Greenland boom after about 1120: Star and others 2018); to 1066 set norway_north 3, iceland 2 from 870, greenland 1 from 985. `wanted` 3 in the south from 793 is early: the carving schools are of the later 10th and 11th centuries; 1 or 2 before 950 [eco: furs §10.1 nos. 8, 9]. |
| narwhal | wrong | "Sold as unicorn's horn" has no evidence before the 12th century [W: Narwhal gives no medieval date; eco: furs §10.1 no. 7 (?)], and Greenland ivory of any kind is a trickle before 1066. With `from` 1000, value 600 and `wanted` 3 in three regions it is a great trade the period did not have. Move `from` to 1100 (outside the game) or keep one tusk as a wonder on a card with no market. |
| elephant_ivory | right | Court workshops used it, much of it old Roman ivory cut again [mem]. |
| antler | right | Reindeer antler at Ribe from the first half of the 8th century: Ashby, Coutu and Sindbaek 2015 [mem]. |
| combs | right | Comb-makers at Ribe, Hedeby and Dorestad. Sound. |
| horn | right | Sound and harmless; no source. |
| whalebone | right | Ohthere: whale's bone in the tribute [mem]. |

### Stone, pottery and glass

| id | Verdict | Finding |
|---|---|---|
| quernstones | wrong | The trade is right and central [W: Dorestad: "lava querns for grinding corn were also transported to Dorestad"]. Wrong: (1) `kg` 90 a pair is too heavy for hand querns: 25 to 40 kg dressed, 45 to 55 rough (?). Set 45. It decides whether querns are worth a voyage. (2) `src` "kirk" ("Kirk, R. and others") is not a reference. Use Parkhouse 1997 (The distribution and exchange of Mayen lava quernstones, in Exchange and Trade in Medieval Europe, Brugge) and Kars 1980 (the Dorestad querns, BROB 30) [mem; eco: stone_metal §14 nos. 2, 23]. (3) "To Birka" is doubtful: Mayen lava is well shown at Hedeby and Ribe, thinly beyond (?). Say "from Dorestad to Hedeby". |
| millstones | doubtful | A mill stone of the period is nearer 100 to 200 kg than 500 (?). At 500 kg one stone is three quarters of the boat [eco: stone_metal §14 no. 3]. No source in the row. |
| whetstones | wrong | (1) The note names only Eidsborg. The first Norwegian hones in the south, at Ribe from the early 8th century, are from Mostadmarka in Trondelag; Eidsborg in Telemark grows from the 9th century (Baug, Skre, Heldal and Jansen 2019, Journal of Maritime Archaeology 14) [mem]. (2) `made` norway_west 1 is wrong: neither quarry is in western Norway. Set vestfold 3 (Telemark's door) and norway_north 2 (nearest to Trondelag). (3) `src` skre2008 is the silver volume; the hones are in Skre (ed.) 2011, Things from the Town (Kaupang 3) [mem]. |
| soapstone | doubtful | Right as a good. By years: Norse quarrying in Shetland begins with the Norse settlement, about 850; at Dorestad soapstone appears only after 850; Iceland after 874 [eco: stone_metal §14 nos. 7, 8]. Source as whetstones (Skre 2011, not 2008). |
| badorf_ware | wrong | `to` 0: it runs on beside Pingsdorf for ever. Badorf ware gives way to Pingsdorf about 900: set `to` 900 (or 925 to overlap). The note joins Badorf (the Vorgebirge near Cologne) and Mayen, which are different wares from different places [eco: pots_glass §16.1 no. 3]. |
| tating_ware | wrong | (1) `made` frisia 1: nothing points to Frisia. Where it was made is unknown [W: de, Tatinger Kanne: "Herstellungsorte oder Herkunftsregionen sind derzeit noch unbekannt"]; the name is only the find-spot, Tating in North Frisia. Set rhineland 2 with (?), and drop frisia. (2) `to` 900 is long: it is dated to the late 8th and early 9th century [W: same page]. Set `to` 850. The spread "from Scandinavia to the Rhineland" is right. |
| pingsdorf_ware | right | Red-painted ware from about 900, following Badorf. No source in the row. |
| ipswich_ware | wrong | "Wheel-thrown" is wrong: it was built by coiling and finished on a slow wheel, then kiln-fired [W: Ipswich ware]. Say "kiln-fired and wheel-finished, the first mass-made pottery in England since the Romans". Dates (about 720 to 850) are right. `wanted` mercia_west has no backing: it reached Kent, York and west to Oxfordshire [W]. |
| glass_vessels | wrong | "Claw beakers" are of the 5th to 7th centuries. In 793 the Rhenish shapes are the funnel beaker, the palm cup and the reticella bowl. Change the note [eco: pots_glass §16.1 no. 1]. The rest is right [W: Dorestad: glassware traded there]. |
| glass_beads | doubtful | Right for 793 (Ribe's bead makers: Feveile; Sindbaek). By years it tells the story backwards: from about 790 cheap beads flood in from the Caliphate by Ladoga and Birka, and Ribe's own making fades. Split into northern wound beads (to about 850) and eastern beads (790 to 1000, made caliphate 3, hubs ladoga and svealand 0.5) [eco: pots_glass §16.1 no. 5]. |
| raw_glass | right | Mosaic cubes and broken glass melted for beads at Ribe: Feveile 2006 [mem]. The cubes come from Italy; the Rhineland gives cullet. |
| carnelian | doubtful | Right in kind (Noonan). By years: few before 800, plenty from about 860, almost none after about 970 to 1000 [eco: pots_glass §16.1 no. 7]. Needs `from` 800 and `to` 1000. |
| amber | wrong | `cheap` [10,11,12] and `dear` [6,7,8] are backwards for a merchant. It is gathered from autumn to spring while no ship sails; it is plentiful and cheap when the water opens (April to June) and scarce by autumn. Set `cheap` [4,5,6], `dear` [9,10,11] [eco: furs §10.1 no. 6]. `wanted` lacks the places with workshops and no beach: svealand, vestfold, schleswig. No source in the row (Wulfstan for Truso). |
| amber_beads | doubtful | Sound; no source, no note. Beside raw amber (30 d for 10 kg) the beads (24 d for 0.3 kg) are 27 times dearer by weight, which is too much [eco: furs §10.2 no. 20]. |
| jet | right | Bede praises Britain's jet (HE I.1) [mem]; Whitby jet; York's workers in the 10th century. |
| pearls | doubtful | Bede also names Britain's pearls of all colours (HE I.1), which carries the river pearls. `made` dalriata 0.5 uses the mark for "brought in, scarce and dear" on a place that finds them: set 1. No source in the row. |
| coral | doubtful | Plausible; no source, no note. |

### Metals and arms

| id | Verdict | Finding |
|---|---|---|
| bar_iron | doubtful | The Veluwe as one of the great iron districts of the 7th to 9th centuries is right, but its ore was rattlestone from the hills, not bog ore (Joosten 2004) [mem]. Change "bog iron ... (the Veluwe)" to "iron smelted in the forests (bog ore in the north, rattlestone on the Veluwe)". `made` lacks england_south (the Weald) and mercia_west (the Forest of Dean). No source in the row. |
| iron_tools | doubtful | Sound; no source. "Crate" is not a thing of the time: chest or bundle. |
| rivets | wrong | The Old Norse word "sau" is not a word. It is *saumr* (ship nails; the rove is *ró*) [mem]. No source in the row. |
| crucible_steel | doubtful | The row marks itself (?), rightly: Williams 2009 argued for crucible steel in some Ulfberht blades; others dispute it and no trade route is shown. Keep it off ordinary markets. |
| copper | wrong | `made` saxony 1 from year 0 against its own note: the Rammelsberg is said to open in 968 (Widukind and Thietmar on the silver found in Otto's day) [mem]. Before 968 copper in the north is mostly scrap melted again. Give saxony a start of 968 (needs change 3), or set `from` 968 and let brass carry the earlier years. |
| brass | doubtful | Brass bars are found at Ribe and Hedeby, and melting them for brooches is right. "Mosan brass" at strength 3 in 793 is early: the Meuse brass towns are known from the 11th century; the calamine is near Aachen (?). Set meuse 2, rhineland 2. "Messing" is a later word. |
| tin | doubtful | Cornish tin is right in kind, though the evidence for 793 to 1066 is thin (?). It cannot be bought: cornwall has no port. Give england_south `made` 0.5 [eco: stone_metal §14 no. 22]. The Tating-ware foil is a good touch. |
| lead | wrong | "Charlemagne asked Offa for lead" has no source. The letter of 796 is about black stones and cloaks [W: Offa of Mercia: lead is not in it]. The attested request is Abbot Lupus of Ferrières to King Aethelwulf of Wessex, about 852, asking for lead to roof his church, to be delivered at Etaples [mem; W: Lupus Servatus does not carry it, so (?) on the detail]. Also the Wirksworth charter of 835 (a rent of lead worth 300 shillings to Canterbury) [mem]. Change the note; `sawyer2013` should not stand for the Offa claim. |
| quicksilver | right | Almadén, and fire-gilding. No source in the row. |
| swords | doubtful | 84 d is 7 Ripuarian shillings at 12 d: Lex Ribuaria 36 (40), 11 and 12 [mem (?)]. The project's own note warns the shillings may be gold ones of 40 d (then 280 d). Mark the value (?). `made` rhine_delta 1 has no evidence: Dorestad sold swords on, it did not forge them (0.5 at most). The law: see the `arms` law below. In 793 selling a sword to a Dane was not yet forbidden by any text in the list. Ibn Khurradadhbih on the Rus selling swords is right. |
| sword_blades | doubtful | The inscription is right. `from` 800 is early: the signed blades are of the 9th to 11th centuries, the earliest usually put near 850 (?) [eco: stone_metal §14 no. 10]. Value 60 for a bare blade stands above the Ripuarian sword without scabbard (3 shillings, 36 d) [mem (?)]: fine only if an Ulfberht is meant to be dear. |
| spearheads | right | Winged Frankish spearheads were wanted and copied. Law as swords. |
| axes | doubtful | Plausible; no source. Carries `law` arms although no Frankish text names axes; see the law. |
| byrnies | right | 12 shillings in the Ripuarian law [mem (?)]; the ban is the one clear arms law of the start year (Herstal 779, c. 20). Doubt only on mail as an ordinary shop good: it was made to order and moved as gift, loot and ransom; truer as a commission [eco: stone_metal §14 no. 28]. |
| helmets | right | 6 shillings in the Ripuarian law [mem (?)]. Law as swords. |
| shields | right | Shield and spear together 2 shillings in the Ripuarian law [mem (?)]; lime-wood boards with an iron boss are right. |
| bows | doubtful | Plausible; no source. `made` wales rests on the Welsh bowmen of the 12th century (?). |

### Captives, books and church goods

| id | Verdict | Finding |
|---|---|---|
| captives | wrong | (1) `value` 48 is three to five times too low. The evidence runs from 120 to 240 d for an ordinary adult from the Baltic to England (a mark of silver in the north; a pound in England). Set 160 [eco: furs §10.1 no. 1, §5.6]. (2) The law in the row and in the rules treats all captives alike; the whole of the law is the difference between a Christian and a heathen captive. Needs two goods or a tag (see the law). (3) `wanted` lacks schleswig (Hedeby, the best-attested market: Rimbert), meuse (Verdun), italy (Venice). Verdun to Spain is attested by Liudprand (Antapodosis VI.6, 10th century) [mem]. (4) Ireland as a market and a source is of 841 on (Dublin). |
| books | doubtful | Sound. `wanted` svealand 1 is true only around Ansgar's missions (829 to 831, 852) and after 1000; northumbria stops making after 867 (by years). |
| parchment | right | "A big book takes a whole herd": the Codex Amiatinus took over 500 calves [mem]. |
| papyrus | right | The papal chancery used it to 1057 [mem]; Corbie drew 50 quires in 716. `to` 1050 is right within a few years. |
| pigments | doubtful | "Lapis lazuli from Afghanistan" is early for the north: the blues of the books around 793 are mostly indigo or woad; lapis is rare before the 10th century (?). Say "orpiment, red lead, vermilion; indigo and, rarely, lapis" [eco: pots_glass §16.1 no. 11]. Orpiment is in the Corbie list of 716. |
| incense | right | Bede's pepper and incense (735); Arabian incense for the churches. |
| relics | wrong | "Buying or selling holy relics is simony" is not the law of the time. Simony is the sale of holy office. What the period's law says is: no saint's body moved without the leave of prince or bishops (Council of Mainz 813, c. 51) [mem], and no church treasure sold to merchants (Nijmegen 806). In practice relics passed by gift for gift, with a bishop's letter, and were openly bought: the Roman deacon Deusdona supplied Einhard in 827 (Einhard, Translation of Marcellinus and Peter) [mem]. A flat "no sale" forbids what the sources are full of. Change the rule to "needs leave" [eco: pots_glass §16.1 no. 12, §16.3 no. 6]. `made` italy only: add neustria. |
| church_plate | doubtful | The law is right and has a source the table lacks: Nijmegen 806, where Charlemagne says merchants boast they can buy what they like from church treasuries [mem]. Doubtful: 1.5 kg of silver is about 880 d melted against a value of 240; a chalice is nearer 300 to 500 g (?). Set `kg` 0.4 [eco: silver_coin §13.4 no. 22]. |
| brooches | right | Oval brooches cast at Ribe (Feveile). "Dalkr" is a cloak pin; fair. Iceland by years. |
| musk | doubtful | Real, but no northern market makes or wants it; a row with no use until the Caliphate has ports. No source. |

### Timber, ship stores and the rest

| id | Verdict | Finding |
|---|---|---|
| oak_timber | wrong | (1) `made` norway_west 2: western Norway is pine and birch country; oak grows only in the far south (Vestfold, Agder). Drop norway_west. (2) The treeless coast's oak came down the Rhine: the well casks and timbers of Dorestad are of wood from the middle and upper Rhine (tree-ring studies of Dorestad) [mem (?)]. Add `made` rhineland 3, meuse 1. Iceland and Greenland by years. No source in the row. |
| masts | doubtful | Plausible (Norway's pine); no source. Iceland by years. |
| yew_staves | doubtful | Yew bows are found at Hedeby; where the wood came from is a guess. No source. |
| tar | right | Large tar pits in the forests of Uppland from the 8th century: Hennius 2018, Antiquity 92 [mem]. Raise svealand to 3 and add the source. "Caulking" was tarred wool or hair; the tar preserves. |
| pitch | doubtful | Plausible; no source, no note. Doubles tar. |
| rope | doubtful | Lime-bast rope is right for the north; no source. |
| sailcloth | doubtful | Wool sails, greased: right. 40 d for 15 kg undersells the most labour-heavy cloth there was; every ship-building coast wants it [eco: cloth_wool §13.1 no. 13]. Iceland by years. |
| potash | doubtful | Wood-ash glass begins in the Carolingian realm about 800 (?), but the ash was burned at the glasshouse; no trade in it is shown. Harmless. |
| falcons | doubtful | The value fits the one price there is: the Ripuarian law gives a hawk 3, 6 or 12 shillings (36 to 144 d) by its training [mem (?)]: add `lexrib`. The trade of 793 went the other way from the row: King Aethelberht of Kent asked Boniface for falcons from Germany (c. 750) [mem]. Norway's gyrfalcons as kings' gifts are of the 11th century on; Iceland and Greenland by years. |
| hounds | doubtful | Plausible (hounds as gifts); no source. |
| eider_down | doubtful | `src` ohthere: Ohthere speaks of feathers, not of eider down by name [mem]. Keep the good, mark the source (?). Iceland by years. |
| feathers | right | Ohthere: ten "ambers" of feathers in the tribute. A warning for the writers: the amber here is a measure (a tub), not the stone. |
| whale_oil | doubtful | Plausible; no source. |
| seal_oil | doubtful | Plausible (the Baltic seal hunt); no source, no note. |
| wax | right | The Church's want of wax is right. No source in the row: Raffelstetten (wax as a toll good), the Rus treaties. Wrong category ("Animals"). No season: cheap [9,10,11], dear before Candlemas [eco: food §10.1 no. 13]. |
| sulphur | doubtful | Iceland's sulphur trade is known from the 13th century; nothing shows it before 1066 (?). Iceland by years. Keep it out of the early game. |

---

## 2. goods_793.json: the head of the file

| Entry | Verdict | Finding |
|---|---|---|
| about | doubtful | "Deniers of about 1.7 g after Charlemagne's reform of 793/4": right as a yardstick, but Frankfurt is June 794 and the game starts 21 June 793, so the coin in the purse on the first day is the old light denier of about 1.3 g [eco: silver_coin §13.1 no. 3]. |
| made_factor, wanted_factor | not judged | Tuning. |
| regions | doubtful | The list is sound. Lacking for goods the table holds: no Brittany (the salt carriers), no Moselle or Seille (the Rhineland's salt), no place for Melle (Carolingian silver and lead), no word of the Harz under saxony from 968, no Trondelag (the first whetstones), no Peak (the lead of Wirksworth). "Sliesthorp (Hedeby)": before 808 it is a small place; Reric is the market until Godfred moves its merchants (Royal Frankish Annals, 808) [mem]. "Iceland (settled from 874)" and "Greenland (settled from 985)" are right, but no good obeys the dates. |
| src: mccormick2001, verhulst2002, hodges1982, lebecq1983, sawyer2013, noonan1998, willemsen2010, feveile2006 | right | Real books, rightly titled and dated [mem]. |
| src: skre2008 | doubtful | Real and rightly titled, but it is the silver volume. The whetstone and soapstone rows that cite it need Skre (ed.) 2011, Things from the Town (Kaupang Excavation Project 3) [mem]. |
| src: lexrib | doubtful | The values are as I remember them, but I could not open the text today (?). Two cautions belong in the line: the copies differ (cow 1 or 3, horse 7 or 12), and the 12 d shilling is a Carolingian clause (36, 12); the older text may mean gold shillings of 40 d. |
| src: frankfurt794 | right | c. 4: oats 1, barley 2, rye 3, wheat 4 d the modius; 12 wheat loaves of 2 pounds a penny [mem]. |
| src: arms | doubtful | The three texts are real: Herstal 779 c. 20 (mail), Thionville 805 c. 7 (arms and mail to Slavs and Avars; the whole load forfeit), Pîtres 864 c. 25 (death) [mem; W: Edict of Pistres for 864]. Lacking: Mantua 781 c. 7 (arms, stallions and slaves out of the realm (?)) and Boulogne 811. |
| src: raffelstetten | right | c. 903 to 906; salt, slaves, horses, wax. |
| src: ohthere | right | Rightly described. Add that Wulfstan's voyage to Truso stands in the same book. |
| src: ibnfadlan, ibnkh | right | Rightly dated and described. |
| src: pitres864 | doubtful | Doubles `arms`. Should say c. 25 and that it bound West Francia only [W: Edict of Pistres]. |
| src: sindbaek | right | Antiquity 81 (2007) [mem]. |
| src: coupland | wrong | "Coupland, S. (2005-): on Carolingian coinage and Frisian trade" is not a reference and no row cites it. Use Coupland 2007, Carolingian Coinage and the Vikings (Variorum), or "Trading places: Quentovic and Dorestad reassessed", Early Medieval Europe 11 (2002) [mem]. |
| src: barrett2008 | right | Journal of Archaeological Science 35 [mem]. |
| src: ashby2015 | right | European Journal of Archaeology 18 [mem]. |
| src: kirk | wrong | No such work that I can find. Replace with Parkhouse 1997 and Kars 1980 (see quernstones). |
| sources lacking | wrong | 76 of 140 rows have `src` empty, against BRIEF rule 1. The texts most needed: the Corbie charter of 716; the Capitulare de villis; Charlemagne's letter to Offa (796); Rimbert's Life of Anskar; Notker; IV Aethelred (the London tolls); Domesday Book; Bede HE I.1; the Praeceptum negotiatorum of 828. |

---

## 3. trade_rules.json

### Coins

| Entry | Verdict | Finding |
|---|---|---|
| denier | right | About 1.7 g, 240 to the pound, from the reform of 793/4 [mem; eco: silver_coin §1]. One caution: on the start day it is not yet struck; the old denier is about 1.3 g. |
| penny | wrong | 1.2 g is the old light penny. Offa's heavy penny dates from 792 to 793 [W: Offa of Mercia: "the weight of the pennies was increased again" about 792-93] and weighs about 1.45 g (?) [mem, Grierson and Blackburn 1986]. Set `silver_g` 1.45 and the English zone's `unit_d` 0.853. The note's "older sceattas" went out of use about forty years before the start; drop them [eco: silver_coin §13.1 no. 1]. |
| dirham | right | The lawful weight is 2.975 g; 2.9 is near. "By weight, often cut" in the north is right. |
| hacksilver | doubtful | Right for about 850 to 1000. In 793 the cut-silver economy with scales and standard weights is two generations away [eco: silver_coin §13.1 no. 5]. |
| coins lacking | doubtful | No coin changes weight in 273 years (English penny 1.3 to 1.7 g by reign; denier 1.75 from 864). No styca for Northumbria. No gold (mancus, dinar, nomisma). |

### Zones

| Entry | Verdict | Finding |
|---|---|---|
| frankish | wrong | The tenth is right, but only where the texts put it: Quentovic, Dorestad and the Alpine passes ("where the tenth is taken for our use", Praeceptum negotiatorum 828; the same three in the abbey charters of 775 and 779) [mem]. The row charges 0.10 in every port of twelve regions and its note adds "and the river tolls". River tolls were small fixed dues by the boat or the load, not a tenth. Set the tenth on the ports, not the zone. `foreign_loss` 0.10 is a guess; the one figure is the moneyer's shilling in 22 (about 4.5 %) at Ver, 755 (?). The zone has no dates: after 843 the Rhineland, Frisia and Saxony are another kingdom with another coin. |
| english | wrong | (1) `toll` 0.05 has no source. The one English tariff (IV Aethelred 2, London, c. 1000) charges by the ship: a small boat a halfpenny, a larger one with a sail a penny, a keel or hulk four pence; the only twentieth in it is on the Rouen men's fish [mem; eco: food §10.4 no. 7]. For 793 the charters freeing church ships at London prove a toll and give no rate. (2) Northumbria does not belong: Offa's pennies did not pass there; it had its own base coin (the styca, to 867) [eco: silver_coin §13.2 no. 9]. (3) `unit_d` follows the penny: 0.853. |
| weight | wrong | (1) `toll` 0.0 is wrong where a king held the market: Godfred moved the merchants of Reric to Sliesthorp in 808 for the dues they paid him (Royal Frankish Annals 808); Birka had a king's reeve (Rimbert) [mem]. (2) Ireland, Wales, Cornwall, Dal Riata and Shetland in 793 are not lands of weighed silver but of no silver: cattle and kind. Weighed silver comes with the Northmen from the 840s. (3) "No coin of their own" is wrong at both ends: Ribe counted its own small pennies in the 8th century (?), Hedeby struck from about 825, Dublin from 997, the northern kings from about 995 [eco: silver_coin §13.2 nos. 9 to 11]. |
| dirham | doubtful | The Khazar tenth and the Bulgar king's tenth are attested (Ibn Khurradadhbih; Ibn Fadlan) [mem]. Doubtful: Spain is lumped with the Volga under a note about the Khazars; Ladoga weighed as much as it counted (?) and has no known lord to take a tenth in 793; the dirhams stop between about 970 and 1015 and Itil falls about 965 to 969, but the zone runs for ever. |
| byzantine | doubtful | The kommerkion of a tenth is right [mem]. Hacksilver is a stand-in the file admits. No law on silk. |

### Laws

| Entry | Verdict | Finding |
|---|---|---|
| arms, frankish, from 779, "no export" | wrong | Wider than the texts. Herstal 779 c. 20 bans only mail shirts. Arms as such are banned to the Slavs and Avars in 805 (Thionville c. 7, with nine named border posts and the whole load forfeit). As written, selling a spearhead to an Englishman or a sword to a Dane in 793 is a crime; no text in the list says so. Set: byrnies from 779, to anyone outside the realm; swords, sword_blades, spearheads, helmets from 805, toward wendland (and the Avars); axes not at all before 864 [eco: stone_metal §14 no. 4]. If the earlier general ban is wanted, its text is Mantua 781 c. 7 (Italy) (?). |
| arms, frankish, from 864, "death" | wrong | The penalty and year are right [W: Edict of Pistres]. The reach is wrong: Pîtres is Charles the Bald's law for West Francia. In 864 the Rhine delta, Frisia, the Rhineland and Saxony were not his. Bind neustria, loire, aquitaine, flanders only. `to` 0 runs to 1066: after 911 the Northmen of Rouen are the king's own men. Give it `to` 911 [eco: stone_metal §14 nos. 5, 6]. |
| horses, frankish, from 864 | wrong | Same fault of reach and end as the last. Lacking: England, no horse over sea except as a gift (II Aethelstan 18, c. 930) (?). |
| captives, frankish and english, from 0 | wrong | No dates and no difference between Christian and heathen, which is the whole law. Francia from 779 (Herstal c. 19): no sale beyond the march, and sale only before bishop or count; heathen captives lawfully bought and sold within the realm (Meaux 845) [mem]. England: Ine (c. 690) forbids only selling one's own countryman over sea; the general ban on selling Christians out of the land is 1008 (V Aethelred), and it was broken at Bristol into the 1060s. Rule: "no Christians to heathens or out of the land", `from` 779 Franks, `from` 1008 English [eco: furs §11 no. 1]. |
| sacred, frankish and english, from 0, "no sale" | wrong | For plate, right (Nijmegen 806). For relics, wrong: see `relics`. As it stands the player can never lawfully carry a relic, though carrying them by leave and by gift is what the sources show. Add a rule "needs leave" (Mainz 813 c. 51). |
| laws lacking | doubtful | No coin law (794: the new denier must be taken; 864: old coin void). No famine law (Thionville 805: no food out of the realm; the caps of 794 and 806: oats 2, barley 3, rye 4, wheat 6 d the modius) [mem]. No English witness rule (nothing over 20 pence bought outside a town, from about 930). No Sunday rule (Francia from 789). No silk law for Byzantium. No toll freedom by charter, though the act `show_charter` exists. |

### The market block and the start

| Entry | Verdict | Finding |
|---|---|---|
| spread, stock_value, stock_min, stock_max, regen, elasticity, appetite_value, glut_elasticity, glut_fade, unwanted, shock_fade | not judged | Tuning; no period fact in them. |
| price_min 0.6, price_max 2.2 | right | The law of 806 sets famine prices at one and a half to twice those of 794 (wheat 4 to 6, oats 1 to 2) [mem]: a ceiling of about twice is the period's own. |
| cheap 0.85, dear 1.2 | doubtful | Too gentle for grain and wine. Nijmegen 806 c. 17 blames men who buy at harvest for 2 d and sell for 4 or 6: a swing of two to three [mem; eco: food §10.4 no. 12]. One number for all goods cannot be right: grain about 0.7 to 1.6, cloth much flatter. |
| start.debt 480 | right | Two pounds of 240 d: the reckoning is the period's (816 g of silver). No source can be asked for an invented debt. |
| start.coins: denier 40 | doubtful | On 21 June 793 these are old light deniers (see `about`). The data should say which. |
| start.coins: penny 25 | right | English pennies in a Dorestad purse are sound (the trade to London and Hamwic). |
| start.coins: dirham 6 | wrong | Dirhams reach Ladoga in the 780s and the Baltic about 800; none to speak of reach the Rhine, and Frankish mints melted foreign coin. Set 0, or 1 as a curiosity with a line about it [eco: silver_coin §13.1 no. 4]. |
| start.coins: hacksilver 30 | doubtful | A Frisian of 793 might carry scraps for a smith; it should not read as a money he spends at Ribe (see the coin). |
| start.creditor, due_days | not judged | Invented. One caution for the writers: lending at interest is forbidden to all from 789 (Admonitio generalis c. 5) [mem], so the count's gain must be a share, a pledge or a service, not interest. |

---

## 4. The count

| Verdict | Goods | Rules and head |
|---|---|---|
| right | 44 | 13 |
| doubtful | 69 | 14 |
| wrong | 27 | 13 |
| not judged (tuning) | 0 | 3 |

Most "doubtful" rows are doubtful for one of two reasons: no source in the row, or true for only part of the
period. Few are doubtful in kind. The 27 wrong goods are: barley, ships_bread, sea_salt, rock_salt, herring, sugar,
hops, wool_cloth, raw_wool, fleeces, madder, alum, squirrel, narwhal, quernstones, whetstones, badorf_ware,
tating_ware, ipswich_ware, glass_vessels, amber, rivets, copper, lead, captives, relics, oak_timber.

---

## 5. The ten changes that matter most

Written for the programmer. Each says what to change, in which file, and why it comes before the others. Paths:
`Assets/Kapman/Game/Data/goods_793.json` (G) and `Assets/Kapman/Game/Data/trade_rules.json` (R).

### 1. Take the toll at the port, not across the zone (R; needs code)

Now: `zones[].toll` is one share for every port in the zone (Frankish 0.10, English 0.05, weight 0.0).
Change: add a toll list keyed by port, and let a zone's `toll` be only the fallback. Three kinds of toll are needed:

```json
"tolls": [
 {"ports": ["dorestad", "quentovic"], "kind": "share", "share": 0.10, "from": 0, "to": 0, "src": "Praeceptum negotiatorum 828"},
 {"zone": "frankish", "kind": "per_ship", "d": 1, "note": "river and bridge dues: small, fixed"},
 {"zone": "english", "kind": "per_ship", "d_small": 0.5, "d_sail": 1, "d_keel": 4, "src": "IV Aethelred 2"},
 {"ports": ["reric"], "kind": "share", "share": 0.05, "from": 0, "to": 808, "note": "the Danish king's due (rate a guess); the merchants are moved to Sliesthorp in 808"},
 {"ports": ["sliesthorp", "birka"], "kind": "share", "share": 0.05, "from": 808, "note": "the king's due (rate a guess)"}
]
```

Set `zones.frankish.toll` to 0 and `zones.english.toll` to 0 once the list is read. Use the port ids of
`ports_793.json`. Why first: the tenth at Dorestad is the best-attested money fact in the game and the player meets
it on day one; charged everywhere it is wrong in every Frankish port but two, and it hides the reason a merchant
would land his goods somewhere else.

### 2. Make the arms and horse laws say what the texts say (R and G)

In R `laws`, replace the two `arms` rows and the `horses` row with:

```json
{"law": "mail",   "zones": ["frankish"], "from": 779, "to": 0,   "rule": "no export", "note": "Herstal 779 c. 20: no byrnie sold out of the realm."},
{"law": "arms",   "zones": ["frankish"], "from": 805, "to": 0,   "rule": "no export", "toward": ["wendland"], "penalty": "whole load forfeit", "note": "Thionville 805 c. 7: to the Slavs and Avars."},
{"law": "arms",   "regions": ["neustria", "loire", "aquitaine", "flanders"], "from": 864, "to": 911, "rule": "death", "toward": "northmen", "note": "Pîtres 864 c. 25."},
{"law": "horses", "regions": ["neustria", "loire", "aquitaine", "flanders"], "from": 864, "to": 911, "rule": "death", "toward": "northmen", "note": "Pîtres 864 c. 25."}
```

In G set `"law": "mail"` on `byrnies`; leave `"law": "arms"` on `swords`, `sword_blades`, `spearheads`, `helmets`;
set `"law": ""` on `axes`. The code must read the new optional fields `regions` (instead of `zones`) and `toward`
(the buyer's region, or "northmen" for the weight zone's Norse regions). Fix the `risk` words on those six goods to
match. Why: as written the player's most profitable early trade, a sword to Ribe in 793, is a crime that no law of
793 made one; and from 864 a West Frankish law hangs men in Dorestad, which was not in that kingdom.

### 3. Let a region start and stop making or wanting a good (G; needs code)

Now: one `from` and `to` per good. Change: allow a region's strength to be either a number or an object with
years, and read both:

```json
"made": {"norway_west": 2, "iceland": {"s": 3, "from": 930}, "shetland": {"s": 2, "from": 850}}
```

Then set these, all from the findings above:

- `iceland` from 874 (930 for surplus goods) on: wadmal, fleeces, sailcloth, fox, walrus_ivory, falcons, eider_down,
  sulphur, and as a buyer on soapstone, oak_timber, masts, brooches.
- `greenland` from 985 on: bear_skins, seal_skins, walrus_rope, walrus_ivory (strength 1), falcons.
- `shetland` from 850 on soapstone; `frisia` wanted from 850 on soapstone.
- `saxony` made from 968 on copper (and add lead, from 968).
- `flanders` wanted on raw_wool: 1 from 1000, 3 from 1050; nothing before.
- stockfish: good `from` 875; `wanted` schleswig, jutland, vestfold 2 from 875; england_east, rhine_delta,
  flanders, neustria from 1000.
- walrus_ivory `wanted` england_south, rhineland, neustria: 1 to 950, 3 after.
- carnelian `from` 800, `to` 1000. glass_beads: see its row.

Why: about thirty rows are wrong only by their years, and the game runs 273 of them. This one change of format
mends them all and is the ground for change 10.

### 4. Mend the money of the first day (R)

- `coins.penny.silver_g`: 1.2 to 1.45; note: "Offa's heavy penny of 792/3 (about 1.45 g)"; drop "the older sceattas".
- `zones.english.unit_d`: 0.706 to 0.853.
- `zones.english.regions`: remove `northumbria`. Add a zone `northumbrian` (coin: a new `styca`, silver_g 0.1,
  `foreign_loss` 0.5, regions `["northumbria"]`, `to` 867), or, if a `to` on zones is not built yet, move
  `northumbria` to the `weight` zone with a note.
- `zones.weight.regions`: move `ireland`, `dalriata`, `shetland`, `cornwall`, `wales` to a new zone `kind` (no
  coin; silver taken by weight at a keen price; `toll` 0) until 840.
- `start.coins.dirham`: 6 to 0.
- `start.coins.denier`: keep 40 but add `"_denier": "old light deniers of about 1.3 g; the new coin comes in 794"`.

Why: every price the player sees in England is turned from deniers through `unit_d`; at 0.706 they are all one
fifth too high in pence.

### 5. Captives: the price and the law (G and R)

- G `captives.value`: 48 to 160.
- G: split into `captives_heathen` and `captives_christian` (same row otherwise), or add a `faith` tag to a lot.
- R `laws`: replace the `captives` row with two:

```json
{"law": "captives_christian", "zones": ["frankish"], "from": 779,  "to": 0, "rule": "no export", "note": "Herstal 779 c. 19: no sale beyond the march; sale only before bishop or count."},
{"law": "captives_christian", "zones": ["english"],  "from": 1008, "to": 0, "rule": "no export", "note": "V Aethelred: no Christian sold out of the land. Before it only Ine 11: not one's own countryman over sea."}
```

- G `captives.wanted`: add `schleswig` 3, `meuse` 2, `italy` 2.

Why: it is the one good where the table's price is wrong by a factor of three or more against good evidence, and
the one law where the table forbids what was lawful (heathen captives in Francia) and allows nothing to turn on
what the sources turn on.

### 6. Put the salt right (G)

- `sea_salt`: `name` "Sea salt (the Loire)"; `made` `{"loire": 3, "aquitaine": 1}`; `wanted`
  `{"neustria": 2, "loire": 1, "ireland": 1}`; `cheap` [7, 8, 9]; `dear` [10, 11]; `risk` "Wet: rain and spray melt it."
- New row `peat_salt`: "Peat salt", made `zeeland` 2, `frisia` 2; wanted `rhine_delta` 2, `rhineland` 1,
  `flanders` 1, `saxony` 1; value 2; note "Dark, bitter salt burned from salt peat on the Zeeland and Frisian
  coast; abbeys held pans there from the 770s."
- `rock_salt`: `id` may stay; `name` "Spring salt (the Seille)"; `made` `{"rhineland": 2}`; `wanted`
  `{"rhineland": 1, "meuse": 1, "rhine_delta": 1}`; note "Boiled from the salt springs of Vic, Marsal and Moyenvic;
  Prüm and other abbeys held pans there." Take Reichenhall and Hallein out.
- `brine_salt.wanted`: `{"england_south": 2, "mercia_west": 1, "wales": 1}`.

Why: salt is in the player's home water, three of the four rows have a wrong fact, and the first has a name and a
whole northern trade taken from 500 years later.

### 7. Correct the wrong facts in the notes (G; words only)

- `lead.note`: "For church roofs and gutters: about 852 Abbot Lupus of Ferrières asked King Aethelwulf for English
  lead for his church roof (?)." Remove the Offa sentence.
- `hops.note`: "Corvey's" to "Corbie's (Abbot Adalhard, 822); hop gardens were left to Saint-Denis in 768."
- `glass_vessels.note`: "claw beakers" to "palm cups".
- `ipswich_ware.note`: "The first mass-made pottery of England since the Romans: kiln-fired and finished on a
  slow wheel, from the kilns of Gipeswic."
- `tating_ware`: `made` `{"rhineland": 2}`; `to` 850; add to the note "where they were made is not known (?)".
- `badorf_ware.to`: 900.
- `whetstones`: `made` `{"vestfold": 3, "norway_north": 2}`; note: "Schist hones from Mostadmarka in Trondelag
  (the first to go south) and from Eidsborg in Telemark, shipped out through Kaupang."
- `rivets.period`: "sau" to "saumr".
- `walrus_ivory.period`: "horshwael (OE), rostungr (ON)".
- `squirrel.name`: "Squirrel pelts"; note: "Grey winter squirrel: the forest's tax fur; the great bulk fur from
  about 1000."
- `relics.risk`: "No saint's body may be moved without leave of prince or bishop (Mainz 813); they pass by gift,
  with a bishop's letter. Stealing them is sacrilege, though thieves' tales were told as miracles."
- `almonds.note`, `rice.note`, `pigments.note`, `bar_iron.note`, `ships_bread.note`: as in their rows above.
- `oak_timber.made`: `{"rhineland": 3, "saxony": 2, "vestfold": 2, "meuse": 1, "svealand": 1, "england_south": 1}`.

Why: these are plain errors of fact that the game will print as true; each is one line.

### 8. Mend the weights and prices that a source can hold (G)

| good | field | now | set | by |
|---|---|---|---|---|
| barley | value | 4 | 3 | Frankfurt 794 |
| raw_wool | value | 8 | 40 | Ine; Edgar |
| wool_cloth | value | 18 | 40 | three to five times its wool |
| fleeces | kg, value | 25, 8 | 8, 14 (or drop the row) | Ine |
| horses | value | 120 | 84 | Lex Ribuaria (?) |
| quernstones | kg | 90 | 45 | the size of hand querns (?) |
| millstones | kg | 500 | 150 | (?) |
| church_plate | kg | 1.5 | 0.4 | a chalice's weight (?) |
| ale | value | 10 | 6 | against barley |
| cattle, sheep, pigs | kg | 300, 40, 80 | 230, 30, 60 | small beasts of the time (?) |
| herring | unit, kg, keeps | barrel, 100, 270 | "thousand (dry-salted)", 120, 100 | Domesday counts |

Why: wool at a quarter of a penny a kilo makes the whole English trade worthless to carry, and querns at 90 kg
halve the Rhine's best small-boat cargo. These are the numbers that decide which voyages pay.

### 9. Set the months right, and widen the swing for food (G and R)

- G grain (`wheat`, `rye`, `barley`, `oats`, `malt`, `flour`): `dear` [5, 6, 7]; `cheap` [8, 9, 10, 11].
- G `amber`: `cheap` [4, 5, 6]; `dear` [9, 10, 11].
- G wines: `cheap` [11, 12, 1]; `dear` [3, 4, 8, 9].
- G `sea_salt`: as in change 6. G `herring.dear`: [2, 3, 4, 12]. G `wax`: `cheap` [9, 10, 11], `dear` [12, 1, 2].
  G `cattle_hides`, `leather`: `cheap` [11, 12, 1], `dear` [6, 7, 8].
- R `market`: let `cheap` and `dear` be overridden by category, and add:
  `"swing": {"Food": {"cheap": 0.7, "dear": 1.6}, "Drink": {"cheap": 0.75, "dear": 1.5}}`; keep 0.85 and 1.2 as
  the default.

Why: "buy after the harvest, sell in the hungry gap" is the first lesson of the game and the law of 806 gives its
size (2 d to 4 or 6); at 0.85 and 1.2 it does not pay the voyage, and with the gap set in March the player is
taught the wrong month.

### 10. Take out, or date, the goods the period did not trade (G)

- `sugar`: `from` 1000; remove `neustria` from `made`; remove `wanted` or set both regions to 1; note "(?)".
- `narwhal`: `from` 1100, or remove from markets and keep for a card.
- `alum`: `wanted` frisia and flanders 2 to nothing; or `from` 1050.
- `aquitaine_wine`: every strength to 1; note "(?) no text of the period shows this trade".
- `madder.made`: `{"neustria": 2, "rhineland": 1}` (no zeeland).
- `sulphur`: `from` 1050 and "(?)".
- `pepper.made`, drop `ladoga`. `tin.made`, add `england_south` 0.5 so it can be bought at all.
- `herring`: strengths before 1000 down by one step (with change 3: `england_east` 1 to 1000, 2 after).
- `sword_blades.from`: 800 to 850 (?).

Why: BRIEF rule 2 names sugar outright, and each of these puts on the stall a thing no merchant of the time could
have sold or bought there. They are small edits once change 3 is in.

---

## 6. What I could not check

- The Lex Ribuaria price list (ox, cow, horse, mare, sword, byrnie, helmet, shield, hawk) and its 12 d shilling: from
  memory; two tries to open the text failed. Every value hung on it carries (?).
- Lupus of Ferrières's letter to Aethelwulf about lead: from memory; the Wikipedia page on Lupus does not carry it.
- The capitulary texts by chapter (Herstal c. 19 and 20, Mantua 781 c. 7, Thionville 805 c. 7, Nijmegen 806, Mainz
  813 c. 51, Pîtres c. 25): from memory; only Pîtres was checked today, and only for its drift.
- The Praeceptum negotiatorum of 828 and the charters of 775 and 779 for where the tenth was taken: from memory.
- Offa's heavy penny at 1.45 g: the date is checked, the weight is from memory.
- The Corbie charter of 716: from memory.
- The tree-ring origin of Dorestad's timber: from memory, and the least sure of these.
- No period price exists for most goods; I have not judged those values.
