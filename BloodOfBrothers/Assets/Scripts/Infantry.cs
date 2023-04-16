using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Infantry : MonoBehaviour
{
    public float damage = 10;
    public float elivationModifier = 0.1f;
    public float distanceModifier = 0.1f;
    public float flankModifier = 0.1f;
    public GameObject target;
    public UnitStats target_stats;
    public List<GameObject> range = new List<GameObject>();
    public Vector3 destination;
    

    // Start is called before the first frame update
    void Start()
    {
        destination = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            Vector3 lookPos = target.transform.position - transform.position;
            lookPos.y = 0;
            Quaternion r = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, r, Time.deltaTime * 2);
            target_stats.health -= CalculateDamage();
        }
        else SetTarget();
    }
    
    private float CalculateDamage()
    {
        Vector3 dist = target.transform.position - transform.position;
        float modY = Mathf.Max(0, dist.y * elivationModifier);
        float modDist = Mathf.Max(1, dist.magnitude*distanceModifier);
        dist.y = 0;
        Quaternion r = Quaternion.LookRotation(dist);
        float flank = Quaternion.Angle(r, transform.rotation);
        float modFlank = Mathf.Pow(flank, 2)*flankModifier;
        return (damage + (modY))/(modDist*modFlank);
    }
    
    private void AddTarget(GameObject g) { 
        range.Add(g);
    }
    private void RemoveTarget(GameObject g)
    {
        if (target == g)
        {
            range.Remove(g);
            SetTarget();   
        }
        else range.Remove(g);
    }
    private void SetTarget()
    {
        if (range.Count > 0)
        {
            try
            {
                target = range[Random.Range(0, range.Count)];
                target_stats = target.GetComponent<UnitStats>();
            }
            catch
            {
                target = null;
                target_stats = null;
            }
            
        }
        else
        {
            target = null;
            target_stats = null;
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        try
        {
            UnitStats stats = GetComponent<UnitStats>();
            UnitStats stats2 = obj.GetComponent<UnitStats>();
            if (stats.team != stats2.team)
            {
                AddTarget(obj);
            }
        }
        catch { }


    }
    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;
        RemoveTarget(obj);
    }
}
