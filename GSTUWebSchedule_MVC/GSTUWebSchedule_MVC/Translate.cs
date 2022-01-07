using System;
using System.Collections.Generic;
using System.Linq;

namespace GSTUWebSchedule_MVC
{
    public class Translate
    {
        public static List<string> ru = new List<string>();
        public static List<string> en = new List<string>();

        public static void Init()
        {
            ru = Properties.Resources.ru.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            en = Properties.Resources.en.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static string Tr(string x, string local)
        {
            if (en.FindIndex(s => s == x) < 0) return "TRANSLATE_ERROR";
            if (local == "ru") return ru[en.FindIndex(s => s == x)]; else return x;
        }
    }
}
