using UnityEngine;

/// <summary>
/// One killable enemy, living on the body object that carries the patrol movement and the
/// meshes.
///
/// What happens when the player lands on an enemy's head used to live in PlayerController,
/// which reached the pieces it needed by walking the hierarchy: a check on the literal name
/// "EnemyBody" to decide whether to disable one collider or three, an index loop that threw
/// the moment an enemy had fewer meshes than that, and a two step walk to the parent's parent
/// to find the Animator. Every reference is wired in the prefab now, so adding a mesh to an
/// enemy or renaming an object cannot break the kill.
/// </summary>
public class Enemy : MonoBehaviour
{
    private static readonly int IsDeadParam = Animator.StringToHash("isDead");

    [Header("Wiring")]
    [SerializeField]
    [Tooltip("Animator on the enemy root, told the enemy died so the death clip plays.")]
    private Animator _animator;

    [SerializeField]
    [Tooltip("Patrol movement, stopped on death. Leave empty for an enemy that does not move.")]
    private WayPointFollower _movement;

    [SerializeField]
    [Tooltip("The head trigger the player bounces off, hidden as soon as the enemy dies.")]
    private GameObject _headTrigger;

    [SerializeField]
    [Tooltip("Mesh colliders switched off on death so the body stops blocking the player.")]
    private MeshCollider[] _bodyColliders;

    [Header("Timing")]
    [SerializeField]
    [Tooltip("Seconds between the death animation starting and the body being removed.")]
    private float _despawnDelay = 2f;

    private bool _isDead;

    /// <summary>
    /// Kills the enemy: hides the head, stops it blocking and moving, plays the death
    /// animation and removes the body. Does nothing if the enemy is already dead.
    /// </summary>
    public void Kill()
    {
        if (_isDead)
        {
            return;
        }

        _isDead = true;

        if (_headTrigger != null)
        {
            _headTrigger.SetActive(false);
        }

        for (int i = 0; i < _bodyColliders.Length; i++)
        {
            if (_bodyColliders[i] != null)
            {
                _bodyColliders[i].enabled = false;
            }
        }

        if (_movement != null)
        {
            _movement.enabled = false;
        }

        if (_animator != null)
        {
            _animator.SetBool(IsDeadParam, true);
        }

        Destroy(gameObject, _despawnDelay);
    }
}
