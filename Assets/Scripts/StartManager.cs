using System.Collections;
using UnityEngine;

namespace Guarana
{
    public class StartManager : MonoBehaviour
    {
        [SerializeField] private float _timeBeforeStart;
        [SerializeField] private Background[] _BGs;
        [SerializeField] private ParticleSystem[] _winds;
        [SerializeField] private ParticleSystem[] _playerFootSteps;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private Canvas _canvas;

        void Start()
        {
            StartCoroutine(StartGame());
        }

        public void StopGame()
        {
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

            _playerController.enabled = false;
            UfoManager.IsPaused = true;
        }

        private IEnumerator StartGame()
        {
            yield return new WaitForSeconds(_timeBeforeStart);

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

            _playerController.enabled = true;
            _canvas.gameObject.SetActive(true);
        }
    }
}
