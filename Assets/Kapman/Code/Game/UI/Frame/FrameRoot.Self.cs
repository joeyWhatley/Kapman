using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// WHO ARE YOU? (the user, 7 Oct 2026: "in character customisation we will add culture, religion and personal
    /// background"; "depending on your culture you start somewhere else and the world is a different colour").
    /// On a new game on the chart a floating window asks the three choices on one page: your people, your faith, your
    /// past, each with a line of what it changes. A click on a people shows the world in that people's colours at once.
    /// It must be answered and holds the clock. Begin makes the self (Self.Begin), lays her at her people's own port
    /// where the game has it (ChartWorld.StartAt), and says who she is in the log. A loaded game brings its own self
    /// and is not asked.
    /// </summary>
    public partial class FrameRoot
    {
        const string SelfId = "self";
        string askPeople, askFaith, askPast;

        /// <summary>The creation window is up.</summary>
        public bool AskingSelf => FloatOpen(SelfId);
        /// <summary>What the creation window has picked now (for a test, a tool).</summary>
        public (string people, string faith, string past) SelfPicks => (askPeople, askFaith, askPast);

        partial void SelfUpdate()
        {
            bool ask = !Self.Chosen && ChartWorld.Current != null && !ChartWorld.SaveLoaded && KapmanGame.Current != null;
            if (!ask)
            {
                if (FloatOpen(SelfId))
                    CloseFloat(SelfId);
                return;
            }
            var menu = MainMenu.Current;
            if (menu != null && menu.OnTitle)
                return;                                                   // (after the title: New game asks)
            if (AskedElsewhere)
            {
                if (FloatOpen(SelfId))
                    CloseFloat(SelfId);
                return;                                                   // (the opening asks, in the lender's mouth: Showtime)
            }
            if (!FloatOpen(SelfId))
                OpenSelf();
        }

        /// <summary>Picks in the creation window as a click does (a people, a faith or a past by id; null leaves it).</summary>
        public void PickSelf(string people, string faith, string past)
        {
            if (people != null && people != askPeople)
            {
                askPeople = people;
                askFaith = null;                                          // (the faiths offered are the people's)
            }
            if (faith != null)
                askFaith = faith;
            if (past != null)
                askPast = past;
            if (FloatOpen(SelfId))
                OpenSelf();
        }

        void OpenSelf()
        {
            var book = SelfBook.Current;
            var peoples = book.Offered(id => GameData.Port(id) != null);
            if (peoples.Count == 0 || book.pasts.Count == 0)
            {
                Self.Begin(null, null, null);                             // (nothing to offer: the plain start)
                return;
            }
            var people = peoples.Find(p => p.id == askPeople) ?? peoples.Find(p => p.id == Self.People) ?? peoples[0];
            askPeople = people.id;
            var faiths = book.FaithsFor(people);
            if (faiths.Count == 0)
            {
                Self.Begin(people.id, null, null);                        // (a book with no faith in it: nothing to ask of that)
                return;
            }
            var faith = faiths.Find(f => f.id == askFaith) ?? faiths[0];
            askFaith = faith.id;
            var past = book.Past(askPast) ?? book.pasts[0];
            askPast = past.id;
            MapPalette.SetPeople(people.id);                              // (the world in her people's colours, as she chooses)

            var content = new VisualElement();
            content.AddToClassList("frame-self");
            var chips = new List<(string id, string name)>();
            foreach (var p in peoples)
                chips.Add((p.id, p.name));
            content.Add(Group("Your people", chips, people.id, id => PickSelf(id, null, null), PeopleLine(people)));
            chips.Clear();
            foreach (var f in faiths)
                chips.Add((f.id, f.name));
            content.Add(Group("Your faith", chips, faith.id, id => PickSelf(null, id, null), FaithLine(faith)));
            chips.Clear();
            foreach (var p in book.pasts)
                chips.Add((p.id, p.name));
            content.Add(Group("Your past", chips, past.id, id => PickSelf(null, null, id), PastLine(book, past)));

            var spec = new FloatSpec
            {
                id = SelfId, title = "Who are you?", who = "Three choices before the first morning. They hold for the whole life.",
                face = "you", content = content, look = "frame-float-self", mustAnswer = true, holdsClock = true,
            };
            spec.options.Add(new FloatOption { label = $"Begin: {people.name}; {faith.name}; {past.name}", isDefault = true, pick = BeginSelf });
            OpenFloat(spec);
        }

        static VisualElement Group(string title, List<(string id, string name)> chips, string picked, Action<string> pick, string line)
        {
            var box = Box("frame-self-group");
            box.Add(FrameUi.H(title));
            var row = FrameUi.Row();
            foreach (var (id, name) in chips)
            {
                string to = id;
                var b = FrameUi.Btn(name, () => pick(to));
                b.EnableInClassList("on", id == picked);
                row.Add(b);
            }
            box.Add(row);
            var words = FrameUi.Text(line);
            words.AddToClassList("frame-self-line");
            box.Add(words);
            return box;
        }

        /// <summary>What a people changes, in one line: where she starts, the world's colours, the first tongue, what is hard.</summary>
        public static string PeopleLine(PeopleDef p)
        {
            if (p == null)
                return "";
            var home = GameData.Port(p.home);
            string palette = "";
            foreach (var pal in MapPalette.All)
                if (pal.people == p.id)
                    palette = pal.name;
            string s = home != null ? $"You start at {home.name}" : "You start at home";
            if (palette.Length > 0)
                s += $", and the world has the colours of the {palette} palette";
            s += ".";
            if (p.firstTongue.Length > 0)
                s += $" Your tongue is {TongueName(p.firstTongue)}.";
            string hard = FirstSentence(p.hard);
            return hard.Length > 0 ? s + " " + hard : s;
        }

        /// <summary>What a faith changes, in one line: what it asks and what it opens.</summary>
        public static string FaithLine(FaithDef f)
        {
            if (f == null)
                return "";
            string s = f.asks.Length > 0 ? "It asks: " + Stop(f.asks) : "";
            if (f.opens.Length > 0)
                s += (s.Length > 0 ? " " : "") + "It opens: " + Stop(f.opens);
            return s;
        }

        /// <summary>What a past changes, in one line: the stats it tilts, its gift, and the thing in the chest.</summary>
        public static string PastLine(SelfBook book, PastDef p)
        {
            if (p == null)
                return "";
            var tilt = new List<string>();
            foreach (string id in Self.StatIds)
                if (p.tilt.TryGetValue(id, out int v))
                    tilt.Add($"{book?.Stat(id)?.name ?? id} {v}");
            string s = tilt.Count > 0 ? string.Join(", ", tilt) + "." : "";
            if (p.gift.Length > 0)
                s += (s.Length > 0 ? " " : "") + Stop(FirstSentence(p.gift));
            if (p.chestThing.Length > 0)
                s += (s.Length > 0 ? " " : "") + "You keep: " + Stop(LowerFirst(p.chestThing));
            return s;
        }

        static string TongueName(string id) => Self.TongueName(id);

        static string FirstSentence(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            int end = text.IndexOf(". ", StringComparison.Ordinal);
            return end > 0 ? text.Substring(0, end + 1) : text;
        }

        static string Stop(string s) => string.IsNullOrEmpty(s) || s.EndsWith(".", StringComparison.Ordinal) || s.EndsWith("?", StringComparison.Ordinal) ? s : s + ".";
        static string LowerFirst(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1);

        /// <summary>The three choices are being asked somewhere else (the opening's talk with the lender): the creation
        /// window stays shut.</summary>
        public static bool AskedElsewhere;

        /// <summary>Begins with picks made elsewhere.</summary>
        public void BeginSelfAs(string people, string faith, string past)
        {
            askPeople = people;
            askFaith = faith;
            askPast = past;
            BeginSelf();
        }

        /// <summary>Begins with what the creation window has picked (its one button).</summary>
        public void BeginSelf()
        {
            Self.Begin(askPeople, askFaith, askPast);
            CloseFloat(SelfId);
            var book = SelfBook.Current;
            var people = book.People(Self.People);
            var faith = book.Faith(Self.Faith);
            var past = book.Past(Self.Past);
            if (past != null && past.giftFlag.Length > 0)
                ContentFlags.Set(past.giftFlag);                          // (the past's gift, where the cards read it as a flag)
            var world = ChartWorld.Current;
            if (people != null && world != null)
                world.StartAt(people.home);
            var home = GameData.Port(world != null ? world.homePort : null);
            var said = new List<string>();
            if (people != null)
                said.Add(people.name + (home != null ? ", of " + home.name : ""));
            if (faith != null)
                said.Add(faith.name);
            if (past != null)
                said.Add(past.name);
            FrameLog.Add("You begin. " + string.Join(". ", said) + ".");
            // (what she carries and whose it is, and what is owed and by when: nothing told a new player either)
            FrameLog.Add(FrameGame.AboardWords());
            if (Trade.Started && Trade.Debt > 0.01)
                FrameLog.Add($"You owe {FrameGame.CreditorName()} {Trade.Debt:0} d by {FrameGame.DebtDay(true)}.");
            nextFill = 0f;
        }
    }
}
