using Pvr_UnitySDKAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VelocityEstimator : MonoBehaviour {
    [Tooltip("Begin the sampling routine when the script is enabled.")]
    public bool autoStartSampling = true;
    [Tooltip("The number of frames to average when calculating velocity.")]
    public int velocityAverageFrames = 5;
    [Tooltip("The number of frames to average when calculating angular velocity.")]
    public int angularVelocityAverageFrames = 10;
    public Text txt;
    protected Vector3[] velocitySamples;
    protected Vector3[] angularVelocitySamples;
    protected int currentSampleCount;
    /// <summary>
    /// 这个是记录当前正在运行的异步方法(速度估计)
    /// </summary>
    protected Coroutine calculateSamplesRoutine;
    /// <summary>
    /// 这个方法启用时记录位置与旋转信息
    /// </summary>
    public virtual void StartEstimation()
    {
        EndEstimation();
        calculateSamplesRoutine = StartCoroutine(EstimateVelocity());
    }
    /// <summary>
    /// 停止记录
    /// </summary>
    public virtual void EndEstimation()
    {
        if (calculateSamplesRoutine != null)
        {
            StopCoroutine(calculateSamplesRoutine);
            calculateSamplesRoutine = null;
        }
    }
    int i = 0;
    /// <summary>
    /// 始终估计速度
    /// Tip: Editor面板内想要测试时必须要Game面板和Scene面板同时显示
    /// IEnumerator成立的条件就是要在主面板中看到
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator EstimateVelocity()
    {
        currentSampleCount = 0;

        Vector3 previousPosition = transform.position;
        Quaternion previousRotation = transform.rotation;
        while (true)
        {
            //txt.text += "正在计算..";
            yield return new WaitForEndOfFrame();

            float velocityFactor = 1.0f / Time.deltaTime;

            int v = currentSampleCount % velocitySamples.Length;
            int w = currentSampleCount % angularVelocitySamples.Length;
            currentSampleCount++;

            velocitySamples[v] = velocityFactor * (transform.position - previousPosition);
            Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(previousRotation);

            float theta = 2.0f * Mathf.Acos(Mathf.Clamp(deltaRotation.w, -1.0f, 1.0f));
            if (theta > Mathf.PI)
            {
                theta -= 2.0f * Mathf.PI;
            }

            Vector3 angularVelocity = new Vector3(deltaRotation.x, deltaRotation.y, deltaRotation.z);
            if (angularVelocity.sqrMagnitude > 0.0f)
            {
                angularVelocity = theta * velocityFactor * angularVelocity.normalized;
            }

            angularVelocitySamples[w] = angularVelocity;
            //Debug.Log("position: " + transform.position + " previousPosition: " + previousPosition);
            //txt.text = "position: " + transform.position + " previousPosition: " + previousPosition;
            previousPosition = transform.position;
            previousRotation = transform.rotation;

        }
    }

    public virtual Vector3 GetVelocityEstimate()
    {
        Vector3 velocity = Vector3.zero;
        int velocitySampleCount = Mathf.Min(currentSampleCount, velocitySamples.Length);
        if (velocitySampleCount != 0)
        {
            for (int i = 0; i < velocitySampleCount; i++)
            {
                velocity += velocitySamples[i];
            }
            //Debug.Log("累加计算后velocity的值: " + velocity);
            velocity *= (1.0f / velocitySampleCount);
        }
        //Debug.Log("获取速度估计: " + velocity.ToString()+" velocitySampleCount: "+ velocitySampleCount.ToString());
        return velocity;
    }
    public virtual Vector3 GetAngularVelocityEstimate()
    {
        Vector3 angularVelocity = Vector3.zero;
        int angularVelocitySampleCount = Mathf.Min(currentSampleCount, angularVelocitySamples.Length);
        if (angularVelocitySampleCount != 0)
        {
            for (int i = 0; i < angularVelocitySampleCount; i++)
            {
                angularVelocity += angularVelocitySamples[i];
            }
            angularVelocity *= (1.0f / angularVelocitySampleCount);
        }
        Debug.Log("获取旋转速度估计: " + angularVelocity.ToString());
        return angularVelocity;
    }

    /// <summary>
    /// The GetAccelerationEstimate method returns the current acceleration estimate.
    /// </summary>
    /// <returns>The acceleration estimate vector of the GameObject</returns>
    public virtual Vector3 GetAccelerationEstimate()
    {
        Vector3 average = Vector3.zero;
        for (int i = 2 + currentSampleCount - velocitySamples.Length; i < currentSampleCount; i++)
        {
            if (i < 2)
            {
                continue;
            }

            int first = i - 2;
            int second = i - 1;

            Vector3 v1 = velocitySamples[first % velocitySamples.Length];
            Vector3 v2 = velocitySamples[second % velocitySamples.Length];
            average += v2 - v1;
        }
        average *= (1.0f / Time.deltaTime);
        //Debug.Log("获取AccelerationE速度估计: " + average.ToString());
        return average;
    }


    protected virtual void InitArrays()
    {
        velocitySamples = new Vector3[velocityAverageFrames];
        angularVelocitySamples = new Vector3[angularVelocityAverageFrames];
    }

    float x=0, y=0, z=0;
    bool equal(float a,float b)
    {
        if (Mathf.Abs(a - b)<1e-6)
        {
            return true;
        }
        return false;
    }
    private void Update()
    {
        //Vector3 nv = GetVelocityEstimate();
        //if(equal(x,nv.x) || equal(y, nv.y) || equal(z, nv.z))
        //{
        //    txt.text += "当前速度为: " + x.ToString() + " " + y.ToString() + " " + z.ToString();
        //    x = nv.x;y = nv.y;z = nv.z;
        //}
        //Debug.Log("当前速度为: " + GetVelocityEstimate().ToString());
        //txt.text = "当前速度为: " + GetVelocityEstimate().ToString();
        //if (Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.A))
        //{
        //    txt.text = GetVelocityEstimate().ToString() + "\n" + " ";
        //}
        //Debug.Log("默认组件速度: " + GetComponent<Rigidbody>().velocity);
        //Debug.Log("默认组件角速度: " + GetComponent<Rigidbody>().angularVelocity);
    }
    protected virtual void OnEnable()
    {
        InitArrays();
        if (autoStartSampling)
        {
            Debug.Log("启动速度估计");
            StartEstimation();
        }
    }

    protected virtual void OnDisable()
    {
        EndEstimation();
    }
}
