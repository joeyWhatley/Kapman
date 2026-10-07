# Real pictures standing in for art not yet drawn

The user, 7 Oct 2026: "I want you to use this as placeholder for Dorestad's view, and use any wiki images of ports and
places of that period instead of AI drawn stuff"; "just apply a Byzantium / PSX filter to it"; and, with pictures of
faces: "do the same for all these and then exchange them for all NPC portraits we have made by AI in the game".

## How it works

- A stand-in is a real picture put through the game's filter (Bake/art/standin_filter.py: made small, eight tones of
  Byzantium from night to cream, dots where one tone meets the next) and kept in `Art/standin/<kind>/<slot>.png`.
- The game looks for a slot's picture in `Art/final` (the user's own drawing), then `Art/standin`, then
  `Art/placeholder` (the drawn placeholder of the art list). So a drawing of the user's own always wins.
- `Art/standin` is NOT in the repository (.gitignore) and is not part of any build until each file's licence is
  cleared below. The pictures as they came are kept in `Art/standin/source`.

## What may ship, and what may not

| Marked | Meaning |
|---|---|
| SHIP WITH CREDIT | From Wikimedia Commons under a licence that allows it (public domain, CC0, CC BY, CC BY-SA): the credit line in the table must go in the game's credits; CC BY-SA also asks that the changed picture be shared alike. |
| NOT TO SHIP | Supplied by the user from somewhere else, source and rights not known or not ours: for looking at while the game is made, and to be replaced by the user's own drawing before anything is released. |

## The port views (`Art/standin/port_view`)

