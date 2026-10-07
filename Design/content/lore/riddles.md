# Lore: riddles

Sixty riddles a player could be set, each with its English words, who put them into English and when, the answer,
other answers to take, where it is from, and who in the game's world would ask it. The same sixty are in
`riddles.json` beside this file, as data. Written 6 Oct 2026. A draft for the user to overwrite.

Read this with `Assets/Kapman/Game/Data/riddles_793.json` (the riddles the game already holds) and
`texts_793.json` (their original words). Section 3 compares the two.

## 1. What is here

| Source | Made | Riddles here | English by | Fits the quoting rule (BRIEF.md rule 4)? |
|---|---|---|---|---|
| Exeter Book (Old English) | copied c. 965-975; made 8th-10th c. | 22 | Benjamin Thorpe 1842 (12); Herbert B. Brougham in Cook and Tinker 1902 (5); Stopford Brooke 1892 (2); the game's own draft (3) | Thorpe (d. 1870) yes. Brooke (d. 1916) yes. Brougham: printed 1902, death year not found (?) |
| Riddles of Gestumblindi, Hervarar saga (Old Norse) | saga 13th c.; riddles older (?) | 12 | Nora Kershaw 1921 | **No.** Printed 1921, but Kershaw (Chadwick) died in 1972. See 2.2 |
| Symphosius (Latin) | late 4th-5th c. | 8 | Elizabeth Hickman du Bois 1912 | Printed 1912; death year not found (?) |
| Aldhelm (Latin) | c. 685-695 | 6 | James Hall Pitman 1925 | Printed 1925; death year not found (?) |
| Alcuin, Disputatio Pippini (Latin) | c. 790-800 | 6 | Andrew Fleming West 1892 (3); the game's own draft (3) | West (d. 1943) yes |
| Tatwine (Latin) | before 734 | 3 | the game's own draft | own words: free |
| Eusebius (Latin) | first half of 8th c. | 3 | the game's own draft | own words: free |

Every answer is the one the source gives (the Latin riddles carry their answers as titles; King Heithrek answers
each of Gestumblindi's in the saga; Alcuin answers Pippin) or, for the Exeter Book, which gives no answers, the
one scholars had agreed by Tupper's edition of 1910. Where no answer is agreed (the little black birds, the
poker) every serious guess is in `accept`.

## 2. Four things found while doing this

### 2.1 Tupper 1910 has no English

BRIEF.md rule 4 lists "the Exeter riddles (Tupper 1910)" as safe to quote. Frederick Tupper's *The Riddles of the
Exeter Book* (Boston: Ginn, 1910) is an edition: the Old English text, notes and a glossary. It has no
translation. It is the right book for the Old English words and for the answers, and it is used for both here and
in `texts_793.json`. The English has to come from somewhere else:

- **Benjamin Thorpe, Codex Exoniensis (1842).** The whole book, line for line. Plain, stiff, and wrong in places.
  Already the game's source for 31 riddles. Thorpe left every coarse riddle in Old English only.
- **Herbert B. Brougham, in Cook and Tinker, Select Translations from Old English Poetry (1902), pp. 70-75.** Eight
  riddles in blank verse: storm on land, storm at sea, swan, horn, bow, book, mead, and one he calls the falcon.
  Readable and mostly right. The best of the old ones.
- **Stopford Brooke, The History of Early English Literature (1892).** Many of the riddles, whole or in part, in a
  long alliterating line. Lively but heavily old-fashioned ("lovelily adorned", "battle-brattling"). Used here only
  twice, where he is the best there is.
- Not usable under the rule: Mackie 1934 (printed too late), Gordon 1926 (d. 1973), Baum 1963, and every modern one.

### 2.2 Kershaw 1921 is the plainest Gestumblindi, and breaks the rule

The task named Kershaw 1921, and her prose is by far the plainest English of the Norse riddles: it sounds like the
game. It is used for all twelve here. But rule 4 asks for a translator who died before 1956, and Nora Kershaw
(Mrs Chadwick) died in 1972. Her book is out of copyright in the United States (printed before 1931) and is not
in the United Kingdom or the European Union until 2043. The game's own files already hold the same riddles in the
translation of Vigfusson (d. 1889) and York Powell (d. 1904), *Corpus Poeticum Boreale* (1883), which passes the
rule on both counts. **Recommendation: ship the 1883 words (they are in `texts_793.json` already, ids t-gest-NN),
or the user's own; use Kershaw as the model for tone.** Each Kershaw entry below names the game's id for the same
riddle (`in_game`).

### 2.3 The riddles with a double meaning have never been put into English by anyone who can be quoted

The Exeter Book's coarse riddles are what the brief means by "riddles with a double meaning (the Exeter Book's)".
Thorpe printed them in Old English and skipped the English; Brooke passed over them; Tupper discusses them in
notes; Kershaw cut the one coarse riddle in the saga ("I saw a horse standing...", the loom, the game's
r-gest-30) down to its first five words. So the onion, the key and the dough are given here in **the game's own
draft from the Old English**, marked as the game's files already mark such drafts. One coarse riddle did slip
through Thorpe (the poker, no. 62) and is quoted from him.

### 2.4 Some of Thorpe's English spoils the riddle

Three of the riddles the game holds in Thorpe's words cannot be solved from them, because he misread the line
that carries the clue:

- **Exeter 26, the book.** Thorpe: "Separately ground my fingers the earth, and me foul swine, amid the bounteous
  drops, sought out busily." The Old English says that fingers folded the skin and the bird's joy (the quill)
  went over it scattering drops. Brougham has it right. Use Brougham (entry 10).
- **Exeter 68-69, ice.** Thorpe: "water was turn'd to poison". The Old English is *to bane*: to bone. Change one
  word (entry 20).
- **Exeter 10, the barnacle goose.** Thorpe: "heavy only to sailors in ships with my body". The Old English says
  the creature clung with its body to a drifting piece of wood. Brooke has it (entry 3).

