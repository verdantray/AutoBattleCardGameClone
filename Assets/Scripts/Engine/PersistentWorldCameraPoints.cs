using System;
using ProjectABC.Utils;
using Unity.Cinemachine;
using UnityEngine;

namespace ProjectABC.Engine
{
    public sealed class PersistentWorldCameraPoints : MonoSingleton<PersistentWorldCameraPoints>
    {
        [Serializable]
        private class CameraPoint
        {
            public string name;
            public CinemachineCamera point;
        }

        [SerializeField] private CameraPoint[] cameraPoints;
        protected override bool SetPersistent => false;

        private void OnEnable()
        {
            if (!IsInstance)
            {
                return;
            }
            
            SwapPoint("Default");
        }

        public void SwapPoint(string cameraName)
        {
            foreach (var cameraPoint in cameraPoints)
            {
                cameraPoint.point.gameObject.SetActive(cameraPoint.name == cameraName);
            }
        }
    }
}
