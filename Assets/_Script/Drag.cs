using System;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.Events;

namespace _Script
{
    public class Drag : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        private Shape _shape;
        public bool CanDrag = true;

        private Vector2 _oldPosition;

        private void Awake()
        {
            // _rectTransform = GetComponent<RectTransform>();
            // _shape = GetComponent<Shape>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("OnPointerDown");
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // if (!CanDrag) return;
            // _oldPosition = _rectTransform.anchoredPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // if (!CanDrag) return;
            // if (IsValidDrag())
            // {
            //     var position = _rectTransform.anchoredPosition;
            //     var x = (int)Math.Round(position.x / GameHandler.GridSize);
            //     var y = (int)Math.Round(position.y / GameHandler.GridSize);

            //     _rectTransform.anchoredPosition = new Vector2(x * GameHandler.GridSize, y * GameHandler.GridSize);
                
            //     CanDrag = false;

            //     _shape.Box.IsHavingShape = false;
            // }
            // else
            // {
            //     _rectTransform.anchoredPosition = new Vector2(_oldPosition.x, _oldPosition.y);
            // }
            // _shape.UpdateHashTable(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            // if (!CanDrag) return;
            // _rectTransform.anchoredPosition += eventData.delta;
        }

        private bool IsValidDrag()
        {
            return true;
            // var position = _rectTransform.anchoredPosition;
            // var x = (int)Math.Round(position.x / GameHandler.GridSize);
            // var y = (int)Math.Round(position.y / GameHandler.GridSize);
            // _rectTransform.anchoredPosition = new Vector2(x * GameHandler.GridSize, y * GameHandler.GridSize);

            // if (x < GameHandler.Board.WidthSt || x > GameHandler.Board.WidthEd) return false;
            // if (y < GameHandler.Board.HeightSt || y > GameHandler.Board.HeightEd) return false;

            // if (!_shape.IsValidShape()) return false;
            
            // _shape.UpdateHashTable(true);
            // if (GameHandler.Snake.AStarSearchChecked()) return true;
            // _shape.UpdateHashTable(false);
            // return false;
        }
    }
}