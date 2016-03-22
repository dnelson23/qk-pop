using UnityEngine;
using System.Collections;

public class Init : MonoBehaviour {

	//Design options for placement and size of prompt textbox and text
	public string textBoxText;
	public int textBoxWidth;
	public int textBoxHeight;
	public int textBoxVertOffset;
	public int textBoxHorzOffset;
	public bool inDialogue;

	private bool triggerDialoguer;
	private GameObject player;
	private bool dialogueCol;
	private GameObject dialogueGUI;

	void Awake () {
		Dialoguer.Initialize ();
	}

	// Use this for initialization
	void Start () {
		triggerDialoguer = false;
		dialogueCol = false;
		player = GameObject.FindGameObjectWithTag ("Player");
		dialogueGUI = GameObject.Find ("GUI");
	}
	
	// Update is called once per frame
	void Update () {
		//Check if NPC has dialogue
		if (player.GetComponentInChildren<DialogueCollider> ().NPCDialogue == true) {
			//Asks player if they want to interact
			dialogueCol = true;
			if (Input.GetKeyDown (KeyCode.F)) {
				triggerDialoguer = true;
			}
		}
		else {
			dialogueCol = false;
		}
		if (!dialogueGUI.GetComponent<DialoguerGUI> ().showingDialoguer) {
			inDialogue = false;
		}
	}

	//Checks if player is interacting and runs dialoguer
	void OnGUI() {
		if (dialogueCol) {
			GUI.Box (new Rect ((Screen.width / 2) - (textBoxWidth / 2) + textBoxHorzOffset, (Screen.height / 2) - (textBoxHeight / 2) + textBoxVertOffset, textBoxWidth, textBoxHeight), textBoxText);
			if (triggerDialoguer) {
				Dialoguer.StartDialogue (player.GetComponentInChildren<DialogueCollider> ().NPCDialogueNumber, dialoguerCallback);
				this.enabled = false;
				triggerDialoguer = false;
				inDialogue = true;
			}
		}
	}

	private void dialoguerCallback(){
		this.enabled = true;
	}
}
