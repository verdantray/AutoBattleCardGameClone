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
        
        public List<int> DrawCardIndexes => _drawCardIndexes;
        private readonly List<int> _drawCardIndexes = new();
        
        private List<int> _drawCards = new();

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            UIManager.Instance.DEBUGLayoutInGame.OnFinishRecruitLevelAmount += OnFinishRecruitLevelAmount;
        }
        
        public void OnStartRecruitLevelAmount(IReadOnlyList<Tuple<LevelType, int>> pair)
        {
            _isRecruitLevelAmountFinished = false;
            UIManager.Instance.DEBUGLayoutInGame.OnStartRecruitLevelAmount(pair);
        }
        public void OnFinishRecruitLevelAmount(int index)
        {
            _recruitLevelAmountIndex = index;
            _isRecruitLevelAmountFinished = true;
        }

        public void OnStartDrawCard()
        {
            
        }
    }
}