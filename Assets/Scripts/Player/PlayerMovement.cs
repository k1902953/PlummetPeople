using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.Dynamics;

public class PlayerMovement : MonoBehaviourPun
{
    // OBJECTS
    public Transform cam;
    [HideInInspector]
    public CharacterController controller;

    public PlayerAnimatorManager playerAnimatorManager;

    // CAMERA
    Vector3 camF;
    Vector3 camR;

    // PHYSICS
    Vector3 intent;
    Vector3 velocityXZ;
    public Vector3 velocity;

    public float speed = 3f;
    public float start_speed;
    public float defaultAccel = 15;
    float accel = 15;
    float turnSpeed = 5;
    public float turnSpeedLow = 5;
    public float turnSpeedHigh = 7;
    public bool rotationDisabled = false;

    // GRAVITY
    public float grav = 10;
    bool grounded = false;
    bool checkForGround = true;

    public float jumpHeight;

    public float dashHeight;
    public float dashForce;
    public float dashCooldown;
    private bool canDash = true;

    public Vector3 groundCheckV;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public PuppetMaster puppetMaster;
    public bool isShocked = false;
    float start_pinWeight, start_muscleWeight;

    MP_Lobby mpLobby;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        start_pinWeight = puppetMaster.pinWeight;
        start_muscleWeight = puppetMaster.muscleWeight;
        start_speed = speed;

