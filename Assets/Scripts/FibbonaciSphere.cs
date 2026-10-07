using System.Collections.Generic;
using UnityEngine;

public class FibbonaciSphere : MonoBehaviour
{
    [Range(10, 1000)]
    public int numPoints = 100;

    [Range(1f, 10f)]
    public float radius = 1f;

    private List<Vector3> points = new List<Vector3>();

    private List<(int A, int B, int trianglesCount)> activeEdges
    = new List<(int A, int B, int trianglesCount)>(); //Minindex with Maxindex

    private List<int> TempActiveVertecies = new List<int>();
    private void Start()
    {
        GeneratePoints();
    }

    private void OnValidate()
    {
        GeneratePoints();
    }

    private void GeneratePoints()
    {
        points.Clear();
        activeEdges.Clear();
        TempActiveVertecies.Clear();

        // Złota proporcja
        float goldenRatio = (1f + Mathf.Sqrt(5f)) / 2f;

        // Kąt złoty
        float angleIncrement = Mathf.PI * (3f - Mathf.Sqrt(5f));

        for (int i = 0; i < numPoints; i++)
        {
            float t = (float)i / numPoints;

            float angle1 = Mathf.Acos(1f - 2f * t);
            float angle2 = angleIncrement * i;

            float x = Mathf.Sin(angle1) * Mathf.Cos(angle2);
            float y = Mathf.Sin(angle1) * Mathf.Sin(angle2);
            float z = Mathf.Cos(angle1);

            Vector3 pointOnSphere = new Vector3(x, y, z);

            // Zmiana sfery jednostkowej na sferę o zadanym promieniu
            pointOnSphere *= radius;

            points.Add(pointOnSphere);
            

        }
        GenerateEdges();
    }

