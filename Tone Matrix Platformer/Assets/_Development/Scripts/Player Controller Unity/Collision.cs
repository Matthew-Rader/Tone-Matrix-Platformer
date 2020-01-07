using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Collision : MonoBehaviour
{
	public CollisionInfo collInfo;

	private bool groundedLastFrame = false;

	[Header("Collision Layers")]
	[SerializeField] private LayerMask whatIsGround;
	[SerializeField] private LayerMask whatIsWall;
	[SerializeField] private LayerMask whatIsHazard;

	[Header("Collision check points")]
	private Vector2 bottomOffset = new Vector2(0f, -0.5f);
	private Vector2 bottomOverlapBox = new Vector2(0.95f, 0.02f);
	private Vector2 aboveOffset = new Vector2(0f, 0.5f);
	private Vector2 aboveOverlapBox = new Vector2(0.95f, 0.02f);
	private Vector2 leftOffset = new Vector2(-0.5f, 0f);
	private Vector2 leftOverlapBox = new Vector2(0.02f, 0.95f);
	private Vector2 rightOffset = new Vector2(0.5f, 0f);
	private Vector2 rightOverlapBox = new Vector2(0.02f, 0.95f);

	[Header("Events")]
	[Space]
	public UnityEvent OnLandEvent;

	void Awake()
	{
		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();
	}

	// Update is called once per frame
	void FixedUpdate () {
		bool wasGrounded = collInfo.onGround;

		// Check for ground collision
		collInfo.onGround = Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, bottomOverlapBox, 0.0f, whatIsGround);

		if (!collInfo.onGround && groundedLastFrame) {
			groundedLastFrame = false;
			collInfo.timeLeftGround = Time.time;
		}
		groundedLastFrame = collInfo.onGround;

		if (!wasGrounded && collInfo.onGround)
			OnLandEvent.Invoke();

		// Check for left wall collision
		collInfo.onWallLeft = Physics2D.OverlapBox((Vector2)transform.position + leftOffset, leftOverlapBox, 0.0f, whatIsWall);

		// Check for right wall collision
		collInfo.onWallRight = Physics2D.OverlapBox((Vector2)transform.position + rightOffset, rightOverlapBox, 0.0f, whatIsWall);

		collInfo.onWall = collInfo.onWallLeft || collInfo.onWallRight ? true : false;

		collInfo.wallSide = collInfo.onWallRight ? -1 : 1;

		// Check to see if we hit a hazard
		collInfo.touchedHazard = (Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, bottomOverlapBox, 0.0f, whatIsHazard) ||
								  Physics2D.OverlapBox((Vector2)transform.position + aboveOffset, aboveOverlapBox, 0.0f, whatIsHazard) ||
								  Physics2D.OverlapBox((Vector2)transform.position + leftOffset, leftOverlapBox, 0.0f, whatIsHazard) ||
								  Physics2D.OverlapBox((Vector2)transform.position + rightOffset, rightOverlapBox, 0.0f, whatIsHazard));
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;

		Gizmos.DrawWireCube(transform.position + (Vector3)bottomOffset, new Vector3(0.95f, 0.05f, 0.0f));
		Gizmos.DrawWireCube(transform.position + (Vector3)leftOffset, new Vector3(0.05f, 0.95f, 0.0f));
		Gizmos.DrawWireCube(transform.position + (Vector3)rightOffset, new Vector3(0.05f, 0.95f, 0.0f));
	}

	public struct CollisionInfo {
		public bool onGround;
		public bool onWall;
		public bool onWallLeft;
		public bool onWallRight;
		public bool touchedHazard;
		public int wallSide;
		public float timeLeftGround;

		public void Reset () {
			onGround = onWall = false;
			onWallLeft = onWallRight = false;
			wallSide = 0;
		}
	}
}
