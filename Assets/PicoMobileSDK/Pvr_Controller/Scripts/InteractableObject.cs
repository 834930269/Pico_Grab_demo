using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Event Payload
/// </summary>
/// <param name="interactingObject">The GameObject that is initiating the interaction (e.g. a controller).</param>
public struct InteractableObjectEventArgs
{
    public GameObject interactingObject;
}
/// <summary>
/// Event Payload
/// </summary>
/// <param name="sender">this object</param>
/// <param name="e"><see cref="InteractableObjectEventArgs"/></param>
public delegate void InteractableObjectEventHandler(object sender, InteractableObjectEventArgs e);

public class InteractableObject : MonoBehaviour {
    public Text txt;
    [Tooltip("Determines if the Interactable Object can be grabbed.")]
    public bool isGrabbable = false;
    [Tooltip("If this is checked then the grab button on the controller needs to be continually held down to keep grabbing. If this is unchecked the grab button toggles the grab action with one button press to grab and another to release.")]
    public bool holdButtonToGrab = true;
    public BaseGrabAttach grabAttachMechanicScript;
    protected Transform primaryControllerAttachPoint;
    protected Transform secondaryControllerAttachPoint;
    protected Transform previousParent;
    public BaseGrabAction secondaryGrabActionScript;
    protected bool customTrackPoint = false;
    protected Transform trackPoint;
    protected bool forcedDropped;
    protected bool forceDisabled;
    protected Rigidbody interactableRigidbody;
    protected bool previousKinematicState;
    protected bool previousIsGrabbable;
    protected bool startDisabled = false;
    /// <summary>
    /// Emitted when the Interactable Object script is enabled;
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectEnabled;
    /// <summary>
    /// Emitted when the Interactable Object script is disabled;
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectDisabled;
    /// <summary>
    /// Emitted when another interacting object near touches the current Interactable Object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectNearTouched;
    /// <summary>
    /// Emitted when the other interacting object stops near touching the current Interactable Object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectNearUntouched;
    /// <summary>
    /// Emitted when another interacting object touches the current Interactable Object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectTouched;
    /// <summary>
    /// Emitted when the other interacting object stops touching the current Interactable Object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectUntouched;
    /// <summary>
    /// Emitted when another interacting object grabs the current Interactable Object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectGrabbed;
    /// <summary>
    /// Emitted when the other interacting object stops grabbing the current Interactable Object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectUngrabbed;
    [Header("General Settings")]

    [Tooltip("If this is checked then the Interactable Object component will be disabled when the Interactable Object is not being interacted with.")]
    public bool disableWhenIdle = true;
    private void Start()
    {
        
    }
    public virtual void GrabStart()
    {

    }

