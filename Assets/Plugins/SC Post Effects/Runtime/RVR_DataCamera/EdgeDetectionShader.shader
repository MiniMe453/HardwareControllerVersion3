Shader "Hidden/SC Post Effects/Edge Detection" {

	HLSLINCLUDE

	#define REQUIRE_DEPTH
	#define REQUIRE_DEPTH_NORMALS
	#include "../../Shaders/Pipeline/Pipeline.hlsl"
	//Camera depth textures

	//Parameters
	sampler2D _CameraGBufferTexture1;
	sampler2D _CameraGBufferTexture0;
	sampler2D _CameraGBufferTexture2;
	uniform half4 _Sensitivity;
	uniform half _BackgroundFade;
	uniform float _EdgeSize;
	uniform float4 _EdgeColor;
	uniform float _EdgeOpacity;
	uniform float _Exponent;
	uniform float _Threshold;
	float4x4 _viewToWorld;
	float4 _FadeParams;

	TEXTURE2D(_ObjectsStencil);
	// SamplerState sampler_Stencil;
	sampler2D sampler_Stencil;
	//X: Start
	//Y: End
	//Z: Invert
	//W: Enabled

	uniform float4 _SobelParams;

	inline half IsSame(half2 centerNormal, float centerDepth, half4 theSample)
	{
		// difference in normals
		half2 diff = abs(centerNormal - theSample.xy) * _Sensitivity.y;
		half isSameNormal = (diff.x + diff.y) * _Sensitivity.y < 0.1;
		// difference in depth
		float sampleDepth = DecodeFloatRG(theSample.zw);
		float zdiff = abs(centerDepth - sampleDepth);
		// scale the required threshold by the distance
		half isSameDepth = zdiff * _Sensitivity.x < 0.09 * centerDepth;

		// return:
		// 1 - if normals and depth are similar enough
		// 0 - otherwise

		return isSameNormal * isSameDepth;
	}

	float4 fragSobel(Varyings input) : SV_Target
	{
//		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

		// inspired by borderlands implementation of popular "sobel filter"
		half4 original = SCREEN_COLOR(UV);
		half4 screenColor = tex2D (_CameraGBufferTexture0, input.uv);
		//half4 screenNormals = tex2D(_CameraGBufferTexture2, input.uv);
		//return screenNormals;
		
		float4 depthnormal = tex2D(_CameraDepthNormalsTexture, input.uv);

		//decode depthnormal
		float3 normal;
		float depth;
		DecodeDepthNormal(depthnormal, depth, normal);
		normal = normal = mul((float3x3)_viewToWorld, normal);
		original = half4(normal, 1);

		float centerDepth = LINEAR_DEPTH(SAMPLE_DEPTH(UV));
		//return centerDepth;
		float4 depthsDiag;
		float4 depthsAxis;

		float slopeAlpha;
		float skyMask;

		if(depth < 1)
			skyMask = 0;
		else
			skyMask = 1;

		if(normal.g < 0.85)
			slopeAlpha = 1;
		else
			slopeAlpha = 0;

		slopeAlpha = slopeAlpha * (1 - skyMask);

		float2 texelSize = _EdgeSize * _MainTex_TexelSize.xy;

		depthsDiag.x = LINEAR_DEPTH(SAMPLE_DEPTH(UV + texelSize)); // TR
		depthsDiag.y = LINEAR_DEPTH(SAMPLE_DEPTH(UV + texelSize * half2(-1, 1))); // TL
		depthsDiag.z = LINEAR_DEPTH(SAMPLE_DEPTH(UV - texelSize * half2(-1, 1))); // BR
		depthsDiag.w = LINEAR_DEPTH(SAMPLE_DEPTH(UV - texelSize)); // BL

		depthsAxis.x = LINEAR_DEPTH(SAMPLE_DEPTH(UV + texelSize * half2(0, 1))); // T
		depthsAxis.y = LINEAR_DEPTH(SAMPLE_DEPTH(UV - texelSize * half2(1, 0))); // L
		depthsAxis.z = LINEAR_DEPTH(SAMPLE_DEPTH(UV + texelSize * half2(1, 0))); // R
		depthsAxis.w = LINEAR_DEPTH(SAMPLE_DEPTH(UV - texelSize * half2(0, 1))); // B	

		//Thin edges
		if (_SobelParams.x == 1) {
			depthsDiag = (depthsDiag > centerDepth.xxxx) ? depthsDiag : centerDepth.xxxx;
			depthsAxis = (depthsAxis > centerDepth.xxxx) ? depthsAxis : centerDepth.xxxx;
		}
		depthsDiag -= centerDepth;
		depthsAxis /= centerDepth;

		const float4 HorizDiagCoeff = float4(1,1,-1,-1);
		const float4 VertDiagCoeff = float4(-1,1,-1,1);
		const float4 HorizAxisCoeff = float4(1,0,0,-1);
		const float4 VertAxisCoeff = float4(0,1,-1,0);

		float4 SobelH = depthsDiag * HorizDiagCoeff + depthsAxis * HorizAxisCoeff;
		float4 SobelV = depthsDiag * VertDiagCoeff + depthsAxis * VertAxisCoeff;


		float SobelX = dot(SobelH, float4(1,1,1,1));
		float SobelY = dot(SobelV, float4(1,1,1,1));
		float Sobel = sqrt(SobelX * SobelX + SobelY * SobelY);

		Sobel = 1.0 - pow(saturate(Sobel), _Exponent);

		float edgeSobel = 1 - Sobel;

		//Orthographic camera: Still not correct, but value should be flipped
		if (unity_OrthoParams.w) edgeSobel = 1 - edgeSobel;

		//Edges only
		half4 originalSobel = lerp(original, float4(0, 0, 0, 1), _BackgroundFade);

		//Opacity
		float3 edgeColor = lerp(originalSobel.rgb, float3(1,1,1), _EdgeOpacity * LinearDepthFade(centerDepth, _FadeParams.x, _FadeParams.y, _FadeParams.z, _FadeParams.w));

		//End of Sobel stuff

		//Luminance edge detection
		half3 p1 = original.rgb;
		half3 p2 = ScreenColor(UV + float2(-_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _EdgeSize).rgb;
		half3 p3 = ScreenColor(UV + float2(+_MainTex_TexelSize.x, -_MainTex_TexelSize.y) * _EdgeSize).rgb;
		p2 = normal;
		p3 = normal;

		half3 diff = p1 * 2 - p2 - p3;
		half edgeLum = dot(diff, diff);
		edgeLum = step(edgeLum, 0.2);

		edgeLum = 1 - edgeLum;

		//Edges only
		half4 originalLum = lerp(original, float4(0, 0, 0, 1), _BackgroundFade);

		//Opacity
		float3 edgeColorLum = lerp(originalLum.rgb, float3(1,1,1), _EdgeOpacity * LinearDepthFade(centerDepth, _FadeParams.x, _FadeParams.y, _FadeParams.z, _FadeParams.w));
		edgeColorLum = saturate(edgeColorLum);

		//Combining sobel and luminance edge detections

		float edgeFinal = max(edgeSobel, edgeLum / 1.5);
		float4 completedEdgeValue = float4(lerp(originalLum.rgb, edgeColorLum.rgb, edgeFinal).rgb, originalLum.a);
		//completedEdgeValue += ((stencilAlpha * 0.5) + (slopeAlpha * 0.25));

		float4 stencilColor;

		stencilColor = float4(0,1,0,1);
//		stencilColor = lerp(float4(1,0.9,0.1,1), stencilColor, 1 - slopeAlpha);


		float4 dataColor = (stencilColor * completedEdgeValue) + screenColor;
		//float4 dataColor = completedEdgeValue + screenColor;



		//return float4(lerp(original.rgb, edgeColor.rgb, edge).rgb, original.a);
		return dataColor;
	}

	ENDHLSL

	Subshader 
	{
		ZTest Always Cull Off ZWrite Off
		
		CGINCLUDE
		#include "UnityCG.cginc"

		ENDCG

		Pass
		{
			Name "Edge Detection: Sobel"
			
			HLSLPROGRAM
			#pragma multi_compile_vertex _ _USE_DRAW_PROCEDURAL
			#pragma exclude_renderers gles
			
			#pragma vertex Vert
			#pragma fragment fragSobel
			ENDHLSL
		}
	}

	Fallback off

} // shader
