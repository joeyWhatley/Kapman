using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE TALKING TEXT (the user, 7 Oct 2026: "a Pokémon style text reveal ... how it appears like they're actually
    /// talking but if you press a key it skips to the next set of dialogue"). The lines of a talk come out a letter at
    /// a time, at a speaking pace, with a breath at a comma and a longer one at a full stop; one line follows another
    /// after a beat. A key (Enter, or a click on the words) finishes the line being said at once and starts the next.
    ///
    /// The words not yet said are already in their place, drawn clear, so the window has its full size from the first
    /// letter and no word jumps to the next row as it grows. A line whose text has a tag of its own in it ("&lt;") is
    /// not cut into: it comes whole when its turn comes. A line that is one label a word (SprinkledText) comes a word
    /// at a time. It goes by the wall clock: the game's speed does not hurry anybody's speech.
    ///
    /// Off in a batch run (the tests read the words on the screen) unless a test asks for it: <see cref="Showtime.On"/>.
    /// </summary>
    public sealed class TextReveal
    {
        /// <summary>Letters a second; the breath after a comma and after a full stop; the beat between two lines.</summary>
        public static float CharsPerSecond = 46f, CommaPause = 0.11f, StopPause = 0.26f, BetweenLines = 0.38f;

        const string ClearOpen = "<color=#00000000>", ClearClose = "</color>";

        sealed class Item
        {
            public VisualElement root;                                    // the line's own element (shown or put away as one)
            public TextElement label;                                     // a plain line
            public readonly List<VisualElement> words = new List<VisualElement>();   // or a line of one label a word
            public readonly List<int> starts = new List<int>();           // the letter each such word begins at
            public string full = "";
            public bool whole;                                            // it has tags of its own: never cut into
        }

        readonly List<Item> items = new List<Item>();
        float chars, wait;

        public int Count => items.Count;
        /// <summary>The line being said (Count when all are said).</summary>
        public int Index { get; private set; }
        /// <summary>Letters of that line said so far.</summary>
        public int Shown { get; private set; }
        public bool Done => Index >= items.Count;
        /// <summary>ONE LINE AT A TIME (the user, 7 Oct 2026: "you need to give far less text at one time, a paragraph
        /// at any one time"): only the line being said is shown. Said in full it stands and waits for the key, and the
        /// next takes its place; the last stays when all are said. Set before <see cref="StartAt"/>.</summary>
        public bool OneAtATime;
        /// <summary>The line shown stands whole and waits for the key (one at a time only).</summary>
        public bool Waiting { get; private set; }
        /// <summary>The whole text of a line.</summary>
        public string Key(int i) => i >= 0 && i < items.Count ? items[i].full : "";

        /// <summary>A line to be said: a label, or an element of one label a word.</summary>
        public void Add(VisualElement el)
        {
            if (el == null)
                return;
            var item = new Item { root = el };
            if (el is TextElement t)
            {
                item.label = t;
                item.full = t.text ?? "";
                item.whole = item.full.IndexOf('<') >= 0;
            }
            else
            {
                var sb = new System.Text.StringBuilder();
                el.Query<TextElement>().ForEach(w =>
                {
                    if (sb.Length > 0)
                        sb.Append(' ');
                    item.starts.Add(sb.Length);
                    item.words.Add(w);
                    sb.Append(w.text ?? "");
                });
                item.full = sb.ToString();
            }
            if (item.full.Length > 0)
                items.Add(item);
        }

        /// <summary>The lines before `first` stand whole (they were said already); from there on they are said, the
        /// first of them from its letter `shown` (a window drawn again in the middle of a line goes on where it was).</summary>
        public void StartAt(int first, int shown = 0)
        {
            Index = Mathf.Clamp(first, 0, items.Count);
            Waiting = false;
            for (int i = 0; i < items.Count; i++)
                Apply(items[i], i < Index ? items[i].full.Length : 0);
            Shown = Done ? 0 : Mathf.Clamp(shown, 0, items[Index].full.Length);
            chars = Shown;
            wait = Shown > 0 ? 0f : 0.12f;
            if (!Done && Shown > 0)
                Apply(items[Index], Shown);
            // (one at a time: a line that stood whole when the window was drawn again stands whole again, and waits)
            if (OneAtATime && !Done && Shown >= items[Index].full.Length)
            {
                if (Index >= items.Count - 1)
                    Next(0f);
                else
                    Waiting = true;
            }
            Page();
        }

        /// <summary>Says on, by the wall clock.</summary>
        public void Tick(float dt)
        {
            if (Done || Waiting || dt <= 0f)
                return;
            if (wait > 0f)
            {
                wait -= dt;
                if (wait > 0f)
                    return;
                dt = -wait;
                wait = 0f;
            }
            var item = items[Index];
            int length = item.full.Length;
            if (item.whole)
                Shown = length;
            else
            {
                chars += CharsPerSecond * dt;
                int target = Mathf.Min(length, (int)chars);
                while (Shown < target)
                {
                    Shown++;
                    float breath = Shown < length ? Breath(item.full, Shown - 1) : 0f;
                    if (breath > 0f)
                    {
                        wait = breath;
                        chars = Shown;
                        break;
                    }
                }
            }
            Apply(item, Shown);
            if (Shown >= length)
            {
                if (OneAtATime && Index < items.Count - 1)
                    Waiting = true;                                       // (it stands until the key: the next takes its place then)
                else
                    Next(BetweenLines);
            }
        }

        /// <summary>The key: the line being said is finished at once, and the next begins. False when all were said.</summary>
        public bool Advance()
        {
            if (Done)
                return false;
            if (OneAtATime && !Waiting && Index < items.Count - 1 && Shown < items[Index].full.Length)
            {
                // one at a time: the first press finishes the line, which then waits for the next press
                Shown = items[Index].full.Length;
                Apply(items[Index], Shown);
                chars = Shown;
                wait = 0f;
                Waiting = true;
                return true;
            }
            Apply(items[Index], items[Index].full.Length);
            Next(0f);
            return true;
        }

        /// <summary>Everything said, at once.</summary>
        public void Finish()
        {
            while (Advance())
            {
            }
        }

        void Next(float beat)
        {
            Index++;
            Shown = 0;
            chars = 0f;
            Waiting = false;
            wait = Done ? 0f : OneAtATime ? 0.06f : beat;
            Page();
        }

        /// <summary>One at a time: the line being said is the one shown (the last, once all are said).</summary>
        void Page()
        {
            if (!OneAtATime || items.Count == 0)
                return;
            int show = Mathf.Min(Index, items.Count - 1);
            for (int i = 0; i < items.Count; i++)
                if (items[i].root != null)
                    items[i].root.style.display = i == show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>The breath after the letter at `at`: a stop or a comma that ends a word (not the comma of "1,920",
        /// not the stop of "8.5").</summary>
        static float Breath(string s, int at)
        {
            char c = s[at];
            char after = at + 1 < s.Length ? s[at + 1] : ' ';
            if (!(after == ' ' || after == '\n' || after == '"' || after == '”' || after == '\''))
                return 0f;
            switch (c)
            {
                case '.':
                case '!':
                case '?':
                case '…':
                    return StopPause;
                case ',':
                case ';':
                case ':':
                case '—':
                    return CommaPause;
                default:
                    return 0f;
            }
        }

        // ------------------------------------------------------------------ Pokémon pacing: a box of a sentence or two

        /// <summary>
        /// POKEMON PACING (the user, 9 Oct 2026: "have it more Pokémon paced with dialogue"). How many letters one box
        /// of talk holds at the most: about two rows of the talk box. A line longer than this is said in several
        /// boxes, each brought by the key.
        /// </summary>
        public static int PageChars = 110;

        /// <summary>
        /// A text cut into boxes: whole sentences, as many to a box as fit in `most` letters; a sentence longer than a
        /// box is cut at a comma, a semicolon or a colon (else between two words). A text with a tag of its own in it
        /// is never cut. The boxes put end to end with a space between them are the text again.
        /// </summary>
        public static List<string> Pages(string text, int most)
        {
            var pages = new List<string>();
            text = (text ?? "").Trim();
            if (text.Length == 0)
                return pages;
            if (most < 20 || text.Length <= most || text.IndexOf('<') >= 0)
            {
                pages.Add(text);
                return pages;
            }
            // the sentences: a stop, a question or a cry, with whatever closes it, and a space after
            var sentences = new List<string>();
            int from = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c != '.' && c != '!' && c != '?' && c != '\u2026')
                    continue;
                int j = i + 1;
                while (j < text.Length && (text[j] == '.' || text[j] == '!' || text[j] == '?' || text[j] == '\u2026' || text[j] == '"' || text[j] == '\u201d' || text[j] == '\'' || text[j] == '\u2019' || text[j] == ')'))
                    j++;
                if (j < text.Length && text[j] != ' ')
                    continue;                                            // (the stop of "8.5", of "c.20")
                sentences.Add(text.Substring(from, j - from).Trim());
                from = j;
                i = j - 1;
            }
            if (from < text.Length)
                sentences.Add(text.Substring(from).Trim());
            string page = "";
            foreach (string sentence in sentences)
            {
                if (sentence.Length == 0)
                    continue;
                if (page.Length > 0 && page.Length + 1 + sentence.Length <= most)
                {
                    page += " " + sentence;
                    continue;
                }
                if (page.Length > 0)
                    pages.Add(page);
                page = "";
                string rest = sentence;
                while (rest.Length > most)
                {
                    int cut = BreakBefore(rest, most);
                    pages.Add(rest.Substring(0, cut).TrimEnd());
                    rest = rest.Substring(cut).TrimStart();
                }
                page = rest;
            }
            if (page.Length > 0)
                pages.Add(page);
            return pages;
        }

        /// <summary>Where a sentence too long for a box is cut: after the last comma, semicolon or colon that leaves at
        /// least a third of a box before it; else at the last space; else at the box's end.</summary>
        static int BreakBefore(string s, int most)
        {
            int last = Mathf.Min(most, s.Length - 1);
            for (int i = last - 1; i > most / 3; i--)
                if ((s[i] == ',' || s[i] == ';' || s[i] == ':') && s[i + 1] == ' ')
                    return i + 1;
            int space = s.LastIndexOf(' ', last);
            return space > 0 ? space : last;
        }

        static void Apply(Item item, int n)
        {
            int length = item.full.Length;
            n = Mathf.Clamp(n, 0, length);
            if (item.label != null)
            {
                if (n >= length)
                    item.label.text = item.full;
                else if (item.whole)
                    item.label.style.opacity = 0f;
                else
                {
                    if (n > 0 && char.IsHighSurrogate(item.full[n - 1]))
                        n++;
                    item.label.text = item.full.Substring(0, n) + ClearOpen + item.full.Substring(n) + ClearClose;
                }
                if (n >= length && item.whole)
                    item.label.style.opacity = StyleKeyword.Null;
                return;
            }
            for (int i = 0; i < item.words.Count; i++)
            {
                if (n >= length || item.starts[i] < n)
                    item.words[i].style.opacity = StyleKeyword.Null;
                else
                    item.words[i].style.opacity = 0f;
            }
        }
    }
}
