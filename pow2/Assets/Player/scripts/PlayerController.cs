using UnityEngine;
using System.Collections;
using InControl;

public class PlayerController : MonoBehaviour {
    // basic props
    public int hp;
    private bool isGrounded;

    // player properties for multiplayer
    public int playerNumber;
    public GameObject indicator;

    // name variables for buttons
    private string moveAxisJoystickName;
    private string jumpButtonName;
    private string diveButtonName;

    // Movement variables
    private bool isFacingRight;
    private Vector2 jumpVelocity;
    private float moveVelocity;
    private float diveVelocity;
    
    // utility for stick deadzone
    private float deadzoneThreshold;
    
    // components of the gameobject and children of the gameobject
    private Color playerColor;
    private SpriteRenderer playerSpriteRenderer;
	private GameObject playerAim;
    private Rigidbody2D playerRigidbody;
	private Animator playerAnimator;

    // jump/dive variables
    private int usedJumps;
    private int totalJumps;
    private int nFramesAttack;
    private int curFramesAttack;

    private bool isSpawning;
    private bool isOnWallLeft;
    private bool isOnWallRight;

	public float rotationAngleOffset;
	public float shootAngleOffset;
	private bool canShoot;

	private float gravityNormal;
	private float gravityShoot;
	private float gravityWall;

	public bool jumpAttacking;
	public bool attacking;
	public bool jumping;
	public bool shooting;
	public bool dying;
	public bool running;
	public bool wallRiding;

	// Use this for initialization
	void Start () {
		moveAxisJoystickName = "L_XAxis_" + playerNumber;
		jumpButtonName = "A_" + playerNumber;
		diveButtonName = "X_" + playerNumber;
		
		moveVelocity = 500.0f;
		diveVelocity = 25.0f;
		isFacingRight = true;
		jumpVelocity = new Vector2(moveVelocity, 1250.0f);
		
		deadzoneThreshold = .2f;
		
		playerSpriteRenderer = GameObject.Find("Sprite").GetComponent<SpriteRenderer>();
		playerRigidbody = GetComponent<Rigidbody2D>();
		playerAnimator = GameObject.Find("Sprite").GetComponent<Animator>();
		playerAim = GameObject.Find("aim");		

		usedJumps = 0;
		totalJumps = 2;
		nFramesAttack = 3;
		curFramesAttack = 3;
		
		isSpawning = false;
		isOnWallLeft = false;
		isOnWallRight = false;
		canShoot = true;
	
		//rotationAngleOffset = 210;
		//shootAngleOffset = 150;

		gravityNormal = 5.0f;
		gravityShoot = 2.0f;
		gravityWall = .2f;

		jumpAttacking = false;
		attacking = false;
		jumping = false;
		shooting = false;
		dying = false;
		running = false;
		wallRiding = false;

		//SetupPlayerColor();
        
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void FixedUpdate() {
        if (isSpawning) {
            return;
        }

		if (wallRiding) {
			this.playerRigidbody.gravityScale = gravityWall;
		} else {
			this.playerRigidbody.gravityScale = gravityNormal;
		}

        DoMovement();
        //DoJump();
		DoControllerRotation();
		DoShoot();
		DoAttack();
        //DoDive();
    }

    void DoMovement() {
        float inputX = InputManager.ActiveDevice.LeftStickX.Value;//Input.GetAxisRaw(moveAxisJoystickName);

        if (inputX != 0) {
            if (!shooting) {
				if (inputX > 0 && !isFacingRight && !wallRiding) {
	                Flip();
	            } else if (inputX < 0 && isFacingRight && !wallRiding) {
	                Flip();
	            }
				
	
	            if (inputX > 0 && isOnWallRight) {
	                return;
	            } else if (inputX < 0 && isOnWallLeft) {
					print("I'm on the left wall");
	                return;
	            }

				running = true;
				this.playerAnimator.SetBool("running", running);
	
	            this.playerRigidbody.velocity = new Vector2(moveVelocity * inputX * Time.fixedDeltaTime, this.playerRigidbody.velocity.y);
        	}
		} else {
			running = false;
			this.playerAnimator.SetBool ("running", running);
		}
    }

    /*void DoJump() {
		if (InputManager.ActiveDevice.Action1.WasPressed) {//if (Input.GetButtonDown(jumpButtonName)) {
			canShoot = true;
			jumping = true;
			this.playerAnimator.SetBool("jumping", jumping);
            if (usedJumps < totalJumps) {      
                usedJumps++;
                isGrounded = false;

                Vector2 jumpWithDirectionVector = new Vector2(0, jumpVelocity.y);
                this.playerRigidbody.velocity = jumpWithDirectionVector * Time.fixedDeltaTime;
            }
        }
    }*/

	void DoShoot() {
		canShoot = usedJumps < totalJumps;
		if (InputManager.ActiveDevice.RightTrigger.WasPressed && canShoot) {
			usedJumps++;
			shooting = true;
			canShoot = false;
			float rad = (playerAim.transform.rotation.eulerAngles.z - shootAngleOffset) * Mathf.Deg2Rad;
			float x = Mathf.Cos (rad);
			float y = Mathf.Sin (rad);



			float deg = playerAim.transform.rotation.eulerAngles.z - shootAngleOffset;
			if (deg < 0) {
				deg += 360.0f;		
			}

			print("shooting at angle: " + deg);

			jumping = shooting = false;
			if (deg >= 0 && deg <= 180) {
				jumping = true;
				this.playerRigidbody.velocity = new Vector2(x,y).normalized * 25;
			} else {
				if (deg > 180 && deg <= 270)
					Flip ();
				shooting = true;
				playerSpriteRenderer.transform.rotation = Quaternion.Euler(0,0,deg);
				this.playerRigidbody.velocity = new Vector2(x,y).normalized * 25;
			}
			this.playerAnimator.SetBool("jumping", jumping);
			this.playerAnimator.SetBool("shooting", shooting);
		}
		return;
	}

	void DoControllerRotation() {
		if (InputManager.ActiveDevice.RightStick.Value != Vector2.zero) {
			this.playerRigidbody.freezeRotation = false;
			this.playerRigidbody.gravityScale = gravityShoot;
			float dx = InputManager.ActiveDevice.RightStickX.RawValue;
			float dy = InputManager.ActiveDevice.RightStickY.RawValue;

			float rad = Mathf.Atan2(dy, dx);
			float deg = rad * (180.0f / Mathf.PI) - rotationAngleOffset;
			if (deg < 0) {
				deg = (360.0f + deg);
			}
			playerAim.transform.rotation = Quaternion.AngleAxis( deg, new Vector3(0,0,1));
		} else {
			shooting = false;
		}
	}

	void DoAttack() {
		curFramesAttack++;
		if (curFramesAttack >= nFramesAttack) {
			attacking = false;
			this.playerAnimator.SetBool("attacking", attacking);
		}
		if (InputManager.ActiveDevice.Action2.WasPressed && !attacking && !shooting) {
			attacking = true;
			this.playerAnimator.SetBool("attacking", attacking);
			curFramesAttack = 0;
			print("I'm pushign");
		}
	}

	public void DoneAttacking() {
		attacking = false;
	}


    /*
        Getters
    */

    /*
        Utility functions
    */
    void Flip() {
		// Switch the way the player is labelled as facing
		isFacingRight = !isFacingRight;
		
		// Multiply the player's x local scale by -1
		Vector3 theScale = playerSpriteRenderer.transform.localScale;
		theScale.x *= -1;
		playerSpriteRenderer.transform.localScale = theScale;
    }

    void DoDie() {
        hp -= 1;
        if (hp <= 0) {
            Destroy(indicator.gameObject);
            Destroy(gameObject);
        }

        GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("DeathParticleSystem"), this.transform.position, Quaternion.identity);
        go.GetComponent<ParticleSystem>().startColor = playerSpriteRenderer.color;

        go = (GameObject)GameObject.Instantiate(Resources.Load("MinusOneParticleSystem"), this.transform.position, Quaternion.identity);
        go.GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>().material.SetColor("_DETAIL_MULX2", playerSpriteRenderer.color);
         go.GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>().material.SetColor("_EMISSION", playerSpriteRenderer.color);

        playerSpriteRenderer.enabled = false;
        GetComponent<PolygonCollider2D>().enabled = false;
        indicator.SetActive(false);

        StartCoroutine(Respawn());
    }

