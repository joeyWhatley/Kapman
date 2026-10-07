using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>The water she is on, as the 3D ship view shows it.</summary>
    public enum StagePlace { River, Coast, OpenSea, Port }

    /// <summary>
    /// WHAT THE 3D SHIP VIEW SHOWS (the user, 7 Oct 2026: "an immersive 3d view of the trader ship in a simulated
    /// environment based on the location of the ship on the map"). One plain record, filled once a frame by a source
    /// (the chart game: ChartStageSource; a test: any fake) and read by the stage (ShipStage). The stage knows nothing of
    /// the chart game, and the chart game knows nothing of the stage: this record is all that passes between them.
    /// All speeds are real metres a second, all bearings compass degrees (0 north, 90 east).
    /// </summary>
    public struct ShipStageState
    {
        // ---- where she is and what she is doing
        public StagePlace place;
        /// <summary>Made fast at a berth: the jetty lies alongside, the sail is furled.</summary>
        public bool moored;
        /// <summary>Coming in to a berth: the jetty is ahead, <see cref="berthDistance"/> metres off (the chart's game
        /// metres to the berth; one of them is one metre of the stage).</summary>
        public bool berthing;
        public float berthDistance;
        /// <summary>The chart's game metres to the berth she is bound for while it is within the source's look ahead (0: none
        /// set, or farther off): the stage puts that jetty in the mist ahead long before she is in the port's own reach
        /// (<see cref="berthing"/>), so it is never seen to appear. <see cref="portId"/> is then that port.</summary>
        public float berthAhead;
        /// <summary>The port she lies at or is coming in to ("" at sea).</summary>
        public string portId;
        /// <summary>Under oars (else, with way on and a sail, under sail).</summary>
        public bool underOars;
        /// <summary>Lying to for a gale: no sail, no oars, no way.</summary>
        public bool lyingTo;
        /// <summary>Her speed through the water, and the way her bow points.</summary>
        public float speed, heading;

        // ---- the weather where she is
        /// <summary>The true wind: the bearing it blows from, its strength and its gusts.</summary>
        public float windFrom, windSpeed, gust;
        /// <summary>Cloud cover 0..1; the share of it that is a grey layer; thunder cloud; rain and snow 0..1; fog 0..1.</summary>
        public float cloud, overcast, storm, rain, snow, fog;
        /// <summary>How far one sees, metres.</summary>
        public float visibility;
        /// <summary>The share of the Sun's light the cloud lets through, 0..1.</summary>
        public float cloudLight;

        // ---- the moment and the place on the globe
        /// <summary>The clock's Julian day (local mean time, as LevelClock.JulianDay), and the game's hours since its start.</summary>
        public double julianDay, hours;
        public double latitude, longitude;
        /// <summary>How fast the game runs: 0 paused, else the clock's speed (1 and up). The stage does NOT go faster with
        /// it (the user, 7 Oct 2026: "the view of the ship does not race at high speed: it keeps its own calm motion and
        /// lets the sky and light pass"): it only asks whether it is 0.</summary>
        public float timeFactor;

        // ---- the water round her
        /// <summary>Open water upwind (km) and the depth (m): what the wind has to raise a sea on.</summary>
        public float fetchKm, depth;

        // ---- the boat
        public string classId;
        public float length, beam, draught, closestToWind;
        public int crew, oars;
        public bool hasSail;

        /// <summary>Degrees off her bow the true wind comes from; + from starboard.</summary>
        public float WindOffBow => Mathf.DeltaAngle(heading, windFrom);
        public bool UnderWay => speed > 0.05f && !moored;
        /// <summary>Her speed as the stage shows the water, the wake and the jetty going by: her own, in real seconds,
        /// whatever the clock's speed; none while the game is paused.</summary>
        public float Way => timeFactor > 0f ? speed : 0f;
        /// <summary>The sail is set and drawing.</summary>
        public bool SailSet => hasSail && UnderWay && !underOars && !lyingTo;

        /// <summary>The Julian day of a date as the game's clock reckons it (LevelClock.JulianDay: the Julian calendar,
        /// day 172 of 793 is 21 June).</summary>
        public static double JulianDayOf(int year, int dayOfYear, double hour) =>
            1721423.5 + 365.0 * (year - 1) + System.Math.Floor((year - 1) / 4.0) + (dayOfYear - 1) + hour / 24.0;

        /// <summary>The Gislinge boat on the lower Rhine on a fair June noon, a light south-westerly: a start for a fake.</summary>
        public static ShipStageState Fair() => new ShipStageState
        {
            place = StagePlace.River, portId = "", speed = 2f, heading = 300f,
            windFrom = 230f, windSpeed = 5f, gust = 7f, cloud = 0.35f, visibility = 25000f, cloudLight = 0.8f,
            julianDay = JulianDayOf(793, 172, 12.0), hours = 12.0, latitude = 51.98, longitude = 5.33, timeFactor = 1f,
            fetchKm = 0.4f, depth = 3f,
            classId = "gislinge", length = 7.7f, beam = 1.5f, draught = 0.25f, closestToWind = 58f, crew = 4, oars = 6, hasSail = true,
        };
    }

    /// <summary>Whoever fills the record: the chart game, or a test.</summary>
    public interface IShipStageSource
    {
        /// <summary>Fills the record for this frame. False when there is nothing to show yet (no game, no ship).</summary>
        bool Read(ref ShipStageState state);
    }

    /// <summary>
    /// A LOOK of the stage (the user, 7 Oct 2026: "create a few different shaders, make it psx style in a few different
    /// ways"). The other builder's part plugs in here: a look may change the picture's height in pixels and its filter
    /// (ShipStage.View), swap the materials of everything on the stage (ShipStage.Remat), add a custom pass or a volume
    /// for the stage's camera (ShipStage.View.Camera, ShipStage.Volume), and read the waves from the shader globals
    /// StageSea sets. Remove puts back what Apply changed.
    /// </summary>
    public interface IShipStageLook
    {
        string Name { get; }
        void Apply(ShipStage stage);
        void Remove(ShipStage stage);
    }

    /// <summary>
    /// THE CREW ABOARD (the other builder's part: the pack's men, seated and rowing). The stage gives it the ship with
    /// her seats and oars (StageShip.Seats, HelmSeat, OarsPort, OarsStarboard) and, each frame, the state and the stroke
    /// (StageShip.StrokePhase: 0 the catch, 0.4 the release, 1 the next catch). A rig that moves the oars itself sets
    /// StageShip.OarsDriven and the stage leaves them alone.
    /// </summary>
    public interface IShipCrewRig
    {
        void Attach(ShipStage stage, StageShip ship);
        void Tick(in ShipStageState state, float dt);
        void Detach();
    }

    /// <summary>A crew rig or a look that eases things over seconds (oars coming out, a stroke building) and can show
    /// them at once as if the state had long been so: ShipStage.Settle calls it (a test's picture, a loaded game).</summary>
    public interface IShipStageSettles
    {
        void Settle();
    }

    /// <summary>A look that makes the picture itself (a low picture with hard pixels, its colours cut down): the stage
    /// calls Draw once a step in place of its own drawing (ShipStageView.Render), after everything on it has moved.</summary>
    public interface IShipStageLookDraws
    {
        void Draw(ShipStage stage, float dt);
    }
}
