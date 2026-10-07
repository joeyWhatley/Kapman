using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// The frame's part in the linked words (WikiLinks): after everything has written its words for the frame, the texts
    /// get their links; the pointer on one brings a small box by it (the page's name, its kind, a brief word); a click
    /// turns the Book to the page.
    /// </summary>
    public partial class FrameRoot
    {
        VisualElement linkBox;
        Label linkName, linkKind, linkBrief;
        string linkShown = "";

        void LateUpdate()
        {
            if (root == null)
                return;
            FrameFonts.Keep(root);                                        // (the game's typeface, with the faces behind it: FrameFonts)
            if (!WikiLinks.On)
                return;
            WikiLinks.Pressed -= LinkPressed;
            WikiLinks.Pressed += LinkPressed;
            WikiLinks.Pass(root);
            LinkBox();
        }

        void LinkPressed(string page)
        {
            BookPanel.Show(this, page);
        }

        void LinkBox()
        {
            string page = WikiLinks.Hover;
            if (linkBox == null || linkBox.panel == null || linkBox.parent != root)
            {
                linkBox = new VisualElement { pickingMode = PickingMode.Ignore };
                linkBox.AddToClassList("frame-link-box");
                linkName = new Label { pickingMode = PickingMode.Ignore };
                linkName.AddToClassList("frame-link-name");
                linkKind = new Label { pickingMode = PickingMode.Ignore };
                linkKind.AddToClassList("frame-link-kind");
                linkBrief = new Label { pickingMode = PickingMode.Ignore };
                linkBrief.AddToClassList("frame-link-brief");
                linkBox.Add(linkName);
                linkBox.Add(linkKind);
                linkBox.Add(linkBrief);
                linkBox.style.display = DisplayStyle.None;
                root.Add(linkBox);
                linkShown = "";
            }
            var p = page.Length > 0 ? WikiBook.Current.Page(page) : null;
            if (p == null)
            {
                if (linkShown.Length > 0)
                {
                    linkShown = "";
                    linkBox.style.display = DisplayStyle.None;
                }
                return;
            }
            if (linkShown != page)
            {
                linkShown = page;
                linkName.text = p.title;
                linkKind.text = WikiBook.KindName(p.kind);
                linkBrief.text = p.brief.Length > 0 ? p.brief : p.hint;
                linkBox.style.display = DisplayStyle.Flex;
                linkBox.BringToFront();
            }
            // by the pointer: under it and to its right, kept on the screen
            Vector2 at = root.WorldToLocal(WikiLinks.HoverAt);
            float w = root.resolvedStyle.width, h = root.resolvedStyle.height;
            float bw = Mathf.Max(linkBox.resolvedStyle.width, 240f), bh = Mathf.Max(linkBox.resolvedStyle.height, 60f);
            float x = at.x + 16f, y = at.y + 22f;
            if (!float.IsNaN(w) && x + bw > w - 8f)
                x = Mathf.Max(8f, at.x - bw - 12f);
            if (!float.IsNaN(h) && y + bh > h - 8f)
                y = Mathf.Max(8f, at.y - bh - 12f);
            linkBox.style.left = x;
            linkBox.style.top = y;
        }
    }
}
