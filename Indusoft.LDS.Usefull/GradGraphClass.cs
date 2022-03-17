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
    /// <remarks>
    ///   <example>
    ///     Пример использования. Условия: Опт.плотность является разностью измеренной и опт.плотности холостого испытания D = Dp - Dx. Концентрация С находится по графику C = f(D).
    ///     <code>
    ///       Dх - опт.плотность холостой пробы
    ///       Dр - опт.плотность рабочей пробы
    ///       D - опт.плотность (используемая в построении графика)
    ///       C - концентрация, найденая по ГГ
    ///       
    ///       //Создается класс ГГ. Даем ему продукт образца и перечисляем оптические плотности, по которым в дальнейшем находятся искомые значения. В данном случае - один показатель D
    ///       GradGraphClass gg = new GradGraphClass(Product, D);
    ///       
    ///       //При построении ГГ нужно синхронизовать измерения у D и Dp. (В новых версиях в ГГ есть галочка синхронизации в группе определений как стандартный функционал)
    ///       gg.CheckGGMeasures(D, Dp);
    ///       
    ///       //Вызываем градуировочный график
    ///       gg.GradGraph(C, D);
    ///     </code>
    ///   </example>
    /// </remarks>
    public class GradGraphClass
    {
        /// <summary>
        /// Продукт образца
        /// </summary>
        public string Product { get; private set; }

        /// <summary>
        /// Шаблон продукта для заданий построения ГГ (Устанавливаемое в типах возможностей ГГ)
        /// </summary>
        public string GGProductPattern { get; private set; }

        private Dictionary<AnalogTechTest, List<double>> Dpublics = new Dictionary<AnalogTechTest, List<double>>();
        private bool Initialized = false;

        /// <summary>
        /// Инициализация класса. Для упрощения записи в расчетах
        /// </summary>
        /// <param name="product">Продукт образца</param>
        /// <param name="GGProductPattern">Маска наименования продукта для построения ГГ (Устанавливаемое в типах возможностей ГГ)</param>
        /// <param name="AnalitSignal_Tests">Показатели, по которым из ГГ определяются искомые значения</param>
        /// <remarks>
        ///   <example>
        ///     Пример использования. Условия: Опт.плотность является разностью измеренной и опт.плотности холостого испытания D = Dp - Dx. Концентрация С находится по графику C = f(D).
        ///     <code>
        ///       До точки начала создается экземпляр класса ГГ:
        ///       ...
        ///       GradGraphClass gg = new GradGraphClass();
        ///       [ScriptEntryPoint]
        ///       ...
        ///       Dх - опт.плотность холостой пробы
        ///       Dр - опт.плотность рабочей пробы
        ///       D - опт.плотность (используемая в построении графика)
        ///       C - концентрация, найденая по ГГ
        ///       
        ///       В методе Run()
        ///       //Инициализируем ГГ. Даем ему продукт образца, указываем продукт при построении ГГ, если он отличается от типового "градуировочно" и перечисляем оптические плотности, по которым в дальнейшем находятся искомые значения. В данном случае - один показатель D.
        ///       gg.Init(Product, "Построение градуировочного", D);
        ///       
        ///       //При построении ГГ нужно синхронизовать измерения у D и Dp. (В новых версиях в ГГ есть галочка синхронизации в группе определений как стандартный функционал)
        ///       gg.CheckGGMeasures(D, Dp);
        ///       
        ///       //Вызываем градуировочный график
        ///       gg.GradGraph(C, D);
        ///     </code>
        ///   </example>
        /// </remarks>
        /// <returns></returns>
        public void Init(string product, string GGProductPattern, params AnalogTechTest[] AnalitSignal_Tests)
        {
            if (!Initialized)
            {
                SetGGProductPattern(GGProductPattern);
                SetProduct(product);
                InitializeDpublic(AnalitSignal_Tests);
                Initialized = true;
            }
        }

        /// <summary>
        /// Инициализация класса. Для упрощения записи в расчетах
        /// </summary>
        /// <param name="product">Продукт образца</param>
        /// <param name="AnalitSignal_Tests">Показатели, по которым из ГГ определяются искомые значения</param>
        /// <remarks>
        ///   <example>
        ///     Пример использования. Условия: Опт.плотность является разностью измеренной и опт.плотности холостого испытания D = Dp - Dx. Концентрация С находится по графику C = f(D).
        ///     <code>
        ///       До точки начала создается экземпляр класса ГГ:
        ///       ...
        ///       GradGraphClass gg = new GradGraphClass();
        ///       [ScriptEntryPoint]
        ///       ...
        ///       Dх - опт.плотность холостой пробы
        ///       Dр - опт.плотность рабочей пробы
        ///       D - опт.плотность (используемая в построении графика)
        ///       C - концентрация, найденая по ГГ
        ///       
        ///       В методе Run()
        ///       //Инициализируем ГГ. Даем ему продукт образца и перечисляем оптические плотности, по которым в дальнейшем находятся искомые значения. В данном случае - один показатель D.
        ///       gg.Init(Product, D);
        ///       
        ///       //При построении ГГ нужно синхронизовать измерения у D и Dp. (В новых версиях в ГГ есть галочка синхронизации в группе определений как стандартный функционал)
        ///       gg.CheckGGMeasures(D, Dp);
        ///       
        ///       //Вызываем градуировочный график
        ///       gg.GradGraph(C, D);
        ///     </code>
        ///   </example>
        /// </remarks>
        /// <returns></returns>
        public void Init(string product, params AnalogTechTest[] AnalitSignal_Tests)
        {
            Init(product, "градуировочно", AnalitSignal_Tests);
        }

        /// <summary>
        /// Создание Dpublic для перечисленных показателей
        /// </summary>
        /// <param name="Opt_plotnosti">Показатели</param>
        private void InitializeDpublic(params AnalogTechTest[] Opt_plotnosti)
        {
            foreach (AnalogTechTest analogTechTest in Opt_plotnosti)
            {
                if (analogTechTest.Exists)
                {
                    if (!Dpublics.ContainsKey(analogTechTest))
                    {
                        Dpublics.Add(analogTechTest, NaNList(analogTechTest.Measures.Count));
                    }
                }
            }
        }

        /// <summary>
        /// Генерация нулевого листа
        /// </summary>
        /// <param name="amount">Число элементов</param>
        /// <returns></returns>
        private List<double> NaNList(int amount)
        {
            List<double> result = new List<double>();
            for (int i = 0; i < amount; i++)
            {
                result.Add(double.NaN);
            }
            return result;
        }

        /// <summary>
        /// Синхронизация количества элементов Dpublic из измерений показателя
        /// </summary>
        /// <param name="AnalitSignal">Показатель с измерениями</param>
        private void CheckDpublicMeasures(AnalogTechTest AnalitSignal)
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
                        Dpublic.InsertRange(Dpublic.Count, NaNList(Math.Abs(difference)));
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
        /// Установить маску наименования продукта для построения ГГ (Устанавливаемое в типах возможностей ГГ)
        /// </summary>
        /// <param name="pattern"></param>
        private void SetGGProductPattern(string pattern)
        {
            GGProductPattern = pattern;
        }

        private void SetProduct(string product)
        {
            Product = product;
        }

        /// <summary>
        /// Встроенная синхронизация количества измерений ГГ для упрощения записи. Условие содержания в Product шаблона GGProductPattern. Ссылается на g.CheckMeasures. Регистр сохраняется.
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
                //Пересчитываем все, а не только то, что изменилось
                for (int i = 0; i < Analit.Measures.Count; i++)
                {
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
                    else if (!AnalitSignal.Measures[i].Value.Equals(Dpublic[i]))
                    {
                        //Присвоение D, для проверки его на изменение
                        Dpublic[i] = AnalitSignal.Measures[i].Value;
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
                    else if (!AnalitSignal.Measures[i].Value.Equals(Dpublic[i]))
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
