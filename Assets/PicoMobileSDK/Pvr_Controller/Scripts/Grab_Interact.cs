using Pvr_UnitySDKAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct GrabEventArgs{
    public GameObject Grabbed;
    public int TakeHand;
}
/// <summary>
/// 暂未选择如何赋值
/// 1是右手
/// 0是左手
/// </summary>
public struct ControllerMsg {
    public static int TouchingHandId;
    public static int GrabbingHandId;
    public static GameObject GrabbingHandObject;
    public static GameObject TouchingHandObject;
    public static GameObject primaryGrabHandObject;
    public static GameObject secondaryGrabHandObject;
}

public delegate void GrabEventHandler(object sender, GrabEventArgs e);
public class Grab_Interact : MonoBehaviour {
    public static Pvr_KeyCode Xpress = Pvr_KeyCode.X;
    /// <summary>
    /// 事件控制器,用于控制事件是否执行
    /// </summary>
    public EventsControl eventManager;
    public Text DebugText;
    [Tooltip("The rigidbody point on the controller model to snap the grabbed Interactable Object to. If blank it will be set to the SDK default.")]
    public Rigidbody controllerAttachPoint = null;
    //[Tooltip("The Controller Events to listen for the events on. If the script is being applied onto a controller then this parameter can be left blank as it will be auto populated by the controller the script is on at runtime.")]
    //public VRTK_ControllerEvents controllerEvents;
    /// <summary>
    /// 这个是哪个controller
    /// </summary>
    public int hid;
    public static int hand_id;
    //[Tooltip("是否允许抓取")]
    //public bool isGrabbed = true;
    //[Tooltip("是否允许换手")]
    //public bool isSwapable = false;

    public event GrabEventHandler GrabStart;
    public event GrabEventHandler GrabEnd;
    public event GrabEventHandler TouchingStart;
    public event GrabEventHandler TouchingOn;
    public event GrabEventHandler TouchingStartEnd;
    public event GrabEventHandler TouchingOnEnd;
    public event GrabEventHandler TouchingEnd;
    private static bool GetGrabKey(int id)
    {
        return Controller.UPvr_GetKey(id,Xpress) ||
            Input.GetKey(KeyCode.X);
    }
    /// <summary>
    /// 获取键弹起的事件
    /// </summary>
    /// <param name="id">哪只手</param>
    /// <returns></returns>
    private static bool GetGrabKeyUp(int id)
    {
        return Controller.UPvr_GetKeyUp(id, Xpress) ||
            Input.GetKeyUp(KeyCode.X);
    }
    #region 获取接触物体
    public GameObject touchedObject = null;
    public List<Collider> touchedObjectColliders = new List<Collider>();
    public List<Collider> touchedObjectActiveColliders = new List<Collider>();
    public bool triggerIsColliding = false;

