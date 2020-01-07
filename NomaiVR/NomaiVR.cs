using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;

namespace OWML.NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        public static IModHelper Helper
        static NomaiVR _instance;

        public static void Log(string s) {
            _instance.ModHelper.Console.WriteLine("NomaiVR: " + s);
        }

        void Start() {
            Log("Start Main");

            _instance = this;
            Helper = ModHelper;

            // Add all modules here.
            gameObject.AddComponent<Menus>();
            gameObject.AddComponent<PlayerBodyPosition>();
        }
    }
}
