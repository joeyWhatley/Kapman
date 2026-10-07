using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE BOOK - the wiki as a window (Design/wiki.md; the Book button of the rack). Three leaves: the kinds (how many
    /// pages of each are open to you), the open pages of a kind (with a box to look for a name), and a page: what it is
    /// in the game, what the record says of it with its source, and the pages it names. A page is open by its own rule
    /// or because you have come upon it (WikiBook.Open). <see cref="Show"/> turns the window to a page from anywhere.
    /// </summary>
    public sealed class BookPanel : FramePanel
    {
        public override string Id => "book";
        public override string Title => "Book";
        public override int Order => 48;

        const int MostRows = 60;
        string kind = "", page = "", find = "";
        readonly List<string> back = new List<string>();

        protected override int Stamp() => FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(17, kind), page), FrameUi.Mix(WikiBook.Version, find));

        /// <summary>Turns the book to a page (and the window to the book, if a frame is given).</summary>
        public static void Show(FrameRoot frame, string pageId)
        {
            if (!(FramePanels.Get("book") is BookPanel book) || WikiBook.Current.Page(pageId) == null)
                return;
            if (book.page.Length > 0 && book.page != pageId)
                book.back.Add(book.page);
            book.page = pageId;
            WikiBook.Meet(pageId);
            frame?.Windows.Show("book");
            book.Redraw();
        }

        protected override void OnLoaded()
        {
            kind = page = find = "";
            back.Clear();
        }

        protected override void Fill(VisualElement body)
        {
            var book = WikiBook.Current;
            if (book.Pages.Count == 0)
            {
                body.Add(FrameUi.P("The book has no pages: Data/story/wiki_793.json is not there."));
                return;
            }
            if (page.Length > 0 && book.Page(page) != null)
                PageLeaf(body, book, book.Page(page));
            else if (kind.Length > 0)
                KindLeaf(body, book);
            else
                KindsLeaf(body, book);
        }

        void KindsLeaf(VisualElement body, WikiBook book)
        {
            body.Add(FrameUi.H("The book"));
            body.Add(FrameUi.P("What you know, and how you came to know it. The rules are here from the first day; the world comes in as you learn it."));
            for (int i = 0; i < WikiBook.Kinds.Length; i++)
            {
                string k = WikiBook.Kinds[i];
                int open = 0, all = 0;
                foreach (var p in book.OfKind(k))
                {
                    if (!p.built)
                        continue;
                    all++;
                    if (book.Open(p))
                        open++;
                }
                if (all == 0)
                    continue;
                body.Add(FrameUi.LnButton(WikiBook.KindNames[i], open == all ? $"{all} pages" : $"{open} of {all} pages open to you", () => { kind = k; find = ""; Redraw(); }));
            }
        }

        void KindLeaf(VisualElement body, WikiBook book)
        {
            body.Add(FrameUi.Row(FrameUi.Btn("The book", () => { kind = ""; find = ""; Redraw(); })));
            body.Add(FrameUi.H(WikiBook.KindName(kind)));
            var field = new TextField { value = find, isDelayed = true };
            field.AddToClassList("frame-book-find");
            field.tooltip = "Look for a name: write a part of it and press Enter.";
            field.RegisterValueChangedCallback(e => { find = e.newValue ?? ""; Redraw(); });
            body.Add(field);
            int shown = 0, closed = 0, more = 0;
            foreach (var p in book.OfKind(kind))
            {
                if (!p.built)
                    continue;
                if (!book.Open(p))
                {
                    closed++;
                    continue;
                }
                if (find.Length > 0 && p.title.IndexOf(find, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                if (shown >= MostRows)
                {
                    more++;
                    continue;
                }
                shown++;
                string id = p.id;
                body.Add(FrameUi.LnButton(p.title, First(p.brief), () => { page = id; back.Clear(); WikiBook.Meet(id); Redraw(); }));
            }
            if (shown == 0)
                body.Add(FrameUi.P(find.Length > 0 ? "No open page has that in its name." : "No page of this kind is open to you yet."));
            if (more > 0)
                body.Add(FrameUi.P($"And {more} more: look for a name to narrow them."));
            if (closed > 0)
                body.Add(FrameUi.P($"{closed} not yet known."));
        }

        void PageLeaf(VisualElement body, WikiBook book, WikiPage p)
        {
            var row = new List<VisualElement>();
            if (back.Count > 0)
                row.Add(FrameUi.Btn("Back", () => { page = back[back.Count - 1]; back.RemoveAt(back.Count - 1); Redraw(); }));
            row.Add(FrameUi.Btn(WikiBook.KindName(p.kind), () => { kind = p.kind; page = ""; back.Clear(); Redraw(); }));
            row.Add(FrameUi.Btn("The book", () => { kind = ""; page = ""; back.Clear(); Redraw(); }));
            body.Add(FrameUi.Row(row.ToArray()));
            body.Add(FrameUi.H(p.title));
            if (!book.Open(p))
            {
                body.Add(FrameUi.P(p.hint.Length > 0 ? p.hint + ": not yet known." : "Not yet known."));
                return;
            }
            if (p.brief.Length > 0)
                body.Add(FrameUi.P(p.brief));
            if (p.game.Length > 0)
            {
                body.Add(FrameUi.H("In the game"));
                foreach (string para in p.game.Split('\n'))
                    if (para.Trim().Length > 0)
                        body.Add(FrameUi.Text(para.Trim()));
            }
            if (p.known.Length > 0)
            {
                body.Add(FrameUi.H("What is known"));
                foreach (string para in p.known.Split('\n'))
                    if (para.Trim().Length > 0)
                        body.Add(FrameUi.Text(para.Trim()));
                if (p.source.Length > 0)
                    body.Add(FrameUi.P("From: " + p.source));
            }
            int named = 0;
            foreach (string id in p.links)
            {
                var to = book.Page(id);
                if (to == null || !to.built || !book.Open(to) || named >= 24)
                    continue;
                if (named++ == 0)
                    body.Add(FrameUi.H("See also"));
                string go = id;
                body.Add(FrameUi.LnButton(to.title, WikiBook.KindName(to.kind), () => { back.Add(page); page = go; WikiBook.Meet(go); Redraw(); }));
            }
        }

        static string First(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            int end = text.IndexOf(". ", StringComparison.Ordinal);
            return end > 0 ? text.Substring(0, end + 1) : text;
        }
    }
}
