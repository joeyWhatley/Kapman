using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// CHEST - what is in the chest, as a list: each text, verse and riddle with who it came from, a riddle with whether
    /// it is solved, your own notes as a count, and the thing kept from your past. The chest itself, where a text is
    /// read and a riddle tried, still opens over the chart on [I]; this window is the list at a glance.
    /// </summary>
    public sealed class ChestFramePanel : FramePanel
    {
        public override string Id => "chest";
        public override string Title => "Chest";
        public override int Order => 94;

        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(17, Chest.Version), Chest.Items.Count), Self.Version);

        protected override void Fill(VisualElement body)
        {
            int shown = 0;
            var past = Self.Chosen ? SelfBook.Current.Past(Self.Past) : null;
            if (past != null && past.chestThing.Length > 0)
            {
                body.Add(FrameUi.Ln(past.chestThing, past.chestWhat.Length > 0 ? past.chestWhat : "Kept from before.", FrameUi.Tag("yours")));
                shown++;
            }
            foreach (var item in Chest.Items)
            {
                var text = item.Text;
                string title = text != null && !string.IsNullOrEmpty(text.title) ? text.title : item.kind == "riddle" ? "A riddle" : "A " + item.kind;
                string from = string.IsNullOrEmpty(item.from) ? "" : "From " + item.from + ".";
                string tag = item.kind == "riddle" ? (item.solved ? "solved" : item.resting ? "resting" : "open") : item.kind;
                body.Add(FrameUi.Ln(title, from.Length > 0 ? from : null, FrameUi.Tag(tag)));
                shown++;
            }
            if (Chest.Notes.Count > 0)
            {
                body.Add(FrameUi.Ln("Your notes", null, FrameUi.Num(Chest.Notes.Count.ToString())));
                shown++;
            }
            if (shown == 0)
                body.Add(FrameUi.P("The chest is empty."));
            body.Add(FrameUi.P("To read a text or try a riddle, open the chest: press I."));
        }
    }
}
