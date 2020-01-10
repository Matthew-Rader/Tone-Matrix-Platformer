using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixGrid : MonoBehaviour
{
	[SerializeField] private NoteBlock noteBlockPrefab;
	[SerializeField] private LevelInformation levelInfo;
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private float noteBlockPadding = 0.4f;
	[SerializeField] private bool isFreePlay = false;

	private NoteBlock[,] noteBlockMatrix;
	private int numNoteBlocks;
	private int currentColToPlay = 0;
	private float timeBetweenBeatsCounter = 0.0f;

    void Start() {
		numNoteBlocks = levelInfo.scale.notes.Length;

		CreateMatrix();

		if (!isFreePlay) {
			GameManager.SetSpawnPoint(spawnPoint);
			GameManager.SetPlayerToCurrentSpawnPoint();
		}
    }

	void CreateMatrix () {
		noteBlockMatrix = new NoteBlock[numNoteBlocks, numNoteBlocks];
		float offset = (float)numNoteBlocks / 2.0f;
		offset = numNoteBlocks % 2 == 0 ? offset - 0.5f : offset;

		float startSpwnOffset = 0 - (offset) - (noteBlockPadding * (offset));

		Vector3 blockSpawnPosition = new Vector3(startSpwnOffset, startSpwnOffset, 0.0f);

		for (int x = 0; x < numNoteBlocks; ++x) {
			for (int y = 0; y < numNoteBlocks; ++y) {
				noteBlockMatrix[x, y] = Instantiate(noteBlockPrefab, blockSpawnPosition, Quaternion.identity, this.gameObject.transform);
				noteBlockMatrix[x, y].SetNote(levelInfo.scale.notes[y]);
				noteBlockMatrix[x, y].SetSelectableBlockCount(levelInfo.selectableBlockCount);
				blockSpawnPosition.y += (1.0f + noteBlockPadding);
			}
			blockSpawnPosition.x += (1.0f + noteBlockPadding);
			blockSpawnPosition.y = startSpwnOffset;
		}
	}

    void Update() {
		if (timeBetweenBeatsCounter < ((60.0f / levelInfo.BPM) / 4.0f)) {
			timeBetweenBeatsCounter += Time.deltaTime;
		}
		else {
			PlayEnabledBlocksInCol(currentColToPlay);
			currentColToPlay = (currentColToPlay + 1) % numNoteBlocks;
			timeBetweenBeatsCounter = 0.0f;
		}
    }

	void PlayEnabledBlocksInCol (int col) {
		for (int i = 0; i < numNoteBlocks; ++i) {
			if (noteBlockMatrix[col, i].IsBlockEnabled()) {
				noteBlockMatrix[col, i].PlayAudio();
			}
		}
	}
}
