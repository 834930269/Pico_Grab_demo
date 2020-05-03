using Pvr_UnitySDKAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ControllerInteractionEventArgs
{
    /// <summary>
    /// 当前操作的id
    /// </summary>
    public int hid;
}
public delegate void ControllerInteractionEventHandler(object sender, ControllerInteractionEventArgs e);

public class Pvr_ControllerEvents : MonoBehaviour {
    public EventsControl eventManager;
    public Grab_Interact gi;
    public enum ButtonAlias
    {
        Undefined,
        Trigger,
        TriggerPressed,
        TriggerReleased,
        Grip,
        GripClicked,
        GripUnClicked,
        A,
        AClicked,
        AUnClicked,
        B,
        BClicked,
        BUnClicked,
        X,
        XClicked,
        XUnClicked,
        Y,
        YClicked,
        YUnClicked
    }
    #region controller triger events
    public event ControllerInteractionEventHandler TriggerPressed;
    public event ControllerInteractionEventHandler TriggerReleased;
    #endregion
    #region controller grip events
    /// <summary>
    /// Emitted when the grip is squeezed about half way in.
    /// </summary>
    public event ControllerInteractionEventHandler GripPressed;
    /// <summary>
    /// Emitted when the grip is released under half way.
    /// </summary>
    public event ControllerInteractionEventHandler GripReleased;
    /// <summary>
    /// Emitted when the grip is squeezed all the way down.
    /// </summary>
    public event ControllerInteractionEventHandler GripClicked;
    /// <summary>
    /// Emitted when the grip is no longer being held all the way down.
    /// </summary>
    public event ControllerInteractionEventHandler GripUnclicked;
    #endregion

    #region controller A,B,X,Y events
    public event ControllerInteractionEventHandler AClicked;
    public event ControllerInteractionEventHandler AUnclicked;
    public event ControllerInteractionEventHandler BClicked;
    public event ControllerInteractionEventHandler BUnclicked;
    public event ControllerInteractionEventHandler XClicked;
    public event ControllerInteractionEventHandler XUnclicked;
    public event ControllerInteractionEventHandler YClicked;
    public event ControllerInteractionEventHandler YUnclicked;
    #endregion

    #region events methods
    public virtual ControllerInteractionEventArgs SetControllerEvent()
    {
        ControllerInteractionEventArgs e=new ControllerInteractionEventArgs();
        
        return e;
    }
    public virtual ControllerInteractionEventArgs SetControllerEvent(int hid)
    {
        ControllerInteractionEventArgs e = new ControllerInteractionEventArgs();
        e.hid = hid;
        return e;
    }
    public virtual ControllerInteractionEventArgs SetControllerEvent(ref bool buttonBool, bool value = false, float buttonPressure = 0f)
    {
        //VRTK_ControllerReference controllerReference = VRTK_ControllerReference.GetControllerReference(gameObject);
        //buttonBool = value;
        ControllerInteractionEventArgs e=new ControllerInteractionEventArgs();
        //e.controllerReference = controllerReference;
        //e.buttonPressure = buttonPressure;
        //e.touchpadAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.Touchpad, controllerReference);
        //e.touchpadAngle = CalculateVector2AxisAngle(e.touchpadAxis);
        //e.touchpadTwoAxis = VRTK_SDK_Bridge.GetControllerAxis(SDK_BaseController.ButtonTypes.TouchpadTwo, controllerReference);
        //e.touchpadTwoAngle = CalculateVector2AxisAngle(e.touchpadTwoAxis);
        return e;
    }
    public virtual void OnTriggerPressed(ControllerInteractionEventArgs e)
    {
        if (TriggerPressed != null)
        {
            TriggerPressed(this, e);
        }
    }
    public virtual void OnTriggerReleased(ControllerInteractionEventArgs e)
    {
        if (TriggerPressed != null)
        {
            TriggerReleased(this, e);
        }
    }

    public virtual void OnGripPressed(ControllerInteractionEventArgs e)
    {
        if (GripPressed != null)
        {
            GripPressed(this, e);
        }
    }
    public virtual void OnGripReleased(ControllerInteractionEventArgs e)
    {
        if (GripPressed != null)
        {
            GripReleased(this, e);
        }
    }
    public virtual void OnGripClicked(ControllerInteractionEventArgs e)
    {
        if (GripClicked != null)
        {
            GripClicked(this, e);
        }
    }
    public virtual void OnGripUnclicked(ControllerInteractionEventArgs e)
    {
        if (GripUnclicked != null)
        {
            GripUnclicked(this, e);
        }
    }

