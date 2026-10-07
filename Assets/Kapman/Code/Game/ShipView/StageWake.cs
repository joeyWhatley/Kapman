using Kapman.Levels.Water;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// HER WAKE ON HDRP'S WATER: foam put into the water itself with HDRP's water decals, as the 3D levels' ShipWake and
    /// the look lab do (this is a small part of that: the foam only, no heights). A line of patches astern, each wider
    /// and fainter than the one before it, and one at each shoulder of her bow; all of them stronger with her speed and
    /// gone when she lies still. The patches stay with her; the foam's own pattern is HDRP's and goes astern with the
    /// water (the treadmill moves the whole surface), so the wake is seen to run aft. Under oars a puddle is left where
    /// each blade came out.
    /// Uses the game's own tools for it, which are plain helpers with no state the chart reads: WaterDecalKit (a decal,
    /// its foam patch texture) and the material Assets/Kapman/Water/Ship_Foam.mat (the kit holds it).
    /// On the stage's mesh sea none of this shows (a decal marks HDRP's water only): there the ribbon is drawn instead.
    /// </summary>
    public sealed class StageWake
    {
        /// <summary>How white the wake is at 3 m/s, just astern.</summary>
        public float foam = 0.55f;
        /// <summary>How white each bow shoulder is at 3 m/s.</summary>
        public float bowFoam = 0.3f;

        const int Astern = 6;
        readonly WaterDecal[] trail = new WaterDecal[Astern];
        readonly WaterDecal[] bow = new WaterDecal[2];
        WaterDecal[] puddles = new WaterDecal[0];
        float[] puddleAge = new float[0];
        Vector3[] puddleAt = new Vector3[0];
        Transform root;
        MaterialPropertyBlock block;
        float lastPhase;

        public bool Built => root != null;

        public void Build(ShipStage stage, Material foamMaterial)
        {
            if (foamMaterial == null)
                return;
            root = new GameObject("Wake (foam in HDRP's water)").transform;
            root.SetParent(stage.transform, false);
            block = new MaterialPropertyBlock();
            var patch = WaterDecalKit.FoamPatch();
            for (int i = 0; i < Astern; i++)
                trail[i] = Decal("Astern " + (i + 1), foamMaterial, patch);
            for (int i = 0; i < 2; i++)
                bow[i] = Decal(i == 0 ? "Bow, port" : "Bow, starboard", foamMaterial, patch);
            puddles = new WaterDecal[8];
            puddleAge = new float[puddles.Length];
            puddleAt = new Vector3[puddles.Length];
            for (int i = 0; i < puddles.Length; i++)
            {
                puddles[i] = Decal("Oar puddle " + (i + 1), foamMaterial, patch);
                puddleAge[i] = 99f;
            }
            stage.Adopt(root.gameObject);
        }

        WaterDecal Decal(string name, Material material, Texture2D patch)
        {
            var d = WaterDecalKit.MakeDecal(name, root, material, new Vector2(1f, 1f), new Vector2Int(64, 64));
            WaterDecalKit.SetTextures(d, block, null, patch);
            return d;
        }

        /// <summary>Lays the patches for this moment. on: HDRP's water is the sea drawn.</summary>
        public void Tick(in ShipStageState s, StageShip ship, float dt, bool on)
        {
            if (root == null)
                return;
            if (root.gameObject.activeSelf != on)
                root.gameObject.SetActive(on);
            if (!on)
                return;
            float strength = s.moored ? 0f : Mathf.Clamp01(s.speed / 3f);
            float a = s.heading * Mathf.Deg2Rad;
            Vector3 fwd = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)), right = new Vector3(fwd.z, 0f, -fwd.x);
            var facing = Quaternion.Euler(0f, s.heading, 0f);
            float L = ship.Length, B = ship.Beam;
            // astern: each patch a little longer than its step so they run into one another
            float step = 1.1f + 0.7f * s.speed;
            for (int i = 0; i < Astern; i++)
            {
                var d = trail[i];
                float back = L * 0.44f + (i + 0.5f) * step;
                d.transform.localPosition = -fwd * back;
                d.transform.localRotation = facing;
                d.regionSize = new Vector2(B * (0.5f + 0.34f * i), step * 1.7f);
                float fade = Mathf.Exp(-i / 2.4f);
                d.surfaceFoamDimmer = Mathf.Clamp01(foam * strength * fade);
                d.deepFoamDimmer = Mathf.Clamp01(foam * 0.7f * strength * Mathf.Exp(-i / 3.6f));
            }
            // the bow's shoulders: the water she throws aside
            for (int i = 0; i < 2; i++)
            {
                var d = bow[i];
                float side = i == 0 ? -1f : 1f;
                d.transform.localPosition = fwd * (L * 0.3f) + right * (side * B * 0.4f);
                d.transform.localRotation = facing * Quaternion.Euler(0f, side * 14f, 0f);
                d.regionSize = new Vector2(B * 0.42f, L * 0.3f);
                d.surfaceFoamDimmer = Mathf.Clamp01(bowFoam * strength * strength);
                d.deepFoamDimmer = Mathf.Clamp01(bowFoam * 0.5f * strength);
            }
            // under oars: a puddle where each blade left the water (the stroke's finish, phase 0.4), which then falls astern
            bool rowing = s.underOars && s.UnderWay && ship.OarsOut > 0.8f;
            if (rowing && lastPhase < 0.4f && ship.StrokePhase >= 0.4f)
                Puddles(ship, fwd, right);
            lastPhase = ship.StrokePhase;
            Vector3 way = fwd * s.Way;
            for (int i = 0; i < puddles.Length; i++)
            {
                var d = puddles[i];
                puddleAge[i] += dt;
                puddleAt[i] -= way * dt;
                float life = Mathf.Clamp01(1f - puddleAge[i] / 4f);
                d.transform.localPosition = puddleAt[i];
                d.regionSize = Vector2.one * (0.7f + 0.5f * puddleAge[i]);
                d.surfaceFoamDimmer = 0.5f * life * life;
                d.deepFoamDimmer = 0.4f * life;
            }
        }

        int nextPuddle;

        void Puddles(StageShip ship, Vector3 fwd, Vector3 right)
        {
            for (int k = 0; k < ship.OarPivots.Count; k++)
            {
                bool starboard = k % 2 == 1;
                var oar = (starboard ? ship.OarsStarboard : ship.OarsPort)[k / 2];
                if (oar == null)
                    continue;
                // the blade's middle: down the oar's own -y from its thole, a little short of its whole outboard length
                Vector3 blade = oar.position - oar.up * (ship.BladeOut * 0.9f) - root.parent.position;
                int i = nextPuddle++ % puddles.Length;
                puddleAt[i] = new Vector3(blade.x, 0f, blade.z);
                puddleAge[i] = 0f;
            }
        }
    }
}
