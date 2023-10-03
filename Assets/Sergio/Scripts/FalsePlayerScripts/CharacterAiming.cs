using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CharacterAiming : MonoBehaviour
{
    public float turnSpeed = 15;
    public float aimdDuration = 0.3f;

    public bool canShoot = false;

    Camera mainCamera;
    public Rig aimLayer;

    public bool PlayerShoot = false;

    void Start()
    {
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }
    void FixedUpdate()
    {
        float yawCamera = mainCamera.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0,yawCamera,0),turnSpeed*Time.deltaTime);
    }

    private void Update() {
        if (Input.GetMouseButton(0))
        {
            PlayerShoot = true;

            aimLayer.weight += Time.deltaTime / aimdDuration;
            if (aimLayer.weight == 1){
                canShoot = true;
            }
            else{
                canShoot = false;
            }
        }
        else
        {
             aimLayer.weight -= Time.deltaTime / aimdDuration;
        }
    }
}
