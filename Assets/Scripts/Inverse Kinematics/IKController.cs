using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKCardInteractionController : MonoBehaviour
{
    protected Animator animator;

    public bool ikActive = false; // Activar IK durante la interacción
    public Transform rightHandTarget = null; // Objetivo de la mano derecha
    public Transform leftHandTarget = null; // Objetivo de la mano izquierda

    [Header("Finger Control")]
    public float thumbBendAngle = 7.2f; // Ángulo de flexión del pulgar (sincronizado para ambas manos)
    public float fingersBendAngle = 18.8f; // Ángulo de flexión de los otros dedos (sincronizado para ambas manos)


    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            if (ikActive)
            {
                // === MANO DERECHA ===
                if (rightHandTarget != null)
                {
                    // Mantener la posición y rotación de la mano derecha fija en el objetivo
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);
                }

                // === MANO IZQUIERDA ===
                if (leftHandTarget != null)
                {
                    // Mantener la posición y rotación de la mano izquierda fija en el objetivo
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
                }

                // === CONTROL DE DEDOS ===
                ControlarDedos();
            }
            else
            {
                // === DESACTIVAR IK ===
                ResetIK();
            }
        }
    }

    private void ControlarDedos()
    {
        // === FLEXIÓN DEL PULGAR ===
        FlexThumb(HumanBodyBones.RightThumbProximal, HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbDistal);
        FlexThumb(HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbDistal);

        // === FLEXIÓN DE LOS OTROS DEDOS ===
        FlexFinger(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal);
        FlexFinger(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal);
        FlexFinger(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal);
        FlexFinger(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal);

        FlexFinger(HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal);
        FlexFinger(HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal);
        FlexFinger(HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal);
        FlexFinger(HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal);
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
