using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;

public class UIGameOver : UIPage<UIGameOver>
{
    [Header("Object References")]
    [SerializeField] TextMeshProUGUI TXT_Level;
    [SerializeField] TextMeshProUGUI TXT_Encourage;
    [SerializeField] GameObject BTN_PlayAgain;
    [SerializeField] CanvasGroup CanvasGroup;

    [Header("Settings")]
    [SerializeField] float FadeDuration;

    Coroutine _fadeCo;

    public static void Open(int level)
    {
        if (_instance == null)
        {
            _instance = FindFirstObjectByType<UIGameOver>();
            if (_instance == null)
            {
                GameObject uiPage = Instantiate(GameAssetStore.Instance.UI.UIGameOver, UICanvas.Instance.transform);
                _instance = uiPage.GetComponent<UIGameOver>();
            }
        }

        _instance.transform.SetAsLastSibling();
        _instance.gameObject.SetActive(true);
        _instance.Init(level);
    }

    public void Init(int level)
    {
        TXT_Level.text = level.ToString();

        if (level > GameSetting.Instance.AlliesInEachLevel.Count)
        {
            TXT_Encourage.text = GameSetting.Instance.EncourageSentences.Last();
        }
        else
        {
            TXT_Encourage.text = GameSetting.Instance.EncourageSentences[level - 1];
        }

        if (_fadeCo != null)
        {
            StopCoroutine(_fadeCo);
        }
        _fadeCo = StartCoroutine(DoFadeIn());
    }

    public void OnPlayAgain()
    {
        GameManager.Instance.StartGame();
        gameObject.SetActive(false);
    }

    IEnumerator DoFadeIn()
    {
        BTN_PlayAgain.SetActive(false);
        TXT_Encourage.gameObject.SetActive(false);
        TXT_Level.gameObject.SetActive(false);

        float progress = 0f;

        while (progress < 1f)
        {
            progress += Time.deltaTime / FadeDuration;

            CanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }
        yield return new WaitForSeconds(0.65f);

        TXT_Level.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.65f);

        TXT_Encourage.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.65f);

        BTN_PlayAgain.SetActive(true);
    }
}
