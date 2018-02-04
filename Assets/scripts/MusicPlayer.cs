using UnityEngine;
using System.Collections;

public class MusicPlayer : MonoBehaviour {
	static MusicPlayer instance = null;
	
	public AudioClip startClip;
	public AudioClip gameClip;
	public AudioClip endClip;
	
	private AudioSource music;
	
	void Start () {
		if (instance != null && instance != this) {
			Destroy (gameObject);
			print ("Duplicate music player self-destructing!");
		} else {
			instance = this;
			GameObject.DontDestroyOnLoad(gameObject);
			music = GetComponent<AudioSource>();
			music.clip = startClip;
			music.loop = true;
			music.Play ();
		}
	}
	
	void OnLevelWasLoaded(int Level){
	
	Debug.Log ("MusicPlayer: Loaded level"+Level);
	music.Stop ();
	
	
			if(Level == 0){
			music.clip = startClip;
			}
	
			if(Level == 1){
				music.clip = gameClip;
			}
		
			if(Level == 2){
				music.clip = endClip;
			}
	
	music.loop = true;
	music.Play ();
	
	}
}
