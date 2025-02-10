using UnityEngine;
using UnityEngine.UI;

public class UIBattle : UIPage<UIBattle>
{

    [SerializeField] UIBattleInfo UI_InfoHero;
    [SerializeField] UIBattleInfo UI_InfoEnemy;
    [SerializeField] RawImage UI_BattleProjection;

    public static void Open(CombatCharacter hero, CombatCharacter enemy, CharmData charm = null)
    {
        if (_instance == null)
        {
            _instance = FindFirstObjectByType<UIBattle>();
            if (_instance == null)
            {
                GameObject uiPage = Instantiate(GameAssetStore.Instance.UI.UIBattle, UICanvas.Instance.transform);
                _instance = uiPage.GetComponent<UIBattle>();
            }
        }

        _instance.gameObject.SetActive(true);
        _instance.Init(hero, enemy, charm);
    }

    public void Init(CombatCharacter hero, CombatCharacter enemy, CharmData charm)
    {
        UI_InfoHero.InitCombatantInfo(hero, charm != null ? charm.BonusStat : new BasicStat());
        UI_InfoEnemy.InitCombatantInfo(enemy);
    }

    public void UpdateHPBar()
    {
        UI_InfoHero.UpdateHPBar();
        UI_InfoEnemy.UpdateHPBar();
    }

    public void Close()
    {

    }
}
