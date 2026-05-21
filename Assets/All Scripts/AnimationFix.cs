using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationFix : MonoBehaviour
{
    // Отключение физич тела при анимации сидения и смерти (для избежания конфликтов)
    public CapsuleCollider capsuleCollider;

    public void OnSitDown()
    {
        capsuleCollider.enabled = false;
    }

    public void OnStandUp()
    {
        capsuleCollider.enabled = true;
    }
    public void OnDead()
    {
        capsuleCollider.enabled = false;
    }
    public void OnRespawn()
    {
        capsuleCollider.enabled = true;
    }
}
