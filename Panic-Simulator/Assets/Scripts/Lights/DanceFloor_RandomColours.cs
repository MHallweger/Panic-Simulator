using UnityEngine;
using System.Collections;

public class DanceFloor_RandomColours : MonoBehaviour {
	
public int uvAnimationTileX = 8; //Here you can place the number of columns of your sheet. 
                           //The above sheet has 24
 
public int uvAnimationTileY = 8; //Here you can place the number of rows of your sheet. 
                          //The above sheet has 1

private float framesPerSecond = 1.0f;

private int currentTimeInt = 0;
private int randomNumX;
private int randomNumY;
private Vector2 offsetSquare;

private Renderer _myRenderer;




void Start () 
{

		_myRenderer = GetComponent<Renderer>();
		if(_myRenderer == null)
		enabled = false;

		framesPerSecond = DanceFloor_ChangeSpeed.animSpeed;

		// print (framesPerSecond);
}


void Update (){
		
 	// framesPerSecond = DanceFloor_ChangeSpeed.animSpeed;


	float index = Time.time * framesPerSecond;
	


	if (index > currentTimeInt) 
	{
	currentTimeInt = (int)index;
	currentTimeInt = currentTimeInt + 1;

    // Select random coloured square
	randomNumX = (1 * (Random.Range (1, uvAnimationTileX)));
	randomNumY = (1 * (Random.Range (1, uvAnimationTileY)));
	}

   
	// Set size of every tile
	Vector2 size = new Vector2 (1.0f / uvAnimationTileX, 1.0f / uvAnimationTileY);
	
	Vector2	offsetSquare = new Vector2 ( 1.0f / uvAnimationTileX  * randomNumX, 1.0f / uvAnimationTileY * randomNumY );
	
	_myRenderer.material.SetTextureOffset ("_MainTex", offsetSquare);
	_myRenderer.material.SetTextureScale ("_MainTex", size);

}
}