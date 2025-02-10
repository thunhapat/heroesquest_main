using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CombatCharacter : MonoBehaviour
{
    [Header("Object References")]
    public Transform CharSprite;
    public Animator Anim;

    public CombatCharacterData Data { get { return _data; } }
    public bool IsHero { get { return _isHero; } }
    public Vector2Int CurrentCoordinate { get { return _currentCoordinate; } }

    CombatCharacterData _data;
    bool _isHero;
    Vector2Int _currentCoordinate;
    Coroutine _moveCoroutine;

    public void InitCombatCharacter(CombatCharacterClass charClass, Vector2Int coord, bool isHero)
    {
        Anim.keepAnimatorStateOnDisable = true;

        _data = new CombatCharacterData();
        _data.InitCharacterData(charClass);

        _currentCoordinate = coord;

        _isHero = isHero;
        Anim.SetBool("IsHero", isHero);
        SetParty(false);
    }

    private void OnDisable()
    {
        //Reset changes done by animator.
        Anim.Play("Character_Default", 0, 0f);
    }

    public void SetCurrentCoordinate(Vector2Int coord)
    {
        _currentCoordinate = coord;
    }

    public void SetParty(bool isInParty)
    {
        Anim.SetBool("InParty", isInParty);
    }

    public void OnLevelUp(int level)
    {
        _data.SetLevel(level++);
    }

    public void OnTakeDamage(int damage)
    {
        int hp = Mathf.Clamp(_data.RemainingHP - damage, 0, _data.MaxHP);
        _data.UpdateRemainingHP(hp);
    }
    public void OnTakeHeal(int amount)
    {
        int hp = Mathf.Clamp(_data.RemainingHP + amount, 0, _data.MaxHP);
        _data.UpdateRemainingHP(hp);
    }

    public bool IsDead()
    {
        return _data.RemainingHP <= 0;
    }
    public void SetFacing(Vector2Int dir)
    {
        SetFacing(dir.x);
    }
    public void SetFacing(float dir)
    {
        Vector3 localScale = CharSprite.localScale;
        localScale.x = dir;
        CharSprite.localScale = localScale;
    }

    public void Move(Vector2 targetPos, bool isInstantMove = false, UnityAction onComplete = null)
    {
        if(isInstantMove)
        {
            transform.position = targetPos;
            onComplete?.Invoke();
            return;
        }

        if(_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
        _moveCoroutine = StartCoroutine(MoveCoroutine(targetPos, onComplete));
    }

    IEnumerator MoveCoroutine(Vector2 target, UnityAction onComplete = null)
    {
        Vector2 currentPos = transform.position;

        float progress = 0f;

        if (target.x < currentPos.x)
        {
            SetFacing(Vector2Int.left);
        }
        else if (target.x > currentPos.x)
        {
            SetFacing(Vector2Int.right);
        }

        Anim.SetTrigger("Walk");

        while (progress < 1f)
        {
            progress += Time.deltaTime / GameSetting.Instance.PlayerMoveTime;

            Vector2 nextMoveTick = Vector2.Lerp(currentPos, target, GameSetting.Instance.PlayerMoveCurve.Evaluate(progress));

            transform.position = nextMoveTick;

            yield return null;
        }
        onComplete?.Invoke();
    }

}

public class CombatCharacterData
{
    public CombatCharacterClass CharacterClass { get { return _characterClass; } }
    public int MaxHP { get { return _maxHP; } }
    public int RemainingHP { get { return _remainingHP; } }
    public int Attack { get { return _attack; } }
    public int Defence { get { return _defence; } }
    public int Level { get { return _level; } }

    CombatCharacterClass _characterClass;
    int _maxHP;
    int _remainingHP;
    int _attack;
    int _defence;
    int _level;

    public void InitCharacterData(CombatCharacterClass charClass)
    {
        _characterClass = charClass;
        SetLevel(1);
        UpdateRemainingHP(_maxHP);
    }

    public void UpdateRemainingHP(int hp)
    {
        _remainingHP = hp;
    }

    public void SetLevel(int level)
    {
        _level = level;
        RecalculateStats();
    }

    public void RecalculateStats(bool isRecoverHP = false)
    {
        CharacterStat charStat = null;
        switch (_characterClass)
        {
            default:
            case CombatCharacterClass.Knight:
                charStat = new CharacterStat(GameSetting.Instance.KnightStats);
                break;

            case CombatCharacterClass.Wizard:
                charStat = new CharacterStat(GameSetting.Instance.WizardStats);
                break;

            case CombatCharacterClass.Rogue:
                charStat = new CharacterStat(GameSetting.Instance.RogueStats);
                break;
        }

        _maxHP = charStat.BaseStat.MaxHP + (charStat.StatPerLevel.MaxHP * _level);
        _attack = charStat.BaseStat.Attack + (charStat.StatPerLevel.Attack * _level);
        _defence = charStat.BaseStat.Defence + (charStat.BaseStat.Defence * _level);
        if(isRecoverHP)
        {
            _remainingHP = _maxHP;
        }
    }
}

[System.Serializable]
public class CharacterStat
{
    public BasicStat BaseStat;
    public BasicStat StatPerLevel;

    public CharacterStat()
    {

    }
    public CharacterStat(CharacterStat charStat)
    {
        BaseStat = charStat.BaseStat;
        StatPerLevel = charStat.StatPerLevel;
    }

    public CharacterStat(BasicStat baseStat, BasicStat statPerLevel)
    {
        BaseStat = baseStat;
        StatPerLevel = statPerLevel;
    }
}

[System.Serializable]
public struct BasicStat
{
    public int MaxHP;
    public int Attack;
    public int Defence;
}

public enum CombatCharacterClass
{
    None = 0,

    Knight = 1,
    Wizard = 2,
    Rogue = 3
}
