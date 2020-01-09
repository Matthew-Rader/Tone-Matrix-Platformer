using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
public class Collision : MonoBehaviour
{
	public CollisionInfo collInfo;

	private Collider2D collider;

	private bool groundedLastFrame = false;

	[Header("Collision Layers")]
	[SerializeField] private LayerMask whatIsGround;
	[SerializeField] private LayerMask whatIsWall;
	[SerializeField] private LayerMask whatIsHazard;

	[Header("Collision check points")]
	private Vector2 bottomOffset;
	private Vector2 aboveOffset;
	private Vector2 leftOffset;
	private Vector2 rightOffset;
	private Vector2 widthOverlapBox;
	private Vector2 heightOverlapBox;

	[Header("Events")]
	[Space]
	public UnityEvent OnLandEvent;

	void Start()
	{
		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		collider = GetComponent<BoxCollider2D>();

		Bounds bounds = collider.bounds;
		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;
		float overlapBoxWidth = boundsWidth - 0.02f;
		float overlapBoxHeight = boundsHeight - 0.02f;

		bottomOffset = new Vector2(0.0f, -(bounds.size.y / 2.0f));
		aboveOffset = new Vector2(0.0f, (bounds.size.y / 2.0f));
		leftOffset = new Vector2(-(bounds.size.x / 2.0f), 0.0f);
		rightOffset = new Vector2((bounds.size.x / 2.0f), 0.0f);

		heightOverlapBox = new Vector2(0.02f, overlapBoxHeight);
		widthOverlapBox = new Vector2(overlapBoxWidth, 0.02f);
	}

	void FixedUpdate () {
		bool wasGrounded = collInfo.onGround;

		collInfo.Reset();

		// Check for ground collision
		collInfo.onGround = Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, widthOverlapBox, 0.0f, whatIsGround);

		if (!collInfo.onGround && groundedLastFrame) {
			groundedLastFrame = false;
			collInfo.timeLeftGround = Time.time;
		}
		groundedLastFrame = collInfo.onGround;

		if (!wasGrounded && collInfo.onGround)
			OnLandEvent.Invoke();

		// Check for left wall collision
		collInfo.onWallLeft = Physics2D.OverlapBox((Vector2)transform.position + leftOffset, heightOverlapBox, 0.0f, whatIsWall);

		// Check for right wall collision
		collInfo.onWallRight = Physics2D.OverlapBox((Vector2)transform.position + rightOffset, heightOverlapBox, 0.0f, whatIsWall);

		collInfo.onWall = collInfo.onWallLeft || collInfo.onWallRight ? true : false;

		// Check to see if we hit a hazard
		collInfo.touchedHazard = (Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, widthOverlapBox, 0.0f, whatIsHazard) ||
								  Physics2D.OverlapBox((Vector2)transform.position + aboveOffset, widthOverlapBox, 0.0f, whatIsHazard) ||
								  Physics2D.OverlapBox((Vector2)transform.position + leftOffset, heightOverlapBox, 0.0f, whatIsHazard) ||
								  Physics2D.OverlapBox((Vector2)transform.position + rightOffset, heightOverlapBox, 0.0f, whatIsHazard));
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;

		Gizmos.DrawWireCube(transform.position + (Vector3)bottomOffset, widthOverlapBox);
		Gizmos.DrawWireCube(transform.position + (Vector3)aboveOffset, widthOverlapBox);
		Gizmos.DrawWireCube(transform.position + (Vector3)leftOffset, heightOverlapBox);
		Gizmos.DrawWireCube(transform.position + (Vector3)rightOffset, heightOverlapBox);
	}

	public struct CollisionInfo {
		public bool onGround;
		public bool onWall;
		public bool onWallLeft;
		public bool onWallRight;
		public bool touchedHazard;
		public float timeLeftGround;

		public void Reset () {
			onGround = onWall = false;
			onWallLeft = onWallRight = false;
			touchedHazard = false;
		}
	}
}
