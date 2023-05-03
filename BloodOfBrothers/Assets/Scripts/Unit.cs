using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int team = 0;
    public int unitType = 0; //0 = infantry, 1 = cavalry, 2 = artillery
    public int numUnits = 20;
    public int unitHealth = 1;
    public int unitDamage = 1;
    public float staticMaximumAccuracyRange = 0.75f;
    public float movementModifier = 0.10f; // Subtracted from staticMaximumAccuracyRange when moving
    public float distanceModifier = 0.05f; // Subtracted from staticMaximumAccuracyRange for every 10 units distance from target
    public float flankModifier = 0.2f; // Added to staticMaximumAccuracyRange when hitting a target angled away from you

    public bool inCooldown = false;

    public float lerpConstant = 0.2f;
    public float rotationSpeed = 10;
    public float movementSpeed = 1;
    public float maxMovementSpeed = 10;

    public GameObject target;
    public Unit target_stats;
    public List<GameObject> range = new List<GameObject>();
    public Vector3 destination;
    public float maxSlope = 5;
    public ActionManager actionManager;


    public List<GameObject> leftFlank = new List<GameObject>();
    public List<GameObject> rightFlank = new List<GameObject>();

    public ParticleSystem gunSmoke;

    public bool debugDamage = false;
    public bool debugPreventDeath = false;

    // Start is called before the first frame update
    void Start()
    {
        destination = transform.position;
        actionManager = GameObject.Find("SceneManager").GetComponent<ActionManager>();
        gunSmoke = transform.GetChild(1).GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        //Execute Move
        if (!actionManager.paused)
        {
            destination = new Vector3(destination.x, transform.position.y, destination.z);
            Vector3 newPos = Vector3.Lerp(transform.position, destination, movementSpeed * lerpConstant * Time.deltaTime);
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
            }
            if (numUnits <= 0 && !debugPreventDeath)
            {
                Destroy(this.gameObject);
            }
            if (target != null)
            {
                Vector3 lookPos = target.transform.position - transform.position;
                lookPos.y = 0;
                Quaternion r = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, r, Time.deltaTime * rotationSpeed);

                if (!inCooldown)
                {
                    gunSmoke.Play();
                    inCooldown = true;
                    StartCoroutine(Cooldown());
                    StartCoroutine(target_stats.TakeDamage(CalculateDamage()));
                }
            }
            else UpdateTarget();
        }

    }

    private int CalculateDamage()
    {
        float distance = Mathf.Sqrt(Mathf.Pow(target.transform.position.x - transform.position.x, 2f)
        + Mathf.Pow(target.transform.position.y - transform.position.y, 2f)
        + Mathf.Pow(target.transform.position.z - transform.position.z, 2f)); //calculate straight line distance from target
        float totalDistanceDrop = distanceModifier * (distance / 10); //calculate accuracy deduction from distance

        float totalMovementDrop = transform.position == destination ? 0.0f : movementModifier; //calculate accuracy deducation from moving

        float flank = 180 - Mathf.Abs(Vector3.SignedAngle(transform.forward, target.transform.forward, Vector3.up)); //calculate angle between this unit front and target front
        float totalFlankBuff = flank < 90 ? 0.0f : flankModifier; // calculate accuracy increase from flanking

        float maxAccuracyRange = staticMaximumAccuracyRange + totalFlankBuff - totalMovementDrop - totalDistanceDrop < 0 ? 0.0f : staticMaximumAccuracyRange + totalFlankBuff - totalMovementDrop - totalDistanceDrop;
        maxAccuracyRange = maxAccuracyRange > 100 ? 1.0f : maxAccuracyRange;

        float minAccuracyRange = maxAccuracyRange - 0.25f < 0 ? 0.0f : maxAccuracyRange - 0.25f;
        minAccuracyRange = minAccuracyRange > 100 ? 1.0f : minAccuracyRange;

        int finalDamage = (int)Mathf.Round(Random.Range(numUnits * minAccuracyRange, numUnits * maxAccuracyRange));

        return finalDamage;
    }

    private bool leftExposed()
    {
        return leftFlank.Count == 0;
    }
    private bool rightExposed()
    {
        return rightFlank.Count == 0;
    }

    private void AddTarget(GameObject g)
    {
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

    IEnumerator TakeDamage(int damage)
    {
        yield return new WaitForSeconds(0);

        if (!debugDamage)
        {
            numUnits -= damage / unitHealth;
            for (int i = 0; i < damage; i++)
            {
                Destroy(transform.GetChild(4 + i).gameObject);
            }
        }
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(5f);
        inCooldown = false;
    }
}
