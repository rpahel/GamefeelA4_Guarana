using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Guarana
{
    public class UfoSpawner : MonoBehaviour
    {
        //==== Debug ====
        [Header("Debug")]
        [SerializeField, Tooltip("Colorize enemies so it shows as a gradient from blue to red.")]
        private bool _coloredUfos = false;

        //==== Set Up ====
        [Header("Set Up")]
        [SerializeField, Tooltip("Center of the zone the UFOs are contained in.")]
        private Vector2 _ufoZoneCenter = Vector2.zero;
        [SerializeField, Tooltip("Size of the zone the UFOs are contained in.")]
        private Vector2 _ufoZoneRange = Vector2.zero;
        [SerializeField, Tooltip("Size of one UFO.")]
        private Vector2 _ufoSize = Vector2.one;

        private Vector2[,] _ufoTiles = null;

        //==== Game ====
        [Header("Gameplay values")]
        [SerializeField, Tooltip("UFO GameObject.")]
        private GameObject _ufoGo = null;
        [SerializeField, Tooltip("In seconds, rate at which the UFO are being spawned.")]
        private float _spawnTime = 1f;
        [SerializeField, Tooltip("In seconds, time to wait after Start() for the first UFO to spawn.")]
        private float _spawnStartDelay = 2f;
        [SerializeField, Tooltip("Initial number of rows on game start.")]
        private int _nbOfRowsToSpawnOnStart = 2;

        private float _spawnTimeCountdown = 0f;

        //==== Pool ====
        private uint _poolInitialSize = 0;
        //private Ufo[] _ufoPool = null;
        private List<GameObject> _testPool = null;

        #region UNITY FUNCTIONS
        private void Start()
        {
            UpdateUfoTiles();
            SpawnUfos();
            HideUfoRows(_nbOfRowsToSpawnOnStart);
        }

        private void OnDrawGizmosSelected()
        {
            // Center
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_ufoZoneCenter, .5f * Vector3.one);

            // Vertical
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_ufoZoneCenter, _ufoZoneCenter + new Vector2(0, _ufoZoneRange.y * 0.5f));
            Gizmos.DrawWireCube(_ufoZoneCenter + new Vector2(0, _ufoZoneRange.y * 0.5f), new Vector2(1, 0.1f));
            Gizmos.DrawLine(_ufoZoneCenter, _ufoZoneCenter - new Vector2(0, _ufoZoneRange.y * 0.5f));
            Gizmos.DrawWireCube(_ufoZoneCenter - new Vector2(0, _ufoZoneRange.y * 0.5f), new Vector2(1, 0.1f));

            // Horizontal
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_ufoZoneCenter, _ufoZoneCenter + new Vector2(_ufoZoneRange.x * 0.5f, 0));
            Gizmos.DrawWireCube(_ufoZoneCenter + new Vector2(_ufoZoneRange.x * 0.5f, 0), new Vector2(.1f, 1));
            Gizmos.DrawLine(_ufoZoneCenter, _ufoZoneCenter - new Vector2(_ufoZoneRange.x * 0.5f, 0));
            Gizmos.DrawWireCube(_ufoZoneCenter - new Vector2(_ufoZoneRange.x * 0.5f, 0), new Vector2(.1f, 1));

            if (UnityEditor.EditorApplication.isPlaying)
                return;

            // Ufo boxes
            UpdateUfoTiles();
            Gizmos.color = Color.cyan;
            for (int i = 0; i < _ufoTiles.GetLength(0); i++)
            {
                for(int j = 0; j < _ufoTiles.GetLength(1); j++)
                {
                    Gizmos.DrawWireCube(_ufoTiles[i, j], _ufoSize);
                }
            }
        }
        #endregion

        #region CUSTOM FUNCTIONS
        private Vector2Int NumberOfUfosThatCanFit()
        {
            int x = (int)(_ufoZoneRange.x / _ufoSize.x);
            int y = (int)(_ufoZoneRange.y / _ufoSize.y);

            return new Vector2Int(x, y);
        }

        private void UpdateUfoTiles()
        {
            Vector2Int possibleNbOfUfos = NumberOfUfosThatCanFit();

            _ufoTiles ??= new Vector2[possibleNbOfUfos.x, possibleNbOfUfos.y];

            Vector2Int comparator = new(_ufoTiles.GetLength(0), _ufoTiles.GetLength(1));

            if (_ufoTiles == null || comparator != possibleNbOfUfos)
                _ufoTiles = new Vector2[possibleNbOfUfos.x, possibleNbOfUfos.y];

            float leftOverX = _ufoZoneRange.x - possibleNbOfUfos.x * _ufoSize.x;
            float leftOverY = _ufoZoneRange.y - possibleNbOfUfos.y * _ufoSize.y;

            if(leftOverX < 0) leftOverX = 0;
            if(leftOverY < 0) leftOverY = 0;

            float paddingX = leftOverX / (possibleNbOfUfos.x + 1);
            float paddingY = leftOverY / (possibleNbOfUfos.y + 1);

            Vector2 start = _ufoZoneCenter + .5f * new Vector2(-_ufoZoneRange.x, _ufoZoneRange.y);


            float newX = 0;
            float newY = 0;
            Vector2 previousPoint = Vector2.zero;
            for (int j = 0; j < possibleNbOfUfos.y; j++)
            {
                if (j == 0)
                    newY = start.y - (paddingY + _ufoSize.x * .5f);
                else
                    newY = previousPoint.y - (paddingY + _ufoSize.y);

                for (int i = 0; i < possibleNbOfUfos.x; i++)
                {
                    // ZigZag (Vers la droite puis vers la gauche) // Pb ici
                    if(i == 0)
                    {
                        if(j % 2 == 0)
                            newX = start.x + paddingX + _ufoSize.x * .5f;
                        else
                            newX = -start.x - paddingX - _ufoSize.x * .5f;
                    }
                    else
                    {
                        if (j % 2 == 0)
                            newX = previousPoint.x + paddingX + _ufoSize.x;
                        else
                            newX = previousPoint.x - paddingX - _ufoSize.x;
                    }

                    _ufoTiles[i, j] = new Vector2(newX, newY);
                    previousPoint = new Vector2(newX, newY);
                }
            }
        }
        
        private void SpawnUfos()
        {
            _poolInitialSize = (uint)(_ufoTiles.GetLength(0) * _ufoTiles.GetLength(1));
            _testPool = new List<GameObject>((int)_poolInitialSize);

            int a = 0;
            for (int j = 0; j < _ufoTiles.GetLength(1); j++)
            {
                for (int i = 0; i < _ufoTiles.GetLength(0); i++)
                {
                    GameObject obj = Instantiate(_ufoGo, _ufoTiles[i, j], Quaternion.identity, this.transform);
                    obj.transform.localScale = _ufoSize;
                    
                    // Juste pour test
                    if(_coloredUfos)
                        obj.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.blue, Color.red, (float)a / _poolInitialSize);

                    _testPool.Add(obj);
                    a++;
                }
            }
        }

        private void HideUfoRows(int NbOfRowsStillVisible = 0)
        {
            if(NbOfRowsStillVisible >= _ufoTiles.GetLength(1))
                NbOfRowsStillVisible = _ufoTiles.GetLength(1);

            int a = NbOfRowsStillVisible * _ufoTiles.GetLength(0);
            for (int j = NbOfRowsStillVisible; j < _ufoTiles.GetLength(1); j++)
            {
                for (int i = 0; i < _ufoTiles.GetLength(0); i++)
                {
                    _testPool[a].SetActive(false);
                    a++;
                }
            }
        }
        #endregion
    }
}