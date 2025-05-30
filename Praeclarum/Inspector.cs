#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using NGraphics;

#if __IOS__
using SceneKit;
using UIKit;
using SCNFloat = System.Single;
#elif __MACOS__
using SceneKit;
using UIKit;
#if NET6_0_OR_GREATER
using SCNFloat = System.Runtime.InteropServices.NFloat;
#else
using SCNFloat = System.nfloat;
#endif
#endif

namespace Praeclarum.Inspector
{
    public class InspectorAttribute : Attribute
    {
        public bool HideName { get; set; }
        public int Order { get; set; } = 500;
        public bool IsIntegral { get; set; }
        public bool InlineList { get; set; }
        public string? Units { get; set; }
        public double UnitsScale { get; set; } = 1;
        public bool HasUnits => !string.IsNullOrEmpty (Units);
        public double Min { get; set; }
        public double Max { get; set; }
        public bool IsColor { get; set; }
        public bool IsCode { get; set; }
    }

    public interface INumericValueProvider
    {
        double DoubleValue { get; }
        string ValueUnits { get; }
    }

    public class InspectorIgnoreAttribute : Attribute
    {
    }

    public class EditContextBase
    {
        #if __IOS__ || __MACOS__
        public virtual Foundation.NSUndoManager? UndoManager => null;
        #endif
        
        public virtual object? EvalExpression (string exp)
        {
	        if (Double.TryParse (exp, out var d))
	        {
		        return d;
	        }
	        return null;
        }
    }

    public interface IHasEditInfos
    {
        IEnumerable<EditInfo> GetEditInfos (EditContextBase context);
        void SetEditValue (string name, object? value);
    }

    public class EditInfo
    {
        public EditContextBase Context { get; }

        #if __IOS__ || __MACOS__
        public Foundation.NSUndoManager? UndoManager => Context.UndoManager;
        #endif

        public readonly InspectorAttribute Attributes;

        public MemberInfo? Member;
        public int MemberDepth;
        public int SortOrder => Attributes.Order;

        public Func<object?>? Getter;
        public Action<object?, object?, bool>? Setter;
        public Action? Executor;

        public object[] Choices;

        bool firstEdit = true;

        public object? Target { get; }
        public string EnglishName { get; }
        public string LocalizedName { get; }
        public Type ValueType { get; set; }

        object? val;
        public object? Value {
            get => val;
            set {
                if (val == null && value == null)
                    return;
                if (val != null && val.Equals (value))
                    return;
                var oldVal = val;
                val = value;
                Setter?.Invoke (val, oldVal, firstEdit);
                firstEdit = false;
                try {
                    ValueUpdated?.Invoke (this, EventArgs.Empty);
                }
                catch (Exception ex) {
                    Log.QuietError ("Failed to respond to set value", ex);
                }
            }
        }

        public event EventHandler? ValueUpdated;

        public EditInfo (EditContextBase context, object? target, string englishName, object? initialValue, Type valueType, InspectorAttribute attributes)
        {
            Context = context;
            Target = target;
            val = initialValue;
            ValueType = valueType;
            EnglishName = englishName;
            LocalizedName = englishName.Localize ();
            Attributes = attributes;
            Choices = Array.Empty<object> ();
        }

        public override string ToString () => $"{EnglishName} = {DisplayValueString}";

        public void UpdateValue ()
        {
            if (Getter == null)
                return;

            try {
                var nval = Getter ();
                if (nval == null)
                    return;
                if (nval.Equals (val))
                    return;
                val = nval;
                ValueUpdated?.Invoke (this, EventArgs.Empty);
            }
            catch (Exception ex) {
                Log.QuietError ("Failed to get value", ex);
            }
        }

        public void BeginEdit ()
        {
	        #if __IOS__ || __MACOS__
	        UndoManager?.BeginUndoGrouping ();
	        #endif
        }

        public void EndEdit ()
        {
			#if __IOS__ || __MACOS__
	        UndoManager?.EndUndoGrouping ();
	        #endif
        }

        public bool IsCommand => Executor != null;

        public bool IsColor =>
	        #if __IOS__ || __MACOS__
            (Value == null && (ValueType == typeof (UIColor) || ValueType == typeof (Color)))
            || Value is UIColor
            || (Attributes.IsColor && (Value is SCNVector3 || Value is SCNVector4)) ||
            #endif
            Value is Color;

