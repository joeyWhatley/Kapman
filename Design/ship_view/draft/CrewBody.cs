// PROJECT PATH (later): Assets/Kapman/Code/Game/ShipView/CrewBody.cs      assembly: Kapman.Game
// DRAFT, 7 Oct 2026. Compile-checked outside Unity; its poses were run on the pack's real skeleton (Design/ship_view/check).
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// ONE MAN OF THE CREW - a character of the Stylized Medieval Vikings pack (SMV_Viking_Karl_1..5, Settler_1..4,
    /// Hersir, Jarl: all nineteen share one skeleton of 31 bones under "metarig"), posed by code. The pack has no
    /// animation clips and no avatar, so nothing else moves these bones: every frame the pose is put back to the rest
    /// pose and built again from a few numbers (how far he leans, where his hands and feet go).
    ///
    /// All turning is done about axes given in the man's own root frame (x his right, y up, z the way he faces), never
    /// about a bone's own axes: the pack's bones are Blender's (y along the bone, the others any way), and this way their
    /// axes do not matter. Read from the prefabs: he faces +z, his left side is at -x, he is 1.79 m tall at scale 1 (the
    /// rig sits under "metarig", scale 100, turned -90 degrees about x), hips ("Main") 0.94 m up, shoulder joints 1.48 m
    /// up and 0.18 m out, upper arm 0.26 m, forearm 0.24 m, thigh 0.41 m, shin 0.41 m, ankle 0.13 m above the sole.
    /// </summary>
    public sealed class CrewBody
    {
        public readonly Transform root;
        public readonly Transform hips, belly, middle, chest, lowNeck, neck, head;
        public readonly Transform collarL, upperL, foreL, handL, collarR, upperR, foreR, handR;
        public readonly Transform thighL, shinL, footL, thighR, shinR, footR;

        readonly Transform[] bones;
        readonly Quaternion[] rest;

        /// <summary>The hips' height over the root at rest (m, about 0.94).</summary>
        public readonly float hipHeight;
        /// <summary>Shoulder joint to wrist with the arm straight (m, about 0.50).</summary>
        public readonly float armReach;

        /// <summary>False when a bone the crew needs was not found under this root (another pack's character).</summary>
        public bool Valid { get; }

        public CrewBody(Transform root)
        {
            this.root = root;
            hips = Find(root, "Main");
            belly = Find(root, "Stomach");
            middle = Find(root, "Middle");
            chest = Find(root, "Chest");
            lowNeck = Find(root, "LowNeck");
            neck = Find(root, "Neck");
            head = Find(root, "Head");
            collarL = Find(root, "shoulder.L");
            upperL = Find(root, "upper_arm.L");
            foreL = Find(root, "forearm.L");
            handL = Find(root, "hand.L");
            collarR = Find(root, "shoulder.R");
            upperR = Find(root, "upper_arm.R");
            foreR = Find(root, "forearm.R");
            handR = Find(root, "hand.R");
            thighL = Find(root, "thigh.L");
            shinL = Find(root, "shin.L");
            footL = Find(root, "foot.L");
            thighR = Find(root, "thigh.R");
            shinR = Find(root, "shin.R");
            footR = Find(root, "foot.R");
            Valid = hips != null && belly != null && middle != null && chest != null && neck != null && head != null
                 && upperL != null && foreL != null && handL != null && upperR != null && foreR != null && handR != null
                 && thighL != null && shinL != null && footL != null && thighR != null && shinR != null && footR != null;
            if (!Valid)
            {
                bones = new Transform[0];
                rest = new Quaternion[0];
                return;
            }
            // every bone from the hips down: the rest pose, as the prefab has it
            var list = new System.Collections.Generic.List<Transform>();
            Collect(hips, list);
            bones = list.ToArray();
            rest = new Quaternion[bones.Length];
            for (int i = 0; i < bones.Length; i++)
                rest[i] = bones[i].localRotation;
            hipHeight = Vector3.Dot(hips.position - root.position, root.up);
            armReach = (foreL.position - upperL.position).magnitude + (handL.position - foreL.position).magnitude;
        }

        static void Collect(Transform t, System.Collections.Generic.List<Transform> into)
        {
            into.Add(t);
            for (int i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
                // a weapon or a shield in a hand is not a bone (the pack hangs them under hand.L and hand.R)
                if (c.GetComponent<Renderer>() != null)
                    continue;
                Collect(c, into);
            }
        }

        static Transform Find(Transform t, string name)
        {
            if (t.name == name)
                return t;
            for (int i = 0; i < t.childCount; i++)
            {
                var f = Find(t.GetChild(i), name);
                if (f != null)
                    return f;
            }
            return null;
        }

        // ------------------------------------------------------------------ the pose, built again every frame

        /// <summary>Back to the rest pose (the pack's A-pose: standing, arms out and 32 degrees down).</summary>
        public void ResetPose()
        {
            for (int i = 0; i < bones.Length; i++)
                bones[i].localRotation = rest[i];
        }

        /// <summary>Puts the hips at a point in the world, the man facing along `facing`'s z. Call after ResetPose.</summary>
        public void PlaceHips(Vector3 hipsWorld, Quaternion facing)
        {
            root.rotation = facing;
            root.position += hipsWorld - hips.position;
        }

        /// <summary>Turns a bone, and all that hangs from it, about an axis given in the man's root frame.</summary>
        public void Turn(Transform bone, Vector3 axisInRoot, float degrees)
        {
            if (bone == null || degrees == 0f)
                return;
            bone.rotation = Quaternion.AngleAxis(degrees, root.TransformDirection(axisInRoot)) * bone.rotation;
        }

        /// <summary>
        /// The back: forward (+, the way he faces) or back (-) in degrees, shared out over the three bones of the spine
        /// so it bends, with a twist about the upright (+ to his right) and a lean to the side (+ to his right).
        /// </summary>
        public void Lean(float forward, float twist = 0f, float side = 0f)
        {
            Bend(belly, 0.5f, forward, twist, side);
            Bend(middle, 0.3f, forward, twist, side);
            Bend(chest, 0.2f, forward, twist, side);
        }

        void Bend(Transform bone, float share, float forward, float twist, float side)
        {
            Turn(bone, Vector3.right, forward * share);
            Turn(bone, Vector3.up, twist * share);
            Turn(bone, Vector3.forward, -side * share);
        }

        /// <summary>The head: turned to his right (+) and nodded down (+), degrees, over the neck and the head.</summary>
        public void Look(float yaw, float pitch)
        {
            Turn(neck, Vector3.up, yaw * 0.4f);
            Turn(head, Vector3.up, yaw * 0.6f);
            Turn(neck, Vector3.right, pitch * 0.5f);
            Turn(head, Vector3.right, pitch * 0.5f);
        }

        /// <summary>Breath: the chest rises and the shoulders with it. `breath` is -1 .. 1.</summary>
        public void Breathe(float breath)
        {
            Turn(chest, Vector3.right, -1.2f * breath);
            Turn(collarL, Vector3.forward, -1.5f * breath);
            Turn(collarR, Vector3.forward, 1.5f * breath);
        }

        public Vector3 Shoulder(bool left) => (left ? upperL : upperR).position;
        public Vector3 Knee(bool left) => (left ? shinL : shinR).position;

        /// <summary>The wrist of one arm to a point in the world; the elbow bends toward `elbowHint` (a world point).</summary>
        public void Arm(bool left, Vector3 wrist, Vector3 elbowHint)
        {
            if (left)
                Limb.Solve(upperL, foreL, handL, wrist, elbowHint);
            else
                Limb.Solve(upperR, foreR, handR, wrist, elbowHint);
        }

        /// <summary>The ankle of one leg to a point in the world; the knee bends toward `kneeHint` (a world point).</summary>
        public void Leg(bool left, Vector3 ankle, Vector3 kneeHint)
        {
            if (left)
                Limb.Solve(thighL, shinL, footL, ankle, kneeHint);
            else
                Limb.Solve(thighR, shinR, footR, ankle, kneeHint);
        }

        /// <summary>A world point moved by an offset given in the man's root frame (x his right, y up, z ahead; metres).</summary>
        public Vector3 From(Vector3 world, float right, float up, float ahead)
        {
            return world + root.TransformDirection(new Vector3(right, up, ahead));
        }
    }

    /// <summary>
    /// A LIMB OF TWO BONES (shoulder, elbow, wrist; hip, knee, ankle) put to a point: where the middle joint must be is
    /// worked out from the two lengths and the side the hint is on, and each bone is then swung the shortest way onto its
    /// new line. No avatar, no Animation Rigging package. A target out of reach is met as far as the limb goes.
    /// </summary>
    public static class Limb
    {
        public static void Solve(Transform upper, Transform mid, Transform tip, Vector3 target, Vector3 hint)
        {
            Vector3 a = upper.position;
            float l1 = (mid.position - a).magnitude, l2 = (tip.position - mid.position).magnitude;
            if (l1 < 1e-5f || l2 < 1e-5f)
                return;
            Vector3 at = target - a;
            float d = at.magnitude;
            if (d < 1e-5f)
                return;
            Vector3 u = at / d;
            d = Mathf.Clamp(d, Mathf.Abs(l1 - l2) + 0.001f, l1 + l2 - 0.001f);
            // the middle joint: x along the line to the target, h off it, on the hint's side
            float x = (l1 * l1 - l2 * l2 + d * d) / (2f * d);
            float h = Mathf.Sqrt(Mathf.Max(0f, l1 * l1 - x * x));
            Vector3 w = hint - a;
            w -= u * Vector3.Dot(w, u);
            if (w.sqrMagnitude < 1e-8f)
            {
                w = Vector3.Cross(u, Vector3.up);
                if (w.sqrMagnitude < 1e-8f)
                    w = Vector3.Cross(u, Vector3.right);
            }
            w.Normalize();
            Vector3 joint = a + u * x + w * h;
            Vector3 end = a + u * d;
            upper.rotation = Quaternion.FromToRotation(mid.position - a, joint - a) * upper.rotation;
            mid.rotation = Quaternion.FromToRotation(tip.position - mid.position, end - mid.position) * mid.rotation;
        }
    }
}
