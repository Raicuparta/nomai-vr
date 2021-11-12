using System;
using System.Diagnostics;
using System.Linq;
using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using NomaiVR.Tools;
using Steamworks;
using UnityEngine;
using Valve.VR;

namespace NomaiVR.Hands
{
    public class Hand : MonoBehaviour
    {
        public const float minimumPointDistance = 0.1f;
        private const float handSkeletonBlendAmmount = 0.5f;
        private const float gloveSkeletonBlendAmmount = 0.275f;
        private static readonly Vector3 gloveIndexTipOffset = new Vector3(0.034f, 0.0002f, 0.0003f);

        public event Action Initialized;
        public event Action SkeletonBlendReset;
        public GameObject handPrefab;
        public SteamVR_Action_Pose pose;
        public SteamVR_Skeleton_Pose fallbackPoint;
        public SteamVR_Skeleton_Pose fallbackRelax;
        public SteamVR_Skeleton_Pose fallbackFist;
        public bool isLeft;

        public SteamVR_Input_Sources InputSource { get; private set; }
        public Transform Palm { get; private set; }
        public Transform IndexTip { get; private set; }

        private Renderer handRenderer;
        private Renderer gloveRenderer;
        private EHandState handState = EHandState.Free;
        private EHandState lastHandState = EHandState.Free;
        private NomaiVRHandSkeleton skeleton;
        private SteamVR_Skeleton_Poser reachPoser;
        private SteamVR_Skeleton_Poser pointPoser;
        private EVRSkeletalMotionRange rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
        private FollowTarget followPose;
        private float closestPointable = float.PositiveInfinity;
        private float currentPointAmmount = 0.0f;
        private float currentPointBlendVelocity = 0.0f;

        internal enum EHandState
        {
            Free,
            Reaching,
            Holding
        }

        internal void Start()
        {
            SetUpModel();
            SetUpVrPose();

            Initialized?.Invoke();

            GlobalMessenger.AddListener("SuitUp", OnSuitChanged);
            GlobalMessenger.AddListener("RemoveSuit", OnSuitChanged);
            GlobalMessenger.AddListener("SimulationEnter", OnSimulationEnter);
            GlobalMessenger.AddListener("SimulationExit", OnSimulationExit);
        }

        private void Update()
        {
            if (VRToolSwapper.InteractingHand == this)
            {
                followPose.positionSmoothTime = 0.1f;
                followPose.rotationSmoothTime = 0.1f;
            }
            else
            {
                followPose.positionSmoothTime = 0.02f;
                followPose.rotationSmoothTime = 0.02f;
            }
        }

        internal void LateUpdate()
        {
            float pointAmmount = 1f - Mathf.Clamp(closestPointable / minimumPointDistance, 0, 1);
            currentPointAmmount = Mathf.SmoothDamp(currentPointAmmount, pointAmmount, ref currentPointBlendVelocity, 0.05f);
            if (currentPointAmmount > float.Epsilon && handState != EHandState.Holding)
            {
                var pointSnapshot = pointPoser.GetBlendedPose(skeleton, skeleton.BonePositions, skeleton.BoneRotations);
                for (int i = 0; i < pointSnapshot.bonePositions.Length; i++)
                    skeleton.BlendBonePosition(i, pointSnapshot.bonePositions[i], currentPointAmmount);
                for (int i = 0; i < pointSnapshot.boneRotations.Length; i++)
                    skeleton.BlendBoneRotation(i, pointSnapshot.boneRotations[i], currentPointAmmount);
            }

            closestPointable = float.PositiveInfinity;
        }

        internal void OnDestroy()
        {
            GlobalMessenger.RemoveListener("SuitUp", OnSuitChanged);
            GlobalMessenger.RemoveListener("RemoveSuit", OnSuitChanged);
            GlobalMessenger.RemoveListener("SimulationEnter", OnSimulationEnter);
            GlobalMessenger.RemoveListener("SimulationExit", OnSimulationExit);
        }

