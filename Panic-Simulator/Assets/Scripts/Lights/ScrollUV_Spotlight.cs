using UnityEngine;
using System.Collections;

public class ScrollUV_Spotlight : MonoBehaviour {


	public float horizontalScrollSpeed = 0.5f;
	public float verticalScrollSpeed = 0.5f;

	private Renderer _myRenderer;

	private bool scroll = true;

	void Start () 
	{
		_myRenderer = GetComponent<Renderer>();
		if(_myRenderer == null)
			enabled = false;
	}

	public void FixedUpdate()
	{
		if (scroll)
		{
			float verticalOffset = Time.time * verticalScrollSpeed;
			float horizontalOffset = Time.time * horizontalScrollSpeed;
			_myRenderer.material.mainTextureOffset = new Vector2(horizontalOffset, verticalOffset);
		}
	}

	public void DoActivateTrigger()
	{
		scroll = !scroll;
	}

}