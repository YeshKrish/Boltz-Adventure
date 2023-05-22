//
//
//NOTES:
//
//This script is used for DEMONSTRATION porpuses of the Projectiles. I recommend everyone to create their own code for their own projects.
//THIS IS JUST A BASIC EXAMPLE PUT TOGETHER TO DEMONSTRATE VFX ASSETS.
//
//




#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class ProjectileMoveScript : MonoBehaviour {

    public bool rotate = false;
    public float rotateAmount = 90;
    public bool bounce = false;
    public float bounceForce = 10;
    public float speed;
	[Tooltip("From 0% to 100%")]
	public float accuracy;
	public float fireRate;
	public GameObject muzzlePrefab;
	public GameObject hitPrefab;
	public List<GameObject> trails;
    public GameObject ProjectileEnd;

    private Vector3 startPos;
	private Rigidbody rb;
    private bool _playerHit = false;
    private bool _hasIncremented = false;
    private int _bulletsSpawned = 0;

    public static float startTime;
    public static float activeTime;

    public static event Action DeactivateAllActiveBullets;

    private void OnEnable()
    {
        startTime = Time.time;
    }

    void Start () {
        startPos = transform.position;
        rb = GetComponent <Rigidbody> ();
        _bulletsSpawned = 0;
    }

    private void Update()
    {
        if (transform.position.x < SpecialMonsters._startPos.x - 15)
        {
            gameObject.SetActive(false);
        }
    }
	void OnCollisionEnter (Collision co) {
       
        if (co.gameObject.CompareTag("Player"))
        {
            _playerHit = true;
            DeactivateAllActiveBullets?.Invoke();
            GameManager.instance.GameOver();
        }
	}
    private void OnDisable()
    {
        activeTime += Time.time - startTime;
    }
}
