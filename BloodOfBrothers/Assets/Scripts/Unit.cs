using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public float health = 100;
    public int team = 0;
    public float damage = 10;

    public float elivationModifier = 0.1f; //Added to Damage
    public float distanceModifier = 0.1f; // Subtracted from Damage
    public float flankModifier = 0.1f; //Added to Damage, based on angle of attack
    
    public float lerpConstant = 0.02f;
    public float rotationSpeed = 4;
    public float movementSpeed = 1;
    public float maxMovementSpeed = 10;

    public GameObject target;
    public Unit target_stats;
    public List<GameObject> range = new List<GameObject>();
    public Vector3 destination;
    public float maxSlope = 5;
    public ActionManager actionManager;


    public bool debugDamage = false;
    public bool debugPreventDeath = false;
    // Start is called before the first frame update
    void Start()
    {
        destination = transform.position;
        actionManager = GameObject.Find("SceneManager").GetComponent<ActionManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!actionManager.paused) {
            destination = new Vector3(destination.x, transform.position.y, destination.z);
            Vector3 newPos = Vector3.Lerp(transform.position, destination, movementSpeed*lerpConstant * Time.deltaTime);
            Vector3 dir = newPos - transform.position;
            transform.position = newPos;
            Ray floor = new Ray(newPos, Vector3.down);
            Debug.DrawRay(newPos, Vector3.down);
            Ray collision = new Ray(transform.position, dir.normalized);
            Debug.DrawRay(newPos, dir.normalized);
            RaycastHit hit;
            bool a = Physics.Raycast(collision, 2);
            bool b = Physics.Raycast(floor, out hit, 100);
        
            if (!a && b)
            {
                transform.position += (hit.point.y - transform.position.y) * Vector3.up;
                Debug.Log(hit.point);
            }
            if (health <= 0 && !debugPreventDeath)
            {
                Destroy(this.gameObject);
            }
            if (target != null)
            {
                Vector3 lookPos = target.transform.position - transform.position;
                lookPos.y = 0;
                Quaternion r = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, r, Time.deltaTime * rotationSpeed);
            
                target_stats.health -= CalculateDamage();
            }
            else UpdateTarget();
        }
    }
    
    private float CalculateDamage()
    {
        Vector3 dist = target.transform.position - transform.position;
        float modY = Mathf.Max(0, dist.y * elivationModifier);
        float modDist = Mathf.Max(1, Mathf.Pow(dist.magnitude, 2)* distanceModifier);
        dist.y = 0;
        //flank = angle between target.forward and this.forward
        float flank = 180-Mathf.Abs(Vector3.SignedAngle(transform.forward, target.transform.forward, Vector3.up));
        float modFlank = flank*flankModifier/10;
        float mod = (modFlank + modY);
        float dmg = (damage * mod) / modDist;
        if (debugDamage)
        {
            Debug.Log("Mod:" + mod + " Dist:" + modDist + " Damage:" + dmg);
        }
        return dmg;
    }
    
    private void AddTarget(GameObject g) { 
        range.Add(g);
    }
    private void RemoveTarget(GameObject g)
    {
        if (target == g)
        {
            range.Remove(g);
            UpdateTarget();   
        }
        else range.Remove(g);
    }
    private void UpdateTarget()
    {
        if (range.Count > 0)
        {
            try
            {
                SetTarget(range[Random.Range(0, range.Count)]);
            }
            catch
            {
                SetTarget();
            }
        }
        else
        {
            SetTarget();
        }
        
    }
    public void SetTarget()
    {
        target = null;
        target = null;
    }
    public void SetTarget(Unit unit)
    {
        target_stats = unit;
        target = unit.gameObject;
    }
    public void SetTarget(GameObject unit)
    {
        target_stats = unit.GetComponent<Unit>();
        target = unit;
    }
    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        try
        {
            Unit stats = obj.GetComponent<Unit>();
            if (this.team != stats.team)
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
