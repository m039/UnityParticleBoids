Shader "Unlit/SimpleSprite"
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 4.5
            #pragma multi_compile_instancing
            #pragma multi_compile __ USE_IN_PARTICLE

            #pragma instancing_options procedural:vertInstancingSetup

            #define UNITY_PARTICLE_INSTANCE_DATA SimpleParticleInstanceData
            #define UNITY_PARTICLE_INSTANCE_DATA_NO_ANIM_FRAME

            struct SimpleParticleInstanceData
            {
                float3x4 transform;
                uint color;
                float4 custom1;
            };

            #include "UnityCG.cginc"
            #include "UnityStandardParticleInstancing.cginc"

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
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            sampler2D _MainTex;

            vertOut vert(vertIn v)
            {
                vertOut o;

                UNITY_SETUP_INSTANCE_ID(v);

#ifdef UNITY_INSTANCING_ENABLED
                o.color = v.color * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
#else
                o.color = v.color;
#endif

                o.uv = v.uv;

#ifdef USE_IN_PARTICLE
                UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];

                vertInstancingColor(o.color);
                vertInstancingUVs(v.uv, o.uv);
#endif

                o.vertex = UnityObjectToClipPos(v.vertex);

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
