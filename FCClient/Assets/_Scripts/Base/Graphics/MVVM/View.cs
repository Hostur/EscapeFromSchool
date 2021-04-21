using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Base.Graphics;
using FCData.Attributes;
using FCData.Common;
using FCData.DataUtils;
using FCData.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Graphics.MVVM
{
  public interface IView { }
  /// <summary>
  /// Base class for all the views.
  /// Each view injects its <see cref="ViewModelBase"/> implementation in awake.
  /// </summary>
  /// <typeparam name="T">Registered <see cref="ViewModelBase"/> implementation.</typeparam>
  public abstract partial class View<T> : FCBehaviour, IView where T : ViewModelBase
  {
    [SerializeField]
    [Header("Parent view if view model is the same.")]
    private GameObject _parent = null;

    [FCInject] protected T ViewModel;

    protected override void OnBeforeInjection()
    {
      if (_parent != null) InjectViewModelFromParent();
    }

    protected override async void OnAwake()
    {
      AssertViewModelExists();  
      CacheSelectableUiElements();
      base.OnAwake();
      ViewModel.Initialize();
      CheckForEventHandlerRegistration();
      AssignPropertyBindings();
      AssignTextBindings();
      AssignButtonEnableBindings();
      AssignInputFieldBindings();
      AssignToggleBindings();
      AssignEnableDisableBind();
      await ViewModel.PostInitializeAsync().ConfigureAwait(true);
    }

    private void AssignEnableDisableBind()
    {
      ViewModel.OnEnableDisable += OnEnableDisable;
    }

    protected virtual void OnEnableDisable(bool active)
    {
      gameObject.SetActive(active);
      ViewModel.SetIsActiveIndicator(active);
    }

    /// <summary>
    /// Call view model refresh on view enable.
    /// </summary>
    protected virtual void OnEnable()
    {
      ViewModel.Refresh();
      ViewModel.SetIsActiveIndicator(true);
    }

    private void AssertViewModelExists()
    {
      if (ViewModel != null) return;
      if (_parent != null)
      {
        InjectViewModelFromParent();
      }
      else
      {
        enabled = false;
        throw new Exception($"ViewModel of '{GetType().FullName}' is empty and there is no Parent \n" +
                            $"that can provide any other view model.");
      }
    }

    private void InjectViewModelFromParent()
    {
      View<T> parentView = _parent.GetComponent<View<T>>();
      if (parentView == null)
        throw new Exception($"Parent view on '{gameObject.name}' " +
                            $"on scene '{SceneManager.GetActiveScene().name}' " +
                            $"have a parent which is not a valid View.");
      ViewModel = _parent.GetComponent<View<T>>().ViewModel;
    }

    /// <summary>
    /// Dictionary that contains bindings between ViewModel.PropertyInfo with View.PropertyInfo.
    /// </summary>
    private Dictionary<PropertyInfo, PropertyInfo> _propertyBindings;

    /// <summary>
    /// Bind all the properties witch <see cref="PropertyBindAttribute"/> with matching properties on ViewModel side.
    /// </summary>
    private void AssignPropertyBindings()
    {
      var propertiesToBind = this.GetPropertiesWithAttribute<View<T>, PropertyBindAttribute>();
      _propertyBindings = new Dictionary<PropertyInfo, PropertyInfo>(propertiesToBind.Length);
      for (int i = 0; i < propertiesToBind.Length; i++)
      {
        if (TryToGetAssignableMemberInfo(propertiesToBind[i], ViewModel, out var vieModelProperty))
        {
          _propertyBindings.Add(vieModelProperty, propertiesToBind[i]);
        }
        else
        {
          throw new Exception($"<color=blue>[UI]</color> Invalid binding on property '{propertiesToBind[i].Name}'. \n" +
                              $"There is no ViewModel property that match name and type on '{ViewModel.GetType().Name}'.");
        }
      }

      if (_propertyBindings.Count > 0)
      {
        ViewModel.AssignOnPropertyChanged(NotifyOnPropertyChanged);
      }
    }

    /// <summary>
    /// Dictionary that contains bindings between ViewModel.PropertyInfo with View.PropertyInfo.
    /// </summary>
    private Dictionary<PropertyInfo, FieldInfo> text_bindings;

    /// <summary>
    /// Bind all the Text and TMP_Text witch <see cref="TextBindAttribute"/> with matching properties on ViewModel side.
    /// </summary>
    private void AssignTextBindings()
    {
      var fieldsToBind = this.GetFieldsWithAttribute<View<T>, TextBindAttribute>();
      text_bindings = new Dictionary<PropertyInfo, FieldInfo>(fieldsToBind.Length);
      for (int i = 0; i < fieldsToBind.Length; i++)
      {
        if (TryToGetAssignableMemberInfo(fieldsToBind[i], ViewModel, out var vieModelProperty, false))
        {
          text_bindings.Add(vieModelProperty, fieldsToBind[i]);
        }
        else
        {
          throw new Exception($"<color=blue>[UI]</color> Invalid binding on property '{fieldsToBind[i].Name}'. \n" +
                              $"There is no ViewModel property that match name and type on '{ViewModel.GetType().Name}'.");
        }
      }

      if (text_bindings.Count > 0)
      {
        ViewModel.AssignOnPropertyChanged(NotifyOnPropertyChangedForTextFields);
      }
    }

    /// <summary>
    /// Dictionary that contains bindings between ViewModel.PropertyInfo with View.Button.
    /// When boolean VieModel.Property changed it effects View.Button.interactable.
    /// </summary>
    private Dictionary<PropertyInfo, List<Button>> m_buttonsEnableBindings;

    /// <summary>
    /// Bind all the buttons with <see cref="ButtonBindAttribute"/> with view model function and view model enable property.
    /// </summary>
    private void AssignButtonEnableBindings()
    {
      var buttonsToBind = this.GetFieldsWithAttribute<View<T>, ButtonBindAttribute>();
      m_buttonsEnableBindings = new Dictionary<PropertyInfo, List<Button>>(buttonsToBind.Length);

      for (int i = 0; i < buttonsToBind.Length; i++)
      {
        var button = buttonsToBind[i].GetValue(this) as Button;
        if (button == null)
          throw new Exception("ButtonBindAttribute can be used only on UnityEngine.UI.Button fields.");

        var attribute = buttonsToBind[i].GetCustomAttribute<ButtonBindAttribute>();

        // OnButtonEnableChangedPropertyName is optional binding for buttons.
        var enableViewModelPropertyName = attribute.OnButtonEnableChangedPropertyName;

        if (enableViewModelPropertyName != null)
        {
          if (TryToGetAssignableButtonEnableMemberInfo(enableViewModelPropertyName, ViewModel, out var vieModelProperty))
          {
            if (!m_buttonsEnableBindings.ContainsKey(vieModelProperty))
              m_buttonsEnableBindings.Add(vieModelProperty, new List<Button> { button });
            else
            {
              m_buttonsEnableBindings[vieModelProperty].Add(button);
            }
          }
          else
          {
            throw new Exception($"<color=blue>[UI]</color> Invalid binding on button '{buttonsToBind[i].Name}'. \n" +
                                $"There is no ViewModel boolean property that match name '{enableViewModelPropertyName}'.");
          }
        }

        var functionName = attribute.OnButtonClickFunctionName;
        if (TryToGetAssignableFunction(functionName, ViewModel, out MethodInfo method))
        {
          button.onClick.AddListener(() => method.Invoke(ViewModel, null));
        }
        else
        {
          throw new Exception($"<color=blue>[UI]</color> Invalid binding on button '{buttonsToBind[i].Name}'. \n" +
                              $"There is no ViewModel function that match name '{functionName}'.");
        }
      }

      if (m_buttonsEnableBindings.Count > 0)
      {
        ViewModel.AssignOnPropertyChanged(NotifyOnButtonEnableChanged);
      }
    }

    private void AssignToggleBindings()
    {
      var togglesForBind = this.GetFieldsWithAttribute<View<T>, ToggleBindAttribute>();

      for (int i = 0; i < togglesForBind.Length; i++)
      {
        var toggle = togglesForBind[i].GetValue(this) as Toggle;
        if (toggle == null)
          throw new Exception($"<color=blue>[UI]</color> ToggleBindAttribute can be used only on UnityEngine.UI.Toggle fields.\n" +
                              $"Field '{togglesForBind[i].Name}' in '{GetType().Name}'.");

        var attribute = togglesForBind[i].GetCustomAttribute<ToggleBindAttribute>();
        string viewModelBooleanPropertyName = attribute.ViewModelStringProperty;

        if (TryToGetAssignableMemberInfo<T, bool>(viewModelBooleanPropertyName, ViewModel,
          out PropertyInfo vieModelProperty))
        {
          if (!vieModelProperty.CanWrite)
            throw new Exception($"<color=blue>[UI]</color> ViewModel property must have setter if you want to bind it with toggle field.\n" +
                                $"Field '{togglesForBind[i].Name}' in '{GetType().Name}'.");

          toggle.onValueChanged.AddListener(delegate
          {
            vieModelProperty.SetValue(ViewModel, toggle.isOn);
          });
        }
        else
        {
          throw new Exception($"There is no boolean property '{viewModelBooleanPropertyName}' in '{ViewModel.GetType().Name}'.\n" +
                              $"Field '{togglesForBind[i].Name}' in '{GetType().Name}'.");
        }
      }
    }

    private void AssignInputFieldBindings()
    {
      var inputFieldsForBind = this.GetFieldsWithAttribute<View<T>, InputFieldBindAttribute>();

      for (int i = 0; i < inputFieldsForBind.Length; i++)
      {
        var check = inputFieldsForBind[i].GetValue(this);
        if (check is InputField)
        {
          InputField(check as InputField, inputFieldsForBind[i]);
        }
        else if (check is TMP_InputField)
        {
          TMP_InputField(check as TMP_InputField, inputFieldsForBind[i]);
        }
        else
        {
          throw new Exception(
            $"<color=blue>[UI]</color> InputFieldBindAttribute can be used only on UnityEngine.UI.InputField fields or TMP_InputFields.\n" +
            $"Field '{inputFieldsForBind[i].Name}' in '{GetType().Name}'.");
        }
      }

      void TMP_InputField(TMP_InputField tmpInputField, FieldInfo fieldInfo)
      {
        if (tmpInputField == null)
          throw new Exception(
            $"<color=blue>[UI]</color> InputFieldBindAttribute can be used only on UnityEngine.UI.InputField fields or TMP_InputFields.\n" +
            $"Field '{fieldInfo.Name}' in '{GetType().Name}'.");

        var attribute = fieldInfo.GetCustomAttribute<InputFieldBindAttribute>();
        string viewModelStringPropertyName = attribute.ViewModelStringProperty;

        if (TryToGetAssignableMemberInfo<T, string>(viewModelStringPropertyName, ViewModel,
          out PropertyInfo vieModelProperty))
        {
          if (!vieModelProperty.CanWrite)
            throw new Exception(
              $"<color=blue>[UI]</color> ViewModel property must have setter if you want to bind it with input field or TMP_InputFields.\n" +
              $"Field '{fieldInfo.Name}' in '{GetType().Name}'.");

          tmpInputField.onValueChanged.AddListener(delegate
          {
            vieModelProperty.SetValue(ViewModel, tmpInputField.text);
          });
        }
        else
        {
          throw new Exception(
            $"There is no string property '{viewModelStringPropertyName}' in '{ViewModel.GetType().Name}'.\n" +
            $"Field '{fieldInfo.Name}' in '{GetType().Name}'.");
        }
      }

      void InputField(InputField inputField, FieldInfo fieldInfo)
      {
        if (inputField == null)
          throw new Exception(
            $"<color=blue>[UI]</color> InputFieldBindAttribute can be used only on UnityEngine.UI.InputField fields or TMP_InputFields.\n" +
            $"Field '{fieldInfo.Name}' in '{GetType().Name}'.");

        var attribute = fieldInfo.GetCustomAttribute<InputFieldBindAttribute>();
        string viewModelStringPropertyName = attribute.ViewModelStringProperty;

        if (TryToGetAssignableMemberInfo<T, string>(viewModelStringPropertyName, ViewModel,
          out PropertyInfo vieModelProperty))
        {
          if (!vieModelProperty.CanWrite)
            throw new Exception(
              $"<color=blue>[UI]</color> ViewModel property must have setter if you want to bind it with input field or TMP_InputFields.\n" +
              $"Field '{fieldInfo.Name}' in '{GetType().Name}'.");

          inputField.onValueChanged.AddListener(delegate
          {
            vieModelProperty.SetValue(ViewModel, inputField.text);
          });
        }
        else
        {
          throw new Exception(
            $"There is no string property '{viewModelStringPropertyName}' in '{ViewModel.GetType().Name}'.\n" +
            $"Field '{fieldInfo.Name}' in '{GetType().Name}'.");
        }
      }
    }

    /// <summary>
    /// Fire when any property on view model changed and check if any button is assigned to that property.
    /// </summary>
    /// <param name="vieModelButtonEnableProperty">Boolean property provided by view model when it changed.</param>
    private void NotifyOnButtonEnableChanged(PropertyInfo vieModelButtonEnableProperty)
    {
      if (!m_buttonsEnableBindings.ContainsKey(vieModelButtonEnableProperty))
      {
        return;
      }

      try
      {
        bool interactable = (bool) vieModelButtonEnableProperty?.GetValue(ViewModel);

        if (m_buttonsEnableBindings != null && m_buttonsEnableBindings.ContainsKey(vieModelButtonEnableProperty))
          m_buttonsEnableBindings[vieModelButtonEnableProperty].Where(c => c != null && c.gameObject != null).Each(c => c.interactable = interactable);
        
        // Experimental
        // Each disabled button, with SpriteSwap transition mode without assigned disabled sprite receive disabled color
        m_buttonsEnableBindings[vieModelButtonEnableProperty]
          .Where(b => b != null && b.transition == Selectable.Transition.SpriteSwap && b.spriteState.disabledSprite == null)
          .Each((b) =>
          {
            b.SetColor(interactable ? new Color(1, 1, 1, 1) : new Color(0.5f, 0.5f, 0.5f, 0.5f));
          });
      }
      catch (Exception e)
      {
        throw new Exception(
          $"Cannot resolve runtime NotifyOnButtonEnableChanged in View({this.GetType().Name}) on property '{vieModelButtonEnableProperty.Name}'. {e.Message}");
      }
    }

    /// <summary>
    /// When view model property changed it provide it through this notificator.
    /// If current view is assigned to this property we push its value into the view. 
    /// </summary>
    /// <param name="vieModelProperty">Property provided by view model when it changed.</param>
    private void NotifyOnPropertyChanged(PropertyInfo vieModelProperty)
    {
      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      if (this == null || !_propertyBindings.ContainsKey(vieModelProperty))
      {
        return;
      }

      try
      {
        var viewProperty = _propertyBindings[vieModelProperty];
        var modelProperty = _propertyBindings.Keys.FirstOrDefault(b => b == vieModelProperty);

        if (modelProperty == null)
        {
          throw new Exception($"There is no matching property info in '{this.GetType().Name}' " +
                              $"bindings for property changed '{vieModelProperty.Name}'.");
        }

        if (viewProperty.CanWrite)
          viewProperty.SetValue(this, modelProperty.GetValue(ViewModel));
      }
      catch (Exception e)
      {
        throw new Exception(
          $"Cannot resolve runtime NotifyOnPropertyChanged in View({this.GetType().Name}) on property '{vieModelProperty.Name}'. {e.Message}");
      }
    }

    /// <summary>
    /// When view model property changed it provide it through this notificator.
    /// If current view is assigned to this property we push its value into the view. 
    /// </summary>
    /// <param name="vieModelProperty">Property provided by view model when it changed.</param>
    private void NotifyOnPropertyChangedForTextFields(PropertyInfo vieModelProperty)
    {
      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      if (this == null || !text_bindings.ContainsKey(vieModelProperty))
      {
        return;
      }

      try
      {
        var viewTextField = text_bindings[vieModelProperty];
        var modelProperty = text_bindings.Keys.FirstOrDefault(b => b == vieModelProperty);

        if (modelProperty == null)
        {
          throw new Exception($"There is no matching property info in '{this.GetType().Name}' " +
                              $"bindings for text property changed '{vieModelProperty.Name}'.");
        }

        var tmp = viewTextField.GetValue(this);
        if (tmp is Text)
        {
          ((Text) tmp).text = (string)modelProperty.GetValue(ViewModel);
        }
        else if (tmp is TMP_Text)
        {
          ((TMP_Text)tmp).text = (string)modelProperty.GetValue(ViewModel);
        }
        else
        {
          throw new Exception($"Text binding attribute can be used only for UnityEngine.UI.Text or TMP_Text fields. View({this.GetType().Name}) on property '{vieModelProperty.Name}");
        }
      }
      catch (Exception e)
      {
        throw new Exception(
          $"Cannot resolve runtime NotifyOnPropertyChangedForTextFields in View({this.GetType().Name}) on property '{vieModelProperty.Name}'. {e.Message}");
      }
    }

    public List<UIBehaviour> SelectableUiElements { get; private set; }

    private void CacheSelectableUiElements()
    {
      try
      {
        var selectableMembers = this.GetMembersInfoWithAttribute<View<T>, SelectableElementAttribute>();
        selectableMembers =
          selectableMembers.OrderBy(m => m.GetCustomAttribute<SelectableElementAttribute>().Order).ToArray();

        SelectableUiElements = new List<UIBehaviour>(selectableMembers.Length);

        for (int i = 0; i < selectableMembers.Length; i++)
        {
          if (selectableMembers[i].MemberType == MemberTypes.Field)
          {
            UIBehaviour behaviour = (selectableMembers[i] as FieldInfo).GetValue(this) as UIBehaviour;
            if (behaviour == null)
            {
              throw new Exception(
                $"<color=blue>[UI]</color> Only UIBehaviours may be decorated by SelectableElementAttribute.\n" +
                $"Field '{selectableMembers[i].Name}' in '{GetType().Name}'.");
            }
            SelectableUiElements.Add(behaviour);
          }
          else
          {
            UIBehaviour behaviour = (selectableMembers[i] as PropertyInfo).GetValue(this) as UIBehaviour;
            if (behaviour == null)
            {
              throw new Exception(
                $"<color=blue>[UI]</color> Only UIBehaviours may be decorated by SelectableElementAttribute.\n" +
                $"Property '{selectableMembers[i].Name}' in '{GetType().Name}'.");
            }
            SelectableUiElements.Add(behaviour);
          }
        }
      }
      catch (Exception e)
      {
        this.Log(
          $"<color=blue>[UI]</color> Exception occur during caching selectable elements on View {GetType().Name}. \n{e.Message}.", LogLevel.Error);
      }
    }
  }
}
