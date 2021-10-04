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
		/// 
		/// </summary>
		/// <param name="MainTest">Главный показатель</param>
		/// <param name="MeasureTests">Вспомогательные показатели-измерения в порядке их соответствия номеру измерения главного показателя.</param>
		public SerialTest(AnalogTechTest MainTest, params AnalogTechTest[] MeasureTests)
		{
			this.MainTest = MainTest;
			this.MeasureTests = MeasureTests;

			this.ValueMode = ReturnValue.LastMeasure;
			this.Threshold = double.NaN;
			Decimals = 5;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Threshold">Пороговая разница последнего и предпоследнего измерения вспомогательного показателя, при привышении которой ему будет добавлено измерение</param>
		/// <param name="MainTest">Главный показатель</param>
		/// <param name="MeasureTests">Вспомогательные показатели-измерения в порядке их соответствия номеру измерения главного показателя.</param>
		public SerialTest(double Threshold, AnalogTechTest MainTest, params AnalogTechTest[] MeasureTests) : this(MainTest, MeasureTests)
		{
			this.Threshold = Threshold;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="valueMode">Выбор значения показателя-измерения, которое идет в измерение основного показателя</param>
		/// <param name="Threshold">Пороговая разница последнего и предпоследнего измерения вспомогательного показателя, при привышении которой ему будет добавлено измерение</param>
		/// <param name="MainTest">Главный показатель</param>
		/// <param name="MeasureTests">Вспомогательные показатели-измерения в порядке их соответствия номеру измерения главного показателя.</param>
		public SerialTest(ReturnValue valueMode, double Threshold, AnalogTechTest MainTest, params AnalogTechTest[] MeasureTests) : this(Threshold, MainTest, MeasureTests)
		{
			this.ValueMode = valueMode;
		}

		/// <summary>
		/// Основной показатель для массы прокаленной чашки без осадка
		/// </summary>
		public AnalogTechTest MainTest { get; private set; }

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
		/// Обработать
		/// </summary>
		public void Process()
		{
			HideShow();
			foreach (AnalogTechTest ATest in MeasureTests)
			{
				if (ATest.Visible)
				{
					g.AddMeasureByThreshold(ATest, Decimals, Threshold);
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
	}
}
