using UnityEngine;
using Cinemachine.Utility;
using Cinemachine;

/// <summary>
/// An add-on module for Cinemachine Virtual Camera that adds a final offset to the camera
/// </summary>
[AddComponentMenu("")] // Hide in menu
[ExecuteAlways]
//[HelpURL(Documentation.BaseURL + "api/Cinemachine.CinemachineCameraOffset.html")]
[SaveDuringPlay]
public class CinemachineRecoil : CinemachineExtension
{
    /// <summary>
    /// Offset the camera's rotation by this much (camera space)
    /// </summary>
    [Tooltip("Recoil Amount")]
    public Vector3 m_RecoilAmount = Vector3.zero;

    /// <summary>
    /// When to apply the offset
    /// </summary>
    [Tooltip("When to apply the recoil")]
    public CinemachineCore.Stage m_ApplyAfter = CinemachineCore.Stage.Aim;




    /// <summary>
    /// Applies the specified offset to the camera state
    /// </summary>
    /// <param name="vcam">The virtual camera being processed</param>
    /// <param name="stage">The current pipeline stage</param>
    /// <param name="state">The current virtual camera state</param>
    /// <param name="deltaTime">The current applicable deltaTime</param>
    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == m_ApplyAfter)
        {

            state.OrientationCorrection *= Quaternion.Euler(m_RecoilAmount);
            //if (!preserveAim)
            //    state.ReferenceLookAt += offset;
            //else
            //{
            //    var q = Quaternion.LookRotation(
            //        state.ReferenceLookAt - state.CorrectedPosition, state.ReferenceUp);
            //    q = q.ApplyCameraRotation(-screenOffset, state.ReferenceUp);
            //    state.RawOrientation = q;
            //}
        }
    }
}
