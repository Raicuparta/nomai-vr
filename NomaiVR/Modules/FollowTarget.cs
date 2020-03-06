using UnityEngine;

namespace NomaiVR {
    // This behaviour is useful for replicating a child / parent hierarchy,
    // without actually changing the hierarchy, since that can break stuff in the game.
    class FollowTarget: MonoBehaviour {
        public Transform target;
        public Vector3 localPosition;
        public Quaternion localRotation = Quaternion.identity;

        void LateUpdate () {
            if (!target) {
                return;
            }

            transform.rotation = target.rotation * localRotation;
            transform.position = target.TransformPoint(localPosition);
        }
    }
}