    public virtual void OnTriggerStay(Collider collider)
    {
        //Debug.Log("手柄停留");
        ControllerMsg.TouchingHandId = hid;
        ControllerMsg.TouchingHandObject = this.gameObject;
        GameObject colliderInteractableObject = TriggerStart(collider);
        if(touchedObject == null || collider.transform.IsChildOf(touchedObject.transform))
        {
            triggerIsColliding = true;
        }
        if(touchedObject == null && colliderInteractableObject != null && IsObjectInteractable(collider.gameObject))
        {
            touchedObject = colliderInteractableObject;
            InteractableObject touchedObjectScript = touchedObject.GetComponent<InteractableObject>();
            //这里实现禁止某个控制器抓取
            //end
            OnControllerStartTouchInteractableObject(SetControllerInteractEvent(touchedObject));
            StoreTouchedObjectColliders(collider);

            touchedObjectScript.StartTouching(this);

            OnControllerTouchInteractableObject(SetControllerInteractEvent(touchedObject));
        }
    }
    public virtual void OnTriggerExit(Collider collider)
    {
        touchedObjectActiveColliders.Remove(collider);
    }
    public virtual void OnTriggerEnter(Collider collider)
    {
        Debug.Log("手柄进入");
        GameObject colliderInteractableObject = TriggerStart(collider);
        InteractableObject touchedObjectScript = (touchedObject != null ? touchedObject.GetComponent<InteractableObject>() : null);

        if(touchedObject!=null && colliderInteractableObject!=null && touchedObject!=colliderInteractableObject && touchedObjectScript != null && !touchedObjectScript.IsGrabbed())
        {

        }
    }
    public virtual void OnControllerTouchInteractableObject(GrabEventArgs e)
    {
        if (TouchingOn != null)
        {
            TouchingOn(this, e);
        }
    }
    /// <summary>
    /// The ForceStopTouching method will stop the Interact Touch from touching an Interactable Object even if the Interact Touch is physically touching the Interactable Object.
    /// </summary>
    public virtual void ForceStopTouching()
    {
        
        if (touchedObject != null)
        {
            StopTouching(touchedObject);
        }
    }
    protected virtual void StopTouching(GameObject untouched)
    {
        Debug.Log("停止接触");
        OnControllerStartUntouchInteractableObject(SetControllerInteractEvent(untouched));
        if (IsObjectInteractable(untouched))
        {
            InteractableObject untouchedObjectScript = (untouched != null ? untouched.GetComponent<InteractableObject>() : null);
            if (untouchedObjectScript != null)
            {
                untouchedObjectScript.StopTouching(this);
            }
        }

        OnControllerUntouchInteractableObject(SetControllerInteractEvent(untouched));
        CleanupEndTouch();
    }
    protected virtual void CleanupEndTouch()
    {
        touchedObject = null;
        touchedObjectActiveColliders.Clear();
        touchedObjectColliders.Clear();
    }
    public virtual void OnControllerStartUntouchInteractableObject(GrabEventArgs e)
    {
        if (TouchingStartEnd != null)
        {
            TouchingStartEnd(this, e);
        }
    }
    public virtual void OnControllerUntouchInteractableObject(GrabEventArgs e)
    {
        if (TouchingEnd != null)
        {
            TouchingEnd(this, e);
        }
    }
    protected virtual void StoreTouchedObjectColliders(Collider collider)
    {
        touchedObjectColliders.Clear();
        touchedObjectActiveColliders.Clear();
        Collider[] touchedObjectChildColliders = touchedObject.GetComponentsInChildren<Collider>();
        for (int i = 0; i < touchedObjectChildColliders.Length; i++)
        {
            AddListValue(touchedObjectColliders, touchedObjectChildColliders[i], true);
        }
        AddListValue(touchedObjectActiveColliders, collider, true);
    }
    public virtual GrabEventArgs SetControllerInteractEvent(GameObject target)
    {
        GrabEventArgs e;
        e.Grabbed = target;
        e.TakeHand = hand_id;
        return e;
    }
    public virtual void OnControllerStartTouchInteractableObject(GrabEventArgs e)
    {
        if (TouchingStart != null)
        {
            TouchingStart(this, e);
        }
    }

    /// <summary>
    /// The IsObjectInteractable method is used to check if a given GameObject is a valid Interactable Object.
    /// </summary>
    /// <param name="obj">The GameObject to check to see if it's a valid Interactable Object.</param>
    /// <returns>Returns `true` if the given GameObjectis a valid Interactable Object.</returns>
    public virtual bool IsObjectInteractable(GameObject obj)
    {
        if (obj != null)
        {
            InteractableObject io = obj.GetComponentInParent<InteractableObject>();
            if (io != null)
            {
                if (io.disableWhenIdle && !io.enabled)
                {
                    return true;
                }
                return io.enabled;
            }
        }
        return false;
    }


