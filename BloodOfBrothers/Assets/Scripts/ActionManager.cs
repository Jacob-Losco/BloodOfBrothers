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
    public float minHeight = 10;
    public float maxHeight = 200;
    public float ySpeed = 200;
    public float xSpeed = 20;
    public float borderRatio = 0.3f;

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
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && _mainCamera.transform.position.y > minHeight)
        {
            _mainCamera.transform.position +=  Time.deltaTime * ySpeed * Vector3.down;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && _mainCamera.transform.position.y < maxHeight)
        {
            _mainCamera.transform.position += Time.deltaTime * ySpeed * Vector3.up;
        }
        Vector3 mousePos = Input.mousePosition;
        Debug.Log(mousePos);
        float borderY = borderRatio * Screen.height;
        float borderX = borderRatio * Screen.width;
        if (mousePos.y > Screen.height - borderY)
        {
            _mainCamera.transform.position += Time.deltaTime * xSpeed * _mainCamera.transform.up;
        }
        if (mousePos.y < borderY)
        {
            _mainCamera.transform.position -= Time.deltaTime * xSpeed * _mainCamera.transform.up;
        }
        if (mousePos.x > Screen.width - borderX)
        {
            _mainCamera.transform.position += Time.deltaTime * xSpeed * _mainCamera.transform.right;
        }
        if (mousePos.x < borderX)
        {
            _mainCamera.transform.position -= Time.deltaTime * xSpeed * _mainCamera.transform.right;
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
