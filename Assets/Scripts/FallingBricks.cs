using UnityEngine;
using System.Threading.Tasks;

public static class CharacterAnimatorParamId
{
    public static readonly int BrickFalling = Animator.StringToHash("BrickFall");
}

public class FallingBricks : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody _rb;

    private float _currentTime;
    private bool _isPlatformBurst;
    private bool _hasfallen = false;
    private Vector3 _intialPos;
    private float _platStartToShake = 1f;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;
        _isPlatformBurst = Mathf.FloorToInt(Time.time) % 1 == 0;

        _intialPos = transform.position;

        if(transform.position.y < _intialPos.y - 2f)
        {
            _hasfallen = true;
        }
        else
        {
            _hasfallen = false;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_isPlatformBurst && !_hasfallen && _currentTime > _platStartToShake)
            {
                if(_animator != null)
                {
                    _animator.SetBool("canFall", true);
                    BrickStartFalling();
                }
            }
        }
    }
    async private void BrickStartFalling()
    {
        await Task.Delay(1000);
        _hasfallen = true;
        _rb.isKinematic = false;
        _rb.useGravity = true;
    }

}
