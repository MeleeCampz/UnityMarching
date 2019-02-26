using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class RayMarchingCamera : SceneViewFilter
{
	public Shader Shader;
	public float maxDistance;
	[Range(1,1000)]
	public int maxIterations;
	[Range(0.1f, 0.0001f)]
	public float accuracy;
	public float repetation;

	[Header("Directional Light")]
	public Light directionalLight;

	[Header("Shadow")]
	public Vector2 shadowDistance;
	[Range(1,128)]
	public float shadowPenumbra;

	[Header("AO")]
	[Range(0.01f, 10.0f)]
	public float aoStepSize;
	[Range(0.0f, 1.0f)]
	public float aoIntensity;
	[Range(1, 10)]
	public int aoIterations;

	[Header("SDF")]
	public Color mainColor;
	public Vector4 sphere1;
	public Vector4 box1;
	public float box1Round;
	public float boxSphereSmooth;
	public Vector4 sphere2;
	public float sphereIntersectSmooth;

	public Material RaymarchMaterial
	{
		get
		{
			if (!_raymatchMat && Shader)
			{
				_raymatchMat = new Material(Shader);
				_raymatchMat.hideFlags = HideFlags.HideAndDontSave;
			}
			return _raymatchMat;
		}
	}
	private Material _raymatchMat;

	public Camera Camera
	{
		get
		{
			if (!_cam)
			{
				_cam = GetComponent<Camera>();
			}
			return _cam;
		}
	}
	private Camera _cam;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!RaymarchMaterial)
		{
			Graphics.Blit(source, destination);
			return;
		}

		_raymatchMat.SetMatrix("_CamFrustum", CamFrustum(Camera));
		_raymatchMat.SetMatrix("_CamToWorld", Camera.cameraToWorldMatrix);

		_raymatchMat.SetFloat("_MaxIterations", maxIterations);
		_raymatchMat.SetFloat("_Accuracy", accuracy);
		_raymatchMat.SetFloat("_Repetation", repetation);

		_raymatchMat.SetFloat("_LightIntensity", directionalLight ? directionalLight.intensity : 0.5f);
		_raymatchMat.SetFloat("_ShadowIntensity", directionalLight ? directionalLight.shadowStrength : 0.5f);
		_raymatchMat.SetFloat("_ShadowPenumbra", shadowPenumbra);
		_raymatchMat.SetFloat("_MaxDistance", maxDistance);
		_raymatchMat.SetFloat("_Box1Round", box1Round);
		_raymatchMat.SetFloat("_BoxSphereSmooth", boxSphereSmooth);
		_raymatchMat.SetFloat("_SphereIntersectionSmooth", sphereIntersectSmooth);

		_raymatchMat.SetVector("_LightDir", directionalLight ? directionalLight.transform.forward : Vector3.one);
		_raymatchMat.SetVector("_Sphere1", sphere1);
		_raymatchMat.SetVector("_Sphere2", sphere2);
		_raymatchMat.SetVector("_Box1", box1);
		_raymatchMat.SetVector("_ShadowDistance", shadowDistance);

		_raymatchMat.SetColor("_MainColor", mainColor);
		_raymatchMat.SetColor("_LightCol", directionalLight ? directionalLight.color : Color.white);

		//AO
		_raymatchMat.SetFloat("_AoStepSize", aoStepSize);
		_raymatchMat.SetFloat("_AoIntensity", aoIntensity);
		_raymatchMat.SetInt("_AoIterations", aoIterations);

		RenderTexture.active = destination;
		_raymatchMat.SetTexture("_MainTex", source);

		GL.PushMatrix();
		GL.LoadOrtho();
		RaymarchMaterial.SetPass(0);
		GL.Begin(GL.QUADS);

		//BL
		GL.MultiTexCoord2(0, 0.0f, 0.0f);
		GL.Vertex3(0.0f, 0.0f, 3.0f);
		//BR
		GL.MultiTexCoord2(0, 1.0f, 0.0f);
		GL.Vertex3(1.0f, 0.0f, 2.0f);
		//TR
		GL.MultiTexCoord2(0, 1.0f, 1.0f);
		GL.Vertex3(1.0f, 1.0f, 1.0f);
		//TL
		GL.MultiTexCoord2(0, 0.0f, 1.0f);
		GL.Vertex3(0.0f, 1.0f, 0.0f);

		GL.End();
		GL.PopMatrix();
	}

	private Matrix4x4 CamFrustum(Camera cam)
	{
		Matrix4x4 frustum = Matrix4x4.identity;
		float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad);

		Vector3 goUp = Vector3.up * fov;
		Vector3 goRight = Vector3.right * fov * cam.aspect;

		Vector3 TL = (-Vector3.forward - goRight + goUp);
		Vector3 TR = (-Vector3.forward + goRight + goUp);
		Vector3 BR = (-Vector3.forward + goRight - goUp);
		Vector3 BL = (-Vector3.forward - goRight - goUp);

		frustum.SetRow(0, TL);
		frustum.SetRow(1, TR);
		frustum.SetRow(2, BR);
		frustum.SetRow(3, BL);

		return frustum;
	}
}
