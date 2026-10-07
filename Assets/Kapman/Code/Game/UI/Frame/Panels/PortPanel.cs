using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// PORT - where she lies, and what can be done there: the port's own menu, which the ports' graph still makes (in the
    /// words of the port's people: talk to people, the place's errands, the orders posted), as lines of this window
    /// instead of a numbered menu in the middle of the screen. What a line leads to (the waterfront's talk, an errand's
    /// outcome) opens as a floating window. Two of the menu's own choices are not here, because the frame has them
    /// elsewhere: the market is the Trade window, and casting off is the function button's.
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
            return FrameUi.Mix(h, here != null ? (int)Reputation.At(here) : 0);
        }

        protected override void Fill(VisualElement body)
        {
            var here = FrameGame.PortHere;
            var text = TextPanel.Current;
            if (here == null || text == null)
            {
                body.Add(FrameUi.H("At sea"));
                body.Add(FrameUi.P("She lies at no port. When she moors, what can be done there shows here."));
                return;
            }
            body.Add(FrameUi.H(here.Title));
            body.Add(FrameUi.P(Reputation.Line(here)));
            if (FirstJobs.Here && FirstJobs.SailWorn)
            {
                // the first jobs: the worn sail, mended here for a little silver and some hours
                if (FirstJobs.Mending)
                    body.Add(FrameUi.Ln("The sail", "Ashore, being mended. Let the clock run: it is six hours' work."));
                else
                {
                    // (the user's own words, 8 Oct 2026: "Sails tattered.... / Needs to be repaired." Then the cost next to it:
                    // the cost is the button)
                    var mend = FrameUi.Btn($"{FirstJobs.MendCost:0} d", () =>
                    {
                        var game = KapmanGame.Current;
                        FirstJobs.Mend(game != null ? game.Day : 0.0, out string fact);
                        if (!string.IsNullOrEmpty(fact))
                            FrameLog.Add(fact);
                    });
                    mend.SetEnabled(FrameGame.Silver >= FirstJobs.MendCost);
                    mend.name = "mend-sail";                              // (Ravn points at it by this name)
                    mend.tooltip = $"Have the sails repaired: {FirstJobs.MendCost:0} d, and six hours' work.";
                    body.Add(FrameUi.Ln("Sails tattered....", "Needs to be repaired.", mend));
                }
            }
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
                if (key == "port.town" || key == "port.leave")
                    continue;                                             // (the Trade window; the Cast off button)
                int index = i;
                lines++;
                body.Add(FrameUi.LnButton(label, tip != label ? tip : null, () => text.Pick(index)));
            }
            if (lines == 0)
                body.Add(FrameUi.P("Nothing more to be done here but trade."));
            body.Add(FrameUi.P("The market is the Trade window. Casting off is the button above the windows."));
        }
    }
}
