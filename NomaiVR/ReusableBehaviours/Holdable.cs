using OWML.Utils;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class Holdable : MonoBehaviour
    {
        public Hand hand = HandsController.Behaviour.RightHandBehaviour;
        public SteamVR_Skeleton_Pose holdPose = AssetLoader.GrabbingHandlePose;
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
            Transform solveToolsTransform = transform.Find("Props_HEA_Signalscope") ??
                                            transform.Find("Props_HEA_ProbeLauncher") ??
                                            transform.Find("TranslatorGroup/Props_HEA_Translator"); //Tried to find the first renderer bu the probelauncher has multiple of them, doing it this way for now...
            IActiveObserver enableObserver = transform.childCount > 0 ? (solveToolsTransform != null ? solveToolsTransform.gameObject.AddComponent<EnableObserver>() : null)
                                                                        : transform.gameObject.AddComponent<ChildThresholdObserver>() as IActiveObserver;

            // Both this holdable and the observer should be destroyed at the end of a cycle so no leaks here
            if(enableObserver != null)
            {
                enableObserver.OnActivate += () => hand.NotifyAttachedTo(_poser);
                enableObserver.OnDeactivate += () => hand.NotifyDetachedFrom(_poser);
            }
        }
    }
}
