using UnityEngine;

public class LineCreator : MonoBehaviour
{
    [SerializeField] private GameObject _line;
    private Vector3 _mousePosition;
    private bool _creating;
    private GameObject _drawer;

    private void Create()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _mousePosition.z = 1.0f;
            if (_drawer == null) // Multiple drawers shall not exist at the same time
            {
                _drawer = Instantiate(_line, _mousePosition, Quaternion.Euler(0.0f, 0.0f, 0.0f));
                _drawer.transform.SetParent(GetComponentInParent<Canvas>().transform);
            }
        }
    }

    private void Update()
    {
        if (!_creating)
        {
            _creating = !!Camera.main;
        }

        if (_creating)
        {
            Create();
        }
    }
}