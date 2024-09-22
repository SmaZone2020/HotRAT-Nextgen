using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public static class Log
{
    private static List<LogEntry> logEntries = new List<LogEntry>();
    private const int LogsPerPage = 10;
    public static void AddLog(string text, string hexColor)
    {
        logEntries.Add(new LogEntry
        {
            Message = text,
            HexColor = hexColor,
            Timestamp = DateTime.Now
        });
    }

    public static string GetLogs(int page)
    {
        int totalPages = (int)Math.Ceiling((double)logEntries.Count / LogsPerPage);

        if (page < 1) page = 1;
        if (page > totalPages) page = totalPages;

        var logsForPage = logEntries
            .Skip((page - 1) * LogsPerPage)
            .Take(LogsPerPage)
            .Select(log => new
            {
                log.Message,
                log.HexColor,
                Timestamp = log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .ToList();

        var result = new
        {
            当前页 = page,
            全部页面 = totalPages,
            log = logsForPage
        };

        return JsonConvert.SerializeObject(result);
    }
    private class LogEntry
    {
        public string Message { get; set; }
        public string HexColor { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
