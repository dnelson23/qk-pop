using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;
//using Debug = FFP.Debug;
 
public class QuestSaveManager : SaveManager {

	QuestManager _questManager;
	Quest _quest;

	public void SaveQuests(List<Quest> currQuests) {
		if (currQuests == null || currQuests.Count == 0) {
			return;
		}

		JSONClass questJSONNode = new JSONClass ();

		int count = 0;
		foreach (Quest q in currQuests) {
			questJSONNode["Quests"] = GenerateProgressJSON(q);
			count++;
		}

		Debug.Log(questJSONNode.ToString ());
		PlayerPrefs.SetString ("PlayerQuests", questJSONNode.ToString ());
	}

	JSONClass GenerateProgressJSON(Quest q) {
		JSONClass progress = new JSONClass ();

		for(int i = 0; i < q.GetGoal().Count(); i++) {
			if(q.GetGoal()[i].GetProgress() > -1) {
				progress[q.GetID().ToString()][-1].AsInt = q.GetGoal()[i].GetProgress();
				Debug.Log("added progress, progress = " + q.GetGoal()[i].GetProgress().ToString());
			}
			else {
				progress[q.GetID().ToString()][-1] = "null";
				Debug.Log("did not add progress");
				
			}
		}
		return progress;
	}
	
	public void SaveCompletedQuest(Quest questToSave) {
		if (PlayerPrefs.HasKey ("CompletedQuests") == true) {
			JSONNode completedQuests = JSONClass.Parse(PlayerPrefs.GetString("CompletedQuests"));
			for(int i = 0; i < completedQuests["CompletedQuest"].Count; i++){
				if(completedQuests["CompletedQuests"][i] == questToSave.GetID().ToString()){
					completedQuests["CompletedQuests"][-1] = questToSave.GetID().ToString();
					PlayerPrefs.SetString("CompletedQuests", completedQuests.ToString());
					return;
				}
			}
		}

		JSONClass completedQuestJSONNode = new JSONClass ();
		completedQuestJSONNode ["CompletedQuests"] [0] = questToSave.GetID ().ToString();
		PlayerPrefs.SetString("CompletedQuests", completedQuestJSONNode.ToString());
		return;

	}

	public bool CompletedQuest(int questID) {
        return false;   // TODO The list of completed quest only contained one entry, no matter how many were added
		if (PlayerPrefs.HasKey ("CompletedQuests") == true) {
			JSONNode completedQuests = JSONClass.Parse(PlayerPrefs.GetString("CompletedQuests"));
			for(int i = 0; i < completedQuests["CompletedQuests"].Count; i++) {
				if(completedQuests["CompletedQuests"][i] == questID.ToString()) {
					return true;
				}
			}
		} else {
			return false;
		}
		return true;
	}

	public List<Quest> LoadQuests() {

		if (PlayerPrefs.HasKey("PlayerQuests")) {

			_quest = new Quest (null, null, null, -1, null);
			List<Quest> quests = new List<Quest> ();
			JSONNode loadedQuests = JSONClass.Parse (PlayerPrefs.GetString ("PlayerQuests"));

			for (int i = 0; i < loadedQuests["Quests"].Count; i++) {

				int[] progress = new int[loadedQuests ["Quests"] [i].Count];

				for (int j = 0; j < loadedQuests["Quests"][i].Count; j++) {
					progress [j] = loadedQuests ["Quests"] [i] [j].AsInt;
				}

				Quest newQuest = _quest.AddQuestFromSave (loadedQuests ["Quests"] [i].AsInt, progress);
				quests.Add (newQuest);
			}
			return quests;

		}

		return null;
	}

	public int DebugFunctionDeleteLater(){
		return 10;
	}

}