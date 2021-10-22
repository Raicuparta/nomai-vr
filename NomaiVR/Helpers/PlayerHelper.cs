using UnityEngine;

namespace NomaiVR.Helpers
{
    public static class PlayerHelper
    {
        private static Transform playerHead;
        public static Transform PlayerHead
        {
            get {
                if (playerHead == null)
                {
                    playerHead = GameObject.FindObjectOfType<ToolModeUI>().transform;
                }
                return playerHead;
            }
        }

        public static bool IsWearingSuit()
        {
            var suit = Locator.GetPlayerSuit();
            return suit != null && suit.IsWearingSuit(true);
        }
    }
}
