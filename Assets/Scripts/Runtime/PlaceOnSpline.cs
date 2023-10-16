using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOnSpline : MonoBehaviour
{

    public Spline _spline;

    [SerializeField,Range(0,100)] private float _distanceBetweenObject = 1;

    [SerializeField] private GameObject _repeteadObject;



    // Start is called before the first frame update
    void Start()
    {
        if (_spline == null)
            return;

        if (_repeteadObject == null)
            return;

        for (float distance = 0; distance < _spline.length(); distance += _distanceBetweenObject)
        {
            Vector3 position = _spline.transform.TransformPoint(_spline.computePointWithLength(distance));
            Orientation orientation = _spline.computeOrientationWithRMFWithLength(distance);

            Quaternion rotation = Quaternion.LookRotation(_spline.transform.TransformDirection(orientation.forward), _spline.transform.TransformDirection(orientation.upward));
            
            Vector3 offsetX = transform.TransformDirection(rotation * Vector3.right * Random.Range(-5f, 5f));
            Vector3 offsetY = transform.TransformDirection(rotation * Vector3.up * Random.Range(1f, 3f));
            GameObject.Instantiate(_repeteadObject, position + offsetX + offsetY, rotation, this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
