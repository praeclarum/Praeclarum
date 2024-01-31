#nullable disable

using System;

namespace Praeclarum
{
    public static class NumberFormatting
    {
        static string[] _zeroPrecisionFormats;
        static string[] _precisionFormats;

        static NumberFormatting ()
        {
            var maxP = 9;
            _zeroPrecisionFormats = new string[maxP];
            _precisionFormats = new string[maxP];
            for (var p = 0; p < maxP; p++) {
                _zeroPrecisionFormats[p] = "0." + new string ('0', p);
                _precisionFormats[p] = "0." + new string ('#', p);
            }
        }

        public class ValScale
        {
            public readonly double Scale;
            public readonly string Prefix;
            public readonly string Suffix;
            public ValScale (double s, string p)
            {
                Scale = s;
                Prefix = p;
                Suffix = " " + p;
            }
        }

        public static readonly ValScale[] Scales = new ValScale[] {
            new ValScale (1e18, "E"),
            new ValScale (1e15, "P"),
            new ValScale (1e12, "T"),
            new ValScale (1e9, "G"),
            new ValScale (1e6, "M"),
            new ValScale (1e3, "k"),
            new ValScale (1e0, ""),
            new ValScale (1e-3, "m"),
            new ValScale (1e-6, "µ"),
            new ValScale (1e-6, "u"),
            new ValScale (1e-9, "n"),
            new ValScale (1e-12, "p"),
            new ValScale (1e-15, "f")
        };

        public static string ToUnitsString (this double value, string units = "", int precision = 3, bool showZeroes = false)
        {
            if (double.IsNaN (value))
                return "NaN";
            if (double.IsPositiveInfinity (value))
                return "∞" + (units.Length > 0 ? " " + units : "");
            if (double.IsNegativeInfinity (value))
                return "-∞" + (units.Length > 0 ? " " + units : "");

            if (units == "mm") {
                units = "m";
                value *= 1.0e-3;
            }

            if (units == "s") {
                return ToTimeString (value, precision, showZeroes);
            }

            var suffix = "";

            var neg = value < 0;
            var v = Math.Abs (value);
            if (v < 1e-18) {
                v = 0;
                neg = false;
            }

            for (var i = 0; i < Scales.Length; i++) {
                if (v >= Scales[i].Scale) {
                    v /= Scales[i].Scale;
                    suffix = Scales[i].Suffix;
                    break;
                }
            }

            if (!string.IsNullOrEmpty (units)) {
                if (suffix.Length == 0) {
                    suffix = " " + units;
                }
                else {
                    suffix += units;
                }
            }

            var f = "";
            if (showZeroes) {
                f = _zeroPrecisionFormats[precision];
            }
            else {
                f = _precisionFormats[precision];
            }

            return (neg ? -v : v).ToString (f) + suffix.TrimEnd ();
        }

        public static string ToTimeString (this double value, int precision = 3, bool showZeroes = false)
        {
            var seconds = Math.Abs (value);
            double yearSeconds = 60 * 60 * 60 * 24 * 365;
            if (seconds > yearSeconds) {
                var years = Math.Ceiling (seconds / yearSeconds);
                if (years > 100) {
                    return "∞ s";
                }
                return $"{years} years";
            }
                var ts = TimeSpan.FromSeconds (value);
            if (ts.TotalDays >= 1) {
                return ts.ToString (@"d\.hh\:mm\:ss");
            }
            else if (ts.TotalHours >= 1) {
                return ts.ToString (@"h\:mm\:ss");
            }
            else if (ts.TotalMinutes >= 1) {
                return ts.ToString (@"m\:ss");
            }
            else {
                return ToUnitsString (value, "X", precision, showZeroes).Replace ("X", "s");
            }
        }

        public static string ToShortScaledUnitsString (this double value, double scale, string units)
        {
            if (double.IsNaN (value))
                return "NaN";

            var v = Math.Abs (value);
            var prefix = value < 0 ? "-" : "";
            var r = (v / scale);
            if (r < 1)
                return r.ToString ("0.##") + " " + units;
            if (r < 10)
                return r.ToString ("0.#") + " " + units;
            return prefix + r.ToString ("0") + " " + units;
        }

        public static string ToShortScaledString (this double value, double scale)
        {
            if (double.IsNaN (value))
                return "NaN";

            var v = Math.Abs (value);
            var prefix = value < 0 ? "-" : "";
            var r = (v / scale);
            if (r < 1)
                return r.ToString ("0.##");
            if (r < 10)
                return r.ToString ("0.#");
            return prefix + r.ToString ("0");
        }

        public static string ToShortUnitsString (this double value, string units)
        {
            if (double.IsNaN (value))
                return "NaN";

            var v = Math.Abs (value);
            var prefix = value < 0 ? "-" : "";
            if (v < 1e-12)
                return "0 " + units;
            if (v < 1e-9)
                return prefix + Math.Round (v / 1e-12) + " f" + units;
            if (v < 1e-6)
                return prefix + Math.Round (v / 1e-9) + " n" + units;
            if (v < 1e-3)
                return prefix + Math.Round (v / 1e-6) + " µ" + units;
            if (v < 1)
                return prefix + Math.Round (v / 1e-3) + " m" + units;
            if (v < 1e1)
                return prefix + Math.Round (v / 1, 1) + " " + units;
            if (v < 1e3)
                return prefix + Math.Round (v / 1) + " " + units;
            if (v < 1e4)
                return prefix + Math.Round (v / 1e3, 1) + " k" + units;
            if (v < 1e6)
                return prefix + Math.Round (v / 1e3) + " k" + units;
            if (v < 1e7)
                return prefix + Math.Round (v / 1e6, 1) + " M" + units;
            if (v < 1e9)
                return prefix + Math.Round (v / 1e6) + " M" + units;
            if (v < 1e12)
                return prefix + Math.Round (v / 1e9) + " G" + units;
            return prefix + Math.Round (v / 1e12) + " T" + units;
        }
    }
}

