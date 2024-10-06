using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPEffects.Components;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
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

    [SerializeField]
    private SpriteRenderer _bg;

    [Header("Canvas")]
    [SerializeField]
    private CanvasGroup _upgradeWindow;

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
    private TextMeshProUGUI _comboCounterLabel;

    [SerializeField]
    private TextMeshProUGUI _levelCompleteCounterLabel;

    [SerializeField]
    private TextMeshProUGUI _gameCompleteRoninLabel;

    [SerializeField]
    private TextMeshProUGUI _atkLabel;

    [SerializeField]
    private TextMeshProUGUI _sizeLabel;

    [SerializeField]
    private TextMeshProUGUI _dashLabel;

    [SerializeField]
    private TextMeshProUGUI _rangeLabel;

    [SerializeField]
    private TextMeshProUGUI _speedLabel;

    [SerializeField]
    private TMPWriter _gameCompleteThanksLabel;

    [SerializeField]
    private TMP_Dropdown _selectMusicDropdownPause;

    [SerializeField]
    private Button _gameStartEasyButton;

    [SerializeField]
    private Button _gameStartNormalButton;

    [SerializeField]
    private Button _gameStartHardButton;

    [SerializeField]
    private Button _gameStartOneLifeButton;

    [SerializeField]
    private Button _resumeGameButton;

    [SerializeField]
    private Button _restartLevelButton;

    [SerializeField]
    private Button _restartGameButton;

    [SerializeField]
    private Button _restartGameAfterCompleteButton;

    [SerializeField]
    private Button _upgradeEnlargeButton;

    [SerializeField]
    private Button _upgradeShrinkButton;

    [SerializeField]
    private CustomSlider _soundSlider;

    [SerializeField]
    private CustomSlider _musicSlider;

    [SerializeField]
    private GameObject _anime;

    [SerializeField]
    private List<Image> _playerHealthImages;


    [SerializeField]
    private SpriteRenderer _trailPrefab;

    [SerializeField]
    private float _projectileSpeed;

    [Header("Enemies")]
    [SerializeField]
    private Ant _eggPrefab;

    [SerializeField]
    private Transform _startUpdateEnemeiesPoint;

    [SerializeField]
    private bool _shouldKillAll = false;

    private List<Ant> _meleeEnemies;
    private List<Ant> _rangeEnemies;
    private List<Ant> _bossEnemies;
    private List<Ant> _eggEnemies;
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


    [Header("SFX")]
    [SerializeField]
    private AudioSource _enemyDieSfx;

    [SerializeField]
    private AudioSource _dashSfx;

    [SerializeField]
    private AudioSource _levelFailSfx;

    [SerializeField]
    private AudioSource _levelUpSfx;

    [Header("Debug")]
    [SerializeField]
    private TextMeshProUGUI _debugLabel;

    private int _killStreak;

    private void Start()
    {
        Application.targetFrameRate = 60;
        UnityEngine.Random.InitState(42);
        DOTween.Init().SetCapacity(200, 50);

        Game.IsPause = true;

        bool isInGameStart = SceneManager.GetActiveScene().buildIndex == 0;
        if (isInGameStart)
        {
            Game.UpgradeSize = 0;
            Game.UpgradeAttack = 0;
            // Game.UpgradeHealth = 0;
            Game.UpgradeAttackRange = 0;
            Game.UpgradeDash = 0;
            Game.UpgradeSpeed = 0;
        }

        LoadData();
        InitializeCanvas();
        InitializePlayer();
        InitializeEnemies();
        UpdateHUD();
        if (!isInGameStart)
        {
            _ = StartGame(Game.Difficulty);
        }
    }

    private void LoadData()
    {
        var musicNumber = PlayerPrefs.GetInt("MusicNumber", 1);
        ChangeMusicBg(musicNumber);
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
                _player.Health = 12;
                _player.AttackRange *= 1.5f;
                _player.Damage += 2;
                break;
            case Difficulty.Normal:
                _player.Health = 6;
                _player.AttackRange *= 1.2f;
                break;
            case Difficulty.Hard:
                _player.Health = 3;
                break;
            case Difficulty.OneLife:
                _player.Health = 1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null);
        }

        ApplyPlayerUpgrades();

        Game.IsPause = true;
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            await _gameStartMenu.FadeOut();
        }

        _gameStartMenu.gameObject.SetActive(false);
        Game.IsPause = false;
    }

    private void InitializeEnemies()
    {
        var allAnts = FindObjectsByType<Ant>(FindObjectsSortMode.None);
        _meleeEnemies = allAnts.Where(t => t.gameObject.CompareTag("EnemyMelee")).ToList();
        _rangeEnemies = allAnts.Where(t => t.gameObject.CompareTag("EnemyRange")).ToList();
        _notEnemies = allAnts.Where(t => t.gameObject.CompareTag("NotEnemy")).ToList();
        _eggEnemies = allAnts.Where(t => t.gameObject.CompareTag("Egg")).ToList();
        _bossEnemies = allAnts.Where(t => t.gameObject.CompareTag("EnemyBoss")).ToList();
    }


    private void InitializePlayer()
    {
        _player.gameObject.SetActive(true);
    }

    private void ApplyPlayerUpgrades()
    {
        var newScale = _player.transform.localScale.x + 0.075f * Game.UpgradeSize;
        var clapedScale = Math.Clamp(newScale, 0.5f, 2.8f);
        _player.transform.localScale = new Vector3(clapedScale, clapedScale, 1);
        _player.Damage = Mathf.Clamp(_player.Damage + Game.UpgradeAttack, 1, 15);
        // _player.Health += Game.UpgradeHealth;
        _player.AttackRange = Mathf.Clamp(_player.AttackRange + (0.05f * Game.UpgradeAttackRange), 0.7f, 2f);
        _player.AttackDelay -= 0.005f * Game.UpgradeDash;
        _player.MoveSpeed += 0.3f * Game.UpgradeSpeed;

        _camFollow.FollowOffset.z -= 0.075f * Game.UpgradeSize;
    }

    private void InitializeCanvas()
    {
        var bgColor = _bg.color;
        bgColor.a = Math.Clamp(CurrentLevelN() / 10f, 0.2f, 1f);
        _bg.color = bgColor;
        var curEuler = _bg.transform.rotation.eulerAngles;
        curEuler.z = Random.rotation.z;
        _bg.transform.rotation = Quaternion.Euler(curEuler);

        _gameStartMenu.gameObject.SetActive(true);
        _gameOverMenu.gameObject.SetActive(true);
        _pauseMenu.gameObject.SetActive(true);
        _levelCompleteMenu.gameObject.SetActive(true);
        _gameCompleteMenu.gameObject.SetActive(true);


        _gameStartEasyButton.onClick.AddListener(() => _ = StartGame(Difficulty.Easy));
        _gameStartNormalButton.onClick.AddListener(() => _ = StartGame(Difficulty.Normal));
        _gameStartHardButton.onClick.AddListener(() => _ = StartGame(Difficulty.Hard));
        _gameStartOneLifeButton.onClick.AddListener(() => _ = StartGame(Difficulty.OneLife));
        _resumeGameButton.onClick.AddListener(() => _ = UnPause());
        _restartLevelButton.onClick.AddListener(RestartLevel);
        _restartGameButton.onClick.AddListener(RestartGame);
        _restartGameAfterCompleteButton.onClick.AddListener(RestartGameAfterComplete);

        _upgradeEnlargeButton.onClick.AddListener(SelectEnlargeUpgrade);
        _upgradeShrinkButton.onClick.AddListener(SelectShrinkUpgrade);

        SetSound(Game.SoundValue);
        SetMusic(Game.MusicValue);
        _soundSlider.SetValueWithoutNotify(Game.SoundValue);
        _musicSlider.SetValueWithoutNotify(Game.MusicValue);

        var musicNumber = PlayerPrefs.GetInt("MusicNumber", 1);
        _selectMusicDropdownPause.SetValueWithoutNotify(musicNumber - 1);
        _selectMusicDropdownPause.onValueChanged.AddListener(ChangeMusicBg);

        _soundSlider.OnValueChanged += SetSound;
        _musicSlider.OnValueChanged += SetMusic;
        _gameStartMenu.gameObject.SetActive(true);
        _gameOverMenu.gameObject.SetActive(false);
        _pauseMenu.gameObject.SetActive(false);
        _levelCompleteMenu.gameObject.SetActive(false);
        _gameCompleteMenu.gameObject.SetActive(false);
    }

    private int CurrentLevelN()
    {
        return SceneManager.GetActiveScene().buildIndex + 1;
    }

    private void SelectEnlargeUpgrade()
    {
        Game.UpgradeSize += 1;
        Game.UpgradeAttack += 1;
        // Game.UpgradeHealth += 1;
        Game.UpgradeAttackRange += 1;
        Game.UpgradeDash -= 1;
        _levelUpSfx.Play();
        _ = LevelComplete();
    }

    private void SelectShrinkUpgrade()
    {
        Game.UpgradeSize -= 1;
        Game.UpgradeSpeed += 1;
        Game.UpgradeDash += 1;
        Game.UpgradeAttackRange -= 1;
        _levelUpSfx.Play();
        _ = LevelComplete();
    }

    private void ChangeMusicBg(int arg0)
    {
        PlayerPrefs.SetInt("MusicNumber", arg0);
        MusicDontDestroy.Instance.ChangeTrack(arg0);
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

        UpdateNotEnemies();
        UpdateEnemyProjectiles();
    }

    private void TryStartUpdateEnemies()
    {
        if (_isUpateEnemies)
        {
            return;
        }

        if (_player.transform.IsNear(_startUpdateEnemeiesPoint, 3.5f))
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
        DropMovement();
        Time.timeScale = 1f;
        _levelFailSfx.Play();

        _gameOverMenu.alpha = 0f;
        _gameOverMenu.gameObject.SetActive(true);
        await _gameOverMenu.FadeIn(1f);
    }

    private async UniTask<bool> TryCompleteLevel()
    {
        if (_meleeEnemies.Count == 0 && _rangeEnemies.Count == 0 && _eggEnemies.Count == 0 && _bossEnemies.Count == 0)
        {
            if (_shouldKillAll && _notEnemies.Count != 0)
            {
                return false;
            }

            DOTween.KillAll();

            bool isLastScene = SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1;
            if (isLastScene)
            {
                _ = GameComplete();
            }
            else
            {
                await ShowUpgradeMenu();
            }

            return true;
        }

        return false;
    }

    private async UniTask ShowUpgradeMenu()
    {
        Game.IsPause = true;
        DropMovement();
        Time.timeScale = 1f;
        _upgradeWindow.alpha = 0f;
        _upgradeWindow.gameObject.SetActive(true);
        _upgradeEnlargeButton.interactable = false;
        _upgradeShrinkButton.interactable = false;
        await _upgradeWindow.FadeIn(0.6f);
        _upgradeEnlargeButton.interactable = true;
        _upgradeShrinkButton.interactable = true;
    }

    private async UniTask LevelComplete()
    {
        _levelCompleteMenu.alpha = 0f;
        _levelCompleteCounterLabel.text = "";
        _levelCompleteMenu.gameObject.SetActive(true);
        await _levelCompleteMenu.FadeIn(.5f);
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

        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) &&
            _pauseTask.Status is UniTaskStatus.Succeeded or UniTaskStatus.Pending)
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

        DropMovement();

        _pauseMenu.gameObject.SetActive(true);

        _atkLabel.text = "ATK: " + _player.Damage;
        _sizeLabel.text = "SIZE: " + Math.Round(_player.transform.localScale.x, 2);
        _dashLabel.text = "DASH: " + Math.Round(_player.AttackDelay, 2);
        _rangeLabel.text = "ATK RANGE: " + Math.Round(_player.AttackRange, 2);
        _speedLabel.text = "SPEED: " + Math.Round(_player.MoveSpeed, 2);
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

    private void DropMovement()
    {
        _player.rb.linearVelocity = Vector2.zero;

        for (var i = 0; i < _meleeEnemies.Count; i++)
        {
            var ant = _meleeEnemies[i];
            ant.rb.linearVelocity = Vector2.zero;
            ant.Animator.SetBool(Walk, false);
        }

        for (var i = 0; i < _rangeEnemies.Count; i++)
        {
            var ant = _rangeEnemies[i];
            ant.rb.linearVelocity = Vector2.zero;
        }

        for (var i = 0; i < _eggEnemies.Count; i++)
        {
            var ant = _eggEnemies[i];
            ant.rb.linearVelocity = Vector2.zero;
        }

        for (var i = 0; i < _bossEnemies.Count; i++)
        {
            var ant = _bossEnemies[i];
            ant.rb.linearVelocity = Vector2.zero;
            if (ant.Animator != null)
            {
                ant.Animator.SetBool(Walk, false);
            }
        }
    }

    private void UpdateNotEnemies()
    {
        foreach (var ant in _notEnemies)
        {
            var dir = (_player.transform.position - ant.transform.position).normalized;
            ant.transform.up = dir;
        }
    }

    private void UpdateEnemies()
    {
        for (var i = 0; i < _meleeEnemies.Count; i++)
        {
            var ant = _meleeEnemies[i];
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

        for (var i = 0; i < _rangeEnemies.Count; i++)
        {
            var ant = _rangeEnemies[i];
            if (Vector2.Distance(ant.transform.position, _player.transform.position) > 20)
            {
                continue;
            }

            if (ant.MoveSpeed > 0)
            {
                var dir = (_player.transform.position - ant.transform.position).normalized;
                var velocity = dir * ant.MoveSpeed;
                ant.rb.linearVelocity = velocity;
            }

            bool isCanAttack = IsEnemyCanAttack(ant);
            if (isCanAttack)
            {
                RangeEnemyAttack(ant, _player);
            }
        }

        for (var i = 0; i < _eggEnemies.Count; i++)
        {
            var ant = _eggEnemies[i];
            if (Vector2.Distance(ant.transform.position, _player.transform.position) > 20)
            {
                continue;
            }

            ant.transform.localScale += 0.7f * Vector3.one * Time.deltaTime;
            bool isCanAttack = IsEnemyCanAttack(ant);
            if (isCanAttack)
            {
                SpawnAntFromEgg(ant);
                _eggEnemies.Remove(ant);
                Destroy(ant.gameObject);
            }
        }

        for (var i = 0; i < _bossEnemies.Count; i++)
        {
            var ant = _bossEnemies[i];
            if (Vector2.Distance(ant.transform.position, _player.transform.position) > 20)
            {
                continue;
            }

            var dirToPlayer = (_player.transform.position - ant.transform.position).normalized;
            // Move
            if (ant.transform.IsNear(_player.transform, ant.AttackRange))
            {
                ant.Animator.SetBool(Walk, true);
                var velocity = -dirToPlayer * ant.MoveSpeed;
                ant.rb.linearVelocity = velocity;
            }
            else
            {
                ant.Animator.SetBool(Walk, false);
                ant.rb.linearVelocity = Vector2.zero;
            }

            ant.transform.up = dirToPlayer;


            bool isCanAttack = IsEnemyCanAttack(ant);
            if (isCanAttack)
            {
                RangeEnemyAttack(ant, _player);
                var randomPos = (Vector2)ant.transform.position + Random.insideUnitCircle * 3;
                TrySpawnEgg(randomPos);
            }
        }
    }

    private void TrySpawnEgg(Vector2 randomPos)
    {
        var newEnemy = Instantiate(_eggPrefab, randomPos, Quaternion.identity);
        _eggEnemies.Add(newEnemy);
    }

    [SerializeField]
    private Ant _antFromEggPrefab;

    private void SpawnAntFromEgg(Ant ant)
    {
        var newEnemy = Instantiate(_antFromEggPrefab, ant.transform.position, Quaternion.identity);
        switch (newEnemy.tag)
        {
            case "EnemyMelee":
                _meleeEnemies.Add(newEnemy);
                break;
            case "EnemyRange":
                _rangeEnemies.Add(newEnemy);
                break;
            case "EnemyBoss":
                _bossEnemies.Add(newEnemy);
                break;
            case "Egg":
                _eggEnemies.Add(newEnemy);
                break;
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
        var projectilesParent = Instantiate(ant.ProjectilePrefab, ant.transform.position, Quaternion.identity);
        projectilesParent.up = (_player.transform.position - ant.transform.position).normalized;
        AddProjectileAndDestroyAfterTime(projectilesParent);
        ant.LastAttackTime = Time.time;
    }

    private List<Transform> _projectiles = new();
    private static readonly int Walk = Animator.StringToHash("walk");

    private void AddProjectileAndDestroyAfterTime(Transform projectilesParent)
    {
        for (int i = 0; i < projectilesParent.childCount; i++)
        {
            var child = projectilesParent.GetChild(i);
            child.DOScale(Vector3.zero, 3).SetEase(Ease.InBack).OnComplete(() => { _projectiles.Remove(child); });
            _projectiles.Add(child);
        }

        Destroy(projectilesParent.gameObject, 3.05f);
    }

    private void UpdateEnemyProjectiles()
    {
        var count = _projectiles.Count;
        for (var i = 0; i < count; i++)
        {
            var projectile = _projectiles[i];

            if (projectile == null || !projectile.gameObject.activeSelf)
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
        if (ant.IsPassFirstAttack)
        {
            ant.LastAttackTime = Time.time;
            ant.IsPassFirstAttack = false;
            return false;
        }

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
        _player.HitImpactSprite.DOFade(1f, 0.1f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo);
        _isPlayerInvincible = true;
        DOTween.Sequence().InsertCallback(_player.InvinsibleTime, () => { _isPlayerInvincible = false; });
        LongShake();
        _levelFailSfx.Play();
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
        bool isDash = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftShift);
        if (isDash && _isCanDash)
        {
            _isCanDash = false;
            _isPlayerInvincible = true;
            _currentDashTime = _dashTime; // Reset the dash timer.
            _isPlayerDash = true;
            _player.LastAttackTime = Time.time;
            await PlayerDash(_player.transform.up);
            _player.LastAttackTime = Time.time;
        }

        if (_player.LastAttackTime > 0 && _isPlayerDash == false)
        {
            _dashSlider.value = Mathf.Clamp01((Time.time - _player.LastAttackTime) / _player.AttackDelay);
            _isCanDash = Time.time - _player.LastAttackTime > _player.AttackDelay;
        }
    }

    private async UniTask PlayerDash(Vector2 direction)
    {
        _dashSfx.Play();
        _anime.transform.localScale = Vector3.one;
        _anime.SetActive(true);
        _anime.transform.DOScale(1.5f * Vector3.one, _dashTime);
        var startZ = _camFollow.FollowOffset.z;
        _player.Animator.Play("dash");
        if (!_isZooming)
        {
            SmallZoom(startZ);
        }

        while (_currentDashTime > 0f)
        {
            _currentDashTime -= Time.deltaTime; // Lower the dash timer each frame.

            _player.rb.linearVelocity = direction * _dashSpeed;
            TryPlayerAttackNearEnemies();
            TryDestroyNearProjectiles();
            await UniTask.Yield();
        }

        if (_isZooming)
        {
            UnZoom(startZ);
        }

        _anime.SetActive(false);
        _player.rb.linearVelocity = Vector2.zero;
        _isPlayerDash = false;
        _isPlayerInvincible = false;
    }

    private void SpawnDashTrail()
    {
        var trail = Instantiate(_trailPrefab, _player.transform.position, Quaternion.identity);
        trail.transform.right = _player.transform.up;
        trail.DOFade(0.2f, 0.1f);
        Destroy(trail.gameObject, 0.15f);
    }


    private bool _isZooming;

    private void SmallZoom(float startZ)
    {
        _isZooming = true;
        DOTween.To(() => _camFollow.FollowOffset.z, x => _camFollow.FollowOffset.z = x, startZ + 3f, 0.3f)
            .SetEase(Ease.Flash);
    }

    private void UnZoom(float startZ)
    {
        DOTween.To(() => _camFollow.FollowOffset.z, x => _camFollow.FollowOffset.z = x, Mathf.Min(-12, startZ), 0.2f)
            .SetEase(Ease.Flash).OnComplete(() => _isZooming = false);
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
            var enemyOverSize = enemy.transform.localScale.x - 1;
            if (_player.transform.IsNear(enemy.transform, _player.AttackRange + enemyOverSize / 2f))
            {
                _ = PlayerAttack(enemy, _meleeEnemies);
            }
        }

        for (var i = 0; i < _rangeEnemies.Count; i++)
        {
            var enemy = _rangeEnemies[i];
            var enemyOverSize = enemy.transform.localScale.x - 1;
            if (_player.transform.IsNear(enemy.transform, _player.AttackRange + enemyOverSize / 2f))
            {
                _ = PlayerAttack(enemy, _rangeEnemies);
            }
        }

        for (var i = 0; i < _eggEnemies.Count; i++)
        {
            var enemy = _eggEnemies[i];
            var enemyOverSize = enemy.transform.localScale.x - 1;
            if (_player.transform.IsNear(enemy.transform, _player.AttackRange + enemyOverSize / 2f))
            {
                _ = PlayerAttack(enemy, _eggEnemies);
            }
        }

        for (var i = 0; i < _bossEnemies.Count; i++)
        {
            var enemy = _bossEnemies[i];
            var enemyOverSize = enemy.transform.localScale.x - 1;
            if (_player.transform.IsNear(enemy.transform, _player.AttackRange + enemyOverSize / 2f))
            {
                _ = PlayerAttack(enemy, _bossEnemies);
            }
        }

        for (int i = 0; i < _notEnemies.Count; i++)
        {
            var notEnemy = _notEnemies[i];
            var enemyOverSize = notEnemy.transform.localScale.x - 1;
            if (_player.transform.IsNear(notEnemy.transform, _player.AttackRange + enemyOverSize / 2f))
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

        SpawnDashTrail();
        SmallShake();

        Time.timeScale = 0.4f;

        target.transform.DOKill(true);
        target.LastGetDamageTime = Time.time;
        target.transform.DOPunchScale(1.015f * Vector3.one, 0.15f, 1, 0.1f).SetEase(Ease.Flash);
        target.Health -= _player.Damage;
        if (target.HitImpactSprite != null)
        {
            target.HitImpactSprite.DOFade(1f, 0.1f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo);
        }

        if (target.SlashMaskTransform != null)
        {
            target.SlashMaskTransform.DOScaleY(30f, 0.2f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                target.SlashMaskTransform.localScale = new Vector3(15, 0, 15);
            });
        }

        if (target.Health <= 0)
        {
            AppendKillStreak();
            _enemyDieSfx.Play();
            LongShake();
            targetArray.Remove(target);
            await target.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).ToUniTask();

            target.transform.DOKill();
            Destroy(target.gameObject);
            var isCompleted = await TryCompleteLevel();
        }

        Time.timeScale = 1f;
    }

    [SerializeField]
    private float _killStreakTime;

    private float _killStreakTimeStamp;

    private void AppendKillStreak()
    {
        if (Time.time - _killStreakTimeStamp > _killStreakTime)
        {
            _killStreak = 0;
        }

        _killStreak++;
        _killStreakTimeStamp = Time.time;
        if (_killStreak >= 3)
        {
            AnimateComboCounter(_killStreak);
        }
    }

    private void AnimateComboCounter(int killStreak)
    {
        _comboCounterLabel.text = $"<wave amplitude=1.5><palette>COMBO X{killStreak}";
        var targetScale = 1 + 0.2f * killStreak;
        _comboCounterLabel.transform.DOKill(true);
        _comboCounterLabel.DOKill(false);
        _comboCounterLabel.alpha = 1f;
        _comboCounterLabel.transform.DOScale(targetScale, 0.3f).SetUpdate(true).SetEase(Ease.OutBack).OnComplete(() =>
        {
            _comboCounterLabel.DOFade(0f, _killStreakTime).OnComplete(() =>
            {
                _comboCounterLabel.transform.localScale = Vector3.zero;
            });
        });
    }

    #endregion

    private Sequence _shakeSequence;

    private void SmallShake()
    {
        var dool = E.DynamicContainer.DynamicParent;
        _shakeSequence?.Kill(true);
        _shakeSequence = DOTween.Sequence();
        _shakeSequence.Append(dool.DOShakePosition(0.2f, .8f));
        _shakeSequence.OnUpdate(() =>
        {
            _camFollow.FollowOffset.x = dool.position.x;
            _camFollow.FollowOffset.y = dool.position.y;
        });
        _shakeSequence.OnComplete(() =>
        {
            _camFollow.FollowOffset.x = 0f;
            _camFollow.FollowOffset.y = 0f;
        });
    }

    private void LongShake()
    {
        var dool = E.DynamicContainer.DynamicParent;
        _shakeSequence?.Kill(true);
        _shakeSequence = DOTween.Sequence();
        _shakeSequence.Append(dool.DOShakePosition(0.3f, 1f));
        _shakeSequence.OnUpdate(() =>
        {
            _camFollow.FollowOffset.x = dool.position.x;
            _camFollow.FollowOffset.y = dool.position.y;
        });
        _shakeSequence.OnComplete(() =>
        {
            _camFollow.FollowOffset.x = 0f;
            _camFollow.FollowOffset.y = 0f;
        });
    }

    [SerializeField]
    private AudioMixer _masterMixer;

    public void SetMusic(float arg0)
    {
        Game.MusicValue = arg0;
        float v = arg0 > 0 ? Mathf.Log10(arg0) * 20 : -80;
        _masterMixer.SetFloat("MusicVolume", v);
    }

    public void SetSound(float arg0)
    {
        Game.SoundValue = arg0;
        float v = arg0 > 0 ? Mathf.Log10(arg0) * 20 : -80;
        _masterMixer.SetFloat("SoundVolume", v);
    }
}