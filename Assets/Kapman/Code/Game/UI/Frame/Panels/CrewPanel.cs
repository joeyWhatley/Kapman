using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// CREW - the men aboard as the game knows them today: how many, their health and their heart as words, what they
    /// are owed, how long they have been at sea. They are not yet people with names: the panel says so.
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
            h = FrameUi.Mix(h, (int)Voyage.PayOwed);
            return FrameUi.Mix(h, Voyage.DaysAtSea);
        }

        protected override void Fill(VisualElement body)
        {
            if (!Voyage.Started)
            {
                body.Add(FrameUi.P("There is no crew to speak of yet."));
                return;
            }
            body.Add(FrameUi.H(Voyage.Men == 1 ? "One man" : Voyage.Men + " men"));
            body.Add(FrameUi.Ln("Health", null, FrameUi.Tag(FrameGame.HealthWord(Voyage.Health))));
            body.Add(FrameUi.Ln("Heart", null, FrameUi.Tag(FrameGame.HeartWord(Voyage.Morale))));
            body.Add(FrameUi.Ln("Pay owed", null, FrameUi.Num($"{Voyage.PayOwed:0} d")));
            body.Add(FrameUi.Ln("At sea", null, FrameUi.Num(Voyage.DaysAtSea <= 0 ? "not yet" : Voyage.DaysAtSea == 1 ? "1 day" : Voyage.DaysAtSea + " days")));
            body.Add(FrameUi.Ln("Bread and other food", null, FrameUi.Num(Voyage.FoodDays < 0.25 ? "none" : FrameUi.Days(Voyage.FoodDays))));
            body.Add(FrameUi.Ln("Ale and water", null, FrameUi.Num(Voyage.DrinkDays < 0.25 ? "none" : FrameUi.Days(Voyage.DrinkDays))));
            body.Add(FrameUi.P("The crew are not yet people with names, tongues and faiths of their own: that comes with hiring."));
        }
    }
}
