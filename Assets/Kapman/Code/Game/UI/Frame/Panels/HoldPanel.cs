using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// HOLD - what she carries: each good aboard with how much of it, what it weighs and what it cost; the room taken and
    /// the room left; and the crew's stores as days. All from the merchant's hold (Trade.Hold) and the voyage's stores.
    /// </summary>
    public sealed class HoldPanel : FramePanel
    {
        public override string Id => "hold";
        public override string Title => "Hold";
        public override int Order => 30;

        protected override int Stamp()
        {
            int h = FrameGame.HoldStamp();
            if (Trade.Started)
            {
                h = FrameUi.Mix(h, (int)Trade.Hold.capacity);
                foreach (var lot in Trade.Hold.Lots)
                    h = FrameUi.Mix(FrameUi.Mix(h, lot.whose), (int)lot.cost);   // (whose it is and what it cost are written on the line)
            }
            if (Voyage.Started)
                h = FrameUi.Mix(FrameUi.Mix(h, (int)(Voyage.FoodDays * 4.0)), (int)(Voyage.DrinkDays * 4.0));
            return h;
        }

        protected override void Fill(VisualElement body)
        {
            // (the sections of Game/UI/Panels/hold.uxml)
            var room = Section("room");
            if (!Trade.Started)
            {
                room.Add(FrameUi.P("There is no hold to look into yet."));
                return;
            }
            var hold = Trade.Hold;
            room.Add(FrameUi.H($"{hold.Used:0} of {hold.capacity:0} kg of room taken"));
            var goods = Section("goods");
            int kinds = 0;
            foreach (var (good, count) in hold.Contents)
            {
                kinds++;
                goods.Add(FrameUi.Ln(FrameGame.Units(good, count), FrameGame.CostWords(hold, good, count), FrameUi.Num($"{count * good.kg:0} kg")));
            }
            if (kinds == 0)
                goods.Add(FrameUi.P("She is empty."));
            Section("room-left").Add(FrameUi.P($"Room left: {hold.Free:0} kg. Light, bulky goods take more room than they weigh."));
            if (!Voyage.Started)
                return;
            var stores = Section("stores");
            stores.Add(FrameUi.H("Stores"));
            stores.Add(FrameUi.Ln("Bread and other food", $"{Voyage.FoodKg:0} kg for {(Voyage.Men == 1 ? "one man" : Voyage.Men + " men")}", FrameUi.Num(Days(Voyage.FoodDays))));
            stores.Add(FrameUi.Ln("Ale and water", $"{Voyage.DrinkL:0} litres", FrameUi.Num(Days(Voyage.DrinkDays))));
        }

        static string Days(double days) => days < 0.25 ? "none" : FrameUi.Days(days);

    }
}
