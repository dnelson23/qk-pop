using UnityEngine;
using System.Collections;

public class DialoguerGUI : MonoBehaviour {

	public bool showingDialoguer;

	private string dialoguerText;
	private string[] dialoguerChoices;

	private GameObject player;
	private GameObject dialogueCollider;

	// Use this for initialization
	void Start () {
		Dialoguer.events.onStarted += onStarted;
		Dialoguer.events.onEnded += onEnded;
		Dialoguer.events.onTextPhase += onTextPhase;
	}

	//Dialoguer run and options script
	void OnGUI() {
		/* Old implementation of the dialoguer using unity GUI
		 * Look at DialogueManager.cs for Unity 4.6 UI implementation
		 * 
		if (!showingDialoguer)
			return;

		//If dialogue has no branches
		GUI.Box (new Rect (10, 10, 200, 150), dialoguerText);
		if (dialoguerChoices == null) {
			if (GUI.Button (new Rect (10, 220, 200, 30), "Continue")) {
				Dialoguer.ContinueDialogue ();
			}
		} 
		//If dialogue has branches
		else
		{
			string[] temp = dialoguerChoices;
			for (int i = 0; i < temp.Length; i++) 
			{
				if (GUI.Button (new Rect(10, 220 + (40 * i), 200, 30), temp[i])){
					Dialoguer.ContinueDialogue(i);
				}
			}
		}
		*/
	}

	private void onStarted() {
		showingDialoguer = true;
	}

	private void onEnded() {
		showingDialoguer = false;
	}

	private void onTextPhase(DialoguerTextData data) {
		dialoguerText = data.text;
		dialoguerChoices = data.choices;
	}
}
