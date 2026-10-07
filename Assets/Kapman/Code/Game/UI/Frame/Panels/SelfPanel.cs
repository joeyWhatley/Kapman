using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// YOURSELF - who the player is: people and home, faith, past, and the five stats as pips with the word for each
    /// level; what the past left (the thing kept, the gift). All from the self (Self) and its tables (SelfBook).
    /// </summary>
    public sealed class SelfPanel : FramePanel
    {
        public override string Id => "self";
        public override string Title => "Yourself";
        public override int Order => 80;

        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(17, Self.Version), ChartWorld.Current != null ? ChartWorld.Current.homePort : "");

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
            foreach (string id in Self.StatIds)
            {
                var stat = book.Stat(id);
                int pips = Self.Stat(id);
                string name = stat != null ? stat.name : id;
                string level = stat != null ? stat.Level(pips) : "";
                body.Add(FrameUi.Ln(name, stat != null ? stat.what : null, level.Length > 0 ? FrameUi.Tag(level) : null, FrameUi.Pips(pips)));
            }
            body.Add(FrameUi.P("A stat rises by use, never by spending: each check you win by it adds a little, more against someone who resisted."));
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
