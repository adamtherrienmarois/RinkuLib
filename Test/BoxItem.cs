using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using RinkuLib.Commands;
using RinkuLib.DbParsing;

namespace Test;

public class BoxItem {
    public int ID { get; set; }
    public int SourceID { get; set; }
    public KeyValuePair<int, string> Product { get; set; }
    public KeyValuePair<int, string> BoxType { get; set; }
    public KeyValuePair<int, string> Station { get; set; }
    public KeyValuePair<int, string>? User { get; set; }
    public Status? Status { get; set; }
    public bool Quarantine { get; set; }
    public string? BatchNB { get; set; }
    public DateTime Date { get; set; }
    public decimal Gross { get; set; }
    public decimal Tare { get; set; }
    public decimal Net { get; set; }
    public override string ToString() {
        //[BoxItem 34124] Product: , Status: N/A, Net: 0 kg, Date: 2024-10-26 12:20
        return $"[BoxItem {ID}] Product: {Product.Value}";//, Status: {Status?.Value ?? "N/A"}, Net: {Net} kg, Date: {Date:yyyy-MM-dd HH:mm}";
    }
}
public record struct Status([JumpIfNull] int ID, string Value, string Color) : RinkuLib.DbParsing.IDbReadable;