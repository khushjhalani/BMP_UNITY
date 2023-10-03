using System.Collections;
using UnityEngine.AI;
using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private CapsuleCollider collider;

    public float health = 50f;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider>();
    }

    public void TakeDamage (float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die ()
    {
        navMeshAgent.isStopped = true;
        collider.enabled = false;
        animator.CrossFadeInFixedTime("Death", 0.5f);
        Destroy(gameObject, 2.5f);
    }
}
