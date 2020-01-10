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
	private int numNoteBlocksHeight;
	private int numNoteBlocksWidth;
	private int currentColToPlay = 0;
	private float timeBetweenBeatsCounter = 0.0f;

    void Start() {
		numNoteBlocksHeight = levelInfo.scale.notes.Length;
		numNoteBlocksWidth = levelInfo.matrixWidth;

		CreateMatrix();

		if (!isFreePlay) {
			GameManager.SetSpawnPoint(spawnPoint);
			GameManager.SetPlayerToCurrentSpawnPoint();
		}
    }

	void CreateMatrix () {
		noteBlockMatrix = new NoteBlock[numNoteBlocksWidth, numNoteBlocksHeight];

		float offsetX = (float)numNoteBlocksWidth / 2.0f;
		offsetX = numNoteBlocksWidth % 2 == 0 ? offsetX - 0.5f : offsetX;

		float offsetY = (float)numNoteBlocksHeight / 2.0f;
		offsetY = numNoteBlocksHeight % 2 == 0 ? offsetY - 0.5f : offsetY;

		float startSpwnOffsetX = 0 - (offsetX) - (noteBlockPadding * (offsetX));
		float startSpwnOffsetY = 0 - (offsetY) - (noteBlockPadding * (offsetY));

		Vector3 blockSpawnPosition = new Vector3(startSpwnOffsetX, startSpwnOffsetY, 0.0f);

		for (int x = 0; x < numNoteBlocksWidth; ++x) {
			for (int y = 0; y < numNoteBlocksHeight; ++y) {
				noteBlockMatrix[x, y] = Instantiate(noteBlockPrefab, blockSpawnPosition, Quaternion.identity, this.gameObject.transform);
				noteBlockMatrix[x, y].SetNote(levelInfo.scale.notes[y]);
				noteBlockMatrix[x, y].SetSelectableBlockCount(levelInfo.selectableBlockCount);
				blockSpawnPosition.y += (1.0f + noteBlockPadding);
			}
			blockSpawnPosition.x += (1.0f + noteBlockPadding);
			blockSpawnPosition.y = startSpwnOffsetY;
		}
	}

    void Update() {
		if (timeBetweenBeatsCounter < ((60.0f / levelInfo.BPM) / 4.0f)) {
			timeBetweenBeatsCounter += Time.deltaTime;
		}
		else {
			PlayEnabledBlocksInCol(currentColToPlay);
			currentColToPlay = (currentColToPlay + 1) % numNoteBlocksWidth;
			timeBetweenBeatsCounter = 0.0f;
		}
    }

	void PlayEnabledBlocksInCol (int col) {
		for (int i = 0; i < numNoteBlocksHeight; ++i) {
			if (noteBlockMatrix[col, i].IsBlockEnabled()) {
				noteBlockMatrix[col, i].PlayAudio();
			}
		}
	}
}
