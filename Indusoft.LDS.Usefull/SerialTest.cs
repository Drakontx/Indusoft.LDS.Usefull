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
	/// <summary>
	/// Серийный показатель.
	/// Значение одного измерения выбранного показателя = выбранное значение определенного показателя с N измерений.
	/// </summary>
	public class SerialTest
	{
		/// <summary>
		/// Серийный показатель.
		/// Значение одного измерения выбранного показателя = выбранное значение определенного показателя с N измерений.
		/// </summary>
		/// <param name="MainTest">Главный показатель</param>
		/// <param name="MeasureTests">Вспомогательные показатели-измерения в порядке их соответствия номеру измерения главного показателя.</param>
		private SerialTest(AnalogTechTest MainTest, params AnalogTechTest[] MeasureTests)
		{
			this.MainTest = MainTest;
			this.MeasureTests = MeasureTests;

			this.ValueMode = ReturnValue.LastMeasure;
			this.Compare = CompareMode.Absolute;
			this.Threshold = double.NaN;
			Decimals = 5;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Threshold">Пороговая разница последнего и предпоследнего измерения вспомогательного показателя, при привышении которой ему будет добавлено измерение</param>
		/// <param name="MainTest">Главный показатель</param>
		/// <param name="MeasureTests">Вспомогательные показатели-измерения в порядке их соответствия номеру измерения главного показателя.</param>
		/// <remarks>
		///   <example>
		///     <code>
		///       Test0 - m2 - масса тигля с пробой после прокаливания (1 определение)
		///       Test1 - m2 - масса тигля с пробой после прокаливания (2 определение)
		///       Test2 - m2 - масса тигля с пробой после прокаливания (3 определение)
		///       Test3 - m2 - масса тигля с пробой после прокаливания (4 определение)
		///       Test4 - m2 - масса тигля с пробой после прокаливания
		///       
		///		  //4 показателя являются 4 измерениями показателя Test4. Добавление измерений показателям Test0-Test3 происходит при превышении порога 2 абсолютные единицы
		///		  SerialTest tigl_posle = new SerialTest(2, Test4, Test0, Test1, Test2, Test3);
		///		  
		///		  //Можно изменить режим сравнения - абсолютный или процентный. По умолчанию режим - абсолютный. Можно изменить на процентное, в итоге порог будет 2% разныцы между последним и предыдущим измерением
		///		  tigl_posle.Compare = CompareMode.Percent;
		///		  
		///		  //Можно менять то, какое значение будет использоваться в качестве измерения для основного показателя. По умолчанию - последнее измерение.
		///		  //tigl_posle.ValueMode = ReturnValue.LastMeasure;
		///		  
		///		  //Вызов обработки набора
		///		  tigl_posle.Process();
		///     </code>
		///   </example>
		/// </remarks>
		public SerialTest(double Threshold, AnalogTechTest MainTest, params AnalogTechTest[] MeasureTests) : this(MainTest, MeasureTests)
		{
			this.Threshold = Threshold;
		}

		/// <summary>
		/// Основной показатель для массы прокаленной чашки без осадка
		/// </summary>
		public AnalogTechTest MainTest { get; private set; }

        /// <summary>
        /// Дополнительный показатель от которого считать % значение.
        /// </summary>
        public AnalogTechTest ThresholdTest { get; private set; }

        /// <summary>
        /// Показатели-измерения
        /// </summary>
        private AnalogTechTest[] MeasureTests;

		/// <summary>
		/// Порог разницы между измерениями массы высушиваемой чашки без осадка, при котором будет добавлено новое измерение его показателям-измерениям
		/// </summary>
		public double Threshold { get; set; }

		/// <summary>
		/// Количество знаков, которым ограничивается сравнение
		/// </summary>
		public int Decimals { get; set; }

		/// <summary>
		/// Выбор значения для измерения
		/// </summary>
		public ReturnValue ValueMode { get; set; }

		/// <summary>
		/// Режим сравнения предыдущего измерения
		/// </summary>
		public CompareMode Compare { get; set; }

		/// <summary>
		/// Обработать
		/// </summary>
		public void Process()
		{
			HideShow();
			foreach (AnalogTechTest ATest in MeasureTests)
			{
				if (ATest.Visible)
				{
					switch (Compare)
					{
						case CompareMode.Percent:
                            {
                                double threshold_from_perc = double.NaN;
                                if (ThresholdTest != null)
                                {
                                    threshold_from_perc = (ATest.Measures.Count > 1) ? Math.Round(ThresholdTest.Measures[MeasuresIndex].Value * Threshold * 0.01, Decimals, MidpointRounding.AwayFromZero) : double.NaN;
                                }
                                else
                                {
                                    threshold_from_perc = (ATest.Measures.Count > 1) ? Math.Round(ATest.Measures[ATest.Measures.Count - 2].Value * Threshold * 0.01, Decimals, MidpointRounding.AwayFromZero) : double.NaN;
                                }
                                g.AddMeasureByThreshold(ATest, Decimals, threshold_from_perc);
                                break;
                            }
                        case CompareMode.Absolute:
						default:
							g.AddMeasureByThreshold(ATest, Decimals, Threshold);
							break;
					}

				}
			}
			if (MainTest.Exists)
			{
				for (int i = 0; i < MainTest.Measures.Count; i++)
				{
					double value = double.NaN;
					switch (ValueMode)
					{
						case ReturnValue.Average:
							value = g.AvgMeasures(MeasureTests[i]);
							break;
						case ReturnValue.Final:
							value = MeasureTests[i].Value;
							break;
						case ReturnValue.LastMeasure:
							value = MeasureTests[i].Measures[MeasureTests[i].Measures.Count - 1].Value;
							break;
					}
					MeasureTests[i].SetValue(value);
					MainTest.Measures[i].Value = value;
				}
			}
		}

		void HideShow()
		{
			if (MainTest.Exists && MeasureTests.Length != 0)
			{
				int measures_diff = MeasureTests.Length - MainTest.Measures.Count;
				int measures = Math.Min(MainTest.Measures.Count, MeasureTests.Length);
				for (int i = 0; i < measures; i++)
				{
					g.Show(MeasureTests[i]);
				}
				if (measures_diff > 0)
				{
					for (int i = 0; i < measures_diff; i++)
					{
						g.Hide("СКРЫТ", true, MeasureTests[MeasureTests.Length - 1 - i]);
					}
				}
			}
		}

		/// <summary>
		/// Определение какой значение показателя является результатом для измерения основного
		/// </summary>
		public enum ReturnValue
		{
			/// <summary>
			/// Последнее измерение
			/// </summary>
			LastMeasure,
			/// <summary>
			/// Конечное значение
			/// </summary>
			Final,
			/// <summary>
			/// Среднее значение
			/// </summary>
			Average
		}

		/// <summary>
		/// Режим сравнения предыдущего измерения
		/// </summary>
		public enum CompareMode
        {
			/// <summary>
			/// По абсолютному значению
			/// </summary>
			Absolute,
			/// <summary>
			/// Процентное значение
			/// </summary>
			Percent
        }
	}
}
