using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.ShipView;
using Kapman.Game.Sim;
using Kapman.Levels;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Kapman.Tests
{
    /// <summary>
    /// THE PLAY-THROUGH, A PICTURE AT EACH STEP (the alpha's row 18; the road's step 9). The game played by the test's
    /// hands through the real buttons, from a new game to the Michaelmas reckoning: the opening, the first jobs, and
    /// then a season of trade by a plain trader (it sells what it brought, buys what the next port pays most for, keeps
    /// bread aboard, rests the men, answers every card, and is home for the day). Every step is a picture in
    /// Captures/playthrough, and the walk is told in journal.txt beside them, with every fault the page's rule found
    /// (faults.txt). It is LONG (a season is 151 days) and is not part of the suites: run it by its name.
    ///   -runTests -testPlatform PlayMode -testFilter PlayThroughTests     (with graphics: no -nographics)
    /// KAPMAN_PT_DAYS in the environment stops the season after that many days (a short walk to try it).
    /// The trader decides from the markets as they are (a player has only the word on them): it is there to carry the
    /// hands through every window, not to play well.
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class PlayThroughTests
    {
        const string Home = "dorestad";
        const double Reserve = 14.0;              // silver kept back for bread and the men
        const double KmADay = 75.0;               // what the trader reckons she makes in a day and a night, to be home in time

        ChartPlay play;
        readonly StringBuilder journal = new StringBuilder();
        readonly List<string> found = new List<string>();
        readonly Dictionary<string, int> seen = new Dictionary<string, int>();
        readonly Dictionary<string, string> boughtFor = new Dictionary<string, string>();
        int pictures, calls, legs, cardsAnswered, lost, restedTimes, alongside;
        double silverAtStart, bestSilver;
        bool ended;
        string endedHow = "";

        static FrameRoot Frame => FrameRoot.Current;
        static KapmanGame Game => KapmanGame.Current;
        static string Folder => Path.Combine(Application.dataPath, "..", "Captures", "playthrough");

        // ------------------------------------------------------------------ the title, as a new player has it

        static IEnumerator FreshTitle()
        {
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                if (SceneManager.GetActiveScene().name == "L_Chart")
                    go.SetActive(false);
            MainMenu.ClearAllState();
            Showtime.ForceOn = true;
            FrameWindows.OneWindow = true;
            Showtime.LoadingSeconds = 6.5f;
            Showtime.SlideSeconds = 1.4f;
            FrameRoot.FreshLayout = true;
            ClockSettings.AsNew = true;
            string saves = Path.Combine(Application.temporaryCachePath, "kapman_playthrough_saves");
            Directory.CreateDirectory(saves);
            foreach (string old in Directory.GetFiles(saves))
                File.Delete(old);
            SaveGame.FolderOverride = saves;
            ShipStageHook.TurnOn();
            var load = SceneManager.LoadSceneAsync("L_Chart", LoadSceneMode.Single);
            Assert.IsNotNull(load, "L_Chart is not in the build settings");
            while (!load.isDone)
                yield return null;
            Showtime.ForceOn = true;
            FrameWindows.OneWindow = true;
            SaveGame.FolderOverride = saves;
            FrameRoot.FreshLayout = true;
            ClockSettings.AsNew = true;
            yield return ChartPlay.Frames(5);
        }

        static bool Up(VisualElement v) => v != null && v.resolvedStyle.display != DisplayStyle.None && v.resolvedStyle.opacity > 0.5f;

        // ------------------------------------------------------------------ the journal

        string Now()
        {
            var game = Game;
            var clock = LevelClock.Current;
            string when = clock != null ? clock.Text : "";
            var here = FrameGame.PortHere;
            return when + (here != null ? ", at " + here.name : game != null && game.ship != null && !game.ship.anchored ? ", under way" : ", lying to");
        }

        void Say(string line)
        {
            journal.AppendLine("[" + Now() + "] " + line);
        }

        void Fault(string line)
        {
            found.Add("[" + Now() + "] " + line);
            journal.AppendLine("[" + Now() + "] FAULT: " + line);
            Debug.LogWarning("Play-through, a fault: " + line);
        }

        void Keep()
        {
            Directory.CreateDirectory(Folder);
            File.WriteAllText(Path.Combine(Folder, "journal.txt"), journal.ToString());
            var all = new StringBuilder();
            all.AppendLine("WHAT THE WALK ITSELF FOUND (" + found.Count + ")");
            foreach (string f in found)
                all.AppendLine(f);
            if (play != null)
            {
                all.AppendLine();
                all.AppendLine("WORDS WRITTEN FOR THE MAKERS THAT REACHED A SCREEN (" + play.pageFaults.Count + ")");
                foreach (string f in play.pageFaults)
                    all.AppendLine(f);
                all.AppendLine();
                all.AppendLine("TEXT OUT OF ITS BOX, OR A WORD BROKEN (" + play.cuts.Count + ")");
                foreach (string f in play.cuts)
                    all.AppendLine(f);
                all.AppendLine();
                all.AppendLine("OTHER FAULTS OF THE SCREEN (" + play.faults.Count + ")");
                foreach (string f in play.faults)
                    all.AppendLine(f);
                all.AppendLine();
                all.AppendLine("WRITING NOT WRITTEN YET, AS IT SHOWED (" + play.unwritten.Count + ")");
                foreach (string f in play.unwritten)
                    all.AppendLine(f);
            }
            File.WriteAllText(Path.Combine(Folder, "faults.txt"), all.ToString());
        }

        static string Safe(string s)
        {
            var b = new StringBuilder();
            foreach (char c in (s ?? "").ToLowerInvariant())
                b.Append(char.IsLetterOrDigit(c) ? c : '_');
            string t = b.ToString().Trim('_');
            while (t.Contains("__"))
                t = t.Replace("__", "_");
            return t.Length > 48 ? t.Substring(0, 48) : t;
        }

        IEnumerator Shot(string label, float settle = 0.35f)
        {
            string name = $"pt_{pictures++:000}_{Safe(label)}";
            yield return play.Shot(name, settle);
            Say("picture " + name);
        }

        int Count(string key)
        {
            seen.TryGetValue(key, out int n);
            seen[key] = n + 1;
            return n;
        }

        // ------------------------------------------------------------------ parts of the frame, found as a reader finds them

        VisualElement Win(string title)
        {
            VisualElement found = null;
            if (Frame == null)
                return null;
            Frame.Root.Query<VisualElement>(className: "frame-win").ForEach(w =>
            {
                var name = w.Q<Label>(className: "frame-win-name");
                if (found == null && name != null && name.text == title && ChartPlay.Shown(w))
                    found = w;
            });
            return found;
        }

        VisualElement SideWindow()
        {
            VisualElement found = null;
            if (Frame != null)
                Frame.Root.Query<VisualElement>(className: "frame-win").ForEach(w =>
                {
                    if (found == null && ChartPlay.Shown(w))
                        found = w;
                });
            return found;
        }

        Button Verb(string name) => play.Buttons(name, Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-verb"));

        Button Small(string words, VisualElement under = null) =>
            play.Buttons(words, under ?? Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        static string Title(VisualElement floating) => floating?.Q<Label>(className: "frame-float-title")?.text ?? "";

        static WorldXZ Where()
        {
            var w = FloatingOrigin.ToWorld(Game.ship.transform.position);
            return new WorldXZ(w.x, w.z);
        }

        /// <summary>A window had by its button in the rack (Voyage, Trade, Crew, Hold, Wares, Journey, Errands, Book).</summary>
        IEnumerator Open(string verb, string title)
        {
            if (play.FrontFloat() != null && !Chased)
                yield return HandleFloats();                             // (a window that wants its answer comes first)
            if (Win(title) != null && !Frame.TodoOn)
                yield break;
            var b = Verb(verb);
            if (b == null || !b.enabledSelf)
            {
                Fault($"the {verb} button is not there to press" + (b != null ? ": greyed out (" + b.tooltip + ")" : ""));
                yield break;
            }
            yield return play.Click(b, "the " + verb + " button");
            yield return ChartPlay.Until(() => Win(title) != null, 3f);
            yield return ChartPlay.Frames(4);
            if (Win(title) == null)
                Fault($"the {verb} button did not bring the {title} window");
        }

        // ------------------------------------------------------------------ what comes up and wants an answer

        /// <summary>Which choice the trader takes on a card: the lender is paid when the silver is there and fed when it
        /// is not; a card with a rule for what it takes when nobody answers takes that; else the first that can be chosen.</summary>
        int PickFor(EncounterSession s)
        {
            string id = s.card.id;
            string[] want = null;
            if (id.Contains("wokerers_man"))
                want = FrameGame.Silver >= Trade.Debt ? new[] { "all", "feed", "put_off" } : new[] { "feed", "put_off" };
            else if (id.Contains(".goons."))
                want = FrameGame.Silver >= Trade.Debt ? new[] { "all" } : FrameGame.Silver >= 240.0 ? new[] { "pound", "cargo", "nothing" } : new[] { "cargo", "nothing" };
            else if (id.Contains("wokerers_day"))
                want = new[] { "all", "short" };
            else if (id.Contains("wokerers_stick"))
                want = new[] { "take" };
            else
            {
                var deck = EncounterRunner.Director;
                var rule = deck != null && deck.rules != null ? deck.rules.For(id) : null;
                if (rule != null && rule.take != null)
                    want = rule.take.ToArray();
            }
            if (want != null)
                foreach (string key in want)
                    for (int i = 0; i < s.options.Count; i++)
                        if (s.options[i].enabled && s.options[i].def != null && s.options[i].def.key == key)
                            return i;
            return s.options.FindIndex(o => o.enabled);
        }

        /// <summary>Which choice the trader takes in a window that is not a card: an order offered is taken when its goods
        /// are aboard or can be bought where she lies, else refused; raiders are run from, and talked or paid off when
        /// they are alongside; anything else, the first choice that can be made.</summary>
        int PickOther(string title, List<(Button button, string words, bool can)> choices)
        {
            string id = Frame.FloatIds.LastOrDefault() ?? "";
            string[] want = null;
            if (id.StartsWith("order."))
            {
                var offer = Commissions.OfferedToYou.FirstOrDefault(c => OrderAsk.IdOf(c) == id);
                var market = FrameDeal.MarketHere();
                var sold = offer != null && market != null ? market.Get(offer.good) : null;
                bool can = offer != null && (Trade.Hold.Count(offer.good) >= offer.units || (sold != null && sold.OnOffer && sold.stock >= offer.units));
                Say($"an order offered ({(offer != null ? Commissions.Describe(offer) : id)}): {(can ? "taken" : "refused: its goods are not to be had here")}");
                want = can ? new[] { "Take" } : new[] { "Refuse" };
            }
            else if (title == "Raiders")
            {
                alongside = 0;
                want = new[] { "Run", "Parley", "Pay tribute", "Submit" };
            }
            else if (title == "Raiders alongside")
            {
                // (a parley they will not hear brings the same window back, with Parley still on it: talk once, then
                // pay, then give up)
                want = alongside == 0 ? new[] { "Parley", "Pay tribute", "Submit" } : alongside == 1 ? new[] { "Pay tribute", "Submit" } : new[] { "Submit", "Turn and fight" };
                Say($"raiders alongside, the {(alongside == 0 ? "first" : alongside == 1 ? "second" : "third")} time: {ChartPlay.Short(ChartPlay.Read(play.FrontFloat()))}");
                alongside++;
            }
            if (want != null)
                foreach (string w in want)
                {
                    int at = choices.FindIndex(c => c.can && c.words.Contains(w));
                    if (at >= 0)
                        return at;
                }
            return choices.FindIndex(c => c.can);
        }

        /// <summary>A chase is on: its window stays up while she runs, and is not to be answered (the clock runs under it).</summary>
        bool Chased => play.FrontFloat() != null && Title(play.FrontFloat()) == "Running from raiders";

        /// <summary>The window in front is said through and answered, one step of it: a card's choice by the trader's
        /// rule, any other window by its first choice that can be made (or its cross).</summary>
        IEnumerator HandleFloat()
        {
            var front = play.FrontFloat();
            if (front == null || Chased)
                yield break;
            if (Frame.FloatOpen(FrameRoot.LostId))
            {
                yield return TheLastPage();
                yield break;
            }
            for (int i = 0; i < 40 && Frame.FrontReveal != null && !Frame.FrontReveal.Done; i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(4);
            }
            yield return ChartPlay.Settle(0.2f);
            front = play.FrontFloat();
            if (front == null)
                yield break;
            string title = Title(front);
            var deck = EncounterRunner.Director;
            var session = Frame.FloatOpen("card") && deck != null ? deck.Current : null;
            var choices = play.Choices(front);
            bool asks = session != null && !session.Chosen && choices.Count == session.options.Count;
            string key = session != null ? session.card.id + (asks ? "" : " (what came of it)") : "window: " + title;
            int times = Count(key);
            if (times < 2)
            {
                yield return Shot((session != null ? "card_" + session.card.id + (asks ? "" : "_end") : "window_" + title));
                if (play.FrontFloat() != front)
                    yield break;                                         // (another window came up over it while the picture was taken: that one first)
            }
            int pick = asks ? PickFor(session) : session == null ? PickOther(title, choices) : choices.FindIndex(c => c.can);
            if (pick < 0 || pick >= choices.Count)
            {
                var cross = front.Q<Button>(className: "frame-float-shut");
                if (cross == null)
                {
                    yield return Shot("FAULT_no_way_out_of_" + title);
                    Fault($"the window '{title}' has nothing that can be chosen and no cross: {ChartPlay.Short(ChartPlay.Read(front))}");
                    Assert.Fail("a window with no way out: " + title);
                }
                Say($"'{title}': shut by its cross");
                yield return play.Click(cross, "the cross of '" + title + "'");
                yield break;
            }
            if (asks)
                cardsAnswered++;
            if (session != null && asks)
                Say($"card {session.card.id} '{title}': {choices[pick].words}");
            else if (times < 3)
                Say($"'{title}': {choices[pick].words}");
            yield return play.Click(choices[pick].button, $"the choice '{choices[pick].words}' of '{title}'");
            yield return ChartPlay.Frames(4);
        }

        /// <summary>A scene is being told over the game (Ravn's death, 9 Oct 2026): it is read through a box at a time, as a
        /// player would, with a picture of its first box and of the name it ends on. The clock is held under it.</summary>
        IEnumerator ASceneToldThrough()
        {
            var show = Showtime.Current;
            if (show == null || !Showtime.Telling)
                yield break;
            Say("a scene is told over the game");
            float until = Time.realtimeSinceStartup + 240f;
            int boxes = 0;
            bool named = false;
            while (Showtime.Telling && Time.realtimeSinceStartup < until)
            {
                if (!named && show.CardText.Length > 0)
                {
                    named = true;
                    yield return Shot("scene_the_name_it_ends_on");
                    Say("the scene ends on: " + show.CardText);
                }
                if (show.SceneWaits)
                {
                    if (boxes == 0)
                        yield return Shot("scene_its_first_box");
                    boxes++;
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(3);
                }
                else
                    yield return null;
            }
            if (Showtime.Telling)
            {
                Fault("a scene told over the game did not end in four minutes on the wall");
                Assert.Fail("a scene that does not end");
            }
            Say($"the scene was read through: {boxes} boxes of talk");
        }

        IEnumerator HandleFloats(int most = 12)
        {
            yield return ASceneToldThrough();
            for (int i = 0; i < most && play.FrontFloat() != null && !Chased && !ended; i++)
            {
                yield return HandleFloat();
                yield return ChartPlay.Frames(3);
            }
        }

        /// <summary>She is lost: the last page is pictured, and the last saved game is loaded (the game saves itself at
        /// every mooring). Lost four times, the walk gives it up.</summary>
        IEnumerator TheLastPage()
        {
            lost++;
            yield return ChartPlay.Until(() => Frame.FloatOpen(FrameRoot.LostId), 6f);
            yield return Shot("she_is_lost_" + lost);
            string page = ChartPlay.Short(ChartPlay.Read(play.FrontFloat()));
            Say("SHE IS LOST: " + Boat.LostHow + " | the last page: " + page);
            var ways = play.Choices(play.FrontFloat());
            if (lost >= 4 || ways.Count == 0 || !ways[0].can)
            {
                ended = true;
                endedHow = lost >= 4 ? "she was lost four times: the walk gave it up" : "she was lost and no saved game could be loaded";
                yield break;
            }
            yield return play.Click(ways[0].button, "Load the last saved game");
            yield return ChartPlay.Until(() => !Boat.IsLost && !Frame.FloatOpen(FrameRoot.LostId), 30f);
            yield return ChartPlay.Frames(30);
            if (Boat.IsLost)
            {
                Fault("the last page's 'Load the last saved game' did not bring her back");
                ended = true;
                endedHow = "she was lost and the load did not bring her back";
                yield break;
            }
            Say("the last saved game is loaded");
            yield return Shot("back_from_the_last_save_" + lost);
        }

        // ------------------------------------------------------------------ the opening

        static IEnumerator EnterUntil(ChartPlay play, System.Func<bool> there, int most = 40)
        {
            for (int i = 0; i < most && !there(); i++)
            {
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(4);
            }
        }

        IEnumerator TheOpening()
        {
            yield return ChartPlay.Until(() => Showtime.Current != null && Showtime.Current.Stage == "loading", 15f);
            var show = Showtime.Current;
            Assert.IsNotNull(show, "the show did not start with the menu");
            yield return ChartPlay.Until(() => Up(show.Loader), 6f);
            yield return ChartPlay.Settle(2.6f);
            yield return Shot("the_loader", 0f);
            yield return ChartPlay.Until(() => show.Stage == "title", 25f);
            yield return ChartPlay.Settle(1.5f);
            yield return Shot("the_title", 0f);
            // Esc: the menu under the name
            yield return play.Press(Key.Escape);
            yield return ChartPlay.Frames(4);
            yield return Shot("the_title_menu");
            yield return play.Press(Key.Escape);
            yield return ChartPlay.Frames(4);
            // a click anywhere plays
            Rect whole = show.BigName.parent.worldBound;
            play.ClickAt(new Vector2(whole.width * 0.3f, whole.height * 0.75f));
            yield return ChartPlay.Until(() => show.Stage == "slide" && Up(show.Box), 16f);
            yield return ChartPlay.Settle(0.5f);
            yield return Shot("the_box_rising", 0f);
            yield return ChartPlay.Until(() => show.Stage == "asking", 15f);
            Assert.AreEqual("asking", show.Stage, "the box did not open to ask");
            yield return EnterUntil(play, () => show.Choices.childCount > 0);
            if (show.Stage == "face")
            {
                yield return ChartPlay.Settle(0.3f);
                yield return Shot("your_face");
                yield return play.Press(Key.Digit1);
                yield return ChartPlay.Until(() => show.Stage == "asking", 4f);
                yield return EnterUntil(play, () => show.Stage == "asking" && show.Choices.childCount > 0 && show.Words.text == Showtime.AskPeople);
            }
            yield return Shot("where_do_you_hail_from");
            yield return play.Press(Key.Digit1);
            yield return ChartPlay.Until(() => show.NameField != null, 6f);
            Assert.IsNotNull(show.NameField, "no box to type a name in");
            yield return ChartPlay.Until(() => show.Choices.Query<Button>().ToList().Count >= 3, 4f);
            var given = show.Choices.Query<Button>().ToList().FirstOrDefault(b => b.text == "A man's name");
            if (given != null)
                yield return play.Click(given, "A man's name");
            yield return ChartPlay.Frames(3);
            yield return Shot("your_name");
            yield return play.Press(Key.Enter);
            yield return ChartPlay.Until(() => show.NameField == null, 4f);
            yield return ChartPlay.Until(() => show.Words.text == Showtime.AskFaith && show.Choices.childCount > 0, 6f);
            yield return Shot("by_whom_do_you_swear");
            yield return play.Press(Key.Digit1);
            yield return ChartPlay.Until(() => show.Words.text == Showtime.AskPast && show.Choices.childCount > 0, 6f);
            yield return Shot("what_were_you_before");
            yield return play.Press(Key.Digit1);
            yield return ChartPlay.Until(() => show.Stage == "lender", 6f);
            Assert.AreEqual("lender", show.Stage, "the lender did not speak after the questions");
            yield return ChartPlay.Settle(0.5f);
            yield return play.Press(Key.Enter);
            yield return ChartPlay.Frames(3);
            yield return Shot("the_lender");
            // each of his lines, pictured as it stands whole
            for (int i = 0; i < 60 && show.Stage == "lender"; i++)
            {
                string was = show.Words.text;
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(4);
                if (show.Stage == "lender" && show.Words.text != was && !show.Words.text.Contains("<color=#00000000>") && Count("lender line") < 12)
                    yield return Shot("the_lender_goes_on", 0.1f);
            }
            yield return ChartPlay.Until(() => show.Stage == "showroom" || show.Stage == "first morning", 8f);
            if (show.Stage == "showroom")
            {
                yield return ChartPlay.Settle(4f);
                yield return Shot("the_showroom");
                yield return play.Press(Key.Enter);
            }
            yield return ChartPlay.Until(() => show.Stage == "first morning", 8f);
            yield return ChartPlay.Settle(1.2f);
            yield return Shot("the_first_morning", 0f);
            yield return ChartPlay.Until(() => show.Stage == "free", 15f);
            Assert.AreEqual("free", show.Stage, "the first morning did not give the game to the player");
            yield return ChartPlay.Settle(1f);
            yield return Shot("the_game_begins");
            Say($"the game begins: {Self.Called}, {Self.People}; the purse {FrameGame.Silver:0} d, owed {Trade.Debt:0} d by {Trade.DebtDueName}");
            silverAtStart = bestSilver = FrameGame.Silver;
            // the first click: the marks, then Ravn
            Rect all = Frame.Root.worldBound;
            play.ClickAt(new Vector2(all.width * 0.5f, all.height * 0.5f));
            yield return ChartPlay.Until(() => show.Stage == "marks", 3f);
            yield return ChartPlay.Settle(0.75f);
            yield return Shot("the_marks", 0f);
            yield return ChartPlay.Until(() => Frame.FloatOpen("ravn.first"), 12f);
            Assert.IsTrue(Frame.FloatOpen("ravn.first"), "Ravn did not speak after the marks");
            yield return ChartPlay.Settle(0.8f);
            // Ravn, a paragraph at a time: each pictured when it stands whole
            for (int press = 0; press < 40 && Frame.FloatOpen("ravn.first") && Frame.FrontReveal != null && !Frame.FrontReveal.Done; press++)
            {
                float until = Time.realtimeSinceStartup + 3f;
                while (Time.realtimeSinceStartup < until && Frame.FrontReveal != null && !Frame.FrontReveal.Waiting && !Frame.FrontReveal.Done)
                    yield return null;
                if (Frame.FrontReveal == null || Frame.FrontReveal.Done)
                    break;
                if (!Frame.FrontReveal.Waiting)
                {
                    yield return play.Press(Key.Enter);                 // (the paragraph finished at once)
                    yield return ChartPlay.Frames(3);
                }
                yield return Shot("ravn_" + press, 0.1f);
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Frames(3);
            }
            yield return ChartPlay.Settle(0.4f);
            if (Frame.FloatOpen("ravn.first"))
            {
                yield return Shot("ravn_has_said_it");
                Say("Ravn: " + ChartPlay.Short(ChartPlay.Read(play.FrontFloat())));
                yield return play.Press(Key.Enter);
                yield return ChartPlay.Until(() => !Frame.FloatOpen("ravn.first"), 4f);
            }
            // the first order: asked, and taken
            yield return ChartPlay.Until(() => Commissions.Mine.Count > 0 ||
                                               (Commissions.OfferedToYou.Count > 0 && Frame.FloatOpen(OrderAsk.IdOf(Commissions.OfferedToYou[0]))), 6f);
            if (Commissions.OfferedToYou.Count > 0 && Frame.FloatOpen(OrderAsk.IdOf(Commissions.OfferedToYou[0])))
            {
                yield return ChartPlay.Frames(10);
                yield return Shot("the_first_order");
                Say("the first order: " + ChartPlay.Short(ChartPlay.Read(play.FrontFloat())));
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Take the order");
                yield return ChartPlay.Frames(10);
            }
            else if (Commissions.Mine.Count == 0)
                Fault("Ravn has done and no order is offered");
        }

        // ------------------------------------------------------------------ the first jobs, by their own buttons

        IEnumerator TheFirstJobs()
        {
            yield return ChartPlay.Until(() => Frame.JobsShown, 3f);
            yield return Shot("the_list_before_she_sails");
            if (!Frame.JobsShown)
                Fault("the list Before she sails is not on the screen after Ravn");
            // every window, as it stands on the first morning
            yield return EveryWindow("first_morning");
            // the sail, by its row's button
            var todo = SideWindow()?.Q<Button>("todo");
            if (todo != null && !Frame.TodoOn)
                yield return play.Click(todo, "the tab To do");
            yield return ChartPlay.Settle(0.3f);
            var mend = Frame.Root.Q<Button>("mend-sail");
            if (mend != null && ChartPlay.Shown(mend))
            {
                yield return play.Click(mend, "the cost, in the sail's row of the list");
                yield return ChartPlay.Settle(0.4f);
                yield return Shot("the_sail_goes_ashore");
            }
            else
                Fault("the sail's row has no button to have it mended");
            // the men, in the alehouse
            var ale = Verb("Alehouse");
            if (ale == null || !ale.enabledSelf)
                Fault("there is no Alehouse to go to for men");
            else
            {
                yield return play.Click(ale, "the Alehouse button");
                yield return ChartPlay.Until(() => play.FrontFloat() != null, 4f);
                yield return ChartPlay.Settle(0.5f);
                for (int i = 0; i < 40 && Frame.FrontReveal != null && !Frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);
                    yield return ChartPlay.Frames(4);
                }
                yield return Shot("the_alehouse");
                for (int i = 0; i < 4 && FirstJobs.HandsShort > 0; i++)
                {
                    var hire = play.Choices(play.FrontFloat()).FirstOrDefault(c => c.can && c.words.Contains("Hire "));
                    if (hire.button == null)
                    {
                        Fault("the alehouse has no man to hire though she wants " + FirstJobs.HandsShort + ": " + ChartPlay.Short(ChartPlay.Read(play.FrontFloat())));
                        break;
                    }
                    Say("hired: " + hire.words);
                    yield return play.Click(hire.button, hire.words);
                    yield return ChartPlay.Settle(0.5f);
                    for (int k = 0; k < 40 && Frame.FrontReveal != null && !Frame.FrontReveal.Done; k++)
                    {
                        yield return play.Press(Key.Enter);
                        yield return ChartPlay.Frames(4);
                    }
                    if (i == 0)
                        yield return Shot("a_man_hired");
                }
                yield return Shot("the_men_are_hired");
                var leave = play.Choices(play.FrontFloat()).FirstOrDefault(c => c.can && c.words.Contains("Leave"));
                if (leave.button != null)
                    yield return play.Click(leave.button, "Leave");
                yield return ChartPlay.Frames(6);
                yield return HandleFloats();
            }
            if (FirstJobs.HandsShort > 0)
            {
                Fault("the men could not be hired by the alehouse's own buttons: hired by the back door, to go on");
                for (int i = 0; i < FirstJobs.Hands.Count && FirstJobs.HandsShort > 0; i++)
                    FirstJobs.Hire(i, out _);
            }
            // six hours of the clock mend the sail
            yield return Speed(5);
            float from = Time.realtimeSinceStartup;
            while (FirstJobs.SailWorn && Time.realtimeSinceStartup - from < 90f)
            {
                if (play.FrontFloat() != null)
                    yield return HandleFloat();
                else if (GameSpeed.Level < 4)
                    yield return Speed(5);
                yield return null;
            }
            if (FirstJobs.SailWorn)
                Fault("the sail was not mended in six hours of the clock");
            yield return Speed(1);
            yield return ChartPlay.Settle(0.4f);
            yield return Shot("the_sail_is_mended");
        }

        // ------------------------------------------------------------------ the clock

        IEnumerator Speed(int level)
        {
            if (play.FrontFloat() != null && !Chased)
                yield break;                                             // (a window that wants its answer holds the clock)
            var b = Small(level.ToString());
            if (b == null)
            {
                Fault("no button for speed " + level);
                GameSpeed.SetLevel(level - 1);
                yield break;
            }
            yield return play.Click(b, "speed " + level);
        }

        // ------------------------------------------------------------------ every window, looked at

        /// <summary>The side window stepped through all it can show by its own arrow, a picture of each.</summary>
        IEnumerator EveryWindow(string when)
        {
            var win = SideWindow();
            if (win == null)
            {
                Fault("no side window to look through");
                yield break;
            }
            var titles = new List<string>();
            for (int i = 0; i < 30; i++)
            {
                win = SideWindow();
                var next = win?.Q<Button>("next");
                if (next == null || !ChartPlay.Shown(next))
                {
                    Fault("the side window has no arrow to its next kind");
                    break;
                }
                yield return play.Click(next, "the side window's arrow");
                yield return ChartPlay.Settle(0.3f);
                win = SideWindow();
                string title = win?.Q<Label>(className: "frame-win-name")?.text ?? "";
                if (titles.Contains(title))
                    break;
                titles.Add(title);
                yield return Shot($"window_{title}_{when}");
            }
            Say($"every window ({when}): " + string.Join(", ", titles));
        }

        // ------------------------------------------------------------------ the market

        static bool Stores(string good) =>
            System.Array.IndexOf(Voyage.Rules.foodGoods, good) >= 0 || System.Array.IndexOf(Voyage.Rules.drinkGoods, good) >= 0;

        /// <summary>A button of a good's row in the Trade window (buy-one, buy-half, buy-all, sell-one, sell-half, sell-all).</summary>
        Button TradeButton(GoodDef good, string name)
        {
            var win = Win("Trade");
            if (win == null || good == null)
                return null;
            Label label = null;
            win.Query<Label>("name").ForEach(l =>
            {
                if (label == null && l.text == good.name && ChartPlay.Shown(l))
                    label = l;
            });
            if (label == null)
                return null;
            VisualElement line = label;
            while (line != null && !line.ClassListContains("frame-ln"))
                line = line.parent;
            if (line == null || line.parent == null)
                return null;
            var body = line.parent;
            for (int j = body.IndexOf(line); j < body.childCount; j++)
            {
                var child = body[j];
                if (child != line && child.ClassListContains("frame-ln"))
                    break;
                var b = child.Q<Button>(name);
                if (b != null && ChartPlay.Shown(b))
                    return b;
            }
            return null;
        }

        /// <summary>Sells a good by the lot's buttons: all of it (or its one).</summary>
        IEnumerator SellAll(GoodDef good)
        {
            int had = Trade.Hold.Count(good.id);
            if (had <= 0)
                yield break;
            yield return Open("Trade", "Trade");
            double silver = FrameGame.Silver;
            var b = TradeButton(good, "sell-all") ?? TradeButton(good, "sell-one");
            if (b == null)
            {
                Fault($"the Trade window has no button to sell {good.name} though {had} are aboard");
                yield break;
            }
            yield return play.Click(b, "sell " + good.name);
            yield return ChartPlay.Frames(5);
            // (one that was left, where the lot was a single)
            for (int i = 0; i < 3 && Trade.Hold.Count(good.id) > 0 && Trade.Hold.Count(good.id) < had; i++)
            {
                b = TradeButton(good, "sell-all") ?? TradeButton(good, "sell-one");
                if (b == null)
                    break;
                yield return play.Click(b, "sell the rest of " + good.name);
                yield return ChartPlay.Frames(5);
            }
            int sold = had - Trade.Hold.Count(good.id);
            Say($"sold {sold} of {had} {good.name} for {FrameGame.Silver - silver:0.#} d");
            if (sold == 0)
                Say($"   (they would not take it: {ChartPlay.Short(ChartPlay.Read(Win("Trade")?.Q<Label>(className: "frame-text")))})");
        }

        /// <summary>Buys up to `count` of a good by the lot's buttons: all, half, and ones.</summary>
        IEnumerator Buy(GoodDef good, int count)
        {
            int had = Trade.Hold.Count(good.id);
            double silver = FrameGame.Silver;
            var market = FrameDeal.MarketHere();
            if (market == null || count <= 0)
                yield break;
            yield return Open("Trade", "Trade");
            for (int click = 0; click < 12; click++)
            {
                int left = count - (Trade.Hold.Count(good.id) - had);
                if (left <= 0)
                    break;
                var all = FrameDeal.Quote(market, good.id, true, LotSize.All);
                if (!all.ok)
                    break;
                int half = FrameDeal.CountOf(LotSize.Half, all.count);
                string name = left >= all.count && all.count > 1 ? "buy-all" : half > 1 && half < all.count && left >= half ? "buy-half" : "buy-one";
                var b = TradeButton(good, name) ?? TradeButton(good, "buy-one");
                if (b == null)
                {
                    Fault($"the Trade window has no button to buy {good.name}");
                    break;
                }
                int before = Trade.Hold.Count(good.id);
                yield return play.Click(b, name + " of " + good.name);
                yield return ChartPlay.Frames(5);
                if (Trade.Hold.Count(good.id) == before)
                    break;
            }
            int got = Trade.Hold.Count(good.id) - had;
            if (got > 0)
                Say($"bought {got} {good.name} for {silver - FrameGame.Silver:0.#} d");
        }

        sealed class Plan
        {
            public string dest;
            public double km, days, gain, score;
            public readonly List<(GoodDef good, int count)> buys = new List<(GoodDef, int)>();
        }

        /// <summary>The ports the Voyage window lists, with the way to each as it says it.</summary>
        IEnumerator Reach(List<(PortDef port, double km)> reach)
        {
            reach.Clear();
            yield return Open("Voyage", "Voyage");
            float from = Time.realtimeSinceStartup;
            bool reckoning = true;
            while (reckoning && Time.realtimeSinceStartup - from < 12f)
            {
                reckoning = false;
                var win = Win("Voyage");
                if (win == null)
                    break;
                win.Query<Label>("small").ForEach(l => reckoning |= l.text == "reckoning the way");
                yield return null;
            }
            var window = Win("Voyage");
            if (window == null)
                yield break;
            window.Query<Button>().ForEach(b =>
            {
                var name = b.Q<Label>("name");
                var small = b.Q<Label>("small");
                var way = name != null && small != null ? System.Text.RegularExpressions.Regex.Match(small.text, @"^(\d+) km by water") : null;
                if (way == null || !way.Success)
                    return;
                var def = GameData.Ports.FirstOrDefault(p => p.name == name.text);
                if (def != null && double.TryParse(way.Groups[1].Value, out double km))
                    reach.Add((def, km));
            });
        }

        double KmHome(PortDef from)
        {
            if (from.id == Home)
                return 0.0;
            var (x, z) = WorldFrame.ToWorld(from.lat, from.lon);
            var line = new List<WorldXZ>();
            return Course.Plan(Home, new WorldXZ(x, z), from.id, line, out double km) ? km : 1.0e9;
        }

        /// <summary>Where next, and with what: for each port in reach, what could be bought here and sold there for the
        /// most, with the silver and the room there is; the port whose gain a day is best, that still leaves the days
        /// to be home for the day.</summary>
        Plan Choose(PortDef here, List<(PortDef port, double km)> reach, double daysLeft)
        {
            var game = Game;
            var market = Markets.Of(here, game.Year, game.Day);
            double silver = Trade.Purse.Worth(market.zone, TradeRules.Current) - Reserve;
            Plan best = null;
            foreach (var (port, km) in reach)
            {
                if (port.id == here.id)
                    continue;
                double days = System.Math.Max(0.4, km / KmADay);
                if (days + KmHome(port) / KmADay + 2.0 > daysLeft)
                    continue;                                            // (she could not be home for the day from there)
                var there = Markets.Of(port, game.Year, game.Day);
                if (there == null || there.Disrupted(game.Day))
                    continue;
                double toll = Chest.TollFreeAt(port, game.Year) ? 0.0 : there.zone.toll * EncounterRunner.TollFactor(port.id, game.Day);
                var plan = new Plan { dest = port.id, km = km, days = days };
                var picks = new List<(MarketGood g, double ask, double pay)>();
                if (!market.Disrupted(game.Day))
                    foreach (var g in market.Goods)
                    {
                        if (!g.OnOffer || g.stock < 1f || Stores(g.good.id) || !string.IsNullOrEmpty(g.good.law))
                            continue;
                        var wanted = there.Get(g.good.id);
                        if (wanted == null)
                            continue;
                        double ask = Deal.AsksFor(market, g, game.Month, "player");
                        double pay = Deal.PaysFor(there, wanted, game.Month, "player") * (1.0 - toll);
                        if (pay > ask * 1.12 && ask > 0.0)
                            picks.Add((g, ask, pay));
                    }
                picks.Sort((a, b) => ((b.pay - b.ask) / b.ask).CompareTo((a.pay - a.ask) / a.ask));
                double left = silver;
                float room = Trade.Hold.Free;
                foreach (var (g, ask, pay) in picks)
                {
                    var wanted = there.Get(g.good.id);
                    int n = System.Math.Min((int)g.stock, System.Math.Min((int)(left / (ask * 1.12)), (int)(room / System.Math.Max(0.001f, g.good.Space))));
                    n = System.Math.Min(n, System.Math.Max(1, (int)System.Math.Ceiling(wanted.appetite - wanted.glut)));
                    if (n <= 0)
                        continue;
                    plan.buys.Add((g.good, n));
                    left -= n * ask * 1.06;
                    room -= n * g.good.Space;
                    plan.gain += n * (pay - ask) * 0.8;                  // (the price climbs as she buys and falls as she sells)
                    if (plan.buys.Count >= 3)
                        break;
                }
                // what is aboard already and would fetch more there than here
                foreach (var (good, count) in Trade.Hold.Contents)
                {
                    if (Stores(good.id))
                        continue;
                    var wanted = there.Get(good.id);
                    if (wanted != null)
                        plan.gain += count * Deal.PaysFor(there, wanted, game.Month, "player") * (1.0 - toll) * 0.5;
                }
                // (an order of hers that is wanted there: its day is a stake)
                foreach (var c in Commissions.Mine)
                    if (c.port == port.id && Trade.Hold.Count(c.good) > 0)
                        plan.gain += 60.0;
                plan.score = plan.gain / (days + 0.6);
                if (best == null || plan.score > best.score)
                    best = plan;
            }
            return best;
        }

        // ------------------------------------------------------------------ a call at a port

        IEnumerator PortCall()
        {
            calls++;
            var game = Game;
            var here = FrameGame.PortHere;
            if (here == null)
                yield break;
            yield return Speed(1);
            // (the place's name stands across the screen for a moment, and what meets her on the quay comes after it)
            yield return ChartPlay.Settle(1.2f);
            yield return HandleFloats();
            if (ended)
                yield break;
            bestSilver = System.Math.Max(bestSilver, FrameGame.Silver);
            Say($"== call {calls}: {here.name}. Purse {FrameGame.Silver:0} d, owed {Trade.Debt:0} d ({Trade.DebtDaysLeft(game.Day):0} days left). " +
                $"Men {Voyage.Men}, {FrameGame.HealthWord(Voyage.Health)}, {FrameGame.HeartWord(Voyage.Morale)}, {Boat.WearyWord}. " +
                $"Hull {Boat.HullWord}, sail {Boat.ClothWord}. Bread {Voyage.FoodDays:0.#} days. Aboard: " +
                string.Join(", ", Trade.Hold.Contents.Select(c => c.count + " " + c.good.name)));
            if (calls <= 3 || calls % 6 == 0)
                yield return Shot("arrived_at_" + here.name);

            // ---- an order to hand over here
            if (Commissions.Mine.Any(c => c.port == here.id && Trade.Hold.Count(c.good) > 0 && c.Left > 0))
            {
                yield return Open("Errands", "Errands");
                yield return ChartPlay.Settle(0.3f);
                var hand = play.ButtonStarting("Hand over", Win("Errands"));
                if (hand != null)
                {
                    yield return Shot("an_order_to_hand_over_at_" + here.name);
                    double before = FrameGame.Silver;
                    yield return play.Click(hand, "Hand over");
                    yield return ChartPlay.Settle(0.3f);
                    Say($"handed over an order here: {FrameGame.Silver - before:0.#} d");
                    yield return Shot("the_order_handed_over");
                }
                else
                    Fault("an order of hers is wanted here and aboard, and Errands has no button to hand it over");
                yield return HandleFloats();
            }

            // ---- her seams or her sail, at a yard
            if (Boat.WantsSeams || Boat.SailMendCost > 0.0)
            {
                Frame.Show("ship");
                yield return ChartPlay.Settle(0.4f);
                yield return Shot("the_ship_wants_a_yard");
                foreach (string words in new[] { "Have her seams done", "Have her sail done" })
                {
                    var mend = play.Button(words, Win("Ship"));
                    if (mend != null && mend.enabledSelf)
                    {
                        yield return play.Click(mend, words);
                        yield return ChartPlay.Settle(0.3f);
                        Say(words + ": done");
                    }
                }
                yield return Shot("the_ship_after_the_yard");
            }

            // ---- the market: sell what she brought
            yield return Open("Trade", "Trade");
            yield return ChartPlay.Settle(0.3f);
            if (calls <= 4 || calls % 5 == 0)
                yield return Shot("the_market_at_" + here.name);
            var market = FrameDeal.MarketHere();
            if (market == null || market.Disrupted(game.Day))
                Say("no market here today");
            else
            {
                foreach (var (good, count) in Trade.Hold.Contents.ToList())
                {
                    if (Stores(good.id) || count <= 0)
                        continue;
                    if (Commissions.Mine.Any(c => c.good == good.id && c.Left > 0 && c.port != here.id))
                        continue;                                        // (kept for the order it is promised to)
                    var quote = FrameDeal.Quote(market, good.id, false, LotSize.All);
                    boughtFor.TryGetValue(good.id, out string meant);
                    bool sell = quote.ok && (meant == here.id || meant == null || quote.deniers - quote.toll >= quote.cost * 1.02);
                    if (sell)
                        yield return SellAll(good);
                    else
                        Say($"kept {count} {good.name}: here it would fetch {quote.deniers - quote.toll:0} d against {quote.cost:0} d paid");
                }
                yield return HandleFloats();
                // bread for the men
                if (Voyage.FoodDays < 12.0)
                {
                    GoodDef bread = null;
                    double cheapest = double.MaxValue;
                    foreach (string id in Voyage.Rules.foodGoods)
                    {
                        var g = market.Get(id);
                        if (g == null || !g.OnOffer || g.stock < 1f)
                            continue;
                        double perKg = Deal.AsksFor(market, g, game.Month, "player") / System.Math.Max(0.1f, g.good.kg);
                        if (perKg < cheapest)
                        {
                            cheapest = perKg;
                            bread = g.good;
                        }
                    }
                    if (bread == null)
                        Say($"NO FOOD ON OFFER HERE and only {Voyage.FoodDays:0.#} days aboard");
                    else
                    {
                        int want = (int)System.Math.Ceiling((24.0 - Voyage.FoodDays) * Voyage.Men * Voyage.Rules.foodKg / System.Math.Max(0.1f, bread.kg));
                        double days = Voyage.FoodDays;
                        yield return Buy(bread, System.Math.Max(1, System.Math.Min(want, 6)));
                        Say($"bread: from {days:0.#} to {Voyage.FoodDays:0.#} days");
                    }
                }
            }

            // ---- the debt, when the silver is there
            if (Trade.Debt > 0.01 && FrameGame.Silver >= Trade.Debt + Reserve)
            {
                yield return Open("Errands", "Errands");
                yield return ChartPlay.Settle(0.3f);
                yield return Shot("the_silver_for_the_debt");
                var all = play.Button("Pay it all", Win("Errands"));
                if (all != null && all.enabledSelf)
                {
                    yield return play.Click(all, "Pay it all");
                    yield return ChartPlay.Settle(0.4f);
                    Say($"THE DEBT IS PAID, with {Trade.DebtDaysLeft(game.Day):0} days to spare. The purse: {FrameGame.Silver:0} d");
                    yield return Shot("the_debt_is_paid");
                }
                else
                    Fault("she has the silver for the debt and Errands will not take it");
                yield return HandleFloats();
            }

            // ---- the men rest
            if (Boat.Weary >= 0.35)
            {
                restedTimes++;
                yield return Open("Crew", "Crew");
                if (restedTimes <= 2)
                    yield return Shot("the_men_are_" + Boat.WearyWord);
                double was = Boat.Weary, fromDay = game.Day;
                yield return Speed(5);
                float t = Time.realtimeSinceStartup;
                while (Boat.Weary > 0.08 && Time.realtimeSinceStartup - t < 60f && !ended)
                {
                    if (play.FrontFloat() != null)
                        yield return HandleFloat();
                    else if (GameSpeed.Level < 4)
                        yield return Speed(5);
                    yield return null;
                }
                yield return Speed(1);
                Say($"the men rested {(game.Day - fromDay) * 24.0:0} hours made fast: weariness {was:0.00} to {Boat.Weary:0.00}");
                if (restedTimes <= 2)
                    yield return Shot("the_men_rested");
            }
            if (calls == 3 || calls == 12 || calls == 30)
                yield return EveryWindow("call_" + calls);
        }

        /// <summary>The course set in the Voyage window: the port's line, then Set this course.</summary>
        IEnumerator SetCourse(string dest)
        {
            var def = GameData.Port(dest);
            yield return Open("Voyage", "Voyage");
            if (Course.IsSet && Course.Destination == dest)
                yield break;
            Button line = null;
            float from = Time.realtimeSinceStartup;
            while (line == null && Time.realtimeSinceStartup - from < 6f)
            {
                var win = Win("Voyage");
                if (win != null)
                    win.Query<Button>().ForEach(b =>
                    {
                        var name = b.Q<Label>("name");
                        if (line == null && name != null && name.text == def.name && ChartPlay.Shown(b))
                            line = b;
                    });
                if (line == null)
                    yield return null;
            }
            if (line == null)
            {
                Fault($"the Voyage window does not list {def.name}");
                yield break;
            }
            yield return play.Click(line, def.name + ", in the Voyage window");
            yield return ChartPlay.Frames(4);
            float t = Time.realtimeSinceStartup;
            Button set = null;
            while (set == null && Time.realtimeSinceStartup - t < 8f)
            {
                set = play.Button("Set this course", Win("Voyage"));
                if (set == null)
                    yield return null;
            }
            if (set == null)
            {
                yield return Shot("FAULT_no_course_to_" + def.name);
                Fault($"the Voyage window gives no 'Set this course' for {def.name}: {ChartPlay.Short(ChartPlay.Read(Win("Voyage")))}");
                // (put away again: the line picked stays picked otherwise)
                yield break;
            }
            if (legs < 3)
                yield return Shot("the_way_to_" + def.name);
            yield return play.Click(set, "Set this course");
            yield return ChartPlay.Frames(6);
        }

        /// <summary>She sails to a port: the course, Cast off, the clock at its fastest, and whatever comes on the way
        /// answered. Ends made fast somewhere (where she was bound, or where a gale or a card took her).</summary>
        IEnumerator SailTo(string dest)
        {
            var game = Game;
            var def = GameData.Port(dest);
            string fromName = FrameGame.PortHere != null ? FrameGame.PortHere.name : "the sea";
            yield return SetCourse(dest);
            if (!Course.IsSet || Course.Destination != dest)
            {
                Fault($"no course could be set for {def.name} from {fromName}");
                yield break;
            }
            double km = Course.Km, fromDay = game.Day;
            if (FrameGame.PortHere != null)
            {
                var cast = Verb("Cast off");
                if (cast == null || !cast.enabledSelf)
                {
                    Frame.Look(FrameIcon.Kind.CastOff, out _, out string why, out _, out _);
                    yield return Shot("FAULT_cannot_cast_off");
                    Fault("she cannot cast off: " + why);
                    yield break;
                }
                if (legs < 2)
                    yield return Shot("ready_to_cast_off_for_" + def.name);
                yield return play.Click(cast, "the Cast off button");
                yield return ChartPlay.Frames(10);
                if (game.ship.anchored)
                {
                    yield return HandleFloats();
                    yield return ChartPlay.Frames(10);
                }
                if (game.ship.anchored)
                {
                    yield return Shot("FAULT_cast_off_did_nothing");
                    Fault("the Cast off button was pressed and she is still made fast");
                    Assert.Fail("Cast off did not unmoor her");
                }
            }
            legs++;
            Say($"cast off from {fromName} for {def.name}: {km:0} km");
            bool half = false;
            float began = Time.realtimeSinceStartup, moved = began;
            var was = Where();
            double hours = game.Hours;
            int signs = Boat.Sign;
            while (!ended)
            {
                var port = FrameGame.PortHere;
                if (port != null && game.ship.anchored && game.Day - fromDay > 0.02)
                    break;
                if (Boat.IsLost || Frame.FloatOpen(FrameRoot.LostId))
                {
                    yield return TheLastPage();
                    yield break;                                         // (the loaded game is somewhere else: the season goes on from there)
                }
                if (Showtime.Telling)
                {
                    yield return ASceneToldThrough();                    // (the lender's men have killed Ravn: his scene)
                    moved = Time.realtimeSinceStartup;
                    continue;
                }
                if (play.FrontFloat() != null && !Chased)
                {
                    yield return HandleFloat();
                    yield return ChartPlay.Frames(3);
                    moved = Time.realtimeSinceStartup;
                    continue;
                }
                if (Chased && Count("a chase") < 2)
                    yield return Shot("running_from_raiders");
                if (!Course.IsSet && !Chased)
                {
                    // (a card or the weather took her course: where she lies now, she is given it again)
                    if (FrameGame.PortHere != null)
                        break;
                    Say("she has no course: given it again");
                    yield return SetCourse(dest);
                    if (!Course.IsSet)
                    {
                        yield return Shot("FAULT_no_course_at_sea");
                        Fault("at sea with no course and none could be set for " + def.name);
                        Assert.Fail("at sea with no course");
                    }
                }
                if (Boat.Sign != signs)
                {
                    signs = Boat.Sign;
                    if (signs > 0 && Count("a sign of weather") < 3)
                    {
                        Frame.Show("ship");
                        yield return ChartPlay.Settle(0.3f);
                        yield return Shot("a_sign_of_weather");
                    }
                }
                if (Boat.InGale && Count("in a gale") < 2)
                {
                    Frame.Show("ship");
                    yield return ChartPlay.Settle(0.3f);
                    yield return Shot("in_a_gale");
                }
                if (GameSpeed.Level < 4 || GameSpeed.PlayerPaused)
                    yield return Speed(5);
                if (!half && Course.IsSet && Course.KmLeft(ChartWorld.ShipWorld()) < km * 0.5)
                {
                    half = true;
                    if (legs <= 3 || legs % 6 == 0)
                        yield return Shot("at_sea_for_" + def.name);
                }
                yield return null;
                yield return null;
                if (game.Hours > hours + 0.01 || WorldXZ.Distance(Where(), was) > 5.0)
                {
                    moved = Time.realtimeSinceStartup;
                    hours = game.Hours;
                    was = Where();
                }
                else if (Time.realtimeSinceStartup - moved > 45f)
                {
                    yield return Shot("FAULT_nothing_moves");
                    Fault($"nothing moves on the way to {def.name}: paused {GameSpeed.Paused}, held {EncounterRunner.ShipHeld}, anchored {game.ship.anchored}, " +
                          $"course {Course.IsSet}, aground {Aground.Stage}, tide wait {TideWait.On}, windows {string.Join(",", Frame.FloatIds)}");
                    Assert.Fail("nothing moves at sea");
                }
                if (Time.realtimeSinceStartup - began > 1500f)
                {
                    yield return Shot("FAULT_a_leg_without_end");
                    Fault($"the leg to {def.name} ({km:0} km) did not end in 25 minutes of the clock on the wall: {game.Day - fromDay:0.0} days of the game");
                    Assert.Fail("a leg without end");
                }
            }
            if (ended)
                yield break;
            var at = FrameGame.PortHere;
            double took = game.Day - fromDay;
            Say($"made fast at {(at != null ? at.name : "?")} after {took:0.0} days ({(took > 0.01 ? km / took : 0.0):0} km a day)" +
                (at != null && at.id != dest ? $": NOT where she was bound ({def.name})" : ""));
        }

        // ------------------------------------------------------------------ the season

        IEnumerator TheSeason(double mostDays)
        {
            var game = Game;
            var reach = new List<(PortDef port, double km)>();
            double began = game.Day;
            bool first = true;
            string next = null;
            while (!ended)
            {
                yield return HandleFloats();
                if (ended)
                    break;
                var here = FrameGame.PortHere;
                if (here == null)
                {
                    // (at sea after a load, or left lying to: make for home)
                    yield return SailTo(next ?? Home);
                    continue;
                }
                double daysLeft = Trade.DebtDaysLeft(game.Day);
                if (!first)
                    yield return PortCall();
                if (ended)
                    break;
                if (mostDays > 0.0 && game.Day - began >= mostDays)
                {
                    endedHow = $"the short walk's {mostDays:0} days are done";
                    break;
                }
                if (daysLeft <= 0.0 && here.id == Home)
                    break;                                               // (the day: the reckoning)
                // ---- where next
                yield return Reach(reach);
                if (reach.Count == 0)
                {
                    yield return Shot("FAULT_nowhere_to_go");
                    Fault("the Voyage window lists nowhere to go from " + here.name);
                    Assert.Fail("nowhere to go");
                }
                Plan plan = null;
                if (first)
                {
                    // (the first cargo is Ravn's word, and the first port the first order's)
                    first = false;
                    var order = Commissions.Mine.FirstOrDefault();
                    var tip = GameData.Good(FirstJobs.TipGood);
                    plan = new Plan { dest = order != null ? order.port : FirstJobs.TipPort.Length > 0 ? FirstJobs.TipPort : "traiectum" };
                    yield return Open("Trade", "Trade");
                    yield return ChartPlay.Settle(0.3f);
                    yield return Shot("the_market_on_the_first_morning");
                    var market = FrameDeal.MarketHere();
                    if (tip != null && market != null && market.Get(tip.id) != null && market.Get(tip.id).OnOffer)
                    {
                        double each = Deal.AsksFor(market, market.Get(tip.id), game.Month, "player");
                        int n = (int)((FrameGame.Silver - Reserve) / (each * 1.1));
                        if (n > 0)
                        {
                            yield return Buy(tip, n);
                            boughtFor[tip.id] = FirstJobs.TipPort;
                            yield return Shot("ravns_word_bought");
                        }
                        else
                            Say($"Ravn's word is {tip.name} at {each:0.#} d and the purse has {FrameGame.Silver:0} d: none bought");
                    }
                    else
                        Say("Ravn named no good that is on offer here: " + FirstJobs.TipGood);
                    var list = SideWindow()?.Q<Button>("todo");
                    if (list != null && ChartPlay.Shown(list))
                    {
                        yield return play.Click(list, "the tab To do");
                        yield return ChartPlay.Settle(0.3f);
                        yield return Shot("the_list_with_a_cargo");
                    }
                }
                else
                {
                    // (cargo aboard that was bought for a port: go there unless the day forbids)
                    plan = Choose(here, reach, daysLeft);
                    if (plan == null)
                        plan = new Plan { dest = Home };                  // (nowhere she could be home from in time)
                    else if (plan.gain <= 0.0)
                    {
                        // nothing worth carrying from here: home if the day is near, else the nearest other market
                        var near = reach.Where(r => r.port.id != here.id).OrderBy(r => r.km).FirstOrDefault();
                        plan = new Plan { dest = daysLeft < 20.0 || near.port == null ? Home : near.port.id };
                    }
                    if (here.id == Home && plan.dest == Home)
                    {
                        // at home with nowhere to go before the day: she waits for it
                        Say($"at home with {daysLeft:0} days to the day and nowhere to go and be back: she waits");
                        yield return Wait(System.Math.Max(0.5, System.Math.Min(daysLeft, 3.0)));
                        continue;
                    }
                    if (plan.buys.Count > 0)
                    {
                        yield return Open("Trade", "Trade");
                        yield return ChartPlay.Settle(0.3f);
                        foreach (var (good, count) in plan.buys)
                        {
                            yield return Buy(good, count);
                            boughtFor[good.id] = plan.dest;
                        }
                        if (calls <= 4 || calls % 5 == 0)
                            yield return Shot("bought_at_" + here.name);
                    }
                    Say($"the plan: {GameData.Port(plan.dest)?.name}, {plan.km:0} km, about {plan.days:0.#} days, hoping for {plan.gain:0} d");
                }
                yield return HandleFloats();
                if (ended)
                    break;
                next = plan.dest;
                Keep();
                yield return SailTo(plan.dest);
            }
        }

        /// <summary>She lies where she is while days of the clock pass (made fast: the men rest).</summary>
        IEnumerator Wait(double days)
        {
            var game = Game;
            double until = game.Day + days;
            yield return Speed(5);
            float t = Time.realtimeSinceStartup;
            while (game.Day < until && Time.realtimeSinceStartup - t < 240f && !ended)
            {
                if (Showtime.Telling)
                    yield return ASceneToldThrough();
                else if (play.FrontFloat() != null)
                    yield return HandleFloat();
                else if (GameSpeed.Level < 4)
                    yield return Speed(5);
                yield return null;
            }
            yield return Speed(1);
        }

        // ------------------------------------------------------------------ the reckoning

        IEnumerator TheReckoning()
        {
            var game = Game;
            yield return HandleFloats();
            Say($"== THE DAY. Purse {FrameGame.Silver:0} d, owed {Trade.Debt:0} d; working for the lender: {Trade.Working}");
            yield return Shot("the_day");
            yield return Open("Errands", "Errands");
            yield return ChartPlay.Settle(0.3f);
            yield return Shot("errands_on_the_day");
            // the lender's own card comes at home on the day: the clock is let run for it
            float t = Time.realtimeSinceStartup;
            double from = game.Day;
            yield return Speed(3);
            while (Time.realtimeSinceStartup - t < 120f && game.Day - from < 3.0 && !ended)
            {
                if (play.FrontFloat() != null)
                {
                    var deck = EncounterRunner.Director;
                    string id = Frame.FloatOpen("card") && deck != null && deck.Current != null ? deck.Current.card.id : "";
                    yield return HandleFloat();
                    yield return ChartPlay.Frames(3);
                    if (id.Contains("wokerers_day") || id.Contains("wokerers_stick"))
                    {
                        yield return HandleFloats();
                        break;
                    }
                }
                else if (GameSpeed.Level < 2)
                    yield return Speed(3);
                if (Trade.Working || (Trade.Debt <= 0.01 && ContentFlags.Ready && ContentFlags.Has("wk_clean")))
                    break;
                yield return null;
            }
            yield return Speed(1);
            yield return HandleFloats();
            yield return ChartPlay.Settle(0.5f);
            string how = Trade.Debt <= 0.01 ? "THE STICK IS CLEAN: the debt was paid" + (Trade.Working ? " (late)" : " in time")
                       : Trade.Working ? $"THE DAY WENT UNPAID: {Trade.Debt:0} d still owed, and she works for the lender"
                       : $"the day passed with {Trade.Debt:0} d owed and the lender did not come";
            Say("== THE RECKONING: " + how + $". The purse: {FrameGame.Silver:0} d.");
            if (Trade.Debt > 0.01 && !Trade.Working)
                Fault("the day passed with the debt unpaid and the lender's card did not come at home");
            endedHow = how;
            yield return Shot("after_the_reckoning");
            yield return Open("Errands", "Errands");
            yield return ChartPlay.Settle(0.3f);
            yield return Shot("errands_after_the_reckoning");
            yield return Open("Journey", "Journey");
            yield return ChartPlay.Settle(0.3f);
            yield return Shot("the_journey_at_the_end");
            Frame.Show("self");
            yield return ChartPlay.Settle(0.4f);
            yield return Shot("yourself_at_the_end");
            yield return EveryWindow("the_end");
        }

        // ------------------------------------------------------------------ the walk

        [UnityTest, Timeout(10800000)]
        public IEnumerator FromANewGameToTheMichaelmasReckoning()
        {
            Directory.CreateDirectory(Folder);
            double mostDays = 0.0;
            double.TryParse(System.Environment.GetEnvironmentVariable("KAPMAN_PT_DAYS") ?? "", out mostDays);
            yield return FreshTitle();
            play = ChartPlay.Begin(true, "playthrough");
            float wall = Time.realtimeSinceStartup;
            try
            {
                yield return TheOpening();
                yield return TheFirstJobs();
                Keep();
                yield return TheSeason(mostDays);
                if (!ended && mostDays <= 0.0)
                    yield return TheReckoning();
                var game = Game;
                Say($"== THE END: {endedHow}. {calls} calls at ports, {legs} passages, {cardsAnswered} cards answered, lost {lost} times, " +
                    $"{pictures} pictures, {play.clicks} clicks and {play.keys} keys, {(Time.realtimeSinceStartup - wall) / 60f:0} minutes on the wall. " +
                    $"The purse began at {silverAtStart:0} d and stood highest at {bestSilver:0} d; it ends at {FrameGame.Silver:0} d with {Trade.Debt:0} d owed.");
                Debug.Log("The play-through:\n" + journal);
                Assert.IsFalse(ended && lost >= 4, endedHow);
            }
            finally
            {
                Keep();
                play.End();
                Showtime.ForceOn = false;
                Showtime.LoadingSeconds = 6.5f;
                Showtime.SlideSeconds = 2.8f;
                FrameRoot.AskedElsewhere = false;
                FrameRoot.FreshLayout = false;
                ClockSettings.AsNew = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
