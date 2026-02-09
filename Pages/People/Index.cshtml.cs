using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Bramki.Data;
using Bramki.Models;

namespace Bramki.Pages.People;

public class IndexModel(AppDbContext db) : PageModel
{
    // Filters
    [BindProperty(SupportsGet = true)] public int? ID { get; set; }
    [BindProperty(SupportsGet = true)] public string? ERPID { get; set; }        // string now
    [BindProperty(SupportsGet = true)] public string? FirstName { get; set; }
    [BindProperty(SupportsGet = true)] public string? Surname { get; set; }
    [BindProperty(SupportsGet = true)] public string? CardNumber { get; set; }
    [BindProperty(SupportsGet = true)] public string? CardNumber2 { get; set; }
    [BindProperty(SupportsGet = true)] public string? LunchCard { get; set; }   // Numer karty (Obiady)
    [BindProperty(SupportsGet = true)] public string? PpeCard { get; set; }     // Numer karty (ŚOI)
    [BindProperty(SupportsGet = true)] public bool? Forklifts { get; set; }
    [BindProperty(SupportsGet = true)] public bool? Cranes { get; set; }
    [BindProperty(SupportsGet = true)] public bool? Gantries { get; set; }

    // Sorting
    [BindProperty(SupportsGet = true)] public string? SortBy { get; set; } // "ID|ERPID|FirstName|Surname|CardNumber|CardNumber2|LunchCard|PpeCard|Forklifts|Cranes|Gantries"
    [BindProperty(SupportsGet = true)] public string? Dir { get; set; }    // "asc" | "desc"

    public List<Person> Rows { get; set; } = [];

