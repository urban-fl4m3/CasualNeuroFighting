using UnityEngine;

namespace Game
{
    public class ActionController : MonoBehaviour
    {
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _jumpForceMultiplier;
        
        private Rigidbody2D _rb;
        
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void ActionMoveLeft()
        {
            var movePosition = new Vector3(-1 * _movementSpeed * Time.fixedDeltaTime, 0, 0);
            _rb.AddForce(movePosition);
        }

        public void ActionMoveRight()
        {
            var movePosition = new Vector3(_movementSpeed * Time.fixedDeltaTime, 0, 0);
            _rb.AddForce(movePosition);
        }

        public void ActionAttack()
        {
            
        }

        public void ActionJump()
        {
            var jumpForce = new Vector3(0, 1 * Time.fixedDeltaTime * _jumpForceMultiplier, 0);
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
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ActionJump();    
            }
        }
    }
}
