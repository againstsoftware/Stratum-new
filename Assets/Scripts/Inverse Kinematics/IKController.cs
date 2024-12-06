using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
public class IKCardInteractionController : MonoBehaviour
{
    private Animator animator;

    public bool ikActive = false; // Activar IK durante la interacción

    [HideInInspector] public Transform RightHandTarget;
    [HideInInspector] public Transform LeftHandTarget;

    [FormerlySerializedAs("rightHandTarget")] [SerializeField]
    private Transform _rightHandTargetBase = null; // Objetivo de la mano derecha

    [FormerlySerializedAs("leftHandTarget")] [SerializeField]
    private Transform _leftHandTargetBase = null; // Objetivo de la mano derecha

    private float _resetTime = .45f;

    [Header("Finger Control")]
    public float thumbBendAngle = 7.2f; // Ángulo de flexión del pulgar (sincronizado para ambas manos)

    public float fingersBendAngle = 18.8f; // Ángulo de flexión de los otros dedos (sincronizado para ambas manos)

    
    [Header("IK Hints")]
    public Transform RightElbowHint;
    public Transform LeftElbowHint;
    

    private bool _leftReset, _rightReset;
    private Action _leftResetCallback, _rightResetCallback;
    private Vector3 _leftResetInitialPos, _rightResetInitialPos;
    private Quaternion _leftResetInitialRot, _rightResetInitialRot;
    private float _leftResetT, _rightResetT;

    private readonly Vector3 _positionOffsetFromCard = new Vector3(-.16f, -.52f, -.13f);
    private readonly Quaternion _rotationOffsetFromCard = Quaternion.Euler(-52f, 0f, 180f);

    private bool _isAssignedTargetLeft;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        RightHandTarget = Instantiate(_rightHandTargetBase, _rightHandTargetBase.position,
            _rightHandTargetBase.rotation,
            _rightHandTargetBase.parent);

