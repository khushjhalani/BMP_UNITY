using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLifeTower : MonoBehaviour
{
    private Animator animator;
    private CapsuleCollider collider;

    public float health = 200f;

    public Bullets magazineSpawner;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Debug.Log(health);
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Muerto");
        //collider.enabled = false;
        animator.CrossFadeInFixedTime("DeathTower", 0.5f);

        magazineSpawner.SpawnMagazine(transform.position);

        Destroy(gameObject, 2.5f);
    }
}
