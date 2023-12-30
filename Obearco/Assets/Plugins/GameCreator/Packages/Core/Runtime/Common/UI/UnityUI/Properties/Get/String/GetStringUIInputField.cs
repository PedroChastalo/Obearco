using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameCreator.Runtime.Common.UnityUI
{
    [Title("Input Field")]
    [Category("UI/Input Field")]
    
    [Description("Gets the Input Field text value")]
    [Image(typeof(IconUIInputField), ColorTheme.Type.TextLight)]

    [Serializable] [HideLabelsInEditor]
    public class GetStringUIInputField : PropertyTypeGetString
    {
        [SerializeField] private PropertyGetGameObject m_InputField = GetGameObjectInstance.Create();

        public override string Get(Args args)
        {
            GameObject gameObject = this.m_InputField.Get(args);
            if (gameObject == null) return default;

            InputField inputField = gameObject.Get<InputField>();
            return inputField != null ? inputField.text : string.Empty;
        }

        public static PropertyGetString Create => new PropertyGetString(
            new GetStringUIInputField()
        );
        
        public override string String => this.m_InputField.ToString();
    }
}