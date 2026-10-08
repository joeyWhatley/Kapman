using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    public enum PlaceKind { None, Alehouse, Holy, Rest }

    /// <summary>
    /// A PLACE GONE INTO (the user, 7 Oct 2026, late: "we need a visit the bar / pub: this will open a window for the pub
    /// using another placeholder art, and it will have different choices, mainly just finding people to join you, meet
    /// other traders that are also visiting; Rest should bring up a camp fire on the river with a bunch of men sleeping,
    /// maybe a choice to talk to crew before sleeping; maybe visiting a religious site specific to this location").
    /// By the picker: the place's picture takes the world's frame (as the port's own picture does: PortView shows it),
    /// and what can be done there comes as choices in the box at the foot.
    ///
    /// THE ALEHOUSE is the port's own house of that kind out of the places' files (a tavern, a lodging; a hall where
    /// there is neither): its name, and what is seen there, are the files' own words. In it: the shipmen who look for
    /// a boat (the first jobs' own, while she wants men), the people who are found there, and the benches where the
    /// other masters sit (PortLife.Talk: the news not yet heard and the prices they bring, into the ledger).
    /// THE HOLY PLACE is the port's minster, abbey or church, or its grove or shrine: who is there, and the offering
    /// (whose rule is not in the game yet, and it says so).
    /// REST is the fire on the bank, or a watch below at sea: the crew to talk with, and sleep till first light or till
    /// the wind comes fair (FrameWait, as the Wait button did).
    ///
    /// The pictures are the user's to send (one for each: scene/place.alehouse.png, place.minster.png, place.grove.png,
    /// place.camp.png, in Art/final or Art/standin); till then the frame shows a plain ground with the place's name.
    /// Every line of words here is a placeholder of mine for the user to write over.
    /// </summary>
    public static class FramePlace
    {
        public const string FloatId = "place";
        static readonly string[] AleKinds = { "tavern", "lodging", "hall" };
        static readonly string[] HolyKinds = { "minster", "abbey", "church", "grove", "shrine" };

        public static PlaceKind At { get; private set; }
        public static SiteDef Site { get; private set; }
        /// <summary>Counts up when the place gone into changes (the picture's stamp).</summary>
        public static int Version { get; private set; }
        static string said = "";                                      // what came of the last thing done there
        static string portAt = "";

        /// <summary>The port's own site of that kind this year, or null (Rest has none).</summary>
        public static SiteDef SiteHere(PlaceKind kind)
        {
            var here = FrameGame.PortHere;
            var game = KapmanGame.Current;
            if (here == null || kind == PlaceKind.Rest || kind == PlaceKind.None)
                return null;
            var sites = PlaceBook.Current.Sites(here.id, game != null ? game.Year : 793);
            foreach (string k in kind == PlaceKind.Alehouse ? AleKinds : HolyKinds)
            {
                var s = sites.Find(x => x.kind == k);
                if (s != null)
                    return s;
            }
            return null;
        }

        /// <summary>What the holy place of this port is called on its button: Minster, Grove or Shrine.</summary>
        public static string HolyWord(SiteDef s) => s == null ? "Minster" : s.kind == "grove" ? "Grove" : s.kind == "shrine" ? "Shrine" : "Minster";

        /// <summary>The art slot of the place gone into ("" for none).</summary>
        public static string Picture =>
            At == PlaceKind.Alehouse ? "scene/place.alehouse.png"
            : At == PlaceKind.Holy ? (Site != null && (Site.kind == "grove" || Site.kind == "shrine") ? "scene/place.grove.png" : "scene/place.minster.png")
            : At == PlaceKind.Rest ? "scene/place.camp.png" : "";

        /// <summary>Its name, for the bar of its box and for the plain ground that stands in for its picture.</summary>
        public static string Name
        {
            get
            {
                if (At == PlaceKind.Rest)
                    return FrameGame.PortHere != null ? "The fire on the bank" : "A watch below";
                if (Site != null && Site.name.Length > 0)
                    return Site.name;
                return At == PlaceKind.Alehouse ? "The alehouse" : At == PlaceKind.Holy ? "The holy place" : "";
            }
        }

        public static void Enter(FrameRoot frame, PlaceKind kind)
        {
            if (frame == null || kind == PlaceKind.None)
                return;
            At = kind;
            Site = SiteHere(kind);
            said = "";
            portAt = FrameGame.PortHere != null ? FrameGame.PortHere.id : "";
            Version++;
            frame.OpenFloat(Spec(frame));
        }

        public static void Leave(FrameRoot frame)
        {
            if (At == PlaceKind.None)
                return;
            At = PlaceKind.None;
            Site = null;
            said = "";
            Version++;
            frame?.CloseFloat(FloatId);
        }

        /// <summary>Each look of the frame: a place of a port is left when she is no longer at that port, and a place
        /// whose box was shut is left with it.</summary>
        public static void Tick(FrameRoot frame)
        {
            if (At == PlaceKind.None || frame == null)
                return;
            string here = FrameGame.PortHere != null ? FrameGame.PortHere.id : "";
            if (here != portAt || !frame.FloatOpen(FloatId))
                Leave(frame);
        }

        static void Again(FrameRoot frame, string whatCame)
        {
            said = whatCame ?? "";
            if (At != PlaceKind.None)
                frame.OpenFloat(Spec(frame));
        }

        static FloatSpec Spec(FrameRoot frame)
        {
            var spec = new FloatSpec { id = FloatId, title = Name, face = "", shut = () => Leave(frame) };
            var here = FrameGame.PortHere;
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            if (said.Length > 0)
            {
                foreach (string line in said.Split('\n'))
                    if (line.Trim().Length > 0)
                        spec.lines.Add((line.Trim(), true));
            }
            else if (Site != null && Site.see.Length > 0)
                spec.lines.Add((Site.see, true));
            else if (At == PlaceKind.Rest)
                spec.lines.Add((here != null ? "A fire on the bank above the boat. The men lie round it in their cloaks, feet to the embers."
                                             : "The watch below lie under the awning, wrapped in their cloaks. One man keeps the look-out.", true));

            switch (At)
            {
                case PlaceKind.Alehouse:
                    Alehouse(frame, spec, here, year);
                    break;
                case PlaceKind.Holy:
                    Holy(frame, spec, here, year);
                    break;
                case PlaceKind.Rest:
                    Rest(frame, spec);
                    break;
            }
            spec.options.Add(new FloatOption { label = At == PlaceKind.Rest ? "Get up" : "Leave", isDefault = true, pick = () => Leave(frame) });
            return spec;
        }

        static void Alehouse(FrameRoot frame, FloatSpec spec, PortDef here, int year)
        {
            // the shipmen who look for a boat (the first jobs' own, while she still wants men)
            if (FirstJobs.Here && FirstJobs.HandsShort > 0)
            {
                int shown = 0;
                for (int i = 0; i < FirstJobs.Hands.Count && shown < 3; i++)
                {
                    var man = FirstJobs.Hands[i];
                    if (man.hired)
                        continue;
                    int index = i;
                    shown++;
                    spec.options.Add(new FloatOption
                    {
                        label = $"Hire {man.name}, {FirstJobs.HandMoney:0} d", enabled = FrameGame.Silver >= FirstJobs.HandMoney,
                        rule = "You have not the hiring silver.",
                        pick = () =>
                        {
                            FirstJobs.Hire(index, out string fact);
                            if (!string.IsNullOrEmpty(fact))
                                FrameLog.Add(fact);
                            Again(frame, fact);
                        },
                    });
                }
            }
            People(frame, spec, here, year, 2);
            if (here != null)
                spec.options.Add(new FloatOption
                {
                    label = "Sit with the other masters",
                    pick = () =>
                    {
                        string heard = PortLife.Talk(here);
                        FrameLog.Add("You sat with the masters at " + Name + ".");
                        Again(frame, heard);
                    },
                });
        }

        static void Holy(FrameRoot frame, FloatSpec spec, PortDef here, int year)
        {
            People(frame, spec, here, year, 2);
            spec.options.Add(new FloatOption
            {
                label = "Make an offering", enabled = false, rule = "Offerings and vows come with the faiths: not in the game yet.",
                pick = () => { },
            });
        }

        static void Rest(FrameRoot frame, FloatSpec spec)
        {
            spec.options.Add(new FloatOption
            {
                label = "Talk with the men",
                pick = () => Again(frame, Voyage.Started
                    ? $"The men are {FrameGame.HealthWord(Voyage.Health)} and {FrameGame.HeartWord(Voyage.Morale)}. What each of them says comes when the crew are people, not a count."
                    : "Nobody is aboard to talk with."),
            });
            string bound = FrameWait.Bound;
            string to = bound != null ? GameData.Port(bound)?.name ?? bound : null;
            spec.options.Add(new FloatOption
            {
                label = "Sleep till first light", enabled = FrameWait.Dark && !FrameWait.On, rule = FrameWait.On ? "You are waiting already." : "It is light already.",
                pick = () => { Leave(frame); FrameWait.Begin(FrameWait.For.FirstLight); },
            });
            spec.options.Add(new FloatOption
            {
                label = to != null ? "Wait for a fair wind for " + to : "Wait for a fair wind",
                enabled = to != null && !FrameWait.On && !FrameWait.WindFair(),
                rule = FrameWait.On ? "You are waiting already." : to == null ? "No course is set: fair for where?" : "It is fair enough now.",
                pick = () => { Leave(frame); FrameWait.Begin(FrameWait.For.FairWind); },
            });
            if (FrameWait.On)
                spec.options.Add(new FloatOption { label = "Stop waiting", pick = () => { FrameWait.End("You stop waiting."); Again(frame, "You are up again."); } });
        }

        /// <summary>The people found at the place (the files' own "person:" lines of the site), each to be spoken with.</summary>
        static void People(FrameRoot frame, FloatSpec spec, PortDef here, int year, int most)
        {
            if (here == null || Site == null)
                return;
            var book = PlaceBook.Current;
            int n = 0;
            foreach (var p in book.People(here.id, year))
            {
                var at = book.SiteOf(p, year);
                if (at == null || at.id != Site.id || n >= most)
                    continue;
                var person = p;
                n++;
                spec.options.Add(new FloatOption
                {
                    label = "Speak with " + (p.name.Length > 0 ? p.name + ", the " + p.Role : "the " + p.Role),
                    pick = () => PersonTalk.Open(frame, person),
                });
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            At = PlaceKind.None;
            Site = null;
            said = "";
            portAt = "";
            Version = 0;
        }
    }
}
