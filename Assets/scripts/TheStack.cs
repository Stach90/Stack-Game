using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TheStack : MonoBehaviour
{
	public Text scoreText;												//Variable for score counting.
	public Color32[] gameColors;										//Variable for colors for Tiles and background. Available in Unity Inspector.
	public Material stackMat;											//Variable for material placement for gameObjects. Available in Unity Inspector.
	public GameObject endPanel;											//Variable for ending panel. Available in Unity Inspector.
	public AudioClip[] clips;											//Variable for music track. Available in Unity Inspector.

	private const float BOUNDS_SIZE = 3.5f;
	private const float STACK_MOVING_SPEED = 5.0f;						
	private const float ERROR_MARGIN = 0.25f;							//Margin for succesful placing.
	private const float STACK_BOUNDS_GAIN = 0.25f;						//How much Stack grow with succesful COMBO
	private const int COMBO_START_GAIN = 3; 							//Number of succesfoul placements that grants boost | makes tiles bigger. 

	private GameObject[] theStack;
	private Vector2 stackBounds = new Vector2 (BOUNDS_SIZE, BOUNDS_SIZE);

	private int stackIndex;												//Stack number for counting whitch next will appear.
	private int scoreCount = 0;											//Variable for Score.
	private int combo = 0;												//Variable for COMBO

	private int lastColorIndex = 0;									//
	private Color32 startColor;										// Variables for gameObjects color changes.
	private Color32 endColor;										//
	private float colorTransition = 0;								//

	private float tileTransition = 0.0f;
	private float tileSpeed = 2.5f;										//Game Speed - sets tile movement speed.
	private float secondaryPosition;
	

	private bool isMovingOnX = true;									//Variable for saving direction of tile movement.
	private bool gameOver = false;										

	private Vector3 desiredPosition;
	private Vector3 lastTilePosition;

	public GameObject Fire1;										//
	public GameObject Fire2;										//
	public GameObject ParticlesEffects;								//	Variables for Particle Effects.
	public GameObject EndEffect;									//
	public GameObject Splash;										//

	private void Start () 
	{
		theStack = new GameObject[transform.childCount];
		startColor = gameColors [0];
		endColor = gameColors [1];
		lastColorIndex = 1;
		for (int i = 0; i < transform.childCount; i++) 							//loop for Tile counting and changes
		{
			theStack [i] = transform.GetChild (i).gameObject;
			ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
		}

		stackIndex = transform.childCount - 1;									//Give as adequate count start from bottom in Array of Tiles.
	}

// CreateRubble makes cutted elements that fall to the catcher-GameObject that delete it for saving memory and CPU power.
	private void CreateRubble(Vector3 pos,Vector3 scale)
	{
		GameObject go = GameObject.CreatePrimitive (PrimitiveType.Cube);
		go.transform.localPosition = pos;										//Set Rubble position.
		go.transform.localScale = scale;										//Set Rubble cutted form.
		go.AddComponent<Rigidbody> ();											//Give Rigidbody for physics | that's why they falling.

		go.GetComponent<MeshRenderer> ().material = stackMat;					//Give material for rubble.
		ColorMesh(go.GetComponent<MeshFilter> ().mesh); 						//Get color from last tile.
	}

	private void Update ()
	{
		if (gameOver)
			return;

		if (Input.GetMouseButtonDown (0))										//Loop for events of click or touch.
		{
			if (PlaceTile ()) 
			{
				
				SpawnTile ();
				scoreCount++;
				scoreText.text = scoreCount.ToString ();						//change int to string for Score show.
				EffectsTiming();												//Start time for effects 
			}
			else
			{
				EndGame ();
			}
		}

		MoveTile ();
		transform.position = Vector3.Lerp(transform.position,desiredPosition,STACK_MOVING_SPEED * Time.deltaTime); 		//Stack movement
	}

	// Start time for effects 
	private void EffectsTiming()
	{
		if (scoreCount == 25){
				EffectsParticle ();
				}
	}


	// Fire Particle effects Instantiate.
	private void EffectsParticle()
	{
				Instantiate(Fire1, ParticlesEffects.transform);
				Instantiate(Fire2, ParticlesEffects.transform);
	}
	// Splash Particle effects Instantiate.
	private void EffectsSplash()
	{
				Instantiate(Splash, ParticlesEffects.transform);
	}



	private void MoveTile()
	{
		tileTransition += Time.deltaTime * tileSpeed;
		if(isMovingOnX)
			theStack [stackIndex].transform.localPosition = new Vector3 (Mathf.Sin (tileTransition) * BOUNDS_SIZE, scoreCount, secondaryPosition);
		else
			theStack [stackIndex].transform.localPosition = new Vector3 (secondaryPosition, scoreCount, Mathf.Sin (tileTransition) * BOUNDS_SIZE);
	}

	private void SpawnTile()
	{
		tileTransition = 1.0f;
		lastTilePosition = theStack [stackIndex].transform.localPosition;
		stackIndex--;
		if (stackIndex < 0)
			stackIndex = transform.childCount - 1;

		desiredPosition = (Vector3.down) * scoreCount;
		theStack [stackIndex].transform.localPosition = new Vector3 (0, scoreCount, 0);
		theStack [stackIndex].transform.localScale = new Vector3(stackBounds.x,1,stackBounds.y);

		ColorMesh(theStack [stackIndex].GetComponent<MeshFilter> ().mesh);
	}

	private bool PlaceTile()
	{
		Transform t = theStack [stackIndex].transform;

		if (isMovingOnX) 
		{
			float deltaX = lastTilePosition.x - t.position.x;
			if (Mathf.Abs (deltaX) > ERROR_MARGIN) 
			{
            // Put something in the clips array before un-commenting this line
			//	AudioSource.PlayClipAtPoint (clips [0], Camera.main.transform.position);
				// CUT THE TILE
				combo = 0;
				stackBounds.x -= Mathf.Abs (deltaX);
				if (stackBounds.x <= 0)
					return false;

				float middle = lastTilePosition.x + t.localPosition.x / 2;
				t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
				CreateRubble
				(
					new Vector3 ((t.position.x > 0) 
						? t.position.x + (t.localScale.x / 2)
						: t.position.x - (t.localScale.x / 2)
						, t.position.y
						, t.position.z),
					new Vector3 (Mathf.Abs (deltaX), 1, t.localScale.z)
				);
				t.localPosition = new Vector3 (middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
			} 
			else 
			{
                // Put something in the clips array before un-commenting this line
				// AudioSource.PlayClipAtPoint (clips [1], Camera.main.transform.position);
				if (combo > COMBO_START_GAIN) 
				{
					stackBounds.x += STACK_BOUNDS_GAIN;
					if (stackBounds.x > BOUNDS_SIZE)
						stackBounds.x = BOUNDS_SIZE;
					
					float middle = lastTilePosition.x + t.localPosition.x / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
				}

				combo++;
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);
			}
		}
		else
		{
			float deltaZ = lastTilePosition.z - t.position.z;
			if (Mathf.Abs (deltaZ) > ERROR_MARGIN)
			{
                // Put something in the clips array before un-commenting this line
				// AudioSource.PlayClipAtPoint (clips [0], Camera.main.transform.position);
				// CUT THE TILE
				combo = 0;
				stackBounds.y -= Mathf.Abs (deltaZ);
				if (stackBounds.y <= 0)
					return false;

				float middle = lastTilePosition.z + t.localPosition.z / 2;
				t.localScale = new Vector3(stackBounds.x,1,stackBounds.y);
				CreateRubble
				(
					new Vector3 (t.position.x
						, t.position.y
						, (t.position.z > 0) 
						? t.position.z + (t.localScale.z / 2)
						: t.position.z - (t.localScale.z / 2)),
					new Vector3 (t.localScale.x, 1, Mathf.Abs (deltaZ))
				);
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount,middle - (lastTilePosition.z / 2));
			}
			else 
			{
                // Put something in the clips array before un-commenting this line
				// AudioSource.PlayClipAtPoint (clips [1], Camera.main.transform.position);
				if (combo > COMBO_START_GAIN) 
				{
					if (stackBounds.y > BOUNDS_SIZE)
						stackBounds.y = BOUNDS_SIZE;
					Instantiate(Splash, ParticlesEffects.transform);
					stackBounds.y += STACK_BOUNDS_GAIN;
					float middle = lastTilePosition.z + t.localPosition.z / 2;
					t.localScale = new Vector3 (stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3 (lastTilePosition.x, scoreCount,middle - (lastTilePosition.z / 2));
				}
				combo++;
				t.localPosition = new Vector3 (lastTilePosition.x, scoreCount, lastTilePosition.z);
			}
		}
			
		secondaryPosition = (isMovingOnX)
			? t.localPosition.x
			: t.localPosition.z;
		isMovingOnX = !isMovingOnX;

		return true;
	}
		
	private void ColorMesh(Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		Color32[] colors = new Color32[vertices.Length];
		colorTransition += 0.1f;
		if (colorTransition > 1) 
		{
			colorTransition = 0.0f;
			startColor = endColor;
			int ci = lastColorIndex;
			while (ci == lastColorIndex)
				ci = Random.Range (0, gameColors.Length);
			endColor = gameColors [ci];
		}
		Color c = Color.Lerp(startColor,endColor,colorTransition);

		for (int i = 0; i < vertices.Length; i++)
			colors [i] = c;

		mesh.colors32 = colors;
	}

	private void EndGame()
	{
		if (PlayerPrefs.GetInt ("score") < scoreCount)
			PlayerPrefs.SetInt ("score", scoreCount);
		gameOver = true;
		Instantiate(EndEffect, ParticlesEffects.transform);
		endPanel.SetActive (true);
		theStack [stackIndex].AddComponent<Rigidbody> ();

		#if UNITY_EDITOR
		#elif UNITY_ANDROID
		if (Admob.Instance().isRewardedVideoReady()) {
		Admob.Instance().showRewardedVideo();
		}
		#elif UNITY_IPHONE
		if (Admob.Instance().isRewardedVideoReady()) {
		Admob.Instance().showRewardedVideo();
		}
		#endif
	}

	public void OnButtonClick(string sceneName)
	{
		SceneManager.LoadScene (sceneName);
	}
}
