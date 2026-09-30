using UnityEngine;
using UnityEngine.SceneManagement;

public class Chanthink_Out : MonoBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name);
        }
    }
}
