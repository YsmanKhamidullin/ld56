using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
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

        UpdateInput();
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

    private void UpdateInput()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        if (horizontal != 0 || vertical != 0)
        {
            horizontal *= _playerSpeed * Time.deltaTime;
            vertical *= _playerSpeed * Time.deltaTime;
            _player.transform.position += new Vector3(horizontal, 0, vertical);
        }
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