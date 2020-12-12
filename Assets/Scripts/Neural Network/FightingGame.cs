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
                if (_player1.Health != 0) _player1.AddReward(1.0f -_gameTime / _gameTimeThreshold);
                if (_player2.Health != 0) _player2.AddReward(1.0f -_gameTime / _gameTimeThreshold);
                _player1.AddReward((1 - _player2.Health) * _player1.Health * 20.0f);
                _player2.AddReward((1 - _player1.Health) * _player2.Health * 20.0f);
                _gameTime = -99999999.0f;
                StopFight();
            }
        }

        private void StopFight()
        {
            _player1.EndGame();
            _player2.EndGame();
        }

        public void ResetTime()
        {
            _gameTime = 0.0f;
        }
    }
}
