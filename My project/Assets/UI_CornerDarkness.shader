Shader "UI/CornerDarknessSolid"
{
    Properties
    {
        _Radius ("Radius", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Radius;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);

                // policz przejście: 0 = przezroczysty, 1 = pełna czerń
                float vignette = smoothstep(_Radius, 0.647, dist);

                fixed4 col;
                col.rgb = float3(0, 0, 0); // kolor czarny
                col.a = vignette; // alpha zależna od odległości

                return col;
            }
            ENDCG
        }
    }
}
