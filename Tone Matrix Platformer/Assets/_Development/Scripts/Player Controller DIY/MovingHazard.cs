using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingHazard : MonoBehaviour {
	[Header("Platform Path Variables")] // -----------
	[SerializeField] private Vector3[] localWaypoints;
	[SerializeField] private float speed;
	[SerializeField] private bool cyclicPath;
	[SerializeField] private float waitTimeBetweenWaypoints;
	[SerializeField] [Range(1, 3)] private float easeAmount;
	int fromWaypointIndex;
	float percentBetweenWaypoints;
	float nextMoveTime;
	// Store the localWaypoints in global space
	Vector3[] globalWaypoints;

	[SerializeField] private bool displayPath = false;
	[SerializeField] private GameObject pathLineRenderer;

	private void Start () {
		globalWaypoints = new Vector3[localWaypoints.Length];

		for (int i = 0; i < localWaypoints.Length; ++i) {
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}

		// Draw path line
		if (localWaypoints != null && displayPath) {
			DrawPathLine();
		}
	}

	// Update is called once per frame
	void Update () {
		Vector3 velocity = CalculatePlatformMovement();

		transform.Translate(velocity);
	}

	void DrawPathLine () {
		LineRenderer newLine = Instantiate(pathLineRenderer, transform).GetComponent<LineRenderer>(); ;
		newLine.positionCount = 1;

		for (int i = 0; i < localWaypoints.Length; ++i) {
			Vector3 globalWaypointPositionA = (Application.isPlaying) ?
				globalWaypoints[i] : localWaypoints[i] + transform.position;

			int nextWaypoint = (i + 1) % localWaypoints.Length;
			Vector3 globalWaypointPositionB = (Application.isPlaying) ?
				globalWaypoints[nextWaypoint] : localWaypoints[nextWaypoint] + transform.position;

			if ((i < localWaypoints.Length - 1) || (i == localWaypoints.Length - 1 && cyclicPath)) {
				newLine.positionCount += 1;
				newLine.SetPosition(i, globalWaypointPositionA);
				newLine.SetPosition(i + 1, globalWaypointPositionB);
			}
		}
	}

	// This function is an implementation of the equation y = (x^a) / (x^a + (1-x)^a)
	float Ease (float x) {
		float a = easeAmount;
		return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow((1 - x), a));
	}

	Vector3 CalculatePlatformMovement () {
		if (Time.time < nextMoveTime) {
			return Vector3.zero;
		}

		fromWaypointIndex %= globalWaypoints.Length;
		int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length; ;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

		if (percentBetweenWaypoints >= 1) {
			percentBetweenWaypoints = 0;
			fromWaypointIndex++;

			if (!cyclicPath) {
				if (fromWaypointIndex >= globalWaypoints.Length - 1) {
					fromWaypointIndex = 0;
					System.Array.Reverse(globalWaypoints);
				}
			}
			nextMoveTime = Time.time + waitTimeBetweenWaypoints;
		}

		return newPos - transform.position;
	}

	void OnDrawGizmos () {
		if (localWaypoints != null) {
			float size = 0.3f;

			for (int i = 0; i < localWaypoints.Length; ++i) {
				Vector3 globalWaypointPositionA = (Application.isPlaying) ?
					globalWaypoints[i] : localWaypoints[i] + transform.position;

				int nextWaypoint = (i + 1) % localWaypoints.Length;
				Vector3 globalWaypointPositionB = (Application.isPlaying) ?
					globalWaypoints[nextWaypoint] : localWaypoints[nextWaypoint] + transform.position;

				Gizmos.color = Color.red;
				Gizmos.DrawLine(globalWaypointPositionA - Vector3.up * size, globalWaypointPositionA + Vector3.up * size);
				Gizmos.DrawLine(globalWaypointPositionA - Vector3.left * size, globalWaypointPositionA + Vector3.left * size);

				if ((i < localWaypoints.Length - 1) || (i == localWaypoints.Length - 1 && cyclicPath)) {
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(globalWaypointPositionA, globalWaypointPositionB);
				}
			}
		}
	}
}
