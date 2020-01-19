using OWML.Common;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;

namespace NomaiVR
{
    class ToolHolster: MonoBehaviour
    {
        SphereCollider _collider;
        public Transform hand;

        void Start() {
            //var rigidbody = gameObject.AddComponent<Rigidbody>();
            //rigidbody.isKinematic = false;
            //rigidbody.useGravity = false;
            //_collider = gameObject.AddComponent<SphereCollider>();
            //_collider.center = Vector3.zero;
            //_collider.radius = 1f;
            //_collider.isTrigger = true;
            //_collider.enabled = true;
        }

        public void Equip() {
            FindObjectOfType<ToolModeSwapper>().EquipToolMode(ToolMode.SignalScope);
        }

        void Update() {
            transform.position = Common.MainCamera.transform.parent.TransformPoint(Common.MainCamera.transform.localPosition + new Vector3(0.2f, -0.7f, 0.1f));

            if (Vector3.Distance(transform.position, hand.position) < 0.2f) {
                gameObject.SetActive(false);
                FindObjectOfType<ToolModeSwapper>().EquipToolMode(ToolMode.SignalScope);
            }
        }
    }
}
