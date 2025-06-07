using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayersGameBoard : MonoBehaviour
{
    public const float TimeToActiveCard = 0.5f;
    public const float TimeToBenchCard = 0.25f;
    
    [SerializeField] private SlotBench[] _slotBenches;
    [SerializeField] private Transform _deckSlot;
    [SerializeField] private Transform _activeCardSlot;

    [SerializeField] private GameObject _prefabCard;

    private readonly List<CardInGame> _activeCards = new();
    private readonly Dictionary<int, List<CardInGame>> _benchCards = new();
    
    public void OnDrawCardDefence(Card card)
    {
        var instance = Instantiate(_prefabCard, _deckSlot);
        var cardInGame = instance.GetComponent<CardInGame>();
        cardInGame.SetCard(card);

        StartCoroutine(DrawCard(cardInGame));
        _activeCards.Add(cardInGame);
    }
    public void OnDrawCardAttack(Card card)
    {
        var instance = Instantiate(_prefabCard, _deckSlot);
        var cardInGame = instance.GetComponent<CardInGame>();
        cardInGame.SetCard(card);
        
        StartCoroutine(DrawCard(cardInGame));
        _activeCards.Add(cardInGame);
    }
    public void OnFinishTurn()
    {
        StartCoroutine(FinishRound());
    }
    public void OnEndRound()
    {
        
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

    private IEnumerator FinishRound()
    {
        var targetCards = new List<CardInGame>();
        targetCards.AddRange(_activeCards);
        _activeCards.Clear();

        while (targetCards.Count > 0)
        {
            var card = targetCards[^1];
            var id = card.GetCardID();

            if (_benchCards.ContainsKey(id))
            {
                _benchCards[id].Add(card);
                targetCards.Remove(card);
            }
            else
            {
                var cards = new List<CardInGame> { card };
                _benchCards.Add(id, cards);
                targetCards.Remove(card);
            }

            var keys = _benchCards.Keys.ToList();
            var index = keys.IndexOf(id);
            var targetSlot = _slotBenches[index];

            var cardCount = _benchCards[id].Count - 1;
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
}
