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

	public bool mergeCurrentComplete = false;
	public bool mergeTargetComplete = false;

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
		} else if (mergeRoundCompleted) {
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
			int[] currentPos = new int[2] { currentSquare.x , currentSquare.y };
			// update pos in grid
			grid[targetPos[0], targetPos[1]] = currentSquare;
			grid[currentPos[0], currentPos[1]] = targetSquare;
			// move actual squares
			currentSquare.SwapTo(targetSquare.transform.position);
			targetSquare.SwapTo(currentSquare.transform.position);
			currentSquare.SetPos(targetPos[0], targetPos[1]);
			targetSquare.SetPos(currentPos[0], currentPos[1]);
		}
	}

	void OnSwappingComplete (Square square) {
		if (square == currentSquare) {
			OnMergeComplete(currentSquare);
			OnMergeComplete(targetSquare);
		}
	}

	void OnMergeRoundComplete () {
		Debug.Log("Merge Round Complete");
		if (targetSquare != null && currentSquare.value == targetSquare.value) {
			List<Square> mergeList = CheckValues(currentSquare, null);
			MergeRound(currentSquare, null, mergeList);
		} else {
			currentSquare = null;
			targetSquare = null;
			mergeRoundCompleted = true;
			mergeCurrentComplete = false;
			mergeTargetComplete = false;
		}
	}

	void OnMergeComplete (Square square) {
		Debug.Log("Merge Complete");
		if (square == currentSquare) {
			List<Square> mergeList = CheckValues(currentSquare, targetSquare);
			if (mergeList.Count > 0) {
				MergeRound(currentSquare, targetSquare, mergeList);
			} else {
				mergeCurrentComplete = true;
			}
		}
	
		if (square == targetSquare) {
			List<Square> mergeList2 = CheckValues(targetSquare, currentSquare);
			if (mergeList2.Count > 0) {
				MergeRound(targetSquare, currentSquare, mergeList2);	
			} else {
				mergeTargetComplete = true;
			}
		}

		if (targetSquare == null) {
			mergeTargetComplete = true;
		}
		
		
		
		if (mergeCurrentComplete && mergeTargetComplete) {
			OnMergeRoundComplete();
		}
	}

	void MergeRound (Square startSquare, Square excludeSquare, List<Square> mergeList) {
		mergeRoundCompleted = false;
		int newValue = startSquare.value + 1;
		Color newColor = colors[newValue];
		startSquare.UpdateValue(newValue, newColor);
		Merge(startSquare, mergeList);
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
			bool isLast = (square == squares[squares.Count - 1]);
			square.MergeTo(target, isLast);
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
