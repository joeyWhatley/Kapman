using System;

namespace Kapman.Chain
{
    /// <summary>
    /// THE CORRIDOR'S SIDES - which level a ship in a corridor belongs to (gateways 2.4 and 2.11). The corridor parameter s
    /// runs along the join's a-to-b way, 0 at the anchors: in level a it is how far past a's anchor she is, outward; in
    /// level b, how far in from b's anchor. The cut a to b comes when s passes +20 m, the cut back when it falls under
    /// -20 m, so she may turn about in the corridor as often as she likes without flickering between levels. A save is
    /// never "half across": it holds the side the hysteresis gives, and the pose in that level's metres.
    /// </summary>
    public static class ChainSave
    {
        /// <summary>How far past the anchor (m) the cut comes, each way.</summary>
        public const double CutMetres = 20.0;

        /// <summary>The corridor parameter of a point in one of the join's two levels (m; + toward b).</summary>
        public static double CorridorS(ChainJoin join, ChainLevel level, double x, double z)
        {
            if (!join.IsCorridor)
                throw new InvalidOperationException($"The join {join.id} ({join.kind}) has no corridor");
            if (level == join.a.level)
            {
                var (dx, dz) = ChainPose.Dir(join.a.pose.heading);
                return (x - join.a.pose.x) * dx + (z - join.a.pose.z) * dz;
            }
            if (level == join.b.level)
            {
                var (dx, dz) = ChainPose.Dir(join.b.pose.heading);
                return -((x - join.b.pose.x) * dx + (z - join.b.pose.z) * dz);
            }
            throw new ArgumentException($"{level?.id} is not a side of the join {join.id}");
        }

        /// <summary>Whether a ship in a level at a corridor parameter now belongs to the other side.</summary>
        public static bool Crosses(ChainJoin join, ChainLevel level, double s) =>
            level == join.a.level ? s > CutMetres : level == join.b.level && s < -CutMetres;

        /// <summary>
        /// A carried state (a save, or the live ship) put on the side of a corridor join it belongs to: unchanged unless it is
        /// past the cut toward the other level, in which case it is moved into that level's metres by the cut's transform.
        /// A state outside the corridor's length, or in neither of the join's levels, is left alone.
        /// </summary>
        public static LevelCarry Normalise(ChainData data, ChainJoin join, LevelCarry carry)
        {
            var level = data.Level(carry.levelId);
            if (level == null || (level != join.a.level && level != join.b.level))
                return carry;
            var side = level == join.a.level ? join.a : join.b;
            double s = CorridorS(join, level, carry.x, carry.z);
            if (Math.Abs(s) > side.CorridorHalfLength || !Crosses(join, level, s))
                return carry;
            var other = join.Other(side);
            return carry.MovedBy(ChainData.Transform(join, side), other.level.id);
        }
    }
}
