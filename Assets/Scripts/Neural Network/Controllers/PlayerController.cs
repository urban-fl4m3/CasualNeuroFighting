using System;
using Game;
using Neural_Network.NeuralNetwork;
using UnityEngine;

namespace Neural_Network.Controllers
{
    public class PlayerController : ActionController
    {
        private const float _verySmallValue = 0.001f;
        
        private Brain _currentBrain;
        private ActionController _opponent;
        private float[] _tempValues;

        public bool Stop { get; set; }
        public bool Initialized { private get; set; }

        [SerializeField] private Transform _wallLeft;
        [SerializeField] private Transform _wallRight;
        [SerializeField] private Transform _wallBot;

        public void InitializePlayer(Brain brain, ActionController opponent)
        {
            _opponent = opponent;
            _currentBrain = brain;
            _tempValues = new float[brain[0]];

            Initialized = true;
        }

        protected override void onUpdate()
        {
            
            if (!Initialized || Stop) return;

            NeuronsMove();
        }

        public void Reset()
        {
            attackCounts = 0.0f;
            _hp = 1.0f;
            Rigidbody.velocity = new Vector2(0.0f, 0.0f);
            _bCanJump = true;
            _bCanAttack = true;
        }

        private void NeuronsMove()
        {
            var playerVelocity = Rigidbody.velocity * _verySmallValue;
            _tempValues[0] = playerVelocity.x;
            _tempValues[1] = playerVelocity.y;

            var enemyVelocity = _opponent.Rigidbody.velocity * _verySmallValue;
            _tempValues[2] = enemyVelocity.x;
            _tempValues[3] = enemyVelocity.y;

            var playerPosition = transform.position * _verySmallValue;
            _tempValues[4] = playerPosition.x;
            _tempValues[5] = playerPosition.z;

            var enemyPosition = _opponent.transform.position * _verySmallValue;
            _tempValues[6] = enemyPosition.x;
            _tempValues[7] = enemyPosition.z;

            var playerHealth = CurrentHealth;
            var opponentHealth = _opponent.CurrentHealth;
            _tempValues[8] = playerHealth;
            _tempValues[9] = opponentHealth;

            var leftDistance = playerPosition.x - _wallLeft.position.x;
            var rightDistance = _wallRight.position.x - playerPosition.x;
            var botDistance = playerPosition.y - _wallBot.position.y;
            _tempValues[10] = leftDistance;
            _tempValues[11] = rightDistance;
            _tempValues[12] = botDistance;

            var playerCanAttack = Convert.ToInt32(CanAttack);
            var playerCanJump = Convert.ToInt32(CanJump);
            _tempValues[13] = playerCanAttack;
            _tempValues[14] = playerCanJump;
            
            var opponentCanJump = Convert.ToInt32(_opponent.CanJump);
            _tempValues[15] = opponentCanJump;

            var result = _currentBrain.Process(_tempValues);
            var direction = result[0];
            var jump = result[1];
            var attack = result[2];

            if (direction < 0.5f) ActionMoveLeft();
            else ActionMoveRight();
            
            if (jump >= 0.5f) ActionJump();
            if (attack >= 0.5f) ActionAttack();
        }
    }
}