Smaller slips are noted on the entries (the anchor's middle lines; the swan, where Brougham is better).

## 3. Compared with riddles_793.json

The game's file holds 137 riddle entries. By source:

| Source | In the game | Here | Here and in the game | New here |
|---|---|---|---|---|
| Gestumblindi (Hervarar saga) | 38 | 12 | 12 | 0 |
| Vafthrudnismal | 21 | 0 | 0 | 0 |
| Alvissmal | 13 | 0 | 0 | 0 |
| Exeter Book | 32 | 22 | 16 | 6 |
| Aldhelm | 12 | 6 | 6 | 0 |
| Symphosius | 12 | 8 | 7 | 1 |
| Tatwine | 5 | 3 | 3 | 0 |
| Eusebius | 4 | 3 | 3 | 0 |
| Alcuin, Disputatio Pippini | 0 | 6 | 0 | 6 |

The game's Gestumblindi count includes the four halves of riddle 7 as four entries. Vafthrudnismal and Alvissmal are wisdom contests, not riddles a player guesses; they were not part of this task.

Entry by entry:

| No. | Here | Answer | The game's id | The game's English now | English here |
|---|---|---|---|---|---|
| 1 | lore.exeter.07 | A swan | riddle_exeter_07_swan | Benjamin Thorpe 1842 | Herbert B. Brougham 1902 |
| 2 | lore.exeter.08 | A nightingale | (not in the game) | - | Stopford A. Brooke 1892 |
| 3 | lore.exeter.10 | A barnacle goose | riddle_exeter_10_barnacle_goose | Benjamin Thorpe 1842 | Stopford A. Brooke 1892 |
| 4 | lore.exeter.12 | An ox, and the leather made of its hide | riddle_exeter_12_ox | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 5 | lore.exeter.14 | A horn (an ox's horn: war-horn and drinking-horn) | riddle_exeter_14_horn | Benjamin Thorpe 1842 | Herbert B. Brougham 1902 |
| 6 | lore.exeter.16 | An anchor | riddle_exeter_16_anchor | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 7 | lore.exeter.21 | A plough | riddle_exeter_21_plough | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 8 | lore.exeter.23 | A bow | riddle_exeter_23_bow | Benjamin Thorpe 1842 | Herbert B. Brougham 1902 |
| 9 | lore.exeter.25 | An onion | riddle_exeter_25_onion | none (original only) | the game's own draft 2026 |
| 10 | lore.exeter.26 | A book: a Gospel book or Bible | riddle_exeter_26_book | Benjamin Thorpe 1842 | Herbert B. Brougham 1902 |
| 11 | lore.exeter.27 | Mead | riddle_exeter_27_mead | Benjamin Thorpe 1842 | Herbert B. Brougham 1902 |
| 12 | lore.exeter.32 | A ship: a merchant ship | riddle_exeter_32_ship | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 13 | lore.exeter.33 | An iceberg, or floating ice | riddle_exeter_33_iceberg | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 14 | lore.exeter.44 | A key | (not in the game) | - | the game's own draft 2026 |
| 15 | lore.exeter.45 | Dough | (not in the game) | - | the game's own draft 2026 |
| 16 | lore.exeter.46 | Lot, his two daughters and their two sons | (not in the game) | - | Benjamin Thorpe 1842 |
| 17 | lore.exeter.47 | A bookworm (a book-moth) | riddle_exeter_47_bookworm | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 18 | lore.exeter.57 | Swallows (or swifts, or starlings) | (not in the game) | - | Benjamin Thorpe 1842 |
| 19 | lore.exeter.62 | A poker, or a smith's burning iron (a borer) | (not in the game) | - | Benjamin Thorpe 1842 |
| 20 | lore.exeter.69 | Ice | riddle_exeter_68_69_ice | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 21 | lore.exeter.85 | A fish in a river | riddle_exeter_85_fish_river | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 22 | lore.exeter.86 | A one-eyed man selling garlic (or onions) | riddle_exeter_86_garlic_seller | Benjamin Thorpe 1842 | Benjamin Thorpe 1842 |
| 23 | lore.gest.01 | Ale | r-gest-01 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 24 | lore.gest.02 | A bridge over a river (with birds overhead and fish below) | r-gest-02 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 25 | lore.gest.03 | Dew | r-gest-03 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 26 | lore.gest.05 | Fog | r-gest-05 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 27 | lore.gest.06 | An anchor (with its cable) | r-gest-06 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 28 | lore.gest.07 | A raven; dew; a fish; a waterfall | r-gest-07a, r-gest-07b, r-gest-07c, r-gest-07d | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 29 | lore.gest.08 | A leek | r-gest-08 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 30 | lore.gest.12 | A sow with nine piglets inside her | r-gest-12 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 31 | lore.gest.14 | A spider | r-gest-14 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 32 | lore.gest.21 | The waves (Aegir's daughters) | r-gest-21 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 33 | lore.gest.24 | A dead snake on a dead horse on an ice-floe, floating down a river | r-gest-24 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 34 | lore.gest.28 | A cow | r-gest-28 | Vigfusson and Powell 1883 | Nora Kershaw (later Chadwick) 1921 |
| 35 | lore.symph.04 | A key | riddle_symphosius_04_clavis | none (original only) | Elizabeth Hickman du Bois 1912 |
| 36 | lore.symph.10 | Ice | riddle_symphosius_10_glacies | none (original only) | Elizabeth Hickman du Bois 1912 |
| 37 | lore.symph.11 | Snow | riddle_symphosius_11_nix | none (original only) | Elizabeth Hickman du Bois 1912 |
| 38 | lore.symph.12 | A river and its fish | riddle_symphosius_12_flumen | none (original only) | Elizabeth Hickman du Bois 1912 |
| 39 | lore.symph.13 | A ship | riddle_symphosius_13_navis | none (original only) | Elizabeth Hickman du Bois 1912 |
| 40 | lore.symph.61 | An anchor | riddle_symphosius_61_ancora | none (original only) | Elizabeth Hickman du Bois 1912 |
| 41 | lore.symph.91 | Money (coin) | riddle_symphosius_91_pecunia | none (original only) | Elizabeth Hickman du Bois 1912 |
| 42 | lore.symph.99 | Sleep | (not in the game) | - | Elizabeth Hickman du Bois 1912 |
| 43 | lore.aldhelm.02 | The wind | riddle_aldhelm_02_ventus | none (original only) | James Hall Pitman 1925 |
| 44 | lore.aldhelm.19 | Salt | riddle_aldhelm_19_salis | none (original only) | James Hall Pitman 1925 |
| 45 | lore.aldhelm.40 | Pepper | riddle_aldhelm_40_piper | none (original only) | James Hall Pitman 1925 |
| 46 | lore.aldhelm.80 | A glass cup | riddle_aldhelm_80_calix_vitreus | none (original only) | James Hall Pitman 1925 |
| 47 | lore.aldhelm.89 | A book chest | riddle_aldhelm_89_arca_libraria | none (original only) | James Hall Pitman 1925 |
| 48 | lore.aldhelm.92 | A lighthouse | riddle_aldhelm_92_farus | none (original only) | James Hall Pitman 1925 |
| 49 | lore.alcuin.snow | Dry water. | (not in the game) | - | Andrew Fleming West 1892 |
| 50 | lore.alcuin.frost | The persecutor of plants; the destruction of leaves; the bond of the earth; the source of waters. | (not in the game) | - | Andrew Fleming West 1892 |
| 51 | lore.alcuin.apple | An apple. | (not in the game) | - | Andrew Fleming West 1892 |
| 52 | lore.alcuin.fire | Fire, from wood rubbed on wood | (not in the game) | - | the game's own draft 2026 |
| 53 | lore.alcuin.dream | A dream | (not in the game) | - | the game's own draft 2026 |
| 54 | lore.alcuin.three | Adam, Elijah (or Enoch) and Lazarus | (not in the game) | - | the game's own draft 2026 |
| 55 | lore.tatwine.05 | Parchment | riddle_tatwine_05_membranis | none (original only) | the game's own draft 2026 |
| 56 | lore.tatwine.15 | Snow, hail and ice | riddle_tatwine_15_nive_glacie | none (original only) | the game's own draft 2026 |
| 57 | lore.tatwine.39 | A whetstone | riddle_tatwine_39_coticulo | none (original only) | the game's own draft 2026 |
| 58 | lore.eusebius.23 | The sea | riddle_eusebius_23_aequore | none (original only) | the game's own draft 2026 |
| 59 | lore.eusebius.31 | Wax (the wax tablet and the candle) | riddle_eusebius_31_cera | none (original only) | the game's own draft 2026 |
| 60 | lore.eusebius.40 | A fish | riddle_eusebius_40_pisce | none (original only) | the game's own draft 2026 |

What this file adds to what the game has:

1. **English for the Latin riddles.** The game holds 33 Latin riddles (Aldhelm 12, Symphosius 12, Tatwine 5,
   Eusebius 4) with their Latin words and no English at all, so a player cannot read them. Here: du Bois 1912 for
   seven of the game's Symphosius, Pitman 1925 for six of its Aldhelm, and the game's own drafts for three Tatwine
   and three Eusebius. Du Bois translated all hundred of Symphosius and Pitman all hundred of Aldhelm: the other
   five and six of the game's can be had from the same two books.
2. **English for the onion** (Exeter 25), which the game holds in Old English only.
3. **Better English** for the swan, horn, bow, book and mead (Brougham) and the barnacle goose (Brooke).
4. **Thirteen riddles the game does not have:** Exeter 8 (nightingale), 44 (key), 45 (dough), 46 (Lot's family),
   57 (the little black birds), 62 (poker); Symphosius 99 (sleep); and six from Alcuin's Disputatio, of which the game's
   riddle file has nothing (the sayings writer has drafted some of its question pairs as sayings, `say.alc.*`).
5. **Differences in the answers** between Kershaw 1921 and the game's 1883 answers for Gestumblindi, none of them
   in the sixty here but worth knowing:
   - r-gest-19 and r-gest-25: Kershaw says "a game of chess" and "the Hunn in chess". Chess did not reach the north
     before the 11th or 12th century (?). The game's "hnef-tafl" is right; keep it.
   - r-gest-20: the game's 1883 answer is "Snow-flakes and rain [the text gives Rep-hens (!)]"; Kershaw follows the
     saga's own answer, ptarmigan (white in winter, dark in summer). The saga's answer is the better one, and the
     game's palette already has a ptarmigan.
   - r-gest-24: the 1883 answer is "a dead horse floated on an icefloe"; the saga as Kershaw gives it adds a dead
     snake on the horse, which is what makes "the blind riding the blind" work.
   - r-gest-30 (the loom): Kershaw leaves the riddle out as unfit to print. It is the saga's one riddle with a
     double meaning.
   - Gestumblindi's last question, what Odin said in Balder's ear, is not a riddle and has no answer; it should
     never be set to a player as one.
6. **Who asks.** Every entry names the peoples (`people`, by the ids of `Design/content/peoples/`) and the kind of
   person (`who`) who would ask it. The rule of thumb: Old English riddles from the English; Latin ones from anyone
   schooled by the church (English, Frankish, Irish; a Frisian clerk at a toll); the Norse ones from Northmen,
   Danes and Svear. A Latin riddle is asked in Latin and needs the player to have that tongue or an interpreter.

## 4. Double meanings and word-play

Marked `"double": true` in the data. A double meaning here is a riddle built to make the hearer think of something coarse while the true answer is a household thing: the period's own kind of joke.

- **4. The ox and its hide** (Exeter Book riddle 12 (Krapp-Dobbie; Tupper 13)): In part. The first half is plain (bonds, a leather bottle, shoes); the serving woman working the hide by night is told so as to sound like something else.
- **9. The onion** (Exeter Book riddle 25 (Krapp-Dobbie; Tupper 26)): The best known of the Exeter Book's riddles with two answers: every line fits a man's body until the last, and the plain answer is in the kitchen.
- **14. The key** (Exeter Book riddle 44 (Krapp-Dobbie; Tupper 45)): Meant to be heard wrongly. The plain answer hangs at every householder's belt.
- **15. The dough** (Exeter Book riddle 45 (Krapp-Dobbie; Tupper 46)): Five lines, one joke: the second meaning is plain, the true answer is bread rising under a cloth.
- **19. The poker** (Exeter Book riddle 62 (Krapp-Dobbie; Tupper 63)): The whole of it. Thorpe put this one into English where he left the onion and the key alone, perhaps because he did not see it.
- **46. Calix vitreus** (Aldhelm, Enigmata 80): In part, and gently: the bishop lets the cup talk like a woman who is held, kissed and brings men down. The Exeter Book's glass beaker (no. 63) takes the same joke further.
- Not among the sixty: the loom riddle of Gestumblindi (the game's r-gest-30), which Kershaw would not print; and the Exeter Book's churn (54), bellows (37), shirt or helmet (61) and glass beaker (63), which Thorpe either skipped or blurred.

Word-play of another kind (marked `"wordplay"` in the data):

- **8. The bow**: The answer is hidden in the first line: the manuscript has AGOF, which is BOGA (bow) backwards with one letter changed by a later scribe.
- **57. De coticulo**: A pun in Latin: cos is a whetstone, and Cos is the Greek island city. In English the second line is only a hint that the answer is a word with two meanings.

How a card might use one: the asker says the riddle; the choices are the plain answer, the coarse answer, and
silence. The coarse answer is always wrong, and gets a laugh at the player's cost ("You have a mind like a
byre, Frisian. It is an onion."). A monk never asks one of these; a monk may well know them (the Exeter Book
was a bishop's gift to his cathedral).

## 5. The sixty

Line breaks are the translator's where he wrote verse. Thorpe printed short half-lines facing the Old English;
his are run on here as prose, with his words and stops unchanged. A star in a Thorpe text is his own mark for
words he could not read. `(?)` marks anything not checked against the page.

### The Exeter Book

#### 1. The swan

> My robe is noiseless while I tread the earth,  
> Or tarry 'neath the banks, or stir the shallows;  
> But when these shining wings, this depth of air,  
> Bear me aloft above the bending shores  
> Where men abide, and far the welkin's strength  
> Over the multitudes conveys me, then  
> With rushing whir and clear melodious sound  
> My raiment sings. And like a wandering spirit  
> I float unweariedly o'er flood and field.  

- **Answer:** A swan
- **Also take:** swan; wild swan; mute swan; whooper swan
- **From:** Exeter Book riddle 7 (Krapp-Dobbie; Tupper 8); copied c. 965-975; made 8th-10th c.
- **English by:** Herbert B. Brougham, 1902
- **Source:** Cook and Tinker, Select Translations from Old English Poetry (Boston: Ginn, copyright 1902; the scan read is a printing of 1913 or later with the prefatory note revised), '8. The Swan', p. 72; answer: Tupper 1910, 'all scholars accept the solution Swan'
- **Who would ask it:** an English monk, or a poet in a hall (peoples: english)
- **In the game as:** riddle_exeter_07_swan
- **Note:** The game has Thorpe 1842 for this one. Brougham is the plainer and the more correct: the feathers are silent on land and sing in flight.

#### 2. The evening singer

> Many varied voices voice I through my mouth.  
> Cunning are the notes I sing, and incessantly I change them.  
> Clear I cry and loud; with the chant within my head;  
> Holding to my tones, hiding not their sweetness.  
> I, the Evening-singer old, unto earls I bring  
> Bliss within the burgs, when I burst along  
> With a cadenced song. Silent in their dwellings  
> They are sitting, bending forwards. Say what is my name.  

- **Answer:** A nightingale
- **Also take:** nightingale; song thrush; thrush; a singing bird
- **From:** Exeter Book riddle 8 (Krapp-Dobbie; Tupper 9); copied c. 965-975
- **English by:** Stopford A. Brooke, 1892
- **Source:** Stopford A. Brooke, The History of Early English Literature (London and New York: Macmillan, 1892), ch. 9, p. 149, 'Riddle ix'; answer: Dietrich, followed by Brooke and Tupper 1910 (others: jay, chough, wood-pigeon)
- **Who would ask it:** an English monk, or a poet in a hall (peoples: english)
- **In the game as:** not there yet
- **Note:** Not in the game yet. Brooke's wording is old-fashioned ('burgs', 'earls'); it is his, not the game's voice.

#### 3. The barnacle goose

> In a narrow was my neb, and beneath the wave I lived;  
> Underflowen by the flood; in the mountain-billows  
> Low was I besunken; in the sea I waxed  
> Over-covered with the waves, clinging with my body  
> To a wandering wood. . . .  
> Quick the life I had, when I from the clasping came  
> Of the billows, of the beam-wood, in my black array;  
> White in part were then my pranked garments fair,  
> When the Lift upheaved me, me a living creature,  
> Wind from wave upblowing; and as wide as far  
> Bore me o'er the bath of seals. Say, what is my name!  

- **Answer:** A barnacle goose
- **Also take:** barnacle goose; goose; barnacle; tree goose
- **From:** Exeter Book riddle 10 (Krapp-Dobbie; Tupper 11); copied c. 965-975
- **English by:** Stopford A. Brooke, 1892
- **Source:** Stopford A. Brooke, The History of Early English Literature (1892), ch. 10, p. 179 (?), 'the 11th riddle'; the answer is Brooke's own and is generally accepted (Tupper 1910)
- **Who would ask it:** a monk of a sea-coast house, or an old seaman who has seen the geese come in autumn (peoples: english, frisian, irish_gaelic)
- **In the game as:** riddle_exeter_10_barnacle_goose
- **Note:** The belief that these geese grow from driftwood under the sea: the oldest witness to it. The game has Thorpe 1842, who misreads the driftwood line ('heavy only to sailors in ships'); Brooke keeps the clue. 'Lift' is the air.

#### 4. The ox and its hide [double meaning]

> On feet I go, the earth tear, the green fields, while a soul I bear. If my life perish, fast I bind the swart strangers; sometimes to a better man, sometimes to beasts, I give to drink the stream from my bosom; sometimes me the bride treads, much proud, with her feet; sometimes, brought from far, the strange woman, dusk of hair, moves and impels me: the foolish serving maid, in dark nights, wets in water; sometimes warms well at the fire, me in her bosom sticks, wild-minded in her hand, turns me abundantly, glides over me, black. Say what I am call'd, I who living rob the land, and after death multitudes serve.  

- **Answer:** An ox, and the leather made of its hide
- **Also take:** ox; ox hide; oxhide; leather; hide; an ox and its hide; skin
- **From:** Exeter Book riddle 12 (Krapp-Dobbie; Tupper 13); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (London: Society of Antiquaries, 1842), Riddle XIII, pp. 393-394; answer: 'accepted by all authorities' (Tupper 1910)
- **Who would ask it:** a poet in a hall, late, when the ale has gone round (peoples: english)
- **Double meaning:** In part. The first half is plain (bonds, a leather bottle, shoes); the serving woman working the hide by night is told so as to sound like something else.
- **In the game as:** riddle_exeter_12_ox
- **Note:** Thorpe prints half-lines; they are run on here, words and stops unchanged. 'Swart strangers' are Welsh bondmen in the Old English (wealas).

#### 5. The horn

> I was an armed warrior, but now  
> The youthful courtier covers my proud neck  
> With twisted filigree of gold and silver.  
> Sometimes I'm kissed by heroes, and again  
> I woo to battle with my melody  
> Comrades in full accord. At times the courser  
> Bears me across the border, and again  
> Over the floods the stallion of the sea  
> Conveys me radiant with ornaments.  
> Sometimes a maiden, garlanded with jewels,  
> Brims full my winding bosom, and again  
> Perforce I lie, hard, headless, solitary,  
> Upon the board. Sometimes, set off with trappings,  
> In comely guise upon the wall I hang  
> Where heroes drink. Again, horsed warriors  
> On forays wear me, glorious apparel;  
> Then, dappled with gold, I must inspire the wind  
> From some one's bosom. Whilom stately men  
> I summon to banquetings and wine; sometimes  
> My voice resounds with freedom to the captive,  
> Flight to the foe. Now find out what I'm called.  

- **Answer:** A horn (an ox's horn: war-horn and drinking-horn)
- **Also take:** horn; ox horn; drinking horn; war horn; a horn
- **From:** Exeter Book riddle 14 (Krapp-Dobbie; Tupper 15); copied c. 965-975
- **English by:** Herbert B. Brougham, 1902
- **Source:** Cook and Tinker, Select Translations from Old English Poetry (1902), '15. The Horn', pp. 72-73; answer: Dietrich 1859, Tupper 1910
- **Who would ask it:** a poet in a hall, with the horn in his hand (peoples: english, saxon)
- **In the game as:** riddle_exeter_14_horn
- **Note:** 'The stallion of the sea' is a ship. The game has Thorpe 1842.

#### 6. The anchor

> Oft with the wave I must contend, and against the wind fight, against them together strive, when I go to visit the earth with billows cover'd: the land is strange to me. I am strong in the strife, if I am still; if it well befall me, they are more powerful than I, and tearing me, soon put me to flight. They will bear me away, that I shall protect them; I hinder them from that, if my tail suffers, and me rigid, against the stones, they can fast retain. Say what I am call'd.  

- **Answer:** An anchor
- **Also take:** anchor; ship's anchor; an anchor
- **From:** Exeter Book riddle 16 (Krapp-Dobbie; Tupper 17); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), Riddle XVII, p. 398; answer: Dietrich 1859, 'unquestionably correct' (Tupper 1910)
- **Who would ask it:** the master of an English ship, or a monk who has crossed the sea (peoples: english, frisian)
- **In the game as:** riddle_exeter_16_anchor
- **Note:** Thorpe's middle lines are muddled ('if it well befall me' should be 'if I fail in that'). Compare Symphosius 61 and Gestumblindi's anchor: three peoples, one riddle.

#### 7. The plough

> My face is downward, prone I go, and along the ground dig, as me a youth directs, the hoar wood's foe; and my master goes bent, a guardian at my tail drives me on the field, moves and impels me, sows on my track. I hasten forth, brought from a tree, bound by craft, borne on a wain. I have wonders many, to me going there is green on one side, and my track manifest, black on the other. Through my back driven, hangs beneath one curious dart, another on my head; fast and forward falls at my side what with teeth I tear, if me well serve behind, he who is my master.  

- **Answer:** A plough
- **Also take:** plough; plow; a plough
- **From:** Exeter Book riddle 21 (Krapp-Dobbie; Tupper 22); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), Riddle XXII, p. 403 ('By this, no doubt, a plough is intended'); Tupper 1910
- **Who would ask it:** a farmer at a market, or a reeve (peoples: english, saxon, frankish)
- **In the game as:** riddle_exeter_21_plough
- **Note:** 'The hoar wood's foe' is the ox, or the ploughman who cleared the land. A heavy wheeled plough with coulter and share: true to the period.

#### 8. The bow

> My name is AGOB, with the letters changed.  
> I am a wondrous creature shaped for strife;  
> When I am bent, and from my bosom darts  
> The venomous sting, with dexterous speed I send  
> Far and away the quivering stroke of death.  
> Soon as my guide who fashioned me for torture  
> Lets loose my pliant limbs, in agony  
> I stretch, until I vomit forth the broth  
> Of fatal, piercing poison that erewhile  
> I swallowed. Never a man I then bespeak  
> Shall easily depart, once he is grazed  
> By that which takes its flight from out my vitals.  
> He makes dear purchase of the treacherous draught,  
> The full, stern cup which robs him of his life!  
> Except when fettered fast no use I serve;  
> Unbound I fail. Now tell what I am called.  

- **Answer:** A bow
- **Also take:** bow; a bow; boga; longbow; bow and arrow
- **From:** Exeter Book riddle 23 (Krapp-Dobbie; Tupper 24); copied c. 965-975
- **English by:** Herbert B. Brougham, 1902
- **Source:** Cook and Tinker, Select Translations from Old English Poetry (1902), '24. The Bow', p. 73; answer: the first word read backwards (Old English boga), Tupper 1910
- **Who would ask it:** a poet in a hall; a hunter (peoples: english)
- **Word-play:** The answer is hidden in the first line: the manuscript has AGOF, which is BOGA (bow) backwards with one letter changed by a later scribe.
- **In the game as:** riddle_exeter_23_bow
- **Note:** The game has Thorpe 1842 ('Agof is my name transposed').

#### 9. The onion [double meaning]

> I am a wonderful creature, a joy to women, useful to neighbours. I harm no one who lives in a town but my slayer alone. My stem is tall and upright; I stand in a bed; I am shaggy somewhere below. Sometimes a churl's fair daughter, a proud-minded girl, dares to take hold of me. She rushes at my red skin, plunders my head, and shuts me in a tight place. She soon feels her meeting with me, the woman with braided hair who squeezes me. Wet is that eye.  

- **Answer:** An onion
- **Also take:** onion; leek; an onion; garlic
- **From:** Exeter Book riddle 25 (Krapp-Dobbie; Tupper 26); copied c. 965-975
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Old English as printed by Tupper 1910, Riddle 26, pp. 19-20 (the text is in texts_793.json, exeter_25_onion). No translation in the public domain was found: Thorpe 1842 (p. 407) prints the Old English and leaves the English out, as he does for all the coarse ones. Answer: Dietrich 1859; Tupper 1910.
- **Who would ask it:** a poet in a hall, or a market woman who sells onions (peoples: english)
- **Double meaning:** The best known of the Exeter Book's riddles with two answers: every line fits a man's body until the last, and the plain answer is in the kitchen.
- **In the game as:** riddle_exeter_25_onion
- **Note:** The game has this riddle with no English at all. Compare Symphosius 44 (the onion that bites the biter).

#### 10. The book

> An enemy deprived me of my life,  
> Stripped me of worldly strength, immersed me then  
> In water, whence again he took me dripping,  
> Planted me in the sun, and there I lost  
> My nap of hair. The knife's keen edge then dressed me,  
> Sharpened with pumice. Fingers folded me,  
> And next the joyous quill traced eagerly  
> Across my burnished surface, scattering  
> The fluent drops along. Again it drank  
> Of the tinctured stream, again stepped over me  
> With blackening print. The craftsman bound me then  
> In leathern covers locked with golden clasps,  
> The wondrous work of artists. Thus adorned  
> With scarlet dyes resplendent, lo! in me  
> The glorious abodes afar renowned,  
> The Shield of nations, and good will toward men!  
> And if the children of this world will use me,  
> The happier, the surer of success  
> They'll be, the keener-hearted, and in thought  
> The kinder, and more fraught with wisdom. Then  
> More friends they'll have, their own, familiar friends  
> So good and true, and capable, and trusty,  
> Who will prolong their fame and happiness  
> And hedge them round with graceful gifts, and fast  
> In bonds of love within their bosoms fold them.  
> Find out what I am called for men's advantage!  
> Famous in sacred story is my name,  
> Renowned 'mongst heroes, and itself divine.  

- **Answer:** A book: a Gospel book or Bible
- **Also take:** book; bible; gospel book; a book; codex; the gospels; parchment; a holy book
- **From:** Exeter Book riddle 26 (Krapp-Dobbie; Tupper 27); copied c. 965-975
- **English by:** Herbert B. Brougham, 1902
- **Source:** Cook and Tinker, Select Translations from Old English Poetry (1902), '27. The Bible-Codex', pp. 73-74; answer: 'Liber' (L. C. Mueller 1835), Tupper 1910
- **Who would ask it:** the master of a scriptorium; a monk homesick for books (peoples: english, irish_gaelic, frankish)
- **In the game as:** riddle_exeter_26_book
- **Note:** How a book was made, step by step: the calf killed, the skin soaked, stretched, scraped, folded, written with a quill and bound with gold. The game has Thorpe 1842, whose middle lines go badly wrong ('my fingers the earth, and me foul swine'): use Brougham.

#### 11. Mead

> On every hand I'm found and prized by men,  
> Borne from the fertile glades and castled heights  
> And vales and hills. Daily the wings of bees  
> Carried me through the air, and with deft motion  
> Stored me beneath the low-crowned, sheltering roof.  
> Then in a cask men cherished me. But now  
> The old churl I tangle, and trip, at last o'erthrow  
> Flat on the ground. He that encounters me  
> And sets his will 'gainst my subduing might  
> Forthwith shall visit the earth upon his back!  
> If from his course so ill-advised he fails  
> To abstain, deprived of strength, yet strong in speech,  
> He's reft of all his power o'er hand or foot,  
> His mind dethroned. Now find out what I'm called,  
> Who bind again the freeman to the soil,  
> Stupid from many a fall, in broad daylight!  

- **Answer:** Mead
- **Also take:** mead; honey; honey mead; drink
- **From:** Exeter Book riddle 27 (Krapp-Dobbie; Tupper 28); copied c. 965-975
- **English by:** Herbert B. Brougham, 1902
- **Source:** Cook and Tinker, Select Translations from Old English Poetry (1902), '28. The Mead', pp. 74-75; answer: Lange, adopted by Dietrich; Tupper 1910
- **Who would ask it:** the man who pours in a hall; a bee-keeper selling honey (peoples: english, saxon, danish)
- **In the game as:** riddle_exeter_27_mead
- **Note:** Mead is a trade good in the game. Compare Gestumblindi's first riddle (ale). 'Castled heights' is Brougham's; the Old English has only 'hill-slopes'.

#### 12. The merchant ship

> This mid-earth is in various guises beautified, curiously adorn'd, in its conditions wonderful. I saw a curious thing going about, grinding against the dust, going yelling; the wonderous thing had not sight nor hands, shoulder nor arms, must on one foot the curious thing go about, rapidly move, go o'er the fields. It had many ribs, its mouth was in the middle: useful to mankind, in its course nutritive abundance it to people draws, food bears in, and to men pays tribute every year, of what people use, high and low. Explain, if thou canst, thou in wise words skill'd, what the thing is.  

- **Answer:** A ship: a merchant ship
- **Also take:** ship; merchant ship; a ship; boat; trading ship; cargo ship
- **From:** Exeter Book riddle 32 (Krapp-Dobbie; Tupper 33); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), Riddle XXXIII, pp. 414-415; answer: Dietrich 1859, 'generally accepted' (Tupper 1910)
- **Who would ask it:** a port reeve, or a skipper at the quay: this is the player's own boat (peoples: english, frisian)
- **In the game as:** riddle_exeter_32_ship
- **Note:** The one foot is the keel, the ribs are the frames, the mouth in the middle is the open hold. A cargo ship, not a warship: 'food bears in ... tribute every year, of what people use, high and low'. The best riddle in the book for this game.

#### 13. The iceberg

> A thing came along the wave, curious, sailing, comely, from a vessel, it call'd to land, loud sounded: its laughter was horrible, terrific in the land: its edges were sharp, fierce was its hate, in battle too slow; bitter in works of war, bucklers it pierced, stern and desolating hatred it bound, said, skill'd in guile, about its own nature: My mother is of womankind the dearest; she is my daughter, magnified, exalted, as that is to mortals known, to men among people, so that she on earth shall, in every land whatever, in favour stand.  

- **Answer:** An iceberg, or floating ice
- **Also take:** iceberg; ice; ice floe; floating ice; sea ice
- **From:** Exeter Book riddle 33 (Krapp-Dobbie; Tupper 34); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), Riddle XXXIV, pp. 415-416; answer: Dietrich 1859, Tupper 1910
- **Who would ask it:** a seaman who has been north; a monk (peoples: english, norse)
- **In the game as:** riddle_exeter_33_iceberg
- **Note:** The mother who is also the daughter is water: ice is born of water and water of ice. The same turn is in Symphosius 10 and in Tatwine's three sisters. 'Bucklers it pierced' is the ship's side (Old English bordweallas).

#### 14. The key [double meaning]

> A strange thing hangs by a man's thigh, under its master's cloak. It is pierced in front; it is stiff and hard, and it has a good place. When the man lifts his own garment above his knee, he means to greet with the head of his hanging thing that well-known hole which it has often filled before, being of the same length.  

- **Answer:** A key
- **Also take:** key; a key; key and lock; dagger sheath
- **From:** Exeter Book riddle 44 (Krapp-Dobbie; Tupper 45); copied c. 965-975
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Old English (Tupper 1910, Riddle 45; Thorpe 1842, Riddle XLV, p. 431, where the Old English stands without any English). Answer: Dietrich 1859; Tupper 1910.
- **Who would ask it:** a poet in a hall; a housewife with the keys at her belt (peoples: english)
- **Double meaning:** Meant to be heard wrongly. The plain answer hangs at every householder's belt.
- **In the game as:** not there yet
- **Note:** Not in the game. Seven lines in the Old English. Keys and locks are common finds in the trading towns (Dorestad, Hedeby, York).

#### 15. The dough [double meaning]

> I have heard of something growing in a corner, swelling and standing up, lifting its cover. A proud bride took hold of that boneless thing with her hands; a lord's daughter covered the swelling thing with a cloth.  

- **Answer:** Dough
- **Also take:** dough; bread dough; bread; rising dough; leaven
- **From:** Exeter Book riddle 45 (Krapp-Dobbie; Tupper 46); copied c. 965-975
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Old English (Tupper 1910, Riddle 46; Thorpe 1842, Riddle XLVI, p. 431, Old English only). Answer: Dietrich 1859, after the 'boneless' bread of other riddles; Tupper 1910.
- **Who would ask it:** a poet in a hall; a baker; a woman at the kneading trough (peoples: english)
- **Double meaning:** Five lines, one joke: the second meaning is plain, the true answer is bread rising under a cloth.
- **In the game as:** not there yet
- **Note:** Not in the game. The shortest of the coarse ones and the easiest to set.

#### 16. The family at table

> A man sat at wine with his two wives, and his sons twain, and his two daughters, own brothers and sisters, and their sons twain, gentle first-borns. The father was therein of those youths both, with uncle and nephew. In all were five men and women sitting within.  

- **Answer:** Lot, his two daughters and their two sons
- **Also take:** lot; lot and his daughters; lot and his family; lot his daughters and their sons
- **From:** Exeter Book riddle 46 (Krapp-Dobbie; Tupper 47); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), Riddle XLVII, pp. 431-432; answer: from Genesis 19:30-38 (Wright; Dietrich; Tupper 1910)
- **Who would ask it:** a priest or a schoolmaster: it needs the book of Genesis (peoples: english, frankish, irish_gaelic)
- **In the game as:** not there yet
- **Note:** Not in the game. A counting riddle: twelve kin are named and only five people sit there. Only a Christian with some schooling can answer it, which makes it a good test at a minster.

#### 17. The bookworm

> A moth ate words; to me that seem'd a curious hap, when I that wonder heard, that the worm had swallow'd some man's saying, a thief in darkness, some noble speech, and the strong's * The stealing guest was not in aught the wiser, who the words had swallow'd.  

- **Answer:** A bookworm (a book-moth)
- **Also take:** bookworm; moth; book moth; worm; a moth; maggot
- **From:** Exeter Book riddle 47 (Krapp-Dobbie; Tupper 48); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), Riddle XLVIII, p. 432; answer: 'Bookmoth' (Grein 1858); from Symphosius 16, Tinea
- **Who would ask it:** a librarian monk, sourly (peoples: english, irish_gaelic, frankish)
- **In the game as:** riddle_exeter_47_bookworm
- **Note:** The star marks two words Thorpe could not read ('and its strong foundation', the parchment). The riddle gives its own answer in the first word; the joke is the last line. Du Bois 1912 has Symphosius's three lines: 'I thrive on letters yet no letters know, / I live in books, not made more studious so, / Devour the Muses, but no wiser grow.'

