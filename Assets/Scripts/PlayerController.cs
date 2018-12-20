using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    public GameObject PlayerRightHand;
    public GameObject PlayerLeftHand;

    [SerializeField] private float _speed = 6.0f;

    [SerializeField] private float _gravity = 1000.0f;

    [SerializeField] private GameObject _fireBallEffectPrefab;

    [SerializeField] private GameObject _iceWallPrefab;

    [SerializeField] private GameObject _lightningEffectPrefab;

    [SerializeField] private GameObject _lightningHandEffectPrefab;

    [SerializeField] private GameObject _waterJailPrefab;

    [SerializeField] private bool _useSpeller;

    [SerializeField] private Slider _healthBar;

    [SerializeField] private GameObject _cameraGo;

    [SerializeField] private GameObject _canvasGo;

    [SerializeField] private int _spellStoreLimit = 5;

    [SerializeField] private CharacterController _characterController;

    [SerializeField] private int _secondsBetweenFires = 3;
    
    [SerializeField] private RawImage Spell0;
    [SerializeField] private RawImage Spell1;
    [SerializeField] private RawImage Spell2;
    [SerializeField] private RawImage Spell3;
    [SerializeField] private RawImage Spell4;

    public Texture FireBallLogo;
    public Texture IceWallLogo;
    public Texture LightningLogo;
    public Texture TimeShiftLogo;
    public Texture WaterJailLogo;

    private float _currentHealth = 100;
    private const float MaxHealth = 100;
    public Animator Anim;
    private Vector3 _moveDirection = Vector3.zero;
    private Camera _camera;
    private CustomNetworkManager _networkController;
    private GameObject _currentEffect;
    private GameObject _currentRightHandEffect;
    private GameObject _currentLeftHandEffect;
    private FixedSizedQueue<Spell> _spellStore;
    private GameManager _gameManager;
    private bool _canFireAndMove = true;
    
    private void Awake()
    {
        _spellStore = new FixedSizedQueue<Spell> {Limit = _spellStoreLimit};
        UpdateSpells();
    }

    public void Spelled(Prediction prediction)
    {
        if (!_useSpeller)
            return;

        switch (prediction)
        {
            case Prediction.Triangle:
                _spellStore.Enqueue(Spell.FireBall);
                break;
            case Prediction.Circle:
                _spellStore.Enqueue(Spell.WaterJail);
                break;
            case Prediction.Square:
                _spellStore.Enqueue(Spell.IceWall);
                break;
            case Prediction.Star:
                _spellStore.Enqueue(Spell.Lighting);
                break;
            case Prediction.Hourglass:
                _spellStore.Enqueue(Spell.TimeShift);
                break;
        }

        UpdateSpells();
    }

    private void UpdateSpells()
    {
        var store = _spellStore.ToArray();

        Spell0.GetComponent<RawImage>().enabled = false;
        Spell1.GetComponent<RawImage>().enabled = false;
        Spell2.GetComponent<RawImage>().enabled = false;
        Spell3.GetComponent<RawImage>().enabled = false;
        Spell4.GetComponent<RawImage>().enabled = false;

        for (var i = _spellStore.GetCount() - 1; i >= 0; i--)
        {
            RawImage currentSpell;
            switch (i)
            {
                case 0:
                    currentSpell = Spell4;
                    break;
                case 1:
                    currentSpell = Spell3;
                    break;
                case 2:
                    currentSpell = Spell2;
                    break;
                case 3:
                    currentSpell = Spell1;
                    break;
                case 4:
                    currentSpell = Spell0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var image = currentSpell.GetComponent<RawImage>();

            switch ((Spell) store[i])
            {
                case Spell.Lighting:
                    image.texture = LightningLogo;
                    image.enabled = true;
                    break;
                case Spell.FireBall:
                    image.texture = FireBallLogo;
                    image.enabled = true;
                    break;
                case Spell.IceWall:
                    image.texture = IceWallLogo;
                    image.enabled = true;
                    break;
                case Spell.TimeShift:
                    image.texture = TimeShiftLogo;
                    image.enabled = true;
                    break;
                case Spell.WaterJail:
                    image.texture = WaterJailLogo;
                    image.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        _camera = _cameraGo.GetComponent<Camera>();
        UpdateHealthBar();

        gameObject.tag = hasAuthority ? "Player" : "Enemy";

        if (isServer)
        {
            GameObject.Find("NetworkDiscovery").GetComponent<NetworkServerDiscoveryController>().enabled = true;
        }

        if (hasAuthority)
        {
            _cameraGo.SetActive(true);
            _canvasGo.SetActive(true);
        }
        else
        {
            _cameraGo.SetActive(false);
            _canvasGo.SetActive(false);
        }
    }

    private void UpdateHealthBar()
    {
        _healthBar.value = _currentHealth / MaxHealth;
    }

    public void LowerHealthPoint(int loss)
    {
        _currentHealth = _currentHealth - loss;
        UpdateHealthBar();

        if (_currentHealth <= 0)
        {
            _gameManager.SetState(gameObject.CompareTag("Enemy") ? GameState.Won : GameState.Lost);

            StartCoroutine(nameof(Stop));
        }

        if (gameObject.CompareTag("Enemy"))
        {
            _gameManager.IncrementSpellLanded();
        }
    }

    private IEnumerator Stop()
    {
        yield return new WaitForSeconds(2);

        if (isServer)
        {
            NetworkManager.singleton.StopServer();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _useSpeller = !_useSpeller;
        }

        if (hasAuthority && Anim.GetCurrentAnimatorStateInfo(0).IsName("BaseLayer.Idle") && _canFireAndMove)
        {
            _moveDirection = Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0) *
                             new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            _moveDirection *= _speed;

//            // Apply Gravity. Gravity is multiplied by deltaTime twice (once here, and once below
//            // when the _moveDirection is multiplied by deltaTime). This is because Gravity should be applied
//            // as an acceleration (ms^-2)
            _moveDirection.y -= _gravity * Time.deltaTime;

            // Move the controller
            _characterController.Move(_moveDirection * Time.deltaTime);
        }

        if (hasAuthority && _characterController.isGrounded)
        {
            if (!_useSpeller)
            {
                if (Input.GetKeyDown(KeyCode.A))
                {
                    CastSpell(Spell.WaterJail);
                }

                if (Input.GetKeyDown(KeyCode.Z))
                {
                    CastSpell(Spell.IceWall);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    CastSpell(Spell.Lighting);
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    CastSpell(Spell.FireBall);
                }

                if (Input.GetKeyDown(KeyCode.T))
                {
                    CastSpell(Spell.TimeShift);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (_spellStore.GetCount() > 0 && Anim.GetCurrentAnimatorStateInfo(0).IsName("BaseLayer.Idle") && _canFireAndMove)
                {
                    _canFireAndMove = false;
                    StartCoroutine(nameof(ReEnableFiring));
                    var spell = (Spell) _spellStore.Dequeue();
                    UpdateSpells();
                    CastSpell(spell);
                }
            }
        }
    }

    private IEnumerator ReEnableFiring()
    {
        yield return new WaitForSeconds(_secondsBetweenFires);
        _canFireAndMove = true;
    }

    private void CastSpell(Spell spell)
    {
        var ownEnemy = GameObject.FindWithTag("Enemy");
        var target = new Vector3(0, 0, 0);

        if (ownEnemy)
        {
            target = new Vector3(ownEnemy.transform.position.x, ownEnemy.transform.position.y + 1,
                ownEnemy.transform.position.z);
        }

        CmdSpellOn(spell, target, gameObject);
        _gameManager.IncrementSpellFired();
    }

    [Command]
    private void CmdSpellOn(Spell spell, Vector3 target, GameObject player)
    {
        RpcSpellOn(spell, target, player);
    }

    [ClientRpc]
    private void RpcSpellOn(Spell spell, Vector3 target, GameObject player)
    {
        var playerRightHand = player.GetComponent<PlayerController>().PlayerRightHand;
        var playerLeftHand = player.GetComponent<PlayerController>().PlayerLeftHand;
        var anim = player.GetComponent<PlayerController>().Anim;

        switch (spell)
        {
            case Spell.WaterJail:
                anim.SetTrigger("Throw");
                _currentEffect = Instantiate(_waterJailPrefab, target, Quaternion.identity);
                _currentEffect.SetActive(false);
                break;
            case Spell.FireBall:
                anim.SetTrigger("Throw");
                _currentEffect = Instantiate(_fireBallEffectPrefab, playerRightHand.transform.position,
                    Quaternion.identity, playerRightHand.transform);
                _currentEffect.SetActive(false);
                _currentEffect.GetComponentInChildren<TransformMotion>(true).SetTarget(target);
                break;
            case Spell.IceWall:
                var iceWallPosition = new Vector3(0.5f, 2.5f, 0.0f);
                Instantiate(_iceWallPrefab, iceWallPosition, Quaternion.identity);
                break;
            case Spell.Lighting:
                anim.SetTrigger("Light");
                _currentEffect = Instantiate(_lightningEffectPrefab,
                    player.transform.position +
                    Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0) * new Vector3(0, 0, 1), Quaternion.identity,
                    transform);
                _currentRightHandEffect = Instantiate(_lightningHandEffectPrefab, playerRightHand.transform.position,
                    Quaternion.identity, transform);
                _currentLeftHandEffect = Instantiate(_lightningHandEffectPrefab, playerLeftHand.transform.position,
                    Quaternion.identity, transform);
                _currentEffect.SetActive(false);
                _currentLeftHandEffect.SetActive(false);
                _currentRightHandEffect.SetActive(false);
                _currentEffect.GetComponentInChildren<ParticleLight>(true).SetTarget(target);
                _currentEffect.GetComponentInChildren<RaycastCollision>(true).SetTarget(target);
                break;
            case Spell.TimeShift:
                var iceWall = GameObject.Find("IceWall(Clone)");
                if (iceWall)
                    Destroy(iceWall);
                break;
        }
    }

    public void ActivateEffect()
    {
        if (_currentEffect)
        {
            _currentEffect.SetActive(true);
        }
    }

    public void ActivateCharacterEffects()
    {
        if (_currentLeftHandEffect)
        {
            _currentLeftHandEffect.SetActive(true);
        }

        if (_currentRightHandEffect)
        {
            _currentRightHandEffect.SetActive(true);
        }
    }
}