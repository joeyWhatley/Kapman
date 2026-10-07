# Town games and pastimes, 793 to 1066

4 October 2026. Data: `Assets/Kapman/Game/Data/story/town_games_793.json` (ten games, each with its evidence, what
is not known, complete rules, stakes, what it gives, sources, and a line for the artist). A draft for you to
overwrite. `(?)` marks what could not be checked.

## What goes on screen

| Game | Act | On screen | Takes | Stake | Gives |
|---|---|---|---|---|---|
| The high throw (dice, knucklebones) | `dice` | cards, two dice drawn on the card | an hour | 1 to 12 d a round, 60 d in a hall | silver, a rumour, a cheat shown up |
| Tafl, the king's table | `tafl` | **a board**: 9 by 9, 25 pieces (7 by 7 in Gaelic ports) | an evening | 1 to 60 d or a thing | silver, hall standing, a tafl friend, a riddle |
| Mill (nine men's morris) | `tafl` | **a board**: 24 points, 18 pieces | an hour | a cup, a penny | harbour talk, a small friend, crew heart |
| A riddle match | `riddles` | cards (the chest's palette answers) | an evening | a cup a miss, a ring | `won_riddle`, a new riddle always |
| The harp goes round | `song` | cards | an evening | pride | a gift, a new text, the host |
| The cups (sumbel) | `drink` | cards | an evening (and a morning) | your word | standing from a kept vow, a rumour, crew heart |
| Matching men (flyting) | card at drink | cards | an hour | your name | standing, your deed told abroad, a friend or a feud |
| Wrestling, green or water | card at a fair or quay | cards | an hour | wagers 2 to 24 d | silver, a name, crew heart |
| The horse fight | card at a Norse gathering | cards | half a day | 4 to 40 d | silver, a district's news, a friend and an enemy |
| A day with the hawks | card after a gift or audience | cards | a day | a day | a lord's ear, a falcon commission |

**Two little boards to draw and code: tafl and mill** (the two faces of the Gokstad board). The dice want only two
dice drawn on a card. The other seven are runs of three to six cards on the engine as it stands. The four contests
have no act id; they come as cards (see `needs` in the data file).

## What is known, and what is not

- **The rules of tafl are reconstructed.** Boards and pieces survive (Gokstad 13 by 13, Ballinderry 7 by 7, Birka,
  Salme), and riddles about them. Nobody of the time wrote the rules. The only eyewitness rules are Linnaeus's notes
  on the Sami game tablut in 1732, and the 1811 English of them has a mistake most modern sets carry (a king taken
  only on four sides). The game follows Linnaeus as now read, on his 9 by 9 board, and says on the board that it is
  a reconstruction.
- **No dice rule survives** beyond the kings at Konungahella about 1020: two dice, highest throw wins, ties thrown
  again (Heimskringla). "Stand or throw again" is the game's own.
- **Mill's rules** are first written in 1283. **Riddle matches** of ordinary people are not recorded: the poems stake
  heads. **The sumbel** as a rite is Snorri, three hundred years late. **Horse fights** are Icelandic sagas and one
  carved stone. **Wrestling holds** are later Iceland. **Hawking** is sure for lords (letters, Asser, Maldon, the
  Bayeux Tapestry) but no day of it is described.
- **At home in Dorestad** the evidence gives dice, the harp (the blind Frisian singer Bernlef), riddles among
  clerks, the cups, wrestling and swimming (Charlemagne's own sport). Tafl comes with Danes. Franks played tables
  (dice on a board), left out until someone has the sources.

## Each game in play, with three turns

**1. The high throw.** Lay equal stakes; each throws two dice and may stand or throw once more; higher takes both.
- You ask Radward, a skipper in from Witla, for a throw: 4 d each on the hatch. He throws 6 and stands. You throw 5: *stand, or throw
  again (likely to beat him)*. You throw again: 9. Yours.
- You throw first now: 7, and stand. He throws 4, takes the dice up, throws 8. His.
- You ask for double. He won the last, so he throws first: 10, and he stands. You throw 3: *only an eleven or better will do*. You throw again:
  11. He laughs, pays, and tells you the toll man at Witla was changed at Easter.

**2. Tafl.** The king and eight dark men in the middle, sixteen fair men at the sides. All move in straight lines; a
man caught between two enemies is taken; the king wins at any edge. Ketill's board, in an ale-house at Hedeby; he
takes the fair men and gives you the king.
- Ketill brings d1 up to d3, beside your man at e3. You bring c5 down to c3: his man stands between c3 and e3 and
  comes off.
- Ketill slides a4 to c4, above your man at c3, meaning to close on him from c2. You step d5 across to c5: his man
  at c4 is between c5 and c3 and comes off.
- Ketill runs d9 all the way down to d5: your man at c5 is between it and b5, and comes off. You bring c3 up to c5:
  his man at d5 is now between c5 and the king, and comes off. The d column is empty from edge to edge. Next turn
  the king can step onto it with a road both ways; Ketill has one move to stop that, and looks at the board a
  long time.

**3. Mill.** Set nine men each, one at a time; three in a row takes a man; then slide along the lines. On a hatch
cover at the ferry, with the ferryman's boy.
- You set a7. He sets d6.
- You set g7: a7 and g7 want d7 between them. He sets d7 and shuts it, and now his d7 and d6 want d5.
- You set d5 and shut that. He sets f6, wanting b6 for a mill along the middle square. You set b6. Nobody has
  taken a man yet, the ferry is still on the far bank, and he has told you which skipper left before dawn without
  paying his toll.

**4. A riddle match.** Three riddles each, turn about; you answer from the palette, and may set only what you have
solved. A hall at Ribe; the host sets first.
- He sets "I came from home, I wended my way from home, I saw a way of ways" (r-gest-02). You lay *a bridge* on it.
  A point.
- You set "Men's damager, words' hinderer, and yet words' arouser" (r-gest-01), the only one you have solved. The
  card said he *surely knows it*: it is his own people's. "Ale," he says, and drinks some. A point to him.
- He sets "the maids that fight weaponless around their lord" (r-gest-19). You have never seen a tafl board. Three
  tries, three cups. The riddle goes into your chest to rest until you see one. He leads two to one with three riddles to come, and you know
  now to bring a Latin riddle to a Danish hall.

**5. The harp goes round.** Sing a text from the chest, let a crewman sing, or pass. The room weighs three fits:
new here, theirs, the day's. The steward's hall at Dorestad, the eve of Saint John.
- The harp comes down the bench. The man before you sang how Hengest wintered with Finn of the Frisians
  (beowulf_17). It is theirs, and they have heard it every winter.
- Your choices: *The Seafarer* (new here, and the day's on a bench of skippers: likely well taken), *Deor* (new
  here), or pass the harp. You sing The Seafarer, in English, slowly.
- Two fits: a full cup, and the steward asks where you had it. A Saxon across the fire answers with a song of his
  own; your steersman has the ear for it, and it goes into the chest.

**6. The cups.** Three cups, a card each: the blessing, the vow, the memory. Drain or sip. A chief's hall in
Vestfold; you are a Christian among men who are not.
- You are seated on the middle bench. The first cup is Odin's, for the king's luck. *Drink as they do, sign it, or
  pass it on.* You make the cross over it. The hall goes quiet. The host's brother says: "He hammers it. Drink."
  (Likely, since the host is your friend.)
- The vow cup. On offer: *two casks of Rhine wine for this hall before Martinmas*; *the count's debt paid by
  Michaelmas*; or praise the host. You vow the wine. It is written down with its day.
- The memory cup. You name Liafbod, lost off Walcheren last autumn. Your men drink standing. Three cups drained:
  your bench-mate has told you the king's men at Hedeby search every Frankish hold this year, and in the morning
  you remember telling him what wine fetches at Lundenwic.

**7. Matching men.** Three rounds before the room: a true boast from your own record, a turn from what you know of
him, a swollen boast, or a laugh. Thorir, a skipper, over the ale at Skiringssal.
- Thorir: "A Frisian: half a cloak and half a boat." You answer with a true boast: "Four men in her, and we lay at
  Lundenwic two days before your friend with his twelve." The room gives it to you.
- Thorir: "And sold what? Pots." You have heard at Ribe what he lost off Jutland. The turn, mild: "Pots float. I
  hear your cargo did not." (The hard form, about the men he left, is offered and not taken.) The room roars: two
  rounds, the match.
- You drink, and hand him the cup: "Half a boat. Come and sail in her." He laughs. A friend, and the Lundenwic
  crossing is told where the ships from here go.

**8. Wrestling.** Best of three falls. Each fall both choose unseen: Grip beats Wait, Wait beats Hook, Hook beats
Grip; the card tells one true thing about him first. The fair at Ribe, a smith's son.
- "A head taller, and slow." You Hook. He Grips. He goes down on a knee.
- "He is watching your feet now." You Hook again. He Waits, and you are the one on the grass.
- "He is breathing hard and wants it over." You Hook. He Grips. Down he goes. He takes your hand; 9 d from the
  wagers; your crew talk about it all the way home.

**9. The horse fight.** You watch and wager; three clashes; the trouble is the men. Late summer at a thing-place in
Vestfold.
- Thorolfr's old grey stands still. Vemundr's young black throws his head. The man beside you says the black has
  never fought. You lay 12 d on the grey with Asbjorn.
- First clash: the black comes in fast and has the grey by the neck; the grey gives ground. *Hold, add, or buy
  yourself out for 6 d.* You hold.
- The grey rears and comes down on him. The black turns away, and turns away again. Vemundr lifts his rod at the
  grey. *Step between, stand witness, or go.* You step between and take it on the shoulder. Asbjorn pays, and
  both owners know your name.

**10. A day with the hawks.** Three flights; each time you choose where to put yourself: at the lord's side, with
the falconer, or after the quarry. The king's reeve at Lundenwic, the marsh west of the town, the week after
Lammas.
- Morning, a hawk at duck. The moment looks bad for talk. You hold back with the falconer, who shows you what to
  look for in a bird's feet.
- Midday, a falcon at a heron. She misses and rings up; the reeve has nothing to do but wait. The moment looks
  good. You ride up and speak of the toll on your wine. He hears you out.
- Afternoon. She will not come back. You ride for the reeds and find her on a heron at the water's edge. At dusk
  he gives you a word for the toll man, and asks whether you can bring him a falcon from the Northmen's country
  before next harvest.

## Rules that hold for every game

The clock stands still while a game is open and moves by the game's time when it closes. Odds are a word (sure,
likely, doubtful). No game can take the boat, the debt or your freedom, and a stake is never more than a tenth of
your silver. Every game can be left for the price of pride, and the loser still has the talk. A man plays you
once an evening and remembers who won. Riddles, song and matching men need his tongue; dice, boards, wrestling
and horses do not.

## Sources

Heimskringla (Laing 1844): Hakon the Good 16 and 18, Olaf Trygvason 39 and 88, Olaf Haraldson 97 and 162-163,
Sigurd the Crusader 25 and 36, read at gutenberg.org/ebooks/598. Tacitus, Germania 24 (Church and Brodribb 1877).
Njal's saga 58 (Dasent 1861). Beowulf, Havamal and Gestumblindi's riddles as in `texts_793.json` (the riddle words
quoted above are Vigfusson and Powell 1883). Wikipedia: Tafl games, Alea evangelii, Nine men's morris, Salme ships,
Birka female Viking warrior, Symbel, Flyting, Glima, Anglo-Saxon lyre, Falconry, Knucklebones, Upplands
runinskrifter 664. Not checked and marked (?) in the data: the finds at York, Dublin, Hedeby and Toftanes; Bede,
Altfrid, Alcuin, Einhard, Asser and Boniface (given from memory in plain words, not quoted).
