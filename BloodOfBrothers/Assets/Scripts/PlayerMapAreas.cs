using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMapAreas : MonoBehaviour
{
    public enum State
    {
        Neutreal,
        CapturedAlly,
        CapturedEnemy
    }
    public int control = 0;
    private List<MapAreaCollider> mapAreaColliderList;
    private State state;
    private float progress;
    // Start is called before the first frame update
    private void Awake()
    {
        mapAreaColliderList = new List<MapAreaCollider>();

        foreach(Transform child in transform)
        {
            MapAreaCollider mapAreaCollider = child.GetComponent<MapAreaCollider>();
            if(mapAreaCollider != null)
            {
                mapAreaColliderList.Add(mapAreaCollider);
            }
        }
        state = State.Neutreal;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case State.Neutreal:
                int playerCountInsideMapArea = 0;
                int enemyCountInsideMapArea = 0;
                
                foreach (MapAreaCollider mapAreaCollider in mapAreaColliderList)
                {
                    playerCountInsideMapArea += mapAreaCollider.GetPlayerMapAreasList().Count;
                    enemyCountInsideMapArea += mapAreaCollider.GetEnemyMapAreasList().Count;
                }

                float progressSpeed = 1f;
                control = playerCountInsideMapArea - enemyCountInsideMapArea;
                progress += control * progressSpeed * Time.deltaTime;
                if (progress >= 30f)
                {
                    state = State.CapturedAlly;
                }
                if (progress <= 30f)
                {
                    state = State.CapturedEnemy;
                }

                break;
            case State.CapturedAlly:
                break;
            case State.CapturedEnemy:
                break;
        }

    }
}
