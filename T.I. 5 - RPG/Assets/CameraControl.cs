using System;
using Unity.Mathematics;
using UnityEngine;

public class CameraControl : MonoBehaviour //Modifica a posição da camera para fazer com que os elementos 3D caibam na visão corretamente
{
    //public float middleKey;
    public double ResolutionConstant = 0.086, testvar = 0.0935;
    float baselocalPosition;
    //public List<Vector3> PositionByResolution; // pelo editor da unity coloque nessa lista a posição que a camera deve assumir de acordo com a lista de resoluções presentes em QuitApp.cs
    void Awake()
    {
        //CalculateResolution();
        
        CalculateRes(/*middleKey*/);
    }
    /*void CalculateResolution()
    {
        transform.localPosition = PositionByResolution[QuitApp.Instance.AspectIndex];
    }*/
    void CalculateRes(/*float pointX*/)//seta a camera para que seu centro se encontre X teclas para a direita ou esquerda
    {
        baselocalPosition = -15 - Math.Clamp(((float)AspectRatioConstant(ResolutionConstant)*((Camera.main.aspect)-1.777978f)),0, math.INFINITY);
        transform.localPosition = new Vector3(/*pointX*0.05f*/transform.localPosition.x, transform.localPosition.y, baselocalPosition/* + (float)AspectRatioConstant(ZoomConstant)*-MathF.Abs(pointX)*/);
    }
    void OnValidate()
    {
        CalculateRes(/*middleKey*/);
    }

    double AspectRatioConstant(double cons)
    {
        //double constant = 0.0914285714285714;
        return cons/Camera.main.aspect;
    }
}
