using UnityEngine;

using System.Collections;

using Cinemachine;
using Cinemachine.Assets;
using Cinemachine.Blending;

public class CustomBlendRoutine : MonoBehaviour 
{
    [SerializeField]
    private CinemachineVirtualCamera m_CameraFrom = null;
    [SerializeField]
    private CinemachineVirtualCamera m_CameraTo = null;
    [SerializeField]
    private float m_BlendTime = 2f;

    private class CustomBlendProvider : IVirtualCameraBlendProvider
    {
        public readonly CinemachineVirtualCamera FromCam;
        public readonly CinemachineVirtualCamera ToCam;

        public float FromCamWeight;
        public float ToCamWeight;

        public CustomBlendProvider(CinemachineVirtualCamera from, CinemachineVirtualCamera to)
        {
            FromCam = from;
            ToCam = to;
            FromCamWeight = 1f;
            ToCamWeight = 0f;

            IsComplete = false;
        }

        public void Advance(float byDT) 
        { 
            //Driven externally, do nothing here
        }

        public AnimationCurve BlendCurve
        {
            //Driven externally, do nothing here
            get { return null; }
        }

        public LensSettings Lens { get { return new LensSettings(Cinemachine.CinemachineCoreAccess.CoreInstance.ActiveCamera); } }

        public Quaternion BlendOrientation { get { return Quaternion.Slerp(FromCam.CameraOrientation, ToCam.CameraOrientation, FromToBlendWeight); } }
        public Vector3 BlendPosition { get { return Vector3.Lerp(FromCam.CameraPosition, ToCam.CameraPosition, FromToBlendWeight); } }

        public float BlendPeriod
        {
            get { return -1; }
        }

        public float FromToBlendWeight
        {
            get { return ToCamWeight / (FromCamWeight + ToCamWeight); }
        }

        public bool IsComplete
        {
            get;
            set;
        }
    }

    private CustomBlendProvider mActiveBlend = null;
    private ICinemachineCore mCoreAssociatedWith = null;
    private float mBlendStart = 0f;

    private void OnEnable()
    {
        UnityEngine.Assertions.Assert.IsNotNull(m_CameraFrom);
        m_CameraFrom.AutoAddToPriorityStack = false;
        m_CameraFrom.gameObject.SetActive(true);

        UnityEngine.Assertions.Assert.IsNotNull(m_CameraTo);
        m_CameraTo.AutoAddToPriorityStack = false;
        m_CameraTo.gameObject.SetActive(true);

        mCoreAssociatedWith = CinemachineCoreAccess.CoreInstance;
        mCoreAssociatedWith.AddVirtualCameraWithAutoBlend(m_CameraFrom);
    }


    private void OnDisable()
    {
        mCoreAssociatedWith.RemoveVirtualCamerNoBlend(m_CameraFrom);
        mCoreAssociatedWith.RemoveVirtualCamerNoBlend(m_CameraTo);
        mCoreAssociatedWith = null;
    }

    private void Update()
    {
        if (mActiveBlend != null)
        {
            float blendProgress = (Time.time - mBlendStart) /  m_BlendTime;
            mActiveBlend.FromCamWeight = 1f - blendProgress;
            mActiveBlend.ToCamWeight = blendProgress;

            //Marking the blend as complete will remove it from the CM runtime
            if (mActiveBlend.FromToBlendWeight >= 1f)
            {
                FinalizeBlend();
            }
        }
    }

    private void OnGUI()
    {
        GUI.Window(45257, new Rect(300f, 0f, 100f, 100f), DrawWindow, "Custom Blend Controller");
    }

    private void DrawWindow(int index)
    {
        if ((mActiveBlend != null) && GUILayout.Button("Cancel Blend"))
        {
            FinalizeBlend();
        }
        else if ((mActiveBlend == null) && GUILayout.Button("Start Blend"))
        {
            mBlendStart = Time.time;
            mActiveBlend = new CustomBlendProvider(m_CameraFrom, m_CameraTo);
            mCoreAssociatedWith.AddVirtualCameraWithExplicitBlend(m_CameraTo, mActiveBlend);

        }
    }

    private void FinalizeBlend()
    {
        mCoreAssociatedWith.RemoveVirtualCamerNoBlend(m_CameraFrom);

        CinemachineVirtualCamera temp = m_CameraTo;
        m_CameraTo = m_CameraFrom;
        m_CameraFrom = temp;
        mActiveBlend.IsComplete = true;
        mActiveBlend = null;
    }
}
