using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CallbackEvents;

[System.Serializable]
public class HurtMesh
{
    public MeshRenderer mesh;
    public Material hurtMaterial;
    public Material material;
}

public class ShootAI : MonoBehaviour
{
    //components
    public NavMeshAgent navMeshAgent;
    public Animator animator;

    //navigation settings
    public Transform target;
    public float viewDistance; //view distance for spotting player
    public float agroViewDistance; //view distance after agro'd
    public float viewConeAngle; //view cone angle for spotting player
    public float agroViewConeAngle; //view cone angle when player is spoitted
    public int lookForLostPlayerMs; //the amount of time the bot should look for a lost player
    public float timeBetweenSeePlayerChecks; //increaments to check on whether or not we can see the player
    public float rotationDamping; //controls ai rotation speed

    //attack settings
    public float minStrafeDistance;
    public float maxStrafeDistance;
    public float maxProjectileDistance;
    public float projectileDamage;
    public float projectileSpeed;
    public bool invisible; //if the player is invisible this frame
    public float attackChargeSpeed = 6.0f;
    public GameObject projectilePrefab;

    //damage settings
    public HurtMesh[] hurtMesh;
    // public MeshRenderer[] hurtMesh; //meshs to apply the hurt material to on damaged
    public Material hurtMaterial; //material to apply on damaged
    public Material normalMaterial; //material to restore normal colors

    //health settings
    public float maxHealth;

    //audio sources for each sound effect
    public AudioSource attackPrep1Sound;
    public AudioSource attackSound;

    //Navagiation Properties
    private float lastSawPlayerTime;
    private Vector3 lastSeenPlayerPos;
    private float lastPlayerSeenCheck;

    //health settings
    private float health;

    //the speed the ai is suppose to move at (pulled from the navagent comp)
    private float normalSpeed;

    //state flags
    private bool isAgro;
    private bool isStrafing;
    private bool movingCloser;
    private bool movingAway;
    private bool hasLastPlayerPos;
    private bool didSeePlayer;
    private bool canSeePlayer;
    private bool isAttacking;
    private bool isAttackCoolingDown;

    /*
    ....###....####....####.##....##.########.########.##....##.########..######.
    ...##.##....##......##..###...##....##....##.......###...##....##....##....##
    ..##...##...##......##..####..##....##....##.......####..##....##....##......
    .##.....##..##......##..##.##.##....##....######...##.##.##....##.....######.
    .#########..##......##..##..####....##....##.......##..####....##..........##
    .##.....##..##......##..##...###....##....##.......##...###....##....##....##
    .##.....##.####....####.##....##....##....########.##....##....##.....######.
    */

    // void AttackPlayer()
    // {
    //     lastSeenPlayerPos = target.position;
    //     lastSawPlayerTime = Time.time;
    //     hasLastPlayerPos = true;
    //     navMeshAgent.SetDestination(target.position);

    //     Vector3 lookPos = target.position - transform.position;
    //     lookPos.y = 0;
    //     Quaternion rotation = Quaternion.LookRotation(lookPos);
    //     transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
    //     // transform.LookAt(target.position, Vector3.up);
    // }

    void Strafe()
    {
        navMeshAgent.SetDestination(transform.position);

        if (!isAttacking && !isAttackCoolingDown)
        {
            isAttacking = true;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance < minStrafeDistance)
        {
            movingCloser = false;
            movingAway = true;
            isStrafing = false;
        }
        else if (distance > maxStrafeDistance)
        {
            movingCloser = true;
            movingAway = false;
            isStrafing = false;
        }
        else if (Mathf.Abs(maxStrafeDistance - distance) < 0.3)
        {
            movingCloser = false;
            movingAway = false;
            isStrafing = true;
        }
        else if (distance > minStrafeDistance && distance < maxStrafeDistance)
        {
            float ran = Random.Range(0, 1);
            if (ran < 0.6)
            {
                movingCloser = false;
                movingAway = false;
                isStrafing = true;
            }
        }

        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
    }

