using Kapman.Chain;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The cut's rigid transform: an anchor lands on the far anchor (or the far side's arrival point at a mouth),
    /// the way back undoes it, and a corridor's metres keep their distance from the anchor across the cut.</summary>
    public class JoinTransformTests
    {
        const double Mm = 0.001;

        [Test]
        public void MapsAnchorToAnchor()
        {
            // a corridor join: anchor onto anchor, her heading turned to face into the far level
            var d = ChainSamples.BendChain();
            var bend = d.Join("the_bend");
            var ab = ChainData.Transform(bend, bend.a);
            var (x, z) = ab.Apply(bend.a.pose.x, bend.a.pose.z);
            Assert.AreEqual(bend.b.pose.x, x, Mm);
            Assert.AreEqual(bend.b.pose.z, z, Mm);
            Assert.AreEqual(0.0, ChainPose.Turn(ChainPose.Wrap(bend.b.pose.heading + 180.0), ab.ApplyHeading(bend.a.pose.heading)), 1e-6,
                            "leaving a by its outward heading, she comes into b facing in (b's outward heading turned about)");
            var ba = ChainData.Transform(bend, bend.b);
            Assert.IsTrue(ab.Then(ba).IsIdentity(Mm), "T_ab then T_ba is no move at all");
            Assert.IsTrue(ba.Then(ab).IsIdentity(Mm));
            Assert.IsTrue(ab.Then(ab.Inverse()).IsIdentity(Mm), "the inverse undoes it");
            var p = new ChainPose(512.3, 1477.9, 33.0);
            var back = ab.Inverse().Apply(ab.Apply(p));
            Assert.AreEqual(p.x, back.x, Mm);
            Assert.AreEqual(p.z, back.z, Mm);
            Assert.AreEqual(0.0, ChainPose.Turn(p.heading, back.heading), 1e-6);

            // a velocity turns with her and keeps its length
            var (vx, vz) = ab.ApplyDirection(1.2, 0.5);
            Assert.AreEqual(1.3, System.Math.Sqrt(vx * vx + vz * vz), 1e-9);

            // the real mouth: leaving Dorestad by the exit she is put at the sea's arrival point, facing out to sea
            var real = ChainSamples.Real();
            var mouth = real.Join("rhine_mouth");
            var down = ChainData.Transform(mouth, mouth.a);
            var arrive = mouth.b.ArrivalPose;
            var put = down.Apply(mouth.a.pose);
            Assert.AreEqual(arrive.x, put.x, Mm);
            Assert.AreEqual(arrive.z, put.z, Mm);
            Assert.AreEqual(0.0, ChainPose.Turn(arrive.heading, put.heading), 1e-6);
            // and coming back up she is put at Dorestad's arrival point, facing up the river
            var up = ChainData.Transform(mouth, mouth.b);
            var home = up.Apply(mouth.b.pose);
            Assert.AreEqual(mouth.a.ArrivalPose.x, home.x, Mm);
            Assert.AreEqual(mouth.a.ArrivalPose.z, home.z, Mm);
            Assert.AreEqual(0.0, ChainPose.Turn(mouth.a.ArrivalPose.heading, home.heading), 1e-6);

            // a join to a level not built has no transform yet
            var thames = real.Join("thames_mouth");
            Assert.Throws<System.InvalidOperationException>(() => ChainData.Transform(thames, thames.a));
        }

        [Test]
        public void CorridorParamIsInvariant()
        {
            var d = ChainSamples.BendChain();
            var bend = d.Join("the_bend");
            var ab = ChainData.Transform(bend, bend.a);
            var (ox, oz) = ChainPose.Dir(bend.a.pose.heading);
            foreach (double s in new[] { -100.0, -20.0, 0.0, 35.0, 140.0 })
                foreach (double side in new[] { -25.0, 0.0, 18.0 })
                {
                    // s metres past a's anchor along the way to b, and some metres to one side of the line
                    double x = bend.a.pose.x + ox * s + oz * side, z = bend.a.pose.z + oz * s - ox * side;
                    Assert.AreEqual(s, ChainSave.CorridorS(bend, bend.a.level, x, z), Mm, "measured in a");
                    var (bx, bz) = ab.Apply(x, z);
                    Assert.AreEqual(s, ChainSave.CorridorS(bend, bend.b.level, bx, bz), Mm, $"{s} m in a is {s} m in b");
                    double before = System.Math.Sqrt((x - bend.a.pose.x) * (x - bend.a.pose.x) + (z - bend.a.pose.z) * (z - bend.a.pose.z));
                    double after = System.Math.Sqrt((bx - bend.b.pose.x) * (bx - bend.b.pose.x) + (bz - bend.b.pose.z) * (bz - bend.b.pose.z));
                    Assert.AreEqual(before, after, Mm, "the distance from the anchor is kept");
                }
        }
    }
}
