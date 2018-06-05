using UnityEngine;

namespace JJFramework.Runtime.Util
{
    //public class PlayerPrefsType
    //{
    //}

    public class PrefsBool : PrefsType<bool>
    {
        public bool Value
        {
            get
            {
                _value = PlayerPrefs.GetInt(_key) == 1;
                return _value;
            }
            set
            {
                PlayerPrefs.SetInt(_key, value == false ? 0 : 1);
                _value = value;
            }
        }

        public PrefsBool(string key, bool value) : base(key, value) { }
    }

    public class PrefsInt : PrefsType<int>
    {
        public int Value
        {
            get
            {
                _value = PlayerPrefs.GetInt(_key);
                return _value;
            }
            set
            {
                PlayerPrefs.SetInt(_key, value);
                _value = value;
            }
        }

        public PrefsInt(string key, int value) : base(key, value) { }
    }

    public class PrefsFloat : PrefsType<float>
    {
        public float Value
        {
            get
            {
                _value = PlayerPrefs.GetFloat(_key);
                return _value;
            }
            set
            {
                PlayerPrefs.SetFloat(_key, value);
                _value = value;
            }
        }

        public PrefsFloat(string key, float value) : base(key, value) { }
    }

    public class PrefsString : PrefsType<string>
    {
        public string Value
        {
            get
            {
                _value = PlayerPrefs.GetString(_key);
                return _value;
            }
            set
            {
                PlayerPrefs.SetString(_key, value);
                _value = value;
            }
        }

        public PrefsString(string key, string value) : base(key, value) { }
    }

    public class PrefsType<T>
    {
        protected string _key;
        protected T _value;

        public PrefsType(string key, T value)
        {
            _key = key;
            _value = value;
        }
    }
}
