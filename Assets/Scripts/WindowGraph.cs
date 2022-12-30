using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using PupilLabs.Demos;

public class WindowGraph : MonoBehaviour
{
    [SerializeField]

    //sprite used to draw line chart
    private Sprite dotSprite;

    //path to the .txt file where data is stored
    string path = "Assets/Data/data" + DateTime.Now.ToString().Replace("/", "-").Replace(" ", "_").Replace(":", "-") + ".txt";

    //pupil subscriber supplying data to ploter
    public PupilDataDemo pupilSubscriber;

    //toggle buttons
    private Toggle thetaLStatus;
    private Toggle phiLStatus;
    private Toggle pupilLStatus;

    private Toggle thetaRStatus;
    private Toggle phiRStatus;
    private Toggle pupilRStatus;

    //ui element containing line chart
    private RectTransform graphContainer;

    //template objects used to mark axis
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    //list of all ui elements making up the chart
    private List<GameObject> gameObjectList;

    //number of data points to be plotted
    public int numDisplayedValues = 200;
    public int updatePeriod = 100;
    public int baselinePeriod = 1000;
    public string mode = "none";
    private float triggerTime;
    private int sizeOnTrigger;
    public float experimentDuration = 10;
    //public float offset = 20.0f;

    //colors of different chart lines
    List<Color> colorList = new List<Color> { Color.green, Color.cyan, Color.blue, Color.red, Color.magenta, Color.yellow };

    //used to determine the frequency at which the chart is updated
    int iteration = 0;

    List<float> time;

    //to check if new data is available
    double prevTime = 0;

    private void Start()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();

        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("DashTemplateY").GetComponent<RectTransform>();

        gameObjectList = new List<GameObject>();

        thetaLStatus = graphContainer.Find("ThetaL").GetComponent<Toggle>();
        phiLStatus = graphContainer.Find("PhiL").GetComponent<Toggle>();
        pupilLStatus = graphContainer.Find("PupilL").GetComponent<Toggle>();

