using UnityEngine;

namespace JJFramework.Runtime.Input
{
    public class InputReceiver : MonoBehaviour
    {
        public void SetController(IControllerInterface inController)
        {
            m_controller = inController;
        }

        void Awake()
        {
            DontDestroyOnLoad(this);
            name = "InputReceiver";

            InputKeyGroup = new string[35];
            InputKeyGroup[0] = "Horizontal";
            InputKeyGroup[1] = "Vertical";
            InputKeyGroup[2] = "MouseX";
            InputKeyGroup[3] = "MouseY";
            InputKeyGroup[4] = "MouseScrollWheel";
            InputKeyGroup[5] = "1";
            InputKeyGroup[6] = "2";
            InputKeyGroup[7] = "3";
            InputKeyGroup[8] = "4";
            InputKeyGroup[9] = "5";
            InputKeyGroup[10] = "6";
            InputKeyGroup[11] = "7";
            InputKeyGroup[12] = "8";
            InputKeyGroup[13] = "9";
            InputKeyGroup[14] = "10";
            InputKeyGroup[15] = "11";
            InputKeyGroup[16] = "12";
            InputKeyGroup[17] = "13";
            InputKeyGroup[18] = "14";
            InputKeyGroup[19] = "15";
            InputKeyGroup[20] = "16";
            InputKeyGroup[21] = "17";
            InputKeyGroup[22] = "18";
            InputKeyGroup[23] = "19";
            InputKeyGroup[24] = "20";
            InputKeyGroup[25] = "21";
            InputKeyGroup[26] = "22";
            InputKeyGroup[27] = "23";
            InputKeyGroup[28] = "24";
            InputKeyGroup[29] = "25";
            InputKeyGroup[30] = "26";
            InputKeyGroup[31] = "27";
            InputKeyGroup[32] = "28";
            InputKeyGroup[33] = "29";
            InputKeyGroup[34] = "30";
        }

        void OnDestroy()
        {
            m_controller = null;
        }

        void Update()
        {
            if (m_controller == null)
                return;

            foreach(string element in InputKeyGroup)
            {
                if (element == null)
                    continue;
                string currentMethodKey = "On" + element;
                System.Reflection.MethodInfo localMethodInfo = typeof(IControllerInterface).GetMethod(currentMethodKey);
                if (localMethodInfo == null)
                    continue;
                System.Reflection.ParameterInfo[] localParameterInfo = localMethodInfo.GetParameters();
                bool bisAxisMethod = localParameterInfo.Length > 0;
                if(bisAxisMethod == true)
                {
                    float localValue = UnityEngine.Input.GetAxis(element);
                    object[] localParams = new object[1];
                    localParams[0] = localValue;
                    localMethodInfo.Invoke(m_controller, localParams);
                }
                else
                {
                    localMethodInfo.Invoke(m_controller, null);
                }
            }
        }

        IControllerInterface m_controller = null;
        private string [] InputKeyGroup;
    }
}