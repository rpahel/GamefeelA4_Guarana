using System;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Guarana
{
    public class UfoManager : MonoBehaviour
    {
        //==== Exposed Fields ====

        [Header("Set Up")]
        [SerializeField, Tooltip("The zone the UFOs are contained in.")]
        private Transform _ufoZone = null;
        [SerializeField, Tooltip("Size of the zone the UFOs are contained in.")]
        private Vector2 _ufoZoneSize = Vector2.zero;
        [SerializeField, Tooltip("Size of the game screen")]
        private Vector2 _gameArea = Vector2.zero;
        [SerializeField, Tooltip("Size of one UFO.")]
        private Vector2 _ufoSize = Vector2.one;

        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private ParticleSystem[] _scoreParticles;

        [Header("Gameplay values")]
        [SerializeField, Tooltip("UFO GameObject.")]
        private GameObject _ufoGo = null;
        [SerializeField, Tooltip("In seconds, time to wait after Start() for the first UFOs to spawn.")]
        private float _spawnStartDelay = 2f;
        [SerializeField, Tooltip("Easing of the movement of the UFOs between two waypoints.")]
        private Ease _movementEase = Ease.Linear;
        [SerializeField, Tooltip("Easing of the movement when an Ufo dies during movement.")]
        private Ease _movementEaseOnUfoDeath = Ease.Linear;
        [SerializeField, Tooltip("Speed of the Ufos at Start().")]
        private float _initialSpeed = 1f;
        [SerializeField, Tooltip("Amount to speed to add to the UFOs when one is killed.")]
        private float _speedIncrementAmount = 0.1f;
        [SerializeField, Tooltip("Time the Ufos wait before moving again to the next waypoint.")]
        private float _movePauseDuration = 1f;
        
        [Header("Start Animation")]
        [SerializeField] private float _startAnimationTime = 1f;
        [SerializeField] private Ease _startAnimationEase = Ease.Linear;

        [Header("Effects")]
        [SerializeField, Tooltip("Blood splatter to spawn on wall.")]
        private GameObject _bloodSplatter;
        [SerializeField, Range(0f, 0.5f)]
        private float _bloodSplatterSize = 0.5f;
        [SerializeField]
        private UnityEvent[] OnUfoDeath;
        [SerializeField]
        private UnityEvent[] OnUfoHurt;
        [SerializeField]
        private UnityEvent[] OnPlayerDeath;

        //==== Fields ====
        private int[] _nbOfAliveUfos;
        private int _currentWaypointIndex;
        private int _currentColumnsNb;
        private float _currentSpeed;
        private Vector2[,] _ufoTiles;
        private Vector2[] _wayPoints;
        private Vector2 _initialUfoZoneSize;
        private Ufo[,] _ufos;
        private Coroutine _movementCoroutine;
        private Tweener _moveTween;
        private int _score;

        //==== Properties ====
        public static bool IsPaused { get; set; }

        //==== Methods ====
        #region UNITY FUNCTIONS
        private void Awake()
        {
            IsPaused = true;
            _currentSpeed = _initialSpeed;
            _initialUfoZoneSize = _ufoZoneSize;

            BoxCollider2D leftBoxCollider2D = null;
            BoxCollider2D rightBoxCollider2D = null;

            leftBoxCollider2D = gameObject.AddComponent<BoxCollider2D>();
            rightBoxCollider2D = gameObject.AddComponent<BoxCollider2D>();

            leftBoxCollider2D.size = new Vector2(1f, _gameArea.y);
            leftBoxCollider2D.offset = new Vector2((-_gameArea.x - 1f) * .5f, 0);

            rightBoxCollider2D.size = new Vector2(1f, _gameArea.y);
            rightBoxCollider2D.offset = new Vector2((_gameArea.x + 1f) * .5f, 0);
        }

        private IEnumerator Start()
        {
            PinUfoZoneToTopLeft();
            UpdateUfoTiles();
            CalculateWaypoints();
            SpawnUfos();
            HideUfos();

            yield return new WaitForSeconds(_spawnStartDelay);

            ShowUfoRows();

            Ufo._score = _scoreText;

            StartCoroutine(StartAnimationCoroutine());
        }

        private void Update()
        {
            if (IsPaused)
                return;

            MoveTo(_currentWaypointIndex + 1, _movementEase, true);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log(collision.ToString());
            Debug.Log(collision.gameObject.ToString());
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Game area Vertical
            Gizmos.color = Color.green;
            Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + new Vector2(0, _gameArea.y * 0.5f));
            Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(0, _gameArea.y * 0.5f), new Vector2(1, 0.1f));
            Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position - new Vector2(0, _gameArea.y * 0.5f));
            Gizmos.DrawWireCube((Vector2)transform.position - new Vector2(0, _gameArea.y * 0.5f), new Vector2(1, 0.1f));

            // Game area Horizontal
            Gizmos.color = Color.red;
            Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + new Vector2(_gameArea.x * 0.5f, 0));
            Gizmos.DrawWireCube((Vector2)transform.position + new Vector2(_gameArea.x * 0.5f, 0), new Vector2(.1f, 1));
            Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position - new Vector2(_gameArea.x * 0.5f, 0));
            Gizmos.DrawWireCube((Vector2)transform.position - new Vector2(_gameArea.x * 0.5f, 0), new Vector2(.1f, 1));

            // Ufo Zone
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_ufoZone.position, _ufoZoneSize);

            if (UnityEditor.EditorApplication.isPlaying)
            {
                DrawPath();
                return;
            }

            PinUfoZoneToTopLeft();

            UpdateUfoTiles();
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _ufoTiles.GetLength(0); i++)
            {
                for (int j = 0; j < _ufoTiles.GetLength(1); j++)
                {
                    Gizmos.DrawWireCube(_ufoTiles[i, j], _ufoSize);
                }
            }

            CalculateWaypoints();

            Gizmos.color = Color.magenta;
            DrawPath();
        }
