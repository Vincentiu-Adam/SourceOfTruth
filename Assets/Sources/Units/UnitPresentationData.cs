using UnityEngine;

public class UnitPresentationData
{
    public Animator Animator;
    public GameObject GameObject;

    public UnitPresentationData(GameObject unitObject)
    {
        GameObject = unitObject;

        Animator = unitObject.GetComponent<Animator>();
    }
}
