using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Labb3MorganWestin;
using Microsoft.EntityFrameworkCore;

namespace Labb3Morgan
{
    public class Anställd
    {
        [Key]
        public int personal_id { get; set; }
        public string anst_id { get; set; }
        public string förnamn { get; set; }
        public string efternamn { get; set; }
        public string personnummer { get; set; }
        public string adress { get; set; }
        public DateTime? anst_datum { get; set; }
        public string befattning { get; set; }
    }

    public class Student
    {
        [Key]
        public int student_id { get; set; }
        public string stud_id { get; set; }
        public string förnamn { get; set; }
        public string efternamn { get; set; }
        public string personnummer { get; set; }
        public string adress { get; set; }
        public int klass_id { get; set; }
        public DateTime? ålder { get; set; }
    }

    public class Betyg
    {
        [Key]
        public int betyg_id { get; set; }
        public string betyg { get; set; }
        public DateTime? datum_satt { get; set; }
        public int kurs_id { get; set; }
        public int lärare_id { get; set; }
        public int student_id { get; set; }
        public Student Student { get; set; }
        public Kurs Kurs { get; set; }
    }
    public class Kurs
    {
        [Key]
        public int kurs_id { get; set; }
        public string kurs_namn { get; set; }
    }

    public class SkolDbContext : DbContext
    {
        public DbSet<Anställd> Personal { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<Betyg> Betyg { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=VärmdöGymnasie;Trusted_Connection=True;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfigurera relationen mellan Betyg och Student
            modelBuilder.Entity<Betyg>()
                .HasOne(b => b.Student)
                .WithMany()
                .HasForeignKey(b => b.student_id);

            // Konfigurera relationen mellan Betyg och Kurs
            modelBuilder.Entity<Betyg>()
                .HasOne(b => b.Kurs)
                .WithMany()
                .HasForeignKey(b => b.kurs_id);

            // ...
        }
    }

    class Program
    {
        static void Main()
        {
            using (var context = new SkolDbContext())
            {
                while (true)
                {
                    Console.WriteLine("Välj ett alternativ:");
                    Console.WriteLine("1. Hämta personal");
                    Console.WriteLine("2. Hämta elever och sortera");
                    Console.WriteLine("3. Hämta betyg från senaste månaden");
                    Console.WriteLine("4. Hämta kursstatistik");
                    Console.WriteLine("5. Lägg till ny Student");
                    Console.WriteLine("6. Lägg till ny Personal");
                    Console.WriteLine("7. Avsluta");

                    int val;
                    if (!int.TryParse(Console.ReadLine(), out val))
                    {
                        Console.WriteLine("Ogiltigt val. Försök igen.");
                        continue;
                    }

                    switch (val)
                    {
                        case 1:
                            HämtaPersonal(context);
                            break;
                        case 2:
                            HämtaEleverOchSortera(context);
                            break;
                        case 3:
                            HämtaBetygSenasteMånaden(context);
                            break;
                        case 4:
                            HämtaKursStatistik(context);
                            break;
                        case 5:
                            LäggTillNyElev(context);
                            break;
                         case 6:
                             LäggTillNyPersonal(context);
                             break;
                        case 7:
                            Environment.Exit(0);
                            break;
                        default:
                            Console.WriteLine("Ogiltigt val. Försök igen.");
                            break;

                    }
                }
            }
        }

        static void HämtaPersonal(SkolDbContext context)
        {
            Console.WriteLine("Välj ett alternativ:");
            Console.WriteLine("1. Visa alla anställda");
            Console.WriteLine("2. Visa anställda inom en kategori");
            Console.WriteLine("3. Återgå till huvudmenyn");

            int val;
            if (!int.TryParse(Console.ReadLine(), out val))
            {
                Console.WriteLine("Ogiltigt val. Försök igen.");
                return;
            }

            switch (val)
            {
                case 1:

                    VisaAllaAnställda(context);
                    break;
                case 2:
                    VisaAnställdaInomKategori(context);
                    break;
                case 3:
                    break;
                default:
                    Console.WriteLine("Ogiltigt val. Försök igen.");
                    break;
            }
        }

        static void VisaAllaAnställda(SkolDbContext context)
        {
            var anställda = context.Personal.ToList();
            SkrivUtAnställda(anställda);
        }

        static void VisaAnställdaInomKategori(SkolDbContext context)
        {
            Console.Write("Ange kategori (t.ex. lärare, rektor): ");
            string kategori = Console.ReadLine();

            var anställdaIKategori = context.Personal.Where(p => p.befattning.ToLower() == kategori.ToLower()).ToList();
            SkrivUtAnställda(anställdaIKategori);
        }

