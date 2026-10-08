using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// SHIP - the boat she is: her kind, her size, what she draws and carries, how she goes under sail and under oars,
    /// from her class in the tables. Her wear, her fittings and a yard to mend her at are not in the game yet: the panel
    /// says so.
    /// </summary>
    public sealed class ShipPanel : FramePanel
    {
        public override string Id => "ship";
        public override string Title => "Ship";
        public override int Order => 90;

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(17, game != null && game.ship != null && game.ship.shipClass != null ? game.ship.shipClass.shipName : "");
            return FrameUi.Mix(h, Trade.Started ? (int)Trade.Hold.capacity : 0);
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            var ship = game != null ? game.ship : null;
            if (ship == null || ship.shipClass == null)
            {
                body.Add(FrameUi.P("There is no ship to look over yet."));
                return;
            }
            var c = ship.shipClass;
            body.Add(FrameUi.H("The " + c.shipName));
            if (!string.IsNullOrEmpty(c.about))
                body.Add(FrameUi.Text(c.about));
            body.Add(FrameUi.Ln("Length and beam", null, FrameUi.Num($"{c.length:0.#} m by {c.beam:0.#} m")));
            body.Add(FrameUi.Ln("Draws", "The water she needs under her, loaded.", FrameUi.Num($"{c.draught:0.##} m")));
            if (Trade.Started)
                body.Add(FrameUi.Ln("Carries", "Room for cargo, the water casks taken off.", FrameUi.Num($"{Trade.Hold.capacity:0} kg")));
            var cls = game.World != null && ChartBody.Current != null ? game.World.ClassOf(ChartBody.Current.classId) : null;
            if (cls != null)
            {
                if (cls.hasSail)
                    body.Add(FrameUi.Ln("Under sail", "In a fair wind.", FrameUi.Num($"{cls.sailKn:0.#} knots")));
                if (cls.oarsKn > 0f)
                    body.Add(FrameUi.Ln("Under oars", null, FrameUi.Num($"{cls.oarsKn:0.#} knots")));
                if (cls.crew > 0f)
                    body.Add(FrameUi.Ln("Hands she wants", null, FrameUi.Num($"{cls.crew:0}")));
            }
        }
    }
}
