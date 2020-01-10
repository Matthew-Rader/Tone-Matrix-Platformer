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

	#region  MOVEMENT VARIABLES
	[Header("Movement")]
	[SerializeField] private float runSpeed = 40f;
	[SerializeField] private float accelerationTimeAirborne = 0.1f;
	[SerializeField] private float decelerationTimeAirborne = 0.1f;
	[SerializeField] private float accelerationTimeGrounded = 0.15f;
	[SerializeField] private float decelerationTimeGrounded = 0.05f;
	[SerializeField] private float controllerDeadZone = 0.25f;
	private bool canMove = true;
	private bool facingRight = true;
	private Vector3 velocitySmoothing = Vector3.zero;
	private float moveX;
	private float moveY;
	#endregion

	#region JUMP SETTINGS AND VARIABLES
	[Header("Jumping")]
	[SerializeField] private float maxJumpHeight = 2.25f;
	[SerializeField] private float minJumpHeight = 1.0f;
	[SerializeField] private float timeToJumpApex = 0.25f;
	[Tooltip("Better Jumping Gravity Multiplier")]
	[SerializeField] private float fallMultiplier = 3.5f;
	[SerializeField] private Vector2 wallJumpVertical = new Vector2(2.0f, 10.0f);
	[SerializeField] private Vector2 wallJumpUp = new Vector2(6.0f, 25.0f);
	[SerializeField] private Vector2 wallJumpAway = new Vector2(20.0f, 10.0f);
	[SerializeField] private float wallJumpVerticalControlDelay = 0.1f;
	[SerializeField] private float wallJumpAwayControlDelay = 0.15f;
	[SerializeField] private float wallJumpUpControlDelay = 0.15f;
	[SerializeField] private float jumpQueueTimer = 0.15f;
	[SerializeField] private float coyoteJumpTimer = 1.0f;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private bool jumpInputDown = false;
	private bool jumpInputUp = false;
	private bool jumping = false;
	private bool applyJumpQueue = false;
	private bool canCoyoteJump = false;
	private float _DefaultGravityMultiplier;
	#endregion

	#region WALL MECHANIC VARIABLES
	[Header("Wall Mechanics")]
	[SerializeField] private bool _CanGrabWall = false;
	[SerializeField] private float slideRate = 2f;
	[SerializeField] private float wallGrabStaminaMax = 3.0f;
	[SerializeField] private float wallStickTime = 0.15f;
	private float timeToWallUnstick;
	private float wallGrabStamina = 0.0f;
	private bool grabWall = false;
	private bool applyWallSlide = true;
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

	private void Start()
	{
		characterRigi = GetComponent<Rigidbody2D>();
		playerColl = GetComponent<Collision>();
		animator = GetComponent<Animator>();

		float tempGravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		_DefaultGravityMultiplier = tempGravity / Physics2D.gravity.y;

		maxJumpVelocity = Mathf.Abs(Physics2D.gravity.y * _DefaultGravityMultiplier) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * _DefaultGravityMultiplier) * minJumpHeight);
	}

	void Update()
	{
		if (!alive) {
			return;
		}

		if (canMove) {
			GetMovementInput();
		}

		GetJumpInput();

		DetermineIfCanCoyoteJump();

		if (_CanGrabWall) {
			HandleWallGrab();
		}

		if (jumpInputDown ||
			(applyJumpQueue && playerColl.collInfo.onGround)) {
			HandleJump();
		}

		if (jumpInputUp) {
			if (characterRigi.velocity.y > minJumpVelocity) {
				characterRigi.velocity = new Vector2(characterRigi.velocity.x, minJumpVelocity);
			}
		}

		Move();

		ApplyGravityScale();
	}

	private void GetMovementInput () {
		Vector2 movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		if (movementInput.magnitude < controllerDeadZone)
			movementInput = Vector2.zero;

		moveX = movementInput.x;
		moveY = movementInput.y;

		if (moveX != 0) {
			animator.SetBool("running", true);
			startedRunning.Invoke();
		}
		else {
			animator.SetBool("running", false);
			stopMoving.Invoke();
		}
	}

	private void GetJumpInput () {
		jumpInputDown = Input.GetButtonDown("Jump") ? true : false;
		jumpInputUp = Input.GetButtonUp("Jump") ? true : false;
	}

	private void DetermineIfCanCoyoteJump () {
		if (!playerColl.collInfo.onGround && !playerColl.collInfo.onWall && !jumping && characterRigi.velocity.y < 0f &&
			(Time.time <= (playerColl.collInfo.timeLeftGround + coyoteJumpTimer))) {
			canCoyoteJump = true;
		}
		else {
			canCoyoteJump = false;
		}
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
				GetComponent<SpriteRenderer>().color = new Color(255, 0, 0);
			}
			else {
				grabWall = false;
				GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
			}
		}
		else {
			grabWall = false;
		}
	}

	private void HandleJump () {
		Vector2 jumpVelocity;

		// STANDARD GROUND JUMP ---------------------------------------------------------
		if (playerColl.collInfo.onGround) {
			jumpVelocity = new Vector2(characterRigi.velocity.x, maxJumpVelocity);
			DoJump(jumpVelocity);
		}
		// COYOTE JUMP ------------------------------------------------------------------
		else if (canCoyoteJump) {
			jumpVelocity = new Vector2(characterRigi.velocity.x, maxJumpVelocity);
			DoJump(jumpVelocity);
		}
		// SOME SORT OF WALL JUMP -------------------------------------------------------
		else if (playerColl.collInfo.onWall && !playerColl.collInfo.onGround) {
			int wallDirX = playerColl.collInfo.onWallLeft ? -1 : 1;

			// JUMP VERTICALLY UP WALL --------------------------------------------------
			if (grabWall && 
				(moveX == 0 || (moveX < 0f && playerColl.collInfo.onWallLeft) || (moveX > 0f && playerColl.collInfo.onWallRight))) {
				StopCoroutine(DisableMovementWallJumpUp(0));
				StartCoroutine(DisableMovementWallJumpUp(wallJumpVerticalControlDelay));

				jumpVelocity = new Vector2(wallJumpVertical.x, wallJumpVertical.y);
				DoJump(jumpVelocity);
			}
			// JUMP UP A WALL -----------------------------------------------------------
			else if ((wallDirX == -1 && moveX <0f) || (wallDirX == 1 && moveX > 0f)) {
				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpUpControlDelay));

				Vector2 jumpAway = new Vector2(wallJumpUp.x * -wallDirX, wallJumpUp.y);
				DoJump(jumpAway);
			}
			// JUMP AWAY FROM A WALL ----------------------------------------------------
			else {
				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpAwayControlDelay));

				Vector2 jumpAway = new Vector2(wallJumpAway.x * -wallDirX, wallJumpAway.y);
				DoJump(jumpAway);
			}
		}
		// START JUMP QUEUE -------------------------------------------------------------
		else if (!applyJumpQueue) {
			StartCoroutine(JumpQueueTimer(jumpQueueTimer));
		}
	}

	private void DoJump (Vector2 jumpVelocity) {
		characterRigi.velocity = jumpVelocity;
		jumping = true;
		playerColl.collInfo.onGround = false;
	}

	public void Move()
	{
		if (playerColl.collInfo.touchedHazard) {
			alive = false;
			touchedHazard.Invoke();
		}

		// If the player is grabbing a wall then lock x movement and allow them to move up or down the wall
		if (_CanGrabWall) {
			if (playerColl.collInfo.onWall && grabWall && !playerColl.collInfo.onGround && canMove) {
				characterRigi.velocity = new Vector2(0f, moveY * runSpeed);
			}
		}
		else
		{
			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(moveX * runSpeed, characterRigi.velocity.y);

			// ACCELERATION
			if (targetVelocity.x != 0) {
					characterRigi.velocity = Vector3.SmoothDamp(characterRigi.velocity, targetVelocity, ref velocitySmoothing,
						(playerColl.collInfo.onGround) ? accelerationTimeGrounded : accelerationTimeAirborne);
			}
			// DECELERATION
			else {
				characterRigi.velocity = Vector3.SmoothDamp(characterRigi.velocity, targetVelocity, ref velocitySmoothing,
						(playerColl.collInfo.onGround) ? decelerationTimeGrounded : decelerationTimeAirborne);
			}

			if (!playerColl.collInfo.onGround && playerColl.collInfo.onWall && (moveX != 0f))
				HandleWallSlide();

			HandleStickyWallDelay();
		}

		HandlePlayerSpriteFlip();
	}

	private void HandleWallSlide () {
		if (applyWallSlide) {
			characterRigi.velocity = new Vector2(characterRigi.velocity.x, -slideRate);
		}
	}

	private void HandleStickyWallDelay () {
		if (playerColl.collInfo.onWall && !playerColl.collInfo.onGround && characterRigi.velocity.y < 0) {
			if (timeToWallUnstick > 0) {
				characterRigi.velocity = new Vector2(0.0f, characterRigi.velocity.y);

				int wallDirX = playerColl.collInfo.onWallLeft ? -1 : 1;

				if (moveX != wallDirX && moveX != 0) {
					timeToWallUnstick -= Time.deltaTime;
				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
		}
		else {
			timeToWallUnstick = wallStickTime;
		}
	}

	private void ApplyGravityScale()
	{
		if (characterRigi.velocity.y < 0) {
			characterRigi.gravityScale = fallMultiplier;
			applyWallSlide = true;
		}
		else if (characterRigi.velocity.y > 0) {
			characterRigi.gravityScale = _DefaultGravityMultiplier;
			applyWallSlide = false;
		}
		else if (_CanGrabWall) {
			if (grabWall && playerColl.collInfo.onWall && !playerColl.collInfo.onGround) {
				characterRigi.gravityScale = 0f;
			}
		}
	}

	IEnumerator DisableMovementWallJumpOff(float time)
	{
		canMove = false;
		applyWallSlide = false;
		yield return new WaitForSeconds(time);
		applyWallSlide = true;
		canMove = true;
	}

	IEnumerator DisableMovementWallJumpUp(float time)
	{
		canMove = false;
		grabWall = false;
		applyWallSlide = false;
		yield return new WaitForSeconds(time);
		applyWallSlide = true;
		grabWall = true;
		canMove = true;
	}

	private void HandlePlayerSpriteFlip()
	{
		// If the input is moving the player right and the player is facing left...
		if (moveX > 0 && !facingRight)
		{
			// ... flip the player.
			Flip();
		}
		// Otherwise if the input is moving the player left and the player is facing right...
		else if (moveX < 0 && facingRight)
		{
			// ... flip the player.
			Flip();
		}
	}

	// Flips the player sprite
	private void Flip()
	{
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
	}

	IEnumerator JumpQueueTimer (float time) {
		applyJumpQueue = true;
		yield return new WaitForSeconds(time);
		applyJumpQueue = false;
	}
}
