using System.Collections.Generic;
using Kapman.Encounters;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// BONDS - the people you know and how they feel. Two kinds are kept today: the people of the cards (the encounter
    /// engine's registry: each with a feeling toward you as a word, and the last thing they remember of you), and the
    /// people of the places files you have spoken with (only that you have spoken, how often and when). Bonds as a
    /// system of their own are still to be built; the panel says what is not kept.
    /// </summary>
    public sealed class BondsPanel : FramePanel
    {
        public override string Id => "bonds";
        public override string Title => "Bonds";
        public override int Order => 82;

        protected override int Stamp()
        {
            var d = EncounterRunner.Director;
            int h = FrameUi.Mix(17, Self.Version);
            if (d == null)
                return h;
            foreach (var n in d.npcs.All)
                if (d.npcs.Met(n.id, d.merchant))
                    h = FrameUi.Mix(FrameUi.Mix(h, n.id), (int)d.npcs.Feeling(n.id, d.merchant));
            return h;
        }

        protected override void Fill(VisualElement body)
        {
            int shown = 0;
            var d = EncounterRunner.Director;
            if (d != null)
            {
                var known = new List<Npc>();
                foreach (var n in d.npcs.All)
                    if (d.npcs.Met(n.id, d.merchant))
                        known.Add(n);
                known.Sort((a, b) => d.npcs.Feeling(b.id, d.merchant).CompareTo(d.npcs.Feeling(a.id, d.merchant)));
                foreach (var n in known)
                {
                    string title = d.book.roles.Role(n.role)?.title ?? n.role.Replace('_', ' ');
                    var port = GameData.Port(n.homePort);
                    string name = (n.name.Length > 0 ? n.name + ", " : "") + title + (port != null ? " of " + port.name : "");
                    var rel = d.npcs.Rel(n.id, d.merchant);
                    string last = "";
                    for (int i = rel.memory.Count - 1; i >= 0 && last.Length == 0; i--)
                        last = rel.memory[i].note ?? "";
                    string word = d.book.words.WordFor(WordTable.Feeling, rel.feeling);
                    body.Add(FrameUi.Ln(Cap(name), last.Length > 0 ? last : null, word.Length > 0 ? FrameUi.Tag(word) : null));
                    shown++;
                }
            }
            var book = PlaceBook.Current;
            var game = KapmanGame.Current;
            double today = game != null ? game.Day : 0.0;
            int spoken = 0;
            foreach (var kv in Self.MetAll)
            {
                var p = book.Person(kv.Key);
                if (p == null)
                    continue;
                if (spoken++ == 0)
                    body.Add(FrameUi.H("Spoken with"));
                var port = GameData.Port(p.port);
                string times = kv.Value.times == 1 ? "once" : kv.Value.times == 2 ? "twice" : kv.Value.times + " times";
                body.Add(FrameUi.Ln(p.name.Length > 0 ? p.name : "The " + p.Role,
                                    p.Role + (port != null ? " of " + port.name : "") + ". Last " + FrameUi.Ago(today - kv.Value.last) + ".",
                                    FrameUi.Tag(times)));
                shown++;
            }
            if (shown == 0)
            {
                body.Add(FrameUi.P("You know nobody yet. The People here window has those you can talk to in a port."));
                return;
            }
            if (spoken > 0)
                body.Add(FrameUi.P("How the people of the towns feel about you is not kept yet: only that you have spoken."));
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
