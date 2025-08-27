using System;
using System.Collections.Generic;
using ProjectABC.Core;
using ProjectABC.Data;
using UnityEngine;

namespace ProjectABC.InGame
{
    public class InGameController : MonoBehaviour
    {
        public static InGameController Instance;
        
        public bool IsRecruitLevelAmountFinished => _isRecruitLevelAmountFinished;
        private bool _isRecruitLevelAmountFinished = false;
        
        public int RecruitLevelAmountIndex => _recruitLevelAmountIndex;
        private int _recruitLevelAmountIndex = -1;
        
        public bool IsDrawCardFinished => _isDrawCardFinished;
        private bool _isDrawCardFinished = false;
        
        public List<Card> DrawCards => _drawCards;
        private readonly List<Card> _drawCards = new();

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            UIManager.Instance.DEBUGLayoutInGame.OnFinishRecruitLevelAmount += OnFinishRecruitLevelAmount;
            UIManager.Instance.DEBUGLayoutInGame.OnFinishDrawCard += OnFinishDrawCard;
        }
        
        public void OnStartRecruitLevelAmount(IReadOnlyList<Tuple<GradeType, int>> pair)
        {
            _recruitLevelAmountIndex = -1;
            _isRecruitLevelAmountFinished = false;
            UIManager.Instance.DEBUGLayoutInGame.OnStartRecruitLevelAmount(pair);
        }
        public void OnFinishRecruitLevelAmount(int index)
        {
            _recruitLevelAmountIndex = index;
            _isRecruitLevelAmountFinished = true;
        }

        public void OnStartDrawCard(PlayerState state, GradeType gradeType, int amount)
        {
            DrawCards.Clear();
            _isDrawCardFinished = false;
            UIManager.Instance.DEBUGLayoutInGame.OnStartDrawCard(state, gradeType, amount);
        }

        public void OnFinishDrawCard(List<Card> drawCards)
        {
            DrawCards.AddRange(drawCards);
            _isDrawCardFinished = true;
        }
    }
}