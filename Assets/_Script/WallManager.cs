using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

        private void Update()
        {
            if (GameManager.Instance.Debug)
                ShowWallByHashTable();
        }

        private void CreateWalls()
        {
            _walls = new List<GameObject>();
            // for (var i = 0; i < GridSystem.Instance.Width * GridSystem.Instance.Height / 50; i++)
            // {
            //     // var x = -GridSystem.Instance.Width/2 + i * 2;
            //     var x = Random.Range(-GridSystem.Instance.Width / 2, GridSystem.Instance.Width / 2);
            //     var y = Random.Range(-GridSystem.Instance.Height / 2, GridSystem.Instance.Height / 2);
            //     if (x == 0 && y == 0) continue;
            //     var wall = CreateNewWall(new Vector3(x, y, 0));
            //     _walls.Add(wall);
            //     GridSystem.Instance.SetGrid(x, y, -1);
            // }
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
            spriteRenderer.sortingOrder = 5;
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

        private void ShowWallByHashTable()
        {
            foreach (var wall in _walls)
            {
                wall.SetActive(false);
            }

            var index = 0;
            foreach (var (key, value) in GridSystem.Instance.GetHashTable())
            {
                if (value != 0)
                {
                    if (index < _walls.Count)
                    {
                        _walls[index].SetActive(true);
                        _walls[index].transform.position = new Vector3(key.Item1, key.Item2, 0);
                    }
                    else
                    {
                        _walls.Add(CreateNewWall(new Vector3(key.Item1, key.Item2, 0)));
                    }

                    index++;
                }
            }
        }

        public GameObject GetDefaultWall()
        {
            return _defaultWall;
        }
    }
}
