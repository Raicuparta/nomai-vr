using System;
using NomaiVR.Assets;
using NomaiVR.Hands;
using NomaiVR.Helpers;
using NomaiVR.ModConfig;
using NomaiVR.Tools;
using UnityEngine;
using Valve.VR;

namespace NomaiVR.ReusableBehaviours
{
    public class Holdable : MonoBehaviour
    {
        public bool IsOffhand { get; set; }
        public bool CanFlipX { get; set; } = true;
        public event Action<bool> OnFlipped;
        public event Action<bool> OnHoldStateChanged;

        private Vector3 CurrentPositionOffset => PlayerHelper.IsWearingSuit() ? glovePositionOffset : handPositionOffset;
        private SteamVR_Skeleton_Poser CurrentPoser => PlayerHelper.IsWearingSuit() ? glovePoser : handPoser;

        private Transform hand = HandsController.Behaviour.DominantHand;
        private Transform holdableTransform;
        private Transform rotationTransform;
        private Quaternion rotationOffset;
        private Vector3 handPositionOffset;
        private Vector3 glovePositionOffset;
        private SteamVR_Skeleton_Pose handHoldPose = AssetLoader.GrabbingHandlePose;
        private SteamVR_Skeleton_Pose gloveHoldPose = AssetLoader.GrabbingHandleGlovePose;
        private SteamVR_Skeleton_Pose handBlendedPose;
        private SteamVR_Skeleton_Pose gloveBlendedPose;
        private SteamVR_Skeleton_Poser handPoser;
        private SteamVR_Skeleton_Poser glovePoser;
        private float blendSpeed;
        private bool beingHold;
        private IActiveObserver activeObserver;

        public void SetPositionOffset(Vector3 handOffset, Vector3? gloveOffset = null)
        {
            handPositionOffset = handOffset;
            glovePositionOffset = gloveOffset ?? handOffset;
        }

        public void SetPoses(string handPoseName, string glovePoseName = null)
        {
            handHoldPose = AssetLoader.Poses[handPoseName];
            gloveHoldPose = glovePoseName != null ? AssetLoader.Poses[glovePoseName] : handHoldPose;
        }

        public void SetBlendPoses(string handBlendedPoseName, string gloveBlendedPoseName = null, float blendSpeed = 0f)
        {
            this.blendSpeed = blendSpeed;
            handBlendedPose = AssetLoader.Poses[handBlendedPoseName];
            gloveBlendedPose = gloveBlendedPoseName != null ? AssetLoader.Poses[gloveBlendedPoseName] : handBlendedPose;
        }

        public void SetActiveObserver(IActiveObserver observer)
        {
            activeObserver = observer;
        }

        public void UpdateBlending(float blendAmmount)
        {
            if(handBlendedPose != null)
            {
                handPoser.SetBlendingBehaviourValue("blend_behaviour", blendAmmount);
                glovePoser.SetBlendingBehaviourValue("blend_behaviour", blendAmmount);
            }
        }

        public void SetRotationOffset(Quaternion rotation)
        {
            rotationOffset = rotation;
        }

        internal void Start()
        {
            holdableTransform = new GameObject().transform;
            holdableTransform.localRotation = Quaternion.identity;
            rotationTransform = new GameObject().transform;
            rotationTransform.SetParent(holdableTransform, false);
            rotationTransform.localPosition = Vector3.zero;
            rotationTransform.localRotation = rotationOffset;
            transform.parent = rotationTransform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            var tool = gameObject.GetComponent<PlayerTool>();
            if (tool)
            {
                tool._stowTransform = null;
                tool._holdTransform = null;
            }

            SetupPoses();
            OnInteractingHandChanged();

            VRToolSwapper.InteractingHandChanged += OnInteractingHandChanged;
            ModSettings.OnConfigChange += OnInteractingHandChanged;
            GlobalMessenger.AddListener("SuitUp", OnSuitChanged);
            GlobalMessenger.AddListener("RemoveSuit", OnSuitChanged);
        }

        internal void OnDestroy()
        {
            GlobalMessenger.RemoveListener("SuitUp", OnSuitChanged);
            GlobalMessenger.RemoveListener("RemoveSuit", OnSuitChanged);
            ModSettings.OnConfigChange -= OnInteractingHandChanged;
            VRToolSwapper.InteractingHandChanged -= OnInteractingHandChanged;
        }

        internal void OnSuitChanged()
        {
            if (hand != null && activeObserver != null && activeObserver.IsActive)
                hand.GetComponent<Hand>().NotifyAttachedTo(CurrentPoser);
            UpdateHoldableOffset(hand == HandsController.Behaviour.RightHand);
        }

