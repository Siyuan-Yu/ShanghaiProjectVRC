
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PointProgressBar : UdonSharpBehaviour
{
    public RectTransform progressBarFill; // 指向填充Image的RectTransform
    public float progress = 0.0f; // 进度，范围0到1
    public int maxPoint;
    public int curPoint;
    public DetectionWallTrigger wallTrigger;
    public PointSystem pointSystem;
    /*public Text pointLineText;
    public Text curPointText;*/ //upgrade to TMP
    private void Start()
    {
        maxPoint = wallTrigger.pointLine;
        
        //pointLineText.text = maxPoint.ToString();
    }

    void Update()
    {
        if (wallTrigger != null && pointSystem != null)
        {
            curPoint = pointSystem.points;
            float progress = curPoint / (float)maxPoint;
            // 假设progress在其他地方更新
            
           // pointLineText.text = progress.ToString();
            // curPointText.text = curPoint.ToString();
            
            progressBarFill.localScale = new Vector3(1, progress, 1);
        }
    }
}
