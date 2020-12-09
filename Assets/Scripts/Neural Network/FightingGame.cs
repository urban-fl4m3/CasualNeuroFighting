using System;
using System.Collections.Generic;
using Neural_Network.Controllers;
using Neural_Network.NeuralNetwork;
using UnityEngine;

namespace Neural_Network
{
    public class FightingGame : MonoBehaviour
    {
        public int ID { get; private set; }
        public bool IsPlaying { get; private set; }

        public Transform CameraPlace => _cameraPlace;
        [SerializeField] private Transform _cameraPlace;
        [SerializeField] private Transform _firstPlace;
        [SerializeField] private Transform _secondPlace;

        [SerializeField] private PlayerController _player1;
        [SerializeField] private PlayerController _player2;

        public float GameTime { get; private set; }
        public Vector2 FitnessValue;

        private Action<FightingGame> _onStop;

        public Tuple<Brain, float> playedBrain1;
        public Tuple<Brain, float> playedBrain2;

        public void Initialize(Action<FightingGame> onStop)
        {
            _onStop = onStop;

            MovePermission(false);
        }

        public void StartGame(Tuple<Brain, float> player1Brain, Tuple<Brain, float> player2Brain)
        {
            IsPlaying = true;
            GameTime = Time.time;
            FitnessValue = new Vector2(0.0f, 0.0f);
            
            playedBrain1 = player1Brain;
            playedBrain2 = player2Brain;
            
            _player1.InitializePlayer(player1Brain.Item1, _player2);
            _player2.InitializePlayer(player2Brain.Item1, _player1);
            MovePermission(true);
            ResetAll();
        }

        public void StopGame()
        {
            IsPlaying = false;
            MovePermission(false);
            GameTime = Time.time - GameTime;

            FitnessValue.x = _player1.CurrentHealth * 3.0f + (1.0f - _player2.CurrentHealth) * 6.0f + _player1.attackCounts * 0.0001f;
            if (_player2.CurrentHealth <= 0.0f) FitnessValue.x += 15.0f;
            if (_player1.CurrentHealth <= 0.0f) FitnessValue.x -= 15.0f;
            
            FitnessValue.y = _player2.CurrentHealth * 3.0f - (1.0f - _player1.CurrentHealth) * 6.0f + _player2.attackCounts * 0.0001f;
            if (_player1.CurrentHealth <= 0.0f) FitnessValue.y += 15.0f;
            if (_player2.CurrentHealth <= 0.0f) FitnessValue.y -= 15.0f;
            FitnessValue.x += 5000;
            FitnessValue.y += 5000;

            playedBrain1 = new Tuple<Brain, float>(playedBrain1.Item1, FitnessValue.x);
            playedBrain2 = new Tuple<Brain, float>(playedBrain2.Item1, FitnessValue.y);
            
            _player1.Initialized = false;

            ResetAll();
            _onStop?.Invoke(this);
        }

        private void Update()
        {
            if (!IsPlaying) return;
            if (Time.time - GameTime > 10.0f) StopGame();
            if (_player1.CurrentHealth <= 0 || _player2.CurrentHealth <= 0) StopGame();
        }

        private void ResetAll(Vector3? playerPos1 = null, Vector3? playerPos2 = null)
        {
            _player1.Reset();
            _player2.Reset();

            var newPlayer1Pos = _firstPlace.position;
            var newPlayer2Pos = _secondPlace.position;
            
            _player1.transform.position = newPlayer1Pos;
            _player2.transform.position = newPlayer2Pos;
        }

        private void MovePermission(bool p)
        {
            _player1.Stop = !p;
            _player2.Stop = !p;
        }
    }
}
