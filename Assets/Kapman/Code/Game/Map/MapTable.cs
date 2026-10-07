using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE TABLE ABOARD (Plans/map_play.md) - what sits round the map while the merchant works at it:
    ///   - the top bar: the date and hour, the game's speed (pause, 1x, 2x, 4x), the silver, the debt, the alerts (what
    ///     needs you at sea: click one to go), and the chest;
    ///   - the outliner on the right: the ship and crew, the course, the hold, the latest news heard ([O] folds it; it
    ///     folds itself while the market is docked);
    ///   - the chest ([I]): everything written the merchant keeps. For now the price notes (the rune sticks: each port's
    ///     prices as noted, how and how long ago); the texts, riddles and your own notes come with the riddles (M2).
    /// Facts only; the words are plain, and the names of things are yours to write.
    /// </summary>
    public sealed class MapTable
    {
        readonly VisualElement bar, right, alertList, outliner, chest, chestArea;
        readonly ChestPanel pages;
        readonly Label date, silver, debt, alertsButtonText, outShip, outCourse, outHold, outNews, outKnown;
        readonly Button alertsButton;
        readonly List<Button> speedButtons = new List<Button>();
        readonly List<Button> chestTabs = new List<Button>();
        string chestTab = "riddles";
        bool outlinerFolded, alertsShown;
        int alertsVersion = -1, chestVersion = -1;
        float nextFill;

        public bool ChestOpen => chest.style.display == DisplayStyle.Flex;

        public MapTable(VisualElement root)
        {
            // ---- the top bar
            bar = Box("map-topbar");
            date = Text("map-topbar-date");
            bar.Add(date);
            var speed = Box("map-topbar-speed");
            speed.Add(SpeedButton("Pause", -1, "Pause and go on [Space]"));
            for (int i = 0; i < GameSpeed.Count; i++)
                speed.Add(SpeedButton((i + 1).ToString(), i, GameSpeed.Tips[i] + (i == 0 ? " [-]" : " [+]")));
            bar.Add(speed);
            silver = Text("map-topbar-figure");
            silver.tooltip = "Your silver, in deniers' worth (all your coin and hacksilver together).";
            bar.Add(silver);
            debt = Text("map-topbar-figure");
            bar.Add(debt);
            alertsButton = new Button(() => { alertsShown = !alertsShown; FillAlerts(true); });
            alertsButton.AddToClassList("map-button");
            alertsButton.AddToClassList("map-topbar-button");
            alertsButtonText = new Label();
            alertsButton.Add(alertsButtonText);
            alertsButton.tooltip = "What needs you at sea. Click one to go to the ship.";
            bar.Add(alertsButton);
            var chestButton = new Button(ToggleChest) { text = "Chest  (I)" };
            chestButton.AddToClassList("map-button");
            chestButton.AddToClassList("map-topbar-button");
            chestButton.tooltip = "Everything written you keep: your price notes, and (to come) texts, riddles and your own notes.";
            bar.Add(chestButton);
            root.Add(bar);

            // ---- the right column: the alerts, then the outliner under them
            right = Box("map-right");
            root.Add(right);
            alertList = Box("map-alerts");
            alertList.style.display = DisplayStyle.None;
            right.Add(alertList);

            // ---- the outliner
            outliner = Box("map-outliner");
            outliner.Add(Heading("Your ship"));
            outShip = Text("map-outliner-text");
            outliner.Add(outShip);
            outCourse = Text("map-outliner-text");
            outliner.Add(outCourse);
            outHold = Text("map-outliner-text");
            outliner.Add(outHold);
            outliner.Add(Heading("Latest news heard"));
            outNews = Text("map-outliner-text");
            outliner.Add(outNews);
            outliner.Add(Heading("What you know of the markets"));
            outKnown = Text("map-outliner-text");
            outKnown.tooltip = "From your price notes: how many markets you have prices for, and how fresh the newest and oldest are. " +
                               "A price goes half stale in a month: what everyone knows (dear where wanted, cheap where made) takes over.";
            outliner.Add(outKnown);
            var fold = Text("map-outliner-hint");
            fold.text = "[O] fold";
            outliner.Add(fold);
            right.Add(outliner);

            // ---- the chest
            chest = Box("map-chest");
            var head = Box("map-chest-head");
            var title = Text("map-title");
            title.text = "The chest";
            head.Add(title);
            var shut = new Button(ToggleChest) { text = "Close  (I)" };
            shut.AddToClassList("map-button");
            head.Add(shut);
            chest.Add(head);
            var tabs = Box("map-chest-tabs");
            foreach (var (id, label) in new[] { ("riddles", "Riddles"), ("texts", "Texts"), ("mine", "Your notes"), ("notes", "Price notes") })   // (no hoards tab until the hunt is rebuilt)
            {
                string tab = id;
                var b = new Button(() => { chestTab = tab; chestVersion = -1; FillChest(); }) { text = label };
                b.AddToClassList("map-button");
                b.AddToClassList("map-chest-tab");
                b.userData = id;
                chestTabs.Add(b);
                tabs.Add(b);
            }
            chest.Add(tabs);
            chestArea = Box("map-chest-area");
            chest.Add(chestArea);
            chest.style.display = DisplayStyle.None;
            root.Add(chest);
            pages = new ChestPanel(chest)
            {
                OpenTab = t => { chestTab = t; chestVersion = -1; FillChest(); },
                LayOut = id => { chest.style.display = DisplayStyle.None; MapView.Current?.LayOut(id); },
                MarkDig = id => { chest.style.display = DisplayStyle.None; MapView.Current?.MarkDig(id); },
            };
            UiTyping.Watch(root);

            GameSpeed.Changed += SpeedChanged;
            Alerts.Changed += AlertsChanged;
        }

        public void Dispose()
        {
            GameSpeed.Changed -= SpeedChanged;
            Alerts.Changed -= AlertsChanged;
        }

        void SpeedChanged() => FillSpeed();
        void AlertsChanged() => alertsVersion = -1;

        /// <summary>The frame is up (MapView.Framed): the top bar and the right column (the alerts, the outliner) stand
        /// down; the chest stays, on [I].</summary>
        public bool Framed
        {
            set
            {
                bar.style.display = value ? DisplayStyle.None : DisplayStyle.Flex;
                right.style.display = value ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        /// <summary>Each frame the map is open: the keys; the figures a few times a second.</summary>
        public void Update(bool marketDocked)
        {
            var kb = Keyboard.current;
            if (kb != null && !UiTyping.Active)
            {
                if (kb.iKey.wasPressedThisFrame)
                    ToggleChest();
                else if (kb.oKey.wasPressedThisFrame)
                    outlinerFolded = !outlinerFolded;
            }
            outliner.style.display = outlinerFolded || marketDocked || ChestOpen ? DisplayStyle.None : DisplayStyle.Flex;
            if (alertsVersion != Alerts.Open.Count * 1000 + (alertsShown ? 1 : 0))
                FillAlerts(false);
            if (Time.unscaledTime < nextFill)
                return;
            nextFill = Time.unscaledTime + 0.25f;
            FillBar();
            FillSpeed();
            if (outliner.style.display == DisplayStyle.Flex)
                FillOutliner();
            if (ChestOpen)
                FillChest();
        }

        // ------------------------------------------------------------------------------------------ the top bar

        void FillBar()
        {
            var game = KapmanGame.Current;
            date.text = game != null ? game.Date : "";
            if (!Trade.Started)
                return;
            var rules = TradeRules.Current;
            silver.text = $"Silver: {Trade.Purse.Silver(rules):0} d";
            if (Trade.Debt > 0.01)
            {
                double due = game != null ? Trade.DebtDaysLeft(game.Day) : 0.0;
                debt.text = $"Owed: {Trade.Debt:0} d, " + (due >= 0.0 ? $"due in {due:0} days" : $"{-due:0} days overdue");
                debt.tooltip = "What you owe your creditor, and when it falls due.";
                debt.EnableInClassList("warn", due < 14.0);
            }
            else
                debt.text = "Owed: nothing";
        }

        void FillSpeed()
        {
            foreach (var b in speedButtons)
            {
                int level = (int)b.userData;
                bool on = level < 0 ? GameSpeed.Paused : !GameSpeed.Paused && GameSpeed.Level == level;
                b.EnableInClassList("on", on);
                b.SetEnabled(!GameSpeed.Multiplayer || level == 0);
            }
        }

        Button SpeedButton(string label, int level, string tip)
        {
            var b = new Button(() =>
            {
                if (level < 0)
                    GameSpeed.TogglePause();
                else
                    GameSpeed.SetLevel(level);
                FillSpeed();
            }) { text = label, userData = level, tooltip = tip };
            b.AddToClassList("map-button");
            b.AddToClassList("map-speed-button");
            speedButtons.Add(b);
            return b;
        }

        void FillAlerts(bool force)
        {
            int version = Alerts.Open.Count * 1000 + (alertsShown ? 1 : 0);
            if (!force && version == alertsVersion)
                return;
            alertsVersion = version;
            int n = Alerts.Open.Count;
            bool urgent = Alerts.Open.Any(a => a.urgent);
            alertsButtonText.text = n == 0 ? "No alerts" : $"Alerts: {n}";
            alertsButton.EnableInClassList("warn", urgent);
            alertsButton.EnableInClassList("lit", n > 0);
            alertList.Clear();
            if (n > 0 && urgent)
                alertsShown = true;                                       // (urgent: always in view)
            alertList.style.display = alertsShown && n > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            foreach (var a in Alerts.Open)
            {
                var alert = a;
                var row = new Button(() => Alerts.Answer(alert)) { text = a.text + "  → go" };
                row.AddToClassList("map-alert-row");
                row.EnableInClassList("warn", a.urgent);
                row.tooltip = a.urgent ? "The game is stopped till you answer this (going on answers it)." : "The game has slowed to speed 1 for this.";
                alertList.Add(row);
            }
        }

        // ------------------------------------------------------------------------------------------ the outliner

        void FillOutliner()
        {
            var game = KapmanGame.Current;
            if (game == null)
                return;
            string kind = game.ship != null && game.ship.shipClass != null ? Readable(game.ship.shipClass.name) : "Your ship";
            outShip.text = kind + (Voyage.Started ? "\n" + MapView.CrewLine() : "");
            var w = MapWorld.ToWorld(game.ShipPosition);                      // ported 4 Oct: the level's place in the world, not the floating origin's
            var here = new Kapman.Game.Sim.WorldXZ(w.x, w.z);
            outCourse.text = Course.IsSet ? $"Bound for {GameData.Port(Course.Destination)?.name}: {Course.KmLeft(here):0} km to go, about {Course.Days(Course.KmLeft(here)):0.#} days"
                                          : "No course set: click a port.";
            if (Trade.Started)
            {
                var goods = Trade.Hold.Contents.OrderByDescending(c => c.count).Take(4).Select(c => Trade.Units(c.good, c.count)).ToList();
                outHold.text = $"Hold: {Trade.Hold.Used:0} of {Trade.Hold.capacity:0} kg of room" + (goods.Count > 0 ? "\n" + string.Join(", ", goods) : "\nEmpty");
            }
            var news = Known.News();
            outNews.text = news.Count == 0 ? "Nothing yet: talk on the waterfront, or hail a ship."
                         : string.Join("\n", news.Take(3).Select(f => $"{f.text} ({Ago(game.Day - f.trueDay)})"));
            if (Trade.Started)
            {
                int ports = 0;
                double newest = double.PositiveInfinity, oldest = 0.0;
                foreach (var n in Trade.Ledger.Notes)
                {
                    ports++;
                    double age = game.Day - n.day;
                    newest = Math.Min(newest, age);
                    oldest = Math.Max(oldest, age);
                }
                outKnown.text = ports == 0 ? "No prices noted yet." : $"Prices for {ports} {(ports == 1 ? "market" : "markets")}: the freshest {Ago(newest)}, the oldest {Ago(oldest)}.";
            }
        }

        // ------------------------------------------------------------------------------------------ the chest

        /// <summary>Opens the chest at a tab ("riddles", "texts", "mine", "notes").</summary>
        public void OpenChest(string tab)
        {
            chestTab = tab;
            chestVersion = -1;
            chest.style.display = DisplayStyle.Flex;
            FillChest();
        }

        void ToggleChest()
        {
            bool open = !ChestOpen;
            chest.style.display = open ? DisplayStyle.Flex : DisplayStyle.None;
            chestVersion = -1;
            if (open)
                FillChest();
        }

        void FillChest()
        {
            foreach (var b in chestTabs)
                b.EnableInClassList("on", (string)b.userData == chestTab);
            if (chestTab != "notes")
            {
                bool fresh = chestVersion != -2;
                chestVersion = -2;                                         // (the pages keep themselves up to date)
                if (fresh)
                    chestArea.Clear();
                pages.Show(chestArea, chestTab, force: fresh);
                return;
            }
            int version = (Trade.Started ? Trade.Ledger.Count : 0) * 31 + Known.Version;
            if (version == chestVersion)
                return;
            chestVersion = version;
            chestArea.Clear();
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.AddToClassList("map-chest-scroll");
            chestArea.Add(scroll);
            var chestBody = scroll.contentContainer;
            var game = KapmanGame.Current;
            switch (chestTab)
            {
                case "notes":
                    chestBody.Add(Note("Your price notes: a bundle of rune sticks for each market, carved with the prices you've seen or been told, " +
                                       "and who told you. Told prices can be a little off; rumours more so."));
                    if (!Trade.Started || Trade.Ledger.Count == 0)
                    {
                        chestBody.Add(Note("Nothing noted yet."));
                        break;
                    }
                    foreach (var n in Trade.Ledger.Notes.OrderBy(x => game != null ? game.Day - x.day : 0.0))
                    {
                        var port = GameData.Port(n.port);
                        double age = game != null ? game.Day - n.day : 0.0;
                        float sure = Known.SureWhenLearned(n.how, n.hops) * Known.Weight(age, Known.Rules.priceHalfLife);
                        int sticks = Trade.Ledger.History(n.port).Count;
                        var row = Line(port != null ? port.name : n.port,
                                       $"{HowWords(n)} {Ago(age)} · {Known.Words(sure)} · {sticks} {(sticks == 1 ? "stick" : "sticks")}");
                        row.tooltip = (n.source == null ? "Seen in the market yourself." : "Told: " + n.source + ".") +
                                      $"\nHow sure now: {sure * 100f:0}% (as sure as it was when learned, halving every {Known.Rules.priceHalfLife:0} days)." +
                                      (sticks > 1 ? $"\n{sticks} notes for this market in all, the oldest {Ago(game != null ? game.Day - Trade.Ledger.History(n.port)[0].day : 0.0)}." : "");
                        chestBody.Add(row);
                    }
                    break;
            }
        }

        static string HowWords(PriceNote n) =>
            n.how == How.Seen ? "seen" : n.how == How.Told ? "told by one who saw them," : n.how == How.Rumour ? "a rumour," : n.hops > 1 ? "talk at many removes," : "talk,";

        // ------------------------------------------------------------------------------------------ bits

        /// <summary>"Gislinge_WorkingBoat" as "Gislinge working boat".</summary>
        static string Readable(string id)
        {
            var b = new System.Text.StringBuilder();
            foreach (char c in (id ?? "").Replace('_', ' '))
            {
                if (char.IsUpper(c) && b.Length > 0 && b[b.Length - 1] != ' ')
                    b.Append(' ');
                b.Append(b.Length > 0 && char.IsUpper(c) && b[b.Length - 1] == ' ' ? char.ToLowerInvariant(c) : c);
            }
            return b.ToString();
        }

        static string Ago(double days) =>
            double.IsInfinity(days) ? "never" : days < 1.0 ? "today" : days < 2.0 ? "yesterday" : $"{days:0} days ago";

        static VisualElement Box(string cls)
        {
            var v = new VisualElement();
            v.AddToClassList(cls);
            return v;
        }

        static Label Text(string cls)
        {
            var l = new Label();
            l.AddToClassList(cls);
            return l;
        }

        static Label Heading(string t)
        {
            var l = new Label(t);
            l.AddToClassList("map-outliner-heading");
            return l;
        }

        static Label Note(string t)
        {
            var l = new Label(t);
            l.AddToClassList("map-chest-note");
            return l;
        }

        static VisualElement Line(string left, string right)
        {
            var row = Box("map-chest-row");
            var a = new Label(left);
            a.AddToClassList("map-chest-row-name");
            var b = new Label(right);
            b.AddToClassList("map-chest-row-text");
            row.Add(a);
            row.Add(b);
            return row;
        }
    }
}
