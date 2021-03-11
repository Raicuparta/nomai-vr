using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Valve.VR;
using Valve.VR.InteractionSystem.Sample;
public class PoserTest : MonoBehaviour {

	public SteamVR_Skeleton_Poser poser;

	// Use this for initialization
	void OnEnable () {
		GetComponent<SteamVR_Behaviour_Skeleton>().BlendToPoser(poser);
	}

	private void OnDisable() {
		GetComponent<SteamVR_Behaviour_Skeleton>().BlendToSkeleton();
	}
}
