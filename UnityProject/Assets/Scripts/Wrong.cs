using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wrong : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        Invoke("DelayedFunction", 3f); // todo:3  Call the DelayedFunction() method with a 3-second delay
    }

    private void DelayedFunction()
    {
        this.gameObject.SetActive(false);
    }
}
