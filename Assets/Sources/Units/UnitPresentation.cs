using UnityEngine;

public class UnitPresentation
{
    public void Attack(UnitPresentationData unit)
    {
        unit.Animator.SetTrigger("Attack");
        unit.Animator.ResetTrigger("Hit");
    }

    public void Hit(UnitPresentationData unit)
    {
        unit.Animator.SetTrigger("Hit");
    }

    public void Death(UnitPresentationData unit)
    {
        unit.Animator.SetTrigger("Death");
    }

    public void Idle(UnitPresentationData unit)
    {
        unit.Animator.SetTrigger("Idle");
    }

    public void PlayVFXOnUnit(UnitPresentationData unit, UnitPresentationData targetUnit)
    {
        unit.VFX.transform.position = targetUnit.GameObject.transform.position;
        foreach (ParticleSystem particleSystem in unit)
        {
            particleSystem.Play();
        }

        unit.VFX.SetActive(true);
    }

    public void ResetVFX(UnitPresentationData unit)
    {
        unit.VFX.transform.localPosition = Vector3.zero;
    }
}
