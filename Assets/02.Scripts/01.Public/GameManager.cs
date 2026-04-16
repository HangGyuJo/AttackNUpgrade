using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 사용


    public InventoryManager inventoryManager; // 인벤토리 매니저

    // Start is called before the first frame update
    void Start()
    {
        // 싱글톤이 이미 선언되어 있으면 코드 종료
        if (Instance != null) return;


        Instance = this;

        // 씬이 전환되어도 설정한 오브젝트가 계속 유지되도록 함.
        DontDestroyOnLoad(inventoryManager);
        DontDestroyOnLoad(Instance);

        // 화면이 꺼지지 않도록 함
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (SceneManager.GetActiveScene().name.CompareTo("Title") != 0
                && SceneManager.GetActiveScene().name.CompareTo("Play") != 0)
        {
            ChangeScene("Title");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 씬을 전환하는 함수
    /// </summary>
    /// <param name="scene_Name">씬 이름</param>
    public IEnumerator ChangeScene(string scene_Name)
    {
        // 비동기적으로 씬을 로드함
        AsyncOperation op = SceneManager.LoadSceneAsync(scene_Name);

        // 씬 로드가 완료되어도 즉시 화면 전환을 하지 않도록 설정
        op.allowSceneActivation = false;

        yield return op;
    }
}
