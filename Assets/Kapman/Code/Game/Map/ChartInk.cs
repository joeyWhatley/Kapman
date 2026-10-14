using System;

namespace Kapman.Game
{
    /// <summary>
    /// THE NAMES INKED ON THE CHART (the user, 14 Oct 2026, by the picker: "Inked names on the chart", over a mark
    /// with a note: "there has to be a better mechanic"). A water or a landmark she has found is not a mark with a
    /// name under the pointer any more: its name is lettered on the paper where it lies, as on a real chart, and stays.
    /// A river or a water is lettered ALONG the water, its lie taken from the world's own water about the point; a
    /// point that the table put on dry land is first brought to the nearest water within the thing's reach. An island,
    /// a headland, a shore, a thing built or a holy place is lettered upright at its place. A shoal or a tide race,
    /// which matter to sailing, keep a small chart sign on the water, with the name beside it. The log says the
    /// thing's note once, when it is found; nothing on the chart is clicked or hovered.
    /// This part is the reckoning: where a name goes and how it lies. The drawing is MapView.Finds.
    /// </summary>
    public static class ChartInk
    {
        /// <summary>A name shows when the thing's reach stands this many pixels or more on the chart: far out, the
        /// small things' names would crowd the paper.</summary>
        public const double MinPx = 24.0;

        /// <summary>Lettered along the water it names.</summary>
        public static bool Along(string kind) => kind == "river" || kind == "water";

        /// <summary>Kept as a small sign on the water, with the name beside it (what matters to sailing).</summary>
        public static bool Signed(string kind) => kind == "bank" || kind == "race";

        /// <summary>
        /// Where the name of a thing is lettered (real metres, the world frame) and how it lies (degrees, as the
        /// chart's screen turns: 0 reads west to east, a positive angle turns clockwise), by the water about it.
        /// `water` says whether a place is water (null: the bake is not there; the name goes at the point, level).
        /// False only when a river or a water has no water within its reach of the point, when nothing is lettered.
        /// </summary>
        public static bool Place(DiscoveryDef d, Func<double, double, bool> water, out double x, out double z, out float angle)
        {
            x = d.x;
            z = d.z;
            angle = 0f;
            if (!Along(d.kind) || water == null)
                return true;
            double step = d.kind == "river" ? 150.0 : 500.0;
            if (!water(x, z))
            {
                // the point lies on land: the nearest water within the thing's reach, ring by ring
                bool found = false;
                for (double r = step; r <= d.reach && !found; r += step)
                    for (int k = 0; k < 24; k++)
                    {
                        double a = k * Math.PI * 2.0 / 24.0;
                        double sx = d.x + r * Math.Cos(a), sz = d.z + r * Math.Sin(a);
                        if (water(sx, sz))
                        {
                            x = sx;
                            z = sz;
                            found = true;
                            break;
                        }
                    }
                if (!found)
                    return false;
            }
            // the lie of the water about the place: the long axis of the water sampled round it
            double radius = d.kind == "river" ? 1500.0 : 6000.0;
            const int n = 12;
            double h = radius / n;
            double sx1 = 0, sz1 = 0, sxx = 0, szz = 0, sxz = 0;
            int c = 0;
            for (int i = -n; i <= n; i++)
                for (int j = -n; j <= n; j++)
                {
                    double dx = i * h, dz = j * h;
                    if (dx * dx + dz * dz > radius * radius || !water(x + dx, z + dz))
                        continue;
                    sx1 += dx;
                    sz1 += dz;
                    sxx += dx * dx;
                    szz += dz * dz;
                    sxz += dx * dz;
                    c++;
                }
            if (c < 3)
                return true;
            double mx = sx1 / c, mz = sz1 / c;
            double cxx = sxx / c - mx * mx, czz = szz / c - mz * mz, cxz = sxz / c - mx * mz;
            double theta = 0.5 * Math.Atan2(2.0 * cxz, cxx - czz);     // (the long axis, x east and z north)
            double rot = -theta * 180.0 / Math.PI;                       // (the screen's y runs south)
            while (rot > 90.0)
                rot -= 180.0;
            while (rot <= -90.0)
                rot += 180.0;
            angle = (float)rot;
            return true;
        }
    }
}
