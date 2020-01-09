using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SpatialTracking;

namespace OWML.NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        public static IModHelper Helper;
        static NomaiVR _instance;

        public static void Log(string s) {
            _instance.ModHelper.Console.WriteLine("NomaiVR: " + s);
        }

        void Start() {
            _instance = this;

            Log("Start Main");

            Helper = ModHelper;

            // Add all modules here.
            gameObject.AddComponent<Menus>();
            gameObject.AddComponent<PlayerBodyPosition>();
        }

        void Update() {
            //_transform.
            //_obj.transform.parent = null;
        }
    }
}
