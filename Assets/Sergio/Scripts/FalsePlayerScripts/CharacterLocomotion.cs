using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLocomotion : MonoBehaviour
{
    Animator animator;
    Vector2 input;

    public bool isCrouching = false;
    


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        animator.SetFloat("InputX", input.x);
        animator.SetFloat("InputY", input.y);

        CrouchCheck();
    }

    private void CrouchCheck()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            animator.SetBool("IsCrouched", true);
            transform.Translate(0,0,input.y*Time.deltaTime*5);
            transform.Translate(input.x*Time.deltaTime*5,0,0);
            isCrouching =  true;

        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            animator.SetBool("IsCrouched", false);
            isCrouching = false;
        }
        
    }

}
