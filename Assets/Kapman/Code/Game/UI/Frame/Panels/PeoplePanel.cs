using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// PEOPLE HERE - the people of the port she lies at who are there this year, each with what they are and where they
    /// are found, and a Talk button that opens their lines in a floating window (PersonTalk); and the places of the port
    /// that stand this year, each with what is seen there. From the places files (PlaceBook). At sea, and at a port
    /// nothing is written of yet, it says so.
    /// </summary>
    public sealed class PeoplePanel : FramePanel
    {
        public override string Id => "people";
        public override string Title => "People here";
        public override int Order => 27;

        protected override int Stamp()
        {
            var here = FrameGame.PortHere;
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(17, here != null ? here.id : "");
            h = FrameUi.Mix(h, game != null ? game.Year : 0);
            h = FrameUi.Mix(h, ContentFiles.Version);
            h = FrameUi.Mix(FrameUi.Mix(h, FirstJobs.Version), (int)FrameGame.Silver);
            return FrameUi.Mix(h, Self.Version);
        }

        /// <summary>The first jobs' hands for hire, on the home quay, while she still wants men (FirstJobs).</summary>
        void Hands(VisualElement body)
        {
            if (!FirstJobs.Here || FirstJobs.HandsShort <= 0)
                return;
            int n = FirstJobs.HandsShort;
            body.Add(FrameUi.P($"Shipmen looking for a boat. She wants {(n == 1 ? "one" : n == 2 ? "two" : n == 3 ? "three" : n.ToString())} more. The hiring silver is {FirstJobs.HandMoney:0} d a man."));
            for (int i = 0; i < FirstJobs.Hands.Count; i++)
            {
                var hand = FirstJobs.Hands[i];
                if (hand.hired)
                    continue;
                int index = i;
                var hire = FrameUi.Btn("Hire", () =>
                {
                    FirstJobs.Hire(index, out string fact);
                    if (!string.IsNullOrEmpty(fact))
                        FrameLog.Add(fact);
                });
                hire.SetEnabled(FrameGame.Silver >= FirstJobs.HandMoney);
                body.Add(FrameUi.Ln(hand.name, hand.line, hire));
            }
        }

        protected override void Fill(VisualElement body)
        {
            var here = FrameGame.PortHere;
            if (here == null)
            {
                body.Add(FrameUi.H("At sea"));
                body.Add(FrameUi.P("She lies at no port. When she moors, the people of the place show here."));
                return;
            }
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            var book = PlaceBook.Current;
            body.Add(FrameUi.H(here.name));
            Hands(body);
            if (book.Place(here.id) == null)
            {
                body.Add(FrameUi.P("Nothing is written of the people of this place yet."));
                return;
            }
            string era = book.Era(here.id, year);
            var people = book.People(here.id, year);
            if (people.Count == 0)
                body.Add(FrameUi.P("Nobody here is written of for this year."));
            foreach (var p in people)
            {
                var person = p;
                var met = Self.Met(p.id);
                string small = PersonTalk.Where(p, book.SiteOf(p, year)) + (met != null ? ". You have spoken." : "");
                body.Add(FrameUi.Ln(p.name.Length > 0 ? p.name : "The " + p.Role, small, FrameUi.Btn("Talk", () => PersonTalk.Open(Frame, person))));
            }
            var sites = book.Sites(here.id, year);
            if (sites.Count > 0)
                body.Add(FrameUi.H("Places"));
            foreach (var s in sites)
                body.Add(FrameUi.Ln(s.name, First(s.see)));
            // (the times, last: the people are what the window is for, and it is often a short one)
            if (era.Length > 0)
            {
                body.Add(FrameUi.H("These years"));
                body.Add(FrameUi.P(era));
            }
        }

        /// <summary>The first sentence of a longer text.</summary>
        static string First(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            int end = text.IndexOf(". ", System.StringComparison.Ordinal);
            return end > 0 ? text.Substring(0, end + 1) : text;
        }
    }
}
