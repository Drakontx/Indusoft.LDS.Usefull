using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Indusoft.LDS.Script.Common;
using Indusoft.LDS.Script.Engine.UI;
using Indusoft.LDS.Data.Integration;
using Indusoft.LDS.Data.Integration.Script;
using Indusoft.LDS.IMath;
using Indusoft.LDS.Client.Samples.Script;
using Indusoft.LDS.Client.Samples.Script.Technique;
using Indusoft.LDS.Services.Contracts.Repository;
using Indusoft.LDS.Format.Common.Enums;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Indusoft.LDS.Usefull
{
    /// <summary>
    /// Работа с градуировочными графиками
    /// </summary>
    public class GradGraphClass
    {
        string Product;
        string GGProductPattern = "градуировочно";
        Dictionary<AnalogTechTest, List<double>> Dpublics = new Dictionary<AnalogTechTest, List<double>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="product">Продукт образца</param>
        /// <param name="AnalitSignal_Tests">Показатели, по которым из ГГ определяются искомые значения</param>
        public GradGraphClass(string product, params AnalogTechTest[] AnalitSignal_Tests)
        {
            Product = product;
            InitializeDpublic(AnalitSignal_Tests);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="product">Продукт образца</param>
        /// <param name="GGProductPattern">Маска наименования продукта для построения ГГ (Устанавливаемое в типах возможностей ГГ)</param>
        /// <param name="AnalitSignal_Tests">Показатели, по которым из ГГ определяются искомые значения</param>
        public GradGraphClass(string product, string GGProductPattern, params AnalogTechTest[] AnalitSignal_Tests):this(product, AnalitSignal_Tests)
        {
            SetGGProductPattern(GGProductPattern);
        }

        /// <summary>
        /// Установить маску наименования продукта для построения ГГ (Устанавливаемое в типах возможностей ГГ)
        /// </summary>
        /// <param name="pattern"></param>
        public void SetGGProductPattern(string pattern)
        {
            GGProductPattern = pattern;
        }

        /// <summary>
        /// Создание Dpublic для перечисленных показателей
        /// </summary>
        /// <param name="Opt_plotnosti">Показатели</param>
        void InitializeDpublic(params AnalogTechTest[] Opt_plotnosti)
        {
            foreach (AnalogTechTest analogTechTest in Opt_plotnosti)
            {
                if (analogTechTest.Exists)
                {
                    if (!Dpublics.ContainsKey(analogTechTest))
                    {
                        Dpublics.Add(analogTechTest, ZeroList(analogTechTest.Measures.Count));
                    }
                }
            }
        }

        /// <summary>
        /// Генерация нулевого листа
        /// </summary>
        /// <param name="amount">Число элементов</param>
        /// <returns></returns>
        List<double> ZeroList(int amount)
        {
            List<double> result = new List<double>();
            for (int i = 0; i < amount; i++)
            {
                result.Add(0);
            }
            return result;
        }

        /// <summary>
        /// Синхронизация количества элементов Dpublic из измерений показателя
        /// </summary>
        /// <param name="AnalitSignal">Показатель с измерениями</param>
        void CheckDpublicMeasures(AnalogTechTest AnalitSignal)
        {
            List<double> Dpublic;
            if (Dpublics.TryGetValue(AnalitSignal, out Dpublic))
            {
                //Если количество измерений показателя не сходится с его Dpublic и в Dpublic что-то есть
                if (AnalitSignal.Measures.Count != Dpublic.Count && Dpublic.Count > 0)
                {
                    int difference = AnalitSignal.Measures.Count - Dpublic.Count;
                    if (difference < 0)
                    {
                        //Убираем лишние
                        Dpublic.RemoveRange(Dpublic.Count + difference, Math.Abs(difference));
                    }
                    else
                    {
                        //Добавляем недостающие
                        Dpublic.InsertRange(Dpublic.Count, ZeroList(Math.Abs(difference)));
                    }
                }
            }
            else
            {
                //Если такого показателя в словаре нет - инициализируем
                InitializeDpublic(AnalitSignal);
            }
        }

        /// <summary>
        /// Встроенная синхронизация количества измерений ГГ для упрощения записи. Условие содержания в Product шаблона GGProductPattern. Ссылается на g.CheckMeasures
        /// </summary>
        /// <param name="SyncTechTests"></param>
        public void CheckGGMeasures(params AnalogTechTest[] SyncTechTests)
        {
            if (Product.Contains(GGProductPattern))
            {
                g.CheckMeasures(SyncTechTests);
            }
        }

        /// <summary>
        /// Расчет значений по градуировочному графику
        /// </summary>
        /// <param name="Analit">показатель Аналит</param>
        /// <param name="AnalitSignal">показатель Аналитический сигнал</param>
        /// <param name="GGNoEachMeasureError">Не показывать уведомления для каждого измерения (по умолчанию = да)</param>
        public void GradGraph(AnalogTechTest Analit, AnalogTechTest AnalitSignal, bool GGNoEachMeasureError = true)
        {
            bool clbrGraphExists = false;
            Guid? clbrGraphUid = null;

            //Проверка наличия показателя для вывода результата и проверка продукта: расчет не должен работать при построении ГГ и контроле стабильности (ГГ не утвержден)
            if (Analit.Exists && AnalitSignal.Exists && !Product.Contains(GGProductPattern))
            {
                //Синхронизация элементов Dpublic из измерений показателя
                CheckDpublicMeasures(AnalitSignal);
                List<double> Dpublic = Dpublics[AnalitSignal];

                for (int i = 0; i < Analit.Measures.Count; i++)
                {
                    //Проверка на пустоту оптической  плотности
                    if (g.Exists(AnalitSignal, i))
                    {
                        //Проверка на наличие графика
                        clbrGraphExists = Analit.CheckGraphExists(Analit, AnalitSignal, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                        if (clbrGraphExists)
                        {
                            //Получаем его Guid, если есть
                            clbrGraphUid = Analit.GetAnalitGraphUid(Analit, AnalitSignal, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                            break;
                        }
                    }
                }
                for (int i = 0; i < Analit.Measures.Count; i++)
                {
                    //Пересчитываем все, а не только то, что изменилось
                    //Подходящий график существует
                    if (clbrGraphExists)
                    {
                        //На всякий случай
                        if (clbrGraphUid != null)
                        {
                            //Рассчет значения по ГГ
                            double C = Analit.CalcAnalitValueByClbrGraphUid(AnalitSignal, (Guid)clbrGraphUid, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                            if (g.mExists(Analit, i)) { Analit.Measures[i].Value = C; }
                        }
                    }
                    //Подходящего нет и значение сигнала изменилось
                    else if (Dpublic[i] != AnalitSignal.Measures[i].Value)
                    {
                        try
                        {
                            //Показываем возможные варианты
                            clbrGraphUid = Analit.GetAnalitGraphUid(Analit, AnalitSignal, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                            MessageBox.Show(
                            "Рассчитанное значение вышло за диапазон определения градуировочного графика.\n\nДля дальнейшей работы необходимо либо расширить диапазон графика, либо ввести следующий показатель вручную:\n[" + Analit.Name + "]",
                            Analit.Tech.Name,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        }
                        catch
                        {
                            //Вариантов ВООБЩЕ нет
                            MessageBox.Show(
                            "В системе отсутствует подходящий градуировочный график.\n\nДля дальнейшей работы необходимо либо сформировать в системе подходящий градуировочный график, либо ввести следующий показатель вручную:\n[" + Analit.Name + "]",
                            Analit.Tech.Name,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        }
                        if (GGNoEachMeasureError) break;
                    }
                }
            }
        }

        /// <summary>
        /// Расчет значений по градуировочному графику, построенному по другим показателям
        /// </summary>
        /// <param name="Analit">показатель Аналит</param>
        /// <param name="AnalitSignal">показатель Аналитический сигнал</param>
        /// <param name="Analit_Uid_source">показатель Аналит, по которому построен ГГ</param>
        /// <param name="AnalitSignal_Uid_source">показатель Аналит.сигнал, по которому построен ГГ</param>
        /// <param name="GGNoEachMeasureError">Не показывать уведомления для каждого измерения (по умолчанию = да)</param>
        public void GradGraph(AnalogTechTest Analit, AnalogTechTest AnalitSignal, AnalogTechTest Analit_Uid_source, AnalogTechTest AnalitSignal_Uid_source, bool GGNoEachMeasureError = true)
        {
            bool clbrGraphExists = false;
            Guid? clbrGraphUid = null;

            //Проверка наличия показателя для вывода результата и проверка продукта: расчет не должен работать при построении ГГ и контроле стабильности (ГГ не утвержден)
            if (Analit.Exists && AnalitSignal.Exists && Analit_Uid_source.Exists && AnalitSignal_Uid_source.Exists && !Product.Contains(GGProductPattern))
            {
                CheckDpublicMeasures(AnalitSignal);
                List<double> Dpublic = Dpublics[AnalitSignal];

                for (int i = 0; i < Analit.Measures.Count; i++)
                {
                    //Проверка на пустоту оптической  плотности
                    if (g.Exists(AnalitSignal, i))
                    {
                        //Проверка на наличие графика
                        clbrGraphExists = Analit.CheckGraphExists(Analit_Uid_source, AnalitSignal_Uid_source, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                        if (clbrGraphExists)
                        {
                            clbrGraphUid = Analit.GetAnalitGraphUid(Analit_Uid_source, AnalitSignal_Uid_source, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                            break;
                        }
                    }
                }
                for (int i = 0; i < Analit.Measures.Count; i++)
                {
                    if (clbrGraphExists)
                    {
                        if (clbrGraphUid != null)
                        {
                            //Рассчет значения по ГГ
                            double C = Analit.CalcAnalitValueByClbrGraphUid(AnalitSignal, (Guid)clbrGraphUid, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                            if (g.mExists(Analit, i)) { Analit.Measures[i].Value = C; }
                        }
                    }
                    else if (Dpublic[i] != AnalitSignal.Measures[i].Value)
                    {
                        try
                        {
                            clbrGraphUid = Analit.GetAnalitGraphUid(Analit_Uid_source, AnalitSignal_Uid_source, AnalitSignal.Measures[i].Value, AnalitSignal.Eqps);
                            MessageBox.Show(
                            "Рассчитанное значение вышло за диапазон определения градуировочного графика.\n\nДля дальнейшей работы необходимо либо расширить диапазон графика, либо ввести следующий показатель вручную:\n\n[" + Analit.Name + "]",
                            Analit.Tech.Name,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        }
                        catch
                        {
                            MessageBox.Show(
                            "В системе отсутствует подходящий градуировочный график.\n\nДля дальнейшей работы необходимо либо сформировать в системе подходящий градуировочный график, либо ввести следующий показатель вручную:\n\n[" + Analit.Name + "]",
                            Analit.Tech.Name,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        }
                        if (GGNoEachMeasureError) break;
                    }
                }
            }
        }
    }
}
