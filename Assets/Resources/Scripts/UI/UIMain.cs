using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMain : UIPage<UIMain>
{
    [Header("Object References")]
    [SerializeField] TextMeshProUGUI TXT_Class;
    [SerializeField] TextMeshProUGUI TXT_Attack;
    [SerializeField] TextMeshProUGUI TXT_Defence;
    [SerializeField] Image IMG_HPFill;
    [SerializeField] UIItemInfo ItemInfo;
    [SerializeField] GameObject UIParent;

    public void ToggleShow(bool isShow)
    {
        UIParent.SetActive(isShow);
    }

    public void UpdateUIMain(string charClass, float hpRatio, string attack, string defence, CharmData charm)
    {
        TXT_Class.text = charClass;
        TXT_Attack.text = attack;
        TXT_Defence.text = defence;

        IMG_HPFill.fillAmount = hpRatio;

        ItemInfo.gameObject.SetActive(charm != null);
        if (charm != null)
        {

            ItemInfo.Init(charm.Icon, charm.Name, charm.Description);
        }
    }
}
