using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]

public class ActionManager : MonoBehaviour
{
    public LayerMask terrainLayer;
    public bool paused = false;

    [SerializeField] private Camera _mainCamera;
    // Collects the current selection   
    private HashSet<Unit> selection = new HashSet<Unit>();
    // Stores the current Coroutine controlling the selection process
    private Coroutine selectionRoutine;

    private void Start()
    {
        if(!_mainCamera) _mainCamera = Camera.main;
    }

    // Depending on how exactly you want to start and stop the selection     
    private void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            StartSelection();
        }
        if(Input.GetMouseButtonUp(0)) {
            EndSelection();
        }
        if(Input.GetKeyDown(KeyCode.Space)) {
            paused = !paused;
        }
        if (selection.Count > 0)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log("ExecuteMove");
                executeMove();
            }
            if (Input.GetKey(KeyCode.CapsLock))
            {
                Debug.Log("SwitchAI");
                switchAI();
            }   
        }
    }

    private void switchAI()
    {
        foreach (Unit unit in selection)
        {
            if (unit.mode == Unit.AIType.hold)
            {
                unit.mode = Unit.AIType.engage;
            } else
            {
                unit.mode = Unit.AIType.hold;
            }
        }
    }

    private void executeRotate()
    {
        foreach (Unit unit in selection)
        {
            float rotation = Mathf.Clamp(Input.GetAxis("Mouse ScrollWheel"), -1, 1) * Time.deltaTime * unit.rotationSpeed * 1000;
            unit.transform.Rotate(Vector3.up, rotation);
        }
    }

    //David
    private void executeMove()
    {
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out var hit))
        {
            RaycastHit[] hits = Physics.RaycastAll(ray);
            GameObject obj = hit.collider.gameObject;
            foreach (Unit unit in selection)
            {
                if (obj.tag == "Terrain")
                {
                    //Jacob
                    Vector3 unitDestination = new Vector3(hit.point.x + Random.Range(-obj.transform.localScale.z, obj.transform.localScale.z) *(selection.Count-1), hit.point.y, hit.point.z + Random.Range(-obj.transform.localScale.z, obj.transform.localScale.z) * (selection.Count - 1));
                    unit.destination = unitDestination;
                }

                if (obj.tag == "Enemy Unit")
                {
                    Unit enemy = obj.GetComponentInParent<Unit>();
                    if (enemy != null && !selection.Contains(enemy) && enemy.team != unit.team)
                    {
                        unit.enemy = enemy;
                        unit.target = enemy.gameObject;
                    }
                }
            }
        }
    }

    public void StartSelection()
    {
        // if there is already a selection running you don't wanr to start another one
        if(selectionRoutine != null) return;
    
        selectionRoutine = StartCoroutine (SelectionRoutine ());
    }
    
    public void EndSelection()
    {

        if(selectionRoutine == null) return;
        StopCoroutine (selectionRoutine);
        selectionRoutine = null;
    }
    
    private IEnumerator SelectionRoutine()
    {
        // Start with an empty selection
        selection.Clear();
        
        // This is ok in a Coroutine as long as you yield somewhere within it
        while(true)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 500))
            {
                Debug.DrawRay(ray.origin, ray.direction, Color.red, 2);
                GameObject obj = hit.collider.gameObject;
                if (obj.tag == "Unit")
                {
                    Unit stats = obj.GetComponentInParent<Unit>();
                    if (stats != null && !selection.Contains(stats) && stats.team == 1)
                    {
                        selection.Add(stats);
                        Debug.Log(selection);
                    }
                }
            }

            // IMPORTANT: Tells Unity to "pause" here, render this frame and continue from here in the next frame
            yield return null;
        }
    }
}

