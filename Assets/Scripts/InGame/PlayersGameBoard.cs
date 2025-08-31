using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectABC.Core;
using UnityEngine;

namespace ProjectABC.InGame
{
    public class PlayersGameBoard : MonoBehaviour
    {
        public const float TimeToActiveCard = 0.5f;
        public const float TimeToBenchCard = 0.25f;
        public const float TimeToDeckCard = 0.125f;

        [SerializeField] private SlotBench[] _slotBenches;
        [SerializeField] private Transform _deckSlot;
        [SerializeField] private Transform _activeCardSlot;
        [SerializeField] private Transform _cardBack;

        [SerializeField] private GameObject _prefabCard;

        private readonly List<CardInGame> _activeCards = new();
        private readonly List<CardInGame> _inactiveCards = new();
        private readonly Dictionary<string, List<CardInGame>> _benchCards = new();

        public void OnDrawCardDefence(Card card)
        {
            OnDrawCard(card);
        }
        public void OnDrawCardAttack(Card card)
        {
            OnDrawCard(card);
        }
        public void OnDrawCard(CardSnapshot card)
        {
            if (_inactiveCards.Count > 0)
            {
                var cardInGame = _inactiveCards[0];
                cardInGame.SetCard(card);
                
                StartCoroutine(DrawCard(cardInGame));
                _activeCards.Add(cardInGame);
                _inactiveCards.Remove(cardInGame);
            }
            else
            {
                var instance = Instantiate(_prefabCard, _deckSlot);
                var cardInGame = instance.GetComponent<CardInGame>();
                cardInGame.SetCard(card);
                
                StartCoroutine(DrawCard(cardInGame));
                _activeCards.Add(cardInGame);
            }
        }
        private void OnDrawCard(Card card)
        {
            if (_inactiveCards.Count > 0)
            {
                var cardInGame = _inactiveCards[0];
                cardInGame.SetCard(card);
                
                StartCoroutine(DrawCard(cardInGame));
                _activeCards.Add(cardInGame);
                _inactiveCards.Remove(cardInGame);
            }
            else
            {
                var instance = Instantiate(_prefabCard, _deckSlot);
                var cardInGame = instance.GetComponent<CardInGame>();
                cardInGame.SetCard(card);
                
                StartCoroutine(DrawCard(cardInGame));
                _activeCards.Add(cardInGame);
            }
        }
        public void SetCardBackVisibility(bool state)
        {
            _cardBack.gameObject.SetActive(state);
        }
        public void OnTryPutCardInfirmary()
        {
            StartCoroutine(FinishTurn());
        }
        public void OnEndRound()
        {
            StartCoroutine(EndRound());
        }

        private IEnumerator DrawCard(CardInGame card)
        {
            card.transform.SetParent(_activeCardSlot);
            card.FlipCard();

            var startPosition = card.transform.localPosition;
            var cardOffset = new Vector3(0f, 0.1f, 0.1f) * _activeCards.Count;
            var targetPosition = cardOffset;

            for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToActiveCard)
            {
                var position = Vector3.Lerp(startPosition, targetPosition, t);
                card.transform.localPosition = position;
                yield return null;
            }

            card.transform.localPosition = cardOffset;
        }
        private IEnumerator FinishTurn()
        {
            var targetCards = new List<CardInGame>();
            targetCards.AddRange(_activeCards);
            _activeCards.Clear();

            while (targetCards.Count > 0)
            {
                var card = targetCards[^1];
                var cardID = card.GetCardInstance().CardData.id;

                if (_benchCards.ContainsKey(cardID))
                {
                    _benchCards[cardID].Add(card);
                    targetCards.Remove(card);
                }
                else
                {
                    var cards = new List<CardInGame> { card };
                    _benchCards.Add(cardID, cards);
                    targetCards.Remove(card);
                }

                var keys = _benchCards.Keys.ToList();
                var index = keys.IndexOf(cardID);
                if (index >= _slotBenches.Length)
                    index = _slotBenches.Length - 1;
                var targetSlot = _slotBenches[index];
                
                var cardCount = _benchCards[cardID].Count - 1;
                var cardSpace = targetSlot.GetCardSpace() * cardCount;
                var cardOffset = new Vector3(0f, cardSpace, cardSpace);

                card.transform.SetParent(targetSlot.transform);

                var startPosition = card.transform.localPosition;
                var targetPosition = Vector3.zero + cardOffset;
                for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToBenchCard)
                {
                    var position = Vector3.Lerp(startPosition, targetPosition, t);
                    var scale = Vector3.Lerp(Vector3.one, targetSlot.GetCardSize(), t);

                    card.transform.localPosition = position;
                    card.transform.localScale = scale;

                    yield return null;
                }

                card.transform.localPosition = cardOffset;
                card.transform.localScale = targetSlot.GetCardSize();
            }

            yield return null;
        }
        private IEnumerator EndRound()
        {
            yield return new WaitForSeconds(2f);
            
            var activeCards = new List<CardInGame>();
            activeCards.AddRange(_activeCards);
            _activeCards.Clear();
            
            var benchCards = _benchCards.Keys.ToDictionary(key => key, key => _benchCards[key]);
            _benchCards.Clear();
            
            foreach (var card in activeCards)
            {
                card.transform.SetParent(_deckSlot);

                var startPosition = card.transform.localPosition;
                var targetPosition = Vector3.zero;
                
                for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToDeckCard)
                {
                    var position = Vector3.Lerp(startPosition, targetPosition, t);
                    card.transform.localPosition = position;
                    yield return null;
                }
            }
            
            while (true)
            {
                var targetBenchCards = new Dictionary<CardInGame, Vector3>();
                foreach (var (id, cards) in benchCards)
                {
                    if (cards.Count <= 0)
                        continue;
                    
                    var card = cards[^1];
                    card.transform.SetParent(_deckSlot);
                    
                    cards.Remove(card);
                    targetBenchCards.Add(card, card.transform.localPosition);
                }

                if (targetBenchCards.Count == 0)
                    break;
                
                var benchCardSize = _slotBenches[0].GetCardSize();
                for (var t = 0f; t <= 1f; t += Time.deltaTime / TimeToDeckCard)
                {
                    foreach (var (card, startPosition) in targetBenchCards)
                    {
                        card.transform.SetParent(_deckSlot);
                    
                        var targetPosition = Vector3.zero;
                        var position = Vector3.Lerp(startPosition, targetPosition, t);
                        var scale = Vector3.Lerp(benchCardSize, Vector3.one, t);

                        card.transform.localPosition = position;
                        card.transform.localScale = scale;
                    }
                    yield return null;
                }

                foreach (var (card, startPosition) in targetBenchCards)
                {
                    card.transform.localPosition = Vector3.zero;
                    card.transform.localScale = Vector3.one;
                }
            }
        }
    }
}