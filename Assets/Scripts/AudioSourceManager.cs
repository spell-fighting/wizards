using UnityEngine;

public class AudioSourceManager : MonoBehaviour
{
	[SerializeField] private AudioSource _audioSource;

	private void Awake () {
		DontDestroyOnLoad(_audioSource);
	}
}
