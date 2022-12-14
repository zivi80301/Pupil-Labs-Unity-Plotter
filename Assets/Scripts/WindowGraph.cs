using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using PupilLabs.Demos;

public class WindowGraph : MonoBehaviour
{
    [SerializeField]

    private Sprite dotSprite;

    //public InputGenerator valueGenerator;
    public PupilDataDemo pupilSubscriber;

    private Toggle thetaLStatus;
    private Toggle phiLStatus;
    private Toggle pupilLStatus;

    private Toggle thetaRStatus;
    private Toggle phiRStatus;
    private Toggle pupilRStatus;

    private RectTransform graphContainer;

    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;

    private List<GameObject> gameObjectList;

    private List<float> time;

    public int numDisplayedValues = 100;

    int iteration = 0;

    //private void Awake()
    //{
    //    graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
    //    time = new List<float> { Time.time };

    //    labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
    //    labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
    //    dashTemplateX = graphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
    //    dashTemplateY = graphContainer.Find("DashTemplateY").GetComponent<RectTransform>();

    //    gameObjectList = new List<GameObject>();

    //    List<int> valueList = new List<int> { 0 };
    //    List<int> valueList2 = new List<int> { 0 };

    //    FunctionPeriodic.Create(() =>
    //    {
    //        InputGenerator values = valueGenerator.GetComponent<InputGenerator>();

    //        valueList.Add(values.val);
    //        valueList2.Add(values.val2);

    //        time.Add(Time.time);

    //        int max = Mathf.Max(MaxYValue(valueList), MaxYValue(valueList2));
    //        int min = Mathf.Min(MinYValue(valueList), MinYValue(valueList2));

    //        if (iteration % 50 == 0)
    //        {
    //            ClearAll();

    //            ShowGraph(valueList, Color.green, (float)max, (float)min, numDisplayedValues);
    //            ShowGraph(valueList2, Color.red, (float)max, (float)min, numDisplayedValues);

    //            CreateLabelY((float)max, (float)min);
    //            CreateLabelX(valueList.Count, numDisplayedValues);
    //        }

    //        if ((numDisplayedValues > 0) && (valueList.Count >= numDisplayedValues))
    //        {
    //            for (int i = 0; i < valueList.Count - numDisplayedValues; i++)
    //            {
    //                valueList.RemoveAt(0);
    //                valueList2.RemoveAt(0);
    //                time.RemoveAt(0);
    //            }
    //        }

    //        iteration++;

    //    }, 0.1f);
    //}

    private void Start()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        time = new List<float> { Time.time };

        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("DashTemplateY").GetComponent<RectTransform>();

        gameObjectList = new List<GameObject>();

        List<float> valueListThetaL = new List<float> { };
        List<float> valueListPhiL = new List<float> { };
        List<float> valueListPupilL = new List<float> { };

        List<float> valueListThetaR = new List<float> { };
        List<float> valueListPhiR = new List<float> { };
        List<float> valueListPupilR = new List<float> { };

        List<Color> colorList = new List<Color> { Color.green, Color.cyan, Color.blue, Color.red, Color.magenta, Color.yellow };

        thetaLStatus = graphContainer.Find("ThetaL").GetComponent<Toggle>();
        phiLStatus = graphContainer.Find("PhiL").GetComponent<Toggle>();
        pupilLStatus = graphContainer.Find("PupilL").GetComponent<Toggle>();

        thetaRStatus = graphContainer.Find("ThetaR").GetComponent<Toggle>();
        phiRStatus = graphContainer.Find("PhiR").GetComponent<Toggle>();
        pupilRStatus = graphContainer.Find("PupilR").GetComponent<Toggle>();

        FunctionPeriodic.Create(() =>
        {
            List<bool> activationList = new List<bool> { thetaLStatus.isOn, phiLStatus.isOn, pupilLStatus.isOn, thetaRStatus.isOn, phiRStatus.isOn, pupilRStatus.isOn };

            PupilDataDemo values = pupilSubscriber.GetComponent<PupilDataDemo>();

            if (values.listener != null ) {
                valueListThetaL.Add(values.thetaL);
                valueListPhiL.Add(values.phiL);
                valueListPupilL.Add(values.pupilDiameterL * 10.0f);

                valueListThetaR.Add(values.thetaR);
                valueListPhiR.Add(values.phiR);
                valueListPupilR.Add(values.pupilDiameterR * 10.0f);
            }

            List<List<float>> data = new List<List<float>> { valueListThetaL, valueListPhiL, valueListPupilL, valueListThetaR, valueListPhiR, valueListPupilR };

            time.Add(Time.time);

            float max = -1.0f/0.0f;
            float min = 1.0f/0.0f;

            for (int i = 0; i < activationList.Count; i++)
            {
                if (activationList[i] && MaxYValue(data[i]) > max)
                {
                    max = MaxYValue(data[i]);
                }

                if (activationList[i] && MinYValue(data[i]) < min)
                {
                    min = MinYValue(data[i]);
                }
            }

            if (iteration % 10 == 0)
            {
                ClearAll();

                for (int i = 0; i < activationList.Count; i++)
                {
                    if (activationList[i])
                    {
                        ShowGraph(data[i], colorList[i], max, min, numDisplayedValues);
                    }
                }
                //ShowGraph(valueListThetaL, Color.green, max, min, numDisplayedValues);

                CreateLabelY(max, min);
                CreateLabelX(valueListThetaL.Count, numDisplayedValues);
            }

            if ((numDisplayedValues > 0) && (valueListThetaL.Count >= numDisplayedValues))
            {
                for (int i = 0; i < valueListThetaL.Count - numDisplayedValues; i++)
                {
                    valueListThetaL.RemoveAt(0);
                    valueListPhiL.RemoveAt(0);
                    valueListPupilL.RemoveAt(0);

                    valueListThetaR.RemoveAt(0);
                    valueListPhiR.RemoveAt(0);
                    valueListPupilR.RemoveAt(0);

                    time.RemoveAt(0);
                }
            }

            iteration++;
            //print(MinYValue(valueListPupilR) + "\t" + MaxYValue(valueListPupilL));
            print(min + "\t" + max);

        }, 0.1f);
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

