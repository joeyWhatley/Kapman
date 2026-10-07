// PROJECT PATH (later): Assets/Kapman/Code/Game/ShipView/ShipCrewFeed.cs      assembly: Kapman.Game
// DRAFT, 7 Oct 2026. Compile-checked outside Unity against the game's own classes (not run).
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// WHAT THE CREW IS TOLD, read from the chart game each frame: the same things FrameRoot.FillShip reads for the drawn
    /// stand-in (FrameShipView.Look), and her class's own figures for sail against oars, worked as Travel.Speed works
    /// them, so the window shows oars exactly when the game is moving her by oars.
    /// The stage calls:  crew.inputs = ShipCrewFeed.Read(Time.deltaTime);
    /// </summary>
    public static class ShipCrewFeed
    {
        static float lastHeading;
        static bool hasHeading;

        public static CrewInputs Read(float dt)
        {
            var i = new CrewInputs { closestToWind = 58f, oarKnots = 2.5f };
            var game = KapmanGame.Current;
            if (game == null || game.ship == null)
            {
                i.moored = true;
                return i;
            }
            var body = ChartBody.Current;
            float heading = game.ship.Heading;
            i.moored = game.ship.anchored;
            i.onRiver = ChartWorld.Current != null && ChartWorld.OnRiver();
            if (body != null)
            {
                i.lyingTo = body.Sheltering;
                i.knots = body.Knots;
                i.wind = body.Wind;
                i.windOffBow = Mathf.DeltaAngle(heading, body.WindFrom);
            }
            // off the berth and onto it under oars: within twice a port's reach of the nearest berth (60 game metres:
            // about half a minute of rowing at 1x)
            var port = Port.Nearest(game.ShipPosition);
            i.nearPort = !i.moored && port != null && port.DistanceTo(game.ShipPosition) < port.arrivalRadius * 2f;

            // sail against oars, as Travel.Speed has it
            var cls = game.World != null && body != null ? game.World.ClassOf(body.classId) : null;
            if (cls != null)
            {
                i.closestToWind = cls.closestToWind;
                i.oarKnots = cls.oarsKn * Mathf.Min(1f, cls.rowingHours / 12f);
                i.sailKnots = 0f;
                if (cls.hasSail && i.wind > 1.5f)
                {
                    double twa = Mathf.Abs(i.windOffBow);
                    double through = cls.sailKn * cls.polar * System.Math.Pow(i.wind / 8.0, 0.6);
                    double sail = twa >= cls.closestToWind
                        ? through * Travel.Polar(twa, cls.closestToWind)
                        : through * Travel.Polar(cls.closestToWind, cls.closestToWind) * System.Math.Cos(cls.closestToWind * System.Math.PI / 180.0);
                    i.sailKnots = (float)System.Math.Min(sail, cls.maxKn);
                }
            }
            else
                i.sailKnots = i.wind > 1.5f ? 4f : 0f;

            // how fast she is turning, for the steering oar
            if (hasHeading && dt > 1e-5f)
                i.turning = Mathf.DeltaAngle(lastHeading, heading) / dt;
            lastHeading = heading;
            hasHeading = true;
            return i;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            hasHeading = false;
        }
    }
}
