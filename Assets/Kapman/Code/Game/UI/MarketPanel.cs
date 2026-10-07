using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE MARKET PANEL - a port's market, sleek and plain:
    ///   left    the goods: on offer here, wanted here, or in your hold; what the market asks and pays (in the
    ///           zone's own reckoning: deniers, pennies, grams of silver, dirhams); buy and sell buttons
    ///   right   your purse (and what it's worth here), your debt, your hold (how full), the good you've picked
    ///           (what it is, its season, the laws on it, and the prices you've seen elsewhere)
    ///   bottom  what came of each deal, as facts
    /// No story is written here: the line under the title is your writing slot (market.{port}, in writing.json).
    /// The graphs open it (Market: Open) and wait for it to close (Esc or Leave).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MarketPanel : MonoBehaviour
    {
        public StyleSheet style;

        public static MarketPanel Current { get; private set; }
        public bool IsOpen { get; private set; }
        /// <summary>The good picked in the list (the map marks where it's made and wanted), or null.</summary>
        public string Picked => IsOpen ? picked : null;
        /// <summary>Beside the map (the town view), rather than over the 3D.</summary>
        public bool Docked { get; private set; }
        Button leaveButton;

        Market market;
        PortDef port;
        string category = "All", picked;
        VisualElement root, tabs, details, purseBox, holdBox, left, headRow;
        ScrollView list;
        Label title, writing, subtitle, outcome, holdBar, holdText;
        VisualElement holdFill;
        // the word on each good (Known.WordOn), asked once a redraw: the day, this port, the world's ships
        WordAsk wordAsk;
        readonly Dictionary<string, GoodWord> words = new Dictionary<string, GoodWord>();

        static readonly string[] CategoryOrder =
        {
            "Food", "Provisions", "Drink", "Spices", "Livestock", "Textiles", "Fibres", "Dyes", "Furs", "Hides",
            "Ivory and bone", "Crafts", "Stone", "Pottery", "Glass", "Beads and gems", "Metals", "Weapons", "Captives",
            "Books and church goods", "Timber", "Ship stores", "Animals",
        };

        void Awake() => Current = this;

        void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            var docRoot = doc.rootVisualElement;
            root?.RemoveFromHierarchy();                             // only our own elements
            if (style != null && !docRoot.styleSheets.Contains(style))
                docRoot.styleSheets.Add(style);
            // this document nests in the game's (its object is a child of '--- GAME ---'): fill the screen, and let
            // clicks through while the market's shut
            docRoot.style.position = Position.Absolute;
            docRoot.style.left = 0;
            docRoot.style.top = 0;
            docRoot.style.right = 0;
            docRoot.style.bottom = 0;
            docRoot.pickingMode = PickingMode.Ignore;
            root = new VisualElement { pickingMode = PickingMode.Ignore };   // (docked, the map beside it takes the clicks)
            root.AddToClassList("market-root");
            var box = new VisualElement();
            box.AddToClassList("market-box");
            root.Add(box);

            var head = new VisualElement();
            head.AddToClassList("market-head");
            var titles = new VisualElement();
            titles.AddToClassList("market-titles");
            title = new Label();
            title.AddToClassList("market-title");
            subtitle = new Label();
            subtitle.AddToClassList("market-subtitle");
            writing = new Label();
            writing.AddToClassList("market-writing");
            titles.Add(title);
            titles.Add(subtitle);
            titles.Add(writing);
            head.Add(titles);
            leaveButton = new Button(Close) { text = "Leave the market  (Esc)" };
            leaveButton.AddToClassList("market-leave");
            head.Add(leaveButton);
            box.Add(head);

            var body = new VisualElement();
            body.AddToClassList("market-body");
            box.Add(body);

            left = new VisualElement();
            left.AddToClassList("market-left");
            tabs = new VisualElement();
            tabs.AddToClassList("market-tabs");
            left.Add(tabs);
            left.Add(headRow = Row(true, null));
            list = new ScrollView(ScrollViewMode.Vertical);
            list.AddToClassList("market-list");
            left.Add(list);
            body.Add(left);

            var right = new VisualElement();
            right.AddToClassList("market-right");
            purseBox = new VisualElement();
            purseBox.AddToClassList("market-card");
            right.Add(purseBox);
            holdBox = new VisualElement();
            holdBox.AddToClassList("market-card");
            right.Add(holdBox);
            details = new VisualElement();
            details.AddToClassList("market-card");
            details.AddToClassList("market-details");
            right.Add(details);
            body.Add(right);

            outcome = new Label();
            outcome.AddToClassList("market-outcome");
            box.Add(outcome);

            docRoot.Add(root);
            root.style.display = DisplayStyle.None;
            IsOpen = false;
        }

        void OnDisable()
        {
            UiFocus.Release(this);
            Deal.Dealt -= OthersDealt;
        }

        void Start() => Deal.Dealt += OthersDealt;

        /// <summary>Someone dealt in this market while it's open: its stock and prices moved. Another ship: show them. The
        /// merchant: the price notes are renewed (the market as it is after the deal).</summary>
        void OthersDealt(DealInfo d)
        {
            if (!IsOpen || port == null || d.port != port.id)
                return;
            var game = KapmanGame.Current;
            if (d.who == "player")
            {
                if (game != null && market != null && Trade.Started)
                    Trade.Ledger.Note(market, game.Day, game.Month);
            }
            else
                Refresh();
        }

        /// <summary>Opens the market of the port the ship lies at: over the 3D, or docked beside the map (the town view).</summary>
        public void Open(Port at, bool docked = false)
        {
            var game = KapmanGame.Current;
            port = at != null ? at.Def : null;
            if (port == null || game == null || !Trade.Started)
            {
                IsOpen = false;
                return;
            }
            market = Markets.Of(port, game.Year, game.Day);
            Trade.Ledger.Note(market, game.Day, game.Month);
            IsOpen = true;
            Docked = docked;
            UiFocus.Take(this);
            root.style.display = DisplayStyle.Flex;
            root.EnableInClassList("docked", docked);
            // in the words of the place (Data/wording_793.json)
            leaveButton.text = (docked ? W("leave", "Back to the ship") : "Leave the market") + "  (Esc)";
            title.text = Wording.MarketTitle(port) + (string.IsNullOrEmpty(port.modern) ? "" : $" ({port.modern})");
            string tollLine = W("toll_line", "toll {toll}% on sales");
            subtitle.text = $"{Cap(game.Season)} {game.Year} · prices in {market.zone.unit} ({market.zone.name})" +
                            (market.zone.toll > 0 ? " · " + tollLine.Replace("{toll}", (market.zone.toll * 100).ToString("0"))
                                                  : tollLine.Contains("{toll}") ? "" : " · " + tollLine);
            int headAt = left.IndexOf(headRow);
            headRow.RemoveFromHierarchy();
            left.Insert(Mathf.Max(0, headAt), headRow = Row(true, null));
            string key = "market." + port.id;
            // your writing for the market, if there is any (no placeholder on screen)
            string w = TextPanel.Current != null ? TextPanel.Current.Writing(key) : "";
            writing.text = w.StartsWith("[write:") ? "" : w;
            writing.style.display = writing.text.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            outcome.text = W("market_intention", "See what's for sale and what they'll pay.");
            category = "All";
            picked = null;
            Refresh();
        }

        public void Close()
        {
            IsOpen = false;
            UiFocus.Release(this);
            if (root != null)
                root.style.display = DisplayStyle.None;
        }

        void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (IsOpen && kb != null && kb.escapeKey.wasPressedThisFrame)
                Close();
        }

        // ------------------------------------------------------------------------------------------ drawing

        void Refresh()
        {
            if (market == null)
                return;
            var game = KapmanGame.Current;
            int month = game != null ? game.Month : 6;
            var shown = Shown().ToList();
            words.Clear();
            wordAsk = new WordAsk
            {
                today = game != null ? game.Day : 0.0, month = month, year = game != null ? game.Year : 793, here = port,
                ships = game != null && game.World != null ? game.World.ships : null,
            };

            // the category tabs: those with something to show, and your hold
            tabs.Clear();
            var cats = new List<string> { "All" };
            cats.AddRange(CategoryOrder.Where(c => shown.Any(g => g.good.cat == c)));
            cats.Add("Your hold");
            int year = game != null ? game.Year : 793;
            bool writings = Chest.OffersAt(port, year).Count > 0 || port.kind == "monastery";
            if (writings)
                cats.Add("Writings");                                     // (books, letters, rune sticks; at a monastery, reading)
            else if (category == "Writings")
                category = "All";
            foreach (var c in cats)
            {
                string cat = c;
                var t = new Button(() => { category = cat; Refresh(); }) { text = c };
                t.AddToClassList("market-tab");
                t.EnableInClassList("on", c == category);
                tabs.Add(t);
            }

            var scroll = list.scrollOffset;                          // (keep the place in the list after a deal)
            list.Clear();
            headRow.style.display = category == "Writings" ? DisplayStyle.None : DisplayStyle.Flex;
            if (category == "Writings")
                Writings(year);
            else
            foreach (var g in shown.Where(g => category == "All" || g.good.cat == category ||
                                               (category == "Your hold" && Trade.Hold.Count(g.good.id) > 0)))
                list.Add(Row(false, g, month));
            if (list.childCount == 0)
                list.Add(new Label("Nothing here.") { name = "empty" });
            list.schedule.Execute(() => list.scrollOffset = scroll);

            DrawPurse();
            DrawHold();
            DrawDetails(month);
        }

        /// <summary>
        /// The writings to be had here (Chest.ForSaleAt: riddles and texts, each with who has it and its price), and at a
        /// monastery what its monks do for a merchant: read a Latin writing out (its English is open from then on), or teach
        /// letters (after enough lessons, Latin is read). Plain facts; the words are the user's to write.
        /// </summary>
        void Writings(int year)
        {
            var rules = TradeRules.Current;
            var zone = market.zone;
            foreach (var o in Chest.OffersAt(port, year))
            {
                var offer = o;
                var t = o.kind == "riddle" ? ChestData.Text(ChestData.Riddle(o.def)?.textId) : o.kind == "verse" ? ChestData.Text(ChestData.Verse(o.def)?.textId) : ChestData.Text(o.def);
                var row = new VisualElement();
                row.AddToClassList("market-row");
                row.Add(Cell(o.title + (o.kind == "riddle" ? " (a riddle)" : o.kind == "verse" ? " (a verse)" : t != null && t.voyage ? " (sailing directions)" : ""), "c-name"));
                row.Add(Cell(t != null ? $"{t.FormName}, {t.language}" : "", "c-unit"));
                row.Add(Cell(zone.Show(o.acquire.price), "c-num"));
                var buy = new Button(() => { outcome.text = Chest.Buy(offer, port); Refresh(); }) { text = W("buy", "Buy") };
                buy.AddToClassList("market-act");
                buy.tooltip = (string.IsNullOrEmpty(o.acquire.who) ? "" : "Had from " + o.acquire.who + ". ") + (o.acquire.why ?? "") +
                              (string.IsNullOrEmpty(o.acquire.needs) ? "" : " Needs: " + o.acquire.needs + ".");
                row.Add(buy);
                list.Add(row);
            }
            if (port.kind == "monastery")
            {
                const double readCost = 2.0, lessonCost = 4.0;
                foreach (var item in Chest.Items.Where(i => i.Text != null && i.Text.NeedsLearning && !Chest.CanRead(i) && !string.IsNullOrEmpty(i.Text.translation)))
                {
                    var it = item;
                    var row = new VisualElement();
                    row.AddToClassList("market-row");
                    row.Add(Cell("Have it read to you: " + it.Text.title, "c-name"));
                    row.Add(Cell(it.Text.language, "c-unit"));
                    row.Add(Cell(zone.Show(readCost), "c-num"));
                    var go = new Button(() =>
                    {
                        if (Trade.Purse.Pay(readCost, zone, rules, out string how))
                        {
                            Chest.ReadToYou(it);
                            outcome.text = $"A monk reads it out to you: its English is open in your chest. Paid {how}.";
                        }
                        else
                            outcome.text = $"You haven't the {zone.Show(readCost)} it costs.";
                        Refresh();
                    }) { text = "Ask" };
                    go.AddToClassList("market-act");
                    row.Add(go);
                    list.Add(row);
                }
                if (!Chest.KnowsLatin)
                {
                    var row = new VisualElement();
                    row.AddToClassList("market-row");
                    row.Add(Cell($"A lesson in letters ({Chest.LatinLessons} of {Chest.LessonsForLatin})", "c-name"));
                    row.Add(Cell("Latin", "c-unit"));
                    row.Add(Cell(zone.Show(lessonCost), "c-num"));
                    var go = new Button(() =>
                    {
                        outcome.text = Trade.Purse.Pay(lessonCost, zone, rules, out string how) ? Chest.Lesson() + $" Paid {how}." : $"You haven't the {zone.Show(lessonCost)} it costs.";
                        Refresh();
                    }) { text = "Learn" };
                    go.AddToClassList("market-act");
                    row.Add(go);
                    list.Add(row);
                }
            }
        }

        /// <summary>The goods to show: on offer here, wanted here, or in the hold. Made here first, then wanted.</summary>
        IEnumerable<MarketGood> Shown()
        {
            return market.Goods.Where(g => g.OnOffer || g.wanted > 0f || Trade.Hold.Count(g.good.id) > 0)
                         .OrderBy(g => System.Array.IndexOf(CategoryOrder, g.good.cat))
                         .ThenByDescending(g => g.OnOffer)
                         .ThenBy(g => g.good.name);
        }

        VisualElement Row(bool header, MarketGood g, int month = 6)
        {
            var row = new VisualElement();
            row.AddToClassList(header ? "market-row-head" : "market-row");
            if (header)
            {
                row.Add(Cell("Good", "c-name"));
                row.Add(Cell("Unit", "c-unit"));
                row.Add(Cell(W("here", "Here"), "c-num"));
                row.Add(Cell(W("asks", "They ask"), "c-num"));
                row.Add(Cell(W("pays", "They pay"), "c-num"));
                row.Add(Cell(W("aboard", "Aboard"), "c-num"));
                row.Add(Cell("", "c-buttons"));
                return row;
            }
            var good = g.good;
            int held = Trade.Hold.Count(good.id);
            var name = new VisualElement();
            name.AddToClassList("c-name");
            var n1 = new Label(good.name);
            n1.AddToClassList("good-name");
            name.Add(n1);
            string marks = (g.made > 0f ? "made here" : "") + (g.made > 0f && g.wanted > 0f ? " · " : "") + (g.wanted > 0f ? "wanted here" : "");
            var n2 = new Label(marks);
            n2.AddToClassList("good-marks");
            n2.EnableInClassList("made", g.made > 0f);
            n2.EnableInClassList("wanted", g.wanted > 0f);
            name.Add(n2);
            if (!string.IsNullOrEmpty(good.law) && TradeRules.Current.LawsOn(good, market.zone, KapmanGame.Current.Year).Count > 0)
            {
                var law = new Label("the law forbids it");
                law.AddToClassList("good-law");
                name.Add(law);
            }
            row.Add(name);
            row.Add(Cell(Short(good.unit) + $"\n{good.kg:0.#} kg", "c-unit"));
            row.Add(Cell(g.OnOffer ? Mathf.FloorToInt(g.stock).ToString() : "-", "c-num"));
            row.Add(Cell(g.OnOffer ? market.zone.Show(Deal.AsksFor(market, g, month, "player")) : "-", "c-num"));
            row.Add(Cell(market.zone.Show(Deal.PaysFor(market, g, month, "player")), "c-num"));
            row.Add(Cell(held > 0 ? held.ToString() : "", "c-num"));
            var buttons = new VisualElement();
            buttons.AddToClassList("c-buttons");
            if (g.OnOffer)
            {
                buttons.Add(Act(W("buy", "Buy") + " 1", () => Buy(good.id, 1)));
                buttons.Add(Act(W("buy", "Buy") + " 5", () => Buy(good.id, 5)));
            }
            if (held > 0)
            {
                buttons.Add(Act(W("sell", "Sell") + " 1", () => Sell(good.id, 1), true));
                buttons.Add(Act(W("sell_all", "Sell all"), () => Sell(good.id, held), true));
            }
            row.Add(buttons);
            // the good and, under it, the word on it: what the merchant last saw or heard of it elsewhere
            var item = new VisualElement();
            item.AddToClassList("market-item");
            item.Add(row);
            item.Add(new WordLine(WordOn(good)));
            item.EnableInClassList("picked", picked == good.id);
            item.RegisterCallback<ClickEvent>(e =>
            {
                picked = good.id;
                Refresh();
            });
            return item;
        }

        /// <summary>The word on a good (the freshest thing known of it anywhere but here; else the season, the ships that
        /// left with it, the law), worked out once a redraw.</summary>
        GoodWord WordOn(GoodDef good)
        {
            if (!words.TryGetValue(good.id, out var w))
                words[good.id] = w = Known.WordOn(good, wordAsk);
            return w;
        }

        /// <summary>A label of the market in the words of the place.</summary>
        string W(string name, string fallback) => Wording.Market(port, name, fallback);

        static Label Cell(string text, string cls)
        {
            var l = new Label(text);
            l.AddToClassList(cls);
            return l;
        }

        static Button Act(string text, System.Action act, bool sell = false)
        {
            var b = new Button(act) { text = text };
            b.AddToClassList("market-act");
            b.EnableInClassList("sell", sell);
            return b;
        }

        void DrawPurse()
        {
            var rules = TradeRules.Current;
            purseBox.Clear();
            purseBox.Add(Heading(W("purse", "Your purse")));
            foreach (var kv in Trade.Purse.All.OrderBy(kv => kv.Key == market.zone.coin ? 0 : 1))
                purseBox.Add(Line(Purse.Amount(kv.Key, kv.Value, rules), kv.Key == market.zone.coin ? "good money here" : "less here"));
            purseBox.Add(Line(W("worth_here", "Worth here"), market.zone.Show(Trade.Purse.Worth(market.zone, rules)), "strong"));
            if (Trade.Debt > 0.0)
            {
                string who = TextPanel.Current != null ? TextPanel.Current.Writing(Trade.Creditor) : "";
                if (who.Length == 0 || who.StartsWith("[write:"))
                    who = "the count";                                // (until you write his name in writing.json: creditor.count)
                double days = Trade.DebtDaysLeft(KapmanGame.Current.Day);
                purseBox.Add(Line(W("owed", "Owed to") + " " + who, $"{market.zone.Show(Trade.Debt)}, due in {Mathf.Max(0, (int)days)} days", "debt"));
            }
        }

        void DrawHold()
        {
            holdBox.Clear();
            var hold = Trade.Hold;
            holdBox.Add(Heading(W("hold", "Your hold")));
            if (Voyage.Started)
                holdBox.Add(Line("Stores", $"food {Voyage.FoodDays:0.#} days, drink {Voyage.DrinkDays:0.#} days for {Voyage.Men} men"));
            var bar = new VisualElement();
            bar.AddToClassList("hold-bar");
            var fill = new VisualElement();
            fill.AddToClassList("hold-fill");
            float share = hold.capacity > 0f ? Mathf.Clamp01(hold.Used / hold.capacity) : 0f;
            fill.style.width = new Length(share * 100f, LengthUnit.Percent);
            bar.Add(fill);
            holdBox.Add(bar);
            holdBox.Add(Line($"{share * 100f:0}% full", $"{hold.Weight:0} kg aboard, room for {hold.Free:0} kg more (dense goods)"));
            foreach (var (good, count) in hold.Contents)
            {
                holdBox.Add(Line(Trade.Units(good, count), $"cost {market.zone.Show(hold.CostEach(good.id))} each"));
                holdBox.Add(new WordLine(WordOn(good)));                  // (the word on it, under it)
            }
        }

        void DrawDetails(int month)
        {
            details.Clear();
            var g = picked != null ? market.Get(picked) : null;
            if (g == null)
            {
                details.Add(Heading("Pick a good"));
                details.Add(Note("Click a good to see what it is, when it's cheap, the laws on it, and what you've seen it fetch elsewhere."));
                return;
            }
            var good = g.good;
            details.Add(Heading(good.name));
            if (!string.IsNullOrEmpty(good.period))
                details.Add(Note(good.period, "period"));
            details.Add(Note($"{good.unit} · {good.kg:0.#} kg · {good.cat}"));
            if (!string.IsNullOrEmpty(good.note))
                details.Add(Note(good.note));
            if (!string.IsNullOrEmpty(good.season))
                details.Add(Note("Season: " + good.season + (market.Season(good, month) < 1f ? " (cheap now)" : market.Season(good, month) > 1f ? " (dear now)" : "")));
            foreach (var law in Trade.LawNotes(good, market.zone, KapmanGame.Current.Year))
                details.Add(Note("The law: " + law, "law"));
            if (!string.IsNullOrEmpty(good.risk) && Trade.LawNotes(good, market.zone, KapmanGame.Current.Year).Count == 0)
                details.Add(Note("Elsewhere: " + good.risk, "risk"));
            // what you've seen it fetch
            var seen = Trade.Ledger.Seen(good.id).Where(s => s.port != port.id).Take(6).ToList();
            details.Add(Heading(W("seen_elsewhere", "Seen elsewhere"), true));
            if (seen.Count == 0)
                details.Add(Note("Nowhere yet: the prices of other markets are learned by going there, by talk on the waterfront, or from ships you hail."));
            foreach (var s in seen)
            {
                var p = GameData.Port(s.port);
                var z = TradeRules.Current.ZoneOf(p?.region);
                double ago = KapmanGame.Current.Day - s.day;
                var note = Trade.Ledger.Latest(s.port);
                float sure = note != null ? Known.SureWhenLearned(note.how, note.hops) * Known.Weight(ago, Known.Rules.priceHalfLife) : 0f;
                string how = note == null || note.how == How.Seen ? "seen" : note.how == How.Told ? "told" : "heard";
                details.Add(Line(p != null ? p.name : s.port,
                                 (s.price.onOffer ? $"asks {z.Show(s.price.asks)}, " : "") + $"pays {z.Show(s.price.pays)}" +
                                 $" · {how} {(ago < 1 ? "today" : $"{ago:0} days ago")} · {Known.Words(sure)}"));
            }
            var sources = good.sources.Where(id => GameData.Sources.ContainsKey(id)).Select(id => GameData.Sources[id]).ToList();
            if (sources.Count > 0)
                details.Add(Note("Sources: " + string.Join(" · ", sources), "sources"));
        }

        static Label Heading(string text, bool small = false)
        {
            var l = new Label(text);
            l.AddToClassList(small ? "card-subheading" : "card-heading");
            return l;
        }

        static VisualElement Line(string left, string right, string cls = null)
        {
            var row = new VisualElement();
            row.AddToClassList("card-line");
            if (cls != null)
                row.AddToClassList(cls);
            row.Add(new Label(left) { name = "l" });
            row.Add(new Label(right) { name = "r" });
            return row;
        }

        static Label Note(string text, string cls = null)
        {
            var l = new Label(text);
            l.AddToClassList("card-note");
            if (cls != null)
                l.AddToClassList(cls);
            return l;
        }

        // ------------------------------------------------------------------------------------------ dealing

        void Buy(string good, int n)
        {
            var game = KapmanGame.Current;
            var r = Trade.Buy(market, good, n, game.Day, game.Month, game.Year);
            Show(r);
        }

        void Sell(string good, int n)
        {
            var game = KapmanGame.Current;
            var r = Trade.Sell(market, good, n, game.Day, game.Month, game.Year);
            Show(r);
        }

        void Show(TradeResult r)
        {
            picked = r.good ?? picked;
            outcome.text = "Outcome: " + r.text + (r.laws.Count > 0 ? "  (The law forbids it here: see the good.)" : "");
            outcome.EnableInClassList("failed", !r.ok);
            Refresh();
        }

        static string Short(string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return "";
            int cut = unit.IndexOf('(');
            return (cut > 0 ? unit.Substring(0, cut) : unit).Trim();
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
