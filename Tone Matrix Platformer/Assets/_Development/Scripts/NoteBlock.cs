using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class NoteBlock : MonoBehaviour
{
	const string _SortingLayerDisabled = "NoteBlock_Disabled";
	const string _SortingLayerEnabled = "NoteBlock_Enabled";

	AudioSource noteAudio;
	SpriteRenderer spriteRenderer;
	Light2D _light;

	Color nodeBlockEnabled = new Color(218.0f/255.0f, 218.0f/255.0f, 218.0f/255.0f, 1.0f);
	Color nodeBlockPlaying = Color.white;
	Color nodeBlockDisabled = new Color(35.0f/255.0f, 35.0f/255.0f, 35.0f/255.0f, 1.0f);

	[HideInInspector]
	public bool blockEnabled = false;

	void Awake () { 
		noteAudio = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		_light = GetComponent<Light2D>();

		spriteRenderer.color = nodeBlockDisabled;
		spriteRenderer.sortingLayerName = _SortingLayerDisabled;

		_light.enabled = true;
		_light.intensity = 0;
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			blockEnabled = !blockEnabled;
			spriteRenderer.color = blockEnabled ? nodeBlockEnabled : nodeBlockDisabled;
			spriteRenderer.sortingLayerName = blockEnabled ? _SortingLayerEnabled : _SortingLayerDisabled;
		}
	}

	public void PlayAudio () {
		if (noteAudio != null) {
			noteAudio.Play();
			StartCoroutine(LightFadeEffect());
		}
	}

	public void SetNote (AudioClip inClip) {
		noteAudio.clip = inClip;
	}

	IEnumerator LightFadeEffect () {
		float t;
		float counter;

		#region FadeIn 
		t = 0.0f;
		counter = 0.0f;

		while (counter < 0.15f) {
			t += Time.deltaTime / 0.15f;
			counter += Time.deltaTime;

			_light.intensity = Mathf.Lerp(0, 15, t);
			spriteRenderer.color = Color.Lerp(nodeBlockEnabled, nodeBlockPlaying, t);

			yield return null;
		}
		#endregion

		#region FadeOut
		t = 0.0f;
		counter = 0.0f;

		while (counter < 0.3f) {
			t += Time.deltaTime / 0.3f;
			counter += Time.deltaTime;

			_light.intensity = Mathf.Lerp(15, 0, t);
			spriteRenderer.color = Color.Lerp(nodeBlockPlaying, nodeBlockEnabled, t);

			yield return null;
		}
		#endregion
	}
}
