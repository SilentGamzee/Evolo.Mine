using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ChangeFontSize : MonoBehaviour
{
    public enum FontStyles
    {
        Normal,
        Bold,
        Italic,
        BoldAndItalic
    };

    public FontStyles ActiveState = FontStyles.Normal;
    public Color color;
    public Text[] AllText;
    public bool ChangeDefault = true;
    [Range(0, 15)] public int size;
    public Font font;

    void Update()
    {
        AllText = FindObjectsOfType(typeof(Text)) as Text[];
        if (ChangeDefault)
        {
            foreach (Text txt in AllText)
            {
                txt.font = font;
                if (txt.fontSize <= 15)
                    txt.fontSize = size;
                FStyle(txt);
            }
        }
    }

    //pass text components here
    void FStyle(Text mytext)
    {
        switch (ActiveState)
        {
            // Check one case
            case FontStyles.Normal:
                //Set Normal Font style
                mytext.fontStyle = FontStyle.Normal;
                break;
            case FontStyles.Bold:
                //Set Bold Font style
                mytext.fontStyle = FontStyle.Bold;
                break;
            case FontStyles.Italic:
                //Set Italic Font style
                mytext.fontStyle = FontStyle.Italic;
                break;
            case FontStyles.BoldAndItalic:
                //Set BoldAndItalic Font style
                mytext.fontStyle = FontStyle.BoldAndItalic;
                break;
        }
    }
}