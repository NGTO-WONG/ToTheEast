using UnityEngine;

namespace QFramework.Example
{
    public class EasyEventExample : MonoBehaviour
    {
        // private EasyEvent<string> mSomeStringEvent = new EasyEvent<string>();

        public class TestEvent : EasyEvent<int>
        {
            
        }
        private void Awake()
        {

            EasyEvents.Get<TestEvent>().Register(OnEvent).UnRegisterWhenGameObjectDestroyed(gameObject);

            // mSomeStringEvent.Register(str =>
            // {
            //     Debug.Log(str);
            // }).UnRegisterWhenGameObjectDestroyed(gameObject);
            //
            // EasyEvents.Register<EasyEvent<int>>();
            // EasyEvents.Register<MyEvent>();
            //
            // EasyEvents.Get<EasyEvent<int>>().Register(number =>
            // {
            //     Debug.Log(number);
            // }).UnRegisterWhenGameObjectDestroyed(gameObject);
            //
            // EasyEvents.Get<MyEvent>().Register((str1,str2)=>
            // {
            //     Debug.Log(str1 +":" + str2);
            // }).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnEvent(int obj)
        {
            Debug.Log(obj);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                EasyEvents.Get<TestEvent>().Trigger(123);
            }
            //
            // if (Input.GetMouseButtonDown(1))
            // {
            //     EasyEvents.Get<EasyEvent<int>>().Trigger(123);
            // }
            //
            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     EasyEvents.Get<MyEvent>().Trigger("你好","EasyEvent");
            // }
        }
        
        // public class MyEvent : EasyEvent<string,string>
        // {
        //     
        // }
    }
}