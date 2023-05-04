using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFlankManager : MonoBehaviour
{
    public Unit unit;
    public bool right;
    // Start is called before the first frame update
    void Start()
    {
    }
    private List<GameObject> flank()
    {
        if (right)
        {
            return unit.rightFlankDefenders;
        }
        else
        {
            return unit.leftFlankDefenders;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        GameObject obj = other.gameObject;
        if (obj.tag == "Unit")
        {
            Unit flanker = obj.GetComponentInParent<Unit>();
            bool allied = flanker.team == unit.team;

            bool exists = flank().Contains(obj);

            if (allied)
            {
                if (!exists)
                {
                    flank().Add(obj);
                }
            }
            else
            {
                if (flank().Count > 0)
                {
                    flank().RemoveAt(0);
                }
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject obj = other.gameObject;
        bool exists = flank().Contains(obj);
        if (exists)
            flank().Remove(obj);
    }
}
