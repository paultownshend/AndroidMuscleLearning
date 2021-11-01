using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class RenderOption : MonoBehaviour {

    private float myAlpha = 1;
    public float Alpha
    {
        get { return myAlpha; }
        set
        {
            myAlpha = value;
            UpdateMats();
        }
    }

    private Color myColor = Color.white;
    public Color Emmision
    {
        get { return myColor; }
        set
        {
            myColor = value;
            UpdateMats();
        }
    }

    private Shader OpaqueShader;
    private Shader TransparencyShader;

    private Material[] myMats;
    private int numMats = 0;
    // Use this for initialization
    void Start () {
        OpaqueShader = Shader.Find("Standard");
        TransparencyShader = Shader.Find("Legacy Shaders/Particles/Additive");
        myMats = gameObject.GetComponent<SkinnedMeshRenderer>().materials;
        numMats = myMats.Length;
    }
	
	// Update is called once per frame
	private void UpdateMats () {
        for (int i = 0; i < numMats; i++)
        {
            if(myAlpha > 0.5f)
            {
                var material = myMats[i];
                material.shader = OpaqueShader;
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                Color tmp = myMats[i].color;
                myMats[i].color = new Color(myColor.r, myColor.g, myColor.b, myAlpha);
            }
            else
            {
                var material = myMats[i];
                material.shader = TransparencyShader;
                material.SetColor("_TintColor", new Color(myColor.r, myColor.g, myColor.b, myAlpha));
            }

        }
    }

    
}
