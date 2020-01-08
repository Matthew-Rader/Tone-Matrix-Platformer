using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collision))]
public class CharacterController2D : MonoBehaviour
{
	// PRIVATE ----------------------------------------------------------------
	private Collision playerColl;
	private Rigidbody2D characterRigi;
	private bool facingRight = true;
	private Vector3 velocitySmoothing = Vector3.zero;
	private float moveX;
	private float moveY;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private bool jumpInputDown = false;
	private bool jumpInputUp = false;
	private bool jumping = false;
	private bool grabWall = false;
	private bool applyWallSlide = true;
	private bool canMove = true;
	private float _DefaultGravityMultiplier;
	private bool applyJumpQueue = false;

	// SERIALIZED PRIVATE -----------------------------------------------------
	[SerializeField] private GameManager gameManager;

	[Header("Movement")]
	[SerializeField] private float runSpeed = 40f;
	[SerializeField] private float accelerationTimeAirborne = 0.1f;
	[SerializeField] private float decelerationTimeAirborne = 0.1f;
	[SerializeField] private float accelerationTimeGrounded = 0.15f;
	[SerializeField] private float decelerationTimeGrounded = 0.05f;
	[SerializeField] private float controllerDeadZone = 0.25f;

	[Header("Jump Parameters")]
	[SerializeField] private float maxJumpHeight = 2.25f;
	[SerializeField] private float minJumpHeight = 1.0f;
	[SerializeField] private float timeToJumpApex = 0.25f;
	[SerializeField] private float jumpQueueTimer = 0.15f;
	[SerializeField] private float coyoteJumpTimer = 1.0f;


	[SerializeField] private float wallJumpForce = 13f;
	[SerializeField] private float wallJumpVerticalControlDelay = 0.1f;
	[SerializeField] private float wallJumpAwayControlDelay = 0.15f;
	[Tooltip("0 will result in a vertical jump and 1 ~45*")]
	[Range (0, 1)] [SerializeField] private float wallJumpAwayAngleModifier = 0.5f;
	private bool canCoyoteJump = false; 

	[Header("Better Jumping Gravity Multiplier")]
	[SerializeField] private float fallMultiplier = 3.5f;
	[SerializeField] private float lowJumpMultiplier = 5f;

	[Header("Wall Mechanics")]
	[SerializeField] private bool _CanGrabWall = false;
	[SerializeField] private float slideRate = 2f;
	[SerializeField] private float wallGrabStaminaMax = 3.0f;
	[SerializeField] private float wallStickTime = 0.15f;
	private float timeToWallUnstick;
	private float wallGrabStamina = 0.0f;
	private bool wallGrabDepleted = false;

