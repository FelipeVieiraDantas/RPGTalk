using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace RPGTALK.Texts
{
	
    public interface ITextWithIcon
    {
        Image icon { get; set; }
        Vector3 iconPosition { get; set; }
		List<Image> icons { get; set; } //= new List<Image>();
        List<int> indexes { get; set; }
		RPGTalk rpgtalk { get; set; }

        void RepopulateImages();

        bool FitImagesOnText(int y);

       
    }
}