        private void SetUpModel()
        {
            var handObject = Instantiate(handPrefab);
            handObject.SetActive(false);
            var hand = handObject.transform;
            Palm = handObject.transform.Find("skeletal_hand/Root/wrist_r/HoldPoint");

            var renderers = handObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            handRenderer = renderers.Where(r => r.transform.name.Contains("Hand")).FirstOrDefault();
            gloveRenderer = renderers.Where(r => r.transform.name.Contains("Glove")).FirstOrDefault();

            SetUpShaders(handRenderer, "Outer Wilds/Character/Skin", "Outer Wilds/Character/Skin");
            SetUpShaders(gloveRenderer, "Outer Wilds/Character/Clothes");
            
            handRenderer.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender += ShouldRenderHands;
            gloveRenderer.gameObject.AddComponent<ConditionalRenderer>().GetShouldRender += ShouldRenderGloves;

            SetUpModel(hand);
            skeleton = SetUpSkeleton(handObject, hand);


            handObject.SetActive(true);
        }

        private void OnSimulationEnter()
        {
            handRenderer.gameObject.layer = LayerMask.NameToLayer("DreamSimulation");
            gloveRenderer.gameObject.layer = LayerMask.NameToLayer("DreamSimulation");
            SetUpShaders(handRenderer, "Outer Wilds/Environment/Invisible Planet/Cyberspace", "Outer Wilds/Environment/Invisible Planet/Cyberspace");
            SetUpShaders(gloveRenderer, "Outer Wilds/Environment/Invisible Planet/Cyberspace");
        }

        private void OnSimulationExit()
        {
            handRenderer.gameObject.layer = LayerMask.NameToLayer("Default");
            gloveRenderer.gameObject.layer = LayerMask.NameToLayer("Default");
            SetUpShaders(handRenderer, "Outer Wilds/Character/Skin", "Outer Wilds/Character/Skin");
            SetUpShaders(gloveRenderer, "Outer Wilds/Character/Clothes");
        }

        private void SetUpShaders(Renderer renderer, params string[] shader)
        {
            if (shader.Length == 0)
                return;

            var toAssign = shader.Select(x => Shader.Find(x)).ToArray();
            for (var i = 0; i < renderer.materials.Length; i++)
                renderer.materials[i].shader = toAssign[Mathf.Clamp(i, 0, toAssign.Length)];
        }

        private void SetUpModel(Transform model)
        {
            model.parent = transform;
            model.localPosition = Vector3.zero;
            model.localRotation = Quaternion.identity;
            model.localScale = Vector3.one;
        }

        private static string BoneToTarget(string bone, bool isSource) => isSource ? $"SourceSkeleton/Root/{bone}" : $"skeletal_hand/Root/{bone}";
        
        private static string FingerBoneName(string fingerName, int depth)
        {
            var name = $"finger_{fingerName}_meta_r";
            for (var i = 0; i < depth; i++)
                name += $"/finger_{fingerName}_{i}_r";
            return name;
        }

        private static Transform FingerEnd(Transform source, string fingerName)
        {
            Transform parent = source.Find($"finger_{fingerName}_meta_r");
            while (parent.childCount > 0) parent = parent.GetChild(0);
            return parent;
        }

        private static string ThumbBoneName(string fingerName, int depth)
        {
            var name = $"finger_{fingerName}_0_r";
            for (var i = 0; i < depth; i++)
                name += $"/finger_{fingerName}_{i + 1}_r";
            return name;
        }

