using UnityEngine;

namespace NomaiVR
{
    public class PersistObject : MonoBehaviour
    {
        internal void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
