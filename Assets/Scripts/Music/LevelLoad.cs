using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoad : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
       if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(0);
        }
    }
}