    public virtual GameObject GetTouchedObject()
    {
        return touchedObject;
    }
    public virtual GameObject TriggerStart(Collider collider)
    {
        //是否允许自由掉落

        //end
        return GetColliderInteractableObject(collider);
    }
    /// <summary>
    /// 获取包含可抓取对象的collider
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    public virtual GameObject GetColliderInteractableObject(Collider collider)
    {
        InteractableObject checkIO = collider.GetComponentInParent<InteractableObject>();
        return (checkIO != null ? checkIO.gameObject : null);
    }
    public virtual void AddActiveCollider(Collider collider)
    {
        if(touchedObject!=null && touchedObjectColliders.Contains(collider))
        {
            AddListValue(touchedObjectActiveColliders, collider, true);
        }
    }
    /// <summary>
    /// The AddListValue method adds the given value to the given list. If `preventDuplicates` is `true` then the given value will only be added if it doesn't already exist in the given list.
    /// </summary>
    /// <typeparam name="TValue">The datatype for the list value.</typeparam>
    /// <param name="list">The list to retrieve the value from.</param>
    /// <param name="value">The value to attempt to add to the list.</param>
    /// <param name="preventDuplicates">If this is `false` then the value provided will always be appended to the list. If this is `true` the value provided will only be added to the list if it doesn't already exist.</param>
    /// <returns>Returns `true` if the given value was successfully added to the list. Returns `false` if the given value already existed in the list and `preventDuplicates` is `true`.</returns>
    public static bool AddListValue<TValue>(List<TValue> list, TValue value, bool preventDuplicates = false)
    {
        if (list != null && (!preventDuplicates || !list.Contains(value)))
        {
            list.Add(value);
            return true;
        }
        return false;
    }

    public bool triggerWasColliding = false;
    public virtual void FixedUpdate()
    {
        if (!triggerIsColliding && !triggerWasColliding)
        {
            CheckStopTouching();
        }
        triggerWasColliding = triggerIsColliding;
        triggerIsColliding = false;

        //if (touchedObject == null)
        //{
        //    Debug.Log("是" + hid.ToString() + "未接触物体");
        //}
        //else
        //{
        //    Debug.Log("是" + hid.ToString() + "接触的物体.");
        //}

        //if (GetGrabKey(hid))
        //{
        //    Debug.Log("手柄： " + hid.ToString() + "触发抓取物体按键.");
        //}
        //if (GetGrabKeyUp(hid))
        //{
        //    Debug.Log("手柄： " + hid.ToString() + "触发释放物体按键.");
        //}
    }
    public virtual void CheckStopTouching()
    {
        if (touchedObject != null)
        {
            InteractableObject touchedObjectScript = touchedObject.GetComponent<InteractableObject>();

            //If it's being grabbed by the current touching object then it hasn't stopped being touched.
            if (touchedObjectScript != null && touchedObjectScript.GetGrabbingObject() != gameObject)
            {
                StopTouching(touchedObject);
            }
        }
    }
    public virtual void LateUpdate()
    {
        if (touchedObjectActiveColliders.Count == 0)
        {
            CheckStopTouching();
        }
    }

    #region 抓取部分
    /// <summary>
    /// 强制释放被抓取物体
    /// </summary>
    /// <param name="applyGrabbingObjectVelocity"></param>
    public virtual void ForceRelease(bool applyGrabbingObjectVelocity = false)
    {
        InitUngrabbedObject(applyGrabbingObjectVelocity);
    }
    protected GameObject grabbedObject = null;
    protected bool influencingGrabbedObject = false;
    protected int grabEnabledState = 0;
    protected virtual void InitUngrabbedObject(bool applyGrabbingObjectVelocity)
    {
        if (grabbedObject != null)
        {
            OnControllerStartUngrabInteractableObject(SetControllerInteractEvent(grabbedObject));
            InteractableObject grabbedObjectScript = grabbedObject.GetComponent<InteractableObject>();
            if (grabbedObjectScript != null)
            {
                if (!influencingGrabbedObject)
                {
                    grabbedObjectScript.grabAttachMechanicScript.StopGrab(applyGrabbingObjectVelocity);
                }
                grabbedObjectScript.Ungrabbed(this);
                //切换控制器是否可视
                //ToggleControllerVisibility(true);

                OnControllerUngrabInteractableObject(SetControllerInteractEvent(grabbedObject));
            }
        }

        CheckInfluencingObjectOnRelease();

        grabEnabledState = 0;
        grabbedObject = null;
    }

