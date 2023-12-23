//Author: Ian Stolte
//Date: 7/12/23
//Desc: Controls player movement

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;

    //Game objects
    [Header("GameObjects")]
    [SerializeField] GameObject groundCheck;
    [SerializeField] GameObject wallCheck;

    //Layer masks
    [Header("LayerMasks")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask wallLayer;
    [SerializeField] LayerMask hazardLayer;

    //Movement
    public int hInput;
    [Header("Movement")]
    [SerializeField] float moveSpeed;

    //Movement states
    [Tooltip("Freezes the character's movement")] public bool freeze;
    public bool pulling;

    //Jump
    [Header("Jump")]
    [SerializeField] float jumpPower;
    public float jumpMultiplier;
    public int maxJumps;
    [Tooltip("Default gravity (used while moving upward)")][SerializeField] float gravity;
    [Tooltip("Multiplier to increase gravity while falling")][SerializeField] float fastFall;
    [Tooltip("Multiplier to decrease gravity at the top of a jump")][SerializeField] float jumpHang;
    float jumpsLeft;
    float jumpResetTimer;
    [Tooltip("Time after moving off a platform where you can still jump")][SerializeField] float coyoteTime;
    float coyoteTimer;
    [Tooltip("Whether to jump higher when space is held")][SerializeField] bool variableJump;
    bool airborne;

    //Land
    Vector2 pastVelocity;

    //Wall jump
    [Header("Wall jump")]
    [Tooltip("Velocity in X direction")][SerializeField] float wallJumpPower;
    [Tooltip("Velocity in Y direction")][SerializeField] float wallJumpLift;
    public bool canWallJump;
    [HideInInspector] public bool onWall;

    //Climbing
    [Header("Climb")]
    public bool canClimb;
    public bool climbing;

    //Pulling
    [HideInInspector] public GameObject pullObject;
    Vector3 previousPos;
    public bool canPull;

    //Movement abilities
    [HideInInspector] public float moveTimer;
    float speed;

    //Dash
    [Header("Dash")]
    [SerializeField] float dashSpeed;
    public bool canDash;
    bool dashing;
    float dashTimer;

    //Respawn
    public Vector3 checkpoint;
    public UnityEvent deathEvent;

    //Script references
    AudioManager audioManager;
    PlayerInfo playerInfo;


    void Awake()
    {
        audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
        playerInfo = GetComponent<PlayerInfo>();
        
        maxJumps--;
        jumpsLeft = maxJumps;
        canPull = true;
        jumpMultiplier = 1;
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu")
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        else if (scene.name != "Test Level" && scene.name != "Dark Level" && scene.name != "Library (5)" && scene.name != "Parkour (3)")
        {
            GetComponent<SpriteRenderer>().enabled = true;
            LevelSelect levelSelect = GameObject.Find("Level Select").GetComponent<LevelSelect>();
            transform.position = levelSelect.startingPositions[levelSelect.currentLevel - 1];
            GameObject.Find("Spirit Orb").GetComponent<Animator>().Play("SpiritOrbOn");
            checkpoint = GameObject.Find("Spirit Orb").transform.position;
        }
    }

    void FixedUpdate()
    {
        //Movement without acceleration
        //horizontal = Input.GetAxisRaw("Horizontal");

        if ((Input.GetKey(playerInfo.binds[0]) || Input.GetKey(playerInfo.binds[1])) &&
            !(Input.GetKey(playerInfo.binds[2]) || Input.GetKey(playerInfo.binds[3])))
        {
            hInput = -1;
        }
        else if ((Input.GetKey(playerInfo.binds[2]) || Input.GetKey(playerInfo.binds[3])) &&
            !(Input.GetKey(playerInfo.binds[0]) || Input.GetKey(playerInfo.binds[1])))
        {
            hInput = 1;
        }
        else
        {
            hInput = 0;
        }

        //Basic x-axis movement
        if (moveTimer < 0 && !freeze)
        {
            dashing = false;
            if (pulling)
            {
                rb.velocity = new Vector2(hInput * moveSpeed * 0.5f, rb.velocity.y);
            }
            else if (climbing)
            {
                rb.velocity = new Vector2(hInput * moveSpeed * 0.5f, Input.GetAxisRaw("Vertical") * moveSpeed * 0.6f);
            }
            else
            {
                rb.velocity = new Vector2(hInput * moveSpeed, rb.velocity.y);
            }
        }
        else if (!freeze)
        {
            if (dashing)
            {
                rb.velocity = new Vector2(DirectionFacing() * dashSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(speed * moveTimer + ((1 - moveTimer * 2) * hInput * moveSpeed), rb.velocity.y);
            } 
        }
        else
        {
            rb.velocity = new Vector2(0, 0);
        }

        //Set scale (direction of sprite)
        if (hInput > 0 && !pulling && !freeze) //moving right
        {
            wallCheck.transform.localScale = new Vector2(1, 1);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        }
        else if (hInput < 0 && !pulling && !freeze) //moving left
        {
            wallCheck.transform.localScale = new Vector2(-1, 1);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        //Falling
        if (freeze || climbing)
        {
            rb.gravityScale = 0;
        }
        //Wall slide
        else if (((OnWall() == 1 && hInput == -1) || (OnWall() == 2 && hInput == 1)) && rb.velocity.y < 0)
        {
            //when first hit wall
            if (!onWall)
            {
                rb.gravityScale = 0;
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.33f);
                onWall = true;
            }
            else
            {
                rb.gravityScale += (gravity*fastFall - rb.gravityScale) / 50;
            }
        }
        else if (rb.velocity.y < -2)
        {
            rb.gravityScale = gravity * fastFall;
        }
        else if (rb.velocity.y > 2)
        {
            rb.gravityScale = gravity;
        }
        else if (!IsGrounded())
        {
            rb.gravityScale = gravity * jumpHang;
        }


        //max fall speed
        if (onWall && rb.velocity.y < -15)
            rb.velocity = new Vector2(rb.velocity.x, -15);
        else if (rb.velocity.y < -30)
            rb.velocity = new Vector2(rb.velocity.x, -30);

        if (OnWall() == 0)
        {
            onWall = false;
        }

        //Pulling cart
        if (pullObject != null)
        {
            pullObject.transform.position += (transform.position - previousPos);
        }
        previousPos = transform.position;

        GetComponent<Animator>().SetFloat("yVelocity", rb.velocity.y);
        GetComponent<Animator>().SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
    }

    void Update()
    {
        bool paused = GameObject.Find("Level Select").GetComponent<LevelSelect>().paused;

        //Climbing
        if (Physics2D.OverlapBox(transform.position, new Vector2(1, 2), 0, LayerMask.GetMask("Climbable")) && !climbing && Input.GetKeyDown(KeyCode.W)
        && canClimb)
        {
            climbing = true;
            transform.position += new Vector3(0, 0.2f, 0);
        }
        else if (IsGrounded() || !Physics2D.OverlapBox(transform.position, new Vector2(1, 2), 0, LayerMask.GetMask("Climbable")))
        {
            climbing = false;
        }

        if (climbing && Input.GetKeyDown(playerInfo.binds[5]))
        {
            climbing = false;
        }

        if (Physics2D.OverlapBox(transform.position, new Vector2(1, 2), 0, LayerMask.GetMask("StartDescent")) && !climbing &&
        Input.GetKeyDown(KeyCode.S) && canClimb)
        {
            climbing = true;
            transform.position += new Vector3(0, -1, 0);
        }

        //Hazards
        if (Physics2D.OverlapBox(transform.position, new Vector2(1, 2), 0, hazardLayer) && !freeze)
        {
            StartCoroutine(GameOver());
        }

        //Timers
        jumpResetTimer -= Time.deltaTime;
        coyoteTimer -= Time.deltaTime;
        moveTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;

        //Dash
        if (Input.GetMouseButtonDown(0) && dashTimer < 0 && !paused && !pulling && canDash) //TODO: make dash part of the keybinds menu (I don't think you can include mouse presses though...)
        {
            audioManager.Play("Dash");
            dashing = true;
            dashTimer = 0.5f;
            moveTimer = 0.15f;
        }

        //Pull
        if (Input.GetKeyDown(playerInfo.binds[5]) && pulling)
        {
            StartCoroutine(StopPulling());
        }

        /////////////
        /// JUMPS ///
        /////////////

        GetComponent<Animator>().SetBool("Grounded", IsGrounded());
        GetComponent<Animator>().SetBool("Frozen", freeze);

        //Wall Jump
        if (OnWall() != 0 && Input.GetKeyDown(playerInfo.binds[4]) && !IsGrounded() && !climbing && canWallJump && !pulling && !freeze)
        {
            //audioManager.Play("Wall Jump");
            if (OnWall() == 1) {
                speed = wallJumpPower;
            }
            else {
                speed = -wallJumpPower;
            }
            moveTimer = 0.5f;
            rb.velocity = new Vector2(speed, wallJumpLift);
            GetComponent<Animator>().Play("PlayerJump");
            //Rotate Box
            if (Physics2D.OverlapBox(transform.position, new Vector2(2, 2), 0, LayerMask.GetMask("RotateBoxWall"))) {
                StartCoroutine(RotateObject(Physics2D.OverlapBox(transform.position, new Vector2(2, 2), 0, LayerMask.GetMask("RotateBoxWall")).transform));
            }
        }

        //Press jump button (vertical jump)
        else if (Input.GetKeyDown(playerInfo.binds[4]) && (coyoteTimer > 0 || jumpsLeft > 0) && jumpResetTimer < 0 && !pulling && !freeze)
        {
            dashing = false;
            moveTimer = -1;
            if (coyoteTimer < 0) //if air jump
            {
                jumpsLeft--;
                //audioManager.Play("Air Jump");
            }
            else
            {
                //audioManager.Play("Jump");
            }
            jumpResetTimer = 0.1f;
            rb.velocity = new Vector2(rb.velocity.x, jumpPower*jumpMultiplier);
            GetComponent<Animator>().Play("PlayerJump");
            //Rotate Box
            if (Physics2D.OverlapBox(transform.position + new Vector3(0, -1, 0), new Vector2(2, 1), 0, LayerMask.GetMask("RotateBoxGround"))) {
                StartCoroutine(RotateObject(Physics2D.OverlapBox(transform.position + new Vector3(0, -1, 0), new Vector2(2, 1), 0, LayerMask.GetMask("RotateBoxGround")).transform));
            }
        }

        //Release jump button
        if (Input.GetKeyUp(playerInfo.binds[4]) && rb.velocity.y > 0f && moveTimer < 0 && variableJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            
        }

        //Land
        if ((IsGrounded() || climbing) && jumpResetTimer < 0)
        {
            jumpsLeft = maxJumps;
            coyoteTimer = coyoteTime;
            if (airborne)
            {
                airborne = false;
                int n = UnityEngine.Random.Range(1, 3); //1 or 2
                Sound s = Array.Find(audioManager.sfx, sound => sound.name == "Land" + n);
                s.source.volume = Mathf.Pow(pastVelocity.y, 2)/4000.0f;
                if (n == 1) {
                    s.source.volume /= 2;
                }
                if (pastVelocity.y < -5)
                {
                    GetComponent<Animator>().Play("PlayerLand");
                    s.source.volume = Mathf.Max(s.source.volume, 0.05f);
                }
                else{
                    s.source.volume = 0.02f;
                }
                audioManager.Play("Land" + n);
            }
        }
        pastVelocity = rb.velocity;

        if (!IsGrounded() && !climbing)
        {
            airborne = true;
        }
    }

    private float DirectionFacing()
    {
        return Mathf.Abs(transform.localScale.x) / transform.localScale.x;
    }

    public int OnWall()
    {
        Bounds b1 = wallCheck.transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>().bounds;
        Bounds b2 = wallCheck.transform.GetChild(1).gameObject.GetComponent<BoxCollider2D>().bounds;
        if (Physics2D.OverlapBox(b1.center, b1.extents * 2, 0, wallLayer)) {
            return 1;
        }
        else if (Physics2D.OverlapBox(b2.center, b2.extents * 2, 0, wallLayer)) {
            return 2;
        }
        else {
            return 0;
        }
    }

    public bool IsGrounded()
    {
        Bounds b = groundCheck.GetComponent<BoxCollider2D>().bounds;
        return Physics2D.OverlapBox(b.center, b.extents * 2, 0, groundLayer);
    }

    public bool OnPlatform()
    {
        Bounds b = groundCheck.GetComponent<BoxCollider2D>().bounds;
        return Physics2D.OverlapBox(b.center, b.extents * 2, 0, LayerMask.GetMask("MovingPlatform", "Climbable"));
    }

    public IEnumerator StopPulling()
    {
        canPull = false;
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Stop("CartPush");
        pullObject = null;
        pulling = false;
        yield return new WaitForSeconds(0.1f);
        canPull = true;
    }

    IEnumerator GameOver()
    {
        deathEvent.Invoke();
        GameObject.Find("Audio Manager").GetComponent<AudioManager>().Play("Die");
        GetComponent<SpriteRenderer>().enabled = false;
        freeze = true;

        yield return new WaitForSeconds(0.5f);
        transform.position = checkpoint;
        GetComponent<SpriteRenderer>().enabled = true;
        GameObject.Find("Main Camera").GetComponent<CameraFollow>().GoToPlayer();
        //fade in
        for (float i = 0; i < 1.1f; i+= 0.05f)
        {
            Color c = Color.white;
            c.a = i;
            GetComponent<SpriteRenderer>().color = c;
            yield return new WaitForSeconds(0.025f);
        }
        rb.gravityScale = 1;
        freeze = false;
    }

    IEnumerator RotateObject(Transform obj)
    {
        if (Mathf.Abs(obj.rotation.eulerAngles.z) < 0.1f || Mathf.Abs(obj.rotation.eulerAngles.z-180) < 0.1f) {
            obj.gameObject.layer = LayerMask.NameToLayer("RotateBoxGround");
        }
        else {
            obj.gameObject.layer = LayerMask.NameToLayer("RotateBoxWall");
        }
        yield return new WaitForSeconds(0.1f);
        for (float i = 0.01f; i < 0.5f; i += 0.01f)
        {
            if (freeze)
                yield break;
            obj.Rotate(0, 0, -90.0f/50);
            yield return new WaitForSeconds(0.01f);
        }
    }
}