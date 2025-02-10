using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBattleInfo : MonoBehaviour
{
    [Header("Object References")]
    [SerializeField] Image IMG_HPBarFill;
    [SerializeField] TextMeshProUGUI TXT_Class;
    [SerializeField] TextMeshProUGUI TXT_Attack;
    [SerializeField] TextMeshProUGUI TXT_Defence;

    CombatCharacter _combatChar;

    public void InitCombatantInfo(CombatCharacter combatChar, BasicStat bonusStat = new BasicStat())
    {
        _combatChar = combatChar;

        int maxHP = combatChar.Data.MaxHP + bonusStat.MaxHP;
        int ramainingHP = combatChar.Data.RemainingHP;
        int attack = combatChar.Data.Attack + bonusStat.Attack;
        int defence = combatChar.Data.Defence + bonusStat.Defence;

        TXT_Class.text = $"{combatChar.Data.CharacterClass} Lv.{combatChar.Data.Level}";
        TXT_Attack.text = attack.ToString();
        TXT_Defence.text = defence.ToString();

        UpdateHPBar();
    }

    public void UpdateHPBar()
    {
        float ratio = (float)_combatChar.Data.RemainingHP / (float)_combatChar.Data.MaxHP;
        IMG_HPBarFill.fillAmount = ratio;
    }

}