    /// <summary>
    /// Emitted when a grab of a valid object is started.
    /// </summary>
    public event GrabEventHandler ControllerStartGrabInteractableObject;
    /// <summary>
    /// Emitted when a valid object is grabbed.
    /// </summary>
    public event GrabEventHandler ControllerGrabInteractableObject;
    /// <summary>
    /// Emitted when a ungrab of a valid object is started.
    /// </summary>
    public event GrabEventHandler ControllerStartUngrabInteractableObject;
    /// <summary>
    /// Emitted when a valid object is released from being grabbed.
    /// </summary>
    public event GrabEventHandler ControllerUngrabInteractableObject;
    public virtual void OnControllerStartUngrabInteractableObject(GrabEventArgs e)
    {
        if (ControllerStartUngrabInteractableObject != null)
        {
            ControllerStartUngrabInteractableObject(this, e);
        }
    }
    public virtual void OnControllerUngrabInteractableObject(GrabEventArgs e)
    {
        if (ControllerUngrabInteractableObject != null)
        {
            ControllerUngrabInteractableObject(this, e);
        }
    }
    protected virtual void CheckInfluencingObjectOnRelease()
    {
        if (!influencingGrabbedObject)
        {
            ForceStopTouching();
            //ToggleControllerVisibility(true);
        }
        influencingGrabbedObject = false;
    }
    public virtual void OnControllerStartGrabInteractableObject(GrabEventArgs e)
    {
        if (ControllerStartGrabInteractableObject != null)
        {
            ControllerStartGrabInteractableObject(this, e);
        }
    }

    public virtual void OnControllerGrabInteractableObject(GrabEventArgs e)
    {
        if (ControllerGrabInteractableObject != null)
        {
            ControllerGrabInteractableObject(this, e);
        }
    }
    #endregion
    #endregion
    /// <summary>
    /// 这个变量是为了标定BaseGrab
    /// </summary>
    [Tooltip("这个变量是为了标定BaseGrabAttach中的释放速度的倍率")]
    public float throwMultiplier = 1f;
    public Vector3 GetControllerVelocity()
    {
        //Debug.Log("测量速度的obj: " + controllerAttachPoint.gameObject.name);
        VelocityEstimator vs = GetComponent<VelocityEstimator>();
        return vs.GetVelocityEstimate(); 
    }
    public Vector3 GetControllerAngularVelocity()
    {
        VelocityEstimator vs = GetComponent<VelocityEstimator>();
        return vs.GetAngularVelocityEstimate();
    }