| Slot | Picture | By | Licence | Page | Marked |
|---|---|---|---|---|---|
| portview.dorestad | A painted reconstruction of Dorestad's harbour, supplied by the user on 7 Oct 2026 (Art/standin/source/dorestad_user_7oct.jpg) | not known (a modern reconstruction painting) | not known: taken as all rights reserved | - | NOT TO SHIP |
| portview.traiectum | Castellum Hoge Woerd.jpg | Cornutus | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Castellum_Hoge_Woerd.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.medemelacha | Kasteel Radboud te Medemblik Het Kasteel van Medenblik 1640 (titel op object), RP-P-OB-73.378.jpg | Rijksmuseum | CC0 | https://commons.wikimedia.org/wiki/File:Kasteel_Radboud_te_Medemblik_Het_Kasteel_van_Medenblik_1640_(titel_op_object),_RP-P-OB-73.378.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.wijnaldum | Terpentoer (Terpentoren) met uitzicht over de terpen van de Voorijp.jpg | Roepers | CC BY 4.0 | https://commons.wikimedia.org/wiki/File:Terpentoer_(Terpentoren)_met_uitzicht_over_de_terpen_van_de_Voorijp.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.doccinga | Oude kaart van Dokkum.jpg | Bernardus Schotanus à Sterringa | Public domain | https://commons.wikimedia.org/wiki/File:Oude_kaart_van_Dokkum.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.daventre | Deventer 1581.jpg | Braun Hogenberg | Public domain | https://commons.wikimedia.org/wiki/File:Deventer_1581.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.noviomagus | View on the waal janvangoyen.jpg | Jan van Goyen | Public domain | https://commons.wikimedia.org/wiki/File:View_on_the_waal_janvangoyen.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.quentovic | Pêche à pied estuaire de la Canche.jpg | phileole | CC BY 2.0 | https://commons.wikimedia.org/wiki/File:P%C3%AAche_%C3%A0_pied_estuaire_de_la_Canche.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.parisius | Siège de Paris par les Normands en 885, G.41053.jpg | Lacoste, Gabriel (dit l'aîné) (Toulouse, 03–10–1807), graveur | CC0 | https://commons.wikimedia.org/wiki/File:Si%C3%A8ge_de_Paris_par_les_Normands_en_885,_G.41053.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.mogontia | Schedelsche Weltchronik - Mainz.jpg | Michel Wolgemut, Wilhelm Pleydenwurff (Text: Hartmann Schedel) | Public domain | https://commons.wikimedia.org/wiki/File:Schedelsche_Weltchronik_-_Mainz.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.diusburg | DuisburgVogelschauplan1566.jpg | Unknown authorUnknown author | Public domain | https://commons.wikimedia.org/wiki/File:DuisburgVogelschauplan1566.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.hamwic | Southampton Friary.jpg | Murgatroyd49 | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Southampton_Friary.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.eoforwic | Anglian Tower York 01.jpg | Malcolmxl5 | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Anglian_Tower_York_01.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.sarre | Elmham Map Thanet All Saints St Nicholas St Giles.jpg | Nortonius | CC BY-SA 3.0 | https://commons.wikimedia.org/wiki/File:Elmham_Map_Thanet_All_Saints_St_Nicholas_St_Giles.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.dofras | Dover - St Mary sub Castro, Dover - 20200716122510.jpg | Balon Greyjoy | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Dover_-_St_Mary_sub_Castro,_Dover_-_20200716122510.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.lindisfarena | Holy Island Lindisfarne Castle.jpg | Michael D Beckwith | CC0 | https://commons.wikimedia.org/wiki/File:Holy_Island_Lindisfarne_Castle.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.bebbanburg | Bamburgh 2006 closeup.jpg | Michael Hanselmann - Quaoar10 | CC BY-SA 3.0 | https://commons.wikimedia.org/wiki/File:Bamburgh_2006_closeup.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.gyrwe | Thirlings Hall, Gearwe, Bede's World, Jarrow (6886487725).jpg | dun_deagh | CC BY-SA 2.0 | https://commons.wikimedia.org/wiki/File:Thirlings_Hall,_Gearwe,_Bede%27s_World,_Jarrow_(6886487725).jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.streonaeshalch | 'Whitby' RMG PW2107.jpg | William Lionel Wyllie | Public domain | https://commons.wikimedia.org/wiki/File:%27Whitby%27_RMG_PW2107.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.dommoc | The beach at Dunwich, Suffolk - geograph.org.uk - 6148263.jpg | Mat Fascione | CC BY-SA 2.0 | https://commons.wikimedia.org/wiki/File:The_beach_at_Dunwich,_Suffolk_-_geograph.org.uk_-_6148263.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.legacaestir | The Roman Gardens and City Walls, Chester - geograph.org.uk - 5723149.jpg | Jeff Buck | CC BY-SA 2.0 | https://commons.wikimedia.org/wiki/File:The_Roman_Gardens_and_City_Walls,_Chester_-_geograph.org.uk_-_5723149.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.hi | Iona Cathedral, Iona, Scotland, ca. 1899.jpg | Photoglob Zürich, reprinted by Detroit Publishing Co. | Public domain | https://commons.wikimedia.org/wiki/File:Iona_Cathedral,_Iona,_Scotland,_ca._1899.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.ripa | Ribe VikingeCenter - cowshed.jpg | Västgöten | CC BY-SA 3.0 | https://commons.wikimedia.org/wiki/File:Ribe_VikingeCenter_-_cowshed.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.sliesthorp | Busdorf - Haithabu - Wikinger-Häuser 05 ies.jpg | Frank Vincentz | CC BY-SA 3.0 | https://commons.wikimedia.org/wiki/File:Busdorf_-_Haithabu_-_Wikinger-H%C3%A4user_05_ies.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.birka | Plastic reconstruction Birka4.jpg | Selfoss | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Plastic_reconstruction_Birka4.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.helgo | Helgö - KMB - 16000700014579.jpg | Jan Norrman | CC BY 2.5 | https://commons.wikimedia.org/wiki/File:Helg%C3%B6_-_KMB_-_16000700014579.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.uppsalir | Gamla Uppsala högar (2019)-1.jpg | Ruthven | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Gamla_Uppsala_h%C3%B6gar_(2019)-1.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.paviken | Västergarn - Paviken 01.jpg | Jan Norrman | CC BY 2.5 se | https://commons.wikimedia.org/wiki/File:V%C3%A4stergarn_-_Paviken_01.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.ahus | Åhus modell.jpg | Zhian giorges | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:%C3%85hus_modell.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.truso | 02016 Visualization of Truso Settlement, bay.jpg | Muzeum Archeologiczno-Historyczne w Elblągu | CC BY 3.0 | https://commons.wikimedia.org/wiki/File:02016_Visualization_of_Truso_Settlement,_bay.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.grobin | Grobinas pilskalns.jpg | Gatis Kalniņš | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Grobinas_pilskalns.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.aldeigjuborg | Крепость Старая Ладога с высоты птичьего полета 01.jpg | Караванов Лев | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:%D0%9A%D1%80%D0%B5%D0%BF%D0%BE%D1%81%D1%82%D1%8C_%D0%A1%D1%82%D0%B0%D1%80%D0%B0%D1%8F_%D0%9B%D0%B0%D0%B4%D0%BE%D0%B3%D0%B0_%D1%81_%D0%B2%D1%8B%D1%81%D0%BE%D1%82%D1%8B_%D0%BF%D1%82%D0%B8%D1%87%D1%8C%D0%B5%D0%B3%D0%BE_%D0%BF%D0%BE%D0%BB%D0%B5%D1%82%D0%B0_01.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.ogvaldsnes | The Viking Farm, Avaldsnes 05.2010 - panoramio (1).jpg | Mark Voigt | CC BY 3.0 | https://commons.wikimedia.org/wiki/File:The_Viking_Farm,_Avaldsnes_05.2010_-_panoramio_(1).jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.hleidr | Land of Legends Lejre 05.jpg | Sintakso | CC0 | https://commons.wikimedia.org/wiki/File:Land_of_Legends_Lejre_05.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.herio | Noirmoutier en l'Ile - Vue générale.jpg | Jean-Christophe BENOIST | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Noirmoutier_en_l%27Ile_-_Vue_g%C3%A9n%C3%A9rale.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| portview.burdigala | Wayfaring in France, from Auvergne to the Bay of Biscay (1913) (14579692237).jpg | Internet Archive Book Images | No restrictions | https://commons.wikimedia.org/wiki/File:Wayfaring_in_France,_from_Auvergne_to_the_Bay_of_Biscay_(1913)_(14579692237).jpg | SHIP WITH CREDIT (to be checked on its page before release) |

