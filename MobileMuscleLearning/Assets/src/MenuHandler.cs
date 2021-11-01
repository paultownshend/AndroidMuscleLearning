using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Workaround for the menu button sometimes requires two taps
public class MenuHandler : MonoBehaviour {

    public GameObject dropdownMenu;
    public Text ButtonText;
    public bool isClosed = true;

    private Vector3 MenuPos;

    private void Start()
    {
        MenuPos = dropdownMenu.transform.localPosition;
        Close();
    }
    public void Close()
    {
        isClosed = true;
        var canvasrect = GameObject.FindObjectOfType<Canvas>().GetComponent<RectTransform>();
        dropdownMenu.transform.localPosition = new Vector3(-canvasrect.rect.width * 2, 0, 0);
        ButtonText.text = "+";
    }

    public void Open()
    {
        isClosed = false;
        dropdownMenu.transform.localPosition = MenuPos;
        ButtonText.text = "-";
    }

    public void Toggle()
    {
        if(isClosed)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
}
