using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTALK.Localization
{

	/// <summary>
	/// This class that keep every text asset for every language
	/// </summary>
	[System.Serializable]
	public class LanguageString{
		[Tooltip("Language of the string below")]
		public SupportedLanguages language;
		[Tooltip("What should be the text if the the language above is chosen?")]
		public string thisString;
	}

	[AddComponentMenu("Seize Studios/RPGTalk/Localization/RPGTalk Localization String")]
	public class RPGTalkLocalizationString : MonoBehaviour {

		[Tooltip("Here you should set all your languages and possible strings")]
		public List<LanguageString> language;

		// Use this for initialization
		void Start () {
			ChangeCurrentTextToActualLanguage ();
		}

		/// <summary>
		/// Changes the current text to actual language.
		/// </summary>
		public void ChangeCurrentTextToActualLanguage(){
			//If we have a Text component attached to this gameObject, check it already if it should change the text
			if(GetComponent<Text>()){
				GetComponent<Text> ().text = CheckForCorrectLanguage ();
			}
		}

		/// <summary>
		/// Checks for correct language to use
		/// </summary>
		/// <returns>The string for the correct language set in 'language' list</returns>
		public string CheckForCorrectLanguage(){
			
			//find out what language we are on and select the text
			for (int i = 0; i < language.Count; i++) {
				if (language [i].language == LanguageSettings.actualLanguage) {
					return language [i].thisString;
				}
			}

			return "";

		}
	}

}