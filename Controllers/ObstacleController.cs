using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using NRLApp.Models;

namespace NRLApp.Controllers
{
    /// Håndterer skjema (GET/POST) og liste mot MariaDB.
    public class ObstacleController : Controller
    {
        private readonly IConfiguration _config;
        public ObstacleController(IConfiguration config) => _config = config;

        private MySqlConnection CreateConnection()
        {
            var cs = _config.GetConnectionString("DefaultConnection");
            return new MySqlConnection(cs);
        }

        [HttpGet]
        public IActionResult DataForm() => View(new ObstacleData());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ObstacleData data)
        {
            // 1) Sjekk at bruker faktisk har valgt posisjon i kartet
            if (data.Latitude == 0 && data.Longitude == 0)
            {
                
                ModelState.AddModelError(string.Empty, "Klikk i kartet for å velge posisjon.");
                return View(data); // vis skjemaet igjen med feilmelding
            }

            // 2) Vanlig modellvalidering
            if (!ModelState.IsValid) return View(data);

            // 3) Lagre som før
            using var conn = CreateConnection();
            await conn.OpenAsync();

            const string createSql = @"
    CREATE TABLE IF NOT EXISTS Obstacles(
      Id INT AUTO_INCREMENT PRIMARY KEY,
      ObstacleName VARCHAR(100) NOT NULL,
      ObstacleHeight DOUBLE NOT NULL,
      ObstacleDescription VARCHAR(500),
      Latitude DOUBLE NOT NULL,
      Longitude DOUBLE NOT NULL,
      CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
    );";
            await conn.ExecuteAsync(createSql);

            const string insertSql = @"
    INSERT INTO Obstacles(ObstacleName, ObstacleHeight, ObstacleDescription, Latitude, Longitude)
    VALUES (@ObstacleName, @ObstacleHeight, @ObstacleDescription, @Latitude, @Longitude);";
            await conn.ExecuteAsync(insertSql, data);

            return View("Overview", data);
        }


        [HttpGet]
        public async Task<IActionResult> List()
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();
            var rows = await conn.QueryAsync<ObstacleData>(@"
                SELECT ObstacleName, ObstacleHeight, ObstacleDescription, Latitude, Longitude
                FROM Obstacles
                ORDER BY CreatedAt DESC;");
            return View(rows);
        }
    }
}