    public virtual void GrabEnd()
    {

    }
    /// <summary>
    /// The StartTouching method is called automatically when the Interactable Object is touched initially.
    /// </summary>
    /// <param name="currentTouchingObject">The interacting object that is currently touching this Interactable Object.</param>
    public virtual void StartTouching(Grab_Interact currentTouchingObject = null)
    {
        Debug.Log("Wow|Qaq");
        GameObject currentTouchingGameObject = (currentTouchingObject != null ? currentTouchingObject.gameObject : null);
        if (currentTouchingGameObject != null)
        {
            IgnoreColliders(currentTouchingGameObject);
            if (touchingObjects.Add(currentTouchingGameObject))
            {
                ToggleEnableState(true);
                OnInteractableObjectTouched(SetInteractableObjectEvent(currentTouchingGameObject));
            }
        }
    }
    public virtual void ToggleEnableState(bool state)
    {
        if (disableWhenIdle)
        {
            enabled = state;
        }
    }
    public bool isKinematic
    {
        get
        {
            if (interactableRigidbody != null)
            {
                return interactableRigidbody.isKinematic;
            }
            return true;
        }
        set
        {
            if (interactableRigidbody != null)
            {
                interactableRigidbody.isKinematic = value;
            }
        }
    }
    /// <summary>
    /// The GetGrabbingObject method is used to return the GameObject that is currently grabbing this Interactable Object.
    /// </summary>
    /// <returns>The GameObject of what is grabbing the current Interactable Object.</returns>
    public virtual GameObject GetGrabbingObject()
    {
        return (IsGrabbed() ? grabbingObjects[0] : null);
    }
    /// <summary>
    /// The StopTouching method is called automatically when the Interactable Object has stopped being touched.
    /// </summary>
    /// <param name="previousTouchingObject">The interacting object that was previously touching this Interactable Object.</param>
    public virtual void StopTouching(Grab_Interact previousTouchingObject = null)
    {
        Debug.Log("88|66");
        GameObject previousTouchingGameObject = (previousTouchingObject != null ? previousTouchingObject.gameObject : null);
        if (previousTouchingGameObject != null && touchingObjects.Remove(previousTouchingGameObject))
        {
            ResetUseState(previousTouchingGameObject);
            OnInteractableObjectUntouched(SetInteractableObjectEvent(previousTouchingGameObject));
        }
    }
    public virtual void OnInteractableObjectUntouched(InteractableObjectEventArgs e)
    {
        if (InteractableObjectUntouched != null)
        {
            InteractableObjectUntouched(this, e);
        }
    }
    public virtual void ResetUseState(GameObject checkObject)
    {
        if (checkObject != null)
        {
            
        }
    }
    /// <summary>
    /// The GetSecondaryGrabbingObject method is used to return the GameObject that is currently being used to influence this Interactable Object whilst it is being grabbed by a secondary influencing.
    /// </summary>
    /// <returns>The GameObject of the secondary influencing object of the current grabbed Interactable Object.</returns>
    public virtual GameObject GetSecondaryGrabbingObject()
    {
        return (grabbingObjects.Count > 1 ? grabbingObjects[1] : null);
    }
    protected List<GameObject> grabbingObjects = new List<GameObject>();
    /// <summary>
    /// The IsGrabbed method is used to determine if the Interactable Object is currently being grabbed.
    /// </summary>
    /// <param name="grabbedBy">An optional GameObject to check if the Interactable Object is grabbed by that specific GameObject. Defaults to `null`</param>
    /// <returns>Returns `true` if the Interactable Object is currently being grabbed.</returns>
    public virtual bool IsGrabbed(GameObject grabbedBy = null)
    {
        if (grabbingObjects.Count > 0 && grabbedBy != null)
        {
            return (grabbingObjects.Contains(grabbedBy));
        }
        return (grabbingObjects.Count > 0);
    }
    /// <summary>
    /// 停用碰撞体delay时间
    /// 为了防止多只手碰撞
    /// </summary>
    /// <param name="delay"></param>
    public virtual void PauseCollisions(float delay)
    {
        if (delay > 0f)
        {
            Rigidbody[] childRigidbodies = GetComponentsInChildren<Rigidbody>();
            for (int i = 0; i < childRigidbodies.Length; i++)
            {
                childRigidbodies[i].detectCollisions = false;
            }
            Invoke("UnpauseCollisions", delay);
        }
    }
    public InteractableObjectEventArgs SetInteractableObjectEvent(GameObject interactingObject)
    {
        InteractableObjectEventArgs e;
        e.interactingObject = interactingObject;
        return e;
    }
    public virtual void OnInteractableObjectTouched(InteractableObjectEventArgs e)
    {
        if (InteractableObjectTouched != null)
        {
            InteractableObjectTouched(this, e);
        }
    }
    [Tooltip("An array of colliders on the GameObject to ignore when being touched.")]
    public Collider[] ignoredColliders;
    protected HashSet<GameObject> currentIgnoredColliders = new HashSet<GameObject>();
    protected HashSet<GameObject> hoveredSnapObjects = new HashSet<GameObject>();
    protected HashSet<GameObject> nearTouchingObjects = new HashSet<GameObject>();
    protected HashSet<GameObject> touchingObjects = new HashSet<GameObject>();
    public virtual void IgnoreColliders(GameObject touchingObject)
    {
        if (ignoredColliders != null && !currentIgnoredColliders.Contains(touchingObject))
        {
            bool objectIgnored = false;
            Collider[] touchingColliders = touchingObject.GetComponentsInChildren<Collider>();
            for (int i = 0; i < ignoredColliders.Length; i++)
            {
                for (int j = 0; j < touchingColliders.Length; j++)
                {
                    Physics.IgnoreCollision(touchingColliders[j], ignoredColliders[i]);
                    objectIgnored = true;
                }
            }

            if (objectIgnored)
            {
                currentIgnoredColliders.Add(touchingObject);
            }
        }
    }