        LeftHandTarget = Instantiate(_leftHandTargetBase, _leftHandTargetBase.position, _leftHandTargetBase.rotation,
            _leftHandTargetBase.parent);
    }

    private void Update()
    {
        if (_leftReset)
        {
            LeftHandTarget.SetPositionAndRotation(
                Vector3.Lerp(_leftResetInitialPos, _leftHandTargetBase.position, _leftResetT),
                Quaternion.Lerp(_leftResetInitialRot, _leftHandTargetBase.rotation, _leftResetT)
            );
            _leftResetT += Time.deltaTime / _resetTime;
            if (_leftResetT >= 1f)
            {
                _leftReset = false;
                LeftHandTarget.SetPositionAndRotation(_leftHandTargetBase.position, _leftHandTargetBase.rotation);
                _leftResetCallback?.Invoke();
                _leftResetCallback = null;
            }
        }

        if (_rightReset)
        {
            RightHandTarget.SetPositionAndRotation(
                Vector3.Lerp(_rightResetInitialPos, _rightHandTargetBase.position, _rightResetT),
                Quaternion.Lerp(_rightResetInitialRot, _rightHandTargetBase.rotation, _rightResetT)
            );
            _rightResetT += Time.deltaTime / _resetTime;
            if (_rightResetT >= 1f)
            {
                _rightReset = false;
                RightHandTarget.SetPositionAndRotation(_rightHandTargetBase.position, _rightHandTargetBase.rotation);
                _rightResetCallback?.Invoke();
                _rightResetCallback = null;
            }
        }
    }

    public void AssignTarget(Transform target)
    {
        float leftDistance = Vector3.Distance(target.position, _leftHandTargetBase.position);
        float rightDistance = Vector3.Distance(target.position, _rightHandTargetBase.position);

        if (leftDistance <= rightDistance) AssignLeftTarget(target);
        else AssignRightTarget(target);
    }


    private void AssignLeftTarget(Transform target)
    {
        _isAssignedTargetLeft = true;
        LeftHandTarget.SetParent(target);
        LeftHandTarget.localPosition = _positionOffsetFromCard;
        LeftHandTarget.localRotation = _rotationOffsetFromCard;
    }


    private void AssignRightTarget(Transform target)
    {
        _isAssignedTargetLeft = false;
        RightHandTarget.SetParent(target);
        RightHandTarget.localPosition = _positionOffsetFromCard;
        RightHandTarget.localRotation = _rotationOffsetFromCard;
    }

    public void ResetTarget(Action callback)
    {
        if (_isAssignedTargetLeft) ResetLeftTarget(callback);
        else ResetRightTarget(callback);
    }

    private void ResetLeftTarget(Action callback)
    {
        _leftReset = true;
        _leftResetT = 0f;
        _leftResetInitialPos = LeftHandTarget.position;
        _leftResetInitialRot = LeftHandTarget.rotation;
        _leftResetCallback = callback;
        LeftHandTarget.SetParent(_leftHandTargetBase.parent);
    }

    private void ResetRightTarget(Action callback)
    {
        _rightReset = true;
        _rightResetT = 0f;
        _rightResetInitialPos = RightHandTarget.position;
        _rightResetInitialRot = RightHandTarget.rotation;
        _rightResetCallback = callback;
        RightHandTarget.SetParent(_rightHandTargetBase.parent);
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if (animator is null) return;
        if (!ikActive)
        {
            ResetIK();
            return;
        }

        // === MANO DERECHA ===
        if (RightHandTarget != null)
        {
            // Mantener la posición y rotación de la mano derecha fija en el objetivo
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, RightHandTarget.rotation);
        }

        // === MANO IZQUIERDA ===
        if (LeftHandTarget != null)
        {
            // Mantener la posición y rotación de la mano izquierda fija en el objetivo
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandTarget.rotation);
        }
        
        if (RightElbowHint != null)
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, RightElbowHint.position);
        }

        if (LeftElbowHint != null)
        {
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
            animator.SetIKHintPosition(AvatarIKHint.LeftElbow, LeftElbowHint.position);
        }

        // === CONTROL DE DEDOS ===
        ControlFingers();
    }

    private void ControlFingers()
    {
        // === FLEXIÓN DEL PULGAR ===
        FlexThumb(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate,
            HumanBodyBones.RightThumbDistal);
        FlexThumb(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate,
            HumanBodyBones.LeftThumbDistal);

        // === FLEXIÓN DE LOS OTROS DEDOS ===
        FlexFinger(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate,
            HumanBodyBones.RightIndexDistal);
        FlexFinger(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate,
            HumanBodyBones.RightMiddleDistal);
        FlexFinger(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate,
            HumanBodyBones.RightRingDistal);
        FlexFinger(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate,
            HumanBodyBones.RightLittleDistal);

        FlexFinger(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate,
            HumanBodyBones.LeftIndexDistal);
        FlexFinger(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate,
            HumanBodyBones.LeftMiddleDistal);
        FlexFinger(HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal);
        FlexFinger(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate,
            HumanBodyBones.LeftLittleDistal);
    }

    private void FlexThumb(HumanBodyBones proximal, HumanBodyBones intermediate, HumanBodyBones distal)
    {
        animator.SetBoneLocalRotation(proximal, Quaternion.Euler(thumbBendAngle, 0, 0));
        animator.SetBoneLocalRotation(intermediate, Quaternion.Euler(thumbBendAngle, 0, 0));
        animator.SetBoneLocalRotation(distal, Quaternion.Euler(thumbBendAngle, 0, 0));
    }

    private void FlexFinger(HumanBodyBones proximal, HumanBodyBones intermediate, HumanBodyBones distal)
    {
        animator.SetBoneLocalRotation(proximal, Quaternion.Euler(fingersBendAngle, 0, 0));
        animator.SetBoneLocalRotation(intermediate, Quaternion.Euler(fingersBendAngle, 0, 0));
        animator.SetBoneLocalRotation(distal, Quaternion.Euler(fingersBendAngle, 0, 0));
    }

    private void ResetIK()
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);

        // Restablecer flexión de los dedos
        ResetFingerWeights();
    }

    private void ResetFingerWeights()
    {
        thumbBendAngle = 0.0f;
        fingersBendAngle = 0.0f;
    }
}