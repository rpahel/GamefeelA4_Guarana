using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Guarana
{
    public class UfoManager : MonoBehaviour
    {
        //==== Exposed Fields ====
        [Header("Debug")]
        [SerializeField, Tooltip("Colorize enemies so it shows as a gradient from blue to red.")]
        private bool _coloredUfos = false;

        [Header("Set Up")]
        [SerializeField, Tooltip("The zone the UFOs are contained in.")]
        private Transform _ufoZone = null;
        [SerializeField, Tooltip("Size of the zone the UFOs are contained in.")]
        private Vector2 _ufoZoneSize = Vector2.zero;
        [SerializeField, Tooltip("Size of the game screen")]
        private Vector2 _gameArea = Vector2.zero;
        [SerializeField, Tooltip("Size of one UFO.")]
        private Vector2 _ufoSize = Vector2.one;

        [Header("Gameplay values")]
        [SerializeField, Tooltip("UFO GameObject.")]
        private GameObject _ufoGo = null;
        [SerializeField, Tooltip("Speed of the Ufos at Start().")]
        private Vector2 _initialSpeed = Vector2.one;
        [SerializeField, Tooltip("In seconds, time to wait after Start() for the first UFOs to spawn.")]
        private float _spawnStartDelay = 2f;

        //==== Fields ====
        private Vector2[,] _ufoTiles = null;
        private Vector2[] _wayPoints = null;
        private GameObject[] _ufos = null;

        //==== Properties ====
        public static bool IsPaused { get; set;  }

        //==== Methods ====
        #region UNITY FUNCTIONS
        private IEnumerator Start()
        {
            IsPaused = true;

            PinUfoZoneToTopLeft();
            UpdateUfoTiles();
            CalculateWaypoints();
            SpawnUfos();
            HideUfos();

            yield return new WaitForSeconds(_spawnStartDelay);

            ShowUfoRows();

            IsPaused = false;
        }

        private void Update()
        {
            if(IsPaused)
                return;

            //MoveUfos();
        }

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
                return;

            PinUfoZoneToTopLeft();

            UpdateUfoTiles();
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _ufoTiles.GetLength(0); i++)
            {
                for(int j = 0; j < _ufoTiles.GetLength(1); j++)
                {
                    Gizmos.DrawWireCube(_ufoTiles[i, j], _ufoSize);
                }
            }

            CalculateWaypoints();

            Gizmos.color = Color.magenta;
            DrawPath();
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

            if(leftOverX < 0) leftOverX = 0;
            if(leftOverY < 0) leftOverY = 0;

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
                    if(i == 0)
                    {
                        if(j % 2 == 0)
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
            int arraySize = (_ufoTiles.GetLength(0) * _ufoTiles.GetLength(1));
            _ufos = new GameObject[arraySize];

            int a = 0;
            for (int j = 0; j < _ufoTiles.GetLength(1); j++)
            {
                for (int i = 0; i < _ufoTiles.GetLength(0); i++)
                {
                    GameObject obj = Instantiate(_ufoGo, _ufoTiles[i, j], Quaternion.identity, _ufoZone);
                    obj.transform.localScale = _ufoSize;
                    
                    // Juste pour test
                    if(_coloredUfos)
                        obj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.blue, Color.red, (float)a / arraySize);

                    _ufos[a] = obj;
                    a++;
                }
            }
        }

        private void HideUfos()
        {
            int a = 0;
            for (int j = 0; j < _ufoTiles.GetLength(1); j++)
            {
                for (int i = 0; i < _ufoTiles.GetLength(0); i++)
                {
                    _ufos[a].SetActive(false);
                    a++;
                }
            }
        }

        private void ShowUfoRows()
        {
            int a = 0;
            for (int j = 0; j < _ufoTiles.GetLength(1); j++)
            {
                for (int i = 0; i < _ufoTiles.GetLength(0); i++)
                {
                    _ufos[a].SetActive(true);
                    a++;
                }
            }
        }

        private void CalculateWaypoints()
        {
            int fitY = (int)(_gameArea.y / _ufoSize.y) * 2; // Max number of waypoints

            if (_wayPoints == null || _wayPoints.Length != fitY)
                _wayPoints = new Vector2[fitY];

            float waypointX = 0;
            float waypointY = 0;
            int n = 0;
            for(int j = 0; j < fitY * .5f; j++)
            {
                waypointY = transform.position.y + .5f * (_gameArea.y - _ufoZoneSize.y) - j * _ufoSize.y;

                for (int i = 0; i < 2; i++)
                {
                    if(j % 2 == 0)
                        waypointX = transform.position.x + .5f * (_ufoZoneSize.x - _gameArea.x) + i * (_gameArea.x - _ufoZoneSize.x);
                    else
                        waypointX = transform.position.x - .5f * (_ufoZoneSize.x - _gameArea.x) - i * (_gameArea.x - _ufoZoneSize.x);

                    _wayPoints[n] = new Vector2 (waypointX, waypointY);
                    n++;
                }
            }
        }

        private void PinUfoZoneToTopLeft()
        {
            _ufoZone.localPosition = .5f * new Vector2(-_gameArea.x + _ufoZoneSize.x, _gameArea.y - _ufoZoneSize.y);
        }

        private void DrawPath()
        {
            for(int i = 0; i < _wayPoints.Length - 1; i++)
            {
                Gizmos.DrawWireSphere(_wayPoints[i], .2f);
                Gizmos.DrawLine(_wayPoints[i], _wayPoints[i + 1]);

                if(i == _wayPoints.Length - 2)
                    Gizmos.DrawWireSphere(_wayPoints[i+1], .2f);
            }
        }

        private void Move()
        {

        }
        #endregion
    }
}