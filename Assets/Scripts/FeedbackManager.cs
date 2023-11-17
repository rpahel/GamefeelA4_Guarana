using System;
using DG.Tweening;
using Guarana.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
     [SerializeField] private KeyCode _allFeedbacks;
     [SerializeField] private KeyCode _zeroFeedbacks;
     [SerializeField] private KeyCode _reloadScene;
     
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
         
         if (Input.GetKeyDown(_allFeedbacks))
         { 
             EnemyShotBlood = true;
             EnemyShotState = true; 
             WindFootSteps = true; 
             Music = true; 
             Score = true; 
             EnemyCanShoot = true;
             
             for (int i = 0; i < _BGs.Length; i++)
             {
                 _BGs[i].enabled = true;
             }
             
             for (int i = 0; i < _winds.Length; i++)
             {
                 _winds[i].gameObject.SetActive(true);
             }
             
             for (int i = 0; i < _playerFootSteps.Length; i++)
             {
                 _playerFootSteps[i].gameObject.SetActive(true);
             }

             _chromaticAbberation.gameObject.SetActive(true);
             
             if (Ufo._firstHurt)
             {
                 _scoreText.DOColor(new Color(1f, 0f, 0f), 0.5f);
                 _scoreText.font = _creepyFont;  
                 ServiceLocator.Get().ChangeMusic(_creepyMusic);
             }
         }
         if (Input.GetKeyDown(_zeroFeedbacks))
         { 
             EnemyShotBlood = false;
             EnemyShotState = false; 
             WindFootSteps = false; 
             Music = false; 
             Score = false; 
             EnemyCanShoot = false;
             
             for (int i = 0; i < _BGs.Length; i++)
             {
                 _BGs[i].enabled = false;
             }
             
             for (int i = 0; i < _winds.Length; i++)
             {
                 _winds[i].gameObject.SetActive(false);
             }
             
             for (int i = 0; i < _playerFootSteps.Length; i++)
             {
                 _playerFootSteps[i].gameObject.SetActive(false);
             }

             _chromaticAbberation.gameObject.SetActive(false);
             
             _scoreText.DOColor(new Color(1f, 0.5424528f, 0.9089146f), 0.5f);
             _scoreText.font = _cuteFont;
             ServiceLocator.Get().ChangeMusic(_normalMusic);
         }

         if (Input.GetKeyDown(_reloadScene))
         {
             SceneManager.LoadScene("FinalScene");
         }
         
     }
 
     private void Start()
     {
         StartAnimation = _startAnimation;
     }
 }   
}