using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var gameObject = other.gameObject;
        ActionController actionController;
        if (gameObject.TryGetComponent(out actionController))
        {
            actionController.TakeDamage();
        }
    }
}
