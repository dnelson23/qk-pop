using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using Debug = FFP.Debug;

//! InputType responsible for normal QK in game movement controls
public class GameInputType : InputType
{
	protected bool dPadUp { get { return Input.GetAxis(GetInputOrFail(controllerButtons, "dPadVertString")) > 0; } }
	protected bool dPadDown { get { return Input.GetAxis(GetInputOrFail(controllerButtons, "dPadVertString")) < 0; } }
	protected bool dPadRight { get { return Input.GetAxis(GetInputOrFail(controllerButtons, "dPadHorString")) > 0; } }
	protected bool dPadLeft { get { return Input.GetAxis(GetInputOrFail(controllerButtons, "dPadHorString")) < 0; } }


    void Awake()
    {
        keyButtons = new Dictionary<string, string>();
        controllerButtons = new Dictionary<string, string>();

        if (!loadListFromFile(StringManager.INPUTKEYS))
        {
            Debug.Log("input", "JSON file did not load");
        }
        else {
            Debug.Log("input", "JSON file loaded");
        }
    }

	string GetInputOrFail(Dictionary<string, string> type, string input)
	{
        try {
			return GetKey(type, input);
		}
		catch (System.ArgumentException e) {
			Debug.Log ("input", "INPUTMANAGER: " + e.Message);
			return null;
		}
	}

	string GetKey(Dictionary<string, string> type, string input)
	{
		if (type.ContainsKey (input))
			return type [input];
		else
			throw new System.ArgumentException("InputManager has no input called \"" + input + "\"");
	}

	public override float CameraVerticalAxis() {
		if(Input.GetAxis("Mouse Y") != 0)
			return Input.GetAxis("Mouse Y");
		else if(Input.GetAxis(GetInputOrFail(controllerButtons, "controllerRightVert")) != 0)
			return Input.GetAxis(GetInputOrFail(controllerButtons, "controllerRightVert"));
			
		if(Input.GetAxis("Mouse Y") > 0 || Input.GetAxis(GetInputOrFail(controllerButtons, "controllerRightVert"))>0){
			return 1;
		}else if(Input.GetAxis("Mouse Y") < 0 || Input.GetAxis(GetInputOrFail(controllerButtons, "controllerRightVert")) < 0){
			return -1;
		}
		return 0;
	}

	public override float CameraHorizontalAxis() {
		if(Input.GetAxis("Mouse X") != 0)
			return Input.GetAxis("Mouse X");
		else if(Input.GetAxis(GetInputOrFail(controllerButtons, "controllerRightHor")) != 0)
			return Input.GetAxis(GetInputOrFail(controllerButtons, "controllerRightHor"));
			
		if(Input.GetAxis("Mouse X") > 0 || Input.GetAxis(GetInputOrFail(controllerButtons, "controllerRightHor"))>0){
			return 1;
		}else if(Input.GetAxis("Mouse X") < 0 || Input.GetAxis(GetInputOrFail(controllerButtons, "controllerRightHor")) < 0){
			return -1;
		}
		return 0;
	}

	public override float MoveVerticalAxis() {
		if(Input.GetKey(GetInputOrFail(keyButtons, "forward")) && Input.GetKey(GetInputOrFail(keyButtons, "backward")))
			return 0;
		else if(Input.GetKey(GetInputOrFail(keyButtons, "forward"))) {
			return 1;
		} else if(Input.GetKey(GetInputOrFail(keyButtons, "backward")))
			return -1;
		else if(Input.GetAxis(GetInputOrFail(controllerButtons, "controllerVert")) != 0) {
            return Input.GetAxis(GetInputOrFail(controllerButtons, "controllerVert"));
		} 
		return 0;
	}
	public override float MoveHorizontalAxis() {
        string left = GetInputOrFail(keyButtons, "left");
        string right = GetInputOrFail(keyButtons, "right");

		if(Input.GetKey(left) && Input.GetKey(right))
			return 0;
		else if(Input.GetKey(GetInputOrFail(keyButtons, "right")))
			return 1;
		else if(Input.GetKey(GetInputOrFail(keyButtons, "left")))
			return -1;
		else if(Input.GetAxis(GetInputOrFail(controllerButtons, "controllerHor")) != 0) {
            return Input.GetAxis(GetInputOrFail(controllerButtons, "controllerHor"));
		} 
		return 0;
	}
	
