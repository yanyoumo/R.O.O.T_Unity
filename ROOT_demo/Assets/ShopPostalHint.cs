using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using UnityEngine;

namespace ROOT
{
    public partial class HintMaster : MonoBehaviour
    {
        public ShopPostalHint shopPostalHint;
    }

    public class ShopPostalHint : MonoBehaviour
    {
        //TODO Yuxuan 在这个脚本所在Prefab（名为ShopPostalHint）上面添加RigidBody和Collider，为触摸屏适配做准备。
        //**请依次完成任务。（难度依次提高）
        //1、如何添加RigidBody和Collider参考名为TutorialCheckList的Prefab。
        //2、并且在Collider所在GameObject上面新建并添加名为“AdvShopPanel”的Tag。
        //3、在ShopPostalHint Prefab中所有AdvShop_Text(及_2)添加i2 Localizer组件。
        //4、在脚本中Start部分，根据是否使用触摸屏切换对应Localizer的Term
        //   4a:AdvShop_Text对应为：AdvShop_KM和AdvShop_Touch
        //   4b:AdvShop_Text_2对应为：AdvShop_2_KM和AdvShop_2_Touch

        public Localize AdvShopTextLocalizer;
        public Localize AdvShopTextTwoLocalizer;
        public LocalizationParamsManager LocalizerMessageParam;
        public int PostalPrice
        {
            set => LocalizerMessageParam.SetParameterValue("VALUE", value + "");
        }

        void Start()
        {
            //
            /*if (StartGameMgr.UseTouchScreen)
            {
                AdvShopTextLocalizer.SetTerm(ScriptTerms.AdvShop_Touch);
                AdvShopTextTwoLocalizer.SetTerm(ScriptTerms.AdvShop_2_Touch);
            }
            else
            {
                AdvShopTextLocalizer.SetTerm(ScriptTerms.AdvShop_KM);
                AdvShopTextTwoLocalizer.SetTerm(ScriptTerms.AdvShop_2_KM);
            }*/
        }
    }
}