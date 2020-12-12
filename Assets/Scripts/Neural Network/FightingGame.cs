using System;
using Game;
using UnityEngine;

namespace Neural_Network
{
    public class FightingGame : MonoBehaviour
    {
        [SerializeField] private float _gameTimeThreshold;
        [SerializeField] private ActionController _player1;
        [SerializeField] private ActionController _player2;
        
        private float _gameTime;

        private void Update()
        {
            _gameTime += Time.deltaTime;

            if (_gameTime >= _gameTimeThreshold ||
                _player1.Health == 0 ||
                _player2.Health == 0)
            {
                _player1.SetReward(1 - _player2.Health * _player1.Health - _gameTime / _gameTimeThreshold);
                _player2.SetReward(1 - _player1.Health * _player2.Health - _gameTime / _gameTimeThreshold);
                StopFight();
            }
        }

        private void StopFight()
        {
            _player1.EndGame();
            _player2.EndGame();
            ResetTime();
        }

        public void ResetTime()
        {
            _gameTime = 0.0f;
        }
    }
}