    private void OnDrawGizmos()
    {
    foreach (Vector3 point in points)
    {
        int index = points.IndexOf(point);

        //if (index % 3 == 0)
          //  Gizmos.color = Color.blue;
        //else if (index % 3 == 1)
          //  Gizmos.color = Color.yellow;
        //else
          //  Gizmos.color = Color.red;
        if (TempActiveVertecies.Contains(index))
            {
                Gizmos.color = Color.blue;
            }
            else{
        float t = (float)index / (numPoints - 1);
        byte gray = (byte)Mathf.RoundToInt((1.0f - t) * 255);
        Gizmos.color = new Color32(gray, gray, gray, 255);
            }
        Gizmos.DrawSphere(transform.position + point, 0.03f);
    }
    for (int i=0;i<activeEdges.Count;i++){
        int A = activeEdges[i].A;
        int B = activeEdges[i].B;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position+points[A], transform.position + points[B]);
    }
    }

    private void GenerateEdges(){
        
        activeEdges.Add((0, FindFirstEdge(), 0));
        List<(int, int, int)> Activetriangles = new List<(int, int, int)>();
        //while (Activetriangles.Count < 2 * points.Count - 4) // chodzi o to by z jednej pętli stworzyć brzeg
        for (int looper=0;looper<10;looper++){
        
            List<(int A, int B, int trianglesCount)> activeEdgesCOPY = new List<(int A, int B, int trianglesCount)>(activeEdges);
            foreach (var element in activeEdges){
                if (element.trianglesCount == 2) {continue;}
    
            List<(float kat, int idx)> PotencialVertecies = FindPotencialVertecies(element);
            List<(float kat, int idx)> PotencialVerteciesCOPY = new List<(float kat, int idx)>();
            Debug.Log(PotencialVertecies[PotencialVertecies.Count-1].kat);

            // a) sprawdzenie skrajnych przypadków kąta
            for (int i = PotencialVertecies.Count - 1; i >= 0; i--)
            {
                if (PotencialVertecies[i].kat >= 30.0 && PotencialVertecies[i].kat <= 150.0) 
                {
                    PotencialVerteciesCOPY.Add(PotencialVertecies[i]);
                }
            }
            PotencialVertecies.Clear();
            PotencialVertecies = new List<(float kat, int idx)>(PotencialVerteciesCOPY); //nie bierze wskaźnika tylko wartości
            Debug.Log(PotencialVertecies.Count + "SIZZE");
            PotencialVerteciesCOPY.Clear();

            // b - odległości - zostaje 10 najmneijszych
            for (int i = PotencialVertecies.Count - 1; i >= 0; i--)
            {
                int idxA = element.A;
                int idxB = element.B;
                int idxC = PotencialVertecies[i].idx;
                float distanceAC = Vector3.Distance(points[idxA], points[idxC]);
                float distanceBC = Vector3.Distance(points[idxB], points[idxC]);
                PotencialVerteciesCOPY.Add((distanceAC + distanceBC, idxC));
            }
            PotencialVerteciesCOPY.Sort(
                (a, b) => a.kat.CompareTo(b.kat)
            );
            int amount = Mathf.Min(5, PotencialVerteciesCOPY.Count);
            PotencialVertecies.Clear();
            for (int i = 0; i < amount; i++)
            {
                PotencialVertecies.Add(PotencialVerteciesCOPY[i]);
                
            }
            PotencialVerteciesCOPY.Clear();
            Debug.Log(PotencialVertecies.Count + "SIZZE2");

            // c) najmniejszy największy kąt z listy - jeżeli takie same to jeszcze dystans
            for (int i = PotencialVertecies.Count - 1; i >= 0; i--)
            {
                int idxA = element.A;
                int idxB = element.B;
                int idxC = PotencialVertecies[i].idx;
                float minAngle = FindMinAngle(idxA, idxB, idxC);
                PotencialVerteciesCOPY.Add((minAngle, idxC));
            }
            PotencialVerteciesCOPY.Sort(
                (a, b) => a.kat.CompareTo(b.kat)
            );
            PotencialVertecies.Clear();
            float bestAngle = PotencialVerteciesCOPY[^1].kat;
            for (int i = 0; i < PotencialVerteciesCOPY.Count-1; i++)
            {
                if (Mathf.Approximately(PotencialVerteciesCOPY[i].kat, bestAngle))
                {
                    PotencialVertecies.Add(PotencialVerteciesCOPY[i]);
                }
            }
            PotencialVertecies.Add(PotencialVerteciesCOPY[^1]);
            // jeżeli jest więcej niż jeden, rozstrzygnij dystansem
            if (PotencialVertecies.Count > 1)
            {
                PotencialVerteciesCOPY.Clear();

                for (int i = 0; i < PotencialVertecies.Count; i++)
                {
                    int idxA = element.A;
                    int idxB = element.B;
                    int idxC = PotencialVertecies[i].idx;

                    float distanceAC = Vector3.Distance(points[idxA], points[idxC]);
                    float distanceBC = Vector3.Distance(points[idxB], points[idxC]);

                    PotencialVerteciesCOPY.Add(
                        (distanceAC + distanceBC, idxC)
                    );
                }

                PotencialVerteciesCOPY.Sort(
                    (a, b) => a.kat.CompareTo(b.kat)
                );
                
            }
            
            PotencialVertecies.Clear();
            PotencialVertecies.Add(PotencialVerteciesCOPY[0]);
            TempActiveVertecies.Add(PotencialVerteciesCOPY[0].idx);
            Debug.Log(PotencialVerteciesCOPY[0].kat+"HERE");

            int A = element.A;
            int B = element.B;
            int C = PotencialVertecies[0].idx;

            // AC
            int minAC = Mathf.Min(A, C);
            int maxAC = Mathf.Max(A, C);

            
            int edgeIndex = activeEdges.FindIndex(
                e => e.A == minAC && e.B == maxAC
            );

            if (edgeIndex != -1)
            {
                var edge = activeEdgesCOPY[edgeIndex];

                if (edge.trianglesCount < 2)
                {
                    edge.trianglesCount++;
                    activeEdgesCOPY[edgeIndex] = edge;
                }
            }
            else
            {
                activeEdgesCOPY.Add((minAC, maxAC, 1));
            }

            // BC
            int minBC = Mathf.Min(B, C);
            int maxBC = Mathf.Max(B, C);

            edgeIndex = activeEdgesCOPY.FindIndex(
                e => e.A == minBC && e.B == maxBC
            );

            if (edgeIndex != -1)
            {
                var edge = activeEdgesCOPY[edgeIndex];

                if (edge.trianglesCount < 2)
                {
                    edge.trianglesCount++;
                    activeEdgesCOPY[edgeIndex] = edge;
                }
            }
            else
            {
                activeEdgesCOPY.Add((minBC, maxBC, 1));
            }

            
        }
    activeEdges = new List<(int A, int B, int trianglesCount)>(activeEdgesCOPY);
            activeEdgesCOPY.Clear();
    }
}

    private int FindFirstEdge(){
        float minDistance = float.PositiveInfinity;  
        int minIndex = 1;

        for (int i=1;i<numPoints*0.1;i++){
            float newDistance = Vector3.Distance(points[0], points[i]);
            if (newDistance<=minDistance){
                minDistance = newDistance;
                minIndex = i;
            }

        }
        return minIndex;
    }
    // dodaj, że nie może być to a ani b
    private List<(float angle, int index)> FindPotencialVertecies(
    (int A, int B, int count) edge) //nasze (A, B, bool)
    {
        List<(float, int)> PotencialVertecies = new List<(float, int)>();
        for (int i=edge.A; i<numPoints*0.3; i++)
        {
            Vector3 AB = points[edge.B] - points[edge.A];
            Vector3 AC = points[i] - points[edge.A];

            float kat = Vector3.Angle(AB, AC);
            PotencialVertecies.Add((kat, i));

            if (i+1>numPoints-1){break;}
        }
        PotencialVertecies.Sort((a, b) => a.Item1.CompareTo(b.Item1)); //sortowanie względem wartości float
        return PotencialVertecies;
    }

    private float FindMinAngle(int idxA, int idxB, int idxC)
{
    Vector3 AB = points[idxB] - points[idxA];
    Vector3 AC = points[idxC] - points[idxA];

    Vector3 BA = points[idxA] - points[idxB];
    Vector3 BC = points[idxC] - points[idxB];

    Vector3 CA = points[idxA] - points[idxC];
    Vector3 CB = points[idxB] - points[idxC];

    float angleA = Vector3.Angle(AB, AC);
    float angleB = Vector3.Angle(BA, BC);
    float angleC = Vector3.Angle(CA, CB);

    return Mathf.Min(angleA, Mathf.Min(angleB, angleC));
}

    
    }