#### 18. The little black ones

> This air bears little creatures over the mountain brows; they are very black, swart, sallow-coated, sweet of song, in swarms they go, loudly cry; they tread the woody promontories, sometimes the city-halls of the sons of men. Name them yourselves.  

- **Answer:** Swallows (or swifts, or starlings)
- **Also take:** swallows; swifts; starlings; martins; swallow; swift; starling; jackdaws; crows; gnats; midges; small birds
- **From:** Exeter Book riddle 57 (Krapp-Dobbie; Tupper 58); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), Riddle LVIII, pp. 438-439; answers: swallows (Dietrich), starlings (Prehn, Brooke 1892), gnats (Trautmann), listed by Tupper 1910
- **Who would ask it:** anyone, looking up at evening (peoples: english)
- **In the game as:** not there yet
- **Note:** Not in the game. No answer has ever been agreed: take any small dark flocking bird. The last line may hide the answer ('they name themselves': a bird named after its cry).

#### 19. The poker [double meaning]

> I am hard and sharp, of entrance strong, of departure strenuous, to my master faithful. I go under the belly, and for myself a way, direct I clear. The man is in haste who urges me lingering behind. A man with his garment sometimes draws me out hot from a hole; sometimes retreats into a narrow space, I know not where, violently compels the southern man. Say what I am call'd.  

