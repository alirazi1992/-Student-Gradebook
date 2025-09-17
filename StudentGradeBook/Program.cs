using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace StudentGradebook
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var gradebook = new Gradebook();

            const string gradebookFile = "gradebook.csv";
            const string summaryFile = "summary.csv";

            while (true)
            {
                Console.WriteLine("\n📊 Student Gradebook");
                Console.WriteLine("1) Add student");
                Console.WriteLine("2) Add grade to student");
                Console.WriteLine("3) List students & grades");
                Console.WriteLine("4) Show statistics (avg / high / low)");
                Console.WriteLine("5) Save to CSV");
                Console.WriteLine("6) Load from CSV");
                Console.WriteLine("7) Export summary CSV");
                Console.WriteLine("0) Exit");
                Console.Write("Choose: ");

                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        AddStudent(gradebook);
                        break;
                    case "2":
                        AddGrade(gradebook);
                        break;
                    case "3":
                        ListStudents(gradebook);
                        break;
                    case "4":
                        ShowStats(gradebook);
                        break;
                    case "5":
                        gradebook.SaveCsv(gradebookFile);
                        Console.WriteLine($"✅ Saved: {gradebookFile}");
                        break;
                    case "6":
                        if (File.Exists(gradebookFile))
                        {
                            gradebook.LoadCsv(gradebookFile);
                            Console.WriteLine($"✅ Loaded: {gradebookFile}");
                        }
                        else Console.WriteLine("⚠️ gradebook.csv not found.");
                        break;
                    case "7":
                        gradebook.ExportSummaryCsv(summaryFile);
                        Console.WriteLine($"✅ Summary exported: {summaryFile}");
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("❓ Invalid option.");
                        break;
                }
            }
        }

        static void AddStudent(Gradebook gb)
        {
            Console.Write("Enter student name: ");
            var name = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("⚠️ Name cannot be empty.");
                return;
            }
            if (gb.TryGetStudent(name, out _))
            {
                Console.WriteLine("⚠️ Student already exists.");
                return;
            }
            gb.AddStudent(new Student(name));
            Console.WriteLine($"✅ Added student: {name}");
        }

        static void AddGrade(Gradebook gb)
        {
            if (gb.Count == 0)
            {
                Console.WriteLine("⚠️ No students yet. Add a student first.");
                return;
            }

            Console.Write("Student name: ");
            var name = Console.ReadLine()?.Trim();
            if (!gb.TryGetStudent(name ?? "", out var student))
            {
                Console.WriteLine("⚠️ Student not found.");
                return;
            }

            Console.Write("Enter grade (0–100): ");
            var raw = Console.ReadLine();
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var grade) ||
                grade < 0 || grade > 100)
            {
                Console.WriteLine("⚠️ Invalid grade.");
                return;
            }

            student!.Grades.Add(grade);
            Console.WriteLine($"✅ Added grade {grade:0.##} to {student.Name}");
        }

        static void ListStudents(Gradebook gb)
        {
            if (gb.Count == 0)
            {
                Console.WriteLine("No students yet.");
                return;
            }

            foreach (var s in gb.Students.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
            {
                var grades = s.Grades.Count == 0
                    ? "—"
                    : string.Join(", ", s.Grades.Select(g => g.ToString("0.##", CultureInfo.InvariantCulture)));
                Console.WriteLine($"👤 {s.Name}  |  Grades: {grades}");
            }
        }

        static void ShowStats(Gradebook gb)
        {
            if (gb.TotalGrades == 0)
            {
                Console.WriteLine("No grades yet.");
                return;
            }

            var avg = gb.GlobalAverage;
            var (hi, hiName) = gb.HighestGrade();
            var (lo, loName) = gb.LowestGrade();

            Console.WriteLine($"📈 Average (all grades): {avg:0.##}");
            Console.WriteLine($"🏆 Highest: {hi:0.##}  (by {hiName})");
            Console.WriteLine($"🔻 Lowest : {lo:0.##}  (by {loName})");
        }
    }
}
