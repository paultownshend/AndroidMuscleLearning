using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateWithSlider : MonoBehaviour {


    private Slider slider;

    private void Start()
    {
        slider = GameObject.FindObjectOfType<Slider>();
    }

    private void Update()
    {
        gameObject.transform.rotation = Quaternion.Euler(0, slider.value, 0);
    }

}