- **Answer:** A poker, or a smith's burning iron (a borer)
- **Also take:** poker; fire iron; borer; gimlet; branding iron; burning iron; fire poker; awl
- **From:** Exeter Book riddle 62 (Krapp-Dobbie; Tupper 63); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), third series of riddles, no. II, pp. 479-480 (two words that Thorpe adds in italics are read from a poor scan (?)); answers: 'Gimlet' (Dietrich), 'Poker' or 'Burning arrow' (others), in Tupper 1910
- **Who would ask it:** a smith at his forge (peoples: english)
- **Double meaning:** The whole of it. Thorpe put this one into English where he left the onion and the key alone, perhaps because he did not see it.
- **In the game as:** not there yet
- **Note:** Not in the game. 'The southern man' is a puzzle of its own (a man from the south, or one thrusting from the south).

#### 20. A wonder on the way

> I saw the creature on the way going, it was curiously, wonderously, equip'd. A wonder was on the way, water was turn'd to poison.  

- **Answer:** Ice
- **Also take:** ice; frost; winter; frozen water; a frozen river
- **From:** Exeter Book riddles 68-69 (Krapp-Dobbie; Tupper 69, Grein 68); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), third series of riddles, nos. VII-VIII; answers: 'Ice' (Dietrich), 'Winter' (Grein), in Tupper 1910
- **Who would ask it:** anyone, in winter, at a frozen landing (peoples: english, frisian, saxon)
- **In the game as:** riddle_exeter_68_69_ice
- **Note:** Thorpe's last word is wrong and spoils the riddle: the Old English is 'waeter weard to bane', water turned to bone. If it is set to a player, change 'poison' to 'bone' and credit it as Thorpe 1842, one word altered.

