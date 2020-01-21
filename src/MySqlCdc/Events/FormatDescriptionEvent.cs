using MySqlCdc.Constants;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Written as the first event in binlog file or when replication is started.
    /// See <a href="https://mariadb.com/kb/en/library/format_description_event/">MariaDB docs</a>
    /// See <a href="https://dev.mysql.com/doc/internals/en/format-description-event.html">MySQL docs</a>
    /// See <a href="https://mariadb.com/kb/en/library/5-slave-registration/#events-transmission-after-com_binlog_dump">start events flow</a>
    /// </summary>
    public class FormatDescriptionEvent : BinlogEvent
    {
        public int BinlogVersion { get; }
        public string ServerVersion { get; }
        public ChecksumType ChecksumType { get; }

        public FormatDescriptionEvent(
            EventHeader header,
            int binlogVersion,
            string serverVersion,
            ChecksumType checksumType) : base(header)
        {
            BinlogVersion = binlogVersion;
            ServerVersion = serverVersion;
            ChecksumType = checksumType;
        }
    }
}