    public virtual void Ungrabbed(Grab_Interact previousGrabbingObject = null)
    {
        GameObject previousGrabbingGameObject = (previousGrabbingObject != null ? previousGrabbingObject.gameObject : null);
        GameObject secondaryGrabbingObject = GetSecondaryGrabbingObject();
        if (secondaryGrabbingObject == null || secondaryGrabbingObject != previousGrabbingGameObject)
        {
            SecondaryControllerUngrab(secondaryGrabbingObject);
            PrimaryControllerUngrab(previousGrabbingGameObject, secondaryGrabbingObject);
        }
        else
        {
            SecondaryControllerUngrab(previousGrabbingGameObject);
        }
        OnInteractableObjectUngrabbed(SetInteractableObjectEvent(previousGrabbingGameObject));
    }
    public virtual void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        if (InteractableObjectUngrabbed != null)
        {
            InteractableObjectUngrabbed(this, e);
        }
    }
    protected virtual void SecondaryControllerUngrab(GameObject previousGrabbingObject)
    {
        if (grabbingObjects.Remove(previousGrabbingObject))
        {
            Destroy(secondaryControllerAttachPoint.gameObject);
            secondaryControllerAttachPoint = null;
            if (secondaryGrabActionScript != null)
            {
                secondaryGrabActionScript.ResetAction();
            }
        }
    }
    protected virtual void ForceReleaseGrab()
    {
        GameObject grabbingObject = GetGrabbingObject();
        if (grabbingObject != null)
        {
            grabbingObject.GetComponentInChildren<Grab_Interact>().ForceRelease();
        }
    }
    protected virtual void SetTrackPoint(GameObject currentGrabbingObject)
    {
        //AddTrackPoint(currentGrabbingObject);
        //primaryControllerAttachPoint = CreateAttachPoint(GetGrabbingObject().name, "Original", trackPoint);

        if (grabAttachMechanicScript != null)
        {
            //grabAttachMechanicScript.SetTrackPoint(trackPoint);
            grabAttachMechanicScript.SetInitialAttachPoint(primaryControllerAttachPoint);
        }
    }
    protected virtual void AddTrackPoint(GameObject currentGrabbingObject)
    {
        Grab_Interact grabScript = currentGrabbingObject.GetComponentInChildren<Grab_Interact>();
        Transform controllerPoint = ((grabScript && grabScript.controllerAttachPoint) ? grabScript.controllerAttachPoint.transform : currentGrabbingObject.transform);
        //grabAttachMechanicScript是抓取的那个controller的script
        if (grabAttachMechanicScript != null)
        {
            trackPoint = grabAttachMechanicScript.CreateTrackPoint(controllerPoint, gameObject, currentGrabbingObject, ref customTrackPoint);
        }
    }
    /// <summary>
    /// 零速
    /// </summary>
    public virtual void ZeroVelocity()
    {
        if (interactableRigidbody != null)
        {
            interactableRigidbody.velocity = Vector3.zero;
            interactableRigidbody.angularVelocity = Vector3.zero;
        }
    }
    ///是否允许丢弃,默认不允许
    /// <returns>Returns `true` if the Interactable Object can currently be dropped, returns `false` if it is not currently possible to drop.</returns>
    public virtual bool IsDroppable()
    {
        return true;
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
    protected virtual void PrimaryControllerGrab(GameObject currentGrabbingObject)
    {
        //if (snappedInSnapDropZone)
        //{
        //    ToggleSnapDropZone(storedSnapDropZone, false);
        //}
        ForceReleaseGrab();
        RemoveTrackPoint();
        AddListValue(grabbingObjects, currentGrabbingObject, true);
        SetTrackPoint(currentGrabbingObject);
        if (!IsSwappable())
        {
            previousIsGrabbable = isGrabbable;
            isGrabbable = false;
        }
    }
    protected virtual void PrimaryControllerUngrab(GameObject previousGrabbingObject, GameObject previousSecondaryGrabbingObject)
    {
        UnpauseCollisions();
        RemoveTrackPoint();
        ResetUseState(previousGrabbingObject);
        grabbingObjects.Clear();
        if (secondaryGrabActionScript != null && previousSecondaryGrabbingObject != null)
        {
            secondaryGrabActionScript.OnDropAction();
            previousSecondaryGrabbingObject.GetComponentInChildren<Grab_Interact>().ForceRelease();
        }
        LoadPreviousState();
    }
    /// <summary>
    /// 未完善,不清楚Attach是什么意思
    /// </summary>
    /// <param name="currentGrabbingObject"></param>
    protected virtual void SecondaryControllerGrab(GameObject currentGrabbingObject)
    {
        if (AddListValue(grabbingObjects, currentGrabbingObject, true))
        {
            //secondaryControllerAttachPoint = CreateAttachPoint(currentGrabbingObject.name, "Secondary", currentGrabbingObject.transform);

            if (secondaryGrabActionScript != null)
            {
                secondaryGrabActionScript.Initialise(this, GetGrabbingObject().GetComponentInChildren<Grab_Interact>(), GetSecondaryGrabbingObject().GetComponentInChildren<Grab_Interact>(), primaryControllerAttachPoint, secondaryControllerAttachPoint);
            }
        }
    }

    protected virtual void UnpauseCollisions()
    {
        Rigidbody[] childRigidbodies = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < childRigidbodies.Length; i++)
        {
            childRigidbodies[i].detectCollisions = true;
        }
    }
    protected virtual void LoadPreviousState()
    {
        if (gameObject.activeInHierarchy)
        {
            transform.SetParent(previousParent);
            forcedDropped = false;
        }
        if (interactableRigidbody != null)
        {
            interactableRigidbody.isKinematic = previousKinematicState;
        }
        if (!IsSwappable())
        {
            isGrabbable = previousIsGrabbable;
        }
    }
    public virtual bool IsSwappable()
    {
        return (secondaryGrabActionScript != null ? secondaryGrabActionScript.IsSwappable() : false);
    }
    protected virtual void RemoveTrackPoint()
    {
        if (customTrackPoint && trackPoint != null)
        {
            Destroy(trackPoint.gameObject);
        }
        else
        {
            trackPoint = null;
        }

        if (primaryControllerAttachPoint != null)
        {
            Destroy(primaryControllerAttachPoint.gameObject);
        }
    }
    public virtual void ForceStopSecondaryGrabInteraction()
    {
        GameObject grabbingObject = GetSecondaryGrabbingObject();
        if (grabbingObject != null)
        {
            grabbingObject.GetComponentInChildren<Grab_Interact>().ForceRelease();
        }
    }

    public virtual void SaveCurrentState()
    {
        if (!IsGrabbed())
        {
            previousParent = transform.parent;
            if (!IsSwappable())
            {
                previousIsGrabbable = isGrabbable;
            }

            if (interactableRigidbody != null)
            {
                previousKinematicState = interactableRigidbody.isKinematic;
            }
        }
    }

    protected virtual void Awake()
    {
        interactableRigidbody = GetComponent<Rigidbody>();
        if (interactableRigidbody != null)
        {
            interactableRigidbody.maxAngularVelocity = float.MaxValue;
        }
        if (disableWhenIdle && enabled && IsIdle())
        {
            startDisabled = true;
            enabled = false;
        }
    }
    /// <summary>
    /// determines if this object is currently idle
    /// used to determine whether or not the script
    /// can be disabled for now
    /// </summary>
    /// <returns>whether or not the script is currently idle</returns>
    protected virtual bool IsIdle()
    {
        return !IsNearTouched() && !IsTouched() && !IsGrabbed();
    }

    /// <summary>
    /// The IsTouched method is used to determine if the Interactable Object is currently being touched.
    /// </summary>
    /// <returns>Returns `true` if the Interactable Object is currently being touched.</returns>
    public virtual bool IsTouched()
    {
        return (touchingObjects.Count > 0);
    }
    /// <summary>
    /// The IsNearTouched method is used to determine if the Interactable Object is currently being near touched.
    /// </summary>
    /// <returns>Returns `true` if the Interactable Object is currently being near touched.</returns>
    public virtual bool IsNearTouched()
    {
        return (!IsTouched() && nearTouchingObjects.Count > 0);
    }

    protected virtual void OnEnable()
    {
        forceDisabled = false;
        if (forcedDropped)
        {
            LoadPreviousState();
        }
        forcedDropped = false;
        startDisabled = false;
        OnInteractableObjectEnabled(SetInteractableObjectEvent(null));
    }
    public virtual void OnInteractableObjectEnabled(InteractableObjectEventArgs e)
    {
        if (InteractableObjectEnabled != null)
        {
            InteractableObjectEnabled(this, e);
        }
    }
    /// <summary>
    /// The ForceStopInteracting method forces the Interactable Object to no longer be interacted with and will cause an interacting object to drop the Interactable Object and stop touching it.
    /// </summary>
    public virtual void ForceStopInteracting()
    {
        if (gameObject.activeInHierarchy)
        {
            forceDisabled = false;
            StartCoroutine(ForceStopInteractingAtEndOfFrame());
        }

        if (!gameObject.activeInHierarchy && forceDisabled)
        {
            ForceStopAllInteractions();
            forceDisabled = false;
        }
    }
    protected virtual IEnumerator ForceStopInteractingAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        ForceStopAllInteractions();
    }
    /// <summary>
    /// 强制停止所有的行为
    /// </summary>
    protected virtual void ForceStopAllInteractions()
    {
        if (touchingObjects == null)
        {
            return;
        }

        StopTouchingInteractions();
        StopGrabbingInteractions();
    }
    /// <summary>
    /// 停止触摸物体
    /// </summary>
    protected virtual void StopTouchingInteractions()
    {
        foreach (GameObject touchingObject in new HashSet<GameObject>(touchingObjects))
        {
            if (touchingObject.activeInHierarchy || forceDisabled)
            {
                touchingObject.GetComponentInChildren<Grab_Interact>().ForceStopTouching();
            }
        }
    }
    /// <summary>
    /// 停止抓取物体
    /// </summary>
    protected virtual void StopGrabbingInteractions()
    {
        GameObject grabbingObject = GetGrabbingObject();

        if (grabbingObject != null && (grabbingObject.activeInHierarchy || forceDisabled))
        {
            Grab_Interact grabbingObjectScript = grabbingObject.GetComponentInChildren<Grab_Interact>();
            if (grabbingObjectScript != null && grabbingObjectScript != null)
            {
                grabbingObjectScript.ForceStopTouching();
                grabbingObjectScript.ForceRelease();
                forcedDropped = true;
            }
        }
    }
    protected virtual void OnDisable()
    {
        if (!startDisabled)
        {
            forceDisabled = true;
            ForceStopInteracting();
        }
        OnInteractableObjectDisabled(SetInteractableObjectEvent(null));
    }
    public virtual void OnInteractableObjectDisabled(InteractableObjectEventArgs e)
    {
        if (InteractableObjectDisabled != null)
        {
            InteractableObjectDisabled(this, e);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (trackPoint != null && grabAttachMechanicScript != null)
        {
            grabAttachMechanicScript.ProcessFixedUpdate();
        }

        if (secondaryGrabActionScript != null)
        {
            secondaryGrabActionScript.ProcessFixedUpdate();
        }
    }
    protected virtual void Update()
    {
        //Debug.Log("Update");
        AttemptSetGrabMechanic();
        AttemptSetSecondaryGrabAction();

        if (trackPoint != null && grabAttachMechanicScript != null)
        {
            grabAttachMechanicScript.ProcessUpdate();
        }

        if (secondaryGrabActionScript != null)
        {
            secondaryGrabActionScript.ProcessUpdate();
        }
    }
    protected virtual void AttemptSetSecondaryGrabAction()
    {
        if (isGrabbable && secondaryGrabActionScript == null)
        {
            secondaryGrabActionScript = GetComponent<BaseGrabAction>();
        }
    }
    protected virtual void AttemptSetGrabMechanic()
    {
        if (isGrabbable && grabAttachMechanicScript == null)
        {
            BaseGrabAttach setGrabMechanic = GetComponent<BaseGrabAttach>();
            if (setGrabMechanic == null)
            {
                setGrabMechanic = gameObject.AddComponent<ChildOfControllerGrabAttach>();
            }
            grabAttachMechanicScript = setGrabMechanic;
        }
    }

    protected virtual void LateUpdate()
    {
        if (disableWhenIdle && IsIdle())
        {
            ToggleEnableState(false);
        }
    }


    /// <summary>
    /// 返回是否允许第二个控制器来辅助控制
    /// </summary>
    /// <returns></returns>
    public virtual bool PerformSecondaryAction()
    {
        return (GetGrabbingObject() != null && GetSecondaryGrabbingObject() == null && secondaryGrabActionScript != null ? secondaryGrabActionScript.IsActionable() : false);
    }
    /// <summary>
    /// 是否允许抓取
    /// </summary>
    /// <returns></returns>
    public virtual bool IsValidInteractableController()
    {
        return true;
    }
    /// <summary>
    /// 当物体被抓取时触发
    /// </summary>
    /// <param name="e"></param>
    public virtual void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        if (InteractableObjectGrabbed != null)
        {
            InteractableObjectGrabbed(this, e);
        }
    }
    public virtual void Grabbed(Grab_Interact currentGrabbingObject = null)
    {
        GameObject currentGrabbingGameObject = (currentGrabbingObject != null ? currentGrabbingObject.gameObject : null);
        ToggleEnableState(true);
        if (!IsGrabbed() || IsSwappable())
        {
            PrimaryControllerGrab(currentGrabbingGameObject);
        }
        else
        {
            SecondaryControllerGrab(currentGrabbingGameObject);
        }
        OnInteractableObjectGrabbed(SetInteractableObjectEvent(currentGrabbingGameObject));
    }

}
