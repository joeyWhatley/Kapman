using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE ASK FOR AN ORDER (Design/fixes/orders.md, rule 1: offered, then taken or refused). When a card offers you an
    /// order, a window comes up with its terms: what, for where, by what day, at how much a unit, the toll, how many
    /// loads it is for the boat you have, and what it costs to break it. Two answers: take it, or refuse it. It comes
    /// when the card that offered it has closed, so that what came of the card is read first; it holds the clock as a
    /// card does, and lets it run if you set it running. Shut without an answer, the offer waits in Errands for its
    /// seven days. THE WORDS ARE PLACEHOLDERS, the user's to write.
    /// </summary>
    public static class OrderAsk
    {
        static readonly List<Commission> waiting = new List<Commission>();
        static Commission asked;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Hook()
        {
            waiting.Clear();
            asked = null;
            Commissions.Offered -= Heard;
            Commissions.Offered += Heard;
        }

        static void Heard(Commission c)
        {
            if (c != null && !waiting.Contains(c))
                waiting.Add(c);
        }

        /// <summary>The window of an order's ask, by the order.</summary>
        public static string IdOf(Commission c) => "order." + c.id;

        /// <summary>Offers still to be asked (the one being asked among them).</summary>
        public static int Waiting => waiting.Count;

        /// <summary>Each frame (the frame): the next offer is asked, one at a time, when no card is up and nothing else
        /// keeps a card from opening (the title, the opening, the chest).</summary>
        public static void Tick(FrameRoot frame)
        {
            if (frame == null || waiting.Count == 0)
                return;
            var c = waiting[0];
            if (asked == c)
            {
                // (asked: it goes from the line when it is answered, or shut to wait in Errands)
                if (!frame.FloatOpen(IdOf(c)))
                {
                    asked = null;
                    waiting.RemoveAt(0);
                }
                return;
            }
            if (!Commissions.All.Contains(c) || c.mine || !c.offered)
            {
                waiting.RemoveAt(0);                                     // (answered in Errands meanwhile, or run out, or of a game before)
                return;
            }
            var d = EncounterRunner.Director;
            if ((d != null && d.Current != null) || (EncounterRunner.Blocked != null && EncounterRunner.Blocked()))
                return;
            frame.OpenFloat(Spec(c));
            asked = c;
        }

        /// <summary>The ask as a window: the terms, and the two answers.</summary>
        public static FloatSpec Spec(Commission c)
        {
            var port = GameData.Port(c.port);
            var spec = new FloatSpec { id = IdOf(c), title = port != null ? "An order at " + port.name : "An order", holdsClock = true };
            spec.lines.Add((Cap(Commissions.Describe(c)) + ".", true));
            string loads = Commissions.LoadsLine(c);
            if (loads.Length > 0)
                spec.lines.Add((loads, true));
            double toll = 0.0;
            var game = KapmanGame.Current;
            if (port != null && TradeRules.Current != null)
            {
                var zone = TradeRules.Current.ZoneOf(port.region);
                toll = zone != null ? zone.toll * EncounterRunner.TollFactor(port.id, game != null ? game.Day : c.posted) : 0.0;
            }
            spec.lines.Add(((toll > 0.0 ? $"Each part is paid as it lands, less the toll there ({toll * 100.0:0} in the hundred today). " : "Each part is paid as it lands. ") +
                            "Bring all of it by the day and they will think the better of you. Bring none and they will remember it; bring part and they will remember that a little.", true));
            spec.small = $"Unanswered, the offer stands {Commissions.OfferDays:0} days and then goes to another.";
            string why = Commissions.WhyNot(c);
            spec.options.Add(new FloatOption { label = "Take the order", enabled = why.Length == 0, rule = why, pick = () => Answer(c, true) });
            spec.options.Add(new FloatOption { label = "Refuse it", pick = () => Answer(c, false) });
            return spec;
        }

        static void Answer(Commission c, bool take)
        {
            string fact = take ? Commissions.Take(c) : Commissions.Refuse(c);
            if (!string.IsNullOrEmpty(fact))
                FrameLog.Add(fact);
            FrameRoot.Current?.CloseFloat(IdOf(c));
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
