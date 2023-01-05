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

    //Sprite used to draw line chart
    private Sprite dotSprite;

    //Path to the .txt file where data is stored
    string path = "Assets/Data/data" + DateTime.Now.ToString().Replace("/", "-").Replace(" ", "_").Replace(":", "-") + ".txt";

    //Pupil subscriber supplying data to ploter
    public PupilDataDemo pupilSubscriber;

    //Toggle buttons
    private Toggle thetaLStatus;
    private Toggle phiLStatus;
    private Toggle pupilLStatus;

    private Toggle thetaRStatus;
    private Toggle phiRStatus;
    private Toggle pupilRStatus;

    //UI element containing line chart
    private RectTransform graphContainer;

    //Template objects used to mark axis
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    //List of all ui elements making up the chart
    private List<GameObject> gameObjectList;

    //Number of data points to be plotted
    public int numDisplayedValues = 200;

    //Number of iterations which need to pass for the Chart to be updated
    public int updatePeriod = 100;

    //Number of data points considered to average over when calculating the baseline value
    public int baselinePeriod = 1000;

    //Mode of operation. default for raw data plotting, experiment when using the experiment funcitonality
    public string mode = "none";

    //Timestamp at which experiment is started
    private float triggerTime;

    //Number of elements in the list containing timestamps when experiment is started
    private int sizeOnTrigger;

    //Duration for which data is plotted in experiment mode in seconds
    public float experimentDuration = 10;

    //Colors of different chart lines
    List<Color> colorList = new List<Color> { Color.green, Color.cyan, Color.blue, Color.red, Color.magenta, Color.yellow };

    //Used to determine the frequency at which the chart is updated
    int iteration = 0;
    
    //List containing timestamps 
    List<float> time;

    //To check if new data is available
    double prevTime = 0;

    //Initializes some UI elements
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

    //Iterates over pupil data and updates chart every updatePeriod iteration. Plots data depending on mode of operation and selected values to be plotted
    //Calls Average, PlotDefault, PlotExperiment, PrintToText
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

    //Called on button press. Sets mode to default
    public void SetModeDefault()
    {
        mode = "default";
    }

    //Called on button oress. Sets mode to experiment, activates pupilL and pupilR, deactivates the rest. Sets sizeOnTrigger and triggerTime at time of button press
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
    
    //Ceates dot object form dot sprite at coordinates anchoredPosition
    //Takes Vector2 anchoredPosition
    //Returns position of created dot
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

    //Calculates maximum of last maxVisibleNumValues elements in a list of floats. Optionally applies the offset baseline
    //Takes List<float> valueList, the list to find the maximum in, int maxVisibleNumValues the number of values to consider, flaot baseline the optional offset
    //Returns float maximum
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

    //Calculates minimum of last maxVisibleNumValues elements in a list of floats. Optionally applies the offset baseline
    //Takes List<float> valueList, the list to find the minimum in, int maxVisibleNumValues the number of values to consider, flaot baseline the optional offset
    //Retuns float minimum
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

    //Removes all UI elements cerated by this script
    private void ClearAll()
    {
        foreach (GameObject gameObject in gameObjectList)
        {
            Destroy(gameObject);
        }

        gameObjectList.Clear();
    }

    //Creates evenly spaced label for time axis with timestamps
    //Takes List<float> time the list with all timestamps, maxVisibleNumValues the number of values to be displayed
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

    //Creates evenly spaced label for input values, adaptively changes linear scale to fit range of displayed data to the size of the chart.
    //Takes float yMax the maximum upper limit of displayed data, float yMin the lower limit of displayed data
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

    //Creates 2D objects to make up line chart from input data in form of a list of floats
    //Takes List<float> valueList the input data, Color color the color associated with this data series, float yMax, yMin the maximum and minimum of all input values, not necessarily in the valueList, 
    //int maxVisibleNumValues the number of values to be plotted, default is all, float baseline the offset to apply to input data, default is 0
    //Calls CrateDot, CreateDotConnection
    private void ShowGraph(List<float> valueList, Color color, float yMax, float yMin, int maxVisibleNumValues = -1, float baseline = 0.0f)
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

    //Draws line between two 2D points on the chart
    //Takes Vector2 startPosition, endPosition the coordinates of both ends of the straight line
    //Returns Vector2 the position of the line object
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

    //Appends line text to .txt file at location path
    //Takes string path the path to the .txt file, text the line to be appended to the text file
    private void PrintToText(string path, string text)
    {
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(text);
        writer.Close();
    }

    //Plotts data series selected with UI toggle buttons
    //Takes List<List<float> data the list containing the individual data series, List<float> time the list containing all timestamps, int numDisplayValues the number of datapoints to plot
    //double prevTime the last timestamp when new data was awaylable, List<Color> color the colorList, string path the path to the .txt file storing the data, int iteration the current iteration in the FixedUpdate function
    //the flobal updatePeriod
    //Calls MaxYValue, MinYValue, ClearAll, ShowGraph, CreateLabelY, CreateLabelX, PrintToText
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

    //Plotts pupil size data series for both eyes for a predetermined timeframe
    //Takes List<List<float> data the list containing the individual data series, List<float> baseline contains the baseline for each data series, List<float> time the list containing all timestamps,
    //List<bool> activationList list of bools storing the activation status for each pupil data series, int sizeOnTrigger the^number of elements in the timeStamp list time,
    //double prevTime the last timestamp when new data was awaylable, List<Color> color the colorList, int iteration the current iteration in the FixedUpdate function, int updatePeriod
    //the global updatePeriod
    //Calls MaxYValue, MinYValue, ClearAll, ShowGraph, CreateLabelY, CreateLabelX, PrintToText
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
        float medianBaseline = 0;
        int count = 0;

        for(int i = 0; i < activationList.Count; i++)
        {
            if (activationList[i])
            {
                medianBaseline += baseline[i];
                count++;
            }
        }

        medianBaseline = medianBaseline / count;


        if (iteration % updatePeriod == 0 && time.Last() != prevTime)
        {
            ClearAll();

            ShowGraph(data[2], colorList[2], yMax, yMin, numDisplayedValues, medianBaseline);
            ShowGraph(data[5], colorList[5], yMax, yMin, numDisplayedValues, medianBaseline);

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

    //Averages the float values in a list
    //Takes List<floatY values
    //Returns float average
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