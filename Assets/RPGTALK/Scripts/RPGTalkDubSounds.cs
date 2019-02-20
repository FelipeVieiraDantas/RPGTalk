using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using RPGTALK.Localization;

namespace RPGTALK.Dub
{
	[AddComponentMenu("Seize Studios/RPGTalk/RPGTalk Dub Sounds")]
	public class RPGTalkDubSounds : MonoBehaviour {

		public RPGTalkDubLanguage[] dubsByLanguage;
		public AudioMixerGroup audioMixerToUse;
		AudioSource aS;

		void Start(){
			aS = gameObject.AddComponent<AudioSource> ();
			aS.outputAudioMixerGroup = audioMixerToUse;
			aS.loop = false;
			aS.playOnAwake = false;
		}

		/// <summary>
		/// Plays a clip keeped in thedubsByLanguage array
		/// </summary>
		/// <param name="clipNum">Clip number.</param>
		public void PlayDubClip(int clipNum){
			if (aS == null) {
				return;
			}

			//find out what is the language to use
			int languageToUse = 0;
			for (int i = 0; i < dubsByLanguage.Length; i++) {
				if (dubsByLanguage [i].language == LanguageSettings.actualLanguage) {
					languageToUse = i;
                    break;
				}
			}

			if (clipNum >= dubsByLanguage [languageToUse].dubClip.Length) {
				Debug.LogError ("Tried to play a dub clip that is not set to current language");
				return;
			}

			aS.clip = dubsByLanguage [languageToUse].dubClip [clipNum];
			aS.Play ();

		}

		/// <summary>
		/// Stops whatever clip is playing
		/// </summary>
		public void StopCurrentDub(){
			if (aS == null) {
				return;
			}
			aS.Stop ();
			aS.clip = null;
		}

	}

	//A class to keep the dub
	[System.Serializable]
	public class RPGTalkDubLanguage{
		public RPGTalkLanguage language;
		public AudioClip[] dubClip;
	}
}