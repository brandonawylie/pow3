using UnityEngine;
using System.Collections;

public class PlayerSpriteController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void DoneAttacking() {

		GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().DoneAttacking();
	}	
}
