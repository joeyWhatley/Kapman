using Kapman.Game.Sim;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// SHIP - the boat she is: her kind, her size, what she draws and carries, how she goes under sail and under oars,
    /// from her class in the tables; and the boat as she is today (Voyage/Boat.cs; Design/fixes/risk_is_real.md): her
    /// hull, the water in her, her sail, the order for a gale, and what a night of gale would be to her as she is, in
    /// a word. The orders are given here: at sea how the sail is set, what the men do in a gale, and running for the
    /// nearest port; made fast, a yard's work on her seams and her sail.
    /// The window is BUILT only when its lines change (port or sea, lost, a yard's work offered). What its lines say
    /// changes in place (Live): the water in her moves every hour, and at the clock's fast speeds a window built
    /// again under the pointer loses the click.
    /// </summary>
    public sealed class ShipPanel : FramePanel
    {
        public override string Id => "ship";
        public override string Title => "Ship";
        public override int Order => 90;

        Label carries, carriesSmall, hull, water, waterSmall, cloth, set, order, gale, weather, hullCost, sailCost;
        VisualElement weatherLine;
        Button mendHull, mendSail;
        readonly Button[] sails = new Button[3], orders = new Button[3];

        static readonly SailSet[] Sets = { SailSet.Full, SailSet.Reefed, SailSet.Down };
        static readonly GaleOrder[] Orders = { GaleOrder.LieTo, GaleOrder.RunReefed, GaleOrder.CarrySail };

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(17, game != null && game.ship != null && game.ship.shipClass != null ? game.ship.shipClass.shipName : "");
            h = FrameUi.Mix(h, Trade.Started);
            h = FrameUi.Mix(h, Boat.IsLost);
            h = FrameUi.Mix(h, FrameGame.PortHere != null ? FrameGame.PortHere.id : "");
            h = FrameUi.Mix(h, (FirstJobs.Active && FirstJobs.SailWorn ? 1 : 0) + (FirstJobs.Mending ? 2 : 0));
            // (made fast: which of a yard's works are offered)
            return FrameUi.Mix(h, (Boat.HullMendCost > 0.0 ? 1 : 0) + (Boat.SailMendCost > 0.0 ? 2 : 0) + (Boat.Cloth == SailCloth.Split ? 4 : 0));
        }

        /// <summary>The sea's chance of striking her in an hour of gale, as she is now.</summary>
        public static double ChanceNow()
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            var cls = world != null && ChartBody.Current != null ? world.ClassOf(ChartBody.Current.classId) : null;
            if (cls == null)
                return 0.0;
            return GaleDice.Chance(world.rules.galeLossPerHour, cls.galeFragility, Boat.DiceFactor(ChartWorld.OnRiver(), game.Day));
        }

        protected override void Fill(VisualElement body)
        {
            carries = carriesSmall = hull = water = waterSmall = cloth = set = order = gale = weather = hullCost = sailCost = null;
            weatherLine = null;
            mendHull = mendSail = null;
            for (int i = 0; i < 3; i++)
                sails[i] = orders[i] = null;
            var game = KapmanGame.Current;
            var ship = game != null ? game.ship : null;
            if (ship == null || ship.shipClass == null)
            {
                body.Add(FrameUi.P("There is no ship to look over yet."));
                return;
            }
            var c = ship.shipClass;
            body.Add(FrameUi.H("The " + c.shipName));
            if (Boat.IsLost)
            {
                body.Add(FrameUi.P(Boat.LostHow));
                return;
            }
            if (!string.IsNullOrEmpty(c.about))
                body.Add(FrameUi.Text(c.about));
            body.Add(FrameUi.Ln("Length and beam", null, FrameUi.Num($"{c.length:0.#} m by {c.beam:0.#} m")));
            body.Add(FrameUi.Ln("Draws", "The water she needs under her, loaded.", FrameUi.Num($"{c.draught:0.##} m")));
            if (Trade.Started)
            {
                carries = FrameUi.Num("");
                var line = FrameUi.Ln("Carries", " ", carries);
                carriesSmall = line.Q<Label>(className: "frame-ln-small");
                body.Add(line);
            }
            var cls = game.World != null && ChartBody.Current != null ? game.World.ClassOf(ChartBody.Current.classId) : null;
            if (cls != null)
            {
                if (cls.hasSail)
                    body.Add(FrameUi.Ln("Under sail", "In a fair wind.", FrameUi.Num($"{cls.sailKn:0.#} knots")));
                if (cls.oarsKn > 0f)
                    body.Add(FrameUi.Ln("Under oars", null, FrameUi.Num($"{cls.oarsKn:0.#} knots")));
                if (cls.crew > 0f)
                    body.Add(FrameUi.Ln("Hands she wants", null, FrameUi.Num($"{cls.crew:0}")));
            }

            // THE BOAT AS SHE IS TODAY (the alpha's row 4: she can be hurt and lost like any other)
            body.Add(FrameUi.H("As she is"));
            hull = FrameUi.Tag("");
            body.Add(FrameUi.Ln("Hull", "How tight she is. A started strake leaks. A patch holds it; a yard, or a haul-out with tar, mends it.", hull));
            water = FrameUi.Tag("");
            var wet = FrameUi.Ln("Water in her", " ", water);
            waterSmall = wet.Q<Label>(className: "frame-ln-small");
            body.Add(wet);
            // (the opening's first job: until the worn sail is mended she has none to set, and the list "Before she
            // sails" is where that is seen to)
            cloth = FrameUi.Tag("");
            if (FirstJobs.Active && FirstJobs.SailWorn)
                body.Add(FrameUi.Ln("Sail", "Rotten along the foot. She cannot put out until it is mended: it stands in the list of things to do.", cloth));
            else
            {
                set = FrameUi.Tag("");
                body.Add(FrameUi.Ln("Sail", "The cloth, and how it is set. Shortened, she makes seven tenths of her speed.", cloth, set));
            }
            order = FrameUi.Tag("");
            body.Add(FrameUi.Ln("In a gale", "What the men do when it blows a gale.", order));
            // (a word, never a number: the cards show odds as words, by the user's own rule)
            gale = FrameUi.Tag("");
            body.Add(FrameUi.Ln("A night of gale, as she is", "Twelve hours of it.", gale));
            // (said across the whole window: beside a tag it stood in a column three words wide)
            body.Add(FrameUi.P("The first sea that strikes her starts a strake. The next takes the sail. The third takes the boat."));
            weather = FrameUi.Tag("");
            weatherLine = FrameUi.Ln("Weather", "The men read the swell and the sky.", weather);
            body.Add(weatherLine);

            // the orders (each row of buttons under its own line: the window is narrow)
            var port = FrameGame.PortHere;
            if (port == null)
            {
                body.Add(FrameUi.Ln("Set the sail", null));
                sails[0] = FrameUi.Btn("All of it", () => Do(() => Boat.SetSail(SailSet.Full)), "Her full speed.");
                sails[1] = FrameUi.Btn("Shorten", () => Do(() => Boat.SetSail(SailSet.Reefed)), "Seven tenths of her speed, and ready for weather.");
                sails[2] = FrameUi.Btn("Down", () => Do(() => Boat.SetSail(SailSet.Down)), "She goes by the oars at the men's own stroke. They tire a little.");
                body.Add(FrameUi.Row(sails[0], sails[1], sails[2]));
                body.Add(FrameUi.Ln("When it blows a gale", null));
                orders[0] = FrameUi.Btn("Lie to", () => Do(() => Boat.SetOrder(GaleOrder.LieTo)),
                                        "The sail struck, her head held to it under oars. She makes no way. The safest: what every other ship does.");
                orders[1] = FrameUi.Btn("Run shortened", () => Do(() => Boat.SetOrder(GaleOrder.RunReefed)),
                                        "With the wind abaft the beam she runs on under the shortened sail. More water aboard, more work for the hull, and the sea finds her sooner.");
                orders[2] = FrameUi.Btn("Carry sail", () => Do(() => Boat.SetOrder(GaleOrder.CarrySail)),
                                        "All of it, and run. Her best speed. The sail may tear, she ships a great deal of water, and the sea finds her far sooner.");
                body.Add(FrameUi.Row(orders[0], orders[1], orders[2]));
                body.Add(FrameUi.Row(FrameUi.Btn("Run for the nearest port", () => Do(() => FrameLog.Add(BoatWatch.RunForShelter() ?? "There is nowhere near to run to.")),
                                                 "Lays her course for the nearest port by water. Made fast, she is out of it.")));
            }
            else if (Trade.Started)
            {
                if (Boat.HullMendCost > 0.0)
                {
                    hullCost = FrameUi.Num("");
                    body.Add(FrameUi.Ln("Her seams", "Hauled up, caulked and tarred: sound again.", hullCost));
                    mendHull = FrameUi.Btn("Have her seams done", () => { Mend(port, true); Redraw(); });
                    body.Add(FrameUi.Row(mendHull));
                }
                if (Boat.SailMendCost > 0.0)
                {
                    sailCost = FrameUi.Num("");
                    body.Add(FrameUi.Ln("Her sail", Boat.Cloth == SailCloth.Split ? "A new cloth sewn to the yard." : "The seam sewn.", sailCost));
                    mendSail = FrameUi.Btn("Have her sail done", () => { Mend(port, false); Redraw(); });
                    body.Add(FrameUi.Row(mendSail));
                }
                if (Boat.HullMendCost <= 0.0 && Boat.SailMendCost <= 0.0)
                    body.Add(FrameUi.P("She wants nothing from a yard today."));
            }
            Live();
        }

        /// <summary>What the lines say, in place: her room, her hull, the water in her and who bails, her sail, the order
        /// that stands, how she would fare, the weather the men read, and a yard's prices.</summary>
        protected override void Live()
        {
            if (hull == null)
                return;
            if (carries != null)
            {
                carries.text = $"{System.Math.Max(0f, Trade.Hold.capacity - Trade.Hold.taken):0} kg";
                if (carriesSmall != null)
                    carriesSmall.text = Trade.Hold.taken >= 1f ? "Room for cargo, the water casks taken off, and what the water in her and the men's bundles take."
                                                               : "Room for cargo, the water casks taken off.";
            }
            hull.text = Boat.HullWord;
            water.text = Boat.WaterWord;
            if (waterSmall != null)
                waterSmall.text = Boat.AllHands ? "All hands are bailing." : Boat.Bailers == 2 ? "Two men are bailing."
                                : Boat.Bailers == 1 ? "One man is bailing." : "It weighs on her, slows her, and spoils what stands in it.";
            if (set != null)
            {
                cloth.text = Boat.ClothWord;
                set.text = Boat.SetWord;
            }
            else
                cloth.text = FirstJobs.Mending ? "ashore, being mended" : "tattered";
            order.text = Boat.OrderWord;
            gale.text = Boat.GaleWord(ChanceNow());
            bool sign = Boat.InGale || Boat.Sign > 0;
            weatherLine.style.display = sign ? DisplayStyle.Flex : DisplayStyle.None;
            if (sign)
                weather.text = Boat.InGale ? "a gale, now" : Boat.GaleIn <= 1.0 ? "a gale within the hour" : $"a gale within {Boat.GaleIn:0} hours";
            // (a button of an order shows pressed in while that order stands)
            for (int i = 0; i < 3; i++)
            {
                sails[i]?.EnableInClassList("on", Boat.EffectiveSet == Sets[i]);
                orders[i]?.EnableInClassList("on", Boat.Order == Orders[i]);
            }
            var port = FrameGame.PortHere;
            if (port != null && (mendHull != null || mendSail != null))
            {
                var zone = TradeRules.Current.ZoneOf(port.region);
                if (mendHull != null)
                {
                    hullCost.text = zone.Show(Boat.HullMendCost);
                    mendHull.SetEnabled(FrameGame.Silver >= Boat.HullMendCost);
                }
                if (mendSail != null)
                {
                    sailCost.text = zone.Show(Boat.SailMendCost);
                    mendSail.SetEnabled(FrameGame.Silver >= Boat.SailMendCost);
                }
            }
        }

        /// <summary>An order given by a button: done, and the lines say so at once (the window is not built again).</summary>
        void Do(System.Action what)
        {
            what();
            Live();
        }

        /// <summary>A yard's work, paid from the purse in the port's money: her seams done and tarred, or her sail made whole.</summary>
        public bool Mend(PortDef port, bool seams)
        {
            var rules = TradeRules.Current;
            double cost = seams ? Boat.HullMendCost : Boat.SailMendCost;
            if (port == null || cost <= 0.0 || !Trade.Purse.Pay(cost, rules.ZoneOf(port.region), rules, out _))
            {
                FrameLog.Add(cost <= 0.0 ? "She wants nothing from a yard today." : "There is not the silver for it.");
                return false;
            }
            if (seams)
                Boat.Tar(KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0);
            else
                Boat.SetCloth(SailCloth.Whole);
            FrameLog.Add(seams ? $"Her seams are done and tarred, for {rules.ZoneOf(port.region).Show(cost)}." : $"The sail is whole again, for {rules.ZoneOf(port.region).Show(cost)}.");
            return true;
        }
    }
}
