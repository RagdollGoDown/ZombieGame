using UnityEngine;
using UnityEngine.SceneManagement;

public class HubManager : MonoBehaviour
{
    public void LoadSimpleLevel()
    {
        SceneManager.LoadScene("SimpleLevel");
    }
}
