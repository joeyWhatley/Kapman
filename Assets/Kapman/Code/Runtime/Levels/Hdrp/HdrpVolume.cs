using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Hdrp
{
    /// <summary>
    /// THE SKY VOLUME - where the sky, the fog, the clouds and the exposure live on HDRP: one global Volume in the level
    /// scene. The sky, the clock and the weather change its runtime profile (a copy HDRP makes of the asset the first
    /// time it's asked for, so the asset on disk is never written), and add any component the profile lacks, so the
    /// builder only has to put a global Volume in the scene (with or without a profile asset). Set the same volume on
    /// SkyDome and LevelWeather, or neither: left empty, both pick the same one here.
    /// </summary>
    public static class HdrpVolume
    {
        /// <summary>The volume to use: the one given, else the best enabled global Volume in the scene (the highest
        /// priority; among equals, one that already carries sky components; then by name, so every caller agrees).</summary>
        public static Volume Find(Volume given)
        {
            if (given != null)
                return given;
            Volume best = null;
            foreach (var v in UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                if (!v.isGlobal || !v.isActiveAndEnabled)
                    continue;
                if (best == null || Better(v, best))
                    best = v;
            }
            return best;
        }

        static bool Better(Volume a, Volume b)
        {
            if (a.priority != b.priority)
                return a.priority > b.priority;
            int sa = SkyScore(a), sb = SkyScore(b);
            if (sa != sb)
                return sa > sb;
            return string.CompareOrdinal(a.name, b.name) < 0;
        }

        static int SkyScore(Volume v)
        {
            var p = v.sharedProfile;
            if (p == null)
                return 0;
            int s = 0;
            if (p.Has<VisualEnvironment>()) s++;
            if (p.Has<PhysicallyBasedSky>()) s++;
            if (p.Has<Fog>()) s++;
            return s;
        }

        /// <summary>A component on the volume's runtime profile, added if it isn't there (with nothing overridden until a
        /// caller overrides it, so a new component never silently resets what another volume sets).</summary>
        public static T Get<T>(Volume volume) where T : VolumeComponent
        {
            if (volume == null)
                return null;
            var profile = volume.profile;                                 // the runtime copy, never the asset
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = "Kapman sky (runtime)";
                profile.hideFlags = HideFlags.DontSave;
                volume.profile = profile;
            }
            if (!profile.TryGet(out T component))
                component = profile.Add<T>(false);
            return component;
        }
    }
}