#endif
        #endregion

        #region CUSTOM PUBLIC
        public void UfoHasDied(Ufo deadUfo)
        {
            int index = GetColumnIndexOfUfo(deadUfo);

            _nbOfAliveUfos[index]--;

            _score += 100;
            _scoreText.text = _score.ToString();

            for (int i = 0; i < _scoreParticles.Length; i++)
            {
                _scoreParticles[i].Play();
            }

            for (int i = 0; i < _nbOfAliveUfos.Length; i++)
            { 
                if (_nbOfAliveUfos[i] != 0)
                    break;

                if (i == _nbOfAliveUfos.Length - 1)
                {
                    // Win
                    return;
                }
            }
            
            _currentSpeed += _speedIncrementAmount;
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
            _moveTween.Kill(false);

            if (HasLostAnExtremitiesColumn())
            {
                _ufoZoneSize = _initialUfoZoneSize - new Vector2(GetNumberOfExtremitiesColumnsThatDisappeared(), 0);
                CalculateWaypoints();
            }

            MoveTo(_currentWaypointIndex + 1, _movementEaseOnUfoDeath, false);
        }

        #endregion

        #region CUSTOM PRIVATES
        private Vector2Int NumberOfUfosThatCanFit()
        {
            int x = (int)(_ufoZoneSize.x / _ufoSize.x);
            int y = (int)(_ufoZoneSize.y / _ufoSize.y);

            return new Vector2Int(x, y);
        }

        private void UpdateUfoTiles()
        {
            Vector2Int possibleNbOfUfos = NumberOfUfosThatCanFit();

            _ufoTiles ??= new Vector2[possibleNbOfUfos.x, possibleNbOfUfos.y];

            Vector2Int comparator = new(_ufoTiles.GetLength(0), _ufoTiles.GetLength(1));

            if (_ufoTiles == null || comparator != possibleNbOfUfos)
                _ufoTiles = new Vector2[possibleNbOfUfos.x, possibleNbOfUfos.y];

            float leftOverX = _ufoZoneSize.x - possibleNbOfUfos.x * _ufoSize.x;
            float leftOverY = _ufoZoneSize.y - possibleNbOfUfos.y * _ufoSize.y;

            if (leftOverX < 0) leftOverX = 0;
            if (leftOverY < 0) leftOverY = 0;

            float paddingX = leftOverX / (possibleNbOfUfos.x + 1);
            float paddingY = leftOverY / (possibleNbOfUfos.y + 1);

            Vector2 start = (Vector2)_ufoZone.position + .5f * new Vector2(-_ufoZoneSize.x, _ufoZoneSize.y);

            float newX = 0;
            float newY = 0;
            for (int j = 0; j < possibleNbOfUfos.y; j++)
            {
                if (j == 0)
                    newY = start.y - (paddingY + _ufoSize.x * .5f);
                else
                    newY = start.y - (paddingY + _ufoSize.x * .5f) - j * (paddingY + _ufoSize.y);

                for (int i = 0; i < possibleNbOfUfos.x; i++)
                {
                    // ZigZag (Vers la droite puis vers la gauche)
                    if (i == 0)
                    {
                        if (j % 2 == 0)
                            newX = start.x + paddingX + _ufoSize.x * .5f;
                        else
                            newX = start.x + _ufoZoneSize.x - (paddingX + _ufoSize.x * .5f);
                    }
                    else
                    {
                        if (j % 2 == 0)
                            newX = start.x + paddingX + _ufoSize.x * .5f + i * (paddingX + _ufoSize.x);
                        else
                            newX = start.x + _ufoZoneSize.x - (paddingX + _ufoSize.x * .5f) - i * (paddingX + _ufoSize.x);
                    }

                    _ufoTiles[i, j] = new Vector2(newX, newY);
                }
            }
        }

        private void SpawnUfos()
        {
            _ufos = new Ufo[_ufoTiles.GetLength(0), _ufoTiles.GetLength(1)];

            for (int j = 0; j < _ufoTiles.GetLength(1); j++)
            {
                for (int i = 0; i < _ufoTiles.GetLength(0); i++)
                {
                    Ufo ufo = Instantiate(_ufoGo, _ufoTiles[i, j], Quaternion.identity, _ufoZone).GetComponent<Ufo>();
                    ufo.transform.localScale = _ufoSize;
                    ufo.UfoManager = this;

                    _ufos[i, j] = ufo;
                }
            }

            _currentColumnsNb = _ufos.GetLength(0);
            _nbOfAliveUfos = new int[_currentColumnsNb];
            
            for(int i = 0; i < _currentColumnsNb; i++)
            {
                _nbOfAliveUfos[i] = _ufos.GetLength(1);
            }
        }

        private void HideUfos()
        {
            for (int j = 0; j < _ufoTiles.GetLength(1); j++)
            {
                for (int i = 0; i < _ufoTiles.GetLength(0); i++)
                {
                    _ufos[i, j].gameObject.SetActive(false);
                }
            }
        }

        private void ShowUfoRows()
        {
            for (int j = 0; j < _ufoTiles.GetLength(1); j++)
            {
                for (int i = 0; i < _ufoTiles.GetLength(0); i++)
                {
                    _ufos[i, j].gameObject.SetActive(true);
                }
            }
        }

        private void CalculateWaypoints()
        {
            int fitY = (int)(_gameArea.y / _ufoSize.y) * 2; // Max number of waypoints

            if (_wayPoints == null || _wayPoints.Length != fitY)
                _wayPoints = new Vector2[fitY];

            
            int n = 0;
            for (int j = 0; j < fitY * .5f; j++)
            {
                float waypointY = 0;
                
                waypointY = transform.position.y + .5f * (_gameArea.y - _ufoZoneSize.y) - j * _ufoSize.y;

                for (int i = 0; i < 2; i++)
                {
                    float waypointX = 0;
                    
                    if (j % 2 == 0)
                        waypointX = transform.position.x + .5f * (_ufoZoneSize.x - _gameArea.x) + i * (_gameArea.x - _ufoZoneSize.x);
                    else
                        waypointX = transform.position.x - .5f * (_ufoZoneSize.x - _gameArea.x) - i * (_gameArea.x - _ufoZoneSize.x);

                    _wayPoints[n] = new Vector2(waypointX, waypointY);
                    n++;
                }
            }
        }

        private void PinUfoZoneToTopLeft()
        {
            _ufoZone.localPosition = .5f * new Vector2(-_gameArea.x + _ufoZoneSize.x, _gameArea.y - _ufoZoneSize.y);
        }

