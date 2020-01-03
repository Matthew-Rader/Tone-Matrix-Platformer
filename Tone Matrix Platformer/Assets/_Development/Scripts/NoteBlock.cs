using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteBlock : MonoBehaviour
{
	AudioSource noteAudio;

	SpriteRenderer spriteRenderer;
	Color nodeBlockEnabled = Color.white;
	Color nodeBlockDisabled = Color.grey;

	[HideInInspector]
	public bool blockEnabled = false;

	void Awake () { 
		noteAudio = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		spriteRenderer.color = nodeBlockDisabled;
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			blockEnabled = !blockEnabled;
			spriteRenderer.color = blockEnabled ? nodeBlockEnabled : nodeBlockDisabled;
		}
	}

	public void PlayAudio () {
		if (noteAudio != null) {
			noteAudio.Play();
		}
	}
}
