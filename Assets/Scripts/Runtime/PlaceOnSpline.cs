using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlaceOnSpline : MonoBehaviour
{
    public SplineBest _spline;
    private float _distance = 0f;

    public float _step = 0.001f;

    public float speed = 0.1f; //m.s

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_spline != null )
        {
            _distance = _distance + speed * Time.deltaTime;
            if( _distance > _spline.length())
            {
                _distance = 0f;
            }

            transform.position = _spline.transform.TransformPoint( _spline.computePointWithLength(_distance ));
        }
    }
}
