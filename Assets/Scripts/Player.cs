using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CallbackEvents;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealthChangedCtx : EventContext
{
    public float dHealth;
    public float currentHealth;
    public Player player;

    public PlayerHealthChangedCtx(float dHealth, float currentHealth, Player player)
    {
        this.dHealth = dHealth;
        this.currentHealth = currentHealth;
        this.player = player;
    }
}

public class BulletHitCtx : EventContext
{
    public RaycastHit hit;
    public float damage;

    public BulletHitCtx(RaycastHit hit, float damage)
    {
        this.hit = hit;
        this.damage = damage;
    }
}

public class GotoNextLevelContext : EventContext
{
    public Vector3 location;
    
    public GotoNextLevelContext(Vector3 location)
    {
        this.location = location;
    }
}

public class GotoPreviousLevelContext : EventContext
{}

public class Player : MonoBehaviour
{
    // public variables
    public LayerMask InteractionLayerMask;
    public GunData StartingGun;
    public Gun[] HeldGuns;
    public int CurrentGun = 0;
    public int InteractionDistance = 2;
    public float maxHealth = 100;
    public Image fadeOverlay;
    public float fadeDamp;
    public GameObject ScoreHolder;
    public bool GodMode = false;
    public bool Invisible = false;

    // private variables
    private Camera playerCamera;
    private float health;
    private int documents; // DOCUMENT COUNT INITILIZED HERE ERIC <----
    private Stack<Vector3> levelPositionStack;
    private Stack<Quaternion> levelRotationStack;
    private bool jamacIsDead;
    public bool IsDead {
        get {
            return jamacIsDead;
        }
    } 
    private float opacity = 0.0f;

    // automatic properties
    public CharacterController CharacterController { get; set; }
    public CPMPlayer CPMPlayer { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        EventSystem.Current.RegisterEventListener<GroundPoundContext>(OnGroundPound);
        EventSystem.Current.RegisterEventListener<GotoNextLevelContext>(OnGotoNextLevel);
        EventSystem.Current.RegisterEventListener<GotoPreviousLevelContext>(OnGotoPreviousLevel);
        EventSystem.Current.RegisterEventListener<PickupWeaponContext>(OnPickupWeapon);
        EventSystem.Current.RegisterEventListener<PickupItemContext>(OnPickupItem);
        EventSystem.Current.RegisterEventListener<ProjectileHitContext>(OnProjectileHit);
        playerCamera = Camera.main;
        health = maxHealth;
        jamacIsDead = false;
        CharacterController = GetComponent<CharacterController>();
        levelPositionStack = new Stack<Vector3>();
        levelRotationStack = new Stack<Quaternion>();

        foreach(Gun gun in HeldGuns)
        {
            gun.Reset();
        }

        // Set starting guns
        HeldGuns[0].SetData(StartingGun);
        SwitchWeapons(HeldGuns[0], HeldGuns[0]);
    }

    // Update is called once per frame
    void Update()
    {
        if (jamacIsDead) {
            //! thank fuickikinbg gOD
            opacity = Mathf.Lerp(opacity, 1.0f, fadeDamp * Time.deltaTime);
            fadeOverlay.color = new Color(fadeOverlay.color.r, fadeOverlay.color.g, fadeOverlay.color.b, opacity);

            if (opacity > 0.99) {
                SceneManager.LoadScene(2);
            }
            return;
        }

        RaycastHit hit;
        Gun heldGun = HeldGuns[CurrentGun];

        // shot
        if(!heldGun.Empty && ((Input.GetButtonDown("Fire1") && !heldGun.GunData.Automatic) || (Input.GetButton("Fire1") && heldGun.GunData.Automatic)))
        {
            heldGun.Fire();
        }

        // check for interactables
        bool success = Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, InteractionDistance, InteractionLayerMask);
        if(success)
        {
            IInteractable interactable = hit.collider.gameObject.GetComponent<IInteractable>();
            interactable.OnInteractable();
            if(Input.GetButtonDown("Interact"))
            {
                interactable.Interact();
            }
        }else{
            EventSystem.Current.FireEvent(new HideInteractionDialogueContext());
        }

