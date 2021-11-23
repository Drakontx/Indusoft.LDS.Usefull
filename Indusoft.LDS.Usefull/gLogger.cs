using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace Indusoft.LDS.Usefull
{
    // TODO: Переработать логер +
    /// <summary>
    /// 
    /// </summary>
    public sealed class gLogger
    {
        /// <summary>
        /// Путь к файлу логов
        /// </summary>
        // TODO: переработать путь хранения, узнать из какой дирректории запущено приложение Directory.GetCurrentDirectory(); 
        // Получить дирректорию хранения логов из конфиг файла.
        private static string __log_file_path = @"C:\Documents and Settings\All Users\Application Data\InduSoft\I-LDS\2\Log\Indusoft.LDS.Calc.log";
        private const string __info_format = @"{0};  [INFO ];    {1}";
        private const string __error_format = "{0};  [ERROR];    {1}\r\n\t{2}\r\n\t{3}\r\n\t{4}";
        /// <summary>
        /// Оставил для совместимости с прошлыми версиями
        /// </summary>
        public static void ShowForm()
        {
            MessageBox.Show(string.Format("Логи теперь здесь: {0}", __log_file_path));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="msg"></param>
        public static void WriteLine(string format, params object[] msg)
        {
            WriteInfo(string.Format(format, msg));
        }

        public static void WriteLine(object msg)
        {
            WriteInfo(msg.ToString());
        }

        private static void WriteLine(string f)
        {
            msg += f;
            LogToFile(msg);
        }

        public static void WriteInfo(string format, params object[] msg)
        {
            string s = string.Format(format, msg);
            LogToFile(string.Format(__info_format, UTC(), s));
        }

        public static void WriteInfo(object msg)
        {
            LogToFile(string.Format(__info_format, UTC(), msg));
        }

        public static void WriteError(Exception exception, object msg)
        {
            string exMessage = "", exData = "", exStackTrace = "";
            if (exception != null)
            {
                exMessage = exception.Message;
                exData = exception.Data.ToString();
                exStackTrace = exception.StackTrace;
            }
            string s = string.Format(__error_format, UTC(), msg, exMessage, exData, exStackTrace);
            LogToFile(s);
        }

        private static string UTC()
        {
            return DateTime.Now.ToUniversalTime().ToString("o");
        }

        /// <summary>
        /// Создает файл
        /// </summary>
        private static void Flush()
        {
            File.WriteAllText(__log_file_path, string.Empty);
        }

        /// <summary>
        /// Пишем в файл данные
        /// </summary>
        private static void LogToFile(string msg)
        {
            try
            {
                if (msg.Length > 0)
                {
                    if (!File.Exists(__log_file_path)) Flush();
                    using (StreamWriter sw = File.AppendText(__log_file_path))
                    {
                        sw.WriteLine(msg);
                        sw.Flush();
                    }
                }
            }
            catch { }
        }

        private static string msg = "";
        public static void Write(string f)
        {
            msg = f;
        }
        public static void Write(string f, params object[] s)
        {
            msg = string.Format(f, s);
            LogToFile(msg);
        }

        /// <summary>
        /// Инспектор объекта
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="obj">Объет для исседования</param>
        public static void Inspect<T>(T obj) where T : class
        {
            Type t = typeof(T);
            MethodInfo[] MArr = t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            WriteLine("*** Список методов класса {0} ***\r\n", obj.ToString());
            foreach (MethodInfo m in MArr)
            {
                Write(" --> " + m.ReturnType.Name + " \t" + m.Name + "(");
                ParameterInfo[] p = m.GetParameters();
                for (int i = 0; i < p.Length; i++)
                {
                    Write(p[i].ParameterType.Name + " " + p[i].Name);
                    if (i + 1 < p.Length) Write(", ");
                }
                Write(")\n");
            }

            WriteLine("\r\n*** Поля ***\r\n");
            FieldInfo[] fieldNames = t.GetFields();
            foreach (FieldInfo fil in fieldNames)
                WriteLine("--> " + fil.FieldType.ToString().PadRight(60) + fil.ReflectedType.Name + "." + fil.Name + "=" + fil.GetValue(obj));

            WriteLine("\r\n*** Cвойства ***\r\n");
            PropertyInfo[] propNames = t.GetProperties();
            foreach (PropertyInfo prop in propNames)
                WriteLine("--> " + prop.PropertyType.ToString().PadRight(60) + prop.ReflectedType.Name + "." + prop.Name + "=" + prop.GetValue(obj, new object[] { 0 }));
        }
    }
}
