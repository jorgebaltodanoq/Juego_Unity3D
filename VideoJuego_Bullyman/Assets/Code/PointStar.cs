using UnityEngine;

public class PointStar : MonoBehaviour, iPlayerRespawnListenner 
{
	public GameObject Effect;
	public int PointsToAdd = 10;
	public AudioClip HitStarSound;
	public Animator Animator;
	public SpriteRenderer Renderer;

	private bool _isCollected;

	public void OnTriggerEnter2D(Collider2D other)
	{
		if(_isCollected)
			return;

		if(other.GetComponent<Player> ()== null)
			return;

		if(HitStarSound != null)
			AudioSource.PlayClipAtPoint(HitStarSound, transform.position);

		GameManager.Instance.AddPoints (PointsToAdd);
		Instantiate (Effect, transform.position, transform.rotation);
		FloatingText.Show(string.Format("+{0}!", PointsToAdd), "PointStarText", new FromWorldPointTextPositioner(Camera.main, transform.position, 1.5f, 50));

		_isCollected = true;

		Animator.SetTrigger ("Collect");
	}

	public void FinishAnimationEvent()
	{
		Renderer.enabled = false;
		Animator.SetTrigger("Reset");
	}

	public void OnPlayerRespawnInThisCheckpoint(Checkpoint chekpoint, Player player)
	{
		_isCollected = false;
		Renderer.enabled = true;
	}
}