        private void SetupPoses()
        {
            transform.gameObject.SetActive(false);
            handPoser = transform.gameObject.AddComponent<SteamVR_Skeleton_Poser>();
            handPoser.skeletonMainPose = handHoldPose;
            glovePoser = transform.gameObject.AddComponent<SteamVR_Skeleton_Poser>();
            glovePoser.skeletonMainPose = gloveHoldPose;

            //Setup Blending Behaviours if needed
            if(handBlendedPose != null)
            {
                SetupBlending(handPoser, handBlendedPose);
                SetupBlending(glovePoser, gloveBlendedPose);
            }

            transform.gameObject.SetActive(true);

            //Listen for events to start poses
            if (activeObserver == null)
            {
                var solveToolsTransform = transform.Find("Props_HEA_Signalscope") ??
                                          transform.Find("Props_HEA_ProbeLauncher") ??
                                          transform.Find("TranslatorGroup/Props_HEA_Translator") ??
                                          transform.GetComponentInChildren<DreamLanternController>()?.transform; //Tried to find the first renderer but the probelauncher has multiple of them, doing it this way for now...
                activeObserver = transform.GetComponent<IActiveObserver>();
                if (activeObserver == null)
                {
                    activeObserver = transform.childCount > 0 ? (solveToolsTransform != null ? solveToolsTransform.gameObject.AddComponent<EnableObserver>() : null)
                                        : transform.gameObject.AddComponent<ChildThresholdObserver>() as IActiveObserver;
                }
            }

            // Both this holdable and the observer should be destroyed at the end of a cycle so no leaks here
            if (activeObserver != null)
            {
                activeObserver.OnActivate += () => SetBeingHold(true);
                activeObserver.OnDeactivate += () => SetBeingHold(false);
            }
        }

        public void OnHandReset()
        {
            //On unpause we need to reset the pose since it was probably removed by the pose menu logic
            //this happens if for some reason the hand has reset the blend state
            if(beingHold) hand.GetComponent<Hand>().NotifyAttachedTo(CurrentPoser);
        }

        //Checks if this holdable is currently active and set the proper pose and events
        private void SetBeingHold(bool active)
        {
            beingHold = active;
            var hand = this.hand.GetComponent<Hand>();
            if (beingHold)
            {
                hand.NotifyAttachedTo(CurrentPoser);
                hand.SkeletonBlendReset += OnHandReset;
            }
            else
            {
                this.hand.GetComponent<Hand>().NotifyDetachedFrom(CurrentPoser);
                hand.SkeletonBlendReset -= OnHandReset;
            }
            OnHoldStateChanged?.Invoke(active);
        }

        private void SetupBlending(SteamVR_Skeleton_Poser poser, SteamVR_Skeleton_Pose blendedPose)
        {
            poser.skeletonAdditionalPoses.Add(blendedPose);
            poser.blendingBehaviours.Add(new SteamVR_Skeleton_Poser.PoseBlendingBehaviour()
            {
                type = SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.Manual,
                enabled = true,
                pose = 1,
                name = "blend_behaviour",
                smoothingSpeed = blendSpeed,
                value = 0
            });
        }

        internal void UpdateHoldableOffset(bool isRight)
        {
            if (isRight)
                holdableTransform.localPosition = CurrentPositionOffset;
            else
                holdableTransform.localPosition = new Vector3(-CurrentPositionOffset.x, CurrentPositionOffset.y, CurrentPositionOffset.z);
        }
        
        internal void UpdateHand()
        {
            hand = IsOffhand ? VRToolSwapper.NonInteractingHand?.transform : VRToolSwapper.InteractingHand?.transform;
            if (hand == null) hand = IsOffhand ? HandsController.Behaviour.OffHand : HandsController.Behaviour.DominantHand;
        }

        internal void OnInteractingHandChanged()
        {
            if(VRToolSwapper.InteractingHand?.transform != hand)
            {
                if (hand != null && activeObserver != null && activeObserver.IsActive)
                {
                    var oldHand = hand.GetComponent<Hand>();
                    oldHand.SkeletonBlendReset -= OnHandReset;
                    oldHand.NotifyDetachedFrom(CurrentPoser);
                }

                UpdateHand();

                var handBehaviour = hand.GetComponent<Hand>();
                holdableTransform.SetParent(handBehaviour.Palm, false);

                var isRight = hand == HandsController.Behaviour.RightHand;
                if (isRight)
                    holdableTransform.localScale = new Vector3(1, 1, 1);
                else
                {
                    if (CanFlipX)
                        holdableTransform.localScale = new Vector3(-1, 1, 1);
                }

                UpdateHoldableOffset(isRight);

                if (CanFlipX)
                {
                    RestoreCanvases(isRight);
                    OnFlipped?.Invoke(isRight);
                }

                if (hand != null && activeObserver != null && activeObserver.IsActive)
                {
                    handBehaviour.NotifyAttachedTo(CurrentPoser);
                    handBehaviour.SkeletonBlendReset += OnHandReset;
                }
            }
        }

        internal void RestoreCanvases(bool isRight)
        {
            //Assures canvases are always scaled with x > 0
            Array.ForEach(transform.GetComponentsInChildren<Canvas>(true), canvas =>
            {
                var canvasTransform = canvas.transform;
                var canvasScale = canvasTransform.localScale;
                var tagetScale = Mathf.Abs(canvasScale.x);
                if (!isRight) tagetScale *= -1;
                canvasTransform.localScale = new Vector3(tagetScale, canvasScale.y, canvasScale.z);
            });
        }
    }
}
