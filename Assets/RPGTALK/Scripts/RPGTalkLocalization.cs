using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGTALK.Localization
{


	/// <summary>
	/// The settings of the current language
	/// </summary>
	public class LanguageSettings{
		/// <summary>
		/// What is the default language of your game? The language here should be the language that you set every "txtToParse"
		/// in RPGTalk. If no equivalent localization is found, this language will be shown
		/// </summary>
		public static RPGTalkLanguage defaultLanguage;

		/// <summary>
		/// What language is the game currently in?
		/// </summary>
		public static RPGTalkLanguage actualLanguage {
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
		static RPGTalkLanguage _actualLanguage;

    }


	[AddComponentMenu("Seize Studios/RPGTalk/Localization/RPGTalk Localization")]
	public class RPGTalkLocalization : MonoBehaviour {

		[Tooltip("Here you should set all languages available for the game")]
		public RPGTalkLanguage[] languages;

		/// <summary>
		/// The singleton is the instance of the RPGTalkLocalization, so you can call it from whatever script you like
		/// using RPGTalkLocalization.singleton
		/// </summary>
		public static RPGTalkLocalization singleton;

		void Awake(){

            if(singleton != null)
            {
                Destroy(gameObject);
                return;
            }

            singleton = this;
			//This object often is the same in every scene, so we don't need to create it more than once.
			DontDestroyOnLoad (gameObject);
            SetDefaultLanguage();
		}

        //A simple function to go through every language and get which is the main one. Also sets the current language to that.
        void SetDefaultLanguage()
        {
            foreach(RPGTalkLanguage lang in languages)
            {
                if (lang.mainLanguage)
                {
                    LanguageSettings.defaultLanguage = lang;
                    LanguageSettings.actualLanguage = lang;
                    return;
                }
            }
            Debug.LogError("No main language was found in you RPGTalk Localization object!");
        }

        /// <summary>
        /// Given a base text, returns the textAsset for the actual language
        /// </summary>
        /// <returns>Text Asset for the actual language</returns>
        /// <param name="baseTxt">Base text in the default language.</param>
        public TextAsset CheckForCorrectLanguage(TextAsset baseTxt){
			//Find out what is the id of the txt in the main language
			int idOfTheTXT = -1;

			for (int j = 0; j < LanguageSettings.defaultLanguage.txts.Length; j++) {
				if(LanguageSettings.defaultLanguage.txts[j] == baseTxt){
					idOfTheTXT = j;
					break;
				}
			}

            if (idOfTheTXT == -1){
                Debug.LogWarning("The given baseTxt wasn't on the Default Language");
                return baseTxt;
            }

            //find out what language we are on and select the same text id that we found above
            if (LanguageSettings.actualLanguage.txts.Length > idOfTheTXT)
            {
                return LanguageSettings.actualLanguage.txts[idOfTheTXT];
            }

            //if everything went wrong, just return the baseTxt
			return baseTxt;

		}
	}
}