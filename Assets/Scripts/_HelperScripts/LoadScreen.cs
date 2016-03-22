using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Debug = FFP.Debug;

public class LoadScreen : MonoBehaviour {

	GameObject loadScreen;

	void Awake(){
		loadScreen = GameObject.Find ("LoadScreen");
		if (!loadScreen) {
			Debug.Error ("ui", "Could not find the GameObject 'LoadScreen' in the scene: " + Application.loadedLevelName);
		}
		loadScreen.SetActive (false);
	}

	public void ShowLoadScreen(){
		loadScreen.SetActive (true);
	}
}