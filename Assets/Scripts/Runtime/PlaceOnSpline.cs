using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceOnSpline : MonoBehaviour
{

    public SplineBest _spline;

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
            Orientation orientation = _spline.computeOrientationWithLenght(distance, Vector3.up);

            Quaternion rotation = Quaternion.LookRotation(_spline.transform.TransformDirection(orientation.forward), _spline.transform.TransformDirection(orientation.upward));
            GameObject.Instantiate(_repeteadObject, position, rotation, this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
