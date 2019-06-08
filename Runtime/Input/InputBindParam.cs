namespace JJFramework.Runtime.Input
{
    public class InputBindParam
    {
        public InputBindParam(in object inTarget, in string inMethodName)
        {
            m_target = inTarget;
            m_methodName = inMethodName;
        }

        public object target { get { return m_target; } }
        public string methodName { get { return m_methodName; } }

        object m_target;
        string m_methodName;
    }
}