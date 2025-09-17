using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace StudentGradebook
{
    public class Gradebook
    {
        public List<Student> Students { get; } = new();
        public int Count => Students.Count;
        public int TotalGrades => Students.Sum(s => s.Grades.Count);

        public void AddStudent(Student s) => Students.Add(s);

        public bool TryGetStudent(string name, out Student? student)
        {
            student = Students.FirstOrDefault(s =>
                string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            return student is not null;
        }

        // -------- Stats --------
        public double GlobalAverage =>
            TotalGrades == 0 ? 0 : Students.SelectMany(s => s.Grades).Average();

        public (double grade, string name) HighestGrade()
        {
            var best = Students
                .SelectMany(s => s.Grades.Select(g => (g, s.Name)))
                .OrderByDescending(x => x.g)
                .First();
            return (best.g, best.Name);
        }

        public (double grade, string name) LowestGrade()
        {
            var worst = Students
                .SelectMany(s => s.Grades.Select(g => (g, s.Name)))
                .OrderBy(x => x.g)
                .First();
            return (worst.g, worst.Name);
        }

        // -------- CSV I/O --------
        // gradebook.csv format:
        // Name,Grades
        // Alice,90|85.5|100
        public void SaveCsv(string path)
        {
            using var sw = new StreamWriter(path, false, Encoding.UTF8);
            sw.WriteLine("Name,Grades");
            foreach (var s in Students)
            {
                var gradesJoined = string.Join('|',
                    s.Grades.Select(g => g.ToString("0.##", CultureInfo.InvariantCulture)));
                var nameEsc = Student.Escape(s.Name);
                var gradesEsc = Student.Escape(gradesJoined);
                sw.WriteLine($"{nameEsc},{gradesEsc}");
            }
        }

        public void LoadCsv(string path)
        {
            Students.Clear();
            using var sr = new StreamReader(path, Encoding.UTF8);
            string? line;
            bool headerSkipped = false;

            while ((line = sr.ReadLine()) is not null)
            {
                if (!headerSkipped) { headerSkipped = true; continue; } // skip header
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = SplitCsvLine(line);
                if (parts.Length < 2) continue;

                var name = parts[0].Trim();
                var gradesStr = parts[1].Trim();

                var student = new Student(name);

                if (!string.IsNullOrEmpty(gradesStr))
                {
                    foreach (var token in gradesStr.Split('|', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var g))
                            student.Grades.Add(g);
                    }
                }

                Students.Add(student);
            }
        }

        // CSV split for 2 columns with quotes support
        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var cur = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        cur.Append('"'); // escaped quote
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(cur.ToString());
                    cur.Clear();
                }
                else
                {
                    cur.Append(c);
                }
            }
            result.Add(cur.ToString());
            return result.ToArray();
        }

        // -------- Summary export --------
        // summary.csv columns:
        // Name,Count,Average,Highest,Lowest
        // plus a final GLOBAL row.
        public void ExportSummaryCsv(string path)
        {
            using var sw = new StreamWriter(path, false, Encoding.UTF8);
            sw.WriteLine("Name,Count,Average,Highest,Lowest");

            foreach (var s in Students.OrderBy(s => s.Name, StringComparer.OrdinalIgnoreCase))
            {
                var count = s.Grades.Count;
                var avg = s.Average?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";
                var highest = s.Highest?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";
                var lowest = s.Lowest?.ToString("0.##", CultureInfo.InvariantCulture) ?? "";

                sw.WriteLine($"{Student.Escape(s.Name)},{count},{avg},{highest},{lowest}");
            }

            if (TotalGrades > 0)
            {
                var (hi, hiName) = HighestGrade();
                var (lo, loName) = LowestGrade();
                sw.WriteLine();
                sw.WriteLine($"GLOBAL (all grades),{TotalGrades},{GlobalAverage.ToString("0.##", CultureInfo.InvariantCulture)},{hi.ToString("0.##", CultureInfo.InvariantCulture)} by {Student.Escape(hiName)},{lo.ToString("0.##", CultureInfo.InvariantCulture)} by {Student.Escape(loName)}");
            }
        }
    }
}