#### 21. The fish and the river

> My comrade is not silent, nor I myself loud, for us two the Lord assign'd * * * a course together. I am swifter than he, at times stronger, he is more enduring: while I rest he must run forth: I in him dwell, ever while I live, if we part, death is decreed to me.  

- **Answer:** A fish in a river
- **Also take:** fish and river; fish; a fish in a river; river and fish; fish in water; a fish and the stream
- **From:** Exeter Book riddle 85 (Krapp-Dobbie; Tupper 85, Grein 82); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), third series of riddles; answer: 'Flood and Fish' (Tupper 1910); from Symphosius 12, Flumen et piscis
- **Who would ask it:** a fisherman; a monk who knows his Symphosius (peoples: english, frisian, frankish)
- **In the game as:** riddle_exeter_85_fish_river
- **Note:** The stars mark a gap in Thorpe's reading. The same riddle is in Symphosius (no. 12) and in Alcuin's Disputatio ('I saw a guest running with his house').

#### 22. The one-eyed seller

> A creature came walking where men sate, many in assembly, prudent-minded; it had one eye, and two ears, and two feet, twelve hundred heads, a back and belly, and two hands, arms and shoulders, one neck, and two sides. Say what I am call'd.  

- **Answer:** A one-eyed man selling garlic (or onions)
- **Also take:** one-eyed garlic seller; garlic seller; a one-eyed man selling garlic; onion seller; one-eyed onion seller; a one-eyed man with garlic; a trader with one eye
- **From:** Exeter Book riddle 86 (Krapp-Dobbie; Tupper 86, Grein 83); copied c. 965-975
- **English by:** Benjamin Thorpe, 1842
- **Source:** Benjamin Thorpe, Codex Exoniensis (1842), third series of riddles; answer: Dietrich's later solution, from Symphosius 94, Luscus alium vendens (Tupper 1910)
- **Who would ask it:** a market reeve; a trader mocking another trader (peoples: english, frankish)
- **In the game as:** riddle_exeter_86_garlic_seller
- **Note:** Nobody could guess it without the Latin riddle behind it. A market joke, and the only riddle in the book about a pedlar. Good for a market reeve to ask a merchant.

### The riddles of Gestumblindi (Hervarar saga)

#### 23. What I had yesterday

> I would that I had that which I had yesterday. Guess O King, what that was: Exhauster of men, retarder of words, yet originator of speech. King Heithrek, read me this riddle!  

