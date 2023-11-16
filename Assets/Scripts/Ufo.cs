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

    //==== Private methods ====
    private void Die()
    {
        _currentHp = 0;
        UfoManager.UfoHasDied();

        _spriteGameObject.SetActive(false);
        GetComponent<Collider2D>().enabled = false;

        ServiceLocator.Get().PlaySound(_deathSfx);

        for (int i = 0; i < _bloodParticles.Length; i++)
        {
            _bloodParticles[i].Play();
        }

        IsDead = true;
    }

    private void Hurt()
    {
        _firstBloodParticles.Play();
    }
}
