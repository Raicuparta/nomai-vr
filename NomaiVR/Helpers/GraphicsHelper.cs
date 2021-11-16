using UnityEngine;
using Valve.VR;

namespace NomaiVR.Helpers
{
    public static class GraphicsHelper
    {
		private static readonly Matrix4x4 scaleBiasDXToGL = Matrix4x4.TRS(Vector3.back, Quaternion.identity, new Vector3(1, 1, 2));
		public static void ForceCameraToEye(Camera targetCamera, Camera monoCamera, EVREye eye)
        {
			targetCamera.transform.position = monoCamera.transform.TransformPoint(SteamVR.instance.eyes[(int)eye].pos);
			targetCamera.transform.rotation = monoCamera.transform.rotation * SteamVR.instance.eyes[(int)eye].rot;
			targetCamera.projectionMatrix = scaleBiasDXToGL * GetSteamVREyeProjection(targetCamera, eye);
		}

		public static Matrix4x4 GetSteamVREyeProjection(Camera cam, EVREye eye)
		{
			return HmdMatrix44ToMatrix4X4(SteamVR.instance.hmd.GetProjectionMatrix(eye, cam.nearClipPlane, cam.farClipPlane));
		}

		public static Matrix4x4 HmdMatrix44ToMatrix4X4(HmdMatrix44_t mat)
		{
			var m = new Matrix4x4();
			m.m00 = mat.m0;
			m.m01 = mat.m1;
			m.m02 = mat.m2;
			m.m03 = mat.m3;
			m.m10 = mat.m4;
			m.m11 = mat.m5;
			m.m12 = mat.m6;
			m.m13 = mat.m7;
			m.m20 = mat.m8;
			m.m21 = mat.m9;
			m.m22 = mat.m10;
			m.m23 = mat.m11;
			m.m30 = mat.m12;
			m.m31 = mat.m13;
			m.m32 = mat.m14;
			m.m33 = mat.m15;
			return m;
		}
	}
}
