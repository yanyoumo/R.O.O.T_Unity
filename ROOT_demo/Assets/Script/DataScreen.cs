using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ROOT
{
    public enum RowEnum{
        CurrentMoney,
        DeltaMoney,
        Time
    }

    public class DataScreen : MonoBehaviour
    {
        public LCDRow CurrentMoneyLCDRow;
        public LCDRow DeltaMoneyLCDRow;
        public LCDRow TimeLCDRow;

        public MeshRenderer CurrentMoneyIcon;
        public MeshRenderer DeltaMoneyIcon;
        public MeshRenderer TimeIcon;

        void Awake()
        {
            SetAlertLevel(1.0f, RowEnum.CurrentMoney);
            SetAlertLevel(1.0f, RowEnum.DeltaMoney);
            SetAlertLevel(1.0f, RowEnum.Time);
        }

        public void SetLCD(float number,RowEnum row)
        {
            SetLCD(Mathf.FloorToInt(number), row);
        }

        public void SetLCD(int number, RowEnum row)
        {
            switch (row)
            {
                case RowEnum.CurrentMoney:
                    CurrentMoneyLCDRow.SetNumber(Mathf.Abs(number), number >= 0);
                    break;
                case RowEnum.DeltaMoney:
                    DeltaMoneyLCDRow.SetNumber(Mathf.Abs(number), number >= 0);
                    break;
                case RowEnum.Time:
                    TimeLCDRow.SetNumber(Mathf.Abs(number), number >= 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(row), row, null);
            }
        }

        public void SetAlertLevel(float lvl, RowEnum row)
        {
            switch (row)
            {
                case RowEnum.CurrentMoney:
                    CurrentMoneyIcon.material.SetFloat("_Shutter",lvl);
                    break;
                case RowEnum.DeltaMoney:
                    DeltaMoneyIcon.material.SetFloat("_Shutter", lvl);
                    break;
                case RowEnum.Time:
                    TimeIcon.material.SetFloat("_Shutter", lvl);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(row), row, null);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}