    IEnumerator Respawn() {
        yield return new WaitForSeconds(2.0f);
        indicator.SetActive(true);
        this.transform.position = Camera.main.GetComponent<EnvironmentController>().GetSpawnPoint();
        playerSpriteRenderer.enabled = true;
        playerRigidbody.isKinematic = true;
        isSpawning = true;
        yield return new WaitForSeconds(1.0f);
        GetComponent<PolygonCollider2D>().enabled = true;
        playerRigidbody.isKinematic = false;
        isSpawning = false;
    }

    /*
        Collision functions
    */
    void OnTriggerStay2D(Collider2D other) {
		
    }

    void OnCollisionEnter2D(Collision2D coll) {
		this.playerRigidbody.gravityScale = gravityNormal;
        usedJumps = 0;
        isGrounded = true;
		canShoot = true;
		jumping = false;
		shooting = false;
		this.playerAnimator.SetBool ("shooting", shooting);
		this.playerAnimator.SetBool("jumping", jumping);

        if (coll.gameObject.tag == "Enemy") { 
            GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("PlusOneParticleSystem"), this.transform.position, Quaternion.identity);
            go.GetComponent<ParticleSystem>().startColor = playerSpriteRenderer.color;
            playerRigidbody.AddForce(jumpVelocity * (isFacingRight ? 1 : -1));
			return;
        }

        if (coll.gameObject.tag == "Player") {
            /*if (coll.gameObject.GetComponent<PlayerController>().GetIsSpinning() || coll.gameObject.transform.position.y > transform.position.y) {
                DoDie();
            }*/

			return;
        }

        if (coll.gameObject.tag == "Wall") {
			wallRiding = true;
			playerAnimator.SetBool("wallRiding", wallRiding);
			Flip ();
	        isOnWallRight = isOnWallLeft = false;
	        if (coll.transform.position.x < transform.position.x) {	
	            isOnWallLeft = true;
	        } else {
	            isOnWallRight = true;
	        }
			return;
        }
		print ("hitting ground");
		this.playerRigidbody.freezeRotation = true;
		this.transform.rotation = Quaternion.Euler(0,0,0);
		this.playerSpriteRenderer.transform.rotation = Quaternion.Euler (0, 0, 0);
    }

     void OnCollisionExit2D(Collision2D coll) {
		canShoot = true;

		if (coll.gameObject.tag == "Wall") {
			wallRiding = false;
			playerAnimator.SetBool("wallRiding", wallRiding);
			isOnWallRight = isOnWallLeft = false;
			Flip ();
			return;
		}
    }

}