- **Answer:** Ale
- **Also take:** ale; beer; drink; mead
- **From:** Hervarar saga ok Heidreks, chs. X-XI in Kershaw's numbering (the riddles of Gestumblindi), no. 1; saga written 13th c.; the riddles older, of unknown age (?)
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (Cambridge University Press, 1921), 'The Saga of Hervor and Heithrek', p. 115. The king's answer: 'Give him some ale. That is what confounds many people's reason. Some are made garrulous by it, but some become confused in their speech.'
- **Who would ask it:** a skald in a king's hall; the game's Ravn (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-01
- **Note:** The first riddle in every version of the saga. The asker is Odin in a borrowed shape. Kershaw died in 1972: see the notes on what may be quoted.

#### 24. The road of roads

> I went from home, I made my way from home, I looked upon a road of roads. A road was beneath me, a road above and a road on every side. King Heithrek, read me this riddle!  

- **Answer:** A bridge over a river (with birds overhead and fish below)
- **Also take:** bridge; a bridge; a bridge over a river; river bridge
- **From:** Hervarar saga, riddles of Gestumblindi, no. 2; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 115. The king's answer: 'You went over a bridge, and the course of the river was beneath it, and birds were flying over your head and on either side of you; that was their road; you saw a salmon in the river, and that was his road.'
- **Who would ask it:** a skald; a traveller at a ferry or a bridge (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-02
- **Note:** Bridges were rare and famous works in the north (the Ravning bridge in Jutland, c. 980): a king's work, and a fit thing to riddle on.

#### 25. Yesterday's drink

> What was the drink that I had yesterday? It was neither wine nor water, mead nor ale, nor any kind of food; and yet I went away with my thirst quenched. King Heithrek, read me this riddle!  

- **Answer:** Dew
- **Also take:** dew; the dew; morning dew
- **From:** Hervarar saga, riddles of Gestumblindi, no. 3; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), pp. 115-116. The king's answer: 'You lay in the shade and cooled your lips in dew.'
- **Who would ask it:** a skald; a herdsman (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-03

#### 26. The huge one over the earth

> What is that huge one that passes over the earth, swallowing lakes and pools? He fears the wind, but he fears not man, and carries on hostilities against the sun. King Heithrek, read me this riddle!  

- **Answer:** Fog
- **Also take:** fog; mist; sea fog; the fog
- **From:** Hervarar saga, riddles of Gestumblindi, no. 5; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 116. The king's answer: 'That is fog. One cannot see the sea because of it. Yet as soon as the wind blows, the fog lifts; but men can do nothing to it. Fog kills the sunshine.'
- **Who would ask it:** a Norse skipper you hail, in fog (peoples: norse, danish, svea_gotlandic, frisian)
- **In the game as:** r-gest-05
- **Note:** Set it at sea when the fog banks come in: the game has them.

#### 27. Half of it faces Hell

> What is that huge one that controls many things and of which half faces towards Hell? It saves people's lives and grapples with the earth, if it has a trusty friend. King Heithrek, read me this riddle!  

- **Answer:** An anchor (with its cable)
- **Also take:** anchor; an anchor; anchor and cable; anchor and rope
- **From:** Hervarar saga, riddles of Gestumblindi, no. 6; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), pp. 116-117. The king's answer: 'That is an anchor with its thick strong cable. It controls many a ship, and grips the earth with one of its flukes which is pointing towards Hell. It is a means of safety to many people.'
- **Who would ask it:** a Norse skipper; a shipwright (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-06
- **Note:** The trusty friend is the cable. 'Hell' is Hel, the place of the dead under the earth, not the Christian hell. Compare Exeter 16 and Symphosius 61.

#### 28. Four in one

> What lives in high mountains? What falls in deep valleys? What lives without breathing? What is never silent? King Heithrek, read me this riddle!  

- **Answer:** A raven; dew; a fish; a waterfall
- **Also take:** raven dew fish waterfall; a raven, dew, a fish, a waterfall; raven, dew, fish, waterfall; the raven, the dew, a fish, a fall
- **From:** Hervarar saga, riddles of Gestumblindi, no. 7; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 117. The king's answer: 'A raven always lives in high mountains, and dew falls in deep valleys, a fish lives without breathing, and the booming waterfall is never silent.'
- **Who would ask it:** a skald, quickly, to catch a man out (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-07a, r-gest-07b, r-gest-07c, r-gest-07d
- **Note:** Four answers are wanted, in order. The game already splits it into four.

#### 29. Outside Delling's doorway

> What is the marvel which I have seen outside Delling's doorway? It points its head towards Hell and turns its feet to the sun. King Heithrek, read me this riddle!  

- **Answer:** A leek
- **Also take:** leek; a leek; onion; garlic
- **From:** Hervarar saga, riddles of Gestumblindi, no. 8; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 117. The king's answer: 'That is a leek. Its head grows down into the ground, and its blades upward into the air.'
- **Who would ask it:** a skald; the woman who keeps the hall's hearth (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-08
- **Note:** Delling is the father of Day in Vafthrudnismal 25: 'outside Delling's doorway' is at daybreak. Seven of the riddles begin so, until the king complains of it.

#### 30. Ten tongues

> What is the marvel that I have seen outside Delling's doorway? This creature has ten tongues, twenty eyes, forty feet, and walks with difficulty. King Heithrek, read me this riddle!  

- **Answer:** A sow with nine piglets inside her
- **Also take:** sow; a sow with nine piglets; pregnant sow; a sow in farrow; sow and piglets; a sow with a litter of nine
- **From:** Hervarar saga, riddles of Gestumblindi, no. 12; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 118. The king's answer: 'That was a sow with nine little pigs.' The saga adds: 'Then the King had the sow killed and they found they had killed with her nine little pigs.'
- **Who would ask it:** a skald; a farmer at a market (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-12
- **Note:** Aldhelm has the same sum in Latin (no. 84, Scrofa praegnans) centuries before the saga was written: a riddle that crossed the sea.

#### 31. Eight feet and four eyes

> What is the marvel which I have seen outside Delling's doorway? It has eight feet and four eyes, and carries its knees higher than its body. King Heithrek, read me this riddle!  

- **Answer:** A spider
- **Also take:** spider; a spider
- **From:** Hervarar saga, riddles of Gestumblindi, no. 14; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 119. The king's answer: 'I notice firstly that you have a long hood; and secondly that you look downwards more than most people, since you observe every creature of the earth. That is a spider.'
- **Who would ask it:** a skald (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-14
- **Note:** The king's remark about the long hood is the first hint of who the stranger is: Odin goes hooded.

#### 32. The white-hooded ones

> Who are the damsels who go sorrowing for their father's pleasure? These white-hooded ladies have shining hair, and are very wide awake in a gale. King Heithrek, read me this riddle!  

- **Answer:** The waves (Aegir's daughters)
- **Also take:** waves; the waves; billows; aegir's daughters; the sea's daughters; breakers
- **From:** Hervarar saga, riddles of Gestumblindi, no. 21; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 121. The king's answer: 'Those are the billows, which are called Aegir's maidens.'
- **Who would ask it:** the master of a Norse ship you hail; a weather-wise old seaman (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-21
- **Note:** Three wave riddles follow one another in the saga (the game's r-gest-21, 22, 23). Aegir is the sea-giant; his nine daughters are the waves.

#### 33. The blind riding the blind

> I saw a barrow-dweller pass by, a corpse sitting on a corpse, the blind riding on the blind towards the ocean-path. Lifeless was the steed. King Heithrek, read me this riddle!  

- **Answer:** A dead snake on a dead horse on an ice-floe, floating down a river
- **Also take:** a dead horse on an ice floe; dead horse on ice; a dead snake on a dead horse on an ice floe; a dead horse floating on ice; ice floe; a carcass on the ice
- **From:** Hervarar saga, riddles of Gestumblindi, no. 24; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 122. The king's answer: 'It is that you came to a river; and an ice-floe was floating along the stream, and on it a dead horse was lying, and on the horse was a dead snake; and thus the blind was carrying the blind when they were all three together.'
- **Who would ask it:** a skald, late in the contest, when it turns dark (peoples: norse, danish, svea_gotlandic)
- **In the game as:** r-gest-24
- **Note:** The hardest of the set: a thing seen once, which no one could guess who had not seen it. The game's answer (from the 1883 translation) names only the horse and the ice.

#### 34. Four walking, four hanging

> Four walking, four hanging, two pointing the way, two warding off the dogs, one, generally dirty, dangling behind! King Heithrek, read me this riddle!  

- **Answer:** A cow
- **Also take:** cow; a cow; ox; cattle
- **From:** Hervarar saga, riddles of Gestumblindi, no. 28; saga 13th c.
- **English by:** Nora Kershaw (later Chadwick), 1921
- **Source:** N. Kershaw, Stories and Ballads of the Far Past (1921), p. 123. The king's answer: 'That is a cow. She has four feet and four udders, two horns and two eyes, and the tail dangles behind.'
- **Who would ask it:** anyone: a child could ask it (peoples: norse, danish, svea_gotlandic, saxon, frisian)
- **In the game as:** r-gest-28
- **Note:** The most widely told riddle in Europe; it is found in almost every language. The easiest of the sixty: a good first riddle.

### Symphosius

#### 35. Clavis

> Great deeds with little strength I do,  
> I close the open, ope the closed for you.  
> I keep the master's house, the master keeps me, too.  

- **Answer:** A key
- **Also take:** key; a key; clavis
- **From:** Symphosius, Aenigmata 4; late 4th or 5th c.; read in every school of the 8th-10th c.
- **English by:** Elizabeth Hickman du Bois, 1912
- **Source:** Elizabeth Hickman du Bois, The Hundred Riddles of Symphosius (Woodstock, Vermont: Elm Tree Press, 1912), riddle IV; the answer is the title in the manuscripts
- **Who would ask it:** a schoolmaster to his boys; a cellarer (peoples: frankish, english, irish_gaelic)
- **In the game as:** riddle_symphosius_04_clavis
- **Note:** The clean key, to set beside the Exeter Book's (no. 44). The game has the Latin only.

#### 36. Glacies

> Wave I was and wave again will be  
> Though now by heaven's chain held rigidly;  
> So without gloves beware of handling me.  

- **Answer:** Ice
- **Also take:** ice; glacies; frozen water
- **From:** Symphosius, Aenigmata 10; late 4th or 5th c.
- **English by:** Elizabeth Hickman du Bois, 1912
- **Source:** E. H. du Bois, The Hundred Riddles of Symphosius (1912), riddle X; the answer is the title in the manuscripts
- **Who would ask it:** a schoolmaster; a monk in winter (peoples: frankish, english, irish_gaelic)
- **In the game as:** riddle_symphosius_10_glacies
- **Note:** Du Bois's third line is loose: the Latin says 'I cannot bear to be trodden on, nor be held in a bare hand'.

#### 37. Nix

> Dust of the water, light of weight am I;  
> I'm dripping in the sun, but in the cold am dry,  
> I rivers make, though first upon the ground I lie.  

- **Answer:** Snow
- **Also take:** snow; nix
- **From:** Symphosius, Aenigmata 11; late 4th or 5th c.
- **English by:** Elizabeth Hickman du Bois, 1912
- **Source:** E. H. du Bois, The Hundred Riddles of Symphosius (1912), riddle XI; the answer is the title in the manuscripts
- **Who would ask it:** a schoolmaster; a monk in winter (peoples: frankish, english, irish_gaelic)
- **In the game as:** riddle_symphosius_11_nix
- **Note:** Alcuin boils it down to two words: 'What is snow? Dry water.'

#### 38. Flumen et piscis

> A house there is which rings throughout the land with song,  
> The house itself doth sing, the guests in silence throng,  
> Yet both the house and guests together move along.  

- **Answer:** A river and its fish
- **Also take:** river and fish; a river and fish; fish in a river; river; a river with fish; stream and fish
- **From:** Symphosius, Aenigmata 12; late 4th or 5th c.
- **English by:** Elizabeth Hickman du Bois, 1912
- **Source:** E. H. du Bois, The Hundred Riddles of Symphosius (1912), riddle XII; the answer is the title in the manuscripts
- **Who would ask it:** a monk on a river landing; a toll clerk (peoples: frankish, english, irish_gaelic, frisian)
- **In the game as:** riddle_symphosius_12_flumen
- **Note:** The mother of Exeter 85 and of Alcuin's 'guest running with his house'. A river riddle for a river port.

#### 39. Navis

> Child of the forest, swift and full of grace  
> And countless comrades move with equal pace;  
> O'er many roads I pass, nor leave behind a trace.  

- **Answer:** A ship
- **Also take:** ship; a ship; boat; navis
- **From:** Symphosius, Aenigmata 13; late 4th or 5th c.
- **English by:** Elizabeth Hickman du Bois, 1912
- **Source:** E. H. du Bois, The Hundred Riddles of Symphosius (1912), riddle XIII; the answer is the title in the manuscripts
- **Who would ask it:** a clerk of the toll, to a skipper (peoples: frankish, english, irish_gaelic, frisian)
- **In the game as:** riddle_symphosius_13_navis
- **Note:** 'A traveller who leaves no tracks' is in Alcuin's answer to 'What is a ship?' too: he had it from here.

#### 40. Ancora

> My double points in one by iron bound,  
> With winds I struggle, fight with gulfs profound,  
> I search the waters and I bite the ground.  

- **Answer:** An anchor
- **Also take:** anchor; an anchor; ancora
- **From:** Symphosius, Aenigmata 61; late 4th or 5th c.
- **English by:** Elizabeth Hickman du Bois, 1912
- **Source:** E. H. du Bois, The Hundred Riddles of Symphosius (1912), riddle LXI; the answer is the title in the manuscripts
- **Who would ask it:** a clerk of the toll; a monk who has crossed the sea (peoples: frankish, english, irish_gaelic, frisian)
- **In the game as:** riddle_symphosius_61_ancora
- **Note:** The source of Exeter 16. An iron anchor with two flukes: the Roman kind, and the kind found with the Ladby and Oseberg ships.

#### 41. Pecunia

> Earth was I at first, 'twixt rugged boulders caught,  
> Now fire another name and price has taught,  
> And earth no more I'm called, yet with me earth is bought.  

- **Answer:** Money (coin)
- **Also take:** money; coin; silver; a penny; pennies; gold; pecunia; a coin
- **From:** Symphosius, Aenigmata 91; late 4th or 5th c.
- **English by:** Elizabeth Hickman du Bois, 1912
- **Source:** E. H. du Bois, The Hundred Riddles of Symphosius (1912), riddle XCI; the answer is the title in the manuscripts
- **Who would ask it:** a moneyer at the mint; a toll clerk weighing silver (peoples: frankish, english, frisian)
- **In the game as:** riddle_symphosius_91_pecunia
- **Note:** Ore out of the ground, struck into coin, and land bought with it. The right riddle for a mint (Dorestad, Quentovic, Lundenwic, Hedeby after c. 825).

#### 42. Somnus

> Unbidden do I come, in varied guise,  
> Without the truth, I empty dreams devise,  
> And me no mortal sees unless he shuts his eyes.  

- **Answer:** Sleep
- **Also take:** sleep; a dream; dreams; somnus
- **From:** Symphosius, Aenigmata 99; late 4th or 5th c.
- **English by:** Elizabeth Hickman du Bois, 1912
- **Source:** E. H. du Bois, The Hundred Riddles of Symphosius (1912), riddle XCIX; the answer is the title in the manuscripts
- **Who would ask it:** a monk at the night office; a watchman (peoples: frankish, english, irish_gaelic)
- **In the game as:** not there yet
- **Note:** Not in the game. Good for a night card at anchor.

### Aldhelm

#### 43. Ventus

> None can espy me, none lay hands on me;  
> My rushing voice shrills swift through all the earth.  
> I shatter oaks with harsh and hideous might,  
> Yea, beat upon the skies, and sweep the fields.  

- **Answer:** The wind
- **Also take:** wind; the wind; a storm; gale; ventus
- **From:** Aldhelm, Enigmata 2; c. 685-695, in the letter to 'Acircius' (King Aldfrith of Northumbria)
- **English by:** James Hall Pitman, 1925
- **Source:** James Hall Pitman, The Riddles of Aldhelm: Text and Verse Translation (Yale Studies in English 67; New Haven: Yale University Press, 1925), no. 2, pp. 4-5; the answer is Aldhelm's own title
- **Who would ask it:** a schoolmaster of a minster; any skipper would like it (peoples: english, frankish, irish_gaelic)
- **In the game as:** riddle_aldhelm_02_ventus
- **Note:** The game has the Latin only. Aldhelm was abbot of Malmesbury and bishop of Sherborne (d. 709 or 710); his hundred riddles were a schoolbook in England and Francia.

#### 44. Salis

> Once I was water, full of scaly fish;  
> But, by a new decision, Fate has changed  
> My nature: having suffered fiery pangs,  
> I now gleam white, like ashes or bright snow.  

- **Answer:** Salt
- **Also take:** salt; sea salt; sal
- **From:** Aldhelm, Enigmata 19; c. 685-695
- **English by:** James Hall Pitman, 1925
- **Source:** J. H. Pitman, The Riddles of Aldhelm (1925), no. 19, pp. 12-13; the answer is Aldhelm's own title
- **Who would ask it:** a salt-boiler on the coast; a cellarer buying salt (peoples: english, frankish, frisian)
- **In the game as:** riddle_aldhelm_19_salis
- **Note:** Sea water boiled over a fire: how salt was made on the Frisian and English coasts. Salt is a trade good in the game.

#### 45. Piper

> Black is my outside, clothed in wrinkled bark,  
> But inside you will find a snowy pith.  
> I season dainty food: rich, royal feasts,  
> And have a place in every soup and stew;  
> But no such virtue will you find in me  
> Unless you crush my shining inward parts.  

- **Answer:** Pepper
- **Also take:** pepper; a peppercorn; peppercorn; black pepper; piper
- **From:** Aldhelm, Enigmata 40; c. 685-695
- **English by:** James Hall Pitman, 1925
- **Source:** J. H. Pitman, The Riddles of Aldhelm (1925), no. 40; the answer is Aldhelm's own title
- **Who would ask it:** a monk of Saint-Denis at the fair; a bishop's steward (peoples: english, frankish)
- **In the game as:** riddle_aldhelm_40_piper
- **Note:** Pepper reached the north through the Frankish fairs; Bede left his pepper to his brethren when he died (735). A rare, dear good: a riddle for the top of the market.

#### 46. Calix vitreus [double meaning]

> In sluggish stream I flowed from rifted rocks,  
> As flames broke up the stones, and fire applied  
> The unleashed ardor of the furnace-heat;  
> My form capacious now is clear as ice.  
> Yea, many long to hold me in their hand,  
> Fingering my slippery shape in dainty grasp;  
> But I befool their minds, the while I lay  
> Sweet kisses on their lips that press me close,  
> And urge their tottering footsteps to a fall.  

- **Answer:** A glass cup
- **Also take:** glass cup; a glass; glass; glass beaker; cup; a drinking glass; beaker; wine glass
- **From:** Aldhelm, Enigmata 80; c. 685-695
- **English by:** James Hall Pitman, 1925
- **Source:** J. H. Pitman, The Riddles of Aldhelm (1925), no. 80, pp. 46-49; the answer is Aldhelm's own title
- **Who would ask it:** a glass trader from the Rhine; a bishop's steward (peoples: english, frankish)
- **Double meaning:** In part, and gently: the bishop lets the cup talk like a woman who is held, kissed and brings men down. The Exeter Book's glass beaker (no. 63) takes the same joke further.
- **In the game as:** riddle_aldhelm_80_calix_vitreus
- **Note:** Rhineland glass was one of Dorestad's goods: funnel beakers went from there to Hedeby, Birka and Kaupang.

#### 47. Arca libraria

> My inwards overflow with words divine,  
> And sacred volumes crowd my vital parts,  
> But from them I can never learn one whit:  
> Unhappy creature, robbed of such a gift,  
> By my grim fate denied the light of books.  

- **Answer:** A book chest
- **Also take:** book chest; bookcase; a book chest; book box; a chest of books; library; book cupboard
- **From:** Aldhelm, Enigmata 89; c. 685-695
- **English by:** James Hall Pitman, 1925
- **Source:** J. H. Pitman, The Riddles of Aldhelm (1925), no. 89, pp. 52-53; the answer is Aldhelm's own title
- **Who would ask it:** a librarian monk; the priest who wants a book carried (peoples: english, frankish, irish_gaelic)
- **In the game as:** riddle_aldhelm_89_arca_libraria
- **Note:** Books were kept and carried in chests, not on shelves: Pitman's 'bookcase' is the wrong picture. Fits the book-carrying errands in the game.

#### 48. Farus editissima

> On high crags, where the blue seas pound the reefs,  
> And briny billows swell the heaving flood,  
> Mechanic art has built me, great and high,  
> For ships a guiding finger to reveal  
> The open channel. Yet I never roam  
> The level sea in ships of many oars,  
> That cut a curving furrow through the deep;  
> But, pointing from my pinnacle, I lead  
> Those wanderers buffeted by mountain-waves  
> Safely to shore, lifting a fiery brand,  
> For high upon my tower a torch I set,  
> When wintry clouds conceal the flaming stars.  

- **Answer:** A lighthouse
- **Also take:** lighthouse; a lighthouse; beacon; the old roman lighthouse; pharos; a sea mark; light tower
- **From:** Aldhelm, Enigmata 92; c. 685-695
- **English by:** James Hall Pitman, 1925
- **Source:** J. H. Pitman, The Riddles of Aldhelm (1925), no. 92, pp. 54-55; the answer is Aldhelm's own title
- **Who would ask it:** a monk at Dover or Boulogne, under the Roman towers (peoples: english, frankish)
- **In the game as:** riddle_aldhelm_92_farus
- **Note:** The Roman lighthouses at Dover and Boulogne still stood: Charlemagne had the Boulogne tower repaired and its fire lit again in 811 (Royal Frankish Annals). The game's palette already has 'the old Roman lighthouse'.

### Alcuin, the Disputation of Pippin with Albinus

#### 49. What is snow?

> What is snow?  

- **Answer:** Dry water.
- **Also take:** dry water; water that is dry
- **From:** Alcuin, Disputatio regalis et nobilissimi juvenis Pippini cum Albino scholastico; c. 790-800, for Pippin of Italy, Charlemagne's son
- **English by:** Andrew Fleming West, 1892
- **Source:** Andrew Fleming West, Alcuin and the Rise of the Christian Schools (New York: Scribner, 1892), p. 107; Latin in Patrologia Latina 101, cols 975-980
- **Who would ask it:** a schoolboy of the court to his master, or the master to the boy (peoples: frankish, english)
- **In the game as:** not there yet
- **Note:** The Disputatio runs the other way from a riddle: the thing is named and the answer is the dark saying. To set it as a riddle, turn it round: 'What is dry water?' The game's sayings file drafts more of these pairs (say.alc.*).

#### 50. What is frost?

> What is frost?  

- **Answer:** The persecutor of plants; the destruction of leaves; the bond of the earth; the source of waters.
- **Also take:** the persecutor of plants; the bond of the earth; the destruction of leaves; the source of waters; the bridge of the waters
- **From:** Alcuin, Disputatio Pippini cum Albino; c. 790-800
- **English by:** Andrew Fleming West, 1892
- **Source:** A. F. West, Alcuin and the Rise of the Christian Schools (1892), p. 107
- **Who would ask it:** a schoolmaster at the palace or a minster school (peoples: frankish, english)
- **In the game as:** not there yet
- **Note:** Turned round: 'Who persecutes the plants, destroys the leaves and binds the earth?' West's last phrase is 'the source of waters'; other printings of the Latin have 'the bridge of the waters' (pons aquarum) (?), which is the better riddle and true of a frozen Rhine.

#### 51. What is man like?

> What is man? The slave of death; a passing traveler; a stranger in his place. What is man like?  

- **Answer:** An apple.
- **Also take:** an apple; apple; an apple on a tree; fruit on a tree
- **From:** Alcuin, Disputatio Pippini cum Albino; c. 790-800
- **English by:** Andrew Fleming West, 1892
- **Source:** A. F. West, Alcuin and the Rise of the Christian Schools (1892), pp. 106-107, with West's gloss: 'man hangs like an apple on a tree without being able to know when he is to fall'
- **Who would ask it:** a monk, to a merchant who has just lost a friend or a ship (peoples: frankish, english)
- **In the game as:** not there yet
- **Note:** The Latin goes on: 'How is man placed? Like a lantern in the wind.' (the game's own rendering (?))

#### 52. The dead who bore the living

> I saw the dead give birth to the living, and the anger of the living ate up the dead.  

- **Answer:** Fire, from wood rubbed on wood
- **Also take:** fire; fire from rubbing sticks; a fire of dry wood; fire from wood; need-fire; flame
- **From:** Alcuin, Disputatio Pippini cum Albino, the riddles at the end; c. 790-800
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin as remembered, to check against Patrologia Latina 101, cols 975-980 (?): 'Vidi mortuos generare vivum, et in ira vivi consumpti sunt mortui.' Albinus answers: 'Fire was born from the rubbing of trees, and it eats the trees.' West 1892 does not translate this part ('the dialogue rapidly runs into puzzles and then closes').
- **Who would ask it:** a schoolboy of the court; a clerk at a fireside (peoples: frankish, english)
- **In the game as:** not there yet
- **Note:** Fire made by friction was real practice: the 'need-fire' (nodfyr) is forbidden as a heathen custom in the Frankish Indiculus superstitionum (c. 743).

#### 53. The stranger without a tongue

> A stranger spoke with me without tongue or voice. He never was before and never will be again, and I neither heard him nor knew him.  

- **Answer:** A dream
- **Also take:** a dream; dream; a man in a dream; sleep
- **From:** Alcuin, Disputatio Pippini cum Albino, the riddles at the end; c. 790-800
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin as remembered, to check against Patrologia Latina 101, cols 975-980 (?): 'Quidam ignotus mecum sine lingua et voce locutus est, qui nunquam ante fuit, nec postea erit, et quem non audiebam nec novi.' Albinus answers: 'Perhaps a dream troubled you, my son.'
- **Who would ask it:** a schoolboy of the court; a passenger on board, at dawn (peoples: frankish, english)
- **In the game as:** not there yet
- **Note:** Pair it with Symphosius 99 (sleep).

#### 54. Three men

> Three there were: one never born and once dead; one once born and never dead; one once born and twice dead.  

- **Answer:** Adam, Elijah (or Enoch) and Lazarus
- **Also take:** adam elijah lazarus; adam, elijah and lazarus; adam, enoch and lazarus; adam enoch lazarus; adam, elias and lazarus
- **From:** Alcuin, Disputatio Pippini cum Albino, the riddles at the end; c. 790-800
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin as remembered, to check against Patrologia Latina 101, cols 975-980 (?): 'Tres fuere: unus numquam natus et semel mortuus; alter semel natus et numquam mortuus; tertius semel natus et bis mortuus.' Albinus answers with a hint for each name.
- **Who would ask it:** a priest, to test whether a stranger is truly a Christian (peoples: frankish, english, irish_gaelic)
- **In the game as:** not there yet
- **Note:** A Bible question in riddle dress, of the kind called Joca monachorum (monks' jokes). A heathen cannot answer it: use it as a test of faith, not of wit.

### Tatwine

#### 55. De membranis

> A savage robber stripped me of my covering and took the breath of life from me as well. But a maker turned me afterwards into a level field. A tiller soon waters its fruitful furrows, and my meadows give back a harvest of every kind of balm, with which I bring food to the healthy and healing to the hurt.  

- **Answer:** Parchment
- **Also take:** parchment; vellum; a book; a page; calfskin; a skin for writing
- **From:** Tatwine, Aenigmata 5; before 734 (Tatwine, a Mercian, archbishop of Canterbury 731-734)
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin (T. Wright, Anglo-Latin Satirical Poets, vol. II, Rolls Series 59, 1872, p. 526; text in texts_793.json, tatwine_05_membranis). No English translation in the public domain is known. The answer is the title in the manuscripts.
- **Who would ask it:** a scribe of Christ Church, Canterbury (peoples: english)
- **In the game as:** riddle_tatwine_05_membranis
- **Note:** The same story as Exeter 26, in six lines: the beast killed, the skin made a field, the pen its ploughman, the ink its rain.

#### 56. De nive, grandine et glacie

> A father in the sky begot us three sisters of one mother, under a hard law: by our father's grudge it is our lot to be broken (?). Yet under such a law we have one hope: that we soon go back into our mother's royal womb.  

- **Answer:** Snow, hail and ice
- **Also take:** snow hail and ice; snow, hail and ice; snow hail ice; snow, hail, ice; ice snow and hail; hail snow ice
- **From:** Tatwine, Aenigmata 15; before 734
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin (Wright 1872, vol. II, p. 529; text in texts_793.json, tatwine_15_nive_glacie). The third line is hard and the rendering of it is unsure (?). The answer is the title in the manuscripts.
- **Who would ask it:** a cleric of the archbishop; a monk in winter (peoples: english)
- **In the game as:** riddle_tatwine_15_nive_glacie
- **Note:** The mother is water, the father the cold sky. Three answers are wanted.

#### 57. De coticulo

> They say I was born from the cold belly of the earth; yet long ago the Romans also called me a famous city. Now I lie dirty and unarmed under the feet of those who tread on me. He who gnaws at me and licks me long laughs with a sharp edge.  

- **Answer:** A whetstone
- **Also take:** whetstone; a whetstone; hone; a hone; sharpening stone; cos
- **From:** Tatwine, Aenigmata 39; before 734
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin (Wright 1872, vol. II, p. 534; text in texts_793.json, tatwine_39_coticulo). The answer is the title in the manuscripts.
- **Who would ask it:** a schoolmaster: it cannot be solved without Latin (peoples: english, frankish)
- **Word-play:** A pun in Latin: cos is a whetstone, and Cos is the Greek island city. In English the second line is only a hint that the answer is a word with two meanings.
- **In the game as:** riddle_tatwine_39_coticulo
- **Note:** Whetstones of Norwegian schist (Eidsborg) were a real cargo of the age, found at Kaupang, Hedeby, Ribe and Dorestad. A Latin-reading buyer of whetstones might well ask it. 'He who licks me' is the blade.

### Eusebius

#### 58. De aequore

> I am moved, I run, I carry swiftly, and I never leave my seat. Thin and wandering as I am, I bear very great loads. Snow does not cover me, hail does not weigh on me, frost does not master me. Many lie spread upon me, but more within.  

- **Answer:** The sea
- **Also take:** the sea; sea; ocean; water; the deep
- **From:** Eusebius, Aenigmata 23; first half of the 8th c. (Eusebius is taken to be Hwaetberht, abbot of Wearmouth-Jarrow from 716 (?))
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin (J. A. Giles, Anecdota Bedae, Lanfranci et aliorum, London 1851, p. 58; text in texts_793.json, eusebius_23_aequore; the first word of the Latin is doubtful in the scan (?)). No English translation in the public domain is known. The answer is the title in the manuscripts.
- **Who would ask it:** a monk of Jarrow or Wearmouth, looking at the sea from the cloister (peoples: english)
- **In the game as:** riddle_eusebius_23_aequore
- **Note:** The last line: ships and birds lie on it, fish and the drowned within. Eusebius added sixty riddles to Tatwine's forty to make the hundred.

#### 59. De cera

> My face is level, and a sharp ploughshare cuts it; yet, furrowed, I stay for ever without seed. Cut, I am then pressed smooth, and am the fairer for it. Now I hold words; now, often, I drive back the dark.  

- **Answer:** Wax (the wax tablet and the candle)
- **Also take:** wax; beeswax; a wax tablet; wax tablet; a candle; candle; writing tablet
- **From:** Eusebius, Aenigmata 31; first half of the 8th c.
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin (Giles 1851, p. 59; text in texts_793.json, eusebius_31_cera). The answer is the title in the manuscripts.
- **Who would ask it:** the cellarer who buys wax; a toll clerk with his tablets (peoples: english, frankish)
- **In the game as:** riddle_eusebius_31_cera
- **Note:** Two uses in one riddle: the writing tablet scored by the stylus and smoothed again, and the candle. Wax is a Baltic and Frankish trade good in the game, and tablets are what a merchant's tallies were kept on. Compare Aldhelm 32 (writing tablets).

#### 60. De pisce

> I do not fly the sky on wings, nor wander the fields on foot. I have neither hands nor feet, and a little fin holds me up. I swim through sounding waters and the swollen blue; and I cross the starry pole and the high seat of heaven as well.  

- **Answer:** A fish
- **Also take:** fish; a fish; the fishes; pisces; herring
- **From:** Eusebius, Aenigmata 40; first half of the 8th c.
- **English by:** the game's own draft from the original (for the user to edit), 2026
- **Source:** The game's own draft from the Latin (Giles 1851, p. 60; text in texts_793.json, eusebius_40_pisce). The answer is the title in the manuscripts.
- **Who would ask it:** a monk who reckons time: he knows the signs of the sky (peoples: english)
- **In the game as:** riddle_eusebius_40_pisce
- **Note:** The last line is the trick: the fish is also in the sky, as the sign of the Fishes. A monk who reckons Easter by Bede's book would ask it.

## 6. Where each could be asked

- **At sea, hailing a ship:** the anchor three ways (Exeter 16, Gestumblindi, Symphosius 61), fog, the waves, the
  ship (Exeter 32, Symphosius 13), the wind (Aldhelm 2), the sea (Eusebius 23), the barnacle goose, the iceberg.
- **At a quay or a toll:** the merchant ship (Exeter 32), money (Symphosius 91), wax (Eusebius 31), the river and
  its fish, the key, the one-eyed garlic seller.
- **At a market:** salt, pepper, the glass cup, mead, ale, the onion, the leek, the whetstone, the cow, the sow.
- **In a hall at night:** the horn, the bow, mead, ale, and every riddle with a double meaning.
- **At a minster or a school:** the book, the bookworm, parchment, the book chest, Lot's family, the three men,
  the fish in the sky, all of Alcuin.
- **In winter, iced in:** ice four ways (Exeter 68-69, Symphosius 10, Tatwine 15, Alcuin's frost), snow two ways,
  the dead horse on the floe.

Riddles that travelled are the best lore in the set, because the player can meet the same riddle in three
harbours and three tongues: the anchor (Latin, English, Norse); the fish and its river (Symphosius, the Exeter
Book, Alcuin); the bookworm (Symphosius, the Exeter Book); the pregnant sow (Aldhelm, Gestumblindi); the onion
(Symphosius, the Exeter Book, and the leek of Gestumblindi); ice as its own mother and daughter (Symphosius,
Tatwine, the Exeter iceberg).

## 7. Sources

Texts read for this file (scans at the Internet Archive, read as plain text; the item names are the archive's):

- Benjamin Thorpe, *Codex Exoniensis: A Collection of Anglo-Saxon Poetry* (London: Society of Antiquaries, 1842).
  Thorpe died in 1870.
- Albert S. Cook and Chauncey B. Tinker (eds), *Select Translations from Old English Poetry* (Boston: Ginn,
  copyright 1902), pp. 70-75, riddles translated by Herbert B. Brougham. archive.org: selecttranslatio01cookuoft.
- Stopford A. Brooke, *The History of Early English Literature* (London and New York: Macmillan, 1892).
  archive.org: historyofearlyen00broouoft. Brooke died in 1916.
- Frederick Tupper, *The Riddles of the Exeter Book* (Boston: Ginn, 1910): Old English text and the answers; no
  translation. archive.org: riddlesofexeterbook00tuppuoft.
- N. Kershaw, *Stories and Ballads of the Far Past* (Cambridge University Press, 1921), pp. 113-126.
  archive.org: storiesballadsof00chaduoft. Nora Kershaw Chadwick died in 1972.
- G. Vigfusson and F. York Powell, *Corpus Poeticum Boreale* (Oxford, 1883), vol. I, pp. 86-92: the game's
  present Gestumblindi.
- Elizabeth Hickman du Bois, *The Hundred Riddles of Symphosius* (Woodstock, Vermont: Elm Tree Press, 1912).
  archive.org: hundredriddlesof00symp.
- James Hall Pitman, *The Riddles of Aldhelm: Text and Verse Translation with Notes* (Yale Studies in English 67;
  New Haven: Yale University Press, 1925). archive.org: riddlesofaldhelm0000aldh.
- Andrew Fleming West, *Alcuin and the Rise of the Christian Schools* (New York: Scribner, 1892), pp. 106-107.
  archive.org: alcuinriseofchri00west. West died in 1943.
- Alcuin, *Pippini regalis et nobilissimi juvenis disputatio cum Albino scholastico*, Patrologia Latina 101,
  cols 975-980 (Latin; not read for this file: the three drafts from it are from memory and marked).
- Tatwine: T. Wright, *The Anglo-Latin Satirical Poets and Epigrammatists of the Twelfth Century*, vol. II (Rolls
  Series 59, 1872), Appendix I. Eusebius: J. A. Giles, *Anecdota Bedae, Lanfranci et aliorum* (London, 1851). Both
  Latin only; the Latin of the six riddles used is in `texts_793.json`.

## 8. Not checked

- Every quotation was taken from machine-read scans, cleaned by hand. Check each against the page image before it
  ships. Two words in the poker riddle (Thorpe's italics) and one in the swallows ("sweet of song") were unclear
  in the scan.
- Death years of Herbert B. Brougham, Elizabeth Hickman du Bois and James Hall Pitman were not found. All three
  books were printed before 1931; whether the translators died before 1956 is open (?).
- Page numbers for Brooke's barnacle goose (p. 179), for du Bois (not paged here) and for Pitman's pepper.
- The Latin of the three Alcuin drafts (fire, dream, three men) is from memory, as is "pons aquarum" in the note
  on frost. The sayings writer's drafts use the same remembered Latin; one check against Patrologia Latina 101
  settles both.
- Tatwine 15, line 3: the sense is unsure.
- The dates given for when chess reached the north.
