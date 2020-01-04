using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISelectableBlockCount : MonoBehaviour
{
	[SerializeField] private SelectableBlockCount selectedBlockCount;
	Text selectedBlockCountText;

	void Awake () {
		selectedBlockCountText = GetComponent<Text>();
	}

	void Update () {
		selectedBlockCountText.text = "x " + selectedBlockCount.GetRemainingSelectableCount();
	}
}
