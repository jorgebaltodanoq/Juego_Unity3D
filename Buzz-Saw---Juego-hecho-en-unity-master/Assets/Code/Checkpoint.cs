using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Checkpoint : MonoBehaviour 
{
	private List<iPlayerRespawnListenner> _listeners;

	public void Awake()
	{
		_listeners = new List<iPlayerRespawnListenner> ();
	}

	public void PlayerHitCheckpoint()
	{
		StartCoroutine(PlayerHitCheckpointCo(LevelManager.Instance.CurrentTimeBonus));
	}

	private IEnumerator PlayerHitCheckpointCo(int bonus)
	{
		FloatingText.Show("Checkpoint", "CheckpointText", new CenterTextPositioner(.5f));
		yield return new WaitForSeconds(.5f);
		FloatingText.Show(string.Format("+{0}Time Bonus", bonus), "CheckpointText", new CenterTextPositioner(.5f));
	}

	public void PlayerLeftCheckpoint()
	{

	}

	public void SpawnPlayer(Player player)
	{
		player.RespawnAt (transform);

		foreach (var listener in _listeners)
			listener.OnPlayerRespawnInThisCheckpoint(this, player);
	}

	public void AssingObjectToCheckpoint(iPlayerRespawnListenner listener)
	{
		_listeners.Add (listener);
	}
}
