Shader "Unlit/DefaultSimpleSprite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        BindChannels {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
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
            #pragma multi_compile __ USE_IN_PARTICLE

            #pragma instancing_options procedural:vertInstancingSetup

            #include "UnityCG.cginc"
            #include "UnityStandardParticles.cginc"

            struct vertIn
            {
                float4 vertex : POSITION;
                float2 texcoords : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertOut
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            vertOut vert(vertIn v)
            {
                vertOut o;

                UNITY_SETUP_INSTANCE_ID(v);

                o.color = v.color;
                o.texcoord = v.texcoords;

                vertColor(o.color);
                vertTexcoord(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(vertOut i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

                col.rgb *= col.a;

                return col;
            }
            ENDCG
        }
    }
}
