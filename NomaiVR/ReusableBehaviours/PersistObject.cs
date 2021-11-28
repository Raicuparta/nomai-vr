using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    public class PersistObject : MonoBehaviour
    {
        internal void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