        public bool IsBool => Value is bool;

        public bool IsNumeric {
            get {
                return Value is double || Value is float || Value is int || Value is long || Value is byte || Value is INumericValueProvider;
            }
        }

        public bool IsIntegral => Attributes.IsIntegral;

        public bool IsCode => Attributes.IsCode;

        public bool HasChoices => Choices.Length > 0;

        public int IntegralValue => (int)Math.Round (DoubleValue);

        public bool BoolValue {
            get {
                try {
                    return GetBoolValue (Value);
                }
                catch (Exception) {
                    return false;
                }
            }
            set {
                Value = value;
            }
        }

        public static bool GetBoolValue (object? value)
        {
            return value switch
            {
                null => false,
                bool b => b,
                var _ => GetDoubleValue (value) != 0.0,
            };
        }

        public double DoubleValue {
            get {
                try {
                    return GetDoubleValue (Value);
                }
                catch (Exception) {
                    return 0;
                }
            }
            set {
                try {
                    var eval = value;
                    if (Attributes.Min != 0.0 || Attributes.Max != 0.0) {
                        eval = Math.Max (Attributes.Min, eval);
                        eval = Math.Min (Attributes.Max, eval);
                    }
                    switch (Value) {
                        case null:
                        case double d:
                            Value = eval;
                            break;
                        case int i:
                            Value = (int)(eval + 0.5);
                            break;
                        default:
                            Value = Convert.ChangeType (eval, Value.GetType ());
                            break;
                    }
                }
                catch (Exception ex) {
                    Log.Error ("Failed to set editable double ", ex);
                }
            }
        }

        public static double GetDoubleValue (object? value)
        {
            return value switch
            {
                null => 0.0,
                double d => d,
                INumericValueProvider p => p.DoubleValue,
                int i => i,
                long i => i,
                byte i => i,
                float f => f,
                var v => Convert.ToDouble (v),
            };
        }

        public static readonly string NullString = "None".Localize ();

        #if __IOS__ || __MACOS__
	    public bool IsVector => Value is SCNVector3;
        public SCNVector3 VectorValue {
            get {
                try {
                    return Value switch
                    {
                        SCNVector3 v3 => v3,
                        _ => SCNVector3.Zero,
                    };
                }
                catch (Exception) {
                    return SCNVector3.Zero;
                }
            }
            set {
                try {
                    switch (Value) {
                        case SCNVector3 v3:
                            Value = value;
                            break;
                    }
                }
                catch (Exception ex) {
                    Log.Error ("Failed to set editable color", ex);
                }
            }
        }
        #endif

#if __IOS__ || __MACOS__ || __MACCATALYST__
        static Color GetNColor (UIKit.UIColor color)
        {
            if (color.CGColor is {} cgc) {
                return GetNColor (cgc);
            }
            return Colors.Black;
        }
        static Color GetNColor (CoreGraphics.CGColor color)
        {
            var comps = color.Components;
            if (color.ColorSpace?.Components >= 3) {
                var a = comps.Length > 3 ? comps[3] : (nfloat)1.0;
                return new Color (comps[0], comps[1], comps[2], a);
            }
            else {
                var a = comps.Length > 1 ? comps[1] : (nfloat)1.0;
                var w = comps[0];
                return new Color (w, w, w, a);
            }
        }
#endif

        public Color ColorValue {
            get {
                try {
                    return Value switch
                    {
                        Color ngc => ngc,
                        #if __IOS__ || __MACOS__ || __MACCATALYST__
                        System.Numerics.Vector3 v3 => new Color(v3.X, v3.Y, v3.Z),
                        System.Numerics.Vector4 v4 => new Color(v4.X, v4.Y, v4.Z, v4.W),
                        UIColor uic => GetNColor (uic),
                        #endif
                        _ => Colors.Black,
                    };
                }
                catch (Exception) {
                    return Colors.Black;
                }
            }
            set {
                try {
                    switch (Value) {
                        case Color ngc:
                            Value = value;
                            break;
                        #if __IOS__ || __MACOS__ || __MACCATALYST__
                        case null when ValueType == typeof (UIColor):
	                        Value = UIColor.FromRGBA ((float)value.Red, (float)value.Green, (float)value.Blue, (float)value.Alpha);
	                        break;
                        case System.Numerics.Vector3 v3:
                            Value = new System.Numerics.Vector3 ((float)value.Red, (float)value.Green, (float)value.Blue);
                            break;
                        case System.Numerics.Vector4 v4:
                            Value = new System.Numerics.Vector4 ((float)value.Red, (float)value.Green, (float)value.Blue, (float)value.Alpha);
                            break;
                        case UIColor uic:
                            Value = UIColor.FromRGBA ((float)value.Red, (float)value.Green, (float)value.Blue, (float)value.Alpha);
                            break;
                        #endif
                    }
                }
                catch (Exception ex) {
                    Log.Error ("Failed to set editable color", ex);
                }
            }
        }
        
