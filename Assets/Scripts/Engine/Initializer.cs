using System;
using UnityEngine;
using ProjectABC.Engine.Scene;
using ProjectABC.Utils;

namespace ProjectABC.Engine
{
    public class Initializer : MonoBehaviour
    {
        private void Start()
        {
            SceneLoader.Instance.LoadSceneAsync("InGame").Forget();
        }
    }
}
