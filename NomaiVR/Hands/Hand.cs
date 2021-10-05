using System;
using System.Linq;
using UnityEngine;
using Valve.VR;
using Valve.VR.Helpers;

namespace NomaiVR
{
    public class Hand : MonoBehaviour
    {
        private const float k_HandSkeletonBlendAmmount = 0.5f;
        private const float k_GloveSkeletonBlendAmmount = 0.275f;

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
        public Transform IndexTip => _skeleton.indexTip;

        private Renderer _handRenderer;
        private Renderer _gloveRenderer;
        private EHandState _handState = EHandState.Free;
        private EHandState _lastHandState = EHandState.Free;
        private NomaiVR_Hand_Skeleton _skeleton;
        private SteamVR_Skeleton_Poser _reachPoser;
        private EVRSkeletalMotionRange _rangeOfMotion = EVRSkeletalMotionRange.WithoutController;

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
        }

        internal void OnDestroy()
        {
            GlobalMessenger.RemoveListener("SuitUp", OnSuitChanged);
            GlobalMessenger.RemoveListener("RemoveSuit", OnSuitChanged);
        }

        private void SetUpModel()
        {
            var handObject = Instantiate(handPrefab);
            handObject.SetActive(false);
            var hand = handObject.transform;
            Palm = handObject.transform.Find("skeletal_hand/Root/wrist_r/HoldPoint");

            var renderers = handObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            _handRenderer = renderers.Where(r => r.transform.name.Contains("Hand")).FirstOrDefault();
            _gloveRenderer = renderers.Where(r => r.transform.name.Contains("Glove")).FirstOrDefault();

            SetUpShaders(_handRenderer, "Outer Wilds/Character/Skin", "Outer Wilds/Character/Skin");
            SetUpShaders(_gloveRenderer, "Outer Wilds/Character/Clothes");
            
            _handRenderer.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderHands;
            _gloveRenderer.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderGloves;

            SetUpModel(hand);
            _skeleton = SetUpSkeleton(handObject, hand);


            handObject.SetActive(true);
        }

        private void SetUpShaders(Renderer renderer, params string[] shader)
        {
            if (shader.Length == 0)
                return;

            Shader[] toAssign = shader.Select(x => Shader.Find(x)).ToArray();
            for (int i = 0; i < renderer.materials.Length; i++)
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
            string name = $"finger_{fingerName}_meta_r";
            for (int i = 0; i < depth; i++)
                name += $"/finger_{fingerName}_{i}_r";
            return name;
        }

        private static string ThumbBoneName(string fingerName, int depth)
        {
            string name = $"finger_{fingerName}_0_r";
            for (int i = 0; i < depth; i++)
                name += $"/finger_{fingerName}_{i + 1}_r";
            return name;
        }

        private NomaiVR_Hand_Skeleton SetUpSkeleton(GameObject prefabObject, Transform prefabTransform)
        {
            var skeletonDriver = prefabObject.AddComponent<NomaiVR_Hand_Skeleton>();
            InputSource = isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            skeletonDriver.inputSource = InputSource;
            skeletonDriver.rangeOfMotion = _rangeOfMotion;
            skeletonDriver.skeletonRoot = prefabTransform.Find("SourceSkeleton/Root");
            skeletonDriver.updatePose = false;
            skeletonDriver.onlySetRotations = true;
            skeletonDriver.skeletonBlend = 0.5f;
            skeletonDriver.fallbackPoser = prefabObject.AddComponent<SteamVR_Skeleton_Poser>();

            if (isLeft)
            {
                //Flip X axis of skeleton and skinned meshes
                for (int i = 0; i < prefabTransform.childCount; i++)
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
            _reachPoser = prefabObject.AddComponent<SteamVR_Skeleton_Poser>();
            _reachPoser.skeletonMainPose = AssetLoader.ReachForPose;

            return skeletonDriver;
        }

        internal void OnSuitChanged()
        {
            //This is done to avoid clipping the gloves when the hand is closed
            if(_handState == EHandState.Free)
                UpdateSkeletonBlendAmmount();
            if(_handState == EHandState.Reaching)
                _skeleton.basePoseInfluence = ShouldRenderGloves() ? k_GloveSkeletonBlendAmmount : k_HandSkeletonBlendAmmount;
        }

        internal void UpdateSkeletonBlendAmmount()
        {
            float blendAmmount = ShouldRenderGloves() ? k_GloveSkeletonBlendAmmount : k_HandSkeletonBlendAmmount;
            _skeleton.BlendTo(blendAmmount, 0.1f);
            _skeleton.ClearSnapshot();
        }

        internal void ResetSkeletonBlend()
        {
            if (_skeleton == null)
                return;

            _skeleton.basePoseInfluence = 0;
            _skeleton.BlendToSkeleton();
            _skeleton.ResetTemporaryRangeOfMotion();
            UpdateSkeletonBlendAmmount();

            _lastHandState = _handState;
            _handState = EHandState.Free;

            SkeletonBlendReset?.Invoke();
        }

        internal void BlendToReach(SteamVR_Skeleton_Poser overrideReachPoser = null, float time = 0.1f)
        {
            _skeleton.BlendToPoser(overrideReachPoser ?? _reachPoser, time);
            _skeleton.basePoseInfluence = ShouldRenderGloves() ? k_GloveSkeletonBlendAmmount : k_HandSkeletonBlendAmmount;
            _lastHandState = _handState;
            _handState = EHandState.Reaching;
        }

        internal void BlendToPoser(SteamVR_Skeleton_Poser poser, float time = 0.1f)
        {
            _skeleton.basePoseInfluence = 0;
            _lastHandState = _handState;
            _handState = EHandState.Holding;
            _skeleton.BlendToPoser(poser, time);
        }

        internal void NotifyReachable(bool canReach, SteamVR_Skeleton_Poser poser = null)
        {
            if (_handState == EHandState.Holding)
            {
                _lastHandState = canReach ? EHandState.Reaching : EHandState.Free;
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
            if(_lastHandState != EHandState.Reaching)
                ResetSkeletonBlend();
            else
                BlendToReach();
        }

        private void SetUpVrPose()
        {
            gameObject.SetActive(false);

            var poseDriver = transform.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;

            gameObject.SetActive(true);
        }

        public void SetLimitRangeOfMotion(bool isShown)
        {
            _rangeOfMotion = isShown ? EVRSkeletalMotionRange.WithController : EVRSkeletalMotionRange.WithoutController;
            _skeleton?.SetRangeOfMotion(_rangeOfMotion); // Back to main menu we have a nullreference here
            ResetSkeletonBlend();
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
