using UnityEngine;
using System.Collections;

public class MouseOrbitImproved : MonoBehaviour
{
    public Transform enviroment;

    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float sideMoveSpeed = 1.0f;
    public float zoomSpeed = 10.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float xMinLimit = -20f;
    public float xMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    private Rigidbody rigidbody;

    float x = 0.0f;
    float y = 0.0f;
    float z = 0.0f;

    float horizontalMove = 0.0f;

    private bool isFlipped = false;
    private bool isFlipping = false;

    public Vector3 angles;

    // Use this for initialization
    void Start()
    {
        angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        rigidbody = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (rigidbody != null)
        {
            rigidbody.freezeRotation = true;
        }
    }

    void LateUpdate()
    {
        angles = transform.rotation.eulerAngles;

        distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, distanceMin, distanceMax);

        Quaternion rotation = Quaternion.identity;

        if ((y % 270 < -90.0f || y % 270 > 90.0f))
        {
            if (z != 180.0f) { z = Mathf.Lerp(z, 180.0f, 0.5f); if (z >= 177.0f) { z = 180.0f; } isFlipping = true; }
            else {
                isFlipping = false;
                if (!isFlipped && !Input.GetMouseButton(1)) {
                    isFlipped = true;
                    sideMoveSpeed = -sideMoveSpeed;
                    ySpeed = -ySpeed;
                    //Debug.Log("Changed directions, isFlipped is now " + isFlipped + ", side speed is " + sideMoveSpeed + ", ySpeed is " + ySpeed); 
                }
            }
        }
        else
        {
            if (z != 0.0f) { z = Mathf.Lerp(z, 0.0f, 0.5f); if (z <= 3.0f) { z = 0.0f; } isFlipping = true; }
            else
            {
                isFlipping = false;
                if (isFlipped && !Input.GetMouseButton(1)) { isFlipped = false;
                    sideMoveSpeed = -sideMoveSpeed;
                    ySpeed = -ySpeed;
                    //Debug.Log("Changed directions, isFlipped is now " + isFlipped + ", side speed is " + sideMoveSpeed + ", ySpeed is " + ySpeed); 
                }
            }
        }

        if (Input.GetMouseButton(1) && target )
        {
            x -= Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;



            //y = ClampAngle(y, yMinLimit, yMaxLimit);
            y = y % 360.0f;
            x = ClampAngle(x, xMinLimit, xMaxLimit);

            /*
            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit))
            {
                distance -= hit.distance;
            }
            */            
        }

        if (Input.GetAxis("Horizontal") != 0) horizontalMove = horizontalMove + Input.GetAxis("Horizontal") * sideMoveSpeed;

        rotation = Quaternion.Euler(y, x, z);

        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position + new Vector3(horizontalMove, 0.0f, 0.0f); //Vector3 position = rotation * negDistance + target.position

        transform.rotation = rotation;
        transform.position = position;

        float envRotOffset = Mathf.Abs(y);
        if (envRotOffset < 180.0f) { envRotOffset = envRotOffset.Remap(0, 180.0f, 10.0f, 50.0f); }
        else { envRotOffset = 50.0f - envRotOffset.Remap(180.0f, 360.0f, 0, 50.0f); }

        enviroment.rotation = Quaternion.Euler(y - envRotOffset, 0.0f, 0.0f);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}