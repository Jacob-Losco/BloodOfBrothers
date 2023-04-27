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
        if(Input.GetKeyDown("p")) {
            paused = !paused;
        }
        if (selection.Count > 0)
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown("space"))
            {
                executeMove();
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                executeRotate();
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
            GameObject obj = hit.collider.gameObject;
            foreach (Unit unit in selection)
            {
                if (obj.tag == "Terrain")
                {
                    //Jacob
                    Vector3 unitDestination = new Vector3(hit.point.x + Random.Range(-3, 3)*(selection.Count-1), hit.point.y, hit.point.z + Random.Range(-3, 3) * (selection.Count - 1));
                    unit.destination = unitDestination;
                }

                if (obj.tag == "Unit")
                {
                    Unit enemy = obj.GetComponentInParent<Unit>();
                    if (enemy != null && !selection.Contains(enemy) && enemy.team != 1)
                    {
                        unit.target_stats = enemy;
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
            RaycastHit[] hits = Physics.RaycastAll(ray, 200);
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];
                {
                    GameObject obj = hit.collider.gameObject;
                    if (obj.tag == "Unit")
                    {
                        Unit stats = obj.GetComponentInParent<Unit>();
                        if (stats != null && !selection.Contains(stats) && stats.team == 1)
                        {
                                selection.Add(stats);
                        }
                    }
                }
            }
    
            // IMPORTANT: Tells Unity to "pause" here, render this frame and continue from here in the next frame
            yield return null;
        }
    }
}