        public string DisplayValueString {
            get {
                try {
                    if (IsNumeric) {
                        var units = (Value as INumericValueProvider)?.ValueUnits ?? Attributes.Units;
                        if (string.IsNullOrEmpty (units)) {
                            return DoubleValue.ToUnitsString ("");
                        }
                        else {
                            if (Math.Abs (Attributes.UnitsScale - 1) < 1e-8) {
                                return DoubleValue.ToUnitsString (units);
                            }
                            else {
                                return DoubleValue.ToShortScaledUnitsString (Attributes.UnitsScale, units);
                            }
                        }
                    }
                    else if (IsColor) {
                        if (Value == null) {
                            return NullString;
                        }
                        else {
                            var c = ColorValue;
                            return string.Format ("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
                        }
                    }
                    else if (IsCode) {
                        var code = Value?.ToString () ?? "";
                        var lines = code.Split (new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var rlines = lines
                            .Where (x => x.Length > 0 && !char.IsWhiteSpace (x[0]))
                            .Select (CleanCodeLineForDisplay)
                            .Where (x => !string.IsNullOrEmpty (x))
                            .ToList ();
                        var maxlines = 4;
                        var scode = string.Join ("\n", rlines.Take (maxlines));
                        if (rlines.Count > maxlines)
                            scode += "...";
                        return scode;
                    }
                    #if __IOS__ || __MACOS__
                    else if (IsVector) {
                        var v = VectorValue;
                        return $"[{((double)v.X).ToUnitsString ("")}, {((double)v.Y).ToUnitsString ("")}, {((double)v.Z).ToUnitsString ("")}]";
                    }
                    #endif
                    else {
                        return Value?.ToString () ?? "";
                    }
                }
                catch (Exception ex) {
                    return "Error: " + ex.Message;
                }
            }
        }
        
        public string EditableValueString {
            get {
                if (IsNumeric) {
                    return DoubleValue.ToUnitsString ("");
                }
                else if (IsCode) {
                    return Value?.ToString () ?? "";
                }
                else {
                    return DisplayValueString;
                }
            }
            set {
                var safeValue = value ?? "";
                try {
                    var trim = safeValue.Trim ();
                    if (IsNumeric) {
                        if (trim.Length == 0)
                            return;
                        var exp = Context.EvalExpression (trim);
                        if (exp is double eval) {
                            DoubleValue = eval;
                        }
                    }
                    else if (IsColor) {
                        switch (trim.ToLowerInvariant ()) {
                            case "no":
                            case "none":
                                Value = null;
                                break;
                            default:
                                if (trim.Length > 0 && trim[0] != '#')
                                    trim = "#" + trim;
                                if (trim.Length >= 4 && trim[0] == '#') {
                                    if (trim.Length == 4) {
                                        var rn = ParseHexNibble (trim, 1);
                                        var gn = ParseHexNibble (trim, 2);
                                        var bn = ParseHexNibble (trim, 3);
                                        var r = rn * 16 + rn;
                                        var g = gn * 16 + gn;
                                        var b = bn * 16 + bn;
                                        ColorValue = new Color { R = (byte)r, G = (byte)g, B = (byte)b, A = 255 };
                                    }
                                    else {
                                        var i = 1;
                                        byte r = 0, g = 0, b = 0;
                                        while (i + 1 < trim.Length) {
                                            var h = ParseHexByte (trim, i);
                                            if (i == 1)
                                                r = h;
                                            else if (i == 3)
                                                g = h;
                                            else if (i == 5)
                                                b = h;
                                            i += 2;
                                        }
                                        ColorValue = new Color { R = r, G = g, B = b, A = 255 };
                                    }
                                }
                                break;
                        }
                    }
                    #if __IOS__ || __MACOS__
                    else if (IsVector) {
                        var exp = Context.EvalExpression (trim);
                        if (exp is double[] ae) {
                            var r = SCNVector3.Zero;
                            if (ae.Length > 0)
                                r.X = (float)ae[0];
                            if (ae.Length > 1)
                                r.Y = (float)ae[1];
                            if (ae.Length > 2)
                                r.Z = (float)ae[2];
                            VectorValue = r;
                        }
                    }
                    #endif
                    else if (Value != null) {
                        if (TryChangeType (safeValue, Value.GetType (), out var tvalue)) {
                            Value = tvalue;
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error ("Failed to set editable value from string", ex);
                }
            }
        }

        static bool TryChangeType (object value, Type newType, out object result)
        {
            var vt = value.GetType ();
            if (vt == newType) {
                result = value;
                return true;
            }
            if (newType.IsEnum && vt == typeof (string)) {
                if (Enum.TryParse (newType, value.ToString (), out var enumVal)) {
                    result = enumVal;
                    return true;
                }
            }
            if (newType == typeof (bool) && vt == typeof (string)) {
                var input = value.ToString ()?.Trim ().ToLowerInvariant ();
                result = (input == "true" || input == "yes");
                return true;
            }
            try {
                result = Convert.ChangeType (value, newType);
                return true;
            }
            catch (InvalidCastException) {
                result = value;
                return false;
            }
        }

        static string CleanCodeLineForDisplay (string codeLine)
        {
            if (codeLine == "}") return "";
            if (codeLine[^1] == '{') return codeLine[0..^1];
            return codeLine;
        }

        int ParseHexNibble (string s, int index)
        {
            var c = char.ToLowerInvariant (s[index]);
            if ('0' <= c && c <= '9')
                return c - '0';
            if ('a' <= c && c <= 'f')
                return c - 'a' + 10;
            return 0;
        }

        byte ParseHexByte (string s, int index)
        {
            var b = ParseHexNibble (s, index);
            var l = ParseHexNibble (s, index + 1);
            return (byte)(b * 16 + l);
        }

        public static EditInfo[] GetEditInfos (object target, EditContextBase context)
        {
            var infos = new List<EditInfo> ();

            if (target is IHasEditInfos hei) {
                infos.AddRange (hei.GetEditInfos (context));
            }

            var type = target.GetType ();
            var members =
                type.GetMembers (BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance)
                    .Where (x => x.MemberType != MemberTypes.Constructor && x.MemberType != MemberTypes.Event);

            foreach (var m in members) {
                infos.AddRange (GetEditInfosForMember (target, m, context));
            }

            return infos.OrderBy (x => x.SortOrder).ToArray ();
        }

        public static IEnumerable<EditInfo> GetEditInfosForMember (object? target, MemberInfo m, EditContextBase context)
        {
            if (m.Name.StartsWith ("ShouldInspect", StringComparison.Ordinal) ||
                m.Name.StartsWith ("ShouldSerialize", StringComparison.Ordinal))
                yield break;

            var shouldProp = (target != null ? target.GetType () : m.DeclaringType)?.GetMethod ("ShouldInspect" + m.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            //Console.WriteLine ($"? {m.Name} {shouldProp}");
            if (shouldProp is not null && (bool)(shouldProp.Invoke (target, new object[0]) ?? false) == false)
                yield break;

            var ignore = m.GetCustomAttribute<InspectorIgnoreAttribute> ();
            if (ignore != null)
                yield break;

            var attrs = m.GetCustomAttribute<InspectorAttribute> ();
            if (attrs == null) {
                attrs = new InspectorAttribute ();
            }

            if (m is MethodInfo method) {
                if (method.IsSpecialName) yield break;
                var parameters = method.GetParameters ();
                if (parameters.Length > 1) yield break;
                if (parameters.Length > 0 && parameters[0].ParameterType != typeof (EditContextBase)) yield break;
                if (method.ReturnType != typeof (void)) yield break;

                var baseMethod = method;
                var baseBaseMethod = baseMethod.GetBaseDefinition ();
                while (baseBaseMethod != null && baseBaseMethod != baseMethod) {
                    baseMethod = baseBaseMethod;
                    baseBaseMethod = baseMethod.GetBaseDefinition ();
                }
                if (baseMethod.DeclaringType == typeof (object))
                    yield break;

                var depth = 0;
                var baseMethodType = target?.GetType () ?? baseMethod.DeclaringType;
                while (baseMethodType is not null && baseMethodType != baseMethod.DeclaringType && baseMethodType != typeof (object)) {
                    depth++;
                    baseMethodType = baseMethodType.BaseType;
                }

                var editInfo = new EditInfo (context, target, method.Name, method, method.ReturnType, attrs) {
                    MemberDepth = depth,
                    Member = method,
                    Executor = () => {
                        try {
                            if (parameters.Length == 0)
                                method.Invoke (target, Array.Empty<object> ());
                            else
                                method.Invoke (target, new object[] { context });
                        }
                        catch (Exception ex) {
                            Log.Error ("Failed to execute method: " + method, ex);
                        }
                    }
                };
                yield return editInfo;
            }
            else if (m is PropertyInfo property) {

                var getter = property.GetMethod;
                if (getter == null || getter.GetParameters ().Length > 0)
                    yield break;

                var setter = property.SetMethod;
                if (setter != null && !setter.IsPublic)
                    setter = null;
                var valueType = property.PropertyType;

                var baseMethod = getter;
                var baseBaseMethod = baseMethod.GetBaseDefinition ();
                while (baseBaseMethod != null && baseBaseMethod != baseMethod) {
                    baseMethod = baseBaseMethod;
                    baseBaseMethod = baseMethod.GetBaseDefinition ();
                }

                var depth = 0;
                var baseMethodType = target?.GetType () ?? baseMethod.DeclaringType;
                while (baseMethodType is not null && baseMethodType != baseMethod.DeclaringType && baseMethodType != typeof (object)) {
                    depth++;
                    baseMethodType = baseMethodType.BaseType;
                }

                object? value = null;
                Exception? vex = null;
                try {
                    value = getter.Invoke (target, new object[0]);
                }
                catch (Exception ex) {
                    vex = ex;
                }

                if (vex == null) {

                    if (attrs.InlineList && value is System.Collections.IList list) {
                        var items = new List<object> ();
                        try {
                            items.AddRange (list.Cast<object> ());
                        }
                        catch (Exception ex) {
                            Log.QuietError ("Failed to get list items", ex);
                        }
                        foreach (var i in items) {
                            var name = "";
                            var lname = name;
                            var itype = i.GetType ();
                            var namep = itype.GetProperty ("Name");
                            if (namep != null) {
                                try {
                                    name = namep.GetValue (i)?.ToString () ?? name;
                                    lname = name;
                                }
                                catch (Exception ex) {
                                    Log.QuietError ("Failed to get list item name", ex);
                                }
                            }
                            if (name.Length == 0) {
                                name = property.Name;
                                lname = name.Localize ();
                            }
                            var editInfo = new EditInfo (context, target, name, i, itype, attrs) {
                                MemberDepth = depth,
                                Member = property,
                            };

                            yield return editInfo;
                        }
                    }
                    else {
                        var editInfo = new EditInfo (context, target, m.Name, value, valueType, attrs) {
                            Getter = () => getter.Invoke (target, Array.Empty<object> ()),
                            MemberDepth = depth,
                            Member = property,
                        };
                        if (valueType.IsEnum) {
                            editInfo.Choices = Enum.GetValues (valueType).Cast<object> ().OrderBy(x => x.ToString ()).ToArray ();
                        }

                        if (setter != null) {
                            editInfo.Setter = (x, _, firstTime) => {
                                try {
                                    var ptype = setter.GetParameters ()[0].ParameterType;
                                    var arg = Convert.ChangeType (x, ptype);
                                    setter.Invoke (target, new object?[] { arg });
                                    if (firstTime) {
                                        Log.Analytics ("Set Property", ("Type", (target?.GetType () ?? setter.DeclaringType)?.Name), ("Property", property.Name));
                                    }
                                }
                                catch (Exception ex) {
                                    Log.QuietError ("Failed to set value: " + x, ex);
                                }
                            };
                        }
                        yield return editInfo;
                    }
                }
            }
        }
    }
}
