using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// Points Unity's visual scripting at the game's code, so every Kapman type and its members are nodes in the
    /// fuzzy finder (besides the game's own nodes, under Kapman/...), then builds the node library again.
    /// Menu: Kapman ▸ Graphs ▸ Set up visual scripting (the node library)
    /// </summary>
    public static class GraphLibrarySetup
    {
        static readonly string[] Assemblies = { "Kapman.Core", "Kapman.Runtime", "Kapman.Game", "Kapman.Graphs" };   // (ported 4 Oct: no Kapman.HDRP assembly in the rebuild)

        /// <summary>Visual scripting loads its settings the first time it's used in a session; this makes sure it has.</summary>
        static void EnsureLoaded()
        {
            if (PluginContainer.initialized && BoltCore.Configuration != null)
                return;
            typeof(PluginContainer).GetMethod("Initialize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(null, null);
        }

        [MenuItem("Kapman/Graphs/Set up visual scripting (the node library)", priority = 1)]
        public static void Setup()
        {
            EnsureLoaded();
            var options = BoltCore.Configuration.assemblyOptions;
            int added = 0;
            foreach (var name in Assemblies)
            {
                var loose = new LooseAssemblyName(name);
                if (!options.Contains(loose))
                {
                    options.Add(loose);
                    added++;
                }
            }
            BoltCore.Configuration.Save();
            // (Save only changes them in memory: this writes ProjectSettings/VisualScriptingSettings.asset, so the
            // setting outlives the session - without it the node library forgot the game's code at the next start)
            BoltCore.Configuration.SaveProjectSettingsAsset(true);
            Codebase.UpdateSettings();
            UnitBase.Rebuild();
            Debug.Log($"Kapman: visual scripting's node library has the game's code ({added} assemblies added; " +
                      $"{options.Count} in all). Its nodes are under Kapman/... in the fuzzy finder.");
        }

        /// <summary>Whether the node library already has the game's assemblies.</summary>
        public static bool IsSetUp
        {
            get
            {
                EnsureLoaded();
                return BoltCore.Configuration != null &&
                       Assemblies.All(n => BoltCore.Configuration.assemblyOptions.Contains(new LooseAssemblyName(n)));
            }
        }
    }
}
