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

        private PupilListener listener;

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

        double prevTime = 0;

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
                phiL = Mathf.Rad2Deg * pupilData.Circle.Phi;
                pupilDiameterL = pupilData.Diameter3d;

                EyeLStatus.text = "Theta L = " + ((thetaL - prevThetaL) / (time - prevTime)).ToString() + "\tPhi L = " + ((phiL - prevPhiL) / (time - prevTime)).ToString() + "\tPupil L = " + pupilDiameterL.ToString();

                prevTime = time;
                prevThetaL = thetaL;
                prevPhiL = phiL;
            }

            if (pupilData.EyeIdx == 0 && !(pupilData.Circle.Theta == 0 || pupilData.Circle.Phi == 0 || pupilData.Diameter3d == 0))
            {
                thetaR = Mathf.Rad2Deg * pupilData.Circle.Theta;
                phiR = Mathf.Rad2Deg * pupilData.Circle.Phi;
                pupilDiameterR = pupilData.Diameter3d;

                //EyeRStatus.text = "Theta = " + thetaR + "\nPhi = " + phiR + "\nDiameter = " + pupilDiameterR + "\nTime Stamp = " + timestamp;
            }
        }
    }
}