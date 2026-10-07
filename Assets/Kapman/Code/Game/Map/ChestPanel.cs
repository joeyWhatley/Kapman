using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// THE CHEST'S PAGES (inside the map's chest, MapTable) - the texts, the riddles and the merchant's own notes:
    ///   - a text is shown in its own tongue; the switch turns it into English where the merchant can read it (their own
    ///     tongue and runes at once; Latin once learned or read out by a monk), and says why not where they can't;
    ///   - a riddle shows its words (click a word to mark it as a clue), its tries, the answers tried (struck through),
    ///     and a place to drag the answer: the things the merchant knows are below, to drag up onto it;
    ///   - notes: written beside a riddle or a text (joined to it), or on their own; all searchable.
    /// Plain words; the framing of each text (who gives it, what's said) is the user's to write.
    /// </summary>
    public sealed class ChestPanel
    {
        readonly VisualElement root;                                  // the chest panel (for the dragged ghost)
        VisualElement area;
        string tab;
        int selected = -1;                                            // the item open
        bool english;                                                 // the switch: English (where readable) or its own tongue
        string paletteFilter = "";
        string noteFilter = "";
        int shownVersion = -1;

        // dragging a thing onto a riddle or a kenning: the places it can be dropped, and what dropping there does
        string dragThing;
        Vector2 dragFrom;
        Label ghost;
        readonly List<(VisualElement zone, Func<string, string> drop)> drops = new List<(VisualElement, Func<string, string>)>();

        /// <summary>Asks the map to lay out an old text's sailing directions (its id).</summary>
        public Action<string> LayOut;
        /// <summary>Asks the map to set a hoard's dig mark (its id).</summary>
        public Action<string> MarkDig;
        string hoardOpen;

        public ChestPanel(VisualElement chestRoot) => root = chestRoot;

        /// <summary>Shows a tab ("texts", "riddles", "mine") in the area.</summary>
        public void Show(VisualElement into, string which, bool force = false)
        {
            int version = Chest.Version * 7 + Known.Version;
            if (!force && (UiTyping.Active || (into == area && which == tab && version == shownVersion)))
                return;                                                   // (never under the hands of someone typing)
            area = into;
            tab = which;
            shownVersion = version;
            area.Clear();
            drops.Clear();
            switch (which)
            {
                case "texts": Texts(); break;
                case "riddles": Riddles(); break;
                case "hoards": HoardsPage(); break;
                default: MyNotes(); break;
            }
        }

        void Redraw()
        {
            drops.Clear();
            Show(area, tab, force: true);
        }

        // ------------------------------------------------------------------------------------------ texts

        void Texts()
        {
            var list = Chest.Items.Where(i => i.kind == "text").ToList();
            if (list.Count == 0)
            {
                area.Add(Note("No texts yet. Books, letters and rune sticks you buy, are given or find will be kept here, in their own " +
                              "tongue, with the English beside them where you can read it."));
                return;
            }
            var (left, right) = Columns();
            foreach (var i in list)
                left.Add(ListRow(i, i.Text?.title ?? i.def, i.Text != null ? $"{i.Text.FormName} · {i.Text.language}" : ""));
            var open = Chest.Get(selected);
            if (open == null || open.kind != "text")
                open = list[0];
            selected = open.id;
            TextView(right, open, marking: false);
            if (open.Text != null && open.Text.voyage)
                LegsView(right, open);
            NotesBeside(right, "text:" + open.def);
        }

        /// <summary>The way an old text sails, leg by leg (in its own tongue or English, as the switch is), what's been
        /// found, and laying it out on the map.</summary>
        void LegsView(VisualElement into, ChestItem item)
        {
            var head = new Label("The way it sails");
            head.AddToClassList("chest-subhead");
            into.Add(head);
            bool eng = english && Chest.CanRead(item);
            foreach (var leg in ChestData.LegsOf(item.def))
            {
                var row = new VisualElement();
                row.AddToClassList("chest-leg");
                bool found = Chest.Found(leg);
                var words = new Label($"{leg.order}. " + ((eng && !string.IsNullOrEmpty(leg.translation)) ? leg.translation : leg.original));
                words.AddToClassList("chest-leg-words");
                row.Add(words);
                var meta = Note($"{leg.bearing} · {leg.days}" + (string.IsNullOrEmpty(leg.landmarks) ? "" : " · " + leg.landmarks) +
                                (found ? $" · found: {leg.toName}" : ""));
                row.Add(meta);
                row.EnableInClassList("found", found);
                into.Add(row);
            }
            var lay = new Button(() => LayOut?.Invoke(item.def)) { text = "Lay it out on the map" };
            lay.AddToClassList("map-button");
            lay.tooltip = "Pin where each leg ends with the dividers' help; sail there to find out.";
            into.Add(lay);
        }

        // ------------------------------------------------------------------------------------------ hoards

        void HoardsPage()
        {
            var list = Hoards.Open.ToList();
            if (list.Count == 0)
            {
                area.Add(Note("No hoards yet. A clue to a buried hoard can be won: answer a set of riddles, read a verse, find a stone."));
                return;
            }
            var (left, right) = Columns();
            var open = list.FirstOrDefault(h => h.id == hoardOpen) ?? list[0];
            hoardOpen = open.id;
            foreach (var h in list)
            {
                var hoard = h;
                bool lifted = Hoards.Lifted(h.id);
                var row = new Button(() => { hoardOpen = hoard.id; Redraw(); });
                row.AddToClassList("chest-list-row");
                row.EnableInClassList("on", h.id == hoardOpen);
                var a = new Label(lifted ? h.name : $"A buried hoard near {GameData.Port(h.seaPort ?? h.port)?.name ?? "a coast"}");
                a.AddToClassList("chest-list-title");
                var b = new Label(lifted ? "lifted" : Hoards.MarkOf(h.id) != null ? "your dig mark is set" : "no dig mark yet");
                b.AddToClassList("chest-list-sub");
                row.Add(a);
                row.Add(b);
                left.Add(row);
            }
            HoardView(right, open);
        }

        void HoardView(VisualElement into, HoardDef h)
        {
            bool lifted = Hoards.Lifted(h.id);
            var head = new Label(lifted ? h.name : "A clue to a buried hoard");
            head.AddToClassList("chest-title");
            into.Add(head);
            var port = GameData.Port(h.seaPort ?? h.port);
            into.Add(Note($"By sea: from {port?.name ?? "the nearest port"}, {h.seaDirection}, about {h.seaDays:0.#} days. {h.seaRoute}"));
            var sub = new Label("The clue's measures (a pace is five feet, 1.48 m; lay them out with the dividers and string)");
            sub.AddToClassList("chest-subhead");
            into.Add(sub);
            int i = 1;
            foreach (var m in h.measures)
                into.Add(Note($"{i++}. {m}"));
            var lmHead = new Label("Its landmarks (on the map when you're close in)");
            lmHead.AddToClassList("chest-subhead");
            into.Add(lmHead);
            into.Add(Note(string.Join(" · ", h.landmarks.Select(l => l.name))));
            if (!lifted)
            {
                var bar = new VisualElement();
                bar.AddToClassList("chest-switch");
                var mark = new Button(() => MarkDig?.Invoke(h.id)) { text = Hoards.MarkOf(h.id) != null ? "Move your dig mark" : "Set your dig mark" };
                mark.AddToClassList("map-button");
                bar.Add(mark);
                string why = Hoards.WhyNotDig(h);
                var dig = new Button(() => { Voyage.Say(Hoards.Dig(h)); Redraw(); }) { text = "Dig at your mark" };
                dig.AddToClassList("map-button");
                dig.SetEnabled(why == null);
                dig.tooltip = why ?? $"The crew dig for half a day at your mark (within {Hoards.Within:0} m of it, they'll strike it).";
                bar.Add(dig);
                into.Add(bar);
                if (why != null)
                    into.Add(Note(why));
            }
            else
            {
                into.Add(Note(h.held));
                if (!string.IsNullOrEmpty(h.foundStory))
                    into.Add(Note("In our own time: " + h.foundStory));
            }
            NotesBeside(into, "hoard:" + h.id);
        }

        // ------------------------------------------------------------------------------------------ riddles

        void Riddles()
        {
            var list = Chest.Items.Where(i => i.kind == "riddle" || i.kind == "verse").OrderBy(i => i.solved).ThenBy(i => i.resting).ThenByDescending(i => i.got).ToList();
            if (list.Count == 0)
            {
                area.Add(Note("No riddles yet. Riddles you're given, buy or find will be kept here to work on at sea."));
                return;
            }
            var (left, right) = Columns();
            foreach (var i in list)
            {
                if (i.kind == "verse")
                {
                    var v = i.Verse;
                    left.Add(ListRow(i, i.Text?.title ?? i.def, $"a verse · {i.slotsDone.Count} of {v?.slots.Count ?? 0} kennings read"));
                    continue;
                }
                var r = i.Riddle;
                string state = i.solved ? "answered" : i.resting ? "resting" : $"{i.tries} {(i.tries == 1 ? "try" : "tries")} left";
                left.Add(ListRow(i, i.Text?.title ?? i.def, $"{Hardness(r)} · {state}"));
            }
            var open = Chest.Get(selected);
            if (open == null || (open.kind != "riddle" && open.kind != "verse"))
                open = list[0];
            selected = open.id;
            if (open.kind == "verse")
                VerseView(right, open);
            else
                RiddleView(right, open);
        }

        void RiddleView(VisualElement into, ChestItem item)
        {
            var r = item.Riddle;
            TextView(into, item, marking: !item.solved);
            // where it stands
            var state = new Label();
            state.AddToClassList("chest-state");
            state.enableRichText = true;
            if (item.solved)
            {
                var answer = r.answers.Select(ChestData.Thing).FirstOrDefault(t => t != null);
                state.text = "Answered: " + (answer != null ? answer.name : r.answerWords) + (item.rewardDue ? ". Its reward waits where you had it: take the answer back." : ".");
            }
            else if (item.resting)
                state.text = "Out of tries. It rests until you learn something new that bears on it: a good seen, a place visited, a text read, news heard.";
            else
                state.text = "Tries left: " + new string('●', item.tries) + new string('○', Math.Max(0, r.tries - item.tries));
            into.Add(state);
            if (item.tried.Count > 0)
            {
                var tried = new Label("Tried: " + string.Join(", ", item.tried.Select(t => "<s>" + string.Join(" and ", t.Split('+').Select(x => ChestData.Thing(x)?.name ?? x)) + "</s>")));
                tried.enableRichText = true;
                tried.AddToClassList("chest-tried");
                into.Add(tried);
            }
            // the answer: dragged here from the things known, below
            if (!item.solved && !item.resting && r.SlotCount > 1)
            {
                // two or three answers, given at once: fill each place, then answer
                var row = new VisualElement();
                row.AddToClassList("chest-slots");
                for (int s = 0; s < r.SlotCount; s++)
                {
                    int slot = s;
                    string have = item.filled != null && slot < item.filled.Length ? item.filled[slot] : null;
                    var zone = new VisualElement();
                    zone.AddToClassList("chest-drop");
                    zone.AddToClassList("small");
                    zone.EnableInClassList("done", have != null);
                    zone.Add(new Label(have != null ? ChestData.Thing(have)?.name ?? have : $"answer {slot + 1}") { pickingMode = PickingMode.Ignore });
                    drops.Add((zone, thing => { Chest.Fill(item, slot, thing); return ""; }));
                    row.Add(zone);
                }
                into.Add(row);
                var judge = new Button(() => { string fact = Chest.Judge(item); if (fact.Length > 0) Voyage.Say(fact); Redraw(); }) { text = $"Give these {r.SlotCount} as the answer" };
                judge.AddToClassList("map-button");
                judge.tooltip = "This riddle has more than one answer: all are judged together, and a try is lost if any is wrong.";
                into.Add(judge);
                into.Add(Palette());
            }
            else if (!item.solved && !item.resting)
            {
                var zone = new VisualElement();
                zone.AddToClassList("chest-drop");
                var dropText = new Label("Drag the answer here");
                dropText.pickingMode = PickingMode.Ignore;
                zone.Add(dropText);
                into.Add(zone);
                drops.Add((zone, thing => Chest.Answer(item, thing)));
                into.Add(Palette());
            }
            NotesBeside(into, "riddle:" + item.def);
        }

        /// <summary>A verse: each kenning gets its plain meaning, dragged onto it.</summary>
        void VerseView(VisualElement into, ChestItem item)
        {
            var v = item.Verse;
            TextView(into, item, marking: false);
            if (v == null)
                return;
            if (!string.IsNullOrEmpty(v.hint))
            {
                var hint = new Label(v.hint);
                hint.AddToClassList("chest-state");
                into.Add(hint);
            }
            var head = new Label("Its kennings: drag what each means onto it");
            head.AddToClassList("chest-subhead");
            into.Add(head);
            for (int i = 0; i < v.slots.Count; i++)
            {
                int slot = i;
                var s = v.slots[i];
                var row = new VisualElement();
                row.AddToClassList("chest-kenning");
                var words = new Label(s.original);
                words.AddToClassList("chest-kenning-words");
                row.Add(words);
                bool done = item.slotsDone.Contains(i);
                var zone = new VisualElement();
                zone.AddToClassList("chest-drop");
                zone.AddToClassList("small");
                var label = new Label(done ? ChestData.Thing(s.thing)?.name ?? s.means : "?") { pickingMode = PickingMode.Ignore };
                zone.Add(label);
                zone.EnableInClassList("done", done);
                if (done)
                    zone.tooltip = s.literal;
                else
                    drops.Add((zone, thing => Chest.AnswerKenning(item, slot, thing)));
                row.Add(zone);
                into.Add(row);
            }
            if (item.solved && !string.IsNullOrEmpty(v.answer))
            {
                var answer = new Label(v.answer);
                answer.AddToClassList("chest-state");
                into.Add(answer);
            }
            if (!item.solved)
                into.Add(Palette());
            NotesBeside(into, "verse:" + item.def);
        }

        static string Hardness(RiddleDef r) =>
            r == null ? "" : r.difficulty >= 5 ? "very hard" : r.difficulty >= 4 ? "hard" : r.difficulty >= 3 ? "middling" : r.difficulty >= 2 ? "fair" : "easy";

        // ------------------------------------------------------------------------------------------ one text, with the switch

        void TextView(VisualElement into, ChestItem item, bool marking)
        {
            var t = item.Text;
            if (t == null)
            {
                into.Add(Note("This writing is missing from the tables."));
                return;
            }
            var head = new Label(t.title);
            head.AddToClassList("chest-title");
            into.Add(head);
            double ago = KapmanGame.Current != null ? KapmanGame.Current.Day - item.got : 0.0;
            var origin = Note($"On a {t.FormName}, in {t.language}" + (string.IsNullOrEmpty(item.from) ? "" : $", from {item.from}") +
                              (item.at != null ? $" at {GameData.Port(item.at)?.name ?? item.at}" : "") +
                              $", {(ago < 1.0 ? "today" : ago < 2.0 ? "yesterday" : $"{ago:0} days ago")}." +
                              (t.toldLater ? " Told of the time, written down later." : ""));
            into.Add(origin);
            // the switch
            string why = Chest.WhyNotRead(item);
            bool canRead = why == null;
            bool showEnglish = english && canRead;
            var bar = new VisualElement();
            bar.AddToClassList("chest-switch");
            var own = new Button(() => { english = false; Redraw(); }) { text = t.language };
            own.AddToClassList("map-button");
            own.EnableInClassList("on", !showEnglish);
            var eng = new Button(() => { english = true; Redraw(); }) { text = "English" };
            eng.AddToClassList("map-button");
            eng.EnableInClassList("on", showEnglish);
            eng.SetEnabled(canRead);
            eng.tooltip = canRead ? "Turn it into English: your reading." : why;
            bar.Add(own);
            bar.Add(eng);
            if (!canRead)
            {
                var whyLabel = Note(why);
                whyLabel.AddToClassList("chest-why");
                bar.Add(whyLabel);
            }
            into.Add(bar);
            // the words: each line a row; a riddle's words can be clicked to mark them as clues
            string words = showEnglish ? t.translation : t.original;
            int baseIndex = showEnglish ? 100000 : 0;
            var body = new VisualElement();
            body.AddToClassList("chest-text");
            if (t.script == "runic" && !showEnglish)
                body.AddToClassList("runic");
            int index = 0;
            foreach (string line in (words ?? "").Replace("\r", "").Split('\n'))
            {
                var row = new VisualElement();
                row.AddToClassList("chest-line");
                foreach (string w in line.Split(' '))
                {
                    if (w.Length == 0)
                        continue;
                    int at = baseIndex + index++;
                    var word = new Label(w);
                    word.AddToClassList("chest-word");
                    word.EnableInClassList("marked", item.marks.Contains(at));
                    if (marking)
                        word.RegisterCallback<ClickEvent>(_ => { Chest.ToggleMark(item, at); Redraw(); });
                    row.Add(word);
                }
                body.Add(row);
            }
            into.Add(body);
            if (marking)
                into.Add(Note("Click a word to mark it as a clue."));
            if (t.script == "runic" && t.normalized != null)
                RunePuzzle(into, item);
            var credit = Note((showEnglish && !string.IsNullOrEmpty(t.translator) ? $"Translated by {t.translator}{(t.translated > 0 ? " (" + t.translated + ")" : "")}. " : "") + t.credit);
            credit.AddToClassList("chest-credit");
            credit.tooltip = t.where;
            into.Add(credit);
        }

        // ------------------------------------------------------------------------------------------ reading runes yourself

        /// <summary>Under each rune, the sound it stands for here: click to turn to the next it can be. A word read lights
        /// up; one whose spelling can't be matched sound for sound is given.</summary>
        void RunePuzzle(VisualElement into, ChestItem item)
        {
            var head = new Label(item.runesRead ? "You've read these runes yourself." : "Read them yourself: under each rune, choose the sound it stands for here.");
            head.AddToClassList("chest-subhead");
            into.Add(head);
            var words = RuneReading.Words(item.Text.original);
            var targets = RuneReading.Targets(item.Text.normalized);
            bool aligned = words.Count == targets.Count;
            var box = new VisualElement();
            box.AddToClassList("rune-puzzle");
            int at = 0;
            for (int w = 0; w < words.Count; w++)
            {
                var word = new VisualElement();
                word.AddToClassList("rune-word");
                var chosen = new List<string>();
                int first = at;
                foreach (var rune in words[w])
                {
                    var sounds = RuneReading.SoundsOf(rune);
                    int here = at++;
                    string sound = item.runeChoice.TryGetValue(here, out int s) && s < sounds.Length ? sounds[s] : null;
                    chosen.Add(sound);
                    var col = new VisualElement();
                    col.AddToClassList("rune-col");
                    var r = new Label(rune);
                    r.AddToClassList("rune-glyph");
                    r.tooltip = "Can stand for: " + string.Join(", ", sounds);
                    col.Add(r);
                    var turn = new Button(() => { string fact = Chest.TurnRune(item, here, sounds.Length); if (fact.Length > 0) Voyage.Say(fact); Redraw(); })
                               { text = sound ?? "?" };
                    turn.AddToClassList("rune-sound");
                    turn.SetEnabled(!item.runesRead);
                    col.Add(turn);
                    word.Add(col);
                }
                if (aligned)
                {
                    bool given = !RuneReading.Readable(words[w], targets[w]);
                    bool read = item.runesRead || given || RuneReading.IsRead(chosen, targets[w]);
                    word.EnableInClassList("read", read);
                    if (read)
                        word.tooltip = targets[w] + (given ? " (its spelling can't be matched sound for sound: given)" : "");
                }
                box.Add(word);
            }
            into.Add(box);
            if (!aligned)
                into.Add(Note("(The runes and the spoken words don't line up word for word here: read it as a whole.)"));
        }

        // ------------------------------------------------------------------------------------------ the things known (drag them)

        VisualElement Palette()
        {
            var box = new VisualElement();
            box.AddToClassList("chest-palette");
            var head = new VisualElement();
            head.AddToClassList("chest-palette-head");
            head.Add(new Label("Things you know: drag one up") { name = "palette-title" });
            var search = new TextField { value = paletteFilter, tabIndex = -1 };
            search.AddToClassList("chest-search");
            search.RegisterValueChangedCallback(e => { paletteFilter = e.newValue; FillChips(box); });
            head.Add(search);
            box.Add(head);
            var chips = new VisualElement { name = "chips" };
            chips.AddToClassList("chest-chips");
            box.Add(chips);
            FillChips(box);
            return box;
        }

        void FillChips(VisualElement box)
        {
            var chips = box.Q<VisualElement>("chips");
            chips.Clear();
            string f = paletteFilter.Trim();
            foreach (var group in Chest.KnownThings().Where(t => f.Length == 0 || t.name.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0).GroupBy(t => t.kind))
            {
                var kind = new Label(KindName(group.Key));
                kind.AddToClassList("chest-chip-kind");
                chips.Add(kind);
                foreach (var th in group)
                    chips.Add(Chip(th));
            }
        }

        static string KindName(string kind)
        {
            switch (kind)
            {
                case "good": return "Goods";
                case "place": return "Places";
                case "animal": return "Animals";
                case "god": return "Gods";
                case "person": return "People";
                case "weather": return "Weather";
                case "body": return "The body";
                case "tool": return "Tools";
                case "ship": return "Ships";
                default: return "Things";
            }
        }

        VisualElement Chip(ThingDef th)
        {
            var chip = new Label(th.name);
            chip.AddToClassList("chest-chip");
            string id = th.id;
            chip.RegisterCallback<PointerDownEvent>(e =>
            {
                if (e.button != 0)
                    return;
                dragThing = id;
                dragFrom = e.position;
                chip.CapturePointer(e.pointerId);
                e.StopPropagation();
            });
            chip.RegisterCallback<PointerMoveEvent>(e =>
            {
                if (dragThing != id || !chip.HasPointerCapture(e.pointerId))
                    return;
                if (ghost == null && ((Vector2)e.position - dragFrom).magnitude > 4f)
                {
                    ghost = new Label(th.name) { pickingMode = PickingMode.Ignore };
                    ghost.AddToClassList("chest-chip");
                    ghost.AddToClassList("ghost");
                    ghost.style.position = Position.Absolute;
                    root.Add(ghost);
                }
                if (ghost != null)
                {
                    var p = root.WorldToLocal(e.position);
                    ghost.style.left = p.x + 6f;
                    ghost.style.top = p.y + 6f;
                    foreach (var (zone, _) in drops)
                        zone.EnableInClassList("over", zone.worldBound.Contains(e.position));
                }
            });
            chip.RegisterCallback<PointerUpEvent>(e =>
            {
                if (chip.HasPointerCapture(e.pointerId))
                    chip.ReleasePointer(e.pointerId);
                Func<string, string> drop = null;
                if (ghost != null)
                    foreach (var (zone, act) in drops)
                        if (zone.worldBound.Contains(e.position))
                            drop = act;
                ghost?.RemoveFromHierarchy();
                ghost = null;
                string thing = dragThing;
                dragThing = null;
                if (drop == null)
                    return;
                string fact = drop(thing);
                if (!string.IsNullOrEmpty(fact))
                    Voyage.Say(fact);
                Redraw();
            });
            chip.tooltip = th.kind == "place" ? "A place: drag it onto the riddle." : "Drag it onto the riddle.";
            return chip;
        }

        // ------------------------------------------------------------------------------------------ notes

        /// <summary>The notes joined to a riddle or text, and a box to add one.</summary>
        void NotesBeside(VisualElement into, string link)
        {
            var head = new Label("Your notes on it");
            head.AddToClassList("chest-subhead");
            into.Add(head);
            foreach (var n in Chest.NotesFor(link))
                into.Add(NoteEditor(n));
            var add = new Button(() => { Chest.AddNote("", link); Redraw(); }) { text = "Add a note" };
            add.AddToClassList("map-button");
            into.Add(add);
        }

        void MyNotes()
        {
            var bar = new VisualElement();
            bar.AddToClassList("chest-switch");
            var search = new TextField { value = noteFilter, tabIndex = -1 };
            search.AddToClassList("chest-search");
            search.RegisterValueChangedCallback(e => { noteFilter = e.newValue; Redraw(); });
            bar.Add(new Label("Find:"));
            bar.Add(search);
            var add = new Button(() => { Chest.AddNote(""); noteFilter = ""; Redraw(); }) { text = "New note" };
            add.AddToClassList("map-button");
            bar.Add(add);
            area.Add(bar);
            var scroll = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.AddToClassList("chest-scroll");
            area.Add(scroll);
            var (found, writings) = Chest.Search(noteFilter);
            if (found.Count == 0 && noteFilter.Trim().Length == 0)
                scroll.Add(Note("Nothing written yet. Add notes here, or beside a riddle or a text (they're joined to it)."));
            foreach (var n in found.OrderByDescending(x => x.day))
                scroll.Add(NoteEditor(n));
            if (noteFilter.Trim().Length > 0)
            {
                var head = new Label("In your writings");
                head.AddToClassList("chest-subhead");
                scroll.Add(head);
                if (writings.Count == 0)
                    scroll.Add(Note("Not found in any writing in the chest."));
                foreach (var i in writings)
                {
                    var item = i;
                    var row = new Button(() => { selected = item.id; tab = item.kind == "riddle" ? "riddles" : "texts"; OpenTab?.Invoke(tab); }) { text = (i.Text?.title ?? i.def) + (i.kind == "riddle" ? " (a riddle)" : "") };
                    row.AddToClassList("chest-list-row");
                    scroll.Add(row);
                }
            }
        }

        /// <summary>Asks the chest to switch to a tab (a search result opened).</summary>
        public Action<string> OpenTab;

        VisualElement NoteEditor(PlayerNote n)
        {
            var box = new VisualElement();
            box.AddToClassList("chest-note");
            var field = new TextField { multiline = true, value = n.text, tabIndex = -1 };
            field.AddToClassList("chest-note-field");
            field.RegisterValueChangedCallback(e => n.text = e.newValue ?? "");     // (kept as typed; the chest is told on leaving the box)
            field.RegisterCallback<FocusOutEvent>(_ => { n.text = ""; Chest.EditNote(n, field.value); });
            box.Add(field);
            var foot = new VisualElement();
            foot.AddToClassList("chest-note-foot");
            foreach (var l in n.links)
                foot.Add(Note(LinkName(l)));
            var pin = new Button(() =>
            {
                var game = KapmanGame.Current;
                if (game != null)
                {
                    var w = MapWorld.ToWorld(game.ShipPosition);                      // ported 4 Oct: the level's place in the world, not the floating origin's
                    Chest.Pin(n, w.x, w.z);
                    Redraw();
                }
            }) { text = double.IsNaN(n.x) ? "Pin where she is" : "Pinned on the map" };
            pin.AddToClassList("map-button");
            pin.tooltip = "Pins the note to the map where your ship is now.";
            foot.Add(pin);
            var del = new Button(() => { Chest.DeleteNote(n); Redraw(); }) { text = "Scrap it" };
            del.AddToClassList("map-button");
            foot.Add(del);
            box.Add(foot);
            return box;
        }

        static string LinkName(string link)
        {
            int c = link.IndexOf(':');
            string kind = c > 0 ? link.Substring(0, c) : link, id = c > 0 ? link.Substring(c + 1) : "";
            switch (kind)
            {
                case "riddle": return "On: " + (ChestData.Text(ChestData.Riddle(id)?.textId)?.title ?? id);
                case "text": return "On: " + (ChestData.Text(id)?.title ?? id);
                case "port": return "At: " + (GameData.Port(id)?.name ?? id);
                case "thing": return "About: " + (ChestData.Thing(id)?.name ?? id);
                default: return link;
            }
        }

        // ------------------------------------------------------------------------------------------ bits

        (VisualElement left, VisualElement right) Columns()
        {
            var cols = new VisualElement();
            cols.AddToClassList("chest-columns");
            var left = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            left.AddToClassList("chest-list");
            var right = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            right.AddToClassList("chest-detail");
            cols.Add(left);
            cols.Add(right);
            area.Add(cols);
            return (left.contentContainer, right.contentContainer);
        }

        VisualElement ListRow(ChestItem i, string title, string sub)
        {
            var item = i;
            var row = new Button(() => { selected = item.id; Redraw(); });
            row.AddToClassList("chest-list-row");
            row.EnableInClassList("on", i.id == selected);
            var a = new Label(title);
            a.AddToClassList("chest-list-title");
            var b = new Label(sub);
            b.AddToClassList("chest-list-sub");
            row.Add(a);
            row.Add(b);
            return row;
        }

        static Label Note(string t)
        {
            var l = new Label(t);
            l.AddToClassList("map-chest-note");
            return l;
        }
    }
}
