using DG.Tweening;
using Guarana;
using Guarana.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Ufo : MonoBehaviour
{
    //==== Exposed fields ====
    [SerializeField]
    private int _hp = 2;

    [SerializeField]
    private GameObject _spriteGameObject;
    [SerializeField]
    private float _repeatRate;
    [SerializeField]
    private GameObject _projectile;
    [SerializeField]
    private float _bulletspeed;
    [SerializeField]
    private int _randomRange;
    
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer _faceSpriteRenderer;
    [SerializeField] private SpriteRenderer _leftEarSpriteRenderer;
    [SerializeField] private SpriteRenderer _rightEarSpriteRenderer;
    [SerializeField] private Sprite _idleHurt;
    [SerializeField] private Sprite _attack;
    [SerializeField] private Sprite _attackHurt;
    [SerializeField] private Animator _animator;

    [Header("Particles")]
    [SerializeField] private ParticleSystem[] _bloodParticles;
    [SerializeField] private ParticleSystem _firstBloodParticles;

    [Header("SFX")]
    [SerializeField] private AudioClip _deathSfx;
    [SerializeField] private AudioClip _criStart;
    public static TextMeshProUGUI _score;
    public static GameObject _chromaticAbberation;
    [SerializeField] private TMP_FontAsset _font;
    [SerializeField] private AudioClip _creepyMusic;

    [SerializeField]
    private UnityEvent[] OnStart;
    [SerializeField]
    private UnityEvent[] OnShoot;
    [SerializeField]
    private UnityEvent[] OnHurt;
    [SerializeField]
    private UnityEvent[] OnDeath;

    //==== Hidden fields ====
    private int _currentHp = 0;
    public UfoManager UfoManager { get; set; }
    public bool IsDead { get; private set; }
    
    private bool _hasAttacked;
    private bool _isHurt;

    public static bool _firstHurt;

    //==== Public methods ====
    public void TakeDamage(int damage)
    {
        if (damage >= _currentHp)
        {
            Die();
            return;
        }

        _currentHp -= damage;

        Hurt();
    }

    //==== Unity methods ====
    private void Awake()
    {
        _currentHp = _hp;
        IsDead = false;
    }

    private void Start()
    {
        InvokeRepeating("Shoot", 0, _repeatRate);
    }

    //==== Private methods ====
    private void Die()
    {
        _currentHp = 0;
        
        if (FeedbackManager.EnemyShotBlood)
        {
            GetComponent<Collider2D>().enabled = false;
            _faceSpriteRenderer.color = Color.clear;
            _rightEarSpriteRenderer.color = Color.clear;
            _leftEarSpriteRenderer.color = Color.clear;
        
            _animator.SetTrigger("death");

            ServiceLocator.Get().PlaySound(_deathSfx);

            for (int i = 0; i < _bloodParticles.Length; i++)
            {
                _bloodParticles[i].Play();
            }
        }
        else
        {
            GetComponent<Collider2D>().enabled = false;
            _faceSpriteRenderer.color = Color.clear;
            _rightEarSpriteRenderer.color = Color.clear;
            _leftEarSpriteRenderer.color = Color.clear;
        }

        IsDead = true;
        UfoManager.UfoHasDied(this);
    }

    private void Hurt()
    {
        if (FeedbackManager.EnemyShotBlood)
        {
            _firstBloodParticles.Play();
        }
        
        _isHurt = true;
        
        if (!_firstHurt)
        {
            _firstHurt = true;

            if (FeedbackManager.Score)
            {
                _score.DOColor(new Color(1f, 0f, 0f), 0.5f);
                _score.font = _font;
            }

            if (FeedbackManager.Music)
            {
                ServiceLocator.Get().PlaySound(_criStart);
                ServiceLocator.Get().ChangeMusic(_creepyMusic);
            }

            if (FeedbackManager.WindFootSteps)
            {
                _chromaticAbberation.SetActive(true);
            }
        }

        if (FeedbackManager.EnemyShotState)
        {
            if (_hasAttacked)
            {
                _faceSpriteRenderer.sprite = _attackHurt;
                return;
            }
            _faceSpriteRenderer.sprite = _idleHurt;
        }
    }

    private void Shoot()
    {
        if (IsDead)
            return;

        if (Random.Range(0, _randomRange) != 0)
        {
            return;
        }


        GameObject proj = Instantiate(_projectile, transform.position, Quaternion.identity);

        proj.GetComponent<Rigidbody2D>().velocity = Vector3.down * _bulletspeed;
    }
}