From Wikimedia Commons, found by Bake/art/standin_search.py on 7 Oct 2026 and chosen by me from small copies (the choices:
Bake/art/standin_choices.json). The user then called the searching off ("maybe forget about searching for images, you
don't seem to be very good at it"): no more are looked for; these stand until the user keeps or bins them. Thirteen
ports have none (nothing fitting was found): they show the drawn placeholder.

## The portraits (`Art/standin/portrait`, from `Art/standin/portrait_pool`)

Twelve pictures supplied by the user on 7 Oct 2026 (Art/standin/source/people/p01 to p12). Eleven are photographs of
real people in costume, by the look of them from films or television; the twelfth is an old engraving of a crowned
king. Bake/art/standin_portraits.py cuts each to head and shoulders, filters it, and gives every portrait slot of
somebody met (person.*, role.*, weight.*: 573 slots; not the player's own, self.*) one of them: a woman's for a woman
(2 pictures), a man's for a man (9), the same one every time for the same slot.

| Pool | What | Given by hand to | Marked |
|---|---|---|---|
| p01 to p11 | photographs of real people, source and rights not known | p04: Ravn (person.cast.crew.ravn); p03: the lender (person.lender); the rest by sex and by the slot's name | NOT TO SHIP (other people's photographs, and real people's faces) |
| p12 | an old engraving of a crowned king | Alfred (the user: "use that second one for Alfred"): person.fordwic.alfred, and any slot named Alfred | NOT TO SHIP until its source is found (an engraving of that age is most likely in the public domain, but that is not checked) |

Ravn and the lender were given theirs by me, not by the user: to be changed at a word.

### Pictures of the very person (`Art/standin/portrait`, over the pool's face)

From Wikimedia Commons, found by Bake/art/standin_people.py on 7 Oct 2026 (their own coins, a mosaic, manuscripts,
statues, old engravings), cut to the head and filtered. The searching was called off the same evening; these fourteen
stand until the user keeps or bins them (Bake/art/standin_people_choices.json: a slot set to null goes back to the pool).

| Slot | Picture | By | Licence | Page | Marked |
|---|---|---|---|---|---|
| weight.charles | Charlemagne-by-Durer.jpg | Albrecht Dürer | Public domain | https://commons.wikimedia.org/wiki/File:Charlemagne-by-Durer.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| weight.offa | Offa, King of Mercia, silver penny; (obverse).png | Offa (07..-0796 ; roi de Mercie). Autorité émettrice de monnaie | Public domain | https://commons.wikimedia.org/wiki/File:Offa,_King_of_Mercia,_silver_penny;_(obverse).png | SHIP WITH CREDIT (to be checked on its page before release) |
| weight.cynethryth | Cynethryth penny.jpg | Classical Numismatic Group (CNG), Inc., www.cngcoins.com | CC BY-SA 3.0 | https://commons.wikimedia.org/wiki/File:Cynethryth_penny.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| weight.cenwulf | Coenwulf, King of Mercia, gold mancus; struck 796–821 AD (obverse).png | Scan: British Museum | Public domain | https://commons.wikimedia.org/wiki/File:Coenwulf,_King_of_Mercia,_gold_mancus;_struck_796%E2%80%93821_AD_(obverse).png | SHIP WITH CREDIT (to be checked on its page before release) |
| weight.ecgberht | Egbert of Wessex in the Brief Abridgement of the Chronicles of England.jpg | Matthew Paris | Public domain | https://commons.wikimedia.org/wiki/File:Egbert_of_Wessex_in_the_Brief_Abridgement_of_the_Chronicles_of_England.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| weight.leo | Pope Leo III portrait – Triclinium Leoninum.jpg | Various: Original: Unknown authorUnknown author Restoration: Ferdinando Fuga (1743) | Public domain | https://commons.wikimedia.org/wiki/File:Pope_Leo_III_portrait_%E2%80%93_Triclinium_Leoninum.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| weight.angilbert | Saint-Riquier, Abbaye de Saint-Riquier 31.JPG | Mattis | CC BY 2.0 | https://commons.wikimedia.org/wiki/File:Saint-Riquier,_Abbaye_de_Saint-Riquier_31.JPG | SHIP WITH CREDIT (to be checked on its page before release) |
| weight.sigfred | Syvard II - Sigfred (Danish king 812).jpg | Erico Olai Tormio (publisher) | Public domain | https://commons.wikimedia.org/wiki/File:Syvard_II_-_Sigfred_(Danish_king_812).jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| person.cast.anskar | Bornemann, Hans - Ansgar - 1457 - detail.jpg | Hans Bornemann | CC0 | https://commons.wikimedia.org/wiki/File:Bornemann,_Hans_-_Ansgar_-_1457_-_detail.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| person.cast.godfred | Kong Gøtrik den Gavmilde.jpg | Erico Olai Tormio (publisher) | Public domain | https://commons.wikimedia.org/wiki/File:Kong_G%C3%B8trik_den_Gavmilde.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| person.cast.horik | Kong Erik II Barn (Horik II).jpg | Erico Olai Tormio (publisher) | Public domain | https://commons.wikimedia.org/wiki/File:Kong_Erik_II_Barn_(Horik_II).jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| person.cast.rollo | Rollo of Normandy statue Rouen f.jpg | Man vyi | Public domain | https://commons.wikimedia.org/wiki/File:Rollo_of_Normandy_statue_Rouen_f.jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| person.cast.thorkell | Thorkild-den-Høje(cropped).jpg | Ole Ryhl Olsson | Public domain | https://commons.wikimedia.org/wiki/File:Thorkild-den-H%C3%B8je(cropped).jpg | SHIP WITH CREDIT (to be checked on its page before release) |
| person.cast.aethelheard | Aethelhard.2.gif | DarnFineCoffee | CC BY-SA 4.0 | https://commons.wikimedia.org/wiki/File:Aethelhard.2.gif | SHIP WITH CREDIT (to be checked on its page before release) |
