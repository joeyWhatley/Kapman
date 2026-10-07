using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE WORLD'S SHIPS IN 3D - when a ship of the world simulation comes within sight of the player's, a real hull is
    /// put on the water where the simulation says she is: the Gislinge boat's model scaled to her class (the user: "It's
    /// alright to just use larger versions of the same model"), riding the sea, turned to her heading, moved smoothly
    /// between the simulation's hourly steps. Bodies are only for show (no physics, no colliders); at most a few, the
    /// nearest; they go when she's out of sight. A ship the simulation has over land (straight passages until the sea
    /// lanes are baked) isn't shown. Ships anchored off a full landing, and coasting craft lying up for the night, show
    /// with their sail down. They keep clear of each other in the simulation (Seamanship). A raider band of several
    /// karves (the simulation keeps a band as one ship) shows its others in echelon astern of the leader. Plans/ai_fleets.md,
    /// phases 2 and 4.
    /// Ported 4 Oct: where a ship of the world stands in the scene, and what lies under her, comes from the stage
    /// (SimStage: the level loaded now). Only ships on this level's stage are shown. The first build's ShipBody (she
    /// rode the sea and left a wake) is gone with its water system: a hull sits on the water's height, without pitch,
    /// roll or wake.
    /// </summary>
    public class WorldShips3D : MonoBehaviour
    {
        [Tooltip("For now, no other ships are shown at all (the user, 2026-10-03: \"remove all other ships in the rivers and sea for now\"). The simulation still sails them.")]
        public bool off = true;
        [Tooltip("The hull to show (the Gislinge boat), scaled to each class's length.")]
        public GameObject prefab;
        [Tooltip("The most ships shown in 3D at once (the nearest).")]
        public int maxBodies = 6;
        [Tooltip("Metres of the Gislinge model's length (its scale 1).")]
        public float modelLength = 7.7f;
        [Tooltip("Height of the player's lookout above the water (m).")]
        public float lookoutEye = 1.8f;
        [Tooltip("A raider band's other karves: how far astern and abeam of the one ahead, in her lengths.")]
        public float echelonAstern = 1.6f, echelonAbeam = 1.3f;

        sealed class Body
        {
            public GameObject go;
            public int ship = -1, part;                                  // part 0: the ship; 1..: a band's other karves
        }

        readonly List<Body> bodies = new List<Body>();
        readonly List<(int ship, double d2)> near = new List<(int, double)>();

        /// <summary>Where a ship of the world is now in the scene (between the simulation's hourly steps), on the water.</summary>
        public static Vector3 Where(SimShip s)
        {
            var game = KapmanGame.Current;
            float t = game != null && game.World != null ? Mathf.Clamp01((float)(game.Hours - (game.World.Hours - 1.0))) : 1f;
            double x = s.prevPos.x + (s.pos.x - s.prevPos.x) * t, z = s.prevPos.z + (s.pos.z - s.prevPos.z) * t;
            return SimStage.ToScene(new WorldXZ(x, z));
        }

        /// <summary>Where the n-th of a band's karves lies off the leader (n = 1, 2...): in echelon, port and starboard
        /// by turns, each further astern.</summary>
        public static Vector3 EchelonOffset(int n, float heading, float length, float astern = 1.6f, float abeam = 1.3f)
        {
            int rank = (n + 1) / 2;
            float side = n % 2 == 1 ? -1f : 1f;
            return Quaternion.Euler(0f, heading, 0f) * new Vector3(side * abeam * length * rank, 0f, -astern * length * rank);
        }

        void LateUpdate()
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            if (off || world == null || prefab == null || game.ship == null)
            {
                HideAll();
                return;
            }
            var me = SimStage.ToWorld(game.ShipPosition);
            double haze = SimStage.HazeKm;
            double sun = SimStage.SunElevationAt(me);
            double moon = Sight.MoonLit(SimStage.JulianDay);
            float t = Mathf.Clamp01((float)(game.Hours - (world.Hours - 1.0)));

            // the nearest ships in sight
            near.Clear();
            foreach (var s in world.ships)
            {
                if (s.state != ShipState.Sailing && s.state != ShipState.Anchored)
                    continue;
                double x = s.prevPos.x + (s.pos.x - s.prevPos.x) * t, z = s.prevPos.z + (s.pos.z - s.prevPos.z) * t;
                double d2 = (x - me.x) * (x - me.x) + (z - me.z) * (z - me.z);
                double range = 1000.0 * Sight.RangeKm(lookoutEye, s.cls.SeenHeight, haze, sun, moon);
                if (d2 > range * range)
                    continue;
                var here = new WorldXZ(x, z);
                range = 1000.0 * Sight.RangeKm(lookoutEye, s.cls.SeenHeight, SimStage.VisibilityKmTo(here), sun, moon);   // (the air between)
                if (d2 > range * range)
                    continue;
                if (!SimStage.OnStage(here))
                    continue;                                          // (not in this level's scene)
                float land = SimStage.LandAboveWater(here);
                if (!float.IsNaN(land) && land > 0.5f)
                    continue;                                          // (over land: a straight passage, not shown)
                near.Add((s.index, d2));
            }
            near.Sort((a, b) => a.d2.CompareTo(b.d2));
            if (near.Count > maxBodies)
                near.RemoveRange(maxBodies, near.Count - maxBodies);

            // free the bodies whose ships are gone (or whose band is smaller now), then give the newcomers a body
            foreach (var b in bodies)
                if (b.ship >= 0 && (!near.Exists(n => n.ship == b.ship) || b.part >= Parts(world.ships[b.ship])))
                {
                    b.ship = -1;
                    b.go.SetActive(false);
                }
            foreach (var (ship, _) in near)
            {
                var s = world.ships[ship];
                for (int part = 0; part < Parts(s); part++)
                {
                    var body = bodies.Find(b => b.ship == ship && b.part == part) ?? bodies.Find(b => b.ship < 0) ?? Make();
                    body.ship = ship;
                    body.part = part;
                    Place(body, s);
                }
            }
        }

        // a raider band shows each of its karves; any other ship is one
        static int Parts(SimShip s) => s.raider ? Mathf.Clamp(s.bandShips, 1, 4) : 1;

        Body Make()
        {
            var b = new Body { go = ShipBodies.Make(prefab, transform, "World ship (3D)") };
            bodies.Add(b);
            return b;
        }

        void Place(Body b, SimShip s)
        {
            var at = Where(s);
            if (b.part > 0)
            {
                at += EchelonOffset(b.part, (float)s.heading, s.cls.length, echelonAstern, echelonAbeam);
                at.y = SimStage.WaterHeight(at);
            }
            var tr = b.go.transform;
            tr.SetPositionAndRotation(at, Quaternion.Euler(0f, (float)s.heading, 0f));
            tr.localScale = Vector3.one * Mathf.Max(0.4f, s.cls.length / Mathf.Max(0.1f, modelLength));
            bool still = s.state == ShipState.Anchored || s.lyingUp || s.heaveTo;   // at anchor, lying up for the night, hove to
            ShipBodies.SetRig(b.go, !s.cls.hasSail ? ShipBodies.Rig.NoMast : still ? ShipBodies.Rig.Furled : ShipBodies.Rig.Sailing);
            if (!b.go.activeSelf)
                b.go.SetActive(true);
        }

        void HideAll()
        {
            foreach (var b in bodies)
            {
                b.ship = -1;
                if (b.go.activeSelf)
                    b.go.SetActive(false);
            }
        }
    }
}
