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

    public UnityEvent[] OnStart;
    public UnityEvent[] OnShoot;
    public UnityEvent[] OnHurt;
    public UnityEvent[] OnDeath;

    //==== Hidden fields ====
    private int _currentHp = 0;
    public UfoManager UfoManager { get; set; }

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
    }

    //==== Private methods ====
    private void Die()
    {
        _currentHp = 0;
        gameObject.SetActive(false);
        UfoManager.UfoHasDied();
    }

    private void Hurt()
    {

    }
}
