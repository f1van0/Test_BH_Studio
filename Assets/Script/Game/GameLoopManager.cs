using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Script.Data;
using Script.Game.Character;
using Script.UI;
using UnityEngine;

public class GameLoopManager : NetworkBehaviour
{
    public event Action<CharacterContainer, int> GameFinished;
    public event Action<CharacterContainer, int> HitRegistered;

    [field: SerializeField] private GameUI _gameUI;

    private readonly SyncDictionary<uint, ScoreContainer> _playersScoresContainer = new();
    private Dictionary<uint, CharacterContainer> _characters = new();
    private GameSettings _gameSettings;
    private LevelStaticData _levelStaticData;
    private AdvanceNetworkManager _advanceNetworkManager;
    private CharacterContainer _localPlayer;

    public override void OnStartClient()
    {
        var roomManager = (AdvanceNetworkManager) NetworkManager.singleton;
        roomManager.OnClientArenaLoaded(this);
        GameFinished += ShowWinner;
    }

    public void Initialize(
        AdvanceNetworkManager networkManager,
        GameSettings gameSettings,
        LevelStaticData levelStaticData)
    {
        _advanceNetworkManager = networkManager;
        _gameSettings = gameSettings;
        _levelStaticData = levelStaticData;
        _gameUI.Initialize(_gameSettings, this);
    }

    public void SetupPlayer(CharacterContainer container)
    {
        Debug.Log($"Setup player {container.netId}");
        _characters.Add(container.netId, container);
        var localPlayer = container.Identity.isLocalPlayer;
        if (localPlayer)
        {
            AttachCamera(container);
            InitializeLocalClient(container);
        }
        else
        {
            InitializeClient(container);
        }

        if (!localPlayer)
            AttachUI(container);

        if (isServer) FillProgress(container);
    }

    [Server]
    public void RegisterHit(uint gainerId, uint targetId)
    {
        var gainerIdentity = _characters[gainerId].Identity;
        var playerScore = _playersScoresContainer[gainerId].Scores;
        var hitId = playerScore.FindIndex(hits => hits.NetId == targetId);

        var playerHits = playerScore[hitId];
        playerHits.Count++;
        playerScore[hitId] = playerHits;
        _playersScoresContainer[gainerId] = new ScoreContainer() {Scores = playerScore};

        TargetIncrementHitRpc(gainerIdentity.connectionToClient, gainerId, targetId, playerHits.Count);
        CheckWinner(gainerId);
    }

    [TargetRpc]
    private void TargetIncrementHitRpc(NetworkConnection connection, uint gainerId, uint targetId, int score)
    {
        var playerContainer = _characters[targetId];
        HitRegistered?.Invoke(playerContainer, score);
    }

    [ClientRpc]
    private void FinishMatchRpc(CharacterContainer container, int score) =>
        GameFinished?.Invoke(container, score);


    private void CheckWinner(uint gainerId)
    {
        var scores = _playersScoresContainer[gainerId].Scores;
        if (scores.All(score => score.Count >= _gameSettings.RequireHitCount))
        {
            var scoreAmount = scores.Sum(score => score.Count);
            Debug.Log($"Got winner [{gainerId}]:{scoreAmount}");
            FinishMatchRpc(_characters[gainerId], scoreAmount);
            StartCoroutine(RestartMatch(_gameSettings.MatchReloadDelay));
        }
    }

    private IEnumerator RestartMatch(float restartTime)
    {
        yield return new WaitForSecondsRealtime(restartTime);
        _advanceNetworkManager.SwitchToArena();
    }

    private void AttachUI(CharacterContainer container) =>
        _gameUI.DisplayPlayerScore(container);

    private void ShowWinner(CharacterContainer player, int score) =>
        _gameUI.ShowWinner(player, score, _gameSettings.MatchReloadDelay);

    private void FillProgress(CharacterContainer container)
    {
        var playerIdentity = container.Identity;
        var scoreContainer = new ScoreContainer {Scores = new List<PlayerScore>()};
        _playersScoresContainer.Add(playerIdentity.netId, scoreContainer);
        foreach (var pair in _playersScoresContainer)
        {
            if (pair.Key == playerIdentity.netId) continue;

            var enemyProgress = pair.Value;
            var enemyId = pair.Key;

            enemyProgress.Scores.Add(new PlayerScore(playerIdentity.netId, 0));
            scoreContainer.Scores.Add(new PlayerScore(enemyId, 0));
        }
    }

    private void InitializeClient(CharacterContainer container)
    {
        container.Character.enabled = false;
        container.Movement.enabled = true;
        container.Rotation.enabled = true;
        container.ChargeAbility.Initialize(this, _gameSettings);
    }

    private void InitializeLocalClient(CharacterContainer container)
    {
        _localPlayer = container;
        container.Input.enabled = true;
        container.CameraController.enabled = true;
        container.Movement.enabled = true;
        container.Rotation.enabled = true;

        container.Input.ActivateInput();
        container.ChargeAbility.Initialize(this, _gameSettings);
    }

    public void AttachCamera(CharacterContainer container) =>
        container.CameraController.AttachCamera(_levelStaticData.ThirdPersonCamera);
}