    #region 插入选项
    protected virtual void OnEnable()
    {
        //RegrabUndroppableObject();
        ManageGrabListener(true);
        // ManageInteractTouchListener(true);
        //if (controllerEvents != null)
        //{
        //    controllerEvents.ControllerIndexChanged += DoControllerModelUpdate;
        //    controllerEvents.ControllerModelAvailable += DoControllerModelUpdate;
        //}
        //SetControllerAttachPoint();
    }
    [Tooltip("The Controller Events to listen for the events on. If the script is being applied onto a controller then this parameter can be left blank as it will be auto populated by the controller the script is on at runtime.")]
    public Pvr_ControllerEvents controllerEvents;
    protected Rigidbody originalControllerAttachPoint;
    protected Pvr_ControllerEvents.ButtonAlias subscribedGrabButton = Pvr_ControllerEvents.ButtonAlias.Undefined;
    [Tooltip("The button used to grab/release a touched Interactable Object.")]
    public Pvr_ControllerEvents.ButtonAlias grabButton = Pvr_ControllerEvents.ButtonAlias.Grip;
    /// <summary>
    /// 控制抓取的监听器,state为true则为添加事件,否则为撤销事件
    /// </summary>
    /// <param name="state"></param>
    protected virtual void ManageGrabListener(bool state)
    {
        //注册前先撤销一下
        if(controllerEvents!=null && subscribedGrabButton != Pvr_ControllerEvents.ButtonAlias.Undefined && (!state || grabButton != subscribedGrabButton))
        {
            Debug.Log("注销抓取事件");
            controllerEvents.UnsubscribeToButtonAliasEvent(subscribedGrabButton, true,DoGrabObject);
            controllerEvents.UnsubscribeToButtonAliasEvent(subscribedGrabButton, false, DoReleaseObject);
            subscribedGrabButton = Pvr_ControllerEvents.ButtonAlias.Undefined;
        }
        //设置抓取事件,第三个参数的意义为控制当之前注册过了,就不需要再重新注册了
        if(controllerEvents!=null && state && grabButton!=Pvr_ControllerEvents.ButtonAlias.Undefined && grabButton != subscribedGrabButton)
        {
            Debug.Log("注册抓取事件");
            //设置当grabButton按起时触发抓取物体的事件
            controllerEvents.SubscribeToButtonAliasEvent(grabButton, true, DoGrabObject);
            //设置当grabButton松开时触发释放物体的事件
            controllerEvents.SubscribeToButtonAliasEvent(grabButton, false, DoReleaseObject);
            subscribedGrabButton = grabButton;
        }
    }

    protected virtual void Awake()
    {
        originalControllerAttachPoint = controllerAttachPoint;
        controllerEvents = (controllerEvents != null ? controllerEvents : GetComponent<Pvr_ControllerEvents>());

    }
    protected virtual void OnDisable()
    {
        ForceRelease();
        ManageGrabListener(false);
    }
    protected virtual void Update()
    {
        ManageGrabListener(true);
        //检查控制器的连接位置
        CheckControllerAttachPointSet();
        //CreateNonTouchingRigidbody();
        CheckPrecognitionGrab();
    }
    protected virtual void CheckControllerAttachPointSet()
    {
        if (controllerAttachPoint == null)
        {
            SetControllerAttachPoint();
        }
    }
    /// <summary>
    /// 返回控制器的rigidbody
    /// </summary>
    protected virtual void SetControllerAttachPoint()
    {
        //If no attach point has been specified then just use the tip of the controller
        controllerAttachPoint = GetComponent<Rigidbody>();
    }
    #endregion

    #region 抓取预测
    protected float grabPrecognitionTimer = 0f;
    [Tooltip("An amount of time between when the grab button is pressed to when the controller is touching an Interactable Object to grab it.")]
    public float grabPrecognition = 0f;
    protected virtual void CheckPrecognitionGrab()
    {
        if (grabPrecognitionTimer >= Time.time)
        {
            if (GetGrabbableObject() != null)
            {
                AttemptGrabObject();
                if (GetGrabbedObject() != null)
                {
                    grabPrecognitionTimer = 0f;
                }
            }
        }
    }
    /// <summary>
    /// 获取已经抓取到的物体
    /// </summary>
    /// <returns></returns>
    public virtual GameObject GetGrabbedObject()
    {
        return grabbedObject;
    }
    protected virtual void AttemptGrabObject()
    {
        GameObject objectToGrab = GetGrabbableObject();
        if (objectToGrab != null)
        {
            PerformGrabAttempt(objectToGrab);
        }
        else
        {
            grabPrecognitionTimer = Time.time + grabPrecognition;
        }
    }
    protected virtual void IncrementGrabState()
    {
        if (!IsObjectHoldOnGrab(GetTouchedObject()))
        {
            grabEnabledState++;
        }
    }
    protected virtual bool IsObjectHoldOnGrab(GameObject obj)
    {
        if (obj != null)
        {
            InteractableObject objScript = obj.GetComponent<InteractableObject>();
            return (objScript != null && objScript.holdButtonToGrab);
        }
        return false;
    }
    /// <summary>
    /// 尝试抓取
    /// </summary>
    /// <param name="objectToGrab"></param>
    protected virtual void PerformGrabAttempt(GameObject objectToGrab)
    {
        IncrementGrabState();
        IsValidGrabAttempt(objectToGrab);
        undroppableGrabbedObject = GetUndroppableObject();
    }
    protected GameObject undroppableGrabbedObject;
    /// <summary>
    /// 获取不允许丢弃的物体
    /// </summary>
    /// <returns></returns>
    protected virtual GameObject GetUndroppableObject()
    {
        if (grabbedObject != null)
        {
            InteractableObject grabbedObjectScript = grabbedObject.GetComponent<InteractableObject>();
            return (grabbedObjectScript != null && !grabbedObjectScript.IsDroppable() ? grabbedObject : null);
        }
        return null;
    }

