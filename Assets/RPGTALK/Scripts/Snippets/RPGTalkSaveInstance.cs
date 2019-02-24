using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTALK.Snippets
{
    [AddComponentMenu("Seize Studios/RPGTalk/Snippets/Save Instance")]
    [ExecuteInEditMode]
    public class RPGTalkSaveInstance : MonoBehaviour
    {

        public bool saveBetweenPlays;
        [Header("Check the checkbox below to erase all saved data")]
        public bool erase;

        RPGTalk rpgTalk;

        // Start is called before the first frame update
        void Start()
        {
            rpgTalk = GetComponent<RPGTalk>();
            rpgTalk.OnMadeChoice += SaveData;
        }

        private void Update()
        {
            if (erase)
            {
                erase = false;
                PlayerPrefs.DeleteAll();
                if (saveBetweenPlays)
                {
                    PlayerPrefs.Save();
                }
            }
        }

        private void OnDestroy()
        {
            rpgTalk.OnMadeChoice -= SaveData;
        }

        public void SaveData(string choiceID, int answerID)
        {
            PlayerPrefs.SetInt(choiceID, answerID);
            if (saveBetweenPlays)
            {
                PlayerPrefs.Save();
            }
        }

        public bool GetSavedData(string savedData, int modifier)
        {
            if (PlayerPrefs.HasKey(savedData) && PlayerPrefs.GetInt(savedData) == modifier)
            {
                return true;
            }

            return false;
        }
    }
}