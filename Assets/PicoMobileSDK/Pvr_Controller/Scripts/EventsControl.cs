using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制全局事件
/// </summary>
public class EventsControl : MonoBehaviour {
    public Pvr_ControllerEvents LeftHand;
    public Pvr_ControllerEvents RightHand;
    public Grab_Interact LeftHandControl;
    public Grab_Interact RightHandControl;
    public bool AllowGrab = true;
    public bool AllowTouching = true;
    private void Awake()
    {
        LeftHand.eventManager = this;
        RightHand.eventManager = this;
        LeftHandControl.eventManager = this;
        RightHandControl.eventManager = this;
    }
}
