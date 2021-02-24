using System.Xml.Schema;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;
using Valve.VR.InteractionSystem.Sample;

public class SkeletalHandSetup : MonoBehaviour {

	public SteamVR_Action_Single fallbackCurl;
	public SteamVR_Skeleton_Pose fallbackPoint;
	public SteamVR_Skeleton_Pose fallbackRelax;
	public SteamVR_Skeleton_Pose fallbackFist;
	
	private static string BoneToTarget(string bone, bool isSource)
	{
		if(isSource)
	 		return "SourceSkeleton/Root/" + bone;
		return "skeletal_hand/Root/" + bone;
	}

	private static string FingerBoneName(string fingerName, int depth)
	{
		string name = "finger_" + fingerName + "_meta_r";
		for (int i = 0; i < depth; i++)
			name += "/finger_" + fingerName + "_" + i + "_r";
		return name;
	}

	private static string ThumbBoneName(string fingerName, int depth)
	{
		string name = "finger_" + fingerName + "_0_r";
		for (int i = 0; i < depth; i++)
			name += "/finger_" + fingerName + "_" + (i + 1) + "_r";
		return name;
	}

	// Use this for initialization
	void Awake () 
	{
		gameObject.SetActive(false);

		var skeletonDriver = transform.gameObject.AddComponent<SteamVR_Behaviour_Skeleton>();
		skeletonDriver.inputSource = SteamVR_Input_Sources.LeftHand;
		skeletonDriver.rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
		skeletonDriver.skeletonRoot = transform.Find("SourceSkeleton/Root");
		skeletonDriver.updatePose = true;
		skeletonDriver.skeletonBlend = 1;
		skeletonDriver.fallbackPoser = gameObject.AddComponent<SteamVR_Skeleton_Poser>();
		skeletonDriver.mirroring = SteamVR_Behaviour_Skeleton.MirrorType.RightToLeft;
		skeletonDriver.skeletonAction = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>("Skeleton" + skeletonDriver.inputSource.ToString());
		skeletonDriver.fallbackCurlAction = fallbackCurl;
		skeletonDriver.enabled = true;

		var skeletonRetargeter = transform.gameObject.AddComponent<CustomSkeletonHelper>();
		var sourceWristTransform = transform.Find(BoneToTarget("wrist_r", true));
		var targetWristTransform = transform.Find(BoneToTarget("wrist_r", false));
		skeletonRetargeter.wrist = new CustomSkeletonHelper.Retargetable(sourceWristTransform, targetWristTransform);
		skeletonRetargeter.thumbs = new CustomSkeletonHelper.Thumb[1] {
			new CustomSkeletonHelper.Thumb(
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 0)), targetWristTransform.Find(ThumbBoneName("thumb", 0))), //Metacarpal
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 1)), targetWristTransform.Find(ThumbBoneName("thumb", 1))), //Middle
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 2)), targetWristTransform.Find(ThumbBoneName("thumb", 2))), //Distal
				transform.Find(BoneToTarget("finger_thumb_r_aux", true)) //aux
			)
		};
		skeletonRetargeter.fingers = new CustomSkeletonHelper.Finger[2]
		{
			new CustomSkeletonHelper.Finger(
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 0)), targetWristTransform.Find(FingerBoneName("index", 0))), //Metacarpal
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 1)), targetWristTransform.Find(FingerBoneName("index", 1))), //Proximal
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 2)), targetWristTransform.Find(FingerBoneName("index", 2))), //Middle
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 3)), targetWristTransform.Find(FingerBoneName("index", 3))), //Distal
				transform.Find(BoneToTarget("finger_index_r_aux", true)) //aux
			),
			new CustomSkeletonHelper.Finger(
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 0)), targetWristTransform.Find(FingerBoneName("ring", 0))), //Metacarpal
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 1)), targetWristTransform.Find(FingerBoneName("ring", 1))), //Proximal
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 2)), targetWristTransform.Find(FingerBoneName("ring", 2))), //Middle
				new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 3)), targetWristTransform.Find(FingerBoneName("ring", 3))), //Distal
				transform.Find(BoneToTarget("finger_ring_r_aux", true)) //aux
			),
		};

		var skeletonPoser = skeletonDriver.fallbackPoser;
            skeletonPoser.skeletonMainPose = fallbackRelax;
            skeletonPoser.skeletonAdditionalPoses.Add(fallbackPoint);
            skeletonPoser.skeletonAdditionalPoses.Add(fallbackFist);

            //Point Fallback
            skeletonPoser.blendingBehaviours.Add(new SteamVR_Skeleton_Poser.PoseBlendingBehaviour()
            {
                action_bool = SteamVR_Actions.default_GrabGrip,
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


		gameObject.SetActive(true);
	}

	void Start()
	{
	}
}
