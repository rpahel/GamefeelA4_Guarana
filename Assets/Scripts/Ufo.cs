using Guarana;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ufo : MonoBehaviour
{
    //==== Exposed fields ====
    [SerializeField]
    private int _hp = 2;

    [SerializeField] private GameObject _spriteGameObject;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _bloodParticles;

    public UnityEvent[] OnStart;
    public UnityEvent[] OnShoot;
    public UnityEvent[] OnHurt;
    public UnityEvent[] OnDeath;

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
        gameObject.SetActive(false);
        UfoManager.UfoHasDied();

        _spriteRenderer.gameObject.SetActive(false);
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

    }
}
