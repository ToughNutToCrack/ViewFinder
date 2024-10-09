using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFrustumLocalSpace : MonoBehaviour
{
    public Camera finder;
    public float xRatio = 16;
    public float yRatio = 9;
    public float customOffset = 0.1f;
    public Transform capturePoint;
    public PlayerController controller;
    float aspectRatio = 1;
    GameObject leftPrimitivePlane, rightPrimitivePlane, topPrimitivePlane, bottomPrimitivePlane, frustumObject;
    MeshFilter leftPrimitivePlaneMF, rightPrimitivePlaneMF, topPrimitivePlaneMF, bottomPrimitivePlaneMF, frustumObjectMF;
    MeshCollider leftPrimitivePlaneMC, rightPrimitivePlaneMC, topPrimitivePlaneMC, bottomPrimitivePlaneMC, frustumObjectMC;
    List<GameObject> leftToCut, rightToCut, topToCut, bottomToCut, objectsInFrustum;
    Vector3 leftUpFrustum, rightUpFrustum, leftDownFrustum, rightDownFrustum, cameraPos;
    Plane leftPlane, rightPlane, topPlane, bottomPlane;
    PolaroidFilm activeFilm;
    Vector3 forwardVector;
    bool isTakingPicture;
    GameObject ending;

    void Start()
    {
        leftPrimitivePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        leftPrimitivePlane.name = "LeftCameraPlane";
        rightPrimitivePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rightPrimitivePlane.name = "RightCameraPlane";
        topPrimitivePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        topPrimitivePlane.name = "TopCameraPlane";
        bottomPrimitivePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        bottomPrimitivePlane.name = "BottomCameraPlane";
        frustumObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        frustumObject.name = "FrustumObject";

        leftPrimitivePlaneMC = leftPrimitivePlane.GetComponent<MeshCollider>();
        leftPrimitivePlaneMC.convex = true;
        leftPrimitivePlaneMC.isTrigger = true;
        leftPrimitivePlaneMC.enabled = false;

        rightPrimitivePlaneMC = rightPrimitivePlane.GetComponent<MeshCollider>();
        rightPrimitivePlaneMC.convex = true;
        rightPrimitivePlaneMC.isTrigger = true;
        rightPrimitivePlaneMC.enabled = false;

        topPrimitivePlaneMC = topPrimitivePlane.GetComponent<MeshCollider>();
        topPrimitivePlaneMC.convex = true;
        topPrimitivePlaneMC.isTrigger = true;
        topPrimitivePlaneMC.enabled = false;

        bottomPrimitivePlaneMC = bottomPrimitivePlane.GetComponent<MeshCollider>();
        bottomPrimitivePlaneMC.convex = true;
        bottomPrimitivePlaneMC.isTrigger = true;
        bottomPrimitivePlaneMC.enabled= false;

        frustumObjectMC = frustumObject.GetComponent<MeshCollider>();
        frustumObjectMC.convex = true;
        frustumObjectMC.isTrigger = true;
        frustumObjectMC.enabled= false;

        leftPrimitivePlaneMF = leftPrimitivePlane.GetComponent<MeshFilter>();
        rightPrimitivePlaneMF = rightPrimitivePlane.GetComponent<MeshFilter>();
        topPrimitivePlaneMF = topPrimitivePlane.GetComponent<MeshFilter>();
        bottomPrimitivePlaneMF = bottomPrimitivePlane.GetComponent<MeshFilter>();
        frustumObjectMF = frustumObject.GetComponent<MeshFilter>();

        leftPrimitivePlane.GetComponent<MeshRenderer>().enabled = false;
        rightPrimitivePlane.GetComponent<MeshRenderer>().enabled = false;
        topPrimitivePlane.GetComponent<MeshRenderer>().enabled = false;
        bottomPrimitivePlane.GetComponent<MeshRenderer>().enabled = false;
        frustumObjectMF.GetComponent<MeshRenderer>().enabled = false;

        var leftChecker = leftPrimitivePlane.AddComponent<CollisionChecker>();
        leftChecker.frustumLocalSpace = this;
        leftChecker.side = 0;

        var rightChecker = rightPrimitivePlane.AddComponent<CollisionChecker>();
        rightChecker.frustumLocalSpace = this;
        rightChecker.side = 1;

        var topChecker = topPrimitivePlane.AddComponent<CollisionChecker>();
        topChecker.frustumLocalSpace = this;
        topChecker.side = 2;

        var bottomChecker = bottomPrimitivePlane.AddComponent<CollisionChecker>();
        bottomChecker.frustumLocalSpace = this;
        bottomChecker.side = 3;

        var frustumChecker = frustumObject.AddComponent<CollisionChecker>();
        frustumChecker.frustumLocalSpace = this;
        frustumChecker.side = 4;
    }

    public void Cut(bool isTakingPic)
    {
        isTakingPicture = isTakingPic;

        controller.ChangePlayerState(false);
        //SETUP PHASE
        aspectRatio = finder.aspect;
        var frustumHeight = 2.0f * finder.farClipPlane * Mathf.Tan(finder.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * aspectRatio;

        leftUpFrustum = new Vector3(-frustumWidth/2, frustumHeight/2, finder.farClipPlane);
        rightUpFrustum = new Vector3(frustumWidth/2, frustumHeight/2, finder.farClipPlane);
        leftDownFrustum = new Vector3(-frustumWidth/2, -frustumHeight/2, finder.farClipPlane);
        rightDownFrustum = new Vector3(frustumWidth/2, -frustumHeight/2, finder.farClipPlane);

        leftUpFrustum = capturePoint.transform.TransformPoint(leftUpFrustum);
        rightUpFrustum = capturePoint.transform.TransformPoint(rightUpFrustum);
        leftDownFrustum = capturePoint.transform.TransformPoint(leftDownFrustum);
        rightDownFrustum = capturePoint.transform.TransformPoint(rightDownFrustum);

        cameraPos = capturePoint.transform.position;
        forwardVector = capturePoint.transform.forward;

        leftPlane = new Plane(cameraPos, leftUpFrustum, leftDownFrustum);
        rightPlane = new Plane(cameraPos, rightDownFrustum, rightUpFrustum);
        topPlane = new Plane(cameraPos, rightUpFrustum, leftUpFrustum);
        bottomPlane = new Plane(cameraPos, leftDownFrustum, rightDownFrustum);

        var leftOffset = leftPlane.normal * customOffset;
        leftPrimitivePlaneMF.mesh = CreateBoxMesh(cameraPos, leftUpFrustum, (leftUpFrustum + leftDownFrustum)/2 ,leftDownFrustum,
        leftDownFrustum + leftOffset, ((leftUpFrustum + leftDownFrustum)/2) + leftOffset, leftUpFrustum + leftOffset, cameraPos + leftOffset);
        leftPrimitivePlaneMC.sharedMesh = leftPrimitivePlaneMF.mesh;

        var rightOffset = rightPlane.normal * customOffset;
        rightPrimitivePlaneMF.mesh = CreateBoxMesh(cameraPos, rightDownFrustum, (rightUpFrustum + rightDownFrustum)/2 , rightUpFrustum,
        rightUpFrustum + rightOffset, ((rightUpFrustum + rightDownFrustum)/2) + rightOffset, rightDownFrustum + rightOffset, cameraPos + rightOffset);
        rightPrimitivePlaneMC.sharedMesh = rightPrimitivePlaneMF.mesh;

        var topOffset = topPlane.normal * customOffset;
        topPrimitivePlaneMF.mesh = CreateBoxMesh(cameraPos, rightUpFrustum, (leftUpFrustum + rightUpFrustum)/2 ,leftUpFrustum,
        leftUpFrustum + topOffset, ((leftUpFrustum + rightUpFrustum)/2) + topOffset, rightUpFrustum + topOffset, cameraPos + topOffset);
        topPrimitivePlaneMC.sharedMesh = topPrimitivePlaneMF.mesh;

        var bottomOffset = bottomPlane.normal * customOffset;
        bottomPrimitivePlaneMF.mesh = CreateBoxMesh(cameraPos, leftDownFrustum, (leftDownFrustum + rightDownFrustum)/2 ,rightDownFrustum,
        rightDownFrustum + bottomOffset, ((leftDownFrustum + rightDownFrustum)/2) + bottomOffset, leftDownFrustum + bottomOffset, cameraPos + bottomOffset);
        bottomPrimitivePlaneMC.sharedMesh = bottomPrimitivePlaneMF.mesh;

        //CUTTING PHASE
        leftToCut = new List<GameObject>();
        rightToCut = new List<GameObject>();
        topToCut = new List<GameObject>();
        bottomToCut = new List<GameObject>();
        objectsInFrustum = new List<GameObject>();
        ending = null;

        leftPrimitivePlaneMC.enabled = true;
        rightPrimitivePlaneMC.enabled = true;
        topPrimitivePlaneMC.enabled = true;
        bottomPrimitivePlaneMC.enabled = true;

        StartCoroutine(TestCut(isTakingPicture));
    }

    IEnumerator TestCut(bool isTakingPicture) {

        /* trick to give time to unity to detect collisions \ 1 frame isn't enough */
        yield return null;
        yield return null;
        yield return null;
        //

        leftPrimitivePlaneMC.enabled = false;
        rightPrimitivePlaneMC.enabled = false;
        topPrimitivePlaneMC.enabled = false;
        bottomPrimitivePlaneMC.enabled = false;

        List<GameObject> allObjects = new List<GameObject>();
        List<GameObject> intactObjects = new List<GameObject>();

        foreach (var obj in leftToCut) {

            if (isTakingPicture)
            {
                var initialName = obj.name;
                obj.name = obj.name + "/cut";
                var original = Instantiate(obj);
                original.transform.position = obj.transform.position;
                original.transform.rotation = obj.transform.rotation;
                original.name = initialName;
                original.SetActive(false);
                intactObjects.Add(original);
            }

            allObjects.Add(obj);

            var cutPiece = obj.GetComponent<CutPiece>();
            if (cutPiece == null)
            {
                cutPiece = obj.AddComponent<CutPiece>();
                cutPiece.AddChunk(obj);
            }

            var newPiece = Cutter.Cut(obj, (leftDownFrustum + leftUpFrustum + cameraPos) / 3, leftPlane.normal);
            cutPiece.AddChunk(newPiece);
            allObjects.Add(newPiece);
        }

        foreach (var obj in rightToCut) {

            if (isTakingPicture)
            {
                var s = obj.name.Split('/');
                if (s.Length == 1)
                {
                    var initialName = obj.name;
                    obj.name = obj.name + "/cut";
                    var original = Instantiate(obj);
                    original.transform.position = obj.transform.position;
                    original.transform.rotation = obj.transform.rotation;
                    original.name = initialName;
                    original.SetActive(false);
                    intactObjects.Add(original);
                }
            }

            if (!allObjects.Contains(obj))
            {
                allObjects.Add(obj);
            }

            var cutPiece = obj.GetComponent<CutPiece>();
            if (cutPiece == null)
            {
                cutPiece = obj.AddComponent<CutPiece>();
                cutPiece.AddChunk(obj);
            }

            int initialCount = cutPiece.chunks.Count;
            for(int i = 0; i < initialCount; i++)
            {
                var newPiece = Cutter.Cut(cutPiece.chunks[i], (rightDownFrustum + rightUpFrustum + cameraPos) / 3, rightPlane.normal);
                cutPiece.AddChunk(newPiece);
                allObjects.Add(newPiece);
            }
        }

        foreach (var obj in topToCut) {


            var s = obj.name.Split('/');
            if (s.Length == 1)
            {
                var initialName = obj.name;
                obj.name = obj.name + "/cut";
                var original = Instantiate(obj);
                original.transform.position = obj.transform.position;
                original.transform.rotation = obj.transform.rotation;
                original.name = initialName;
                original.SetActive(false);
                intactObjects.Add(original);
            }

            if (!allObjects.Contains(obj))
            {
                allObjects.Add(obj);
            }

            var cutPiece = obj.GetComponent<CutPiece>();
            if (cutPiece == null)
            {
                cutPiece = obj.AddComponent<CutPiece>();
                cutPiece.AddChunk(obj);
            }

            int initialCount = cutPiece.chunks.Count;
            for(int i = 0; i < initialCount; i++)
            {
                var newPiece = Cutter.Cut(cutPiece.chunks[i], (leftUpFrustum + rightUpFrustum + cameraPos) / 3, topPlane.normal);
                cutPiece.AddChunk(newPiece);
                allObjects.Add(newPiece);
            }
        }

        foreach (var obj in bottomToCut) {

            var s = obj.name.Split('/');
            if (s.Length == 1)
            {
                var initialName = obj.name;
                obj.name = obj.name + "/cut";
                var original = Instantiate(obj);
                original.transform.position = obj.transform.position;
                original.transform.rotation = obj.transform.rotation;
                original.name = initialName;
                original.SetActive(false);
                intactObjects.Add(original);
            }

            if (!allObjects.Contains(obj))
            {
                allObjects.Add(obj);
            }

            var cutPiece = obj.GetComponent<CutPiece>();
            if (cutPiece == null)
            {
                cutPiece = obj.AddComponent<CutPiece>();
                cutPiece.AddChunk(obj);
            }

            int initialCount = cutPiece.chunks.Count;
            for(int i = 0; i < initialCount; i++)
            {
                var newPiece = Cutter.Cut(cutPiece.chunks[i], (leftDownFrustum + rightDownFrustum + cameraPos) / 3, bottomPlane.normal);
                cutPiece.AddChunk(newPiece);
                allObjects.Add(newPiece);
            }
        }

        //need to add a little margin aiming inside
        frustumObjectMF.mesh = CreateFrustumObject(cameraPos + (forwardVector * -customOffset), rightDownFrustum + (rightPlane.normal * -customOffset), rightUpFrustum + (rightPlane.normal * -customOffset), leftUpFrustum + (leftPlane.normal * -customOffset), leftDownFrustum + (leftPlane.normal * -customOffset));
        frustumObjectMC.sharedMesh = frustumObjectMF.mesh;
        frustumObjectMC.enabled = true;

        /* trick to give time to unity to detect collisions \ 1 frame isn't enough */
        yield return null;
        yield return null;
        yield return null;
        //

        frustumObjectMC.enabled = false;

        if (ending != null)
            objectsInFrustum.Add(ending);

        if (isTakingPicture) {
            activeFilm = new PolaroidFilm(objectsInFrustum, capturePoint);

            foreach(var i in intactObjects)
                i.SetActive(true);

            foreach (var obj in allObjects) {
                if (obj != null)
                    Destroy(obj);
            }
        }
        else {

            foreach(var obj in allObjects)
                Destroy(obj.GetComponent<CutPiece>());

            foreach(var obj in objectsInFrustum)
                Destroy(obj);

            activeFilm.ActivateFilm();
        }

        yield return new WaitForSeconds(0.5f);
        
        controller.ChangePlayerState(true);
    }

    public void AddObjectToCut(GameObject toCut, int side)
    {
        switch (side) {
            case 0:
                if (!leftToCut.Contains(toCut))
                    leftToCut.Add(toCut);
                break;
            case 1:
                if (!rightToCut.Contains(toCut))
                    rightToCut.Add(toCut);
                break;
            case 2:
                if (!topToCut.Contains(toCut))
                    topToCut.Add(toCut);
                break;
            case 3:
                if (!bottomToCut.Contains(toCut))
                    bottomToCut.Add(toCut);
                break;
            case 4:
                if (!objectsInFrustum.Contains(toCut))
                    objectsInFrustum.Add(toCut);
                break;
        }
    }

    public void AddEndingObject(GameObject end) {
        ending = end;
    }

    Mesh CreateBoxMesh(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7, Vector3 v8)
    {
        Vector3[] vertices = new Vector3[] {
            v1,
            v2,
            v3,
            v4,
            v5,
            v6,
            v7,
            v8
        };

        int[] triangles = new int[] {
            0, 1, 2,
            0, 2, 3,

            3, 2, 5,
            3, 5, 3,

            2, 1, 6,
            2, 6, 5,

            7, 4, 5,
            7, 5, 6,

            0, 1, 6,
            0, 6, 7,

            0, 7, 4,
            0, 4, 3
        };

        var mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
        };

        return mesh;

    }

    Mesh CreateFrustumObject(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5)
    {
        Vector3[] vertices = new Vector3[] {
            v1,
            v2,
            v3,
            v4,
            v5
        };

        int[] triangles = new int[] {
            0, 2, 1,

            4, 1, 2,
            4, 2, 3,

            0, 4, 3,

            0, 1, 4,

            0, 3, 2,
        };

        var mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
        };

        return mesh;

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        var aspectRatio = finder.aspect;
        var frustumHeight = 2.0f * finder.farClipPlane * Mathf.Tan(finder.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var frustumWidth = frustumHeight * aspectRatio;

        var leftUpF = new Vector3(-frustumWidth/2, frustumHeight/2, finder.farClipPlane);
        var rightUpF = new Vector3(frustumWidth/2, frustumHeight/2, finder.farClipPlane);
        var leftDownF = new Vector3(-frustumWidth/2, -frustumHeight/2, finder.farClipPlane);
        var rightDownF = new Vector3(frustumWidth/2, -frustumHeight/2, finder.farClipPlane);

        leftUpF = capturePoint.transform.TransformPoint(leftUpF);
        rightUpF = capturePoint.transform.TransformPoint(rightUpF);
        leftDownF = capturePoint.transform.TransformPoint(leftDownF);
        rightDownF = capturePoint.transform.TransformPoint(rightDownF);

        Gizmos.DrawLine(capturePoint.position, rightUpF);
        Gizmos.DrawLine(capturePoint.position, leftUpF);
        Gizmos.DrawLine(capturePoint.position, rightDownF);
        Gizmos.DrawLine(capturePoint.position, leftDownF);

        Gizmos.DrawLine(leftDownF, rightDownF);
        Gizmos.DrawLine(leftUpF, rightUpF);

        Gizmos.DrawLine(leftDownF, leftUpF);
        Gizmos.DrawLine(rightDownF, rightUpF);
    }
}

public class PolaroidFilm {
    List<GameObject> placeHolders;
    public PolaroidFilm(List<GameObject> obj, Transform parentToFollow) {
        placeHolders = new List<GameObject>();
        foreach(var o in obj) {
            var placeholder = GameObject.Instantiate(o);
            placeholder.transform.position = o.transform.position;
            placeholder.transform.rotation = o.transform.rotation;
            placeholder.transform.SetParent(parentToFollow);
            placeholder.SetActive(false);
            placeHolders.Add(placeholder);
        }
    }

    public void ActivateFilm() {
        for (int i = 0; i < placeHolders.Count; i++) {
            placeHolders[i].transform.SetParent(null);
            placeHolders[i].SetActive(true);
        }
    }
}