    public async Task OnGet()
    {
        var list = await db.People.AsNoTracking().ToListAsync();

        var sortByPerms =
            string.Equals(SortBy, "Forklifts", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(SortBy, "Cranes", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(SortBy, "Gantries", StringComparison.OrdinalIgnoreCase);

        // Filters (null-safe)
        if (ID.HasValue)
            list = list.Where(x => x.ID == ID.Value).ToList();

        if (!string.IsNullOrWhiteSpace(ERPID))
            list = list.Where(x => (x.ERPID ?? string.Empty)
                .Contains(ERPID.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(FirstName))
            list = list.Where(x => x.FirstName.Contains(FirstName.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(Surname))
            list = list.Where(x => x.Surname.Contains(Surname.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(CardNumber))
            list = list.Where(x => (x.CardNumber ?? string.Empty)
                .Contains(CardNumber.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(CardNumber2))
            list = list.Where(x => (x.CardNumber2 ?? string.Empty)
                .Contains(CardNumber2.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(LunchCard))
            list = list.Where(x => (x.LunchCardNumber ?? string.Empty)
                .Contains(LunchCard.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        if (!string.IsNullOrWhiteSpace(PpeCard))
            list = list.Where(x => (x.PpeStorageCabinetsCardNumber ?? string.Empty)
                .Contains(PpeCard.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

        // Sort (null-safe). Default = ID desc.
        var desc = string.Equals(Dir, "desc", StringComparison.OrdinalIgnoreCase);
        list = (SortBy?.ToLowerInvariant()) switch
        {
            "id" => (desc ? list.OrderByDescending(x => x.ID) : list.OrderBy(x => x.ID)).ToList(),
            "erpid" => (desc ? list.OrderByDescending(x => x.ERPID ?? string.Empty)
                                   : list.OrderBy(x => x.ERPID ?? string.Empty)).ToList(),
            "firstname" => (desc ? list.OrderByDescending(x => x.FirstName) : list.OrderBy(x => x.FirstName)).ToList(),
            "surname" => (desc ? list.OrderByDescending(x => x.Surname) : list.OrderBy(x => x.Surname)).ToList(),
            "cardnumber" => (desc ? list.OrderByDescending(x => x.CardNumber ?? "") : list.OrderBy(x => x.CardNumber ?? "")).ToList(),
            "cardnumber2" => (desc ? list.OrderByDescending(x => x.CardNumber2 ?? "") : list.OrderBy(x => x.CardNumber2 ?? "")).ToList(),
            "lunchcard" => (desc ? list.OrderByDescending(x => x.LunchCardNumber ?? string.Empty) : list.OrderBy(x => x.LunchCardNumber ?? string.Empty)).ToList(),
            "ppecard" => (desc ? list.OrderByDescending(x => x.PpeStorageCabinetsCardNumber ?? string.Empty) : list.OrderBy(x => x.PpeStorageCabinetsCardNumber ?? string.Empty)).ToList(),
            _ => list.OrderByDescending(x => x.ID).ToList()
        };

        var perms = await db.GatePrivileges
            .AsNoTracking()
            .ToListAsync();

        var map = perms.ToDictionary(p => p.DUserId);

        foreach (var p in list)
        {
            if (map.TryGetValue(p.ID, out var gp))
            {
                p.GatesForklifts = gp.GatesForklifts;
                p.GatesCranes = gp.GatesCranes;
                p.GatesGantries = gp.GatesGantries;
            }
            else
            {
                p.GatesForklifts = null;
                p.GatesCranes = null;
                p.GatesGantries = null;
            }
        }

        if (Forklifts.HasValue)
        {
            if (Forklifts.Value)
                list = list.Where(p => p.GatesForklifts == true).ToList();
            else
                list = list.Where(p => p.GatesForklifts != true).ToList();
        }

        if (Cranes.HasValue)
        {
            if (Cranes.Value)
                list = list.Where(p => p.GatesCranes == true).ToList();
            else
                list = list.Where(p => p.GatesCranes != true).ToList();
        }

        if (Gantries.HasValue)
        {
            if (Gantries.Value)
                list = list.Where(p => p.GatesGantries == true).ToList();
            else
                list = list.Where(p => p.GatesGantries != true).ToList();
        }

        if (sortByPerms)
        {
            if (string.Equals(SortBy, "Forklifts", StringComparison.OrdinalIgnoreCase))
            {
                list = desc
                    ? list.OrderBy(p => p.GatesForklifts == true).ThenBy(p => p.ID).ToList()
                    : list.OrderByDescending(p => p.GatesForklifts == true).ThenBy(p => p.ID).ToList();
            }
            else if (string.Equals(SortBy, "Cranes", StringComparison.OrdinalIgnoreCase))
            {
                list = desc
                    ? list.OrderBy(p => p.GatesCranes == true).ThenBy(p => p.ID).ToList()
                    : list.OrderByDescending(p => p.GatesCranes == true).ThenBy(p => p.ID).ToList();
            }
            else if (string.Equals(SortBy, "Gantries", StringComparison.OrdinalIgnoreCase))
            {
                list = desc
                    ? list.OrderBy(p => p.GatesGantries == true).ThenBy(p => p.ID).ToList()
                    : list.OrderByDescending(p => p.GatesGantries == true).ThenBy(p => p.ID).ToList();
            }
        }

        Rows = list;
    }

    public string SortLink(string col)
    {
        var nextDir = (string.Equals(SortBy, col, StringComparison.OrdinalIgnoreCase) && !string.Equals(Dir, "desc", StringComparison.OrdinalIgnoreCase))
                      ? "desc" : "asc";
        var route = new RouteValueDictionary(new
        {
            ID,
            ERPID,
            FirstName,
            Surname,
            CardNumber,
            CardNumber2,
            LunchCard,
            PpeCard,
            Forklifts,
            Cranes,
            Gantries,
            SortBy = col,
            Dir = nextDir
        });
        return Url.PageLink(null, null, route) ?? "#";
    }

    public bool IsActiveSort(string col)
    {
        var active = string.IsNullOrWhiteSpace(SortBy) ? "ID" : SortBy;
        return string.Equals(active, col, StringComparison.OrdinalIgnoreCase);
    }

    public string SortIcon(string col)
    {
        if (!IsActiveSort(col)) return "↕";

        if (string.IsNullOrWhiteSpace(SortBy) && string.IsNullOrWhiteSpace(Dir) && col == "ID")
            return "▼";

        var desc = string.Equals(Dir, "desc", StringComparison.OrdinalIgnoreCase);
        return desc ? "▼" : "▲";
    }

    public string SortAria(string col)
    {
        if (!IsActiveSort(col)) return "not sorted";
        var desc = string.Equals(Dir, "desc", StringComparison.OrdinalIgnoreCase);
        return desc ? "sorted descending" : "sorted ascending";
    }
}