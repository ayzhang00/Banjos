using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CharController : MonoBehaviourPun
{
    [SerializeField]
    // basic set up
    float camSpeed = 3f;
    float maxCamDist = 1f;
    public Rigidbody rb;
    bool playing = true;
    public bool obscured = false;
    GameObject[] LEDs;
    Vector3 camOffset = new Vector3(-15f, 12f, -15f);
    Vector3 forward, right;
    PhotonView pv;
    // sounds
    public AudioSource swing;
    public AudioSource hit;
    public AudioSource walk;
    public AudioSource solderSound;
    public AudioSource bg;
    public AudioClip bgtrack1;
    public AudioClip bgtrack2;
    public AudioClip coreTrack1;
    public AudioClip coreTrack2;
    // movement
    public float moveSpeed = 6f;
    float jumpSpeed = 4f;
    bool canJump = false;
    public bool isMoving = false;
    // attack
    public GameObject attack;
    public GameObject flash;
    public GameObject deathEffect;
    public GameObject sphere;
    public float health = 5f;
    public bool isAttacking = false;
    float timeToAttack = 0.3f;
    float timeAttacked = 0f;
    // solder
    public GameObject solder;
    public bool canSolder = false;
    public bool isSoldering = false;
    public float timeToCompleteSolder = 2f;
    float timeSoldered = 0f;
    public bool solderComplete;

    // public bool runToTheCoreMusic = false;
    bool isPlayingCoreMusic = false;


    // Start is called before the first frame update
    void Start()
    {
        //Changed this because the camera's forward is pointing slightly down
        // forward = Camera.main.transform.forward;
        // forward.y = 0f;
        // // make sure length is one
        // forward = Vector3.Normalize(forward);
        forward = Quaternion.Euler(new Vector3(0, 45, 0)) * Vector3.forward;
        right = Quaternion.Euler(new Vector3(0, 45, 0)) * Vector3.right;
        Camera.main.transform.position = transform.position + camOffset;

        LEDs = GameObject.FindGameObjectsWithTag("LED");

        pv = gameObject.GetComponent<PhotonView>();
        StartCoroutine("PlayBackgroundMusic");
    }

    // Update is called once per frame
    void Update()
    {
        // isMoving = false;
        if (playing && pv.IsMine) {
            obscured = false;
            if (Input.GetButtonDown("Jump")){
                Solder(false);
                solderSound.Stop();
                Jump();
            } 
            // move
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
                isMoving = true;
                Move();
                Solder(false);
                solderComplete = false;
            } else {
                isMoving = false;
            }
            // attack
            if (!isAttacking && Input.GetButtonDown("Fire")) {
                isAttacking = true;
                Attack();
                Solder(false);
                solderComplete = false;
            } 
            if (isAttacking) {
                timeAttacked += Time.deltaTime;
                if (timeAttacked >= timeToAttack) {
                    isAttacking = false;
                    timeAttacked = 0f;
                }
            }
            // Q is solder
            if (Input.GetButtonDown("Solder") && canSolder) {
                Solder(true);
                solderComplete = false;
                solderSound.Play();
            }
            if (Input.GetButtonUp("Solder")) {
                Solder(false);
                solderSound.Stop();
            }

            if (Input.GetButtonDown("Fire3")) {
                if (!isPlayingCoreMusic) {
                    bg.Stop();
                    StopCoroutine("PlayBackgroundMusic");
                    StartCoroutine("PlayRunToTheCoreMusic");
                    isPlayingCoreMusic = true;
                } else {
                    bg.Stop();
                    StopCoroutine("PlayRunToTheCoreMusic");
                    StartCoroutine("PlayBackgroundMusic");
                    isPlayingCoreMusic = false;

                }
            }

            if (moveSpeed != 0) MoveCamera();
            if (isSoldering) {
                timeSoldered += Time.deltaTime;
                if (timeSoldered >= timeToCompleteSolder) {
                    solderComplete = true;
                    Solder(false);
                    // solderSound.Stop();
                }
            }
            foreach(GameObject LED in LEDs) {
                // if (Vector3.Distance(transform.position, LED.transform.position) < 20) {
                Light l = LED.GetComponent<Light>();
                if (Vector3.Distance(transform.position, LED.transform.position) < 20 && 
                        // !l.enabled) {
                        !LED.activeSelf) {
                    obscured = true;
                }
            }
            Obscure(obscured);

        }
    }

    void Move()
    {
        Vector3 direction = Vector3.Normalize(new Vector3(Input.GetAxis("HorizontalKey"), 0, Input.GetAxis("VerticalKey")));
        Vector3 rightMovement = right * moveSpeed * Time.deltaTime * direction.x;
        Vector3 upMovement = forward * moveSpeed * Time.deltaTime * direction.z;

        // movement heading
        Vector3 heading = Vector3.Normalize(rightMovement + upMovement);
        transform.forward = heading;
        transform.position += rightMovement;
        transform.position += upMovement;
    }

    void Jump() {
        if (canJump) {
            rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
        }
    }

    void OnCollisionStay(Collision collision) {
        if (collision.collider.tag == "Ground") {
            canJump = true;
        }
    }
    
    void OnTriggerEnter(Collider collider) {
        if (collider.tag == "Attack") {
            Spark();
            hit.Play();
            health--;
            if (health <= 0) {
                StartCoroutine(Death());
            }
        }
    }

    void OnCollisionExit(Collision collision) {
        if (collision.collider.tag == "Ground") {
            canJump = false;
        }
    }

    void MoveCamera() {
        Vector3 start = Camera.main.transform.position;
        Vector3 dest = transform.position + camOffset;
        Vector3 dir = start - dest;

        float dist = dir.magnitude;
        float step = (dist / maxCamDist) * camSpeed;

        Camera.main.transform.position -= dir.normalized * step * Time.deltaTime;
    }

    void Solder(bool isActive) {
        pv.RPC("SwitchActiveObject", RpcTarget.All, "Solder", isActive);
    }
    
    void Attack() {
        pv.RPC("SwitchActiveObject", RpcTarget.All, "Attack", true);
    }

    void Spark() {
        pv.RPC("SwitchActiveObject", RpcTarget.All, "Flash", true);
    }

    void Obscure(bool isActive) {
        pv.RPC("SwitchActiveObject", RpcTarget.All, "Sphere", isActive);
    }

    void PlayWalkSound() {
        walk.Play();
    }

    void PlaySwingSound() {
        swing.Play();
    }

    IEnumerator PlayBackgroundMusic() {
        bg.clip = bgtrack1;
        bg.Play();
        yield return new WaitForSeconds(bg.clip.length);
        bg.clip = bgtrack2;
        bg.Play();
    }

    IEnumerator PlayRunToTheCoreMusic() {
        bg.clip = coreTrack1;
        bg.Play();
        yield return new WaitForSeconds(bg.clip.length);
        bg.clip = coreTrack2;
        bg.Play();
    }

    IEnumerator Death() {
        playing = false;
        deathEffect.SetActive(true);
        yield return new WaitForSeconds(1f);
        // gameObject.SetActive(false);
        if (pv.IsMine) {
            PhotonNetwork.Destroy(pv);
        }
    }

    [PunRPC]
    void SwitchActiveObject(string obj, bool isActive) {
        if (obj == "Attack") {
            attack.SetActive(isActive);
        }
        else if (obj == "Flash") {
            flash.SetActive(isActive);
        }
        else if (obj == "Solder") {
            solder.SetActive(isActive);
            isSoldering = isActive;
            if (!isActive) {
                timeSoldered = 0f;
            }
        }
        else if (obj == "Sphere") {
            sphere.SetActive(!isActive);
        }
    }
}
