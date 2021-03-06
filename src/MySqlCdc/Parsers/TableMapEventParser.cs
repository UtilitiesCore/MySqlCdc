using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;
using MySqlCdc.Providers.MySql;

namespace MySqlCdc.Parsers
{
    /// <summary>
    /// Parses <see cref="TableMapEvent"/> events.
    /// Supports all versions of MariaDB and MySQL 5.0+.
    /// </summary>
    public class TableMapEventParser : IEventParser
    {
        /// <summary>
        /// Parses <see cref="TableMapEvent"/> from the buffer.
        /// </summary>
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var tableId = reader.ReadLongLittleEndian(6);

            // Reserved bytes and database name length 
            reader.Advance(3);
            var databaseName = reader.ReadNullTerminatedString();

            // Table name length
            reader.Advance(1);
            var tableName = reader.ReadNullTerminatedString();

            var columnsNumber = (int)reader.ReadLengthEncodedNumber();
            var columnTypes = reader.ReadByteArraySlow(columnsNumber);

            var metadataLength = (int)reader.ReadLengthEncodedNumber();
            var metadata = ParseMetadata(ref reader, columnTypes);

            var nullBitmap = reader.ReadBitmapLittleEndian(columnsNumber);

            TableMetadata tableMetadata = null;
            if (!reader.IsEmpty())
            {
                // Read MySQL 5.6+ metadata. Not supported in MariaDB.
                tableMetadata = new TableMetadata(ref reader, columnTypes);
            }

            return new TableMapEvent(header, tableId, databaseName, tableName, columnTypes, metadata, nullBitmap, tableMetadata);
        }

        private int[] ParseMetadata(ref PacketReader reader, byte[] columnTypes)
        {
            int[] metadata = new int[columnTypes.Length];
            for (int i = 0; i < columnTypes.Length; i++)
            {
                // See https://mariadb.com/kb/en/library/rows_event_v1/#column-data-formats
                switch ((ColumnType)columnTypes[i])
                {
                    case ColumnType.GEOMETRY:
                    case ColumnType.JSON:
                    case ColumnType.TINY_BLOB:
                    case ColumnType.MEDIUM_BLOB:
                    case ColumnType.LONG_BLOB:
                    case ColumnType.BLOB:
                    case ColumnType.FLOAT:
                    case ColumnType.DOUBLE:
                    case ColumnType.TIMESTAMP2:
                    case ColumnType.DATETIME2:
                    case ColumnType.TIME2:
                        metadata[i] = reader.ReadByte();
                        break;

                    case ColumnType.BIT:
                    case ColumnType.VARCHAR:
                    case ColumnType.VAR_STRING:
                    case ColumnType.NEWDECIMAL:
                        metadata[i] = reader.ReadUInt16LittleEndian();
                        break;

                    case ColumnType.ENUM:
                    case ColumnType.SET:
                    case ColumnType.STRING:
                        metadata[i] = reader.ReadUInt16BigEndian();
                        break;

                    default:
                        metadata[i] = 0;
                        break;
                }
            }
            return metadata;
        }
    }
}
