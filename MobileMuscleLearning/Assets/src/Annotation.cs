using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Annotation : MonoBehaviour {

    public enum target
    {
        primary, second, third, fourth
    };

    public target annotationTarget;
    public Color lineColor = new Color();
    private LineRenderer lr;
    public AnnotationGroups annotationGroups; 

	// Use this for initialization
	void Start () {

        if(annotationGroups == null)
        {
            annotationGroups = GameObject.FindObjectOfType<AnnotationGroups>();
        }

        lr = gameObject.GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material.color = lineColor;
        lr.startWidth = 0.0025f;
        lr.endWidth = 0.0025f;
    }
	
	// Update is called once per frame
	void Update () {
        AnnotationGroups.AnatomyObject goTarget = null;
        switch(annotationTarget)
        {
            case target.primary:
                goTarget = annotationGroups.getPrimaryObject();
                break;
            case target.second:
                goTarget = annotationGroups.getSecondObject();
                break;
            case target.third:
                goTarget = annotationGroups.getThirdObject();
                break;
            case target.fourth:
                goTarget = annotationGroups.getFourthObject();
                break;
            default:
                return;
        }
        if(goTarget != null)
        {
            lr.enabled = true;
            var textrenderer = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            textrenderer.enabled = true;

            RectTransform canvasT = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>();
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(goTarget.Annotation.transform.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(((ViewportPosition.x * canvasT.sizeDelta.x) - (canvasT.sizeDelta.x * 0.5f)),((ViewportPosition.y * canvasT.sizeDelta.y) - (canvasT.sizeDelta.y * 0.5f)));

    
            lr.SetPosition(1, WorldObject_ScreenPosition);

            var textmesh = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            var rect = textmesh.GetComponentInChildren<RectTransform>();
            Vector3 labelPos = new Vector3();
            if (WorldObject_ScreenPosition.x + 300 > canvasT.rect.width * 0.4f) //screenpos + x, where x determines threshold switching to right side
            {
                labelPos = new Vector3(-canvasT.rect.width * 0.4f, WorldObject_ScreenPosition.y, 0);
                textmesh.alignment = TextAlignmentOptions.BottomLeft;
                rect.pivot = new Vector2(0, -1);
            }
            else
            {
                labelPos = new Vector3(canvasT.rect.width * 0.4f, WorldObject_ScreenPosition.y, 0);
                textmesh.alignment = TextAlignmentOptions.BottomRight;
                rect.pivot = new Vector2(1, -1);

            }
            

            lr.SetPosition(0, labelPos);
            
            textmesh.color = lineColor;
            textmesh.text = goTarget.DisplayName;
            textmesh.transform.localPosition = labelPos;
        }
        else
        {
            lr.enabled = false;
            var textrenderer = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            textrenderer.enabled = false;
        }
    }
}
