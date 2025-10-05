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

        // Oppretter en databaseforbindelse basert på connection string fra appsettings.json
        private MySqlConnection CreateConnection()
        {
            var cs = _config.GetConnectionString("DefaultConnection");
            return new MySqlConnection(cs);
        }

        // Viser skjemaet for å registrere et nytt hinder
        [HttpGet]
        public IActionResult DataForm() => View(new ObstacleData());

        // Tar imot og lagrer data fra skjemaet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DataForm(ObstacleData data)
        {
            // Sjekker at brukeren har valgt posisjon i kartet
            if (data.Latitude == 0 && data.Longitude == 0)
            {
                ModelState.AddModelError(string.Empty, "Klikk i kartet for å velge posisjon.");
                return View(data);
            }

            // Sjekker at modellen er gyldig
            if (!ModelState.IsValid) return View(data);

            using var conn = CreateConnection();
            await conn.OpenAsync();

            // Oppretter tabellen hvis den ikke finnes
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

            // Setter inn data i databasen
            const string insertSql = @"
    INSERT INTO Obstacles(ObstacleName, ObstacleHeight, ObstacleDescription, Latitude, Longitude)
    VALUES (@ObstacleName, @ObstacleHeight, @ObstacleDescription, @Latitude, @Longitude);";
            await conn.ExecuteAsync(insertSql, data);

            // Viser oversiktssiden etter innsending
            return View("Overview", data);
        }

        // Viser en liste over alle registrerte hindere
        [HttpGet]
        public async Task<IActionResult> List()
        {
            using var conn = CreateConnection();
            await conn.OpenAsync();

            // Henter data fra databasen
            var rows = await conn.QueryAsync<ObstacleData>(@"
                SELECT ObstacleName, ObstacleHeight, ObstacleDescription, Latitude, Longitude
                FROM Obstacles
                ORDER BY CreatedAt DESC;");

            return View(rows);
        }
    }
}
