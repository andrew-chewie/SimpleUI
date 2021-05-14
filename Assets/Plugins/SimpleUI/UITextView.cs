using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace SimpleUI
{
    [DisallowMultipleComponent]
    public class UITextView : UIBaseView
    {
        public bool UseSmallAfterDecimal;
        public bool UseShowSign;

        public string BeforeValue = "";
        public string AfterValue = "";

        public bool HideIfEmpty = false;
        
        [HideInInspector]
        public Action<float> OnValueChange;

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        const string SignPlus = "+";

        private string SetSmallAfterDecimal(float value)
        {
            int intPart = (int) value;
            float fractionalPart = value - intPart;
            float _smallFontSize = _tmpTextUGUI.fontSize * 0.75f;
            float _bigFontSize = _tmpTextUGUI.fontSize * 1.25f;

            var builder = new StringBuilder();
            builder.Append(intPart + "." + "<size=" + _smallFontSize + ">" + fractionalPart.ToString("0.0#").Remove(0,2) + "</size>");

            return builder.ToString();
        }

        private string SetSign(float val)
        {
            if (val > 0)
            {
                return SignPlus;
            }

            return string.Empty;
        }

        private Text _text;
        private TextMeshProUGUI _tmpTextUGUI;
        private TextMeshPro _tmpText;
        //private UITextTag _textTag;

        public TextMeshProUGUI TextMeshProUGUI => _tmpTextUGUI;
        public TextMeshPro TextMeshPro => _tmpText;

        private Dictionary<string, int> _variables;

        public void SetID(long id, Dictionary<string, int> variables)
        {
            _variables = variables;
            SetID(id);
        }

        public override void SetID(long id)
        {
            Profiler.BeginSample("TextView - SetID - " + this.name, this);

            Init();
            base.SetID(id);
            SetValue(id);
            
            Profiler.EndSample();
        }

        public override void SetValue(string value)
        {
            Profiler.BeginSample("TextView - SetValue - " + this.name, this);

            Init();

            if (HideIfEmpty)
            {
                if (value.IsNullOrEmpty())
                {
                    Hide();
                }
                else
                {
                    Show();
                }
            }
                
            var text = BeforeValue + value + AfterValue;

            if (_text != null)
                _text.text = text;

            if (_tmpTextUGUI != null)
                _tmpTextUGUI.text = text;

            if (_tmpText != null)
                _tmpText.text = text;
            
            Profiler.EndSample();
        }

        public override void SetValue(float value)
        {
            Init();

            SetFormat(value);

            OnValueChange?.Invoke(value);
        }

        public override void SetValue(int value)
        {
            Init();

            SetFormat(value);

            OnValueChange?.Invoke(value);
        }

        private void SetFormat(float value)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(value.ToString("0.0#"));

            if (UseSmallAfterDecimal)
            {
                _stringBuilder.Clear();
                _stringBuilder.Append(SetSmallAfterDecimal(value));
            }

            if (UseShowSign)
            {
                _stringBuilder.Insert(0,SetSign(value));
            }

            SetValue(_stringBuilder.ToString());
        }

        private void SetFormat(int value)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(value);

            if (UseShowSign)
            {
                _stringBuilder.Insert(0, SetSign(value));
            }

            SetValue(_stringBuilder.ToString());
        }

        public override void Init()
        {
            if (isInit)
                return;

            base.Init();

            _text = GetComponent<Text>();
            _tmpText = GetComponent<TextMeshPro>();
            _tmpTextUGUI = GetComponent<TextMeshProUGUI>();

            CheckTag();
        }

        private readonly string _tagRequired = "<b><color=#ff0000>[TAG]</color></b> ";

        private void CheckTag()
        {
            Init();

            if (_text != null && !_text.text.IsNullOrEmpty())
            {
                _text.text = _text.text.Insert(0, _tagRequired);
            }
            else if (_tmpTextUGUI != null && !_tmpTextUGUI.text.IsNullOrEmpty())
            {
                _tmpTextUGUI.text = _tmpTextUGUI.text.Insert(0, _tagRequired);
            }
            else if (_tmpText != null && !_tmpText.text.IsNullOrEmpty())
            {
                _tmpText.text = _tmpText.text.Insert(0, _tagRequired);
            }
        }
    }
}