    public virtual void OnAClicked(ControllerInteractionEventArgs e)
    {
        if (AClicked != null)
        {
            AClicked(this, e);
        }
    }
    public virtual void OnAUnclicked(ControllerInteractionEventArgs e)
    {
        if (AUnclicked != null)
        {
            AUnclicked(this, e);
        }
    }
    public virtual void OnBClicked(ControllerInteractionEventArgs e)
    {
        if (BClicked != null)
        {
            BClicked(this, e);
        }
    }
    public virtual void OnBUnclicked(ControllerInteractionEventArgs e)
    {
        if (BUnclicked != null)
        {
            BUnclicked(this, e);
        }
    }
    public virtual void OnXClicked(ControllerInteractionEventArgs e)
    {
        if (XClicked != null)
        {
            XClicked(this, e);
        }
    }
    public virtual void OnXUnclicked(ControllerInteractionEventArgs e)
    {
        if (XUnclicked != null)
        {
            XUnclicked(this, e);
        }
    }
    public virtual void OnYClicked(ControllerInteractionEventArgs e)
    {
        if (YClicked != null)
        {
            YClicked(this, e);
        }
    }
    public virtual void OnYUnclicked(ControllerInteractionEventArgs e)
    {
        if (YUnclicked != null)
        {
            YUnclicked(this, e);
        }
    }

    #endregion

    #region subscription managers
    /// <summary>
    /// 为扳机事件增加回调函数
    /// </summary>
    /// <param name="givenButton">注册事件的按键</param>
    /// <param name="startEvent">true为开始按键,false为停止按键</param>
    /// <param name="callbackMethod">回调函数</param>
    public virtual void SubscribeToButtonAliasEvent(ButtonAlias givenButton, bool startEvent, ControllerInteractionEventHandler callbackMethod)
    {
        ButtonAliasEventSubscription(true, givenButton, startEvent, callbackMethod);
    }
    public virtual void UnsubscribeToButtonAliasEvent(ButtonAlias givenButton, bool startEvent, ControllerInteractionEventHandler callbackMethod)
    {
        ButtonAliasEventSubscription(false, givenButton, startEvent, callbackMethod);
    }
    /// <summary>
    /// 更原始的添加方案,subscribe是决定是添加还是删除事件
    /// </summary>
    /// <param name="subscribe"></param>
    /// <param name="givenButton"></param>
    /// <param name="startEvent"></param>
    /// <param name="callbackMethod"></param>
    protected virtual void ButtonAliasEventSubscription(bool subscribe, ButtonAlias givenButton, bool startEvent, ControllerInteractionEventHandler callbackMethod)
    {
        //Debug.Log("开始注册抓取事件: " + givenButton.ToString());
        switch (givenButton)
        {
            case ButtonAlias.Trigger:
                if (subscribe)
                {
                    if (startEvent)
                    {
                        TriggerPressed += callbackMethod;
                    }else
                    {
                        TriggerReleased += callbackMethod;
                    }
                }else
                {
                    if (startEvent)
                    {
                        TriggerPressed -= callbackMethod;
                    }else
                    {
                        TriggerReleased -= callbackMethod;
                    }
                }
                break;
            case ButtonAlias.Grip:
                if (subscribe)
                {
                    if (startEvent)
                    {
                        //Debug.Log("抓取事件注册完毕");
                        GripClicked += callbackMethod;
                    }else
                    {
                        //Debug.Log("释放事件注册完毕");
                        GripUnclicked += callbackMethod;
                    }
                }else
                {
                    if (startEvent)
                    {
                        GripClicked -= callbackMethod;
                    }
                    else
                    {
                        GripUnclicked -= callbackMethod;
                    }
                }
                break;
            case ButtonAlias.A:
                if (subscribe)
                {
                    if (startEvent)
                    {
                        AClicked += callbackMethod;
                    }else
                    {
                        AUnclicked += callbackMethod;
                    }
                }else
                {
                    if (startEvent)
                    {
                        AClicked -= callbackMethod;
                    }
                    else
                    {
                        AUnclicked -= callbackMethod;
                    }
                }
                break;
            case ButtonAlias.B:
                if (subscribe)
                {
                    if (startEvent)
                    {
                        BClicked += callbackMethod;
                    }
                    else
                    {
                        BUnclicked += callbackMethod;
                    }
                }
                else
                {
                    if (startEvent)
                    {
                        BClicked -= callbackMethod;
                    }
                    else
                    {
                        BUnclicked -= callbackMethod;
                    }
                }
                break;
            case ButtonAlias.X:
                if (subscribe)
                {
                    if (startEvent)
                    {
                        XClicked += callbackMethod;
                    }
                    else
                    {
                        XUnclicked += callbackMethod;
                    }
                }
                else
                {
                    if (startEvent)
                    {
                        XClicked -= callbackMethod;
                    }
                    else
                    {
                        XUnclicked -= callbackMethod;
                    }
                }
                break;
            case ButtonAlias.Y:
                if (subscribe)
                {
                    if (startEvent)
                    {
                        YClicked += callbackMethod;
                    }
                    else
                    {
                        YUnclicked += callbackMethod;
                    }
                }
                else
                {
                    if (startEvent)
                    {
                        YClicked -= callbackMethod;
                    }
                    else
                    {
                        YUnclicked -= callbackMethod;
                    }
                }
                break;
        }
    }
    #endregion

