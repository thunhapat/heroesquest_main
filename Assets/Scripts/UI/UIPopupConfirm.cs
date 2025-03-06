using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class UIPopupConfirm : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] UIItemInfo ItemInfo;
    [SerializeField] TextMeshProUGUI TXT_Message;
    [SerializeField] TextMeshProUGUI TXT_BtnYes;
    [SerializeField] TextMeshProUGUI TXT_BtnNo;
    [SerializeField] RectTransform AnimRect;
    [SerializeField] CanvasGroup CanvasGroup;
    [SerializeField] AnimationCurve AnimCurve;
    [SerializeField] float AnimTime;

    RectTransform _rect;

    UnityAction _onYesButton;
    UnityAction _onNoButton;

    Coroutine _anim;

    public static UIPopupConfirm Open(string msg, string txt_ok, string txt_no, UnityAction onYesButton, UnityAction onNoButton = null)
    {
        UIPopupConfirm uiPopup = GameAssetStore.Instance.UI.UIPopupConfirm.GetFromPool().GetComponent<UIPopupConfirm>();

        uiPopup.transform.SetParent(UICanvas.Instance.transform, false);
        uiPopup.transform.SetParent(UICanvas.Instance.transform, false);
        uiPopup.transform.SetAsLastSibling();

        uiPopup.InitPopup(msg, txt_ok, txt_no, onYesButton, onNoButton);
        uiPopup.ResetRect();
        uiPopup.PlayPopupAnim();

        return uiPopup;
    }

    public static UIPopupConfirm Open(CharmData charm, string msg, string txt_ok, string txt_no, UnityAction onYesButton, UnityAction onNoButton = null)
    {
        UIPopupConfirm uiPopup = Open(msg, txt_ok, txt_no, onYesButton, onNoButton);

        uiPopup.InitItemInfo(charm);

        return uiPopup;
    }
    public static UIPopupConfirm Open(PotionData potion, string msg, string txt_ok, string txt_no, UnityAction onYesButton, UnityAction onNoButton = null)
    {
        UIPopupConfirm uiPopup = Open(msg, txt_ok, txt_no, onYesButton, onNoButton);

        uiPopup.InitItemInfo(potion);

        return uiPopup;
    }


    public void InitPopup(string msg, string txt_ok, string txt_no, UnityAction onYesButton, UnityAction onNoButton = null)
    {
        if(_rect == null)
        {
            _rect = GetComponent<RectTransform>();
        }

        TXT_Message.text = msg;
        TXT_BtnYes.text = txt_ok;
        TXT_BtnNo.text = txt_no;

        ItemInfo.gameObject.SetActive(false);

        _onYesButton = onYesButton;
        _onNoButton = onNoButton;
    }

    public void ResetRect()
    {
        _rect.anchoredPosition = Vector2.zero;
        _rect.localScale = Vector2.one;
    }

    #region Init Item Info
    public void InitItemInfo(CharmData charm)
    {
        ItemInfo.Init(charm.Icon, charm.Name, charm.Description);
        ItemInfo.gameObject.SetActive(true);
    }
    public void InitItemInfo(PotionData potion)
    {
        ItemInfo.Init(potion.Icon, potion.Name, potion.Description);
        ItemInfo.gameObject.SetActive(true);
    }
    #endregion

    public void PlayPopupAnim()
    {
        if(_anim != null)
        {
            StopCoroutine(_anim);
        }
        _anim = StartCoroutine(DoPlayPopupAnim());
    }

    IEnumerator DoPlayPopupAnim()
    {
        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime / AnimTime;

            AnimRect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, AnimCurve.Evaluate(progress));
            CanvasGroup.alpha = Mathf.Lerp(0f, 1f, AnimCurve.Evaluate(progress));

            yield return null;
        }
    }

    public void OnButtonYes()
    {
        GameAssetStore.Instance.Audios.SFXUIClick.PlaySFXOneShot();

        _onYesButton?.Invoke();
        this.gameObject.ReturnToPool();
    }
    public void OnButtonNo()
    {
        GameAssetStore.Instance.Audios.SFXUIClick.PlaySFXOneShot();

        _onNoButton?.Invoke();
        this.gameObject.ReturnToPool();
    }
}
