using System.Collections;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.InGame
{
    public class GameBoardController : MonoBehaviour
    {
        [SerializeField] private PlayersGameBoard _gameBoardPlayer;
        [SerializeField] private PlayersGameBoard _gameBoardEnemy;

        [SerializeField] private Transform _defenceIndicator;
        
        public void OnDrawCard(CardInstance card, MatchState playerState)
        {
            switch (playerState)
            {
                case MatchState.Attacking:
                    _gameBoardPlayer.OnDrawCard(card);
                    break;

                case MatchState.Defending:
                    _gameBoardEnemy.OnDrawCard(card);
                    break;
            }
        }
        public void SetCardBackVisibility(MatchState playerState, bool state)
        {
            switch (playerState)
            {
                case MatchState.Attacking:
                    _gameBoardPlayer.SetCardBackVisibility(state);
                    break;

                case MatchState.Defending:
                    _gameBoardEnemy.SetCardBackVisibility(state);
                    break;
            }
        }

        public void OnTryPutCardInfirmary(MatchState matchState)
        {
            switch (matchState)
            {
                case MatchState.Attacking:
                    _gameBoardEnemy.OnTryPutCardInfirmary();
                    break;

                case MatchState.Defending:
                    _gameBoardPlayer.OnTryPutCardInfirmary();
                    break;
            }
        }

        public void OnFinishTurn(MatchState matchState)
        {
            StartCoroutine(RotateIndicator(matchState));
        }

        private IEnumerator RotateIndicator(MatchState matchState)
        {
            var targetAngle = new Vector3(0f, 0f, -90f);
            if (matchState == MatchState.Attacking)
                targetAngle.z = 90f;
            
            var localEulerAngles = _defenceIndicator.localEulerAngles;
            var startEuler = localEulerAngles;
            var endEuler = targetAngle;
            for (var t = 0f; t <= 1f; t += Time.deltaTime / 0.25f)
            {
                var targetEuler = Vector3.Lerp(startEuler, endEuler, t);
                _defenceIndicator.transform.localEulerAngles = targetEuler;
                yield return null;
            }

            _defenceIndicator.transform.localEulerAngles = endEuler;
        }

        public void OnEndRound()
        {
            _gameBoardPlayer.OnEndRound();
            _gameBoardEnemy.OnEndRound();
        }
    }
}