using System.Collections.Generic;
using Kapman.Game;

namespace Kapman.Chain
{
    /// <summary>
    /// WHAT CROSSES WITH THE SHIP - the state carried from one level to the next at the cut (gateways 2.10), and the same
    /// block a save writes ("carry", GAME_PLAN 2.7): the level and her pose in its metres, her way and her order, the clock
    /// and the weather (copied while each level still has its own; nothing to copy once they live in L_Boot), and the
    /// chart's place. A plain struct, JSON both ways. The merchant's affairs (hold, purse, debts, crew) never move at a cut
    /// and are saved by their own blocks.
    /// </summary>
    public struct LevelCarry
    {
        // the ship (ShipMotion)
        public string levelId;
        public double x, z, heading;
        /// <summary>Her speed (m/s), pitch and roll (degrees).</summary>
        public double speed, pitch, roll;
        /// <summary>Her way over the ground (m/s, x east and z north).</summary>
        public double velocityX, velocityZ;
        /// <summary>Where she is making for, if anywhere (the click-to-make-for goal), in the level's metres.</summary>
        public bool hasGoal;
        public double goalX, goalZ;

        // the clock (LevelClock)
        public double gameHours;
        public double timeSpeed;
        public bool paused;

        // the weather (LevelWeather)
        public string weatherKind;
        public bool weatherManual;
        /// <summary>The wind as LevelWeather.WindVector (m/s, x east and z north).</summary>
        public double windX, windZ;
        public double temperature, snowLying;
        public bool aurora;

        // the chart (RouteMap)
        public string routeId;
        public double realKm;

        /// <summary>The same state on the far side of a cut: her pose, goal and way moved by the join's transform, in the
        /// new level. The clock, the weather and the chart's place are untouched (the chart is re-read from the new level).</summary>
        public LevelCarry MovedBy(JoinTransform t, string toLevel)
        {
            var c = this;
            c.levelId = toLevel;
            (c.x, c.z) = t.Apply(x, z);
            c.heading = t.ApplyHeading(heading);
            (c.velocityX, c.velocityZ) = t.ApplyDirection(velocityX, velocityZ);
            if (hasGoal)
                (c.goalX, c.goalZ) = t.Apply(goalX, goalZ);
            return c;
        }

        /// <summary>The block as JSON values (numbers kept exact: the writer uses round-trip formatting).</summary>
        public Dictionary<string, object> Capture() => new Dictionary<string, object>
        {
            ["level"] = levelId ?? "",
            ["x"] = x,
            ["z"] = z,
            ["heading"] = heading,
            ["speed"] = speed,
            ["pitch"] = pitch,
            ["roll"] = roll,
            ["velocityX"] = velocityX,
            ["velocityZ"] = velocityZ,
            ["hasGoal"] = hasGoal,
            ["goalX"] = goalX,
            ["goalZ"] = goalZ,
            ["gameHours"] = gameHours,
            ["timeSpeed"] = timeSpeed,
            ["paused"] = paused,
            ["weatherKind"] = weatherKind ?? "",
            ["weatherManual"] = weatherManual,
            ["windX"] = windX,
            ["windZ"] = windZ,
            ["temperature"] = temperature,
            ["snowLying"] = snowLying,
            ["aurora"] = aurora,
            ["route"] = routeId ?? "",
            ["realKm"] = realKm,
        };

        /// <summary>The block read back; a missing field gives its default (an older save loads).</summary>
        public static LevelCarry Restore(object o) => new LevelCarry
        {
            levelId = Json.Str(o, "level"),
            x = Json.Num(o, "x"),
            z = Json.Num(o, "z"),
            heading = Json.Num(o, "heading"),
            speed = Json.Num(o, "speed"),
            pitch = Json.Num(o, "pitch"),
            roll = Json.Num(o, "roll"),
            velocityX = Json.Num(o, "velocityX"),
            velocityZ = Json.Num(o, "velocityZ"),
            hasGoal = Json.Bool(o, "hasGoal"),
            goalX = Json.Num(o, "goalX"),
            goalZ = Json.Num(o, "goalZ"),
            gameHours = Json.Num(o, "gameHours"),
            timeSpeed = Json.Num(o, "timeSpeed", 1.0),
            paused = Json.Bool(o, "paused"),
            weatherKind = Json.Str(o, "weatherKind"),
            weatherManual = Json.Bool(o, "weatherManual"),
            windX = Json.Num(o, "windX"),
            windZ = Json.Num(o, "windZ"),
            temperature = Json.Num(o, "temperature"),
            snowLying = Json.Num(o, "snowLying"),
            aurora = Json.Bool(o, "aurora"),
            routeId = Json.Str(o, "route"),
            realKm = Json.Num(o, "realKm"),
        };
    }
}
