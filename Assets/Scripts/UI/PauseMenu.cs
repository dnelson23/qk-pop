using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {
	private bool isPaused = false;

	public bool isOnPauseMenu = false;
	public GameHUD GHud;
    public MainMenuManager Options;
    public float speed = 2.0f;

    public GameObject mainHUD;
    public GameObject UIhud;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {                                // Pause must be on Update() if put on FixedUpdate()                                            
                                                   //Debug.Log(Time.timeScale);                  // The game will get stuck and will have to reset

        // if (InputManager.input.isPause){}
        //  if (InputManager.input.)
        if (Input.GetKeyDown(KeyCode.Escape)) {      // This will need to be changed to call inputManager  
			
			if(!isPaused && !isOnPauseMenu) {                         
				pauseGame();
			} else if(isPaused && isOnPauseMenu) {
				print ("in update of PM");
				unPauseGame();	
			}
		}

        if (InputManager.input.isActionPressed()) {

            Debug.Log("Pressing down");
            
       }

	}

	/* Shows and unlocks the mouse cursor if playing on PC
	 */
	public void showCursor(){
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	/* Hides the mouse cursor and locks it to the center of the screen
	 */
	public void hideCursor(){
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	// Pauses the game and brings up the Pause Menu
	public void pauseGame()
    {                       
		isOnPauseMenu = true;
		isPaused = true;
		showCursor ();
        Time.timeScale = 0f;
        GHud.showPauseMenu();
        if (Options != null)
        {
            Options.mainCanvas.SetActive(false);
        }
	}

	// Unpauses the game and hides menu
	public void unPauseGame() {
		Debug.Log ("Unpausing Game");
		isOnPauseMenu = false;
		isPaused = false;
		hideCursor ();
		Time.timeScale = 1f;
		GHud.hidePauseMenu();
	}

	// When button is used to unpause this function is called
	public void unPauseGameBtt() {                  
		isPaused = false;
		Time.timeScale = 1f;
		
	}

	// It is called if game needs to be paused w/o Menu
    public void setPause() {                        
        isPaused = true;
        Time.timeScale = 0f;
    }

	// It is called to set the time back to normal .. to unpause the game
    public void setTimeNormal() {                   
        isPaused = false;
        Time.timeScale = 1f;
    }

	/* Manipulate time if needed by getting a float and setting it
	 * to Time.timeScale ... Slows down Time or Speeds up Time
	 */
    public void manipulateTime(float speed) {
        Time.timeScale = speed;
    }

	public void openOptions() {        
		// UIhud.SetActive(false);
		mainHUD.SetActive(true);     
		Options.GoToOptions();
	}

	public void OpenJournal(){

	}
}
