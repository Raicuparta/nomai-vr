using OWML.Utils;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class Holdable : MonoBehaviour
    {
        public Hand hand = HandsController.Behaviour.RightHandBehaviour;
        public SteamVR_Skeleton_Pose holdPose = AssetLoader.FallbackFistPose;
        private SteamVR_Skeleton_Poser _poser;

        internal void Start()
        {
            var objectParent = new GameObject().transform;
            objectParent.parent = hand.Palm;
            objectParent.localPosition = transform.localPosition;
            objectParent.localRotation = transform.localRotation;
            transform.parent = objectParent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            var tool = gameObject.GetComponent<PlayerTool>();
            if (tool)
            {
                tool.SetValue("_stowTransform", null);
                tool.SetValue("_holdTransform", null);
            }

            SetupPoses();
        }

        private void SetupPoses()
        { 
            transform.gameObject.SetActive(false);
            _poser = transform.gameObject.AddComponent<SteamVR_Skeleton_Poser>();
            _poser.skeletonMainPose = holdPose;
            transform.gameObject.SetActive(true);

            //Listen for events to start poses
            IActiveObserver enableObserver = transform.childCount > 0 ? transform.GetComponentInChildren<Renderer>(true).gameObject.AddComponent<EnableObserver>()
                                                                        : transform.gameObject.AddComponent<ChildThresholdObserver>() as IActiveObserver;

            //Both this holdable and the observer should be destroyed at the end of a cycle so no leaks here
            enableObserver.OnActivate += () => hand.NotifyAttachedTo(_poser);
            enableObserver.OnDeactivate += () => hand.NotifyDetachedFrom(_poser);
        }
    }
}
