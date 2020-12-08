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
        [SerializeField] private SpriteRenderer _hpVisualizer;
        [SerializeField] private float _attackSpeed = 0.2f;

        public float _hp = 1.0f;
        public bool _bCanJump = true;
        public bool _bCanAttack = true;
        private Vector3 _jumpDirection = new Vector3(0.0f, 1.0f, 0.0f);
        private float lastAttackTime = -0.2f;
        private Color _defaultColor;
        
        public float attackCounts = 0.0f;


        public float CurrentHealth => _hp;
        public bool CanJump => _bCanJump;
        public bool CanAttack => _bCanAttack;
        public Rigidbody2D Rigidbody { get; private set; }

        private void changeDirection(_Direction direction)
        {
            var currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * (int)direction;
            transform.localScale = currentScale;
        }

        private void updateHpBox()
        {
            var newColor = _defaultColor * _hp;
            newColor.a = 1.0f;
            _hpVisualizer.color = newColor;
        }
        
        private void Awake()
        {
            _defaultColor = _hpVisualizer.color;
            Rigidbody = GetComponent<Rigidbody2D>();
        }

        protected void ActionMoveLeft()
        {
            changeDirection(_Direction.Left);
            var movePosition = new Vector3(-1 * _movementSpeed * Time.fixedDeltaTime, 0, 0);
            Rigidbody.AddForce(movePosition);
        }

        protected void ActionMoveRight()
        {
            changeDirection(_Direction.Right);
            var movePosition = new Vector3(_movementSpeed * Time.fixedDeltaTime, 0, 0);
            Rigidbody.AddForce(movePosition);
        }

        protected void ActionAttack()
        {
            if (!_bCanAttack) return;
            attackCounts++;
            _bCanAttack = false;
            _attackSprite.SetActive(true);
            lastAttackTime = Time.time;
        }

        protected void ActionJump()
        {
            if (!_bCanJump) return;
            _bCanJump = false;
            var jumpForce = _jumpDirection * (Time.fixedDeltaTime * _jumpForceMultiplier);
            Rigidbody.AddForce(jumpForce, ForceMode2D.Impulse);
        }

        public void TakeDamage()
        {
            _bCanJump = true;
            _hp -= 0.1f;
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
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     ActionJump();    
            // }
            //
            // if (Input.GetKeyDown(KeyCode.Mouse0))
            // {
            //     ActionAttack();
            // }
            
            if (Time.time - lastAttackTime > (_attackSpeed / 4.0))
            {
                _attackSprite.SetActive(false);
            }
            
            
            if (Time.time - lastAttackTime > _attackSpeed)
            {
                _bCanAttack = true;
            }

            updateHpBox();
            onUpdate();
        }

        protected virtual void onUpdate()
        {
            
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
        
    }
    
}
