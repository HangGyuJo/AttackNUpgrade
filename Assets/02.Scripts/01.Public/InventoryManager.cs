using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager
{
    private static InventoryManager instance = null; // 싱글톤 사용 (보안을 위해 private 선언)

    // 인스턴스에 접근할 수 있는 프로퍼티
    public static InventoryManager Instance
    {
        get
        {
            // 인스턴스가 없을 경우 생성
            if (Instance == null) instance = new InventoryManager();
            return instance;
        }
    }

    Inventory inventory;


}
