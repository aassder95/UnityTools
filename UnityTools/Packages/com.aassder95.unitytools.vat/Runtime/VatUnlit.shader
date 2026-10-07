Shader "UnityTools/VAT/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _VatPositions ("VAT Positions", 2D) = "black" {}
        _VatFrame ("Frame", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            sampler2D _VatPositions;
            float4 _VatPositions_TexelSize;
            float _VatFrame;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; uint id : SV_VertexID; };
            struct v2f { float4 position : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata v)
            {
                v2f o;
                float frame = clamp(_VatFrame, 0, _VatPositions_TexelSize.w - 1);
                float2 uv = float2((v.id + 0.5) * _VatPositions_TexelSize.x, (floor(frame) + 0.5) * _VatPositions_TexelSize.y);
                float3 first = tex2Dlod(_VatPositions, float4(uv, 0, 0)).xyz;
                uv.y = (min(floor(frame) + 1, _VatPositions_TexelSize.w - 1) + 0.5) * _VatPositions_TexelSize.y;
                float3 second = tex2Dlod(_VatPositions, float4(uv, 0, 0)).xyz;
                o.position = UnityObjectToClipPos(float4(lerp(first, second, frac(frame)), 1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target { return tex2D(_MainTex, i.uv) * _Color; }
            ENDCG
        }
    }
}
