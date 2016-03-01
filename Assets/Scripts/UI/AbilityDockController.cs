using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AbilityDockController : MonoBehaviour {
	
	public Image[] abilities;
	public float timeTakenDuringLerp = 0.35f;
	public Image selectionBeam;
	public Image highligtedIcon;

	int[] position;
	float xPosition;
	float timeStartedLerping;
	int numAbilities;
	int selectedAbility;
	bool rotating;
	bool opening;
	bool closing;
	bool canGetInput;
	Vector3[] targetPos = new Vector3[5];

    float wheelDelay = 0;

	void Start () {
		canGetInput = true;
		opening = false;
		rotating = false;
		closing = false;
		selectionBeam.rectTransform.sizeDelta = new Vector2 (0, 0);
		selectionBeam.transform.position = new Vector3 (selectionBeam.transform.position.x, 55, 0);
		position = new int[abilities.Length];
		numAbilities = position.Length;
		for(int i = 0; i < numAbilities; i++){
			position[i] = i;
		}
		hideIcons ();
		xPosition = abilities [0].transform.position.x;

		setSelectedAbility (1); 								//Sets the default ability to "Push"

		abilities[selectedAbility].transform.SetAsLastSibling();
	}
	
	void Update () {

		if (Input.GetKeyDown (KeyCode.Tab)) {					//Open ability dock
			targetPosition();
			opening = true;
			closing = false;
			showIcons();
			startLerping();
		}
		else if (Input.GetKey(KeyCode.Tab)) {	//Ability dock is open
            if (Input.GetKeyDown (KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel") < 0) {			//Scroll the icons down
				newPos (false);
				rotating = true;
				startLerping();
			}
			if (Input.GetKeyDown (KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel") > 0) {			//Scroll the icons up
				newPos (true);
				rotating = true;
				startLerping();
			}
		}
		else if (Input.GetKeyUp (KeyCode.Tab)) {				//Close ability dock
			closedPosition();
			closing = true;
			opening = false;
			selectedAbility = position [2];
			abilities[selectedAbility].transform.SetAsLastSibling();
			startLerping();
		}

        /*if (wheelDelay <= 0)
        {
            if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            {
                if (selectedAbility <= 0)
                {
                    selectedAbility = abilities.Length;
                }
                setSelectedAbility(selectedAbility - 1);
                wheelDelay = 0.25f;
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            {
                if (selectedAbility >= abilities.Length - 1)
                {
                    selectedAbility = -1;
                }
                setSelectedAbility(selectedAbility + 1);
                wheelDelay = 0.25f;
            }
        }
        else
        {
            wheelDelay -= Time.deltaTime;
        }*/

    }

	void FixedUpdate(){
		if (opening || rotating || closing) {
			float timeSinceStarted = Time.time - timeStartedLerping;
			float percentageComplete = timeSinceStarted / timeTakenDuringLerp;
			if(opening){
				highligtedIcon.transform.position = Vector3.Lerp(highligtedIcon.transform.position, new Vector3(highligtedIcon.transform.position.x, 350, 0), percentageComplete);
				selectionBeam.rectTransform.sizeDelta = Vector2.Lerp(selectionBeam.rectTransform.sizeDelta, new Vector2(150, 650), percentageComplete);
				selectionBeam.transform.position = Vector3.Lerp (selectionBeam.transform.position, new Vector3(selectionBeam.transform.position.x, 350, 0), percentageComplete);
			}
			if(closing){
				highligtedIcon.transform.position = Vector3.Lerp(highligtedIcon.transform.position, new Vector3(highligtedIcon.transform.position.x, 100, 0), percentageComplete);
				selectionBeam.rectTransform.sizeDelta = Vector2.Lerp(selectionBeam.rectTransform.sizeDelta, new Vector2(0, 0), percentageComplete);			
				selectionBeam.transform.position = Vector3.Lerp (selectionBeam.transform.position, new Vector3(selectionBeam.transform.position.x, 100, 0), percentageComplete);
			}
			for (int i = 0; i < numAbilities; i++) {
				abilities [i].transform.position = Vector3.Lerp(abilities[i].transform.position, targetPos[i], percentageComplete);
			}

			/* This allows for a slight delay between button presses.
			 * Before this fix, if the user were to spam one of the directional keys, all the abilities would clump together.
			 * This did not look good at all.
			 */
			if(percentageComplete >= .35f){
				if(closing){
					hideIcons();
				}
				canGetInput = true;
			}
			if(percentageComplete >= 1f){
				rotating = false;
				opening = false;
				closing = false;
			}
		}
	}

	/* This function sets a few basic values to help with lerping the icons
	 */
	void startLerping(){
		canGetInput = false;
		timeStartedLerping = Time.time;
	}

	/* This function disables all of the icons that are not the icon of the "Selected Ability"
	 * Some of the icons are semi-transparent so they are all turned off when the ability dock is closed to avoid overlapping
	 */
	void hideIcons(){
		for (int i = 0; i < abilities.Length; i++) {
			if(i != selectedAbility){
				abilities[i].enabled = false;
			}
		}
	}

	/* This function turns all the icons back on when the ability dock is opening
	 */
	void showIcons(){
		for (int i = 0; i < abilities.Length; i++) {
			abilities[i].enabled = true;
		}
	}

	/* Helper function that gets the target position for where the ability icon will travel next.
	 * It takes in a boolean that should be true when the up arrow is pressed and false when the down arrow is.
	 * The "position" array helps figure out where the ability should travel next.
	 * The if statements before and after the for loop change the order of the abilities so they do not cross over each other when moving.
	 * When an ability needs to travel from the top to the bottom or vice-versa it is set to the back so it travels behind all other icons.
	 */
	void newPos(bool isUp){
		if (isUp) {
			abilities[position[0]].transform.SetSiblingIndex (0);
		}
		for (int i = 0; i < position.Length; i++) {
			if(!isUp){
				position[i] = modulo(position[i] - 1, 5);
			}
			else{
				position[i] = modulo(position[i] + 1, 5);
			}
		}
		targetPosition ();
		if (!isUp) {
			abilities[position[0]].transform.SetSiblingIndex (0);
		}
	}

	/* This function sets the target position for each of the icons.
	 * Function is called once when opening the dock and once each time the user scrolls up or down in the selector.
	 */
	void targetPosition(){
		for (int i = 0; i < position.Length; i++) {
			targetPos [i] = new Vector3 (xPosition, getPos (position [4-i]), 0);
		}
	}

	/* This function sets the target position of each of the icons to the location of the lowest icon.
	 * When lerping the icons to this position, the dock closes.
	 */
	void closedPosition(){
		for (int i = 0; i < position.Length; i++) {
			targetPos [i] = new Vector3 (xPosition, getPos (4), 0);
		}
	}

	/* Helper function that returns the modulo. This correctly calculates negative numbers as the % oeprator does not by itsself.
	 */
	int modulo(int a, int b){
		return (a%b + b)%b;
	}

	/* Helper function that returns the Y value for the next position. 
	 * 
	 * This only works with 5 abilities!
	 * 
	 * If you want it to work with more or less abilities, a function that dynamically generates the next y position will be needed.
	 */
	float getPos(int i){
		switch(i){
		case 0:
			return 600;
		case 1:
			return 475;
		case 2:
			return 350;
		case 3:
			return 225;
		case 4:
			return 100;
		default:
			return 0;
		}
	}

	/* This function returns an integer that corresponds to the ability that is selected
	 * 0 is Pull
	 * 1 is Push
	 * 2 is Taze	
	 * 3 is Sound Throw
	 * 4 is Cut
	 */
	public int getSelectedAbility(){
		return selectedAbility;
	}

	/* This public function allows a developer to set a default ability if they wanted to for a certain mission or level.
	 * The function takes in an integer that equips the ability in correspondence with the values below.
	 * 0 is Pull
	 * 1 is Push
	 * 2 is Taze
	 * 3 is Sound Throw
	 * 4 is Cut
	 */
	public void setSelectedAbility(int abilityIndex){
		for (int i = 0; i < position.Length; i++) {
			int pos = modulo(i + 2, 5);
			position[pos] = modulo(abilityIndex + i, 5);
		}
		selectedAbility = abilityIndex;
		abilities[selectedAbility].enabled = true;
		hideIcons();
	}
}