    /// <summary>
    /// 是否不允许抓取
    /// </summary>
    /// <param name="objectToGrab"></param>
    /// <returns></returns>
    protected virtual bool IsValidGrabAttempt(GameObject objectToGrab)
    {
        bool initialGrabAttempt = false;
        InteractableObject objectToGrabScript = (objectToGrab != null ? objectToGrab.GetComponent<InteractableObject>() : null);
        if (grabbedObject == null && IsObjectGrabbable(GetTouchedObject()) && ScriptValidGrab(objectToGrabScript))
        {
            ControllerMsg.GrabbingHandObject = this.gameObject;
            ControllerMsg.GrabbingHandId = hand_id;
            //抓取成功,开始初始化
            InitGrabbedObject();
            //DebugText.text += "触发开始抓取";
            if (!influencingGrabbedObject)
            {
                initialGrabAttempt = objectToGrabScript.grabAttachMechanicScript.StartGrab(gameObject, grabbedObject, controllerAttachPoint);
            }
        }
        return initialGrabAttempt;
    }
    /// <summary>
    /// 是否允许抓取
    /// </summary>
    /// <param name="objectToGrabScript"></param>
    /// <returns></returns>
    protected virtual bool ScriptValidGrab(InteractableObject objectToGrabScript)
    {
        return (objectToGrabScript != null && objectToGrabScript.grabAttachMechanicScript != null && objectToGrabScript.grabAttachMechanicScript.ValidGrab(controllerAttachPoint));
    }
    /// <summary>
    /// 开始抓取
    /// </summary>
    protected virtual void InitGrabbedObject()
    {
        grabbedObject = GetTouchedObject();
        if (grabbedObject != null)
        {
            OnControllerStartGrabInteractableObject(SetControllerInteractEvent(grabbedObject));
            InteractableObject grabbedObjectScript = grabbedObject.GetComponent<InteractableObject>();
            ChooseGrabSequence(grabbedObjectScript);
            //控制控制器是否可见
            //ToggleControllerVisibility(false);
            OnControllerGrabInteractableObject(SetControllerInteractEvent(grabbedObject));
        }
    }
    /// <summary>
    /// 选择抓取方式
    /// </summary>
    /// <param name="grabbedObjectScript"></param>
    protected virtual void ChooseGrabSequence(InteractableObject grabbedObjectScript)
    {
        if (!grabbedObjectScript.IsGrabbed() || grabbedObjectScript.IsSwappable())
        {
            InitPrimaryGrab(grabbedObjectScript);
        }
        else
        {
            InitSecondaryGrab(grabbedObjectScript);
        }
    }
    /// <summary>
    /// 初始化主控制器
    /// </summary>
    /// <param name="currentGrabbedObject"></param>
    protected virtual void InitPrimaryGrab(InteractableObject currentGrabbedObject)
    {
        if (!currentGrabbedObject.IsValidInteractableController())
        {
            grabbedObject = null;
            if (currentGrabbedObject.IsGrabbed(gameObject))
            {
                ForceStopTouching();
            }
            return;
        }

        influencingGrabbedObject = false;
        currentGrabbedObject.SaveCurrentState();
        currentGrabbedObject.Grabbed(this);
        currentGrabbedObject.ZeroVelocity();
        currentGrabbedObject.isKinematic = false;
    }
    /// <summary>
    /// 初始化副控制器
    /// </summary>
    /// <param name="currentGrabbedObject"></param>
    protected virtual void InitSecondaryGrab(InteractableObject currentGrabbedObject)
    {
        influencingGrabbedObject = true;
        currentGrabbedObject.Grabbed(this);
    }
    /// <summary>
    /// 是否允许抓取
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    protected virtual bool IsObjectGrabbable(GameObject obj)
    {
        InteractableObject objScript = obj.GetComponent<InteractableObject>();
        return (IsObjectInteractable(obj) && objScript != null && (objScript.isGrabbable || objScript.PerformSecondaryAction()));
    }

