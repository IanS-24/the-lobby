using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraFollow : MonoBehaviour
{
    bool panning;

    //Follow
    [Header("Follow")]
    [SerializeField] float minX;
    [SerializeField] float maxX;
    [SerializeField] float minY;
    [SerializeField] float maxY;
    [SerializeField] float yStrength;

    //Zoom
    [Header("Zoom")]
    [SerializeField] GameObject point1;
    [SerializeField] GameObject point2;
    [SerializeField] float zoomSpeed;
    float targetSize;

    Transform player;

    void Start()
    {
        player = GameObject.Find("Player").transform;
    }

    void FixedUpdate()
    {
        //ZOOM

        if (!panning)
        {
            if (Mathf.Abs(GetComponent<Camera>().orthographicSize - targetSize) < zoomSpeed)
            {
                GetComponent<Camera>().orthographicSize = targetSize;
            }
            else if (targetSize > GetComponent<Camera>().orthographicSize)
            {
                GetComponent<Camera>().orthographicSize += zoomSpeed;
            }
            else {
                GetComponent<Camera>().orthographicSize -= zoomSpeed;
            }

            GameObject zoomNodes = GameObject.Find("Zoom Nodes");
            if (zoomNodes != null)
            {
                if (zoomNodes.transform.childCount > 0)
                {
                    float minDistance = 9999;
                    foreach (Transform child in zoomNodes.transform)
                    {
                        if (Vector3.Distance(child.position, player.position) + Mathf.Abs(child.position.y-player.position.y) < minDistance /*&& Mathf.Abs(child.position.y - player.position.y) < 10*/)
                        {
                            minDistance = Vector3.Distance(child.position, player.position);
                            point1 = child.gameObject;
                        }
                    }

                    minDistance = 9999;
                    point2 = point1;
                    foreach (Transform child in zoomNodes.transform)
                    {
                        if (Vector3.Distance(child.position, player.position) + Mathf.Abs(child.position.y-player.position.y) < minDistance && IsAngleFar(child.gameObject, point1) /*&& Mathf.Abs(child.position.y - player.position.y) < 10*/)
                        {
                            minDistance = Vector3.Distance(child.position, player.position);
                            point2 = child.gameObject;
                        }
                    }

                    float d1 = Vector3.Distance(point1.transform.position, player.position);
                    float d2 = Vector3.Distance(point2.transform.position, player.position);
                    float scale1 = d1 / (d1 + d2);
                    float scale2 = d2 / (d1 + d2);

                    targetSize = point1.GetComponent<ZoomSize>().size * scale2 + point2.GetComponent<ZoomSize>().size * scale1;
                    transform.GetChild(0).transform.localScale = new Vector3(0.6f * GetComponent<Camera>().orthographicSize, 0.6f * GetComponent<Camera>().orthographicSize, 1);
                }
            }

            //HORIZONTAL
            Vector3 pos = player.position;
            if (pos.x > minX && pos.x < maxX)
            {
                transform.position = new Vector3(GameObject.Find("Player").transform.position.x, transform.position.y, transform.position.z);
            }


            //VERTICAL
            if (player.GetComponent<PlayerMovement>().IsGrounded() || Mathf.Abs(player.position.y - (transform.position.y)) > yStrength || player.GetComponent<PlayerMovement>().climbing)
            {
                if (player.GetComponent<Rigidbody2D>().velocity.y < -10)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + (player.position.y - transform.position.y) / 10, -10);
                }
                else if (player.GetComponent<Rigidbody2D>().velocity.y < 0)
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + (player.position.y - transform.position.y) / 20, -10);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, transform.position.y + (player.position.y - transform.position.y) / 50, -10);
                }
            }

            if (transform.position.y < minY)
            {
                transform.position = new Vector3(transform.position.x, minY, transform.position.z);
            }
            else if (transform.position.y > maxY)
            {
                transform.position = new Vector3(transform.position.x, maxY, transform.position.z);
            }
        }
    }

    public void GoToPlayer()
    {
        Vector3 target = player.position;
        target = new Vector3(target.x, target.y, -10);
        if (target.x < minX)
            target = new Vector3(minX, target.y, target.z);
        else if (target.x > maxX)
            target = new Vector3(maxX, target.y, target.z);
        else if (target.y < minY)
            target = new Vector3(target.x, minY, target.z);
        else if (target.y > maxY)
            target = new Vector3(target.x, maxY, target.z);
        //StartCoroutine(PanToPoint(target, 1, true));
        transform.position = target;
    }

    public IEnumerator PanToPoint(Vector3 point, float speed, /*bool backToPlayer = false,*/ UnityEvent eventToPlay = null)
    {
        panning = true;
        //GameObject.Find("Player").GetComponent<PlayerMovement>().freeze = true;
        for (float i = 0; i < 1.5f; i += 0.01f)
        {
            transform.position = new Vector3(transform.position.x + speed*(point.x - transform.position.x)/50, transform.position.y + speed*(point.y - transform.position.y)/50, transform.position.z);
            yield return new WaitForSeconds(0.01f);
        }
        if (eventToPlay != null)
            eventToPlay.Invoke();
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(() => GameObject.Find("Player").GetComponent<PlayerMovement>().freeze == false);
        panning = false;
        //GameObject.Find("Player").GetComponent<PlayerMovement>().freeze = false;
        GoToPlayer();
    }

    float GetAngle(GameObject p)
    {
        float x = p.transform.position.x - player.position.x;
        float y = p.transform.position.y - player.position.y;
        if (x > 0)
            return Mathf.Atan(y/x);
        else
            return Mathf.Atan(y/x) + Mathf.PI;
    }

    bool IsAngleFar(GameObject p1, GameObject p2)
    {
        
        bool isFar = true;
        if (GetAngle(p1) < 0  &&  GetAngle(p2) > ((3*Mathf.PI/2) + GetAngle(p1)))
            isFar = false;
        else if (GetAngle(p2) < 0  &&  GetAngle(p1) > ((3*Mathf.PI/2) + GetAngle(p1)))
            isFar = false;
        else if (Mathf.Abs(GetAngle(p1) - GetAngle(p2)) < Mathf.PI/2)
            isFar = false;
        return isFar;
    }
}