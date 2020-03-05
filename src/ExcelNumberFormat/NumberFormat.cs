﻿using System;
using System.Collections.Generic;
using System.Globalization;

namespace ExcelNumberFormat
{
    /// <summary>
    /// Parse ECMA-376 number format strings and format values like Excel and other spreadsheet softwares.
    /// </summary>
    public class NumberFormat
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberFormat"/> class.
        /// </summary>
        /// <param name="formatString">The number format string.</param>
        public NumberFormat(string formatString)
        {
            var sections = Parser.ParseSections(formatString, out bool syntaxError);

            IsValid = !syntaxError;
            FormatString = formatString;

            if (IsValid)
            {
                Sections = sections;
                IsDateTimeFormat = Evaluator.GetFirstSection(Sections, SectionType.Date) != null;
                IsTimeSpanFormat = Evaluator.GetFirstSection(Sections, SectionType.Duration) != null;
            }
            else
            {
                Sections = new List<Section>();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the number format string is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the number format string.
        /// </summary>
        public string FormatString { get; }

        /// <summary>
        /// Gets a value indicating whether the format represents a DateTime
        /// </summary>
        public bool IsDateTimeFormat { get; }

        /// <summary>
        /// Gets a value indicating whether the format represents a TimeSpan
        /// </summary>
        public bool IsTimeSpanFormat { get; }

        internal List<Section> Sections { get; }

        /// <summary>
        /// Formats a value with this number format in a specified culture.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="culture">The culture to use for formatting.</param>
        /// <returns>The formatted string.</returns>
        public string Format(object value, CultureInfo culture)
        {
            if (!IsValid || string.IsNullOrEmpty(FormatString))
                CompatibleConvert.ToString(value, culture);

            var section = Evaluator.GetSection(Sections, value);
            if (section == null)
                return CompatibleConvert.ToString(value, culture);

            try
            {
                return Formatter.Format(value, section, culture);
            }
            catch (InvalidCastException)
            {
                // TimeSpan cast exception
                return CompatibleConvert.ToString(value, culture);
            }
            catch (FormatException)
            {
                // Convert.ToDouble/ToDateTime exceptions
                return CompatibleConvert.ToString(value, culture);
            }
        }
    }
}
