using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Guarana
{
    public class Background : MonoBehaviour
    {
        #region Fields

        [SerializeField] private float _moveSpeed;

        private BoxCollider2D _col;

        #endregion
        
        #region Unity Event Functions

        private void Start()
        {
            _col = GetComponent<BoxCollider2D>();
        }

        private void FixedUpdate()
        {
            var bgTransform = transform.position;
            bgTransform = new Vector2(0f, bgTransform.y + _moveSpeed * Time.deltaTime);
            
            transform.position = bgTransform;

            var pos = (Vector2) bgTransform - _col.size/2;
            var screenPos = Camera.main.WorldToScreenPoint(pos);

            if (screenPos.y >= Screen.height)
            {
                transform.position = new Vector2(0f, -_col.size.y);
            }
        }

        #endregion
    }   
}
