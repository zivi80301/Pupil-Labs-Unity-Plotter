using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PupilLabs.Demos
{
    public class PupilDataDemo : MonoBehaviour
    {
        public SubscriptionsController subsCtrl;
        public TimeSync timeSync;

        public Text EyeLStatus;
        public Text EyeRStatus;

        public PupilListener listener;

        public float thetaL;
        public float phiL;
        public float pupilDiameterL;

        public float prevThetaL = 0;
        public float prevPhiL = 0;

        public float thetaR;
        public float phiR;
        public float pupilDiameterR;

        public float prevThetaR = 0;
        public float prevPhiR = 0;

        public List<float> valueListThetaL = new List<float> { };
        public List<float> valueListPhiL = new List<float> { };
        public List<float> valueListPupilL = new List<float> { };

        public List<float> valueListThetaR = new List<float> { };
        public List<float> valueListPhiR = new List<float> { };
        public List<float> valueListPupilR = new List<float> { };

        public List<float> time = new List<float> { };

        string path = "Assets/Data/rawdata" + DateTime.Now.ToString().Replace("/", "-").Replace(" ", "_").Replace(":", "-") + ".txt";

        //on enable register new listener if none is registered. enable listener
        void OnEnable()
        {
            if (listener == null)
            {
                listener = new PupilListener(subsCtrl);
            }

            listener.Enable();
            listener.OnReceivePupilData += ReceivePupilData;
        }

        //disable listener if script is disabled
        void OnDisable()
        {
            listener.Disable();
            listener.OnReceivePupilData -= ReceivePupilData;
        }

        //print pupilData.Method and pupilData.Confidence at unityTime
        //print theta and phi values of eye with index 0
        void ReceivePupilData(PupilData pupilData)
        {
            if (pupilData.EyeIdx == 1 && !(pupilData.Circle.Theta == 0 || pupilData.Circle.Phi == 0 || pupilData.Diameter3d == 0))
            {
                double timeStamp = timeSync.ConvertToUnityTime(pupilData.PupilTimestamp);

                thetaL = Mathf.Rad2Deg * pupilData.Circle.Theta;
                phiL = Mathf.Rad2Deg * pupilData.Circle.Phi;
                pupilDiameterL = pupilData.Diameter3d * 10.0f;

                if (!IsBlinking(pupilData))
                {
                    valueListThetaL.Add(thetaL);
                    valueListPhiL.Add(phiL);
                    valueListPupilL.Add(pupilDiameterL);
                }

                else
                {
                    valueListThetaL.Add(valueListThetaL.Last());
                    valueListPhiL.Add(valueListPhiL.Last());
                    valueListPupilL.Add(valueListPupilL.Last());
                }

                time.Add((float) timeStamp);

                string text = "Theta L = " + thetaL.ToString() + "\tPhi L = " + phiL.ToString() + "\tPupil L = " + (Mathf.Round(pupilDiameterL * 10) / 10).ToString() + "\tt = " + timeStamp;

                EyeLStatus.text = text;

                PrintToText(path, text);
            }

            if (pupilData.EyeIdx == 0 && !(pupilData.Circle.Theta == 0 || pupilData.Circle.Phi == 0 || pupilData.Diameter3d == 0))
            {
                double timeStamp = timeSync.ConvertToUnityTime(pupilData.PupilTimestamp);

                thetaR = Mathf.Rad2Deg * pupilData.Circle.Theta;
                phiR = Mathf.Rad2Deg * pupilData.Circle.Phi;
                pupilDiameterR = pupilData.Diameter3d * 10.0f;

                if (!IsBlinking(pupilData))
                {
                    valueListThetaR.Add(thetaR);
                    valueListPhiR.Add(phiR);
                    valueListPupilR.Add(pupilDiameterR);
                }

                else
                {
                    valueListThetaR.Add(valueListThetaR.Last());
                    valueListPhiR.Add(valueListPhiR.Last());
                    valueListPupilR.Add(valueListPupilR.Last());
                }

                string text = "Theta R = " + thetaR.ToString() + "\tPhi R = " + phiR.ToString() + "\tPupil R = " + (Mathf.Round(pupilDiameterR * 10) / 10).ToString() + "\tt = " + timeStamp;

                EyeRStatus.text = text;

                PrintToText(path, text);
            }
        }

        void PrintToText(string path, string text)
        {
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(text);
            writer.Close();
        }

        bool IsBlinking(PupilData pupilData)
        {
            if(pupilData.Confidence < 0.8f)
            {
                return true;
            }

            return false;
        }
    }
}