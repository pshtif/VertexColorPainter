/*
 *	Created by:  Peter @sHTiF Stefcek
 */

Shader "Hidden/Vertex Color Painter/VertexColorPainterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ChannelMask("Channel Mask", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                half4 color : COLOR0;
                
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ChannelMask;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // Do some mask hacking to show color of correct channel - sHTiF
                half4 color = lerp(v.color, v.uv, _ChannelMask.x);
                color = lerp(color, v.uv1, _ChannelMask.y);
                color = lerp(color, v.uv2, _ChannelMask.z);
                o.color = color;
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}