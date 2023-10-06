using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class EnemyTowerAI : MonoBehaviour
{
    [Header("---------Patrolling----------")]


    private Animator animator;

    public float waitTime = 5.0f;


    [Header("-----Patrolling-On-Alert-----")]

    /*private bool oneTime = false;
    private bool playerIsClose = false;  // Player is near and the enemy can hear him
    private bool stayAlert = false;
    private CharacterAiming playerScriptFire;*/


    [Header("---------Detecting-Player----------")]

    public Transform player;
    private float distanceToPlayer;

    private MovementStateManager playerScript; // It works if the player presses the crouch button

    [Header("---------Raycast----------")]
    public float veryFarZone = 60f; //Distance at which the enemy hears a gunshot and initiates alert mode
    public float farZone = 30f; // Distance at which the enemy's ray notices the player
    public bool playerDetected = false;
    public float ViewAngle = 60f; // Vision angle

    private bool obstacleDetected = false;

    [Header("---------Chasing----------")]

    public float mediumZone = 20f; //Distance the enemy runs and chases the player.
    public float closeZone = 20f;

    [Header("---------Shooting----------")]

    public Transform shootingPosition; //Empty object from which the detector/trigger ray will exit
    public Rig aimLayer; //  To connect the rigging tool
    public float aimSpeed = 3; //to control the speed of the weapon's movement in each animation.

    [Header("-------Shooting Damage-------")]

    public float firingRate = 2.0f;
    public float hardFiringRate = 2.0f;
    public float damage = 10f; //Damage with each contact raycast-player.
    private float timeSinceLastShot = 0f;


    // NEW TO ADD TO PROJECT
    public LayerMask ignoreLayer;

    //public AudioSource audioSource;

    //-------------------------------------------------------------------------------------------------------------
    private void Awake()
    {

        animator = GetComponent<Animator>();
        playerScript = player.GetComponent<MovementStateManager>();
        //playerScriptFire = player.GetComponent<CharacterAiming>();
    }


    private IEnumerator Watchingforintruders()
    {
        yield return new WaitForSeconds(waitTime);
        aimLayer.weight -= Time.deltaTime * aimSpeed;
        animator.SetBool("isLooking", true);
        yield return new WaitForSeconds(waitTime);
        animator.SetBool("isLooking", false);
        aimLayer.weight += Time.deltaTime * aimSpeed;
    }

    private IEnumerator MissedShot()
    {
        transform.LookAt(player);   ///MIRAR ESTOOO
        animator.SetBool("isShooting", true);
        //audioSource.Play();
        aimLayer.weight += Time.deltaTime * (aimSpeed*2);
        yield return new WaitForSeconds(waitTime);
        animator.SetBool("isShooting", true);
        aimLayer.weight -= Time.deltaTime * aimSpeed;

    }

    private IEnumerator Shooting()
    {
        animator.SetBool("isShooting", true);
        aimLayer.weight += Time.deltaTime * (aimSpeed * 2);
        //audioSource.Play();
        yield return new WaitForSeconds(waitTime);
        aimLayer.weight -= Time.deltaTime * (aimSpeed * 2);
        animator.SetBool("isShooting", false);
    

    }

    private IEnumerator ShootingHard()
    {
        animator.SetBool("isShooting", true);
        aimLayer.weight += Time.deltaTime * (aimSpeed * 2);
        //audioSource.Play();
        yield return new WaitForSeconds(waitTime/2);
        aimLayer.weight -= Time.deltaTime * (aimSpeed * 2);
        animator.SetBool("isShooting", false);
    }

    private void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Calculate the vector from player to enemy in each frame.
        RaycastAtAllTime(); // Recognize the object that collides with the raycast in each frame.

        //   ---- Player Distance Range (behave)  ----

        bool anyWhere = false;
        bool far = false;
        bool medium = false;
        bool close = false;
        bool veryClose = false;

        if (distanceToPlayer >= veryFarZone) anyWhere = true;
        //if (distanceToPlayer <= hearingRange) VeryFar = true;
        if (distanceToPlayer >= farZone && distanceToPlayer <= veryFarZone) far = true; // The player is beyond detection - difficult to hit the player
        if (distanceToPlayer <= farZone && distanceToPlayer >= mediumZone) medium = true; // The player is hit by 1 bullet
        if (distanceToPlayer <= mediumZone && distanceToPlayer >= closeZone) close = true; // The player is hit by 2 bullet
        if (distanceToPlayer <= closeZone) veryClose = true; // The enemy cannot shoot the player because he is under the tower.

        if (anyWhere == true)
        {
            Watching();
            Debug.Log("Rutina normal Anywhere");
        }

        if (far == true)
        {
            if ((playerScript.currentState == playerScript.Crouch && (!playerDetected || playerDetected)) || (playerScript.currentState != playerScript.Crouch && playerDetected == false))
            {
                Watching();
                Debug.Log("Rutina normal far");
            }
            else if (playerScript.currentState != playerScript.Crouch && playerDetected == true)
            {
                Debug.Log("disparando erradamente far");
                ApparentlyEnemySawThePlayer();
            }
        }
        if (medium == true)
        {
            if (playerScript.currentState == playerScript.Crouch && !playerDetected)
            {
                Watching();
                Debug.Log("Rutina normal medium");
            }
            if ((playerScript.currentState == playerScript.Crouch && playerDetected) || (!playerDetected && playerScript.currentState != playerScript.Crouch))
            {
                ApparentlyEnemySawThePlayer();
                Debug.Log("disparando erradamente medium");
            }
            if (playerDetected && playerScript.currentState != playerScript.Crouch)
            {
                Debug.Log("disparando 1 en medium");
                timeSinceLastShot += Time.deltaTime;
                if (timeSinceLastShot >= firingRate)
                {
                    timeSinceLastShot = 0f;
                    RaycastThePlayer();
                    ShootingAtThePlayer();
                }
            }
        }
        if (close == true)
        {
            if (playerScript.currentState == playerScript.Crouch && !playerDetected)
            {
                Watching();
                Debug.Log("Rutina normal close");
            }
            else if (!playerDetected && playerScript.currentState != playerScript.Crouch)
            {
                ApparentlyEnemySawThePlayer();
                Debug.Log("disparando erradamente close");
            }
            else if (playerDetected && playerScript.currentState == playerScript.Crouch)
            {
                Debug.Log("disparando 1 en close");
                timeSinceLastShot += Time.deltaTime;
                if (timeSinceLastShot >= firingRate)
                {
                    timeSinceLastShot = 0f;
                    RaycastThePlayer();
                    ShootingAtThePlayer();
                }
            }
            else if (playerDetected && playerScript.currentState != playerScript.Crouch)
            {
                Debug.Log("disparando de a 2 o 3 en close");
                timeSinceLastShot += Time.deltaTime;
                if (timeSinceLastShot >= hardFiringRate)
                {
                    timeSinceLastShot = 0f;
                    RaycastThePlayer();
                    FiringHardAtThePlayer();
                }
            }
        }
        if (veryClose == true)
        {
            if (playerScript.currentState == playerScript.Crouch && !playerDetected)
            {
                Watching();
                Debug.Log("Rutina normal debajo de la torre");
            }
            else if ((playerDetected && (playerScript.currentState == playerScript.Crouch || playerScript.currentState != playerScript.Crouch)) || (!playerDetected && playerScript.currentState != playerScript.Crouch))
            {
                ApparentlyEnemySawThePlayer();
                Debug.Log("disparando erradamente debajo de la torre");
            }
        }
    }

    private void RaycastAtAllTime()
    {
        if (distanceToPlayer <= veryFarZone)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

            if (angleToPlayer <= ViewAngle * 0.5f)
            {
                int layerMask = ~ignoreLayer;
                if (Physics.Raycast(shootingPosition.position, directionToPlayer, out RaycastHit hit, veryFarZone, layerMask))
                {
                    Debug.DrawRay(shootingPosition.position, directionToPlayer * veryFarZone, Color.green);
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerDetected = true;
                        Debug.Log("Detectado");
                        transform.LookAt(player);
                    }
                    else
                    {
                        playerDetected = false;
                        obstacleDetected = false;
                    }
                }
            }
        }
    }

    private void RaycastThePlayer()
    {
        if (distanceToPlayer <= veryFarZone)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);
            if (angleToPlayer <= ViewAngle * 0.5f)
            {
                if (Physics.Raycast(shootingPosition.position, directionToPlayer, out RaycastHit hit, veryFarZone))
                {
                    Debug.DrawRay(shootingPosition.position, directionToPlayer * veryFarZone, Color.green);
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerDetected = true;
                        Debug.Log("Daño al Jugador");
                        PlayerLife target = hit.transform.GetComponent<PlayerLife>();
                        if (target != null)
                        {
                            target.TakeDamage(damage);
                        }
                    }
                    else
                    {
                        playerDetected = false;
                    }
                }
            }
        }
    }



    private void Watching()
    {
        StartCoroutine(Watchingforintruders());
    }

    private void ApparentlyEnemySawThePlayer()
    {
        StartCoroutine(MissedShot());
    }

    private void ShootingAtThePlayer()
    {
        StartCoroutine(Shooting());
    }

    private void FiringHardAtThePlayer()
    {
        StartCoroutine(ShootingHard());
    }

    //   ----- Draw the distance and range of the agent's vision, also where the raycast is pointing  -----

    /*private void OnDrawGizmos()
    {
        if (shootingPosition == null)
        {
            return;
        }
        Vector3 directionToPlayer = (player.position - shootingPosition.position).normalized;
        float halfFieldOfView = ViewAngle * 0.5f;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFieldOfView, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFieldOfView, Vector3.up);
        Vector3 leftRayDirection = leftRayRotation * directionToPlayer;
        Vector3 rightRayDirection = rightRayRotation * directionToPlayer;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(shootingPosition.position, leftRayDirection * veryFarZone);
        Gizmos.DrawRay(shootingPosition.position, rightRayDirection * veryFarZone);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(shootingPosition.position, directionToPlayer * veryFarZone);
    }*/
}
