using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Bouncer : MonoBehaviour
{
    [SerializeField]
    private Animator _bounceAnimator;
    [SerializeField]
    private AnimationClip _bounceAnimationClip;

    private bool _isBounced = false;

    public int SpringHeight;

    //private void Start()
    //{
    //    SpringHeight = _springHeight;
    //}

    //public int SpringHeight
    //{
    //    get { return _springHeight; }
    //    set { _springHeight = value; }
    //}

    private void OnEnable()
    {
        PlayerController.Bounce += Bounce;
    }

    private void OnDisable()
    {
        PlayerController.Bounce -= Bounce;
    }

    private void Bounce()
    {
        Debug.Log("Bounce Value:" + _isBounced);
        if (!_isBounced)
        {
            _bounceAnimator.SetBool("canBounce", true);
            StartCoroutine(IdleState());
        }
    }

    IEnumerator IdleState()
    {
        yield return new WaitForSeconds(_bounceAnimationClip.length);
        _bounceAnimator.SetBool("canBounce", false);
    }
}
