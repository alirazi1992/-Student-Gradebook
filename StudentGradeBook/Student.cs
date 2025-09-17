using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudentGradebook
{
    public class Student
    {
        public string Name { get; }
        public List<double> Grades { get; } = new();

        public Student(string name) => Name = name;

        public double? Average => Grades.Count == 0 ? null : Grades.Average();
        public double? Highest => Grades.Count == 0 ? null : Grades.Max();
        public double? Lowest => Grades.Count == 0 ? null : Grades.Min();

        // CSV escaping for fields with commas/quotes/newlines
        public static string Escape(string s)
        {
            if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            {
                s = s.Replace("\"", "\"\"");
                return $"\"{s}\"";
            }
            return s;
        }
    }
}