        // switching weapons
        if(Input.GetButtonDown("Swap Weapon"))
        {
            if(CurrentGun != 0)
            {
                CurrentGun = 0;
            }
            else
            {
                CurrentGun = 1;
            }

            SwitchWeapons(heldGun, HeldGuns[CurrentGun]);
        }
        else if(Input.GetButtonDown("Select Weapon1"))
        {
            if(CurrentGun != 0)
            {
                SwitchWeapons(heldGun, HeldGuns[0]);
            }

            CurrentGun = 0;
        }
        else if(Input.GetButtonDown("Select Weapon2"))
        {
            if(CurrentGun != 1)
            {
                SwitchWeapons(heldGun, HeldGuns[1]);
            }

            CurrentGun = 1;
        }

        EventSystem.Current.FireEvent(new UpdateHUDContext((180 - transform.rotation.eulerAngles.y) * -1));
    }

    private void SwitchWeapons(Gun prev, Gun next)
    {
        if(!prev.Empty)
        {
            GunManager.Current.GetModel(prev.GunData).SetActive(false);
        }
        if(!next.Empty)
        {
            GunManager.Current.GetModel(next.GunData).SetActive(true);
        }
    }

    void OnGroundPound(GroundPoundContext ctx)
    {
        //distance not including y
        Vector3 from = new Vector3(ctx.Location.x, 0, ctx.Location.z);
        Vector3 current = new Vector3(transform.position.x, 0, transform.position.z);
        float distance = Vector3.Distance(from, current);

        if (distance <= ctx.Radius)
        {
            float damage = ctx.Damage;
            if (ctx.LinearFalloff)
            {
                damage *= (distance / ctx.Radius);
            }

            TakeDamage(damage);
        }
    }

    private void TakeDamage(float damage)
    {
        if(!GodMode)
        {
            health -= damage;

            EventSystem.Current.FireEvent(new PlayerHealthChangedCtx(damage, health, this));

            if (health <= 0)
            {
                jamacIsDead = true;
            }
        }
    }

    public void OnGotoNextLevel(GotoNextLevelContext context)
    {
        levelPositionStack.Push(transform.position);
        levelRotationStack.Push(transform.rotation);
        CharacterController.enabled = false;
        transform.position = context.location;
        CharacterController.enabled = true;
    }

    public void OnGotoPreviousLevel(GotoPreviousLevelContext context)
    {
        if(levelPositionStack.Count != 0)
        {
            CharacterController.enabled = false;
            transform.position = levelPositionStack.Pop();
            transform.rotation = levelRotationStack.Pop();
            CharacterController.enabled = true;
        } else
        {
            ScoreHolder.GetComponent<ScoreHolder>().Score = documents;
            DontDestroyOnLoad(ScoreHolder);
            SceneManager.LoadScene(4);
        }
    }

    public void OnPickupWeapon(PickupWeaponContext context)
    {
        bool hasEmptySlot = false;
        for(int i = 0; i < HeldGuns.Length; i++)
        {
            if(HeldGuns[i].Empty)
            {
                hasEmptySlot = true;
                HeldGuns[i].SetData(context.pickup.gunData);
                if(CurrentGun == i)
                {
                    GunManager.Current.GetModel(context.pickup.gunData).SetActive(true);
                }

                break;
            }
        }

        if(hasEmptySlot)
        {
            Destroy(context.pickup.gameObject);
        }
        else
        {
            GunManager.Current.GetModel(context.pickup.gunData).SetActive(true);
            GunData swapGun = HeldGuns[CurrentGun].GunData;
            HeldGuns[CurrentGun].SetData(context.pickup.gunData);
            context.pickup.gunData = swapGun;
            GunManager.Current.GetModel(context.pickup.gunData).SetActive(false);
        }
    }

    public void OnPickupItem(PickupItemContext context)
    {
        documents++;
    }

    public void OnProjectileHit(ProjectileHitContext ctx) {
        if (ctx.hit.gameObject.tag == "Player") {
            TakeDamage(ctx.damage);
        }
    }
}
