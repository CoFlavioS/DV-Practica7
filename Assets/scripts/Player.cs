using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float time;
    public bool walking;

    Stack<PathMarker> path;
    FindPathAStar fa;

    public void SetPath(Stack<PathMarker> path)
    {
        this.path = path;
    }

    private void Start()
    {
        fa = GameObject.FindGameObjectWithTag("wall").GetComponent<FindPathAStar>();
        walking = false;
        time = 0.05f;
    }

    void Update()
    {
        if(path != null && !walking)
        {
            walking = true;
            StartCoroutine(CoverPath());
        }
    }

    IEnumerator CoverPath()
    {
        Vector2 ogPos;
        Vector2 dest;
        float var = 0.1f;
        float timeAux = 0;

        while(path.Count > 0)
        {
            dest = path.Pop().location.ToVector();
            ogPos = this.transform.position;

            while (1 > timeAux)
            {
                yield return new WaitForSeconds(time);
                timeAux += var;
                float x = Mathf.Lerp(ogPos.x, dest.x, timeAux);
                float y = Mathf.Lerp(ogPos.y, dest.y, timeAux);
                this.transform.position = new Vector3(x, y, 0);
            }

            timeAux = 0;
        }

        yield return new WaitForSeconds(2.5f);
        path = null;
        fa.ClearOnEnd();
        walking = false;
    }
}
