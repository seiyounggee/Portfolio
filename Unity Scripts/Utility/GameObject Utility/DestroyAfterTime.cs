using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField] float destroyTime = 3f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(destroyTime);

        Destroy(gameObject);
    }

    public void SetTimer(float time)
    {
        destroyTime = time;
    }
}