    //private int MaxYValue(List<int> valueList)
    //{
    //    int maxValue = valueList[0];
    //    foreach (int value in valueList)
    //    {
    //        if (value > maxValue)
    //        {
    //            maxValue = value;
    //        }
    //    }

    //    return maxValue;
    //}

    //private int MinYValue(List<int> valueList)
    //{
    //    int minValue = valueList[0];
    //    foreach (int value in valueList)
    //    {
    //        if (value < minValue)
    //        {
    //            minValue = value;
    //        }
    //    }

    //    return minValue;
    //}

    
    //add parameter activationList of type List<bool> to get max of all active relevant lists
    private float MaxYValue(List<float> valueList)
    {
        float maxValue = valueList[0];
        foreach (float value in valueList)
        {
            if (value > maxValue)
            {
                maxValue = value;
            }
        }

        return maxValue;
    }

    private float MinYValue(List<float> valueList)
    {
        float minValue = valueList[0];
        foreach (float value in valueList)
        {
            if (value < minValue)
            {
                minValue = value;
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

    private void CreateLabelX(int listLength, int maxVisibleNumValues)
    {
        if (maxVisibleNumValues <= 0)
        {
            maxVisibleNumValues = listLength;
        }


        float graphWidth = graphContainer.sizeDelta.x;
        float xSize = graphWidth / maxVisibleNumValues; ;
        int xIndex = 0;
        int labelXSpacing = 10 * ((int)(Mathf.Log10(listLength) / Mathf.Log10(2)) + 1);

        //for (int i = Mathf.Max(listLength - maxVisibleNumValues, 0); i < listLength; i++)
        for (int i = 0; i < time.Count; i++)
        {
            float xPosition = xIndex * xSize;
            if (xIndex % labelXSpacing == 0)
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

            }

            xIndex++;

        }
    }

    private void CreateLabelY(float yMax, float yMin)
    {
        int separatorCount = 11;

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
            float normalizedValue = (float) i * 1.0f / ((float) separatorCount - 1);
            labelY.anchoredPosition = new Vector2(-10.0f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = Mathf.RoundToInt(yMin + normalizedValue * (yMax - yMin)).ToString();
            gameObjectList.Add(labelY.gameObject);

            RectTransform dashY = Instantiate(dashTemplateX);
            dashY.SetParent(graphContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(0.0f, normalizedValue * graphHeight);
            gameObjectList.Add(dashY.gameObject);
        }
    }

    //private void ShowGraph(List<int> valueList, Color color, float yMax, float yMin, int maxVisibleNumValues = -1)
    //{
    //    if (maxVisibleNumValues <= 0)
    //    {
    //        maxVisibleNumValues = valueList.Count;
    //    }

    //    float graphHeight = graphContainer.sizeDelta.y;
    //    float graphWidth = graphContainer.sizeDelta.x;

    //    float yDifference = yMax - yMin;

    //    if (yDifference <= 0)
    //    {
    //        yDifference = 5.0f;
    //    }

    //    yMax = yMax + yDifference * 0.1f;
    //    yMin = yMin - yDifference * 0.1f;

    //    float xSize = graphWidth / maxVisibleNumValues;

    //    int xIndex = 0;

    //    GameObject prevDot = null;

    //    for (int i = 0; i < valueList.Count; i++)
    //    {
    //        float xPosition = xIndex * xSize;
    //        float yPosition = ((valueList[i] - yMin) / (yMax - yMin)) * graphHeight;

    //        GameObject dot = CreateDot(new Vector2(xPosition, yPosition));
    //        gameObjectList.Add(dot);

    //        if (prevDot != null)
    //        {
    //            GameObject dotConnectionObject = CreateDotConnection(prevDot.GetComponent<RectTransform>().anchoredPosition, dot.GetComponent<RectTransform>().anchoredPosition, color);
    //            gameObjectList.Add(dotConnectionObject);
    //        }

    //        prevDot = dot;

    //        xIndex++;
    //    }
    //}

    private void ShowGraph(List<float> valueList, Color color, float yMax, float yMin, int maxVisibleNumValues = -1)
    {
        if (maxVisibleNumValues <= 0)
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

        for (int i = 0; i < valueList.Count; i++)
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
}