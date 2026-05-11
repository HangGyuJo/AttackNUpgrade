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

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
