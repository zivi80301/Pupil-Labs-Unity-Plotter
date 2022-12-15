using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using PupilLabs.Demos;

public class WindowGraph : MonoBehaviour
{
    [SerializeField]

    private Sprite dotSprite;

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

    //private List<float> time;

    public int numDisplayedValues = 100;

    int iteration = 0;

    private void Start()
    {
        graphContainer = transform.Find("GraphContainer").GetComponent<RectTransform>();
        //time = new List<float> { Time.time };

        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        dashTemplateX = graphContainer.Find("DashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = graphContainer.Find("DashTemplateY").GetComponent<RectTransform>();

        gameObjectList = new List<GameObject>();

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

                //time.Add(Time.time);

                float max = -1.0f / 0.0f;
                float min = 1.0f / 0.0f;

                for (int i = 0; i < activationList.Count; i++)
                {
                    if (activationList[i] && MaxYValue(data[i], numDisplayedValues) > max)
                    {
                        max = MaxYValue(data[i], numDisplayedValues);
                    }

                    if (activationList[i] && MinYValue(data[i], numDisplayedValues) < min)
                    {
                        min = MinYValue(data[i], numDisplayedValues);
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

                    CreateLabelY(max, min);
                    CreateLabelX(time, numDisplayedValues);
                }

                //if ((numDisplayedValues > 0) && (time.Count >= numDisplayedValues))
                //{
                //    while (time.Count > numDisplayedValues)
                //    {
                //        print(time.Count);
                //        time.RemoveAt(0);
                //    }
                //}

                iteration++;

            }

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
    
    //add parameter activationList of type List<bool> to get max of all active relevant lists
    private float MaxYValue(List<float> valueList, int maxVisibleNumValues)
    {
        if (maxVisibleNumValues < 0 || maxVisibleNumValues > valueList.Count)
        {
            maxVisibleNumValues = valueList.Count;
        }

        //print(valueList.Count + "\t" + maxVisibleNumValues);
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
        //spacing of x-values in (data points)
        int labelXSpacing = 10;

        //if(plot all values == true) -> set limit to #total values
        if(maxVisibleNumValues <= 0 || maxVisibleNumValues > time.Count)
        {
            maxVisibleNumValues = time.Count;
        }

        print(maxVisibleNumValues);
        //if (maxVisibleNumValues <= 0)
        //{
        //    maxVisibleNumValues = time.Count;
        //    labelXSpacing = 5 * ((int)(Mathf.Log10(time.Count) / Mathf.Log10(5)) + 1);
        //}

        //else
        //{
        //    labelXSpacing = 10;
        //}

        //width of rectangel containing graph in pixels
        float graphWidth = graphContainer.sizeDelta.x;

        //#pixesl between plotted values
        float xSize = graphWidth / maxVisibleNumValues; 

        //index for iteration purposes
        int xIndex = 1;


        //iterate over values to be displayed
        for (int i = time.Count - maxVisibleNumValues; i < time.Count; i++)
        {
            //print("XLabel Called");
            //print(xIndex);
            float xPosition = xSize * (i - time.Count + maxVisibleNumValues);
            //if (xPosition % labelXSpacing == 0)
            if(maxVisibleNumValues / xIndex == 10)
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
            float normalizedValue = ((float) i) * 1.0f / (separatorCount - 1.0f);
            labelY.anchoredPosition = new Vector2(-10.0f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = (Mathf.Round((yMin + normalizedValue * (yMax - yMin))*10.0f)/10.0f).ToString();
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
}