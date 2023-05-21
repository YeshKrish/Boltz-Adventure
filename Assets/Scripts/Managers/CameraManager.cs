using System.Threading.Tasks;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainCamera;
    [SerializeField]
    private GameObject _fightCamera;

    private void OnEnable()
    {
        PlayerController.ActivateFightCamera += ActivateCameraFight;
        PlayerController.DeActivateFightCamera += DeActivateCameraFight;
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
