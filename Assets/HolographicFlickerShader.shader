Shader "Custom/HolographicFlicker"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {} // Main button texture
        _FlickerIntensity("Flicker Intensity", Range(0, 1)) = 0.5
        _TimeSpeed("Time Speed", Range(0.1, 5.0)) = 1.0
        _DistortionScale("Distortion Scale", Float) = 1.0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" }
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float _FlickerIntensity;
                float _TimeSpeed;
                float _DistortionScale;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    // Get current time and scale it by speed
                    float flickerTime = _Time.y * _TimeSpeed;

                // Offset the UV coordinates using sine and distortion
                float offset = sin(i.uv.y * 10.0 + flickerTime) * _DistortionScale;
                float2 uvOffset = i.uv + float2(offset, 0.0);

                // Sample the main texture with the offset UVs
                fixed4 col = tex2D(_MainTex, uvOffset);

                // Apply a flicker effect using a sine wave
                float flicker = abs(sin(flickerTime * 20.0)) * _FlickerIntensity;
                col.rgb += flicker; // Add flicker to the texture color

                return col;
            }
            ENDCG
        }
        }
}
