using System.Collections;

using UnityEngine;

public class UnitPresentationData
{
    public Animator Animator;
    public GameObject GameObject;
    public GameObject VFX;

    private ParticleSystem[] m_VFXParticleSystems;

    public UnitPresentationData(GameObject unitObject, GameObject unitVFX)
    {
        GameObject = unitObject;
        VFX = unitVFX;

        Animator = unitObject.GetComponent<Animator>();

        m_VFXParticleSystems = unitVFX.GetComponentsInChildren<ParticleSystem>(true);
    }

    public IEnumerator GetEnumerator()
    {
        return m_VFXParticleSystems.GetEnumerator();
    }
}
