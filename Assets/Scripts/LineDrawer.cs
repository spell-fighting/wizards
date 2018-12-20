using UnityEngine;
using UnityEngine.UI.Extensions;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    [SerializeField] private float _delay = 3.0f;
    [SerializeField] private int _minimumPoints = 30;
    private UILineRenderer _line;
    private Vector3 _mousePosition;
    private const int ImageSize = 28;
    private Predictor _predictor;

    private void Start()
    {
        _line = GetComponent<UILineRenderer>();
        Invoke(nameof(SpellAndDestroy), _delay);
        _predictor = GameObject.Find("Predictor").GetComponent<Predictor>();
        // This will always be called in case the MouseButtonUp event is missed or the player takes too long to draw
    }

    private void SpellAndDestroy()
    {
        var positions = _line.Points;

        if (positions.Length < _minimumPoints)
        {
            Destroy(gameObject);
            return;
        }

        try
        {
            var prediction = _predictor.Predict(positions, ImageSize);

            if (prediction.HasValue)
            {
                GameObject.FindWithTag("Player").GetComponent<PlayerController>().Spelled(prediction.Value);
            }

            Destroy(gameObject);
        }
        catch
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            _mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            var pointList = new List<Vector2>(_line.Points) {_mousePosition};
            _line.Points = pointList.ToArray();
        }

        if (Input.GetMouseButtonUp(0))
        {
            CancelInvoke(nameof(SpellAndDestroy));
            SpellAndDestroy();
        }
    }
}