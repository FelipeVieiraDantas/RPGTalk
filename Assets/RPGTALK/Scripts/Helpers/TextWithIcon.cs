using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using RPGTALK.Helper;

namespace RPGTALK.Texts
{
	[AddComponentMenu("Seize Studios/RPGTalk/Helper/Text With Icon")]
    public class TextWithIcon : Text
    {
        private Image icon;
        private Vector3 iconPosition;
		private List<Image> icons = new List<Image>();
		public List<int> indexes;
		public RPGTalk rpgtalk;

		public void RepopulateImages(){
			foreach (Image childImage in icons) {
				//Destroy every ancient image that might be left standing from previous talks
				if (childImage) {
					DestroyImmediate (childImage.gameObject);
				}
			}

			//new icons, new indexes and new positions
			icons = new List<Image> ();
			indexes = new List<int>();

			foreach (RPGTalkSprite sprite in rpgtalk.spritesUsed) {
				//for each sprites in this talk, let's create an Image.
				GameObject newGo = new GameObject ();
				Image newImg = newGo.AddComponent<Image> ();
				newImg.sprite = sprite.sprite;
				newGo.transform.SetParent (transform);
				icons.Add (newImg);

				//if it has an animator, put it
				if (sprite.animator) {
					newGo.AddComponent<Animator> ().runtimeAnimatorController = sprite.animator;
				}

				//We don't want it to appear right from the start
				newGo.SetActive (false);

				//Where this sprite should appear? Width/2 because each width a new character is put.
				//That way the image will be put right in the middle.
				indexes.Add(sprite.spritePosition + Mathf.CeilToInt(sprite.width/2));
			}
		}
        
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
        }

		public bool FitImagesOnText(int y){
			
			//Check the right position/size of text, considering that canvas could be Scale With Screen Size
			Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
			Vector2 refPoint = Vector2.zero;
			refPoint.x = (textAnchorPivot.x == 1 ? rectTransform.rect.xMax : rectTransform.rect.xMin);
			refPoint.y = (textAnchorPivot.y == 0 ? rectTransform.rect.yMin : rectTransform.rect.yMax);
			Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;
			float unitsPerPixel = 1 / pixelsPerUnit;

			//var textGen = cachedTextGenerator;

			//The position of the image on the text. It will be plus 16 because it will come after <color=#00000000>.
			//The width is mesured in characters, so, whatever characters it may have on width, the middle will be this number/2.
			int position = rpgtalk.spritesUsed [y].spritePosition + 16 + Mathf.CeilToInt(rpgtalk.spritesUsed [y].width/2);

			//If the cached text generator still doesn't have this character... we are not ready to place the image
			if (cachedTextGenerator.characters.Count <= position) {
				return false;
			}

			//Each character occupies 4 verts
			Vector2 locUpperLeft = new Vector2(cachedTextGenerator.verts[position * 4].position.x, cachedTextGenerator.verts[position * 4].position.y) * unitsPerPixel;
			locUpperLeft.x += roundingOffset.x;
			locUpperLeft.y += roundingOffset.y;

			Vector2 locBottomRight = new Vector2(cachedTextGenerator.verts[position * 4 + 2].position.x, cachedTextGenerator.verts[position * 4 + 2].position.y) * unitsPerPixel ;
			locBottomRight.x += roundingOffset.x;
			locBottomRight.y += roundingOffset.y;

			//The middle of the character should be its corners /2
			Vector3 mid = (locUpperLeft + locBottomRight) / 2;
			//If the size would be important...
			//Vector3 size = locBottomRight - locUpperLeft;

			//The font height and width based on the distance between the characters
			float _fontHeight = Vector3.Distance(cachedTextGenerator.verts[position * 4].position, cachedTextGenerator.verts[position * 4 + 2].position);
			float _fontWidth = Vector3.Distance(cachedTextGenerator.verts[position * 4].position, cachedTextGenerator.verts[position * 4 + 1].position);
			if (_fontWidth == 0 || _fontHeight == 0) {
				return false;
			}

			//Finally! Activate the image and put it in the middle position, also change its size based on character height and width
			icons [y].gameObject.SetActive (true);
			icons [y].rectTransform.localPosition = mid;
			icons [y].rectTransform.sizeDelta = new Vector2 (_fontWidth * rpgtalk.spritesUsed[y].width, _fontHeight * rpgtalk.spritesUsed[y].height);
			icons [y].rectTransform.localScale = new Vector3 (1, 1, 1);

			return true;
		}


			
       
    }
}
