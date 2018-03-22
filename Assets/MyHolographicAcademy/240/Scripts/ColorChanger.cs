using HoloToolkit.Unity.InputModule;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Education.FeelPhysics.MyHolographicAcademy
{
    public class ColorChanger : MonoBehaviour, IInputClickHandler
    {
        #region Private Valuables

        private Material material;

        private Color colorNow;

        #endregion

        #region MonoBehaviour CallBacks

        private void Awake()
        {
            material = this.gameObject.GetComponent<Renderer>().material;
            this.ChangeColor(Color.blue);
        }

        #endregion

        #region Public Methods

        public void OnInputClicked(InputClickedEventData eventData)
        {
            DebugLog.Instance.Log += "OnInputClicked\n";
            if (colorNow == Color.red)
            {
                ChangeColor(Color.green);
            }
            else if (colorNow == Color.green)
            {
                ChangeColor(Color.blue);
            }
            else if (colorNow == Color.blue)
            {
                ChangeColor(Color.yellow);
            }
            else if (colorNow == Color.yellow)
            {
                ChangeColor(Color.red);
            }
            else
            {
                throw new Exception("colorNow がおかしいです");
            }
        }

        #endregion
        #region Private Methods

        private void ChangeColor(Color color)
        {
            this.colorNow = color;
            this.material.SetColor("_Color", colorNow);
        }

        #endregion

    }
}