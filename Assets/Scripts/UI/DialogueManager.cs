using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.UI;

[EventVisibleAttribute]
public class DialogueManager : MonoBehaviour
{

    #region Singleton
    private static DialogueManager _instance;
    public static DialogueManager Instance
    {
        get
        {
            _instance = _instance ?? (_instance = GameObject.FindObjectOfType<DialogueManager>());
            if(_instance == null)
            {
                Debug.LogWarning("DialogueManager is not in scene but a script is attempting to access it.");
            }
            return _instance;
        }
    }
    #endregion

    public GameObject[] _choiceButtons;

	private static string portraitPATH = "DialoguePortraits/";
    private static string DIALOGUELIST = "dialogueList";

	public bool _showing = false;
	private bool _needToChange = false;
	private string _text;
	private string[] _choices;
	private bool isGood = true;
	private string _theme;
	private string _portrait;

	GameObject _dialogueGO;
	GameObject _goodBackground;
	GameObject _badBackground;
	GameObject _dialogueText;
	GameObject _continueButton;
	GameObject _goodPortrait;
	GameObject _badPortrait;
	GameObject _portraitImage;
	GameObject _goodBackgroundAndPortrait;
	GameObject _badBackgroundAndPortrait;

	// Use this for initialization
	void Awake () {
		Dialoguer.Initialize ();
		_choiceButtons = new GameObject[5];
	}

	// Use this for initialization
	void Start () {
		Dialoguer.events.onStarted += onStarted;
		Dialoguer.events.onEnded += onEnded;
		Dialoguer.events.onTextPhase += onTextPhase;

		_dialogueGO = GameObject.Find ("Dialogue");
		//_goodBackground = _dialogueGO.transform.FindChild ("GoodBackground").gameObject;
		//_badBackground = _dialogueGO.transform.FindChild ("BadBackground").gameObject;
		_dialogueText = _dialogueGO.transform.FindChild ("DialogueText").gameObject;
		_continueButton = _dialogueGO.transform.FindChild ("ContinueButton").gameObject;
		//_goodPortrait = _dialogueGO.transform.FindChild ("GoodPortrait").gameObject;
		//_badPortrait = _dialogueGO.transform.FindChild ("BadPortrait").gameObject;
		_portraitImage = _dialogueGO.transform.FindChild ("PortraitImage").gameObject;
		_goodBackgroundAndPortrait = _dialogueGO.transform.FindChild ("GoodBackgroundAndPortrait").gameObject;
		_badBackgroundAndPortrait = _dialogueGO.transform.FindChild ("BadBackgroundAndPortrait").gameObject;

		for(int index = 0; index < _choiceButtons.Length; index++){
			_choiceButtons[index] = _dialogueGO.transform.FindChild ("ChoiceButton" + (index + 1).ToString()).gameObject;
			_choiceButtons[index].SetActive (false);
			addListener (_choiceButtons[index].GetComponent<Button>(), index);	
		}
        _continueButton.GetComponent<Button>().onClick.AddListener(() => ClickedContinueButton());
        _dialogueGO.SetActive (false);
	}
	
	private void onStarted() {
		_showing = true;
        QK_Character_Movement.Instance.inADialogue = true;
		GameHUD.Instance.showMinimap = false;
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		PoPCamera.State = Camera_2.CameraState.Pause;
	}
	
	private void onEnded() {
		_showing = false;
        QK_Character_Movement.Instance.inADialogue = false;
		GameHUD.Instance.showMinimap = true;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		PoPCamera.State = Camera_2.CameraState.Normal;
//		_goodBackground.SetActive (false);
//		_badBackground.SetActive (false);
	}

	private void onTextPhase(DialoguerTextData data) {
		_text = data.text;
		_dialogueText.GetComponent<Text> ().text = data.text;
		_choices = data.choices;
		_theme = data.theme;
		_portrait = data.portrait;

		if (_theme == "Good") {
			isGood = true;
		} else if (_theme == "Bad") {
			isGood = false;
		} else {
			Debug.Log("Theme is neither 'Good' or 'Bad'. Check Dialogue Tree");
		}
		
		if (isGood) {
			_goodBackgroundAndPortrait.SetActive (true);
			_badBackgroundAndPortrait.SetActive (false);
		} else {
			_goodBackgroundAndPortrait.SetActive (false);
			_badBackgroundAndPortrait.SetActive (true);
		}
		SetPortrait ();
		if (_choices == null || _choices.Length < 1) {
			for(int i = 0; i < _choiceButtons.Length; i++) {
				_choiceButtons[i].SetActive (false);
			}
			ShowContinueButton();
		} else {
			for(int i = 0; i < _choices.Length; i++) {
				//Debug.Log (_choices[i]);
				_choiceButtons[i].SetActive (true);
				_choiceButtons[i].transform.FindChild ("Text").GetComponent<Text>().text = _choices[i];
			}
			HideContinueButton();
		}
	}

