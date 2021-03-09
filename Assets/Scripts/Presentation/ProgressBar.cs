using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage = null;

    [SerializeField, Range(0, 1)] private float initialValue = 1f; 

    public float Fill
    {
        get => fillImage.fillAmount;
        set => fillImage.fillAmount = Mathf.Clamp01(value);
    }

    private void Awake()
    {
        this.Fill = initialValue;
    }
}
