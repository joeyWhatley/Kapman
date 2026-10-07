using System.IO;
using Kapman.Game;
using Kapman.Sailing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.EditorTools
{
    /// <summary>
    /// Puts the game into the open scene (the world scene, W_World):
    ///   "--- GAME ---"  the game's root: the tables (KapmanGame), the text panel, the shared variables, and one
    ///                   Script Machine per graph (the graphs are made if they're missing, never overwritten);
    ///   "Market panel"  a child: the market (its own UI document, over the text panel);
    ///   "00 Game map (read me)"  a child holding the guide graph (switched off: it's notes);
    ///   "--- PORTS ---" the port blockouts at their places.
    /// Run again to add what's missing; what's there is kept (the graphs and blockouts are the user's).
    /// Menu: Kapman ▸ Game ▸ Add the game to this scene
    /// </summary>
    public static class GameSetup
    {
        const string UiFolder = "Assets/Kapman/Game/UI";

        [MenuItem("Kapman/Game/Add the game to this scene", priority = 20)]
        public static void AddToOpenScene()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("Kapman: stop Play mode first.");
                return;
            }
            AddToScene(true);
        }

        /// <summary>The game in the open scene (the level builders call this before they save; ported 4 Oct 2026).</summary>
        public static GameObject AddToScene(bool save)
        {
            if (!GraphLibrarySetup.IsSetUp)
                GraphLibrarySetup.Setup();
            var game = GameObject.Find("--- GAME ---") ?? new GameObject("--- GAME ---");

            var kg = game.GetComponent<KapmanGame>() ?? game.AddComponent<KapmanGame>();
            kg.ports = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json");
            kg.goods = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json");
            kg.tradeRules = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json");
            kg.wording = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/wording_793.json");
            kg.chronicle = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/chronicle_793.json");
            kg.shipClasses = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ship_classes_793.json");
            kg.traffic = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/traffic_793.json");
            kg.voyageRules = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/voyage_rules.json");
            kg.seamanship = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/seamanship_793.json");
            kg.raiders = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/raiders_793.json");
            kg.patrols = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/patrols_793.json");
            kg.fairs = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/fairs_793.json");
            kg.events = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/events_793.json");
            kg.reputation = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/reputation_793.json");
            kg.commissions = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/commissions_793.json");
            kg.clock = Object.FindAnyObjectByType<Kapman.Levels.LevelClock>();
            kg.ship = Object.FindAnyObjectByType<SailingShip>();
            AddShipBody();

            var doc = game.GetComponent<UIDocument>() ?? game.AddComponent<UIDocument>();
            doc.panelSettings = PanelSettings();
            doc.sortingOrder = 40;
            var panel = game.GetComponent<TextPanel>() ?? game.AddComponent<TextPanel>();
            panel.style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UiFolder}/TextPanel.uss");
            panel.writing = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/writing.json");
            // the voyage planner: sails her where the merchant chose in the map (Course)
            if (game.GetComponent<VoyagePlanner>() == null)
                game.AddComponent<VoyagePlanner>();
            // the economy's dev keys (B buy and N sell a cask of wine at Dorestad; setYear)
            if (game.GetComponent<EconomyDevKeys>() == null)
                game.AddComponent<EconomyDevKeys>();
            // the encounters: a call begins when she comes to rest at a mooring (the toll man), and the card on screen
            if (game.GetComponent<EncounterRunner>() == null)
                game.AddComponent<EncounterRunner>();
            var card = game.GetComponent<EncounterPanel>() ?? game.AddComponent<EncounterPanel>();
            card.font = AssetDatabase.LoadAssetAtPath<Font>($"{UiFolder}/Fonts/GentiumBookPlus-Regular.ttf");
            // the voyage's beats: the landmarks the level's builder wrote, the wind, the way out; the crew's orders (X, V)
            var director = game.GetComponent<VoyageDirector>() ?? game.AddComponent<VoyageDirector>();
            director.levelId = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "L_NorthSea" ? "northsea_south" : "dorestad";
            director.beats = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/beats_793.json");
            director.lessons = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/lessons_793.json");
            director.landmarks = AssetDatabase.LoadAssetAtPath<TextAsset>($"Assets/Kapman/Data/Levels/landmarks_{director.levelId}.json");

            // the variables the graphs share
            var vars = game.GetComponent<Variables>() ?? game.AddComponent<Variables>();
            if (!vars.declarations.IsDefined(GameGraphs.DockedVar))
                vars.declarations.Set(GameGraphs.DockedVar, null);
            if (!vars.declarations.IsDefined(GameGraphs.LeavingVar))
                vars.declarations.Set(GameGraphs.LeavingVar, false);

            // the market panel: its own document, over the text panel
            var mp = game.transform.Find("Market panel")?.gameObject;
            if (mp == null)
            {
                mp = new GameObject("Market panel");
                mp.transform.SetParent(game.transform, false);
            }
            var mdoc = mp.GetComponent<UIDocument>() ?? mp.AddComponent<UIDocument>();
            mdoc.panelSettings = PanelSettings();
            mdoc.sortingOrder = 41;
            var market = mp.GetComponent<MarketPanel>() ?? mp.AddComponent<MarketPanel>();
            market.style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UiFolder}/MarketPanel.uss");

            // the map view: its own document, under the market (the market docks beside it)
            var mv = game.transform.Find("Map view")?.gameObject;
            if (mv == null)
            {
                mv = new GameObject("Map view");
                mv.transform.SetParent(game.transform, false);
            }
            var vdoc = mv.GetComponent<UIDocument>() ?? mv.AddComponent<UIDocument>();
            vdoc.panelSettings = PanelSettings();
            vdoc.sortingOrder = 39;
            var map = mv.GetComponent<MapView>() ?? mv.AddComponent<MapView>();
            map.style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UiFolder}/MapView.uss");
            // (the map's document before the market's: drawn under it)
            mv.transform.SetSiblingIndex(mp.transform.GetSiblingIndex());

            // the notices line: plain facts as they happen (the voyage's and the graphs')
            var nt = game.transform.Find("Notices")?.gameObject;
            if (nt == null)
            {
                nt = new GameObject("Notices");
                nt.transform.SetParent(game.transform, false);
            }
            var ndoc = nt.GetComponent<UIDocument>() ?? nt.AddComponent<UIDocument>();
            ndoc.panelSettings = PanelSettings();
            ndoc.sortingOrder = 43;
            var notices = nt.GetComponent<Notices>() ?? nt.AddComponent<Notices>();
            notices.font = AssetDatabase.LoadAssetAtPath<Font>($"{UiFolder}/Fonts/GentiumBookPlus-Regular.ttf");

            // the main menu: the title at Play, the pause menu on [Esc] (over everything)
            var mm = game.transform.Find("Main menu")?.gameObject;
            if (mm == null)
            {
                mm = new GameObject("Main menu");
                mm.transform.SetParent(game.transform, false);
            }
            var mmdoc = mm.GetComponent<UIDocument>() ?? mm.AddComponent<UIDocument>();
            mmdoc.panelSettings = PanelSettings();
            mmdoc.sortingOrder = 50;
            var menu = mm.GetComponent<MainMenu>() ?? mm.AddComponent<MainMenu>();
            menu.style = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{UiFolder}/MainMenu.uss");

            // the world's ships in 3D (near the player): the Gislinge boat's model, scaled to each class
            var ws = game.transform.Find("World ships (3D)")?.gameObject;
            if (ws == null)
            {
                ws = new GameObject("World ships (3D)");
                ws.transform.SetParent(game.transform, false);
            }
            var ships3d = ws.GetComponent<Kapman.Game.Sim.WorldShips3D>() ?? ws.AddComponent<Kapman.Game.Sim.WorldShips3D>();
            ships3d.prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Kapman/Prefabs/Ships/Ship_Gislinge.prefab");
            var harbour = ws.GetComponent<Kapman.Game.Sim.HarbourLife>() ?? ws.AddComponent<Kapman.Game.Sim.HarbourLife>();
            harbour.prefab = ships3d.prefab;
            var local = ws.GetComponent<Kapman.Game.Sim.LocalBoats>() ?? ws.AddComponent<Kapman.Game.Sim.LocalBoats>();
            local.prefab = ships3d.prefab;
            if (ws.GetComponent<Kapman.Game.Sim.SeaWatch>() == null)          // the lookout: ships sighted, made out, lost
                ws.AddComponent<Kapman.Game.Sim.SeaWatch>();
            if (ws.GetComponent<Kapman.Game.Sim.ShotDirector>() == null)      // the meetings' event shots
                ws.AddComponent<Kapman.Game.Sim.ShotDirector>();

            // a Script Machine per graph
            Machine(game, GameGraphs.ComingInAndLeaving());
            Machine(game, GameGraphs.InPort());
            Machine(game, GameGraphs.TownExplore());
            Machine(game, GameGraphs.TradeMarket());
            Machine(game, GameGraphs.TradeEachDay());
            Machine(game, GameGraphs.SimWorld());
            Machine(game, GameGraphs.ChronicleEffects());
            Machine(game, GameGraphs.VoyageEachDay());
            Machine(game, GameGraphs.EncounterTrader());
            Machine(game, GameGraphs.EncounterRaiders());
            Machine(game, GameGraphs.EncounterKingsMen());
            Machine(game, GameGraphs.Events());
            Machine(game, GameGraphs.AgroundGettingOff());
            var guide = game.transform.Find("00 Game map (read me)")?.gameObject;
            if (guide == null)
            {
                guide = new GameObject("00 Game map (read me)");
                guide.transform.SetParent(game.transform, false);
            }
            var mapMachine = Machine(guide, GameGraphs.GameMap());
            mapMachine.enabled = false;                                  // (it's notes)

            // (the port blockouts of the first build are gone: every port is its own level now)
            EditorSceneManager.MarkSceneDirty(game.scene);
            if (save)
                EditorSceneManager.SaveOpenScenes();
            Debug.Log("Kapman: the game is in the scene: '--- GAME ---' with its graphs. Open '00 Game map (read me)' to find your way.");
            return game;
        }

        const string ShipBodyType = "Kapman.Ships.ShipBody, Kapman.Ships";
        const string ShipsData = "Assets/Kapman/Data/Ships/ships.json";

        /// <summary>
        /// The player's ship as a body (ship physics, 4 Oct 2026): a ShipBody beside the player's ShipMotion, reading
        /// Data/Ships/ships.json, so both level builders give her one at their next rebuild. Found by name, because the
        /// editor assembly does not reference Kapman.Ships; a missing type or file is reported, not passed over.
        /// </summary>
        static void AddShipBody()
        {
            Kapman.Levels.ShipMotion player = null;
            foreach (var m in Object.FindObjectsByType<Kapman.Levels.ShipMotion>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (m.player)
                    player = m;
            if (player == null)
            {
                Debug.LogWarning("Kapman: no player's ShipMotion in this scene, so no ShipBody was added.");
                return;
            }
            var type = System.Type.GetType(ShipBodyType);
            if (type == null)
            {
                Debug.LogError($"Kapman: {ShipBodyType} is not compiled, so '{player.name}' has no ShipBody: she moves by ShipMotion alone.");
                return;
            }
            var data = AssetDatabase.LoadAssetAtPath<TextAsset>(ShipsData);
            if (data == null)
            {
                Debug.LogError($"Kapman: {ShipsData} is missing, so '{player.name}' has no ShipBody: she moves by ShipMotion alone.");
                return;
            }
            var body = player.GetComponent(type) ?? player.gameObject.AddComponent(type);
            var so = new SerializedObject(body);
            so.FindProperty("ships").objectReferenceValue = data;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static ScriptMachine Machine(GameObject on, ScriptGraphAsset graph)
        {
            foreach (var m in on.GetComponents<ScriptMachine>())
                if (m.nest.macro == graph)
                    return m;
            var machine = on.AddComponent<ScriptMachine>();
            machine.nest.source = GraphSource.Macro;
            machine.nest.macro = graph;
            return machine;
        }

        static PanelSettings PanelSettings()
        {
            string path = $"{UiFolder}/GamePanel.asset";
            var ps = AssetDatabase.LoadAssetAtPath<PanelSettings>(path);
            if (ps != null)
                return ps;
            Directory.CreateDirectory(UiFolder);
            ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution = new Vector2Int(1920, 1080);
            ps.match = 0.5f;
            ps.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Assets/Kapman/Build/UI/BuildTheme.tss");
            ps.sortingOrder = 40;
            AssetDatabase.CreateAsset(ps, path);
            return ps;
        }
    }
}
