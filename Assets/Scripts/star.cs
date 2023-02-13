using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class star : MonoBehaviour
{
    public float maxX = 0;
    public float minX = 0;
    public float maxY = 0;
    public float minY = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "character")
        {
            Vector3 newPos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);
            transform.position = newPos;
            Client.instance.updatePosStar(newPos);
        }
    }
}