    protected virtual void OnEnable()
    {

    }
    protected virtual void OnDisable()
    {
    }

    protected virtual void OnDestroy()
    {
    }

    protected virtual void Update()
    {
        CheckTriggerEvents();
        CheckGripEvents();
        CheckAEvents();
        CheckBEvents();
        CheckXEvents();
        CheckYEvents();
    }

    #region CheckMethod
    protected virtual void CheckTriggerEvents()
    {
        if(Controller.UPvr_GetKey(0,Pvr_KeyCode.TRIGGER) || Controller.UPvr_GetKey(1, Pvr_KeyCode.TRIGGER))
        {
            OnTriggerPressed(SetControllerEvent());
        }
        if(Controller.UPvr_GetKeyUp(0,Pvr_KeyCode.TRIGGER) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.TRIGGER))
        {
            OnTriggerReleased(SetControllerEvent());
        }
    }
    /// <summary>
    /// check检测那个手柄的Grip按钮
    /// </summary>
    protected virtual void CheckGripEvents()
    {
        Pvr_KeyCode gkc = (gi.hid == 0) ? Pvr_KeyCode.Left : Pvr_KeyCode.Right;
        if (ControllerMsg.TouchingHandId == gi.hid)
        {
            if (Controller.UPvr_GetKeyDown(gi.hid, gkc) || Input.GetKey(KeyCode.X))
            {
                //Debug.Log("按下开始抓取");
                OnGripClicked(SetControllerEvent(gi.hid));
            }

            if (Controller.UPvr_GetKeyUp(gi.hid, gkc) || Input.GetKey(KeyCode.X))
            {
                //Debug.Log("松开开始释放");
                OnGripUnclicked(SetControllerEvent(gi.hid));
            }
        }
    }
    protected virtual void CheckAEvents()
    {
        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.A) || Controller.UPvr_GetKey(1, Pvr_KeyCode.A))
        {
            OnAClicked(SetControllerEvent());
        }

        if (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.A) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.A))
        {
            OnAUnclicked(SetControllerEvent());
        }
    }
    protected virtual void CheckBEvents()
    {
        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.B) || Controller.UPvr_GetKey(1, Pvr_KeyCode.B))
        {
            OnBClicked(SetControllerEvent());
        }

        if (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.B) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.B))
        {
            OnBUnclicked(SetControllerEvent());
        }
    }
    protected virtual void CheckXEvents()
    {
        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.X) || Controller.UPvr_GetKey(1, Pvr_KeyCode.X))
        {
            OnXClicked(SetControllerEvent());
        }

        if (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.X) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.X))
        {
            OnXUnclicked(SetControllerEvent());
        }
    }
    protected virtual void CheckYEvents()
    {
        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.Y) || Controller.UPvr_GetKey(1, Pvr_KeyCode.Y))
        {
            OnYClicked(SetControllerEvent());
        }

        if (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.Y) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.Y))
        {
            OnYUnclicked(SetControllerEvent());
        }
    }
    #endregion
}
