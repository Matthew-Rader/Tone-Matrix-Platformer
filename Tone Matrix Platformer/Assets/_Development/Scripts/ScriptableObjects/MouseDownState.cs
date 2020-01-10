using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MouseDownState : ScriptableObject, ISerializationCallbackReceiver {
	public enum State { Enable, Disable };

	public State state;

	public void SetState (State inState) {
		state = inState;
	}

	public bool ShouldEnable () {
		return state == State.Enable ? true : false;
	}

	// reset to initial values
	public void OnAfterDeserialize () {
		state = State.Enable;
	}

	// required for ISerializationCallbackReceiver interface
	public void OnBeforeSerialize () { }
}
