using System;
using System.Collections.Generic;
using System.Globalization;
using Kapman.Encounters;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>Ravn's death and the feud that may follow, as a table (Game/Data/feud_rules_793.json; every number a
    /// STARTING GUESS TO TUNE).</summary>
    public sealed class FeudRules
    {
        /// <summary>Off: he is killed as any man is, in a sentence, and nothing follows.</summary>
        public bool on = true;
        /// <summary>THE PUNISHMENT. What his death takes from the crew's heart at once, besides the man; the height
        /// their heart cannot rise over while he lies unanswered; and how many days that lasts when she says nothing.</summary>
        public double deathHeart = -0.2, heartCeiling = 0.6, quietDays = 120.0;
        /// <summary>The mark on her until it is answered: which of her stats, and by how much.</summary>
        public string markStat = "name";
        public double markSize = -1.0;
        /// <summary>What the crew's heart gets back when she swears, when his blood is paid, and when his price is.</summary>
        public double swornHeart = 0.06, bloodHeart = 0.15, priceHeart = 0.05;
        /// <summary>The man-price the count's court sets on the lender (deniers): it comes off the stick.</summary>
        public double priceD = 640.0;
        /// <summary>The fine of a port's lord for blood shed on his quay; and the lender's own price to the count, which
        /// lifts the outlawry of whoever killed him.</summary>
        public double townFineD = 120.0, outlawFineD = 960.0;
        /// <summary>Hours under way before the men look for a shore to burn him on, when she carried him aboard; and
        /// how near land must lie for there to be a shore to look for (km).</summary>
        public double pyreAfterHours = 6.0, shoreKm = 4.0;
        /// <summary>A FIGHT, on her nerve's own scale (a card's chance takes it as its "by"): for each man she has more
        /// or fewer than they (three either way at the most), for weapons in the hold, for their own yard at the
        /// lender's house, and for going in to him alone at his table.</summary>
        public double fightPerMan = 1.0, fightWeapons = 1.0, fightDoor = -1.0, lenderHard = -2.0;
        /// <summary>The men in the lender's yard when none of his are out.</summary>
        public double houseMen = 2.0;
        /// <summary>HIS SCENE, AS IT IS STAGED: line by line, who says it and the writing's key of its words (at sea the
        /// key with ".sea" after it, where that is written); and who those speakers are.</summary>
        public readonly List<SceneBeat> scene = new List<SceneBeat>();
        public readonly Dictionary<string, ScenePerson> sceneCast = new Dictionary<string, ScenePerson>(StringComparer.Ordinal);

        public static FeudRules Read(Dictionary<string, object> o)
        {
            var r = new FeudRules();
            if (o == null)
                return r;
            foreach (var f in typeof(FeudRules).GetFields())
            {
                if (f.IsStatic)
                    continue;
                if (f.FieldType == typeof(double))
                    f.SetValue(r, Json.Num(o, f.Name, (double)f.GetValue(r)));
                else if (f.FieldType == typeof(bool))
                    f.SetValue(r, Json.Bool(o, f.Name, (bool)f.GetValue(r)));
                else if (f.FieldType == typeof(string))
                    f.SetValue(r, Json.Str(o, f.Name, (string)f.GetValue(r)));
            }
            foreach (object beat in Json.Arr(o, "scene") ?? new List<object>())
                r.scene.Add(new SceneBeat { who = Json.Str(beat, "who"), key = Json.Str(beat, "key"), cut = Json.Bool(beat, "cut"), hit = Json.Bool(beat, "hit") });
            foreach (var kv in Json.Obj(o, "sceneCast") ?? new Dictionary<string, object>())
                r.sceneCast[kv.Key] = new ScenePerson { cast = Json.Str(kv.Value, "cast"), role = Json.Str(kv.Value, "role"), name = Json.Str(kv.Value, "name") };
            return r;
        }
    }

    /// <summary>One line of his scene as the table stages it: who says it (a name of the table's sceneCast; "": the
    /// teller), the writing's key of its words, and whether it breaks off.</summary>
    public sealed class SceneBeat
    {
        public string who = "", key = "";
        public bool cut;
        /// <summary>A hit: the screen flashes red and shakes as this line comes up (the knife).</summary>
        public bool hit;
    }

    /// <summary>One who speaks in his scene: who they are in the standing cast, their trade (for a stand-in face), and
    /// the name shown over the box.</summary>
    public sealed class ScenePerson
    {
        public string cast = "", role = "", name = "";
    }

    /// <summary>Where the matter of Ravn's death stands.</summary>
    public enum FeudStage
    {
        /// <summary>He lives.</summary>
        None,
        /// <summary>He has just been killed: his scene is to be played when the card of the rope men is shut.</summary>
        Scene,
        /// <summary>The scene is over: she stands over him, and is to say what is to be done.</summary>
        Oath,
        /// <summary>It is open: sworn, or before the count, or she said nothing and it lies on the boat.</summary>
        Open,
        /// <summary>It is over: his blood paid, or his price, or the lender dead, or the days of saying nothing run out.</summary>
        Settled,
    }

    /// <summary>
    /// RAVN'S DEATH, AND THE BLOOD FEUD (the user, 9 Oct 2026: "Ravn needs an impactful death scene as genuine punishment
    /// for not repaying the debt but opens up a bloodfued opportunity"; by the picker: the feud is "You against the
    /// lender", it ends in "Blood or silver", and "All of it, live").
    ///
    /// THE SCENE. When the lender's men kill him (Collectors: the third time she does not pay) and their card is shut,
    /// the game stops and goes through black: the rope man, Bubo, Ravn and Aldger SPEAK, a box of talk at a time as
    /// Pokémon paces it (Showtime.Scene), Ravn's last line breaks off in the middle of a word, and then his name and
    /// the day. The table stages it (who says which line: "scene", "sceneCast"); the writing words it ("feud.scene.1"
    /// and on, with ".sea" after a key where the sea's differs). It turns on what his own sheet says he feared: to die
    /// among Christians unable to speak, and be signed with their cross. (Its first cut was a narrator's words alone
    /// on the black; the user: "doesn't flow well enough ... reads far too AI written and just doesn't hit".)
    /// THE PUNISHMENT. A man fewer; the crew's heart struck down at once and held under a ceiling while he lies
    /// unanswered (<see cref="HeartCeiling"/>); a mark on her until it is answered; her name among his people; and
    /// every card that leaned on him is gone with him (the cards' engine).
    /// OVER HIM she says what is to be done (the oath's card): swear his blood, take it to the count's court for his
    /// price, or say nothing. Then where he lies (the burial's card): the priest's ground, which is the other half of
    /// what he feared; or carried aboard for a fire on a shore of his own; or the water.
    /// THE FEUD turns the chase round. When the rope men next come, a card of its own comes before theirs: the one who
    /// did it is with them, and she may kill him (asked once each time they come; a card holds nine choices at most,
    /// and one that cannot be taken is still shown, so it could not be a choice on their own card). Sworn, at Dorestat
    /// she may call him out of the lender's yard, or go in to the lender himself.
    /// A FIGHT is carried by her nerve, by how many men she has against theirs, and by weapons aboard
    /// (<see cref="Fight"/>). Blood in a port is fined by its lord. THE LENDER KILLED: the stick burns with him (nothing
    /// is owed), his men come no more, and she is an outlaw in the Franks' ports until his price is paid to the count.
    /// AT LAW the count may set Ravn's price on the lender: it comes off the stick, and closes the matter.
    /// Flags of the cards' deck say how it stands, for the cards to ask: feud_sworn, feud_law, feud_quiet (it lapses),
    /// feud_blood (the one who did it is dead), feud_settled (1 his blood, 2 his price, 3 the lender's blood, 4 let
    /// lie), feud_buried (1 the priest's ground, 2 the fire, 3 the water), ravn_aboard, feud_outlaw. Plain state, saved
    /// whole. EVERY WORD HERE IS A PLACEHOLDER for the user's own.
    /// </summary>
    public static class Feud
    {
        public const string File = "feud_rules_793.json";
        public const string OathCard = "crew.feud.oath", BurialCard = "crew.feud.burial", PyreCard = "crew.feud.pyre", KillerCard = "crew.feud.killer",
                            DoorCard = "harbour.feud.door", CourtCard = "harbour.feud.court", OutlawCard = "harbour.feud.outlaw";
        public const string Sworn = "feud_sworn", AtLaw = "feud_law", Quiet = "feud_quiet", Blood = "feud_blood", SettledFlag = "feud_settled",
                            Buried = "feud_buried", Aboard = "ravn_aboard", Outlaw = "feud_outlaw";
        /// <summary>The lender, in the standing cast.</summary>
        public const string LenderCast = "dorestad.wursing";
        /// <summary>The mark that lies on her until it is answered.</summary>
        public const string Mark = "ravn_unanswered";

        static readonly ContentCache<FeudRules> cache =
            new ContentCache<FeudRules>("Ravn's death and the feud", () => FeudRules.Read(ContentFiles.Exists(File) ? ContentFiles.ParseObject(File) : null));
        static FeudRules own;

        public static FeudRules Rules => own ?? cache.Value;

        /// <summary>Puts a table in the game's place (a test); null gives the game's back.</summary>
        public static void Use(FeudRules rules) => own = rules;

        // ------------------------------------------------------------------ what is so (saved whole)

        public static FeudStage Stage { get; private set; } = FeudStage.None;
        /// <summary>Was he killed at sea; at which port if not; and on which game day.</summary>
        public static bool AtSea { get; private set; }
        public static string Port { get; private set; } = "";
        public static double Day { get; private set; }
        /// <summary>Counts up when what a window shows of this has changed.</summary>
        public static int Version { get; private set; }

        static double aboardSince;
        static bool scenePlaying;
        /// <summary>The sending of the rope men at which the one who did it was last put before her (0: none).</summary>
        static int askedVisit;

        static bool Has(string flag) => ContentFlags.Ready && ContentFlags.Has(flag);
        static double Value(string flag) => ContentFlags.Ready ? ContentFlags.Value(flag) : 0.0;

        /// <summary>Is the lender dead by her hand?</summary>
        public static bool LenderDead => Has(EncounterState.GoneFlag(LenderCast));
        /// <summary>Is the one who killed Ravn alive?</summary>
        public static bool KillerAlive => Stage != FeudStage.None && !Has(Blood);
        /// <summary>He is dead and it is neither answered nor run out.</summary>
        public static bool Unanswered => Stage == FeudStage.Scene || Stage == FeudStage.Oath || Stage == FeudStage.Open;
        /// <summary>She has said what is to be done, and it is not done yet.</summary>
        public static bool IsOpen => Stage == FeudStage.Open;
        /// <summary>She has said what is to be done over him (it is open, or over).</summary>
        public static bool Said => Stage == FeudStage.Open || Stage == FeudStage.Settled;
        public static bool IsSworn => IsOpen && Has(Sworn);
        public static bool IsAtLaw => IsOpen && Has(AtLaw) && !Has(Sworn);
        public static bool IsOutlaw => Has(Outlaw);
        /// <summary>He lies aboard, for a fire on a shore.</summary>
        public static bool LiesAboard => Has(Aboard);
        /// <summary>How it ended (0: not ended; 1 his blood, 2 his price, 3 the lender's blood, 4 let lie).</summary>
        public static int Ended => (int)Math.Round(Value(SettledFlag));

        /// <summary>THE HEIGHT THE CREW'S HEART CANNOT RISE OVER (Voyage.Morale): the table's while he lies unanswered,
        /// else none.</summary>
        public static double HeartCeiling => Unanswered ? Rules.heartCeiling : 1.0;

        static string Show(double deniers)
        {
            var port = GameData.Port(Kapman.Game.Port.LyingId());
            var zone = port != null && TradeRules.Current != null ? TradeRules.Current.ZoneOf(port.region) : null;
            return zone != null ? zone.Show(deniers) : deniers.ToString("0.#", CultureInfo.InvariantCulture) + " d";
        }

        /// <summary>The writing's words for a key (null: not written, or no game).</summary>
        static string Written(string key)
        {
            var cards = EncounterRunner.Director;
            return cards != null && cards.book != null ? cards.book.Written(key) : null;
        }

        // ------------------------------------------------------------------ his death

        /// <summary>
        /// The lender's men have killed him (Collectors: the flag of the dead is set and the man is off the count
        /// already): the punishment falls at once, and his scene is played when their card is shut. `atSea`, `port`:
        /// where. False: the rule is off, and nothing follows.
        /// </summary>
        public static bool RavnKilled(bool atSea, string port)
        {
            var r = Rules;
            var game = KapmanGame.Current;
            if (!r.on)
                return false;
            AtSea = atSea;
            Port = atSea ? "" : port ?? "";
            Day = game != null ? game.Day : 0.0;
            Stage = FeudStage.Scene;
            scenePlaying = false;
            Voyage.Affect(0.0, r.deathHeart);                              // (and the ceiling is on from now: Voyage.Morale)
            if (Self.IsStat(r.markStat) && Math.Abs(r.markSize) >= 0.5)
                Self.Mark(r.markStat, (int)Math.Round(r.markSize), Mark, 0.0, SettledFlag,
                          Written("mark." + Mark) ?? "Ravn was killed for your debt, and nothing has been done about it", Self.Now,
                          Written("mark." + Mark + ".until") ?? "until it is answered");
            var at = GameData.Port(Port) ?? GameData.Port(Collectors.Rules.home);
            if (at != null)
                Deeds.Record("lost_a_man_to_a_debt", "lost a man to a debt", at);
            Version++;
            return true;
        }

        /// <summary>The lines of his scene, for where he died, as the table stages them (who says each, which breaks
        /// off) and the writing words them (at sea a key with ".sea" after it stands in the place of the quay's, where
        /// it is written). A line with no words written is left out.</summary>
        public static List<ToldLine> SceneLines()
        {
            var r = Rules;
            var lines = new List<ToldLine>();
            foreach (var beat in r.scene)
            {
                string text = AtSea ? Written(beat.key + ".sea") ?? Written(beat.key) : Written(beat.key);
                if (string.IsNullOrEmpty(text))
                    continue;
                var line = new ToldLine { text = text, cut = beat.cut, hit = beat.hit };
                if (beat.who.Length > 0 && r.sceneCast.TryGetValue(beat.who, out var who))
                {
                    line.who = who.name;
                    line.face = who.cast;
                    line.slot = Kapman.Game.Frame.FrameArt.OfSpeaker(who.cast, who.role) ?? "";
                }
                lines.Add(line);
            }
            if (lines.Count == 0)
                lines.Add(new ToldLine { text = "Ravn is dead." });
            return lines;
        }

        /// <summary>Where and when, under his name at the scene's end ("Killed at Dorestat, Kalends of May, 793").</summary>
        public static string Plate()
        {
            var at = GameData.Port(Port);
            return "Killed " + (at != null ? "at " + at.name : "at sea") + ", " + Kapman.Game.Frame.FrameGame.DateOfDay(Day);
        }

        static bool CardUp()
        {
            var cards = EncounterRunner.Director;
            return cards != null && (cards.Current != null || cards.Waiting);
        }

        /// <summary>His scene, now. Shown where the show is on; told to the log where it is not (a batch run).</summary>
        static void Play()
        {
            scenePlaying = true;
            var lines = SceneLines();
            bool shown = Showtime.On && Showtime.Current != null;
            Showtime.Scene(lines, "Ravn", Plate(), () =>
            {
                scenePlaying = false;
                if (Stage != FeudStage.Scene)
                    return;                                              // (another game was begun under it)
                // (the log keeps it: all of it where it could not be shown, one plain line where it was)
                var at = GameData.Port(Port);
                if (shown)
                    Voyage.Say("Ravn is dead, killed by the lender's men" + (at != null ? " at " + at.name + "." : " at sea."));
                else
                    foreach (var line in lines)
                        Voyage.Say(line.who.Length > 0 ? line.who + ": " + line.text : line.text);
                // (and the mark that lies on her from now, as the log says every other mark)
                var mark = Self.MarksOn((string)null).Find(m => m.id == Mark);
                if (mark != null)
                    Voyage.Say(Self.Said(mark, Self.Now));
                Stage = FeudStage.Oath;
                Version++;
                EncounterRunner.Director?.Offer(OathCard);
            });
        }

        /// <summary>Each frame of the played game (Showtime's own): his scene begins when the rope men's card is shut,
        /// whether the clock runs or not.</summary>
        public static void Tick()
        {
            if (Stage == FeudStage.Scene && !scenePlaying && !CardUp() && !Showtime.Busy)
                Play();
        }

        /// <summary>One game minute (KapmanGame.StepMinute, after the lender's men).</summary>
        public static void Step(double hours)
        {
            var r = Rules;
            var game = KapmanGame.Current;
            if (game == null || Stage == FeudStage.None)
                return;
            var cards = EncounterRunner.Director;
            switch (Stage)
            {
                case FeudStage.Scene:
                    // (where there is no show to play it, a batch run, it is told to the log here)
                    if (!scenePlaying && !CardUp() && !(Showtime.On && Showtime.Current != null))
                        Play();
                    break;
                case FeudStage.Oath:
                    if (Has(Sworn) || Has(AtLaw) || Has(Quiet))
                    {
                        Stage = FeudStage.Open;                            // (she has said what is to be done)
                        Version++;
                    }
                    else if (cards != null && !CardUp())
                        cards.Offer(OathCard);
                    break;
                case FeudStage.Open:
                    if (Value(SettledFlag) != 0.0)
                    {
                        Stage = FeudStage.Settled;
                        Version++;
                        break;
                    }
                    if (!Has(Sworn) && !Has(AtLaw) && !Has(Quiet))
                        Settle(4.0, 0.0);                                  // (she said nothing, and the days of that have run out)
                    break;
            }
            // THE ONE WHO DID IT is with the rope men who have caught her: put before her once each time they come, ahead
            // of their own card (which Collectors has put in line this same minute)
            if (IsOpen && KillerAlive && Collectors.Caught && askedVisit != Collectors.Visit && cards != null
                && (cards.Current == null || !cards.Current.card.id.Contains(".goons.")) && cards.Offer(KillerCard))
                askedVisit = Collectors.Visit;
            // WHERE HE LIES is asked whatever has become of the feud, once she has said what is to be done
            if (!Said || cards == null || CardUp())
                return;
            // not yet laid anywhere (the card was shut on another's heels): asked again
            if (Value(Buried) == 0.0 && !Has(Aboard))
                cards.Offer(BurialCard);
            // carried aboard: a fire on a shore, some hours out, where there is a shore
            else if (Has(Aboard) && Kapman.Game.Port.LyingId() == null && game.Hours - aboardSince >= r.pyreAfterHours && !Boat.InGale
                     && ShoreNear(ChartWorld.ShipWorld(), r.shoreKm))
                cards.Offer(PyreCard);
        }

        /// <summary>Is there a shore within so many km of a place: she is on a river, or land lies on one of twelve
        /// bearings at a third, two thirds or the whole of that distance (the world's own land and water, where it is
        /// known; where it is not, there is always a shore).</summary>
        public static bool ShoreNear(WorldXZ at, double km)
        {
            if (!Course.KnowsWater || ChartWorld.OnRiver())
                return true;
            for (int ring = 1; ring <= 3; ring++)
            {
                double reach = km * 1000.0 * ring / 3.0;
                for (int b = 0; b < 12; b++)
                {
                    double a = b * Math.PI / 6.0;
                    if (!Course.IsWater(at.x + Math.Sin(a) * reach, at.z + Math.Cos(a) * reach))
                        return true;
                }
            }
            return false;
        }

        // ------------------------------------------------------------------ what a card can ask

        /// <summary>A number for a card's conditions and odds: feud.oath (she stands over him), feud.said (she has said
        /// what is to be done: it is open, or over), feud.open, feud.killer
        /// (the one who did it lives), feud.buried (0 not yet; 1 the priest's ground, 2 the fire, 3 the water),
        /// feud.aboard_days (the days he has lain aboard), feud.days (since he died), feud.fight (against the rope men
        /// who have caught her), feud.door (in the lender's yard), feud.lender (alone at his table), feud.court (her
        /// name before the count).</summary>
        public static double Var(string name)
        {
            var r = Rules;
            var game = KapmanGame.Current;
            switch (name)
            {
                case "feud.oath": return Stage == FeudStage.Oath ? 1.0 : 0.0;
                case "feud.said": return Said ? 1.0 : 0.0;
                case "feud.open": return IsOpen ? 1.0 : 0.0;
                case "feud.killer": return KillerAlive ? 1.0 : 0.0;
                case "feud.buried": return Value(Buried);
                case "feud.aboard_days": return game != null && Has(Aboard) ? Math.Max(0.0, (game.Hours - aboardSince) / 24.0) : 0.0;
                case "feud.days": return game != null && Stage != FeudStage.None ? Math.Max(0.0, game.Day - Day) : 0.0;
                case "feud.fight": return Fight(r, Self.StatNow("nerve"), Voyage.Men, Math.Max(1, Collectors.Men), Armed(), 0.0);
                case "feud.door": return Fight(r, Self.StatNow("nerve"), Voyage.Men, (int)Math.Round(r.houseMen), Armed(), r.fightDoor);
                case "feud.lender": return Self.StatNow("nerve") + r.lenderHard + (Armed() ? r.fightWeapons : 0.0);
                case "feud.court": return Self.StatNow("name");
            }
            return 0.0;
        }

        static bool Armed() => EncounterGameVars.Num("hold.weapons", null) > 0.0;

        /// <summary>HOW A FIGHT STANDS, on her nerve's own scale (a card's chance adds a twentieth to its odds for each
        /// point of this over 3): her nerve, a point for each man she has more than they (or fewer: three either way at
        /// the most), a point for weapons aboard, and what the place costs her.</summary>
        public static double Fight(FeudRules r, double nerve, int herMen, int theirMen, bool weapons, double place) =>
            nerve + r.fightPerMan * Math.Max(-3, Math.Min(3, herMen - theirMen)) + (weapons ? r.fightWeapons : 0.0) + place;

        /// <summary>It is over (1 his blood, 2 his price, 3 the lender's blood, 4 let lie): the ceiling and the mark lift.</summary>
        static void Settle(double how, double heart)
        {
            ContentFlags.Set(SettledFlag, how);
            ContentFlags.Clear(Sworn);
            ContentFlags.Clear(AtLaw);
            ContentFlags.Clear(Quiet);
            Stage = FeudStage.Settled;
            Self.Unmark(Mark);
            Voyage.Affect(0.0, heart);
            Version++;
        }

        /// <summary>The port she lies at; at sea, the lender's own (his men carry the word home).</summary>
        static PortDef WordGoesFrom() => GameData.Port(Kapman.Game.Port.LyingId()) ?? GameData.Port(Collectors.Rules.home);

        static void Deed(string kind)
        {
            var at = WordGoesFrom();
            if (at != null)
                Deeds.Record(kind, kind.Replace('_', ' '), at);
        }

        /// <summary>
        /// WHAT A CARD ORDERS ("law feud_swear" and the rest: Encounters/GameEffects.Law hands every kind that begins
        /// "feud_" here). Returns what happened as a plain fact for the card's end, or null. PLACEHOLDER WORDS.
        ///   feud_swear, feud_law, feud_quiet     what she said over him
        ///   feud_bury_priest, feud_bury_aboard, feud_bury_water, feud_pyre     where he lies
        ///   feud_blood          the one who did it is dead: it is paid in blood (and the port's lord fines it)
        ///   feud_blood_lost     the fight went against her: a man of hers is dead
        ///   feud_lender         the lender is dead: nothing is owed, his men come no more, she is an outlaw
        ///   feud_lender_lost    she was thrown out of his house, and he is past patience
        ///   feud_price          the count sets his price on the lender: it comes off the stick
        ///   feud_outlaw_paid    the lender's price paid to the count: the outlawry is lifted
        ///   feud_outlaw_seized  the count's men take the cargo
        /// </summary>
        public static string Law(string kind)
        {
            var r = Rules;
            var game = KapmanGame.Current;
            var rules = TradeRules.Current;
            switch (kind)
            {
                case "feud_swear":
                    if (Stage == FeudStage.None || Stage == FeudStage.Settled)
                        return null;
                    if (!Has(Sworn))
                        Voyage.Affect(0.0, r.swornHeart);
                    ContentFlags.Set(Sworn);
                    ContentFlags.Clear(AtLaw);
                    ContentFlags.Clear(Quiet);
                    Stage = FeudStage.Open;
                    Version++;
                    return null;
                case "feud_law":
                    if (Stage == FeudStage.None || Stage == FeudStage.Settled)
                        return null;
                    ContentFlags.Set(AtLaw);
                    ContentFlags.Clear(Quiet);
                    Stage = FeudStage.Open;
                    Version++;
                    return null;
                case "feud_quiet":
                    if (Stage == FeudStage.None || Stage == FeudStage.Settled)
                        return null;
                    ContentFlags.Set(Quiet, 1.0, Math.Max(1.0, r.quietDays));
                    Stage = FeudStage.Open;
                    Version++;
                    return null;
                case "feud_bury_priest":
                case "feud_bury_water":
                case "feud_pyre":
                    ContentFlags.Set(Buried, kind == "feud_bury_priest" ? 1.0 : kind == "feud_pyre" ? 2.0 : 3.0);
                    ContentFlags.Clear(Aboard);
                    Version++;
                    return null;
                case "feud_bury_aboard":
                    ContentFlags.Set(Aboard);
                    aboardSince = game != null ? game.Hours : 0.0;
                    Version++;
                    return null;
                case "feud_blood":
                {
                    string fine = "";
                    // (blood shed in a port is its lord's to fine; at sea there is nobody to)
                    if (Kapman.Game.Port.LyingId() != null && Trade.Started && r.townFineD > 0.0)
                    {
                        double paid = Trade.Purse.PayUpTo(r.townFineD, rules);
                        fine = paid > 0.5 ? $" The lord of the port takes {Show(paid)} from you for blood shed on his quay."
                                          : " The lord of the port finds nothing on you to take for the blood shed on his quay.";
                    }
                    Deed("took_blood_for_her_man");
                    ContentFlags.Set(Blood);
                    Settle(1.0, r.bloodHeart);
                    return "The man who put his knife in Ravn is dead, and so Ravn is answered." + fine;
                }
                case "feud_blood_lost":
                {
                    int lost = Voyage.LoseMen(1);
                    return lost > 0 ? "The fight goes against you, and one of your men does not get up again." : "The fight goes against you.";
                }
                case "feud_lender":
                    ContentFlags.Set(EncounterState.GoneFlag(LenderCast));
                    ContentFlags.Set(Outlaw);
                    ContentFlags.Clear(Trade.WorkFlag);
                    if (Trade.Started)
                        Trade.Debt = 0.0;
                    Deed("killed_the_lender");
                    Settle(3.0, r.bloodHeart);
                    return $"{Trade.LenderName} is dead at his own table and the stick has burned with him, so nothing is owed. From now on you are an outlaw in every port of the Franks.";
                case "feud_lender_lost":
                    Collectors.Rage();
                    return "His men have you by the arms before you are halfway across the room, and they throw you into the river.";
                case "feud_price":
                {
                    double counted = Trade.Started ? Math.Min(Trade.Debt, Math.Round(r.priceD)) : 0.0;
                    if (Trade.Started)
                        Trade.Debt = Math.Max(0.0, Trade.Debt - counted);
                    Settle(2.0, r.priceHeart);
                    return $"The count sets Ravn's price on the lender at {Show(counted)}, which comes off your stick. Still owed: {Show(Trade.Started ? Trade.Debt : 0.0)}.";
                }
                case "feud_outlaw_paid":
                {
                    if (!Trade.Started || Trade.Purse.Silver(rules) < r.outlawFineD - 0.5)
                        return "You do not have the silver for his price, so you are an outlaw still.";
                    double paid = Trade.Purse.PayUpTo(r.outlawFineD, rules);
                    ContentFlags.Clear(Outlaw);
                    Version++;
                    return $"You pay the count {Show(paid)}, which is the lender's price, and you are an outlaw no longer.";
                }
                case "feud_outlaw_seized":
                {
                    // (the crew's own bread and ale are left: they are not sent to starve anybody)
                    var gone = new List<string>();
                    if (Trade.Started)
                        foreach (var (good, count) in new List<(GoodDef good, int count)>(Trade.Hold.Contents))
                        {
                            if (count <= 0 || Array.IndexOf(Voyage.Rules.foodGoods, good.id) >= 0 || Array.IndexOf(Voyage.Rules.drinkGoods, good.id) >= 0)
                                continue;
                            Trade.Hold.Remove(good, count);
                            gone.Add(Trade.UnitsNamed(good, count));
                        }
                    return gone.Count == 0 ? "The count's men turn out the hold and find nothing." : "The count's men take the cargo: " + string.Join(", ", gone) + ".";
                }
            }
            return null;
        }

        // ------------------------------------------------------------------ how it stands, for a window

        /// <summary>She may still swear it: it is open, and she has not (she said nothing, or took it to the count).</summary>
        public static bool CanSwear => IsOpen && !Has(Sworn);

        /// <summary>She swears his blood now (the Errands window's button): what a card's "law feud_swear" does.</summary>
        public static string Swear()
        {
            if (!CanSwear)
                return "";
            Law("feud_swear");
            return "You swear Ravn's blood on the one who killed him.";
        }

        /// <summary>How the matter of Ravn stands, in a sentence or two ("" while he lives). PLACEHOLDER WORDS.</summary>
        public static string Words()
        {
            if (Stage == FeudStage.None)
                return "";
            string said;
            switch (Stage)
            {
                case FeudStage.Settled:
                    said = Ended == 1 ? "The man who killed him is dead, and so he is answered."
                         : Ended == 2 ? "The count set his price on the lender and took it off your stick, which closes it."
                         : Ended == 3 ? "The lender is dead."
                         : "Nothing was done about it.";
                    break;
                case FeudStage.Open:
                    said = Has(Sworn) ? "You swore his blood on the one who killed him."
                         : Has(AtLaw) ? "You said the count should have it."
                         : "You said nothing over him.";
                    break;
                default:
                    said = "The lender's men killed him for your debt.";
                    break;
            }
            // (why their heart is held down is the Crew window's to say)
            if (Has(Aboard))
                said += " He lies aboard, for a fire on a shore.";
            if (Has(Outlaw))
                said += $" You are an outlaw in the Franks' ports until you pay the count the lender's price of {Rules.outlawFineD:0} d.";
            return said;
        }

        // ------------------------------------------------------------------ a test's hands

        /// <summary>A test's: the scene is over (as if it had been played), and she stands over him.</summary>
        public static void PastScene()
        {
            if (Stage != FeudStage.Scene)
                return;
            scenePlaying = false;
            Stage = FeudStage.Oath;
            Version++;
        }

        // ------------------------------------------------------------------ a saved game

        static object Capture() => new Dictionary<string, object>
        {
            ["stage"] = Stage.ToString(), ["at_sea"] = AtSea, ["port"] = Port, ["day"] = Day, ["aboard_since"] = aboardSince,
            ["asked_visit"] = (double)askedVisit,
        };

        static void Restore(object saved)
        {
            Forget();
            if (saved == null)
                return;
            Stage = Enum.TryParse(Json.Str(saved, "stage"), out FeudStage s) ? s : FeudStage.None;
            AtSea = Json.Bool(saved, "at_sea");
            Port = Json.Str(saved, "port");
            Day = Json.Num(saved, "day");
            aboardSince = Json.Num(saved, "aboard_since");
            askedVisit = (int)Json.Num(saved, "asked_visit");
            Version++;
        }

        /// <summary>As a new game has it: he lives.</summary>
        public static void Forget()
        {
            Stage = FeudStage.None;
            AtSea = false;
            Port = "";
            Day = aboardSince = 0.0;
            scenePlaying = false;
            askedVisit = 0;
            Version++;
        }

        // Enter Play Mode runs without a domain reload, and a new game runs this too (MainMenu.ClearAllState)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            own = null;
            Forget();
            ContentSave.Register("feud", Capture, Restore);
        }
    }
}
