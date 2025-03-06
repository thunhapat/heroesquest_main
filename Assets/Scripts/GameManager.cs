using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class GameManager : MonoSingleton<GameManager>
{
    [Header("Object References")]
    [SerializeField] Transform BoardParent;

    #region Private Properties
    Player _player;
    Dictionary<Vector2Int, GameObject> _tileDict = new Dictionary<Vector2Int, GameObject>();
    Dictionary<CombatCharacter, Vector2Int> _combatCharactersDict = new Dictionary<CombatCharacter, Vector2Int>();
    Dictionary<Vector2Int, GameObject> _obstaclesDict = new Dictionary<Vector2Int, GameObject>();
    Dictionary<Vector2Int, GameObject> _randomBoxesDict = new Dictionary<Vector2Int, GameObject>();

    int _levelGoal = 0;

    VFXMoveSuggestion _moveSuggestion;
    #endregion

    private void Start()
    {
        InitGame();
        StartGame();
    }

    private void Update()
    {
        float inputHorizontal = Input.GetAxis("Horizontal");
        float inputVertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(inputHorizontal) > 0.1f)
        {
            if(inputHorizontal > 0)
            {
                OnPlayerInputDirectionalKey(Vector2Int.right);
            }
            else
            {
                OnPlayerInputDirectionalKey(Vector2Int.left);
            }
        }
        else if (Mathf.Abs(inputVertical) > 0.1f)
        {
            if (inputVertical > 0)
            {
                OnPlayerInputDirectionalKey(Vector2Int.up);
            }
            else
            {
                OnPlayerInputDirectionalKey(Vector2Int.down);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            OnPlayerInputRotateFromLast();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            OnPlayerInputRotateToLast();
        }

    }

    public void InitGame()
    {
        //Generate playboard.
        GeneratePlayBoard();
    }

    public void StartGame()
    {
        GameAssetStore.Instance.Audios.BGMInGame.PlayAsBackgroundMusic();

        //Initialize player data.
        InitPlayer();

        //Spawn Obstacles
        //Clear existing obstacles;

        foreach(var obstacle in _obstaclesDict)
        {
            obstacle.Value.gameObject.ReturnToPool();
        }

        _obstaclesDict = new Dictionary<Vector2Int, GameObject>();

        for (int i = 0; i < GameSetting.Instance.InitialObstacleCount; i++)
        {
            SpawnObstacle();
        }

        //Clear all combat characters
        
        foreach(var combatChar in _combatCharactersDict)
        {
            combatChar.Key.gameObject.ReturnToPool();
        }

        _combatCharactersDict = new Dictionary<CombatCharacter, Vector2Int>();

        //clear all random boxes

        foreach (var randomBox in _randomBoxesDict)
        {
            randomBox.Value.gameObject.ReturnToPool();
        }

        _randomBoxesDict = new Dictionary<Vector2Int, GameObject>();

        //Create first character for player.
        SpawnCharacterForPlayer(RandomCharacterClass());

        //Create all elements in level.
        InitializeLevel();

        CombatCharacter partyLeader = _player.GetPartyLeader();
        CameraManager.Instance.SetFocus(partyLeader.transform);

        UpdateMainUI();
    }

    public void UpdateMainUI()
    {
        CombatCharacter partyLeader = _player.GetPartyLeader();
        if(partyLeader == null)
        {
            UIMain.Instance.ToggleShow(false);
            return;
        }
        else
        {
            UIMain.Instance.ToggleShow(true);
        }

        partyLeader.Data.RecalculateStats();

        float hpRatio = (float)partyLeader.Data.RemainingHP / (float)partyLeader.Data.MaxHP;
        int attack = partyLeader.Data.Attack;
        int defence = partyLeader.Data.Defence;

        if(_player.Charm != null)
        {
            attack += _player.Charm.BonusStat.Attack;
            defence += _player.Charm.BonusStat.Defence;
        }

        UIMain.Instance.UpdateUIMain(partyLeader.Data.CharacterClass.ToString(), partyLeader.Data.Level, hpRatio, attack.ToString(), defence.ToString(), _player.Charm);
    }

    public void TriggerEventAtCoord(Vector2Int targetCoord)
    {
        Dictionary<CombatCharacter, Vector2Int> tempCharDict = new Dictionary<CombatCharacter, Vector2Int>(_combatCharactersDict);
        tempCharDict.Remove(_player.GetPartyLeader()); //No coliding with party leader itself. 
        if (tempCharDict.ContainsValue(targetCoord))
        {
            CombatCharacter combatChar = null;
            foreach(KeyValuePair<CombatCharacter, Vector2Int> charData in tempCharDict)
            {
                if(charData.Value == targetCoord)
                {
                    combatChar = charData.Key;
                    if (combatChar.IsHero)
                    {
                        if(_player.IsMemberInParty(combatChar))
                        {
                            OnGameOver();

                            return;
                        }
                        else
                        {
                            _player.AddPartyMember(combatChar);

                            UpdateMainUI();
                        }
                    }
                    else
                    {
                        GameAssetStore.Instance.Audios.BGMBattle.PlayAsBackgroundMusic();
                        GameAssetStore.Instance.Audios.SFXEncounter.PlaySFXOneShot();

                        InitiateBattle(combatChar, 
                        (isPlayerWin) =>
                        {
                            GameAssetStore.Instance.Audios.BGMInGame.PlayAsBackgroundMusic();

                            UpdateMainUI();

                            if (isPlayerWin)
                            {
                                _combatCharactersDict.Remove(combatChar);
                                combatChar.gameObject.ReturnToPool();

                                _levelGoal--;

                                CheckLevelComplete();
                            }
                            
                            CheckGameOver();
                            
                        });
                    }

                    break;
                }
            }
        }
        else if (_obstaclesDict.ContainsKey(targetCoord))
        {
            RemovePlayerPartyLeader();
            GameAssetStore.Instance.VFX.VFXDead.GetFromPool(GetTile(targetCoord).transform.position);

            UpdateMainUI();
            CheckGameOver();
        }
        else if (_randomBoxesDict.ContainsKey(targetCoord))
        {
            _randomBoxesDict[targetCoord].ReturnToPool();
            _randomBoxesDict.Remove(targetCoord);

            TriggerRandomBox();
        }
    }

    public void OnGameOver()
    {
        _player.SetPlayerState(PlayerState.GameOver);
        UIGameOver.Open(_player.Level);
    }

    #region Playboard
    public void GeneratePlayBoard()
    {
        ClearPlayboard();

        for (int y = 0; y < GameSetting.Instance.BoardSize.y; y++)
        {
            for (int x = 0; x < GameSetting.Instance.BoardSize.x; x++)
            {
                Vector2Int coord = new Vector2Int(x, y);

                GameObject newTile = GameAssetStore.Instance.Prefabs.Tile.GetFromPool();
                newTile.gameObject.name = $"Tile:{coord}";
                newTile.transform.SetParent(BoardParent);
                Vector3 pos = new Vector3(x * GameSetting.Instance.TileSize, y * GameSetting.Instance.TileSize, 0f);
                newTile.transform.localPosition = pos;

                _tileDict.Add(coord, newTile);
            }
        }
    }

    public void ClearPlayboard()
    {
        foreach (var tile in _tileDict.Values)
        {
            tile.ReturnToPool();
        }

        _tileDict = new Dictionary<Vector2Int, GameObject>();
    }

    public void PlayMoveSuggestion()
    {

    }

    /// <summary>
    /// Check if tile at coordinate is occupied by player party member, collectible hero, enemy or obstacle.
    /// </summary>
    /// <param name="coord">Target coordinate</param>
    /// <returns></returns>
    public bool IsTileOccupied(Vector2Int coord)
    {
        if( _combatCharactersDict.ContainsValue(coord)||
            _obstaclesDict.ContainsKey(coord) ||
            _randomBoxesDict.ContainsKey(coord))
        {
            return true;
        }

        return false;
    }

    public List<Vector2Int> GetEmptyTileCoordiates(bool removeOuterTiles = false)
    {
        List<Vector2Int> availableTiles = new List<Vector2Int>();
        var allTiles = _tileDict.Keys;

        foreach(var tile in allTiles)
        {
            if(IsTileOccupied(tile))
            {
                continue;
            }

            availableTiles.Add(tile);
        }

        if(removeOuterTiles)
        {
            var outerTiles = GetOuterTileCoordinates();
            availableTiles.RemoveAll(x => outerTiles.Contains(x));
        }

        return availableTiles;
    }

    public GameObject GetTile(Vector2Int coord)
    {
        if (_tileDict.ContainsKey(coord))
        {
            return _tileDict[coord];
        }

        return null;
    }

    public List<Vector2Int> GetCornerCoordinates()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        //Top left
        result.Add(new Vector2Int(0, GameSetting.Instance.BoardSize.y - 1));
        //Top right
        result.Add(new Vector2Int(GameSetting.Instance.BoardSize.x - 1, GameSetting.Instance.BoardSize.y - 1));
        //bottom left
        result.Add(new Vector2Int(0, 0));
        //bottom right
        result.Add(new Vector2Int(GameSetting.Instance.BoardSize.x - 1, 0));

        return result;
    }

    public List<Vector2Int> GetOuterTileCoordinates()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach(Vector2Int coord in _tileDict.Keys)
        {
            if(coord.x == 0 || coord.y == 0 || coord.x == GameSetting.Instance.BoardSize.x - 1 || coord.y == GameSetting.Instance.BoardSize.y - 1)
            {
                result.Add(coord);
            }
        }

        return result;
    }

    public Vector2Int GetOppositeDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up)
        {
            return Vector2Int.down;
        }

        if (dir == Vector2Int.right)
        {
            return Vector2Int.left;
        }

        if (dir == Vector2Int.down)
        {
            return Vector2Int.up;
        }

        if (dir == Vector2Int.left)
        {
            return Vector2Int.right;
        }

        return Vector2Int.zero;
    }

    #endregion

    #region Character-related methods.

    public void SpawnCombatCharacter(Vector2Int coord, bool isEnemy)
    {
        CombatCharacterClass charClass = RandomCharacterClass();
        CombatCharacter combatChar = CreateCharacterObjByClass(charClass, isEnemy);
        combatChar.InitCombatCharacter(charClass, coord, !isEnemy);
        combatChar.Data.SetLevel(_player.Level, true);

        _combatCharactersDict.Add(combatChar, coord);

        GameObject targetTile = GetTile(coord);
        combatChar.Move(targetTile.transform.position, true);
    }

    public CombatCharacter CreateCharacterObjByClass(CombatCharacterClass charClass, bool isEnemy)
    {
        GameObject prefab;
        if(isEnemy)
        {
            switch (charClass)
            {
                default:
                case CombatCharacterClass.Knight:
                    prefab = GameAssetStore.Instance.Prefabs.EnemyKnight.GetFromPool();
                    break;

                case CombatCharacterClass.Wizard:
                    prefab = GameAssetStore.Instance.Prefabs.EnemyWizard.GetFromPool();
                    break;

                case CombatCharacterClass.Rogue:
                    prefab = GameAssetStore.Instance.Prefabs.EnemyRogue.GetFromPool();
                    break;
            }
        }
        else
        {
            switch (charClass)
            {
                default:
                case CombatCharacterClass.Knight:
                    prefab = GameAssetStore.Instance.Prefabs.HeroKnight.GetFromPool();
                    break;

                case CombatCharacterClass.Wizard:
                    prefab = GameAssetStore.Instance.Prefabs.HeroWizard.GetFromPool();
                    break;

                case CombatCharacterClass.Rogue:
                    prefab = GameAssetStore.Instance.Prefabs.HeroRogue.GetFromPool();
                    break;
            }
        }

        CombatCharacter combatChar = prefab.GetComponent<CombatCharacter>();

        return combatChar;
    }

    public void SpawnCharacterForPlayer(CombatCharacterClass charClass)
    {
        //Spawn character for player.
        CombatCharacter combatChar = CreateCharacterObjByClass(charClass, false);

        //Randomize the spawn position from 4 corners of the playboard.
        Vector2Int randomedSpawnCoord = Vector2Int.zero;
        List<Vector2Int> posssibleCoords = GetCornerCoordinates();
        if (posssibleCoords.Count > 0)
        {
            randomedSpawnCoord = posssibleCoords[Random.Range(0, posssibleCoords.Count)];
        }

        combatChar.InitCombatCharacter(charClass, randomedSpawnCoord, true);

        //Add the character to player's party.
        _player.AddPartyMember(combatChar, randomedSpawnCoord);
    }

    public void UpdateCombatCharacterCoordinate(Vector2Int coord, CombatCharacter combatChar)
    {
        if(_combatCharactersDict.ContainsKey(combatChar))
        {
            _combatCharactersDict[combatChar] = coord;
        }
        combatChar.SetCurrentCoordinate(coord);
    }

    #endregion

    #region Obstacle methods.
    public void SpawnObstacle()
    {
        List<Vector2Int> availableCoords = GetEmptyTileCoordiates(true);

        if (availableCoords.Count <= 0) return;

        Vector2Int randomedCoord = availableCoords[Random.Range(0, availableCoords.Count)];

        GameObject targetTile = GetTile(randomedCoord);

        GameObject newObstacle = GameAssetStore.Instance.Prefabs.Obstacle.GetFromPool();

        newObstacle.transform.position = targetTile.transform.position;

        _obstaclesDict.Add(randomedCoord, newObstacle);
    }

    public void TriggerRandomBox()
    {
        _player.SetPlayerState(PlayerState.PickingBox);

        ScriptableObject reward = RandomBox.GetRandomReward(GameSetting.Instance.RandomBoxChance);

        if (reward is CharmData)
        {
            CharmData charm = reward as CharmData;

            UIPopupConfirm.Open(
                charm,
                "You've got an item!" + System.Environment.NewLine
                + "Would you like to equip now?" + System.Environment.NewLine
                + "(Equiped Item will be destroyed.)",
                "Equip",
                "Discard",
                () =>
                {
                    _player.EquipCharm(charm);

                    UpdateMainUI();

                    _player.SetPlayerState(PlayerState.Idle);
                },
                () =>
                {
                    _player.SetPlayerState(PlayerState.Idle);
                }
                );
        }
        else if (reward is PotionData)
        {
            PotionData potion = reward as PotionData;

            UIPopupConfirm.Open(
                potion,
                "You've got an item!" + System.Environment.NewLine
                + "Would you like to use now?" + System.Environment.NewLine
                + "(This item cannot be saved for later use.)",
                "Use",
                "Discard",
                () =>
                {
                    GameAssetStore.Instance.Audios.SFXHeal.PlaySFXOneShot();
                    GameAssetStore.Instance.VFX.VFXHeal.GetFromPool(GetTile(_player.GetPartyLeader().CurrentCoordinate).transform.position);

                    foreach (var combatChar in _player.PartyMembers)
                    {
                        combatChar.OnTakeHeal(potion.RecoverAmount);
                    }

                    UpdateMainUI();

                    _player.SetPlayerState(PlayerState.Idle);
                },
                () => {
                    _player.SetPlayerState(PlayerState.Idle);
                }
                );
        }
    }
    #endregion

    #region Random Box methods.
    public void SpawnRandomBox()
    {
        List<Vector2Int> availableCoords = GetEmptyTileCoordiates(true);

        if (availableCoords.Count <= 0) return;

        Vector2Int randomedCoord = availableCoords[Random.Range(0, availableCoords.Count)];

        GameObject targetTile = GetTile(randomedCoord);

        GameObject newRandomBox = GameAssetStore.Instance.Prefabs.RandomBox.GetFromPool();

        newRandomBox.transform.position = targetTile.transform.position;

        _randomBoxesDict.Add(randomedCoord, newRandomBox);
    }

    #endregion

    #region Combat
    public void InitiateBattle(CombatCharacter enemy, UnityAction<bool> onComplete)
    {
        _player.SetPlayerState(PlayerState.InCombat);

        BattleManager.Instance.InitBattle(_player, enemy,
            (isPlayerWin)=>
            {
                _player.SetPlayerState(PlayerState.Idle);
                onComplete?.Invoke(isPlayerWin);
            }
        );
    }

    public bool IsClassAdvantage(CombatCharacter attacker, CombatCharacter defender)
    {
        switch (attacker.Data.CharacterClass)
        {
            case CombatCharacterClass.Knight:
                return defender.Data.CharacterClass == CombatCharacterClass.Rogue;

            case CombatCharacterClass.Wizard:
                return defender.Data.CharacterClass == CombatCharacterClass.Knight;

            case CombatCharacterClass.Rogue:
                return defender.Data.CharacterClass == CombatCharacterClass.Wizard;
        }

        return false;
    }
    #endregion

    #region Player Data
    public void InitPlayer()
    {
        _player = new Player();
        _player.InitPlayer();
    }

    public bool IsPlayerDead()
    {
        return _player.PartyMembers.Count <= 0;
    }

    public void RemovePlayerPartyLeader(bool isShiftLine = false)
    {
        if(isShiftLine)
        {
            _player.ShiftPartyLine();
        }

        CombatCharacter partyLeader = _player.GetPartyLeader();
        _player.RemovePartyMember(partyLeader, false);
        _combatCharactersDict.Remove(partyLeader);
        partyLeader.gameObject.ReturnToPool();

        //Try getting party leader again
        partyLeader = _player.GetPartyLeader();
        if(partyLeader != null)
        {
            CameraManager.Instance.SetFocus(partyLeader.transform);
        }
    }

    public void RotatePlayerPartyToLast()
    {
        if(_player.CurrentState == PlayerState.Idle)
        {
            _player.RotatePartyToLast();
            UpdateMainUI();
            CameraManager.Instance.SetFocus(_player.GetPartyLeader().transform);
        }

    }

    public void RotatePlayerPartyFromLast()
    {
        if (_player.CurrentState == PlayerState.Idle)
        {
            _player.RotatePartyFromLast();
            UpdateMainUI();
            CameraManager.Instance.SetFocus(_player.GetPartyLeader().transform);
        }
    }
    #endregion

    #region Level Manager
    public void InitializeLevel()
    {
        int heroesCount;
        int enemiesCount;
        int boxesCount;

        if(_player.Level > GameSetting.Instance.AlliesInEachLevel.Count)
        {
            heroesCount = GameSetting.Instance.AlliesInEachLevel.Last();
        }
        else
        {
            heroesCount = GameSetting.Instance.AlliesInEachLevel[_player.Level - 1];
        }

        if (_player.Level > GameSetting.Instance.EnemiesInEachLevel.Count)
        {
            enemiesCount = GameSetting.Instance.EnemiesInEachLevel.Last();
        }
        else
        {
            enemiesCount = GameSetting.Instance.EnemiesInEachLevel[_player.Level - 1];
        }

        if (_player.Level > GameSetting.Instance.RandomBoxedInEachLevel.Count)
        {
            boxesCount = GameSetting.Instance.RandomBoxedInEachLevel.Last();
        }
        else
        {
            boxesCount = GameSetting.Instance.RandomBoxedInEachLevel[_player.Level - 1];
        }

        SpawnLevelObjects(heroesCount, enemiesCount, boxesCount);

        _levelGoal = enemiesCount;
    }

    public void CheckLevelComplete()
    {
        if (IsLevelComplete())
        {
            _player.LevelUp();
            InitializeLevel();
        }
    }

    public bool IsLevelComplete()
    {
        return _levelGoal <= 0;
    }

    public void SpawnLevelObjects(int heroes, int enemies, int boxes)
    {
        //Spawn collectible heroes
        for (int i = 0; i < heroes; i++)
        {
            List<Vector2Int> availableCoords = GetEmptyTileCoordiates(true);
            SpawnCombatCharacter(availableCoords[Random.Range(0, availableCoords.Count)], false);
        }
        //Spawn enemy characters
        for (int i = 0; i < enemies; i++)
        {
            List<Vector2Int> availableCoords = GetEmptyTileCoordiates(true);
            SpawnCombatCharacter(availableCoords[Random.Range(0, availableCoords.Count)], true);
        }
        //Spawn Random Boxes
        for (int i = 0; i < boxes; i++)
        {
            SpawnRandomBox();
        }
    }

    public void CheckGameOver()
    {
        if(_player.PartyMembers.Count <= 0)
        {
            OnGameOver();
        }
    }

    IEnumerator OnPlayerLevelUp()
    {
        yield return null;
    }
    #endregion

    #region Player Input Handle
    public void OnPlayerInputDirectionalKey(Vector2Int direction)
    {
        if (_player.CurrentState == PlayerState.GameOver) return;

        Vector2Int oppositeDir = GetOppositeDirection(_player.LastDirection);

        if (direction == oppositeDir)
        {
            //Play Move Suggestion
            if (_moveSuggestion == null)
            {
                _moveSuggestion = GameAssetStore.Instance.VFX.VFXMoveSuggestion.GetFromPool().GetComponent<VFXMoveSuggestion>();
            }

            List<Vector2Int> possibleDirection = new List<Vector2Int>() { Vector2Int.down, Vector2Int.left, Vector2Int.right, Vector2Int.up };
            possibleDirection.Remove(oppositeDir);

            _moveSuggestion.PlayArrowSuggestion(possibleDirection);
            _moveSuggestion.transform.position = _player.GetPartyLeader().transform.position;
        }
        else
        {
            _player.MovePartyLine(direction);
        }
    }

    public void OnPlayerInputRotateFromLast()
    {
        RotatePlayerPartyFromLast();
    }

    public void OnPlayerInputRotateToLast()
    {
        RotatePlayerPartyToLast();
    }
    #endregion

    #region Helper
    CombatCharacterClass RandomCharacterClass()
    {
        return GameSetting.Instance.AvailableCharacterClasses[Random.Range(0, GameSetting.Instance.AvailableCharacterClasses.Count)];
    }
    #endregion
}
