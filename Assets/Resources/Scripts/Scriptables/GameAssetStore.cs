using UnityEngine;

[CreateAssetMenu(fileName = "GameAssetStore", menuName = "HeroesQuest/GameAssetStore")]
public class GameAssetStore : ScriptableSingleton<GameAssetStore>
{
    public Prefab Prefabs;
    public UIPrefab UI;
    public VisualEffect VFX;
    public Audio Audios;

    [System.Serializable]
    public class Prefab
    {
        public GameObject Tile;
        public GameObject Obstacle;
        public GameObject RandomBox;

        public GameObject HeroKnight;
        public GameObject HeroWizard;
        public GameObject HeroRogue;

        public GameObject EnemyKnight;
        public GameObject EnemyWizard;
        public GameObject EnemyRogue;

        public GameObject BattleRing;
    }

    [System.Serializable]
    public class UIPrefab
    {
        public GameObject UIBattle;
        public GameObject UIGameOver;
        public GameObject UIPopupConfirm;
    }

    [System.Serializable]
    public class VisualEffect
    {
        public GameObject VFXAttackHit;
        public GameObject VFXDead;
        public GameObject VFXHeal;
        public GameObject VFXMoveSuggestion;
    }

    [System.Serializable]
    public class Audio
    {
        public AudioClip BGMMain;
        public AudioClip BGMInGame;
        public AudioClip BGMBattle;

        public AudioClip SFXUIClick;

        public AudioClip SFXAttackHit;
        public AudioClip SFXEncounter;
        public AudioClip SFXHeal;
        public AudioClip SFXJump;
        public AudioClip SFXMovementStep;
    }
}
