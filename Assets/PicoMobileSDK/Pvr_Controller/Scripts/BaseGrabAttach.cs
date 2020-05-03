using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseGrabAttach : MonoBehaviour {
    [Tooltip("选中此选项,则抓取物体时,可以将物体置为isK方式,在丢弃时可以恢复重力")]
    public bool precisionGrab;
    public Text tx;
    public Transform rightSnapHandle;
    public Transform leftSnapHandle;
    public bool throwVelocityWithAttachDistance;

    public float throwMultiplier = 10f;
    [Tooltip("第一次捕获可交互对象时，延迟影响该对象的碰撞的时间量。如果可交互对象在被抓取时可能被卡在另一个游戏对象中，那么这非常有用。")]
    public float onGrabCollisionDelay = 0f;

    protected bool tracked;
    protected bool climbable;
    protected bool kinematic;
    protected GameObject grabbedObject;
    protected Rigidbody grabbedObjectRigidBody;
    protected InteractableObject grabbedObjectScript;
    protected Transform trackPoint;
    protected Transform grabbedSnapHandle;
    protected Transform initialAttachPoint;
    protected Rigidbody controllerAttachPoint;

    /// <summary>
    /// 可追踪
    /// </summary>
    /// <returns>Is true if the mechanic is of type tracked.</returns>
    public virtual bool IsTracked()
    {
        return tracked;
    }

    public virtual bool IsKinematic()
    {
        return kinematic;
    }

    public virtual bool ValidGrab(Rigidbody checkAttachPoint)
    {
        return true;
    }

    public virtual void SetTrackPoint(Transform givenTrackPoint)
    {
        trackPoint = givenTrackPoint;
    }

    public virtual void SetInitialAttachPoint(Transform givenInitialAttachPoint)
    {
        initialAttachPoint = givenInitialAttachPoint;
    }
    /// <summary>
    /// 开始抓取
    /// </summary>
    /// <param name="grabbingObject"></param>
    /// <param name="givenGrabbedObject"></param>
    /// <param name="givenControllerAttachPoint"></param>
    /// <returns></returns>
    public virtual bool StartGrab(GameObject grabbingObject, GameObject givenGrabbedObject, Rigidbody givenControllerAttachPoint)
    {
        Debug.Log("物体触发抓取事件");
        grabbedObject = givenGrabbedObject;
        if (grabbedObject == null)
        {
            return false;
        }

        grabbedObjectScript = grabbedObject.GetComponent<InteractableObject>();
        grabbedObjectRigidBody = grabbedObject.GetComponent<Rigidbody>();
        controllerAttachPoint = givenControllerAttachPoint;
        grabbedSnapHandle = GetSnapHandle(grabbingObject);

        grabbedObjectScript.PauseCollisions(onGrabCollisionDelay);

        //ControllerMsg.primaryGrabHandObject = this.gameObject;

        return true;
    }
    /// <summary>
    /// 获取抓取手柄是右手还是左手
    /// </summary>
    /// <param name="grabbingObject"></param>
    /// <returns></returns>
    protected virtual Transform GetSnapHandle(GameObject grabbingObject)
    {
        if (rightSnapHandle == null && leftSnapHandle != null)
        {
            rightSnapHandle = leftSnapHandle;
        }

        if (leftSnapHandle == null && rightSnapHandle != null)
        {
            leftSnapHandle = rightSnapHandle;
        }

        //if (VRTK_DeviceFinder.IsControllerRightHand(grabbingObject))
        //{
        //    return rightSnapHandle;
        //}

        //if (VRTK_DeviceFinder.IsControllerLeftHand(grabbingObject))
        //{
        //    return leftSnapHandle;
        //}
        return ControllerMsg.TouchingHandObject.transform;
    }

    /// <summary>
    /// 停止抓取
    /// </summary>
    /// <param name="applyGrabbingObjectVelocity"></param>
    public virtual void StopGrab(bool applyGrabbingObjectVelocity)
    {
        grabbedObject = null;
        grabbedObjectScript = null;
        trackPoint = null;
        grabbedSnapHandle = null;
        initialAttachPoint = null;
        controllerAttachPoint = null;
        ControllerMsg.primaryGrabHandObject = null;
    }
    public virtual Transform CreateTrackPoint(Transform controllerPoint, GameObject currentGrabbedObject, GameObject currentGrabbingObject, ref bool customTrackPoint)
    {
        customTrackPoint = false;
        return controllerPoint;
    }

    /// <summary>
    /// The ProcessUpdate method is run in every Update method on the Interactable Object.
    /// </summary>
    public virtual void ProcessUpdate()
    {
    }

    /// <summary>
    /// The ProcessFixedUpdate method is run in every FixedUpdate method on the Interactable Object.
    /// </summary>
    public virtual void ProcessFixedUpdate()
    {
    }
    public virtual void ResetState()
    {
        Initialise();
    }

    protected virtual void Awake()
    {
        ResetState();
    }
    protected abstract void Initialise();

    protected virtual Rigidbody ReleaseFromController(bool applyGrabbingObjectVelocity)
    {
        return grabbedObjectRigidBody;
    }
    /// <summary>
    /// 强制释放抓取的物体
    /// </summary>
    protected virtual void ForceReleaseGrab()
    {
        if (grabbedObjectScript)
        {
            GameObject grabbingObject = grabbedObjectScript.GetGrabbingObject();
            if (grabbingObject != null)
            {
                Grab_Interact grabbingObjectScript = grabbingObject.GetComponentInChildren<Grab_Interact>();
                if (grabbingObjectScript != null)
                {
                    grabbingObjectScript.ForceRelease();
                }
            }
        }
    }

    protected virtual void ReleaseObject(bool applyGrabbingObjectVelocity)
    {
        Rigidbody releasedObjectRigidBody = ReleaseFromController(applyGrabbingObjectVelocity);
        if (releasedObjectRigidBody != null && applyGrabbingObjectVelocity)
        {
            ThrowReleasedObject(releasedObjectRigidBody);
        }
    }

    protected virtual void ThrowReleasedObject(Rigidbody objectRigidbody)
    {
        if (grabbedObjectScript != null)
        {
            //Grab_Interact grabbingObjectScript = ControllerMsg.TouchingHandObject.GetComponentInChildren<Grab_Interact>();
            Grab_Interact grabbingObjectScript = ControllerMsg.GrabbingHandObject.GetComponent<Grab_Interact>();
            if (grabbingObjectScript != null)
            {
                //这里暂时用抓取后的controller
                //Transform origin = ControllerMsg.primaryGrabHandObject.transform;
                //获取其速度
                //grabbingObjectScript.transform.localScale = new Vector3(0.02f,0.02f,0.02f);
                Vector3 velocity = grabbingObjectScript.GetControllerVelocity();
                //tx.text += "速度为" + velocity.ToString();
                //Debug.Log("直接调用速度估计: " + grabbingObjectScript.GetComponent<VelocityEstimator>().GetVelocityEstimate());
                //Debug.Log("物体名称: " + grabbingObjectScript.gameObject.name);
                Vector3 angularVelocity = grabbingObjectScript.GetControllerAngularVelocity();
                float grabbingObjectThrowMultiplier = grabbingObjectScript.throwMultiplier;
                //暴力实现速度限制
                if (velocity.x > 3) velocity.x = 3;
                if (velocity.x < -3) velocity.x = 3;
                if (velocity.y > 3) velocity.y = 3;
                if (velocity.y < -3) velocity.y = 3;
                if (velocity.z > 3) velocity.z = 3;
                if (velocity.z < -3) velocity.z = 3;
                //if(origin != null)
                //{
                //    objectRigidbody.velocity = origin.TransformVector(velocity) * (grabbingObjectThrowMultiplier * throwMultiplier);
                //    objectRigidbody.angularVelocity = origin.TransformDirection(angularVelocity);
                //}else
                {//直接用计算出的速度附加
                    objectRigidbody.velocity = velocity * (grabbingObjectThrowMultiplier * throwMultiplier);
                    objectRigidbody.angularVelocity = angularVelocity;
                }
                //if (throwVelocityWithAttachDistance)
                //{
                //    Collider rigidbodyCollider = objectRigidbody.GetComponentInChildren<Collider>();
                //    if (rigidbodyCollider != null)
                //    {
                //        Vector3 collisionCenter = rigidbodyCollider.bounds.center;
                //        objectRigidbody.velocity = objectRigidbody.GetPointVelocity(collisionCenter + (collisionCenter - transform.position));
                //    }
                //    else
                //    {
                //        objectRigidbody.velocity = objectRigidbody.GetPointVelocity(objectRigidbody.position + (objectRigidbody.position - transform.position));
                //    }
                //}
            }
        }
    }
}
