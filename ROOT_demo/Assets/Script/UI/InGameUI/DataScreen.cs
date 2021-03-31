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
        public LCDRow CurrentMoneyLcdRow;
        public LCDRow DeltaMoneyLcdRow;
        public LCDRow TimeLcdRow;

        public MeshRenderer CurrentMoneyIcon;
        public MeshRenderer DeltaMoneyIcon;
        public MeshRenderer TimeIcon;

        void Awake()
        {
            SetAlertLevel(1.0f, RowEnum.CurrentMoney);
            SetAlertLevel(1.0f, RowEnum.DeltaMoney);
            SetAlertLevel(1.0f, RowEnum.Time);
        }

        /*public void AnimateSetLcd(float number, RowEnum row)
        {
            SetLcd(Mathf.FloorToInt(number), row);
        }*/

        public void SetLcd(float number,RowEnum row)
        {
            SetLcd(Mathf.FloorToInt(number), row);
        }

        public void SetLcd(int number, RowEnum row)
        {
            switch (row)
            {
                case RowEnum.CurrentMoney:
                    CurrentMoneyLcdRow.SetAniNumber(number);
                    //CurrentMoneyLcdRow.SetNumber(number);
                    break;
                case RowEnum.DeltaMoney:
                    DeltaMoneyLcdRow.SetAniNumber(number);
                    //DeltaMoneyLcdRow.SetNumber(number);
                    break;
                case RowEnum.Time:
                    TimeLcdRow.SetNumber(number);//一直都是一步，就不用了。
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