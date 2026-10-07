using System.Collections.Generic;
using Kapman.Levels.Water;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Kapman.Levels
{
    /// <summary>
    /// A LEVEL'S ONE ROOT - every level scene keeps all it holds under one object with this on it, saved disabled by the
    /// builder, so a level can be loaded beside the current one without anything in it running or drawing (gateways 2.5).
    /// It wakes in two stages:
    ///   Prepare   the ground, the renderers, the colliders and the water come on, with the terrain's drawing held off and
    ///             the level's own volume at weight 0; the level's singletons (its LevelInfo, clock, weather, water, sea,
    ///             stream, sky, ship, camera and listener) stay off, so there is still one sun, one clock, one listener;
    ///   Activate  at the cut: the singletons come on (their guarded OnEnable takes each static Current), the terrain
    ///             draws, the volume goes to weight 1.
    /// Deactivate is the reverse of Activate (the level left behind, until it is unloaded). A level opened alone (the
    /// editor's Play, the shots) has no streamer: its root wakes itself fully as soon as its scene is loaded.
    /// </summary>
    [DisallowMultipleComponent]
    public class LevelRoot : MonoBehaviour
    {
        public enum Stage { Dormant, Prepared, Active }

        [Tooltip("The level's id in chain.json (Assets/Kapman/Data/Chain/chain.json).")]
        public string levelId = "";
        [Tooltip("The level's layer from chain.json (Level0 or Level1): the camera's culling mask and the physics queries " +
                 "keep two resident levels apart by it. The builder sets it on every object under the root.")]
        public string layerName = "Level0";
        [Tooltip("The components that must be one of a kind while two levels are loaded: switched on only at the cut. Empty: " +
                 "found under the root by kind when the level is prepared (the builder fills it, adding the game's own).")]
        public Behaviour[] singletons = new Behaviour[0];
        [Tooltip("The level's own volume (its local fog and mist), at weight 0 until the cut. Optional.")]
        public Volume localVolume;

        /// <summary>Where the level is in its waking.</summary>
        public Stage State { get; private set; }

        Terrain[] terrains;
        bool[] drewHeightmap, drewTrees;
        LevelInfo info;

        /// <summary>The level's LevelInfo (enabled or not).</summary>
        public LevelInfo Info => info != null ? info : (info = GetComponentInChildren<LevelInfo>(true));

        /// <summary>The level's layer index, or -1 while the project has no layer of that name (ProjectSetup adds them).</summary>
        public int Layer => LayerMask.NameToLayer(layerName);

        /// <summary>The level root a component lives under, or null (the persistent scene's own things, or a scene built
        /// before the levels had roots).</summary>
        public static LevelRoot Of(Component component) =>
            component != null ? component.GetComponentInParent<LevelRoot>(true) : null;

        void Awake()
        {
            // a root that is already on (a scene saved before the two-stage enable, or one woken by hand) counts as active
            if (gameObject.activeInHierarchy && State == Stage.Dormant)
                State = Stage.Active;
        }

        /// <summary>The kinds that are one per game while two levels are loaded, in the order they are switched on.</summary>
        public void CollectSingletons()
        {
            var list = new List<Behaviour>();
            void Add<T>() where T : Behaviour
            {
                foreach (var c in GetComponentsInChildren<T>(true))
                    if (!list.Contains(c))
                        list.Add(c);
            }
            Add<LevelInfo>();
            Add<LevelClock>();
            Add<LevelWeather>();
            Add<LevelWater>();
            Add<HdrpSea>();
            Add<RiverCurrent>();
            Add<SkyDome>();
            foreach (var ship in GetComponentsInChildren<ShipMotion>(true))
                if (ship.player && !list.Contains(ship))
                    list.Add(ship);
            Add<Camera>();
            Add<AudioListener>();
            Add<OrbitCamera>();
            Add<LevelSoundscape>();
            Add<DevHud>();
            singletons = list.ToArray();
        }

        /// <summary>The first stage: the level's ground, water and buildings on, its singletons still off. Does nothing
        /// to a level already prepared or active.</summary>
        public void Prepare()
        {
            if (State != Stage.Dormant)
                return;
            if (singletons == null || singletons.Length == 0)
                CollectSingletons();
            foreach (var s in singletons)
                if (s != null)
                    s.enabled = false;
            terrains = GetComponentsInChildren<Terrain>(true);
            drewHeightmap = new bool[terrains.Length];
            drewTrees = new bool[terrains.Length];
            for (int i = 0; i < terrains.Length; i++)
            {
                drewHeightmap[i] = terrains[i].drawHeightmap;
                drewTrees[i] = terrains[i].drawTreesAndFoliage;
                terrains[i].drawHeightmap = false;
                terrains[i].drawTreesAndFoliage = false;
            }
            if (localVolume != null)
                localVolume.weight = 0f;
            State = Stage.Prepared;              // before waking: Awake runs inside SetActive and must not take it for active
            gameObject.SetActive(true);
        }

        /// <summary>The cut: the singletons on, the terrain drawn, the local volume at full weight. Prepares first if needed.</summary>
        public void Activate()
        {
            if (State == Stage.Active)
                return;
            Prepare();
            ShowTerrain(true);
            if (localVolume != null)
                localVolume.weight = 1f;
            foreach (var s in singletons)
                if (s != null)
                    s.enabled = true;
            State = Stage.Active;
        }

        /// <summary>The level left behind: its singletons off (giving up their statics), the terrain not drawn, the local
        /// volume at weight 0. It stays loaded until the streamer unloads it.</summary>
        public void Deactivate()
        {
            if (State != Stage.Active)
                return;
            if (singletons == null || singletons.Length == 0)
                CollectSingletons();
            if (terrains == null)
                RememberTerrain();
            foreach (var s in singletons)
                if (s != null)
                    s.enabled = false;
            ShowTerrain(false);
            if (localVolume != null)
                localVolume.weight = 0f;
            State = Stage.Prepared;
        }

        void RememberTerrain()
        {
            terrains = GetComponentsInChildren<Terrain>(true);
            drewHeightmap = new bool[terrains.Length];
            drewTrees = new bool[terrains.Length];
            for (int i = 0; i < terrains.Length; i++)
            {
                drewHeightmap[i] = terrains[i].drawHeightmap;
                drewTrees[i] = terrains[i].drawTreesAndFoliage;
            }
        }

        void ShowTerrain(bool show)
        {
            if (terrains == null)
                return;
            for (int i = 0; i < terrains.Length; i++)
            {
                if (terrains[i] == null)
                    continue;
                terrains[i].drawHeightmap = show && drewHeightmap[i];
                terrains[i].drawTreesAndFoliage = show && drewTrees[i];
            }
        }

        // ---- a level opened alone wakes itself

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void WatchScenes()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
            WakeLoneLevels();
        }

        static void SceneLoaded(Scene scene, LoadSceneMode mode) => WakeLoneLevels();

        /// <summary>With no streamer running, every dormant level root in the loaded scenes is activated (the editor's Play
        /// from a level scene keeps working once the builders save the roots disabled).</summary>
        public static void WakeLoneLevels()
        {
            if (ChainRuntime.Streaming)
                return;
            foreach (var root in Resources.FindObjectsOfTypeAll<LevelRoot>())
                if (root.State == Stage.Dormant && !root.gameObject.activeSelf && root.gameObject.scene.isLoaded)
                    root.Activate();
        }
    }
}
