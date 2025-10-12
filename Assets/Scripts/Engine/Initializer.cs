using ProjectABC.Core;
using UnityEngine;
using ProjectABC.Utils;

namespace ProjectABC.Engine
{
    public class Initializer : MonoBehaviour
    {
        private void Start()
        {
            SceneLoader.Instance.LoadSceneAsync(GameConst.SceneName.IN_GAME).Forget();
        }
    }
}
