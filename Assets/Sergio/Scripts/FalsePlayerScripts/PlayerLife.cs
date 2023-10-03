using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerLife : MonoBehaviour
{
    private CapsuleCollider collider;

    public float health = 2000f;

    void Awake()
    {
        collider = GetComponent<CapsuleCollider>();
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
        collider.enabled = false;
        Debug.Log("Player Murio");
    }
}
