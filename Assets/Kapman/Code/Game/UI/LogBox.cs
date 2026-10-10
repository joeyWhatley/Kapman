using System.Collections.Generic;
using System.Text;

namespace Kapman.Game
{
    /// <summary>One row of a box: what it is, how much, and what it changed by ("" none). `sub`: it hangs under the row
    /// above it. `words`: a whole sentence across the box (the label alone).</summary>
    public struct BoxRow
    {
        public string label, value, change;
        public bool sub, words;
    }

    /// <summary>Something that gives a box of rows for its hover card (a line of the log).</summary>
    public interface IHoverBox
    {
        LogBox Box();
    }

    /// <summary>
    /// THE BOX THAT COMES UP OVER A LINE OF THE LOG (the user, 10 Oct 2026: "... if you hover over it has an info box pop
    /// up with the more key details but laid out like / Plus however many days", "Like how ck does it"; by the picker:
    /// "Short, with signed numbers"). A heading and rows, one fact a row: what it is on the left, how much on the right,
    /// and what it changed by, signed. The log's own line is a few words and the number that changed; everything else
    /// is here. It is for the LOG, where the game reports. What people say (cards, talk) stays in whole sentences.
    /// </summary>
    public sealed class LogBox : IHoverBox
    {
        public string head;
        public readonly List<BoxRow> rows = new List<BoxRow>();

        public LogBox(string head = "")
        {
            this.head = head ?? "";
        }

        public LogBox Box() => this;

        /// <summary>A row: what it is, how much, and what it changed by.</summary>
        public LogBox Row(string label, string value = "", string change = "")
        {
            if (!string.IsNullOrEmpty(label) || !string.IsNullOrEmpty(value))
                rows.Add(new BoxRow { label = label ?? "", value = value ?? "", change = change ?? "" });
            return this;
        }

        /// <summary>A row that hangs under the one above it.</summary>
        public LogBox Sub(string label, string value = "", string change = "")
        {
            if (!string.IsNullOrEmpty(label) || !string.IsNullOrEmpty(value))
                rows.Add(new BoxRow { label = label ?? "", value = value ?? "", change = change ?? "", sub = true });
            return this;
        }

        /// <summary>A whole sentence across the box (what was said in full, a thing's description).</summary>
        public LogBox Say(string sentence)
        {
            if (!string.IsNullOrWhiteSpace(sentence))
                rows.Add(new BoxRow { label = sentence.Trim(), value = "", change = "", words = true });
            return this;
        }

        /// <summary>A telling of several sentences, a sentence a row.</summary>
        public LogBox SayEach(string telling)
        {
            foreach (string s in Kapman.Game.Frame.FrameLog.Sentences(telling))
                Say(s);
            return this;
        }

        public bool Empty => head.Length == 0 && rows.Count == 0;

        /// <summary>A sum that changed, signed: "+6 d", "-37 d" ("" for none). The plain hyphen is the minus: the
        /// game's font has no other.</summary>
        public static string Signed(double amount, string unit = "d")
        {
            double r = System.Math.Round(amount);
            if (r == 0.0)
                return "";
            return (r > 0.0 ? "+" : "-") + System.Math.Abs(r).ToString("0") + (string.IsNullOrEmpty(unit) ? "" : " " + unit);
        }

        /// <summary>The box as plain text (a test, a journal): the heading, then a row a line.</summary>
        public string Text()
        {
            var sb = new StringBuilder(head);
            foreach (var r in rows)
            {
                if (sb.Length > 0)
                    sb.Append('\n');
                sb.Append(r.sub ? "  " : "").Append(r.label);
                if (r.value.Length > 0)
                    sb.Append("  ").Append(r.value);
                if (r.change.Length > 0)
                    sb.Append("  ").Append(r.change);
            }
            return sb.ToString();
        }

        /// <summary>The box as plain data (a saved game keeps the log's lines with their boxes).</summary>
        public Dictionary<string, object> Capture()
        {
            var list = new List<object>(rows.Count);
            foreach (var r in rows)
                list.Add(new Dictionary<string, object> { ["l"] = r.label, ["v"] = r.value, ["c"] = r.change, ["s"] = r.sub, ["w"] = r.words });
            return new Dictionary<string, object> { ["head"] = head, ["rows"] = list };
        }

        /// <summary>A box out of a saved game (null where there was none).</summary>
        public static LogBox Restore(Dictionary<string, object> saved)
        {
            if (saved == null)
                return null;
            var box = new LogBox(Json.Str(saved, "head"));
            foreach (object o in Json.Arr(saved, "rows") ?? new List<object>())
                box.rows.Add(new BoxRow { label = Json.Str(o, "l"), value = Json.Str(o, "v"), change = Json.Str(o, "c"), sub = Json.Bool(o, "s"), words = Json.Bool(o, "w") });
            return box.Empty ? null : box;
        }
    }
}
