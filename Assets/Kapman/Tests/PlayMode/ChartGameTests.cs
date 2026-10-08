using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
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
    /// THE CHART GAME, PLAYED AS A PLAYER PLAYS IT (the user, 4 Oct 2026: "the chart is the game"; 7 Oct: it is run from
    /// the command frame). L_Chart is opened in Play and played through the real controls (ChartPlay: a click is a
    /// pointer pressed on the screen where the thing is, a key is a key of the keyboard), with a picture at each step
    /// in Captures/chart. Open the pictures after a run: they are what the player sees.
    ///
    /// 1. SheSailsTheChart: from the title, a new game: who are you (people, faith, past); every workspace read; every
    ///    panel brought up by a window's own list; windows opened, laid out and closed by key and by button; the pause
    ///    menu and its keys; a talk with a person; the waterfront's talk; a cask bought; the measure; a course set in
    ///    the Voyage window; cast off; the look-out and the lead at sea; whatever comes on the way answered; the toll
    ///    man's card at Traiectum, with the clock let run under it; a cask sold; a happening in port.
    /// 2. ThenADaneStartsAtRipa: a new game from the pause menu; a Dane lies at Ripa in her own colours.
    /// 3. ThenNothingHoldsHerAtSea: what used to wait for ever on the chart happens there, and none of it can hold
    ///    her for ever: a happening of the events table, a guard boat (answered by itself when its time is up, and let
    ///    go when its talk is lost), a trader hailed and spoken with, a band of raiders.
    /// (The names put them in this order: each starts the game again or goes on from the last.)
    /// Run: -runTests -testPlatform PlayMode -testFilter ChartGameTests   (with graphics: no -nographics)
    /// </summary>
    public class ChartGameTests
    {
        const string Toll = "harbour.tollman.frankish";

        /// <summary>Whatever a test changed of the machine is put back, also when it failed half way (a test that fails
        /// does not reach its own clean-up).</summary>
        [TearDown]
        public void PutBack()
        {
            GameSpeed.Rates[2] = 1440.0;
            ClockDrops.Quiet = false;
            ClockSettings.AsNew = false;
            KapmanGame.ClockByHand = false;
            ChartPlay.EndActive();
            FrameRoot.FreshLayout = false;
        }

        static IEnumerator Frames(int n) => ChartPlay.Frames(n);
        static IEnumerator Settle(float seconds = 0.35f) => ChartPlay.Settle(seconds);

        static WorldXZ Where(KapmanGame game)
        {
            var w = FloatingOrigin.ToWorld(game.ship.transform.position);
            return new WorldXZ(w.x, w.z);
        }

        /// <summary>The saved games of a test run go in a folder of their own: never among the player's. One folder a
        /// copy of the project: another copy (a worktree with its own Unity) has the same temporary folder, and its
        /// test run emptied this one's saves in the middle of a test.</summary>
        static string TestSaves()
        {
            string folder = Path.Combine(Application.temporaryCachePath, "kapman_test_saves_" + ((uint)Application.dataPath.ToLowerInvariant().GetHashCode()).ToString("x8"));
            Directory.CreateDirectory(folder);
            return folder;
        }

        static IEnumerator OpenChart()
        {
            var load = SceneManager.LoadSceneAsync("L_Chart", LoadSceneMode.Single);
            Assert.IsNotNull(load, "L_Chart is not in the build settings (Kapman > Levels > Build the chart game)");
            while (!load.isDone)
                yield return null;
            yield return Frames(5);
        }

        // ------------------------------------------------------------------ parts of the frame, found as a reader finds them

        /// <summary>A tiled window by the title on it.</summary>
        static VisualElement Window(ChartPlay play, string title)
        {
            VisualElement found = null;
            play.Frame.Root.Query<VisualElement>(className: "frame-win").ForEach(w =>
            {
                var name = w.Q<Label>(className: "frame-win-name");
                if (found == null && name != null && name.text == title && ChartPlay.Shown(w))
                    found = w;
            });
            return found;
        }

        /// <summary>The tiled windows on screen, top to bottom.</summary>
        static List<VisualElement> Windows(ChartPlay play)
        {
            var list = new List<VisualElement>();
            play.Frame.Root.Query<VisualElement>(className: "frame-win").ForEach(w =>
            {
                if (ChartPlay.Shown(w))
                    list.Add(w);
            });
            return list;
        }

        /// <summary>The button with these words on the line of this name in the window of this title (null: not there now).</summary>
        static Button LineButton(ChartPlay play, string window, string line, string words) =>
            play.Line(line, Window(play, window))?.Query<Button>().ToList().FirstOrDefault(b => b.text == words);

        /// <summary>One of the eight function buttons by its name.</summary>
        static Button Verb(ChartPlay play, string name) =>
            play.Buttons(name, play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-verb"));

        /// <summary>A button of the top bar or of a panel (not a function button) by its words.</summary>
        static Button Small(ChartPlay play, string words, VisualElement under = null) =>
            play.Buttons(words, under ?? play.Frame.Root).FirstOrDefault(b => b.ClassListContains("frame-k"));

        static string Title(VisualElement floating) => floating?.Q<Label>(className: "frame-float-title")?.text ?? "";

        /// <summary>
        /// Answers the floating window in front as a player in a hurry would: the choice whose words hold `prefer` if
        /// there is one, else the first that can be chosen; a window with nothing to choose is shut by its cross.
        /// What was answered is noted in `said`.
        /// </summary>
        static IEnumerator Answer(ChartPlay play, List<string> said, string prefer = null)
        {
            var front = play.FrontFloat();
            if (front == null)
                yield break;
            var choices = play.Choices(front);
            Button pick = null;
            string words = "";
            foreach (var c in choices)
                if (pick == null && c.can && prefer != null && c.words.Contains(prefer))
                {
                    pick = c.button;
                    words = c.words;
                }
            foreach (var c in choices)
                if (pick == null && c.can)
                {
                    pick = c.button;
                    words = c.words;
                }
            if (pick == null)
            {
                var cross = front.Q<Button>(className: "frame-float-shut");
                Assert.IsNotNull(cross, $"the window '{Title(front)}' has nothing that can be chosen and no cross: it cannot be answered or shut. It says: {ChartPlay.Short(ChartPlay.Read(front))}");
                said?.Add(Title(front) + ": shut");
                yield return play.Click(cross, "the cross of the window '" + Title(front) + "'");
                yield break;
            }
            said?.Add(Title(front) + ": " + words);
            yield return play.Click(pick, $"the choice '{words}' of the window '{Title(front)}'");
        }

        /// <summary>Answers the window in front unless it is one of those to be left waiting (by its title): a card that
        /// came up over a talk the test is watching.</summary>
        static IEnumerator AnswerOthers(ChartPlay play, List<string> said, params string[] leave)
        {
            var front = play.FrontFloat();
            if (front == null || leave.Contains(Title(front)))
                yield break;
            yield return Answer(play, said);
        }

        /// <summary>Answers every floating window that is up, one after another, until none is left (or `most` were).</summary>
        static IEnumerator AnswerAll(ChartPlay play, List<string> said, int most = 12)
        {
            for (int i = 0; i < most && play.FrontFloat() != null; i++)
            {
                yield return Answer(play, said);
                yield return Frames(3);
            }
        }

        static void NoFaults(ChartPlay play)
        {
            if (play.unwritten.Count > 0)
                Debug.Log($"Chart test: writing not written yet, as it showed on a card or a talk ({play.unwritten.Count}):\n" + string.Join("\n", play.unwritten));
            Debug.Log($"Chart test: {play.clicks} clicks and {play.keys} keys; {play.pressedByKey} buttons had to be pressed the keyboard's way; " +
                      $"{play.pictures.Count} pictures: {string.Join(", ", play.pictures)}");
            // (the text: four sizes in the frame at 1080p and no other, and nothing under 18 true pixels on any screen shown)
            var sizes = play.frameSizes.OrderBy(s => s).ToArray();
            Debug.Log($"Chart test: the frame's text sizes at 1080p: {string.Join(", ", sizes)}; the smallest text seen on any screen was {play.smallestPx:0.#} true px ({play.smallestWhere})");
            CollectionAssert.IsSubsetOf(sizes, new[] { 16, 24 }, "the frame shows a size of text that is not one of Hearth's two (16 and 24): " + string.Join(", ", sizes));
            Assert.AreEqual(0, play.faults.Count, "faults a reader would see on screen:\n" + string.Join("\n", play.faults));
        }

        // =====================================================================================================
        [UnityTest]
        public IEnumerator SheSailsTheChart()
        {
            FrameRoot.FreshLayout = true;                                 // the workspaces a new player has, whatever the settings keep
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;                                      // (the clock is not dropped back under a test in a hurry; close in to Traiectum it is let work)
            ClockSettings.AsNew = true;                                   // (the switches a new player has, whatever this machine keeps)
            SaveGame.FolderOverride = TestSaves();
            foreach (string old in Directory.GetFiles(SaveGame.FolderOverride))
                File.Delete(old);
            yield return OpenChart();
            var play = ChartPlay.Begin(true);
            var said = new List<string>();
            try
            {
                var map = MapView.Current;
                Assert.IsTrue(map.chartMode, "the map is not in chart mode");

                // ---- THE TITLE: New game
                var menu = MainMenu.Current;
                Assert.IsNotNull(menu, "no main menu in L_Chart");
                if (menu.OnTitle)
                {
                    yield return play.Shot("00_title");
                    yield return play.SmallShot("00_title");
                    Assert.IsNull(play.ButtonStarting("2. Continue"), "a test run must not see the player's own saved games");
                    yield return play.ClickButton("1. New game");
                }
                Assert.IsFalse(menu.OnTitle, "New game did not leave the title");
                yield return Frames(120);

                var clock = LevelClock.Current;
                var game = KapmanGame.Current;
                Assert.IsNotNull(clock, "no LevelClock");
                Assert.IsNotNull(game, "no KapmanGame");
                Assert.IsNotNull(game.World, "the world sim was not made");
                Assert.IsNotNull(game.ship, "the game has no ship");
                Assert.IsNotNull(ChartBody.Current, "the ship has no ChartBody");

                // ---- WHO ARE YOU: a new game asks the three choices in a floating window, and the clock waits for them
                var frame = FrameRoot.Current;
                Assert.IsNotNull(frame, "no frame in L_Chart (Kapman > Levels > Build the chart game adds it)");
                Assert.IsFalse(Self.Chosen, "a new game has not chosen who it is yet");
                Assert.IsTrue(frame.AskingSelf, "a new game did not ask who the player is");
                Assert.AreEqual(1, frame.FloatCount, "only the creation window should be up: " + string.Join(", ", frame.FloatIds));
                Assert.IsTrue(PanelClock.Holding, "the creation window should hold the clock");
                Assert.AreEqual(793, clock.Date.year);
                Assert.AreEqual(5, clock.Date.month, "the game begins on 1 May 793");
                Assert.AreEqual(1, clock.Date.day, "the game begins on 1 May 793");
                Assert.IsFalse(Chronicle.SoFar().Any(e => e.id == "793-06-08-lindisfarena-raid"), "on 1 May Lindisfarne has not fallen yet");
                yield return play.Shot("01_who_are_you");
                yield return play.SmallShot("01_who_are_you");
                var book = SelfBook.Current;
                // a people clicked shows the world in its colours at once
                yield return play.ClickButton(book.People("danish").name, play.FrontFloat());
                yield return Settle(1.0f);
                Assert.AreEqual("danish", MapPalette.Current.people, "clicking a people did not change the world's colours");
                Assert.AreEqual("danish", frame.SelfPicks.people);
                yield return play.Shot("02_who_are_you_a_dane");
                yield return play.ClickButton(book.People("frisian").name, play.FrontFloat());
                yield return play.ClickButton(book.Faith("christian").name, play.FrontFloat());
                yield return play.ClickButton(book.Past("steersmans_child").name, play.FrontFloat());
                Assert.AreEqual(("frisian", "christian", "steersmans_child"), frame.SelfPicks, "the three clicks did not pick the three things");
                var begin = play.Choices(play.FrontFloat());
                Assert.AreEqual(1, begin.Count, "the creation window should have one way on");
                StringAssert.StartsWith("Begin", begin[0].words);
                yield return play.Shot("03_who_are_you_picked");
                yield return play.Click(begin[0].button, "Begin");
                yield return Frames(30);
                Assert.IsTrue(Self.Chosen, "Begin did not make the self");
                Assert.IsFalse(frame.AskingSelf, "the creation window did not shut");
                Assert.AreEqual("frisian", Self.People);
                Assert.AreEqual("steersmans_child", Self.Past);
                Assert.AreEqual(2, Self.Stat("nerve"), "the steersman's child has Nerve 2");
                Assert.AreEqual("frisian", MapPalette.Current.people);

                // only a card stops the clock (the ruling of 7 Oct): in port, with the windows up, it runs
                Assert.IsFalse(PanelClock.Holding, "windows must not hold the clock: only a card does");
                double heldAt = clock.GameHours;
                // (the clock moves a whole game minute at a time: at speed 1 that is once a second)
                for (int i = 0; i < 120 && clock.GameHours <= heldAt; i++)
                    yield return null;
                Assert.Greater(clock.GameHours, heldAt, "the clock should run while she lies in port");
                Assert.AreEqual(1f, Time.timeScale, 1e-6f, "the clock is stepped: the time scale stays 1 while it runs");
                Assert.AreEqual(GameData.Ports.Count, Port.All.Count, "the chart did not make a port for every trading place of the tables");
                Assert.IsTrue(map.IsOpen, "the chart is not open");
                Assert.IsFalse(MapView.AwayFromSea, "on the chart the player is never away from the sea");
                Assert.IsNotNull(MapArt.Shared(), "no map art (Bake/world/make_map_art.py writes Kapman2/MapArt/world)");
                var home = GameData.Port("dorestad");
                var (hx, hz) = WorldFrame.ToWorld(home.lat, home.lon);
                var start = Where(game);
                Assert.Less(WorldXZ.Distance(start, new WorldXZ(hx, hz)), 3000.0, "she does not lie at Dorestad");
                Debug.Log($"Chart test: at Dorestad, moored {game.ship.anchored}, ports {Port.All.Count}, world ships {game.World.ships.Count}, {clock.Text}");

                // ---- THE SOUND of the chart: the soundscape is there, fed by where she is; in this run it plays nothing
                var sound = LevelSoundscape2.Current;
                Assert.IsNotNull(sound, "no soundscape in L_Chart (Kapman > Levels > Build the chart game puts it on the listener)");
                Assert.IsNotNull(ChartSound.Current, "no ChartSound in L_Chart");
                Assert.IsNotNull(LevelSoundscape2.Feed, "the chart does not feed the soundscape");
                yield return Frames(5);
                Assert.AreEqual(ChartSound.Ground.Port, ChartSound.Now, "made fast at Dorestat the ear should be in port");
                Assert.AreEqual(0f, ChartSound.Last.shipSpeed, 1e-6f, "made fast she makes no way");
                Assert.Greater(ChartSound.Last.town, 0.2f, "in port the place is round the ear");
                Assert.AreEqual(0f, sound.master, 1e-6f, "a test run must make no noise on the machine");
                Debug.Log($"Chart test: the sound at Dorestat: {LevelSoundscape2.Loudest}; asked level {ChartSound.Asked:0.00}, setting {ChartSound.SettingWord}");

                // ---- THE FRAME: up over the chart, the map's own panels stood down, the Port workspace because she lies in port
                Assert.IsTrue(map.Framed, "the map has not stood its own panels down for the frame");
                Assert.IsTrue(TextPanel.External, "the port's menu is still drawn in the middle");
                Assert.AreEqual(1, TextPanel.Waiters, "one flow of the graphs should be waiting on the port's menu, and no more");
                Assert.AreEqual(4, frame.Windows.spaces.Count);
                Assert.AreEqual(FrameWindows.PortSpace, frame.Windows.Current, "moored at Dorestat the Port workspace should be up");
                CollectionAssert.AreEqual(new[] { "trade", "port", "people" }, frame.Windows.Space.slots, "the Port workspace should show Trade, Port and People here");

                // ---- THE MONEY (rulings 14 and 30 of 7 Oct): the world's traders have been at work since 1 April; the debt
                // is eight pounds and falls due on a day; the four casks are the count's; the log says what is aboard
                Assert.Greater(KapmanGame.PreRunMs, 0, "the world's traders were not at work before the first morning (no pre-run)");
                Assert.Less(System.Math.Abs(game.World.Hours - clock.GameHours), 1.01, "the world's hours are not the clock's after the pre-run");
                Assert.AreEqual(1920.0, Trade.Debt, 0.5, "the debt should be eight pounds");
                Assert.AreEqual("Michaelmas", Trade.DebtDueName);
                Assert.AreEqual(151.0, Trade.DebtDaysLeft(game.Day), 0.01, "from 1 May to Michaelmas is 151 days");
                yield return Settle();
                string bar = ChartPlay.Read(frame.Root.Q<VisualElement>(className: "frame-top"));
                StringAssert.Contains("Owed 1920 d by Michaelmas: 151 days left", bar, "the top bar does not say the debt's day and the days left");
                // (the day as it was kept in 793: 1 May is the Kalends of May)
                StringAssert.Contains("Kalends of May, 793", bar, "the top bar does not say the day as the time reckoned it");
                Assert.AreEqual(4, Trade.Hold.Count("rhenish_wine"));
                Assert.AreEqual(4, Trade.Hold.Owed("rhenish_wine"), "the four casks should be marked as the count's");
                Assert.IsTrue(FrameLog.Lines.Any(l => l.text.StartsWith("She has aboard 4 casks of Rhenish wine that came with the boat and 1 sack of")),
                              "the log does not say what is aboard and whose: " + string.Join(" / ", FrameLog.Lines.Select(l => l.text)));
                Assert.IsTrue(FrameLog.Lines.Any(l => l.text == "You owe the count 1920 d by Michaelmas, 29 September 793."), "the log does not say what is owed and by when");
                Debug.Log($"Chart test: the pre-run took {KapmanGame.PreRunMs} ms; Dorestat asks " +
                          $"{Markets.Of(home, game.Year, game.Day).Asks(Markets.Of(home, game.Year, game.Day).Get("rhenish_wine"), game.Month):0.0} d for a cask of Rhenish wine; " +
                          $"the purse is {FrameGame.Silver:0.0} d of silver and buys {FrameGame.SilverHere(home):0.0} d's worth here");
                yield return play.Shot("04_dorestat_port");
                yield return play.SmallShot("04_dorestat_port");

                // ---- EVERY WORKSPACE, read through: its button in the bar under the windows; in tabs, each tab
                for (int i = 0; i < frame.Windows.spaces.Count; i++)
                {
                    var space = frame.Windows.spaces[i];
                    yield return play.ClickButton((i + 1) + " " + space.name, frame.Root);
                    Assert.AreEqual(i, frame.Windows.Current, "the workspace button did not bring up " + space.name);
                    if (space.layout != FrameLayout.Tabs)
                    {
                        yield return play.Shot($"05_workspace_{i + 1}_{space.name.ToLowerInvariant()}");
                        yield return play.SmallShot($"05_workspace_{i + 1}_{space.name.ToLowerInvariant()}");
                        continue;
                    }
                    foreach (string id in space.slots.ToArray())
                    {
                        var tab = play.Buttons(FramePanels.Title(id), frame.Root).FirstOrDefault(b => b.ClassListContains("frame-tab"));
                        yield return play.Click(tab, "the tab " + FramePanels.Title(id));
                        Assert.AreEqual(id, frame.Windows.Focused, "the tab did not show " + id);
                        yield return play.Shot($"05_workspace_{i + 1}_{space.name.ToLowerInvariant()}_{id}");
                        yield return play.SmallShot($"05_workspace_{i + 1}_{space.name.ToLowerInvariant()}_{id}");
                    }
                }
                // the number keys do the same
                yield return play.Press(Key.Digit1);
                Assert.AreEqual(0, frame.Windows.Current, "[1] did not bring up the first workspace");

                // ---- EVERY PANEL, by a window's own title: its list of panels, then the one picked
                Assert.GreaterOrEqual(FramePanels.All.Count, 19, "the frame's panels are not all there");
                CollectionAssert.AreEqual(new[] { "voyage", "hold" }, frame.Windows.Space.slots, "the Sea workspace should show Voyage and Hold");
                bool listShown = false;
                foreach (var panel in FramePanels.All.ToArray())
                {
                    var second = Windows(play)[1];
                    yield return play.Click(second.Q<Button>(className: "frame-win-title"), "the title of the second window");
                    if (!listShown)
                    {
                        listShown = true;
                        yield return play.Shot("06_list_of_panels");
                        yield return play.SmallShot("06_list_of_panels");
                    }
                    var entry = play.Buttons(panel.Title, frame.Root).FirstOrDefault(b => b.ClassListContains("frame-pick"));
                    yield return play.Click(entry, "the panel '" + panel.Title + "' in the list");
                    Assert.AreEqual(panel.Id, frame.Windows.Space.slots[1], "picking '" + panel.Title + "' did not put it in the window");
                    yield return play.Shot("07_panel_" + panel.Id);
                    yield return Frames(2);
                    var shownWindow = Window(play, panel.Title);
                    Assert.IsNotNull(shownWindow, "no window on screen is titled '" + panel.Title + "'");
                    string reads = ChartPlay.Read(shownWindow.Q<VisualElement>(className: "frame-body"));
                    if (reads.Length <= 10)
                    {
                        // (said in full, to find out why: every piece of text in the window and whether it is on screen)
                        var all = new List<string>();
                        shownWindow.Query<TextElement>().ForEach(l => all.Add($"'{ChartPlay.Short(l.text)}' shown {ChartPlay.Shown(l)} at {l.worldBound}"));
                        Assert.Fail("the window '" + panel.Title + "' is empty. In it: " + string.Join("; ", all));
                    }
                    // (the debt's day and its Sunday, and whose the casks are, where the player reads them)
                    // (the days are counted now: the clock runs while the windows are read, and midnight may have passed)
                    double daysLeft = Trade.DebtDaysLeft(game.Day);
                    if (panel.Id == "tasks")
                    {
                        StringAssert.Contains($"Due at Michaelmas, 29 September 793: {daysLeft:0} days from now.", reads);
                        StringAssert.Contains("Michaelmas is a Sunday in 793.", reads, "the Tasks window does not warn that the debt's day is a Sunday");
                    }
                    else if (panel.Id == "house")
                    {
                        StringAssert.Contains($"The debt falls due at Michaelmas, 29 September 793: {daysLeft:0} days from now.", reads);
                        StringAssert.Contains("have the silver in hand by Saturday, 28 September", reads, "the House window does not warn that the debt's day is a Sunday");
                    }
                    else if (panel.Id == "hold")
                        StringAssert.Contains("Came with the boat. Yours to sell, and owed for already.", reads, "the Hold window does not say where the casks came from");
                }
                // as it was: Voyage above, Hold below (a panel picked that another window shows changes places with it)
                yield return play.Click(Windows(play)[1].Q<Button>(className: "frame-win-title"));
                yield return play.Click(play.Buttons("Hold", frame.Root).FirstOrDefault(b => b.ClassListContains("frame-pick")));
                if (frame.Windows.Space.slots[0] != "voyage")
                {
                    yield return play.Click(Windows(play)[0].Q<Button>(className: "frame-win-title"));
                    yield return play.Click(play.Buttons("Voyage", frame.Root).FirstOrDefault(b => b.ClassListContains("frame-pick")));
                }
                CollectionAssert.AreEqual(new[] { "voyage", "hold" }, frame.Windows.Space.slots, "the Sea workspace was not put back as it was");

                // ---- WINDOWS opened, laid out and closed: by key, then by button
                yield return play.Press(Key.N);
                Assert.AreEqual(3, frame.Windows.Space.slots.Count, "[N] did not open a window");
                yield return play.Press(Key.C);
                Assert.AreEqual(FrameLayout.Columns, frame.Windows.Space.layout, "[C] did not lay them side by side");
                yield return play.Shot("08_windows_columns");
                yield return play.SmallShot("08_windows_columns");
                yield return play.Press(Key.T);
                Assert.AreEqual(FrameLayout.Tabs, frame.Windows.Space.layout, "[T] did not make tabs of them");
                yield return play.Shot("09_windows_tabs");
                yield return play.SmallShot("09_windows_tabs");
                yield return play.Press(Key.R);
                Assert.AreEqual(FrameLayout.Rows, frame.Windows.Space.layout, "[R] did not lay them in rows");
                int focusWas = frame.Windows.Space.focus;
                Assert.AreEqual(2, focusWas, "the new window should have the focus");
                yield return play.Press(Key.DownArrow);
                Assert.AreEqual(0, frame.Windows.Space.focus, "the arrow did not move the focus on (round to the first)");
                yield return play.Press(Key.UpArrow);
                Assert.AreEqual(2, frame.Windows.Space.focus, "the arrow did not move the focus back");
                yield return play.Press(Key.X);
                Assert.AreEqual(2, frame.Windows.Space.slots.Count, "[X] did not close the window in focus");
                yield return play.ClickButton("Columns", frame.Root);
                Assert.AreEqual(FrameLayout.Columns, frame.Windows.Space.layout);
                yield return play.ClickButton("Rows", frame.Root);
                Assert.AreEqual(FrameLayout.Rows, frame.Windows.Space.layout);
                yield return play.Click(play.ButtonByTip("A new window", frame.Root), "the new window button");
                Assert.AreEqual(3, frame.Windows.Space.slots.Count, "the plus did not open a window");
                string third = frame.Windows.Space.slots[2];
                yield return play.Click(play.ButtonByTip("The next panel", Windows(play)[2]), "the next panel arrow");
                Assert.AreNotEqual(third, frame.Windows.Space.slots[2], "the arrow did not step to the next panel");
                yield return play.Click(play.ButtonByTip("The panel before", Windows(play)[2]), "the panel before arrow");
                Assert.AreEqual(third, frame.Windows.Space.slots[2], "the arrow did not step back");
                // (a section of the side has no cross since the look of 7 Oct 2026: the key closes the one in focus)
                Assert.AreEqual(2, frame.Windows.Space.focus, "a click in the third window did not give it the focus");
                yield return play.Press(Key.X);
                CollectionAssert.AreEqual(new[] { "voyage", "hold" }, frame.Windows.Space.slots, "closing the third window did not leave Voyage and Hold");

                // ---- THE PAUSE MENU and its keys: Esc brings it, Esc sends it away, and its numbers are not the frame's
                yield return play.Press(Key.Digit2);
                Assert.AreEqual(FrameWindows.PortSpace, frame.Windows.Current);
                yield return play.Press(Key.Escape);
                Assert.IsTrue(menu.Open, "Esc did not bring the menu");
                yield return play.Shot("10_pause_menu");
                yield return play.SmallShot("10_pause_menu");
                Assert.IsNotNull(play.ButtonStarting("1. Resume"), "the pause menu has no Resume");
                // the setting of the sound: a press takes the next of off, low, middling and loud
                int soundWas = ChartSound.Setting;
                ChartSound.Setting = 2;
                yield return play.Press(Key.Escape);                      // (shut and brought again: the list is drawn with the setting as it now is)
                yield return Frames(3);
                yield return play.Press(Key.Escape);
                Assert.IsTrue(menu.Open, "Esc did not bring the menu again");
                var soundLine = play.Screen.Query<Button>().ToList().FirstOrDefault(b => ChartPlay.Shown(b) && (b.text ?? "").EndsWith(". Sound: middling", System.StringComparison.Ordinal));
                Assert.IsNotNull(soundLine, "the pause menu has no Sound line: " + ChartPlay.Short(ChartPlay.Read(play.Screen)));
                yield return play.Click(soundLine, "Sound in the pause menu");
                Assert.AreEqual("loud", ChartSound.SettingWord, "a press on Sound did not take the next setting");
                yield return play.Click(play.Screen.Query<Button>().ToList().First(b => ChartPlay.Shown(b) && (b.text ?? "").EndsWith(". Sound: loud", System.StringComparison.Ordinal)), "Sound again");
                Assert.AreEqual("off", ChartSound.SettingWord);
                yield return play.Shot("10_pause_menu_sound_off");
                ChartSound.Setting = soundWas;                            // (the player's own setting is left as it was)
                Debug.Log($"Chart test: at the pause menu the saved games are in '{SaveGame.Folder}': " +
                          string.Join(", ", SaveGame.Slots().Select(s => s.slot + " (" + s.when.ToString("HH:mm:ss") + ")")) + "; the menu reads: " +
                          ChartPlay.Short(string.Join(" / ", play.Screen.Query<Button>().ToList().Where(b => ChartPlay.Shown(b) && (b.text ?? "").Length > 2 && char.IsDigit(b.text[0])).Select(b => b.text))));
                yield return play.Press(Key.Escape);
                yield return Frames(3);
                Assert.IsFalse(menu.Open, "Esc did not send the menu away: it came back at once");
                yield return play.Press(Key.Escape);
                Assert.IsTrue(menu.Open);
                yield return play.Press(Key.Digit1);
                yield return Frames(3);
                Assert.IsFalse(menu.Open, "[1] Resume did not shut the menu");
                Assert.AreEqual(FrameWindows.PortSpace, frame.Windows.Current, "the menu's [1] also changed the frame's workspace");
                Assert.IsFalse(GameSpeed.Paused, "the game is still held after the menu was shut");

                // ---- PEOPLE HERE: a talk with one of them; his saying is a verse that comes up in Norse and turns
                CollectionAssert.AreEqual(new[] { "trade", "port", "people" }, frame.Windows.Space.slots);
                var ravn = PlaceBook.Current.Person("dorestad.ravn");
                Assert.IsNotNull(ravn, "the Dorestat file has no Ravn");
                Assert.IsNull(Self.Met(ravn.id));
                yield return ChartPlay.Until(() => play.Line(ravn.name, Window(play, "People here")) != null, 5f);
                var ravnLine = play.Line(ravn.name, Window(play, "People here"));
                Assert.IsNotNull(ravnLine, "Ravn is not in the People here window: " + ChartPlay.Short(ChartPlay.Read(Window(play, "People here"))));
                yield return play.ClickWhen(() => LineButton(play, "People here", ravn.name, "Talk"), "Talk, beside Ravn");
                yield return Settle();
                Assert.IsTrue(frame.FloatOpen(PersonTalk.FloatId(ravn)), "the talk did not open as a floating window");
                Assert.IsFalse(PanelClock.Holding, "a talk with someone of the place must not hold the clock");
                Assert.IsNotNull(Self.Met(ravn.id), "that you have spoken is not kept");
                Assert.Greater(Self.Seen("heill"), 0, "the word he hailed with is not counted as heard");
                yield return play.Shot("11_talk_with_ravn");
                yield return play.SmallShot("11_talk_with_ravn");
                yield return play.ClickButton("Ask for a saying", play.FrontFloat());
                yield return Frames(3);
                var turning = frame.Root.Q<TurningLine>();
                Assert.IsNotNull(turning, "his saying did not come up as a line that turns");
                Assert.AreEqual(0.3, turning.Turn.skill, 1e-6, "a Frisian follows three parts in ten of Norse");
                var turnPlan = turning.Turn.Plan;
                Assert.Greater(turnPlan.Turning, 0, "none of it turns");
                Assert.Greater(turnPlan.Staying, 0, "a tongue half followed should leave its hard pieces");
                string asSaid = turnPlan.ReadAt(0f);
                turning.HoldAt(turnPlan.pieces.Where(p => p.at > 0f).Min(p => p.at) + 0.2f);   // (held for the picture: the first piece has turned)
                yield return Settle();
                Assert.AreNotEqual(asSaid, turning.Reads, "nothing had turned in mid-turn");
                yield return play.Shot("12_line_in_mid_turn");
                turning.HoldAt(-1f);
                yield return Settle(turnPlan.Duration + 0.8f);
                Assert.IsTrue(turning.Done, "the line did not finish turning");
                Assert.Greater(Self.Seen(turnPlan.pieces[0].original), 0, "the pieces heard are not counted");
                yield return play.Shot("13_line_half_followed");
                yield return play.ClickButton("Take your leave", play.FrontFloat());
                yield return Frames(3);
                if (frame.FloatOpen(PersonTalk.FloatId(ravn)))
                {
                    yield return play.Shot("14_farewell");
                    yield return play.ClickButton("Go", play.FrontFloat());
                }
                yield return Frames(3);
                Assert.AreEqual(0, frame.FloatCount, "the talk did not shut: " + string.Join(", ", frame.FloatIds));

                // ---- THE CARDS on the chart: the book is sound, a mooring at every port, and the call at home in progress, quietly
                var cards = EncounterRunner.Director;
                Assert.IsNotNull(cards, "no encounter director: the cards did not load (" + string.Join("; ", EncounterRunner.ShutFiles) + ")");
                Assert.AreEqual(0, EncounterRunner.ShutFiles.Count, "card files shut for faults: " + string.Join("; ", EncounterRunner.ShutFiles));
                Assert.AreEqual(Port.All.Count, cards.watch.points.Count, "the watch has no mooring at every port of the chart");
                Assert.IsTrue(cards.InCall, "a new game: the call at home is not in progress");
                Assert.AreEqual("dorestad", cards.state.callPort);
                Assert.IsNull(cards.Current, "a card opened at home on a new game: nobody comes down to a boat that has not arrived");
                Assert.IsTrue(ChartWorld.TryWeather(out var air), "the chart has no weather where she lies");
                Assert.AreEqual(air.windSpeed, EncounterGameVars.Num("wind", home), 1e-3, "the cards do not read the chart's wind");
                Assert.AreEqual("river_place", EncounterGameVars.Text("level_kind", home), "at Dorestad the cards should read a river");
                Debug.Log($"Chart test: {EncounterRunner.Book.cards.Count} cards; wind {air.windSpeed:0.0} m/s from {air.windFrom:0}, storm {air.storm:0.00}, fog {air.fog:0.00}; on a river {ChartWorld.OnRiver()}");

                // ---- THE PORT'S OWN BUSINESS: the waterfront's talk, from the Port window, as a floating window with a way back
                var text = TextPanel.Current;
                Assert.IsNotNull(text, "no text panel");
                Assert.AreEqual(TextPanel.PortMenuKey, text.Key, "the port's menu is not up at Dorestad");
                string talkWords = text.Options.First(o => o.key == "port.news").label;
                yield return play.ClickWhen(() => play.Button(talkWords, Window(play, "Port")), "the Port window's '" + talkWords + "'");
                yield return Frames(10);
                Assert.IsTrue(frame.FloatOpen("talk"), "the waterfront's talk is not shown as a floating window");
                Assert.IsFalse(PanelClock.Holding, "a talk with a way back must not hold the clock");
                yield return play.Shot("15_waterfront_talk");
                string backWords = text.Options.First(o => o.key == "port.back").label;
                yield return play.ClickButton(backWords, play.FrontFloat());
                yield return Frames(10);
                Assert.AreEqual(0, frame.FloatCount, "the talk did not shut by its way back");
                Assert.AreEqual(TextPanel.PortMenuKey, text.Key, "the port's menu did not come back after the talk");
                Assert.AreEqual(1, TextPanel.Waiters, "more than one flow waits on the port's menu after the talk");

                // ---- TRADE at Dorestat, by the Sell and Buy buttons of the Trade window. She begins with a full hold, so first
                // a cask of the wine goes ashore (and a second cask, to have the silver), then one is bought back
                string wine = GameData.Good("rhenish_wine").name;
                yield return ChartPlay.Until(() => LineButton(play, "Trade", wine, "Sell") != null, 5f);
                var tradeWindow = Window(play, "Trade");
                Assert.IsNotNull(tradeWindow, "no Trade window in the Port workspace");
                var wineRow = play.Line(wine, tradeWindow);
                Assert.IsNotNull(wineRow, "the Trade window at Dorestat does not show the wine she carries: " + ChartPlay.Short(ChartPlay.Read(tradeWindow)));
                double silverBefore = FrameGame.Silver;
                int heldBefore = Trade.Hold.Count("rhenish_wine");
                yield return play.ClickWhen(() => LineButton(play, "Trade", wine, "Sell"), "Sell, beside the wine");
                Assert.AreEqual(heldBefore - 1, Trade.Hold.Count("rhenish_wine"), "one cask should have gone ashore: " + FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                Assert.Greater(FrameGame.Silver, silverBefore, "selling brought no silver");
                // (the critic's B3, 8 Oct 2026: the cask is the player's to sell, so the sale says nothing of "his stake")
                StringAssert.DoesNotContain("stake", FrameLog.Lines[FrameLog.Lines.Count - 1].text, "the sale of a cask that came with the boat still reads as another man's money");
                StringAssert.DoesNotContain("more than", FrameLog.Lines[FrameLog.Lines.Count - 1].text, "the count's cask was told as gain");
                yield return play.Shot("16_sold_at_dorestat");
                yield return play.SmallShot("16_sold_at_dorestat");
                yield return play.ClickWhen(() => LineButton(play, "Trade", wine, "Sell"), "Sell, beside the wine, again");
                double silverMid = FrameGame.Silver;
                yield return play.ClickWhen(() => LineButton(play, "Trade", wine, "Buy"), "Buy, beside the wine");
                Assert.AreEqual(heldBefore - 1, Trade.Hold.Count("rhenish_wine"), "one cask should have come back aboard: " + FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                Assert.Less(FrameGame.Silver, silverMid, "buying cost no silver");
                yield return play.Shot("16_bought_at_dorestat");
                // ---- BY THE LOT (the alpha's row 5; the road's step 4): under the wine's line stand the lots she could sell,
                // each with its count and its sum on it. (Nothing is dealt here: the walk goes on with the hold as it is.)
                int casks = Trade.Hold.Count("rhenish_wine");
                var lotQuote = FrameDeal.Quote(FrameDeal.MarketHere(), "rhenish_wine", false, LotSize.All);
                // ("all 3 for 83 d" beside "Sell 2 for 58 d"; alone it carries its verb)
                string allText = $"all {casks} for {FrameDeal.MarketHere().zone.Show(lotQuote.deniers)}";
                var lots = Window(play, "Trade").Query<Button>().ToList().Where(b => ChartPlay.Shown(b)).Select(b => b.text).ToList();
                if (casks > 1)
                    Assert.IsTrue(lots.Contains(allText) || lots.Contains("Sell " + allText), "no button sells all the wine in one deal (" + allText + "): " + string.Join(" | ", lots));
                Debug.Log("Chart test: the Trade window at Dorestat reads: " + ChartPlay.Read(Window(play, "Trade")).Replace("\n", " / "));
                Assert.AreEqual(1, play.Line(wine, Window(play, "Trade")).Query<Button>().ToList().Count(b => b.text == "Sell"), "one button, and only one, sells a single cask");
                Assert.IsNotNull(Known.Here(GameData.Good("rhenish_wine"), Known.AskNow()).port, "she lies at a market and the word has nothing to be said against");
                Debug.Log("Chart test: the lots at Dorestat: " + string.Join(" | ", lots.Where(t => t.Contains(" for "))));
                Debug.Log($"Chart test: sold two casks and bought one back at Dorestat; silver {silverBefore:0} -> {silverMid:0} -> {FrameGame.Silver:0}; " +
                          $"the log's last line '{FrameLog.Lines[FrameLog.Lines.Count - 1].text}'");

                // ---- AN ORDER IS OFFERED, ASKED AND TAKEN (the alpha's row 9; the road's step 6; Design/fixes/orders.md).
                // Somebody offers her an order by name (a card would; here the test does). The game asks with the terms:
                // what, for where, by what day, at how much a cask, the toll. Taken, it stands in Errands with its day.
                yield return AnswerAll(play, said);
                var orderPort = GameData.Port("traiectum");
                var ordered = Commissions.Offer(orderPort, GameData.Good("rhenish_wine"), 2, 30.0, 0.3, game.Year, game.Month, game.Day);
                Assert.IsNotNull(ordered, "no order could be offered");
                Assert.IsTrue(ordered.offered && !ordered.mine, "an order offered is taken before it is asked");
                yield return ChartPlay.Until(() => frame.FloatOpen(OrderAsk.IdOf(ordered)), 8f);
                Assert.IsTrue(frame.FloatOpen(OrderAsk.IdOf(ordered)), "an order was offered and the game did not ask: take it or refuse it");
                yield return Frames(10);
                Assert.AreEqual("An order at " + orderPort.name, Title(play.FrontFloat()));
                string orderAsk = ChartPlay.Read(play.FrontFloat());
                StringAssert.Contains("2 casks of Rhenish wine for " + orderPort.name + " by ", orderAsk, "the ask does not say what is wanted, where and by what day");
                StringAssert.Contains(" each, ", orderAsk, "the ask does not say the price a cask and the whole");
                StringAssert.Contains("less the toll there", orderAsk, "the ask does not say that the toll comes off");
                StringAssert.Contains("the offer stands 7 days", orderAsk, "the ask does not say how long the offer stands");
                var orderAnswers = play.Choices(play.FrontFloat());
                Assert.AreEqual(2, orderAnswers.Count, "the ask has two answers: take it, or refuse it");
                Assert.IsTrue(orderAnswers[0].can, "she cannot take an order of two casks: " + orderAsk);
                yield return play.Shot("17b_an_order_is_offered");
                yield return play.Click(orderAnswers[0].button, "Take the order");
                yield return Frames(10);
                Assert.IsTrue(ordered.mine, "the order was not taken");
                Assert.IsFalse(frame.FloatOpen(OrderAsk.IdOf(ordered)), "the ask did not shut when it was answered");
                Assert.IsTrue(FrameLog.Lines.Any(l => l.text.StartsWith("You took the order: 2 casks of Rhenish wine for " + orderPort.name)),
                              "the log does not say that the order was taken: " + FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                Debug.Log("Chart test: an order offered and taken: " + ChartPlay.Short(orderAsk));

                // ---- THE VOYAGE WINDOW: the ports she knows with their days; one picked draws its course; the course is set there
                yield return play.ClickButton("1 Sea", frame.Root);
                var voyage = FramePanels.Get("voyage") as VoyagePanel;
                Assert.IsNotNull(voyage, "no Voyage panel");
                float from = Time.realtimeSinceStartup;
                while (!voyage.Reckoned("traiectum") && Time.realtimeSinceStartup - from < 20f)
                    yield return null;
                Assert.IsTrue(voyage.Reckoned("traiectum"), "the Voyage window found no way from Dorestat to Traiectum (the lanes)");
                string traiectum = GameData.Port("traiectum").name;
                // (the window shows "reckoning the way" until it has them all: a player waits for the days to stand there)
                yield return ChartPlay.Until(() => play.Line(traiectum, Window(play, "Voyage"))?.Q<Label>(className: "frame-num") != null, 20f);
                yield return Settle();
                yield return play.ClickWhen(() => play.Button(traiectum, Window(play, "Voyage")), "Traiectum in the Voyage window");
                yield return Settle(1.5f);
                Assert.AreEqual("traiectum", voyage.Picked);
                Assert.Greater(frame.PlannedCount, 1, "picking a port did not draw its course on the chart");
                yield return play.Shot("17_voyage_port_picked");
                yield return play.ClickWhen(() => play.Button("Set this course", Window(play, "Voyage")), "Set this course");
                Assert.IsTrue(Course.IsSet && Course.Destination == "traiectum", "no course from Dorestad to Traiectum");
                Assert.IsTrue(game.ship.anchored, "setting a course in port must not cast her off: that is the Cast off button's");
                double km = Course.Km;
                yield return play.Shot("18_course_set");
                yield return play.SmallShot("18_course_set");
                // the log shows what was just said: its newest line is in sight, not below the edge
                yield return Settle(0.6f);
                var logBox = frame.Root.Q<VisualElement>(className: "frame-log");
                VisualElement newest = null;
                logBox.Query<VisualElement>(className: "frame-log-line").ForEach(l => { if (l.ClassListContains("last")) newest = l; });
                Assert.IsNotNull(newest, "the log has no newest line");
                StringAssert.StartsWith("Course set for", newest.Q<Label>(className: "frame-log-text").text, "the log's newest line is not the course just set");
                Assert.IsTrue(newest.worldBound.yMax <= logBox.worldBound.yMax + 1f && newest.worldBound.yMin >= logBox.worldBound.yMin - 1f,
                              $"the log's newest line is out of sight: it lies at {newest.worldBound.yMin:0} to {newest.worldBound.yMax:0}, the log at {logBox.worldBound.yMin:0} to {logBox.worldBound.yMax:0}");

                // ---- THE MEASURE: its button, then two places clicked on the chart; it says the days by water; Esc puts it down
                // (a card may have come down to the boat meanwhile, as the count's steward does with his tally on a day in
                // port: it lies over the middle of the chart and is answered first, as a player would)
                yield return AnswerAll(play, said);
                yield return play.Click(Verb(play, "Measure"), "the Measure button");
                Assert.IsTrue(frame.Measuring, "the Measure button did not take the measure up");
                // (the marks of two ports she knows, by the names beside them: Dorestat where she lies, and Witla at the
                // river's mouth. The wheel, to see farther, if one of them is under a window.)
                string nameA = GameData.Port("dorestad").name, nameB = GameData.Port("witla").name;
                VisualElement one = null, other = null;
                for (int tries = 0; tries < 8; tries++)
                {
                    one = other = null;
                    play.Screen.Query<VisualElement>(className: "map-port").ForEach(m =>
                    {
                        string named = m.Q<Label>()?.text ?? "";
                        if (named == nameA && play.Reachable(m))
                            one = m;
                        if (named == nameB && play.Reachable(m))
                            other = m;
                    });
                    if (one != null && other != null)
                        break;
                    play.WheelAt(new Vector2(1100f, 520f), 3f);
                    yield return Settle(0.6f);
                }
                Assert.IsNotNull(one, "the mark of " + nameA + " cannot be clicked on the chart");
                Assert.IsNotNull(other, "the mark of " + nameB + " cannot be clicked on the chart");
                var rule = frame.Root.Q<VisualElement>(className: "frame-rule");
                int clicksHeard = play.clickEvents;
                yield return play.Click(one, "the mark of " + nameA + " on the chart");
                string afterOne = ChartPlay.Read(rule);
                bool marksTake = afterOne.Contains("second place");
                if (marksTake)
                    yield return play.Click(other, "the mark of " + nameB + " on the chart");
                else
                {
                    // (the mark did not take the click. Said, with what the screen made of the press, and the two places
                    // are clicked on the open chart beside the marks instead: "click two places on the chart")
                    Debug.Log($"Chart test: a click on a port's mark was not taken by the measure. The screen made {play.clickEvents - clicksHeard} click(s) of the press" +
                              (play.clickEvents > clicksHeard ? ", the last on '" + play.lastClickOn + "'" : "") + ".");
                    foreach (var mark in new[] { one, other })
                    {
                        Vector2 at = Vector2.zero;
                        bool free = false;
                        foreach (var off in new[] { new Vector2(0f, 24f), new Vector2(0f, -24f), new Vector2(-26f, 0f), new Vector2(-26f, 26f), new Vector2(0f, 40f) })
                        {
                            at = mark.worldBound.position + new Vector2(5f, 5f) + off;
                            var hit = play.Panel.Pick(at);
                            free = hit != null && !hit.ClassListContains("map-port") && hit.GetFirstAncestorOfType<Button>() == null;
                            for (var e = hit; e != null && free; e = e.hierarchy.parent)
                                free = e != frame.Root && !e.ClassListContains("map-port") && !e.ClassListContains("map-sim-ship");
                            if (free)
                                break;
                        }
                        Assert.IsTrue(free, "no open chart beside a port's mark to click on");
                        play.ClickAt(at);
                        yield return Frames(3);
                    }
                }
                yield return Settle(1.0f);
                Debug.Log($"Chart test: the measure, {nameA} to {nameB}: {frame.MeasuredKm:0} km by water, '{frame.MeasuredWords}'; by the marks {marksTake}; " +
                          $"the slip said '{ChartPlay.Short(afterOne)}' after the first click and says '{ChartPlay.Short(ChartPlay.Read(rule))}'");
                Assert.Greater(frame.MeasuredKm, 50.0, "the measure found no way by water from " + nameA + " to " + nameB + ": the slip says '" + ChartPlay.Short(ChartPlay.Read(rule)) + "'");
                Assert.Less(frame.MeasuredKm, 200.0, "Dorestat to Witla is about 112 km by the Lek");
                StringAssert.Contains("day", frame.MeasuredWords, "the measure does not say the days");
                yield return play.Shot("19_measure");
                Assert.IsTrue(marksTake, "a click on a port's mark on the chart is not taken (the chart's layer took the pointer, and the click with it)");
                // (the steward may have come down with his tally while the measure was read: Esc is the front window's
                // first, so he is answered first, as a player would)
                yield return AnswerAll(play, said);
                yield return play.Press(Key.Escape);
                Assert.IsFalse(frame.Measuring, "Esc did not put the measure down");
                Assert.IsFalse(menu.Open, "Esc with the measure in hand also opened the menu");
                // with the measure down, a port clicked on the chart is picked in the Voyage window
                yield return play.Click(other, "the mark of " + nameB + " on the chart");
                Assert.AreEqual("witla", voyage.Picked, "a port clicked on the chart was not picked in the Voyage window");
                yield return play.Shot("19_port_clicked_on_the_chart");
                yield return play.Press(Key.F);                           // ([F]: follow her again, if the chart was moved or zoomed)
                yield return Frames(30);

                // ---- CAST OFF, by the function button (the ports' graph unmoors her and lets her go)
                Assert.IsTrue(game.ship.anchored, "a course set in port must wait for the cast off: she is no longer made fast");
                Assert.Less(WorldXZ.Distance(Where(game), start), 300.0, "a course set in port must wait for the cast off: she has left her berth");
                yield return AnswerAll(play, said);
                yield return play.Click(Verb(play, "Cast off"), "the Cast off button");
                yield return Frames(10);
                Assert.IsFalse(game.ship.anchored, "the Cast off button did not unmoor her");
                GameSpeed.Rates[2] = 2400.0;                                // the test's hurry: speed 3 made forty times for this run
                yield return play.Click(Small(play, "3"), "the fastest speed");
                Assert.AreEqual(2, GameSpeed.Level, "the speed button did not set the speed");
                var dest = Course.Points[Course.Points.Count - 1];
                double best = WorldXZ.Distance(start, dest);
                double farthest = 0.0;
                bool arrived = false, seaRead = false;
                int frames = 0, met = 0;
                string metIds = "";
                string lastCard = null;
                for (; frames < 6000 && !arrived; frames++)
                {
                    yield return null;
                    var here = Where(game);
                    best = System.Math.Min(best, WorldXZ.Distance(here, dest));
                    farthest = System.Math.Max(farthest, WorldXZ.Distance(here, start));
                    if (!seaRead && farthest > 5000.0 && Course.IsSet && play.FrontFloat() == null)
                    {
                        seaRead = true;
                        Assert.AreEqual(FrameWindows.SeaSpace, frame.Windows.Current, "under way the Sea workspace should be up");
                        // (the slowest speed while the look-out and the lead are read. A picture waits a third of a real
                        // second, and at the test's forty times that is hours of the game on a machine with nothing else
                        // to do: she was at Traiectum before the lead was cast, and it said no course was set.)
                        yield return play.Click(Small(play, "1"), "the slowest speed, to look about");
                        Assert.AreEqual(0, GameSpeed.Level, "the button of speed 1 did not set the speed");
                        // the sound has gone with her: the river, her way through the water, the oars
                        Assert.AreEqual(ChartSound.Ground.River, ChartSound.Now, "under way on the Rhine the ear should be on the river");
                        Assert.Greater(ChartSound.Last.shipSpeed, 0.05f, "under way she makes way through the water in the ear too");
                        Assert.Greater(LevelSoundscape2.Current.LevelOf("ship_bow"), -79f, "the water past her hull is not heard under way");
                        Debug.Log($"Chart test: the sound on the river: {LevelSoundscape2.Loudest}; speed {ChartSound.Last.shipSpeed:0.0} m/s, rowed {ChartSound.Last.shipRowing:0.0}");
                        yield return play.Shot("20_at_sea");
                        yield return play.SmallShot("20_at_sea");
                        // what is in sight from the masthead: a floating window that holds nothing
                        yield return play.Click(Verb(play, "Look out"), "the Look out button");
                        Assert.IsTrue(frame.FloatOpen("lookout"), "Look out opened no window");
                        Assert.IsFalse(FrameRoot.FloatHoldsClock && play.Floats().Count == 1, "the lookout's window must not hold the clock");
                        yield return play.Shot("21_lookout");
                        yield return play.SmallShot("21_lookout");
                        // (something else may have come up over it meanwhile, a sail to meet: that is answered first, as a
                        // player would; then the lookout's own Go on)
                        for (int i = 0; i < 6 && play.FrontFloat() != null && Title(play.FrontFloat()) != "The lookout"; i++)
                            yield return Answer(play, said);
                        yield return play.ClickButton("Go on", play.Floats().First(f => Title(f) == "The lookout"));
                        // the lead, along her course
                        int logWas = FrameLog.Lines.Count;
                        yield return play.Click(Verb(play, "Sound"), "the Sound button");
                        Assert.Greater(FrameLog.Lines.Count, logWas, "the lead said nothing in the log");
                        StringAssert.StartsWith("The lead", FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                        yield return play.Click(Small(play, "3"), "the fastest speed again");
                        Assert.AreEqual(2, GameSpeed.Level, "the speed button did not set the speed again");
                    }
                    // (ruling 26: made fast, the clock drops back to speed 1 by itself. The passage is the test's hurry,
                    // with no drops; close in they are let work, so it is the game that slows the clock and not a click)
                    if (best < 3000.0)
                        ClockDrops.Quiet = false;
                    // whatever comes on the way is answered as it comes: a card holds the clock until it is. (Not the man
                    // who comes down to the boat as she makes fast: the mooring watch counts in game time now, so at the
                    // test's forty times he is there in the frame she moors. He is the next step's.)
                    var card = cards.Current;
                    bool ashore = best < 3000.0 && card != null && card.card.stage == Kapman.Encounters.Stage.Harbour;
                    if (card != null && !ashore && card.card.id != lastCard)
                    {
                        lastCard = card.card.id;
                        met++;
                        metIds += (metIds.Length > 0 ? ", " : "") + lastCard;
                        if (met == 1)
                            yield return play.Shot("22_card_on_the_way");
                    }
                    if (play.FrontFloat() != null && !ashore)
                        yield return Answer(play, said);
                    arrived = !Course.IsSet && best < 3000.0;
                }
                Assert.IsTrue(seaRead, "she was never far enough out for the look-out and the lead to be tried");
                Assert.LessOrEqual(met, 2, "more than a card or two on a passage of some hours: the pace is not sane (" + metIds + ")");
                Assert.IsFalse(cards.InCall && cards.state.callPort == "dorestad", "the call at Dorestad did not end when she sailed");

                // ---- she has come in: the toll man comes down to the boat (the first arrival of the game)
                string opened = null;
                float waitFrom = Time.realtimeSinceStartup;
                while (opened == null && Time.realtimeSinceStartup - waitFrom < 25f)
                {
                    yield return null;
                    if (cards.Current == null)
                    {
                        if (play.FrontFloat() != null && !frame.FloatOpen("card"))
                            yield return Answer(play, said);               // (a talk or a happening that came as she came in)
                        continue;
                    }
                    if (cards.Current.card.stage != Kapman.Encounters.Stage.Harbour)
                    {
                        // (a card of the crew's or the sea's that came up as she came in, in the evening: answered like
                        // those met on the way; from 1 May she comes in to Traiectum about the hour the crew sings)
                        metIds += (metIds.Length > 0 ? ", " : "") + cards.Current.card.id;
                        met++;
                        yield return Answer(play, said);
                        yield return Frames(3);
                        continue;
                    }
                    opened = cards.Current.card.id;
                }
                // (no click on speed 1 here: a speed chosen with a card open lets the clock run under it. Made fast,
                // the clock has dropped back to speed 1 by itself, and the speed she had is kept for [+])
                GameSpeed.Rates[2] = 1440.0;
                ClockDrops.Quiet = true;
                Assert.AreEqual(0, GameSpeed.Level, "made fast, the clock did not drop back to speed 1 by itself");
                Assert.AreEqual(2, GameSpeed.Before, "the speed she had before she made fast is not kept for [+]");
                Assert.AreEqual(ClockReason.Moored, ClockDrops.Last);
                var lies = game.ship.transform.position;
                var body = ChartBody.Current;
                Debug.Log($"Chart test: course {km:0.0} km; after {frames} frames she is {best / 1000.0:0.0} km from the berth, went {farthest / 1000.0:0.0} km; " +
                          $"arrived {arrived}, moored {game.ship.anchored}; wind {body.Wind:0.0} m/s from {body.WindFrom:0}; {clock.Text}; " +
                          $"patrol {Patrols.Stage}, raid {RaiderEncounter.Stage}, meeting {Encounter.Stage}; status '{ShipMotion.Player.Status}'; " +
                          $"cards met on the way {met} ({metIds}); at Traiectum the card '{opened}'; answered on the way: {string.Join("; ", said)}");
                Debug.Log($"Chart test: the watch at Traiectum: armed {cards.watch.Armed}, under way {cards.watch.UnderWay}, mooring {cards.watch.Within(lies.x, lies.z)}, " +
                          $"call '{cards.state.callPort}', waited {Time.realtimeSinceStartup - waitFrom:0.0} s, lying at '{Port.LyingId()}'");
                Assert.Greater(farthest, 2000.0, "she did not sail: the course was set and she was cast off");
                Assert.IsTrue(arrived, $"she did not come in to Traiectum: {best / 1000.0:0.0} km short");
                Assert.Greater(clock.GameHours - heldAt, 2.0, "the clock did not run while she sailed");
                Assert.AreEqual("traiectum", cards.state.callPort, "the call at Traiectum did not begin when she moored");
                Assert.AreEqual(Toll, opened, "no card opened on her first arrival at Traiectum");
                yield return Frames(20);
                Assert.IsTrue(PanelClock.Holding, "the toll man's card should hold the clock");
                Assert.AreEqual(FrameWindows.PortSpace, frame.Windows.Current, "moored at Traiectum the Port workspace should be up again");

                // ---- the card floats over the chart; Space lets the clock run and the card waits; a talk comes up beside it
                Assert.IsTrue(frame.FloatOpen("card"), "the card is not shown as a floating window");
                Assert.IsTrue(FrameRoot.FloatHoldsClock, "the card should hold the clock");
                var oldCard = Object.FindAnyObjectByType<EncounterPanel>();
                Assert.IsTrue(oldCard == null || !oldCard.enabled, "the old card panel is still drawing under the frame");
                yield return play.Shot("23_toll_man_card");
                yield return play.SmallShot("23_toll_man_card");
                double stopped = clock.GameHours;
                yield return Frames(10);
                Assert.AreEqual(stopped, clock.GameHours, 1e-6, "the clock ran under a card that needs an answer");
                yield return play.Press(Key.Space);
                yield return Frames(10);
                Assert.IsFalse(GameSpeed.PlayerPaused, "going on under a card is not a pause");
                Assert.IsFalse(PanelClock.Holding, "the player went on: the card should let the clock go");
                // (the clock moves a whole game minute at a time: at speed 1 that is once a second)
                for (int i = 0; i < 120 && clock.GameHours <= stopped; i++)
                    yield return null;
                Assert.Greater(clock.GameHours, stopped, "the clock did not run after the player went on");
                Assert.IsTrue(frame.FloatOpen("card") && cards.Current != null, "the card should wait, still open");
                // (rulings 10 and 12 of 7 Oct 2026: left to wait, the card says under itself how long it will, and what
                // it then does by itself. An ordinary card waits a week; the toll man does not hold her)
                Assert.IsFalse(EncounterRunner.ShipHeld, "the toll man's card holds the ship");
                string tollWaits = play.FrontFloat().Q<Label>("hint")?.text ?? "";
                StringAssert.StartsWith("It waits 7 days more. Then: ", tollWaits, "the card left to wait does not say how long it waits and what it then does");
                var tollRow = play.FrontFloat().Q<Label>("hint");
                Assert.GreaterOrEqual(tollRow.worldBound.yMin + 0.5f, play.FrontFloat().Q<VisualElement>("body").worldBound.yMax, "the waiting line lies over the words of the card");
                yield return play.Shot("23b_toll_man_left_to_wait");
                yield return play.Press(Key.Escape);
                Assert.IsTrue(frame.FloatOpen("card"), "Esc shut a card that must be answered");
                Assert.IsFalse(menu.Open, "Esc on a card that must be answered opened the menu");
                talkWords = text.Options.First(o => o.key == "port.news").label;
                yield return play.ClickWhen(() => play.Button(talkWords, Window(play, "Port")), "the Port window's '" + talkWords + "'");
                yield return Frames(20);
                Assert.IsTrue(frame.FloatOpen("talk"), "the waterfront's talk is not shown as a floating window");
                Assert.AreEqual(2, frame.FloatCount, "the card and the talk should both be up");
                yield return play.Shot("24_card_and_talk");
                backWords = text.Options.First(o => o.key == "port.back").label;
                yield return play.ClickButton(backWords, play.FrontFloat());
                yield return Frames(20);
                Assert.IsFalse(frame.FloatOpen("talk"), "the talk did not shut");
                // the card is answered: the choice, then what came of it with one way on
                string declare = cards.Current.options.First(o => o.def.key == "declare").label;
                var declareChoice = play.Choices(play.FrontFloat()).FirstOrDefault(c => c.words.EndsWith(declare, System.StringComparison.Ordinal));
                yield return play.Click(declareChoice.button, "the toll man's choice '" + declare + "'");
                yield return Frames(20);
                Assert.IsTrue(cards.Current != null && cards.Current.Chosen, "the choice was not taken");
                yield return play.Shot("25_card_answered");
                yield return play.ClickButton("Go on", play.FrontFloat());
                yield return Frames(10);
                yield return AnswerAll(play, said);                        // (a visit's card, if one was drawn)
                Assert.AreEqual(0, frame.FloatCount, "floating windows left up: " + string.Join(", ", frame.FloatIds));
                Assert.AreEqual(TextPanel.PortMenuKey, text.Key, "the port's menu did not come back after the talk");

                // ---- THE ORDER IS BROUGHT (the road's step 6). The landing said what is aboard for it. In Errands it is
                // handed over: paid by the cask at the price fixed when it was given, less the toll as on any sale, and
                // kept, so they think the better of her here.
                Assert.IsTrue(ordered.mine && Commissions.All.Contains(ordered), "the order taken at Dorestat is gone before it was brought");
                int wineAboard = Trade.Hold.Count("rhenish_wine");
                Assert.GreaterOrEqual(wineAboard, 2, "she has not two casks aboard for the order");
                Assert.IsTrue(FrameLog.Lines.Any(l => l.text.StartsWith("Aboard for your order here: 2 casks of Rhenish wine")),
                              "the landing does not say what is aboard for her order here; the log: " + string.Join(" / ", FrameLog.Lines.Skip(System.Math.Max(0, FrameLog.Lines.Count - 8)).Select(l => l.text)));
                yield return play.Click(Verb(play, "Errands"), "the Errands button");
                yield return ChartPlay.Until(() => play.Button("Hand over 2", Window(play, "Errands")) != null, 8f);
                Assert.IsNotNull(play.Button("Hand over 2", Window(play, "Errands")), "Errands has no button to hand the order over: " + ChartPlay.Short(ChartPlay.Read(Window(play, "Errands"))));
                string errands = ChartPlay.Read(Window(play, "Errands"));
                StringAssert.Contains("Orders taken", errands);
                StringAssert.Contains($"Brought: 0 of 2. Aboard: {wineAboard}.", errands, "Errands does not say what is brought and what is aboard");
                StringAssert.Contains("This is the place", errands);
                yield return play.Shot("25b_an_order_to_hand_over");
                double orderSilver = FrameGame.Silver, orderStanding = Reputation.At(orderPort);
                double orderToll = TradeRules.Current.ZoneOf(orderPort.region).toll * EncounterRunner.TollFactor(orderPort.id, game.Day);
                yield return play.ClickWhen(() => play.Button("Hand over 2", Window(play, "Errands")), "Hand over 2, in Errands");
                yield return Frames(10);
                Assert.AreEqual(wineAboard - 2, Trade.Hold.Count("rhenish_wine"), "two casks should have gone ashore for the order");
                Assert.IsFalse(Commissions.All.Contains(ordered), "all of it was brought and the order still stands");
                Assert.AreEqual(2 * ordered.each * (1.0 - orderToll), FrameGame.Silver - orderSilver, 1.5, "the order should pay two casks at its price, less the toll of the place");
                Assert.Greater(Reputation.At(orderPort), orderStanding, "the order was kept and nobody here thinks the better of her");
                string orderSaid = FrameLog.Lines.Last(l => l.text.StartsWith("Delivered")).text;
                StringAssert.StartsWith("Delivered 2 casks of Rhenish wine as ordered, for ", orderSaid);
                yield return Frames(10);
                StringAssert.DoesNotContain("Hand over", ChartPlay.Read(Window(play, "Errands")), "the order is kept and Errands still offers to hand it over");
                yield return play.Shot("25c_the_order_is_kept");
                Debug.Log($"Chart test: the order brought at Traiectum: '{orderSaid}'; silver {orderSilver:0} -> {FrameGame.Silver:0}; toll share {orderToll:0.00}");
                // (back to the port's own windows, where the test goes on)
                yield return play.ClickButton("2 Port", frame.Root);
                yield return Frames(10);

                // ---- TRADE at Traiectum: one of what she carries is sold, by its Sell button
                yield return ChartPlay.Until(() => Window(play, "Trade")?.Query<Button>().ToList().Any(b => b.text == "Sell") == true, 5f);
                tradeWindow = Window(play, "Trade");
                double silver = FrameGame.Silver;
                TradeResult sold = null;
                string soldName = "";
                var sellers = new List<(Button button, string name)>();
                tradeWindow.Query<VisualElement>(className: "frame-ln").ForEach(row =>
                {
                    var b = row.Query<Button>().ToList().FirstOrDefault(x => x.text == "Sell");
                    if (b != null)
                        sellers.Add((b, row.Q<Label>(className: "frame-ln-name")?.text ?? ""));
                });
                Assert.Greater(sellers.Count, 0, "she carries nothing that can be sold here: " + ChartPlay.Short(ChartPlay.Read(tradeWindow)));
                foreach (var (_, name) in sellers)                          // (the first that Traiectum will have: a market may refuse a good)
                {
                    var good = GameData.Goods.First(g => g.name == name);
                    int had = Trade.Hold.Count(good.id);
                    string beside = name;
                    yield return play.ClickWhen(() => LineButton(play, "Trade", beside, "Sell"), "Sell, beside " + name);
                    soldName = name;
                    if (Trade.Hold.Count(good.id) == had - 1)
                    {
                        sold = new TradeResult { ok = true, text = FrameLog.Lines[FrameLog.Lines.Count - 1].text };
                        break;
                    }
                }
                Assert.IsNotNull(sold, "nothing she carries sold at Traiectum; the log's last line: " + FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                Assert.Greater(FrameGame.Silver, silver, "selling brought no silver");
                yield return play.Shot("26_sold_at_traiectum");
                // (the toll of the merchant's sales is reckoned by its graph, "Rule - the toll on a sale", not by the code's sum)
                Assert.Greater(GameRules.ByGraph, 0, "the merchant has sold and the toll's graph was never asked (Resources/GameRules; Kapman.Graphs.RuleGraphs)");
                Assert.AreEqual(0, GameRules.ByCode, "a rule the graphs hold was reckoned by the code's sum in play: its graph is missing or failed (see the Console)");
                Debug.Log($"Chart test: sold one {soldName} at Traiectum: '{sold.text}'; silver {silver:0} -> {FrameGame.Silver:0}");

                // ---- A HAPPENING IN PORT (the events table): it comes as a floating window of its own, over the port's menu,
                // holds the clock until it is answered, tells what came of it, and leaves the port's business as it was
                var portEvent = GameEvents.All.FirstOrDefault(e => e.where == "port");
                Assert.IsNotNull(portEvent, "the events table has no happening for a port");
                Assert.IsTrue(GameEvents.Force(portEvent.id));
                yield return Frames(10);
                Assert.IsTrue(frame.EventOpen, "the happening '" + portEvent.id + "' did not come up on the chart");
                Assert.IsTrue(PanelClock.Holding, "a happening should hold the clock until it is answered");
                Assert.AreEqual(TextPanel.PortMenuKey, text.Key, "the happening took the port's menu away");
                yield return play.Shot("27_happening_in_port");
                yield return Answer(play, said);
                yield return Frames(5);
                Assert.IsNull(GameEvents.Current, "the choice did not end the happening");
                Assert.IsTrue(frame.EventOpen, "what came of the happening was not told");
                Assert.IsFalse(PanelClock.Holding, "what came of it should not hold the clock");
                yield return play.Shot("28_happening_answered");
                yield return play.ClickButton("Go on", play.FrontFloat());
                yield return Frames(5);
                Assert.AreEqual(0, frame.FloatCount, "floating windows left up: " + string.Join(", ", frame.FloatIds));
                Assert.AreEqual(1, TextPanel.Waiters, "the port's menu should still wait for its one choice");
                Assert.Greater(FrameLog.Lines.Count, 8, "the log has not been written in");
                yield return play.Shot("29_traiectum_at_the_end");

                // ---- A LOT IS DEALT (the alpha's row 5; the road's step 4): one button with its count and its sum on it,
                // one deal, one settlement, said once in the log.
                var lotButtons = Window(play, "Trade")?.Query<Button>().ToList().Where(b => ChartPlay.Shown(b)).ToList() ?? new List<Button>();
                var lotButton = lotButtons.FirstOrDefault(b => b.name == "buy-all" || b.name == "sell-all");
                Assert.IsNotNull(lotButton, "the Trade window at Traiectum has no lot to deal: " + string.Join(" | ", lotButtons.Select(b => b.text)));
                string lotWords = lotButton.text;                          // ("all 6 for 31 d", or "Buy all 6 for 31 d" where it stands alone)
                bool lotBuys = lotButton.name == "buy-all";
                var lotParts = lotWords.Split(' ').ToList();
                int lotCount = int.Parse(lotParts[lotParts.IndexOf("all") + 1]);
                Assert.Greater(lotCount, 1, "a lot of one is no lot: " + lotWords);
                // (its good: the lots stand in the row under the good's own line)
                var lotsRow = lotButton.parent;
                int lotAt = lotsRow.parent.IndexOf(lotsRow) - 1;
                while (lotAt > 0 && !lotsRow.parent[lotAt].ClassListContains("frame-ln"))
                    lotAt--;                                               // (the row of selling lots stands between)
                var lotLine = lotsRow.parent[lotAt];
                string lotGoodName = lotLine.Q<Label>(className: "frame-ln-name")?.text ?? "";
                var lotGood = GameData.Goods.FirstOrDefault(g => g.name == lotGoodName);
                Assert.IsNotNull(lotGood, "the lot '" + lotWords + "' stands under no good's line (found '" + lotGoodName + "')");
                int lotHad = Trade.Hold.Count(lotGood.id), lotLog = FrameLog.Lines.Count;
                double lotSilver = FrameGame.Silver;
                yield return play.Click(lotButton, "the lot '" + lotWords + "' under " + lotGoodName);
                yield return Frames(10);
                Assert.AreEqual(lotHad + (lotBuys ? lotCount : -lotCount), Trade.Hold.Count(lotGood.id),
                                "the lot '" + lotWords + "' did not deal its count; the log: " + FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                Assert.AreEqual(1, FrameLog.Lines.Skip(lotLog).Count(l => l.text.StartsWith(lotBuys ? "Bought" : "Sold")), "a lot should be one deal, said once");
                Assert.IsTrue(lotBuys ? FrameGame.Silver < lotSilver : FrameGame.Silver > lotSilver, "the lot moved no silver");
                yield return Frames(10);
                yield return play.Shot("30_a_lot_dealt_at_traiectum");
                Debug.Log($"Chart test: a lot dealt at Traiectum: '{lotWords}' under {lotGoodName}; '{FrameLog.Lines[FrameLog.Lines.Count - 1].text}'; silver {lotSilver:0} -> {FrameGame.Silver:0}");
                NoFaults(play);
            }
            finally
            {
                GameSpeed.Rates[2] = 1440.0;
                GameSpeed.SetLevel(0);
                play.End();
                FrameRoot.FreshLayout = false;
            }
        }

        // =====================================================================================================
        /// <summary>
        /// A NEW GAME FROM THE PAUSE MENU, AS A DANE (the user, 7 Oct 2026: "depending on your culture you start somewhere
        /// else and the world is a different colour"): the game before is cleared, the creation window asks again, and a
        /// Dane lies at Ripa in the Amber colours with Ripa's own menu up and nobody come down to the boat.
        /// </summary>
        [UnityTest]
        public IEnumerator ThenADaneStartsAtRipa()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;
            SaveGame.FolderOverride = TestSaves();
            if (SceneManager.GetActiveScene().name != "L_Chart")
                yield return OpenChart();
            var menu = MainMenu.Current;
            Assert.IsNotNull(menu, "no main menu in L_Chart");
            var play = ChartPlay.Begin(false);
            float from;
            try
            {
                if (menu.OnTitle)
                    yield return play.ClickButton("1. New game");          // (run alone: from the title; after the voyage: from the game)
                yield return AnswerAllIfBegun(play);
                FrameRoot.Current?.SetMeasuring(false);                    // (whatever the test before left in hand: Esc would put it down first)
                // the pause menu: New game, and Yes to "what isn't saved is lost"
                yield return play.Press(Key.Escape);
                Assert.IsTrue(menu.Open, "Esc did not bring the menu");
                var newGame = play.Screen.Query<Button>().ToList().FirstOrDefault(b => ChartPlay.Shown(b) && (b.text ?? "").EndsWith(". New game", System.StringComparison.Ordinal));
                yield return play.Click(newGame, "New game in the pause menu");
                yield return play.Shot("30_new_game_asks");
                yield return play.ClickButton("1. Yes");                   // (clears the game's state and starts the scene again)
                FrameRoot.FreshLayout = true;
                FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
                SaveGame.FolderOverride = TestSaves();                     // (a new game forgets the folder a test named: named again)
                from = Time.realtimeSinceStartup;
                while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                    yield return null;
            }
            finally
            {
                play.End();
            }
            var frame = FrameRoot.Current;
            Assert.IsNotNull(frame, "no frame after a new game");
            Assert.IsTrue(frame.AskingSelf, "a new game from the menu did not ask who the player is");
            Assert.IsFalse(Self.Chosen);
            Assert.IsFalse(FrameLog.Lines.Any(l => l.text.StartsWith("You begin", System.StringComparison.Ordinal) || l.text.StartsWith("Course set", System.StringComparison.Ordinal)),
                           "the log of the game before was carried into the new one: " + string.Join(" / ", FrameLog.Lines.Select(l => l.text)));
            play = ChartPlay.Begin(false);                                // (the scene is new: its screen is taken again)
            try
            {
                yield return Frames(120);
                var book = SelfBook.Current;
                yield return play.ClickButton(book.People("danish").name, play.FrontFloat());
                yield return play.ClickButton(book.Faith("old_gods").name, play.FrontFloat());
                yield return play.ClickButton(book.Past("hall_child").name, play.FrontFloat());
                Assert.AreEqual(("danish", "old_gods", "hall_child"), frame.SelfPicks);
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                from = Time.realtimeSinceStartup;
                while ((FrameGame.PortHere == null || FrameGame.PortHere.id != "ripa") && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                var game = KapmanGame.Current;
                var ripa = GameData.Port("ripa");
                var (rx, rz) = WorldFrame.ToWorld(ripa.lat, ripa.lon);
                Assert.Less(WorldXZ.Distance(Where(game), new WorldXZ(rx, rz)), 3000.0, "a Dane does not lie at Ripa");
                Assert.AreEqual("ripa", Port.LyingId(), "she is not made fast at Ripa");
                Assert.AreEqual("ripa", ChartWorld.Current.homePort);
                Assert.IsNotNull(FrameGame.PortHere, "Ripa's own business is not open");
                Assert.AreEqual("ripa", FrameGame.PortHere.id);
                Assert.AreEqual(TextPanel.PortMenuKey, TextPanel.Current.Key, "Ripa's menu is not up");
                Assert.AreEqual(1, TextPanel.Waiters, "Dorestat's menu is still waiting behind Ripa's: a choice would be taken twice");
                Assert.AreEqual("danish", Self.People);
                Assert.AreEqual("danish", MapPalette.Current.people, "the world is not in the Danes' colours");
                Assert.AreEqual(3, Self.Stat("name"), "a younger child of a hall has Name 3");
                Assert.AreEqual(1.0, Self.TongueHeld("old_norse"), 1e-9);
                var cards = EncounterRunner.Director;
                Assert.IsNotNull(cards);
                Assert.AreEqual("ripa", cards.state.callPort, "the call at Ripa is not in progress");
                yield return Settle(3f);
                Assert.IsNull(cards.Current, "a card opened at home on a new game: nobody comes down to a boat that has not arrived");
                Assert.AreEqual(0, frame.FloatCount, "floating windows up at the start: " + string.Join(", ", frame.FloatIds));
                Assert.IsFalse(PanelClock.Holding, "the clock should run once she has begun");
                yield return play.Shot("31_a_dane_at_ripa");
                yield return play.SmallShot("31_a_dane_at_ripa");

                // ---- from Ripa every port of the first release can be reached now (the opened rivers)
                var line = new List<WorldXZ>();
                foreach (string id in new[] { "dorestad", "traiectum", "walichrum", "lundenwic", "witla", "medemelacha", "doccinga", "daventre", "quentovic", "sarre", "fordwic" })
                    Assert.IsTrue(Course.Plan(id, ChartWorld.ShipWorld(), "ripa", line, out _), "no course from Ripa to " + id);

                // ---- a line in a tongue she knows: the host of Ripa greets a Dane with a verse on guests, and it turns
                // wholly, in well under two seconds
                var host = PlaceBook.Current.Person("ripa.hroald");
                Assert.IsNotNull(host, "the Ripa file has no Hroald");
                yield return ChartPlay.Until(() => play.Line(host.name, Window(play, "People here")) != null, 5f);
                var hostLine = play.Line(host.name, Window(play, "People here"));
                Assert.IsNotNull(hostLine, "Hroald is not in the People here window");
                yield return play.ClickWhen(() => LineButton(play, "People here", host.name, "Talk"), "Talk, beside Hroald");
                float opened = Time.realtimeSinceStartup;
                yield return null;
                var turning = frame.Root.Q<TurningLine>();
                Assert.IsNotNull(turning, "the host of Ripa has no line that turns at a first greeting");
                var plan = turning.Turn.Plan;
                Assert.AreEqual(1.0, turning.Turn.skill, 1e-6, "a Dane has Norse");
                Assert.AreEqual(0, plan.Staying, "a tongue she knows turns wholly");
                while (!turning.Done && Time.realtimeSinceStartup - opened < 4f)
                    yield return null;
                Assert.IsTrue(turning.Done, "the line did not turn");
                Assert.Less(Time.realtimeSinceStartup - opened, 2.5f, "a tongue you know should turn in well under two seconds");
                turning.HoldAt(plan.pieces[plan.pieces.Length / 2].at - 0.03f);   // (held for the picture: the middle piece going out)
                yield return play.Shot("32_line_in_mid_turn_known_tongue");
                turning.HoldAt(-1f);
                yield return play.Shot("33_line_turned");
                var cross = play.FrontFloat().Q<Button>(className: "frame-float-shut");
                yield return play.Click(cross, "the cross of the talk");
                Assert.AreEqual(0, frame.FloatCount, "the cross did not shut the talk");
                NoFaults(play);
            }
            finally
            {
                play.End();
                FrameRoot.FreshLayout = false;
            }
        }

        /// <summary>A game that has not begun yet is begun by its creation window as it stands (a test run alone).</summary>
        static IEnumerator AnswerAllIfBegun(ChartPlay play)
        {
            yield return Frames(60);
            var frame = FrameRoot.Current;
            if (frame != null && frame.AskingSelf)
            {
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                yield return Frames(60);
            }
            yield return AnswerAll(play, null);
        }

        // =====================================================================================================
        /// <summary>
        /// WHAT USED TO WAIT FOR EVER NOW HAPPENS ON THE CHART, AND NONE OF IT HOLDS HER FOR EVER (7 Oct 2026,
        /// Design/fix_sea_meetings.md). She sails from Dorestat for Lundenwic. On the way: a happening of the events
        /// table; a guard boat that signals her (she is held; with the clock let run and no answer given, she heaves to
        /// by herself when the king's men's time is up, and sails on); a guard boat whose talk is lost (let go in
        /// seconds); a trader hailed, closed with, spoken to and parted from; a band of raiders that comes on.
        /// The guard boat and the band are brought on by the test (in May 793 no guard boat sails, and a band may be a
        /// sea away): everything after that is the game's own.
        /// </summary>
        [UnityTest]
        public IEnumerator ThenNothingHoldsHerAtSea()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;
            SaveGame.FolderOverride = TestSaves();
            if (SceneManager.GetActiveScene().name != "L_Chart")
                yield return OpenChart();
            Assert.IsNotNull(MainMenu.Current, "no main menu in L_Chart");
            MainMenu.Current.NewGame();                                   // (a game of its own: the state cleared, the scene again)
            SaveGame.FolderOverride = TestSaves();                        // (a new game forgets the folder a test named: named again)
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;
            float from = Time.realtimeSinceStartup;
            while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                yield return null;
            var frame = FrameRoot.Current;
            Assert.IsNotNull(frame, "no frame after a new game");
            var play = ChartPlay.Begin(false);
            var said = new List<string>();
            try
            {
                yield return Frames(120);
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");   // (the first of each: a Frisian of Dorestat)
                from = Time.realtimeSinceStartup;
                while (FrameGame.PortHere == null && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                var game = KapmanGame.Current;
                var clock = LevelClock.Current;
                var text = TextPanel.Current;
                Assert.AreEqual("dorestad", FrameGame.PortHere?.id, "a Frisian does not lie at Dorestat");
                // a long way to go, so she is at sea for all of it: Lundenwic, by the Lek and the Maas mouth
                Assert.IsTrue(Course.SetTo("lundenwic", ChartWorld.ShipWorld(), Port.LyingId()), "no course from Dorestat to Lundenwic");
                Assert.AreEqual(415.0, Course.Km, 30.0, "the course to Lundenwic should run by the Lek and the Maas mouth, about 415 km");
                yield return Frames(5);
                yield return play.Click(Verb(play, "Cast off"), "the Cast off button");
                yield return Frames(10);
                Assert.IsFalse(game.ship.anchored, "she did not cast off");
                GameSpeed.Rates[2] = 2400.0;
                yield return play.Click(Small(play, "3"), "the fastest speed");
                // out of the port's reach, and on a little
                var start = Where(game);
                for (int i = 0; i < 3000 && WorldXZ.Distance(Where(game), start) < 12000.0; i++)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play, said);
                }
                Assert.Greater(WorldXZ.Distance(Where(game), start), 11000.0, "she did not get clear of Dorestat");
                yield return AnswerAll(play, said);
                Debug.Log($"Chart test (sea): clear of Dorestat at {clock.Text}; answered on the way out: {string.Join("; ", said)}");

                // ---- 1. A HAPPENING AT SEA: it comes as a window, holds the clock, is answered, and she sails on
                Assert.IsTrue(GameEvents.Force("catch"), "the events table has no 'catch'");
                yield return Frames(10);
                Assert.IsTrue(frame.EventOpen, "a happening at sea did not come up on the chart");
                Assert.IsTrue(PanelClock.Holding, "a happening should hold the clock");
                double stood = clock.GameHours;
                yield return Frames(20);
                Assert.AreEqual(stood, clock.GameHours, 1e-6, "the clock ran under a happening that waits for its answer");
                yield return play.Shot("40_happening_at_sea");
                yield return Answer(play, said);
                yield return Frames(5);
                Assert.IsNull(GameEvents.Current);
                yield return play.Shot("41_happening_at_sea_answered");
                // (what came of it: Go on. A picture is hours of the game at the test's forty times, and the crew's
                // evening song may have come up over it meanwhile: both are answered, as a player would)
                yield return AnswerAll(play, said);
                yield return Frames(5);
                Assert.AreEqual(0, frame.FloatCount, "windows left up after the happening: " + string.Join(", ", frame.FloatIds));
                var before = Where(game);
                yield return Frames(60);
                Assert.Greater(WorldXZ.Distance(Where(game), before), 200.0, "she did not sail on after the happening");

                // ---- 1b. A CARD LEFT UNANSWERED (rulings 10 and 12 of 7 Oct 2026; the road's step 5). A squall line is a
                // pressing card: it holds the clock; with the clock let run it holds HER, says under itself how long it
                // will wait and what it then does, and when its hour is up it takes that option by itself, the log says
                // so, and she sails on. (Put in line by the test: a squall wants the weather for it.)
                const string Squall = "sea.squall.line";
                var deck = EncounterRunner.Director;
                Assert.IsNotNull(deck.rules, "the cards have no rules");
                Assert.IsTrue(deck.rules.For(Squall).holdsShip, "the squall's card is not one that holds the ship");
                Assert.IsFalse(EncounterRunner.ShipHeld, "she is held with no card up");
                deck.state.Enqueue(Squall, true);
                yield return ChartPlay.Until(() => deck.Current != null && deck.Current.card.id == Squall && frame.FloatOpen("card"), 12f);
                Assert.AreEqual(Squall, deck.Current?.card.id, "the squall's card did not open");
                yield return Frames(10);
                Assert.IsTrue(PanelClock.Holding, "the squall's card should hold the clock");
                Assert.IsTrue(EncounterRunner.ShipHeld, "a pressing card should hold her where she is");
                string squallTitle = Title(play.FrontFloat());
                for (int i = 0; i < 30 && frame.FrontReveal != null && !frame.FrontReveal.Done; i++)
                {
                    yield return play.Press(Key.Enter);                    // (the lines are said through)
                    yield return Frames(4);
                }
                Assert.AreEqual("", play.FrontFloat().Q<Label>("hint")?.text ?? "", "a card says how long it waits while the clock is held: no time passes then");
                var squallPlace = Where(game);
                double squallFrom = clock.GameHours;
                int squallLog = FrameLog.Lines.Count;
                // (speed 1 first: a speed chosen with a card up lets the clock run under it, and a picture takes real
                // seconds, which at the test's fast clock would be the card's whole hour)
                yield return play.Click(Small(play, "1"), "speed 1, with the squall's card up");
                yield return Frames(5);
                Assert.IsFalse(PanelClock.Holding, "a speed chosen did not let the clock run under the squall's card");
                string squallWaits = play.FrontFloat().Q<Label>("hint")?.text ?? "";
                StringAssert.StartsWith("She is held here. It waits 1 hour more. Then: ", squallWaits, "the pressing card left to wait does not say that she is held, how long, and what then");
                var squallRow = play.FrontFloat().Q<Label>("hint");
                Assert.GreaterOrEqual(squallRow.worldBound.yMin + 0.5f, play.FrontFloat().Q<VisualElement>("body").worldBound.yMax, "the waiting line lies over the words of the card");
                yield return play.Shot("41b_a_squall_left_to_wait");
                yield return play.Click(Small(play, "3"), "speed 3 again");
                double driftedUnderIt = 0.0;
                for (int i = 0; i < 6000 && deck.Current != null && deck.Current.card.id == Squall; i++)
                {
                    yield return null;
                    driftedUnderIt = System.Math.Max(driftedUnderIt, WorldXZ.Distance(Where(game), squallPlace));
                    yield return AnswerOthers(play, said, squallTitle);    // (a happening that came meanwhile would hold the clock)
                }
                double squallWaited = clock.GameHours - squallFrom;
                Assert.IsFalse(deck.Current != null && deck.Current.card.id == Squall, $"the squall's card is still up after {squallWaited:0.0} game hours of running clock");
                Assert.Less(driftedUnderIt, 50.0, "she sailed on under a pressing card: she should make no way until it is answered");
                Assert.AreEqual(1.0, squallWaited, 0.1, "the squall's card should answer itself when its hour is up");
                Assert.IsFalse(EncounterRunner.ShipHeld, "she is still held after the card answered itself");
                Assert.IsTrue(FrameLog.Lines.Skip(squallLog).Any(l => l.text.Contains("no answer, so it went this way: ")),
                              "the log does not say that the squall's card answered itself: " + string.Join(" / ", FrameLog.Lines.Skip(squallLog).Select(l => l.text)));
                Debug.Log("Chart test (sea): the squall's card said: " + squallWaits + " | the log: " + string.Join(" / ", FrameLog.Lines.Skip(squallLog).Select(l => l.text)));
                yield return Frames(5);
                yield return AnswerAll(play, said);
                yield return play.Shot("41c_the_squall_answered_itself");
                before = Where(game);
                yield return Frames(60);
                Assert.Greater(WorldXZ.Distance(Where(game), before), 200.0, "she did not sail on after the squall's card answered itself");

                // ---- 1c. THE OARS (the alpha's row 3; the road's step 2). The order stands beside her course in the
                // Voyage window. Out, the sail is down and she makes her speed under oars whatever the wind, and the
                // window says what the men have left in them; shipped, the order is there to give again.
                var herClass = FramePassage.HerClass;
                Assert.IsNotNull(herClass, "her class is not known");
                Assert.IsFalse(Oars.Out, "the oars are out before the order");
                Assert.AreEqual(herClass.rowingHours, Oars.Left(herClass), 1e-6, "the men are not fresh");
                yield return play.Click(Small(play, "1"), "speed 1, to row a few minutes");
                yield return play.ClickWhen(() => play.Button("Out oars", Window(play, "Voyage")), "the Voyage window's 'Out oars'");
                yield return Frames(10);
                Assert.IsTrue(Oars.Out, "the oars did not come out");
                Assert.IsTrue(FrameLog.Lines.Any(l => l.text == "The sail comes down and the men take to the oars."), "the log does not say that the oars are out");
                var hull = ChartBody.Current;
                double rowedFrom = clock.GameHours, lowKnots = 0.0, highKnots = herClass.oarsKn + 0.05;
                bool rowsAsSheShould = false;
                for (int i = 0; i < 2400 && !rowsAsSheShould; i++)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play, said);                   // (a card that came meanwhile would hold the clock)
                    // (dead into the wind she loses most: between that and her full speed under oars, once she has her way)
                    lowKnots = herClass.oarsKn * (1.0 - 0.5 * Mathf.Min(1f, hull.Wind / 17f)) - 0.15;
                    rowsAsSheShould = clock.GameHours - rowedFrom > 3.0 / 60.0 && hull.Knots >= lowKnots && hull.Knots <= highKnots;
                }
                Assert.IsTrue(rowsAsSheShould, $"under oars she should make between {lowKnots:0.0} and {highKnots:0.0} knots: she makes {hull.Knots:0.0} (wind {hull.Wind:0.0} m/s from {hull.WindFrom:0})");
                Assert.Less(Oars.Left(herClass), herClass.rowingHours, "rowing takes nothing out of the men");
                yield return Frames(20);
                string voyageNow = ChartPlay.Read(Window(play, "Voyage"));
                StringAssert.Contains("Under oars", voyageNow, "the Voyage window does not say that she is under oars");
                StringAssert.Contains("left in them", voyageNow, "the Voyage window does not say what the men have left in them");
                Assert.IsNotNull(play.Button("Ship oars", Window(play, "Voyage")), "there is no way to ship the oars again");
                Debug.Log($"Chart test (sea): under oars she makes {hull.Knots:0.0} knots in {hull.Wind:0.0} m/s from {hull.WindFrom:0}; the Voyage window: {ChartPlay.Short(voyageNow)}");
                yield return play.Shot("41d_under_oars");
                yield return play.ClickWhen(() => play.Button("Ship oars", Window(play, "Voyage")), "the Voyage window's 'Ship oars'");
                yield return Frames(10);
                Assert.IsFalse(Oars.Out, "the oars did not come in");
                Assert.IsTrue(FrameLog.Lines.Any(l => l.text == "The oars come in and the sail goes up."), "the log does not say that the oars are in");
                Assert.IsNotNull(play.Button("Out oars", Window(play, "Voyage")), "the order is gone from the Voyage window");
                yield return play.Click(Small(play, "3"), "speed 3 again");
                yield return Frames(5);

                // ---- 2. A GUARD BOAT: she is held; the clock is let run and no answer is given; when the king's men's
                // time is up she heaves to by herself, the search is told, and she sails on
                Assert.AreEqual(PatrolStage.None, Patrols.Stage);
                Assert.IsTrue(Patrols.Signal(GameData.Port("dorestad"), 1.0), "the guard boat could not signal");
                yield return Frames(10);
                Assert.AreEqual(PatrolStage.Signal, Patrols.Stage);
                Assert.IsTrue(frame.FloatOpen("talk"), "the guard boat's signal did not come up as a window on the chart");
                Assert.AreEqual("The king's men", Title(play.FrontFloat()));
                Assert.AreEqual(3, play.Choices(play.FrontFloat()).Count, "the king's men at sea: heave to, dump, run");
                Assert.IsTrue(PanelClock.Holding, "the king's men should hold the clock until the player answers or goes on");
                Assert.IsNull(play.FrontFloat().Q<Button>(className: "frame-float-shut"), "the king's men's window can be shut without an answer");
                yield return play.Shot("42_guard_boat");
                yield return play.Press(Key.Space);                        // (the clock is let run: the window waits)
                yield return Frames(5);
                Assert.IsFalse(PanelClock.Holding, "Space did not let the clock run under the king's men");
                var heldPlace = Where(game);
                double signalled = clock.GameHours;
                int logAt = FrameLog.Lines.Count;
                bool movedWhileHeld = false;
                for (int i = 0; i < 4000 && Patrols.Stage != PatrolStage.None; i++)
                {
                    yield return null;
                    movedWhileHeld |= WorldXZ.Distance(Where(game), heldPlace) > 400.0;
                    yield return AnswerOthers(play, said, "The king's men");   // (a card that came meanwhile would hold the clock)
                }
                double waited = clock.GameHours - signalled;
                Assert.AreEqual(PatrolStage.None, Patrols.Stage, $"the guard boat still holds her after {waited:0.0} game hours of running clock");
                Assert.IsFalse(movedWhileHeld, "she sailed on under the guard boat's signal: she should be held until it is answered");
                Assert.AreEqual(Patrols.PressHours, waited, 0.5, "the guard boat should be answered for her when its time is up");
                Assert.IsTrue(FrameLog.Lines.Skip(logAt).Any(l => l.text.StartsWith("No answer was given to the guard boat", System.StringComparison.Ordinal)),
                              "the log does not say that the guard boat was answered for her: " + string.Join(" / ", FrameLog.Lines.Skip(logAt).Select(l => l.text)));
                yield return Frames(5);
                yield return play.Shot("43_guard_boat_answered_itself");
                yield return AnswerAll(play, said);                        // (what came of it: Go on)
                before = Where(game);
                for (int i = 0; i < 90; i++)
                {
                    yield return null;
                    yield return AnswerOthers(play, said);                 // (a card that came up meanwhile would hold the clock)
                }
                Assert.Greater(WorldXZ.Distance(Where(game), before), 200.0,
                               "she did not sail on after the guard boat (the clock held: " + PanelClock.Holding + "; the patrol: " + Patrols.Stage
                               + "; in front: " + (play.FrontFloat() != null ? Title(play.FrontFloat()) : "nothing") + "; said: " + string.Join(" / ", said) + ")");
                Debug.Log($"Chart test (sea): the guard boat held her {waited:0.00} game hours and was then answered for her; log: {string.Join(" / ", FrameLog.Lines.Skip(logAt).Select(l => l.text))}");

                // ---- 3. A GUARD BOAT WHOSE TALK IS LOST (the case that held her for ever: a stage set, nothing to answer):
                // let go in seconds
                Assert.IsTrue(Patrols.Signal(GameData.Port("dorestad"), 1.0));
                yield return Frames(5);
                Assert.IsTrue(frame.FloatOpen("talk"));
                TextPanel.AbandonWaits();                                  // (its talk is gone, as when the flow that showed it ended)
                float lostAt = Time.realtimeSinceStartup;
                while (Patrols.Stage != PatrolStage.None && Time.realtimeSinceStartup - lostAt < 20f)
                    yield return null;
                Assert.AreEqual(PatrolStage.None, Patrols.Stage, "a guard boat with no talk to answer still holds her after twenty seconds");
                Assert.Less(Time.realtimeSinceStartup - lostAt, HoldWatch.LostSeconds + 4f, "she was held too long with nothing to answer");
                yield return Frames(10);
                yield return AnswerAll(play, said);
                before = Where(game);
                yield return Frames(90);
                Assert.Greater(WorldXZ.Distance(Where(game), before), 200.0, "she did not sail on after the lost talk");

                // ---- 4. A TRADER: hailed by the Hail button, closed with, spoken to alongside, parted from
                // (when the lookout has a sail that can be hailed the player pauses, as one does to think: at this speed she
                // would be past and out of hail before the button was found)
                SimShip trader = null;
                for (int i = 0; i < 2500 && trader == null; i++)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play, said);
                    if (i % 5 == 0 && FrameRoot.SailToHail() != null && play.FrontFloat() == null)
                    {
                        yield return play.Press(Key.Space);
                        Assert.IsTrue(GameSpeed.PlayerPaused, "Space did not pause the game");
                        trader = FrameRoot.SailToHail();
                        if (trader == null)
                            yield return play.Press(Key.Space);             // (gone already: on she goes)
                    }
                }
                if (trader == null)
                {
                    // (no sail came near enough in the time: one under way is brought alongside her course by the test,
                    // hove to so the world leaves her there, and the lookout makes her out)
                    var me = Where(game);
                    trader = game.World.ships.Where(s => !s.raider && s.state == ShipState.Sailing).OrderBy(s => WorldXZ.Distance(s.pos, me)).FirstOrDefault();
                    Assert.IsNotNull(trader, "no trader is under way anywhere in the world");
                    trader.pos = trader.prevPos = new WorldXZ(me.x + 2500.0, me.z + 1500.0);
                    trader.heaveTo = true;
                    for (int i = 0; i < 600 && FrameRoot.SailToHail() == null; i++)
                        yield return null;
                    yield return play.Press(Key.Space);
                    trader.heaveTo = false;
                    Debug.Log("Chart test (sea): no sail came near by herself; a trader was brought near by the test.");
                }
                Assert.IsTrue(GameSpeed.PlayerPaused, "the game should be paused for the hail");
                Assert.IsNotNull(FrameRoot.SailToHail(), "no sail can be hailed (a trader made out, clear of the ports)");
                yield return Settle(0.6f);
                var hail = Verb(play, "Hail");
                Assert.IsFalse(hail.ClassListContains("off"), "the Hail button is greyed though a sail can be hailed: " + hail.tooltip);
                yield return play.Shot("44_a_sail_to_hail");
                yield return play.Click(hail, "the Hail button");
                yield return Frames(5);
                Assert.AreEqual(EncounterStage.Offered, Encounter.Stage, "hailing her offered no meeting; the log says: " + FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                Assert.IsTrue(frame.FloatOpen("talk"), "the meeting did not come up as a window");
                yield return play.Press(Key.Space);                        // (the pause is lifted: the offer itself holds the clock now)
                Assert.IsFalse(GameSpeed.PlayerPaused);
                Assert.IsTrue(PanelClock.Holding, "the offer of a meeting should hold the clock");
                yield return play.Shot("44_trader_hailed");
                yield return play.ClickButton("Close and hail her", play.FrontFloat());
                yield return Frames(5);
                Assert.AreEqual(EncounterStage.Closing, Encounter.Stage, "she is not closing with the trader");
                for (int i = 0; i < 4000 && Encounter.Stage == EncounterStage.Closing; i++)
                {
                    yield return null;
                    if (play.FrontFloat() != null)
                        yield return Answer(play, said);                   // (a card on the way to her)
                }
                Assert.AreEqual(EncounterStage.Alongside, Encounter.Stage, "she never came alongside: " + SeaHolds.Now);
                yield return Frames(10);
                Assert.IsTrue(frame.FloatOpen("talk"), "alongside, the talk did not come up");
                yield return play.Shot("45_alongside");
                yield return play.ClickButton("Ask what prices she knows", play.FrontFloat());
                yield return Frames(10);
                yield return play.Shot("46_alongside_prices");
                yield return play.ClickButton("Part ways", play.FrontFloat());
                yield return Frames(10);
                Assert.AreEqual(EncounterStage.None, Encounter.Stage, "they did not part");
                Assert.IsFalse(GameSpeed.Paused, "the clock is still held after they parted");
                before = Where(game);
                yield return Frames(90);
                Assert.Greater(WorldXZ.Distance(Where(game), before), 200.0, "she did not take her course up again after the meeting");
                // a second sail is not offered by herself at once (the pace): only a hail brings one
                Assert.Less(Encounter.HoursSinceOffer, Encounter.ChartOfferGapHours, "the pace of meetings on the chart is not kept");

                // ---- 5. RAIDERS: a band comes on. The window holds the clock; let run, she runs from them by herself after
                // twenty minutes of the game; the chase is live; however it ends, she is free again
                // (a band that is at sea if there is one; else one from its port, given a short passage of its own so the
                // world can go on moving it; whatever the test changes of it is put back afterwards)
                var band = game.World.ships.Where(s => s.raider).OrderByDescending(s => s.state == ShipState.Sailing && s.path.Count >= 2).FirstOrDefault();
                if (band == null)
                    Debug.Log("Chart test (sea): there is no band of raiders in the world on " + clock.Text + ": the raiders were not played.");
                else
                {
                    var bandWas = (band.state, band.pos, band.prevPos, band.leg, band.along, band.done, band.total, band.chasing,
                                   path: new List<WorldXZ>(band.path), kinds: new List<byte>(band.kinds));
                    Debug.Log($"Chart test (sea): the band {band.id} ({band.origin}, {band.bandShips} ships, {band.men} men) was {band.state} with a path of {band.path.Count} points.");
                    // (by daylight: they do not come on in the dark)
                    for (int i = 0; i < 2000 && LevelClock.SunAltitude < 5f; i++)
                    {
                        yield return null;
                        if (play.FrontFloat() != null)
                            yield return Answer(play, said);
                    }
                    yield return AnswerAll(play, said);
                    var me = Where(game);
                    band.pos = band.prevPos = new WorldXZ(me.x + 6000.0, me.z - 5000.0);
                    if (band.state != ShipState.Sailing || band.path.Count < 2)
                    {
                        band.state = ShipState.Sailing;
                        band.path.Clear();
                        band.kinds.Clear();
                        band.path.Add(band.pos);
                        band.path.Add(new WorldXZ(band.pos.x + 20000.0, band.pos.z));
                        band.kinds.Add(LaneGraph.Sea);
                        band.leg = 0;
                        band.along = band.done = 0.0;
                        band.total = 20000.0;
                    }
                    band.chasing = true;                                    // (so the world leaves them where the test put them)
                    for (int i = 0; i < 900 && RaiderEncounter.Stage == RaidStage.None; i++)
                        yield return null;
                    if (RaiderEncounter.Stage == RaidStage.None)
                    {
                        band.chasing = false;
                        Debug.Log($"Chart test (sea): the band did not come on (laden, or the air too thick: haze {SimStage.HazeKm:0.0} km): the raiders were not played.");
                    }
                    else
                    {
                        Assert.AreEqual(RaidStage.Threat, RaiderEncounter.Stage);
                        yield return Frames(5);
                        Assert.IsTrue(frame.FloatOpen("talk"), "the raiders did not come up as a window on the chart");
                        Assert.AreEqual("Raiders", Title(play.FrontFloat()));
                        Assert.IsTrue(PanelClock.Holding, "raiders should hold the clock until the player answers or goes on");
                        yield return play.Shot("47_raiders");
                        // (at the game's own pace for a while, to see the chase begin; then on in a hurry)
                        yield return play.Click(Small(play, "1"), "the first speed");
                        Assert.IsFalse(PanelClock.Holding, "a speed chosen should let the clock run under the raiders' window");
                        double came = clock.GameHours;
                        for (int i = 0; i < 3000 && RaiderEncounter.Stage == RaidStage.Threat; i++)
                        {
                            yield return null;
                            yield return AnswerOthers(play, said, "Raiders");
                        }
                        Assert.AreNotEqual(RaidStage.Threat, RaiderEncounter.Stage, "the threat was never answered for her");
                        Debug.Log($"Chart test (sea): the raiders' threat stood {(clock.GameHours - came) * 60.0:0} game minutes with the clock running, then: {RaiderEncounter.Stage}");
                        yield return Frames(5);
                        if (RaiderEncounter.Stage == RaidStage.Running)
                        {
                            Assert.IsFalse(PanelClock.Holding, "a chase must not hold the clock: it is live");
                            yield return play.Shot("48_running_from_raiders");
                            Assert.AreEqual("Running from raiders", Title(play.FrontFloat()), "the chase's own window is not up");
                        }
                        yield return play.Click(Small(play, "3"), "the fastest speed");
                        // the chase runs its course: caught (then she gives up, as a player with no better answer), or free
                        for (int i = 0; i < 6000 && RaiderEncounter.Stage != RaidStage.None; i++)
                        {
                            yield return null;
                            if (RaiderEncounter.Stage == RaidStage.Caught && play.FrontFloat() != null && Title(play.FrontFloat()) == "Raiders alongside")
                            {
                                yield return play.Shot("49_raiders_alongside");
                                yield return Answer(play, said, "Submit");
                            }
                            else
                                yield return AnswerOthers(play, said, "Raiders", "Running from raiders", "Raiders alongside");
                        }
                        Assert.AreEqual(RaidStage.None, RaiderEncounter.Stage, "the raiders still have hold of her: " + SeaHolds.Now);
                        band.state = bandWas.state;                           // (at once; 7 Oct, the ship view's builder: left sailing beside her, the band comes on again at RaiderEncounter's next look, which goes by the wall clock, so whether that fell before the end of this test depended on how fast the machine draws a frame)
                        yield return Frames(10);
                        yield return play.Shot("50_after_the_raiders");
                    }
                    // the band as the world had it (before what came of the raid is shut: the looks for raiders are spaced
                    // in game minutes now, and a band the test left beside her has half a minute to come on again)
                    band.state = bandWas.state;
                    band.pos = bandWas.pos;
                    band.prevPos = bandWas.prevPos;
                    band.leg = bandWas.leg;
                    band.along = bandWas.along;
                    band.done = bandWas.done;
                    band.total = bandWas.total;
                    band.chasing = bandWas.chasing;
                    band.path.Clear();
                    band.path.AddRange(bandWas.path);
                    band.kinds.Clear();
                    band.kinds.AddRange(bandWas.kinds);
                    yield return AnswerAll(play, said);
                }
                Assert.AreEqual(SeaHold.None, SeaHolds.Now, "something still holds her at the end");
                Assert.AreEqual(0, Autosave.Made, "she has not moored anywhere: the game should not have saved itself");
                before = Where(game);
                yield return Frames(120);
                yield return AnswerAll(play, said);
                yield return Frames(60);
                Assert.Greater(WorldXZ.Distance(Where(game), before), 200.0, "at the end of it all she does not sail on");
                Debug.Log($"Chart test (sea): done at {clock.Text}; answered: {string.Join("; ", said)}");
                NoFaults(play);
            }
            finally
            {
                GameSpeed.Rates[2] = 1440.0;
                GameSpeed.SetLevel(0);
                play.End();
                FrameRoot.FreshLayout = false;
                MapPalette.SetPeople("frisian");                          // (the next run of the editor is not left in a test's colours)
            }
        }

        // =====================================================================================================
        static bool SaveThere(string slot) => File.Exists(Path.Combine(SaveGame.Folder, slot + ".kapman.json.gz"));

        static string InfoOf(string slot)
        {
            string path = Path.Combine(SaveGame.Folder, slot + ".kapman.info");
            return File.Exists(path) ? File.ReadAllText(path) : "";
        }

        /// <summary>What a player would know a game by, to tell whether it has come back as it was.</summary>
        sealed class Seen
        {
            public double hours, silver;
            public string hold = "", layout = "", card = "", cast = "", lines = "", course = "";
            public int kinds, log, asked;
            public List<string> logLines = new List<string>();

            public static Seen Now(FrameRoot frame)
            {
                var game = KapmanGame.Current;
                var cards = EncounterRunner.Director;
                var s = new Seen
                {
                    hours = game.Hours, silver = FrameGame.Silver,
                    hold = string.Join(", ", Trade.Hold.Contents.Select(c => c.count + " " + c.good.id).OrderBy(x => x)),
                    layout = frame.Windows.Capture(),
                    course = Course.IsSet ? Course.Destination + " " + Course.Points.Count + " points, next " + Course.Next : "",
                    kinds = Course.Kinds.Count, log = FrameLog.Lines.Count,
                    asked = PersonTalk.Asked("dorestad.ravn", "rumour"),
                };
                foreach (var l in FrameLog.Lines)
                    s.logLines.Add(l.text);
                var open = cards != null ? cards.Current : null;
                if (open != null && !open.Chosen)
                {
                    s.card = open.card.id;
                    s.cast = string.Join(", ", open.actors.OrderBy(a => a.Key).Select(a => a.Key + "=" + a.Value.id + " " + a.Value.name));
                    s.lines = string.Join(" / ", open.lines.Select(l => l.text));
                }
                return s;
            }
        }

        /// <summary>
        /// THE CLOCK HAS A PAUSE AND FIVE SPEEDS, AND DROPS BACK BY ITSELF (ruling 26 of 7 Oct 2026). A new game at
        /// Dorestat: the bar has Pause and 1 to 5; each button, + and -, and Space do what they say; the time scale stays
        /// 1 at every speed (the game steps its own clock); at speed 5 hours pass in a second and every one of them is
        /// a step that was run; the pause menu has the switches. Then she sails for Traiectum at speed 5: nearing the
        /// port the clock drops to speed 2, made fast to speed 1, the bar says why, and + goes back to the speed she had.
        /// </summary>
        [UnityTest]
        public IEnumerator ThenTheClockHasFiveSpeeds()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;                                      // (until she has begun; then this test is of the drops)
            ClockSettings.AsNew = true;                                   // (the switches a new player has, whatever this machine keeps)
            SaveGame.FolderOverride = TestSaves();
            if (SceneManager.GetActiveScene().name != "L_Chart")
                yield return OpenChart();
            var menu = MainMenu.Current;
            Assert.IsNotNull(menu, "no main menu in L_Chart");
            var play = ChartPlay.Begin(false);
            var said = new List<string>();
            float from;
            try
            {
                if (menu.OnTitle)
                    yield return play.ClickButton("1. New game");
                else
                {
                    yield return AnswerAllIfBegun(play);
                    FrameRoot.Current?.SetMeasuring(false);
                    yield return play.Press(Key.Escape);
                    Assert.IsTrue(menu.Open, "Esc did not bring the menu");
                    var newGame = play.Screen.Query<Button>().ToList().FirstOrDefault(b => ChartPlay.Shown(b) && (b.text ?? "").EndsWith(". New game", System.StringComparison.Ordinal));
                    yield return play.Click(newGame, "New game in the pause menu");
                    yield return play.ClickButton("1. Yes");
                    FrameRoot.FreshLayout = true;
                    FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
                    SaveGame.FolderOverride = TestSaves();
                }
                from = Time.realtimeSinceStartup;
                while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                    yield return null;
            }
            finally
            {
                play.End();
            }
            play = ChartPlay.Begin(false);                                // (the scene may be new: its screen is taken again)
            try
            {
                yield return Frames(120);
                var frame = FrameRoot.Current;
                Assert.IsNotNull(frame, "no frame");
                Assert.IsTrue(frame.AskingSelf, "a new game did not ask who the player is");
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                from = Time.realtimeSinceStartup;
                while ((FrameGame.PortHere == null || FrameGame.PortHere.id != "dorestad") && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                Assert.AreEqual("dorestad", FrameGame.PortHere?.id, "a Frisian does not lie at Dorestat");
                var game = KapmanGame.Current;
                var clock = LevelClock.Current;
                var cards = EncounterRunner.Director;
                menu = MainMenu.Current;
                yield return AnswerAll(play, said);

                // ---- THE BAR: Pause and five speeds, by their numbers
                Assert.IsTrue(GameSpeed.Stepped, "on the chart the game should step its own clock");
                Assert.AreEqual(5, GameSpeed.Count);
                Assert.IsNotNull(Small(play, "Pause"), "the top bar has no Pause");
                for (int i = 1; i <= GameSpeed.Count; i++)
                    Assert.IsNotNull(Small(play, i.ToString()), "the top bar has no button for speed " + i);
                Assert.IsNull(Small(play, "4x"), "the old 4x button is still on the bar");
                // each button sets its speed, and the time scale stays 1 (it is the clock that steps, not the world that races)
                for (int i = GameSpeed.Count; i >= 1; i--)
                {
                    yield return play.Click(Small(play, i.ToString()), "the button of speed " + i);
                    Assert.AreEqual(i - 1, GameSpeed.Level, "the button of speed " + i + " did not set it");
                    Assert.AreEqual(GameSpeed.Rates[i - 1], GameSpeed.Rate, 1e-9, "the clock's rate at speed " + i);
                    yield return Frames(3);
                    Assert.AreEqual(1f, Time.timeScale, 1e-6f, "the time scale must stay 1 at speed " + i);
                }
                // + and -, and Space
                yield return AnswerAll(play, said);
                yield return play.Press(Key.Equals);
                Assert.AreEqual(1, GameSpeed.Level, "+ did not make the clock one faster");
                yield return play.Press(Key.Equals);
                yield return play.Press(Key.Equals);
                Assert.AreEqual(3, GameSpeed.Level, "+ twice more should be speed 4");
                yield return play.Press(Key.Minus);
                Assert.AreEqual(2, GameSpeed.Level, "- did not make the clock one slower");
                yield return AnswerAll(play, said);                        // (Space under a card lets the clock run: it is not a pause)
                yield return play.Press(Key.Space);
                Assert.IsTrue(GameSpeed.PlayerPaused, "Space did not pause");
                yield return Frames(3);
                Assert.AreEqual(0f, Time.timeScale, 1e-6f, "paused, the time scale is 0");
                double stood = clock.GameHours;
                yield return Frames(30);
                Assert.AreEqual(stood, clock.GameHours, 1e-9, "the clock moved while it was paused");
                yield return play.Press(Key.Space);
                Assert.IsFalse(GameSpeed.PlayerPaused, "Space did not go on");
                Assert.AreEqual(2, GameSpeed.Level, "going on should keep the speed");

                // ---- SPEED 5 in port: hours in a second, and every minute of them a step that was run
                yield return play.Click(Small(play, "5"), "the button of speed 5");
                double h0 = clock.GameHours;
                int most = 0;
                long steps = 0;
                for (int i = 0; i < 90; i++)
                {
                    yield return null;
                    most = System.Math.Max(most, KapmanGame.StepsLastFrame);
                    steps += KapmanGame.StepsLastFrame;
                    if (play.FrontFloat() != null)
                    {
                        // (someone has come down to the boat: answered, and on; the clock stood meanwhile)
                        yield return Answer(play, said);
                        steps += KapmanGame.StepsLastFrame;
                    }
                }
                double passed = clock.GameHours - h0;
                Debug.Log($"Chart test (clock): at speed 5 in port, 90 frames of a thirtieth of a second: {passed:0.00} game hours, {steps} steps, most in a frame {most}, " +
                          $"last frame's steps took {KapmanGame.StepMsLastFrame:0.0} ms; {clock.Text}; answered {string.Join("; ", said)}");
                Assert.LessOrEqual(most, ClockStepper.MostSteps, "a frame ran more steps than it may");
                Assert.Greater(passed, 3.0, "speed 5 is a day in four seconds: three seconds should be many hours");
                Assert.Less(passed, 18.5, "three seconds at speed 5 cannot be more than eighteen hours");
                Assert.AreEqual(System.Math.Round(clock.GameHours * 60.0), clock.GameHours * 60.0, 1e-6, "the clock should stand on a whole game minute");

                // ---- THE SWITCHES, in the pause menu: one line a reason
                yield return play.Click(Small(play, "1"), "the button of speed 1");
                yield return AnswerAll(play, said);
                yield return play.Press(Key.Escape);
                Assert.IsTrue(menu.Open, "Esc did not bring the menu");
                var clockLine = play.Screen.Query<Button>().ToList().FirstOrDefault(b => ChartPlay.Shown(b) && (b.text ?? "").EndsWith(". The clock", System.StringComparison.Ordinal));
                Assert.IsNotNull(clockLine, "the pause menu has no line for the clock: " + ChartPlay.Short(ChartPlay.Read(play.Screen)));
                yield return play.Click(clockLine, "The clock, in the pause menu");
                var rows = play.Screen.Query<Button>().ToList().Where(b => ChartPlay.Shown(b) && ((b.text ?? "").Contains(". On: ") || (b.text ?? "").Contains(". Off: "))).ToList();
                Assert.AreEqual(System.Enum.GetValues(typeof(ClockReason)).Length, rows.Count, "one line for each reason the clock slows for");
                Assert.IsTrue(rows.Any(b => b.text.EndsWith("Off: a sail close")), "any sail made out should be off from the start");
                yield return play.Shot("70_the_clock_slows_for");
                yield return play.SmallShot("70_the_clock_slows_for");
                Assert.IsTrue(ClockSettings.On(ClockReason.BreadLow));
                yield return play.Click(rows.First(b => b.text.EndsWith("On: bread for two days")), "the switch for bread");
                Assert.IsFalse(ClockSettings.On(ClockReason.BreadLow), "a press did not turn the switch off");
                var again = play.Screen.Query<Button>().ToList().FirstOrDefault(b => ChartPlay.Shown(b) && (b.text ?? "").EndsWith("Off: bread for two days", System.StringComparison.Ordinal));
                Assert.IsNotNull(again, "the list was not drawn again with the switch off");
                yield return play.Click(again, "the switch for bread, again");
                Assert.IsTrue(ClockSettings.On(ClockReason.BreadLow), "a second press did not turn it on again");
                yield return play.Press(Key.Escape);                      // (back to the pause menu)
                yield return play.Press(Key.Escape);                      // (and on)
                yield return Frames(3);
                Assert.IsFalse(menu.Open, "the menu did not shut");

                // ---- SHE SAILS AT SPEED 5: the clock drops back by itself, nearing the port and made fast
                ClockDrops.Quiet = false;
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()), "no course from Dorestat to Traiectum");
                yield return Frames(5);
                yield return play.Click(Verb(play, "Cast off"), "the Cast off button");
                yield return Frames(10);
                Assert.IsFalse(game.ship.anchored, "she did not cast off");
                yield return play.Click(Small(play, "5"), "the button of speed 5");
                Assert.AreEqual(4, GameSpeed.Level);
                bool near = false;
                double cast = clock.GameHours;
                int frames = 0;
                for (; frames < 9000 && !(FrameGame.PortHere != null && FrameGame.PortHere.id == "traiectum"); frames++)
                {
                    yield return null;
                    bool ashore = near && cards.Current != null && cards.Current.card.stage == Kapman.Encounters.Stage.Harbour;
                    if (play.FrontFloat() != null && !ashore)
                        yield return Answer(play, said);
                    if (!near && !ashore && GameSpeed.Level < 4 && ClockDrops.Last != ClockReason.NearPort && play.FrontFloat() == null)
                    {
                        // (the clock slowed for something else on the way, bread or raiders: + goes back, as a player would)
                        said.Add("the clock slowed, " + GameSpeed.DroppedFor + ": + pressed");
                        yield return play.Press(Key.Equals);
                    }
                    if (!near && ClockDrops.Last == ClockReason.NearPort)
                    {
                        near = true;
                        Assert.AreEqual(1, GameSpeed.Level, "nearing the port the clock should have dropped to speed 2");
                        Assert.AreEqual(4, GameSpeed.Before, "the speed she had is not kept");
                        yield return play.Shot("71_slowed_nearing_port");
                        StringAssert.Contains("slowed, nearing port. [+] back to 5", ChartPlay.Read(frame.Root.Q<VisualElement>(className: "frame-top")));
                    }
                }
                Debug.Log($"Chart test (clock): Dorestat to Traiectum at speed 5: {clock.GameHours - cast:0.0} game hours in {frames} frames; near port seen {near}; " +
                          $"level {GameSpeed.Level}, before {GameSpeed.Before}, dropped for '{GameSpeed.DroppedFor}'; {clock.Text}");
                Assert.AreEqual("traiectum", FrameGame.PortHere?.id, "she did not come in to Traiectum at speed 5");
                Assert.IsTrue(near, "the clock did not drop back as she neared Traiectum");
                // THE LANDING'S SLIP (the critic's B4, 8 Oct 2026: "the first voyage ends and nothing marks it"): one framed
                // line of the log, kept among the landings
                yield return ChartPlay.Until(() => Landings.All.Count > 0, 4f);
                Assert.AreEqual(1, Landings.All.Count, "the landing at Traiectum was not kept (or one was kept from a game before)");
                Assert.AreEqual("dorestad", Landings.All[0].from);
                Assert.AreEqual("traiectum", Landings.All[0].to);
                Assert.Greater(Landings.All[0].fetch, 1.0, "the casks aboard would fetch nothing at Traiectum");
                var slip = FrameLog.Lines.LastOrDefault(l => l.kind == "slip");
                Assert.IsNotNull(slip.text, "the log has no slip of the landing: " + string.Join(" / ", FrameLog.Lines.Skip(System.Math.Max(0, FrameLog.Lines.Count - 4)).Select(l => l.text)));
                StringAssert.StartsWith("Dorestat to Traiectum, ", slip.text);
                StringAssert.Contains("The purse holds ", slip.text);
                StringAssert.Contains("would fetch about ", slip.text);
                StringAssert.Contains("Owed 1920 d, with ", slip.text);
                said.Add("the landing: " + slip.text);
                Debug.Log("Chart test (clock): the landing's slip: " + slip.text);
                for (int i = 0; i < 30 && GameSpeed.Level != 0; i++)
                    yield return null;                                     // (the port's graph lays her to after the clock's turn of the frame)
                Assert.AreEqual(0, GameSpeed.Level, "made fast the clock should have dropped to speed 1");
                Assert.AreEqual(4, GameSpeed.Before, "the speed she had before the drops is not kept");
                Assert.AreEqual(ClockReason.Moored, ClockDrops.Last);
                // the bar says why the clock is slow. (The toll man comes down to a hull he has not seen, some game
                // minutes after she is made fast, and a second man may come after him: while one waits the bar says
                // that instead. So he is waited for and answered, and the picture is taken when nobody stands there.)
                for (int i = 0; i < 900 && cards.Current == null; i++)
                    yield return null;
                string bar = "";
                bool pictured = false;
                for (int i = 0; i < 60 && !pictured; i++)
                {
                    yield return Frames(10);
                    yield return AnswerAll(play, said);
                    bar = ChartPlay.Read(frame.Root.Q<VisualElement>(className: "frame-top"));
                    if (play.FrontFloat() != null || !bar.Contains("slowed, moored"))
                        continue;
                    yield return play.Shot("72_slowed_made_fast", 0.05f);
                    pictured = play.FrontFloat() == null;
                }
                StringAssert.Contains("slowed, moored. [+] back to 5", bar, "the bar does not say why the clock slowed");
                yield return play.SmallShot("72_slowed_made_fast");
                yield return play.Press(Key.Equals);
                Assert.AreEqual(4, GameSpeed.Level, "+ did not go straight back to the speed she had");
                Assert.AreEqual(-1, GameSpeed.Before, "a speed chosen is nothing to go back from");
                yield return play.Click(Small(play, "1"), "the button of speed 1");
                yield return AnswerAll(play, said);
                NoFaults(play);
            }
            finally
            {
                play.End();
                FrameRoot.FreshLayout = false;
                ClockDrops.Quiet = false;
                ClockSettings.AsNew = false;
            }
        }

        /// <summary>
        /// THE SAME SEED ENDS THE SAME AT EVERY SPEED (ruling 26 of 7 Oct 2026: "prove it by a test that runs the same
        /// seed at 1 and at 5"). The whole game on the chart, twice from a new game: three game days from Dorestat
        /// toward Lundenwic, once slowly (speed 2 in frames of a tenth of a second: never more than one step of the
        /// clock a frame) and once at speed 5 (six steps a frame). The test runs the clock by hand, so both runs are
        /// counted in frames and not by the wall; whoever comes is answered the same way in both, with the clock
        /// standing; the clock's own drops and its cap are at work, and the speed is picked again after each drop.
        /// At the end the clock, the world's ships, the markets, her place, her silver and her bread must be the same,
        /// and so must the list of everything that came up, each with the game minute it came in.
        /// HOW TO READ A FAILURE: the first line that differs names what is still looked at by the frame and not by
        /// the step (Design/fixes/clock_five_speeds.md, 6.3).
        /// </summary>
        [UnityTest]
        public IEnumerator ThenTheSameSeedEndsTheSameSlowAndAtSpeedFive()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;                                      // (until she has begun)
            ClockSettings.AsNew = true;
            SaveGame.FolderOverride = TestSaves();
            if (SceneManager.GetActiveScene().name != "L_Chart")
                yield return OpenChart();
            var ends = new List<string>();
            try
            {
                foreach (var (level, frameSeconds, name) in new[] { (1, 0.1, "slowly"), (4, 1.0 / 60.0, "at speed 5") })
                {
                    // ---- a new game, from the title or from the game before
                    var menu = MainMenu.Current;
                    Assert.IsNotNull(menu, "no main menu in L_Chart");
                    ClockDrops.Quiet = true;
                    var play = ChartPlay.Begin(false);
                    float from;
                    try
                    {
                        if (menu.OnTitle)
                            yield return play.ClickButton("1. New game");
                        else
                        {
                            yield return AnswerAllIfBegun(play);
                            FrameRoot.Current?.SetMeasuring(false);
                            yield return play.Press(Key.Escape);
                            Assert.IsTrue(menu.Open, "Esc did not bring the menu");
                            var newGame = play.Screen.Query<Button>().ToList().FirstOrDefault(b => ChartPlay.Shown(b) && (b.text ?? "").EndsWith(". New game", System.StringComparison.Ordinal));
                            yield return play.Click(newGame, "New game in the pause menu");
                            yield return play.ClickButton("1. Yes");
                            FrameRoot.FreshLayout = true;
                            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
                            SaveGame.FolderOverride = TestSaves();
                        }
                        from = Time.realtimeSinceStartup;
                        while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                            yield return null;
                    }
                    finally
                    {
                        play.End();
                    }
                    // (the window that asks who the player is holds the clock; from here the test runs it: no minute
                    // passes that the test did not run. A new game clears this, so it is set after each.)
                    KapmanGame.ClockByHand = true;
                    play = ChartPlay.Begin(false);
                    try
                    {
                        yield return Frames(120);
                        var frame = FrameRoot.Current;
                        Assert.IsNotNull(frame, "no frame");
                        Assert.IsTrue(frame.AskingSelf, "a new game did not ask who the player is");
                        yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                        from = Time.realtimeSinceStartup;
                        while ((FrameGame.PortHere == null || FrameGame.PortHere.id != "dorestad") && Time.realtimeSinceStartup - from < 15f)
                            yield return null;
                        Assert.AreEqual("dorestad", FrameGame.PortHere?.id, "a Frisian does not lie at Dorestat");
                        var game = KapmanGame.Current;
                        var said = new List<string>();
                        var seen = new List<string>();
                        yield return AnswerAll(play, said);
                        ClockDrops.Quiet = false;                             // (the game as a player has it: the drops and the cap)
                        Assert.IsTrue(Course.SetTo("lundenwic", ChartWorld.ShipWorld(), Port.LyingId()), "no course from Dorestat to Lundenwic");
                        yield return Frames(5);
                        yield return play.Click(Verb(play, "Cast off"), "the Cast off button");
                        yield return Frames(5);
                        var start = Where(game);
                        double began = game.Hours;
                        // (both runs end on the same game minute, and what the game is then is taken in that very step:
                        // a frame of six steps would else run past the end by up to five)
                        long endMinute = (long)System.Math.Floor(began * 60.0 + 1e-6) + 72 * 60;
                        string end = null;
                        var endAt = start;
                        System.Action<long> atStep = minute =>
                        {
                            if (minute != endMinute)
                                return;
                            endAt = Where(game);
                            end = $"minute {minute}; ships {game.World.StateHash()}; markets {Markets.Hash():X16}; her {System.Math.Round(endAt.x)} {System.Math.Round(endAt.z)}; " +
                                  $"silver {FrameGame.Silver:0.##}; bread {Voyage.FoodDays:0.###} days";
                        };
                        KapmanGame.Stepped += atStep;
                        int frames = 0, most = 0, waited = 0;
                        long steps = 0;
                        for (; end == null && frames < 60000 && waited < 3000; frames++)
                        {
                            var front = play.FrontFloat();
                            if (front != null)
                            {
                                // whoever has come is answered with the clock standing, the same way in both runs
                                int was = said.Count;
                                string title = Title(front);
                                long minute = (long)System.Math.Round(game.Hours * 60.0);
                                yield return Answer(play, said);
                                seen.Add(minute + " " + (said.Count > was ? said[said.Count - 1] : title + ": ?"));
                                yield return Frames(3);
                                waited = 0;
                                continue;
                            }
                            if (PanelClock.Holding || GameSpeed.Paused)
                            {
                                waited++;                                    // (a window is on its way up, and holds the clock already)
                                yield return null;
                                continue;
                            }
                            waited = 0;
                            if (GameSpeed.Level != level)
                                GameSpeed.SetLevel(level);                   // (the player picks the speed again after a drop)
                            // (the last minutes one at a time, so the run does not go past its end)
                            long left = endMinute - (long)System.Math.Round(game.Hours * 60.0);
                            game.RunClock(left <= ClockStepper.MostSteps ? ClockStepper.StepSeconds / GameSpeed.Rates[level] : frameSeconds);
                            most = System.Math.Max(most, KapmanGame.StepsLastFrame);
                            steps += KapmanGame.StepsLastFrame;
                            yield return null;                               // (a real frame: the frame, the graphs, the cards' runner)
                        }
                        KapmanGame.Stepped -= atStep;
                        // (whoever the last minute brought is answered too, the clock standing)
                        yield return Frames(5);
                        for (int i = 0; i < 12 && play.FrontFloat() != null; i++)
                        {
                            int was = said.Count;
                            string title = Title(play.FrontFloat());
                            yield return Answer(play, said);
                            seen.Add(endMinute + " " + (said.Count > was ? said[said.Count - 1] : title + ": ?"));
                            yield return Frames(3);
                        }
                        Debug.Log($"Chart test (same seed, {name}): {steps} steps in {frames} frames, most in a frame {most}; she went {WorldXZ.Distance(endAt, start) / 1000.0:0} km; " +
                                  $"{LevelClock.Current.Text}; lying at '{Port.LyingId()}', hold {SeaHolds.Now}, level {GameSpeed.Level}, paused {GameSpeed.Paused}\n{end}\n" + string.Join("\n", seen));
                        Assert.Less(waited, 3000, $"the clock stood for 3000 frames with no window to answer ({name}): held by a card {PanelClock.Holding}, paused {GameSpeed.Paused}, hold {SeaHolds.Now}");
                        Assert.IsNotNull(end, $"three game days did not pass ({name}): {game.Hours - began:0.0} hours in {frames} frames");
                        Assert.AreEqual(endMinute / 60.0, game.Hours, 1e-6, "the run went past its end (" + name + ")");
                        Assert.LessOrEqual(most, level == 1 ? 1 : ClockStepper.MostSteps, "more steps in a frame than the run should have (" + name + ")");
                        Assert.Greater(WorldXZ.Distance(endAt, start), 50000.0, "in three days she went nowhere (" + name + ")");
                        ends.Add(end + "; came up " + seen.Count + "\n" + string.Join("\n", seen));
                        yield return play.Shot(level == 1 ? "73_three_days_slowly" : "73_three_days_at_speed_5");
                    }
                    finally
                    {
                        play.End();
                    }
                }
            }
            finally
            {
                KapmanGame.ClockByHand = false;
                ClockDrops.Quiet = false;
                ClockSettings.AsNew = false;
                FrameRoot.FreshLayout = false;
            }
            Assert.AreEqual(2, ends.Count);
            Assert.AreEqual(ends[0], ends[1], "the same seed ended differently slowly and at speed 5");
        }

        /// <summary>
        /// THE GAME SAVES ITSELF, AND A SAVED GAME COMES BACK AS IT WAS (7 Oct 2026, Design/save_audit.md). A new game at
        /// home is not saved (nothing has happened). She sails to Traiectum: when she moors the game saves itself. With
        /// the toll man's card open she is saved by [F5]; the game goes on and changes; [F9] brings back the moment of
        /// the save: the clock, the silver, the hold, the card with the same man and words, the log, the windows, the
        /// course with its river legs, what Ravn was asked. The game is quit: it saves itself again, and the last two are
        /// kept in order. From the title, Continue takes the newest and she lies at Traiectum with the card before her.
        /// </summary>
        [UnityTest]
        public IEnumerator ThenTheGameSavesItselfAndComesBack()
        {
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;
            SaveGame.FolderOverride = TestSaves();
            foreach (string old in Directory.GetFiles(SaveGame.FolderOverride))
                File.Delete(old);
            if (SceneManager.GetActiveScene().name != "L_Chart")
                yield return OpenChart();
            Assert.IsNotNull(MainMenu.Current, "no main menu in L_Chart");
            MainMenu.Current.NewGame();
            SaveGame.FolderOverride = TestSaves();
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;
            float from = Time.realtimeSinceStartup;
            while ((FrameRoot.Current == null || !FrameRoot.Current.AskingSelf) && Time.realtimeSinceStartup - from < 30f)
                yield return null;
            var frame = FrameRoot.Current;
            Assert.IsNotNull(frame, "no frame after a new game");
            var play = ChartPlay.Begin(false);
            var said = new List<string>();
            Seen saved = null;
            try
            {
                yield return Frames(120);
                Assert.IsTrue(ContentSave.Registered(FrameSave.Key), "the frame's part of a saved game is not registered");
                Assert.IsNotNull(SaveGame.CannotSave(), "a game that has not begun (who are you?) should not be saved");
                yield return play.Press(Key.F5);
                Assert.IsFalse(SaveThere("quick"), "[F5] saved a game that has not begun");
                yield return play.Click(play.Choices(play.FrontFloat())[0].button, "Begin");
                from = Time.realtimeSinceStartup;
                while (FrameGame.PortHere == null && Time.realtimeSinceStartup - from < 15f)
                    yield return null;
                var game = KapmanGame.Current;
                var clock = LevelClock.Current;
                var cards = EncounterRunner.Director;
                Assert.AreEqual("dorestad", FrameGame.PortHere?.id);
                yield return Settle(2.5f);
                Assert.AreEqual(0, Autosave.Made, "a new game lying at home has not moored: nothing to save yet");
                Assert.IsFalse(SaveThere(Autosave.Slot(1)));

                // ---- things a saved game should keep: Ravn asked for what is said here; the Sea workspace with a third window
                var ravn = PlaceBook.Current.Person("dorestad.ravn");
                yield return play.ClickWhen(() => LineButton(play, "People here", ravn.name, "Talk"), "Talk, beside Ravn");
                yield return Settle();
                var askRumour = play.Choices(play.FrontFloat()).FirstOrDefault(c => c.words == "Ask what is said here");
                if (askRumour.button != null)
                    yield return play.Click(askRumour.button, "Ask what is said here");
                int asked = PersonTalk.Asked(ravn.id, "rumour");
                yield return play.Click(play.FrontFloat().Q<Button>(className: "frame-float-shut"), "the cross of the talk");
                yield return play.Press(Key.Digit1);
                yield return play.Press(Key.N);
                Assert.AreEqual(3, frame.Windows.Space.slots.Count);
                frame.Windows.SetSize(1, FrameWindows.Large);              // (no button sets a size yet: the brief faces' builder's)

                // ---- to Traiectum
                var voyage = FramePanels.Get("voyage") as VoyagePanel;
                from = Time.realtimeSinceStartup;
                while (!voyage.Reckoned("traiectum") && Time.realtimeSinceStartup - from < 20f)
                    yield return null;
                string traiectum = GameData.Port("traiectum").name;
                yield return ChartPlay.Until(() => play.Line(traiectum, Window(play, "Voyage"))?.Q<Label>(className: "frame-num") != null, 20f);
                yield return Settle();
                yield return play.ClickWhen(() => play.Button(traiectum, Window(play, "Voyage")), "Traiectum in the Voyage window");
                yield return Settle(1.0f);
                yield return play.ClickWhen(() => play.Button("Set this course", Window(play, "Voyage")), "Set this course");
                Assert.IsTrue(Course.IsSet);
                Assert.AreEqual(Course.Points.Count, Course.Kinds.Count);
                Assert.Greater(Course.Kinds.IndexOf(LaneGraph.River), 0, "the way from Dorestat to Traiectum has no river leg");
                yield return play.Click(Verb(play, "Cast off"), "the Cast off button");
                GameSpeed.Rates[2] = 2400.0;
                yield return play.Click(Small(play, "3"), "the fastest speed");
                for (int i = 0; i < 6000 && !(FrameGame.PortHere != null && FrameGame.PortHere.id == "traiectum"); i++)
                {
                    yield return null;
                    if (play.FrontFloat() != null && !(cards.Current != null && cards.Current.card.id == Toll))
                        yield return Answer(play, said);
                }
                yield return play.Click(Small(play, "1"), "the first speed");
                GameSpeed.Rates[2] = 1440.0;
                Assert.AreEqual("traiectum", FrameGame.PortHere?.id, "she did not come in to Traiectum");

                // ---- she has moored: the game has saved itself
                // (and the toll man comes down; a card of the crew's that came as she came in is answered first)
                from = Time.realtimeSinceStartup;
                while ((Autosave.Made == 0 || cards.Current == null || cards.Current.card.id != Toll) && Time.realtimeSinceStartup - from < 25f)
                {
                    yield return null;
                    if (play.FrontFloat() != null && !(cards.Current != null && cards.Current.card.id == Toll && !cards.Current.Chosen))
                    {
                        yield return Answer(play, said);
                        yield return Frames(3);
                    }
                }
                Assert.AreEqual(1, Autosave.Made, "mooring at Traiectum did not save the game (" + (Autosave.Refused() ?? "nothing in the way") + ")");
                Assert.IsTrue(SaveThere(Autosave.Slot(1)), "there is no 'autosave 1'");
                Assert.IsFalse(SaveThere(Autosave.Fresh), "the fresh save was left lying");
                StringAssert.Contains("Traiectum", InfoOf(Autosave.Slot(1)), "the list's line for the autosave does not say where she lay");
                Assert.IsNotNull(cards.Current, "the toll man did not come down to the boat");
                Assert.AreEqual(Toll, cards.Current.card.id);
                yield return Frames(20);

                // ---- [F5] with the card open, and a course home charted under it
                Assert.IsTrue(Course.SetTo("dorestad", ChartWorld.ShipWorld(), Port.LyingId()), "no course home from Traiectum");
                int riverLeg = Course.Kinds.IndexOf(LaneGraph.River);
                Assert.Greater(riverLeg, 0, "the way home from Traiectum has no river leg");
                Assert.IsTrue(Course.LegIsRiver(riverLeg));
                yield return Settle();
                yield return play.Shot("60_before_the_save");
                saved = Seen.Now(frame);
                Assert.AreEqual(Toll, saved.card, "the toll man's card should be open and unanswered at the save");
                yield return play.Press(Key.F5);
                Assert.IsTrue(SaveThere("quick"), "[F5] did not save the game: " + FrameLog.Lines[FrameLog.Lines.Count - 1].text);
                StringAssert.StartsWith("Saved", FrameLog.Lines[FrameLog.Lines.Count - 1].text);

                // ---- the game goes on and changes: the card answered, a cask sold, the windows moved, the course given up
                string declare = cards.Current.options.First(o => o.def.key == "declare").label;
                yield return play.Click(play.Choices(play.FrontFloat()).First(c => c.words.EndsWith(declare, System.StringComparison.Ordinal)).button, "declare");
                yield return Frames(10);
                yield return play.ClickButton("Go on", play.FrontFloat());
                yield return AnswerAll(play, said);
                yield return play.Press(Key.Digit2);
                yield return play.ClickWhen(() => Window(play, "Trade")?.Query<Button>().ToList().FirstOrDefault(b => b.text == "Sell"), "Sell, in the Trade window");
                yield return play.Press(Key.Digit3);
                yield return play.Press(Key.R);
                Course.Clear();
                yield return Frames(120);
                var changed = Seen.Now(frame);
                Assert.AreNotEqual(saved.layout, changed.layout, "the windows were not moved");
                Assert.AreNotEqual(saved.log, changed.log);
                Assert.AreEqual("", changed.card);
                yield return play.Shot("61_the_game_went_on");

                // ---- [F9]: the moment of the save comes back
                yield return play.Press(Key.F9);
                yield return Frames(30);
                StringAssert.StartsWith("Loaded", FrameLog.Lines.Last(l => l.text.StartsWith("Loaded", System.StringComparison.Ordinal) || l.text.StartsWith("Not now", System.StringComparison.Ordinal)).text);
                yield return Settle(1.0f);
                var back = Seen.Now(frame);
                Assert.AreEqual(saved.hours, back.hours, 0.25, "the clock did not come back to the hour of the save");
                Assert.AreEqual(saved.silver, back.silver, 0.01, "the silver is not as it was saved");
                Assert.AreEqual(saved.hold, back.hold, "the hold is not as it was saved");
                Assert.AreEqual(saved.card, back.card, "the open card did not come back");
                Assert.AreEqual(saved.cast, back.cast, "the card came back with other people in it");
                Assert.AreEqual(saved.lines, back.lines, "the card came back saying something else");
                Assert.IsTrue(frame.FloatOpen("card"), "the card is not on screen after the load");
                Assert.IsTrue(PanelClock.Holding, "the card should hold the clock after the load, as when it first opened");
                Assert.AreEqual(saved.layout, back.layout, "the windows did not come back as they were saved");
                Assert.IsFalse(ChartPlay.Read(Window(play, "Trade")).Contains("Sold 1"), "the Trade window still says what was sold after the save");
                Assert.AreEqual(FrameWindows.Large, frame.Windows.spaces[FrameWindows.SeaSpace].sizes.TryGetValue(frame.Windows.spaces[FrameWindows.SeaSpace].slots[1], out int size) ? size : -1,
                                "the size of a window did not come back");
                Assert.AreEqual(saved.course, back.course, "the course did not come back");
                Assert.AreEqual(saved.kinds, back.kinds, "the kinds of the course's legs did not come back");
                Assert.IsTrue(Course.LegIsRiver(riverLeg), "after the load the river is not a river");
                Assert.AreEqual(asked, PersonTalk.Asked(ravn.id, "rumour"), "what Ravn was asked did not come back");
                Assert.GreaterOrEqual(back.log, saved.log, "lines of the log were lost");
                for (int i = 0; i < saved.log; i++)
                    Assert.AreEqual(saved.logLines[i], back.logLines[i], "line " + i + " of the log is not as it was saved");
                Assert.IsFalse(back.logLines.Any(l => l.StartsWith("Sold", System.StringComparison.Ordinal) && !saved.logLines.Contains(l)),
                               "the log still shows what happened after the save");
                Assert.AreEqual("traiectum", FrameGame.PortHere?.id, "she does not lie at Traiectum after the load");
                Assert.AreEqual(1, TextPanel.Waiters, "after a load two flows wait on the port's menu: a choice would be taken twice");
                Assert.IsFalse(frame.AskingSelf, "a loaded game asked who the player is");
                yield return play.Shot("62_loaded");

                // ---- the game is quit: it saves itself once more, and the last two are kept in order
                int made = Autosave.Made;
                Autosave.Quitting();                                        // (what the menu's Quit and the closing of the window both do)
                Autosave.Quitting();
                Assert.AreEqual(made + 1, Autosave.Made, "quitting did not save the game once");
                Assert.IsTrue(SaveThere(Autosave.Slot(1)) && SaveThere(Autosave.Slot(2)), "the last two autosaves are not both kept");
                Assert.IsFalse(SaveThere(Autosave.Slot(3)));
                Assert.IsTrue(SaveThere("quick"), "the player's own save was touched");
                var slots = SaveGame.Slots();
                Assert.AreEqual(Autosave.Slot(1), slots[0].slot, "the newest saved game should be the one made at the quit");
            }
            finally
            {
                GameSpeed.Rates[2] = 1440.0;
                GameSpeed.SetLevel(0);
                play.End();
            }

            // ---- another day: the game is started, and from the title Continue takes the newest save
            MainMenu.Current.NewGame();
            SaveGame.FolderOverride = TestSaves();
            FrameRoot.FreshLayout = true;
            FrameWindows.OneWindow = false;                              // (these tests are of the tiled windows)
            ClockDrops.Quiet = true;
            yield return Frames(30);
            from = Time.realtimeSinceStartup;
            while ((FrameRoot.Current == null || KapmanGame.Current == null || KapmanGame.Current.World == null) && Time.realtimeSinceStartup - from < 30f)
                yield return null;
            yield return Frames(60);
            frame = FrameRoot.Current;
            Assert.IsFalse(Self.Chosen, "a game started afresh has not chosen who it is");
            MainMenu.Current.ShowTitle();
            play = ChartPlay.Begin(false);
            try
            {
                yield return play.Shot("63_title_with_continue");
                var cont = play.ButtonStarting("2. Continue");
                Assert.IsNotNull(cont, "the title offers no Continue though saved games are there");
                StringAssert.Contains("Traiectum", cont.text, "Continue does not say where she lay");
                yield return play.Click(cont, "Continue");
                yield return Frames(30);
                Assert.IsFalse(MainMenu.Current.OnTitle, "Continue did not leave the title: " + ChartPlay.Short(ChartPlay.Read(play.Screen)));
                yield return Settle(1.5f);
                var back = Seen.Now(frame);
                Assert.IsTrue(Self.Chosen, "the loaded game has no self");
                Assert.AreEqual("frisian", Self.People);
                Assert.IsFalse(frame.AskingSelf, "a continued game asked who the player is");
                Assert.AreEqual("traiectum", FrameGame.PortHere?.id, "she does not lie at Traiectum after Continue");
                Assert.AreEqual(saved.hours, back.hours, 0.5, "the clock is not that of the saved game");
                Assert.AreEqual(saved.silver, back.silver, 0.01);
                Assert.AreEqual(saved.hold, back.hold);
                Assert.AreEqual(saved.card, back.card, "the open card did not come back after Continue");
                Assert.AreEqual(saved.cast, back.cast, "after Continue the card has other people in it");
                Assert.AreEqual(saved.layout, back.layout, "the windows are not those of the saved game");
                Assert.AreEqual(saved.course, back.course);
                Assert.AreEqual(saved.kinds, back.kinds);
                Assert.Greater(Course.Kinds.IndexOf(LaneGraph.River), 0, "after Continue the course has no river leg");
                Assert.IsTrue(back.logLines.Any(l => l.StartsWith("You begin", System.StringComparison.Ordinal)), "the log of the saved game did not come back");
                Assert.AreEqual(1, TextPanel.Waiters, "after Continue two flows wait on the port's menu");
                Assert.AreEqual(0, Autosave.Made, "loading a game is not a mooring: it must not save itself over what it loaded");
                yield return play.Shot("64_continued");
                // and it plays on: the toll man is answered, once
                var cards = EncounterRunner.Director;
                string declare = cards.Current.options.First(o => o.def.key == "declare").label;
                double silver = FrameGame.Silver;
                yield return play.Click(play.Choices(play.FrontFloat()).First(c => c.words.EndsWith(declare, System.StringComparison.Ordinal)).button, "declare");
                yield return Frames(10);
                yield return play.ClickButton("Go on", play.FrontFloat());
                yield return Settle(3f);
                yield return AnswerAll(play, said);
                Assert.IsNull(cards.Current, "another card came after the toll man was answered: the call's cards were put in line twice");
                Debug.Log($"Chart test (saves): continued at {LevelClock.Current.Text}; silver {silver:0} -> {FrameGame.Silver:0}; autosaves in {SaveGame.Folder}: " +
                          string.Join(", ", SaveGame.Slots().Select(s => s.slot + " (" + s.where + ")")));
                NoFaults(play);
            }
            finally
            {
                // (the saves folder stays the test's own to the end of the run: leaving Play saves the game once more)
                FrameRoot.FreshLayout = false;
                MapPalette.SetPeople("frisian");
                play.End();
            }
        }
    }
}
