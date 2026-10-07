using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE LEVEL'S BOTTOM FOR A HULL (ship physics 2.9): the first thing a ray finds going down past a point, as
    /// ShipMotion.Ground finds it (the terrain, a jetty, a dam, a moored boat), but skipping the ship's own colliders and
    /// triggers, and starting 20 m above the point rather than 500, so a tree or a roof leaning over the water is not
    /// taken for the bottom. The rays go into a buffer made once (no garbage a step). What the bottom is comes from
    /// <see cref="kind"/> for now (the terrain's layers will say, later).
    /// </summary>
    public sealed class SceneBed : IBedField
    {
        /// <summary>What the bottom is everywhere (until the terrain's layers say).</summary>
        public GroundKind kind = GroundKind.Sand;
        /// <summary>How far above the point the ray starts, and how far below it it looks (m).</summary>
        public float above = 20f, below = 80f;

        readonly Transform own;
        readonly RaycastHit[] hits = new RaycastHit[16];

        /// <summary>The bottom for the ship whose GameObject this is (its colliders are skipped).</summary>
        public SceneBed(Transform own)
        {
            this.own = own;
        }

        public bool Bed(Vector3 p, out float height, out Vector3 normal, out GroundKind k)
        {
            height = 0f;
            normal = Vector3.up;
            k = kind;
            Vector3 from = new Vector3(p.x, p.y + above, p.z);
            int n = Physics.RaycastNonAlloc(from, Vector3.down, hits, above + below, ~0, QueryTriggerInteraction.Ignore);
            bool found = false;
            for (int i = 0; i < n; i++)
            {
                var c = hits[i].collider;
                if (c == null || (own != null && c.transform.IsChildOf(own)))
                    continue;
                if (!found || hits[i].point.y > height)
                {
                    height = hits[i].point.y;
                    normal = hits[i].normal;
                    found = true;
                }
            }
            return found;
        }
    }
}
