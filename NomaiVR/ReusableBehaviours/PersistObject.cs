using UnityEngine;

namespace NomaiVR
{
    public class PersistObject : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
