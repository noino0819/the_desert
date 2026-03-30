using System.Collections.Generic;
using UnityEngine;

namespace TheSSand.Card
{
    /// <summary>
    /// 덱 관리 — 드로우 파일/손패/버린 파일 순환
    /// Slay the Spire 스타일 덱빌딩 시스템
    /// </summary>
    public class DeckManager
    {
        readonly List<CardData> _masterDeck = new();
        readonly List<CardData> _drawPile = new();
        readonly List<CardData> _hand = new();
        readonly List<CardData> _discardPile = new();
        readonly List<CardData> _exhaustPile = new();

        public IReadOnlyList<CardData> Hand => _hand;
        public IReadOnlyList<CardData> DrawPile => _drawPile;
        public IReadOnlyList<CardData> DiscardPile => _discardPile;
        public int DrawPileCount => _drawPile.Count;
        public int DiscardPileCount => _discardPile.Count;

        public event System.Action OnDeckChanged;

        public void InitDeck(IEnumerable<CardData> cards)
        {
            _masterDeck.Clear();
            _masterDeck.AddRange(cards);
            Reset();
        }

        public void AddCard(CardData card)
        {
            _masterDeck.Add(card);
            _discardPile.Add(card);
            OnDeckChanged?.Invoke();
        }

        public void Reset()
        {
            _drawPile.Clear();
            _hand.Clear();
            _discardPile.Clear();
            _exhaustPile.Clear();

            _drawPile.AddRange(_masterDeck);
            Shuffle(_drawPile);
            OnDeckChanged?.Invoke();
        }

        public List<CardData> Draw(int count)
        {
            var drawn = new List<CardData>();

            for (int i = 0; i < count; i++)
            {
                if (_drawPile.Count == 0)
                    ReshuffleDiscardIntoDraw();

                if (_drawPile.Count == 0) break;

                var card = _drawPile[0];
                _drawPile.RemoveAt(0);
                _hand.Add(card);
                drawn.Add(card);
            }

            OnDeckChanged?.Invoke();
            return drawn;
        }

        public bool PlayCard(CardData card)
        {
            if (!_hand.Contains(card)) return false;

            _hand.Remove(card);
            _discardPile.Add(card);
            OnDeckChanged?.Invoke();
            return true;
        }

        public void ExhaustCard(CardData card)
        {
            _hand.Remove(card);
            _exhaustPile.Add(card);
            OnDeckChanged?.Invoke();
        }

        public void DiscardHand()
        {
            _discardPile.AddRange(_hand);
            _hand.Clear();
            OnDeckChanged?.Invoke();
        }

        void ReshuffleDiscardIntoDraw()
        {
            _drawPile.AddRange(_discardPile);
            _discardPile.Clear();
            Shuffle(_drawPile);
        }

        static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
