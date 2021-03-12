using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class StereoPlanetaryFogImageEffect : PlanetaryFogImageEffect
    {
		private Camera _originalCamera;
		private Material _fogMaterial;
		private Vector3[] _frustumCorners = new Vector3[4];

		private void Awake()
		{
			Shader.SetGlobalVector("_FogParams", new Vector4(0f, 0f, 1f, 0f));
		}

		private void OnDestroy()
		{
			if (_fogMaterial != null)
			{
				UnityEngine.Object.Destroy(_fogMaterial);
			}
			_fogMaterial = null;
		}

		private Matrix4x4 FrustumCornersMatrix(Camera cam, Camera.MonoOrStereoscopicEye eye)
		{
			var camtr = cam.transform;
			cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, eye, _frustumCorners);

			Matrix4x4 frustumMatrix = Matrix4x4.identity;
			frustumMatrix.SetRow(0, camtr.TransformVector(_frustumCorners[1])); //topLeft
			frustumMatrix.SetRow(1, camtr.TransformVector(_frustumCorners[2])); //topRight
			frustumMatrix.SetRow(2, camtr.TransformVector(_frustumCorners[3])); //bottomRight
			frustumMatrix.SetRow(3, camtr.TransformVector(_frustumCorners[0])); //bottomLeft
			return frustumMatrix;
		}

		[ImageEffectOpaque]
		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (_originalCamera == null)
			{
				_originalCamera = GetComponent<Camera>();
			}
			if (_fogMaterial == null && fogShader != null)
			{
				_fogMaterial = new Material(fogShader);
			}
			if (_originalCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Mono)
				return;
			if (_fogMaterial != null)
			{
				_fogMaterial.SetMatrix("_FrustumCornersWS", FrustumCornersMatrix(_originalCamera, _originalCamera.stereoActiveEye));
				CustomGraphicsBlit(source, destination, _fogMaterial);
			}
		}

		private void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material mat)
		{
			RenderTexture.active = dest;
			mat.SetTexture("_MainTex", source);
			GL.PushMatrix();
			GL.LoadOrtho();
			mat.SetPass(0);
			GL.Begin(GL.QUADS);
			GL.MultiTexCoord2(0, 0f, 0f);
			GL.Vertex3(0f, 0f, 3f);
			GL.MultiTexCoord2(0, 1f, 0f);
			GL.Vertex3(1f, 0f, 2f);
			GL.MultiTexCoord2(0, 1f, 1f);
			GL.Vertex3(1f, 1f, 1f);
			GL.MultiTexCoord2(0, 0f, 1f);
			GL.Vertex3(0f, 1f, 0f);
			GL.End();
			GL.PopMatrix();
		}
	}
}
