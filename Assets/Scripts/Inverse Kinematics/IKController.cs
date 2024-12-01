using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKCardInteractionController : MonoBehaviour
{
    protected Animator animator;

    public bool ikActive = false; // Activar IK durante la interacción
    public Transform rightHandTarget = null; // Objetivo de la mano derecha
    public Transform leftHandTarget = null; // Objetivo de la mano izquierda
    public Transform rightElbowHint = null; // Referencia para el codo derecho
    public Transform leftElbowHint = null; // Referencia para el codo izquierdo

    [Header("Finger Control")]
    public float thumbWeight = 1.0f; // Peso del pulgar (0-1)
    public float fingersWeight = 1.0f; // Peso del resto de los dedos (0-1)

    [Header("Finger Bend Angles")]
    public float thumbBendAngle = 40.0f; // Ángulo de flexión del pulgar
    public float fingersBendAngle = 90.0f; // Ángulo de flexión de los otros dedos

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
                    // Posición y rotación
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation);

                    // Control del codo derecho
                    if (rightElbowHint != null)
                    {
                        animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 1);
                        animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowHint.position);
                    }
                }

                // === MANO IZQUIERDA ===
                if (leftHandTarget != null)
                {
                    // Posición y rotación
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);

                    // Control del codo izquierdo
                    if (leftElbowHint != null)
                    {
                        animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 1);
                        animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowHint.position);
                    }
                }

                // === CONTROL DE DEDOS ===
                ControlarDedos();
            }
            else
            {
                // === DESACTIVAR IK ===
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);

                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);

                // Restablecer pesos de los dedos
                ResetFingerWeights();
            }
        }
    }

    private void ControlarDedos()
    {
        // Flexión del pulgar derecho
        animator.SetBoneLocalRotation(HumanBodyBones.RightThumbProximal, Quaternion.Euler(thumbBendAngle, 0, 0));
        animator.SetBoneLocalRotation(HumanBodyBones.RightThumbIntermediate, Quaternion.Euler(thumbBendAngle, 0, 0));
        animator.SetBoneLocalRotation(HumanBodyBones.RightThumbDistal, Quaternion.Euler(thumbBendAngle, 0, 0));

        // Flexión de los otros dedos derechos
        FlexFinger(HumanBodyBones.RightIndexProximal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal);
        FlexFinger(HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal);
        FlexFinger(HumanBodyBones.RightRingProximal, HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal);
        FlexFinger(HumanBodyBones.RightLittleProximal, HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal);
    }

    private void FlexFinger(HumanBodyBones proximal, HumanBodyBones intermediate, HumanBodyBones distal)
    {
        animator.SetBoneLocalRotation(proximal, Quaternion.Euler(fingersBendAngle, 0, 0));
        animator.SetBoneLocalRotation(intermediate, Quaternion.Euler(fingersBendAngle, 0, 0));
        animator.SetBoneLocalRotation(distal, Quaternion.Euler(fingersBendAngle, 0, 0));
    }

    private void ResetFingerWeights()
    {
        thumbWeight = 0.0f;
        fingersWeight = 0.0f;
    }
}
