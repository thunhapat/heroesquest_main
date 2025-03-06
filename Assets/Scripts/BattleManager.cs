using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleManager : MonoSingleton<BattleManager>
{
    [Header("Object References")]
    [SerializeField] Transform DummyHeroParent;
    [SerializeField] Transform DummyEnemyParent;
    [SerializeField] Transform BattleRing;

    CombatCharacter _dummyHero;
    CombatCharacter _dummyEnemy;

    Player _player;
    CombatCharacter _hero;
    CombatCharacter _enemy;
    CharmData _playerCharm;

    Coroutine _battleCoroutine;

    /// <summary>
    /// Initialize battle info and then start battle
    /// </summary>
    /// <param name="player">Player data</param>
    /// <param name="enemy">Enemy combat character that player is fighting</param>
    /// <param name="onBattleComplete">Called when battle is over.</param>
    public void InitBattle(Player player, CombatCharacter enemy, UnityAction<bool> onBattleComplete)
    {
        _player = player;

        CombatCharacter hero = _player.GetPartyLeader();

        _hero = hero;
        _enemy = enemy;
        _playerCharm = _player.Charm;

        UIBattle.Open(hero, enemy, _playerCharm);

        BattleRing.gameObject.SetActive(true);

        _battleCoroutine = StartCoroutine(BattleCoroutine(onBattleComplete));
    }

    public CombatCharacter CreateDummyCharacter(Transform parent, CombatCharacterClass charClass , bool isEnemy)
    {
        CombatCharacter newDummy = GameManager.Instance.CreateCharacterObjByClass(charClass, isEnemy);
        ChangeLayerInChildren(newDummy.gameObject, 3); //Battle Layer
        newDummy.SetParty(!isEnemy);
        newDummy.transform.SetParent(parent);
        newDummy.CharSprite.localScale = Vector3.one;
        newDummy.transform.localPosition = Vector3.zero;
        newDummy.transform.localScale = Vector3.one;
        newDummy.Anim.SetTrigger("Entrance");
        return newDummy;
    }

    public void DestroyDummyCharacter(CombatCharacter dummy)
    {
        //Change to default layer before destroy.
        ChangeLayerInChildren(dummy.gameObject, 0); //Default Layer
        dummy.gameObject.ReturnToPool();
    }

    public void ChangeLayerInChildren(GameObject gameObj, int layer)
    {
        foreach(Transform t in gameObj.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }
    }

    /// <summary>
    /// Resolve battle damage, decrease HP of both combatant at the same time.
    /// </summary>
    void ResolveBattleDamage()
    {
        BasicStat charmStat = _playerCharm != null ? _playerCharm.BonusStat : new BasicStat();

        (float damage, bool isCrit) damageToEnemy = CalculateDamage(_hero, _enemy, charmStat, new BasicStat());
        (float damage, bool isCrit) damageToHero = CalculateDamage(_enemy, _hero, new BasicStat(), charmStat);


        _hero.OnTakeDamage(Mathf.CeilToInt(damageToHero.damage));
        UIBattle.Instance.PlayDamageTextOnHero(Mathf.CeilToInt(damageToHero.damage), damageToHero.isCrit);

        _enemy.OnTakeDamage(Mathf.CeilToInt(damageToEnemy.damage));
        UIBattle.Instance.PlayDamageTextOnEnemy(Mathf.CeilToInt(damageToEnemy.damage), damageToEnemy.isCrit);
    }

    /// <summary>
    /// Calculate damage done by combat character.
    /// </summary>
    /// <param name="attacker">Attacker</param>
    /// <param name="defender">Defender</param>
    /// <param name="attackerBonusStat">Stat bonus for attacker</param>
    /// <param name="defenderBonusStat">Stat bonus for defender</param>
    /// <returns>Damage dealt by attacker to defeder</returns>
    (float, bool) CalculateDamage(CombatCharacter attacker, CombatCharacter defender, BasicStat attackerBonusStat, BasicStat defenderBonusStat)
    {
        //Calculate stat 
        float attacker_Atk = attacker.Data.Attack + attackerBonusStat.Attack;
        float defender_Def = defender.Data.Defence + defenderBonusStat.Defence;

        float totalDamage = attacker_Atk - defender_Def;

        bool isCritical = GameManager.Instance.IsClassAdvantage(attacker, defender);
        //Checking class advantage
        if (isCritical)
        {
            totalDamage *= GameSetting.Instance.AdvantageDamageMultiplier;
        }
        //Check if its minimum damage.
        totalDamage = Mathf.Clamp(totalDamage, GameSetting.Instance.MinumumDamage, totalDamage);

        return (totalDamage, isCritical);
    }

    /// <summary>
    /// Complete battle, hide all battle ui and return all gameObject to pool.
    /// </summary>
    void BattleDone()
    {
        //Return all dummy game obj to pool
        DestroyDummyCharacter(_dummyHero);
        DestroyDummyCharacter(_dummyEnemy);

        //Hide all battle gameObjects;
        BattleRing.gameObject.SetActive(false);
        UIBattle.Instance.gameObject.SetActive(false);

    }

    /// <summary>
    /// Coroutine that control battle from start to end.
    /// </summary>
    /// <param name="onBattleComplete">Called when battle is over</param>
    /// <returns></returns>
    IEnumerator BattleCoroutine(UnityAction<bool> onBattleComplete)
    {
        bool isPlayerWin = false;

        yield return null;

        //Create all combatants
        _dummyHero = CreateDummyCharacter(DummyHeroParent, _hero.Data.CharacterClass, false);
        _dummyEnemy = CreateDummyCharacter(DummyEnemyParent, _enemy.Data.CharacterClass, true);

        yield return new WaitForSeconds(1f); //Start Delay

        while(!_enemy.IsDead())
        {
            //Play animation anim for both combatant and wait. 
            _dummyHero.Anim.SetTrigger($"Attack_{_hero.Data.CharacterClass}");
            _dummyEnemy.Anim.SetTrigger($"Attack_{_enemy.Data.CharacterClass}");

            yield return new WaitForSeconds(0.35f); 

            //Resolve battle damage at the same time.
            ResolveBattleDamage();
            UIBattle.Instance.UpdateHPBar();
            GameAssetStore.Instance.VFX.VFXAttackHit.GetFromPool(_dummyHero.transform.position);
            GameAssetStore.Instance.VFX.VFXAttackHit.GetFromPool(_dummyEnemy.transform.position);

            yield return new WaitForSeconds(0.4f);

            if(_enemy.IsDead())
            {
                //Play dead VFX
                GameAssetStore.Instance.VFX.VFXDead.GetFromPool(_dummyEnemy.transform.position);

                isPlayerWin = true;
                _dummyEnemy.gameObject.ReturnToPool();

            }

            if(_hero.IsDead())
            {
                //Play dead VFX
                GameAssetStore.Instance.VFX.VFXDead.GetFromPool(_dummyHero.transform.position);

                _dummyHero.gameObject.ReturnToPool();
                yield return new WaitForSeconds(1f); //Wait for Dead animation

                //Remove first party leader and shift line.
                GameManager.Instance.RemovePlayerPartyLeader(true);
                if(GameManager.Instance.IsPlayerDead())
                {
                    //No more character left to fight, game is going to over.
                    break;
                }

                //Change party leader and continue the fight.
                _hero = _player.GetPartyLeader();
                UIBattle.Instance.Init(_hero, _enemy, _playerCharm);

                _dummyHero = CreateDummyCharacter(DummyHeroParent, _hero.Data.CharacterClass, false);

                yield return new WaitForSeconds(1f); //Wait for entrance animation
            }
        }

        yield return new WaitForSeconds(1f); //Wait for entrance animation

        BattleDone();
        onBattleComplete?.Invoke(isPlayerWin);
    }
}
