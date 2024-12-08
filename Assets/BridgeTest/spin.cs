using UnityEngine;

public class spin : MonoBehaviour
{
    public float speed = 50.0f;
    private Vector3 rotation = new Vector3(0.5f, 1, 2);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotation, speed * Time.deltaTime);
    }
}
