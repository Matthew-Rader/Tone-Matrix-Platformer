using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SelectableBlockCount : ScriptableObject, ISerializationCallbackReceiver {
	public int Max;
	public int currentSelectedCount;

	public bool CurrentSelectCountLessThanMax () {
		return currentSelectedCount < Max;
	}

	// reset to initial values
	public void OnAfterDeserialize () {
		currentSelectedCount = 0;
	}

	// required for ISerializationCallbackReceiver interface
	public void OnBeforeSerialize () { }

	public int GetRemainingSelectableCount () {
		return Max - currentSelectedCount;
	}
}
