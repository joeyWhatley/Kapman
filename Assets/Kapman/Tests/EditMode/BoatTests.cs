using System.Collections.Generic;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The boat she truly is (Voyage/Boat.cs): the same dice as every hull, warned first, wounded before she is
    /// lost; her sail, her water and her men read by her speed; the cards' flags are the boat; shares; the save.</summary>
    public class BoatTests
    {
        static ShipClassDef Gislinge() => new ShipClassDef
        {
            id = "gislinge", length = 7.7f, beam = 2.2f, crew = 4, sailKn = 4.8f, maxKn = 6.5f, oarsKn = 3.5f, rowingHours = 6,
            closestToWind = 58, polar = 1, galeFragility = 0.6f, hasSail = true, sea = true, coast = true, river = true,
        };

        /// <summary>An hour at sea: a wind, and the hours to a gale (infinity: none).</summary>
        static BoatHour At(long hour, double wind, double galeIn = double.PositiveInfinity, double lossPerHour = 0.0004, double offWind = 150.0,
                           int men = 4, bool moored = false, bool river = false) => new BoatHour
        {
            hour = hour, wind = wind, offWind = offWind, underWay = !moored, moored = moored, river = river, night = false, galeIn = galeIn,
            men = men, hands = 4, length = 7.7, beam = 2.2, fragility = 0.6, lossPerHour = lossPerHour, seed = 793,
        };

        [SetUp]
        public void Fresh() => Boat.Begin(new BoatRules());

        // ------------------------------------------------------------------ the dice

        [Test]
        public void A_sound_boat_lying_to_has_the_chance_every_hull_has()
        {
            Assert.AreEqual(1.0, Boat.DiceFactor(false, 0.0), 1e-9, "sound, dry, fresh, full crew, lying to");
            Assert.AreEqual(0.0004 * 0.6, GaleDice.Chance(0.0004, 0.6, Boat.DiceFactor(false, 0.0)), 1e-12, "WorldSim's own figure for her class");
        }

        [Test]
        public void Her_choices_and_her_hurts_multiply_it()
        {
            Boat.SetOrder(GaleOrder.RunReefed);
            Assert.AreEqual(2.5, Boat.DiceFactor(false, 0.0), 1e-9);
            Boat.SetOrder(GaleOrder.CarrySail);
            Assert.AreEqual(8.0, Boat.DiceFactor(false, 0.0), 1e-9);
            Boat.Strake();                                                 // hull 0.55: strained
            Assert.AreEqual(16.0, Boat.DiceFactor(false, 0.0), 1e-9);
            Boat.SetOrder(GaleOrder.LieTo);
            Boat.Lash(0.0, 3.0);
            Assert.AreEqual(2.0 * 0.8, Boat.DiceFactor(false, 1.0), 1e-9, "lashed down for three days");
            Assert.AreEqual(2.0 * 0.8 * 0.1, Boat.DiceFactor(true, 1.0), 1e-9, "on a river");
        }

        [Test]
        public void The_throw_has_no_memory()
        {
            Assert.AreEqual(GaleDice.Roll(793, "player", 1234), GaleDice.Roll(793, "player", 1234));
            Assert.AreNotEqual(GaleDice.Roll(793, "player", 1234), GaleDice.Roll(793, "player", 1235));
            Assert.AreNotEqual(GaleDice.Roll(793, "player", 1234), GaleDice.Roll(794, "player", 1234));
        }

        // ------------------------------------------------------------------ warned first

        [Test]
        public void The_signs_come_in_order_and_once_each()
        {
            var signs = new List<int>();
            Boat.SignCame += (s, h) => signs.Add(s);
            for (int h = 0; h < 12; h++)
                Boat.Hour(At(h, 8.0, 12 - h));                             // a gale twelve hours off, coming on
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, signs, "the swell at twelve hours, the sky at six, the last at one");
            Assert.AreEqual(3, Boat.Sign);
            Boat.Hour(At(12, 8.0));                                        // it passed her by
            Assert.AreEqual(0, Boat.Sign);
        }

        [Test]
        public void A_second_gale_on_the_heels_of_the_first_has_its_own_signs()
        {
            var signs = new List<int>();
            Boat.SignCame += (s, h) => signs.Add(s);
            Boat.Hour(At(0, 8.0, 1.0));                                    // the last sign of the first
            Boat.Hour(At(1, 20.0, 0.0));
            Boat.Hour(At(2, 20.0, 0.0));
            Boat.Hour(At(3, 9.0, 5.0));                                    // it is over, and another is five hours off
            CollectionAssert.AreEqual(new[] { 3, 2 }, signs, "the second gale is told afresh");
            Assert.IsFalse(Boat.FairlyWarned, "and has its own hour of warning");
            Boat.Hour(At(4, 9.0, 4.0));
            Assert.IsTrue(Boat.FairlyWarned);
        }

        [Test]
        public void No_dice_in_a_gale_she_was_not_warned_of()
        {
            // certain death by the dice (a chance of 1 an hour), and the gale is on her with no sign before it
            Boat.Hour(At(0, 20.0, 0.0, lossPerHour: 10.0));
            Assert.IsFalse(Boat.IsLost);
            Assert.Greater(Boat.Hull, 0.9, "the hour worked her hull and no more");
            Assert.AreEqual(3, Boat.Sign, "the sign came with it");
            // an hour later she has had her warning: the sea strikes, and wounds
            Boat.Hour(At(1, 20.0, 0.0, lossPerHour: 10.0));
            Assert.IsFalse(Boat.IsLost, "the first strike is a wound");
            Assert.LessOrEqual(Boat.Hull, 0.55);
        }

        [Test]
        public void A_strike_wounds_before_it_kills_lying_to()
        {
            Boat.Hour(At(0, 8.0, 3.0, lossPerHour: 10.0));                 // warned three hours ahead
            Boat.Hour(At(3, 20.0, 0.0, lossPerHour: 10.0));
            Assert.IsFalse(Boat.IsLost);
            Assert.AreEqual("a strake started", Boat.HullWord);
            string how = null;
            Boat.Lost += h => how = h;
            Boat.Hour(At(4, 20.0, 0.0, lossPerHour: 10.0));               // the sail is down: nothing left to take but the boat
            Assert.IsTrue(Boat.IsLost);
            Assert.IsNotNull(how);
            Assert.AreEqual(0, Boat.Hour(At(5, 20.0, 0.0)).Count, "nothing more happens to a lost boat");
        }

        [Test]
        public void Running_she_loses_the_sail_between_the_strake_and_the_boat()
        {
            Boat.SetOrder(GaleOrder.RunReefed);
            Boat.Hour(At(0, 8.0, 3.0, lossPerHour: 10.0));
            Boat.Hour(At(3, 20.0, 0.0, lossPerHour: 10.0));
            Assert.Less(Boat.Hull, 0.7);
            Assert.AreNotEqual(SailCloth.Split, Boat.Cloth);
            Boat.Hour(At(4, 20.0, 0.0, lossPerHour: 10.0));
            Assert.AreEqual(SailCloth.Split, Boat.Cloth);
            Assert.IsFalse(Boat.IsLost);
            Boat.Hour(At(5, 20.0, 0.0, lossPerHour: 10.0));
            Assert.IsTrue(Boat.IsLost);
        }

        [Test]
        public void No_dice_in_hours_the_clock_jumped_over()
        {
            Boat.Hour(At(0, 8.0, 3.0, lossPerHour: 10.0));
            Boat.Grace(6);                                                 // (the clock was jumped to hour 5)
            for (int h = 3; h < 6; h++)
                Boat.Hour(At(h, 20.0, 0.0, lossPerHour: 10.0));
            Assert.Greater(Boat.Hull, 0.9, "three hours of gale inside the jump: worked, not struck");
            Boat.Hour(At(6, 20.0, 0.0, lossPerHour: 10.0));
            Assert.Less(Boat.Hull, 0.7, "the first hour that was played");
        }

        [Test]
        public void Made_fast_she_is_out_of_it()
        {
            Boat.Hour(At(0, 8.0, 3.0, lossPerHour: 10.0));
            for (int h = 3; h < 40; h++)
                Boat.Hour(At(h, 25.0, 0.0, lossPerHour: 10.0, moored: true));
            Assert.IsFalse(Boat.IsLost);
            Assert.AreEqual(1.0, Boat.Hull, 1e-9);
            Assert.IsFalse(Boat.InGale);
        }

        // ------------------------------------------------------------------ her speed

        [Test]
        public void Shortened_sail_is_seven_tenths_and_a_split_one_half()
        {
            var c = Gislinge();
            double full = Boat.Speed(c, 90.0, 270.0, 8.0, out bool lying);  // before a fresh breeze
            Assert.IsFalse(lying);
            Assert.AreEqual(Travel.Speed(c, 90.0, 270.0, 8.0, out _), full, 1e-9, "as she was: the world's ships' own speed");
            Boat.SetSail(SailSet.Reefed);
            Assert.AreEqual(0.7 * full, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9);
            Boat.SetSail(SailSet.Full);
            Boat.SetCloth(SailCloth.Split);
            Assert.AreEqual(0.5 * full, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9);
            Boat.SetCloth(SailCloth.Weak);
            Assert.LessOrEqual(Boat.Speed(c, 90.0, 270.0, 14.0, out _), 0.8 * c.sailKn * SimUnits.KnotMs + 1e-9, "a weak sail is not driven");
            Boat.SetCloth(SailCloth.Whole);
            Boat.SetSail(SailSet.Down);
            Assert.AreEqual(c.oarsKn * 0.5 * SimUnits.KnotMs, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9, "the sail down: the oars, a day's mean");
        }

        [Test]
        public void In_a_gale_she_lies_to_unless_ordered_to_run_with_the_wind_abaft()
        {
            var c = Gislinge();
            Assert.AreEqual(0.0, Boat.Speed(c, 90.0, 270.0, 20.0, out bool lying));
            Assert.IsTrue(lying);
            Boat.SetOrder(GaleOrder.RunReefed);
            Assert.AreEqual(0.7 * c.maxKn * SimUnits.KnotMs, Boat.Speed(c, 90.0, 270.0, 20.0, out lying), 1e-9);
            Assert.IsFalse(lying);
            Assert.AreEqual(0.0, Boat.Speed(c, 270.0, 270.0, 20.0, out lying), "into it: she cannot run");
            Assert.IsTrue(lying);
        }

        [Test]
        public void Water_and_weary_men_slow_her()
        {
            var c = Gislinge();
            Boat.Hour(At(0, 8.0));                                         // (sets what swamps her from her size)
            Boat.WaterBy(-1e9);
            double dry = Boat.Speed(c, 90.0, 270.0, 8.0, out _);
            Boat.WaterBy(Boat.Swamp * 0.5);
            Assert.AreEqual(0.7 * dry, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9, "half swamped: three tenths off");
            Boat.WaterBy(-1e9);
            Boat.SetSail(SailSet.Down);
            Boat.WearyBy(-1.0);
            double fresh = Boat.Speed(c, 90.0, 270.0, 8.0, out _);
            Boat.WearyBy(1.0);
            Assert.AreEqual(0.5 * fresh, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9, "spent men row at half");
        }

        // ------------------------------------------------------------------ water

        [Test]
        public void A_started_strake_keeps_a_man_bailing_and_a_sound_hull_none()
        {
            Boat.Hour(At(0, 6.0));
            Assert.AreEqual(0, Boat.Bailers);
            Boat.Strake();
            Boat.Hour(At(1, 6.0));
            Assert.AreEqual(1, Boat.Bailers);
            Assert.Less(Boat.Water, 100.0, "one man keeps it down");
            Assert.AreEqual(1.0, Boat.FlagView("sea_leak", 0.0));
        }

        /// <summary>An opened hull (0.4) in a gale of fourteen hours (nine gales in ten are shorter) with no dice at all:
        /// the hours she lasts, or -1.</summary>
        static int HoursAfloat(GaleOrder order, int changeAt = -1)
        {
            Boat.Begin(new BoatRules());
            Boat.HullBy(-0.6);
            Boat.SetOrder(order);
            Boat.Hour(At(0, 8.0, 2.0));
            for (int h = 2; h < 16; h++)
            {
                if (h - 2 == changeAt)
                    Boat.SetOrder(GaleOrder.LieTo);
                Boat.Hour(At(h, 22.0, 0.0, lossPerHour: 0.0));
                if (Boat.IsLost)
                    return h - 1;
            }
            return -1;
        }

        [Test]
        public void She_fills_when_the_men_cannot_keep_it_down()
        {
            int carried = HoursAfloat(GaleOrder.CarrySail);
            Assert.Greater(carried, 3, "all sail on an opened hull: she fills, and it takes hours");
            StringAssert.Contains("filled", Boat.LostHow);
            Assert.AreEqual(-1, HoursAfloat(GaleOrder.CarrySail, changeAt: 3), "the sail struck in the third hour: she lives");
            Assert.IsFalse(Boat.IsLost);
        }

        [Test]
        public void Four_fresh_men_keep_a_sound_boat_afloat_through_a_long_gale_lying_to()
        {
            Boat.Hour(At(0, 8.0, 2.0));
            for (int h = 2; h < 42; h++)                                   // forty hours: the longest the model gave in 13 years
                Boat.Hour(At(h, 22.0, 0.0, lossPerHour: 0.0));
            Assert.IsFalse(Boat.IsLost);
            Assert.Less(Boat.Water, 400.0);
            Assert.Greater(Boat.Hull, 0.6, "she has worked, and is not opened");
            Assert.Greater(Boat.Weary, 0.8, "and the men are spent");
        }

        // ------------------------------------------------------------------ the cards' flags are the boat

        [Test]
        public void The_flags_are_the_boat()
        {
            Assert.IsNull(Boat.FlagView("sea_vow_penny", 0.0), "not the boat's: the deck keeps it");
            Assert.IsFalse(Boat.FlagSet("sea_vow_penny", 1.0, 0.0, 0.0, 0.0));
            Assert.IsTrue(Boat.FlagSet("sea_leak", 1.0, 12.0, 0.0, 0.0));
            Assert.AreEqual(1.0, Boat.FlagView("sea_leak", 400.0), "a leak does not mend itself in twelve days");
            Boat.FlagSet("sea_leak", 0.0, 0.0, 0.0, 0.0);
            Assert.AreEqual(0.0, Boat.FlagView("sea_leak", 0.0));
            Assert.AreEqual(0.75, Boat.Hull, 1e-9, "patched, not sound");
            Boat.FlagSet("sea_sail_weak", 1.0, 20.0, 0.0, 0.0);
            Assert.AreEqual(SailCloth.Weak, Boat.Cloth);
            Boat.FlagSet("sea_reefed", 1.0, 2.0, 0.0, 0.0);
            Assert.AreEqual(SailSet.Reefed, Boat.EffectiveSet);
            Boat.Hour(At(48, 6.0));
            Assert.AreEqual(SailSet.Full, Boat.EffectiveSet, "the card's reef is shaken out after its two days");
            Boat.FlagSet("crew_boat_tarred", 1.0, 300.0, 10.0, 0.0);
            Assert.AreEqual(1.0, Boat.Hull, 1e-9);
            Assert.AreEqual(1.0, Boat.FlagView("crew_boat_tarred", 200.0));
            Assert.AreEqual(0.0, Boat.FlagView("crew_boat_tarred", 320.0));
        }

        [Test]
        public void A_card_reads_the_boat_through_its_flags()
        {
            var vars = new EncounterVars { state = new EncounterState(), flagSource = name => Boat.FlagView(name, 0.0) };
            Assert.AreEqual(0.0, vars.Num("flag.sea_leak"));
            Boat.Strake();
            Assert.AreEqual(1.0, vars.Num("flag.sea_leak"));
            vars.state.SetFlag("sea_vow_penny", 1.0, 0.0, 0.0);
            Assert.AreEqual(1.0, vars.Num("flag.sea_vow_penny"), "the deck's own flags are as they were");
        }

        [Test]
        public void The_boat_verb_parses_and_refuses()
        {
            string why = null;
            Assert.IsNotNull(Effects.ParseVerb("boat sail split", ref why));
            Assert.IsNotNull(Effects.ParseVerb("boat lie_to 6", ref why));
            Assert.IsNotNull(Effects.ParseVerb("boat hull -0.15", ref why));
            Assert.IsNotNull(Effects.ParseVerb("boat order carry", ref why));
            Assert.IsNotNull(Effects.ParseVerb("boat shelter", ref why));
            Assert.IsNull(Effects.ParseVerb("boat sail torn", ref why));
            Assert.IsNull(Effects.ParseVerb("boat hull", ref why));
            Assert.IsNull(Effects.ParseVerb("boat sink", ref why));
        }

        // ------------------------------------------------------------------ shares

        [Test]
        public void Shares_halve_the_wage_and_pay_a_tenth_of_the_gain_at_Martinmas()
        {
            Assert.AreEqual(1.0, Boat.WageFactor);
            Boat.Shares(true, 100.0);
            Assert.AreEqual(0.5, Boat.WageFactor);
            Assert.AreEqual(0.0, Boat.Martinmas(793, 11, 10, 300.0, out bool due));
            Assert.IsFalse(due);
            Assert.AreEqual(20.0, Boat.Martinmas(793, 11, 11, 300.0, out due), 1e-9, "a tenth of 200");
            Assert.IsTrue(due);
            Boat.Martinmas(793, 11, 12, 300.0, out due);
            Assert.IsFalse(due, "once a year");
            Assert.AreEqual(0.0, Boat.Martinmas(794, 11, 20, 250.0, out due), "a losing season pays nothing");
            Assert.IsTrue(due, "but it is still the reckoning");
        }

        // ------------------------------------------------------------------ the save, and every speed

        static string Run(int hours, int saveAt)
        {
            Boat.Begin(new BoatRules());
            Boat.SetOrder(GaleOrder.CarrySail);
            for (int h = 0; h < hours; h++)
            {
                // a made-up season: a breeze, with a gale of nine hours every fifty, each seen twelve hours ahead
                int k = h % 50;
                double wind = k >= 30 && k < 39 ? 21.0 : 7.0 + k % 5;
                double galeIn = k >= 30 && k < 39 ? 0.0 : k >= 18 && k < 30 ? 30 - k : double.PositiveInfinity;
                Boat.Hour(At(h, wind, galeIn, lossPerHour: 0.01));
                if (h == saveAt)
                    Boat.Restore((Dictionary<string, object>)Json.Parse(Json.Write(Boat.Capture())));
            }
            return Json.Write(Boat.Capture());
        }

        [Test]
        public void A_save_in_the_middle_changes_nothing()
        {
            string straight = Run(300, -1);
            Assert.AreEqual(straight, Run(300, 33), "saved and loaded in the middle of a gale");
            Assert.AreEqual(straight, Run(300, 120));
            Assert.AreEqual(straight, Run(300, -1), "and the same again from the same seed");
        }

        [Test]
        public void Water_and_bundles_take_room_in_the_hold()
        {
            var hold = new Hold { capacity = 660f };
            Assert.AreEqual(660f, hold.Free);
            Boat.Hour(At(0, 6.0));
            Boat.WaterBy(-1e9);
            Boat.WaterBy(200.0);
            Boat.SetBundles(true);
            hold.taken = (float)Boat.TakenKg;
            Assert.AreEqual(660f - 200f - 80f, hold.Free, 1e-3, "200 litres in her, and four men's bundles of 20 kg");
        }
    }
}
