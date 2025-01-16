using UnityEngine;

public class RandomSongPlayer : MonoBehaviour
{
    // Add your audio clips to this array in the Unity Editor
    public AudioClip[] songs;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Check if there are songs in the array
        if (songs.Length > 0)
        {
            // Play a random song
            PlayRandomSong();
        }
        else
        {
            Debug.LogError("No songs added to the 'songs' array. Add audio clips in the Unity Editor.");
        }
    }

    void PlayRandomSong()
    {
        // Pick a random index from the array
        int randomIndex = Random.Range(0, songs.Length);

        // Set the chosen song as the AudioClip for the AudioSource
        audioSource.clip = songs[randomIndex];

        // Play the selected song
        audioSource.Play();
    }
}
