using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerLife : MonoBehaviour
{
    private Animator animator;

    public float health = 2000f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log(health);
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetBool("Death", true);
        Debug.Log("Player Murio");
    }
}