    /// <summary>
    /// 获取可能抓取的物体
    /// </summary>
    /// <returns></returns>
    protected virtual GameObject GetGrabbableObject()
    {
        GameObject obj = GetTouchedObject();
        if (obj != null && IsObjectInteractable(obj))
        {
            return obj;
        }
        return grabbedObject;
    }
    #endregion

    #region grab事件
    /// <summary>
    /// Emitted when the grab button is pressed.
    /// </summary>
    public event ControllerInteractionEventHandler GrabButtonPressed;
    /// <summary>
    /// Emitted when the grab button is released.
    /// </summary>
    public event ControllerInteractionEventHandler GrabButtonReleased;
    protected bool grabPressed;
    public virtual void OnGrabButtonPressed(ControllerInteractionEventArgs e)
    {
        if (GrabButtonPressed != null)
        {
            GrabButtonPressed(this, e);
        }
    }
    public virtual void OnGrabButtonReleased(ControllerInteractionEventArgs e)
    {
        if (GrabButtonReleased != null)
        {
            GrabButtonReleased(this, e);
        }
    }
    protected virtual void DoGrabObject(object sender, ControllerInteractionEventArgs e)
    {
        if (eventManager.AllowGrab)
        {
            eventManager.AllowGrab = false;
            //Debug.Log("手柄触发开始抓取");
            OnGrabButtonPressed(controllerEvents.SetControllerEvent(ref grabPressed, true));
            AttemptGrabObject();
        }
    }
    protected virtual void DoReleaseObject(object sender, ControllerInteractionEventArgs e)
    {
        eventManager.AllowGrab = true;
        AttemptReleaseObject();
        OnGrabButtonReleased(controllerEvents.SetControllerEvent(ref grabPressed, false));
    }
    protected virtual void AttemptReleaseObject()
    {
        //Debug.Log("触发开始释放0");
        string res = "";
        //if (CanRelease()) res += "可以释放 ";
        //if (IsObjectHoldOnGrab(grabbedObject)) res += "正在抓取 ";
        //if (grabEnabledState >= 2) res += "???";
        //Debug.Log(res);
        if (CanRelease() && (IsObjectHoldOnGrab(grabbedObject) || grabEnabledState >= 2))
        {
            //DebugText.text += "触发开始释放";
            InitUngrabbedObject(true);
        }
    }
    protected virtual bool CanRelease()
    {
        if (grabbedObject != null)
        {
            InteractableObject objectToGrabScript = grabbedObject.GetComponent<InteractableObject>();
            return (objectToGrabScript != null && objectToGrabScript.IsDroppable());
        }
        return false;
    }
    #endregion
}
