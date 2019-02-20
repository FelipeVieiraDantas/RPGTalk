using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if RPGTalk_TMP
using TMPro;
#endif

namespace RPGTALK.Helper
{
	[AddComponentMenu("Seize Studios/RPGTalk/Helper/RPGTalk Helper")]
	public class RPGTalkHelper : MonoBehaviour {

        public static void CopyTextParameters(Object original, Object copy)
        {
            if (original is Text)
            {
                CopyTextParameters(original as Text, copy as Text);
                return;
            }
#if RPGTalk_TMP
            if(original is TextMeshProUGUI)
            {
                CopyTextParameters(original as TextMeshProUGUI, copy as TextMeshProUGUI);
            }
#endif
        }

        public static void CopyTextParameters(Text original, Text copy){
			//Replace every public option of the new text with the ancient one
			copy.text = original.text;
			copy.font = original.font;
			copy.fontStyle = original.fontStyle;
			copy.fontSize = original.fontSize;
			copy.lineSpacing = original.lineSpacing;
			copy.supportRichText = original.supportRichText;
			copy.alignment = original.alignment;
			copy.alignByGeometry = original.alignByGeometry;
			copy.horizontalOverflow = original.horizontalOverflow;
			copy.verticalOverflow = original.verticalOverflow;
			copy.resizeTextForBestFit = original.resizeTextForBestFit;
			copy.color = original.color;
			copy.material = original.material;
			copy.raycastTarget = original.raycastTarget;
		}
#if RPGTalk_TMP
        public static void CopyTextParameters(TextMeshProUGUI original, TextMeshProUGUI copy)
        {
            //Replace every public option of the new text with the ancient one
            copy.text = original.text;
            copy.font = original.font;
            copy.fontStyle = original.fontStyle;
            copy.fontSize = original.fontSize;
            copy.lineSpacing = original.lineSpacing;
            copy.richText = original.richText;
            copy.alignment = original.alignment;
            copy.horizontalMapping = original.horizontalMapping;
            copy.verticalMapping = original.verticalMapping;
            copy.enableAutoSizing = original.enableAutoSizing;
            copy.color = original.color;
            copy.material = original.material;
            copy.raycastTarget = original.raycastTarget;
            copy.enableWordWrapping = original.enableWordWrapping;
            copy.wordWrappingRatios = original.wordWrappingRatios;
            copy.overflowMode = original.overflowMode;
        }
#endif
        public static int CountRichTextCharacters(string line){
			int richTextCount = 0;
			//check for any rich text 
			if (line.IndexOf('<') != -1) {
				bool thereIsRichTextLeft = true;

				//repeat for as long as we find a tag
				while (thereIsRichTextLeft) {
					int inicialBracket = line.IndexOf ('<');
					int finalBracket = line.IndexOf ('>');
					//Here comes the tricky part... First check if there is any '<' before a '>'
					if (inicialBracket < finalBracket) {
						//Ok, there is! It should be a tag. Let's count every char inside of it
						richTextCount += finalBracket-inicialBracket+1;


						//Good! Now finaly, remove it from the original text
						string textWithoutRichText = line.Substring (0, inicialBracket);
						textWithoutRichText += line.Substring (finalBracket + 1);
						line = textWithoutRichText;

					} else {
						thereIsRichTextLeft = false;
					}
				}
					
			}

			return richTextCount;
		}

		public static int CountRPGTalkTagCharacters(string line){
			int tagCount = 0;
			//check for any rich text 
			if (line.IndexOf('[') != -1) {
				bool thereAreTagsLeft = true;

				//repeat for as long as we find a tag
				while (thereAreTagsLeft) {
					int inicialBracket = line.IndexOf ('[');
					int finalBracket = line.IndexOf (']');
					//Here comes the tricky part... First check if there is any '[' before a ']'
					if (inicialBracket < finalBracket) {
						//Ok, there is! It should be a tag. Let's count every char inside of it
						tagCount += finalBracket-inicialBracket+1;


						//Good! Now finaly, remove it from the original text
						string textWithoutTag = line.Substring (0, inicialBracket);
						textWithoutTag += line.Substring (finalBracket + 1);
						line = textWithoutTag;

					} else {
						thereAreTagsLeft = false;
					}
				}

			}

			return tagCount;
		}

		#if UNITY_EDITOR

		[MenuItem("RPGTalk/Create RPGTalk/Base Instance")]
		private static void CreateRPGTalkBase()
		{
			GameObject newGO = new GameObject ();
			newGO.AddComponent<RPGTalk> ();
			newGO.name = "RPGTalk Holder";
			Undo.RegisterCreatedObjectUndo (newGO, "Create RPGTalk");
		}

