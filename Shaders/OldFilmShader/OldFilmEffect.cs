using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OldFilmEffect : MonoBehaviour
{

    public Shader Shader = null;
    //[Range(0, 1)]
    public Color addtiveColor;

    [Range(0, 1)]
    public float HoloAmount = 1.0f;
    public Texture HoloTex;

    [Range(0, 1)]
    public float DustAmount = 1.0f;
    public Texture DustTex;

    [Range(0, 1)]
    public float LinesAmount = 1.0f;
    public Texture LinesTex;
    [Range(0, 1)]
    public float LinesXSpeed = 1.0f;
    [Range(0, 1)]
    public float LinesYSpeed = 1.0f;

    [Range(0, 1)]
    public float EffectAmount = 1.0f;

    private float RandomVal;

    static Material m_Material = null;
    protected Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(Shader);
                m_Material.hideFlags = HideFlags.DontSave;
            }
            return m_Material;
        }
    }

	// Use this for initialization
	void Start () {
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }
        // Disable if the shader can't run on the users graphics card
        if (!Shader || !material.shader.isSupported)
        {
            enabled = false;
            return;
        }
	}

    public void Update()
    {
        RandomVal = Random.Range(-1f, 1f);
    }

	
	// Called by the camera to apply the image effect
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Debug.Log(string.Format("width = {0}, height = {1}", source.width , source.height));
        //RenderTexture buffer = RenderTexture.GetTemporary(source.width, source.height, 0);
        material.SetColor("_AddTiveColor", addtiveColor);
        material.SetFloat("_HoloAmount", HoloAmount);
        material.SetTexture("_HoloTex", HoloTex);
        material.SetFloat("_DustAmount", DustAmount);
        material.SetTexture("_DustTex", DustTex);
        material.SetFloat("_LinesAmount", LinesAmount);
        material.SetTexture("_LinesTex", LinesTex);
        material.SetFloat("_LinesXSpeed", LinesXSpeed);
        material.SetFloat("_LinesYSpeed", LinesYSpeed);
        material.SetFloat("_RandomVal", RandomVal);
        material.SetFloat("_EffectAmount", EffectAmount);
        Graphics.Blit(source, destination, material);
    }
}
