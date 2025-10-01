-- Schema for HerrlogDb
IF OBJECT_ID(N'dbo.Vehicles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vehicles
    (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Plate NVARCHAR(7) NOT NULL UNIQUE,
        Model NVARCHAR(100) NULL
    );
END;

IF OBJECT_ID(N'dbo.TrackingPoints', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrackingPoints
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        VehicleId UNIQUEIDENTIFIER NOT NULL,
        Latitude FLOAT NOT NULL,
        Longitude FLOAT NOT NULL,
        DateUtc DATETIME2 NOT NULL,
        Speed FLOAT NULL,
        Direction FLOAT NULL,
        RawPlate NVARCHAR(7) NULL,
        CONSTRAINT FK_TrackingPoints_Vehicles FOREIGN KEY (VehicleId)
            REFERENCES dbo.Vehicles (Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_TrackingPoints_VehicleId_DateUtc
        ON dbo.TrackingPoints (VehicleId, DateUtc);

END;

-- seed 
IF NOT EXISTS (SELECT 1 FROM dbo.Vehicles WHERE Plate = N'ABC1D23')
BEGIN
    INSERT INTO dbo.Vehicles (Id, Plate, Model)
    VALUES (NEWID(), N'ABC1D23', N'Demo Vehicle');
END;
