using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public delegate void TouchDetectedDelegate(Vector2 touchPosition);
    public static event TouchDetectedDelegate OnTouchDetected;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (OnTouchDetected != null)
                {
                    OnTouchDetected(touch.position);
                }
            }
        }
    }
}
