using System.Collections;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.Engine
{
    public class GameBoardController : MonoBehaviour
    {
        [SerializeField] private PlayersGameBoard _gameBoardPlayer;
        [SerializeField] private PlayersGameBoard _gameBoardEnemy;

        [SerializeField] private Transform _defenceIndicator;
        
        public void OnDrawCard(CardSnapshot card, MatchState playerState)
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
            var targetPoint = new Vector3(-1f, -1.5f, 0.5f);
            if (matchState == MatchState.Attacking)
            {
                targetAngle.z = 90f;
                targetPoint.x = 1f;
            }
            
            var localEulerAngles = _defenceIndicator.localEulerAngles;
            var startEuler = localEulerAngles;
            var endEuler = targetAngle;
            
            var startPosition = _defenceIndicator.localPosition;
            var endPosition = targetPoint;
            
            for (var t = 0f; t <= 1f; t += Time.deltaTime / 0.25f)
            {
                var targetEuler = Vector3.Lerp(startEuler, endEuler, t);
                _defenceIndicator.transform.localEulerAngles = targetEuler;
                
                var targetPosition = Vector3.Lerp(startPosition, endPosition, t);
                _defenceIndicator.transform.localPosition = targetPosition;
                yield return null;
            }

            var indicatorPosition = _defenceIndicator.transform;
            indicatorPosition.localEulerAngles = endEuler;
            indicatorPosition.localPosition = endPosition;
        }

        public void OnEndRound()
        {
            _gameBoardPlayer.OnEndRound();
            _gameBoardEnemy.OnEndRound();
        }
    }
}