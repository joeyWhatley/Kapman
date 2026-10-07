using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kapman.Game
{
    // ported 4 Oct: unchanged from the first build (plain C#: no world, no ship).

    /// <summary>
    /// READING RUNES (Plans/map_play.md: "slide letters under each rune; one rune stands for several sounds, so the word
    /// appears as you get them right") - the Younger Futhark had 16 runes for the sounds of Old Norse, so one rune stood
    /// for several (ᚢ u/o/v/w, ᚴ k/g, ᛏ t/d, ᛒ b/p, ᛁ i/e/j, ᛅ a/æ/e...), doubled sounds were written once, and n and m
    /// fell out before the sounds made in the same place (Guðmundr carved kuþmutr). A word is read when the sounds chosen
    /// under its runes say the word as it was spoken (the normalized Old Norse), allowing for those habits.
    /// </summary>
    public static class RuneReading
    {
        static readonly char[] Dividers = { ' ', '᛫', '᛬', '᛭', ':', '×', '·', '⁝', '|', '/' };

        /// <summary>The sounds a rune can stand for (the futhark table; a rune not in it stands for itself).</summary>
        public static string[] SoundsOf(string rune)
        {
            foreach (var r in ChestData.Futhark)
                if (r.rune == rune)
                    return r.sounds;
            return new[] { rune };
        }

        /// <summary>A text of runes split into words, each a list of runes.</summary>
        public static List<List<string>> Words(string runes)
        {
            var words = new List<List<string>>();
            foreach (var w in (runes ?? "").Split(Dividers, StringSplitOptions.RemoveEmptyEntries))
            {
                var list = new List<string>();
                var e = System.Globalization.StringInfo.GetTextElementEnumerator(w);
                while (e.MoveNext())
                    list.Add((string)e.Current);
                if (list.Count > 0)
                    words.Add(list);
            }
            return words;
        }

        /// <summary>The normalized Old Norse split into words (as many as the runes have, where they agree).</summary>
        public static List<string> Targets(string normalized) =>
            (normalized ?? "").Split(new[] { ' ', ',', '.', ';', ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        /// <summary>A reading brought to the sounds the runes could carve: lower case, length marks and like letters made
        /// one (ð þ, ǫ ø o, æ e, R r), n and m dropped before the sounds made in the same place, doubled letters once.</summary>
        public static string Canon(string s)
        {
            var b = new StringBuilder();
            foreach (char c0 in (s ?? "").Normalize(NormalizationForm.FormD))
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c0) == System.Globalization.UnicodeCategory.NonSpacingMark)
                    continue;                                              // (á é í ó ú: the length marks)
                char c = char.ToLowerInvariant(c0);
                switch (c)
                {
                    case 'ð': c = 'þ'; break;
                    case 'ǫ': case 'ø': case 'ö': case 'œ': c = 'o'; break;
                    case 'æ': c = 'e'; break;
                    case 'ʀ': c = 'r'; break;
                    case 'ą': case 'ã': c = 'a'; break;
                    case 'w': c = 'v'; break;
                }
                if (char.IsLetter(c))
                    b.Append(c);
            }
            // nasals before stops made in the same place were not carved; doubles were carved once
            var s1 = b.ToString();
            var o = new StringBuilder();
            for (int i = 0; i < s1.Length; i++)
            {
                char c = s1[i], next = i + 1 < s1.Length ? s1[i + 1] : '\0';
                if (c == 'n' && (next == 'g' || next == 'k' || next == 'd' || next == 't'))
                    continue;
                if (c == 'm' && (next == 'b' || next == 'p'))
                    continue;
                if (o.Length > 0 && o[o.Length - 1] == c)
                    continue;
                o.Append(c);
            }
            return o.ToString();
        }

        /// <summary>Is a word read: the sounds chosen under its runes say the target word?</summary>
        public static bool IsRead(IList<string> chosen, string target)
        {
            if (chosen == null || chosen.Any(string.IsNullOrEmpty))
                return false;
            return Canon(string.Concat(chosen)) == Canon(target);
        }

        /// <summary>Can the word be read at all from its runes' sounds? (Some spellings can't be matched sound for sound:
        /// those words are given.) A greedy walk: each rune takes the target's next sound if it can carry it.</summary>
        public static bool Readable(IList<string> runes, string target)
        {
            string t = Canon(target);
            int j = 0;
            foreach (var rune in runes)
            {
                var sounds = SoundsOf(rune).Select(Canon).Where(x => x.Length > 0).ToArray();
                if (j < t.Length && sounds.Any(x => x[0] == t[j]))
                {
                    j++;
                    continue;
                }
                if (j > 0 && sounds.Any(x => x[0] == t[j - 1]))
                    continue;                                              // (a rune carved twice for one sound)
                return false;
            }
            return j == t.Length;
        }
    }
}
