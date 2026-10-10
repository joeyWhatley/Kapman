using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// PORT - where she lies, and what can be done there: the port's own menu, which the ports' graph still makes (in the
    /// words of the port's people: talk to people, the place's errands, the orders posted), as lines of this window
    /// instead of a numbered menu in the middle of the screen. What a line leads to (the waterfront's talk, an errand's
    /// outcome) opens as a floating window. Two of the menu's own choices are not here, because the frame has them
    /// elsewhere: the market is the Trade window, and casting off is the function button's. (The worn sail of the first
    /// morning was a line here until 8 Oct 2026: it is the first row of the list under the buttons now, FrameRoot.Jobs.cs.)
    /// </summary>
    public sealed class PortPanel : FramePanel
    {
        public override string Id => "port";
        public override string Title => "Port";
        public override int Order => 25;

        protected override int Stamp()
        {
            var text = TextPanel.Current;
            var here = FrameGame.PortHere;
            int h = FrameUi.Mix(17, here != null ? here.id : "");
            h = FrameUi.Mix(h, text != null ? text.Version : 0);
            h = FrameUi.Mix(FrameUi.Mix(h, FirstJobs.Version), (int)FrameGame.Silver);
            h = FrameUi.Mix(h, EncounterRunner.Director != null ? EncounterRunner.Director.AsBeforeVersion : 0);
            return FrameUi.Mix(h, here != null ? (int)Reputation.At(here) : 0);
        }

        protected override void Fill(VisualElement body)
        {
            var here = FrameGame.PortHere;
            var text = TextPanel.Current;
            if (here == null || text == null)
            {
                body.Add(FrameUi.H("At sea"));
                body.Add(FrameUi.P("She lies at no port."));
                return;
            }
            body.Add(FrameUi.H(here.Title));
            body.Add(FrameUi.P(Reputation.Line(here)));
            if (!FrameGame.PortMenuOpen)
            {
                body.Add(FrameUi.P("Finish what is in front of you first."));
                return;
            }
            if (!string.IsNullOrEmpty(text.StoryText))
                body.Add(FrameUi.Text(text.StoryText));
            if (!string.IsNullOrEmpty(text.IntentionText))
                body.Add(FrameUi.P(text.IntentionText));
            int lines = 0;
            for (int i = 0; i < text.Options.Count; i++)
            {
                var (key, label, tip) = text.Options[i];
                if (key == "port.town" || key == "port.leave" || !PortLife.Built(key))
                    continue;                                             // (the Trade window; the Cast off button; an errand not built has no row)
                int index = i;
                lines++;
                body.Add(FrameUi.LnButton(label, tip != label ? tip : null, () => text.Pick(index)));
            }
            // AS SHE ANSWERED THE LAST TIME (the user's ruling of 10 Oct 2026, of the king's toll): what went by itself at
            // this call, as it went the last time, can be had again, once, and answered by her own hand. PLACEHOLDER WORDS.
            var deck = EncounterRunner.Director;
            if (deck != null)
                foreach (var (id, title) in deck.AsBeforeThisCall)
                {
                    string card = id;
                    lines++;
                    body.Add(FrameUi.LnButton(title, "It went as it went the last time. Press this to answer it yourself.", () => deck.AskAgain(card)));
                }
            if (lines == 0)
                body.Add(FrameUi.P("Nothing more to be done here but trade."));
        }
    }
}
