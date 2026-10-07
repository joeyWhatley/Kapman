using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.Graphs
{
    /// <summary>
    /// THE BOOK OF RULES: the graphs that hold the game's rules (Resources/GameRules). Found without a scene, so that
    /// a rule can be asked for anywhere and a test can run it.
    /// </summary>
    [CreateAssetMenu(menuName = "Kapman/Rule book", fileName = "GameRules")]
    public sealed class RuleBook : ScriptableObject
    {
        public const string ResourceName = "GameRules";
        [Tooltip("The graphs that hold rules (Assets/Kapman/Game/Graphs/Rules). Each starts with a \"Rule: ...\" node.")]
        public List<ScriptGraphAsset> graphs = new List<ScriptGraphAsset>();
    }
}
