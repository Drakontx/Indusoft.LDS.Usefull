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

namespace Indusoft.LDS.Usefull
{
    public static partial class g
    {
        /// <summary>
        /// Проверка на наличие значения определения
        /// </summary>
        /// <param name="ATestParam">Показатель расчета</param>
        /// <param name="AIndex">Индекс опеределения</param>
        /// <returns></returns>
        private static bool mExistsAndNotNull(int AIndex, AnalogTechTest ATestParam)
        {
            if ((ATestParam.Exists) && (AIndex < ATestParam.Measures.Count))
            {
                if (!ATestParam.Measures[AIndex].IsNull)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Возвращает значение показателя
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        private static double v(AnalogTechTest test)
        {
            return test.Value;
        }

        /// <summary>
        /// Возвращает значение показателя
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        private static string v(DigitTechTest test)
        {
            return test.Value;
        }

        /// <summary>
        /// Проверка на существование определения
        /// </summary>
        /// <param name="ATestParam"></param>
        /// <param name="AIndex"></param>
        /// <returns></returns>
        private static bool mExists(int AIndex, AnalogTechTest ATestParam)
        {
            if ((ATestParam.Exists) && (AIndex < ATestParam.Measures.Count))
            {
                return true;
            }
            return false;
        }

        #region GradGraph
        private static double[] Dpublic = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private static double[] Dpublic2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// Расчет ГГ
        /// </summary>
        /// <param name="Product">Имя продукта</param>
        /// <param name="Analit">Показатель аналит (обычно масса)</param>
        /// <param name="AnalitSignal">Показатель сигнала (обычно оптическая плотность)</param>
        private static void GradGraph(string Product, AnalogTechTest Analit, AnalogTechTest AnalitSignal)
        {
            try
            {
                double D, C;
                //Проверка наличия показателя для вывода результата и проверка продукта: расчет не должен работать при построении ГГ и контроле стабильности (ГГ не утвержден)
                if (Analit.Exists && AnalitSignal.Exists && !(Product.Contains("градуировочно") || Product.Contains("ГГ"))) //свои данные
                {
                    for (int i = 0; i < Analit.Measures.Count; i++)
                    {
                        //Проверка на пустоту оптической  плотности
                        if (Exists(AnalitSignal, i, out D) && mExists(Analit, i))
                        {
                            // Проверка на наличие графика
                            if (Analit.CheckGraphExists(Analit, AnalitSignal, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps))
                            {
                                //Рассчет значения по ГГ
                                C = Analit.CalcAnalitValue(AnalitSignal, D);
                                //Следующее условие добавлено для логического исключения невозможных отрицательных значений показателей массы и м.к.
                                if (C < 0) { C = 0; } //Если значение меньше нуля, записать просто 0

                                //Присвоение D, для проверки его на изменение
                                Dpublic[i] = AnalitSignal.Measures[i].Value;
                                Analit.Measures[i].Value = C;  //запись значения аналита в измерение показателя
                            }
                            //проверка на пустоту расчетного и изменилось ли D
                            else if ((Dpublic[i] != AnalitSignal.Measures[i].Value))
                            {
                                //Присвоение D, для проверки его на изменение
                                Dpublic[i] = AnalitSignal.Measures[i].Value;
                                try
                                {
                                    Guid? clbrGraphUid = Analit.GetAnalitGraphUid(Analit, AnalitSignal, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);

                                    MessageBox.Show(
                                        "Рассчитанное значение вышло за диапазон определения градуировочного графика.\n\nДля дальнейшей работы необходимо либо расширить диапазон графика, либо ввести следующий показатель вручную:\n[" + Analit.Name + "]",
                                        Analit.Tech.Name,
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                }
                                catch
                                {
                                    MessageBox.Show(
                                        "В системе отсутствует подходящий градуировочный график.\n\nДля дальнейшей работы необходимо либо сформировать в системе подходящий градуировочный график, либо ввести следующий показатель вручную:\n[" + Analit.Name + "]",
                                        Analit.Tech.Name,
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        
        private static void GradGrap(AnalogTechTest Analit, AnalogTechTest AnalitSignal)
        {
            string Product = "";

            double D, C;
            //Для параллельных определений: если ошибка, то значение станет true, и сообщение появится у пользователя один раз за все определения
            bool gr_error = false;

            //Проверка наличия показателя для вывода результата и проверка продукта: расчет не должен работать при построении ГГ и контроле стабильности (ГГ не утвержден)
            if (Analit.Exists && !Product.Contains("градуировочно")) //свои данные
                for (int i = 0; i < Analit.Measures.Count; i++)
                {
                    //Проверка на пустоту оптической  плотности
                    if (g.Exists(AnalitSignal, i))
                    {
                        //Проверка на наличие графика
                        if (Analit.CheckGraphExists(Analit, AnalitSignal, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps))
                        {
                            D = AnalitSignal.Measures[i].Value;
                            //Рассчет значения по ГГ
                            C = Analit.CalcAnalitValue(AnalitSignal, D);
                            //Присвоение D, для проверки его на изменение
                            Dpublic[i] = AnalitSignal.Measures[i].Value;
                            if (g.mExists(Analit, i)) { Analit.Measures[i].Value = C; }
                        }
                        //Проверка на пустоту расчетного и изменилось ли D
                        //else if ((!g.Exists(Analit,i) || Dpublic[i]!=AnalitSignal.Measures[i].Value) && gr_error == false)
                        //else if ((Dpublic[i]!=AnalitSignal.Measures[i].Value) && gr_error == false)
                        else if ((Dpublic[i] != AnalitSignal.Measures[i].Value))
                        {
                            //Присвоение D, для проверки его на изменение
                            Dpublic[i] = AnalitSignal.Measures[i].Value;
                            try
                            {
                                Guid? clbrGraphUid = Analit.GetAnalitGraphUid(Analit, AnalitSignal, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                                //if (gr_error == false) 
                                {
                                    //gr_error = true; //для отсуствия вывода повторного сообщения для кажого опредления
                                    MessageBox.Show(
                                    "Рассчитанное значение вышло за диапазон определения градуировочного графика.\n\nДля дальнейшей работы необходимо либо расширить диапазон графика, либо ввести следующий показатель вручную:\n[" + Analit.Name + "]",
                                    Analit.Tech.Name,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                }
                            }
                            catch
                            {
                                //if (gr_error == false) 
                                {
                                    //gr_error = true; //для отсуствия вывода повторного сообщения для кажого определения
                                    MessageBox.Show(
                                    "В системе отсутствует подходящий градуировочный график.\n\nДля дальнейшей работы необходимо либо сформировать в системе подходящий градуировочный график, либо ввести следующий показатель вручную:\n[" + Analit.Name + "]",
                                    Analit.Tech.Name,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                }
                            }
                        }
                    }
                }
        }

        /// <summary>
        /// Расчет ГГ GradGraph_V2 (Cizm - для расчета, D - для расчета, Cizm - для поиска UID, D - для поиска UID)
        /// </summary>
        /// <param name="Product">Имя продукта</param>
        /// <param name="Analit"></param>
        /// <param name="AnalitSignal"></param>
        /// <param name="Analit_Uid"></param>
        /// <param name="AnalitSignal_Uid"></param>
        public static void GradGraph(string Product, AnalogTechTest Analit, AnalogTechTest AnalitSignal, AnalogTechTest Analit_Uid, AnalogTechTest AnalitSignal_Uid)
        {
            double D, C;

            //Проверка наличия показателя для вывода результата и проверка продукта: расчет не должен работать при построении ГГ и контроле стабильности (ГГ не утвержден)
            if (Analit.Exists && !Product.Contains("градуировочно")) //свои данные
            {
                for (int i = 0; i < Analit.Measures.Count; i++)
                {
                    //Поиск Uid нужного графика
                    if (Exists(Analit_Uid, i) && Exists(AnalitSignal_Uid, i))
                    {
                        Guid? clbrGraphUid = Analit_Uid.GetAnalitGraphUid(Analit_Uid, AnalitSignal_Uid, AnalitSignal_Uid.Measures[i].Value, null);

                        //Проверяем, есть ли график
                        if (clbrGraphUid != null)
                        {
                            //Проверка на пустоту оптической плотности и проверка на наличие Uid графика
                            if (Exists(AnalitSignal, i))
                            {
                                try
                                {
                                    if (Dpublic2[i] != AnalitSignal.Measures[i].Value)
                                    {
                                        //Присвоение D, для проверки его на изменение
                                        Dpublic2[i] = AnalitSignal.Measures[i].Value;
                                        C = Analit.CalcAnalitValueByClbrGraphUid(Analit, (Guid)clbrGraphUid, AnalitSignal.Measures[i].Value, null);
                                        if (mExists(Analit, i)) Analit.Measures[i].Value = C;
                                    }
                                }
                                catch
                                {
                                    MessageBox.Show(
                                    "В системе отсутствует подходящий градуировочный график.\n\nДля дальнейшей работы необходимо либо сформировать в системе подходящий градуировочный график, либо ввести следующий показатель вручную:\n[" + Analit.Name + "]",
                                    Analit.Tech.Name,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                            "В системе отсутствует подходящий градуировочный график.\n\nДля дальнейшей работы необходимо либо сформировать в системе подходящий градуировочный график, либо ввести следующий показатель вручную:\n[" + Analit.Name + "]",
                            Analit.Tech.Name,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
