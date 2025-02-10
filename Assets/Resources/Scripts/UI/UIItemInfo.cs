using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItemInfo : UIPage<UIBattle>
{
    [Header("Object References")]
    [SerializeField] Image IMG_Icon;
    [SerializeField] TextMeshProUGUI TXT_Name;
    [SerializeField] TextMeshProUGUI TXT_Description;

    public void Init(Sprite icon, string name, string desc)
    {
        if (IMG_Icon != null)
        {
            IMG_Icon.sprite = icon;
        }
        if (TXT_Name != null)
        {
            TXT_Name.text = name;
        }
        if (TXT_Description != null)
        {
            TXT_Description.text = desc;
        }
    }
}
