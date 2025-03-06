using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VFXDamageNumber : MonoBehaviour
{
    public TextMeshProUGUI TXT_Number;

    [SerializeField] float _lifeTime = 1f;
    [SerializeField] Animation _anim;

    public void PlayDamageText(int damage, bool isCritical = false)
    {
        gameObject.SetActive(true);

        string dmgText = $"-{damage}";
        if (isCritical) dmgText += "!";

        TXT_Number.text = dmgText;

        if(isCritical)
        {
            _anim.Play("VFXDamageText_Crit");
        }
        else
        {
            _anim.Play("VFXDamageText_Normal");
        }

        CancelInvoke();
        Invoke("GoDeactive", _lifeTime);
    }

    void GoDeactive()
    {
        gameObject.SetActive(false);
    }
}
