using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Script
{
    public class Snake : MonoBehaviour
    {
        public static Snake Instance;
        public Sprite Body;
        private List<GameObject> _snakeBody;
        private Vector3 _direction;

        private List<GameObject> _pool;
        
        private float _nextUpdate;
        private GameObject _defaultBody;

        private List<Vector3> _moveList;

        private void Awake()
        {
            Instance = this;
            _snakeBody = new List<GameObject>();
            _pool = new List<GameObject>();
            _moveList = new List<Vector3>();
        }

        void Start()
        {
            InitDefaultBody();
            CreateSnake();
            _snakeBody[0].GetComponent<SpriteRenderer>().color = Color.blue;
            _nextUpdate = Time.time;

            _moveList.Add(new Vector3(1, 0, 0));
            _moveList.Add(new Vector3(1, 0, 0));
            _moveList.Add(new Vector3(0, 1, 0));
            _moveList.Add(new Vector3(0, 0, 0));
        }

        void CreateSnake()
        {
            _direction = new Vector3(0, -1, 0);
            var body = CreateNewBody(new Vector3(0, 0, 0));
            _snakeBody.Add(body);   

            GridSystem.Instance.SetGrid(0, 0, true);
        }

        void Update()
        {
            HandleInput();
            if (!(Time.time >= _nextUpdate)) return;
            // AutoMove();
            Move();
            
            _nextUpdate = Time.time + GameManager.Instance.Delay;
        }

        void AutoMove()
        {
            if (_moveList.Count == 0) return;
            _direction = _moveList[0];
            _moveList.RemoveAt(0);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void Move()
        {
            var newPos = GetSnakePosition() + _direction;
            if (!IsValidPosition(newPos)) return;
            
            
            _snakeBody[0].GetComponent<SpriteRenderer>().color = Color.yellow;
            var body = CreateNewBody(newPos);
            _snakeBody.Insert(0, body);
            GridSystem.Instance.SetGrid((int) newPos.x, (int) newPos.y, true);
            if (IsEatFood(newPos))
            {
                Food.Instance.GenerateRandomPosition();
            }
            else
            {
                var tail = _snakeBody[^1];
                GridSystem.Instance.SetGrid((int) tail.transform.position.x, (int) tail.transform.position.y, false);
                RemoveSnakeBodyAt(_snakeBody.Count - 1);
            }
            
            _snakeBody[0].GetComponent<SpriteRenderer>().color = Color.blue;
        }

        GameObject CreateNewBody(Vector3 position)
        {
            GameObject body;
            if (_pool.Count > 0)
            {
                body = _pool[^1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
            {
                body = Instantiate(_defaultBody, _defaultBody.transform.position, _defaultBody.transform.rotation);
            }
            body.SetActive(true);
            body.transform.position = position;
            
            return body;
        }

        void RemoveSnakeBodyAt(int index)
        {
            var body = _snakeBody[index];
            _snakeBody.RemoveAt(index);
            body.SetActive(false);
            _pool.Add(body);
        }
        
        private void HandleInput()
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                if (Math.Abs(_direction.x - 1) > 0.01)
                    _direction = new Vector2(-1, 0);
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                if (Math.Abs(_direction.x - (-1)) > 0.01)
                    _direction = new Vector2(1, 0);
            }
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                if (Math.Abs(_direction.y - (-1)) > 0.01)
                    _direction = new Vector2(0, 1);
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
                if (Math.Abs(_direction.y - 1) > 0.01)
                    _direction = new Vector2(0, -1);
            }
        }

        private void InitDefaultBody()
        {
            var body = new GameObject("Snake");
            var spriteRenderer = body.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Body;
            spriteRenderer.color = Color.yellow;
            body.transform.SetParent(this.transform);
            body.transform.position = new Vector3(0, 0, 0);
            body.SetActive(false);
            spriteRenderer.sortingOrder = 2;
            _defaultBody = body;
        }

        private bool IsEatFood(Vector3 head)
        {
            var foodPos = Food.Instance.transform.position;
            return Vector3.Distance(foodPos, head) < 0.001f;
        }

        private Vector3 GetSnakePosition()
        {
            return _snakeBody[0].transform.position;
        }

        public bool IsValidPosition(Vector3 position)
        {
            return GridSystem.Instance.IsValidPosition(position);
        }
    }
}
