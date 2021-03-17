Shader "Unlit/SimpleSprite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct vertIn
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertOut
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;

            float4 _Color;

            vertOut vert(vertIn v)
            {
                vertOut o;

                UNITY_SETUP_INSTANCE_ID(v);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(vertOut i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
