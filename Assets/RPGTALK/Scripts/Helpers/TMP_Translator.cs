using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RPGTALK.Helper;
#if RPGTalk_TMP
using TMPro;
#endif

namespace RPGTALK.Texts
{
    // This class has the objective of translate any variables that have different names between Unity's regular UI and TMPUGUI.
    public class TMP_Translator
    {

        public Text UIText;
        public bool hasUIText;
#if RPGTalk_TMP
        public TextMeshProUGUI TMPText;
#endif
        public bool errorSetting;

        public TMP_Translator(GameObject obj)
        {
            Text isText = obj.GetComponent<Text>();
            if (isText)
            {
                UIText = isText;
                hasUIText = true;
                errorSetting = false;
                return;
            }

#if RPGTalk_TMP
            TextMeshProUGUI isTMP = obj.GetComponent<TextMeshProUGUI>();
            if (isTMP)
            {
                TMPText = isTMP;
                errorSetting = false;
                return;
            }
#endif
            errorSetting = true;
        }

        public void ChangeTextTo(string text)
        {
            if (errorSetting)
            {
                DebugError();
                return;
            }


            if (UIText != null)
            {
                UIText.text = text;
            }
            else
            {
#if RPGTalk_TMP

                TMPText.text = text;
#endif
            }
        }

        public string GetCurrentText()
        {
            if (errorSetting)
            {
                DebugError();
                return "";
            }


            if (UIText != null)
            {
                return UIText.text;
            }
            else
            {
#if RPGTalk_TMP

                return TMPText.text;
#endif
            }
            return "";
        }

        void DebugError()
        {
            Debug.LogError("The object setted on RPGTalk wasn't a Text or a Text Mesh Pro UGUI. Be sure to check RPGTalk Configuration if you wnat to use the later.");
        }


        public void ChangeRichText(bool active)
        {
            if (errorSetting)
            {
                DebugError();
                return;
            }

            if (UIText != null)
            {
                UIText.supportRichText = active;
            }
            else
            {
#if RPGTalk_TMP

                TMPText.richText = active;
#endif
            }
        }

        public bool RichText()
        {
            if (errorSetting)
            {
                DebugError();
                return false;
            }

            if (UIText != null)
            {
                return UIText.supportRichText;
            }
            else
            {
#if RPGTalk_TMP

                return TMPText.richText;
#endif
            }

            return false;
        }


        public void Enabled(bool enable)
        {
            if (errorSetting)
            {
                DebugError();
            }

            if (UIText != null)
            {
                UIText.enabled = enable;
            }
            else
            {
#if RPGTalk_TMP

                TMPText.enabled = enable;
#endif
            }

        }

        public bool Enabled()
        {
            if (errorSetting)
            {
                DebugError();
                return false;
            }

            if (UIText != null)
            {
                return UIText.enabled;
            }
            else
            {
#if RPGTalk_TMP

                return TMPText.enabled;
#endif
            }

            return false;
        }

        public Object GetTextObject()
        {
            if (errorSetting)
            {
                DebugError();
                return null;
            }

            if (UIText != null)
            {
                return UIText;
            }
            else
            {
#if RPGTalk_TMP

                return TMPText;
#endif
            }

            return null;
        }

        /// <summary>
        /// A function that returns if the Object is an acceptable type (Text or TextMeshProUGUI)
        /// </summary>
        /// <returns><c>true</c>, if valid type was used, <c>false</c> otherwise.</returns>
        /// <param name="obj">Object.</param>
        public static bool IsValidType(GameObject obj)
        {
            if (obj.GetComponent<Text>())
            {
                return true;
            }

#if RPGTalk_TMP
            if (obj.GetComponent<TextMeshProUGUI>())
            {
                return true;
            }
#endif

            return false;
        
        }

