using UnityEngine;

[CreateAssetMenu(fileName = "CharmData", menuName = "Data/ItemData/CharmData")]
public class CharmData : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    public string Description;

    public BasicStat BonusStat;
}
