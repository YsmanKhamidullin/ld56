using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField]
    private CanvasGroup _pauseMenu;

    [SerializeField]
    private CanvasGroup _gameStartMenu;

    [SerializeField]
    private Button _gameStartButton;

    [SerializeField]
    private Button _resumeGameButton;

    [SerializeField]
    private CustomSlider _soundSlider;

    [SerializeField]
    private CustomSlider _musicSlider;

    [Header("Player")]
    [SerializeField]
    private GameObject _player;

    [SerializeField]
    private float _playerSpeed = 2f;

    [FormerlySerializedAs("_playerRotationSpeed"), SerializeField]
    private float _playerRotateDuration = 2f;

    [Header("Debug")]
    [SerializeField]
    private TextMeshProUGUI _debugLabel;

    private void Start()
    {
        UnityEngine.Random.InitState(42);
        DOTween.Init().SetCapacity(100, 50);

        Game.IsPause = true;
        InitializeCanvas();
        InitializePlayer();
        _gameStartButton.onClick.AddListener(() => _ = StartGame());
        _resumeGameButton.onClick.AddListener(() => _ = UnPause());
    }

    private async UniTask StartGame()
    {
        Game.IsPause = true;
        await _gameStartMenu.FadeOut();
        _gameStartMenu.gameObject.SetActive(false);
        Game.IsPause = false;
    }

    private void InitializePlayer()
    {
        _player.gameObject.SetActive(true);
    }

    private void InitializeCanvas()
    {
        _soundSlider.SetValueWithoutNotify(Game.SoundValue);
        _musicSlider.SetValueWithoutNotify(Game.MusicValue);

        _soundSlider.OnValueChanged += SetSound;
        _musicSlider.OnValueChanged += SetMusic;
        _gameStartMenu.gameObject.SetActive(true);
        _pauseMenu.gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdatePause();
        if (Game.IsPause)
        {
            return;
        }

        UpdateRayCast();
        _ = UpdateDash();
        if (!_isDash)
        {
            UpdatePlayerMove();
            UpdatePlayerRotation();
        }
    }

    private Vector3 _mousePos;

    private void UpdatePlayerRotation()
    {
        _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0f; // Set z to zero for 2D mode
        var direction = new Vector2(_mousePos.x - _player.transform.position.x,
            _mousePos.y - _player.transform.position.y);
        float rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        _player.transform.DORotate(new Vector3(0f, 0f, rotationZ), _playerRotateDuration);
    }

    private RaycastHit2D _currentRayHit;

    private void UpdateRayCast()
    {
        _currentRayHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (_currentRayHit.collider != null)
        {
        }
    }

    private UniTask _pauseTask;

    private void UpdatePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _pauseTask.Status is UniTaskStatus.Succeeded or UniTaskStatus.Pending)
        {
            if (Game.IsPause)
            {
                _pauseTask = UnPause();
            }
            else
            {
                _pauseTask = Pause();
            }
        }
    }

    private void UpdatePlayerMove()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        if (horizontal != 0 || vertical != 0)
        {
            horizontal *= _playerSpeed * Time.deltaTime;
            vertical *= _playerSpeed * Time.deltaTime;
            _player.transform.position += new Vector3(horizontal, vertical, 0);
        }
    }

    private async UniTask UpdateDash()
    {
        bool isDash = Input.GetMouseButtonDown(0);
        if (isDash && _isCanDash)
        {
            await PlayerDash(Vector2.up);
            _lastDashTime = Time.time;
        }

        if (_lastDashTime > 0)
        {
            var timeSinceDash = Time.time - _lastDashTime;
            if (timeSinceDash > _dashCoolDown)
            {
                _isCanDash = true;
                _lastDashTime = -1f;
            }
        }
    }

    private bool _isCanDash = true;
    private bool _isPlayerInvincible;
    private bool _isDash;

    [SerializeField]
    private float _dashCoolDown = 0.5f;

    [SerializeField]
    private float _dashTime = 0.2f;

    [SerializeField]
    private float _dashSpeed = 1f;

    private float _currentDashTime = 0.2f;
    private float _lastDashTime;

    private async UniTask PlayerDash(Vector2 direction)
    {
        _isCanDash = false;
        _isPlayerInvincible = true;
        _currentDashTime = _dashTime; // Reset the dash timer.

        _isDash = true;
        while (_currentDashTime > 0f)
        {
            _currentDashTime -= Time.deltaTime; // Lower the dash timer each frame.

            _player.transform.Translate(direction * _dashSpeed); // Dash in the direction that was held down.
            await UniTask.Yield();
        }

        _isDash = false;
        _isPlayerInvincible = false;
    }

    private async UniTask Pause()
    {
        Game.IsPause = true;
        _pauseMenu.gameObject.SetActive(true);
        _pauseMenu.alpha = 0f;
        await _pauseMenu.FadeIn();
    }

    private async UniTask UnPause()
    {
        await _pauseMenu.FadeOut();
        _pauseMenu.gameObject.SetActive(false);
        Game.IsPause = false;
    }

    private void SetMusic(float obj)
    {
        Game.MusicValue = obj;
    }

    private void SetSound(float obj)
    {
        Game.SoundValue = obj;
    }
}