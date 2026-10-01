using System;
using System.Collections.Generic;

namespace AstronomyStellarMapsCreator.Server.Models;

public partial class I10CatalogDat
{
    public int Id { get; set; }

    public double? Gctp { get; set; }

    public string? NGctp { get; set; }

    public int? Dm { get; set; }

    public int? RahH { get; set; }

    public double? RamMin { get; set; }

    public string? De { get; set; }

    public int? DedDeg { get; set; }

    public int? DemArcmin { get; set; }

    public double? MagMag { get; set; }

    public string? NMag { get; set; }

    public string? Sp { get; set; }

    public int? Hd { get; set; }

    public string? Oname { get; set; }

    public int? PmRaMasYr { get; set; }

    public int? PmDeMasYr { get; set; }

    public int? PlxMas { get; set; }

    public int? EPlxMas { get; set; }
}
