
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
}
