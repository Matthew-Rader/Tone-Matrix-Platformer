using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collision))]
public class CharacterController2D : MonoBehaviour
{
	private Collision playerColl;
	private Rigidbody2D characterRigi;
	private Animator animator;
	private bool alive = true;
	private Color _DefaultCharacterColor = new Color(76/255.0f, 154/255.0f, 86/255.0f);


	#region  MOVEMENT VARIABLES
	[Header("Movement")]
	[SerializeField] private float movementSpeed = 10f;
	[SerializeField] private float accelerationTimeAirborne = 0.1f;
	[SerializeField] private float decelerationTimeAirborne = 0.1f;
	[SerializeField] private float accelerationTimeGrounded = 0.15f;
	[SerializeField] private float decelerationTimeGrounded = 0.05f;
	private bool canMove = true;
	private bool facingRight = true;
	private Vector3 velocitySmoothing = Vector3.zero;
	private Vector2 playerVelocity = Vector2.zero;
	private float moveX;
	private float moveY;
	#endregion


	#region JUMP SETTINGS AND VARIABLES
	[Header("Jumping")]
	[SerializeField] private bool _CanWallJump = true;
	[SerializeField] private float maxJumpHeight = 2.25f;
	[SerializeField] private float minJumpHeight = 1.0f;
	[SerializeField] private float timeToJumpApex = 0.25f;
	[Tooltip("Better Jumping Gravity Multiplier")]
	[SerializeField] private float fallMultiplier = 3.5f;
	[SerializeField] private Vector2 wallJumpVertical = new Vector2(2.0f, 10.0f);
	[SerializeField] private Vector2 wallJumpUp = new Vector2(6.0f, 25.0f);
	[SerializeField] private Vector2 wallJumpAway = new Vector2(20.0f, 10.0f);
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private bool jumpInputDown = false;
	private bool jumpInputUp = false;
	private bool jumping = false;
	private bool applyJumpQueue = false;
	private bool canCoyoteJump = false;
	private float _DefaultGravityMultiplier;

	[Header("Jump Timers and Delays")]
	[SerializeField] private float wallJumpVerticalControlDelay = 0.1f;
	[SerializeField] private float wallJumpAwayControlDelay = 0.15f;
	[SerializeField] private float wallJumpUpControlDelay = 0.15f;
	[SerializeField] private float jumpQueueTimer = 0.15f;
	[SerializeField] private float coyoteJumpTimer = 1.0f;
	#endregion


	#region WALL MECHANIC VARIABLES
	[Header("Wall Mechanics")]
	[SerializeField] private bool _CanGrabWall = false;
	[SerializeField] private bool _CanSlideAgainstWall = true;
	[SerializeField] private float slideRate = 2f;
	[SerializeField] private float wallGrabMovementSpeed = 5.0f;
	[SerializeField] private float wallGrabStaminaMax = 3.0f;
	[SerializeField] private float wallStickTime = 0.15f;
	private float timeToWallUnstick;
	private float wallGrabStamina = 0.0f;
	private bool grabWall = false;
	private bool applyWallSlide = true;
	private int wallDirX;
	#endregion


	#region TELEPORTATION 
	[Header("Teleportation")]
	[SerializeField] private LineRenderer aimingLineRenderer;
	[SerializeField] private GameObject teleportProjectilePrefab;
	private GameObject teleportProjectileReference;
	[SerializeField] private float teleportProjectileMovementSpeed;
	[SerializeField] private float slowmoLength = 0.3f;
	private Vector2 aimingVector = Vector2.zero;
	private enum ProjectileState { Fire, Teleport };
	private ProjectileState projectileState = ProjectileState.Fire;
	private ProjectileState _FIRE = ProjectileState.Fire;
	private ProjectileState _TELEPORT = ProjectileState.Teleport;
	private bool hasFired = false;
	private bool hasTeleported = false;
	private bool applyPostTeleportAirSlowmo = false;
	private bool canApplySlowMo = false;
	private bool slowmoInEffect = false;
	private Coroutine slowmoEffectCoroutine;
	private Coroutine playerFlashEffect;
	private bool playerWaitingToTeleport = false;
	[SerializeField] private Vector2 postTeleportVelocity = new Vector2(0.0f, 10.0f);
	[SerializeField] private Vector2 postFireVelocity = new Vector2(0.0f, 10.0f);
	#endregion


