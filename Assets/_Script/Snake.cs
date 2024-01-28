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

        private void Awake()
        {
            Instance = this;
            _snakeBody = new List<GameObject>();
            _pool = new List<GameObject>();
        }

        void Start()
        {
            InitDefaultBody();
            CreateSnake();
            _snakeBody[0].GetComponent<SpriteRenderer>().color = Color.blue;
            _nextUpdate = Time.time;
        }

        void CreateSnake()
        {
            _direction = new Vector3(0, -1, 0);
            var body = CreateNewBody(new Vector3(0,0,0));
            _snakeBody.Add(body);   
        }

        void Update()
        {
            HandleInput();
            if (!(Time.time >= _nextUpdate)) return;
            Move();
            
            _nextUpdate = Time.time + GameManager.Instance.Delay;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void Move()
        {
            var newPos = GetSnakePosition() + _direction;
            if (!IsValidPosition(newPos)) return;
            
            
            _snakeBody[0].GetComponent<SpriteRenderer>().color = Color.yellow;
            var body = CreateNewBody(newPos);
            _snakeBody.Insert(0, body);
            if (IsEatFood(newPos))
            {
                Food.Instance.GenerateRandomPosition();
            }
            else
            {
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
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var sb in _snakeBody)
            {
                var pos = sb.transform.position;
                if (Vector3.Distance(pos, position) < 0.01f)
                    return false;
            }

            if (WallManager.Instance.IsWall(position))
                return false;
            
            var minX = -GridSystem.Instance.Width / 2;
            var maxX = GridSystem.Instance.Width / 2;
            var minY = -GridSystem.Instance.Height / 2;
            var maxY = GridSystem.Instance.Height / 2;
            if (position.x > maxX || position.y > maxY || position.x < minX || position.y < minY)
                return false;

            return true;
        }
    }
}
