using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LegControl : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        // Aseg�rate de que la animaci�n de las piernas se mantenga en una posici�n fija
        if (animator)
        {
            // Obtener las transformaciones espec�ficas de los huesos de las piernas
            Transform rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg); // O ajusta a RightLeg si es el nombre exacto en Mixamo
            Transform leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg); // O ajusta a LeftLeg si es el nombre exacto en Mixamo

            // Si los huesos existen, fijar las posiciones y rotaciones
            if (rightLeg != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, rightLeg.position);  // Usa la posici�n del hueso de la pierna
                animator.SetIKRotation(AvatarIKGoal.RightFoot, rightLeg.rotation);  // Usa la rotaci�n del hueso de la pierna
            }

            if (leftLeg != null)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftLeg.position);  // Usa la posici�n del hueso de la pierna
                animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftLeg.rotation);  // Usa la rotaci�n del hueso de la pierna
            }
        }
    }
}
