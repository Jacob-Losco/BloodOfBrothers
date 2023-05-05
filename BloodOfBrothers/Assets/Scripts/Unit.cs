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
    
    public float maxFindRange = 10;
    public float maxAttackRange = 5;
    public float optimalAttackRange = 4; //Determines how close AI will path to attack
    
    public float staticMaximumAccuracyRange = 0.75f;
    public float movementModifier = 0.10f; // Subtracted from staticMaximumAccuracyRange when moving
    public float distanceModifier = 0.05f; // Subtracted from staticMaximumAccuracyRange for every 10 units distance from target
    public float flankModifier = 0.2f; // Added to staticMaximumAccuracyRange when hitting a target angled away from you
    public enum AIType {hold, engage, flank, charge, retreat, seige};
    public AIType mode = AIType.hold;
    
    public bool inCooldown = false;

    public float lerpConstant = 0.2f;
    public float rotationSpeed = 10;
    public float movementSpeed = 1;
    public float maxMovementSpeed = 10;

    public GameObject target;
    public Unit enemy;
    public List<GameObject> targets = new List<GameObject>();
    
    public Vector3 destination;
    public float maxSlope = 5;

    public ActionManager actionManager;
    
    public List<GameObject> leftFlankDefenders = new List<GameObject>();
    public List<GameObject> rightFlankDefenders = new List<GameObject>();

    public GameObject gunSmoke;

    public bool debugDamage = false;
    public bool debugPreventDeath = false;

    public GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        destination = transform.position;
        actionManager = GameObject.Find("SceneManager").GetComponent<ActionManager>();
        StartCoroutine(Cooldown());
    }

    // Update is called once per frame
    void Update()
    {
        //Execute Move
        if (!actionManager.paused)
        {
            destination = new Vector3(destination.x, transform.position.y, destination.z);
            Vector3 newPos = Vector3.MoveTowards(transform.position, destination, movementSpeed * Time.deltaTime);
            Vector3 dir = newPos - transform.position;
            Ray floor = new Ray(newPos, Vector3.down * 3);
            Ray forward = new Ray(transform.position, dir.normalized);
            if (!Physics.Raycast(forward, 1, 2))
            {
                if (Physics.Raycast(floor, out var hit))
                {
                    transform.position = new Vector3(newPos.x, hit.point.y + transform.localScale.y + 1, newPos.z);
                }
                else
                {
                    transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
                }
            }
            else
            {
                destination = transform.position;
            }
            RaycastHit[] hits;
            switch (mode)
            {
                case AIType.hold:
                    //hold position or follow orders to destination
                    break;
                case AIType.engage:
                    if (!isMoving() && target != null)
                    {
                        if (enemy.target == this.gameObject)
                        {
                            destination = (transform.position - target.transform.position).normalized * enemy.optimalAttackRange;

                        }// else (is distracted), flank
                        else if (enemy.rightExposed() || enemy.leftExposed())
                        {
                            mode = AIType.flank;
                        }
                        else
                        {
                            //hold
                        }
                    }
                    break;
                case AIType.flank:
                    if (target == null || enemy.target == this.gameObject)
                    {
                        destination = this.transform.position;
                        mode = AIType.engage;
                    }
                    else
                    {
                        if (enemy.rightExposed())
                        {
                            if (enemy.leftExposed())
                            {
                                //pick random
                                if (Random.Range(0, 2) == 0)
                                {
                                    //right
                                    destination = (target.transform.position - target.transform.right).normalized * optimalAttackRange;

                                }
                                else
                                {
                                    //left
                                    destination = (target.transform.position + target.transform.right).normalized * optimalAttackRange;
                                }
                            }
                        }
                        else if (enemy.leftExposed())
                        {
                            //left
                            destination = target.transform.position + target.transform.right * optimalAttackRange;
                        }
                        else
                        {
                            mode = AIType.engage;
                        }
                    }

                    break;
                case AIType.charge:
                    destination = target.transform.position;
                    break;
                case AIType.retreat:
                    hits = Physics.SphereCastAll(origin: transform.position, radius: maxFindRange, direction: Vector3.up, maxDistance: 0);
                    int count = 0;
                    Vector3 fleaToward = Vector3.zero;
                    foreach (RaycastHit hit in hits)
                    {
                        GameObject gameObject = hit.collider.gameObject;
                        if (gameObject.tag == "Enemy Unit")
                        {
                            fleaToward += (transform.position - target.transform.position);
                            count++;
                        }
                    }
                    destination = transform.position + fleaToward;
                    break;
                case AIType.seige:
                    break;
                default:
                    break;
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

                if (!inCooldown && lookPos.magnitude < maxAttackRange)
                {
                    GameObject smoke = Instantiate(gunSmoke, null);
                    smoke.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    StartCoroutine(Cooldown());
                    StartCoroutine(enemy.TakeDamage(CalculateDamage()));
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
    public bool isMoving()
    {
        return (destination - transform.position).magnitude > 1;
    }
    public bool leftExposed()
    {
        return leftFlankDefenders.Count == 0;
    }
    public bool rightExposed()
    {
        return rightFlankDefenders.Count == 0;
    }

    public void AddTarget(GameObject g)
    {
        targets.Add(g);
    }

    private void RemoveTarget(GameObject g)
    {
        if (target == g)
        {
            targets.Remove(g);
            UpdateTarget();
        }
        else targets.Remove(g);
    }

    private void UpdateTarget()
    {
        if (targets.Count > 0)
        {
            try
            {
                SetTarget(targets[Random.Range(0, targets.Count)]);
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
        enemy = unit;
        target = unit.gameObject;
    }

    public void SetTarget(GameObject unit)
    {
        enemy = unit.GetComponent<Unit>();
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
        inCooldown = true;
        yield return new WaitForSeconds(5f);
        inCooldown = false;
    }
}
