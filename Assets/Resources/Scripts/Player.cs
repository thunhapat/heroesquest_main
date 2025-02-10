using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player 
{
    public PlayerState CurrentState { get { return _currentState; } }
    public int Level { get { return _level; } }
    public List<CombatCharacter> PartyMembers { get { return _partyMembers; } }
    public CharmData Charm { get { return _charm; } }

    int _level;
    List<CombatCharacter> _partyMembers;
    CharmData _charm;
    Vector2Int _lastDirection;
    Vector2Int _lastSlotInLine;
    PlayerState _currentState;

    //Initialize player data.
    public void InitPlayer()
    {
        _currentState = PlayerState.Idle;
        _level = 1;
        _charm = null;

        //Clear player party.
        if (_partyMembers != null && _partyMembers.Count > 0)
        {
            foreach(CombatCharacter combatChar in PartyMembers)
            {
                combatChar.gameObject.ReturnToPool();
            }
        }
        _partyMembers = new List<CombatCharacter>();
    }

    public void SetPlayerState(PlayerState state)
    {
        _currentState = state;
    }

    public void LevelUp()
    {
        _level++;
    }

    public void EquipCharm(CharmData charm)
    {
        _charm = charm;
    }

    #region Player party-related methods
    public bool IsMemberInParty(CombatCharacter partyMember)
    {
        return _partyMembers.Contains(partyMember);
    }

    public void UpdatePartyMemberCoordinate(CombatCharacter partyMember, Vector2Int targetCoord)
    {
        if (IsMemberInParty(partyMember))
        {
            GameManager.Instance.UpdateCombatCharacterCoordinate(targetCoord, partyMember);
        }
    }

    public void AddPartyMember(CombatCharacter newMember)
    {
        if(IsMemberInParty(newMember))
        {
            return;
        }

        _partyMembers.Add(newMember);
        MovePartyMember(newMember, _lastSlotInLine, true);
        newMember.SetParty(true);
    }
    public void AddPartyMember(CombatCharacter newMember, Vector2Int coord)
    {
        if (IsMemberInParty(newMember))
        {
            return;
        }

        _partyMembers.Add(newMember);
        MovePartyMember(newMember, coord, true);
        newMember.SetParty(true);
    }

    public void RemovePartyMember(CombatCharacter partyMember, bool shiftParty = true)
    {
        if (!IsMemberInParty(partyMember))
        {
            return;
        }

        if(shiftParty)
        {
            //_lastSlotInLine = _partyMembers.Values.ToList().Last();

            int shiftIndex = _partyMembers.IndexOf(partyMember);
            ShiftPartyLine(shiftIndex);
        }

        _partyMembers.Remove(partyMember);
    }

    public void RotatePartyFromLast()
    {
        CombatCharacter lastMember = _partyMembers.Last();
        Vector2Int firstMemberCoord = _partyMembers.First().CurrentCoordinate;

        for (int i = 0; i < _partyMembers.Count - 1; i++)
        {
            Vector2Int moveToCoord = _partyMembers[i + 1].CurrentCoordinate;
            MovePartyMember(_partyMembers[i], moveToCoord);
        }

        _partyMembers.Remove(lastMember);

        _partyMembers.Insert(0, lastMember);
        MovePartyMember(lastMember, firstMemberCoord, true);
    }
    public void RotatePartyToLast()
    {
        CombatCharacter firstMember = _partyMembers.First();
        Vector2Int lastMemberCoord = _partyMembers.Last().CurrentCoordinate;
        RemovePartyMember(firstMember);
        AddPartyMember(firstMember, lastMemberCoord);
    }

    public CombatCharacter GetPartyLeader()
    {
        if(_partyMembers.Count > 0)
        {
            return _partyMembers.First();
        }

        return null;
    }

    public void MovePartyLine(Vector2Int direction)
    {
        CombatCharacter partyLeader = GetPartyLeader();

        if(partyLeader == null)
        {
            //This probably means game is over.
            return;
        }

        if (CanMove(partyLeader.CurrentCoordinate, direction))
        {
            GameAssetStore.Instance.Audios.SFXMovementStep.PlaySFXOneShot();

            _lastDirection = direction;

            Vector2Int targetCoord = partyLeader.CurrentCoordinate + direction;

            //Move members
            _lastSlotInLine = _partyMembers.Last().CurrentCoordinate;
            ShiftPartyLine();

            //Move Leader
            _currentState = PlayerState.Moving;
            MovePartyMember(partyLeader, targetCoord, false, 
                ()=> 
                {
                    _currentState = PlayerState.Idle;
                    GameManager.Instance.TriggerEventAtCoord(targetCoord); 
                });
        }
    }

    public void ShiftPartyLine(int stopAt = 0)
    {
        for (int i = _partyMembers.Count - 1; i > stopAt; i--)
        {
            Vector2Int moveToCoord = _partyMembers[i - 1].CurrentCoordinate;
            MovePartyMember(_partyMembers[i], moveToCoord);
        }
    }

    public void MovePartyMember(CombatCharacter member, Vector2Int toCoord, bool isInstantMove = false, UnityAction onComplete = null)
    {
        GameObject targetTile = GameManager.Instance.GetTile(toCoord);

        member.Move(targetTile.transform.position, isInstantMove, onComplete);

        GameManager.Instance.UpdateCombatCharacterCoordinate(toCoord, member);
    }
    #endregion

    #region Helper
    /// <summary>
    /// Check if player is in movable state and can move to target direction.
    /// </summary>
    /// <param name="targetDirection">Target direction player trying to move to</param>
    /// <param name="currectCoord">Currect Coordinate of party leader.</param>
    /// <returns></returns>
    public bool CanMove(Vector2Int currectCoord, Vector2Int targetDirection)
    {
        //Player isnt in state of movable.
        if (_currentState != PlayerState.Idle)
        {
            return false;
        }

        //Player can't go opposite direction of last moved direction.
        if ((_lastDirection == Vector2Int.up && targetDirection == Vector2.down) ||
            (_lastDirection == Vector2Int.down && targetDirection == Vector2.up) ||
            (_lastDirection == Vector2Int.left && targetDirection == Vector2.right) ||
            (_lastDirection == Vector2Int.right && targetDirection == Vector2.left))
        {
            return false;
        }

        //Player can't move in target direct if there is no tile at that direction.
        if(GameManager.Instance.GetTile(currectCoord + targetDirection) == null)
        {
            return false;
        }

        return true;
    }
    #endregion
}

public enum PlayerState
{
    Idle,
    Moving,
    InCombat,
    PickingBox,
    GameOver
}
