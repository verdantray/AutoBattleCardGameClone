using System;
using ProjectABC.Core;
using UnityEngine;
using ProjectABC.Utils;

namespace ProjectABC.Engine
{
    public class Initializer : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 120;
            
            try
            {
                SceneLoader.Instance.GetLoadSceneTask(GameConst.SceneName.IN_GAME).Forget();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }
    }
}
