using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
using Debug = FFP.Debug;

//! Master manager of the game. This manager manages all other (significant) managers.
public sealed class MasterSceneManager : MonoBehaviour {

	private static List<string> scenes = new List<string>();				//<! Holds all avaiable scenes

	#region singletonEnforcement
	private static MasterSceneManager instance;
	public static MasterSceneManager Instance {
		get {
			return instance;
		}
		private set { }
	}

	void Awake(){
		if (instance == null){
			instance = this;
		}else {
			Destroy(this.gameObject);
			Debug.Error("core", "Second MasterSceneManager detected. Deleting gameOject.");
			return;
		}
	}
	#endregion
	
	public void InitScenesDictionary(){
		if(scenes.Count == 0)
			LoadScenesDictionary();
	}

	private void LoadScenesDictionary(){
		try{
			TextAsset textAsset = Resources.Load(StringManager.SCENELIST) as TextAsset;
			string text = textAsset.ToString();			
			if (text == "")
				Debug.Warning("core", "SceneList empty");
			else {
				JSONNode N = JSON.Parse(text);
				scenes.Clear();
				for (int i = 0; i < N.Count; i++)
					scenes.Add(N[i].Value);
				Debug.Log("core", "Scenes dictionary loaded.");
			}
		}catch(NullReferenceException e){
			Debug.Error("core", "SceneList missing: " + e);
		}
	}

	//! This will load a single level if it exists in the json file. This will clear all other objects
	public void QuickLoadLevel(string levelName) {
		ObjectManager.RemoveAllTopObjects();
		Instance.StartCoroutine(COLoadLevel(levelName, false));
	}

	//! Load level if it exists in the json file.
	public void LoadLevel(string levelName) {
		Instance.StartCoroutine(COLoadLevel(levelName, true));
	}

	IEnumerator COLoadLevel(string levelName, bool additive = true) {
		if (!scenes.Contains(levelName)) {
			Debug.Log("core", "No scenes to be loaded. Please ensure valid scenes are ready to be loaded in Assets/Resources/Json/sceneList.json");
		} else {
			if (additive)
				Application.LoadLevelAdditiveAsync(levelName);
			else
				Application.LoadLevelAsync(levelName);

			yield return new WaitForEndOfFrame();
			ObjectManager.LoadTopObjects();
			Debug.Log("core", "Level " + levelName + " loaded\nAdditive = " + additive + ".");
		}
	}

	public void loadLevelWithIndex (int levelIndex){
		Application.LoadLevel (levelIndex);
	}
	
	public void quitGame(){
		Application.Quit ();
	}
}