        static void SkrivUtAnställda(List<Anställd> anställda)
        {
            foreach (var anställd in anställda)
            {
                Console.WriteLine($"ID: {anställd.personal_id}, Förnamn: {anställd.förnamn}, Efternamn: {anställd.efternamn}, Kategori: {anställd.befattning}");
            }
        }

        static void HämtaEleverOchSortera(SkolDbContext context)
        {
            VisaKlasser(context);
            Console.WriteLine("Välj en klass:");

            if (!int.TryParse(Console.ReadLine(), out int valdKlassId))
            {
                Console.WriteLine("Ogiltigt val.");
                return;
            }

            HämtaOchVisaEleverIFöredragenKlass(context, valdKlassId);
        }

        static void VisaKlasser(SkolDbContext context)
        {
            try
            {
                var klasser = context.Student.Select(s => s.klass_id).Distinct().ToList();

                Console.WriteLine("Tillgängliga klasser:");
                foreach (var klass in klasser)
                {
                    Console.WriteLine(klass);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid visning av klasser. Exception: {ex}");
            }
        }

        static void HämtaOchVisaEleverIFöredragenKlass(SkolDbContext context, int valdKlassId)
        {
            try
            {
                // Visa alternativ för sortering
                Console.WriteLine("Välj sorteringsalternativ:");
                Console.WriteLine("1. Förnamn stigande");
                Console.WriteLine("2. Förnamn fallande");
                Console.WriteLine("3. Efternamn stigande");
                Console.WriteLine("4. Efternamn fallande");

                if (!int.TryParse(Console.ReadLine(), out int sortVal) || sortVal < 1 || sortVal > 4)
                {
                    Console.WriteLine("Ogiltigt val. Försök igen.");
                    return;
                }

                // Hämta och visa elever i vald klass med vald sortering
                var eleverIKlass = context.Student
                    .Where(s => s.klass_id == valdKlassId)
                    .OrderBy(s => sortVal == 1 || sortVal == 2 ? s.förnamn : s.efternamn)
                    .ThenBy(s => sortVal % 2 == 0 ? s.förnamn : s.efternamn)
                    .ToList();

                if (eleverIKlass.Any())
                {
                    Console.WriteLine($"Elever i klass {valdKlassId}:");

                    foreach (var elev in eleverIKlass)
                    {
                        Console.WriteLine($"ElevId: {elev.student_id}, Förnamn: {elev.förnamn}, Efternamn: {elev.efternamn}");
                    }
                }
                else
                {
                    Console.WriteLine($"Inga elever hittades i klass {valdKlassId}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid hämtning och visning av elever i klass. Exception: {ex}");
            }
        }

        static void HämtaBetygSenasteMånaden(SkolDbContext context)
        {
            var föregåendeMånad = DateTime.Now.AddMonths(-1);
            var betygLista = context.Betyg
                .Where(b => b.datum_satt >= föregåendeMånad)
                .Include(b => b.Student)
                .Include(b => b.Kurs)
                .ToList();

            foreach (var betyg in betygLista)
            {
                Console.WriteLine($"Elev: {betyg.Student.förnamn} {betyg.Student.efternamn}, Kurs: {betyg.Kurs.kurs_namn}, Betyg: {betyg.betyg}, Datum: {betyg.datum_satt}");
            }
        }
        static void HämtaKursStatistik(SkolDbContext context)
        {
            var kursStatistik = context.Betyg
                .Include(b => b.Kurs)
                .GroupBy(b => b.Kurs.kurs_namn)
                .Select(g => new
                {
                    KursNamn = g.Key,
                    Betygen = g.ToList()
                })
                .ToList();

            Console.WriteLine("Kurs\tSnittbetyg\tHögsta betyg\tLägsta betyg");
            foreach (var kurs in kursStatistik)
            {
                var snittBetyg = kurs.Betygen.Average(b => BetygTillPoäng(b.betyg));
                var högstaBetyg = kurs.Betygen.Max(b => BetygTillPoäng(b.betyg));
                var lägstaBetyg = kurs.Betygen.Min(b => BetygTillPoäng(b.betyg));

                Console.WriteLine($"{kurs.KursNamn}\t{snittBetyg:F2}\t\t{högstaBetyg}\t\t{lägstaBetyg}");
            }
        }
        static int BetygTillPoäng(string betyg)
        {
            switch (betyg)
            {
                case "A": return 95;
                case "B": return 80;
                case "C": return 50;
                case "D": return 45;
                case "E": return 40;
                default: return 0; // För F eller andra okända betyg
            }
        }
        static void LäggTillNyElev(SkolDbContext context)
        {
            try
            {
                Console.WriteLine("Ange information om den nya eleven:");
                int studentId;
                bool isStudentIdUnique = false;

                do
                {
                    Console.Write("Student ID: ");
                    if (!int.TryParse(Console.ReadLine(), out studentId))
                    {
                        Console.WriteLine("Ogiltigt student ID. Försök igen...");
                        continue;
                    }
                    isStudentIdUnique = !context.Student.Any(s => s.student_id == studentId);

                    if (!isStudentIdUnique)
                    {
                        Console.WriteLine($"Student ID {studentId} är redan taget. Välj ett annat ID.");
                    }

                } while (!isStudentIdUnique);
                string studId;
                bool isStudIdUnique = false;

                do
                {
                    Console.Write("Stud ID: ");
                    studId = Console.ReadLine();
                    isStudIdUnique = !context.Student.Any(s => s.stud_id == studId);

                    if (!isStudIdUnique)
                    {
                        Console.WriteLine($"Stud ID {studId} är redan taget. Välj ett annat ID.");
                    }

                } while (!isStudIdUnique);

                Console.Write("Förnamn: ");
                string förnamn = Console.ReadLine();

                Console.Write("Efternamn: ");
                string efternamn = Console.ReadLine();

                Console.Write("Personnummer: ");
                string personnummer = Console.ReadLine();

                Console.Write("Adress: ");
                string adress = Console.ReadLine();

                Console.Write("Klass ID: ");
                if (!int.TryParse(Console.ReadLine(), out int klassId))
                {
                    Console.WriteLine("Ogiltigt klass ID. Avbryter...");
                    return;
                }

                Console.Write("Ålder: ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime ålder))
                {
                    Console.WriteLine("Ogiltig ålder. Avbryter...");
                    return;
                }
                var nyElev = new Student
                {
                    student_id = studentId,
                    stud_id = studId,
                    förnamn = förnamn,
                    efternamn = efternamn,
                    personnummer = personnummer,
                    adress = adress,
                    klass_id = klassId,
                    ålder = ålder
                };
                context.Student.Add(nyElev);
                context.SaveChanges();

                Console.WriteLine("Ny elev tillagd i databasen!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid tillägg av ny elev. Exception: {ex}");
            }
        }
        static void LäggTillNyPersonal(SkolDbContext context)
        {
            try
            {
                Console.WriteLine("Ange information om den nya personalen:");
                string anstId;
                bool isAnstIdUnique = false;
                Console.WriteLine("Lägg till ny personal:");

                int personalId;
                do
                {
                    Console.Write("Ange personal_id: ");
                    if (!int.TryParse(Console.ReadLine(), out personalId))
                    {
                        Console.WriteLine("Ogiltig input för personal_id. Ange en giltig siffra.");
                        continue;
                    }
                    if (context.Personal.Any(p => p.personal_id == personalId))
                    {
                        Console.WriteLine("Personal_id är redan taget. Ange en annan siffra.");
                    }
                    else
                    {
                        break; 
                    }
                } while (true);
                do
                {
                    Console.Write("Anställnings ID: ");
                    anstId = Console.ReadLine();
                    isAnstIdUnique = !context.Personal.Any(p => p.anst_id == anstId);

                    if (!isAnstIdUnique)
                    {
                        Console.WriteLine($"Anställnings ID {anstId} är redan taget. Välj ett annat ID.");
                    }

                } while (!isAnstIdUnique);

                Console.Write("Förnamn: ");
                string förnamn = Console.ReadLine();

                Console.Write("Efternamn: ");
                string efternamn = Console.ReadLine();

                Console.Write("Personnummer: ");
                string personnummer = Console.ReadLine();

                Console.Write("Adress: ");
                string adress = Console.ReadLine();

                Console.Write("Anställningsdatum: ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime anstDatum))
                {
                    Console.WriteLine("Ogiltigt anställningsdatum. Avbryter...");
                    return;
                }

                Console.Write("Befattning: ");
                string befattning = Console.ReadLine();
                var nyPersonal = new Anställd
                {
                    personal_id = personalId,
                    anst_id = anstId,
                    förnamn = förnamn,
                    efternamn = efternamn,
                    personnummer = personnummer,
                    adress = adress,
                    anst_datum = anstDatum,
                    befattning = befattning
                };
                context.Personal.Add(nyPersonal);
                context.SaveChanges();

                Console.WriteLine("Ny personal tillagd i databasen!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fel vid tillägg av ny personal. Exception: {ex}");
            }
        }
    }
    }


  