using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class LocomotionManager : MonoBehaviour
{
    [SerializeField]
    private MoveScheme _moveScheme;
    public MoveScheme moveScheme
    {
        get => _moveScheme;
        set
        {
            SetMoveScheme(value);
            _moveScheme = value;
        }
    }
    [SerializeField]
    private TurnStyle _turnStyle;
    public TurnStyle turnStyle
    {
        get => _turnStyle;
        set
        {
            SetTurnStyle(value);
            turnStyle = value;
        }
    }
    public MoveForwardSource moveForwardSource;

    /// <summary>
    /// These are the input action assets associated with locomotion to affect when the active movement control scheme is selected
    /// </summary>
    [SerializeField]
    private List<InputActionAsset> actionAssets;

    /// <summary>
    /// These are the input action maps associated with locomotion to effect when the active movement control scheme is set
    /// </summary>
    public List<string> actionMaps;

    [SerializeField]
    private List<InputActionReference> actions;

    [SerializeField]
    private string baseControlScheme;

    [SerializeField]
    private string noncontinuousControlScheme;

    [SerializeField]
    private string continuousControlScheme;

    [SerializeField]
    private ContinuousMoveProviderBase continuousMoveProvider;

    [SerializeField]
    private ContinuousTurnProviderBase continuousTurnProvider;

    [SerializeField]
    private SnapTurnProviderBase snapTurnProvider;

    [SerializeField]
    private Transform headForwardSource;

    [SerializeField]
    private Transform leftHandForwardSource;

    [SerializeField]
    private Transform rightHandForwardSource;


    
    void Start()
    {
        
    }

    private void OnEnable()
    {
        SetMoveScheme(moveScheme);
        SetTurnStyle(turnStyle);
        SetMoveForwardSource(moveForwardSource);
    }

    private void OnDisable()
    {
        ClearBindingMasks();
    }


    private void SetMoveScheme(MoveScheme scheme)
    {
        switch (scheme)
        {
            case MoveScheme.Noncontinuous:
                SetBindingMasks(noncontinuousControlScheme);
                if (continuousMoveProvider != null)
                {
                    continuousMoveProvider.enabled = false;
                }

                break;
            case MoveScheme.Continuous:
                SetBindingMasks(continuousControlScheme);
                if (continuousMoveProvider != null)
                {
                    continuousMoveProvider.enabled = true;
                }

                break;
            default:
                throw new InvalidEnumArgumentException(nameof(scheme), (int)scheme, typeof(MoveScheme));
        }
    }

    private void SetTurnStyle(TurnStyle style)
    {
        switch (style)
        {
            case TurnStyle.Snap:
                if (continuousTurnProvider != null)
                {
                    continuousTurnProvider.enabled = false;
                }

                if (snapTurnProvider != null)
                {
                    // TODO: If the Continuous Turn and Snap Turn providers both use the same
                    // action, then disabling the first provider will cause the action to be
                    // disabled, so the action needs to be enabled, which is done by forcing
                    // the OnEnable() of the second provider to be called.
                    // ReSharper disable Unity.InefficientPropertyAccess
                    snapTurnProvider.enabled = false;
                    snapTurnProvider.enabled = true;
                    snapTurnProvider.enableTurnLeftRight = true;
                }
                break;
            case TurnStyle.Continuous:
                if (snapTurnProvider != null)
                {
                    snapTurnProvider.enableTurnLeftRight = false;
                }

                if (continuousTurnProvider != null)
                {
                    continuousTurnProvider.enabled = true;
                }
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(TurnStyle));
        }
    }

    void SetMoveForwardSource(MoveForwardSource forwardSource)
    {
        if (continuousMoveProvider == null)
        {
            Debug.LogError($"Cannot set forward source to {forwardSource}," +
                $" the reference to the {nameof(ContinuousMoveProviderBase)} is missing or the object has been destroyed.", this);
            return;
        }

        switch (forwardSource)
        {
            case MoveForwardSource.Head:
                continuousMoveProvider.forwardSource = headForwardSource;
                break;
            case MoveForwardSource.LeftHand:
                continuousMoveProvider.forwardSource = leftHandForwardSource;
                break;
            case MoveForwardSource.RightHand:
                continuousMoveProvider.forwardSource = rightHandForwardSource;
                break;
            default:
                throw new InvalidEnumArgumentException(nameof(forwardSource), (int)forwardSource, typeof(MoveForwardSource));
        }
    }

    void SetBindingMasks(string controlSchemeName)
    {
        foreach (var actionReference in actions)
        {
            if (actionReference == null)
                continue;

            var action = actionReference.action;
            if (action == null)
            {
                Debug.LogError($"Cannot set binding mask on {actionReference} since the action could not be found.", this);
                continue;
            }

            // Get the (optional) base control scheme and the control scheme to apply on top of base
            var baseInputControlScheme = FindControlScheme(baseControlScheme, actionReference);
            var inputControlScheme = FindControlScheme(controlSchemeName, actionReference);

            action.bindingMask = GetBindingMask(baseInputControlScheme, inputControlScheme);
        }

        if (actionMaps.Count > 0 && actionAssets.Count == 0)
        {
            Debug.LogError($"Cannot set binding mask on action maps since no input action asset references have been set.", this);
        }

        foreach (var actionAsset in actionAssets)
        {
            if (actionAsset == null)
                continue;

            // Get the (optional) base control scheme and the control scheme to apply on top of base
            var baseInputControlScheme = FindControlScheme(baseControlScheme, actionAsset);
            var inputControlScheme = FindControlScheme(controlSchemeName, actionAsset);

            if (actionMaps.Count == 0)
            {
                actionAsset.bindingMask = GetBindingMask(baseInputControlScheme, inputControlScheme);
                continue;
            }

            foreach (var mapName in actionMaps)
            {
                var actionMap = actionAsset.FindActionMap(mapName);
                if (actionMap == null)
                {
                    Debug.LogError($"Cannot set binding mask on \"{mapName}\" since the action map not be found in '{actionAsset}'.", this);
                    continue;
                }

                actionMap.bindingMask = GetBindingMask(baseInputControlScheme, inputControlScheme);
            }
        }
    }

    void ClearBindingMasks()
    {
        SetBindingMasks(string.Empty);
    }

    InputControlScheme? FindControlScheme(string controlSchemeName, InputActionReference action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (string.IsNullOrEmpty(controlSchemeName))
            return null;

        var asset = action.asset;
        if (asset == null)
        {
            Debug.LogError($"Cannot find control scheme \"{controlSchemeName}\" for '{action}' since it does not belong to an {nameof(InputActionAsset)}.", this);
            return null;
        }

        return FindControlScheme(controlSchemeName, asset);
    }

    InputControlScheme? FindControlScheme(string controlSchemeName, InputActionAsset asset)
    {
        if (asset == null)
            throw new ArgumentNullException(nameof(asset));

        if (string.IsNullOrEmpty(controlSchemeName))
            return null;

        var scheme = asset.FindControlScheme(controlSchemeName);
        if (scheme == null)
        {
            Debug.LogError($"Cannot find control scheme \"{controlSchemeName}\" in '{asset}'.", this);
            return null;
        }

        return scheme;
    }

    static InputBinding? GetBindingMask(InputControlScheme? baseInputControlScheme, InputControlScheme? inputControlScheme)
    {
        if (inputControlScheme.HasValue)
        {
            return baseInputControlScheme.HasValue
                ? InputBinding.MaskByGroups(baseInputControlScheme.Value.bindingGroup, inputControlScheme.Value.bindingGroup)
                : InputBinding.MaskByGroup(inputControlScheme.Value.bindingGroup);
        }

        return baseInputControlScheme.HasValue
            ? InputBinding.MaskByGroup(baseInputControlScheme.Value.bindingGroup)
            : (InputBinding?)null;
    }
}

[System.Serializable]
public enum MoveScheme
{
    Noncontinuous,
    Continuous
}
[System.Serializable]
public enum TurnStyle
{
    Snap,
    Continuous
}

[System.Serializable]
public enum MoveForwardSource
{
    Head,
    LeftHand,
    RightHand
}
