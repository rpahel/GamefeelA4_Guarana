using UnityEngine;

namespace Guarana
{
    public class EnemyProj : MonoBehaviour
    {
        #region Fields

        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private BoxCollider2D _collider;
        
        [SerializeField] private float _autoDeleteTimer;

        #endregion

        #region Properties

        public Rigidbody2D Rb => _rb;
    
        public BoxCollider2D Collider => _collider;

        #endregion

        #region Unity Event Functions

        private void Update()
        {
            _autoDeleteTimer -= Time.deltaTime;

            if (_autoDeleteTimer <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            PlayerController pc = null;
            if (col.TryGetComponent(out pc))
                pc.TakeDamage(1);

            Destroy(gameObject);
        }

        #endregion
    }
}
