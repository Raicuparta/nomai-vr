using UnityEngine;

namespace NomaiVR
{
    public static class PlayerHelper
    {
        private static Transform _playerHead = null;
        public static Transform PlayerHead {
            get {
                if (_playerHead == null)
                {
                    _playerHead = GameObject.FindObjectOfType<ToolModeUI>().transform;
                }
                return _playerHead;
            }
        }
    }
}
