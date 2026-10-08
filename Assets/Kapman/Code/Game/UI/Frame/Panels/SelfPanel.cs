using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// YOURSELF - who the player is: people and home, faith, past, and the five stats as pips with the word for each
    /// level; what the past left (the thing kept, the gift). All from the self (Self) and its tables (SelfBook).
    /// </summary>
    public sealed class SelfPanel : FramePanel
    {
        /// <summary>A mark as its line under the stat: "+1 you fought raiders off (for 12 days)".</summary>
        public static string MarkLine(Self.StatMark m, double today)
        {
            string size = (m.size > 0 ? "+" : "\u2212") + System.Math.Abs(m.size);
            string why = m.why.Length > 0 ? " " + m.why : "";
            return $"{size}{why} ({Self.Lasts(m, today)})";
        }

        public override string Id => "self";
        public override string Title => "Yourself";
        public override int Order => 80;

        // (the day is in the stamp: a mark's days run down, and one that ends changes the pips)
        protected override int Stamp() =>
            FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(17, Self.Version), ChartWorld.Current != null ? ChartWorld.Current.homePort : ""), (int)System.Math.Floor(Self.Now));

        protected override void Fill(VisualElement body)
        {
            var book = SelfBook.Current;
            if (!Self.Chosen)
            {
                body.Add(FrameUi.H("Not yet chosen"));
                body.Add(FrameUi.P("A new game asks who you are: your people, your faith, your past."));
            }
            else
            {
                var people = book.People(Self.People);
                var faith = book.Faith(Self.Faith);
                var past = book.Past(Self.Past);
                var home = GameData.Port(people != null ? people.home : null);
                body.Add(FrameUi.H((people != null ? people.name : Self.People) + (home != null ? ", of " + home.name : "")));
                string line = faith != null ? faith.name + "." : "";
                if (past != null)
                    line += (line.Length > 0 ? " " : "") + past.name + ".";
                if (line.Length > 0)
                    body.Add(FrameUi.Text(line));
            }
            // EVENTS MOVE THE STATS (the user, 7 Oct 2026): the pips are the stat as it is now. Under a stat stands each
            // mark that lies on it: its size, its cause, how long it has yet.
            double today = Self.Now;
            bool anyMark = false;
            foreach (string id in Self.StatIds)
            {
                var stat = book.Stat(id);
                int pips = Self.StatNow(id, today);
                string name = stat != null ? stat.name : id;
                string level = stat != null ? stat.Level(pips) : "";
                body.Add(FrameUi.Ln(name, stat != null ? stat.what : null, level.Length > 0 ? FrameUi.Tag(level) : null, FrameUi.Pips(pips)));
                foreach (var m in Self.MarksOn(id, today))
                {
                    anyMark = true;
                    var line = FrameUi.P(MarkLine(m, today));
                    line.AddToClassList("frame-mark");
                    line.AddToClassList(m.size > 0 ? "up" : "down");
                    body.Add(line);
                }
                if (Self.Capped(id, today))
                    body.Add(FrameUi.P("More than two up or down such things cannot move it. The rest waits until one ends."));
            }
            body.Add(FrameUi.P(anyMark
                ? "These grow for good by use: each time one carries the day it grows a little. What you do and what befalls you lies on them for a while."
                : "These grow by use, never by silver: each time one carries the day it grows a little, and more when someone stood against you. What you do and what befalls you can lie on them for a while."));
            var was = Self.Chosen ? book.Past(Self.Past) : null;
            if (was == null)
                return;
            body.Add(FrameUi.H("From before"));
            if (was.chestThing.Length > 0)
                body.Add(FrameUi.Ln(was.chestThing, was.chestWhat.Length > 0 ? was.chestWhat : null));
            // (the first sentence of the gift: what follows it in the data is a note on the rules, for the builders)
            string gift = was.gift;
            int stop = gift.IndexOf(". ", System.StringComparison.Ordinal);
            if (gift.Length > 0)
                body.Add(FrameUi.P(stop > 0 ? gift.Substring(0, stop + 1) : gift));
            if (was.knowsWhy.Length > 0)
                body.Add(FrameUi.P("Someone knows you: " + was.knowsWhy));
        }
    }
}
