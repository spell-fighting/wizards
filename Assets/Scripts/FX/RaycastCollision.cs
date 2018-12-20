using System;
using UnityEngine;
using System.Collections.Generic;

public class RaycastCollision : MonoBehaviour
{
    public GameObject[] Effects;
    public float Offset;
    public float TimeDelay;
    public float DestroyTime = 0.5f;
    public bool UsePivotPosition;
    public bool UseNormalRotation = true;
    public bool IsWorldSpace = true;
    [HideInInspector] public float HUE = -1;
    [HideInInspector] public List<GameObject> CollidedInstances = new List<GameObject>();
    [SerializeField] private LayerMask _collidesWith = ~0;

    private bool _canUpdate;
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

        if (TimeDelay < 0.001f) ComputeRaycast();
        else Invoke(nameof(ComputeRaycast), TimeDelay);
    }

    private void Update()
    {
        Debug.DrawLine(transform.position, _target, Color.red);
    }

    private void ComputeRaycast()
    {
        RaycastHit raycastHit;
        if (Physics.Linecast(transform.position + new Vector3(0, 1, 0), _target, out raycastHit, _collidesWith))
        {
            Vector3 position;
            if (UsePivotPosition)
                position = raycastHit.transform.position;
            else
                position = raycastHit.point + raycastHit.normal * Offset;

            if (CollidedInstances.Count == 0)
                foreach (var effect in Effects)
                {
                    var instance = Instantiate(effect, position, new Quaternion());
                    CollidedInstances.Add(instance);
                    if (HUE > -0.9f)
                    {
                        RFX4_ColorHelper.ChangeObjectColorByHUE(instance, HUE);
                    }

                    if (!IsWorldSpace)
                        instance.transform.parent = transform;
                    if (UseNormalRotation)
                        instance.transform.LookAt(raycastHit.point + raycastHit.normal);
                    Destroy(instance, DestroyTime);
                    Destroy(gameObject.transform.parent.gameObject, DestroyTime);
                }
            else
                foreach (var instance in CollidedInstances)
                {
                    if (instance == null) continue;
                    instance.transform.position = position;
                    if (UseNormalRotation)
                        instance.transform.LookAt(raycastHit.point + raycastHit.normal);
                }
        }
    }
}