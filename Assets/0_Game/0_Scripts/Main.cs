using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Core.Enums;
using DG.Tweening.Plugins.Options;
using TMPEffects.Components;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
    [SerializeField]
    private CinemachineFollow _camFollow;

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
    private TMPWriter _gameCompleteThanksLabel;

    [SerializeField]
    private Button _gameStartEasyButton;

    [SerializeField]
    private Button _gameStartNormalButton;

    [SerializeField]
    private Button _gameStartHardButton;

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

    [SerializeField]
    private List<Image> _playerHealthImages;

    [SerializeField]
    private float _projectileSpeed;

    [Header("Enemies")]
    [SerializeField]
    private Transform _startUpdateEnemeiesPoint;

    private List<Ant> _meleeEnemies;
    private List<Ant> _rangeEnemies;
    private List<Ant> _bossEnemies;
    private List<Ant> _notEnemies;
    private bool _isUpateEnemies;

    [Header("Player")]
    [SerializeField]
    private Slider _dashSlider;

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
        UpdateHUD();
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            _ = StartGame(Game.Difficulty);
        }
        else
        {
        }
    }

    private void OnDestroy()
    {
        DOTween.KillAll();
    }

    #region GameLoop

    private async UniTask StartGame(Difficulty difficulty)
    {
        Game.Difficulty = difficulty;
        switch (difficulty)
        {
            case Difficulty.Easy:
                _player.Health = 5;
                _player.AttackRange *= 1.2f;
                break;
            case Difficulty.Normal:
                _player.Health = 3;
                break;
            case Difficulty.Hard:
                _player.Health = 1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null);
        }

        Game.IsPause = true;
        await _gameStartMenu.FadeOut();
        _gameStartMenu.gameObject.SetActive(false);
        Game.IsPause = false;
    }

    private void InitializeEnemies()
    {
        var allAnts = FindObjectsByType<Ant>(FindObjectsSortMode.None);
        _meleeEnemies = allAnts.Where(t => t.gameObject.CompareTag("EnemyMelee")).ToList();
        _rangeEnemies = allAnts.Where(t => t.gameObject.CompareTag("EnemyRange")).ToList();
        _notEnemies = allAnts.Where(t => t.gameObject.CompareTag("NotEnemy")).ToList();
    }


    private void InitializePlayer()
    {
        _player.gameObject.SetActive(true);
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    private void InitializeCanvas()
    {
        _gameStartMenu.gameObject.SetActive(true);
        _gameOverMenu.gameObject.SetActive(true);
        _pauseMenu.gameObject.SetActive(true);
        _levelCompleteMenu.gameObject.SetActive(true);
        _gameCompleteMenu.gameObject.SetActive(true);

        _gameStartEasyButton.onClick.AddListener(() => _ = StartGame(Difficulty.Easy));
        _gameStartNormalButton.onClick.AddListener(() => _ = StartGame(Difficulty.Normal));
        _gameStartHardButton.onClick.AddListener(() => _ = StartGame(Difficulty.Hard));
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
        UpdateHUD();
        TryStartUpdateEnemies();
        if (_isUpateEnemies)
        {
            UpdateEnemies();
        }

        UpdateEnemyProjectiles();
    }

    private void TryStartUpdateEnemies()
    {
        if (_isUpateEnemies)
        {
            return;
        }

        if (_player.transform.IsNear(_startUpdateEnemeiesPoint, 2f))
        {
            _isUpateEnemies = true;
        }
    }

    private void UpdateHUD()
    {
        for (int i = 0; i < _playerHealthImages.Count; i++)
        {
            _playerHealthImages[i].gameObject.SetActive(false);
        }

        for (int i = _player.Health - 1; i >= 0; i--)
        {
            _playerHealthImages[i].gameObject.SetActive(true);
        }
    }


    private async UniTask GameOver()
    {
        Game.IsPause = true;
        _gameOverMenu.alpha = 0f;
        _gameOverMenu.gameObject.SetActive(true);
        await _gameOverMenu.FadeIn(1f);
    }

    private async UniTask<bool> TryCompleteLevel()
    {
        if (_meleeEnemies.Count == 0 && _rangeEnemies.Count == 0)
        {
            DOTween.KillAll();

            bool isLastScene = SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1;
            if (isLastScene)
            {
                _ = GameComplete();
            }
            else
            {
                await LevelComplete();
            }

            return true;
        }

        return false;
    }

    private async UniTask LevelComplete()
    {
        _player.rb.linearVelocity = Vector2.zero;
        Game.IsPause = true;
        _levelCompleteMenu.alpha = 0f;
        _levelCompleteCounterLabel.text = "";
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

    private async UniTask GameComplete()
    {
        Game.IsPause = true;
        _gameCompleteMenu.alpha = 0f;
        _gameCompleteMenu.gameObject.SetActive(true);
        _gameCompleteThanksLabel.SetText("");
        _gameCompleteRoninLabel.color = _gameCompleteRoninLabel.color.ZeroAlpha();
        await _gameCompleteMenu.FadeIn(1f);

        _gameCompleteThanksLabel.SetText("You completed your journey. Congratulations!");
        _gameCompleteThanksLabel.RestartWriter();

        await UniTask.WaitUntil(() => !_gameCompleteThanksLabel.IsWriting);
        await UniTask.Delay(1500);

        _gameCompleteThanksLabel.SetText("Thanks for playing!");
        _gameCompleteThanksLabel.RestartWriter();

        await UniTask.WaitUntil(() => !_gameCompleteThanksLabel.IsWriting);
        await UniTask.Delay(1500);

        _gameCompleteThanksLabel.SetText("And remember...");
        _gameCompleteThanksLabel.RestartWriter();

        await UniTask.WaitUntil(() => !_gameCompleteThanksLabel.IsWriting);
        _gameCompleteRoninLabel.DOFade(1f, 2f).SetEase(Ease.InExpo);
        await UniTask.Delay(1000);
        _gameCompleteThanksLabel.SetText("");
        await UniTask.Delay(2000);
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
        foreach (var ant in _meleeEnemies)
        {
            if (Vector2.Distance(ant.transform.position, _player.transform.position) > 20)
            {
                continue;
            }

            if (ant.transform.IsNear(_player.transform, ant.AttackRange))
            {
                ant.rb.linearVelocity = Vector2.zero;
                ant.Animator.SetBool(Walk, false);
                bool isCanAttack = IsEnemyCanAttack(ant) && !_isPlayerDash;
                if (isCanAttack)
                {
                    MeleeEnemyAttack(ant, _player);
                }
            }
            else // Move
            {
                var dir = (_player.transform.position - ant.transform.position).normalized;
                var velocity = dir * ant.MoveSpeed;
                ant.Animator.SetBool(Walk, true);
                ant.rb.linearVelocity = velocity;
                ant.transform.up = dir;
            }
        }

        foreach (var ant in _rangeEnemies)
        {
            if (Vector2.Distance(ant.transform.position, _player.transform.position) > 20)
            {
                continue;
            }

            bool isCanAttack = IsEnemyCanAttack(ant);
            if (isCanAttack)
            {
                RangeEnemyAttack(ant, _player);
            }
        }
    }

    private void MeleeEnemyAttack(Ant enemy, Ant player)
    {
        enemy.transform.DOKill(true);
        enemy.transform.DOPunchScale(1.025f * Vector3.one, 0.15f, 1, 0.1f);
        enemy.LastAttackTime = Time.time;
        TryDealDamageToPlayer(enemy.Damage);
    }

    private void RangeEnemyAttack(Ant ant, Ant player)
    {
        var projectile = Instantiate(ant.ProjectilePrefab, ant.transform.position, Quaternion.identity);
        projectile.up = (_player.transform.position - ant.transform.position).normalized;
        AddProjectileAndDestroyAfterTime(projectile);
        ant.LastAttackTime = Time.time;
    }

    private List<Transform> _projectiles = new();
    private float _projectileAttackRange = 1f;
    private static readonly int Walk = Animator.StringToHash("walk");

    private void AddProjectileAndDestroyAfterTime(Transform projectile)
    {
        _projectiles.Add(projectile);
        projectile.DOScale(Vector3.zero, 3).SetEase(Ease.InBack).OnComplete(() =>
        {
            _projectiles.Remove(projectile);
            Destroy(projectile.gameObject, 3f);
        });
    }

    private void UpdateEnemyProjectiles()
    {
        for (var i = 0; i < _projectiles.Count; i++)
        {
            var projectile = _projectiles[i];
            if (!projectile.gameObject.activeSelf)
            {
                continue;
            }

            projectile.position += projectile.up * (_projectileSpeed * Time.deltaTime);
            if (projectile.IsNear(_player.transform, projectile.localScale.x / 2f))
            {
                TryDealDamageToPlayer(1);
                projectile.gameObject.SetActive(false);
            }
        }
    }

    private bool IsEnemyCanAttack(Ant ant)
    {
        return Time.time - ant.LastAttackTime > ant.AttackDelay + Random.Range(0, 0.5f);
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
        _isPlayerInvincible = true;
        DOTween.Sequence().InsertCallback(0.1f, () => { _isPlayerInvincible = false; });
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
            _player.Animator.SetBool(Walk, true);
        }
        else
        {
            _player.rb.linearVelocity = Vector2.zero;
            _player.Animator.SetBool(Walk, false);
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
            _dashSlider.value = Mathf.Clamp01((Time.time - _player.LastAttackTime) / _player.AttackDelay);
            _isCanDash = Time.time - _player.LastAttackTime > _player.AttackDelay;
        }
    }

    private async UniTask PlayerDash(Vector2 direction)
    {
        _isCanDash = false;
        _isPlayerInvincible = true;
        _currentDashTime = _dashTime; // Reset the dash timer.

        _isPlayerDash = true;

        var startZ = _camFollow.FollowOffset.z;
        _player.Animator.Play("dash");
        SmallZoom(startZ);
        while (_currentDashTime > 0f)
        {
            _currentDashTime -= Time.deltaTime; // Lower the dash timer each frame.

            _player.rb.linearVelocity = direction * _dashSpeed;
            TryPlayerAttackNearEnemies();
            TryDestroyNearProjectiles();
            await UniTask.Yield();
        }

        UnZoom(startZ);
        _player.rb.linearVelocity = Vector2.zero;
        _isPlayerDash = false;
        _isPlayerInvincible = false;
    }

    private void SmallZoom(float startZ)
    {
        DOTween.To(() => _camFollow.FollowOffset.z, x => _camFollow.FollowOffset.z = x, startZ + 1f, 0.3f)
            .SetEase(Ease.Flash);
    }

    private void UnZoom(float startZ)
    {
        DOTween.To(() => _camFollow.FollowOffset.z, x => _camFollow.FollowOffset.z = x, startZ, 0.2f)
            .SetEase(Ease.Flash);
    }

    private void TryDestroyNearProjectiles()
    {
        for (var i = 0; i < _projectiles.Count; i++)
        {
            var projectile = _projectiles[i];
            if (_player.transform.IsNear(projectile, _player.AttackRange))
            {
                projectile.gameObject.SetActive(false);
            }
        }
    }

    private void TryPlayerAttackNearEnemies()
    {
        for (var i = 0; i < _meleeEnemies.Count; i++)
        {
            var enemy = _meleeEnemies[i];
            if (_player.transform.IsNear(enemy.transform, _player.AttackRange))
            {
                _ = PlayerAttack(enemy, _meleeEnemies);
            }
        }

        for (var i = 0; i < _rangeEnemies.Count; i++)
        {
            var enemy = _rangeEnemies[i];
            if (_player.transform.IsNear(enemy.transform, _player.AttackRange))
            {
                _ = PlayerAttack(enemy, _rangeEnemies);
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
        bool isTargetInvinsible = Time.time - target.LastGetDamageTime <= target.InvinsibleTime;
        if (isTargetInvinsible)
        {
            return;
        }

        Time.timeScale = 0.6f;

        target.LastGetDamageTime = Time.time;
        target.transform.DOPunchScale(1.015f * Vector3.one, 0.15f, 1, 0.1f).SetEase(Ease.Flash);
        target.Health -= _player.Damage;
        if (target.HitImpactSprite != null)
        {
            target.HitImpactSprite.DOFade(1f, 0.1f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo);
        }

        SmallShake();
        if (target.Health <= 0)
        {
            targetArray.Remove(target);
            await target.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).ToUniTask();
            Destroy(target.gameObject);
            var isCompleted = await TryCompleteLevel();
            if (!isCompleted)
            {
                SmallPauseGame();
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
        else
        {
            Time.timeScale = 1f;
        }
    }

    private void SmallPauseGame()
    {
        DOTween.Sequence().InsertCallback(0.07f, () => { Time.timeScale = 1f; }).SetUpdate(true);
        Time.timeScale = 0f;
    }

    #endregion

    [NaughtyAttributes.Button]
    private void SmallShake()
    {
        var dool = E.DynamicContainer.DynamicParent;
        dool.DOKill(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(dool.DOShakePosition(0.2f, .8f));
        sequence.OnUpdate(() =>
        {
            _camFollow.FollowOffset.x = dool.position.x;
            _camFollow.FollowOffset.y = dool.position.y;
        });
        sequence.OnComplete(() =>
        {
            _camFollow.FollowOffset.x = 0f;
            _camFollow.FollowOffset.y = 0f;
        });
    }

    private void LongShake()
    {
        var dool = E.DynamicContainer.DynamicParent;
        dool.DOKill(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(dool.DOShakePosition(0.3f, 1.5f));
        sequence.OnUpdate(() =>
        {
            _camFollow.FollowOffset.x = dool.position.x;
            _camFollow.FollowOffset.y = dool.position.y;
        });
        sequence.OnComplete(() =>
        {
            _camFollow.FollowOffset.x = 0f;
            _camFollow.FollowOffset.y = 0f;
        });
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