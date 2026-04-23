using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null; // 싱글톤 사용 (보안을 위해 private 선언)

    // 인스턴스에 접근할 수 있는 프로퍼티
    public static GameManager Instance
    {
        get
        {
            // 인스턴스가 없을 경우 생성
            if (Instance == null) instance = new GameManager();
            return instance;
        }
    }


    // 생성자
    public GameManager()
    {
        // 화면이 꺼지지 않도록 함
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
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
