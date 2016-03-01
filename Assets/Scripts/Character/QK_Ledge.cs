using UnityEngine;
using System.Collections;

public class QK_Ledge : MonoBehaviour {
	public GameObject left;
	public GameObject right;
	// Use this for initialization
	void Start () {
		if (left == null || right == null) {
			//Debug.Log("player", "missing ends");
		}
	}

	public GameObject getLeftPoint(){
		return left;
	}

	public GameObject getRightPoint(){
		return right;
	}
}
