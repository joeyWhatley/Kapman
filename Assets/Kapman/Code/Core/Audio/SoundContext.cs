using System;
using UnityEngine;

namespace Kapman.Audio
{
    /// <summary>What the listener is in, read every frame (audio design 2.1; nights one and two).</summary>
    public struct SoundContext
    {
        /// <summary>The wind at 10 m (m/s) and the rain (0-1).</summary>
        public float wind, rain;
        /// <summary>The air temperature (C) and relative humidity (0-1).</summary>
        public float temperature, humidity;
        /// <summary>The listener's height above the water (m) and the signed distance to the shore (m: + over water, - inland).</summary>
        public float aboveWater, shore;
        /// <summary>0 by day, 1 at night; the day of the year; the storm (0-1).</summary>
        public float night, day, storm;

        /// <summary>The hour of the day (0-24, local mean time), the Sun's altitude (degrees, - under the horizon), the
        /// cloud cover (0-1) and the leaves on the trees (1 in summer, 0 bare).</summary>
        public float hour, sunAltitude, cloud, leaf;

        /// <summary>
        /// What is round the listener, as fractions (0-1) of a disc of about 60 m: reed beds, hedges and scrub, open water,
        /// the wet basins behind the levee, the town, trees, and the fields. Night three samples them from the level's
        /// acoustic cover map; until then the runtime guesses them from the shore distance (SoundscapeModel.GuessSurroundings).
        /// </summary>
        public float reed, hedge, water, basin, town, wood, fields;

        /// <summary>The player's ship: her speed through the water (m/s), how fast she rolls and pitches (degrees a second),
        /// how hard she is rowed (0-1), and how far she is from the listener (m).</summary>
        public float shipSpeed, shipRollRate, shipPitchRate, shipRowing, shipDistance;

        /// <summary>A value by the name the data uses (an unknown name throws, and the loader asks for every name it reads,
        /// so a misspelt key fails at load).</summary>
        public float Get(string key)
        {
            switch (key)
            {
                case "wind": return wind;
                case "force": return SoundscapeModel.Force(wind);
                case "rain": return rain;
                case "temp": return temperature;
                case "humidity": return humidity;
                case "above_water": return aboveWater;
                case "shore": return shore;
                case "shore_abs": return Mathf.Abs(shore);
                case "night": return night;
                case "day": return day;
                case "storm": return storm;
                case "hour": return hour;
                case "sun": return sunAltitude;
                case "cloud": return cloud;
                case "leaf": return leaf;
                case "surround.reed": return reed;
                case "surround.hedge": return hedge;
                case "surround.water": return water;
                case "surround.basin": return basin;
                case "surround.town": return town;
                case "surround.wood": return wood;
                case "surround.field": return fields;
                case "ship.speed": return shipSpeed;
                case "ship.roll_rate": return shipRollRate;
                case "ship.pitch_rate": return shipPitchRate;
                case "ship.motion": return Mathf.Abs(shipRollRate) + Mathf.Abs(shipPitchRate);
                case "ship.rowing": return shipRowing;
                case "ship.distance": return shipDistance;
                case "ship.bow_db": return SoundscapeModel.BowWaveDb(shipSpeed);
                case "ship.creak_db": return SoundscapeModel.HullWorkDb(shipRollRate, shipPitchRate);
                case "wildlife": return SoundscapeModel.WildlifeOpen(this);
                case "inversion": return SoundscapeModel.Inversion(this);
                default: throw new ArgumentException($"soundscape: no context value \"{key}\"");
            }
        }
    }
}
