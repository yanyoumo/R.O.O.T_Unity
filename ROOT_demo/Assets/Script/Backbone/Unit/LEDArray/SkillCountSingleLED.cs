 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 namespace ROOT
 {
     public class SkillCountSingleLED : SingleLED
     {
         public SpriteRenderer LED;
         
         public override void TurnOn()
         {
             LED.color = TurnOnColor;
         }

         public override void TurnOff()
         {
             LED.color=_turnOffColor;
         }
     }
 }