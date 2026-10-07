using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game
{
    /// <summary>
    /// TYPING - true while a text box has the keyboard (a note being written, a search): the game's keys ([Space] pause,
    /// [+] [-], [I] the chest, [O], [M], [Esc]...) leave the keys alone meanwhile. Each panel that has text boxes watches
    /// its own focus (<see cref="Watch"/>).
    /// </summary>
    public static class UiTyping
    {
        public static bool Active { get; private set; }

        /// <summary>Follows the focus within an element (a panel's root): typing while a text box inside it has it.</summary>
        public static void Watch(VisualElement root)
        {
            root.RegisterCallback<FocusInEvent>(e => Active = IsTextBox(e.target as VisualElement), TrickleDown.TrickleDown);
            root.RegisterCallback<FocusOutEvent>(_ => Active = false, TrickleDown.TrickleDown);
        }

        static bool IsTextBox(VisualElement v)
        {
            for (; v != null; v = v.parent)
                if (v is TextField)
                    return true;
            return false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Active = false;
    }
}
