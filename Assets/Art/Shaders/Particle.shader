Shader "Game/Particle"
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
        ZTest Always
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 position : SV_POSITION;
                fixed4 color : COLOR0;
            };

            struct Particle {
                float2 position;
                float rotation;
                float2 scale;
                float speed;
                fixed4 baseColor;
                float alpha;
                float radius;
                int layer;
                int outOfBounds;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            StructuredBuffer<uint> _Triangles;
            StructuredBuffer<float2> _Positions;
            StructuredBuffer<float2> _UV;
            StructuredBuffer<Particle> _Particles;

            uniform float _Scale;
            uniform float _Alpha;
            uniform int _Layer;

            float2 rotate(float2 position, float rotation) {
                float angle = rotation * UNITY_PI / 180.0;
                float2x2 m = float2x2(cos(angle), -sin(angle), sin(angle), cos(angle));
                return mul(m, position);
            }

            v2f vert (uint vertexID: SV_VertexID, uint instanceID : SV_InstanceID)
            {
                Particle particle = _Particles[instanceID];
                float2 position = rotate(_Positions[_Triangles[vertexID]], particle.rotation) * particle.scale * _Scale;
                float2 worldPosition = particle.position;
                float2 uv = _UV[_Triangles[vertexID]];

                v2f o;
                o.position = mul(UNITY_MATRIX_VP, float4(worldPosition + position, -particle.layer, 1.0));
                o.uv = TRANSFORM_TEX(uv, _MainTex);
                o.color = particle.baseColor;
                o.color.a *= particle.alpha;

                if (_Layer != particle.layer) {
                    o.color.a = 0.0;
                }

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.a *= _Alpha * i.color.a;
                col.rgb *= col.a;
                col.rgb *= GammaToLinearSpace(i.color.rgb);

                if (col.a <= 0.0) {
                    discard;
                }

                return col;
            }

            ENDCG
        }
    }
}
