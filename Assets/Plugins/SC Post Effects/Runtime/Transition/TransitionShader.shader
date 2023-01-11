Shader "Hidden/SC Post Effects/Transition"
{
	HLSLINCLUDE

	#include "../../Shaders/Pipeline/Pipeline.hlsl"

	TEXTURE2D(_Gradient);
	SamplerState sampler_Gradient;
	
	//sampler2D _CameraDepthNormalsTexture;

	sampler2D _CameraGBufferTexture1;
	sampler2D _CameraGBufferTexture2;
	//sampler2D _CameraDepthTexture;

	float _Progress;

	float _DepthPower;

	float PosterizeChannel(float val, float amount)
	{
		return floor(val * amount) / (amount -1.0);
	}

	half3 AdjustContrastCurve(half3 color, half contrast) {
		return pow(abs(color * 2 - 1), 1 / max(contrast, 0.0001)) * sign(color - 0.5) + 0.5;
	}

	float4 SobelEdgeDetect(float4 inputCol, Varyings input)
	{
		// UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		// inspired by borderlands implementation of popular "sobel filter"
		half4 original = inputCol;

		float centerDepth = LINEAR_DEPTH(SAMPLE_DEPTH(UV));
		//return centerDepth;
		float4 depthsDiag;
		float4 depthsAxis;

		float2 texelSize = 1 * _MainTex_TexelSize.xy;

		// depthsDiag.x = LINEAR_DEPTH(SAMPLE_DEPTH(UV + texelSize)); // TR
		// depthsDiag.y = LINEAR_DEPTH(SAMPLE_DEPTH(UV + texelSize * half2(-1, 1))); // TL
		// depthsDiag.z = LINEAR_DEPTH(SAMPLE_DEPTH(UV - texelSize * half2(-1, 1))); // BR
		// depthsDiag.w = LINEAR_DEPTH(SAMPLE_DEPTH(UV - texelSize)); // BL

		// depthsAxis.x = LINEAR_DEPTH(SAMPLE_DEPTH(UV + texelSize * half2(0, 1))); // T
		// depthsAxis.y = LINEAR_DEPTH(SAMPLE_DEPTH(UV - texelSize * half2(1, 0))); // L
		// depthsAxis.z = LINEAR_DEPTH(SAMPLE_DEPTH(UV + texelSize * half2(1, 0))); // R
		// depthsAxis.w = LINEAR_DEPTH(SAMPLE_DEPTH(UV - texelSize * half2(0, 1))); // B	

		// //Thin edges
		// // if (_SobelParams.x == 1) {
		// // 	depthsDiag = (depthsDiag > centerDepth.xxxx) ? depthsDiag : centerDepth.xxxx;
		// // 	depthsAxis = (depthsAxis > centerDepth.xxxx) ? depthsAxis : centerDepth.xxxx;
		// // }
		// depthsDiag -= centerDepth;
		// depthsAxis /= centerDepth;

		// const float4 HorizDiagCoeff = float4(1,1,-1,-1);
		// const float4 VertDiagCoeff = float4(-1,1,-1,1);
		// const float4 HorizAxisCoeff = float4(1,0,0,-1);
		// const float4 VertAxisCoeff = float4(0,1,-1,0);

		// float4 SobelH = depthsDiag * HorizDiagCoeff + depthsAxis * HorizAxisCoeff;
		// float4 SobelV = depthsDiag * VertDiagCoeff + depthsAxis * VertAxisCoeff;

		// float SobelX = dot(SobelH, float4(1,1,1,1));
		// float SobelY = dot(SobelV, float4(1,1,1,1));
		// float Sobel = sqrt(SobelX * SobelX + SobelY * SobelY);

		// Sobel = 1.0 - pow(saturate(Sobel), 1);

		// float edgeSobel = 1 - Sobel;

		// //Orthographic camera: Still not correct, but value should be flipped
		// if (unity_OrthoParams.w) edgeSobel = 1 - edgeSobel;

		// //Edges only
		// half4 originalSobel = lerp(original, float4(0, 0, 0, 1), 0);

		// //Opacity
		// float3 edgeColor = lerp(originalSobel.rgb, float3(0,0,0), 1);


		// return float4(lerp(original.rgb, edgeColor.rgb, edgeSobel).rgb, original.a);
		//End of Sobel stuff

		//Luminance edge detection
		half3 p1 = original.rgb;
		half3 p2 = ScreenColor(UV + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * 1).rgb;
		half3 p3 = ScreenColor(UV + float2(+_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * 1).rgb;

		half3 diff = p1 * 2 - p2 - p3;
		half edgeLum = dot(diff, diff);
		edgeLum = step(edgeLum, 0.2);

		edgeLum = 1 - edgeLum;

		//Edges only
		half4 originalLum = lerp(original, float4(0, 0, 0, 1), 0);

		//Opacity
		float3 edgeColorLum = lerp(originalLum.rgb, float3(0,0,0), 1);
		edgeColorLum = saturate(edgeColorLum);

		//Combining sobel and luminance edge detections

		//float edgeFinal = max(edgeSobel + 0.15, edgeLum / 1.5);

		//return float4(lerp(original.rgb, edgeColor.rgb, edge).rgb, original.a);
		return float4(lerp(originalLum.rgb, edgeColorLum.rgb, edgeLum).rgb, originalLum.a);
	}

	half3 AdjustContrast2(half3 color, half contrast) {
    	return saturate(lerp(half3(0.5, 0.5, 0.5), color, contrast));
	}

	float4 Frag(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		float4 screenColor = SCREEN_COLOR(UV);

		float gradientTex = SAMPLE_TEXTURE2D(_Gradient, sampler_Gradient, UV).r;

		float alpha = smoothstep(gradientTex, _Progress, 1.01);

		float4 depthnormal = tex2D(_CameraDepthNormalsTexture, input.uv);

		//decode depthnormal
		float3 normal;
		float depth;
		DecodeDepthNormal(depthnormal, depth, normal);

		depth = PosterizeChannel(depth, 64);

		//float4 depthnormal = tex2D(_CameraDepthNormalsTexture, input.uv);

		float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
		// float3 color = max(0, dot(tex2D(_CameraGBufferTexture2, input.uv), lightDirection));

		//float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(float4(input.uv, 0, 0))).r;

		//return tex2D(_CameraGBufferTexture2, input.uv);
		//return SobelEdgeDetect(float4(AdjustContrastCurve(depth, 2), 1), input);

		return  1 - float4(AdjustContrast2(depth, 3), 1);
		//return max(0, dot(tex2D(_CameraGBufferTexture2, input.uv), lightDirection));
	}

	ENDHLSL

	SubShader
	{
	Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "Transition"
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles

			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}
	}
}