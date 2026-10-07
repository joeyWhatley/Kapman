# The tongues: how to use the data, and what is not yet sure

7 October 2026. One page for the builders and for whoever checks the words before release. The data is
`Assets/Kapman/Game/Data/story/tongues_793.json`: 11 tongues with a kin table, 318 words, 111 real lines. The
writer's and editor's list is `period_words.md`, beside this file. The rule is section 6c of
`Design/fate_faith_and_self.md`: nothing is invented and passed off as old.

## How the game should use it

**1. The player's own tongue is plain English.** Speech in the first tongue of the player's people is written
plain, with the period's own things by their names (toll, thing, wergild, wic). Words are sprinkled only in the
mouths of people whose tongue is not the player's: the foreign word is what makes them foreign.

**2. A sprinkled word.** At most one to a line of speech, and not in every line: about one line in four of a
foreign speaker. It comes from `words` where `tongue` is the speaker's (the line's `tongue`, else the actor's).
Take greetings, farewells, thanks, oaths, curses, kin and trade words first, as 6c says; a ship, sea, law, faith,
food or rank word only when the thing itself is in the scene. The `use` sentence shows the fit: the word drops
into an English sentence that still reads without it. Set it apart (italic); a touch or hover shows `word`,
`say`, `means` and the tongue's `name`. Never sprinkle in the narrator's voice. A writer may only use words that
are in the file.

**3. A line that turns.** Only a line from `lines`, never a made one, and at most one to a card. The moments, from
6c: a first meeting, an oath, a verse, a prayer, a dying word, an hour of history. `who_would_say_it` and
`when_it_fits` say which. `not_before` gives the year before which a dated line must not be spoken as a living
line; in the first life (793 to 805) that leaves the verse, scripture, prayers and proverbs, the vow, and the news
of 793 to 798. A tongue with no lines (Frisian, Wendish, Baltic, Finnic) never turns: its people speak English
with sprinkled words, and for a verse or an oath a Frisian may be given an Old English or Old Saxon line, said to
be that in the credits.

**4. How it turns.** The line comes up whole in `original`. Then each pair turns in order: the left side fades
and the right side takes its place, so the line reads as English at every moment after the first. The user asked
for it quick: about a tenth of a second a pair for a tongue the player knows, the whole line done in under a
second and a half. Show the turned text from `pairs`. Where `pairs_are` is "plain", the credited translation
(`english`, `english_by`) goes in the chest and the credits, not in the speech place.

**5. Skill changes what is seen.** Let *s* be the player's skill in the line's tongue, the greatest of: his own
`tongue.<id>`; his skill in a tongue he does speak times the `follows` number in that tongue's `kin` row for this
one (a Frisian at 1.0 follows English at 0.6); and, when a crewman or passenger speaks it, that man's skill.

| *s* | What the player sees |
|---|---|
| 0.8 and over | Every pair turns, quickly, in order. |
| 0.3 to 0.8 | Pairs turn slowly (three to five times slower). A share of them, 1 minus *s*, do not turn: the longest pairs first, since the long ones carry the catch. The player gets the gist and may miss the condition in an oath. |
| under 0.3 | Nothing turns but single words he has learned (below). The card's choices that need the meaning are shut, or shown as guesses. |
| through a man aboard | It turns after a beat, marked as that man's telling. A card may give its own `as_told` text for a man with reasons to shade it; where it gives none, the plain pairs are used. |

Latin is never reached by kinship (only by `learn latin`), except that a Romance speaker half follows it.

**6. Learning by hearing.** Keep a count for each word and each pair the player has seen turn or has hovered.
At ten the word is his: it shows in English from the start, and a sprinkled word no longer needs its hover. Every
ten words learned in a tongue may add a little to `tongue.<id>`, so that a merchant who winters in Hedeby comes
home half a Dane.

**7. Three things the data does that 6c did not ask for.** `say` is a plain sounding for the hover, for those who
want to read aloud (no sound is made or implied). `text_id` ties a line to the same text in the chest, so the
chest can open at that line. `own` and `check` flag what is the game's own English and what was quoted from
memory.

## What could not be verified

No web search was possible (the session's allowance was spent), so everything rests on pages fetched by name and
on the chest. Marks in the file: `(?)` on 95 of the 318 words, and `check: true` on 7 lines.

- **Dictionary entries not opened today.** Every `where_attested` that names Cleasby-Vigfusson, Bosworth-Toller,
  the Dictionary of the Irish Language, Hofmann and Popkema, or "Old High German glosses" carries (?): the word is
  given as I know the dictionary has it, not as read. The surest are the commonest (*þökk*, *jarl*, *mörk*,
  *pening*, *toll*, *hlāf*, *bó*); the least sure are *troll hafi þik* as a set phrase, *velkominn*, *kāpmon* as an
  attested Old Frisian compound, *koufman*, *fo chen* and *buide* in the exact senses given.
- **Old Frisian.** Twenty of the 29 words were seen in Wikipedia's article "Old Frisian" (after Bremmer, *An
  Introduction to Old Frisian*, 2009), not in Bremmer or in a law book; the other nine are from memory. All are
  13th-century forms but *kabu* (the rune combs of Oostum and Toornwerd), which is from memory too.
- **Wendish.** All 15 entries are (?): six names as Latin writers gave them, nine Old Church Slavonic kin words in
  the usual scholarly spelling. None was read in a source today.
