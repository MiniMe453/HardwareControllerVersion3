Shader "Custom/Double Sided Alpha Cutout Fresnet Sphere Normals"
{
    // https://docs.unity3d.com/Manual/SL-VertexFragmentShaderExamples.html
    
    // From: https://forum.unity.com/threads/beginner-in-graphics-on-a-quest-to-do-the-foliage-from-the-witness.518842/
 
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Cutoff("Alpha cutoff", Range(0.0, 1.0)) = 0.4
        _AlphaOffset("Alpha offset", Range(-1.0, 1.0)) = 0
        _NormalsOffset("Normals offset", Vector) = (0,0,0)
    }
    SubShader
    {
        Tags
        {
            //"RenderType"="Transparent"
            "RenderType"="TransparentCutout"
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
        }
        LOD 100
        Cull Off
 
        Pass
        {
            // try use Alpha To Coverage if multisample anti-aliasing is set to 4 (MSAA)
            //AlphaToMask On
            
            Tags { "LightMode" = "ForwardBase" }
       
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            //#pragma multi_compile_fog
 
            #include "UnityCG.cginc"
            #include "Lighting.cginc"    // for _LightColor0
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                half3 normal : NORMAL;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 worldNormal : NORMAL;
                fixed4 diff : COLOR0;
                float fresnel : TEXCOORD1;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Cutoff;
            float _AlphaOffset;
            float3 _NormalsOffset;
           
            v2f vert (appdata v)
            {
                v2f o;
 
                // transform from local to world space
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
 
                // precalc camera-perpedicular alpha
// this works in test meshes but not good with real tree model
                float3 view = normalize(ObjSpaceViewDir(v.vertex));
                float vn = dot(view, v.normal);
                // double sided, flip normal if necessary
                //o.worldNormal *= sign(o.worldNormal);
                vn *= sign(vn);              
                //o.fresnel = _FresnelBias + _FresnelScale * pow(1 + vn, _FresnelPower);
                o.fresnel = saturate(_AlphaOffset + vn);
 
                // overwrite normal with the normals on a sphere
// this should work too but not good, sometimes light source direction flips
                // attention: verctor 0,0,0 can result to NAN
                o.worldNormal = UnityObjectToWorldNormal(normalize(_NormalsOffset + v.vertex.xyz));
 
                // dot product between normal and light direction for standard diffuse (Lambert) lighting
                half nl = max(0, dot(o.worldNormal, _WorldSpaceLightPos0.xyz));
                // factor in the light color
                o.diff = nl * _LightColor0;
                // in addition to the diffuse lighting from the main light, add illumination from ambient or light probes
                // ShadeSH9 function from UnityCG.cginc evaluates it, using world space normal
                o.diff.rgb += ShadeSH9(half4(o.worldNormal, 1));
                o.diff.a = 1;
 
                //UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
           
            fixed4 frag (v2f i /*, fixed facing : VFACE*/) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
               
                 // multiply by diffuse lighting
                col *= saturate(i.diff);
               
                // calc alpha
                col.a *= i.fresnel;
 
                // test show normals
// if enabled the normals look correct
                //col.rgb = i.worldNormal * 0.5 + 0.5;
 
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
 
                clip(col.a - _Cutoff);
               
                return col;
            }
            ENDCG
        }
        }    }