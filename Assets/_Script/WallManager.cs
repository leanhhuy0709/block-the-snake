using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Script
{
    public class WallManager : MonoBehaviour
    {
        public static WallManager Instance;
        public Sprite Wall;

        private List<GameObject> _walls;

        private GameObject _defaultWall;

        private void Awake()
        {
            Instance = this;
            InitDefaultWall();
        }

        void Start()
        {
            CreateWalls();
        }

        private void CreateWalls()
        {
            _walls = new List<GameObject>();
            for (var i = 0; i < 3; i++)
            {
                var wall = CreateNewWall(new Vector3(2 + i * GridSystem.Instance.CellSize, 2, 0));
                _walls.Add(wall);
                GridSystem.Instance.SetGrid(2 + i, 2, true);
            }
        }

        private void InitDefaultWall()
        {
            var body = new GameObject("Wall");
            var spriteRenderer = body.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Wall;
            spriteRenderer.color = Color.black;
            body.transform.SetParent(this.transform);
            body.transform.position = new Vector3(0, 0, 0);
            body.SetActive(false);
            spriteRenderer.sortingOrder = 2;
            _defaultWall = body;
        }

        GameObject CreateNewWall(Vector3 position)
        {
            var wall = 
                Instantiate(_defaultWall, _defaultWall.transform.position, _defaultWall.transform.rotation);
            
            wall.transform.SetParent(this.transform);
            wall.SetActive(true);
            wall.transform.position = position;
            
            return wall;
        }

        public bool IsWall(Vector3 position)
        {
            foreach (var wall in _walls)
            {
                if (wall.transform.position == position)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetWallCount()
        {
            return _walls.Count;
        }
    }
}
