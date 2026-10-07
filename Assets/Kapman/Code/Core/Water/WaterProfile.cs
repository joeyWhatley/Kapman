using System;
using System.Collections.Generic;
using Kapman.Game;
using UnityEngine;

namespace Kapman.Water
{
    /// <summary>
    /// THE WATER OF ONE LEVEL c. 800 (the user, 4 Oct: "each level has to have the exact right and correct water"): one
    /// record of Assets/Kapman/Data/Water/water_793.json (from Research/water_levels, with its sources) read into the
    /// numbers the level's water runs on, by the day of the year and how much the river is in spate:
    ///   the stream (low in its low months, mean, flood in its flood months, eased between by month);
    ///   the clarity (the Secchi depth: summer, winter, flood) and from it HDRP's absorption distance;
    ///   the colour (the deep-water body colour seen under an overcast sky) for the scattering;
    ///   the water's temperature (February's low to August's high on a cosine);
    ///   the tide (spring and neap range, the stream) and, for the sea, its depth.
    /// Pure: no scene. Missing numbers fall back to plain values and HasRiver/HasTide say what the record holds.
    /// </summary>
    public sealed class WaterProfile
    {
        public string id, name, kind, confidence;
        public bool HasRiver, HasTide, HasSea;
        public float widthM, depthM, floodRiseM;
        public float currentLow, currentMean, currentFlood;
        public readonly bool[] floodMonth = new bool[12], lowMonth = new bool[12];
        public float iceOneIn;
        public float secchiSummer = 1f, secchiWinter = 1f, secchiFlood = 0.3f;
        public Color colour = new Color(0.3f, 0.3f, 0.25f);
        public float tempFeb = 4f, tempAug = 17f;
        public float springRangeM, neapRangeM, streamMaxKn;
        public float seaDepthM;

        static readonly string[] Months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        /// <summary>Every record of the water file by id.</summary>
        public static Dictionary<string, WaterProfile> ParseAll(string json)
        {
            var all = new Dictionary<string, WaterProfile>();
            foreach (var o in Json.Arr(Json.Parse(json), "records") ?? new List<object>())
            {
                var p = From(o);
                if (!string.IsNullOrEmpty(p.id))
                    all[p.id] = p;
            }
            return all;
        }

        static float Num(object o, string key, float fallback) => (float)Json.Num(o, key, fallback);

        public static WaterProfile From(object o)
        {
            var p = new WaterProfile
            {
                id = Json.Str(o, "id"), name = Json.Str(o, "name"), kind = Json.Str(o, "kind"), confidence = Json.Str(o, "confidence"),
            };
            var river = Json.Obj(o, "river");
            if (river != null)
            {
                p.HasRiver = true;
                p.widthM = Num(river, "widthM", 50f);
                p.depthM = Num(river, "channelDepthM", 3f);
                p.floodRiseM = Num(river, "floodRiseM", 0f);
                p.iceOneIn = Num(river, "iceWinterOneIn", 0f);
                var c = Json.Obj(river, "currentMs");
                p.currentMean = Num(c, "mean", 0.4f);
                p.currentLow = Num(c, "low", p.currentMean * 0.6f);
                p.currentFlood = Num(c, "flood", p.currentMean * 2f);
                Mark(Json.Arr(river, "floodMonths"), p.floodMonth);
                Mark(Json.Arr(river, "lowMonths"), p.lowMonth);
            }
            var tide = Json.Obj(o, "tide");
            if (tide != null && Json.Str(tide, "type", "none") != "none")
            {
                p.HasTide = true;
                p.springRangeM = Num(tide, "springRangeM", 0f);
                p.neapRangeM = Num(tide, "neapRangeM", p.springRangeM * 0.5f);
                p.streamMaxKn = Num(tide, "streamMaxKn", 0f);
            }
            var sea = Json.Obj(o, "sea");
            if (sea != null)
            {
                p.HasSea = true;
                p.seaDepthM = Num(Json.Obj(sea, "depthM"), "typical", 20f);
            }
            var optics = Json.Obj(o, "optics");
            if (optics != null)
            {
                var s = Json.Obj(optics, "secchiM");
                p.secchiSummer = Num(s, "summer", 1f);
                p.secchiWinter = Num(s, "winter", p.secchiSummer);
                p.secchiFlood = Num(s, "flood", Mathf.Min(p.secchiWinter, 0.3f));
                var rgb = Json.Arr(optics, "colourRGB");
                if (rgb != null && rgb.Count >= 3)
                    p.colour = new Color(Convert.ToSingle(rgb[0]), Convert.ToSingle(rgb[1]), Convert.ToSingle(rgb[2]));
                var t = Json.Obj(optics, "tempC");
                p.tempFeb = Num(t, "feb", 4f);
                p.tempAug = Num(t, "aug", 17f);
            }
            return p;
        }