	#region GAME EVENTS
	[Header("Game Events")]
	// Should call DeathFade.StartDeathFadeCoroutine()
	public UnityEvent touchedHazard;
	public UnityEvent startedRunning;
	public UnityEvent stopMoving;
	#endregion


	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }


	private void Start() {
		characterRigi = GetComponent<Rigidbody2D>();
		playerColl = GetComponent<Collision>();
		animator = GetComponent<Animator>();

		float tempGravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		_DefaultGravityMultiplier = tempGravity / Physics2D.gravity.y;

		maxJumpVelocity = Mathf.Abs(Physics2D.gravity.y * _DefaultGravityMultiplier) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * _DefaultGravityMultiplier) * minJumpHeight);
	}


	void Update() {
		playerVelocity = characterRigi.velocity;

		wallDirX = playerColl.collInfo.onWallLeft ? -1 : 1;

		if (!alive) {
			return;
		}

		GetInput();

		if (_CanGrabWall) {
			HandleWallGrab();
		}

		HandleJump();

		if (playerColl.collInfo.touchedHazard) {
			alive = false;

			if (slowmoInEffect) {
				StopCoroutine(slowmoEffectCoroutine);
				Time.timeScale = 1.0f;
				Time.fixedDeltaTime = 0.02F;
			}

			touchedHazard.Invoke();
		}

		ApplyGravityScale();

		// Determine the final player velocity
		if (canMove) {
			if (_CanGrabWall && grabWall) {
				// If the player is grabbing a wall then lock x movement and allow them to move up or down the wall
				playerVelocity = new Vector2(0f, moveY * wallGrabMovementSpeed);
			}
			else {
				DeterminePlayerHorizontalVelocity();

				if (_CanSlideAgainstWall) {
					HandleWallSlide();
				}
			}
		}

		// Press B
		if (Input.GetButtonDown("Fire2") && teleportProjectileReference) {
			teleportProjectileReference.GetComponent<MoveInDirection>().direction *= -1;
		}

		// Press X
		if (Input.GetButtonDown("Fire3") && playerColl.collInfo.onGround == false && Input.GetAxisRaw("Vertical") < 0) {
			float stompVelocity = Mathf.Abs(playerVelocity.y) * -10f;
			playerVelocity = new Vector2(playerVelocity.x, stompVelocity);
			Time.timeScale = 1;
		}
		characterRigi.velocity = playerVelocity;

		HandlePlayerVisuals();
	}


	private void GetInput () {
		if (canMove) {
			GetMovementInput();
		}

		GetJumpInput();

		GetAimInput();
	}


	private void GetMovementInput () {
		Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
		//if (movementInput.magnitude < controllerDeadZone)
		//	movementInput = Vector2.zero;

		moveX = movementInput.x;
		moveY = movementInput.y;
	}


	private void GetJumpInput () {
		jumpInputDown = Input.GetButtonDown("Jump") ? true : false;
		jumpInputUp = Input.GetButtonUp("Jump") ? true : false;
	}


	private void GetAimInput () {
		bool _LT = Input.GetAxis("LT") == 0 ? false : true;
		bool _RT = Input.GetAxis("RT") == 0 ? false : true;

		bool haveProjectileReference = (teleportProjectileReference != null) ? true : false;
		if (!haveProjectileReference && !hasTeleported) {
			hasFired = false;
			hasTeleported = false;
			projectileState = _FIRE;
			playerWaitingToTeleport = false;
			canMove = true;

			if (playerFlashEffect != null) {
				StopCoroutine(playerFlashEffect);
				
				if (gameObject.GetComponent<SpriteRenderer>().color != _DefaultCharacterColor) {
					gameObject.GetComponent<SpriteRenderer>().color = _DefaultCharacterColor;
				}
			}
		}

		bool isAiming = false;

		// Determine if the player can aim ----------------------------------------------
		if (_LT && !haveProjectileReference) {
			aimingVector = new Vector2(Input.GetAxis("Right Joystick X"), Input.GetAxis("Right Joystick Y"));

			if (aimingVector != Vector2.zero) {
				aimingLineRenderer.SetPosition(0, transform.position);
				aimingLineRenderer.SetPosition(1, ((aimingVector.normalized * 7) + (Vector2)transform.position));
				isAiming = true;
			}
		}
		else {
			aimingLineRenderer.SetPosition(0, transform.position);
			aimingLineRenderer.SetPosition(1, transform.position);
		}

		// Determine if we should apply slowmo ------------------------------------------
		// If the player is on the ground the reset the canApplySlowMo effect
		if (playerColl.collInfo.onGround) {
			canApplySlowMo = true;
		}
		
		if (_LT && playerColl.collInfo.inAir && canApplySlowMo) {
			slowmoEffectCoroutine = StartCoroutine(SlowMotionEffect());
		}
		else if (!_LT) {
			ResetTimeScale();
		}

		// Determine if we should shoot the projectile or teleport ----------------------
		if (_RT && isAiming && !haveProjectileReference && projectileState == _FIRE) {
			teleportProjectileReference = GameObject.Instantiate(teleportProjectilePrefab, transform.position, Quaternion.identity);
			teleportProjectileReference.GetComponent<MoveInDirection>().direction = aimingVector.normalized;
			teleportProjectileReference.GetComponent<MoveInDirection>().movementSpeed = teleportProjectileMovementSpeed;
			teleportProjectileReference.GetComponent<MoveInDirection>().setLifeSpan = true;
			teleportProjectileReference.GetComponent<MoveInDirection>().lifeSpawn = 5.0f;

			if (playerColl.collInfo.inAir) {
				playerVelocity = Vector2.zero;
			}

			ResetTimeScale();
			canMove = false;
			playerFlashEffect = StartCoroutine(PlayerFlash());
			playerWaitingToTeleport = true;
			hasFired = true;
		}
		else if (_RT && haveProjectileReference && projectileState == _TELEPORT) {
			this.transform.position = teleportProjectileReference.transform.position;
			playerVelocity = postTeleportVelocity;
			Destroy(teleportProjectileReference);

			canMove = true;
			hasTeleported = true;
			canApplySlowMo = true;
			playerWaitingToTeleport = false;

			if (playerFlashEffect != null) {
				StopCoroutine(playerFlashEffect);
				if (gameObject.GetComponent<SpriteRenderer>().color != _DefaultCharacterColor) {
					gameObject.GetComponent<SpriteRenderer>().color = _DefaultCharacterColor;
				}
			}
		}

		// Look for when the Right Trigger has been lifted
		if (!_RT && hasFired) {
			projectileState = _TELEPORT;
			hasFired = false;
		}
		else if (!_RT && hasTeleported) {
			projectileState = _FIRE;
			hasTeleported = false;
		}
	}

	private void ResetTimeScale() {
		if (slowmoInEffect) {
			StopCoroutine(slowmoEffectCoroutine);
		}
		Time.timeScale = 1.0f;
		Time.fixedDeltaTime = 0.02F;
	}

	private void HandleWallGrab () {
		if (_CanGrabWall) {
			bool grabWallInput = Input.GetAxis("LT") == 0 ? false : true;

			if (playerColl.collInfo.onWall && grabWallInput && !playerColl.collInfo.onGround) {
				wallGrabStamina += Time.deltaTime;
			}
			else if (playerColl.collInfo.onGround) {
				wallGrabStamina = 0.0f;
			}

			if (wallGrabStamina >= wallGrabStaminaMax) {
				grabWall = true;
				return;
			}
		}

		grabWall = false;
	}


	private void HandleJump () {
		DetermineIfCanCoyoteJump();

		if (jumpInputDown ||
			(applyJumpQueue && playerColl.collInfo.onGround)) {
			DetermineJumpType();
		}

		if (jumpInputUp) {
			if (playerVelocity.y > minJumpVelocity) {
				playerVelocity = new Vector2(playerVelocity.x, minJumpVelocity);
			}
		}
	}


	private void DetermineIfCanCoyoteJump () {
		if (!playerColl.collInfo.onGround && !playerColl.collInfo.onWall &&
			!jumping && playerVelocity.y < 0f &&
			(Time.time <= (playerColl.collInfo.timeLeftGround + coyoteJumpTimer))) {
			canCoyoteJump = true;
		}
		else {
			canCoyoteJump = false;
		}
	}


	private void DetermineJumpType () {
		Vector2 jumpVelocity;

		// STANDARD GROUND JUMP
		if (playerColl.collInfo.onGround) {
			jumpVelocity = new Vector2(playerVelocity.x, maxJumpVelocity);
			DoJump(jumpVelocity);
		}
		// COYOTE JUMP
		else if (canCoyoteJump) {
			jumpVelocity = new Vector2(playerVelocity.x, maxJumpVelocity);
			DoJump(jumpVelocity);
		}
		// SOME SORT OF WALL JUMP
		else if (_CanWallJump && playerColl.collInfo.onWall && !playerColl.collInfo.onGround) {
			int wallDirX = playerColl.collInfo.onWallLeft ? -1 : 1;

			// JUMP VERTICALLY UP WALL
			if (grabWall && 
				(moveX == 0 || (moveX < 0f && playerColl.collInfo.onWallLeft) || (moveX > 0f && playerColl.collInfo.onWallRight))) {
				StopCoroutine(DisableMovementWallJumpUp(0));
				StartCoroutine(DisableMovementWallJumpUp(wallJumpVerticalControlDelay));

				jumpVelocity = new Vector2(wallJumpVertical.x, wallJumpVertical.y);
				DoJump(jumpVelocity);
			}
			// JUMP UP A WALL
			else if ((wallDirX == -1 && moveX <0f) || (wallDirX == 1 && moveX > 0f)) {
				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpUpControlDelay));

				Vector2 jumpAway = new Vector2(wallJumpUp.x * -wallDirX, wallJumpUp.y);
				DoJump(jumpAway);
			}
			// JUMP AWAY FROM A WALL
			else {
				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpAwayControlDelay));

				Vector2 jumpAway = new Vector2(wallJumpAway.x * -wallDirX, wallJumpAway.y);
				DoJump(jumpAway);
			}
		}
		// START JUMP QUEUE
		else if (!applyJumpQueue) {
			StartCoroutine(JumpQueueTimer(jumpQueueTimer));
		}
	}


	private void DoJump (Vector2 jumpVelocity) {
		playerVelocity = jumpVelocity;
		jumping = true;
		playerColl.collInfo.onGround = false;
	}


	IEnumerator DisableMovementWallJumpOff (float time) {
		canMove = false;
		applyWallSlide = false;
		yield return new WaitForSeconds(time);
		applyWallSlide = true;
		canMove = true;
	}


	IEnumerator DisableMovementWallJumpUp (float time) {
		canMove = false;
		grabWall = false;
		applyWallSlide = false;
		yield return new WaitForSeconds(time);
		applyWallSlide = true;
		grabWall = true;
		canMove = true;
	}


	IEnumerator JumpQueueTimer (float time) {
		applyJumpQueue = true;
		yield return new WaitForSeconds(time);
		applyJumpQueue = false;
	}


	private void ApplyGravityScale () {
		if (playerWaitingToTeleport) {
			characterRigi.gravityScale = 0.2f;
		}
		else if (playerVelocity.y < 0) {
			characterRigi.gravityScale = fallMultiplier;
			applyWallSlide = true;
		}
		else if (playerVelocity.y > 0) {
			characterRigi.gravityScale = _DefaultGravityMultiplier;
			applyWallSlide = false;
		}
		else if (_CanGrabWall) {
			if (grabWall && playerColl.collInfo.onWall && !playerColl.collInfo.onGround) {
				characterRigi.gravityScale = 0f;
			}
		}
	}


	private void DeterminePlayerHorizontalVelocity () {
		// Move the character by finding the target velocity
		Vector3 targetVelocity = new Vector2(moveX * movementSpeed, playerVelocity.y);

		// ACCELERATION
		if (targetVelocity.x != 0) {
			playerVelocity = Vector3.SmoothDamp(playerVelocity, targetVelocity, ref velocitySmoothing,
					(playerColl.collInfo.onGround) ? accelerationTimeGrounded : accelerationTimeAirborne);
		}
		// DECELERATION
		else {
			playerVelocity = Vector3.SmoothDamp(playerVelocity, targetVelocity, ref velocitySmoothing,
					(playerColl.collInfo.onGround) ? decelerationTimeGrounded : decelerationTimeAirborne);
		}
	}


	private void HandleWallSlide () {
		if (!playerColl.collInfo.onGround && playerColl.collInfo.onWall && playerVelocity.y <= 0.0f) {
			//Debug.Log(moveX + "   " + wallDirX);
			if ((moveX == wallDirX) && applyWallSlide) {
				ApplyWallSlide();
				return;
			}
			else if ((moveX != wallDirX) && (moveX != 0.0f)) {
				ApplyStickyWallDelay();
				return;
			}
		}

		timeToWallUnstick = wallStickTime;
	}


	private void ApplyWallSlide () {
		playerVelocity = new Vector2(0.0f, -slideRate);
	}


	private void ApplyStickyWallDelay () {
		if (timeToWallUnstick > 0) {
			playerVelocity = new Vector2(0.0f, -slideRate);

			timeToWallUnstick -= Time.deltaTime;
		}
	}


	private void HandlePlayerVisuals () {
		// ANIMATOR CURRENTLY DISABLED
		//if (playerVelocity.x != 0) {
		//	animator.SetBool("running", true);
		//	startedRunning.Invoke();
		//}
		//else {
		//	animator.SetBool("running", false);
		//	stopMoving.Invoke();
		//}

		HandlePlayerSpriteFlip();
	}


	private void HandlePlayerSpriteFlip()
	{
		// If the input is moving the player right and the player is facing left...
		if (moveX > 0 && !facingRight) {
			FlipPlayerSprite();
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (moveX < 0 && facingRight) {
			FlipPlayerSprite();
		}
	}


	private void FlipPlayerSprite() {
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}


	public void ResetPlayer () {
		jumping = false;
		alive = true;
		velocitySmoothing = Vector2.zero;
		characterRigi.velocity = Vector2.zero;
		playerVelocity = Vector2.zero;
	}

	IEnumerator SlowMotionEffect() {
		canApplySlowMo = false;
		slowmoInEffect = true;
		Time.timeScale = 0.01f;
		Time.fixedDeltaTime = 0.02F * Time.timeScale;

		float t = 0.0f;
		float counter = 0.0f;

		while (counter < slowmoLength) {
			t += Time.deltaTime / slowmoLength;
			counter += Time.deltaTime;

			Time.timeScale = Mathf.Lerp(0.01f, 1.0f, t);
			Time.fixedDeltaTime = 0.02F * Time.timeScale;

			yield return null;
		}

		slowmoInEffect = false;
	}


	IEnumerator PlayerFlash() {
		#region Visual Fade / Flash Despawn Phase
		float t = 0.0f;
		float flashRate = 0.1f;
		float flashCounter = 0.0f;
		bool applyFlashColor = true;
		SpriteRenderer playerSR = gameObject.GetComponent<SpriteRenderer>();

		while (true) {
			flashCounter += Time.deltaTime;

			if (flashCounter >= flashRate) {
				flashCounter = 0.0f;
				
				if (applyFlashColor) {
					playerSR.color = Color.white;
					applyFlashColor = false;
				}
				else {
					playerSR.color = _DefaultCharacterColor;
					applyFlashColor = true;
				}
			}

			yield return null;
		}
		#endregion
	}
}
