using System;
using UnityEngine;

namespace Game
{
    public class ActionController : MonoBehaviour
    {
        private enum _Direction
        {
            Left = -1,
            Right = 1
        }
        
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _jumpForceMultiplier;
        [SerializeField] private GameObject _attackSprite;
        private bool _bCanJump = true;
        private bool _bCanAttack = true;
        private Vector3 _jumpDirection = new Vector3(0.0f, 1.0f, 0.0f);
        private float lastAttackTime = -0.2f;
        
        
        private Rigidbody2D _rb;

        private void changeDirection(_Direction direction)
        {
            var currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * (int)direction;
            transform.localScale = currentScale;
        }
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void ActionMoveLeft()
        {
            changeDirection(_Direction.Left);
            var movePosition = new Vector3(-1 * _movementSpeed * Time.fixedDeltaTime, 0, 0);
            _rb.AddForce(movePosition);
        }

        public void ActionMoveRight()
        {
            changeDirection(_Direction.Right);
            var movePosition = new Vector3(_movementSpeed * Time.fixedDeltaTime, 0, 0);
            _rb.AddForce(movePosition);
        }

        public void ActionAttack()
        {
            if (!_bCanAttack) return;
            _bCanAttack = false;
            _attackSprite.SetActive(true);
            lastAttackTime = Time.time;
        }

        public void ActionJump()
        {
            if (!_bCanJump) return;
            _bCanJump = false;
            var jumpForce = _jumpDirection * (Time.fixedDeltaTime * _jumpForceMultiplier);// new Vector3(0, 1 * Time.fixedDeltaTime * _jumpForceMultiplier, 0);
            _rb.AddForce(jumpForce, ForceMode2D.Impulse);
        }

        private void FixedUpdate()
        {
            if (Input.GetKey(KeyCode.A))
            {
                ActionMoveLeft();
            }
            
            if (Input.GetKey(KeyCode.D))
            {
                ActionMoveRight();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ActionJump();    
            }

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ActionAttack();
            }

            if (Time.time - lastAttackTime > 0.2f)
            {
                _bCanAttack = true;
                _attackSprite.SetActive(false);
            }
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
            
            Debug.Log(_jumpDirection);
        }
    }
    
}
