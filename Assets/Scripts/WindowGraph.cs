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
    public int numDisplayedValues = 100;
    public int updatePeriod = 20;

    //colors of different chart lines
    List<Color> colorList = new List<Color> { Color.green, Color.cyan, Color.blue, Color.red, Color.magenta, Color.yellow };

    //used to determine the frequency at which the chart is updated
    int iteration = 0;

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
        //Default: as is
        //DemoExperiment: pupilLStatus = true, pupilRStatus = true
        //                average out past pupil values
        //                plot pupilValues - average normally for n seconds after button press
        //                change mode outside of game before launch

        //TODO: SetModeDefault():
        //          change mode
        //          change active
        //      SetModeDemoExperiment()
        //          change mode
        //          change active
        //      PlotDefault(values, numDisplayedValues, yMax, yMain, activationList, time, prevTime, color):
        //          clear current canvase, plot all active values according to numDisplayedValues, write all data to .txt file
        //      PlotDemoExperiment(values, experimentTime, yMax, yMin, activationList, time, prevTime, color):
        //          clear current canvase
        //          calculate numDisplayedValues from experiment time
        //          plot active values - average of previous n data points before trigger
        //          write pupil diameter to .txt file during experiment interval 
        //          stop plotting, but do not clear graph once finished

        //Structure:
        //      FixedUpdate:
        //          get values; get active; get yMin/yMax
        //          if(mode == default && index%period == 0):
        //              PlotDefault(...)
        //          if(mode == demoExperiment && index%period == 0 && DemoExperimentTrigger == true):
        //              PlotExperiment

        List<bool> activationList = new List<bool> { thetaLStatus.isOn, phiLStatus.isOn, pupilLStatus.isOn, thetaRStatus.isOn, phiRStatus.isOn, pupilRStatus.isOn };

        PupilDataDemo values = pupilSubscriber.GetComponent<PupilDataDemo>();

        if (values.listener != null)
        {

            List<float> valueListThetaL = values.valueListThetaL;
            List<float> valueListPhiL = values.valueListPhiL;
            List<float> valueListPupilL = values.valueListPupilL;

            List<float> valueListThetaR = values.valueListThetaR;
            List<float> valueListPhiR = values.valueListPhiR;
            List<float> valueListPupilR = values.valueListPupilR;

            List<float> time = values.time;

            List<List<float>> data = new List<List<float>> { valueListThetaL, valueListPhiL, valueListPupilL, valueListThetaR, valueListPhiR, valueListPupilR };

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

            PlotDefault(data, time, activationList, yMax, yMin, numDisplayedValues, prevTime, colorList, path, iteration, updatePeriod);
            //if (iteration % 50 == 0 && time.Last() != prevTime)
            //{
            //    ClearAll();

            //    for (int i = 0; i < activationList.Count; i++)
            //    {
            //        if (activationList[i])
            //        {
            //            ShowGraph(data[i], colorList[i], max, min, numDisplayedValues);
            //        }
            //    }

            //    CreateLabelY(max, min);
            //    CreateLabelX(time, numDisplayedValues);
            //}

            //if (time.Last() != prevTime)
            //{
            //    string text = "Theta L = " + valueListThetaL.Last() +
            //        "\tPhi L = " + valueListPhiL.Last() +
            //        "\tPupil L = " + valueListPupilL.Last() +
            //        "\tTheta R = " + valueListThetaR.Last() +
            //        "\tPhi R = " + valueListPhiR.Last() +
            //        "\tPupil R = " + valueListPupilR.Last() +
            //        "\tt = " + time.Last();
            //    PrintToText(path, text);
            //}

            iteration++;
            prevTime = time.Last();

        }

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

    private float MaxYValue(List<float> valueList, int maxVisibleNumValues)
    {
        if (maxVisibleNumValues < 0 || maxVisibleNumValues > valueList.Count)
        {
            maxVisibleNumValues = valueList.Count;
        }

        float maxValue = valueList[valueList.Count - maxVisibleNumValues];

        for (int i = valueList.Count - maxVisibleNumValues; i < valueList.Count; i++)
        {
            if (valueList[i] > maxValue)
            {
                maxValue = valueList[i];
            }
        }

        return maxValue;
    }

    private float MinYValue(List<float> valueList, int maxVisibleNumValues)
    {
        if (maxVisibleNumValues < 0 || maxVisibleNumValues > valueList.Count)
        {
            maxVisibleNumValues = valueList.Count;
        }

        float minValue = valueList[valueList.Count - maxVisibleNumValues];

        for (int i = valueList.Count - maxVisibleNumValues; i < valueList.Count; i++)
        {
            if (valueList[i] < minValue)
            {
                minValue = valueList[i];
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

    private void ShowGraph(List<float> valueList, Color color, float yMax, float yMin, int maxVisibleNumValues = -1)
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
            float yPosition = ((valueList[i] - yMin) / (yMax - yMin)) * graphHeight;

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

    private void PlotDefault(List<List<float>> data, List<float> time, List<bool> activationList, float yMax, float yMin, int numDisplayedValues, double prevTime, List<Color> colorList, string path, int iteration, int updatePeriod)
    {
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
}