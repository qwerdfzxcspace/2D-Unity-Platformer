using UnityEngine;
using UnityEngine.SceneManagement;

/* <summary>
При триггере меняет сцену
</summary> */

public class LevelChanger : MonoBehaviour
{
    [SerializeField] private BoxCollider2D LevelCollider;
    [SerializeField] private GameObject Music;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            DontDestroyOnLoad(Music);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