        if (FindObjectOfType<MP_Lobby>()) mpLobby = FindObjectOfType<MP_Lobby>();
    }

    private void Start()
    {
        SetupPlayer();

        // checks through Start() first, then OnLevelLoaded() (or whatever it is)
        StartCoroutine(CheckIfSpectate());
    }

    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }

        CalculateCamera();
        GroundCheck();
        DoMove();
        DoGravity();
        DoJump();
        Dash();

        controller.Move(velocity * Time.deltaTime);
    }


    /*
      Gets keyboard input and assigns to a Vector2 which is used to get forward and side input for movement.
      The input is clamped to 1 so that the player does not move faster going diagonally rather than forwards.
      If the player is attacking a target, the input vector is cancelled so that the attack script can rotate
      the player towards the enemy.
    */
    Vector2 GetInput()
    {
        if (mpLobby) 
            if (mpLobby.disableMovement) 
                return Vector2.zero;

        if (GetComponent<PlayerManager>().eliminated || PauseMenu.instance.isPaused) return Vector2.zero;

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        return Vector2.ClampMagnitude(input, 1);
    }

    /*
      Gets forward and right vector of the camera used in the DoMove() function so the player moves facing
      the into the camera direction.
    */
    void CalculateCamera()
    {
        camF = cam.forward;
        camR = cam.right;

        camF.y = 0;
        camR.y = 0;
        camF = camF.normalized;
        camR = camR.normalized;
    }

    void GroundCheck()
    {
        grounded = Physics.CheckSphere(transform.position + groundCheckV, groundDistance, groundMask) && checkForGround && !isShocked;
    }

    /*
      Checks that the player is touching the ground. This is used to see if the player can jump.
    */
    public bool Grounded()
    {
        bool grounded = Physics.CheckSphere(transform.position + groundCheckV, groundDistance, groundMask) && checkForGround && !isShocked;
        playerAnimatorManager.SetAnimBool("grounded", grounded);
        return grounded;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        if (checkForGround)
            Gizmos.DrawSphere(transform.position + groundCheckV, groundDistance);
    }

    /*
      This function controls the movement of the character and 
      rotates the player in the direction of their velocity.
    */
    void DoMove()
    {
        intent = camF * GetInput().y + camR * GetInput().x;

        float tS = velocity.magnitude / 5;
        turnSpeed = Mathf.Lerp(turnSpeedHigh, turnSpeedLow, tS);
        if (GetInput().magnitude > 0 && !rotationDisabled && !isShocked)
        {
            Quaternion rot = Quaternion.LookRotation(intent);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }
        velocityXZ = velocity;
        velocityXZ.y = 0;
        velocityXZ = Vector3.Lerp(velocityXZ, transform.forward * GetInput().magnitude * speed, accel * Time.deltaTime);
        velocity = new Vector3(velocityXZ.x, velocity.y, velocityXZ.z);
    }


    /*
      This gradually decreases y velocity when the player is in the air.
    */
    void DoGravity()
    {
        if (Grounded())
        {
            velocity.y = -0.2f;
            accel = Mathf.Lerp(accel, defaultAccel, Time.deltaTime * 5);
            turnSpeedLow = Mathf.Lerp(turnSpeedLow, 5, Time.deltaTime * 5);
            turnSpeedHigh = Mathf.Lerp(turnSpeedHigh, 15, Time.deltaTime * 5);
        }
        else
        {
            velocity.y -= grav * Time.deltaTime;
            velocity.y = Mathf.Clamp(velocity.y, -53, 53);
            accel = Mathf.Lerp(accel, 1, Time.deltaTime * 15);
            turnSpeedLow = Mathf.Lerp(turnSpeedLow, 2, Time.deltaTime * 15);
            turnSpeedHigh = Mathf.Lerp(turnSpeedHigh, 4, Time.deltaTime * 15);
        }
    }

    /*
      Adds y velocity when the player presses the jump key. Checks to make sure they are grounded and not dashing.
    */
    void DoJump()
    {
        if (Input.GetButtonDown("Jump") && Grounded())
        {
            checkForGround = false;
            StartCoroutine("ResetGroundCheck");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * -grav);
            StartCoroutine(playerAnimatorManager.SetAnimTrigger("jump"));
        }
    }

    private void Dash()
    {
        if (Input.GetButtonDown("Dash") && canDash)
        {
            checkForGround = false;
            StartCoroutine("ResetGroundCheck");
            StartCoroutine(DashCooldown());
            velocity.y = Mathf.Sqrt(dashHeight * -2f * -grav);
            velocity += transform.forward * dashForce;
            playerAnimatorManager.PlayAnim("a|dash");
            StartCoroutine(playerAnimatorManager.SetAnimTrigger("dash"));
        }
    }

    public void GetPushed(Vector3 pushedFromPos, float pushForce)
    {
        checkForGround = false;
        StartCoroutine("ResetGroundCheck");
        Vector3 dir = transform.position - pushedFromPos;
        //velocity.y += 1;
        velocity += dir * pushForce;
    }

    public IEnumerator GetShocked(Vector3 shockDir, float shockForce, float shockTime)
    {
        isShocked = true;

        shockDir *= 100;
        shockDir = Vector3.ClampMagnitude(shockDir, 1);

        puppetMaster.pinWeight = 0.1f;
        puppetMaster.muscleWeight = 0.1f;

        checkForGround = false;
        StartCoroutine("ResetGroundCheck");

        //print(shockDir.magnitude);
        velocity.y += 5;//shockForce * 0.25f;
        velocity += shockDir * shockForce;

        speed = 0.1f;

        yield return new WaitForSeconds(shockTime);

        isShocked = false;

        puppetMaster.pinWeight = start_pinWeight;
        puppetMaster.muscleWeight = start_muscleWeight;

        speed = start_speed;
    }

    /*
      So that when the player jumps, it stops checking for the ground temporarily so they don't get stuck
    */
    IEnumerator ResetGroundCheck()
    {
        yield return new WaitForSeconds(0.1f);
        checkForGround = true;
    }

    IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "eliminate")
        {
            if (GetComponent<PlayerManager>().isSpectate) return;

            GetComponent<PlayerManager>().eliminated = true;
            puppetMaster.state = PuppetMaster.State.Dead;
            CheckForWinner();

            AudioSource audio = Audio.instance.MakeSound(transform.position, transform);
            audio.clip = Audio.instance.clips_vocal_die[Random.Range(0, Audio.instance.clips_vocal_die.Count)];
            audio.volume = 0.5f;
            audio.spatialBlend = 1f;
            audio.Play();
            audio.GetComponent<Sound>().StartDelete(audio.clip.length);

            foreach (CharacterController c in FindObjectsOfType<CharacterController>())
            {
                if (!c.GetComponent<PlayerManager>().eliminated)
                {
                    if (cam)
                        cam.GetComponent<PlayerCamera>().player = c.transform;
                    break;
                }
            }
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        print($"<color=green>OnLevelWasLoaded called - PlayerMovement()</color>");

        SetupPlayer();

        // first checks this via Start(), then through here - not sure why it doesn't do this the first time
        StartCoroutine(CheckIfSpectate());
    }

    private void SetupPlayer()
    {
        puppetMaster.state = PuppetMaster.State.Alive;

        puppetMaster.pinWeight = start_pinWeight;
        puppetMaster.muscleWeight = start_muscleWeight;
        speed = start_speed;
        isShocked = false;
    }

    private IEnumerator CheckIfSpectate()
    {
        if (!photonView.IsMine) yield break;

        GetComponent<PlayerManager>().isSpectate = false;

        yield return new WaitForSeconds(0.5f);

        if (FindObjectOfType<WipeoutManager>())
        {
            print($"<color=green>found WipeoutManager.instance</color>");
            if (FindObjectOfType<WipeoutManager>().GameIsActive())
            {
                print($"<color=orange>GAME IS ALREADY ACTIVE, FORCING SPECTATE</color>");

                GetComponent<PlayerManager>().eliminated = true;
                GetComponent<PlayerManager>().isSpectate = true;
                GetComponent<PlayerManager>().ForceSpectate();
                puppetMaster.state = PuppetMaster.State.Frozen;

                GetComponent<CharacterController>().enabled = false;
                transform.position += Vector3.up * -5;
                GetComponent<CharacterController>().enabled = true;
            }
            else
            {
                print($"<color=green>!WipeoutManager.instance.GameIsActive()</color>");
            }
        }
        else
        {
            print($"<color=green>WipeoutManager.instance not found!</color>");
        }
    }

    private void CheckForWinner()
    {
        List<PlayerManager> alivePlayers = new List<PlayerManager>();

        foreach (CharacterController c in FindObjectsOfType<CharacterController>())
        {
            if (!c.GetComponent<PlayerManager>().eliminated)
                alivePlayers.Add(c.GetComponent<PlayerManager>());
        }

        if (alivePlayers.Count == 1)
        {
            print($"<color=green><b>WINNER: {alivePlayers[0].GetComponent<PhotonView>().Owner.NickName}</b></color>");
            FindObjectOfType<WinUi>().SetWinnerText($"{alivePlayers[0].GetComponent<PhotonView>().Owner.NickName} WINS!");
            FindObjectOfType<ParticleSystem>().Play();

            AudioSource audio = Audio.instance.MakeSound(alivePlayers[0].transform.position);
            audio.clip = Audio.instance.clips_vocal_winVocal[Random.Range(0, Audio.instance.clips_vocal_winVocal.Count)];
            audio.volume = 0.4f;
            audio.spatialBlend = 1f;
            audio.Play();
            audio.GetComponent<Sound>().StartDelete(audio.clip.length);

            if (PhotonNetwork.IsMasterClient) StartCoroutine(LoadLobby());
        }
        else if (alivePlayers.Count == 0 && !FindObjectOfType<WinUi>().activated)
        {
            print($"<color=red><b>Could not find a winner</b></color>");
            FindObjectOfType<WinUi>().SetWinnerText($"WINNER NOT FOUND");
            FindObjectOfType<ParticleSystem>().Play();
            if (PhotonNetwork.IsMasterClient) StartCoroutine(LoadLobby());
        }
    }

    IEnumerator LoadLobby()
    {
        yield return new WaitForSeconds(3);
        PhotonNetwork.LoadLevel("MP_Lobby");
    }
}
