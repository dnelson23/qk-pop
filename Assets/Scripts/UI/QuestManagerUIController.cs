using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QuestManagerUIController : MonoBehaviour {

	public GameObject questContainer;
	public GameObject questUI;
	public float spacing;

	GameObject player;
	GameObject moreQuestInfo;
	QuestManager qm;
	Button questButton;
	float buttonHeight;
	float newScrollVal;
	float qcHeight;
	Text moreQuestInfoTitle;
	Text moreQuestInfoDescription;
	Scrollbar mainScrollbar;
	Scrollbar moreInfoScrollbar;
	bool mainSelected;
	bool isScrolling;
	EventSystem theEventSystem;
	int lastButtonSelected;
	List<GameObject> allQuests;
	List<Quest>[] theLists;
	GameHUD gameHUD;


	void Start(){
		player = GameObject.Find ("_Player");
		if (player) {
			qm = player.GetComponent<QuestManager>();
		}
		else {
			Debug.LogError("QuestManagerUI Script attached to 'QuestManager' object could not find a player in the scene!");
		}
		if (!questContainer) {
			Debug.LogError("QuestManagerUI Script attached to 'QuestManager' object could not find a child GameObject called 'Quests' the prefab connection could be broken.");
		}
		moreQuestInfo = transform.FindChild ("MoreQuestInfo").gameObject;
		if (!moreQuestInfo) {
			Debug.LogError ("QuestManagerUI script could not find the child object called 'MoreQuestInfo' the prefab may be broken");
		}
		moreQuestInfoTitle = moreQuestInfo.transform.FindChild ("QuestTitle").GetComponent<Text> ();
		if (!moreQuestInfoTitle) {
			Debug.LogError ("QuestManagerUI script could not find the child object called 'QuestTitle' the prefab may be broken");
		}
		moreQuestInfoDescription = moreQuestInfo.transform.FindChild ("ScrollView").transform.FindChild ("QuestDescription").GetComponent<Text> ();
		if (!moreQuestInfoDescription) {
			Debug.LogError ("QuestManagerUI script could not find the child object called 'QuestDescription' the prefab may be broken");
		}
		mainScrollbar = transform.FindChild("MainScrollbar").GetComponent<Scrollbar>();
		if(!mainScrollbar){
			Debug.LogError ("QuestManagerUI script could not find the child object called 'MainScrollbar' the prefab may be broken");
		}
		moreInfoScrollbar = transform.FindChild("MoreQuestInfo").FindChild("MoreQuestInfoScrollbar").GetComponent<Scrollbar>();
		if(!moreInfoScrollbar){
			Debug.LogError ("QuestManagerUI script could not find the child object called 'MoreQuestInfoScrollbar' the prefab may be broken");
		}
		theEventSystem = GameObject.Find ("EventSystem").GetComponent<EventSystem>();
		if(!theEventSystem){
			Debug.LogError("QuestManagerUI script could not find the EventSystem in the scene. Make sure the scene has an EventSystem");
		}
		gameHUD = GameObject.Find ("_HUDManager").GetComponent<GameHUD> ();
		if (!gameHUD) {
			Debug.LogError("Could not find the 'GameHUD' script on the '_HUDManager' GameObject in the scene: " + Application.loadedLevelName);
		}
		questButton = questUI.GetComponent<Button> ();
		buttonHeight = questButton.GetComponent<RectTransform> ().sizeDelta.y;
		theLists = new List<Quest>[3];
		theLists[0] = qm.currentQuests;
		theLists[1] = qm.failedQuests;
		theLists[2] = qm.completedQuests;
		
		for(int i = 0; i < theLists.Length; i++){
			qcHeight += (theLists[i].Count * (buttonHeight + spacing) - spacing);
		}
		//qm.LoadQuests ();
	}

	void Update(){
		//For debugging, change to use input manager later.
		if (Input.GetKeyDown (KeyCode.F5)) {
			qm.LoadQuests();
			showQuests();
		}
		if((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow)) && mainSelected){
			isScrolling = true;
		}
		else{
			isScrolling = false;
		}
		if(isScrolling && (theLists[0].Count > 0 || theLists [1].Count > 0 || theLists[2].Count > 0)){
			mainScrollbar.value = 1 - ((Mathf.Abs(theEventSystem.currentSelectedGameObject.transform.localPosition.y) - 50.5f) / (qcHeight - 101f));
		}
		if(Input.GetKeyDown (KeyCode.Escape) && !mainSelected){
			allQuests[lastButtonSelected].GetComponent<Button>().Select();
			mainSelected = true;
		}
		if (Input.GetKeyDown (KeyCode.Escape) && mainSelected) {
			//go back to Journal
			gameHUD.HideQMUI();
		}
	}

	public void showQuests(){
		removeQuestUIobjects ();
		reorganizeQuests ();
	}

	/* This function is called when the quest UI elements need to be reorganized.
	 * Some times when this would be called is when attempting to add a quest or a quest is completed.
	 * This handles the calculation of the size of the scrollable area and physical placement of the quest UI elements in the quest manager.
	 * This function organizes quests by active, failed, then completed quests.
	 */
	public void reorganizeQuests(){
		allQuests = new List<GameObject> ();
		mainSelected = true;
		moreQuestInfoTitle.text = "";
		moreQuestInfoDescription.text = "";
		qcHeight = 0;
		for(int i = 0; i < theLists.Length; i++){
			qcHeight += (theLists[i].Count * (buttonHeight + spacing) - spacing);
		}
		RectTransform containerTransform = questContainer.GetComponent<RectTransform> ();
		containerTransform.sizeDelta = new Vector2 (100, qcHeight);
		newScrollVal = ((buttonHeight + spacing) + spacing) / qcHeight;

		mainScrollbar.value = 0.99f;
		moreInfoScrollbar.value = 0.99f;

		int iter = 0;
		for(int i = 0; i < theLists.Length; i++){
			for(int j = 0; j < theLists[i].Count; j++){
				GameObject newQuestButton = Instantiate(questUI, new Vector3(0, 0 - (iter * (buttonHeight + spacing) + (buttonHeight/2)), 0), Quaternion.identity) as GameObject;
				newQuestButton.transform.SetParent(questContainer.transform, false);
				allQuests.Add(newQuestButton);
				Text newButtonText = newQuestButton.transform.FindChild("Text").GetComponent<Text>();
				newButtonText.text = theLists[i][j].GetName();
				Button qb = newQuestButton.GetComponent<Button>();
				addListener(qb, iter);
				if (i == 0 && j == 0){
					qb.Select();
				}
				iter++;
			}
		}
		if (qm.questCount > 0) {
			showMoreQuestInfo (0);
		} 
		else {
			moreQuestInfoTitle.text = "No Quests!";
			moreQuestInfoDescription.text = "You have no quests! Go explore to find some!";
		}
		//StartCoroutine(showMoreInfoScrollbar());

		if (qm.questCount < 7) {
			GameObject scrollingHandle = mainScrollbar.transform.FindChild ("Sliding Area").transform.FindChild ("Handle").gameObject;
			scrollingHandle.SetActive (false);
		}
		else {
			GameObject scrollingHandle = mainScrollbar.transform.FindChild ("Sliding Area").transform.FindChild ("Handle").gameObject;
			scrollingHandle.SetActive (true);
		}
		StartCoroutine (showMoreInfoScrollbar ());
	}

	/* Each time a button is dynamically created, a listener is added to it which calls the clickButton() function.
	 * The listener helps keep track of the button that was pressed so it can be highlighted again when the user presses the back button.
	 */
	void addListener(Button b, int i){
		b.onClick.AddListener (() => clickButton (i));
	}

	/* The click button function is called when any of the dynamically created buttons are clicked.
	 * It displays the information for the quest in the gold box and selects the respective scrollbar.
	 */
	void clickButton(int iter){
		mainSelected = false;
		lastButtonSelected = iter;
		showMoreQuestInfo(iter);
		moreInfoScrollbar.Select();
		moreInfoScrollbar.value = 1f;
		StartCoroutine (showMoreInfoScrollbar ());
	}

	/* This coroutine is needed to fix a bug.
	 * Without the short delay that this adds into the code, the program was getting the previous value for the height of the moreQuestInfoDescription
	 */
	/*	IEnumerator showMoreInfoScrollbar(){
			Debug.Log ("Showing more info scrollbar. " + moreQuestInfoDescription.GetComponent<RectTransform>().rect.height.ToString());
			yield return new WaitForSeconds (0.00001f);
			Debug.Log ("yield returned new!");
			if(moreQuestInfoDescription.GetComponent<RectTransform>().rect.height < 560){
				moreInfoScrollbar.gameObject.SetActive(false);
				Debug.Log ("more info scroll bar is less than 560!");
			}
			else{
				moreInfoScrollbar.gameObject.SetActive(true);
				Debug.Log ("more info scroll bar is greater than 560!");
			}
		}
	*/

	IEnumerator showMoreInfoScrollbar(){
		while (true) {
			float delayTime = Time.realtimeSinceStartup + 0.00001f;
			while(Time.realtimeSinceStartup < delayTime){
				yield return null;
			}
			break;
		}
		if(moreQuestInfoDescription.GetComponent<RectTransform>().rect.height < 560){
			moreInfoScrollbar.gameObject.SetActive(false);
		}
		else{
			moreInfoScrollbar.gameObject.SetActive(true);
		}
	}
	
	/* Helper function that deletes all of the buttons that were dynamically created
	 */
	public void removeQuestUIobjects(){
		foreach (Transform child in questContainer.transform) {
			GameObject.Destroy(child.gameObject);
		}
	}

	/* This function populates the moreQuestinfo object with information on each quest.
	 * At the beginning a value is created from the iter integer that coresponds to which quest in which list is being selected.
	 * i is the index of the list in the list array theLists
	 * j is the index of the quest in the list
	 */
	void showMoreQuestInfo(int iter){

		int i;
		int j;
		if(iter < theLists[0].Count){
			i = 0;
			j = iter;
		}
		else if(iter < theLists[0].Count + theLists[1].Count){
			i = 1;
			j = iter - theLists[0].Count;
		}
		else{
			i = 2;
			j = iter - theLists[0].Count - theLists[1].Count;
		}
		moreQuestInfoTitle.text = theLists[i][j].GetName();
		Goal[] currQuestGoals = theLists[i][j].GetGoal();
		string goalText = "\n";
		if(theLists[i][j].HasTimer()){
			goalText += "\nQuest Duration: " + theLists[i][j].GetTimerLength () + " Seconds";
		}
		for(int x = 0; x < currQuestGoals.Length; x++){
			goalText += "\n" + currQuestGoals[x].GetName() + ": ";
			if(currQuestGoals[x].GetProgress() == -1){
				if(currQuestGoals[x].IsCompleted()){
					goalText += "Complete";
				}
				else{
					goalText += "Incomplete";
				}
			}
			else{
				 goalText += currQuestGoals [x].GetProgress () + " / " + currQuestGoals [x].GetProgressNeeded ();
			}
		}
        moreQuestInfoDescription.text = theLists[i][j].GetDescription() + "\n" + theLists[i][j].GetObjective() + goalText;
        GameObject moreQuestInfoScrollView = moreQuestInfo.transform.FindChild("ScrollView").gameObject;
	}
}