    void FollowPlayer()
    {
        lastSeenPlayerPos = target.position;
        lastSawPlayerTime = Time.time;
        hasLastPlayerPos = true;
        navMeshAgent.SetDestination(target.position);

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance < minStrafeDistance)
        {
            movingCloser = false;
            movingAway = true;
            isStrafing = false;
        }
        else if (distance > maxStrafeDistance)
        {
            movingCloser = true;
            movingAway = false;
            isStrafing = false;
        }
        else if (Mathf.Abs(maxStrafeDistance - distance) < 0.3)
        {
            movingCloser = false;
            movingAway = false;
            isStrafing = true;
        }
        else if (distance > minStrafeDistance && distance < maxStrafeDistance)
        {
            float ran = Random.Range(0, 1);
            if (ran < 0.6)
            {
                movingCloser = false;
                movingAway = false;
                isStrafing = true;
            }
        }

        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);
    }

    void MoveAwayFromPlayer()
    {
        if (!isAttacking && !isAttackCoolingDown)
        {
            isAttacking = true;
        }

        lastSeenPlayerPos = target.position;
        lastSawPlayerTime = Time.time;
        hasLastPlayerPos = true;
        Vector3 hello = transform.position - transform.forward;
        navMeshAgent.SetDestination(hello);

        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationDamping);

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance < minStrafeDistance)
        {
            movingCloser = false;
            movingAway = true;
            isStrafing = false;
        }
        else if (distance > maxStrafeDistance)
        {
            movingCloser = true;
            movingAway = false;
            isStrafing = false;
        }
        else if (Mathf.Abs(maxStrafeDistance - distance) < 0.3)
        {
            movingCloser = false;
            movingAway = false;
            isStrafing = true;
        }
        else if (distance > minStrafeDistance && distance < maxStrafeDistance)
        {
            float ran = Random.Range(0, 1);
            if (ran < 0.6)
            {
                movingCloser = false;
                movingAway = false;
                isStrafing = true;
            }
        }
    }

    void LookForPlayer()
    {
        if (hasLastPlayerPos)
        {
            navMeshAgent.SetDestination(lastSeenPlayerPos);
        }
        else
        {
            navMeshAgent.SetDestination(transform.position);
        }

        if (Time.time - lastSawPlayerTime >= lookForLostPlayerMs)
        {
            isAgro = false;
            hasLastPlayerPos = false;
        }
    }

    void Wander()
    {
        navMeshAgent.SetDestination(transform.position);
        //TODO: wander around
        hasLastPlayerPos = false;
        isAgro = canSeePlayer;
    }

    /*
    .##.....##.########..######...######.....###.....######...########..######.
    .###...###.##.......##....##.##....##...##.##...##....##..##.......##....##
    .####.####.##.......##.......##........##...##..##........##.......##......
    .##.###.##.######....######...######..##.....##.##...####.######....######.
    .##.....##.##.............##.......##.#########.##....##..##.............##
    .##.....##.##.......##....##.##....##.##.....##.##....##..##.......##....##
    .##.....##.########..######...######..##.....##..######...########..######.
    */

    void Start()
    {
        //get nav mesh agent
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        //get player target
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform;

        //get the animator
        if (animator == null)
            animator = GetComponent<Animator>();

        //intial properties
        lastSeenPlayerPos = new Vector3();
        normalSpeed = navMeshAgent.speed;
        health = maxHealth;

        //intial
        isAgro = false;
        hasLastPlayerPos = false;
        canSeePlayer = false;
        didSeePlayer = false;
        isAttacking = false;
        isAttackCoolingDown = false;
        navMeshAgent.updateRotation = false;
        invisible = false;

        //register event listener
        EventSystem.Current.RegisterEventListener<BulletHitCtx>(OnBulletHit);
    }

    void AgroUpdate()
    {
        if (!isStrafing && !movingCloser && !movingAway)
        {
            movingCloser = true;
        }

        if (canSeePlayer)
        {
            if (isStrafing)
            {
                Strafe();
            }
            else if (movingCloser)
            {
                FollowPlayer();
            }
            else if (movingAway)
            {
                MoveAwayFromPlayer();
            }
        }
        else
        {
            LookForPlayer();
        }
    }

    void IdleUpdate()
    {
        Wander();
    }

    void Update()
    {
        //set animation states
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isAgro", isAgro);

        if (isAgro)
        {
            AgroUpdate();
        }
        else
        {
            IdleUpdate();
        }
    }

    void FixedUpdate()
    {
        if (Time.time - lastPlayerSeenCheck > timeBetweenSeePlayerChecks)
        {
            didSeePlayer = canSeePlayer;
            canSeePlayer = DoesAISeePlayer();
        }
    }

    /*
    .########.##.....##.########.##....##.########..######.
    .##.......##.....##.##.......###...##....##....##....##
    .##.......##.....##.##.......####..##....##....##......
    .######...##.....##.######...##.##.##....##.....######.
    .##........##...##..##.......##..####....##..........##
    .##.........##.##...##.......##...###....##....##....##
    .########....###....########.##....##....##.....######.
    */

    public void OnAttackCooldownFinished()
    {
        isAttackCoolingDown = false;
    }

    public void OnDamgeFinshed()
    {
        invisible = false;
        foreach (HurtMesh obj in hurtMesh)
        {
            MeshRenderer mesh = obj.mesh;
            mesh.material = obj.material;
        }

        if (health <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void OnPrepAttack()
    {
        attackPrep1Sound.Play();
        //TODO: Player sound
    }

    public void OnAttack()
    {
        attackSound.Play();
        //TODO: Shoot your goo my dood

        GameObject ob = Instantiate(projectilePrefab);
        ob.transform.position = transform.position + transform.forward + (transform.up * 1.35f);
        Projectile projectile = ob.GetComponent<Projectile>();
        projectile.velocity = transform.forward * projectileSpeed;
        projectile.damage = projectileDamage;
        isAttackCoolingDown = true;
        isAttacking = false;
    }

    public void OnAttackFinished()
    {
        isAttacking = false;
    }

    public void OnBulletHit(BulletHitCtx ctx)
    {
        if (gameObject.Equals(ctx.hit.collider.gameObject))
        {
            TakeDamage(ctx.damage);
        }
    }

    /*
    .##.....##.########.##.......########..########.########...######.
    .##.....##.##.......##.......##.....##.##.......##.....##.##....##
    .##.....##.##.......##.......##.....##.##.......##.....##.##......
    .#########.######...##.......########..######...########...######.
    .##.....##.##.......##.......##........##.......##...##.........##
    .##.....##.##.......##.......##........##.......##....##..##....##
    .##.....##.########.########.##........########.##.....##..######.
    */

    private bool DoesAISeePlayer()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        RaycastHit hit;

        float angle = (isAgro) ? agroViewConeAngle : viewConeAngle;
        float distance = (isAgro) ? agroViewDistance : viewDistance;

        lastPlayerSeenCheck = Time.time;

        return Mathf.Abs(Vector3.Angle(transform.position, target.position)) <= (angle / 2) &&
            Vector3.Distance(transform.position, target.position) <= distance &&
            Physics.Raycast(transform.position, direction, out hit) &&
            hit.collider.gameObject.tag == "Player" && !hit.collider.gameObject.GetComponent<Player>().Invisible;
    }

    private void TakeDamage(float damage)
    {
        if (!invisible)
        {
            invisible = false;
            health -= damage;
            foreach (HurtMesh obj in hurtMesh)
            {
                MeshRenderer mesh = obj.mesh;
                mesh.material = obj.hurtMaterial;
            }
        }

        EventSystem.Current.CallbackAfter(OnDamgeFinshed, 400);
    }
}
