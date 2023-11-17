using Guarana;
using System.Collections;
using System.Collections.Generic;
using Guarana.Interfaces;
using Unity.VisualScripting;
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

    [Header("Particles")]
    [SerializeField] private ParticleSystem[] _bloodParticles;
    [SerializeField] private ParticleSystem _firstBloodParticles;

    [Header("SFX")]
    [SerializeField] private AudioClip _deathSfx;

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
        _spriteGameObject.SetActive(false);

        GetComponent<Collider2D>().enabled = false;

        ServiceLocator.Get().PlaySound(_deathSfx);

        for (int i = 0; i < _bloodParticles.Length; i++)
        {
            _bloodParticles[i].Play();
        }

        IsDead = true;
        UfoManager.UfoHasDied(this);
    }

    private void Hurt()
    {
        _firstBloodParticles.Play();
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
