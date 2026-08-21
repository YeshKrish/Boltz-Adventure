using System.Collections;
using UnityEngine;

/// <summary>
/// Decides which of the boss level's three cameras is live: the scripted intro camera that
/// flies the arena at the start, the follow camera that plays the level, and the fixed one
/// used while the boss fight is on.
/// </summary>
public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainCamera;
    [SerializeField]
    private GameObject _fightCamera;
    [SerializeField]
    private GameObject _level6Camera;

    [Header("Handback")]
    [SerializeField]
    [Tooltip("X position the intro camera animation ends on. Once the camera is at or past this point the follow camera takes over and the UI comes back.")]
    private float _introEndX = 116.1f;

    [SerializeField]
    [Tooltip("Pause after the boss dies before the follow camera takes back over from the fight camera.")]
    private float _fightHandbackDelay = 0.5f;

    private bool _isReturningFromFight;

    private void OnEnable()
    {
        PlayerController.ActivateFightCamera += ActivateCameraFight;
        PlayerController.DeActivateFightCamera += DeActivateCameraFight;
    }

    private void Start()
    {
        if (!_level6Camera.activeSelf)
        {
            _level6Camera.SetActive(true);
            _mainCamera.SetActive(false);
            _fightCamera.SetActive(false);
            UIManager.Instance.HideUI();
        }
    }

    private void Update()
    {
        // The intro camera animates from x of -3.1 to x of 116.1 over five and a quarter
        // seconds and then holds. This used to test the position against that end point for
        // exact float equality, which worked only because the clip's final keyframe happens to
        // be bit for bit the same value as the literal in the code. Re-authoring the animation,
        // or giving the camera a parent, would have left the UI hidden for the rest of the
        // level with nothing to say why.
        if (_level6Camera.activeSelf && _level6Camera.transform.position.x >= _introEndX)
        {
            _level6Camera.SetActive(false);
            _mainCamera.SetActive(true);
            UIManager.Instance.ActivateUI();
        }
    }

    private void ActivateCameraFight()
    {
        if (_mainCamera.activeSelf)
        {
            _mainCamera.SetActive(false);
            _fightCamera.SetActive(true);
        }
    }

    private void DeActivateCameraFight()
    {
        // PlayerController raises this from Update on every frame once the boss is dead, so
        // without a guard this started a fresh handback timer on each of those frames.
        if (_isReturningFromFight || !SpecialMonsters.IsAlienDead || _mainCamera.activeSelf)
        {
            return;
        }

        _isReturningFromFight = true;
        StartCoroutine(ReturnFromFightCamera());
    }

    private IEnumerator ReturnFromFightCamera()
    {
        // Was an async void awaiting Task.Delay, which is not tied to this object's lifetime.
        // Leaving the level part way through the wait resumed it against destroyed cameras.
        yield return new WaitForSeconds(_fightHandbackDelay);

        _mainCamera.SetActive(true);
        _fightCamera.SetActive(false);
        _isReturningFromFight = false;
    }

    private void OnDisable()
    {
        PlayerController.ActivateFightCamera -= ActivateCameraFight;
        PlayerController.DeActivateFightCamera -= DeActivateCameraFight;
        _isReturningFromFight = false;
    }
}
