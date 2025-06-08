using System.Collections;
using UnityEngine;

namespace ProjectABC.Core
{
    public class GameBoardController : MonoBehaviour
    {
        [SerializeField] private PlayersGameBoard _gameBoardPlayer;
        [SerializeField] private PlayersGameBoard _gameBoardEnemy;

        [SerializeField] private Transform _defenceIndicator;

        public void OnDrawCardDefence(Card card, PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.Player:
                    _gameBoardPlayer.OnDrawCardDefence(card);
                    break;

                case PlayerType.Enemy:
                    _gameBoardEnemy.OnDrawCardDefence(card);
                    break;
            }
        }

        public void OnDrawCardAttack(Card card, PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.Player:
                    _gameBoardPlayer.OnDrawCardAttack(card);
                    break;

                case PlayerType.Enemy:
                    _gameBoardEnemy.OnDrawCardAttack(card);
                    break;
            }
        }

        public void OnFinishTurn(PlayerType playerType)
        {
            switch (playerType)
            {
                case PlayerType.Player:
                    _gameBoardPlayer.OnFinishTurn();
                    break;

                case PlayerType.Enemy:
                    _gameBoardEnemy.OnFinishTurn();
                    break;
            }

            StartCoroutine(RotateIndicator());
        }

        public void OnEndRound(PlayerType playerType)
        {
            _gameBoardPlayer.OnEndRound();
            _gameBoardEnemy.OnEndRound();
        }

        private IEnumerator RotateIndicator()
        {
            var localEulerAngles = _defenceIndicator.localEulerAngles;
            var startEuler = localEulerAngles;
            var endEuler = localEulerAngles + new Vector3(0f, 0f, 180f);
            for (var t = 0f; t <= 1f; t += Time.deltaTime / 0.25f)
            {
                var targetEuler = Vector3.Lerp(startEuler, endEuler, t);
                _defenceIndicator.transform.localEulerAngles = targetEuler;
                yield return null;
            }

            _defenceIndicator.transform.localEulerAngles = endEuler;
        }
    }
}