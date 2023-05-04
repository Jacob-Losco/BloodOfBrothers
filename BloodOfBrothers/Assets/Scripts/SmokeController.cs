using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeController : MonoBehaviour
{
    ParticleSystem smoke;
    bool despawn = false;
    
    // Start is called before the first frame update
    void Start()
    {
        smoke = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (despawn && smoke.isPlaying)
        {
            Destroy(this.gameObject);
        }
    }
    IEnumerator Despawn()
    {
        yield return new WaitForSeconds(2);
        despawn = true;
    }
}
