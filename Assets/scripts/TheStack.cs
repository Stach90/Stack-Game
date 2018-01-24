using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour {


	private const float BOUNDS_SIZE = 3.5f;
	private const float STACK_MOVING_SPEED = 5.0f;
	private const float ERROR_MARGIN = 0.1f;
	private const float STACK_BOUNDS_GAIN = 025f;
	private const int COMBO_START_GAIN = 3;

	private GameObject[] theStack;
	private Vector2 stackBounds = new Vector2(BOUNDS_SIZE, BOUNDS_SIZE);

	private int stackIndex;
	private int scoreCount = 0;
	private int combo = 0;


	private float tileTransition = 0.0f;
	private float tileSpeed = 2.5f;
	private float secondaryPosition;

	private bool isMovingOnX = true;
	private bool gameOver = false;


	private Vector3 desiredPosition;
	private Vector3 lastTilePosition;


	// Use this for initialization
	private void Start () {
		theStack = new GameObject[transform.childCount];
		for(int i = 0; i < transform.childCount; i++)
		theStack [i] = transform.GetChild (i).gameObject;


		stackIndex = transform.childCount -1;

	}
	
	// Update is called once per frame
	private void Update () {
		if (Input.GetMouseButtonDown (0))
		{
			if(PlaceTile ())
		{
			SpawnTile ();
			scoreCount++;
		} else {
			EndGame ();
		}
	  }

	  MoveTile();

	  //Move ALL STACK on click
		transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime); //LERP - Linearly interpolates between two vectors


	}

	private void MoveTile(){
		if(gameOver == true){
			return;
		}

		tileTransition += Time.deltaTime * tileSpeed;
		if(isMovingOnX){
			theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin (tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition); // from left to right
	  } else {
			theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, Mathf.Sin (tileTransition) * BOUNDS_SIZE); // from right to left
	  }

	}





	private void SpawnTile(){
		lastTilePosition = theStack[stackIndex].transform.localPosition;
		stackIndex--;
		if(stackIndex < 0)
			stackIndex = transform.childCount - 1; 

		desiredPosition = (Vector3.down) * scoreCount; 
		theStack [stackIndex].transform.localPosition = new Vector3(0, scoreCount, 0);
		theStack [stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y); // changing block size - cutting the block

	}

	private bool PlaceTile()
	{
		Transform t = theStack [stackIndex].transform;

		if(isMovingOnX)
		{
			float deltaX = lastTilePosition.x - t.position.x;
			if (Mathf.Abs (deltaX) > ERROR_MARGIN)
			{
				//CUT THE TILE
				combo = 0;
				stackBounds.x -= Mathf.Abs (deltaX);
				if(stackBounds.x <= 0)
					return false;

				float middle =  lastTilePosition.x + t.localPosition.x / 2;
				t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y); // changing block size - cutting the block
				t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z); // repositioning of cutted block
			}
			 	else{
				 	if (combo > COMBO_START_GAIN){
				 		stackBounds.x += STACK_BOUNDS_GAIN;
						float middle =  lastTilePosition.x + t.localPosition.x / 2;
					t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
				 	}


			 	combo++;
				t.localPosition = lastTilePosition + new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);
			 	}
		 	} else 
		 	{

				float deltaZ = lastTilePosition.z - t.position.z;
			if (Mathf.Abs (deltaZ) > ERROR_MARGIN){
				//CUT THE TILE
				combo = 0;
				stackBounds.y -= Mathf.Abs (deltaZ);
				if(stackBounds.y <= 0)
					return false;

				float middle =  lastTilePosition.z + t.localPosition.z / 2;
				t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y); // changing block size - cutting the block
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2)); // repositioning of cutted block
			} else
				{
				if (combo > COMBO_START_GAIN){
				 		stackBounds.y += STACK_BOUNDS_GAIN;
						float middle =  lastTilePosition.z + t.localPosition.z / 2;
					t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
				 	}


			 	combo++;
				t.localPosition = lastTilePosition + new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);
			 	}
		
		}


		secondaryPosition = (isMovingOnX)
			 ? t.localPosition.x
			 : t.localPosition.z;
		isMovingOnX = !isMovingOnX; // Changing move direction by changing bool
		return true;
	}


	private void EndGame(){
		Debug.Log ("Lose");
		gameOver = true;
		theStack [stackIndex].AddComponent<Rigidbody> ();
	}
}
