Shader "Custom/MaskedPeepholeShader"
{
    Properties
    {
        _MainTex("Render Texture", 2D) = "white" {}
        _MaskTex("Alpha Mask", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uvMask = v.uv; // still use mesh UVs for the mask
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.screenPos.xy / i.screenPos.w;     // screen-space UVs for _MainTex
                float mask = tex2D(_MaskTex, i.uvMask).a;       // object-space UVs for _MaskTex

                fixed4 viewColor = tex2D(_MainTex, uv);
                fixed4 finalColor = viewColor * mask;
                finalColor.a = mask;

                return finalColor;
            }
            ENDCG
        }
    }
}
