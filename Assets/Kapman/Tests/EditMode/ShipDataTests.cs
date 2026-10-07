using System;
using System.IO;
using Kapman.Game;
using Kapman.Ships;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>Data/Ships/ships.json: it reads, the Gislinge is the first build's Gislinge, Dorestat 1's row waits as
    /// "later" (decision 5), and a broken row is an error that names the ship and the field.</summary>
    public class ShipDataTests
    {
        [Test]
        public void Parses()
        {
            var d = ShipKit.Data();
            var g = d.Get("gislinge");
            Assert.AreEqual("in_use", g.status);
            Assert.AreEqual("later", d.Get("dorestat1").status, "Dorestat 1's row is kept for later");
            Assert.IsTrue(g.OnWaterIn(793), "the Gislinge-class boat is on the water in 793");
            Assert.AreEqual(g.hullMassKg + g.crew * g.manKg + g.gearKg + g.cargoKg, g.LadenMass);
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => d.Get("no_such_ship"));
        }

        [Test]
        public void TheGislingeIsTheFirstBuildsRow()
        {
            var g = ShipKit.Gislinge();
            var old = Json.Parse(File.ReadAllText("Assets/Kapman/Game/Data/ship_classes_793.json"));
            object row = null;
            foreach (var r in Json.Arr(old, "classes"))
                if (Json.Str(r, "id") == "gislinge")
                    row = r;
            Assert.IsNotNull(row, "ship_classes_793.json has the gislinge row");
            Assert.AreEqual(Json.Num(row, "length_m"), g.length, 1e-4);
            Assert.AreEqual(Json.Num(row, "beam_m"), g.beam, 1e-4);
            Assert.AreEqual(Json.Num(row, "oars"), g.oars);
            Assert.AreEqual(Json.Num(row, "crew"), g.crew, 1e-4);
            Assert.AreEqual(Json.Num(row, "cargo_t") * 1000.0, g.cargoKg, 1e-3);
            Assert.AreEqual(Json.Num(row, "from"), g.from);
            Assert.AreEqual(Json.Num(row, "to"), g.to);
        }

        [Test]
        public void ABrokenRowSaysWhatIsWrong()
        {
            string good = File.ReadAllText(ShipClassData.ProjectPath);
            var e = Assert.Throws<FormatException>(() => ShipClassData.Parse(good.Replace("\"lwl_m\": 6.8,", "")));
            StringAssert.Contains("gislinge", e.Message);
            StringAssert.Contains("lwl_m", e.Message);
            e = Assert.Throws<FormatException>(() => ShipClassData.Parse(good.Replace("\"cb\": 0.5,", "\"cb\": 0.9,")));
            StringAssert.Contains("too full", e.Message);
        }
    }
}
