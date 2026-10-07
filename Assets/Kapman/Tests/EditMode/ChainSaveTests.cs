using Kapman.Chain;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>A save inside a corridor is never half across: it is on the side the 20 m hysteresis gives, posed in that
    /// level's metres, and loads to the same pose.</summary>
    public class ChainSaveTests
    {
        static LevelCarry At(ChainJoin join, ChainAnchor side, double s)
        {
            // s metres along the way from a to b, measured from this side's anchor
            double sign = side == join.a ? 1.0 : -1.0;
            var (dx, dz) = ChainPose.Dir(side.pose.heading);
            return new LevelCarry
            {
                levelId = side.level.id,
                x = side.pose.x + dx * s * sign,
                z = side.pose.z + dz * s * sign,
                heading = side == join.a ? side.pose.heading : ChainPose.Wrap(side.pose.heading + 180.0),
                speed = 1.6,
                gameHours = 100.0,
            };
        }

        [Test]
        public void CorridorNormalises()
        {
            var d = ChainSamples.BendChain();
            var bend = d.Join("the_bend");

            // in a, 10 m past the anchor: still a (the cut is at +20 m)
            var ten = ChainSave.Normalise(d, bend, At(bend, bend.a, 10.0));
            Assert.AreEqual("upper", ten.levelId);
            Assert.AreEqual(10.0, ChainSave.CorridorS(bend, bend.a.level, ten.x, ten.z), 1e-6);

            // in a, 30 m past it: b, posed 30 m in from b's anchor, facing into b
            var thirty = ChainSave.Normalise(d, bend, At(bend, bend.a, 30.0));
            Assert.AreEqual("lower", thirty.levelId);
            Assert.AreEqual(30.0, ChainSave.CorridorS(bend, bend.b.level, thirty.x, thirty.z), 1e-6);
            Assert.AreEqual(0.0, ChainPose.Turn(ChainPose.Wrap(bend.b.pose.heading + 180.0), thirty.heading), 1e-6);

            // the hysteresis the other way: in b, 10 m back toward a stays b; 30 m back is a
            Assert.AreEqual("lower", ChainSave.Normalise(d, bend, At(bend, bend.b, -10.0)).levelId);
            var back = ChainSave.Normalise(d, bend, At(bend, bend.b, -30.0));
            Assert.AreEqual("upper", back.levelId);
            Assert.AreEqual(-30.0, ChainSave.CorridorS(bend, bend.a.level, back.x, back.z), 1e-6);

            // far from the corridor nothing moves, whatever the parameter says
            var far = At(bend, bend.a, 30.0);
            far.x += 2000.0;
            Assert.AreEqual("upper", ChainSave.Normalise(d, bend, far).levelId);

            // saved and loaded: the same level and pose within a centimetre
            var loaded = LevelCarry.Restore(Json.Parse(Json.Write(thirty.Capture())));
            Assert.AreEqual(thirty.levelId, loaded.levelId);
            Assert.AreEqual(thirty.x, loaded.x, 0.01);
            Assert.AreEqual(thirty.z, loaded.z, 0.01);
            Assert.AreEqual(thirty.heading, loaded.heading, 1e-6);
            Assert.AreEqual("lower", ChainSave.Normalise(d, bend, loaded).levelId, "a normalised save stays where it is");
        }
    }
}
