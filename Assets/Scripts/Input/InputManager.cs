using UnityEngine;
using System.Collections.Generic;
using Debug = FFP.Debug;

//! Class manages different InputTypes and chooses one to be the active input type
public sealed class InputManager : MonoBehaviour
{
	public static bool changeSkills = false;
	public enum Inputs { UI, Game, Keyboard };

	//Singleton variable
	private static InputManager _instance;
	public static InputManager instance {
		get { return _instance ?? (_instance = GameObject.FindObjectOfType<InputManager>());} 
		private set { }
	}

	private Inputs _curInput { get; set; }
	public static InputType input 
	{ 
		get { return InputManager.instance.inputs[InputManager.instance._curInput]; }
		private set { }
	}

	private Dictionary<Inputs, InputType> inputs = new Dictionary<Inputs, InputType>();

    void Awake()
    {
        _instance = null;
    }

	void Start()
    {
		inputs.Add(Inputs.UI, this.gameObject.AddComponent<UIInputType>());
		inputs.Add(Inputs.Game, this.gameObject.AddComponent<GameInputType>());
		inputs.Add(Inputs.Keyboard, this.gameObject.AddComponent<KeyboardInputType>());

		if(inputs.Count > 0)
			ChangeInputType(Inputs.Game);
		else
			Debug.Error("input", "InputManager.inputs is empty.");

		ChangeInputType(Inputs.Game);
	}

    //!Switch input type
    public static void ChangeInputType(Inputs inputType) {
		if(InputManager.instance.inputs.ContainsKey(inputType))
			InputManager.instance._curInput = inputType;
	}

    // !Returns the mapped key by input name
    public string GetGameInputKey(string input)
    {
        if (inputs[Inputs.Game].keyButtons.ContainsKey(input))
        {
            Debug.Warning("GameInput has no input for \"" + input + "\".");
            return "";
        }
        else
        {
            return inputs[Inputs.Game].keyButtons[input];
        }
    }
}
