using UnityEngine;

[CreateAssetMenu(fileName = "PotionData", menuName = "Data/ItemData/PotionData")]
public class PotionData : ScriptableObject
{
    public Sprite Icon;
    public string Name;
    public string Description;

    public int RecoverAmount;
}
