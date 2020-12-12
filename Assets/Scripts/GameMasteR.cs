using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMasteR : MonoBehaviour
{
    public static GameMasteR gm;

    [SerializeField]
    int maxLives = 3;
    static int _remainingLives = 3;
    public static int RemainingLives
    {
        get { return _remainingLives; }
    }

    [SerializeField]
    int startingMoney;
    public static int Money;

    void Awake()
    {
        if (gm == null)
            gm = this;
    }

    public Transform playerPrefab;
    public Transform spawnPoint;
    public float spawnDelay = 3.5f;
    public Transform spawnPrefab;
    public string respawnSoundName = "RespawnCountdown";
    public string spawnSoundName = "Spawn";

    public string gameOverSoundName = "GameOver";

    public CameraShake cameraShake;

    [SerializeField]
    GameObject gameOverUI;

    [SerializeField]
    GameObject upgradeMenu;
    
    [SerializeField]
    WaveSpawneR waveSpawneR;

    public delegate void UpgradeMenuCallback(bool active);
    public UpgradeMenuCallback onToggleUpgradeMenu;

    // cache
    AudioManager audioManager;

    void Start()
    {
        if (cameraShake == null)
        {
            Debug.LogError("No camera shake referenced in GameMaster");
        }

        _remainingLives = maxLives;

        Money = startingMoney;

        // caching
        audioManager = AudioManager.instance;
        if (audioManager == null)
        {
            Debug.LogError("FREAK OUT! No AudioManager found in the scene.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleUpgradeMenu();
        }
    }

    void ToggleUpgradeMenu()
    {
        upgradeMenu.SetActive(!upgradeMenu.activeSelf);
        waveSpawneR.enabled = !upgradeMenu.activeSelf;
        onToggleUpgradeMenu.Invoke(upgradeMenu.activeSelf);
    }

    public void EndGame()
    {
        audioManager.PlaySound(gameOverSoundName);

        Debug.Log("GAME OVER");
        gameOverUI.SetActive(true);
    }

    public IEnumerator _RespawnPlayer()
    {
        audioManager.PlaySound(respawnSoundName);
        yield return new WaitForSeconds(spawnDelay);

        audioManager.PlaySound(spawnSoundName);
        Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        Transform clone = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation) as Transform;
        Destroy(clone.gameObject, 3f);
    }

    public static void KillPlayer(PlayeR player)
    {
        Destroy(player.gameObject);
        _remainingLives -= 1;
        if (_remainingLives <= 0)
        {
            gm.EndGame();
        }
        else
            gm.StartCoroutine(gm._RespawnPlayer());
    }

    public static void KillEnemy(EnemY enemy)
    {
        gm._KillEnemy(enemy);
    }

    public void _KillEnemy(EnemY _enemy)
    {
        // Some sound playing
        audioManager.PlaySound(_enemy.deathSoundName);

        // Gain some money
        Money += _enemy.moneyDrop;
        audioManager.PlaySound("Money");

        // Add particles
        GameObject _clone = Instantiate(_enemy.deathParticles.gameObject, _enemy.transform.position, Quaternion.identity) as GameObject;
        Destroy(_clone, 2f);

        // Camera shake
        cameraShake.Shake(_enemy.shakeAmt, _enemy.shakeLength);
        Destroy(_enemy.gameObject);
    }
}
