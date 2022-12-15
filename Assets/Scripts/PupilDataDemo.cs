using System.Collections.Generic;
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

        double prevTimeL = 0;
        double prevTimeR = 0;

        public List<float> valueListThetaL = new List<float> { };
        public List<float> valueListPhiL = new List<float> { };
        public List<float> valueListPupilL = new List<float> { };

        public List<float> valueListThetaR = new List<float> { };
        public List<float> valueListPhiR = new List<float> { };
        public List<float> valueListPupilR = new List<float> { };

        //on enable resgiter new listener if none is registered. enable listener
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
                double time = timeSync.ConvertToUnityTime(pupilData.PupilTimestamp);
                thetaL = Mathf.Rad2Deg * pupilData.Circle.Theta;
                //thetaL = Mathf.Round((float)((Mathf.Rad2Deg * pupilData.Circle.Theta - prevThetaL) / (time - prevTimeL)));
                phiL = Mathf.Rad2Deg * pupilData.Circle.Phi;
                //phiL = Mathf.Round((float)((Mathf.Rad2Deg * pupilData.Circle.Phi - prevPhiL) / (time - prevTimeL)));
                pupilDiameterL = pupilData.Diameter3d;

                valueListThetaL.Add(thetaL);
                valueListPhiL.Add(phiL);
                valueListPupilL.Add(pupilDiameterL);

                //EyeLStatus.text = "Theta L = " + ((thetaL - prevThetaL) / (time - prevTime)).ToString() + "\tPhi L = " + ((phiL - prevPhiL) / (time - prevTime)).ToString() + "\tPupil L = " + pupilDiameterL.ToString();
                EyeLStatus.text = "Theta L = " + Mathf.Round((float)((thetaL - prevThetaL) / (time - prevTimeL))).ToString() + "\tPhi L = " + Mathf.Round((float)((phiL - prevPhiL) / (time - prevTimeL))).ToString() + "\tPupil L = " + (Mathf.Round(pupilDiameterL * 10) / 10).ToString();
                //EyeLStatus.text = "Theta L = " + thetaL.ToString() + "\tPhi L = " + Mathf.Round((float)((phiL - prevPhiL) / (time - prevTimeL))).ToString() + "\tPupil L = " + (Mathf.Round(pupilDiameterL * 10) / 10).ToString();

                prevTimeL = time;
                prevThetaL = thetaL;
                prevPhiL = phiL;
            }

            if (pupilData.EyeIdx == 0 && !(pupilData.Circle.Theta == 0 || pupilData.Circle.Phi == 0 || pupilData.Diameter3d == 0))
            {
                double time = timeSync.ConvertToUnityTime(pupilData.PupilTimestamp);
                thetaR = Mathf.Rad2Deg * pupilData.Circle.Theta;
                phiR = Mathf.Rad2Deg * pupilData.Circle.Phi;
                pupilDiameterR = pupilData.Diameter3d;

                valueListThetaR.Add(thetaR);
                valueListPhiR.Add(phiR);
                valueListPupilR.Add(pupilDiameterR);

                EyeRStatus.text = "Theta R = " + Mathf.Round((float) ((thetaR - prevThetaR) / (time - prevTimeR))).ToString() + "\tPhi R = " + Mathf.Round((float) ((phiR - prevPhiR) / (time - prevTimeR))).ToString() + "\tPupil R = " + (Mathf.Round(pupilDiameterR * 10) / 10).ToString();

                prevTimeR = time;
                prevThetaR = thetaR;
                prevPhiR = phiR;
            }
        }
    }
}