- **Line numbers.** Beowulf, The Wanderer, The Seafarer and the Maxims are by the standard numbering; the Heliand
  by the Bibliotheca Augustana's; the Durham Proverbs by Wikipedia's table. The Battle of Maldon is cited by
  phrase, without line numbers.
- **The Paris Conversations** are dated to the 9th, 10th or 11th century by different scholars; only sentences
  51 to 58 were seen. The rest of that phrase book (it has greetings, a question about last night's lodging, and
  some famous insults) would be the best single addition to the Frankish list once someone has Braune's
  *Althochdeutsches Lesebuch* open.
- **Kin numbers** are a judgment, not a measurement.

## What a scholar should check before release

1. **The seven lines with `check: true`**: `oe_thane_right` (Old English from memory of Liebermann);
   `la_alcuin_793` (the chest's Latin, from memory: MGH Epistolae IV no. 16); `la_alcuin_ship` (Alcuin's dialogue
   with Pippin, from memory); `la_mass_dominus`, `la_mass_ite`, `la_mass_agnus`, `la_gloria_patri` (the Mass and
   the doxology from memory: sure in substance, but was each in use, in this form, in a Frankish or English
   church of 800?).
2. **Editions behind the originals.** The old texts are free, but four were taken from modern editors' texts and
   should be compared with a print older than 1931: Hávamál (Guðni Jónsson's normalised text; compare Bugge 1867
   or Finnur Jónsson); The Seafarer and the Maxims (the Anglo-Saxon Poetic Records text; compare Thorpe 1842 or
   Grein-Wülker); the Heliand (compare Sievers 1878); the Durham Proverbs (first printed by Arngart in 1956: the
   Old English is 11th century, the length marks are an editor's, and may be dropped). The rune stones are in the
   Rundata normalisation: credit it. The Annals of Ulster are in the 1983 edition's text at CELT: compare
   Hennessy 1887.
3. **Rights in the English.** Free by the rule (translator dead before 1956, printed before 1931): Bellows, Hall,
   Hickey, Thorpe, Giles, Bosworth, Todd, Stokes and Strachan, Meyer, Gummere, Challoner's Douay-Rheims. **Not
   settled:** Lola La Motte Iddings (The Seafarer, three lines): her death year is not found, as the chest already
   says. **Avoided:** the Book of Common Prayer and the King James Bible, which are under Crown rights in the
   United Kingdom; the Lord's Prayer is given from Douay-Rheims and the Gloria in the game's own words.
4. **The 41 lines marked `own`** are the game's English, for the user to edit: all eight Durham Proverbs, six of
   the Maxims, the four stones, the vow, the Heliand, the Strasbourg oaths, the Annals of Ulster, the Rule, the
   Mass. A free translation exists for some (Thorpe for the Exeter Maxims, Hennessy for the annals, Verheyen for
   the Rule) and was not at hand.
5. **The 24 lines with plain pairs.** Each was made word for word from the original for this file, because the
   credited translation reorders or says thee and thou. They are where a slip of sense is likeliest: Beowulf (ten lines),
   The Wanderer 1-5 (*ārǣd* is disputed: "fixed", "resolute"), Hávamál 3, the St Gall quatrain (*mora minn*: "a
   clear sea", or the name of the Irish Sea), the Breastplate (*etrochta ésci*: Stokes "moon", Todd "snow").
6. **Sense and period of single words:** *prímsigning* (Egils saga 50, of the 930s: was it done in 793?);
   *grið* in English before the Danish settlements; *massere*; *wīcing* (pirate, never a people); *Northman* in
   the Ludwigslied; *līf* as wergild in Frisian; the sounding given in `say` for Old Irish and Old Norse.
7. **The spelling of Old Norse for a Dane of 793.** The dictionary forms are 13th-century Icelandic. If the game
   wants the Danes of the first life to sound of their own time, the stones' forms (*felaga*, *drængʀ*,
   *styrimannr*, *kunungʀ*) are the nearest true ones, and they are a century or two later still.

## Pages read on 7 October 2026

heimskringla.no (Hávamál, Vafþrúðnismál, Guðni Jónsson's text); sacred-texts.com (Bellows's Poetic Edda, poe04
and poe05; the Anglo-Saxon Poetic Records texts a03_09 The Seafarer, a03_13 Maxims I, a15 Maxims II); drbo.org
(the Vulgate beside Douay-Rheims: Psalms 22, 30, 68, 69, 76, 92, 106, 120, 129, 138; Matthew 6, 8, 13, 25; Mark 8;
Luke 23; Proverbs 11; Ecclesiasticus 26; Ruth 2; James 4); hs-augsburg.de, Bibliotheca Augustana (Heliand, fitts
4, 14, 15, 19, 27, 35); celt.ucc.ie (Annals of Ulster, G100001A); thelatinlibrary.com (the Rule of Benedict);
Wikipedia, in source form (Old Saxon Baptismal Vow, The Durham Proverbs, Oaths of Strasbourg, Old High German,
Hildebrandslied, Ludwigslied, Paris Conversations, Kassel conversations, Pangur Bán, Saint Patrick's Breastplate,
Old Frisian, Jelling stones, Glavendrup stone, Stone of Eric, Baltic area runestones); Wikisource (Pangur Bán in
Stokes and Strachan's English). In the project: `texts_793.json` and `Design/content/lore/texts_more.json`.
