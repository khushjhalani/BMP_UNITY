using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class EnemyIA : MonoBehaviour
{
    [Header("---------Patrolling----------")]

    private NavMeshAgent navMeshAgent;
    private Animator animator;
    public Transform[] patrolPoints; //Location points that the enemy will go through
    public Transform[] patrolPointsOnAlert; //Location points that the enemy will go through on alert mode
    private bool isWaiting = false; //If it is false, the enemy can start patrolling and wait at a point
    public float waitTime = 5.0f;
    private int currentPatrolPointIndex = 0;

    [Header("-----Patrolling-On-Alert-----")]

    private bool oneTime = false;
    private bool playerIsClose = false;  // Player is near and the enemy can hear him
    private bool stayAlert = false;
    private WeaponManager playerScriptFire;
    private int currentPatrolPointIndex2 = 0; //destinations on alert mode

    [Header("---------Detecting-Player----------")]

    public Transform player;
    private float distanceToPlayer;
    public float followDistance = 9f; // Distance at which the enemy starts heading towards the player. / Follow distance must be less than shootDistance to pursue
    public bool isFollowing = false;
    private MovementStateManager playerScript; // It works if the player presses the crouch button

    [Header("---------Raycast----------")]
    public float hearingRange = 60f; //Distance at which the enemy hears a gunshot and initiates alert mode
    public float detectionRange = 30f; // Distance at which the enemy's ray notices the player
    public bool playerDetected = false;
    public float ViewAngle = 60f; // Vision angle

    private bool obstacleDetected = false;

    [Header("---------Chasing----------")]

    public float chasingDistance = 20f; //Distance the enemy runs and chases the player.

    [Header("---------Shooting----------")]

    public Transform shootingPosition; //Empty object from which the detector/trigger ray will exit
    public float shootingDistance = 10f; // Distance at which the enemy fires at the player.
    public Rig aimLayer; //  To connect the rigging tool
    public float aimSpeed = 3; //to control the speed of the weapon's movement in each animation.

    [Header("-------Shooting Damage-------")]
    
    public float firingRate = 2.0f;
    public float damage = 10f; //Damage with each contact raycast-player.
    private float timeSinceLastShot = 0f;

    //-------------------------------------------------------------------------------------------------------------
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        playerScript = player.GetComponent<MovementStateManager>();
        playerScriptFire = player.GetComponentInChildren<WeaponManager>();
    }

    private void Start()
    {
        navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
        animator.SetBool("isWalking", true);
    }

    private IEnumerator WaitAndMoveToNextPoint()
    {
        isWaiting = true;
        animator.SetBool("isWalking", false);
        aimLayer.weight = 0;
        yield return new WaitForSeconds(waitTime);
        animator.SetBool("isWalking", true);
        isWaiting = false;
        currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
        navMeshAgent.speed = 3.0f;
        navMeshAgent.SetDestination(patrolPoints[currentPatrolPointIndex].position);
    }

    private IEnumerator WaitAndMoveToNextPointOnAlert()
    {
        isWaiting = true;
        animator.SetBool("isCrouchToRun", false);
        animator.SetBool("isSuspecting", false);
        animator.SetBool("isWalking", false);
        aimLayer.weight = 0;
        yield return new WaitForSeconds(waitTime);
        animator.SetBool("isSuspecting", true);
        isWaiting = false;
        currentPatrolPointIndex2 = (currentPatrolPointIndex2 + 1) % patrolPointsOnAlert.Length;
        navMeshAgent.speed = 3.5f;
        navMeshAgent.SetDestination(patrolPointsOnAlert[currentPatrolPointIndex2].position);
    }

    private IEnumerator FirstAlertMode()
    {
        aimLayer.weight = 0.5f;
        navMeshAgent.speed = 0f;
        animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(waitTime);
        animator.SetBool("isSuspecting", true);
        navMeshAgent.speed = 3.5f;
    }

    private void FixedUpdate()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // Calculate the vector from player to enemy in each frame.
        RaycastAtAllTime(); // Recognize the object that collides with the raycast in each frame.

        //   ---- Player Distance Range (behave)  ----

        bool isVeryFar = false;
        bool isfar = false;
        bool detectionZone = false;
        bool chasingZone = false;
        bool firingZone = false;

        if (distanceToPlayer <= hearingRange) isVeryFar = true;
        if (distanceToPlayer >= detectionRange) isfar = true; // The player is beyond detection
        if (distanceToPlayer <= detectionRange && distanceToPlayer >= chasingDistance) detectionZone = true; // The enemy suspects the player's presence
        if (distanceToPlayer <= chasingDistance && distanceToPlayer >= shootingDistance) chasingZone = true; // The enemy follows the player to shoot him
        if (distanceToPlayer <= shootingDistance) firingZone = true; // The enemy shoots the player

        if ((isVeryFar == true && playerScriptFire.playerShooting == true) || stayAlert == true)
        {
            if (!oneTime)
            {
                StartCoroutine(FirstAlertMode());
                oneTime = true;
            }  

            if (isfar == true)
            {
                if ((playerScript.currentState == playerScript.Crouch && (!playerDetected || playerDetected)) || (playerScript.currentState != playerScript.Crouch && (!playerDetected || playerDetected)))
                {
                    playerIsClose = true;
                    PatrolingOnAlert();
                    isFollowing = false;
                }
            }
            if (detectionZone == true)
            {
                if (playerScript.currentState == playerScript.Crouch && !playerDetected)
                {
                    PatrolingOnAlert();
                }
                if ((playerDetected && (playerScript.currentState == playerScript.Crouch || playerScript.currentState != playerScript.Crouch)) || !playerDetected && playerScript.currentState != playerScript.Crouch)
                {
                    playerIsClose = true;
                    DetectingThePlayer();
                    isFollowing = true;
                }
            }
            if (chasingZone == true)
            {
                if (playerScript.currentState == playerScript.Crouch && !playerDetected)
                {
                    PatrolingOnAlert();
                }
                else if ((playerDetected && (playerScript.currentState == playerScript.Crouch || playerScript.currentState != playerScript.Crouch)) || !playerDetected && playerScript.currentState != playerScript.Crouch)
                {
                    playerIsClose = true;
                    ChasingThePlayer();
                    isFollowing = true;
                }
            }
            if (firingZone == true)
            {
                if ((playerScript.currentState == playerScript.Crouch && !playerDetected) || (!playerDetected && playerScript.currentState != playerScript.Crouch && obstacleDetected == true))
                {
                        PatrolingOnAlert();
                        isFollowing = false;
                }
                else if ((playerDetected && (playerScript.currentState == playerScript.Crouch || playerScript.currentState != playerScript.Crouch)) || (!playerDetected && playerScript.currentState != playerScript.Crouch && obstacleDetected == false))
                {
                    playerIsClose = true;
                    ShootingThePlayer();
                }
            }
        }
        else
        {
            if (playerIsClose == true)
            {
                stayAlert = true;
            }
                if (isfar == true)
                {
                    if ((playerScript.currentState == playerScript.Crouch && (!playerDetected || playerDetected)) || (playerScript.currentState != playerScript.Crouch && (!playerDetected || playerDetected)))
                    {
                        Patroling();
                        isFollowing = false;
                    }
                }
                if (detectionZone == true)
                {
                    if (playerScript.currentState == playerScript.Crouch && !playerDetected)
                    {
                        Patroling();
                    }
                    if ((playerDetected && (playerScript.currentState == playerScript.Crouch || playerScript.currentState != playerScript.Crouch)) || !playerDetected && playerScript.currentState != playerScript.Crouch)
                    {
                        playerIsClose = true;
                        DetectingThePlayer();
                        isFollowing = true;
                    }
                }
                if (chasingZone == true)
                {
                    if (playerScript.currentState == playerScript.Crouch && !playerDetected)
                    {
                        Patroling();
                    }
                    else if ((playerDetected && (playerScript.currentState == playerScript.Crouch || playerScript.currentState != playerScript.Crouch)) || !playerDetected && playerScript.currentState != playerScript.Crouch)
                    {
                        playerIsClose = true;
                        ChasingThePlayer();
                        isFollowing = true;
                    }
                }
                if (firingZone == true)
                {
                    if (playerScript.currentState == playerScript.Crouch && !playerDetected)
                    {
                        Patroling();
                    }
                    else if ((playerDetected && (playerScript.currentState == playerScript.Crouch || playerScript.currentState != playerScript.Crouch)) || !playerDetected && playerScript.currentState != playerScript.Crouch)
                    {
                        playerIsClose = true;
                        ShootingThePlayer();
                    }
            }
            playerScriptFire.playerShooting = false;
        }

        // ----  Control structure to follow player -----

        if (distanceToPlayer >= followDistance && distanceToPlayer <= detectionRange && isFollowing == true)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            Vector3 targetPosition = player.position - directionToPlayer.normalized * followDistance;
            navMeshAgent.SetDestination(targetPosition);  
        }

        // ----  Control the firing rate -----

        timeSinceLastShot += Time.deltaTime;
        if (timeSinceLastShot >= firingRate && aimLayer.weight == 1.0f)
        {
            timeSinceLastShot = 0f;
            RaycastThePlayer();
        }
    }

    private void RaycastAtAllTime()
    {
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);

            if (angleToPlayer <= ViewAngle * 0.5f)
            {
                if (Physics.Raycast(shootingPosition.position, directionToPlayer, out RaycastHit hit, detectionRange))
                {
                    Debug.DrawRay(shootingPosition.position, directionToPlayer * detectionRange, Color.green);
                    if (hit.collider.CompareTag("Player"))
                    {
                        playerDetected = true;
                        //playerVisible = true;
                    }
                    if (hit.collider.CompareTag("Obstacle"))
                    {
                        obstacleDetected = true;
                        //playerVisible = true;
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
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(directionToPlayer, transform.forward);
            if (angleToPlayer <= ViewAngle * 0.5f)
            {
                if (Physics.Raycast(shootingPosition.position, directionToPlayer, out RaycastHit hit, detectionRange))
                {
                    Debug.DrawRay(shootingPosition.position, directionToPlayer * detectionRange, Color.green);
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

    

    private void Patroling()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAndMoveToNextPoint());
        }
    }

    private void PatrolingOnAlert()
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAndMoveToNextPointOnAlert());
        }
    }

    private void DetectingThePlayer()
    {
        animator.SetBool("isSuspecting", true);
        animator.SetBool("isCrouchToRun", false);
        navMeshAgent.speed = 4f;
        aimLayer.weight -= Time.deltaTime * aimSpeed;
    }

    private void ChasingThePlayer()
    {
        animator.SetBool("isSuspecting", true);
        animator.SetBool("isCrouchToRun", true);
        animator.SetBool("isRuntoShoot", false);
        navMeshAgent.speed = 7.0f;

        if (aimLayer.weight > 0.5)
        {
            aimLayer.weight -= Time.deltaTime * aimSpeed;
        }
        else if (aimLayer.weight < 0.5)
        {
            aimLayer.weight += Time.deltaTime * aimSpeed;
        }
    }

    private void ShootingThePlayer()
    {
        animator.SetBool("isShooting", true);
        aimLayer.weight += Time.deltaTime * aimSpeed;
        navMeshAgent.SetDestination(transform.position);
        Vector3 directionToPlayer2 = player.position - transform.position;
        directionToPlayer2.y = 0f;
        Quaternion rotation = Quaternion.LookRotation(directionToPlayer2);
        transform.rotation = rotation;
        animator.SetBool("isRuntoShoot", true);
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
        Gizmos.DrawRay(shootingPosition.position, leftRayDirection * detectionRange);
        Gizmos.DrawRay(shootingPosition.position, rightRayDirection * detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(shootingPosition.position, directionToPlayer * detectionRange);
    }*/
}