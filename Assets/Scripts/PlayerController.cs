using System;
using Guarana.Interfaces;
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
        [SerializeField] private AudioClip _shootSfx;
        [SerializeField] private AudioClip _gameOverSfx;
        [SerializeField] private ParticleSystem _gameOverVfx;

        [SerializeField] private Shake _shake;
        [SerializeField] private StartManager _startManager;
        [SerializeField] private Sprite _gunSprite;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Rigidbody2D _rb;
        private float _yPos;

        private float _shootCooldownTimer;
        private bool _tryToShoot;
        private bool _firstShoot;

        #endregion

        #region Events

        public static Action playerShot;

        public static Action playerMove;

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
            
            playerMove?.Invoke();
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
                var position = new Vector2(transform.position.x + 0.3f, transform.position.y + _projectile.Collider.size.y / 2);
                var projectile = Instantiate(_projectile, position, Quaternion.identity);
                projectile.Rb.velocity = new Vector2(0f, _projectileSpeed);

                if (!_firstShoot)
                {
                    _firstShoot = true;
                    _spriteRenderer.sprite = _gunSprite;
                }

                _shootCooldownTimer = _shootCooldown;
                
                ServiceLocator.Get().PlaySound(_shootSfx);

                playerShot?.Invoke();
            }

            if (_shootCooldownTimer > 0)
            {
                _shootCooldownTimer -= Time.deltaTime;
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Enemy")) return;

            col.transform.position = new Vector3(transform.position.x, transform.position.y + 0.2f, 1);
            
            ServiceLocator.Get().PlaySound(_gameOverSfx);
            _gameOverVfx.Play();
            _shake.StartShake();
            
            _startManager.StopGame();
        }

        #endregion
    }
}
