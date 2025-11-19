-- ================================
-- 03_seed.sql (v1)
-- Minst 10 rader per tabell
-- ================================

-- Piloter (10 rader)
INSERT INTO Pilot (PilotName, Organization, Callsign) VALUES
('Ola Nordmann',      'Luftambulansen Sør',  'LN-OLA'),
('Kari Hansen',       'Forsvaret',           'MIL-KH'),
('Per Olav',          'Politiet',            'POL-PP'),
('Anne Mari',         'Luftambulansen Nord', 'LN-AM'),
('Mats Musstad',      'Charterfly',          'CF-MM'),
('Ida Kristiansen',   'Luftambulansen Natt', 'LN-ID'),
('Filip Johnsen',     'Helicopter West',     'HW-FJ'),
('Lise Nilsen',       'Helicopter East',     'HE-LN'),
('Emil Olai',         'Emergency Ops',       'EO-JA'),
('Mathilde Eriksen',  'Storm Rescue',        'SR-ME');

-- Hindertyper (10 rader)
INSERT INTO ObstacleType (TypeName) VALUES
('Byggekraan'),
('Høyspentledning'),
('Tele/Radio-mast'),
('Vindturbin'),
('Midlertidig bygg'),
('Kranbil'),
('Skipsmast'),
('Tårnkran'),
('Mobilmast'),
('Annet');

-- ObstacleReport (15 rader)
INSERT INTO ObstacleReport
(PilotID, ObstacleTypeID, ReportDate, HeightMeters, Municipality, Description)
VALUES
(1, 1, '2023-03-10', 120, 'Kristiansand', 'Crane near hospital'),
(1, 2, '2023-05-05', 80, 'Kristiansand', 'Power line in valley'),
(1, 3, '2023-11-20', 150, 'Kristiansand', 'Radio mast on hill'),
(1, 1, '2025-01-15', 110, 'Kristiansand', 'Tall crane in city'),
(1, 2, '2025-02-07', 90, 'Kristiansand', 'New power line near route'),

(2, 2, '2023-02-01', 70, 'Kristiansand', 'High voltage line'),
(2, 3, '2023-09-09', 140, 'Kristiansand', 'Tall radio mast'),
(2, 5, '2024-10-05', 50, 'Kristiansand', 'Temporary construction crane'),
(2, 1, '2024-03-03', 100, 'Kristiansand', 'Harbour crane'),
(2, 2, '2025-03-03', 75, 'Kristiansand', 'New power line crossing'),

(3, 1, '2024-01-01', 95, 'Kristiansand', 'Small crane at hospital'),
(3, 4, '2025-09-09', 160, 'Kristiansand', 'Wind turbine near fjord'),
(4, 5, '2023-07-07', 60, 'Kristiansand', 'Festival stage crane'),
(4, 3, '2025-11-11', 145, 'Kristiansand', 'New telecom mast'),
(5, 7, '2023-05-05', 40, 'Kristiansand', 'Ship mast'),
    