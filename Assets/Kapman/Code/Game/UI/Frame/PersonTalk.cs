using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// A TALK WITH A PERSON OF THE PLACE (the user, 7 Oct 2026: "usually the conversation will be an event pop up in
    /// the middle of the screen"). The people of a port are the places files' (PlaceBook); each has lines by key: a
    /// greeting (another once you have met), talk of the place, a saying, a farewell, and whatever else their writer
    /// gave them. The People here window opens one as a floating window: who they are and where, what is seen of them,
    /// what they say, and what you may ask. It needs no answer and does not hold the clock.
    /// That you have spoken is kept (Self.MarkMet); how they feel about you is not, yet.
    /// </summary>
    public static class PersonTalk
    {
        /// <summary>The lines a person may be asked for, in order, with the words of the asking.</summary>
        static readonly (string key, string label)[] Asks =
        {
            ("rumour", "Ask what is said here"),
            ("saying", "Ask for a saying"),
            ("riddle", "Ask for the riddle"),
            ("debt", "Speak of the debt"),
        };

        static readonly Dictionary<string, int> picks = new Dictionary<string, int>(StringComparer.Ordinal);

        public static string FloatId(PersonDef p) => "person:" + (p != null ? p.id : "");

        /// <summary>The line a person greets with: their first greeting, or the one for someone met before.</summary>
        public static string GreetKey(PersonDef p, bool metBefore)
        {
            if (p == null)
                return "";
            if (metBefore && p.Has("greet_again"))
                return "greet_again";
            if (p.Has("greet"))
                return "greet";
            return p.keys.Count > 0 ? p.keys[0] : "";
        }

        static bool IsGreeting(string key) => key == "greet" || key == "greet_again" || key == "greet_friend";

        /// <summary>"Ask about the wreck" for a key the list above does not have.</summary>
        static string AskLabel(string key)
        {
            foreach (var (k, label) in Asks)
                if (k == key)
                    return label;
            return "Ask about " + key.Replace('_', ' ');
        }

        /// <summary>The keys a person can be asked for, in order: the usual ones they have, then their own.</summary>
        public static List<string> AskKeys(PersonDef p)
        {
            var keys = new List<string>();
            if (p == null)
                return keys;
            foreach (var (k, _) in Asks)
                if (p.Has(k))
                    keys.Add(k);
            foreach (string k in p.keys)
                if (!IsGreeting(k) && k != "farewell" && !keys.Contains(k))
                    keys.Add(k);
            return keys;
        }

        /// <summary>"Priest, at Saint Martin's, the upper church".</summary>
        public static string Where(PersonDef p, SiteDef site)
        {
            string role = Cap(p.Role), at = At(site);
            if (at.Length == 0)
                return role;
            return role.Length > 0 ? role + ", at " + at : "At " + at;
        }

        /// <summary>A site's name as it follows "at": "The dam heads" is "the dam heads".</summary>
        public static string At(SiteDef site)
        {
            if (site == null || site.name.Length == 0)
                return "";
            return site.name.StartsWith("The ", StringComparison.Ordinal) ? "the " + site.name.Substring(4) : site.name;
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);

        /// <summary>
        /// The talk as a floating window: the person saying the line of `key` (the `pick`-th of a key with several).
        /// `ask` is called with a key when the player asks for another line; `leave` when the player goes.
        /// </summary>
        public static FloatSpec Spec(PersonDef p, SiteDef site, PortDef port, string key, int pick, Action<string> ask, Action leave,
                                     int year = 793, bool metBefore = false)
        {
            var spec = new FloatSpec
            {
                id = FloatId(p), title = p.name.Length > 0 ? p.name : Cap(p.Role), who = Where(p, site),
                speaker = p.name.Length > 0 ? p.name : Cap(p.Role),
                speakerLine = Cap(p.Role) + (port != null ? " of " + port.name : ""),
                face = p.id, mustAnswer = false, holdsClock = false, shut = leave,
                tongue = TongueBook.Current.Of(p.people),               // (a word of their own tongue in a line is set apart)
            };
            // THE TONGUES (7 Oct 2026): a greeting begins with a word of the speaker's own tongue, and where the tongues
            // file has a true line for such a person it comes up in that tongue and turns into English: at a first
            // greeting, or when the line it belongs to is asked for
            var hail = IsGreeting(key) ? TalkTongues.Current.Hail(p) : null;
            var own = TalkTongues.Current.LineOf(p, year, out string on);
            if (own != null && key == on && (on != "greet" || !metBefore))
                spec.turns.Add(FloatTurn.Of(own));
            string line = p.Line(key, pick);
            if (line.Length > 0 && hail != null)
            {
                line = Cap(hail.word) + ". " + line;
                spec.marks.Add(hail);
            }
            if (line.Length > 0)
                spec.lines.Add((line, true));
            else
                spec.lines.Add(("They have nothing to say to that.", true));
            if (IsGreeting(key) && p.looks.Length > 0)
                spec.small = p.looks;
            if (key == "farewell")
            {
                spec.options.Add(new FloatOption { label = "Go", isDefault = true, pick = leave });
                return spec;
            }
            foreach (string k in AskKeys(p))
            {
                // (a line just said is not asked for again, unless there is more of it to hear)
                if (k == key && p.lines[k].Length < 2)
                    continue;
                string to = k;
                spec.options.Add(new FloatOption { label = k == key ? "Ask what else is said" : AskLabel(k), pick = () => ask?.Invoke(to) });
            }
            spec.options.Add(new FloatOption { label = "Take your leave", isDefault = spec.options.Count == 0, pick = () =>
            {
                if (p.Has("farewell"))
                    ask?.Invoke("farewell");
                else
                    leave?.Invoke();
            } });
            return spec;
        }

        /// <summary>Opens the talk with a person in the frame (the People here window's Talk button), and keeps that
        /// you have spoken.</summary>
        public static void Open(FrameRoot frame, PersonDef p)
        {
            if (frame == null || p == null)
                return;
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            var site = PlaceBook.Current.SiteOf(p, year);
            var port = GameData.Port(p.port);
            bool metBefore = Self.Met(p.id) != null;
            Self.MarkMet(p.id, game != null ? game.Day : 0.0);
            var greeting = Spec(p, site, port, GreetKey(p, metBefore), 0, key => Ask(frame, p, key), () => frame.CloseFloat(FloatId(p)), year, metBefore);
            foreach (var word in greeting.marks)
                Self.See(word.word);                                      // (a word of theirs heard is a little learned)
            ClockDrops.Raise(ClockReason.Talk);                           // (a talk takes the time it takes: the clock drops to speed 1)
            frame.OpenFloat(greeting);
            string at = At(site);
            FrameLog.Add($"You speak with {(p.name.Length > 0 ? p.name : "the " + p.Role)}{(at.Length > 0 ? " at " + at : "")}.");
        }

        /// <summary>Asks a person for one of their lines by its key ("rumour", "saying"...), as a choice in the talk does:
        /// the talk's window shows that line (the next of a key with several), opened if it was not.</summary>
        public static void Ask(FrameRoot frame, PersonDef p, string key)
        {
            if (frame == null || p == null || string.IsNullOrEmpty(key))
                return;
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            string slot = p.id + "/" + key;
            picks.TryGetValue(slot, out int n);
            picks[slot] = n + 1;
            frame.OpenFloat(Spec(p, PlaceBook.Current.SiteOf(p, year), GameData.Port(p.port), key, n,
                                 k => Ask(frame, p, k), () => frame.CloseFloat(FloatId(p)), year, true));
        }

        /// <summary>Which line each person has been asked for, and how often (the next asking gives the next line), for
        /// the saved game.</summary>
        public static Dictionary<string, object> Capture()
        {
            var saved = new Dictionary<string, object>();
            foreach (var kv in picks)
                saved[kv.Key] = (double)kv.Value;
            return saved;
        }

        /// <summary>What has been asked in a loaded game (null, an older save: nothing yet). Before, it was neither kept
        /// nor cleared: after a load a person went on from where the game before had left them.</summary>
        public static void Restore(object saved)
        {
            picks.Clear();
            if (saved is Dictionary<string, object> d)
                foreach (var kv in d)
                    if (kv.Value is double n && n > 0.0)
                        picks[kv.Key] = (int)n;
        }

        /// <summary>How often a person has been asked for a line (a test, a tool).</summary>
        public static int Asked(string personId, string key) => picks.TryGetValue(personId + "/" + key, out int n) ? n : 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => picks.Clear();
    }
}
