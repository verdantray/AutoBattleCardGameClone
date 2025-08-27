using System;
using System.Collections;
using System.Collections.Generic;
using ProjectABC.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProjectABC.InGame
{
    public enum PlayerType
    {
        Player,
        Enemy
    }

    public class InGameControllerOld : MonoBehaviour
    {
        public List<CardDataOld> DEBUGCardDataList;
        public List<CardDataOld> DEBUGCardDataListA;
        public GameBoardController _gameBoardController;

        private readonly List<CardOld> _playerDeckCards = new();
        private readonly List<CardOld> _enemyDeckCards = new();

        private readonly List<CardOld> _playerActiveCards = new();
        private readonly List<CardOld> _enemyActiveCards = new();

        private readonly Dictionary<int, List<CardOld>> _playerBenchCards = new();
        private readonly Dictionary<int, List<CardOld>> _enemyBenchCards = new();

        private int RoundCount;
        private int TurnCount;

        [SerializeField] private PlayerType _currentPlayer = PlayerType.Enemy;


        private Action<CardOld, PlayerType> OnDrawCardDefence;
        private Action<CardOld, PlayerType> OnDrawCardAttack;
        private Action<PlayerType> OnFinishTurn;
        private Action<PlayerType> OnEndRound;
        private Action<List<CardDataOld>> OnStartSelectCards;

        
        public void Start()
        {
            OnDrawCardDefence += _gameBoardController.OnDrawCardDefence;
            OnDrawCardAttack += _gameBoardController.OnDrawCardAttack;
            OnFinishTurn += _gameBoardController.OnFinishTurn;
            OnEndRound += _gameBoardController.OnEndRound;
            
            OnEndRound += UIManager.Instance.DEBUGLayoutInGame.OnEndRound;
            // OnStartSelectCards += UIManager.Instance.DEBUGLayoutInGame.OnStartDrawCard;
            
            // UIManager.Instance.DEBUGLayoutInGame.OnFinishSelectCard += FinishSelectCard;

            SetCards();
            Shuffle();
            StartCoroutine(StartGame());
        }

        private void SetCards()
        {
            for (var i = 0; i < 6; i++)
            {
                var j = Random.Range(0, DEBUGCardDataList.Count);
                var card = new CardOld()
                {
                    Data = DEBUGCardDataList[j]
                };
                _playerDeckCards.Add(card);
            }

            for (var i = 0; i < 6; i++)
            {
                var j = Random.Range(0, DEBUGCardDataList.Count);
                var card = new CardOld()
                {
                    Data = DEBUGCardDataList[j]
                };
                _enemyDeckCards.Add(card);
            }
        }
        private void Shuffle()
        {
            _playerDeckCards.Shuffle();
            _enemyDeckCards.Shuffle();
        }
        
        private void FinishSelectCard(CardDataOld cardData)
        {
            var playerCard = new CardOld
            {
                Data = cardData
            };
            _playerDeckCards.Add(playerCard);
            
            // DEBUG
            var i = Random.Range(0, DEBUGCardDataListA.Count);
            var enemyCard = new CardOld
            {
                Data = DEBUGCardDataListA[i]
            };
            _enemyDeckCards.Add(enemyCard);
            
            StartCoroutine(StartGame());
        }

        private IEnumerator StartGame()
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(DrawCardDefence());
        }
        private IEnumerator DrawCardDefence()
        {
            switch (_currentPlayer)
            {
                case PlayerType.Enemy:
                {
                    var card = _enemyDeckCards[0];
                    _enemyDeckCards.Remove(card);
                    _enemyActiveCards.Add(card);
                    OnDrawCardDefence.Invoke(card, PlayerType.Enemy);
                    break;
                }
                case PlayerType.Player:
                {
                    var card = _playerDeckCards[0];
                    _playerDeckCards.Remove(card);
                    _playerActiveCards.Add(card);
                    OnDrawCardDefence.Invoke(card, PlayerType.Player);
                    break;
                }
            }

            yield return new WaitForSeconds(0.5f);
            FinishTurn();
        }
        private IEnumerator DrawCardOffense()
        {
            var isEnemyTurn = (_currentPlayer == PlayerType.Enemy);
            var targetPower = isEnemyTurn ? _playerActiveCards[^1].Data.Power : _enemyActiveCards[^1].Data.Power;
            var currentPower = 0;

            var offenseDeckCards = isEnemyTurn ? _enemyDeckCards : _playerDeckCards;
            var offenseActiveCards = isEnemyTurn ? _enemyActiveCards : _playerActiveCards;

            // DrawCard
            while (currentPower < targetPower)
            {
                if (offenseDeckCards.Count == 0)
                {
                    EndRound();
                    yield break;
                }

                var card = offenseDeckCards[0];
                currentPower += card.Data.Power;

                offenseDeckCards.Remove(card);
                offenseActiveCards.Add(card);
                OnDrawCardAttack.Invoke(card, _currentPlayer);

                yield return new WaitForSeconds(0.5f);
            }

            // Send Card to Bench
            var targetActiveCards = isEnemyTurn ? _playerActiveCards : _enemyActiveCards;
            var targetBenchCards = isEnemyTurn ? _playerBenchCards : _enemyBenchCards;

            var cardCount = targetActiveCards.Count;
            while (targetActiveCards.Count > 0)
            {
                var card = targetActiveCards[0];
                var id = card.Data.ID;

                if (targetBenchCards.ContainsKey(id))
                {
                    targetBenchCards[id].Add(card);
                    targetActiveCards.Remove(card);
                }
                else
                {
                    if (targetBenchCards.Count >= 6)
                    {
                        EndRound();
                        yield break;
                    }

                    var cards = new List<CardOld> { card };
                    targetBenchCards.Add(id, cards);
                    targetActiveCards.Remove(card);
                }
            }

            yield return new WaitForSeconds(0.25f * cardCount);
            yield return new WaitForSeconds(0.5f);
            FinishTurn();
        }
        private IEnumerator StartSelectCards(List<CardDataOld> cards)
        {
            yield return new WaitForSeconds(2f);
            OnStartSelectCards(cards);
        }

        private void FinishTurn()
        {
            switch (_currentPlayer)
            {
                case PlayerType.Enemy:
                {
                    _currentPlayer = PlayerType.Player;
                    OnFinishTurn.Invoke(_currentPlayer);
                    StartCoroutine(DrawCardOffense());
                    break;
                }
                case PlayerType.Player:
                {
                    _currentPlayer = PlayerType.Enemy;
                    OnFinishTurn.Invoke(_currentPlayer);
                    StartCoroutine(DrawCardOffense());
                    break;
                }
            }
        }
        private void EndRound()
        {
            switch (_currentPlayer)
            {
                case PlayerType.Enemy:
                {
                    _currentPlayer = PlayerType.Player;
                    OnEndRound.Invoke(_currentPlayer);
                    break;
                }
                case PlayerType.Player:
                {
                    _currentPlayer = PlayerType.Enemy;
                    OnEndRound.Invoke(_currentPlayer);
                    break;
                }
            }
            
            ClearCards();
            StartCoroutine(StartSelectCards(DEBUGCardDataListA));
        }

        private void ClearCards()
        {
            _playerDeckCards.AddRange(_playerActiveCards);
            foreach (var (id, list) in _playerBenchCards)
            {
                _playerDeckCards.AddRange(list);
            }
            
            _enemyDeckCards.AddRange(_enemyActiveCards);
            foreach (var (id, list) in _enemyBenchCards)
            {
                _enemyDeckCards.AddRange(list);
            }
            
            _playerActiveCards.Clear();
            _playerBenchCards.Clear();
            
            _enemyActiveCards.Clear();
            _enemyBenchCards.Clear();
        }
    }
}