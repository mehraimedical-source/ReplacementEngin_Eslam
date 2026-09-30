using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using Print_SCP.Models;

namespace Print_SCP.Data
{
    internal sealed class PrintArchiveRepository
    {
        private readonly string _databasePath;
        private readonly string _connectionString;
        private readonly object _sync = new object();

        public PrintArchiveRepository(string databasePath)
        {
            _databasePath = databasePath;
            _connectionString = "Data Source=" + databasePath + ";Version=3;";
        }

        public void Initialize()
        {
            lock (_sync)
            {
                string folder = Path.GetDirectoryName(_databasePath);
                if (!string.IsNullOrWhiteSpace(folder))
                    Directory.CreateDirectory(folder);

                if (!File.Exists(_databasePath))
                    SQLiteConnection.CreateFile(_databasePath);

                using (var cn = new SQLiteConnection(_connectionString))
                {
                    cn.Open();
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS PrintHistory
(
    Id TEXT PRIMARY KEY,
    JobId TEXT,
    ReceivedAt TEXT NOT NULL,
    CallingAeTitle TEXT,
    CalledAeTitle TEXT,
    RemoteIp TEXT,
    FilmSizeId TEXT,
    Orientation TEXT,
    Layout TEXT,
    ImageCount INTEGER,
    WidthMm REAL,
    HeightMm REAL,
    Dpi INTEGER,
    RenderedImagePath TEXT,
    ThumbnailPath TEXT,
    WindowsPrintStatus TEXT,
    DicomForwardStatus TEXT,
    ErrorMessage TEXT
);
CREATE INDEX IF NOT EXISTS IX_PrintHistory_ReceivedAt
ON PrintHistory(ReceivedAt DESC);";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void Save(RenderedPrintInfo item)
        {
            lock (_sync)
            using (var cn = new SQLiteConnection(_connectionString))
            {
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = @"
INSERT OR REPLACE INTO PrintHistory
(Id, JobId, ReceivedAt, CallingAeTitle, CalledAeTitle, RemoteIp,
 FilmSizeId, Orientation, Layout, ImageCount, WidthMm, HeightMm, Dpi,
 RenderedImagePath, ThumbnailPath, WindowsPrintStatus, DicomForwardStatus, ErrorMessage)
VALUES
(@Id, @JobId, @ReceivedAt, @CallingAeTitle, @CalledAeTitle, @RemoteIp,
 @FilmSizeId, @Orientation, @Layout, @ImageCount, @WidthMm, @HeightMm, @Dpi,
 @RenderedImagePath, @ThumbnailPath, @WindowsPrintStatus, @DicomForwardStatus, @ErrorMessage);";

                    cmd.Parameters.AddWithValue("@Id", item.Id.ToString("D"));
                    cmd.Parameters.AddWithValue("@JobId", item.JobId.ToString("D"));
                    cmd.Parameters.AddWithValue("@ReceivedAt", item.ReceivedAt.ToString("O"));
                    cmd.Parameters.AddWithValue("@CallingAeTitle", item.CallingAeTitle ?? "");
                    cmd.Parameters.AddWithValue("@CalledAeTitle", item.CalledAeTitle ?? "");
                    cmd.Parameters.AddWithValue("@RemoteIp", item.RemoteIp ?? "");
                    cmd.Parameters.AddWithValue("@FilmSizeId", item.FilmSizeId ?? "");
                    cmd.Parameters.AddWithValue("@Orientation", item.Orientation ?? "");
                    cmd.Parameters.AddWithValue("@Layout", item.Layout ?? "");
                    cmd.Parameters.AddWithValue("@ImageCount", item.ImageCount);
                    cmd.Parameters.AddWithValue("@WidthMm", item.WidthMm);
                    cmd.Parameters.AddWithValue("@HeightMm", item.HeightMm);
                    cmd.Parameters.AddWithValue("@Dpi", item.Dpi);
                    cmd.Parameters.AddWithValue("@RenderedImagePath", item.RenderedImagePath ?? "");
                    cmd.Parameters.AddWithValue("@ThumbnailPath", item.ThumbnailPath ?? "");
                    cmd.Parameters.AddWithValue("@WindowsPrintStatus", item.WindowsPrintStatus ?? "");
                    cmd.Parameters.AddWithValue("@DicomForwardStatus", item.DicomForwardStatus ?? "");
                    cmd.Parameters.AddWithValue("@ErrorMessage", item.ErrorMessage ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<RenderedPrintInfo> GetRecent(int limit)
        {
            var result = new List<RenderedPrintInfo>();

            lock (_sync)
            using (var cn = new SQLiteConnection(_connectionString))
            {
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT Id, JobId, ReceivedAt, CallingAeTitle, CalledAeTitle, RemoteIp,
       FilmSizeId, Orientation, Layout, ImageCount, WidthMm, HeightMm, Dpi,
       RenderedImagePath, ThumbnailPath, WindowsPrintStatus, DicomForwardStatus, ErrorMessage
FROM PrintHistory
ORDER BY ReceivedAt DESC
LIMIT @Limit;";
                    cmd.Parameters.AddWithValue("@Limit", Math.Max(1, limit));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime received;
                            DateTime.TryParse(
                                Convert.ToString(reader["ReceivedAt"], CultureInfo.InvariantCulture),
                                null, DateTimeStyles.RoundtripKind, out received);

                            result.Add(new RenderedPrintInfo
                            {
                                Id = Guid.Parse(Convert.ToString(reader["Id"])),
                                JobId = Guid.Parse(Convert.ToString(reader["JobId"])),
                                ReceivedAt = received,
                                CallingAeTitle = Convert.ToString(reader["CallingAeTitle"]),
                                CalledAeTitle = Convert.ToString(reader["CalledAeTitle"]),
                                RemoteIp = Convert.ToString(reader["RemoteIp"]),
                                FilmSizeId = Convert.ToString(reader["FilmSizeId"]),
                                Orientation = Convert.ToString(reader["Orientation"]),
                                Layout = Convert.ToString(reader["Layout"]),
                                ImageCount = Convert.ToInt32(reader["ImageCount"]),
                                WidthMm = Convert.ToDouble(reader["WidthMm"]),
                                HeightMm = Convert.ToDouble(reader["HeightMm"]),
                                Dpi = Convert.ToInt32(reader["Dpi"]),
                                RenderedImagePath = Convert.ToString(reader["RenderedImagePath"]),
                                ThumbnailPath = Convert.ToString(reader["ThumbnailPath"]),
                                WindowsPrintStatus = Convert.ToString(reader["WindowsPrintStatus"]),
                                DicomForwardStatus = Convert.ToString(reader["DicomForwardStatus"]),
                                ErrorMessage = Convert.ToString(reader["ErrorMessage"])
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}