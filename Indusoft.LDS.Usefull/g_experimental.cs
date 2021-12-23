using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using Indusoft.LDS.Core;
using Indusoft.LDS.Script.Common;
using Indusoft.LDS.Script.Engine.UI;
using Indusoft.LDS.Data.Integration;
using Indusoft.LDS.Data.Integration.Script;
using Indusoft.LDS.IMath;
using Indusoft.LDS.Client.Samples.Script;
using Indusoft.LDS.Client.Samples.Script.Technique;
using Indusoft.LDS.Services.Contracts.Repository;
using Indusoft.LDS.Format.Common.Enums;
using Indusoft.LDS.Techniques.Common;
using Indusoft.LDS.Services.Contracts.Repository.DataServiceFactory;

namespace Indusoft.LDS.Usefull
{
    public static partial class g
    {

        /// <summary>
        /// Записать границы определения показателей из расчета. Префиксы AnalogTechTest.Prefix по умолчанию: LessOrEqual, None, MoreOrEqual
        /// </summary>
        /// <param name="Low">Нижнаяя граница</param>
        /// <param name="LowDigits">Количество знаков после запятой для нижней границы</param>
        /// <param name="High">Верхняя граница</param>
        /// <param name="HighDigits">Количество знаков после запятой для верхней границы</param>
        /// <param name="techTests">Показатели, для которых проверяется граница</param>
        private static void WriteBounds(double Low, byte LowDigits, double High, byte HighDigits, params AnalogTechTest[] techTests)
        {
            string lowFormat = LowDigits == 0 ? "" : ".";
            for (int i = 0; i < LowDigits; i++) { lowFormat += "0"; }

            string highFormat = HighDigits == 0 ? "" : ".";
            for (int i = 0; i < HighDigits; i++) { highFormat += "0"; }

            foreach (AnalogTechTest techTest in techTests)
            {
                if (techTest.Exists)
                {
                    if (techTest.Value < Low)
                    {
                        techTest.SetLess();
                        techTest.FValue = String.Format("менее {0}", Low.ToString(lowFormat));
                    }
                    if (High < techTest.Value)
                    {
                        techTest.SetMore();
                        techTest.FValue = String.Format("более {0}", High.ToString(highFormat));
                    }
                }
            }
        }

        /// <summary>
        /// Записать границы определения показателей из расчета.
        /// </summary>
        /// <param name="LowPrefix"></param>
        /// <param name="Low">Нижнаяя граница</param>
        /// <param name="LowDigits">Количество знаков после запятой для нижней границы</param>
        /// <param name="HighPrefix"></param>
        /// <param name="High">Верхняя граница</param>
        /// <param name="HighDigits">Количество знаков после запятой для верхней границы</param>
        /// <param name="techTests">Показатели, для которых проверяется граница</param>
        private static void WriteBounds(ValuePrefix2 LowPrefix, double Low, int LowDigits, ValuePrefix2 HighPrefix, double High, int HighDigits, params AnalogTechTest[] techTests)
        {
            string lowFormat = LowDigits == 0 ? "" : ".";
            for (int i = 0; i < LowDigits; i++) { lowFormat += "0"; }

            string highFormat = HighDigits == 0 ? "" : ".";
            for (int i = 0; i < HighDigits; i++) { highFormat += "0"; }

            foreach (AnalogTechTest techTest in techTests)
            {
                if (techTest.Exists)
                {
                    if (techTest.Value < Low)
                    {
                        //Вместо простого выставления префикса или SetPrefix(префикс) используются именные методы на всякий случай, если в них добавят какую-нибудь логику в будущем
                        switch (LowPrefix)
                        {
                            case (ValuePrefix2.Less):
                                techTest.SetLess(); break;
                            case ValuePrefix2.LessOrEqual:
                                techTest.SetLessOrEqual(); break;
                            default:
                                throw new Exception("Неверный формат префикса");
                        }
                        techTest.FValue = String.Format("менее {0}", Low.ToString(lowFormat));
                    }

                    if (High < techTest.Value)
                    {
                        //Вместо простого выставления префикса или SetPrefix(префикс) используются именные методы на всякий случай, если в них добавят какую-нибудь логику в будущем
                        switch (HighPrefix)
                        {
                            case (ValuePrefix2.More):
                                techTest.SetMore(); break;
                            case ValuePrefix2.MoreOrEqual:
                                techTest.SetMoreOrEqual(); break;
                            default:
                                throw new Exception("Неверный формат префикса");
                        }
                        techTest.FValue = String.Format("более {0}", High.ToString(highFormat));
                    }
                }
            }
        }
    }
}
