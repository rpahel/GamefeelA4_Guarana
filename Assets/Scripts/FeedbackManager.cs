using System;
using DG.Tweening;
using Guarana.Interfaces;
using TMPro;
using UnityEngine;

namespace Guarana
{
 public class FeedbackManager : MonoBehaviour
 {
     #region Fields
 
     [SerializeField] private bool _startAnimation;
     [SerializeField] private KeyCode _enemyShotBlood;
     [SerializeField] private KeyCode _enemyShotState;
     [SerializeField] private KeyCode _windFootSteps;
     [SerializeField] private KeyCode _music;
     [SerializeField] private KeyCode _score;
     [SerializeField] private KeyCode _enemyCanShoot;
     
     [SerializeField] private Background[] _BGs;
     [SerializeField] private ParticleSystem[] _winds;
     [SerializeField] private ParticleSystem[] _playerFootSteps;
     [SerializeField] private TextMeshProUGUI _scoreText;
     [SerializeField] private TMP_FontAsset _cuteFont;
     [SerializeField] private TMP_FontAsset _creepyFont;
     [SerializeField] private AudioClip _normalMusic;
     [SerializeField] private AudioClip _creepyMusic;
     [SerializeField] private GameObject _chromaticAbberation;
 
     public static bool EnemyShotBlood;
     public static bool EnemyShotState;
     public static bool WindFootSteps;
     public static bool Music;
     public static bool Score;
     public static bool EnemyCanShoot;
     public static bool StartAnimation;
     
     #endregion
     
     void Update()
     {
         if (Input.GetKeyDown(_enemyShotBlood))
         {
             EnemyShotBlood = !EnemyShotBlood;
         }
         if (Input.GetKeyDown(_enemyShotState))
         {
             EnemyShotState = !EnemyShotState;
         }
         if (Input.GetKeyDown(_windFootSteps))
         {
             WindFootSteps = !WindFootSteps;
             
             for (int i = 0; i < _BGs.Length; i++)
             {
                 _BGs[i].enabled = WindFootSteps;
             }
             
             for (int i = 0; i < _winds.Length; i++)
             {
                 _winds[i].gameObject.SetActive(WindFootSteps);
             }
             
             for (int i = 0; i < _playerFootSteps.Length; i++)
             {
                 _playerFootSteps[i].gameObject.SetActive(WindFootSteps);
             }

             _chromaticAbberation.gameObject.SetActive(WindFootSteps);
         }
         if (Input.GetKeyDown(_music))
         {
             Music = !Music;

             if (Music && Ufo._firstHurt)
             {
                 ServiceLocator.Get().ChangeMusic(_creepyMusic);
             }
             else
             {
                 ServiceLocator.Get().ChangeMusic(_normalMusic);
             }
         }
         if (Input.GetKeyDown(_score))
         {
             Score = !Score;

             if (Score)
             {
                 if (Ufo._firstHurt)
                 {
                     _scoreText.DOColor(new Color(1f, 0f, 0f), 0.5f);
                     _scoreText.font = _creepyFont;   
                 }
             }
             else
             {
                 _scoreText.DOColor(new Color(1f, 0.5424528f, 0.9089146f), 0.5f);
                 _scoreText.font = _cuteFont;
             }
         }
         if (Input.GetKeyDown(_enemyCanShoot))
         {
             EnemyCanShoot = !EnemyCanShoot;
         }
     }
 
     private void Start()
     {
         StartAnimation = _startAnimation;
     }
 }   
}