using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleTrail : MonoBehaviour
{
    [SerializeField] private Vector2 _defaultSizeMultiplayer = Vector2.one;
    [SerializeField] private float _vertexLifeTime = 2;
    [SerializeField] private float _trailLifeTime = 2;
    [SerializeField] private bool _useShaderMaterial;
    [SerializeField] private Material _trailMaterial;
    [SerializeField] private bool _useColorOverLifeTime = false;
    [SerializeField] private Gradient _colorOverLifeTime = new Gradient();
    [SerializeField] private float _colorLifeTime = 1;
    [SerializeField] private bool _useUvAnimation = false;
    [SerializeField] private int _tilesX = 4;
    [SerializeField] private int _tilesY = 4;
    [SerializeField] private int _fps = 30;
    [SerializeField] private bool _isLoop = true;
    [Range(0.001f, 1)] [SerializeField] private float _minVertexDistance = 0.01f;
    [SerializeField] private bool _getVelocityFromParticleSystem = false;
    [SerializeField] private float _gravity = 0.01f;
    [SerializeField] private Vector3 _force = new Vector3(0, 0.01f, 0);
    [SerializeField] private float _inheritVelocity = 0;
    [SerializeField] private float _drag = 0.01f;
    [Range(0.001f, 10)] [SerializeField] private float _frequency = 1;
    [Range(0.001f, 10)] [SerializeField] private float _offsetSpeed = 0.5f;
    [SerializeField] private bool _randomTurbulenceOffset = false;
    [Range(0.001f, 10)] [SerializeField] private float _amplitude = 2;
    [SerializeField] private float _turbulenceStrength = 0.1f;
    [SerializeField] private AnimationCurve _velocityByDistance = AnimationCurve.EaseInOut(0, 1, 1, 1);
    [SerializeField] private float _aproximatedFlyDistance = -1;
    [SerializeField] private bool _smoothCurves = true;

    private readonly Dictionary<int, LineRenderer> dict = new Dictionary<int, LineRenderer>();
    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;
    private TrailRenderer[] _trails;
    private Color _psColor;
    private int _layer;
    private bool _isLocalSpace = true;
    private Vector3 _target;
    private Transform _t;
    //private bool isInitialized;

    public void SetTarget(Vector3 target)
    {
        _target = target;
    }

    private void OnEnable()
    {
        _ps = GetComponent<ParticleSystem>();
        // ps.startRotation3D = new Vector3(100000, 100000, 100000);

        _t = transform;
        _isLocalSpace = _ps.main.simulationSpace == ParticleSystemSimulationSpace.Local;
        _particles = new ParticleSystem.Particle[_ps.main.maxParticles];

        if (_trailMaterial != null)
        {
            _psColor = _trailMaterial.GetColor(_trailMaterial.HasProperty("_TintColor") ? "_TintColor" : "_Color");
        }


        // InvokeRepeating("RemoveEmptyTrails", 1, 1);
        _layer = gameObject.layer;
        //isInitialized = true;
        Update();
    }


    void ClearTrails()
    {
        foreach (var trailRenderer in _trails)
        {
            if (trailRenderer != null) Destroy(trailRenderer.gameObject);
        }

        _trails = null;
    }

    //void OnEnable()
    //{
    //    if(isInitialized) Update();
    //}

    private void Update()
    {
        if (dict.Count > 10)
            RemoveEmptyTrails();

        var count = _ps.GetParticles(_particles);
        for (var i = 0; i < count; i++)
        {
            var hash = (_particles[i].rotation3D).GetHashCode();
            if (!dict.ContainsKey(hash))
            {
                var go = new GameObject(hash.ToString());
                go.transform.parent = transform;
                //go.hideFlags = HideFlags.HideAndDontSave;
                go.transform.position = _ps.transform.position;

                if (_trailLifeTime > 0.00001f) Destroy(go, _trailLifeTime + _vertexLifeTime);
                go.layer = _layer;

                var lineRenderer = go.AddComponent<LineRenderer>();

                lineRenderer.startWidth = 0;
                lineRenderer.endWidth = 0;
                lineRenderer.sharedMaterial = _trailMaterial;
                lineRenderer.useWorldSpace = false;

                if (_useColorOverLifeTime)
                {
                    var shaderColor = go.AddComponent<RFX4_ShaderColorGradient>();
                    shaderColor.Color = _colorOverLifeTime;
                    shaderColor.TimeMultiplier = _colorLifeTime;
                }

                if (_useUvAnimation)
                {
                    var uvAnimation = go.AddComponent<RFX4_UVAnimation>();
                    uvAnimation.TilesX = _tilesX;
                    uvAnimation.TilesY = _tilesY;
                    uvAnimation.FPS = _fps;
                    uvAnimation.IsLoop = _isLoop;
                }

                dict.Add(hash, lineRenderer);
            }
            else
            {
                var trail = dict[hash];
                if (trail == null) continue;


                if (!trail.useWorldSpace)
                {
                    trail.useWorldSpace = true;
                    InitTrailRenderer(trail.gameObject);
                }

                var size = _defaultSizeMultiplayer * _particles[i].GetCurrentSize(_ps);


                trail.startWidth = size.y;
                trail.endWidth = size.x;
//                if (_target != null)
//                {
                var time = 1 - _particles[i].remainingLifetime / _particles[i].startLifetime;
                var pos = Vector3.Lerp(_particles[i].position, _target, time);
                trail.transform.position = Vector3.Lerp(pos, _target, Time.deltaTime * time);
//                }
//                else
//                {
//                    trail.transform.position = isLocalSpace
//                        ? ps.transform.TransformPoint(particles[i].position)
//                        : particles[i].position;
//                }

                trail.transform.rotation = _t.rotation;
                var particleColor = _particles[i].GetCurrentColor(_ps);
                var color = _psColor * particleColor;
                //if (!UseShaderMaterial) trail.material.SetColor("_TintColor", color); 


                trail.startColor = color;
                trail.endColor = color;
            }
        }

        _ps.SetParticles(_particles, count);
    }

    private void InitTrailRenderer(GameObject go)
    {
        var trailRenderer = go.AddComponent<RFX4_TrailRenderer>();

        trailRenderer.Amplitude = _amplitude;
        trailRenderer.Drag = _drag;
        trailRenderer.Gravity = _gravity;
        trailRenderer.Force = _force;
        trailRenderer.Frequency = _frequency;
        trailRenderer.InheritVelocity = _inheritVelocity;
        trailRenderer.VertexLifeTime = _vertexLifeTime;
        trailRenderer.TrailLifeTime = _trailLifeTime;
        trailRenderer.MinVertexDistance = _minVertexDistance;
        trailRenderer.OffsetSpeed = _offsetSpeed;
        trailRenderer.SmoothCurves = _smoothCurves;
        trailRenderer.AproximatedFlyDistance = _aproximatedFlyDistance;
        trailRenderer.VelocityByDistance = _velocityByDistance;
        trailRenderer.RandomTurbulenceOffset = _randomTurbulenceOffset;
        trailRenderer.TurbulenceStrength = _turbulenceStrength;
    }

    private void RemoveEmptyTrails()
    {
        for (var i = 0; i < dict.Count; i++)
        {
            var element = dict.ElementAt(i);
            if (element.Value == null)
                dict.Remove(element.Key);
        }
    }

    private void OnDisable()
    {
        foreach (var trailRenderer in dict)
        {
            if (trailRenderer.Value != null) Destroy(trailRenderer.Value.gameObject);
        }

        dict.Clear();
    }
}