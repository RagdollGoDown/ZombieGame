using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DissolveAnimationForImage : DissolveAnimation
{
    private void Awake()
    {
        StartAnimation(GetComponent<Image>().material);
    }
}
