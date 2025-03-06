using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSetting", menuName = "HeroesQuest/GameSetting")]
public class GameSetting : ScriptableSingleton<GameSetting>
{
    [Header("Grid Setting")]
    public Vector2Int BoardSize;
    public float TileSize;

    [Header("Rule Settings")]
    public List<CombatCharacterClass> AvailableCharacterClasses;
    public int InitialObstacleCount;
    public int MaxHeroSpawnCount;
    public int MaxEnemySpawnCount;
    public bool IsRecoverHpWhenLevelUp;

    [Header("Combat Setting")]
    public float AdvantageDamageMultiplier;
    public int MinumumDamage;

    [Header("Random Box Setting")]
    public List<RandomBoxChance> RandomBoxChance;

    [Header("Character Settings")]
    public CharacterStat KnightStats;
    public CharacterStat WizardStats;
    public CharacterStat RogueStats;

    [Header("Animation Settings")]
    public float PlayerMoveTime;
    public AnimationCurve PlayerMoveCurve;

    [Header("Level Setting")]
    public List<int> EnemiesInEachLevel;
    public List<int> AlliesInEachLevel;
    public List<int> RandomBoxedInEachLevel;

    [Header("Encourage Sentences")]
    public List<string> EncourageSentences;
}
