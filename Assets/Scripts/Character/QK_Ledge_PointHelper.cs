using UnityEngine;
using System.Collections;

public class QK_Ledge_PointHelper : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		this.gameObject.GetComponent<MeshRenderer> ().enabled = false;
	}

}
