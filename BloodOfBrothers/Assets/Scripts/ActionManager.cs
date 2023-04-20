using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]

public class ActionManager : MonoBehaviour
{
    // Collects the current selection   
    private HashSet<GameObject> selection = new HashSet<GameObject>();

    // Stores the current Coroutine controlling he selection process
    private Coroutine selectionRoutine;

    [SerializeField] private Camera _mainCamera;
    

    private void Awake ()
    {
        if(!_mainCamera) _mainCamera = Camera.main;
    }

    // Depending on how exactly you want to start and stop the selection     
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }
        if(Input.GetMouseButtonUp(0))
        {
            EndSelection();
        }
    }
    
    public void StartSelection()
    {
        // if there is already a selection running you don't wanr to start another one
        if(selectionRoutine != null) return;
    
        Debug.Log("Started");
        selectionRoutine = StartCoroutine (SelectionRoutine ());
    }
    
    public void EndSelection()
    {
        if(selectionRoutine == null) return;
    
        StopCoroutine (selectionRoutine);
        Debug.Log("Ended");
        selectionRoutine = null;
    }
    
    private IEnumerator SelectionRoutine()
    {
        // Start with an empty selection
        selection.Clear();
        
        // This is ok in a Coroutine as long as you yield somewhere within it
        while(true)
        {
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
 
            if(Physics.Raycast(ray, out var hit))
            {
                if(!selection.Contains(hit.collider.gameObject)) {
                   selection.Add(hit.collider.gameObject);
                   Debug.Log(selection.Count);
                }
            }
    
            // IMPORTANT: Tells Unity to "pause" here, render this frame and continue from here in the next frame
            yield return null;
        }
    }
}
