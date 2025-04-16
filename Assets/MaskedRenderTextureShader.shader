Shader "Custom/MaskedRenderTexture"
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
                float2 uv : TEXCOORD0;
                float2 uvMask : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uvMask = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uvMask).a;

                fixed4 baseColor = fixed4(0, 0, 0, 1); // solid black
                fixed4 moonCol = tex2D(_MainTex, i.uv);

                fixed4 finalColor = lerp(0, baseColor + moonCol, mask);
                finalColor.a = mask;

                return finalColor;
            }
            ENDCG
        }
    }
}