using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>A fair or seasonal market (Data/fairs_793.json).</summary>
    public sealed class FairDef
    {
        public string port, name;
        public int startDay, days;
        public float stockX = 1.4f, dearer = -0.05f;
    }

    /// <summary>
    /// FAIRS - on a fair's first day its market fills (stock times stock_x) and its prices move (dearer), fading as any
    /// shock does (trade_rules: shock_fade); news of it goes out from the place with the ships. The world's traders follow
    /// the prices there. Plans/ai_fleets.md, phase 6.
    /// </summary>
    public static class Fairs
    {
        public static List<FairDef> Load(TextAsset json)
        {
            var list = new List<FairDef>();
            if (json == null)
                return list;
            foreach (var x in Json.Arr(Json.Parse(json.text), "fairs") ?? new List<object>())
                list.Add(new FairDef
                {
                    port = Json.Str(x, "port"), name = Json.Str(x, "name"), startDay = (int)Json.Num(x, "start_day", 1),
                    days = (int)Json.Num(x, "days", 14), stockX = (float)Json.Num(x, "stock_x", 1.4), dearer = (float)Json.Num(x, "dearer", -0.05),
                });
            return list;
        }

        /// <summary>Once a day: fairs whose first day it is open.</summary>
        public static void Daily(WorldSim w)
        {
            if (w.fairs == null)
                return;
            var date = w.Date;
            foreach (var f in w.fairs)
            {
                if (f.startDay != date.dayOfYear)
                    continue;
                var p = GameData.Port(f.port);
                if (p == null || !p.Exists(date.year))
                    continue;
                w.MarketAt(p.index).Shock(null, f.stockX, f.dearer);
                w.Post("fair", $"{Cap(f.name)} has begun ({f.days} days).", p.index);
            }
        }

        /// <summary>The fair on at a port today, or null.</summary>
        public static FairDef On(WorldSim w, int port)
        {
            if (w?.fairs == null)
                return null;
            var date = w.Date;
            foreach (var f in w.fairs)
                if (GameData.Port(f.port)?.index == port && date.dayOfYear >= f.startDay && date.dayOfYear < f.startDay + f.days)
                    return f;
            return null;
        }

        static string Cap(string t) => string.IsNullOrEmpty(t) ? t : char.ToUpperInvariant(t[0]) + t.Substring(1);
    }
}
