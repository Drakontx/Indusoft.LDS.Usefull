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
    /// <summary>
    /// Общие стандартные функции
    /// </summary>
    public static partial class g
    {
        #region SQL методы
        private static ICommonDataProvider cdm = null;
        public static void ExecuteSqlNonQuery(IScriptSession session, string cmd)
        {
            try
            {
                if (cdm == null)
                {
                    if (session == null) throw new Exception("Session is null.");
                    cdm = ((IGenericServiceProvider)session).GetService<ICommonDataProvider>();
                    if (cdm == null) throw new Exception("Не удалось получить ICommonDataProvider.");
                }
                string injection = string.Format("SysdatabaseVersion]; {0} --", cmd);
                cdm.GetData(new string[] { injection });
            }
            catch (Exception ex)
            {
                gLogger.WriteError(ex, "(SqlExecute)");
            }
        }

        public static DataSet ExecuteSql(IScriptSession session, string cmd)
        {
            try
            {
                if (cdm == null)
                {
                    if (session == null) throw new Exception("Session is null.");
                    cdm = ((IGenericServiceProvider)session).GetService<ICommonDataProvider>();
                    if (cdm == null) throw new Exception("Не удалось получить ICommonDataProvider.");
                }
                return cdm.GetData(new string[] { cmd });
            }
            catch (Exception ex)
            {
                gLogger.WriteError(ex, "(SqlExecute)");
            }
            return null;
        }

        /// <summary>
        /// Получить содержимое указанных таблиц
        /// </summary>
        /// <param name="session"> Сессия расчета</param>
        /// <param name="table_names">Названия таблиц в базе</param>
        /// <returns>Возвращает результат запроса в виде <see cref="DataSet"/></returns>
        public static DataSet GetSqlData(IScriptSession session, string[] table_names)
        {
            if (cdm == null)
            {
                if (session == null) throw new Exception("Session is null.");
                cdm = ((IGenericServiceProvider)session).GetService<ICommonDataProvider>();
                if (cdm == null) throw new Exception("Не удалось получить ICommonDataProvider.");
            }
            return cdm.GetData(table_names);
        }
        #endregion SQL методы

        #region методы Exists
        /// <summary>
        /// Проверяет все ли показатели существуют
        /// </summary>
        /// <param name="TechTests"></param>
        /// <returns></returns>
        public static bool vExists(params Indusoft.LDS.Client.Samples.Script.TechTest[] TechTests)
        {
            for (int i = 0, e = TechTests.Length; i < e; i++)
                if (!TechTests[i].Exists)
                    return false;
            return true;
        }

        /// <summary>
        /// Проверяет существование показателей перечисленный в TechTests и наличие конечного значения.
        /// </summary>
        /// <param name="TechTests"></param>
        /// <returns></returns>
        public static bool vExistsAndNotNull(params Indusoft.LDS.Client.Samples.Script.TechTest[] TechTests)
        {
            for (int i = 0, e = TechTests.Length; i < e; i++)
            {
                if (!TechTests[i].Exists) return false;
                if (TechTests[i].IsNull) return false;
            }
            return true;
        }
        
        /// <summary>
        /// Проверка на существования определения
        /// </summary>
        /// <param name="ATestParam">Показатель расчета</param>
        /// <param name="AIndex">Индекс определения</param>
        /// <returns></returns>
        public static bool mExists(AnalogTechTest ATestParam, int AIndex)
        {
            if ((ATestParam.Exists) && (AIndex < ATestParam.Measures.Count))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка на существование определения
        /// </summary>
        /// <param name="ATestParam"></param>
        /// <param name="AIndex"></param>
        /// <returns></returns>
        public static bool mExists(int AIndex, AnalogTechTest ATestParam)
        {
            return mExists(ATestParam, AIndex);
        }

        /// <summary>
        /// Проверяет есть ли определение с index в показателях TechTests
        /// </summary>
        /// <param name="index"></param>
        /// <param name="TechTests"></param>
        /// <returns></returns>
        public static bool mExists(int index, params AnalogTechTest[] TechTests)
        {
            for (int i = 0, e = TechTests.Length; i < e; i++)
            {
                if (!mExists(TechTests[i], index))
                {
                    return false;
                }
                //if (!TechTests[i].Exists) return false;
                //if (index >= TechTests[i].Measures.Count) return false;
            }
            return true;
        }

        /// <summary>
        /// Проверка на наличие значения определения
        /// </summary>
        /// <param name="ATestParam">Показатель расчета</param>
        /// <param name="AIndex">Индекс опеределения</param>
        /// <returns></returns>
        public static bool Exists(AnalogTechTest ATestParam, int AIndex)
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
        /// Проверка на наличие значения определения
        /// </summary>
        /// <param name="DTestParam">Показатель расчета</param>
        /// <param name="AIndex">Индекс опеределения</param>
        /// <returns></returns>
        public static bool Exists(DigitTechTest DTestParam, int AIndex)
        {
            if ((DTestParam.Exists) && (AIndex < DTestParam.Measures.Count))
            {
                if (!DTestParam.Measures[AIndex].IsNull)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///  Проверка на наличие значения определения
        /// </summary>
        /// <param name="ATestParam">Показатель расчета</param>
        /// <param name="AIndex">Индекс опеределения</param>
        /// <param name="val">Значение измерения</param>
        /// <returns></returns>
        public static bool Exists(AnalogTechTest ATestParam, int AIndex, out double val)
        {
            if (Exists(ATestParam, AIndex))
            {
                val = ATestParam.Measures[AIndex].Value;
                return true;
            }
            val = double.NaN;
            return false;
        }

        /// <summary>
        /// Проверяет на наличие значения в определениях показателей TechTests
        /// </summary>
        /// <param name="index">Индекс определения</param>
        /// <param name="TechTests">Показатели</param>
        /// <returns>Возвращает true если у всех показателей есть определение index</returns>
        public static bool Exists(int index, params AnalogTechTest[] TechTests)
        {
            for (int i = 0, e = TechTests.Length; i < e; i++)
            {
                if (!Exists(TechTests[i], index)) return false;
                //if (!TechTests[i].Exists) return false;
                //if (index >= TechTests[i].Measures.Count) return false;
                //if (TechTests[i].Measures[index].IsNull) return false;
            }
            return true;
        }

        /// <summary>
        /// Проверяет на наличие значения в определениях показателей TechTests
        /// </summary>
        /// <param name="index">Индекс определения</param>
        /// <param name="TechTests">Показатели</param>
        /// <returns>Возвращает true если у всех показателей есть определение index</returns>
        public static bool Exists(int index, params DigitTechTest[] TechTests)
        {
            for (int i = 0, e = TechTests.Length; i < e; i++)
            {
                if (!Exists(TechTests[i], index)) return false;
            }
            return true;
        }
        #endregion методы Exists

        #region Методы округлений

        /// <summary>
        /// Округление до заданной точности например до 0.5
        /// </summary>
        /// <param name="value">Число которое требуется округлить</param>
        /// <param name="frac">Делитель на которое поделиться число</param>
        /// <returns></returns>
        public static double RoundToFrac(double value, double frac)
        {
            return Math.Round(value / frac, MidpointRounding.AwayFromZero) * frac;
        }

        /// <summary>
        /// Округление до заданного количества значащих цифр
        /// </summary>
        /// <param name="d">Число, которое требуется округлить</param>
        /// <param name="digits">Количество значащих цифр, до которого нужно округлить</param>
        /// <returns></returns>
        public static double RoundToSignificantDigits(double d, int digits)
        {
            if (d == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits, MidpointRounding.AwayFromZero);
        }
        /// <summary>
        /// Округление до заданного количества значащих цифр
        /// </summary>
        /// <param name="d">Число, которое требуется округлить</param>
        /// <param name="digits">Количество значащих цифр, до которого нужно округлить</param>
        /// <param name="format">Выходной формат значения для .ToString(format) - "0.00"</param>
        /// <returns></returns>
        public static double RoundToSignificantDigits(double d, int digits, out string format)
        {
            format = "#0.";
            if (!d.Equals(0.0))
            {
                int digsafpoint = (int)(digits - 1 - Math.Floor(Math.Log10(Math.Abs(d))));
                if (digsafpoint < 0) digsafpoint = 0;
                for (int i = 0; i < digsafpoint; i++)
                {
                    format += "0";
                }
                return Math.Round(d, digsafpoint, MidpointRounding.AwayFromZero);
            }
            else
            {
                return d;
            }
        }
        #endregion Методы округлений


        /// <summary>
        /// Проверка на вхождение конечного значения показателя в предел определения показателя
        /// </summary>
        /// <param name="ATestParam">Показатель</param>
        /// <returns></returns>
        public static bool InRangeDefinition(AnalogTechTest ATestParam)
        {
            double value = AvgMeasures(ATestParam);
            if (ATestParam.HaveHiBound && value > ATestParam.HiBound) return false;
            if (ATestParam.HaveLoBound && value < ATestParam.LoBound) return false;
            return true;
        }
        /// <summary>
        /// Возвращает интерполированное значение X (линейная интерполяция)
        /// </summary>
        /// <param name="x1">X1</param>
        /// <param name="y1">Y1</param>
        /// <param name="x2">x2</param>
        /// <param name="y2">y2</param>
        /// <param name="x">x</param>
        /// <returns>Интерполированное значение x</returns>
        public static double InterpolatedValue(double x1, double y1, double x2, double y2, double x)
        {
            double a, b;
            b = (y2 - y1) / (x2 - x1);
            a = y1 - b * x1;
            return b * x + a;
        }

        /// <summary>
        /// Получение интерполированного значения (билинейная интерполяция).
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <param name="d11"></param>
        /// <param name="d12"></param>
        /// <param name="d21"></param>
        /// <param name="d22"></param>
        /// <param name="t"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static double InterpolatedValue(double t1, double t2, double d1, double d2, double d11, double d12, double d21, double d22, double t, double d)
        {
            double r1 = ((t2 - t) / (t2 - t1)) * d11 + ((t - t1) / (t2 - t1)) * d21;
            if (double.IsNaN(r1))
                r1 = d11;

            double r2 = ((t2 - t) / (t2 - t1)) * d12 + ((t - t1) / (t2 - t1)) * d22;
            if (double.IsNaN(r2))
                r2 = d22;

            double res = ((d2 - d) / (d2 - d1)) * r1 + ((d - d1) / (d2 - d1)) * r2;
            if (double.IsNaN(res) && (r1 == r2))
                res = r1;
            return res;
        }

        /// <summary>
        /// Получить имя метрологической характеристики по её guid
        /// </summary>
        /// <param name="parameterId">Идентификатор метрологической характеристики</param>
        /// <returns></returns>
        public static string GetNameParamById(Guid parameterId)
        {
            if (parameterId == MetrologyParameters.ReproducibilityId) // Воспроизводимость
            {
                return "Воспроизводимость MetrologyParameters.ReproducibilityId";
            }
            else if (parameterId == MetrologyParameters.RepeatabilityId) // Сходимость
            {
                return "Сходимость MetrologyParameters.RepeatabilityId";
            }
            else if (parameterId == MetrologyParameters.ErrorRangeId) // Погрешность
            {
                return "Погрешность MetrologyParameters.ErrorRangeId";
            }
            else if (parameterId == MetrologyParameters.InnerPrecisionRmsId) // СКО внутрилабораторной прецизионности
            {
                return "СКО внутрилабораторной прецизионности MetrologyParameters.InnerPrecisionRmsId";
            }
            else if (parameterId == MetrologyParameters.ErrorRmsId)
            {
                return "СКО погрешности результатов MetrologyParameters.ErrorRmsId";
            }
            else if (parameterId == MetrologyParameters.HardErrorExpectId)
            {
                return "МО систематической погрешности MetrologyParameters.HardErrorExpectId";
            }
            else if (parameterId == MetrologyParameters.HardErrorRmsId)
            {
                return "СКО систематической погрешности MetrologyParameters.HardErrorRmsId";
            }
            else if (parameterId == MetrologyParameters.HardErrorRangeId)
            {
                return "Границы систематической погрешности MetrologyParameters.HardErrorRangeId";
            }
            else if (parameterId == MetrologyParameters.RepeatabilityRmsId)
            {
                return "СКО результатов единичного анализа в условиях повторяемости MetrologyParameters.RepeatabilityRmsId";
            }
            else if (parameterId == MetrologyParameters.ReproducibilityRmsId)
            {
                return "СКО всех результатов в условиях воспроизводимости MetrologyParameters.ReproducibilityRmsId";
            }
            else if (parameterId == MetrologyParameters.IntermediatePrecisionRmsId)
            {
                return "СКО промежуточной прецизионности MetrologyParameters.IntermediatePrecisionRmsId";
            }
            else if (parameterId == MetrologyParameters.FullReproducibilityId)
            {
                return "Полная воспроизводимость MetrologyParameters.FullReproducibilityId";
            }
            else if (parameterId == MetrologyParameters.PureReproducibilityId)
            {
                return "Чистая воспроизводимость MetrologyParameters.PureReproducibilityId";
            }
            else if (parameterId == MetrologyParameters.PartReproducibilityId)
            {
                return "Частичная воспроизводимость MetrologyParameters.PartReproducibilityId";
            }
            else if (parameterId == MetrologyParameters.InnerPrecisionRangeId)
            {
                return "Предел внутрилабораторной прецизионности MetrologyParameters.InnerPrecisionRangeId";
            }
            else if (parameterId == MetrologyParameters.TruenessId)
            {
                return "Правильность MetrologyParameters.TruenessId";
            }
            else if (parameterId == MetrologyParameters.PartErrorId)
            {
                return "Оценка погрешности в условиях вариации одного или нескольких факторов MetrologyParameters.PartErrorId";
            }

            else return "";
        }

        /// <summary>
        /// Проверка на равенство double c заданной погрешностью
        /// </summary>
        /// <param name="d1">Первое значение</param>
        /// <param name="d2">Второе значение</param>
        /// <param name="frac">Погрешность</param>
        /// <returns></returns>
        public static bool Equal(double d1, double d2, double frac)
        {
            if (Math.Abs(d1 - d2) < frac)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Возвращает минимальное значение из определений
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public static double MinMeasures(AnalogTechTest test)
        {
            if (test == null) return double.NaN;
            if (!test.Exists) return double.NaN;
            double min = double.MaxValue;
            for (int i = 0, e = test.MeasureCount; i < e; i++)
            {
                if (!test.Measures[i].IsNull)
                {
                    double v = test.Measures[i].Value;
                    if (v < min)
                        min = v;
                }
            }
            return (min == double.MaxValue) ? double.NaN : min;
        }

        /// <summary>
        /// Возвращает максимальное значение из определений
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        public static double MaxMeasures(AnalogTechTest test)
        {
            if (test == null) return double.NaN;
            if (!test.Exists) return double.NaN;
            double max = double.MinValue;
            for (int i = 0, e = test.MeasureCount; i < e; i++)
            {
                if (!test.Measures[i].IsNull)
                {
                    double v = test.Measures[i].Value;
                    if (v > max)
                        max = v;
                }
            }
            return (max == double.MinValue) ? double.NaN : max;
        }

        /// <summary>
        /// Возвращает сумму всех определений
        /// </summary>
        /// <param name="test">Показатель</param>
        /// <returns>Сумма всех определений</returns>
        public static double SumMeasures(AnalogTechTest test)
        {
            if (!test.Exists) return double.NaN;
            double sum = 0; 
            for (int i = 0; i < test.Measures.Count; i++)
            {
                sum += test.Measures[i].Value;
            }
            return sum;
        }

        /// <summary>
        /// Возозвращает среднее значение по всем определениям показателя. TestOutValue
        /// </summary>
        /// <param name="_test"></param>
        /// <returns></returns>
        public static double AvgMeasures(AnalogTechTest _test)
        {
            double SumMeasure = 0;
            double result = 0;
            if (_test.Exists)
            {
                for (int i = 0; i < _test.Measures.Count; i++)
                {
                    if (g.Exists(i, _test))
                    {
                        SumMeasure += _test.Measures[i].Value;
                    }
                }
                result = SumMeasure / _test.Measures.Count;
            }
            return result;
        }
        /// <summary>
        /// Делает видимыми указанные скрытые показатели.
        /// </summary>
        /// <param name="TechTests">Перечесление показателей</param>
        public static void Show(params AnalogTechTest[] TechTests)
        {
            for (int i = 0; i < TechTests.Length; i++)
            {
                if (TechTests[i].Exists)
                {
                    TechTests[i].Visible = true;
                }
            }
        }

        /// <summary>
        /// Скрывает и обнуляет значение указанных показателей
        /// </summary>
        /// <param name="FValue">Форматированное значение скрытых показателей</param>
        /// <param name="SetNull">Зануленеи скрытых показателей</param>
        /// <param name="TechTests">Перечесление показателей</param>
        public static void Hide(string FValue, bool SetNull, params AnalogTechTest[] TechTests)
        {
            for (int i = 0; i < TechTests.Length; i++)
            {
                if (TechTests[i].Exists)
                {
                    if (SetNull) TechTests[i].SetNull();
                    if (!string.IsNullOrWhiteSpace(FValue)) TechTests[i].FValue = "СКРЫТ";
                    TechTests[i].Visible = false;
                }
            }
        }
        
        /// <summary>
         /// Синхронизировать количество измерений показателей
         /// </summary>
         /// <param name="SyncTechTests">Список показателей</param>
        public static void CheckMeasures(params AnalogTechTest[] SyncTechTests)
        {
            int measures = int.MinValue;

            foreach (AnalogTechTest att in SyncTechTests)
            {
                if (att.Exists)
                {
                    if (measures < att.Measures.Count)
                        measures = att.Measures.Count;
                }
            }

            foreach (AnalogTechTest att in SyncTechTests)
            {
                if (att.Exists)
                {
                    if (att.Measures.Count < measures)
                        att.AddMeasures(measures - att.Measures.Count);
                }
            }
        }

        /// <summary>
        /// Получение листа исторических данных по контексту
        /// </summary>
        /// <param name="days">Количество дней для поиска. -1 = 1 день по умолчанию</param>
        /// <param name="context">Контекст для поиска. Test - обязательно</param>
        /// <param name="messages">Показ сообщений об ошибках</param>
        /// <returns></returns>
        public static List<AnalogTechTestResultHistory> HistTech(double days, TechTestHistoryContext context, bool messages = false)
        {
            try
            {
                if (double.IsNaN(days))
                    days = -1;
                if (days > 0)
                    days *= -1;

                if (context.Statuses.Length == 0)
                    context.Statuses = new int[] { 83, 192 };

                DateTime backintime = DateTime.Now.AddDays(days);

                var res = (new TechCalcBase()).GetHistoricalData(context, backintime, DateTime.Now);
                res.Sort((x, y) => -x.CreationDate.CompareTo(y.CreationDate));
                return res;
            }
            catch (Exception ex)
            {
                if (messages)
                    MessageBox.Show("Произошла ошибка расчета методики:\n" + ex.ToString() + "\n \n Обратитесь к администратору системы.", "HistTech", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return new List<AnalogTechTestResultHistory>();
        }

        /// <summary>
        /// Добавить показателю измерение, если разница между нынешним и предыдущим измерением больше заданного
        /// </summary>
        /// <param name="Test">Показатель</param>
        /// <param name="round_decimals">Количество знаков, которым ограничивается сравнение</param>
        /// <param name="threshold">Разница</param>
        /// <param name="measures_amount">Добавляемое количество измерений. (1 по умолчанию)</param>
        public static void AddMeasureByThreshold(AnalogTechTest Test, int round_decimals, double threshold, int measures_amount = 1)
        {
            if (Test.Exists)
            {
                if (Test.Measures.Count > 1 && threshold != double.NaN)
                {
                    if (Math.Round(Math.Abs(Test.Measures[Test.Measures.Count - 1].Value - Test.Measures[Test.Measures.Count - 2].Value), round_decimals, MidpointRounding.AwayFromZero) > threshold)
                    {
                        Test.AddMeasures(measures_amount);
                    }
                }
            }
        }
    }
}
