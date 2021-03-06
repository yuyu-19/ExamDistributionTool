﻿using System;
using System.Collections.Generic;

namespace DistribuisciEsamiCommon
{
    public class Esami
    {
        private readonly Dictionary<string, Esame> dictionary;

        public Esami()
        {
            dictionary = new Dictionary<string, Esame>();
        }

        public Esami(string file)
        {
            dictionary = new Dictionary<string, Esame>();
            object jObject;
            try
            {
                jObject = Newtonsoft.Json.JsonConvert.DeserializeObject(file);
            }
            catch (Exception e)
            {
                throw e;
            }

            if (jObject == null)
            {
                return;
            }

            if (jObject is Newtonsoft.Json.Linq.JArray ja)
            {
                var jac = ja.Children();
                foreach (var x in jac)
                {
                    Aggiungi(x);
                }
            }
        }

        private void Aggiungi(Newtonsoft.Json.Linq.JToken x)
        {
            string nome = GetNomeFromJson(x);
            List<DateTime> date = GetDateTimesFromJson(x);
            int cfu = GetCfuFromJson(x);

            Esame esame = new Esame(nome, date, cfu);
            this.dictionary[nome] = esame;
        }

        public Esame GetExam(string x)
        {
            return this.dictionary[x];
        }

        public Dictionary<string, Esame> GetEsami()
        {
            return dictionary;
        }

        public EsamiFromFile CheckText(string filecontent, string[] lines)
        {
            Esami esami = null;
            try
            {
                esami = new Esami(filecontent);
            }
            catch
            {
                ;
            }

            if (esami != null && esami.GetEsami() != null && esami.GetEsami().Count > 0)
                return new EsamiFromFile(esami);

            //The file isn't formatted like the readme says it should be. Let's see if it's copied from the exams page.
            List<string> JSON = new List<string>
            {
                "["
            };
            foreach (string line in lines)
            {
                if (line.Contains("-"))
                {
                    if (Array.IndexOf(lines, line) + 1 == lines.Length)
                        break; //This line is a new subject, but there's no lines after this so there can't be any dates.
                    else
                    {
                        if (lines[Array.IndexOf(lines, line) + 1].IndexOf("/") != 2)
                            continue; //Is the next line NOT a date? Then keep going. This subject has no listed dates.
                    }
                    JSON.Add("{");
                    string subjectname = line.Substring(0, line.IndexOf("-")).Trim();
                    JSON.Add("\"name\":\"" + subjectname + "\",");
                    string dateline = "\"date\":[";
                    string nextline = lines[Array.IndexOf(lines, line) + 1];
                    while (nextline.IndexOf("/") == 2)
                    {
                        //So long as the next line is a date
                        dateline += "\"" + Convert.ToDateTime(nextline.Substring(0, nextline.IndexOf(":")).Replace("/", "-")).ToString("yyyy-MM-dd") + "\",";
                        nextline = lines[Array.IndexOf(lines, nextline) + 1];
                    }
                    dateline = (dateline + "\"],").Replace("\",\"],", "\"],");  //2lazy to fix it properly
                    JSON.Add(dateline);
                    //MessageBox.Show(dateline);

                    JSON.Add("\"cfu\":\"" + "[CFUNUM-PLACEHOLDER-" + subjectname + "]" + "\"");

                    JSON.Add("},");
                }
            }
            JSON.Add("]");
            JSON[JSON.Count - 2] = "}";    //Remove the comma from the last closed curly bracket
            return new EsamiFromFile(JSON);
            
        }

        private static int GetCfuFromJson(Newtonsoft.Json.Linq.JToken x)
        {
            foreach (var x2 in x.Children())
            {
                if (x2 is Newtonsoft.Json.Linq.JProperty x3)
                {
                    if (x3.Name == "cfu")
                    {
                        try
                        {
                            return Convert.ToInt32(x3.Value.ToString());
                        }
                        catch
                        {
                            ;
                        }
                    }
                }
            }
            return 1;
        }

        private static List<DateTime> GetDateTimesFromJson(Newtonsoft.Json.Linq.JToken x)
        {
            foreach (var x2 in x.Children())
            {
                if (x2 is Newtonsoft.Json.Linq.JProperty x3)
                {
                    if (x3.Name == "date")
                    {
                        var x4 = x3.Value;

                        if (x4 is Newtonsoft.Json.Linq.JArray x5)
                        {
                            List<DateTime> r = new List<DateTime>();
                            foreach (var x6 in x5.Children())
                            {
                                if (x6 is Newtonsoft.Json.Linq.JValue x7)
                                {
                                    var x8 = x7.Value.ToString();
                                    var x9 = x8.Split('-');
                                    int anno, mese, giorno;

                                    if (x9[0].Length == 4)
                                    {
                                        anno = Convert.ToInt32(x9[0]);
                                        mese = Convert.ToInt32(x9[1]);
                                        giorno = Convert.ToInt32(x9[2]);
                                    }
                                    else
                                    {        //It's the ""import"" mode.
                                        anno = Convert.ToInt32(x9[2]);
                                        mese = Convert.ToInt32(x9[1]);
                                        giorno = Convert.ToInt32(x9[0]);
                                    }

                                    DateTime dt = new DateTime(anno, mese, giorno);
                                    r.Add(dt);
                                }
                            }

                            return r;
                        }
                    }
                }
            }
            return null;
        }

        private static string GetNomeFromJson(Newtonsoft.Json.Linq.JToken x)
        {
            foreach (var x2 in x.Children())
            {
                if (x2 is Newtonsoft.Json.Linq.JProperty x3)
                {
                    if (x3.Name == "name")
                    {
                        return x3.Value.ToString();
                    }
                }
            }
            return null;
        }

        internal void Add(Esame esame)
        {
            dictionary[esame.nome] = esame;
        }

        public List<string> GetKeys()
        {
            List<string> r = new List<string>();
            foreach (var x in dictionary.Keys)
            {
                r.Add(x);
            }
            return r;
        }

        public List<DateTime> GetDateTimes(string v)
        {
            return this.dictionary[v].dateTimes;
        }

        public bool IsEmpty()
        {
            return this.dictionary == null || this.dictionary.Keys.Count == 0;
        }
    }
}