using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelInformation : ScriptableObject {
	public int matrixWidth;
	public Scale scale;
	public SelectableBlockCount selectableBlockCount;
	public float BPM;
}
