using Indusoft.LDS.Core;
using Indusoft.LDS.Format.Common.Models;
using Indusoft.LDS.IMath;
using Indusoft.LDS.Script.Common;
using Indusoft.LDS.Techniques.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Indusoft.LDS.Usefull.Rounding
{
    //TODO: Создать шаблоны округлений
    /// <summary>
    /// Округления ЛИМС
    /// </summary>
    public class Rounding
    {
        /// <summary>
        /// Внутренний класс округлений - RoundUtils
        /// </summary>
        public RoundUtils RU { get; private set; }

        /// <summary>
        /// Получение информации форматирования из базы через сессию расчета
        /// </summary>
        /// <param name="session">Сессия расчета</param>
        public Rounding(IScriptSession session)
        {
            RU = new RoundUtils(new ServiceFactory(session));
        }

        /// <summary>
        /// Получение информации форматирования из базы через подключение напрямую
        /// </summary>
        /// <param name="connectionString">Строка подключения</param>
        public Rounding(string connectionString)
        {
            RU = new RoundUtils(new ServiceFactory(connectionString));
        }

        /// <summary>
        /// Задание информации форматирования без базы - вручную
        /// </summary>
        /// <param name="rounding">Тип округления</param>
        /// <param name="separator">Десятичный разделитель</param>
        public Rounding(MidpointRounding rounding, string separator)
        {
            RU = new RoundUtils(new ServiceFactory(rounding, separator));
        }
    }
}
