using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkBootsrapper : MonoBehaviour
{
    // 처음으로 로드할 씬 이름
    [SerializeField] private string firstSceneName = "Lobby";

    private static bool initialized; // 초기화 여부 확인용

    private void Awake()
    {
        // 이미 초기화 되었다면 현재 오브젝트 파괴 후 함수 종료
        if (initialized)
        {
            Destroy(gameObject);
            return;
        }

        // 초기화 여부 확인용
        initialized = true;
        
        // 씬이 변경되어도 파괴되지 않을 오브젝트들
        DontDestroyOnLoad(GameObject.Find("UnityTransport"));
        DontDestroyOnLoad(gameObject);

        // 현재 씬이 처음 로드할 씬이 아닐 경우, 처음 씬을 로드
        if (SceneManager.GetActiveScene().name != firstSceneName)
        {
            SceneManager.LoadScene(firstSceneName);
        }
    }
}