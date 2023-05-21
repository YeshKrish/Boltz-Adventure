using System.Threading.Tasks;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainCamera;
    [SerializeField]
    private GameObject _fightCamera;
    [SerializeField]
    private GameObject _level6Camera;

    private float _maxXdistance = 116.1f;

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
    private void OnEnable()
    {
        PlayerController.ActivateFightCamera += ActivateCameraFight;
        PlayerController.DeActivateFightCamera += DeActivateCameraFight;
    }

    private void Update()
    {
        if(_level6Camera.transform.position.x == _maxXdistance && _level6Camera.activeSelf)
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
        if(SpecialMonsters._isAlienDead && !_mainCamera.activeSelf)
        {
            DeactivateCameraDelay();
        }
    }

    async void DeactivateCameraDelay()
    {
        await Task.Delay(500);
        _mainCamera.SetActive(true);
        _fightCamera.SetActive(false);
    }

    private void OnDisable()
    {
        PlayerController.ActivateFightCamera -= ActivateCameraFight;
        PlayerController.DeActivateFightCamera -= DeActivateCameraFight;
    }
}
