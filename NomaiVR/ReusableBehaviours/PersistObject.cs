using UnityEngine;

namespace NomaiVR
{
    public class PersistObject : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
