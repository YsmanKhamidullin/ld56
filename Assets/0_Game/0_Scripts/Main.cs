using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

/*

raindance74: А будет отдаление при деше?
raindance74: А потом после него приближение
Diccuric_Sigeon: у него же много лап. добавь ещё катан

*/

/// <summary>
/// All game in one class
/// </summary>
public class Main : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField]
    private CanvasGroup _pauseMenu;

    [SerializeField]
    private CanvasGroup _gameStartMenu;

    [SerializeField]
    private CanvasGroup _gameOverMenu;

    [SerializeField]
    private CanvasGroup _levelCompleteMenu;

    [SerializeField]
    private CanvasGroup _gameCompleteMenu;

    [SerializeField]
    private TextMeshProUGUI _levelCompleteCounterLabel;

    [SerializeField]
    private TextMeshProUGUI _gameCompleteRoninLabel;

    [SerializeField]
    private TextMeshProUGUI _gameCompleteThanksLabel;

    [SerializeField]
    private Button _gameStartButton;

    [SerializeField]
    private Button _resumeGameButton;

    [SerializeField]
    private Button _restartLevelButton;

    [SerializeField]
    private Button _restartGameButton;

    [SerializeField]
    private Button _restartGameAfterCompleteButton;

    [SerializeField]
    private CustomSlider _soundSlider;

    [SerializeField]
    private CustomSlider _musicSlider;

    [Header("Enemies")]
    private List<Ant> _enemies;

    private List<Ant> _notEnemies;

    [Header("Player")]
    [SerializeField]
    private Ant _player;

    [SerializeField]
    private float _playerRotateDuration = 2f;

    [Header("Dash")]
    [SerializeField]
    private float _dashTime = 0.2f;

    [SerializeField]
    private float _dashSpeed = 1f;

    private bool _isCanDash = true;
    private bool _isPlayerInvincible;
    private bool _isPlayerDash;
    private float _currentDashTime = 0.2f;

    [Header("Debug")]
    [SerializeField]
    private TextMeshProUGUI _debugLabel;

    private void Start()
    {
        Application.targetFrameRate = 60;
        UnityEngine.Random.InitState(42);
        DOTween.Init().SetCapacity(200, 50);

        Game.IsPause = true;
        InitializeCanvas();
        InitializePlayer();
        InitializeEnemies();
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }

    #region GameLoop

    private async UniTask StartGame()
    {
        Game.IsPause = true;
        await _gameStartMenu.FadeOut();
        _gameStartMenu.gameObject.SetActive(false);
        Game.IsPause = false;
    }

    private void InitializeEnemies()
    {
        _enemies = FindObjectsByType<Ant>(FindObjectsSortMode.None)
            .Where(t => t.gameObject.layer == LayerMask.NameToLayer("Enemy")).ToList();
        _notEnemies = FindObjectsByType<Ant>(FindObjectsSortMode.None)
            .Where(t => t.gameObject.layer != LayerMask.NameToLayer("Player")).Except(_enemies).ToList();
    }


    private void InitializePlayer()
    {
        _player.gameObject.SetActive(true);
    }

    private void InitializeCanvas()
    {
        _gameStartButton.onClick.AddListener(() => _ = StartGame());
        _resumeGameButton.onClick.AddListener(() => _ = UnPause());
        _restartLevelButton.onClick.AddListener(RestartLevel);
        _restartGameButton.onClick.AddListener(RestartGame);
        _restartGameAfterCompleteButton.onClick.AddListener(RestartGameAfterComplete);
        _soundSlider.SetValueWithoutNotify(Game.SoundValue);
        _musicSlider.SetValueWithoutNotify(Game.MusicValue);

        _soundSlider.OnValueChanged += SetSound;
        _musicSlider.OnValueChanged += SetMusic;
        _gameStartMenu.gameObject.SetActive(true);
        _gameOverMenu.gameObject.SetActive(false);
        _pauseMenu.gameObject.SetActive(false);
        _levelCompleteMenu.gameObject.SetActive(false);
        _gameCompleteMenu.gameObject.SetActive(false);
    }

    private void RestartGameAfterComplete()
    {
        Game.RestartAfterComplete++;
        RestartGame();
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        UpdatePauseInput();
        if (Game.IsPause)
        {
            return;
        }

        UpdatePlayer();
        UpdateEnemies();
    }


    private async UniTask GameOver()
    {
        Game.IsPause = true;
        _gameOverMenu.alpha = 0f;
        _gameOverMenu.gameObject.SetActive(true);
        await _gameOverMenu.FadeIn(1f);
    }

    private async UniTask TryCompleteLevel()
    {
        if (_enemies.Count == 0)
        {
            DOTween.KillAll();

            bool isLastScene = SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1;
            if (isLastScene)
            {
                await LevelComplete();
            }
            else
            {
                _ = GameComplete();
            }
        }
    }

    private async UniTask LevelComplete()
    {
        Game.IsPause = true;
        _levelCompleteMenu.alpha = 0f;
        _levelCompleteMenu.gameObject.SetActive(true);
        await _levelCompleteMenu.FadeIn(1f);
        _levelCompleteCounterLabel.text = "Next Level is in 3...";
        await UniTask.Delay(1000);
        _levelCompleteCounterLabel.text = "Next Level is in 2...";
        await UniTask.Delay(1000);
        _levelCompleteCounterLabel.text = "Next Level is in 1...";
        await UniTask.Delay(1000);
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    [SerializeField]
    private AnimationCurve _labelRoninEase;

    private async UniTask GameComplete()
    {
        Game.IsPause = true;
        _gameCompleteMenu.alpha = 0f;
        _gameCompleteMenu.gameObject.SetActive(true);
        await _gameCompleteMenu.FadeIn(1f);
        _gameCompleteRoninLabel.color.ZeroAlpha();
        _gameCompleteRoninLabel.DOFade(1f, 10f).SetEase(_labelRoninEase);
        _gameCompleteThanksLabel.text = "Thanks for playing!";
        await UniTask.Delay(1000);
        _gameCompleteThanksLabel.text = "It was my first game jam!";
        await UniTask.Delay(1000);
        _gameCompleteThanksLabel.text = "And remember...";
        await UniTask.Delay(1000);
        _gameCompleteThanksLabel.text = "";
        _restartGameAfterCompleteButton.gameObject.SetActive(true);
    }

    private UniTask _pauseTask;

    private void UpdatePauseInput()
    {
        if (Game.IsPause && !Game.IsByInputPause)
        {
            return;
        }

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

    private async UniTask Pause()
    {
        Game.IsPause = true;
        Game.IsByInputPause = true;
        _pauseMenu.gameObject.SetActive(true);
        _pauseMenu.alpha = 0f;
        await _pauseMenu.FadeIn();
    }

    private async UniTask UnPause()
    {
        await _pauseMenu.FadeOut();
        _pauseMenu.gameObject.SetActive(false);
        Game.IsPause = false;
        Game.IsByInputPause = false;
    }

    #endregion

    #region Enemy

    private void UpdateEnemies()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.transform.IsNear(_player.transform, enemy.AttackRange))
            {
                enemy.rb.linearVelocity = Vector2.zero;
                bool isCanAttack = CanAttack(enemy) && !_isPlayerDash;
                if (isCanAttack)
                {
                    EnemyAttack(enemy, _player);
                }
            }
            else
            {
                var dir = (_player.transform.position - enemy.transform.position).normalized;
                var velocity = dir * enemy.MoveSpeed;
                enemy.rb.linearVelocity = velocity;
                enemy.transform.up = dir;
            }
        }
    }

    private void EnemyAttack(Ant enemy, Ant player)
    {
        enemy.transform.DOKill(true);
        enemy.transform.DOPunchScale(1.025f * Vector3.one, 0.15f, 1, 0.1f);
        enemy.LastAttackTime = Time.time;
        TryDealDamageToPlayer(enemy.Damage);
    }

    #endregion

    #region Player

    private void UpdatePlayer()
    {
        _ = UpdateDash();
        if (!_isPlayerDash)
        {
            UpdatePlayerMove();
            UpdatePlayerRotation();
        }
    }

    private void TryDealDamageToPlayer(int damage)
    {
        if (_isPlayerInvincible)
        {
            return;
        }

        _player.Health -= damage;
        _player.transform.DOKill(true);
        _player.transform.DOPunchScale(1.025f * Vector3.one, 0.15f, 1, 0.1f);
        CheckPlayerHealth();
    }

    private void CheckPlayerHealth()
    {
        if (_player.Health <= 0)
        {
            _ = GameOver();
        }
    }


    private void UpdatePlayerRotation()
    {
        var _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0f; // Set z to zero for 2D mode
        var direction = new Vector2(_mousePos.x - _player.transform.position.x,
            _mousePos.y - _player.transform.position.y);
        float rotationZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        if (!DOTween.IsTweening(_player.transform))
        {
            _player.transform.DORotate(new Vector3(0f, 0f, rotationZ), _playerRotateDuration);
        }
    }

    private void UpdatePlayerMove()
    {
        var horizontal = Input.GetAxisRaw("Horizontal");
        var vertical = Input.GetAxisRaw("Vertical");
        if (horizontal != 0 || vertical != 0)
        {
            horizontal *= _player.MoveSpeed;
            vertical *= _player.MoveSpeed;
            _player.rb.linearVelocity = new Vector3(horizontal, vertical, 0);
        }
        else
        {
            _player.rb.linearVelocity = Vector2.zero;
        }
    }

    private async UniTask UpdateDash()
    {
        bool isDash = Input.GetMouseButtonDown(0);
        if (isDash && _isCanDash)
        {
            await PlayerDash(_player.transform.up);
            _player.LastAttackTime = Time.time;
        }

        if (_player.LastAttackTime > 0)
        {
            _isCanDash = CanAttack(_player);
        }
    }

    private async UniTask PlayerDash(Vector2 direction)
    {
        _isCanDash = false;
        _isPlayerInvincible = true;
        _currentDashTime = _dashTime; // Reset the dash timer.

        _isPlayerDash = true;
        while (_currentDashTime > 0f)
        {
            _currentDashTime -= Time.deltaTime; // Lower the dash timer each frame.

            _player.rb.linearVelocity = direction * _dashSpeed;
            TryPlayerAttackNearEnemies();
            await UniTask.Yield();
        }

        _player.rb.linearVelocity = Vector2.zero;
        _isPlayerDash = false;
        _isPlayerInvincible = false;
    }

    private void TryPlayerAttackNearEnemies()
    {
        for (var i = 0; i < _enemies.Count; i++)
        {
            var enemy = _enemies[i];
            if (_player.transform.IsNear(enemy.transform, _player.AttackRange))
            {
                _ = PlayerAttack(enemy, _enemies);
            }
        }

        for (int i = 0; i < _notEnemies.Count; i++)
        {
            var notEnemy = _notEnemies[i];
            if (_player.transform.IsNear(notEnemy.transform, _player.AttackRange))
            {
                _ = PlayerAttack(notEnemy, _notEnemies);
                Game.AttackNotEnemy++;
            }
        }
    }

    private async UniTask PlayerAttack(Ant target, List<Ant> targetArray)
    {
        target.transform.DOPunchScale(1.015f * Vector3.one, 0.15f, 1, 0.1f).SetEase(Ease.Flash);
        target.Health -= _player.Damage;
        if (target.Health <= 0)
        {
            targetArray.Remove(target);
            await target.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).ToUniTask();
            Destroy(target.gameObject);
            _ = TryCompleteLevel();
        }
    }

    #endregion

    private bool CanAttack(Ant ant)
    {
        return Time.time - ant.LastAttackTime > ant.AttackDelay;
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