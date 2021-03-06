﻿using System;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Telemetry;

namespace Telimena.WebApp.Core.DTO
{
    public class TelemetryPivotTableRow
    {
        public TelemetryPivotTableRow(TelemetryDetail detail)
        {
            this.ComponentName = detail.EntryKey;
            this.Date = detail.Timestamp.ToString("O");
            this.Time = detail.Timestamp.TimeOfDay.TotalSeconds;
            this.Sequence = detail.Sequence;
            this.UserName = detail.UserIdentifier;
        }

        public TelemetryPivotTableRow()
        {
        }

        public string Sequence { get; set; }
        public string Date { get; set; }
        public double Time { get; set; }
        public string UserName { get; set; }
        public string ComponentName { get; set; }
        public string Key { get; set; }
        public string PropertyValue { get; set; }
        public double MetricValue { get; set; }
    }
}