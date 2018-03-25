using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public int width;
	public int height;
	public int depth;
	public GameObject squarePrefab; 
	public Color[] colors = new Color[8];
	public Square[,] grid;
	public GameObject menu;
	public Square currentSquare;
	public Square targetSquare;
	public delegate void SquareChecked(Square square);
	

	public Square lastSquareToMerge;
	public Square mergeTarget;

	public bool mergeRoundCompleted = true;
	// Use this for initialization
	void Start () {
		// instantiate field	
		grid = new Square[width,height];
	}

	
	// Update is called once per frame
	void Update () {
		
	}	


	void OnEnable () {
		Square.OnSquareSelected += SquareSelected;
		Square.OnSwappingComplete += OnSwappingComplete;
		Square.OnMergeComplete += OnMergeComplete;
	}

	void OnDisable () {
		Square.OnSquareSelected -= SquareSelected;
		Square.OnSwappingComplete -= OnSwappingComplete;
		Square.OnMergeComplete -= OnMergeComplete;
	}

	void SquareSelected (Square square) {
		if (targetSquare != null && currentSquare != null) {
			return;
		}

		if (currentSquare == null) {
			currentSquare = square;
		} else {
			targetSquare = square;
		}
		
		if (currentSquare != null && targetSquare != null) {
			if (!AreNeighbours(currentSquare, targetSquare)) {
				currentSquare = null;
				targetSquare = null;
				return;
			}
			// swap squares
			int[] targetPos = new int[2] { targetSquare.x , targetSquare.y };
			// move squares to new position
			currentSquare.SwapTo(targetSquare.transform.position);
			
		}
	}

	void OnSwappingComplete (Square square) {
		int newValue = currentSquare.value + targetSquare.value;
		targetSquare.UpdateValue(newValue, colors[newValue]);
		OnMergeComplete(targetSquare);
		Destroy(currentSquare.gameObject);
	}

	void OnMergeRoundComplete () {
		targetSquare = null;
		mergeRoundCompleted = true;
	}

	void OnMergeComplete (Square square) {
		// if (square != targetSquare) {
		// 	int newValue = targetSquare.value + 1;
		// 	targetSquare.UpdateValue(newValue, colors[newValue]);
		// }
		List<Square> mergeList = CheckValues(targetSquare, currentSquare);

		if (mergeList.Count > 0) {
			Merge(targetSquare, mergeList);
		} else {
			OnMergeRoundComplete();
		}
	}

	public void CreateGrid () {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				GameObject squareObj = Instantiate(squarePrefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
				int rand =  GenerateValue(x, y);
				Color color = colors[rand];
				squareObj.GetComponent<Square>().Init(color, rand, x, y);
				grid[x,y] = squareObj.GetComponent<Square>();
			}
		}
		// turn UI off
		menu.SetActive(false);
	}

	bool AreNeighbours (Square squareOne, Square squareTwo) {
		Square[] neighbours = GetNeighbours(squareOne);
		foreach (var neighbour in neighbours) {
			if (neighbour == null) 
				continue;
			int[] pos = new int[2] { neighbour.x, neighbour.y };
			if (pos[0] == squareTwo.x && pos[1] == squareTwo.y)
				return true;
		}
		return false;
	}

	List<Square> CheckValues (Square square, Square excludeSquare) {
		Square[] neighbours = GetNeighbours(square);
		List<Square> neighboursToMerge = new List<Square>();
		foreach (var neighbour in neighbours) {
			if (neighbour == null) 
				continue;
			if (neighbour.value == square.value && neighbour != excludeSquare) {
				neighboursToMerge.Add(neighbour);
			}
		} 
		return neighboursToMerge;
	}

	void Merge (Square target, List<Square> squares) {	
		foreach (Square square in squares) {
			// remove square to be merged from grid
			bool isLast = (square == squares[squares.Count - 1]);
			square.MergeTo(target, isLast);
			grid[square.x, square.y] = null;
		}
	}

	Square[] GetNeighbours (Square square) {
		
		Square[] neighbours = new Square[4];
	
		// top 
		if (square.y -1 >= 0) {
			neighbours[0] = grid[square.x, square.y -1];
		}
		// right
		if (square.x + 1 < width) {
			neighbours[1] = grid[square.x + 1, square.y];
		}
		// bottom
		if (square.y + 1 < height) {
			neighbours[2] = grid[square.x, square.y + 1];
		}
		// left
		if (square.x - 1 >= 0) {
			neighbours[3] = grid[square.x -1, square.y];
		}
		return neighbours;
	}

	int GenerateValue (int x, int y) {	
	
		List<int> validColors = new List<int>();
		// check values to exclude
		for (int i = 0; i < depth; i++) {
			if (x == 0 && y == 0) {
				validColors.Add(i);
				continue;
			}

			if (y == 0 && !(grid[x - 1,y].value == i) || x == 0 && !(grid[x,y - 1].value == i)) {
				validColors.Add(i);
				continue;
			}

			if ((x >= 1  && !(grid[x - 1,y].value == i)) && (y >= 1  && !(grid[x,y - 1].value == i))) {
				validColors.Add(i);
			}  
		}

		int rand =  Random.Range(0, validColors.Count);

		return validColors[rand];
	}


}
