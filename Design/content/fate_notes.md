<!-- Written 7 Oct 2026 by the agent that turned Design/fate_faith_and_self.md into data. One page: what was wrong or doubtful there, what was changed, and the source. The data is in Assets/Kapman/Game/Data/story: self_793.json, calendar_793.json, hours_793.json, weight_793.json, rare_793.json. -->

# Fate notes: what was checked, and what changed

**How it was checked.** Against the project's own research first (Design/content realms, peoples, lore, dossiers, spine.json, chronicle_793.json), then against named pages: the NASA catalogs of solar and lunar eclipses for 701 to 900, and Wikipedia for Aethelberht II of East Anglia, Aethelhard, Abul-Abbas, Dicuil, Lex Frisionum, Halley's Comet and Eldgja. The session's web searches were already used up, so nothing was searched: what could not be fetched is marked (?) in the data.

## Wrong, and changed

| The page said | What is so | Source |
|---|---|---|
| Hours "on or near the twelve ports": York, the Tyne, East Anglia, the Irish Sea, the Rhine at Aachen, the Elbe | The book's twelve are Dorestat, Traiectum, Walichrum, Lundenwic, Witla, Medemelacha, Doccinga, Daventre, Quentovic, Sarre, Fordwic, Ripa. Every hour now has a `reach`: 14 can be met in the twelve, 1 needs dim water (the Tyne), 2 come as news (Thule, the elephant). | GAME_MAKER.md stages 1 and 2 |
| The old gods "banned by Frankish law since 785: keep it quiet south of the Eider" | The law of about 782 to 785 binds Saxons (death for refusing baptism, burning the dead, breaking Lent in contempt), and was softened on 28 October 797. A Frisian or Saxon heathen is the king's subject breaking his law; a Dane is a foreign heathen who may trade but not offer in sight, swear or be buried. | Capitulatio de partibus Saxoniae c. 4, 7, 8, 21; Capitulare Saxonicum |
| "A Jew of the Rhineland" with "a letter of the emperor's protection" | No emperor before Christmas 800; the letters are Louis the Pious's, about 825. Jews at Cologne are in a law of 321 and then not until the 11th century. Ibn Khordadbeh wrote about 847 and 885, not 870. No Jews are recorded in England before 1066. Now: "a merchant of the palace", with a palace officer's letter. | ARF 801-802; Formulae imperiales 30, 31, 52; Capit. of Nijmegen 806 c. 4 |
| The steersman's child: "Sea 3" | There is no Sea among the five stats. Now Nerve 2, Lore 2 and a flag that lifts seamanship checks. | section 2 of the page itself |
| 829, Birka: the lot cast at the thing | The lot and the old man's speech are the second visit, about 852. In 829 to 831 the king asked his men and let the monks preach. | Rimbert c. 9-11, 26-27 |
| 921, the Volga | 922: the embassy left Baghdad on 21 June 921 and arrived on 12 May 922. | Ibn Fadlan |
| The raven-finder, "about 825 to 874" | About 860 to 874. 825 is the year Dicuil wrote of Irish hermits in the north. | Landnamabok (late); Ari; Dicuil VII |
| An organ built "by a Greek, 826" | By George, a priest of Venice, at Aachen. The Greek emperor's organ came in 757. | ARF 826, 757 |
| "Two darkenings of the sun in 810" | The annals list 7 June and 30 November; only the November one could be seen from Francia (?). England saw one on Monday 16 July 809. **No eclipse of the sun was visible from the North Sea between 793 and 805.** | ARF 810; ASC 809; NASA |
| Danes wash on Saturday: "an English monk complained" | A 13th-century chronicle (under John of Wallingford's name) about the years before 1002. Kept as a custom, marked late. | calendar_793.json `washing_day` |
| "A skald who will make a verse on you" | Skalds belong to the next century's Norse courts. For 793 the singer is real: Bernlef, the blind Frisian, and the English scop. | Altfrid I.25 |
| "A drowned bell", "a king in disguise in his own hall", the seeress's cat-skin hood | All later: a seamen's tale with no period source; William of Malmesbury, about 1125; Eiriks saga, 13th century. Each card is rewritten to what the period gives (a bell and two opinions; a king hiding in the fens in 878; a woman with an iron staff, as in the graves). | rare_793.json |
| The chronicle's moon eclipse of 20 December 802 | No eclipse that night. The Chronicle's "xiii Kal. Ian." is almost surely a slip for "Iun.": the total eclipse before dawn on 21 May 802. **chronicle_793.json should move `802-12-20-lunar-eclipse` to 802-05-21.** (Not edited: it is not this task's file.) | ASC (A) 802; NASA |

## Doubtful, and marked

- **Aethelberht, 794.** The Chronicle has one line. The day (20 May) is his feast; the place, the wedding and the queen's part are from a Life of about 1100. The "servant of the queen" is kept as an invented rumour and says so. That his road passed Lundenwic is a guess.
- **Jarrow, 794.** The source says "Donemuthe"; Jarrow is a 12th-century reading. No one there is named.
- **The elephant.** It reached Aachen on Wednesday 20 July 802 and died in 810 at Lippeham on the Rhine. Its road north of the Alps is not recorded, so "seen from the river" is only news in the first release.
- **Prime-signing** is Rimbert's, of the 850s: for 793 it is carried back sixty years. **Al-Ghazal's embassy** rests on one writer who died in 1235. **The horse broth at Lade** is Snorri, about 1230.
- **Old Frisian.** "Nothing written before the thirteenth century" leaves out a few short runic inscriptions of before 900 (?), not checked. The rule on the page stands.
- **The Vineyard palette** in MapPalette.cs does not hold the exact #702963 (it has #5a2970 and #70293f).
- **Paris, 886:** the count's small boat is not in Abbo. **1064:** the hidden relics are Wace's, about 1170.
- **Invented in a real frame, and flagged as such:** every ghost of the eight pasts; the debts of the seven peoples other than the Frisian; the characters of Hamacar, Wlemar, Godescalc and Heathoberht, of whom only names and dates survive; where Alcuin, Aethelberht, Eadburh, Ecgberht and Godescalc are met.

## Found right

Alcuin's letter about the boys (no. 20). Lindisfarne on 8 June 793, a Saturday. 151 days from 1 May to Michaelmas. The comet of 837 in Easter week; the eclipse of Wednesday 5 May 840 and the emperor dead on 20 June; the red sun of 939 (Eldgja, 939 to 940); the eclipse of 31 August 1030; the star from 24 April 1066. Maldon, 10 or 11 August 991. Saint Brice's day, Friday 13 November 1002. Twenty-five ships and fourteen.

## Added

- **Easter for 793 to 806**, reckoned by the Julian computus (the formula is in calendar_793.json) and checked against the annals; and the **Jewish year** for the same years by the fixed calendar.
- **Nine hours beyond the page's table** (two of them, the news of Offa's death and Kent laid waste, are its Kent row made into hours), all in the twelve ports: the Frisian rising at Doccinga (793), the news of Offa's death (796), Kent laid waste (798), the king's envoy at Ripa (798), the king on the Channel coast at Easter 800, the archbishop's crossing (801), the queen with her chests and the exile going home (802), the writing of the Frisians' law (802).
- **Four moon eclipses the annals record** (28 March 796, 15 January 800, 21 May 802, 1 September 806) and four more that happened and were not written down.
- **35 people of weight and 33 pillars.** Pillars cover the chronicle's entries that touch the twelve ports and their realms, and every hour; the other 200 or so entries of 793 to 805 stand by one plain rule.

## For whoever loads it

- `me.tongue`, `me.name`, `me.nerve`, `me.lore`, `me.troth` (numbers) and `me.people`, `me.faith`, `me.past` (text) must be added to VarNames in Cond.cs. Every condition in the five files uses only those and the names the engine already has; every effect parses by today's grammar (both checked by script).
- No verb changes a stat yet: stat changes sit beside the effects as `stats`. A verb `me <stat> +n` would carry them.
- Conditions know the year and month, not the day: a dated card has a `fire` day for the scheduler.
- In hours_793.json a failed check's cost is in words under `fails`. Two Turns are written; the second (Ecgberht) is marked held. A third way to fall (the archbishop lost at sea, in weight_793.json) counts as the life's one Turn.
- A field named `when` or `where` is a condition only where the file's `about` says so; in the calendar's heathen and fair rows and in the list of later hours it is plain words.
- Still for the user to rule: whether the prime-signed and the Jewish merchant are offered in the first release (both are written).
