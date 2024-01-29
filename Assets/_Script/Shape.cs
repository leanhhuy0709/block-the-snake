using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Script
{
    public class Shape : MonoBehaviour
    {
        public static Shape Instance;
        public Sprite Wall;

        private List<GameObject> _walls;

        private GameObject _defaultWall;

        private bool _isMouseDown = false;
        private Vector2 _oldPosition;
        public bool IsDrag;

        private void Awake()
        {
            Instance = this;
            
        }

        void Start()
        {
            _oldPosition = this.transform.position;
            CreateWalls();
        }

        private void Update()
        {
            HandleInput();
        }

        protected void CreateWalls()
        {
            _walls = new List<GameObject>();
            for (var i = -1; i <= 1; i++)
            {
                var wall = CreateNewWall(transform.position + new Vector3(i * GridSystem.Instance.CellSize, 0, 0));
                _walls.Add(wall);
            }
        }

        public GameObject CreateNewWall(Vector3 position)
        {
            var wall = 
                Instantiate(WallManager.Instance.GetDefaultWall(), WallManager.Instance.GetDefaultWall().transform.position, WallManager.Instance.GetDefaultWall().transform.rotation);
            
            wall.transform.SetParent(this.transform);
            wall.SetActive(true);
            wall.transform.position = position;
            
            return wall;
        }

        private void HandleInput()
        {
            if (_isMouseDown)
            {
                Vector3 mousePosition = Input.mousePosition;

                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            
                transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);

            }
            else 
            {
                HandlePutShape();
                if (IsDrag)
                    transform.position = _oldPosition;
            }
        }

        private void OnMouseDown()
        {
            if (!IsDrag) return;
            _isMouseDown = true;
        }

        private void OnMouseUp()
        {
            if (!IsDrag) return;
            _isMouseDown = false;
        }

        private void HandlePutShape()
        {
            var pos = transform.position;

            var x = (int) (pos.x + 0.5f);
            var y = (int) (pos.y + 0.5f);

            if(Math.Abs(pos.x - x) < 0.5f && Math.Abs(pos.y - y) < 0.5f && Vector3.Distance(pos, _oldPosition) > 1f)
            {
                IsDrag = false;
                _isMouseDown = false;
                transform.position = new Vector3(x, y, 0);
            }
        }
    }
}
