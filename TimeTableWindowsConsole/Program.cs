using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace TimeTableWindowsConsole
{
    class Program
    {   
        public static List<RaumFinal> raeume = new List<RaumFinal>();

        static void Main(string[] args)
        {
            var json = File.ReadAllText("Infos.json");
            if (json.Length == 0)
            {
                throw new Exception("Leere JSON!");
            }

            Driver Content = Driver.FromJson(json);

            List<StudiengangFinal> studiengaenge = new List<StudiengangFinal>();
            foreach (StudiengangFinal studienga in Content.Studiengaenge)
            {
                StudiengangFinal finalStudiengang = new StudiengangFinal();
                finalStudiengang.Id = studienga.Id;
                finalStudiengang.Name = studienga.Name;
                //finalStudiengang.Semester = studienga.Semester;
                finalStudiengang.Semest = studienga.Semest;
                studiengaenge.Add(finalStudiengang);
                //Console.WriteLine("Eingetragener Studiengang: " + finalStudiengang.Name);
            }

            List<DozentFinal> dozenten = new List<DozentFinal>();
            foreach (var dozent in Content.Dozenten)
            {
                DozentFinal finalDoz = new DozentFinal();
                finalDoz.Id = dozent.Id;
                finalDoz.Name = dozent.Name;
                dozenten.Add(finalDoz);
               // Console.WriteLine("Unterrichtende Dozenten: "+finalDoz.Name);
            }
          
            foreach (var raum in Content.Raeume)
            {
                RaumFinal finalRaum = new RaumFinal();
                finalRaum.Id = raum.Id;
                finalRaum.RaumNr = raum.RaumNr;
                finalRaum.Austattung = raum.Ausstattung;
                finalRaum.RaumStudiengang = raum.RaumStudiengang;
                finalRaum.Belegt = false;
                raeume.Add(finalRaum);
                //Console.WriteLine("Möglicher Raum: " + finalRaum.RaumNr + "mit Austattungsziffer: "+ raum.Ausstattung);
            }

            List<KursFinal> kurse= new List<KursFinal>();
            foreach (var kurs in Content.Kurse)
            {
                KursFinal finalKrs = new KursFinal();
                finalKrs.Id = kurs.Id;
                finalKrs.Name = kurs.Name;
                finalKrs.Dozent = dozenten.Single(dozent => dozent.Id == kurs.Dozent);
                //finalKrs.Raum = raeume.First(raum => raum.Austattung == kurs.Anforderungen);
                finalKrs.KursSemester = kurs.KursSemester;
                finalKrs.KursStudiengang = kurs.KursStudiengang;
                kurse.Add(finalKrs);
                //Console.WriteLine("Kurs " + finalKrs.Name + " wird unterrichtet von "+ finalKrs.Dozent.Name+ ", im Raum "+finalKrs.Raum.RaumNr + ", mit der Austattung "+ finalKrs.Raum.Austattung);
            }

            List<SemesterFinal> semester = new List<SemesterFinal>();
            foreach (var semest in Content.Semester)
            {
                SemesterFinal finalSemester = new SemesterFinal();
                finalSemester.Id = semest.Id;
                finalSemester.SemesterNumber = semest.SemesterNumber;
                finalSemester.AlleKurseDerWeek = new KursFinal[5,4];
                semester.Add(finalSemester);
                //Console.WriteLine("darin mögliche Semester:" + finalSemester.SemesterNumber);
            }

            GenerateTimeTable();





            void GenerateTimeTable()
            {

           


            foreach (StudiengangFinal studienga in studiengaenge) //MIB
            {
              

                for (int semesterZähler =0; semesterZähler <= 1; semesterZähler++)
                {
                    KursFinal[,] KursplanNachStudiengangUndSemester = new KursFinal[4, 3];

                    //KursFinal[] alleKurseAnEinemTag = new KursFinal[4];
                    // KursFinal[] alleKurseEinerWoche = new KursFinal[];
                    int blockZähler = 0;
                    int tagesZähler = 0;
                    List<DozentFinal> BereitsVergebeneDozenten = new List<DozentFinal>();


                    foreach (KursFinal kurs in kurse)
                    {
                        for (int k = 0; k < KursplanNachStudiengangUndSemester.GetLength(0); k++)
                        {
                            for (int l = 0; l < KursplanNachStudiengangUndSemester.GetLength(1); l++)
                            {
                                try
                                {
                                    Console.WriteLine("Tag: " + (tagesZähler) + " " + " Block: " + (blockZähler) + "  Vorlesung " + KursplanNachStudiengangUndSemester[k, l].Name + " im Studiengang: " + KursplanNachStudiengangUndSemester[k, l].KursStudiengang + " bei Professor: " + kurs.Dozent.Name);
                                    
                                }
                                catch (Exception ex)
                                {
                                    // Console.WriteLine("FEHLER");
                                }
                            }
                        }

                        var profPrüfer = BereitsVergebeneDozenten.Find(item => item.Id != kurs.Dozent.Id);
                        

                    if (semesterZähler == kurs.KursSemester && kurs.KursStudiengang == studienga.Name && !BereitsVergebeneDozenten.Contains(kurs.Dozent))
                        {
                            BereitsVergebeneDozenten.Add(kurs.Dozent);
                            kurs.Raum = RaumFinder(studienga, kurs);

                            //nur tageszähler hoch wenn blockzähler full
                            // alleKurseAnEinemTag[blockZähler] = kurs;
                            KursplanNachStudiengangUndSemester[tagesZähler, blockZähler++] = kurs;
                            
                          
                            if (blockZähler >= 3 && KursplanNachStudiengangUndSemester.GetLength(1) <= 5)
                            {
                                tagesZähler++;
                                blockZähler = 0;
                            }
                            //alleKurseEinerWoche.Add(alleKurseAnEinemTag);
                        }
                    }

                    
                    //studienga.Semest[semesterZähler].AlleKurseDerWeek.Add(alleKurseAnEinemTag); 
                }

                
            }

            }






            RaumFinal RaumFinder(StudiengangFinal studiengang,KursFinal kurs)
            {
                foreach (RaumFinal raum in raeume)
                {
                    if (raum.RaumStudiengang == studiengang.Name && raum.Austattung == kurs.Anforderungen && raum.Belegt )
                    {
                        raum.Belegt = true;
                        return raum;                     
                    }
                }
                return null;      
            }
        }
        
       
    }
}















/*

*/
