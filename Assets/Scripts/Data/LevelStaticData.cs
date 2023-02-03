using System.Linq;
using Cinemachine;
using UnityEngine;

namespace Data
{
    public class LevelStaticData : MonoBehaviour
    {
        public Transform[] SpawnPoints;
        public Camera MainCamera;
        public CinemachineVirtualCamera ThirdPersonCamera;

        private void Reset()
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(Constants.SpawnPoint);
            SpawnPoints = spawnPoints.Select(go => go.transform).ToArray();
        }
    }
}