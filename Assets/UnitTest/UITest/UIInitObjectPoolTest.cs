using ible.Foundation.ObjectPool;
using ible.Foundation.UI;
using ible.GameModule.UI.HeroUI;
using UnityEngine;


namespace ible.UnitTest
{
    public class UIInitObjectPoolTest : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            return;
            Debug.Log($"ObjectPoolTest Step0");
            var data1 = HeroUIData.Allocate(1);
            UIManager.Instance.Show(data1);

            var data2 = HeroUIData.Allocate(2);
            UIManager.Instance.Show(data2);

            var data3 = HeroUIData.Allocate(3);
            UIManager.Instance.Show(data3);
            Debug.Log($"ObjectPoolTest HeroData.Data1:{data1.Index}, HeroData.Data2:{data2.Index} , HeroData.Data3:{data3.Index}");
            Debug.Log($"ObjectPoolTest CreatedObjectCount:{SharedObjectPool<HeroUIData>.Instance.CreatedObjectCount}, UseableCount:{SharedObjectPool<HeroUIData>.Instance.UsableCount}");
            
            InvokeRepeating("Step1", 1, 1);
        }

        private void Step1()
        {
            Debug.Log($"ObjectPoolTest Step1");
            CancelInvoke("Step1");
            UIManager.Instance.Hide(UIName.HeroUI);
            UIManager.Instance.Hide(UIName.HeroUI);
            UIManager.Instance.Hide(UIName.HeroUI);

            var data1 = HeroUIData.Allocate(4);
            UIManager.Instance.Show(data1);

            var data2 = HeroUIData.Allocate(5);
            UIManager.Instance.Show(data2);

            var data3 = HeroUIData.Allocate(6);
            UIManager.Instance.Show(data3);

            var data4 = HeroUIData.Allocate(7);
            UIManager.Instance.Show(data4);
            Debug.Log($"ObjectPoolTest HeroData.Data1:{data1.Index}, HeroData.Data2:{data2.Index} , HeroData.Data3:{data3.Index} , HeroData.Data4:{data4.Index}");
            Debug.Log($"ObjectPoolTest CreatedObjectCount:{SharedObjectPool<HeroUIData>.Instance.CreatedObjectCount}, UseableCount:{SharedObjectPool<HeroUIData>.Instance.UsableCount}");

            InvokeRepeating("Step2", 1, 1);
        }

        private void Step2()
        {
            Debug.Log($"ObjectPoolTest Step2");
            CancelInvoke("Step2");
            UIManager.Instance.Hide(UIName.HeroUI);
            UIManager.Instance.Hide(UIName.HeroUI);
            UIManager.Instance.Hide(UIName.HeroUI);

            var data1 = HeroUIData.Allocate(8);
            UIManager.Instance.Show(data1);

            var data2 = HeroUIData.Allocate(9);
            UIManager.Instance.Show(data2);

            var data3 = HeroUIData.Allocate(10);
            UIManager.Instance.Show(data3);

            var data4 = HeroUIData.Allocate(11);
            UIManager.Instance.Show(data4);
            Debug.Log($"ObjectPoolTest HeroData.Data1:{data1.Index}, HeroData.Data2:{data2.Index} , HeroData.Data3:{data3.Index} , HeroData.Data4:{data4.Index}");
            Debug.Log($"ObjectPoolTest CreatedObjectCount:{SharedObjectPool<HeroUIData>.Instance.CreatedObjectCount}, UseableCount:{SharedObjectPool<HeroUIData>.Instance.UsableCount}");
        }
    }
}
