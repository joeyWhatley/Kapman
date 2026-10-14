using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// CREW - the men aboard as the game knows them today: how many, their health and their heart as words, how weary
    /// they are and who is at the bailing (Voyage/Boat.cs), what they are paid by and what they are owed, how long they
    /// have been at sea, and the stores.
    /// </summary>
    public sealed class CrewPanel : FramePanel
    {
        public override string Id => "crew";
        public override string Title => "Crew";
        public override int Order => 35;

        protected override int Stamp()
        {
            int h = FrameUi.Mix(17, Voyage.Started);
            h = FrameUi.Mix(h, Voyage.Men);
            h = FrameUi.Mix(h, (int)(Voyage.Health * 20));
            h = FrameUi.Mix(h, (int)(Voyage.Morale * 20));
            h = FrameUi.Mix(h, Feud.Version);
            h = FrameUi.Mix(h, (int)Voyage.PayOwed);
            h = FrameUi.Mix(h, Boat.WearyWord);
            h = FrameUi.Mix(h, Boat.Bailers + (Boat.AllHands ? 10 : 0) + (Boat.OnShares ? 100 : 0) + (Boat.Bundles ? 1000 : 0));
            h = FrameUi.Mix(h, (int)(Voyage.FoodDays * 2.0) * 100 + (int)(Voyage.DrinkDays * 2.0));
            h = FrameUi.Mix(h, FrameGame.PortHere != null ? FrameGame.PortHere.id : "");
            return FrameUi.Mix(h, Voyage.DaysAtSea);
        }

        protected override void Fill(VisualElement body)
        {
            // (the sections of Game/UI/Panels/crew.uxml)
            var men = Section("men");
            if (!Voyage.Started)
            {
                men.Add(FrameUi.P("There is no crew to speak of yet."));
                return;
            }
            men.Add(FrameUi.H(Voyage.Men == 1 ? "One man" : Voyage.Men + " men"));
            var state = Section("state");
            state.Add(FrameUi.Ln("Health", null, FrameUi.Tag(FrameGame.HealthWord(Voyage.Health))));
            // (9 Oct 2026: while Ravn lies unanswered their heart has a ceiling: Voyage/Feud.cs)
            state.Add(FrameUi.Ln("Heart", Feud.Unanswered ? "They will not be in good heart while Ravn lies unanswered." : null,
                                 FrameUi.Tag(FrameGame.HeartWord(Voyage.Morale))));
            // (the alpha's row 4: weary men row slower, and the sea finds a weary boat sooner)
            state.Add(FrameUi.Ln("Weariness", "Oars, bailing and nights under way tire them; a mooring rests them. Weary men row slower, and the sea finds a weary boat sooner.",
                                 FrameUi.Tag(Boat.WearyWord)));
            if (Boat.Bailers > 0)
                state.Add(FrameUi.Ln("At the bailing", "Men bailing are not at the oars.",
                                     FrameUi.Tag(Boat.AllHands ? "all hands" : Boat.Bailers == 1 ? "one man" : "two men")));
            var pay = Section("pay");
            pay.Add(FrameUi.Ln("Paid by", Boat.OnShares ? "Half the wage each day, and a tenth of the season's gain at Martinmas."
                                                        : "A wage each day aboard, paid out in port.",
                               FrameUi.Tag(Boat.OnShares ? "shares" : "the wage")));
            if (Boat.Bundles)
                pay.Add(FrameUi.Ln("Their own bundles", "Each man carries goods of his own: it comes out of the hold's room.",
                                   FrameUi.Num($"{Boat.R.bundleKgPerMan * Voyage.Men:0} kg")));
            pay.Add(FrameUi.Ln("Pay owed", null, FrameUi.Num($"{Voyage.PayOwed:0} d")));
            var sea = Section("sea");
            // (made fast she is in port, whatever is left of the men's sea legs: the count runs down ashore)
            sea.Add(FrameUi.Ln("At sea", null, FrameUi.Num(FrameGame.PortHere != null || Voyage.DaysAtSea <= 0 ? "still in port"
                                                           : Voyage.DaysAtSea == 1 ? "1 day" : Voyage.DaysAtSea + " days")));
            sea.Add(FrameUi.Ln("Bread and other food", null, FrameUi.Num(Voyage.FoodDays < 0.25 ? "none" : FrameUi.Days(Voyage.FoodDays))));
            sea.Add(FrameUi.Ln("Ale and water", null, FrameUi.Num(Voyage.DrinkDays < 0.25 ? "none" : FrameUi.Days(Voyage.DrinkDays))));
        }
    }
}
