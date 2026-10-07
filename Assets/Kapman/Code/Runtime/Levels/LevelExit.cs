using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Kapman.Levels
{
    /// <summary>Which way in the next level is entered by (set by a LevelExit, read by a LevelEntry).</summary>
    public static class LevelTravel
    {
        public static string Entry;
    }

    /// <summary>
    /// A WAY OUT OF THE LEVEL - the prompt at a river mouth or at the end of a reach, as the design has it ("Sail up the
    /// Rhine to Dorestad"): when the player's ship comes near, the panel offers it, and Enter goes. For now the next
    /// level loads as a scene; the seamless phase (a bend, a narrows or mist hiding the change) comes later.
    /// </summary>
    public class LevelExit : MonoBehaviour
    {
        public string prompt = "Sail down the Rhine to the sea";
        public string targetScene = "L_NorthSea";
        public string targetEntry = "rhine_mouth";
        public float radius = 80f;

        public static LevelExit Near { get; private set; }

        void Update()
        {
            var ship = ShipMotion.Player;
            if (ship == null)
                return;
            Vector3 d = ship.transform.position - transform.position;
            d.y = 0f;
            bool inside = d.magnitude < radius;
            if (inside)
                Near = this;
            else if (Near == this)
                Near = null;
            var kb = Keyboard.current;
            if (inside && kb != null && kb.enterKey.wasPressedThisFrame)
                Go();
        }

        public bool Built => !string.IsNullOrEmpty(targetScene) && Application.CanStreamedLevelBeLoaded(targetScene);

        public void Go()
        {
            if (!Built)
                return;
            LevelTravel.Entry = targetEntry;
            Near = null;
            SceneManager.LoadScene(targetScene);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    /// <summary>Where the player's ship comes into this level by a given way (its position and heading).</summary>
    public class LevelEntry : MonoBehaviour
    {
        public string entryId = "rhine_mouth";

        void Start()
        {
            if (LevelTravel.Entry != entryId)
                return;
            var ship = ShipMotion.Player;
            if (ship == null)
                return;
            float yaw = transform.eulerAngles.y;
            ship.transform.SetPositionAndRotation(new Vector3(transform.position.x, ship.transform.position.y, transform.position.z), Quaternion.Euler(0f, yaw, 0f));
            ship.heading = yaw;
            LevelTravel.Entry = null;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.8f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 30f);
            Gizmos.DrawWireSphere(transform.position, 6f);
        }
    }
}
