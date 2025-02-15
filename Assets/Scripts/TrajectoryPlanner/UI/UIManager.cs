using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    #region Components

    [FormerlySerializedAs("EditorFocusableInputs")] [SerializeField]
    private List<TMP_InputField> _editorFocusableInputs;

    [FormerlySerializedAs("EditorFocusableGOs")] [SerializeField]
    private List<GameObject> _editorFocusableGOs;

    [SerializeField] private List<TMP_Text> _whiteUIText;

    [SerializeField] private GameObject _automaticControlPanelGameObject;

    #endregion

    #region Properties

    public static readonly HashSet<TMP_InputField> FocusableInputs = new();
    public static readonly HashSet<GameObject> FocusableGOs = new();

    #endregion


    private void Awake()
    {
        FocusableInputs.UnionWith(_editorFocusableInputs);
        FocusableGOs.UnionWith(_editorFocusableGOs);
    }

    public static bool InputsFocused
    {
        get { return FocusableInputs.Any(x => x != null ? x.isFocused : false) || FocusableGOs.Any(x => x != null ? x.activeSelf : false); }
    }

    public void EnableAutomaticManipulatorControlPanel(bool enable = true)
    {
        _automaticControlPanelGameObject.SetActive(enable);
    }

    public void SetBackgroundWhite(bool state)
    {
        if (state)
        {
            foreach (TMP_Text textC in _whiteUIText)
                textC.color = Color.black;
            Camera.main.backgroundColor = Color.white;
        }
        else
        {
            foreach (TMP_Text textC in _whiteUIText)
                textC.color = Color.white;
            Camera.main.backgroundColor = Color.black;
        }
    }
}