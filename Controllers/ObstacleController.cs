using System.Text.Json;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using NRLApp.Models;

namespace NRLApp.Controllers
{
    public class ObstacleController : Controller
    {
        private readonly IConfiguration _config;
        public ObstacleController(IConfiguration config) => _config = config;

        private MySqlConnection CreateConnection()
            => new MySqlConnection(_config.GetConnectionString("DefaultConnection"));

        // ---------- Wizard state i Session ----------
        private const string WizardKey = "NRL_WIZARD_STATE";

        private ObstacleWizardState GetState()
        {
            var json = HttpContext.Session.GetString(WizardKey);
            if (string.IsNullOrWhiteSpace(json)) return new ObstacleWizardState();
            try { return JsonSerializer.Deserialize<ObstacleWizardState>(json) ?? new ObstacleWizardState(); }
            catch { return new ObstacleWizardState(); }
        }

        private void SaveState(ObstacleWizardState s)
            => HttpContext.Session.SetString(WizardKey, JsonSerializer.Serialize(s));

        private void ClearState() => HttpContext.Session.Remove(WizardKey);

        // ---------- STEP 1: Område ----------
        [HttpGet]
        public IActionResult Area() => View();

        public sealed class AreaVm
        {
            public double CenterLat { get; set; }
            public double CenterLng { get; set; }
            public int RadiusMeters { get; set; }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Area(AreaVm vm)
        {
            var s = GetState();
            s.CenterLat = vm.CenterLat;
            s.CenterLng = vm.CenterLng;
            s.RadiusMeters = vm.RadiusMeters;
            SaveState(s);
            return RedirectToAction(nameof(Type));
        }

        // ---------- STEP 2: Type ----------
        [HttpGet]
        public IActionResult Type() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Type(ObstacleType type)
        {
            var s = GetState();
            s.Type = type;
            SaveState(s);
            return RedirectToAction(nameof(Height));
        }

        // ---------- STEP 3: Høyde (SEND INN) ----------
        [HttpGet]
        public IActionResult Height() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Height(HeightBand height)
        {
            var s = GetState();
            s.Height = height;
            SaveState(s);

            // må ha alle felter valgt før lagring
            if (s.CenterLat is null || s.CenterLng is null || s.RadiusMeters is null || s.Type is null || s.Height is null)
            {
                TempData["FlashError"] = "Velg område, type og høyde før innsending.";
                return RedirectToAction(nameof(Area));
            }

            var (minH, maxH) = HeightBandToRange(s.Height.Value);

            var data = new ObstacleData
            {
                CenterLat = s.CenterLat.Value,
                CenterLng = s.CenterLng.Value,
                RadiusMeters = s.RadiusMeters.Value,
                Type = s.Type.Value,
                HeightMinMeters = minH,
                HeightMaxMeters = maxH
            };

            try
            {
                using var con = CreateConnection();
                const string sql = @"
                    INSERT INTO obstacles (center_lat, center_lng, radius_m, type, height_min_m, height_max_m)
                    VALUES (@CenterLat, @CenterLng, @RadiusMeters, @Type, @HeightMinMeters, @HeightMaxMeters);";

                await con.ExecuteAsync(sql, new
                {
                    data.CenterLat,
                    data.CenterLng,
                    data.RadiusMeters,
                    Type = data.Type.ToString(), // enum som tekst
                    data.HeightMinMeters,
                    data.HeightMaxMeters
                });

                ClearState();
                TempData["Flash"] = "Hinderet er lagret.";
                return RedirectToAction(nameof(Thanks));
            }
            catch (Exception ex)
            {
                TempData["FlashError"] = "Kunne ikke lagre hinderet: " + ex.Message;
                return RedirectToAction(nameof(Type));
            }
        }

        // ---------- Takk ----------
        [HttpGet]
        public IActionResult Thanks() => View();

        // ---------- Registrerte: liste ----------
        [HttpGet]
        public async Task<IActionResult> List()
        {
            using var con = CreateConnection();
            const string sql = @"
                SELECT id,
                       center_lat   AS CenterLat,
                       center_lng   AS CenterLng,
                       radius_m     AS RadiusMeters,
                       type         AS Type,
                       height_min_m AS HeightMinMeters,
                       height_max_m AS HeightMaxMeters,
                       created_utc  AS CreatedUtc
                FROM obstacles
                ORDER BY id DESC;";
            var rows = await con.QueryAsync<ObstacleListItem>(sql);
            return View(rows);
        }

        // ---------- Registrerte: kart (valgfritt, hvis du har Map.cshtml) ----------
        [HttpGet]
        public async Task<IActionResult> Map()
        {
            using var con = CreateConnection();
            const string sql = @"
                SELECT id,
                       center_lat   AS CenterLat,
                       center_lng   AS CenterLng,
                       radius_m     AS RadiusMeters,
                       type         AS Type,
                       height_min_m AS HeightMinMeters,
                       height_max_m AS HeightMaxMeters,
                       created_utc  AS CreatedUtc
                FROM obstacles
                ORDER BY id DESC;";
            var rows = await con.QueryAsync<ObstacleListItem>(sql);
            return View(rows);
        }

        // ---------- Høydebånd → intervall ----------
        private static (int min, int? max) HeightBandToRange(HeightBand b) => b switch
        {
            HeightBand.H0_15 => (0, 15),
            HeightBand.H15_30 => (15, 30),
            HeightBand.H30_50 => (30, 50),
            HeightBand.H50_75 => (50, 75),
            HeightBand.H75_100 => (75, 100),
            HeightBand.H100Plus => (100, null),
            _ => (0, null)
        };
    }
}
