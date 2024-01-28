using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Script
{
    public class AStarState
    {
        public Vector2 Current;
        public Vector2 Goal;
        public List<Vector2> MoveList = new();

        public void Initialization(Vector2 current, Vector2 goal, List<Vector2> moveList)
        {
            Current = current;
            Goal = goal;
            MoveList = moveList.GetRange(0, moveList.Count);
        }

        public int Value()
        {
            return (int)(MoveList.Count + Math.Abs(Goal.x - Current.x) + Math.Abs(Goal.y - Current.y));
        }   
    }

    public class AStarStateComparer : IComparer<AStarState>
    {
        public int Compare(AStarState x, AStarState y)
        {
            if (y != null && x != null && x.Value() > y.Value())
                return -1;
            else if (y != null && x != null && x.Value() < y.Value())
                return 1;
            else return 0;
        }
    }

    public class Snake : MonoBehaviour
    {
        public static Snake Instance;
        public Sprite Body;
        private List<GameObject> _snakeBody;
        private Vector2 _direction;

        private List<GameObject> _pool;
        
        private float _nextUpdate;
        private GameObject _defaultBody;

        private List<Vector2> _moveList;

        private readonly Dictionary<(int, int), bool> _visited = new Dictionary<(int, int), bool>();

        private void Awake()
        {
            Instance = this;
            _snakeBody = new List<GameObject>();
            _pool = new List<GameObject>();
            _moveList = new List<Vector2>();
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
            _direction = new Vector2(0, -1);
            var body = CreateNewBody(new Vector2(0, 0));
            _snakeBody.Add(body);   

            GridSystem.Instance.SetGrid(0, 0, true);
        }

        void Update()
        {
            // HandleInput();
            if (!(Time.time >= _nextUpdate)) return;
            AutoMove();
            Move();
            
            _nextUpdate = Time.time + GameManager.Instance.Delay;
        }

        void AutoMove()
        {
            if (_moveList.Count == 0)
            {
                _direction = Vector2.zero;
                return;
            }
            _direction = _moveList[0];
            _moveList.RemoveAt(0);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        void Move()
        {
            var newPos = GetSnakePosition() + _direction;
            if (!IsValidPosition(newPos) || Vector2.Distance(_direction, Vector2.zero) < 0.01f) return;
            
            
            _snakeBody[0].GetComponent<SpriteRenderer>().color = Color.yellow;
            var body = CreateNewBody(newPos);
            _snakeBody.Insert(0, body);
            // GridSystem.Instance.SetGrid((int) newPos.x, (int) newPos.y, true);
            if (IsEatFood(newPos))
            {
                Food.Instance.GenerateRandomPosition();
            }
            else
            {
                var tail = _snakeBody[^1];
                // GridSystem.Instance.SetGrid((int) tail.transform.position.x, (int) tail.transform.position.y, false);
                RemoveSnakeBodyAt(_snakeBody.Count - 1);
            }
            
            _snakeBody[0].GetComponent<SpriteRenderer>().color = Color.blue;
        }

        GameObject CreateNewBody(Vector2 position)
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
                _direction = new Vector2(-1, 0);
            }
            else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                _direction = new Vector2(1, 0);
            }
            else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            {
                _direction = new Vector2(0, 1);
            }
            else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            {
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

        private bool IsEatFood(Vector2 head)
        {
            var foodPos = (Vector2) Food.Instance.transform.position;
            return Vector2.Distance(foodPos, head) < 0.001f;
        }

        private Vector2 GetSnakePosition()
        {
            return (Vector2) _snakeBody[0].transform.position;
        }

        public bool IsValidPosition(Vector2 position)
        {
            return GridSystem.Instance.IsValidPosition(position);
        }

        private void AStarSearchFood()
        {
            _visited.Clear();

            List<AStarState> aStarStates = new();
            var tmp = new AStarState();

            var head = GetSnakePosition();

            var food = Food.Instance.transform.position;

            tmp.Initialization(head, food, new List<Vector2>());

            aStarStates.Add(tmp);

            _visited[((int) head.x,(int) head.y)] = true;

            var directionArray = new Vector2[4];
            directionArray[0] = new Vector2(0, 1);
            directionArray[1] = new Vector2(0, -1);
            directionArray[2] = new Vector2(1, 0);
            directionArray[3] = new Vector2(-1, 0);

            while (aStarStates.Count > 0)
            {
                aStarStates.Sort(new AStarStateComparer());
                var currentState = aStarStates[^1];
                aStarStates.RemoveAt(aStarStates.Count - 1);

                var currPos = currentState.Current;
                for (var i = 0; i < 4; i++)
                {
                    var newPos = currPos + directionArray[i];
                    var x = (int) newPos.x;
                    var y = (int) newPos.y;

                    if (IsValidPosition(newPos) && !_visited.ContainsKey((x, y)))
                    {
                        if (currentState.Goal == newPos) {
                            _moveList = currentState.MoveList.GetRange(0, currentState.MoveList.Count);
                            _moveList.Add(directionArray[i]);
                            return;
                        }
                        else
                        {
                            var tmpState = new AStarState();
                            tmpState.Initialization(newPos, food, currentState.MoveList);
                            tmpState.MoveList.Add(directionArray[i]);
                            aStarStates.Add(tmpState);
                            _visited[(x, y)] = true;
                        }
                    }
                }
            }

        }
    }
}