        thetaRStatus = graphContainer.Find("ThetaR").GetComponent<Toggle>();
        phiRStatus = graphContainer.Find("PhiR").GetComponent<Toggle>();
        pupilRStatus = graphContainer.Find("PupilR").GetComponent<Toggle>();
    }

    void FixedUpdate()
    {
        List<bool> activationList = new List<bool> { thetaLStatus.isOn, phiLStatus.isOn, pupilLStatus.isOn, thetaRStatus.isOn, phiRStatus.isOn, pupilRStatus.isOn };

        PupilDataDemo values = pupilSubscriber.GetComponent<PupilDataDemo>();

        if (values.listener != null)
        {
            //print(sizeOnTrigger);
            List<float> valueListThetaL = values.valueListThetaL;
            List<float> valueListPhiL = values.valueListPhiL;
            List<float> valueListPupilL = values.valueListPupilL;

            List<float> valueListThetaR = values.valueListThetaR;
            List<float> valueListPhiR = values.valueListPhiR;
            List<float> valueListPupilR = values.valueListPupilR;

            time = values.time;

            List<List<float>> data = new List<List<float>> { valueListThetaL, valueListPhiL, valueListPupilL, valueListThetaR, valueListPhiR, valueListPupilR };

            //print(activationList[0] + "\t" + activationList[1] + "\t" + activationList[2] + "\t" + activationList[3] + "\t" + activationList[4] + "\t" + activationList[5]);

            List<float> baseline = new List<float> { };
            //List<float> baseline = new List<float> { -10, -10, -10, -10, -10, -10 };
            //List<float> baseline = new List<float> { offset, offset, offset, offset, offset, offset };

            //foreach (List<float> list in data)
            //{
            //    float temp = 0;

            //    if (list.Count < baselinePeriod)
            //    {
            //        foreach (float value in list)
            //        {
            //            temp += value / list.Count;
            //        }
            //        baseline.Add(temp);
            //    }

            //    else
            //    {
            //        for (int i = list.Count; i > list.Count - baselinePeriod; i--)
            //        {
            //            temp += list[i - 1] / baselinePeriod;
            //        }
            //        baseline.Add(temp);
            //    }

            //}

            if (sizeOnTrigger >= baselinePeriod)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    baseline.Add(Average(data[i].GetRange(sizeOnTrigger - baselinePeriod, baselinePeriod)));
                }
            }

            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    baseline.Add(Average(data[i]));
                }
            }

            if (mode == "default" && time.Last() != prevTime)
            {
                PlotDefault(data, time, activationList, numDisplayedValues, prevTime, colorList, path, iteration, updatePeriod);
            }

            if (mode == "experiment" && time.Last() != prevTime)
            {
                PlotExperiment(data, baseline, time, activationList, sizeOnTrigger, prevTime, triggerTime, colorList, iteration, updatePeriod);
                if (time.Last() - triggerTime >= experimentDuration)
                {
                    PrintToText(path, "\n");
                    mode = "none";
                }
            }

            iteration++;
            prevTime = time.Last();

        }

    }

    public void SetModeDefault()
    {
        mode = "default";
    }

    public void SetModeExperiment()
    {
        mode = "experiment";

        sizeOnTrigger = time.Count;
        triggerTime = time.Last();

        thetaLStatus.isOn = false;
        phiLStatus.isOn = false;
        pupilLStatus.isOn = true;
        thetaRStatus.isOn = false;
        phiRStatus.isOn = false;
        pupilRStatus.isOn = true;
    }
    
    private GameObject CreateDot(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("dot", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = dotSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchorMin = new Vector2(0, 0);
        return gameObject;
    }

    private float MaxYValue(List<float> valueList, int maxVisibleNumValues, float baseline = 0)
    {
        if (maxVisibleNumValues < 0 || maxVisibleNumValues > valueList.Count)
        {
            maxVisibleNumValues = valueList.Count;
        }

        float maxValue = valueList[valueList.Count - maxVisibleNumValues] - baseline;

        for (int i = valueList.Count - maxVisibleNumValues; i < valueList.Count; i++)
        {
            if (valueList[i] - baseline > maxValue)
            {
                maxValue = valueList[i] - baseline;
            }
        }

        return maxValue;
    }

    private float MinYValue(List<float> valueList, int maxVisibleNumValues, float baseline = 0)
    {
        if (maxVisibleNumValues < 0 || maxVisibleNumValues > valueList.Count)
        {
            maxVisibleNumValues = valueList.Count;
        }

        float minValue = valueList[valueList.Count - maxVisibleNumValues] - baseline;

        for (int i = valueList.Count - maxVisibleNumValues; i < valueList.Count; i++)
        {
            if (valueList[i] - baseline < minValue)
            {
                minValue = valueList[i] - baseline;
            }
        }

        return minValue;

    }

    private void ClearAll()
    {
        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }

        gameObjectList.Clear();
    }

    private void CreateLabelX(List<float> time, int maxVisibleNumValues)
    {
        if (maxVisibleNumValues <= 0 || maxVisibleNumValues > time.Count)
        {
            maxVisibleNumValues = time.Count;
        }

        //width of rectangel containing graph in pixels
        float graphWidth = graphContainer.sizeDelta.x;

        //#pixesl between plotted values
        float xSize = graphWidth / maxVisibleNumValues;

        //index for iteration purposes
        int xIndex = 1;


        //iterate over values to be displayed
        for (int i = time.Count - maxVisibleNumValues; i < time.Count; i++)
        {
            float xPosition = xSize * (i - time.Count + maxVisibleNumValues);

            if (maxVisibleNumValues / xIndex == 10)
            {
                RectTransform labelX = Instantiate(labelTemplateX);
                labelX.SetParent(graphContainer);
                labelX.gameObject.SetActive(true);

                labelX.anchoredPosition = new Vector2(xPosition - 3.0f, -8.0f);

                labelX.GetComponent<Text>().text = (Mathf.Round(10.0f * time[i]) / 10.0f).ToString();

                gameObjectList.Add(labelX.gameObject);

                RectTransform dashX = Instantiate(dashTemplateY);
                dashX.SetParent(graphContainer);
                dashX.gameObject.SetActive(true);

                dashX.anchoredPosition = new Vector2(xPosition, 0.0f);

                gameObjectList.Add(dashX.gameObject);
                xIndex = 1;
            }

            xIndex++;

        }
    }

    private void CreateLabelY(float yMax, float yMin)
    {
        float separatorCount = 11.0f;

        float graphHeight = graphContainer.sizeDelta.y;

        float yDifference = yMax - yMin;

        if (yDifference <= 0)
        {
            yDifference = 1.0f;
        }

        yMax = yMax + yDifference * 0.1f;
        yMin = yMin - yDifference * 0.1f;

        for (int i = 0; i < separatorCount; i++)
        {
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = ((float)i) * 1.0f / (separatorCount - 1.0f);
            labelY.anchoredPosition = new Vector2(-10.0f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = (Mathf.Round((yMin + normalizedValue * (yMax - yMin)) * 10.0f) / 10.0f).ToString();
            gameObjectList.Add(labelY.gameObject);

            RectTransform dashY = Instantiate(dashTemplateX);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(0.0f, normalizedValue * graphHeight);
            gameObjectList.Add(dashY.gameObject);
        }
    }

    private void ShowGraph(List<float> valueList, Color color, float yMax, float yMin, int maxVisibleNumValues = -1, float baseline = 0)
    {
        if (maxVisibleNumValues <= 0 || maxVisibleNumValues > valueList.Count)
        {
            maxVisibleNumValues = valueList.Count;
        }

        float graphHeight = graphContainer.sizeDelta.y;
        float graphWidth = graphContainer.sizeDelta.x;

        float yDifference = yMax - yMin;

        if (yDifference <= 0)
        {
            yDifference = 5.0f;
        }

        yMax = yMax + yDifference * 0.1f;
        yMin = yMin - yDifference * 0.1f;

        float xSize = graphWidth / maxVisibleNumValues;

        int xIndex = 0;

        GameObject prevDot = null;

        for (int i = valueList.Count - maxVisibleNumValues; i < valueList.Count; i++)
        {
            float xPosition = xIndex * xSize;
            float yPosition = ((valueList[i] - yMin - baseline) / (yMax - yMin)) * graphHeight;

            GameObject dot = CreateDot(new Vector2(xPosition, yPosition));
            gameObjectList.Add(dot);

            if (prevDot != null)
            {
                GameObject dotConnectionObject = CreateDotConnection(prevDot.GetComponent<RectTransform>().anchoredPosition, dot.GetComponent<RectTransform>().anchoredPosition, color);
                gameObjectList.Add(dotConnectionObject);
            }

            prevDot = dot;

            xIndex++;
        }
    }

    private GameObject CreateDotConnection(Vector2 startPosition, Vector2 endPosition, Color color)
    {
        GameObject gameObject = new GameObject("dotConnection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = color;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (endPosition - startPosition).normalized;
        float distance = Vector2.Distance(startPosition, endPosition);

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 1.0f);
        rectTransform.anchoredPosition = startPosition + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));

        return gameObject;
    }

    private void PrintToText(string path, string text)
    {
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();
    }

    private void PlotDefault(List<List<float>> data, List<float> time, List<bool> activationList, int numDisplayedValues, double prevTime, List<Color> colorList, string path, int iteration, int updatePeriod)
    {
        float yMax = -1.0f / 0.0f;
        float yMin = 1.0f / 0.0f;

        for (int i = 0; i < activationList.Count; i++)
        {
            if (activationList[i] && MaxYValue(data[i], numDisplayedValues) > yMax)
            {
                yMax = MaxYValue(data[i], numDisplayedValues);
            }

            if (activationList[i] && MinYValue(data[i], numDisplayedValues) < yMin)
            {
                yMin = MinYValue(data[i], numDisplayedValues);
            }
        }

        List<float> valueListThetaL = data[0];
        List<float> valueListPhiL = data[1];
        List<float> valueListPupilL = data[2];
        List<float> valueListThetaR = data[3];
        List<float> valueListPhiR = data[4];
        List<float> valueListPupilR = data[5];


        if (iteration % updatePeriod == 0 && time.Last() != prevTime)
        {
            ClearAll();

            for (int i = 0; i < activationList.Count; i++)
            {
                if (activationList[i])
                {
                    ShowGraph(data[i], colorList[i], yMax, yMin, numDisplayedValues);
                }
            }

            CreateLabelY(yMax, yMin);
            CreateLabelX(time, numDisplayedValues);
        }

        if (time.Last() != prevTime)
        {
            string text = "Theta L = " + valueListThetaL.Last() +
                "\tPhi L = " + valueListPhiL.Last() +
                "\tPupil L = " + valueListPupilL.Last() +
                "\tTheta R = " + valueListThetaR.Last() +
                "\tPhi R = " + valueListPhiR.Last() +
                "\tPupil R = " + valueListPupilR.Last() +
                "\tt = " + time.Last();
            PrintToText(path, text);
        }
    }

    private void PlotExperiment(List<List<float>> data, List<float> baseline, List<float> time, List<bool> activationList, int sizeOnTrigger, 
        double prevTime, float triggerTime, List<Color> colorList, int iteration, int updatePeriod)
    {
        float yMax = -1.0f / 0.0f;
        float yMin = 1.0f / 0.0f;

        int numDisplayedValues = time.Count - sizeOnTrigger;

        for (int i = 0; i < activationList.Count; i++)
        {
            if (activationList[i] && MaxYValue(data[i], numDisplayedValues, (baseline[2] + baseline[5]) / 2.0f) > yMax)
            {
                yMax = MaxYValue(data[i], numDisplayedValues, (baseline[2] + baseline[5]) / 2.0f);
            }

            if (activationList[i] && MinYValue(data[i], numDisplayedValues, (baseline[2] + baseline[5]) / 2.0f) < yMin)
            {
                yMin = MinYValue(data[i], numDisplayedValues, (baseline[2] + baseline[5]) / 2.0f);
            }
            print(yMax.ToString() +  yMin. ToString());
        }

        //print("Min = " + yMin + "\tMax = " + yMax);


        if (iteration % updatePeriod == 0 && time.Last() != prevTime)
        {
            ClearAll();

            ShowGraph(data[2], colorList[2], yMax, yMin, numDisplayedValues, baseline[2]);
            ShowGraph(data[5], colorList[5], yMax, yMin, numDisplayedValues, baseline[5]);

            CreateLabelY(yMax, yMin);
            CreateLabelX(time, numDisplayedValues);
        }

        if (time.Last() != prevTime)
        {
            string text = "Pupil L = " + (data[2].Last() - baseline[2]) +
                "\tPupil R = " + (data[5].Last() - baseline[5]) +
                "\tt = " + time.Last();
            PrintToText(path, text);
        }
    }

    private float Average(List<float> values)
    {
        float temp = 0;

        foreach(float value in values)
        {
            temp += value;
        }

        return temp / values.Count;
    }
}