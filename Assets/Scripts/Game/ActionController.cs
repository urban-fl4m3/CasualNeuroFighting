using System;
using Neural_Network;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Game
{
    public class ActionController : Agent
    {
        [SerializeField] private FightingGame _fightingGame;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _jumpForceMultiplier;
        [SerializeField] private GameObject _attackSprite;
        [SerializeField] private SpriteRenderer _hpVisualizer;
        [SerializeField] private float _attackSpeed = 0.2f;
        [SerializeField] private ActionController _opponent;

        [SerializeField] private Transform _wallLeft;
        [SerializeField] private Transform _wallRight;
        [SerializeField] private Transform _wallBot;
        [SerializeField] private Transform _wallTop;
        [SerializeField] private Transform _startPosition;

        public float Health { get; private set; }

        private bool _bCanJump;
        private bool _bCanAttack;
        private Vector3 _jumpDirection;
        private float _lastAttackTime;
        private Color _defaultColor;

        private Rigidbody2D _rigidbody;
        
        private Vector3 _wallBotPosition => _wallBot.position;
        private Vector3 _wallTopPosition => _wallTop.position;
        private Vector3 _wallLeftPosition => _wallLeft.position;
        private Vector3 _wallRightPosition => _wallRight.position;

        private float _damageTaken;
        private int _direction;
        
        public override void Initialize()
        {
            _jumpDirection = new Vector3(0.0f, 1.0f, 0.0f);
            _defaultColor = _hpVisualizer.color;
            _rigidbody = GetComponent<Rigidbody2D>();
            Reset();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_rigidbody.velocity);
            sensor.AddObservation(_opponent._rigidbody.velocity);

            var playerPosition = transform.position;
            var enemyPosition = _opponent.transform.position;

            var enemyDistanceToLeftWall = enemyPosition.x - _wallLeftPosition.x;
            var enemyDistanceToFloor = enemyPosition.y - _wallBotPosition.y;

            sensor.AddObservation(enemyDistanceToLeftWall / (_wallRightPosition.x - _wallLeftPosition.x));
            sensor.AddObservation(enemyDistanceToFloor / (_wallTopPosition.y - _wallBotPosition.y));

            var playerDistanceToLeftWall = playerPosition.x - _wallLeftPosition.x;
            var playerDistanceToFloor = playerPosition.y - _wallBotPosition.y;

            sensor.AddObservation(playerDistanceToLeftWall / (_wallRight.position.x - _wallLeft.position.x));
            sensor.AddObservation(playerDistanceToFloor / (_wallTop.position.y - _wallBot.position.y));

            var playerCanAttack = Convert.ToInt32(_bCanAttack);
            var playerCanJump = Convert.ToInt32(_bCanJump);
            var opponentCanAttack = Convert.ToInt32(_opponent._bCanAttack);

            sensor.AddObservation(playerCanAttack);
            sensor.AddObservation(playerCanJump);
            sensor.AddObservation(opponentCanAttack);

            var scale = transform.localScale.x;
            var direction = scale < 0.0f ? -1 : 1;

            sensor.AddObservation(direction);
        }

        public override void OnActionReceived(float[] vectorAction)
        {
            var movementLeft = vectorAction[0];
            var movementRight = vectorAction[1];
            var jump = vectorAction[2];
            var attack = vectorAction[3];
            
            SetReward(-_damageTaken);
            _opponent.AddReward(_damageTaken);

            if (movementLeft >= 0.5f) ActionMoveLeft();
            if (movementRight >= 0.5f) ActionMoveRight();
            
            if (jump >= 0.5f) ActionJump();
            if (attack >= 0.5f) ActionAttack();
        }

        public void EndGame()
        {
            EndEpisode();
        }

        public override void OnEpisodeBegin()
        {
            Reset();
        }

        private void ActionMoveLeft()
        {
            ChangeDirection(-1);
            var movePosition = new Vector3(-1 * _movementSpeed * Time.fixedDeltaTime, 0, 0);
            _rigidbody.AddForce(movePosition);

            if (transform.position.x > _opponent.transform.position.x)
            {
                AddReward(0.001f);
            }
        }

        private void ActionMoveRight()
        {
            ChangeDirection(1);
            var movePosition = new Vector3(_movementSpeed * Time.fixedDeltaTime, 0, 0);
            _rigidbody.AddForce(movePosition);
            
            if (transform.position.x < _opponent.transform.position.x)
            {
                AddReward(0.001f);
            }
        }

        private void ChangeDirection(int direction)
        {
            var currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * direction;
            transform.localScale = currentScale;
            _direction = direction;
        }

        private void ActionAttack()
        {
            if (_bCanAttack) 
            {
                _bCanAttack = false;
                _attackSprite.SetActive(true);
                _lastAttackTime = Time.time;
            }
        }

        private void ActionJump()
        {
            if (_bCanJump)
            {
                _bCanJump = false;
                var jumpForce = _jumpDirection * (Time.fixedDeltaTime * _jumpForceMultiplier);
                _rigidbody.AddForce(jumpForce, ForceMode2D.Impulse);
            }
        }

        public void TakeDamage()
        {
            _bCanJump = true;
            Health -= 0.1f;
            Health = Mathf.Max(0.0f, Health);
            _damageTaken += 0.1f;
        }

        private void Update()
        {
            if (Time.time - _lastAttackTime > (_attackSpeed / 20.0))
            {
                _attackSprite.SetActive(false);
            }

            if (Time.time - _lastAttackTime > _attackSpeed)
            {
                _bCanAttack = true;
            }

            UpdateHealthBox();
        }
        
        private void UpdateHealthBox()
        {
            var newColor = _defaultColor * Health;
            newColor.a = 1.0f;
            _hpVisualizer.color = newColor;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            _bCanJump = true;
            var _normal = new Vector2(0.0f, 4.0f);

            foreach (var contactPoint in other.contacts)
            {
                _normal = (_normal + contactPoint.normal) / 2.0f;
            }

            _jumpDirection = new Vector3(_normal.x, _normal.y, 0.0f).normalized;
        }
        
        private void Reset()
        {
            transform.position = _startPosition.position;

            Health = 1.0f;
            _rigidbody.velocity = new Vector2(0.0f, 0.0f);
            _lastAttackTime = -0.2f;
            _bCanJump = true;
            _bCanAttack = true;

            if (_fightingGame != null)
            {
                _fightingGame.ResetTime();
            }
        }
    }
}