		[MenuItem("RPGTalk/Create RPGTalk/With Dub Sound")]
		private static void CreateRPGTalkWithDub()
		{
			GameObject newGO = new GameObject ();
			newGO.AddComponent<RPGTalk> ();
			newGO.AddComponent<RPGTALK.Dub.RPGTalkDubSounds> ();
			newGO.name = "RPGTalk Holder & Dub";
			Undo.RegisterCreatedObjectUndo (newGO, "Create RPGTalk");
		}
		[MenuItem("RPGTalk/Create RPGTalk/With Timeline")]
		private static void CreateRPGTalkWithTimeline()
		{
			GameObject newGO = new GameObject ();
			newGO.AddComponent<RPGTalk> ();
			newGO.AddComponent<RPGTALK.Timeline.RPGTalkTimeline> ();
			newGO.name = "RPGTalk Holder & Timeline";
			Undo.RegisterCreatedObjectUndo (newGO, "Create RPGTalk");
		}
		[MenuItem("RPGTalk/Create RPGTalk/With Dub and Timeline")]
		private static void CreateRPGTalkWithDubTimeline()
		{
			GameObject newGO = new GameObject ();
			newGO.AddComponent<RPGTalk> ();
			newGO.AddComponent<RPGTALK.Dub.RPGTalkDubSounds> ();
			newGO.AddComponent<RPGTALK.Timeline.RPGTalkTimeline> ();
			newGO.name = "RPGTalk Holder & Dub & Timeline";
			Undo.RegisterCreatedObjectUndo (newGO, "Create RPGTalk");
		}

		[MenuItem("RPGTalk/Create RPGTalk Area")]
		private static void CreateRPGTalkArea()
		{
			
			GameObject newGO = new GameObject ();
			if (EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D) {
				BoxCollider2D newBox = newGO.AddComponent<BoxCollider2D> ();
				newBox.size = new Vector2 (1, 1);
				newBox.isTrigger = true;
			} else {
				BoxCollider newBox = newGO.AddComponent<BoxCollider> ();
				newBox.size = new Vector3 (1, 1, 1);
				newBox.isTrigger = true;
			}
			newGO.AddComponent<RPGTalkArea> ();
			newGO.name = "RPGTalk Area";
			Undo.RegisterCreatedObjectUndo (newGO, "Create RPGTalk Area");
		}

		[MenuItem("RPGTalk/Create RPGTalk Localization")]
		private static void CreateRPGTalkLocalization()
		{

			GameObject newGO = new GameObject ();

			newGO.AddComponent<RPGTALK.Localization.RPGTalkLocalization> ();
			newGO.name = "RPGTalk Localization";
			Undo.RegisterCreatedObjectUndo (newGO, "Create RPGTalk Localization");
		}

		#endif
	}

	//this class has every dialog (line) that need to be show
	public class RpgtalkElement {
		public bool hasDialog = false;
		public bool allowPlayerAdvance = true;
		public string speakerName;
		public string originalSpeakerName;
		public string dialogText;
        public string expression;

        public override string ToString () {
			return  "(" + this.hasDialog + ")" + this.speakerName + "::" + this.dialogText + "\n";
		}
	}


	//A class to be the variables a text could have
	[System.Serializable]
	public class RPGTalkVariable{
		public string variableName;
		public string variableValue;
	}

	//A class to keep any rich text used
	[System.Serializable]
	public class RPGTalkRichText{
		public int lineWithTheRichText;
		public int initialTagPosition;
		public string initialTag;
		public int finalTagPosition;
		public string finalTag;
	}

	//A class to keep any sprite used inside the text
	[System.Serializable]
	public class RPGTalkSprite{
		public Sprite sprite;
		[HideInInspector]
		public int lineWithSprite = -1;
		[HideInInspector]
		public int spritePosition = -1;
		public float width = 1;
		public float height = 1;
		[HideInInspector]
		public bool alreadyInPlace;
		public RuntimeAnimatorController animator;
	}

	//A class to keep any dub used inside the text
	[System.Serializable]
	public class RPGTalkDub{
		public int dubNumber;
		public int lineWithDub = -1;
	}

	//A class to keep any speed changes used inside the text
	[System.Serializable]
	public class RPGTalkSpeed{
		public int speed = 0;
		public int lineWithSpeed = -1;
		public int speedPosition = -1;
		public bool alreadyGone;
	}

	//A class to keep any question used inside the text
	[System.Serializable]
	public class RPGTalkQuestion{
		public string questionID;
		public int lineWithQuestion = -1;
		public bool alreadyHappen;
		public List<string> choices = new List<string>();
	}

	//A class to keep the targets that the canvas can follow
	[System.Serializable]
	public class RPGTalkCharacterSettings{
		[Tooltip("What is the Character that those settings represent?")]
		public RPGTalkCharacter character;
		[Tooltip("Who to follow?")]
		public Transform follow;

		[Tooltip("If he is following someone, should there be an offset?")]
		public Vector3 followOffset;

        [Tooltip("The animations should happen in a different Animator than the one set on RPGTalk?")]
        public Animator animatorOverwrite;
	}

    public class RPGtalkSaveStatement
    {
        public string lineToStart;
        public string lineToBreak;
        public string savedData;
        public int modifier;
    }

    //A class to keep any jitter used inside the text
    [System.Serializable]
    public class RPGTalkJitter
    {
        public float angle = 1;
        public float jitter = 1;
        public int lineWithJitter = -1;
        public int jitterPosition = -1;
        public bool alreadyGone;
        public int numberOfCharacters = 1;
    }


}