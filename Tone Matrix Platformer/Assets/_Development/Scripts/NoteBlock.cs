using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.LWRP;

public class NoteBlock : MonoBehaviour
{
	const string _SortingLayerDisabled = "NoteBlock_Disabled";
	const string _SortingLayerEnabled = "NoteBlock_Enabled";

	[SerializeField] private bool freePlay = false;

	private AudioSource noteAudio;
	private SpriteRenderer spriteRenderer;
	private Light2D _light;
	private SelectableBlockCount selectableBlockCount;
	[SerializeField] private MouseDownState mouseDownState;

	// Potential Note-Block colors
	private Color nodeBlockEnabled = new Color(218.0f/255.0f, 218.0f/255.0f, 218.0f/255.0f, 1.0f);
	private Color nodeBlockPlaying = Color.white;
	private Color nodeBlockDisabled = new Color(35.0f/255.0f, 35.0f/255.0f, 35.0f/255.0f, 1.0f);

	private bool blockEnabled;

	private Coroutine _lightFadeEffect;
	private Coroutine _spriteColorEffect;

	void Awake () { 
		noteAudio = GetComponent<AudioSource>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		_light = GetComponent<Light2D>();

		spriteRenderer.color = nodeBlockDisabled;
		spriteRenderer.sortingLayerName = _SortingLayerDisabled;

		blockEnabled = false;

		gameObject.layer = LayerMask.NameToLayer(_SortingLayerDisabled);

		_light.enabled = true;
		_light.intensity = 0;
	}

	void OnMouseOver () {
		if (Input.GetMouseButtonDown(0) && !GameManager.gamePaused) {
			// Enabled the block
			if (!blockEnabled && 
				(selectableBlockCount.CurrentSelectCountLessThanMax() || freePlay)) {
				EnableNoteBlock();
				mouseDownState.SetState(MouseDownState.State.Enable);
			}
			// Disable the block
			else if (blockEnabled) {
				DisableNoteBlock();
				mouseDownState.SetState(MouseDownState.State.Disable);
			}
		}
		else if (Input.GetMouseButton(0) && !GameManager.gamePaused) {
			// Enabled the block
			if (!blockEnabled && 
				(selectableBlockCount.CurrentSelectCountLessThanMax() || freePlay) && 
				mouseDownState.ShouldEnable()) {
				EnableNoteBlock();
			}
			// Disable the block
			else if (blockEnabled && !mouseDownState.ShouldEnable()) {
				DisableNoteBlock();
			}
		}
	}

	void EnableNoteBlock () {
		blockEnabled = true;
		spriteRenderer.color = nodeBlockEnabled;
		spriteRenderer.sortingLayerName = _SortingLayerEnabled;
		gameObject.layer = LayerMask.NameToLayer(_SortingLayerEnabled);

		if (!freePlay) {
			selectableBlockCount.currentSelectedCount += 1;
		}
	}

	void DisableNoteBlock () {
		blockEnabled = false;

		if (_spriteColorEffect != null)
			StopCoroutine(_spriteColorEffect);

		spriteRenderer.color = nodeBlockDisabled;
		spriteRenderer.sortingLayerName = _SortingLayerDisabled;
		gameObject.layer = LayerMask.NameToLayer(_SortingLayerDisabled);

		if (!freePlay) {
			selectableBlockCount.currentSelectedCount -= 1;
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

	public void SetSelectableBlockCount (SelectableBlockCount blockCount) {
		selectableBlockCount = blockCount;
	}

	public bool IsBlockEnabled () {
		return blockEnabled;
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
