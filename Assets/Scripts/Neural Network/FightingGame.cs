using System;
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

        public Transform LeftClamp;
        public Transform BotClamp;

        public float GameTime { get; private set; }

        private Action<FightingGame> _onStop; 

        public void Initialize(int id, Action<FightingGame> onStop)
        {
            ID = id;
            _onStop = onStop;
            
            MovePermission(false);
        }

        public void StartGame(Brain player1Brain, Brain player2Brain, Vector3 playerPosition, Vector3 devourerPosition)
        {
            IsPlaying = true;
            GameTime = Time.time;
            _player1.InitializePlayer(player1Brain, _player2);
            _player2.InitializePlayer(player2Brain, _player1);
            MovePermission(true);
            ResetAll(playerPosition, devourerPosition);
        }

        public void StopGame()
        {
            IsPlaying = false;
            MovePermission(false);
            GameTime = Time.time - GameTime;

            _player1.Initialized = false;

            ResetAll();
            _onStop?.Invoke(this);
        }

        private void ResetAll(Vector3? playerPos1 = null, Vector3? playerPos2 = null)
        {
            _player1.Reset();
            _player2.Reset();

            var newPlayer1Pos = _firstPlace.position;
            if (playerPos1 != null)
            {
                newPlayer1Pos = playerPos1.Value;
                newPlayer1Pos.x += LeftClamp.position.x;
                newPlayer1Pos.z += BotClamp.position.z;
         
            }
            _player1.transform.position = newPlayer1Pos;

            var newPlayer2Pos = _secondPlace.position;
            if (playerPos2 != null)
            {
                newPlayer2Pos = playerPos2.Value;
                newPlayer2Pos.x += LeftClamp.position.x;
                newPlayer2Pos.z += BotClamp.position.z;
            }
            _player2.transform.position = newPlayer2Pos;
        }

        private void MovePermission(bool p)
        {
            _player1.Stop = !p;
            _player2.Stop = !p;
        }
    }
}