        private NomaiVRHandSkeleton SetUpSkeleton(GameObject prefabObject, Transform prefabTransform)
        {
            var skeletonDriver = prefabObject.AddComponent<NomaiVRHandSkeleton>();
            InputSource = isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            skeletonDriver.inputSource = InputSource;
            skeletonDriver.rangeOfMotion = rangeOfMotion;
            skeletonDriver.skeletonRoot = prefabTransform.Find("SourceSkeleton/Root");
            skeletonDriver.updatePose = false;
            skeletonDriver.onlySetRotations = true;
            skeletonDriver.skeletonBlend = 0.5f;
            skeletonDriver.fallbackPoser = prefabObject.AddComponent<SteamVR_Skeleton_Poser>();

            if (isLeft)
            {
                //Flip X axis of skeleton and skinned meshes
                for (var i = 0; i < prefabTransform.childCount; i++)
                    prefabTransform.GetChild(i).localScale = new Vector3(-1, 1, 1);

                //Flip back palm
                Palm.localScale = new Vector3(-1, 1, 1);

                //Enable SteamVR skeleton mirroring
                skeletonDriver.mirroring = SteamVR_Behaviour_Skeleton.MirrorType.RightToLeft;
            }
            skeletonDriver.skeletonAction = isLeft ? SteamVR_Actions.default_SkeletonLeftHand : SteamVR_Actions.default_SkeletonRightHand;

            skeletonDriver.fallbackCurlAction = SteamVR_Actions.default_Squeeze;
            skeletonDriver.enabled = true;

            var skeletonRetargeter = prefabObject.AddComponent<CustomSkeletonHelper>();
            var sourceWristTransform = prefabTransform.Find(BoneToTarget("wrist_r", true));
            var targetWristTransform = prefabTransform.Find(BoneToTarget("wrist_r", false));
            skeletonRetargeter.wrist = new CustomSkeletonHelper.Retargetable(sourceWristTransform, targetWristTransform);
            skeletonRetargeter.thumbs = new CustomSkeletonHelper.Thumb[1] {
                new CustomSkeletonHelper.Thumb(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 0)), targetWristTransform.Find(ThumbBoneName("thumb", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 1)), targetWristTransform.Find(ThumbBoneName("thumb", 1))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 2)), targetWristTransform.Find(ThumbBoneName("thumb", 2))), //Distal
                    prefabTransform.Find(BoneToTarget("finger_thumb_r_aux", true)) //aux
                )
            };
            skeletonRetargeter.fingers = new CustomSkeletonHelper.Finger[2]
            {
                new CustomSkeletonHelper.Finger(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 0)), targetWristTransform.Find(FingerBoneName("index", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 1)), targetWristTransform.Find(FingerBoneName("index", 1))), //Proximal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 2)), targetWristTransform.Find(FingerBoneName("index", 2))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 3)), targetWristTransform.Find(FingerBoneName("index", 3))), //Distal
                    prefabTransform.Find(BoneToTarget("finger_index_r_aux", true)) //aux
                ),
                new CustomSkeletonHelper.Finger(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 0)), targetWristTransform.Find(FingerBoneName("ring", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 1)), targetWristTransform.Find(FingerBoneName("ring", 1))), //Proximal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 2)), targetWristTransform.Find(FingerBoneName("ring", 2))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 3)), targetWristTransform.Find(FingerBoneName("ring", 3))), //Distal
                    prefabTransform.Find(BoneToTarget("finger_ring_r_aux", true)) //aux
                ),
            };

            var skeletonPoser = skeletonDriver.fallbackPoser;
            skeletonPoser.skeletonMainPose = fallbackRelax;
            skeletonPoser.skeletonAdditionalPoses.Add(fallbackPoint);
            skeletonPoser.skeletonAdditionalPoses.Add(fallbackFist);

            //Point Fallback
            skeletonPoser.blendingBehaviours.Add(new SteamVR_Skeleton_Poser.PoseBlendingBehaviour()
            {
                action_bool = SteamVR_Actions.default_Grip,
                enabled = true,
                influence = 1,
                name = "point",
                pose = 1,
                value = 0,
                type = SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.BooleanAction,
                previewEnabled = true,
                smoothingSpeed = 16
            });

            //Fist Fallback
            skeletonPoser.blendingBehaviours.Add(new SteamVR_Skeleton_Poser.PoseBlendingBehaviour()
            {
                action_bool = SteamVR_Actions.default_GrabPinch,
                enabled = true,
                influence = 1,
                name = "fist",
                pose = 2,
                value = 0,
                type = SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.BooleanAction,
                previewEnabled = true,
                smoothingSpeed = 16
            });

            //Setup Limit/Reach Poser
            reachPoser = prefabObject.AddComponent<SteamVR_Skeleton_Poser>();
            reachPoser.skeletonMainPose = AssetLoader.ReachForPose;

            //Point poser for handheld buttons interaction
            pointPoser = prefabObject.AddComponent<SteamVR_Skeleton_Poser>();
            pointPoser.skeletonMainPose = AssetLoader.FallbackPointPose;

            IndexTip = new GameObject("IndexTip").transform;
            IndexTip.parent = FingerEnd(targetWristTransform, "index");
            IndexTip.localPosition = Vector3.zero;
            IndexTip.localRotation = Quaternion.identity;

            return skeletonDriver;
        }

        internal void OnSuitChanged()
        {
            //This is done to avoid clipping the gloves when the hand is closed
            if(handState == EHandState.Free)
                UpdateSkeletonBlendAmmount();
            if(handState == EHandState.Reaching)
                skeleton.BasePoseInfluence = ShouldRenderGloves() ? gloveSkeletonBlendAmmount : handSkeletonBlendAmmount;

            IndexTip.localPosition = ShouldRenderGloves() ? gloveIndexTipOffset : Vector3.zero;
        }

        internal void UpdateSkeletonBlendAmmount()
        {
            var blendAmmount = ShouldRenderGloves() ? gloveSkeletonBlendAmmount : handSkeletonBlendAmmount;
            skeleton.BlendTo(blendAmmount, 0.1f);
            skeleton.ClearSnapshot();
        }

        internal void ResetSkeletonBlend()
        {
            if (skeleton == null)
                return;

            skeleton.BasePoseInfluence = 0;
            skeleton.BlendToSkeleton();
            skeleton.ResetTemporaryRangeOfMotion();
            UpdateSkeletonBlendAmmount();

            lastHandState = handState;
            handState = EHandState.Free;

            SkeletonBlendReset?.Invoke();
        }

        internal void BlendToReach(SteamVR_Skeleton_Poser overrideReachPoser = null, float time = 0.1f)
        {
            skeleton.BlendToPoser(overrideReachPoser ?? reachPoser, time);
            skeleton.BasePoseInfluence = ShouldRenderGloves() ? gloveSkeletonBlendAmmount : handSkeletonBlendAmmount;
            lastHandState = handState;
            handState = EHandState.Reaching;
        }

        internal void BlendToPoser(SteamVR_Skeleton_Poser poser, float time = 0.1f)
        {
            skeleton.BasePoseInfluence = 0;
            lastHandState = handState;
            handState = EHandState.Holding;
            skeleton.BlendToPoser(poser, time);
        }

        internal void NotifyReachable(bool canReach, SteamVR_Skeleton_Poser poser = null)
        {
            if (handState == EHandState.Holding)
            {
                lastHandState = canReach ? EHandState.Reaching : EHandState.Free;
                return;
            }

            if (canReach)
                BlendToReach(poser);
            else
                ResetSkeletonBlend();
        }

        internal void NotifyAttachedTo(SteamVR_Skeleton_Poser poser)
        {
            BlendToPoser(poser);
        }

        internal void NotifyDetachedFrom(SteamVR_Skeleton_Poser poser)
        {
            if(lastHandState != EHandState.Reaching)
                ResetSkeletonBlend();
            else
                BlendToReach();
        }

        private void SetUpVrPose()
        {
            var poseObject = new GameObject($"SteamVrBehavourPose");
            poseObject.transform.SetParent(transform.parent, false);
            followPose = gameObject.AddComponent<FollowTarget>();
            followPose.target = poseObject.transform;
            followPose.positionSmoothTime = 0.05f;
            followPose.rotationSmoothTime = 0.05f;
            
            poseObject.SetActive(false);

            var poseDriver = poseObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;

            poseObject.SetActive(true);
        }

        public void SetLimitRangeOfMotion(bool isShown)
        {
            rangeOfMotion = isShown ? EVRSkeletalMotionRange.WithController : EVRSkeletalMotionRange.WithoutController;
            skeleton?.SetRangeOfMotion(rangeOfMotion); // Back to main menu we have a nullreference here
            ResetSkeletonBlend();
        }

        public void NotifyPointable(float distance)
        {
            if (distance < closestPointable) closestPointable = distance;
        }

        private bool ShouldRenderGloves()
        {
            return SceneHelper.IsInGame() && PlayerHelper.IsWearingSuit();
        }

        private bool ShouldRenderHands()
        {
            return !SceneHelper.IsInGame() || !PlayerHelper.IsWearingSuit();
        }
    }
}