	public override bool AbilityPressed() {
		if(Input.GetButton(GetInputOrFail(controllerButtons, "controllerAction"))) {
			return true;
		}
		return Input.GetKey(GetInputOrFail(keyButtons, "qAbility1"));
	}

	public override bool isCrouched() {
		if(Input.GetButton(GetInputOrFail(controllerButtons, "controllerCrouch"))) {
			return true;
		}
		return Input.GetKey(GetInputOrFail(keyButtons, "crouch"));
	}
	public override bool isSprinting() {
		if(Input.GetButton(GetInputOrFail(controllerButtons, "controllerSprint"))) {
			return true;
		}
		return Input.GetKey(GetInputOrFail(keyButtons, "sprint"));
	}
	public override bool isJumping() {
		if(Input.GetButton(GetInputOrFail(controllerButtons, "controllerJump"))) {
			return true;
		}
		return Input.GetKey(GetInputOrFail(keyButtons, "jump"));
	}
	public override bool isActionPressed() {
		if(Input.GetButton(GetInputOrFail(controllerButtons, "controllerAction"))) {
			return true;
		}
		return Input.GetKey(GetInputOrFail(keyButtons, "action"));
	}
	public override bool isTargetPressed() {
		if(Input.GetButton(GetInputOrFail(controllerButtons, "controllerTarget"))) {
			return true;
		}
		return Input.GetKeyDown(GetInputOrFail(keyButtons, "target"));
	}
	public override bool isCameraResetPressed() {
		if(Input.GetButton(GetInputOrFail(controllerButtons, "controllerCamReset"))) {
			return true;
		}
		return Input.GetKeyDown(GetInputOrFail(keyButtons, "cameraReset"));
	}
	public override bool isAbilityEquip() {
		if(dPadUp) {
			return true;
		} 
		return Input.GetKey(GetInputOrFail(keyButtons, "abilityEquip"));
	}
	public override bool isNotifications(){
		if(dPadDown) {
		return true;
		}
		return Input.GetKey(GetInputOrFail(keyButtons, "notifications"));
	}
	public override bool isCompass() {
		if(dPadLeft) {
			return true;
		}
		return Input.GetKey(GetInputOrFail(keyButtons, "compass"));
	}
	public override bool isJournal() {
		if(dPadRight) {
			return true;
		}
		return Input.GetKey(GetInputOrFail(keyButtons, "journal"));
	}
	public override int CameraScrollTarget() {
		if(!InputManager.changeSkills)
			return ScrollTarget();
		else
			return 0;
	}
	public override int ScrollTarget() {
		if(Input.GetAxis("Mouse ScrollWheel") > 0 || 
			Input.GetAxis(GetInputOrFail(controllerButtons, "controllerNextAbility"))>0){
			return 1;
		}else if(Input.GetAxis("Mouse ScrollWheel") < 0 || 
			Input.GetAxis(GetInputOrFail(controllerButtons, "controllerPreviousAbility")) < 0){
			return -1;
		}
		return 0;
	}


	// TODO use InputSerialization to save custom keymappings to file
	public override void SaveInput(){
		//InputSerialization.SaveInput (inputs);
	}

	// TODO use InputSerialization to load custom keymappings from file
	public override void LoadInput(){
		//Dictionary<string, string> inputs = InputSerialization.LoadInput ();
	}
	
	//JSON load stuff
	private bool loadListFromFile(string filePath)
    {
        TextAsset loadedFile = Resources.Load<TextAsset>(filePath);

		if(loadedFile == null) {
			Debug.Log("input", "File does not exist: " + filePath);
            throw new System.Exception("File \"" + filePath + "\" does not exit.");
		}

		string json = loadedFile.text;
		string platform = Application.platform.ToString();
		return loadListFromJson(json, platform);
	}

	// Currently only loads input from JSON file at /Resources/inputManager.json
	// TODO add check for custom input file, load from that if exists
	private bool loadListFromJson(string json, string platform)
    {
		JSONClass inputs = JSON.Parse (json) as JSONClass;
		if(inputs == null) {
			Debug.Log("input", "Json file is empty");
			return false;
		}
		JSONClass cont = inputs["Keyboard"] as JSONClass;
		for(int i = 0; i < cont.Count; i++) {
			keyButtons.Add (cont.Key(i), cont[i].Value);
		}
		cont = inputs ["Controller"]["XboxOne"] as JSONClass;
		for(int i = 0; i < cont.Count; i++) {
			controllerButtons.Add (cont.Key(i), cont[i].Value);
		}

		return true;
	}

}
