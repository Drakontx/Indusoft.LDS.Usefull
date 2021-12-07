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
    //TODO: Добавить возможность ручного режима использования независимо от базы
    internal class Rounding
    {
        public RoundUtils RU { get; private set; }

        public Rounding(IScriptSession session)
        {
            RU = new RoundUtils(new ServiceFactory(session));
        }

        public Rounding(string connectionString)
        {
            RU = new RoundUtils(new ServiceFactory(connectionString));
        }

        public bool GetFormattedValue(TechTestValueFmt format, double value, out RoundedView view)
        {
            view = new RoundedView();
            return false;
        }
    }
}
