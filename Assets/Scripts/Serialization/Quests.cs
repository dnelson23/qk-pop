using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System;
//using Debug = FFP.Debug;

public class Quest {

	const string questListFilePath = "questList";

	int iden;
	string name;
	string description;
	string objective;
	Goal[] goal;
	int progress = 0;
	int duration;
	bool timer = false;
	bool failed = false;

	public Quest(string n, string d, string o, int i, Goal[] newGoals ) {
		name = n;
		description = d;
		objective = o;
		iden = i;
		goal = newGoals;
	}

	public Quest (string n, string d, string o, int t, int i, Goal[] newGoals) {
		name = n;
		description = d;
		objective = o;
		duration = t;
		iden = i;
		goal = newGoals;
		timer = true;
	}

	private bool loadListFromFile(string filePath){
		if(!System.IO.File.Exists(Application.dataPath + filePath)) {
			Debug.Log( "File does not exist: " + Application.dataPath + filePath);
			return false;
		}
		string json = System.IO.File.ReadAllText(Application.dataPath + filePath);
		string platform = Application.platform.ToString();
		//return loadListFromJson(json, platform);
		return true;
	}

	public Quest AddQuest(int id) {
		JSONNode quests = RetrieveQuestsFromJSON ();
		char[] deliminerChars = {'"'};

		if (quests[id.ToString()] == null) {
			//Debug.Error("Quest ID: " + id + " doesn't exist! Make sure it is added into the JSON file.");
			return null;
		}
		if (quests [id.ToString()].Count == 4) {

			Goal[] newGoals = new Goal[quests[id.ToString ()][3].Count];
			//Debug.Log("There are " + quests[id.ToString ()][3].Count + " goals");


			for(int i = 0; i < quests[id.ToString ()][3].Count; i++) {

				if(quests[id.ToString ()][3][i][0] == null) {
					Goal newGoal = new Goal(quests[id.ToString ()][3][i]);
					newGoals[i] = newGoal;
				}
				else {
					string[] goalText = quests[id.ToString()][3][i].ToString().Split(deliminerChars);
					Goal newGoal = new Goal(goalText[1],  Convert.ToInt32(goalText[3]));
					Debug.Log( quests[id.ToString ()][3][i].Keys.ToString());
					newGoals[i] = newGoal;
				}
			}
			Quest newQuest = new Quest(quests[id.ToString ()][0], quests[id.ToString ()][1], quests[id.ToString ()][2], id, newGoals);
			return newQuest;
		}

		if (quests [id.ToString()].Count == 5) {
			Goal[] newGoals = new Goal[quests[id.ToString ()][4].Count];

			for(int i = 0; i < quests[id.ToString ()][4].Count; i++) {
				Goal newGoal = new Goal(quests[id.ToString ()][i]);
				newGoals[i] = newGoal;
			}
			Quest newQuest = new Quest(quests[id.ToString ()][0], quests[id.ToString ()][1], quests[id.ToString ()][2], quests[id.ToString ()][3].AsInt, id, newGoals);
			return newQuest;
		}
		Debug.Log ( "Not the right amount of parameters in questList.json for Quest ID:" + id);
		return null;
	}

	public Quest AddQuestFromSave(int id, int[] progress) {
		JSONNode quests = RetrieveQuestsFromJSON ();
		
		
		
		if (quests[id.ToString()] == null) {
			Debug.LogError("Quest ID: " + id + " doesn't exist! Make sure it is added into the JSON file.");
			return null;
		}
		if (quests [id.ToString()].Count == 4) {
			
			Goal[] newGoals = new Goal[quests[id.ToString ()][3].Count];

			for(int i = 0; i < quests[id.ToString ()][3].Count; i++) {
				
				if(quests[id.ToString ()][3][i][0] == null) {
					Goal newGoal = new Goal(quests[id.ToString ()][3][i]);
					newGoals[i] = newGoal;
				}
				else {
					Goal newGoal = new Goal(quests[id.ToString ()][3][i], quests[id.ToString ()][3][i][0].AsInt, progress[i]);
					newGoals[i] = newGoal;
				}
			}
			Quest newQuest = new Quest(quests[id.ToString ()][0], quests[id.ToString ()][1], quests[id.ToString ()][2], id, newGoals);
			return newQuest;
		}
		if (quests [id.ToString()].Count == 5) {
			Goal[] newGoals = new Goal[quests[id.ToString ()][4].Count];
			
			for(int i = 0; i < quests[id.ToString ()][4].Count; i++) {
				Goal newGoal = new Goal(quests[id.ToString ()][i]);
				newGoals[i] = newGoal;
			}
			Quest newQuest = new Quest(quests[id.ToString ()][0], quests[id.ToString ()][1], quests[id.ToString ()][2], quests[id.ToString ()][3].AsInt, id, newGoals);
			return newQuest;
		}
		return null;
	}


	private JSONNode RetrieveQuestsFromJSON()
    {
        TextAsset json = Resources.Load<TextAsset>(questListFilePath);

        if(json == null)
        {
            Debug.LogError("QuestList not loaded from JSON");
            return null;
        }


		string jsonRead = json.text;
		JSONNode jsonParsed = JSON.Parse (jsonRead);

		return jsonParsed;
	}

	public int GetID() {
		return iden;
	}

	public string GetName() {
		return name;
	}

	public string GetDescription(){
		return description;
	}

	public string GetObjective(){
		return objective;
	}

	public bool IsCompleted() {
		bool allCompleted = true;
		foreach (Goal g in goal) {
			if(g.IsCompleted() == false) {
				allCompleted = false;
				break;
			}
		}
		if(allCompleted){
			GameHUD.Instance.calcCompass = false;
		}
		return allCompleted;
	}

	public void CompleteGoalInQuest(int goalIndex) {
		if (goal [goalIndex] == null) {
			Debug.Log( "Goal " + goalIndex + " does not exist!");
			return;
		}

		if (goal [goalIndex].IsCompleted () == true) {
			Debug.Log( "Goal already completed!");
		}

		goal [goalIndex].Complete ();
		return;
	}

	public void ProgressGoalInQuest(int goalIndex) {
		if (goal [goalIndex] == null) {
			Debug.Log( "Goal " + goalIndex + " does not exist!");
			return;
		}
		
		if (goal [goalIndex].IsCompleted () == true) {
			Debug.Log( "Goal already completed!");
		}
		
		goal [goalIndex].Progress ();
		return;
	}

	public Goal[] GetGoal() {
		return goal;
	}

	public bool HasTimer() {
		return timer;
	}

	public int GetTimerLength() {
		return duration;
	}

	public void Fail() {
		failed = true;
		return;
	}

	public bool IsFailed() {
		return failed;
	}
}