Shader "RayMarching/RayMarching"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"
			#include "DistanceFunctions.cginc"

			//#include "SimplexNoise3D.hlsl"
			#include "SimplexNoise2D.hlsl"

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			uniform sampler2D _CameraDepthTexture;
			uniform float4x4 _CamFrustum, _CamToWorld;
			uniform int _MaxIterations;
			uniform float _Accuracy;
			uniform float _MaxDistance, _Box1Round, _BoxSphereSmooth, _SphereIntersectionSmooth;
			uniform float4 _Sphere1, _Sphere2, _Box1;
			uniform float3 _LightDir, _LightCol;
			uniform float _LightIntensity;
			uniform fixed4 _MainColor;

			uniform float _Repetation;

			//Shadow
			uniform float2 _ShadowDistance;
			uniform float _ShadowIntensity;
			uniform float _ShadowPenumbra;

			//AO
			uniform float _AoStepSize, _AoIntensity;
			uniform int _AoIterations;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 ray : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				half index = v.vertex.z;
				v.vertex.z = 0;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				o.ray = _CamFrustum[(int)index].xyz;

				o.ray /= abs(o.ray.z);

				o.ray = mul(_CamToWorld, o.ray);

				return o;
			}

			float BoxSphere(float3 p)
			{
				float sphere1 = sdSphere(p, _Sphere1.w);
				float box1 = sdRoundBox(p, _Box1.www, _Box1Round);
				//float combine1 = opS(sphere1, box1);
				float combine1 = opSS(sphere1, box1, _BoxSphereSmooth);
				//float sphere2 = sdSphere(p - _Sphere2.xyz, _Sphere2.w);
				//float combine2 = opIS(sphere2, combine1, _SphereIntersectionSmooth);

				return combine1;
			}

			float SimplexNoise2D(float3 p, float2 s, float h)
			{
				float4 uv = float4(p.x + p.y, p.z + p.y, 0, 0);
				uv *= s.xyxy;
				return p.y - tex2Dlod(_NoiseTex, uv + float4(1, 1, 0, 0) * 1).r * h;
			}

			float rand_1_05(in float2 uv)
			{
				float2 noise = (frac(sin(dot(uv, float2(12.9898, 78.233)*2.0)) * 43758.5453));
				return abs(noise.x + noise.y) * 0.5;
			}


			float distanceField(float3 p, inout float3 inOutMat)
			{
				//float n1 = SimplexNoise2D(p, float2(1, 1)*0.001, 100);
				//nc = length(p);

				//return nc;

				float3 gn = float3(0.75, 1, 0.12);
				float ground1 = sdPlane(p, normalize(float4(gn, 0)));
				float ground = ground1;

				p += _SinTime * 120.0;

				inOutMat = float3(1, 1, 1);

				if (_Repetation > 0)
				{
					inOutMat = pMod3(p, _Repetation);// *(_SinTime.xyz * 0.5 + float3(1, 1, 1) * 1.5));
					float random1 = rand_1_05(inOutMat.xy);
					float random2 = rand_1_05(inOutMat.zy);
					float random3 = rand_1_05(inOutMat.xz);
					inOutMat = float3(random1, random2, random3);
				}

				float boxSphere1 = BoxSphere(p);

				float blend = 0;
				float geo = opUS(ground, boxSphere1,100, blend);

				float3 floorColor = float3(1, 0, 0);
				inOutMat = lerp(inOutMat, floorColor, blend);

				return geo;
			}

			float distanceField(float3 p)
			{
				float3 dummy = float3(0, 0, 0);
				return distanceField(p, dummy);
			}

			float3 getNormal(float3 p)
			{
				const float2 offset = float2(0.001, 0.0);
				float3 n = float3(
					distanceField(p + offset.xyy) - distanceField(p - offset.xyy),
					distanceField(p + offset.yxy) - distanceField(p - offset.yxy),
					distanceField(p + offset.yyx) - distanceField(p - offset.yyx));

				return normalize(n);
			}

			float hardShadow(float3 ro, float3 rd, float mint, float maxt)
			{
				for (float t = mint; t < maxt;)
				{
					float h = distanceField(ro + rd * t);
					if (h < 0.001)
					{
						return 0.0;
					}
					t += h;
				}

				return 1.0;
			}

			float softShadow(float3 ro, float3 rd, float mint, float maxt, float k)
			{
				int step = 0;
				float result = 1.0;
				for (float t = mint; t < maxt && step < 50; step++)
				{
					float h = distanceField(ro + rd * t);
					if (h < 0.001)
					{
						return 0.0;
					}
					result = min(result, k*h / t);
					t += h;
				}

				return result;
			}

			float AmbientOcclusion(float3 p, float3 n)
			{
				float step = _AoStepSize;
				float ao = 0.0;
				float dist;
				for (int i = 1; i <= _AoIterations; i++)
				{
					dist = step * i;
					ao += max(0.0, (dist - distanceField(p + n * dist)) / dist);
				}
				return (1.0 - (ao * _AoIntensity));
			}

			float3 shading(float3 p, float3 n)
			{
				float3 result;
				//Diffuse Color
				float3 color = _MainColor.rgb;
				//Directional light
				float3 light = (_LightCol * dot(-_LightDir, n) * 0.5f + 0.5f) * _LightIntensity;
				//Shadows
				float shadow = softShadow(p, -_LightDir, _ShadowDistance.x, _ShadowDistance.y, _ShadowPenumbra) * 0.5 + 0.5f;
				shadow = max(0.0, pow(shadow, _ShadowIntensity));
				//Ambient Occlusion
				float ao = AmbientOcclusion(p, n);

				result = light * shadow * ao;

				return result;
			}

			fixed4 rayMarching(float3 ro, float3 rd, float depth)
			{
				fixed4 result = fixed4(1, 1, 1, 1);
				const int max_iteration = _MaxIterations;
				float t = 0; //distance traveld along the ray

				for (int i = 0; i < max_iteration; i++)
				{
					if (t > _MaxDistance || t >= depth)
					{
						//Environment
						result = fixed4(rd, 0);
						break;
					}

					float3 p = ro + rd * t;
					//Check hit in SDF
					float3 mat = float3(1,1,1);
					float d = distanceField(p, mat);
					if (d < _Accuracy)
					{
						//shading!
						float3 n = getNormal(p);
						float3 s = shading(p, n);

						result = fixed4(_MainColor.rgb * s * mat,1);
						break;
					}

					t += d;
				}


				return result;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
				depth *= length(i.ray);

				fixed3 col = tex2D(_MainTex, i.uv);

				float3 rayDirection = normalize(i.ray.xyz);
				float3 rayOrigin = _WorldSpaceCameraPos;

				fixed4 result = rayMarching(rayOrigin, rayDirection, depth);
				return fixed4(col * (1.0 - result.w) + result.xyz * result.w, 1.0);
			}
			ENDCG
		}
	}
}
