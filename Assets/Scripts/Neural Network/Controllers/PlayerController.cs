using System;
using Game;
using Neural_Network.NeuralNetwork;
using UnityEngine;

namespace Neural_Network.Controllers
{
    public class PlayerController : ActionController
    {
        private const float _verySmallValue = 0.01f;
        
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
            var playerVelocity = Rigidbody.velocity;
            _tempValues[0] = playerVelocity.x;
            _tempValues[1] = playerVelocity.y;

            var enemyVelocity = _opponent.Rigidbody.velocity ;
            _tempValues[2] = enemyVelocity.x;
            _tempValues[3] = enemyVelocity.y;

            var playerPosition = (transform.position - _opponent.transform.position) ;

            var enemyPosition = (_opponent.transform.position - transform.position);
            _tempValues[4] = enemyPosition.x;
            _tempValues[5] = enemyPosition.y;

            var playerHealth = CurrentHealth;
            var opponentHealth = _opponent.CurrentHealth;
            _tempValues[6] = playerHealth;
            _tempValues[7] = opponentHealth;

            var leftDistance = (playerPosition.x - _wallLeft.position.x) ;
            var rightDistance = (_wallRight.position.x - playerPosition.x)  ;
            var botDistance = (playerPosition.y - _wallBot.position.y) ;
            _tempValues[8] = leftDistance;
            _tempValues[9] = rightDistance;
            _tempValues[10] = botDistance;

            var playerCanAttack = Convert.ToInt32(CanAttack);
            var playerCanJump = Convert.ToInt32(CanJump);
            _tempValues[11] = playerCanAttack;
            _tempValues[12] = playerCanJump;
            
            var opponentCanJump = Convert.ToInt32(_opponent.CanJump);
            _tempValues[13] = opponentCanJump;
            
            var opponentCanAttack = Convert.ToInt32(_opponent.CanAttack);
            _tempValues[14] = opponentCanAttack;

            var scale = transform.localScale.x;
            var oponentScale = _opponent.transform.localScale.x;

            scale = scale < 0.0f ? -1 : 1;
            oponentScale = oponentScale < 0.0f ? -1 : 1;

            _tempValues[15] = scale;
            _tempValues[16] = oponentScale;

            var result = _currentBrain.Process(_tempValues);
            var left = result[0];
            var right = result[1];
            var jump = result[2];
            var attack = result[3];

            if (left >= 0.5f) ActionMoveLeft();
            if (right >= 0.5f) ActionMoveRight();
            
            if (jump >= 0.5f) ActionJump();
            if (attack >= 0.5f) ActionAttack();
        }
    }
}
