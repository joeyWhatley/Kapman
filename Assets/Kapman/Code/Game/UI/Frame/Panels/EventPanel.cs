using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// EVENT - the card in play, as a window that stays: the same card the floating window shows (who speaks, the words,
    /// the choices with their odds as a word, what came of it), so it can be read and answered here too. With no card it
    /// says so, and whether something waits its turn.
    /// </summary>
    public sealed class EventPanel : FramePanel
    {
        public override string Id => "event";
        public override string Title => "Event";
        public override int Order => 70;

        protected override int Stamp()
        {
            var d = EncounterRunner.Director;
            if (d == null)
                return 0;
            var s = d.Current;
            int h = FrameUi.Mix(17, s != null ? s.card.id : "");
            h = FrameUi.Mix(h, s != null && s.Chosen);
            h = FrameUi.Mix(h, d.Waiting);
            return FrameUi.Mix(h, d.state.callPort);
        }

        protected override void Fill(VisualElement body)
        {
            var d = EncounterRunner.Director;
            var s = d != null ? d.Current : null;
            if (s == null)
            {
                body.Add(FrameUi.H("Nothing is asked of you now"));
                if (d == null)
                    body.Add(FrameUi.P("The cards are not read yet."));
                else if (d.Waiting)
                    body.Add(FrameUi.P("Something waits its turn."));
                else
                {
                    var port = GameData.Port(d.state.callPort);
                    body.Add(FrameUi.P(port != null ? $"She lies at {port.name}. Who comes down to the boat, and what the day brings, shows here and over the chart."
                                                    : "At sea. What the watch brings shows here and over the chart."));
                }
                return;
            }
            var spec = FrameRoot.CardSpec(d, s);
            body.Add(FrameUi.H(spec.title));
            if (spec.who.Length > 0)
                body.Add(FrameUi.P(spec.who));
            foreach (var (text, written) in spec.lines)
                body.Add(written ? FrameUi.Text(text) : FrameUi.P(text));
            if (spec.result.Length > 0)
                body.Add(FrameUi.Text(spec.result));
            if (spec.small.Length > 0)
                body.Add(FrameUi.P(spec.small));
            for (int i = 0; i < spec.options.Count; i++)
            {
                var o = spec.options[i];
                var line = FrameUi.LnButton((i + 1) + ". " + (string.IsNullOrEmpty(o.tag) ? "" : "[" + o.tag + "] ") + o.label, o.enabled ? null : o.rule, () => o.pick?.Invoke(),
                                            string.IsNullOrEmpty(o.odds) ? null : FrameUi.Tag(o.odds));
                line.SetEnabled(o.enabled);
                body.Add(line);
            }
            if (spec.mustAnswer)
                body.Add(FrameUi.P("It waits for your answer. The clock is held."));
        }
    }
}
