using System.Collections.Generic;
using UnityEngine;

public class CutPiece : MonoBehaviour
{
    public List<GameObject> chunks;

    public void AddChunk(GameObject chunk) {
        if (chunks == null) {
            chunks = new List<GameObject>();
        }
        chunks.Add(chunk);
    }
}