        static void Mark(List<object> months, bool[] into)
        {
            foreach (var m in months ?? new List<object>())
            {
                int i = Array.IndexOf(Months, m as string);
                if (i >= 0)
                    into[i] = true;
            }
        }

        /// <summary>The month's own stream (m/s): flood, low or mean.</summary>
        float MonthCurrent(int month0) => floodMonth[month0] ? currentFlood : lowMonth[month0] ? currentLow : currentMean;

        /// <summary>A month-by-month value eased between the middles of the months (day of year 1-366).</summary>
        static float ByMonth(Func<int, float> month, float dayOfYear)
        {
            float m = (dayOfYear - 15.5f) / 30.44f;                               // 0 at mid-January
            int a = Mathf.FloorToInt(m);
            float t = m - a;
            t = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI);
            return Mathf.Lerp(month(((a % 12) + 12) % 12), month(((a + 1) % 12 + 12) % 12), t);
        }

        /// <summary>The stream (m/s) on a day of the year, and in spate (0-1: the weather's flood) toward the flood stream.</summary>
        public float Current(float dayOfYear, float spate = 0f)
        {
            if (!HasRiver)
                return 0f;
            float seasonal = ByMonth(MonthCurrent, dayOfYear);
            return Mathf.Lerp(seasonal, Mathf.Max(seasonal, currentFlood), Mathf.Clamp01(spate));
        }

        /// <summary>How far the river stands above its summer water (m): its flood rise in flood months, by the spate.</summary>
        public float Rise(float dayOfYear, float spate)
        {
            if (!HasRiver)
                return 0f;
            float season = ByMonth(m => floodMonth[m] ? 1f : 0f, dayOfYear);
            return floodRiseM * Mathf.Clamp01(spate) * Mathf.Lerp(0.4f, 1f, season);
        }

        /// <summary>The Secchi depth (m): summer from April to September, winter otherwise, eased; toward the flood's in spate.</summary>
        public float Secchi(float dayOfYear, float spate = 0f)
        {
            float s = ByMonth(m => m >= 3 && m <= 8 ? secchiSummer : secchiWinter, dayOfYear);
            return Mathf.Lerp(s, Mathf.Min(s, secchiFlood), Mathf.Clamp01(spate));
        }

        /// <summary>HDRP's absorption distance (m) for a Secchi depth: one to one, calibrated on Dorestad's river as tuned by
        /// eye under the physical exposure (0.6 m at the Kromme Rijn's summer Secchi of 0.6 m); a dark thing goes from sight
        /// at about the Secchi depth.</summary>
        public static float AbsorptionDistance(float secchi) => Mathf.Max(0.05f, secchi);

        /// <summary>HDRP's scattering and refraction colours from the record's body colour, by the same calibration (the
        /// tuned river was the research colour times 0.65 and 0.92).</summary>
        public Color Scattering => new Color(colour.r * ScatterGain, colour.g * ScatterGain, colour.b * ScatterGain);

        /// <summary>How turbid the water is (0 the clearest seas at a Secchi depth of 5 m and more, 1 a silty river at 0.5 m):
        /// silt scatters light, so a turbid river reads khaki-brown from above where a clear sea stays deep (the 4 Oct water
        /// look test: the Kromme Rijn was near-black from the game's 35 degree view at the old gain).</summary>
        public float Turbidity => 1f - Mathf.InverseLerp(0.5f, 5f, secchiSummer);
        public float ScatterGain => Mathf.Lerp(0.65f, 1.25f, Turbidity);
        public Color Refraction => new Color(colour.r * 0.92f, colour.g * 0.92f, colour.b * 0.92f);

        /// <summary>The water's temperature (C): February's low to August's high on a cosine through the year.</summary>
        public float Temperature(float dayOfYear)
        {
            float phase = (dayOfYear - 45f) / 182.5f * Mathf.PI;                  // 0 at 14 February, pi at mid-August
            return Mathf.Lerp(tempFeb, tempAug, 0.5f - 0.5f * Mathf.Cos(phase));
        }

        /// <summary>The tide's range (m) between neaps and springs by the moon's age (days since new, 0-29.5).</summary>
        public float TideRange(float moonAgeDays)
        {
            if (!HasTide)
                return 0f;
            float springness = 0.5f + 0.5f * Mathf.Cos((moonAgeDays - 1.5f) / 29.53f * 4f * Mathf.PI);   // springs a day or two after new and full
            return Mathf.Lerp(neapRangeM, springRangeM, springness);
        }
    }
}
