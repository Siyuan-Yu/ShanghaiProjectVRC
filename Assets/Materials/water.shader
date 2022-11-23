Shader "Custom/water"
{
   Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_Cube ("Reflection Cubemap", Cube) = "_Skybox" { }
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_BumpFactor ("Bump Height", Range(1, 10)) = 6.
		_WaveSpeed1 ("Wave Speed 1", Range(0, 100)) = 64.
		_WaveSpeed2 ("Wave Speed 2", Range(0, 100)) = 50.2
	}
	//
	//	Quest Water Shader with reflections and animated ripples
	//	
	//	This is the shader used in the Bamboo Temple world especially made
	//	to work on Quest (works fine on PC too)
	//
	//	For best results use a cubemap of the skybox or reflection probe as
	//	the 'reflection cubemap' and a normal map of water ripples
	//
	//	Not suited for big waves, more for a still mirroring lake with ripples
	//
	SubShader {
		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _BumpMap;
		samplerCUBE _Cube;

		fixed4 _Color;
		fixed4 _ReflectColor;
		fixed _BumpFactor, _WaveSpeed1, _WaveSpeed2;

		struct Input {
			float2 uv_BumpMap;
			float3 worldRefl;
			INTERNAL_DATA
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = _Color;
			o.Albedo = c.rgb;

			o.Normal = (UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap+_Time.g/_WaveSpeed1)) + UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap+_Time.g/_WaveSpeed2)))/_BumpFactor;

			float3 worldRefl = WorldReflectionVector (IN, o.Normal);
			fixed4 reflcol = texCUBE (_Cube, worldRefl);
			o.Emission = reflcol.rgb * _ReflectColor.rgb;
			o.Alpha = 1.0;
		}
		ENDCG
	}
}