#if UNITY_EDITOR
        private void DrawPath()
        {
            for (int i = 0; i < _wayPoints.Length - 1; i++)
            {
                Gizmos.DrawWireSphere(_wayPoints[i], .2f);
                Gizmos.DrawLine(_wayPoints[i], _wayPoints[i + 1]);

                if (i == _wayPoints.Length - 2)
                    Gizmos.DrawWireSphere(_wayPoints[i + 1], .2f);
            }
        }
#endif

        private void MoveTo(int wayPointIndex, Ease ease, bool waitPauseDuration)
        {
            if (_movementCoroutine != null)
                return;

            if (wayPointIndex >= _wayPoints.Length)
            {
                IsPaused = true;
                return;
            }

            Vector2 target = _wayPoints[wayPointIndex];
            _movementCoroutine = StartCoroutine(MoveToCoroutine(target, ease, waitPauseDuration));
        }

        private IEnumerator MoveToCoroutine(Vector2 target, Ease ease, bool waitPauseDuration)
        {
            if (waitPauseDuration)
            {
                float wait = _movePauseDuration;

                while (wait > 0)
                {
                    wait -= Time.deltaTime;
                    yield return null; // attends une frame
                }
            }

            Vector2 startPos = _ufoZone.position;
            float duration = (startPos - target).magnitude / _currentSpeed;

            bool completed = false;
            _moveTween = _ufoZone.DOMove(target, duration).SetEase(ease);
            _moveTween.onComplete += () => completed = true;

            yield return _moveTween.WaitForKill();

            if (completed)
                _currentWaypointIndex++;

            _movementCoroutine = null;
            yield break;
        }

        private int GetColumnIndexOfUfo(Ufo ufoThatJustDied)
        {
            for (int j = 0; j < _ufos.GetLength(1); j++)
            {
                for (int i = 0; i < _ufos.GetLength(0); i++)
                {
                    if (_ufos[i, j] == ufoThatJustDied)
                        return j % 2 == 0 ? i : Mathf.Abs(i - (_ufos.GetLength(0) - 1));
                }
            }
            
            return -1;
        }

        private bool HasLostAnExtremitiesColumn()
        {
            for (int i = 0; i < _nbOfAliveUfos.Length; i++)
            {
                if (_nbOfAliveUfos[i] == 0)
                {
                    _nbOfAliveUfos[i] = -1;

                    if ((i - 1) < 0 || (i + 1) >= _nbOfAliveUfos.Length) // Extremité
                        return true;
                        
                    if (_nbOfAliveUfos[i - 1] <= 0 || _nbOfAliveUfos[i + 1] <= 0) // Fausse Extremité
                        return true;
                }
            }

            return false;
        }

        private int GetNumberOfExtremitiesColumnsThatDisappeared()
        {
            int ret = 0;
            // left to right
            for (int i = 0; i < _nbOfAliveUfos.Length; i++)
            {
                if (_nbOfAliveUfos[i] <= 0)
                    ret++;
                else
                    break;
            }

            if (ret >= _nbOfAliveUfos.Length)
                return _nbOfAliveUfos.Length;
            
            // right to left
            for (int i = _nbOfAliveUfos.Length - 1; i >= 0; i--)
            {
                if (_nbOfAliveUfos[i] <= 0)
                    ret++;
                else
                    break;
            }
            
            return ret;
        }

        private IEnumerator StartAnimationCoroutine()
        {
            transform.position = new Vector3(0f, 5f, 0f);
            transform.DOMoveY(0, _startAnimationTime).SetEase(_startAnimationEase);

            yield return new WaitForSeconds(_startAnimationTime);

            IsPaused = false;
        }
    }
        #endregion
}