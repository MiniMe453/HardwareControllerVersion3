// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/LIDARStencilRender"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
        #include "UnityCG.cginc"

        float4x4 _viewToWorld;
        //the depth normals texture
        sampler2D _CameraDepthNormalsTexture;

        //effect customisation
        float4 _Color;
        float _Blend;
        // float4 _Color1; variables for later imo
        // float4 _Color2;

        struct Input
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        sampler2D _MainTex;
        sampler2D _CameraGBufferTexture1;
        sampler2D _CameraDepthTexture;

        float _RoughnessThreashold;

        Varyings Vertex(Input v)
        {
            Varyings o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        fixed4 FragmentPassThrough(Varyings input) : SV_Target
        {
            return tex2D(_MainTex, input.uv);
        }

        fixed4 FragmentSelect(Varyings input) : SV_Target
        {
            float4 roughness = tex2D(_CameraGBufferTexture1, input.uv).a;
            if (roughness < _RoughnessThreashold)
            {
                discard;
            }
            return tex2D(_MainTex, input.uv);
        }

        fixed4 FragmentMix1(Varyings input) : SV_Target
        {
                float4 depthnormal = tex2D(_CameraDepthNormalsTexture, input.uv);
                //decode depthnormal
                float3 normal;
                normal = DecodeViewNormalStereo(depthnormal) * float3(1.0, 1.0, 0);

                normal = mul((float3x3)_viewToWorld, normal);

                if(normal.x < 1)
                    normal.x *= -1;
                
                normal.x *= _Blend;
                normal.x = clamp(normal.x, 0,1);

                return float4(normal.xy, 0.5, 1);
        }

        fixed4 FragmentMix2(Varyings input) : SV_Target
        {
            float4 depthnormal = tex2D(_CameraDepthNormalsTexture, input.uv);

            //decode depthnormal
            float3 normal;
            float depth;
            DecodeDepthNormal(depthnormal, depth, normal);

            //get depth as distance from camera in units 
            depth = depth * _ProjectionParams.z;

            float4 finalCol = (1-depth) * _Color;

            return finalCol;
        }
        ENDCG

        Pass
        {
            Stencil
            {
                Ref 0
                Comp Always
                Pass Replace
            }
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentPassThrough
            ENDCG
        }

        Pass
        {
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }

            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentSelect
            ENDCG
        }

        Pass
        {
            Stencil
            {
                Ref 0
                Comp Equal
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentMix1
            ENDCG
        }

        Pass
        {
            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }

            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentMix2
            ENDCG
        }
    }
}
