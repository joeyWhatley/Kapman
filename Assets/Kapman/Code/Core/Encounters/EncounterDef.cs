using System.Collections.Generic;

namespace Kapman.Encounters
{
    /// <summary>Where a card opens (npc_text_encounters.md 2.1): in a harbour, at sea, among the crew, or as a long card
    /// that stays open across days (a reading, a riddle, a silver fetch).</summary>
    public enum Stage { Harbour, Sea, Crew, Long }

    /// <summary>
    /// Who fills one of a card's parts (its "actors"): a role ("tollman"), the person's people, faith and tongue ("port":
    /// the port's), an age range, and whether the role is FIXED at the port (the same man every call) or a new person each
    /// time. Empty fields fall back to the role's row in npc_roles_793.json.
    /// </summary>
    public sealed class RoleSpec
    {
        public string key = "", role = "", people = "port", faith = "port", tongue = "port", sex = "";
        public int ageMin, ageMax;
        public bool fixedRole = true;
        /// <summary>A member of the standing cast: its id ("cast.crew.ravn") and its name ("Ravn"). Such an actor is ONE
        /// person, the same on every card whatever the port (NpcRegistry.Cast): the crew's Ravn is Ravn everywhere,
        /// with his own feelings and memory. "" for an actor who is whoever holds that trade at the port.</summary>
        public string cast = "", name = "";
        /// <summary>The years this actor is that named person (the years the places' files give them at the port); 0:
        /// no bound. Outside them the card is met with whoever holds the trade then.</summary>
        public int castFrom, castTo;

        /// <summary>Is this actor a named person in that year?</summary>
        public bool CastIn(int year) => cast.Length > 0 && (castFrom <= 0 || year >= castFrom) && (castTo <= 0 || year <= castTo);
    }

    /// <summary>One line of speech: who says it (an actor's key), the writing key of the words, its tongue (empty: the
    /// speaker's own) and when it is said (a condition; empty: always).</summary>
    public sealed class LineDef
    {
        public string who = "", key = "", tongue = "";
        /// <summary>The id of a real line of story/tongues_793.json that this line comes up as, in its own tongue, before
        /// it turns into English ("" for none): only where a real line exists.</summary>
        public string turns = "";
        public Cond when = Cond.True;
    }

    /// <summary>
    /// One choice on a card: its key ("declare"), the writing key of its label and a plain label for until the words are
    /// written, the check that must hold for it to be chosen (a failed one greys it and shows the rule), what it costs
    /// (paid first), what it does, the card it leads to ("then"), and whether it is the one-key choice (Enter) on a
    /// card seen before.
    /// </summary>
    public sealed class OptionDef
    {
        public string key = "", labelKey = "", label = "", then = "";
        /// <summary>The writing keys of what came of it, where the card's data names them outright ("out", and after a
        /// chance "out_success" and "out_failure"); "" where they follow from the label's key (Outcomes).</summary>
        public string outKey = "", outSuccess = "", outFailure = "";
        public Cond check = Cond.True;
        public List<Effect> cost = new List<Effect>();
        public List<Effect> effects = new List<Effect>();
        public bool isDefault;
        public int index;

        /// <summary>The first chance among its effects (its odds are shown as a word), or null.</summary>
        public ChanceEffect Chance
        {
            get
            {
                foreach (var e in effects)
                    if (e is ChanceEffect c)
                        return c;
                return null;
            }
        }
    }

    /// <summary>
    /// AN ENCOUNTER CARD (npc_text_encounters.md 2.2) as the loader made it from the data: where and when it can come, how
    /// often (an arrival card comes every call where it holds; a drawn one by its weight per call or its rate per hour),
    /// who is in it, what they say, the choices, and what the people remember of each choice.
    /// </summary>
    public sealed class EncounterDef
    {
        public string id = "", titleKey = "", title = "", intentionKey = "", intention = "", file = "";
        public Stage stage = Stage.Harbour;
        /// <summary>Opens on arrival (the arrival set: at most two, in the file's order).</summary>
        public bool arrival;
        public Cond when = Cond.True;
        /// <summary>A harbour card's weight per port call (the visit draw).</summary>
        public double weight;
        /// <summary>A sea or crew card's rate per game hour (the hourly draw).</summary>
        public double rate;
        /// <summary>Offered by the sea sim (a ship within hail: "rate": "traffic"), never by the hourly draw.</summary>
        public bool bySim;
        public bool once;
        public double cooldownDays;
        /// <summary>The months (1-12) it can come in; empty: any.</summary>
        public readonly List<int> months = new List<int>();
        /// <summary>"good_name": likelier where the merchant is well regarded; "bad_name": where he is not; "": neither.</summary>
        public string wants = "";
        public readonly List<RoleSpec> actors = new List<RoleSpec>();
        public readonly List<LineDef> lines = new List<LineDef>();
        public readonly List<OptionDef> options = new List<OptionDef>();
        /// <summary>What the people remember of a choice: option key (or "option.success" / "option.failure") to a note.</summary>
        public readonly Dictionary<string, string> memory = new Dictionary<string, string>();
        /// <summary>Its place among all the cards (the loader's order).</summary>
        public int number;

        public RoleSpec Actor(string key)
        {
            foreach (var a in actors)
                if (a.key == key)
                    return a;
            return null;
        }

        public OptionDef Option(string key)
        {
            foreach (var o in options)
                if (o.key == key)
                    return o;
            return null;
        }

        public OptionDef Default
        {
            get
            {
                foreach (var o in options)
                    if (o.isDefault)
                        return o;
                return null;
            }
        }

        /// <summary>Can it come in this month (1-12)?</summary>
        public bool InMonth(int month) => months.Count == 0 || months.Contains(month);
    }
}
