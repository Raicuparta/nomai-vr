using UnityEngine;

namespace NomaiVR {
    class LookAtSmooth: MonoBehaviour {
        public Transform target;

        void FixedUpdate () {
            if (!target) {
                return;
            }

            //transform.LookAt(target);
            transform.LookAt(target, transform.up);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, Time.fixedDeltaTime * 10f);
            transform.position = target.position + target.forward;
        }
    }
}
