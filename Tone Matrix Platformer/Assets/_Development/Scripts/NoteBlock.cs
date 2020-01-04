using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class NoteBlock : MonoBehaviour
{
	const string _SortingLayerDisabled = "NoteBlock_Disabled";
	const string _SortingLayerEnabled = "NoteBlock_Enabled";

	private AudioSource noteAudio;
	private SpriteRenderer spriteRenderer;
	private Light2D _light;
	[SerializeField] private SelectableBlockCount selectableBlockCount;

	// Potential Note-Block colors
	private Color nodeBlockEnabled = new Color(218.0f/255.0f, 218.0f/255.0f, 218.0f/255.0f, 1.0f);
	private Color nodeBlockPlaying = Color.white;
	private Color nodeBlockDisabled = new Color(35.0f/255.0f, 35.0f/255.0f, 35.0f/255.0f, 1.0f);

	[HideInInspector]
	public bool blockEnabled = false;

	private Coroutine _lightFadeEffect;
	private Coroutine _spriteColorEffect;

	void Awake () { 
		noteAudio = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		_light = GetComponent<Light2D>();

		spriteRenderer.color = nodeBlockDisabled;
		spriteRenderer.sortingLayerName = _SortingLayerDisabled;

		gameObject.layer = LayerMask.NameToLayer(_SortingLayerDisabled);

		_light.enabled = true;
		_light.intensity = 0;
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0)) {
			// Enabled the block
			if (!blockEnabled && selectableBlockCount.CurrentSelectCountLessThanMax()) {
				blockEnabled = true;
				spriteRenderer.color = nodeBlockEnabled;
				spriteRenderer.sortingLayerName = _SortingLayerEnabled;
				gameObject.layer = LayerMask.NameToLayer(_SortingLayerEnabled);

				selectableBlockCount.currentSelectedCount += 1;
			}
			// Disable the block
			else if (blockEnabled) {
				blockEnabled = false;

				if (_spriteColorEffect != null)
					StopCoroutine(_spriteColorEffect);

				spriteRenderer.color = nodeBlockDisabled;
				spriteRenderer.sortingLayerName = _SortingLayerDisabled;
				gameObject.layer = LayerMask.NameToLayer(_SortingLayerDisabled);

				selectableBlockCount.currentSelectedCount -= 1;
			}
		}
	}

	public void PlayAudio () {
		if (noteAudio != null) {
			noteAudio.Play();
			_lightFadeEffect = StartCoroutine(LightFadeEffect());
			_spriteColorEffect = StartCoroutine(SpriteColorEffect());
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

			yield return null;
		}
		#endregion
	}

	IEnumerator SpriteColorEffect () {
		float t;
		float counter;

		#region FadeIn 
		t = 0.0f;
		counter = 0.0f;

		while (counter < 0.15f) {
			t += Time.deltaTime / 0.15f;
			counter += Time.deltaTime;

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

			spriteRenderer.color = Color.Lerp(nodeBlockPlaying, nodeBlockEnabled, t);

			yield return null;
		}
		#endregion
	}
}
