using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FalsePlayerCamera : MonoBehaviour
{
    
    public float mouseSensivity = 80f;
    
    public Transform playerBody;

    float xRotation = 0;

    void Start()
    {
        // hide mouse arrow
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;
    
        //Horizontal camera rotation
        playerBody.Rotate(Vector3.up*mouseX);


        xRotation -= mouseY;

        //variable to constrain vertical rotation
        xRotation = Mathf.Clamp(xRotation,-90f,90f);

        //Vertical camera rotation
        transform.localRotation = Quaternion.Euler(xRotation,0,0);

    }
}
