using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTALK.Localization
{
	/// <summary>
	/// An Enum with the supported languages. You can edit this enum at will to add more languages to your game
	/// </summary>
	public enum SupportedLanguages{
		EN_US,ES,FR,PT_BR
	}

	/// <summary>
	/// The settings of the current language
	/// </summary>
	public class LanguageSettings{
		/// <summary>
		/// What is the default language of your game? The language here should be the language that you set every "txtToParse"
		/// in RPGTalk. If no equivalent localization is found, this language will be shown
		/// </summary>
		public static SupportedLanguages defaultLanguage = SupportedLanguages.EN_US;

		/// <summary>
		/// What language is the game currently in?
		/// </summary>
		public static SupportedLanguages actualLanguage {
			set {
				_actualLanguage = value;
				//if we set the actual language, check for any RPGTalkLocalizationStrings in the scene
				foreach (RPGTalkLocalizationString s in GameObject.FindObjectsOfType<RPGTalkLocalizationString>()) {
					s.ChangeCurrentTextToActualLanguage ();
				}
			}
			get {
				return _actualLanguage;
			}
		}
		static SupportedLanguages _actualLanguage = defaultLanguage;
	}

	/// <summary>
	/// This class that keep every text asset for every language
	/// </summary>
	[System.Serializable]
	public class LanguageTXT{
		[Tooltip("Language of the text assets below")]
		public SupportedLanguages language;
		[Tooltip("All the text assets available in this language. It is important that they are set at the same order for every language. For instance: if 'Cutscene_1_EN' is the element 0 in its array, 'Cutscene_1_ES' must be also 0 in its array.")]
		public TextAsset[] txtToParse;
	}

	[AddComponentMenu("Seize Studios/RPGTalk/Localization/RPGTalk Localization")]
	public class RPGTalkLocalization : MonoBehaviour {

		[Tooltip("Here you should set all your languages and possible text assets")]
		public List<LanguageTXT> language;

		/// <summary>
		/// The singleton is the instance of the RPGTalkLocalization, so you can call it from whatever script you like
		/// using RPGTalkLocalization.singleton
		/// </summary>
		public static RPGTalkLocalization singleton;

		void Awake(){
			singleton = this;
			//This object often is the same in every scene, so we don't need to create it more than once.
			DontDestroyOnLoad (gameObject);
		}

		/// <summary>
		/// Given a base text, returns the textAsset for the actual language
		/// </summary>
		/// <returns>Text Asset for the actual language</returns>
		/// <param name="baseTxt">Base text in the default language.</param>
		public TextAsset CheckForCorrectLanguage(TextAsset baseTxt){

			//Find out what language in the list is the default language, then, find out what is the id of the txt in that language
			int idOfTheTXT = 0;
			for (int i = 0; i < language.Count; i++) {
				if (language[i].language == LanguageSettings.defaultLanguage) {
					for (int j = 0; j < language[i].txtToParse.Length; j++) {
						if(language[i].txtToParse[j] == baseTxt){
							idOfTheTXT = j;
							break;
						}
					}

				}	
			} 


			//find out what language we are on and select the same text id that we found above
			for (int i = 0; i < language.Count; i++) {
				if (language [i].language == LanguageSettings.actualLanguage && language[i].txtToParse.Length > idOfTheTXT) {
					return language [i].txtToParse [idOfTheTXT];
				}
			}

			return baseTxt;

		}
	}
}