using FreeTds;
using System;

namespace Tdstream.Server
{
    /// <summary>
    /// Class TdsExtensions.
    /// </summary>
    public static class TdsExtensions
    {
        public static MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> Column(this MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> source, int index, string columnName, TDS_SERVER_TYPE columnType, int? columnSize = null)
        {
            var column = source[index];
            column.ColumnName = columnName;
            column.ColumnType = columnType;
            //if (!G.is_fixed_type((int)columnType))
            {
                if (columnSize != null)
                    column.ColumnSize = columnSize.Value;
            }
            return source;
        }
        public static MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> Column(this MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> source, int index, string columnName, Type columnType, int? columnSize = null) =>
            Column(source, index, columnName, ToServerType(columnType), columnSize);

        public static MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> ColumnData(this MarshaledObjectArrayAccessor<TdsResultInfo, TDSRESULTINFO, TdsColumn, TDSCOLUMN> source, int index, object columnData)
        {
            var column = source[index];
            //column.ColumnData = IntPtr.Zero;
            return source;
        }

        static TDS_SERVER_TYPE ToServerType(Type type, bool unicode = true)
        {
            if (type == typeof(byte[])) return TDS_SERVER_TYPE.SYBVARBINARY;
            else if (type == typeof(string)) return unicode ? TDS_SERVER_TYPE.SYBNVARCHAR : TDS_SERVER_TYPE.SYBVARCHAR;
            else if (type == typeof(char)) return unicode ? TDS_SERVER_TYPE.XSYBNCHAR : TDS_SERVER_TYPE.SYBCHAR;
            else if (type == typeof(byte)) return TDS_SERVER_TYPE.SYBINT1;
            else if (type == typeof(bool)) return TDS_SERVER_TYPE.SYBBIT;
            else if (type == typeof(short)) return TDS_SERVER_TYPE.SYBINT2;
            else if (type == typeof(int)) return TDS_SERVER_TYPE.SYBINT4;
            else if (type == typeof(double)) return TDS_SERVER_TYPE.SYBREAL;
            else if (type == typeof(DateTime)) return TDS_SERVER_TYPE.SYBDATETIME;
            else if (type == typeof(float)) return TDS_SERVER_TYPE.SYBFLT8;
            else if (type == typeof(sbyte)) return TDS_SERVER_TYPE.SYBSINT1;
            else if (type == typeof(ushort)) return TDS_SERVER_TYPE.SYBUINT2;
            else if (type == typeof(uint)) return TDS_SERVER_TYPE.SYBUINT4;
            else if (type == typeof(ulong)) return TDS_SERVER_TYPE.SYBUINT8;
            else if (type == typeof(decimal)) return TDS_SERVER_TYPE.SYBDECIMAL;
            else if (type == typeof(long)) return TDS_SERVER_TYPE.SYBINT8;
            else throw new ArgumentOutOfRangeException(nameof(type), type.Name);
        }
    }
}

