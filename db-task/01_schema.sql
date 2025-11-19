-- ================================
-- 01_schema.sql
-- Kartverket hinder-database
-- ================================

DROP TABLE IF EXISTS ObstacleReport;
DROP TABLE IF EXISTS ObstacleType;
DROP TABLE IF EXISTS Pilot;

-- Piloter som sender inn rapporter
CREATE TABLE Pilot (
                       PilotID INT AUTO_INCREMENT PRIMARY KEY,
                       PilotName      VARCHAR(100) NOT NULL,
                       Organization   VARCHAR(100),
                       Callsign       VARCHAR(20)
);

-- Typer hindere
CREATE TABLE ObstacleType (
                              ObstacleTypeID INT AUTO_INCREMENT PRIMARY KEY,
                              TypeName       VARCHAR(100) NOT NULL
);

-- Rapporter
CREATE TABLE ObstacleReport (
                                ReportID        INT AUTO_INCREMENT PRIMARY KEY,
                                PilotID         INT NOT NULL,
                                ObstacleTypeID  INT NOT NULL,
                                ReportDate      DATE NOT NULL,
                                HeightMeters    INT NOT NULL,
                                Municipality    VARCHAR(100),
                                Description     TEXT,
                                CONSTRAINT fk_report_pilot
                                    FOREIGN KEY (PilotID) REFERENCES Pilot(PilotID),
                                CONSTRAINT fk_report_type
                                    FOREIGN KEY (ObstacleTypeID) REFERENCES ObstacleType(ObstacleTypeID)
);

-- Indekser for ytelse
CREATE INDEX idx_report_date  ON ObstacleReport(ReportDate);
CREATE INDEX idx_report_pilot ON ObstacleReport(PilotID);
CREATE INDEX idx_report_type  ON ObstacleReport(ObstacleTypeID);
