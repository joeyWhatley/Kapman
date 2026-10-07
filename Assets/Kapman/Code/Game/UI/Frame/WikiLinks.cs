using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE LINKED WORDS (Design/wiki.md: "everything links"; the user, 7 Oct 2026: "I want everything to link with
    /// keywords and phrases of interest, they are a different colour and it could be anywhere, it all links together at
    /// the wiki, and if you hover over it it gives a brief overview, then if you click it it will bring up the wiki on
    /// the page"; and, late that day, of the lender's byname: "you should link stuff like that to the wiki ... I don't
    /// see much stuff at all underlined or having links").
    ///
    /// ONE PLACE DOES IT FOR ALL TEXT. Each frame, after the windows have written their words, every text of the frame
    /// is looked at (<see cref="Pass"/>): a name that has a page of the Book (WikiBook.Names, the longest first) becomes
    /// a link in the link colour with a line under it, the first time it stands in that text. Words inside a button are
    /// not links (a click there must do what the button says), nor is a text that has marks of its own already (a line
    /// still being said a letter at a time: it is linked when it has been said). The pointer on a link brings a small
    /// box with the name, its kind and a brief word; a click turns the Book to the page.
    ///
    /// A name on the screen is a thing come upon: its page opens (WikiBook.Meet).
    ///
    /// Off in a batch run (the tests read the words on the screen as they are written) unless a test asks for it.
    /// </summary>
    public static class WikiLinks
    {
        /// <summary>A test may ask for the links in a batch run.</summary>
        public static bool ForceOn;
        public static bool On => ForceOn || !Application.isBatchMode;

        /// <summary>The link colour on a pale ground and on a dark bar: a sea blue kept for links and nothing else.</summary>
        public const string OnPale = "#1B5E7B", OnDark = "#A9D6E8";
        const int MostKept = 3000;

        static Dictionary<string, List<(string name, string page)>> byFirst;
        static string[] shields = Array.Empty<string>();
        static WikiBook indexed;
        static readonly Dictionary<string, string> kept = new Dictionary<string, string>(StringComparer.Ordinal);
        static readonly ConditionalWeakTable<TextElement, object> hooked = new ConditionalWeakTable<TextElement, object>();
        static readonly object Mark = new object();

        /// <summary>The page under the pointer now ("" none), and where the pointer is (the frame shows the box).</summary>
        public static string Hover { get; private set; } = "";
        public static Vector2 HoverAt { get; private set; }
        /// <summary>A link was pressed: the frame turns the Book to it.</summary>
        public static event Action<string> Pressed;

        static void Index()
        {
            var book = WikiBook.Current;
            if (ReferenceEquals(book, indexed) && byFirst != null)
                return;
            indexed = book;
            kept.Clear();
            byFirst = new Dictionary<string, List<(string, string)>>(StringComparer.Ordinal);
            foreach (var (name, page) in book.Names)
            {
                Add(name, page);
                // (a name of several words, or a ware's, also as it stands inside a sentence: "bar iron", "ship's bread")
                if (char.IsUpper(name[0]) && (name.IndexOf(' ') > 0 || page.StartsWith("good.", StringComparison.Ordinal)))
                    Add(char.ToLowerInvariant(name[0]) + name.Substring(1), page);
            }
            foreach (var list in byFirst.Values)
                list.Sort((a, b) => b.name.Length.CompareTo(a.name.Length));
            shields = book.Shields;
        }

        static void Add(string name, string page)
        {
            string first = FirstWord(name, 0);
            if (first.Length == 0)
                return;
            if (!byFirst.TryGetValue(first, out var list))
                byFirst[first] = list = new List<(string, string)>();
            list.Add((name, page));
        }

        static bool Wordy(char c) => char.IsLetterOrDigit(c) || c == '\'' || c == '’';

        static string FirstWord(string s, int at)
        {
            int end = at;
            while (end < s.Length && Wordy(s[end]))
                end++;
            return s.Substring(at, end - at);
        }

        /// <summary>A plain text with its names made links (the same text when it has none). `dark`: it stands on a bar.</summary>
        public static string Link(string plain, bool dark)
        {
            if (string.IsNullOrEmpty(plain) || plain.Length < 3)
                return plain;
            string key = dark ? "\u0001" + plain : plain;
            if (kept.TryGetValue(key, out string known))
                return known;
            Index();
            // the stretches that must not be cut into (a shield: "New York" is not York)
            List<(int from, int to)> held = null;
            foreach (string shield in shields)
            {
                int at = plain.IndexOf(shield, StringComparison.Ordinal);
                while (at >= 0)
                {
                    (held ??= new List<(int, int)>()).Add((at, at + shield.Length));
                    at = plain.IndexOf(shield, at + shield.Length, StringComparison.Ordinal);
                }
            }
            StringBuilder sb = null;
            HashSet<string> done = null;
            int copied = 0, i = 0;
            while (i < plain.Length)
            {
                if (!Wordy(plain[i]) || (i > 0 && Wordy(plain[i - 1])))
                {
                    i++;
                    continue;
                }
                string first = FirstWord(plain, i);
                bool linked = false;
                if (first.Length > 0 && byFirst.TryGetValue(first, out var names))
                    foreach (var (name, page) in names)
                    {
                        int end = i + name.Length;
                        if (end > plain.Length || string.CompareOrdinal(plain, i, name, 0, name.Length) != 0 || (end < plain.Length && Wordy(plain[end])))
                            continue;
                        if (held != null && held.Exists(h => i < h.to && end > h.from))
                            continue;
                        if (done != null && done.Contains(page))
                            break;                                        // (linked once in this text already: the name stands plain)
                        (done ??= new HashSet<string>()).Add(page);
                        sb ??= new StringBuilder(plain.Length + 96);
                        sb.Append(plain, copied, i - copied);
                        sb.Append("<link=\"").Append(page).Append("\"><color=").Append(dark ? OnDark : OnPale).Append("><u>").Append(name).Append("</u></color></link>");
                        copied = end;
                        i = end;
                        linked = true;
                        break;
                    }
                if (!linked)
                    i += Math.Max(1, first.Length);
            }
            string result = plain;
            if (sb != null)
            {
                sb.Append(plain, copied, plain.Length - copied);
                result = sb.ToString();
            }
            if (kept.Count > MostKept)
                kept.Clear();
            kept[key] = result;
            if (done != null)
                foreach (string page in done)
                    WikiBook.Meet(page);
            return result;
        }

        /// <summary>Every text under `root` that has no marks of its own and is not in a button gets its links.</summary>
        public static void Pass(VisualElement root)
        {
            if (!On || root == null || WikiBook.Current.Names.Count == 0)
                return;
            Walk(root, false);
        }

        static void Walk(VisualElement e, bool dark)
        {
            if (e is Button || e is TextField || e.resolvedStyle.display == DisplayStyle.None)
                return;
            if (!dark && (e.ClassListContains("frame-top") || e.ClassListContains("frame-float-bar") || e.ClassListContains("frame-top-state")))
                dark = true;
            if (e is TextElement t)
            {
                string text = t.text;
                if (!string.IsNullOrEmpty(text) && text.Length >= 3 && text.IndexOf('<') < 0)
                {
                    string linked = Link(text, dark);
                    if (!ReferenceEquals(linked, text) && linked != text)
                    {
                        t.enableRichText = true;
                        t.text = linked;
                        Hook(t);
                    }
                }
                return;
            }
            int n = e.hierarchy.childCount;
            for (int i = 0; i < n; i++)
                Walk(e.hierarchy[i], dark);
        }

        static void Hook(TextElement t)
        {
            if (hooked.TryGetValue(t, out _))
                return;
            hooked.Add(t, Mark);
            t.pickingMode = PickingMode.Position;                         // (a text takes the pointer only once it has a link)
            t.RegisterCallback<PointerOverLinkTagEvent>(e => { Hover = e.linkID ?? ""; HoverAt = e.position; });
            t.RegisterCallback<PointerMoveLinkTagEvent>(e => { Hover = e.linkID ?? ""; HoverAt = e.position; });
            t.RegisterCallback<PointerOutLinkTagEvent>(e => Hover = "");
            t.RegisterCallback<PointerLeaveEvent>(e => Hover = "");
            t.RegisterCallback<DetachFromPanelEvent>(e => Hover = "");
            t.RegisterCallback<PointerDownLinkTagEvent>(e =>
            {
                string page = e.linkID ?? "";
                if (page.Length == 0)
                    return;
                e.StopPropagation();
                Hover = "";
                Pressed?.Invoke(page);
            });
        }

        /// <summary>A linked text as it was written (for a note taken of the screen, a test).</summary>
        public static string Plain(string text)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf("<link=", StringComparison.Ordinal) < 0)
                return text;
            var sb = new StringBuilder(text.Length);
            int i = 0;
            while (i < text.Length)
            {
                if (text[i] == '<')
                {
                    int close = text.IndexOf('>', i);
                    if (close > 0)
                    {
                        string tag = text.Substring(i, close - i + 1);
                        if (tag.StartsWith("<link=", StringComparison.Ordinal) || tag == "</link>" || tag.StartsWith("<color=", StringComparison.Ordinal) || tag == "</color>" || tag == "<u>" || tag == "</u>")
                        {
                            i = close + 1;
                            continue;
                        }
                    }
                }
                sb.Append(text[i++]);
            }
            return sb.ToString();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            ForceOn = false;
            Hover = "";
            Pressed = null;
            byFirst = null;
            indexed = null;
            kept.Clear();
        }
    }
}
