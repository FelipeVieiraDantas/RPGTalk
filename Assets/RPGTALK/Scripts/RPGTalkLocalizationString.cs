using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
#if RPGTalk_TMP
using TMPro;
#endif

namespace RPGTALK.Localization
{

	/// <summary>
	/// This class that keep every text asset for every language
	/// </summary>
	[System.Serializable]
	public class LanguageString{
		[Tooltip("Language of the string below")]
		public RPGTalkLanguage language;
		[Tooltip("What should be the text if the the language above is chosen?")]
		public string thisString;
	}

	[AddComponentMenu("Seize Studios/RPGTalk/Localization/RPGTalk Localization String")]
	public class RPGTalkLocalizationString : MonoBehaviour {

		[Tooltip("Here you should set all your languages and possible strings")]
		public LanguageString[] language;

        public TextAsset txtToParse;
        public string lineToRead;

		// Use this for initialization
		void OnEnable () {
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
#if RPGTalk_TMP
            else if(GetComponent<TextMeshProUGUI>())
            {
                GetComponent<TextMeshProUGUI>().text = CheckForCorrectLanguage();
            }
#endif
        }

		/// <summary>
		/// Checks for correct language to use
		/// </summary>
		/// <returns>The string for the correct language set in 'language' list</returns>
		public string CheckForCorrectLanguage(){
            //If we setted a TXT to parse, let's parse it.
            if (txtToParse != null)
            {
                int actualLineToStart;
                //reduce one for the line, if was an int
                //return the default lines to -2 if they were not ints
                if (int.TryParse(lineToRead, out actualLineToStart))
                {
                    actualLineToStart -= 1;
                }
                else
                {
                    actualLineToStart = -2;
                }


                // read the TXT file into the elements list
                StringReader reader;
                if (RPGTalkLocalization.singleton != null)
                {
                    reader = new StringReader(RPGTalkLocalization.singleton.CheckForCorrectLanguage(txtToParse).text);
                }
                else
                {
                    reader = new StringReader(txtToParse.text);
                }

                string line = reader.ReadLine();
                int currentLine = 0;

                while (line != null)
                {
                    //if the lineToStart was string, find out what line it actually is
                    if (actualLineToStart == -2)
                    {
                        if (line.IndexOf("[title=" + lineToRead + "]") != -1)
                        {
                            actualLineToStart = currentLine + 1;
                        }

                        line = reader.ReadLine();
                        currentLine++;
                        continue;

                    }

                    if (currentLine == actualLineToStart)
                    {
                        return line;
                    }
                    else
                    {
                        line = reader.ReadLine();
                        currentLine++;
                        continue;
                    }
                }
            }

            //If we didn't set a TXT, we want a simple string
            //find out what language we are on and select the text
            for (int i = 0; i < language.Length; i++) {
				if (language [i].language == LanguageSettings.actualLanguage) {
					return language [i].thisString;
				}
			}

            if(language.Length > 0)
            {
                return language[0].thisString;
            }

            return "";

		}
	}

}