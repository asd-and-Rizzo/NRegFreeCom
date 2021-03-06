﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace idl2cs.TypeLib
{
    public class ParameterDesc
    {
        public readonly string Name;
        public readonly bool SpecialFlag;
        public readonly bool Hidden;
        public readonly bool In;
        public readonly bool Out;
        public readonly bool Optional;
        public readonly bool RetVal;
        public readonly int IndirectionLevel;
        public readonly bool HasValue;
        public readonly object Value;
        public readonly IReadOnlyList<int> ArraySizes;

        private readonly uint typeReference;

        private readonly Func<uint, TypeDesc> typeFactory;

        private TypeDesc type;
        public TypeDesc Type
        {
            get
            {
                if (type == null)
                    type = typeFactory(typeReference);
                return type;
            }
        }

        public string TypeDeclaration
        {
            get
            {
                int indirectionLevel = IndirectionLevel;

                if (Type.Kind == TypeKind.Interface)
                    indirectionLevel--;

                string fieldType = ReplaceEmbeddedTypeName(Type.Name);
                for (int j = 0; j < indirectionLevel; j++)
                    fieldType += "*";

                return fieldType;
            }
        }

        public bool AutoGeneratedName
        {
            get
            {
                return Name.StartsWith("__MIDL__");
            }
        }

        public unsafe ParameterDesc(ELEMDESC elemdesc, string name, Func<uint, TypeDesc> typeFactory, VARFLAGS flags, bool hasValue, object value)
        {
            Name = EscapeParameterName(name);
            SpecialFlag = flags.HasFlag(VARFLAGS.VARFLAG_FREPLACEABLE);
            Hidden = flags.HasFlag(VARFLAGS.VARFLAG_FHIDDEN);
            In = elemdesc.__union1.paramdesc.wParamFlags.HasFlag(PARAMFLAGS.PARAMFLAG_FIN);
            Out = elemdesc.__union1.paramdesc.wParamFlags.HasFlag(PARAMFLAGS.PARAMFLAG_FOUT);
            Optional = elemdesc.__union1.paramdesc.wParamFlags.HasFlag(PARAMFLAGS.PARAMFLAG_FOPT);
            RetVal = elemdesc.__union1.paramdesc.wParamFlags.HasFlag(PARAMFLAGS.PARAMFLAG_FRETVAL);
            HasValue = hasValue;
            Value = value;
            
            this.typeFactory = typeFactory;

            TYPEDESC typedesc = elemdesc.tdesc;
            while ((typedesc.vt & VARENUM.VT_TYPEMASK) == VARENUM.VT_PTR)
            {
                if ((typedesc.vt & ~VARENUM.VT_TYPEMASK) != VARENUM.VT_EMPTY)
                    throw new Exception("Variant type " + typedesc.vt + " is not supported");

                IndirectionLevel++;
                typedesc = *typedesc.__union1.lptdesc;
            }

            List<int> arraySizes = new List<int>();
            if ((typedesc.vt & VARENUM.VT_TYPEMASK) == VARENUM.VT_CARRAY)
            {
                SAFEARRAYBOUND* boundsPtr = &typedesc.__union1.lpadesc->rgbounds;
                for (int i = 0; i < typedesc.__union1.lpadesc->cDims; i++)
                    arraySizes.Add((int)boundsPtr[i].cElements - boundsPtr[i].lBound);
                typedesc = typedesc.__union1.lpadesc->tdescElem;
            }
            ArraySizes = arraySizes.AsReadOnly();

            if ((typedesc.vt & ~VARENUM.VT_TYPEMASK) != VARENUM.VT_EMPTY)
                throw new Exception("Variant type " + typedesc.vt + " is not supported");
            
            if (elemdesc.__union1.paramdesc.wParamFlags.HasFlag(PARAMFLAGS.PARAMFLAG_FHASDEFAULT))
            {
                if (hasValue)
                    throw new Exception("Explicit value cannot be set if default value is present");
                if (elemdesc.__union1.paramdesc.pparamdescex == null)
                    throw new Exception("Value address is null");
                Value = Marshal.GetObjectForNativeVariant(new IntPtr(&elemdesc.__union1.paramdesc.pparamdescex->varDefaultValue));
                HasValue = true;
            }

            switch (typedesc.vt & VARENUM.VT_TYPEMASK)
            {
                case VARENUM.VT_VARIANT:
                    type = new TypeDesc("VARIANT", TypeKind.Struct);
                    break;
                case VARENUM.VT_SAFEARRAY:
                    type = new TypeDesc("SAFEARRAY", TypeKind.Struct);
                    break;
                case VARENUM.VT_USERDEFINED:
                    typeReference = typedesc.__union1.hreftype;
                    break;
                case VARENUM.VT_UNKNOWN:
                    type = new TypeDesc("IUnknown", TypeKind.Interface);
                    IndirectionLevel++;
                    break;
                case VARENUM.VT_DISPATCH:
                    type = new TypeDesc("IDispatch", TypeKind.Interface);
                    IndirectionLevel++;
                    break;
                case VARENUM.VT_ERROR:
                case VARENUM.VT_HRESULT:
                    type = new TypeDesc("HRESULT");
                    break;
                case VARENUM.VT_LPSTR:
                    type = new TypeDesc("sbyte");
                    IndirectionLevel++;
                    break;
                case VARENUM.VT_BSTR:
                case VARENUM.VT_LPWSTR:
                    type = new TypeDesc("char");
                    IndirectionLevel++;
                    break;
                case VARENUM.VT_NULL:
                    type = new TypeDesc("void");
                    break;
                case VARENUM.VT_BOOL:
                    type = new TypeDesc("bool");
                    break;
                case VARENUM.VT_I1:
                    type = new TypeDesc("sbyte");
                    break;
                case VARENUM.VT_I2:
                    type = new TypeDesc("short");
                    break;
                case VARENUM.VT_I4:
                case VARENUM.VT_INT:
                    type = new TypeDesc("int");
                    break;
                case VARENUM.VT_I8:
                case VARENUM.VT_FILETIME:
                case VARENUM.VT_CY:
                    type = new TypeDesc("long");
                    break;
                case VARENUM.VT_UI1:
                    type = new TypeDesc("byte");
                    break;
                case VARENUM.VT_UI2:
                    type = new TypeDesc("ushort");
                    break;
                case VARENUM.VT_UI4:
                case VARENUM.VT_UINT:
                    type = new TypeDesc("uint");
                    break;
                case VARENUM.VT_UI8:
                    type = new TypeDesc("ulong");
                    break;
                case VARENUM.VT_R4:
                    type = new TypeDesc("float");
                    break;
                case VARENUM.VT_R8:
                case VARENUM.VT_DATE:
                    type = new TypeDesc("double");
                    break;
                case VARENUM.VT_DECIMAL:
                    type = new TypeDesc("decimal");
                    break;
                case VARENUM.VT_VOID:
                    type = new TypeDesc("void");
                    break;
                case VARENUM.VT_INT_PTR:
                case VARENUM.VT_UINT_PTR:
                    type = new TypeDesc("void");
                    IndirectionLevel++;
                    break;
                default:
                    throw new Exception("Variant type " + typedesc.vt + " is not supported");
            }
        }

        private static string EscapeParameterName(string name)
        {
            switch (name)
            {
                case "abstract":
                case "as":
                case "base":
                case "break":
                case "case":
                case "catch":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "default":
                case "delegate":
                case "do":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "namespace":
                case "new":
                case "null":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sealed":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "unchecked":
                case "unsafe":
                case "using":
                case "virtual":
                case "volatile":
                case "while":
                case "add":
                case "alias":
                case "ascending":
                case "async":
                case "await":
                case "descending":
                case "dynamic":
                case "from":
                case "get":
                case "global":
                case "group":
                case "into":
                case "join":
                case "let":
                case "orderby":
                case "partial":
                case "remove":
                case "select":
                case "set":
                case "value":
                case "var":
                case "where":
                case "yield ":

                case "bool":
                case "byte":
                case "char":
                case "decimal":
                case "double":
                case "float":
                case "int":
                case "long":
                case "ushort":
                case "ulong":
                case "uint":
                case "string":
                case "short":
                case "object":
                case "sbyte":
                case "void":
                    return "@" + name;
                default:
                    return name;
            }
        }

        private static string ReplaceEmbeddedTypeName(string typeName)
        {
            switch (typeName)
            {
                case "HRESULT":
                    return "int";
                default:
                    return typeName;
            }
        }
    }
}
