using System;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleLight : MonoBehaviour
{
    [SerializeField] private float _lightIntensityMultiplayer = 1;
    [SerializeField] private LightShadows _shadows = LightShadows.None;
    [SerializeField] private int _lightLimit = 20;

    private ParticleSystem _ps;
    private ParticleSystem.Particle[] _particles;
    private Light[] _lights;
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

        var parent = transform.parent;
        var differentialPosition = _target - parent.position;

        parent.position = differentialPosition / 2;
        parent.LookAt(_target);

        _ps = GetComponent<ParticleSystem>();
        var main = _ps.main;

        if (main.maxParticles > _lightLimit)
        {
            main.maxParticles = _lightLimit;
        }

        _particles = new ParticleSystem.Particle[main.maxParticles];

        _lights = new Light[main.maxParticles];
        for (var i = 0; i < _lights.Length; i++)
        {
            var lightGo = new GameObject();
            _lights[i] = lightGo.AddComponent<Light>();
            _lights[i].transform.parent = transform;
            _lights[i].intensity = 0;
            _lights[i].shadows = _shadows;
        }
    }

    private void Update()
    {
        var count = _ps.GetParticles(_particles);
        for (var i = 0; i < count; i++)
        {
            _lights[i].gameObject.SetActive(true);
            _lights[i].transform.position = _particles[i].position;
            _lights[i].color = _particles[i].GetCurrentColor(_ps);
            _lights[i].range = _particles[i].GetCurrentSize(_ps);
            _lights[i].intensity = _particles[i].GetCurrentColor(_ps).a / 255f * _lightIntensityMultiplayer;
        }

        for (var i = count; i < _particles.Length; i++)
        {
            _lights[i].gameObject.SetActive(false);
        }
    }
}