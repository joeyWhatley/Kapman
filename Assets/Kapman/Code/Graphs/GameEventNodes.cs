using Kapman.Game;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>A whole game hour has passed (1 game hour = 1 real minute at the normal pace).</summary>
    [UnitTitle("On game hour")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnGameHour : GlobalEventUnit<double>
    {
        public const string Hook = "Kapman.GameHour";
        protected override string hookName => Hook;

        [DoNotSerialize]
        public ValueOutput hour { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            hour = ValueOutput<double>(nameof(hour));
        }

        protected override void AssignArguments(Flow flow, double args) => flow.SetValue(hour, args);
    }

    /// <summary>A whole game day has passed.</summary>
    [UnitTitle("On game day")]
    [UnitCategory("Events/Kapman")]
    public sealed class OnGameDay : GlobalEventUnit<int>
    {
        public const string Hook = "Kapman.GameDay";
        protected override string hookName => Hook;

        [DoNotSerialize]
        public ValueOutput day { get; private set; }

        protected override void Definition()
        {
            base.Definition();
            day = ValueOutput<int>(nameof(day));
        }

        protected override void AssignArguments(Flow flow, int args) => flow.SetValue(day, args);
    }

    /// <summary>Passes the game's hours and days on to the graphs' event nodes.</summary>
    static class GameEventBridge
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            KapmanGame.HourPassed -= Hour;
            KapmanGame.HourPassed += Hour;
            KapmanGame.DayPassed -= Day;
            KapmanGame.DayPassed += Day;
        }

        static void Hour(double h) => EventBus.Trigger(OnGameHour.Hook, h);

        static void Day(int d) => EventBus.Trigger(OnGameDay.Hook, d);
    }
}
