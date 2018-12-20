using System;
using UnityEngine;
using System.Collections.Generic;

public class TransformMotion : MonoBehaviour
{
    public float Distance = 30;
    public float Speed = 1;
    public float Dampeen;
    public float MinSpeed = 1;
    public float TimeDelay;
    public LayerMask CollidesWith = ~0;

    public GameObject[] EffectsOnCollision;
    public float CollisionOffset;
    public float DestroyTimeDelay = 5;
    public bool CollisionEffectInWorldSpace = true;
    public GameObject[] DeactivatedObjectsOnCollision;
    [HideInInspector] public float Hue = -1;
    [HideInInspector] public List<GameObject> CollidedInstances;
    public event EventHandler<Rfx4CollisionInfo> CollisionEnter;

    private Vector3 _startPositionLocal;
    private Transform _t;
    private Vector3 _oldPos;
    private bool _isCollided;
    private bool _isOutDistance;
    private Quaternion _startQuaternion;
    private float _currentSpeed;
    private float _currentDelay;
    private const float RayCastTolerance = 0.3f;
    private bool _isInitialized;
    private bool _dropFirstFrameForFixUnityBugWithParticles;
    private Vector3 _target;
    private bool _targetSet;

    public void SetTarget(Vector3 target)
    {
        _target = target;
        _targetSet = true;
    }

    private void Start()
    {
        if (!_targetSet)
        {
            throw new Exception("A target needs to be set.");
        }
        _t = transform;
        _startQuaternion = _t.rotation;
        var relativePos = _target - transform.position;
        transform.rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        _startPositionLocal = _t.localPosition;
        _oldPos = _t.TransformPoint(_startPositionLocal);
        Initialize();
        _isInitialized = true;
    }

    private void OnEnable()
    {
        if (_isInitialized) Initialize();
    }

    private void OnDisable()
    {
        if (_isInitialized) Initialize();
    }

    private void Initialize()
    {
        _isCollided = false;
        _isOutDistance = false;
        _currentSpeed = Speed;
        _currentDelay = 0;
        _startQuaternion = _t.rotation;
        _t.localPosition = _startPositionLocal;
        _oldPos = _t.TransformPoint(_startPositionLocal);
        OnCollisionDeactivateBehaviour(true);
        _dropFirstFrameForFixUnityBugWithParticles = true;
    }

    private void Update()
    {
        if (!_dropFirstFrameForFixUnityBugWithParticles)
        {
            UpdateWorldPosition();
        }
        else _dropFirstFrameForFixUnityBugWithParticles = false;
    }

    private void UpdateWorldPosition()
    {
        _currentDelay += Time.deltaTime;
        if (_currentDelay < TimeDelay)
            return;

        var frameMoveOffset = Vector3.zero;
        var frameMoveOffsetWorld = Vector3.zero;
        if (!_isCollided && !_isOutDistance)
        {
            _currentSpeed = Mathf.Clamp(_currentSpeed - Speed * Dampeen * Time.deltaTime, MinSpeed, Speed);
            var currentForwardVector = Vector3.forward * _currentSpeed * Time.deltaTime;
            frameMoveOffset = _t.localRotation * currentForwardVector;
            frameMoveOffsetWorld = _startQuaternion * currentForwardVector;
        }

        var currentDistance = (_t.localPosition + frameMoveOffset - _startPositionLocal).magnitude;

        RaycastHit hit;
        if (!_isCollided && Physics.Raycast(_t.position, _t.forward, out hit, 10, CollidesWith))
        {
            if (frameMoveOffset.magnitude + RayCastTolerance > hit.distance)
            {
                _isCollided = true;
                _t.position = hit.point;
                _oldPos = _t.position;
                OnCollisionBehaviour(hit);
                OnCollisionDeactivateBehaviour(false);
                return;
            }
        }

        if (!_isOutDistance && currentDistance > Distance)
        {
            _isOutDistance = true;
            _t.localPosition = _startPositionLocal + _t.localRotation * Vector3.forward * Distance;
            _oldPos = _t.position;
            return;
        }

        _t.position = _oldPos + frameMoveOffsetWorld;
        _oldPos = _t.position;
    }


    private void OnCollisionBehaviour(RaycastHit hit)
    {
        CollidedInstances.Clear();
        foreach (var effect in EffectsOnCollision)
        {
            var instance = Instantiate(effect, hit.point + hit.normal * CollisionOffset, new Quaternion());
            CollidedInstances.Add(instance);
            if (Hue > -0.9f)
            {
                RFX4_ColorHelper.ChangeObjectColorByHUE(instance, Hue);
            }

            instance.transform.LookAt(hit.point + hit.normal + hit.normal * CollisionOffset);
            if (!CollisionEffectInWorldSpace) instance.transform.parent = transform;
            Destroy(instance, DestroyTimeDelay);
        }
        
        Destroy(transform.parent.gameObject, DestroyTimeDelay);
    }

    private void OnCollisionDeactivateBehaviour(bool active)
    {
        foreach (var effect in DeactivatedObjectsOnCollision)
        {
            effect.SetActive(active);
        }
    }

    public class Rfx4CollisionInfo : EventArgs
    {
        public RaycastHit Hit;
    }
}