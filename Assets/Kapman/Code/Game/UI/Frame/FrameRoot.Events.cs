namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE HAPPENINGS OF THE EVENTS TABLE ON THE CHART (Data/events_793.json; GameEvents). 7 Oct 2026: on the chart
    /// they never came. Each waited "for the map to be closed", and there the map is the game. Now the frame shows
    /// each as a floating window of its own: its title, what is happening, its choices; then what came of the choice
    /// and one way on. It holds the clock when it opens, as a card does, and the player may let the clock run with it
    /// still open. It does not go through the text panel, so one may come while the port's menu is up, and it never
    /// takes the panel from a talk at sea. One thing at a time: it waits while a card, a talk or any other window
    /// that wants an answer is up.
    /// FOR THE BUILDER OF CARD CLOCKS (ruling 12: an ordinary card left open with the clock running takes an option by
    /// itself after seven game days): this window is such a card. Its id is <see cref="EventId"/>; the option that
    /// changes least is the last one of each event in the table as it is written today.
    /// </summary>
    public partial class FrameRoot
    {
        public const string EventId = "event";

        /// <summary>The event whose window is up and not yet answered (null: none).</summary>
        EventDef eventUp;

        partial void EventsEnable()
        {
            eventUp = null;
            GameEvents.Show = ShowEvent;
        }

        partial void EventsDisable()
        {
            GameEvents.Show = null;
            eventUp = null;
        }

        partial void EventsUpdate()
        {
            // (a saved game loaded under it, or the event ended some other way: its window goes)
            if (eventUp != null && GameEvents.Current != eventUp)
            {
                eventUp = null;
                CloseFloat(EventId);
            }
        }

        /// <summary>GameEvents asks: show this one now. False: not now, something else wants an answer.</summary>
        bool ShowEvent(EventDef e)
        {
            if (e == null || floatLayer == null)
                return false;
            var menu = MainMenu.Current;
            if (menu != null && (menu.Open || menu.OnTitle))
                return false;
            var map = MapView.Current;
            if (map != null && map.ChestOpen)
                return false;
            foreach (var f in floats)
                if (f.spec.mustAnswer || f.spec.id == EventId)
                    return false;
            var text = TextPanel.Current;
            var spec = new FloatSpec { id = EventId, title = e.title ?? "", mustAnswer = true, holdsClock = true };   // (no face place: nobody speaks)
            if (text != null && text.TryWriting(e.Key, out string written))
                spec.lines.Add((written, true));
            if (!string.IsNullOrEmpty(e.intention))
                spec.lines.Add((e.intention, true));
            for (int i = 0; i < e.options.Count; i++)
            {
                var o = e.options[i];
                int index = i;
                string label = text != null && text.TryWriting(e.Key + "." + o.key, out string own) ? own : o.label ?? "";
                spec.options.Add(new FloatOption { label = label, pick = () => EventChose(e, index, label) });
            }
            eventUp = e;
            OpenFloat(spec);
            ToFront(floats.Find(f => f.spec.id == EventId));
            return true;
        }

        /// <summary>A choice made in an event's window: what it does is done (GameEvents says the fact in the log), and
        /// the window tells what came of it, with one way on.</summary>
        void EventChose(EventDef e, int index, string chose)
        {
            if (GameEvents.Current != e)
            {
                CloseFloat(EventId);
                return;
            }
            eventUp = null;
            string fact = GameEvents.Choose(index);
            // (what came of it holds the clock until it is gone on from, as a card's end does: 9 Oct 2026)
            var spec = new FloatSpec { id = EventId, title = e.title ?? "", holdsClock = true };
            if (!string.IsNullOrEmpty(chose))
                spec.lines.Add((chose, true));
            spec.result = string.IsNullOrEmpty(fact) ? "Nothing more comes of it." : fact;
            spec.options.Add(new FloatOption { label = "Go on", isDefault = true, pick = () => CloseFloat(EventId) });
            OpenFloat(spec);
        }

        /// <summary>An event's window is up, answered or not.</summary>
        public bool EventOpen => FloatOpen(EventId);
    }
}