	private void SetPortrait() {
		_portraitImage.SetActive (true);
		Sprite tempSprite = Resources.Load<Sprite> (portraitPATH + _portrait);
		if (tempSprite != null) {
			_portraitImage.GetComponent<Image> ().sprite = tempSprite;
		} else {
			_portraitImage.SetActive (false);
			Debug.Log ("Portrait does not exist");
		}
	}

		
	void OnGUI() {
		if (!_showing) {
			//GameHUD.Instance.showMinimap = true;
			if(_dialogueGO.activeInHierarchy == true) {
				_dialogueGO.SetActive(false);
			}
			return;
		}

		//GameHUD.Instance.showMinimap = false;

		if (_dialogueGO.activeInHierarchy == false) {
			_dialogueGO.SetActive(true);
		}
	}
/*
		if (isGood) {
			//_goodBackground.SetActive (true);
			//_badBackground.SetActive (false);
			//_badPortrait.SetActive(false);
			//
			_goodBackgroundAndPortrait.SetActive (true);
			_badBackgroundAndPortrait.SetActive (false);
			if(_portrait != "") {
//				_goodPortrait.SetActive(true);
//				_portraitImage.SetActive(true);
				SetPortrait();
//			} else {
//				_goodPortrait.SetActive(false);
//				_portraitImage.SetActive(false);
			}

		} else {
			//_goodBackground.SetActive (false);
			//_badBackground.SetActive (true);
			//_goodPortrait.SetActive(false);
			//
			_goodBackgroundAndPortrait.SetActive (false);
			_badBackgroundAndPortrait.SetActive (true);
			if(_portrait != "") {
//				_badPortrait.SetActive(true);
//				_portraitImage.SetActive(true);
				SetPortrait();
//			} else {
//				_badPortrait.SetActive(false);
//				_portraitImage.SetActive(false);
			}
		}

		if (_choices == null || _choices.Length < 1) {
			for(int i = 0; i < _choiceButtons.Length; i++) {
				_choiceButtons[i].SetActive (false);
			}
			ShowContinueButton();
		} else {
			for(int i = 0; i < _choices.Length; i++) {
				//Debug.Log (_choices[i]);
				_choiceButtons[i].SetActive (true);
				_choiceButtons[i].transform.FindChild ("Text").GetComponent<Text>().text = _choices[i];
			}
			HideContinueButton();
		}
	}
	*/
	
	
	private void ShowContinueButton() {
		_continueButton.GetComponent<Image> ().enabled = true;
		_continueButton.GetComponent<Button> ().enabled = true;
		_continueButton.transform.FindChild ("Text").GetComponent<Text> ().enabled = true;
	}

	public void ClickedContinueButton() {
		Dialoguer.ContinueDialogue ();
	}

	public void ClickedChoiceButton(int index){
		Dialoguer.ContinueDialogue (index);
	}

	void addListener(Button b, int i){
		b.onClick.AddListener (() => ClickedChoiceButton (i));
	}

	private void HideContinueButton() {
		_continueButton.GetComponent<Image> ().enabled = false;
		_continueButton.GetComponent<Button> ().enabled = false;
		_continueButton.transform.FindChild ("Text").GetComponent<Text> ().enabled = false;
	}

	[EventVisibleAttribute]
	public void Speak(int npcID, int index) {

		if (_showing) {
			return;
		}

		JSONNode npcDialogue = RetrieveDialogueFromJSON();

		if (npcDialogue [npcID.ToString ()] == null) {
			Debug.Log("NPC ID: " + npcID + " doesn't exist, check dialogue list json file!");
			return;
		}

		if (npcDialogue [npcID.ToString ()] [index] == null) {
			Debug.Log("Index: " + index + " on NPC ID: " + npcID + " doesn't exist, check dialogue list json file!");
			return;
		}

		Dialoguer.StartDialogue (npcDialogue [npcID.ToString ()] [index].AsInt);

		return;
	}
    
	private JSONNode RetrieveDialogueFromJSON()
    {
        TextAsset file = Resources.Load<TextAsset>(DIALOGUELIST);
		if(file == null)
		{
			Debug.LogError ("Could not find Dialogue List JSON file");
			return null;
		}
		
		string jsonRead = file.text;
		JSONNode jsonParsed = JSON.Parse (jsonRead);
		
		return jsonParsed;
	}
}