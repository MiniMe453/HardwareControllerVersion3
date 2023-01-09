using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

// [ExecuteInEditMode]
// [RequireComponent(typeof(Camera))]
// #if UNITY_5_4_OR_NEWER
//     [ImageEffectAllowedInSceneView]
// #endif
public class LIDARStencilRenderer : MonoBehaviour
{
    static class Uniforms
    {
        internal static readonly int _RoughnessThreashold = Shader.PropertyToID("_RoughnessThreashold");
        internal static readonly int _BlendValue = Shader.PropertyToID("_Blend");
        internal static readonly int _LIDARColor = Shader.PropertyToID("_Color");
    }

    private Camera m_Camera;
    private Camera camera_
    {
        get
        {
            if (m_Camera == null)
            {
                m_Camera = GetComponent<Camera>();
                m_Camera.depthTextureMode = m_Camera.depthTextureMode | DepthTextureMode.DepthNormals;
            }
            return m_Camera;
        }
    }

    private Shader m_Shader;
    private Shader shader
    {
        get
        {
            if (m_Shader == null)
            {
                m_Shader = Shader.Find("Hidden/LIDARStencilRender");
            }

            return m_Shader;
        }
    }

    private Material m_Material;
    private Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
            }

            return m_Material;
        }
    }

    [Range(0f, 1f)]
    public float roughnessThreashold = .5f;
    [Range(0f,10f)]
    public float blend = 1f;
    public Color color = Color.red;

    void OnEnable()
    {
        camera_.depthTextureMode = camera_.depthTextureMode | DepthTextureMode.DepthNormals;
        //gameObject.GetComponent<LIDARStencilRenderer>().enabled = true;
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat(Uniforms._RoughnessThreashold, roughnessThreashold);
        material.SetFloat(Uniforms._BlendValue, blend);
        material.SetColor(Uniforms._LIDARColor, color);

        Matrix4x4 viewToWorld = camera_.cameraToWorldMatrix;
        material.SetMatrix("_viewToWorld", viewToWorld);

        // Pass zero resets the stencil buffer
        Graphics.Blit(source, destination, material, 0);

        // Pass one writes one t specific pixels based on fragemt shader
        Graphics.Blit(source, destination, material, 1);

        // Pass two applies the image effect for stencil = 0
        Graphics.Blit(source, destination, material, 2);

        // // Pass three applies the image effect fr stencil = 1
        Graphics.Blit(source, destination, material, 3);
    }
}
