using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelManager : MonoBehaviour 
{
	public static LevelManager Instance { get; private set; }

	public Player Player { get; private set; }
	public CameraController Camera { get; private set; }
	public TimeSpan RunningTime { get { return DateTime.UtcNow - _started; } }

	public int CurrentTimeBonus
	{
		get
		{
			var secondDifference = (int)(BonusCutoffSeconds - RunningTime.TotalSeconds);
			return Mathf.Max(0, secondDifference) * BonusSecondMultiplier;
		}
	}

	private List<Checkpoint> _chekpoints;
	private int _currentChekpointIndex;
	private DateTime _started;
	private int _savePoints;

	public Checkpoint DebugSpawn;
	public int BonusCutoffSeconds;
	public int BonusSecondMultiplier;

	public void Awake()
	{
		_savePoints = GameManager.Instance.Points;
		Instance = this;
	}

	public void Start()
	{
		_chekpoints = FindObjectsOfType<Checkpoint> ().OrderBy (t => t.transform.position.x).ToList();
		_currentChekpointIndex = _chekpoints.Count > 0 ? 0 : -1;

		Player = FindObjectOfType<Player> ();
		Camera = FindObjectOfType<CameraController> ();

		_started = DateTime.UtcNow;

		var listeners = FindObjectsOfType<MonoBehaviour> ().OfType<iPlayerRespawnListenner> ();
		foreach (var listener in listeners) 
		{
			for( var i = _chekpoints.Count - 1; i >= 0; i--)
			{
				var distance = ((MonoBehaviour)listener).transform.position.x - _chekpoints[i].transform.position.x;
				if (distance < 0)
					continue;
				
				_chekpoints[i].AssingObjectToCheckpoint(listener);
				break;
			}
		}

#if UNITY_EDITOR
		if (DebugSpawn != null)
			DebugSpawn.SpawnPlayer(Player);
		else if (_currentChekpointIndex != -1)
			_chekpoints[_currentChekpointIndex].SpawnPlayer(Player);
#else
		if (_currentChekpointIndex != -1)
			_chekpoints[_currentChekpointIndex].SpawnPlayer(Player);
#endif
	}

	public void Update()
	{
		var isAtLastChekpoint = _currentChekpointIndex + 1 >= _chekpoints.Count;
		if(isAtLastChekpoint)
			return;

		var distanceToNextCheckpoint = _chekpoints [_currentChekpointIndex + 1].transform.position.x - Player.transform.position.x;
		if (distanceToNextCheckpoint >= 0)
			return;

		_chekpoints [_currentChekpointIndex].PlayerLeftCheckpoint ();
		_currentChekpointIndex++;
		_chekpoints [_currentChekpointIndex].PlayerHitCheckpoint ();

		GameManager.Instance.AddPoints (CurrentTimeBonus);
		_savePoints = GameManager.Instance.Points;
		_started = DateTime.UtcNow;

	
	}

	public void GotoNextLevel(string levelName)
	{
		StartCoroutine (GotoNextLevelCo (levelName));
	}

	private IEnumerator GotoNextLevelCo (string levelName)
	{
		Player.FinishLevel ();
		GameManager.Instance.AddPoints (CurrentTimeBonus);

		FloatingText.Show ("Level Complete!", "CheckpointText", new CenterTextPositioner (.2f));
		yield return new WaitForSeconds (1);

		FloatingText.Show (String.Format ("{0} Points!", GameManager.Instance.Points), "CheckpointText", new CenterTextPositioner (.1f));
		yield return new WaitForSeconds (5f);

		if(string.IsNullOrEmpty(levelName))
			Application.LoadLevel("Start Screen");
		else
			Application.LoadLevel(levelName);
	}

	public void KillPlayer()
	{
		StartCoroutine (KillPlayerCo());
	}

	private IEnumerator KillPlayerCo()
	{
		Player.Kill ();
		Camera.IsFollowing = false;
		yield return new WaitForSeconds (2f);

		Camera.IsFollowing = true;

		if(_currentChekpointIndex != -1)
			_chekpoints[_currentChekpointIndex].SpawnPlayer(Player);

		_started = DateTime.UtcNow;
		GameManager.Instance.ResetPoints (_savePoints);
	}
}
