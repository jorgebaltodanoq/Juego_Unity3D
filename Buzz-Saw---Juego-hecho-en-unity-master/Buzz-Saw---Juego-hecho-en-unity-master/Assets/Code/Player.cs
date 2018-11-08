using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour, iTakeDamage
{
	private bool _isFacingRight;
	private CharacterController2D _controller;
	private float _normalizedHorizontalSpeed;

	public float MaxSpeed = 8;
	public float SpeedAccelerationOnGround =  10f;
	public float SpeedAccelerationInAir = 5f;
	public int MaxHealth = 100;
	public GameObject OuchEffect;
	public Projectile Projectile;
	public float FireRate;
	public Transform ProjectileFireLocation;
	public GameObject FireProjectileEffect;
	public AudioClip PlayerHitSound;
	public AudioClip PlayerShootSound;
	public AudioClip PlayerHealthSound;
	public Animator Animator;

	public int Health { get; private set; }
	public bool IsDead { get; private set; }

	public float _canFireIn;

	public void Awake()
	{
		_controller = GetComponent<CharacterController2D>();
		_isFacingRight = transform.localScale.x > 0;
		Health = MaxHealth;
	}

	public void Update()
	{
		_canFireIn -= Time.deltaTime;

		if(!IsDead)
		HandleInput();

		var movementFactor = _controller.State.IsGrounded ? SpeedAccelerationOnGround : SpeedAccelerationInAir;

		if (IsDead)
		_controller.SetHorizontalForce (0);
		else
		_controller.SetHorizontalForce(Mathf.Lerp(_controller.Velocity.x, _normalizedHorizontalSpeed * MaxSpeed, Time.deltaTime * movementFactor));

		Animator.SetBool ("IsGrounded", _controller.State.IsGrounded);
		Animator.SetBool ("IsDead", IsDead);
		Animator.SetFloat ("Speed", Mathf.Abs (_controller.Velocity.x) / MaxSpeed);
	}

	public void FinishLevel()
	{
		enabled = false;
		_controller.enabled = false;
		collider2D.enabled = false;
	}

	public void Kill()
	{
		_controller.HandleCollisions = false;
		collider2D.enabled = false;
		IsDead = true;
		Health = 0;

		_controller.SetForce (new Vector2 (0, 20));
	}

	public void RespawnAt(Transform spawnPoint)
	{
		if (! _isFacingRight)
			flip ();

		IsDead = false;
		collider2D.enabled = true; 
		_controller.HandleCollisions = true;
		Health = MaxHealth;

		transform.position = spawnPoint.position;
	}

	public void TakeDamage(int damage, GameObject Instigator)
	{
		AudioSource.PlayClipAtPoint (PlayerHitSound, transform.position);
		FloatingText.Show(string.Format("-{0}", damage), "PlayerTakeDmageText", new FromWorldPointTextPositioner(Camera.main, transform.position, 2f, 60));

		Instantiate (OuchEffect, transform.position, transform.rotation);
		Health -= damage;

		if (Health <= 0)
			LevelManager.Instance.KillPlayer();
	}

	public void GiveHealth(int health, GameObject instigator)
	{
		AudioSource.PlayClipAtPoint (PlayerHealthSound, transform.position);
		FloatingText.Show(string.Format("+{0}!", Health), "PlayerGotHealthText", new FromWorldPointTextPositioner(Camera.main, transform.position, 2f, 60));
		Health = Mathf.Min (Health + health, MaxHealth);
	}

	private void HandleInput()
	{
		if(Input.GetKey(KeyCode.D))
		{
			_normalizedHorizontalSpeed = 1;
			if(!_isFacingRight)
				flip();

		}
		else if(Input.GetKey(KeyCode.A))
		{
			_normalizedHorizontalSpeed = -1;
			if(_isFacingRight)
				flip();
		}
		else
		{
			_normalizedHorizontalSpeed = 0;
		}

		if(_controller.CanJump && Input.GetKeyDown (KeyCode.Space))
		{
			_controller.Jump();
		}

		if(Input.GetMouseButtonDown(0))
			FireProjectile();
	}

	private void FireProjectile()
	{
		if(_canFireIn > 0)
			return;

		if(FireProjectileEffect != null)
		{
			var effect = (GameObject) Instantiate(FireProjectileEffect, ProjectileFireLocation.position, ProjectileFireLocation.rotation);
			effect.transform.parent= transform;
		}
		var direction = _isFacingRight ? Vector2.right : -Vector2.right;

		var projectile = (Projectile)Instantiate (Projectile, ProjectileFireLocation.position, ProjectileFireLocation.rotation);
		projectile.Initialize (gameObject, direction, _controller.Velocity);

		_canFireIn = FireRate;

		AudioSource.PlayClipAtPoint (PlayerShootSound, transform.position);
		Animator.SetTrigger ("Fire");
	}

	private void flip()
	{
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);  
		_isFacingRight = transform.localScale.x >  0;


	}
}
