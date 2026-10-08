namespace Kapman.Encounters
{
    /// <summary>
    /// THE SEAM TO THE LIVE GAME (npc_text_encounters.md 4.1): what an encounter's effects do to the merchant and the
    /// world outside the encounter layer. The real one is Kapman.Game.Encounters.GameEffects (the purse, the hold, the
    /// crew, the standing, the chest, the news, the law); the tests use a fake that writes down every call. What the
    /// encounter layer keeps itself (the people's feelings and memories, flags, follow-up cards, the toll's ease) is not
    /// here: Effects applies those to the EncounterState and the NpcRegistry directly.
    /// Each method returns what happened as a plain fact for the card's outcome ("Paid 12 deniers."), or null for nothing
    /// to say.
    /// </summary>
    public interface IGameEffects
    {
        /// <summary>Silver received (+) or paid (-), in deniers' worth.</summary>
        string Silver(double deniers);
        /// <summary>Units of a good put in (+) or taken out of (-) the hold.</summary>
        string Goods(string good, int count);
        /// <summary>The crew's health changed by a share (-1..1).</summary>
        string Health(double share);
        /// <summary>The crew's heart changed by a share (-1..1).</summary>
        string Morale(double share);
        /// <summary>Men taken on (+) or lost (-).</summary>
        string Men(int count);
        /// <summary>A deed of a kind (reputation_793.json "deeds") done at a port: the standing it earns or costs, and the news.</summary>
        string Deed(string kind, string port);
        /// <summary>A news item (a writing key) posted on a port's board; it travels with the ships.</summary>
        string News(string key, string port);
        /// <summary>A market's price for a good moved by a factor for some days.</summary>
        string PriceShock(string port, string good, double factor, double days);
        /// <summary>An order posted at a port: so many units of a good, within days, at a premium.</summary>
        string Commission(string port, string good, int count, double days, double premium);
        /// <summary>A text into the chest, from a person.</summary>
        string GiveText(string id, string from, string port);
        /// <summary>A riddle into the chest, from a person.</summary>
        string GiveRiddle(string id, string from, string port);
        /// <summary>A silver fetch opened (a long card).</summary>
        string OpenFetch(string id);
        /// <summary>A person taken on as crew.</summary>
        string Hire(Npc npc);
        /// <summary>A crewman paid off.</summary>
        string PayOff(Npc npc);
        /// <summary>A person carried as a passenger to a port.</summary>
        string Passenger(Npc npc, string port);
        /// <summary>An order to the sea sim's meeting: close_and_hail, heave_to, run.</summary>
        string Encounter(string kind);
        /// <summary>The law acts: a seizure of forbidden goods, a fine, a hearing in some days (0: now).</summary>
        string Law(string kind, double days, string port);
        /// <summary>Progress toward reading (latin, runes).</summary>
        string Learn(string what, double amount);
        /// <summary>A tongue learned aboard, to a level (0.5: understood in part).</summary>
        string Tongue(string name, double level);
        /// <summary>A card set a flag. True when the live game keeps that thing itself (the boat's hurts, the men's
        /// shares: Kapman.Game.Boat) and has taken it: the deck then keeps no flag of its own for it.</summary>
        bool Flagged(string name, double value, double days) => false;
        /// <summary>An order to the boat from a card ("boat sail split", "boat lie_to 6"): what, a word or a number.</summary>
        string BoatOrder(string what, string word, double n) => null;
        /// <summary>A mark put on one of the five stats: its size (+1, -1, rarely 2), its name, and its end: so many
        /// days, or until a flag is set ("" for none).</summary>
        string Mark(string stat, double size, string id, double days, string untilFlag);
        /// <summary>A mark taken off.</summary>
        string Unmark(string id);
        /// <summary>The base of a stat moved by one, for good.</summary>
        string StatBase(string stat, double by);
    }
}
