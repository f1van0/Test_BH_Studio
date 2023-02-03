using System.Collections.Generic;
using Script.Data;
using Script.Lobby;
using UnityEngine;

namespace Script.Game.Character
{
    public class CharacterFactory
    {
        private readonly LevelStaticData _levelStaticData;
        private readonly GameObject _playerPrefab;
        private HashSet<int> _occupiedPoints = new();

        public CharacterFactory(LevelStaticData levelStaticData, GameObject playerPrefab)
        {
            _playerPrefab = playerPrefab;
            _levelStaticData = levelStaticData;
        }


        public CharacterContainer SpawnCharacter(LobbyPlayer lobbyPlayer)
        {
            var point = PickRandomPoint();
            var go = Object.Instantiate(_playerPrefab, point.position + Vector3.up * 2, point.rotation);

            var container = go.GetComponent<CharacterContainer>();
            container.lobbyPlayer = lobbyPlayer;

            return container;
        }


        private Transform PickRandomPoint()
        {
            int id;
            id = Random.Range(0, _levelStaticData.SpawnPoints.Length);
            while (_occupiedPoints.Contains(id))
            {
                id = Random.Range(0, _levelStaticData.SpawnPoints.Length);
            }

            _occupiedPoints.Add(id);
            var spawnPoint = _levelStaticData.SpawnPoints[id];
            return spawnPoint;
        }
    }
}