	[Header("Game Events")]
	// Should call DeathFade.StartDeathFadeCoroutine()
	public UnityEvent touchedHazard;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	private void Start()
	{
		characterRigi = GetComponent<Rigidbody2D>();
		playerColl = GetComponent<Collision>();

		float tempGravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		_DefaultGravityMultiplier = tempGravity / Physics2D.gravity.y;

		maxJumpVelocity = Mathf.Abs(Physics2D.gravity.y * _DefaultGravityMultiplier) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Physics2D.gravity.y * _DefaultGravityMultiplier) * minJumpHeight);

		transform.position = gameManager.currentReSpawnPoint.transform.position;
	}

	void Update()
	{
		if (canMove) {
			GetInput();
		}

		DetermineIfCanCoyoteJump();

		if (_CanGrabWall) {
			WallGrabStamina();
		}

		if (jumpInputDown ||
			(applyJumpQueue && (playerColl.collInfo.onGround || ((playerColl.collInfo.onWallLeft || playerColl.collInfo.onWallRight) && !playerColl.collInfo.onGround)))) {
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

	private void GetInput () {
		Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		if (movementInput.magnitude < controllerDeadZone)
			movementInput = Vector2.zero;

		moveX = movementInput.x;
		moveY = movementInput.y;

		grabWall = Input.GetAxis("LT") == 0 ? false : true;

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

	private void WallGrabStamina () {
		if (playerColl.collInfo.onWall && grabWall && !playerColl.collInfo.onGround) {
			wallGrabStamina += Time.deltaTime;
		}
		else if (playerColl.collInfo.onGround) {
			wallGrabStamina = 0.0f;
		}

		if (wallGrabStamina >= wallGrabStaminaMax) {
			wallGrabDepleted = true;
			GetComponent<SpriteRenderer>().color = new Color(255, 0, 0);
		}
		else {
			wallGrabDepleted = false;
			GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
		}
	}

	// Determines which type of jump that needs to be performed
	private void HandleJump () {
		// Standard ground jump
		if (playerColl.collInfo.onGround) {
			playerColl.collInfo.onGround = false;
			DoJump(Vector2.up, maxJumpVelocity);
		}
		// Coyote Jump
		else if (canCoyoteJump) {
			DoJump(Vector2.up, maxJumpVelocity);
		}
		// Some sort of wall jump
		else if (playerColl.collInfo.onWall && !playerColl.collInfo.onGround) {
			// Vertical jump up a wall
			if (grabWall && !wallGrabDepleted && (moveX == 0 || (moveX < 0f && playerColl.collInfo.onWallLeft) || (moveX > 0f && playerColl.collInfo.onWallRight))) {
				StopCoroutine(DisableMovementWallJumpUp(0));
				StartCoroutine(DisableMovementWallJumpUp(wallJumpVerticalControlDelay));

				DoJump(Vector2.up, wallJumpForce);
			}
			// Jump away from a wall
			else {
				StopCoroutine(DisableMovementWallJumpOff(0));
				StartCoroutine(DisableMovementWallJumpOff(wallJumpAwayControlDelay));

				//wallSide is -1 for left and 1 for right
				Vector2 wallDir = playerColl.collInfo.onWallRight ? Vector2.left : Vector2.right;
				DoJump((Vector2.up + wallDir * wallJumpAwayAngleModifier), wallJumpForce);
			}
		}
		else if (!applyJumpQueue) {
			StartCoroutine(JumpQueueTimer(jumpQueueTimer));
		}
	}

	private void DoJump (Vector2 dir, float jumpForce) {
		characterRigi.velocity = new Vector2(characterRigi.velocity.x, 0.0f);
		characterRigi.velocity += dir * jumpForce;
		jumping = true;
	}

	public void Move()
	{
		if (playerColl.collInfo.touchedHazard) {
			touchedHazard.Invoke();
		}

		// If the player is grabbing a wall then lock x movement and allow them to move up or down the wall
		if (_CanGrabWall) {
			if (playerColl.collInfo.onWall && grabWall && !playerColl.collInfo.onGround && canMove && !wallGrabDepleted) {
				characterRigi.velocity = new Vector2(0f, moveY * runSpeed);
			}
		}
		else
		{
			// Move the character by finding the target velocity
			Vector3 targetVelocity = new Vector2(moveX * runSpeed, characterRigi.velocity.y);

			if (targetVelocity.x != 0) {
					characterRigi.velocity = Vector3.SmoothDamp(characterRigi.velocity, targetVelocity, ref velocitySmoothing,
						(playerColl.collInfo.onGround) ? accelerationTimeGrounded : accelerationTimeAirborne);
			}
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

	// Affect gravity scale for better jumping
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
			if (grabWall && playerColl.collInfo.onWall && !playerColl.collInfo.onGround && !wallGrabDepleted) {
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
		velocitySmoothing = Vector2.zero;
		characterRigi.velocity = Vector2.zero;
	}

	IEnumerator JumpQueueTimer (float time) {
		applyJumpQueue = true;
		yield return new WaitForSeconds(time);
		applyJumpQueue = false;
	}
}
