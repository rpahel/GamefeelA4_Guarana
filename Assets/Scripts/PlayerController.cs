using UnityEngine;

namespace Guarana
{
    public class PlayerController : MonoBehaviour
    {
        #region Fields

        [SerializeField] private float _moveSpeed;
        
        [Header("Projectile")]
        [SerializeField] private Projectile _projectile;
        [SerializeField] private float _projectileSpeed;
        [SerializeField] private float _shootCooldown;

        [Header("SFX")] 
        [SerializeField] private AudioClip[] _shootSfxs;

        private Rigidbody2D _rb;
        private float _yPos;

        private float _shootCooldownTimer;
        private bool _tryToShoot;

        #endregion

        #region Unity Event Functions

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _yPos = transform.position.y;
        }

        private void FixedUpdate()
        {
            if (Mathf.Abs(Input.GetAxis("Horizontal")) <= 0.1f) return;

            var screenPos = Camera.main.WorldToScreenPoint(
                new Vector2(transform.position.x + Input.GetAxis("Horizontal") * _moveSpeed * Time.deltaTime, _yPos));
            
            var xPos = Mathf.Clamp(screenPos.x, 0, Screen.width);

            var newScreenPos = new Vector2(xPos, screenPos.y);
            var newWorldPos = Camera.main.ScreenToWorldPoint(newScreenPos);
            
            _rb.MovePosition(newWorldPos);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _tryToShoot = true;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                _tryToShoot = false;
            }
            
            if (_tryToShoot && _shootCooldownTimer <= 0)
            {
                var position = new Vector2(transform.position.x, transform.position.y + _projectile.Collider.size.y / 2);
                var projectile = Instantiate(_projectile, position, Quaternion.identity);
                projectile.Rb.velocity = new Vector2(0f, _projectileSpeed);

                _shootCooldownTimer = _shootCooldown;
            }

            if (_shootCooldownTimer > 0)
            {
                _shootCooldownTimer -= Time.deltaTime;
            }
        }

        #endregion
    }
}
