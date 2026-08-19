// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Globalization;
    using System.Reflection;

    public class Null
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Null));
        public static short NullShort
        {
            get
            {
                return -1;
            }
        }

        public static int NullInteger
        {
            get
            {
                return -1;
            }
        }

        public static byte NullByte
        {
            get
            {
                return 255;
            }
        }

        public static float NullSingle
        {
            get
            {
                return float.MinValue;
            }
        }

        public static double NullDouble
        {
            get
            {
                return double.MinValue;
            }
        }

        public static decimal NullDecimal
        {
            get
            {
                return decimal.MinValue;
            }
        }

        public static DateTime NullDate
        {
            get
            {
                return DateTime.MinValue;
            }
        }

        public static string NullString
        {
            get
            {
                return string.Empty;
            }
        }

        public static bool NullBoolean
        {
            get
            {
                return false;
            }
        }

        public static Guid NullGuid
        {
            get
            {
                return Guid.Empty;
            }
        }

        // sets a field to an application encoded null value ( used in BLL layer )
        public static object SetNull(object objValue, object objField)
        {
			Logger.LogDebug(">>> Entering SetNull(objValue={ObjValue},objField={ObjField})", objValue, objField);
            object returnValue = null;
            if (objValue == DBNull.Value)
            {
                if (objField is short)
                {
                    returnValue = NullShort;
                }
                else if (objField is byte)
                {
                    returnValue = NullByte;
                }
                else if (objField is int)
                {
                    returnValue = NullInteger;
                }
                else if (objField is float)
                {
                    returnValue = NullSingle;
                }
                else if (objField is double)
                {
                    returnValue = NullDouble;
                }
                else if (objField is decimal)
                {
                    returnValue = NullDecimal;
                }
                else if (objField is DateTime)
                {
                    returnValue = NullDate;
                }
                else if (objField is string)
                {
                    returnValue = NullString;
                }
                else if (objField is bool)
                {
                    returnValue = NullBoolean;
                }
                else if (objField is Guid)
                {
                    returnValue = NullGuid;
                }
                else
                {
                    // complex object
                    returnValue = null;
                }
            }
            else
            {
                // return value
                returnValue = objValue;
            }

			Logger.LogDebug("<<< Exiting SetNull(objValue={ObjValue},objField={ObjField})", objValue, objField);
			Logger.LogDebug("<<< Exiting SetNull(objPropertyInfo={ObjPropertyInfo})", objPropertyInfo);
            return returnValue;
        }

        // sets a field to an application encoded null value ( used in BLL layer )
        public static object SetNull(PropertyInfo objPropertyInfo)
        {
            object returnValue = null;
            switch (objPropertyInfo.PropertyType.ToString())
            {
                case "System.Int16":
                    returnValue = NullShort;
                    break;
                case "System.Int32":
                case "System.Int64":
                    returnValue = NullInteger;
                    break;
                case "system.Byte":
                    returnValue = NullByte;
                    break;
                case "System.Single":
                    returnValue = NullSingle;
                    break;
                case "System.Double":
                    returnValue = NullDouble;
                    break;
                case "System.Decimal":
                    returnValue = NullDecimal;
                    break;
                case "System.DateTime":
                    returnValue = NullDate;
                    break;
                case "System.String":
                case "System.Char":
                    returnValue = NullString;
                    break;
                case "System.Boolean":
                    returnValue = NullBoolean;
                    break;
                case "System.Guid":
                    returnValue = NullGuid;
                    break;
                default:
                    // Enumerations default to the first entry
                    Type pType = objPropertyInfo.PropertyType;
                    if (pType.BaseType.Equals(typeof(Enum)))
                    {
                        Array objEnumValues = Enum.GetValues(pType);
                        Array.Sort(objEnumValues);
                        returnValue = Enum.ToObject(pType, objEnumValues.GetValue(0));
                    }
                    else
                    {
                        // complex object
                        returnValue = null;
                    }

                    break;
            }

            return returnValue;
        }

        public static bool SetNullBoolean(object objValue)
        {
			Logger.LogDebug(">>> Entering SetNullBoolean(objValue={ObjValue})", objValue);
			Logger.LogDebug("<<< Exiting SetNullBoolean(objValue={ObjValue})", objValue);
            return objValue != DBNull.Value ? Convert.ToBoolean(objValue, CultureInfo.InvariantCulture) : NullBoolean;
        }

        public static DateTime SetNullDateTime(object objValue)
        {
			Logger.LogDebug(">>> Entering SetNullDateTime(objValue={ObjValue})", objValue);
			Logger.LogDebug("<<< Exiting SetNullDateTime(objValue={ObjValue})", objValue);
			Logger.LogDebug("<<< Exiting SetNullDateTime(objValue={ObjValue},dateTimeKind={DateTimeKind})", objValue, dateTimeKind);
            return objValue != DBNull.Value ? Convert.ToDateTime(objValue, CultureInfo.InvariantCulture) : NullDate;
        }

        public static DateTime SetNullDateTime(object objValue, DateTimeKind dateTimeKind)
        {
            return objValue != DBNull.Value
                ? DateTime.SpecifyKind(Convert.ToDateTime(objValue, CultureInfo.InvariantCulture), dateTimeKind)
                : NullDate;
        }

        public static int SetNullInteger(object objValue)
        {
			Logger.LogDebug(">>> Entering SetNullInteger(objValue={ObjValue})", objValue);
			Logger.LogDebug("<<< Exiting SetNullInteger(objValue={ObjValue})", objValue);
            return objValue != DBNull.Value ? Convert.ToInt32(objValue, CultureInfo.InvariantCulture) : NullInteger;
        }

        public static float SetNullSingle(object objValue)
        {
			Logger.LogDebug(">>> Entering SetNullSingle(objValue={ObjValue})", objValue);
			Logger.LogDebug("<<< Exiting SetNullSingle(objValue={ObjValue})", objValue);
            return objValue != DBNull.Value ? Convert.ToSingle(objValue, CultureInfo.InvariantCulture) : NullSingle;
        }

        public static string SetNullString(object objValue)
        {
			Logger.LogDebug(">>> Entering SetNullString(objValue={ObjValue})", objValue);
			Logger.LogDebug("<<< Exiting SetNullString(objValue={ObjValue})", objValue);
            return objValue != DBNull.Value ? Convert.ToString(objValue, CultureInfo.InvariantCulture) : NullString;
        }

        public static Guid SetNullGuid(object objValue)
        {
			Logger.LogDebug(">>> Entering SetNullGuid(objValue={ObjValue})", objValue);
            if ((objValue != DBNull.Value) && !string.IsNullOrEmpty(objValue.ToString()))
            {
			Logger.LogDebug("<<< Exiting SetNullGuid(objValue={ObjValue})", objValue);
                return new Guid(objValue.ToString());
            }

            return Guid.Empty;
        }

        // convert an application encoded null value to a database null value ( used in DAL )
        public static object GetNull(object objField, object objDBNull)
            => GetNull(objField, objDBNull, CultureInfo.CurrentCulture);

        public static object GetNull(object objField, object objDBNull, IFormatProvider provider)
        {
			Logger.LogDebug(">>> Entering GetNull(objField={ObjField},objDBNull={ObjDBNull})", objField, objDBNull);
            object returnValue = objField;
			Logger.LogDebug("GetNull(objField={ObjField},objDBNull={ObjDBNull},provider={Provider}): returnValue → {ReturnValue}", objField, objDBNull, provider, returnValue);
            if (objField == null)
            {
                returnValue = objDBNull;
            }
            else if (objField is byte)
            {
                if (Convert.ToByte(objField, provider) == NullByte)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is short)
            {
                if (Convert.ToInt16(objField, provider) == NullShort)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is int)
            {
                if (Convert.ToInt32(objField, provider) == NullInteger)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is float)
            {
                if (Convert.ToSingle(objField, provider) == NullSingle)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is double)
            {
                if (Convert.ToDouble(objField, provider) == NullDouble)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is decimal)
            {
                if (Convert.ToDecimal(objField, provider) == NullDecimal)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is DateTime)
            {
                // compare the Date part of the DateTime with the DatePart of the NullDate ( this avoids subtle time differences )
                if (Convert.ToDateTime(objField, provider).Date == NullDate.Date)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is string)
            {
                if (objField == null)
                {
                    returnValue = objDBNull;
                }
                else
                {
                    if (objField.ToString() == NullString)
                    {
                        returnValue = objDBNull;
                    }
                }
            }
            else if (objField is bool)
            {
                if (Convert.ToBoolean(objField, provider) == NullBoolean)
                {
                    returnValue = objDBNull;
                }
            }
            else if (objField is Guid guid)
            {
                if (guid.Equals(NullGuid))
                {
                    returnValue = objDBNull;
                }
            }

			Logger.LogDebug("<<< Exiting GetNull(objField={ObjField},objDBNull={ObjDBNull})", objField, objDBNull);
            return returnValue;
        }

        // checks if a field contains an application encoded null value
        public static bool IsNull(object objField)
        {
			Logger.LogDebug(">>> Entering IsNull(objField={ObjField})", objField);
            bool isNull = false;
            if (objField != null)
            {
                if (objField is int)
                {
                    isNull = objField.Equals(NullInteger);
                }
                else if (objField is short)
                {
                    isNull = objField.Equals(NullShort);
                }
                else if (objField is byte)
                {
                    isNull = objField.Equals(NullByte);
                }
                else if (objField is float)
                {
                    isNull = objField.Equals(NullSingle);
                }
                else if (objField is double)
                {
                    isNull = objField.Equals(NullDouble);
                }
                else if (objField is decimal)
                {
                    isNull = objField.Equals(NullDecimal);
                }
                else if (objField is DateTime objDate)
                {
                    isNull = objDate.Date.Equals(NullDate.Date);
                }
                else if (objField is string)
                {
                    isNull = objField.Equals(NullString);
                }
                else if (objField is bool)
                {
                    isNull = objField.Equals(NullBoolean);
                }
                else if (objField is Guid)
                {
                    isNull = objField.Equals(NullGuid);
                }
                else
                {
                    // complex object
                    isNull = false;
                }
            }
            else
            {
                isNull = true;
            }

			Logger.LogDebug("<<< Exiting IsNull(objField={ObjField})", objField);
            return isNull;
        }
    }
}