        /// <summary>
        /// A simple function that returns true if the object has a Text Component
        /// </summary>
        public static bool IsText(GameObject obj)
        {
            if (obj.GetComponent<Text>())
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public ITextWithIcon AddTextWithIconComponent(GameObject gameObject)
        {
            if (errorSetting)
            {
                DebugError();
                return null;
            }


            if (hasUIText)
            {
                return gameObject.AddComponent<TextWithIconSimpleText>();
            }
            else
            {
                return null;
            }
        }



        public string GetCorrectSpriteLine(string line, ref List<RPGTalkSprite> sprites, ref List<RPGTalkSprite>spritesUsed, int spriteNum, int initialBracket, int finalBracket, int lineWithSprite, string tmpSpriteAtlas)
        {

            if (hasUIText)
            {

                //Neat, we definely have a sprite with a valid number. Time to keep track of it
                RPGTalkSprite newSprite = new RPGTalkSprite();
                newSprite.sprite = sprites[spriteNum].sprite;
                newSprite.width = sprites[spriteNum].width;
                newSprite.height = sprites[spriteNum].height;
                newSprite.spritePosition = initialBracket;
                //Make sure that the the sprite only work for that next line to be added to RpgTalkElements
                //newSprite.lineWithSprite = rpgtalkElements.Count;
                newSprite.lineWithSprite = lineWithSprite;
                newSprite.animator = sprites[spriteNum].animator;

                spritesUsed.Add(newSprite);

                //Looking good! We found out that a sprite should be there and we are already keeping track of it
                //But now we should remove the [sprite=X] from the line.
                //The magic here is that we will replace it with the <color=#00000000> tag and the content will be
                //a text with the length of the sprite's width. So in fact there will be text in there so the next word
                //will have the right margin, but the text will be invisible so the sprite can take its place
                string filledText = "";
                for (int i = 0; i < Mathf.CeilToInt(newSprite.width); i++)
                {
                    //The letter "S" is used to fill because in most fonts the letter S occupies a perfect character square
                    filledText += "S";
                }

                if (finalBracket == line.Length - 1)
                {
                    //if the sprite was the last thing on the text, we should place an empty space to align correctly the vertexes
                    filledText += "SS";
                }
                return line.Substring(0, initialBracket) +
                    "<color=#00000000>" + filledText + "</color>" +
                    line.Substring(finalBracket + 1);

            }
            else
            {

#if RPGTalk_TMP

                //if we are using TMP, everything is easier. We just need to make the text says <sprite=X index=Y> and let TMP's components do the rest
                return line.Substring(0, initialBracket) +
                    "<sprite=\"" + tmpSpriteAtlas + "\" index="+spriteNum+">" +
                    line.Substring(finalBracket + 1);
#endif
                return line;
            }

        }

        /// <summary>
        /// Structure to hold pre-computed animation data.
        /// </summary>
        private struct VertexAnim
        {
            public float angleRange;
            public float angle;
            public float speed;
        }

        //Jitters the part of the text
        public IEnumerator Jitter(RPGTalkJitter jitter)
        {
#if RPGTalk_TMP
            if(TMPText == null)
            {
                Debug.LogError("Only TextMeshPro users can use the Jitter Tag");
                yield return null;
            }


            TMP_TextInfo textInfo = TMPText.textInfo;
            // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
            TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
            int characterCount = textInfo.characterCount;

            // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
            VertexAnim[] vertexAnim = new VertexAnim[1024];
            for (int i = 0; i < 1024; i++)
            {
                vertexAnim[i].angleRange = Random.Range(10f, 25f);
                vertexAnim[i].speed = Random.Range(1f, 3f);
            }

            int loopCount = 0;

            Matrix4x4 matrix;

            while (true)
            {
                int repeatUntil = jitter.jitterPosition + jitter.numberOfCharacters;


                // yield until we have all the characters in the jitter
                while (characterCount < repeatUntil)
                {
                   
                    // Update the copy of the vertex data for the text object.
                    cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                    characterCount = textInfo.characterCount;


                    yield return new WaitForEndOfFrame();
                    continue;
                }




                for (int i = jitter.jitterPosition; i < repeatUntil; i++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    // Skip characters that are not visible and thus have no geometry to manipulate.
                    if (!charInfo.isVisible)
                        continue;

                    // Retrieve the pre-computed animation data for the given character.
                    VertexAnim vertAnim = vertexAnim[i];

                    // Get the index of the material used by the current character.
                    int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                    // Get the cached vertices of the mesh used by this text element (character or sprite).
                    Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                    // If we dont have the vertices yet, don't do it
                    if (sourceVertices.Length < vertexIndex+3)
                    {
                        continue;
                    }

                    // Determine the center point of each character at the baseline.
                    //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                    // Determine the center point of each character.
                    Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                    // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                    // This is needed so the matrix TRS is applied at the origin for each character.
                    Vector3 offset = charMidBasline;

                    Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                    destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                    destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                    destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                    destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                    vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                    Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

                    matrix = Matrix4x4.TRS(jitterOffset * jitter.jitter, Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * jitter.angle), Vector3.one);

                    destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                    destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                    destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                    destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                    destinationVertices[vertexIndex + 0] += offset;
                    destinationVertices[vertexIndex + 1] += offset;
                    destinationVertices[vertexIndex + 2] += offset;
                    destinationVertices[vertexIndex + 3] += offset;

                    vertexAnim[i] = vertAnim;
                }

                // Push changes into meshes
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    TMPText.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                loopCount += 1;

                yield return new WaitForSeconds(0.1f);
            }
#else

            Debug.LogError("Only TextMeshPro users can use the Jitter Tag");
            yield return null;

#endif



        }


    }
}