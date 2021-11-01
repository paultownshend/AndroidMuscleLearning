using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnMoveCloseMenu : MonoBehaviour {

    public Button buttonclose;
    private Vector3 camPos;

	// Update is called once per frame
	void Update () {

        if(buttonclose != null && camPos != Camera.main.transform.position)
        {
            camPos = Camera.main.transform.position;
            if (buttonclose.IsActive())
            {
                buttonclose.onClick.Invoke();
                
            }
        }
        
	}
}
