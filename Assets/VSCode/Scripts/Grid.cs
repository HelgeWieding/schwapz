using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	// Use this for initialization
	public Square[,] grid;
	public int[,] snapshot;

	public Color[] colors = new Color[8];

	public Square squarePrefab;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Init(int width, int height, int[] values) {
		this.grid = new Square[width,height];
		this.snapshot = new int[width,height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (this.grid[x,y] == null) {
					Square squareObj = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity);
					// int rand =  GenerateValue(x, y);
					// Color color = colors[rand];
					// squareObj.GetComponent<Square>().Init(color, rand, x, y, this.moveSpeed, this.scaleFactor);
					// this.grid[x,y] = squareObj.GetComponent<Square>();
				}
			}
		}
	}

	void SetSnapShot(int[,] snap) {
		this.snapshot = snap;
	}

	int[,] GenerateSnapshot(Square[,] grid) {
		// int width = grid.GetLength(0);
		// int length = grid.GetLength(1);
		// var newSnap = new int[width, length];
		// for (int i = 0; i < width; i++) {
		// 	for (int y = 0; y < height; i++) {
		// 		newSnap[x,y] = grid[x,y].value;	
		// 	}
		// }
		// return newSnap;
		return new int[1,2];
	}
}
