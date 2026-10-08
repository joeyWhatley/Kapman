using Kapman.Game.Sim;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE CHART GAME AS THE 3D VIEW READS IT. Every value and where it comes from today:
    ///   moored            Port.LyingId()                      (anchored inside a port's reach)
    ///   berthing          Course.Destination's Port, Port.DistanceTo(ship) under <see cref="berthWithin"/>
    ///   heading, speed    SailingShip.Heading, SailingShip.Speed   (ChartBody publishes them through ShipMotion)
    ///   lying to          ChartBody.Sheltering                (Travel.Speed: wind over 17 m/s)
    ///   sail or oars      NOT IN THE CHART GAME. Worked out here by Travel.Speed's own rule: she rows when her class's
    ///                     oar speed is what Travel.Speed gave (the sail made no more)
    ///   weather           ChartWorld.TryWeather -> WorldSim.WeatherAt (the model her speed is worked out from)
    ///   date, hour        LevelClock.Current.JulianDay / GameHours / TimeFactor
    ///   latitude          WorldFrame.ToLatLon(ChartWorld.ShipWorld())
    ///   place             under way: Course.Kinds at Course.Next (the lane's kind); lying still: ChartWorld.OnRiver(),
    ///                     else the coast within <see cref="coastWithin"/> game metres of a port, else the open sea
    ///   fetch, depth      NOT IN THE CHART GAME. Four pairs by the kind of place (below) until the lanes carry them
    ///   her class         WorldSim.ClassOf(ChartBody.classId): length, beam, draught, crew, sail. The class row's "oars"
    ///                     (6 for the Gislinge boat) is not read into ShipClassDef: <see cref="oars"/> here
    /// </summary>
    public sealed class ChartStageSource : IShipStageSource
    {
        /// <summary>Game metres from the berth she is bound for inside which the jetty is in sight ahead.</summary>
        public float berthWithin = 120f;
        /// <summary>Game metres from the berth she is bound for inside which the stage is told how far off it is (it puts the
        /// jetty in the mist ahead from as far as its mist lets one see: ShipStageKit.mist).</summary>
        public float aheadWithin = 2000f;
        /// <summary>Game metres from the nearest port inside which open water counts as the coast (250 is 15 km).</summary>
        public float coastWithin = 250f;
        /// <summary>Her oars (the class table's row says 6 for the Gislinge boat; ShipClassDef does not carry it).</summary>
        public int oars = 6;
        /// <summary>Open water upwind (km) and depth (m) by place: river, coast, open sea, port.</summary>
        public Vector2 riverWater = new Vector2(0.4f, 3f), coastWater = new Vector2(40f, 15f), seaWater = new Vector2(300f, 40f), portWater = new Vector2(0.8f, 3f);

        public bool Read(ref ShipStageState s)
        {
            var game = KapmanGame.Current;
            var clock = LevelClock.Current;
            if (game == null || game.ship == null || clock == null || ChartWorld.Current == null)
                return false;
            var ship = game.ship;
            var body = ChartBody.Current;
            Vector3 at = game.ShipPosition;

            // the moment and the place on the globe
            s.julianDay = clock.JulianDay;
            s.hours = clock.GameHours;
            // (the chart game steps its own clock, GameSpeed: Unity's time scale stays 1, and a held clock has a rate of 0)
            s.timeFactor = (float)(GameSpeed.Rate / 60.0);
            var w = ChartWorld.ShipWorld();
            var (lat, lon) = WorldFrame.ToLatLon(w.x, w.z);
            s.latitude = lat;
            s.longitude = lon;

            // the boat
            var cls = game.World != null && body != null ? game.World.ClassOf(body.classId) : null;
            s.classId = body != null ? body.classId : "gislinge";
            s.length = cls != null ? cls.length : 7.7f;
            s.beam = cls != null ? cls.beam : 1.5f;
            s.draught = cls != null ? cls.draught : 0.25f;
            s.closestToWind = cls != null ? cls.closestToWind : 58f;
            s.crew = Voyage.Started ? Mathf.Max(1, Voyage.Men) : cls != null ? Mathf.Max(1, Mathf.RoundToInt(cls.crew)) : 4;
            s.oars = oars;
            s.hasSail = cls == null || cls.hasSail;

            // the weather where she is (the same sample her speed on the chart comes from)
            if (ChartWorld.TryWeather(out var air))
            {
                s.windFrom = air.windFrom;
                s.windSpeed = air.windSpeed;
                s.gust = air.gust > 0f ? air.gust : air.windSpeed * 1.4f;
                s.cloud = air.Cover;
                s.overcast = air.layered;
                s.storm = air.storm;
                float falls = Mathf.Clamp01(air.rain / 4f);                  // (mm an hour: 4 is heavy rain; the frame's stand-in used the same)
                s.snow = falls * air.SnowShare;
                s.rain = falls - s.snow;
                s.fog = air.fog;
                s.visibility = air.VisibilityKm * 1000f;
                s.cloudLight = air.HasSky ? air.cloudTransmittance : (float)LightingModel.CloudTransmittance(air.layered, air.storm);
            }

            // lying at a berth, or coming in to one
            string lying = Port.LyingId();
            s.moored = lying != null;
            s.portId = lying ?? "";
            s.berthing = false;
            s.berthDistance = 0f;
            s.berthAhead = 0f;
            if (!s.moored && Course.IsSet)
                foreach (var p in Port.All)
                    if (p != null && p.id == Course.Destination)
                    {
                        float d = p.DistanceTo(at);
                        if (d < aheadWithin)
                        {
                            s.berthAhead = d;                            // (the stage shows the jetty ahead once it is within its mist)
                            s.portId = p.id;
                        }
                        if (d < berthWithin)
                        {
                            s.berthing = true;
                            s.berthDistance = d;
                            s.portId = p.id;
                        }
                        break;
                    }

            // her way: under sail or under oars by the rule her speed was worked out with (Travel.Speed)
            s.heading = ship.Heading;
            s.speed = s.moored ? 0f : ship.Speed;
            s.lyingTo = body != null && body.Sheltering;
            s.underOars = false;
            if (cls != null && s.speed > 0.05f && !s.lyingTo)
            {
                double made = Travel.Speed(cls, s.heading, s.windFrom, s.windSpeed, out bool gale);
                double rowed = cls.oarsKn * System.Math.Min(1.0, cls.rowingHours / 12.0) * SimUnits.KnotMs;
                s.underOars = !gale && (!cls.hasSail || made <= rowed + 1e-4);
            }
            if (Oars.Out && s.speed > 0.05f && !s.lyingTo)
                s.underOars = true;                                      // (the oars out by the player's order: the sail is down)

            // the water she is on
            if (s.moored)
                s.place = StagePlace.Port;
            else if (Course.IsSet && Course.Kinds.Count > 0)
            {
                byte kind = Course.Kinds[Mathf.Clamp(Course.Next, 0, Course.Kinds.Count - 1)];
                s.place = kind == LaneGraph.River || kind == LaneGraph.Pending || kind == LaneGraph.Portage ? StagePlace.River
                        : kind == LaneGraph.Sea ? StagePlace.OpenSea : StagePlace.Coast;
            }
            else if (ChartWorld.OnRiver())
                s.place = StagePlace.River;
            else
            {
                var near = Port.Nearest(at);
                s.place = near != null && near.DistanceTo(at) < coastWithin ? StagePlace.Coast : StagePlace.OpenSea;
            }
            Vector2 water = s.place == StagePlace.River ? riverWater : s.place == StagePlace.Coast ? coastWater
                          : s.place == StagePlace.OpenSea ? seaWater : portWater;
            s.fetchKm = water.x;
            s.depth = water.y;
            return true;
        }
    }
}
