using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectABC.Core;
using ProjectABC.Data;
using UnityEngine;

namespace ProjectABC.Engine
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
        
        public bool IsBattleFinished => _isBattleFinished;
        private bool _isBattleFinished = false;
        
        [SerializeField]
        private GameBoardController _gameBoardController;

        private IPlayer _player;
        private IPlayer _enemy;
        private int _roundCount;

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            UIManagerTemp.Instance.DEBUGLayoutInGame.OnFinishRecruitLevelAmount += OnFinishRecruitLevelAmount;
            UIManagerTemp.Instance.DEBUGLayoutInGame.OnFinishDrawCard += OnFinishDrawCard;
        }
        private void OnDestroy()
        {
            
        }
        
        public void ClearGame()
        {
            _roundCount = 0;
        }

        public void OnStartRecruitLevelAmount(IReadOnlyList<Tuple<GradeType, int>> pair)
        {
            _recruitLevelAmountIndex = -1;
            _isRecruitLevelAmountFinished = false;
            _isBattleFinished = false;
            UIManagerTemp.Instance.DEBUGLayoutInGame.OnStartRecruitLevelAmount(pair);
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
            UIManagerTemp.Instance.DEBUGLayoutInGame.OnStartDrawCard(state, gradeType, amount);
        }

        public void OnFinishDrawCard(List<Card> drawCards)
        {
            DrawCards.AddRange(drawCards);
            _isDrawCardFinished = true;
        }

        public void OnFinishBattle()
        {
            _isBattleFinished = true;
        }

        public void OnEvent()
        {
            
        }
    }
}