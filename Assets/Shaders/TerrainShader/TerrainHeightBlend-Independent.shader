﻿// The MIT License (MIT) (see LICENSE.txt)
// Copyright © 2021 Jens Neitzel

Shader "Terrain/HeightBlend Independent"
{
    Properties
    {
        _IndepControl ("Splat Map", 2D) = "red" {}
        
        _AlbedoMap0 ("Layer 0 Albedo Map", 2D) = "grey" {}
        _HeightMap0 ("Layer 0 Height Map", 2D) = "grey" {}
        _NormalMap0 ("Layer 0 Normal Map", 2D) = "bump" {}
        _NormalScale0 ("Layer 0 Normal Map Scale", Float) = 1.0
        _MetalFactor0 ("Layer 0 Metallic", Range(0.0, 1.0)) = 0.0
        _SmoothnessFactor0 ("Layer 0 Smoothness", Range(0.0, 1.0)) = 1.0
        
        _AlbedoMap1 ("Layer 1 Albedo Map", 2D) = "grey" {}
        _HeightMap1 ("Layer 1 Height Map", 2D) = "grey" {}
        _NormalMap1 ("Layer 1 Normal Map", 2D) = "bump" {}
        _NormalScale1 ("Layer 1 Normal Map Scale", Float) = 1.0
        _MetalFactor1 ("Layer 1 Metallic", Range(0.0, 1.0)) = 0.0
        _SmoothnessFactor1 ("Layer 1 Smoothness", Range(0.0, 1.0)) = 1.0
        
        _AlbedoMap2 ("Layer 2 Albedo Map", 2D) = "grey" {}
        _HeightMap2 ("Layer 2 Height Map", 2D) = "grey" {}
        _NormalMap2 ("Layer 2 Normal Map", 2D) = "bump" {}
        _NormalScale2 ("Layer 2 Normal Map Scale", Float) = 1.0
        _MetalFactor2 ("Layer 2 Metallic", Range(0.0, 1.0)) = 0.0
        _SmoothnessFactor2 ("Layer 2 Smoothness", Range(0.0, 1.0)) = 1.0
        
        
        _DistantMap ("Distant Map", 2D) = "grey" {}
        _DistantMapSmoothnessFactor ("Distant Map Smoothness", Range(0.0, 1.0)) = 1.0
        _DistMapBlendDistance("Distant Map Blend Distance", Range(0.0, 128)) = 64
        _DistMapInfluenceMin("Distant Map Influence Min", Range(0.0, 1.0)) = 0.2
        _DistMapInfluenceMax("Distant Map Influence Max", Range(0.0, 1.0)) = 0.8
        
        _OverlapDepth("Height Blend Overlap Depth", Range(0.001, 1.0)) = 0.07
        _Parallax ("Parallax Height", Range (0.005, 0.08)) = 0.02
    }

    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
        }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard vertex:SplatmapVert addshadow fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        #include "textureNoTile.cginc"

        UNITY_DECLARE_TEX2D(_IndepControl);
        float4 _Control_TexelSize;
        sampler2D _AlbedoMap0, _AlbedoMap1, _AlbedoMap2;
        float4 _AlbedoMap0_ST, _AlbedoMap1_ST, _AlbedoMap2_ST;
        sampler2D _HeightMap0, _HeightMap1, _HeightMap2;
        UNITY_DECLARE_TEX2D_NOSAMPLER(_DistantMap);
        half _DistMapBlendDistance;
        fixed _DistMapInfluenceMin;
        fixed _DistMapInfluenceMax;
        half _OverlapDepth;
        half _Parallax;
        sampler2D _NormalMap0, _NormalMap1, _NormalMap2;
        half _NormalScale0, _NormalScale1, _NormalScale2;
        half _MetalFactor0, _MetalFactor1, _MetalFactor2;
        half _SmoothnessFactor0, _SmoothnessFactor1, _SmoothnessFactor2, _DistantMapSmoothnessFactor;

        struct Input
        {
            float4 tc;
            half ViewDist;
            half3 viewDir;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void SplatmapVert (inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            data.ViewDist = length(UnityObjectToViewPos(v.vertex).xyz);
            data.viewDir = normalize(ObjSpaceViewDir(v.vertex));
            v.tangent.w = -1;
            data.tc.xy = v.texcoord;
        }

        half blendByHeight(half texture1height,  half texture2height,  half control1height,  half control2height,  half overlapDepth,  out half textureHeightOut,  out half controlHeightOut)
        {
            half texture1heightPrefilter = texture1height * sign(control1height);
            half texture2heightPrefilter = texture2height * sign(control2height);
            half height1 = texture1heightPrefilter + control1height;
            half height2 = texture2heightPrefilter + control2height;
            half blendFactor = (clamp(((height1 - height2) / overlapDepth), -1, 1) + 1) / 2;
            // Substract positive differences of the other control height to not make one texture height benefit too much from the other.
            textureHeightOut = max(0, texture1heightPrefilter - max(0, control2height-control1height)) * blendFactor + max(0, texture2heightPrefilter - max(0, control1height-control2height)) * (1 - blendFactor);
            // Propagate sum of control heights to not loose height.
            controlHeightOut = control1height + control2height;
            return blendFactor;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 splat_control = UNITY_SAMPLE_TEX2D (_IndepControl, IN.tc.xy);
            fixed4 texDistantMap = UNITY_SAMPLE_TEX2D_SAMPLER (_DistantMap, _IndepControl, IN.tc.xy) * half4(1,1,1,_DistantMapSmoothnessFactor);

            fixed2 uvSplat0 = TRANSFORM_TEX(IN.tc.xy, _AlbedoMap0);
            fixed2 uvSplat1 = TRANSFORM_TEX(IN.tc.xy, _AlbedoMap1);
            fixed2 uvSplat2 = TRANSFORM_TEX(IN.tc.xy, _AlbedoMap2);
            
            NoTileUVs ntuvs0 = textureNoTileCalcUVs(uvSplat0);
            NoTileUVs ntuvs1 = textureNoTileCalcUVs(uvSplat1);
            NoTileUVs ntuvs2 = textureNoTileCalcUVs(uvSplat2);
            fixed texture0Height = textureNoTile(_HeightMap0, ntuvs0);
            fixed texture1Height = textureNoTile(_HeightMap1, ntuvs1);
            fixed texture2Height = textureNoTile(_HeightMap2, ntuvs2);

            // Calculate Blend factors
            half textHeight1, textHeight2;
            half ctrlHeight1, ctrlHeight2;
            half blendFactor01 = blendByHeight(texture0Height, texture1Height, splat_control.r, splat_control.g, _OverlapDepth, textHeight1, ctrlHeight1);
            half blendFactor12 = blendByHeight(textHeight1, texture2Height, ctrlHeight1, splat_control.b, _OverlapDepth, textHeight2, ctrlHeight2);

            // Calculate Parallax after final heigth is known
            fixed2 paraOffset = ParallaxOffset(textHeight2, _Parallax, IN.viewDir);

            // Calculate UVs again, now with Parallax Offset
            ntuvs0 = textureNoTileCalcUVs(uvSplat0+paraOffset);
            ntuvs1 = textureNoTileCalcUVs(uvSplat1+paraOffset);
            ntuvs2 = textureNoTileCalcUVs(uvSplat2+paraOffset);

            // Sample Textures using the modified UVs
            fixed4 texture0 = textureNoTile(_AlbedoMap0, ntuvs0) * half4(1,1,1,_SmoothnessFactor0);
            fixed4 texture1 = textureNoTile(_AlbedoMap1, ntuvs1) * half4(1,1,1,_SmoothnessFactor1);
            fixed4 texture2 = textureNoTile(_AlbedoMap2, ntuvs2) * half4(1,1,1,_SmoothnessFactor2);

            // Blend Textures based on calculated blend factors
            fixed4 mixedDiffuse = texture0 * blendFactor01 + texture1 * (1 - blendFactor01);
            mixedDiffuse = mixedDiffuse * blendFactor12 + texture2 * (1 - blendFactor12);
    
            // Blend with Distant Map
            fixed influenceDist = clamp(IN.ViewDist/_DistMapBlendDistance+_DistMapInfluenceMin, 0, _DistMapInfluenceMax);
            mixedDiffuse = texDistantMap * influenceDist + mixedDiffuse * (1-influenceDist);

            // Sample Normal Maps using the modified UVs
            fixed3 texture0normal = textureNoTileNormal(_NormalMap0, ntuvs0, _NormalScale0);
            fixed3 texture1normal = textureNoTileNormal(_NormalMap1, ntuvs1, _NormalScale1);
            fixed3 texture2normal = textureNoTileNormal(_NormalMap2, ntuvs2, _NormalScale2);
            
            // Blend Normal maps based on calculated blend factors
            fixed3 mixedNormal = texture0normal * blendFactor01 + texture1normal * (1 - blendFactor01);
            mixedNormal = mixedNormal * blendFactor12 + texture2normal * (1 - blendFactor12);
            mixedNormal.z += 1e-5f; // to avoid nan after normalizing
            
            // Blend Metallness based on calculated blend factors
            fixed mixedMetallic = _MetalFactor0 * blendFactor01 + _MetalFactor1 * (1 - blendFactor01);
            mixedMetallic = mixedMetallic * blendFactor12 + _MetalFactor2 * (1 - blendFactor12);

            o.Albedo = mixedDiffuse.rgb;
            o.Smoothness = mixedDiffuse.a;
            o.Normal = mixedNormal;
            o.Metallic = mixedMetallic;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

