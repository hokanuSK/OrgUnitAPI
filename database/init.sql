/*
    Bootstrap schema for assignment: REST API - Organizational Structure
    SQL Server / SQL Server Express
*/

IF DB_ID(N'CompanyStructureDb') IS NULL
BEGIN
    CREATE DATABASE CompanyStructureDb;
END
GO

USE CompanyStructureDb;
GO

-- ============================================================================
-- Companies
-- ============================================================================
IF OBJECT_ID('dbo.Companies', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Companies
    (
        CompanyId INT IDENTITY(1,1) NOT NULL,
        Code NVARCHAR(30) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        DirectorEmployeeId INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Companies_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Companies_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_Companies PRIMARY KEY CLUSTERED (CompanyId),
        CONSTRAINT UQ_Companies_Code UNIQUE (Code)
    );
END
GO

-- ============================================================================
-- Employees
-- ============================================================================
IF OBJECT_ID('dbo.Employees', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees
    (
        EmployeeId INT IDENTITY(1,1) NOT NULL,
        CompanyId INT NOT NULL,
        Title NVARCHAR(50) NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(30) NULL,
        Email NVARCHAR(254) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Employees_IsActive DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Employees_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Employees_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (EmployeeId),
        CONSTRAINT FK_Employees_Companies FOREIGN KEY (CompanyId)
            REFERENCES dbo.Companies (CompanyId)
            ON DELETE NO ACTION,
        CONSTRAINT UQ_Employees_Company_Employee UNIQUE (CompanyId, EmployeeId),
        CONSTRAINT UQ_Employees_Company_Email UNIQUE (CompanyId, Email)
    );

    CREATE INDEX IX_Employees_CompanyId ON dbo.Employees(CompanyId);
END
GO

-- ============================================================================
-- Organization units (Division / Project / Department)
-- ============================================================================
IF OBJECT_ID('dbo.OrgUnits', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrgUnits
    (
        OrgUnitId INT IDENTITY(1,1) NOT NULL,
        CompanyId INT NOT NULL,
        ParentOrgUnitId INT NULL,
        UnitType VARCHAR(20) NOT NULL,
        Code NVARCHAR(30) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        LeaderEmployeeId INT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_OrgUnits_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_OrgUnits_UpdatedAt DEFAULT SYSUTCDATETIME(),
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_OrgUnits PRIMARY KEY CLUSTERED (OrgUnitId),
        CONSTRAINT CK_OrgUnits_UnitType CHECK (UnitType IN ('Division', 'Project', 'Department')),
        CONSTRAINT CK_OrgUnits_NotSelfParent CHECK (ParentOrgUnitId IS NULL OR ParentOrgUnitId <> OrgUnitId),
        CONSTRAINT FK_OrgUnits_Companies FOREIGN KEY (CompanyId)
            REFERENCES dbo.Companies (CompanyId)
            ON DELETE NO ACTION,
        CONSTRAINT UQ_OrgUnits_Company_OrgUnit UNIQUE (CompanyId, OrgUnitId),
        CONSTRAINT UQ_OrgUnits_Company_Code UNIQUE (CompanyId, Code),
        CONSTRAINT FK_OrgUnits_Parent FOREIGN KEY (CompanyId, ParentOrgUnitId)
            REFERENCES dbo.OrgUnits (CompanyId, OrgUnitId)
            ON DELETE NO ACTION
    );

    CREATE INDEX IX_OrgUnits_CompanyId ON dbo.OrgUnits(CompanyId);
    CREATE INDEX IX_OrgUnits_Company_Parent ON dbo.OrgUnits(CompanyId, ParentOrgUnitId);
END
GO

-- ============================================================================
-- Optional assignment table for employee placements in org units
-- ============================================================================
IF OBJECT_ID('dbo.EmployeeOrgUnitAssignments', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmployeeOrgUnitAssignments
    (
        EmployeeId INT NOT NULL,
        OrgUnitId INT NOT NULL,
        CompanyId INT NOT NULL,
        AssignedFrom DATE NOT NULL CONSTRAINT DF_EOUA_AssignedFrom DEFAULT CONVERT(date, SYSUTCDATETIME()),
        AssignedTo DATE NULL,
        IsPrimary BIT NOT NULL CONSTRAINT DF_EOUA_IsPrimary DEFAULT (0),
        CONSTRAINT PK_EOUA PRIMARY KEY CLUSTERED (EmployeeId, OrgUnitId, AssignedFrom),
        CONSTRAINT CK_EOUA_DateRange CHECK (AssignedTo IS NULL OR AssignedTo >= AssignedFrom),
        CONSTRAINT FK_EOUA_Employee FOREIGN KEY (CompanyId, EmployeeId)
            REFERENCES dbo.Employees (CompanyId, EmployeeId)
            ON DELETE NO ACTION,
        CONSTRAINT FK_EOUA_OrgUnit FOREIGN KEY (CompanyId, OrgUnitId)
            REFERENCES dbo.OrgUnits (CompanyId, OrgUnitId)
            ON DELETE NO ACTION
    );

    CREATE INDEX IX_EOUA_CompanyId ON dbo.EmployeeOrgUnitAssignments(CompanyId);
    CREATE INDEX IX_EOUA_OrgUnitId ON dbo.EmployeeOrgUnitAssignments(OrgUnitId);
END
GO

-- ============================================================================
-- Cross-table FKs that depend on Employees existing
-- ============================================================================
IF OBJECT_ID('dbo.FK_Companies_Director', 'F') IS NULL
BEGIN
    ALTER TABLE dbo.Companies
    ADD CONSTRAINT FK_Companies_Director
        FOREIGN KEY (CompanyId, DirectorEmployeeId)
        REFERENCES dbo.Employees (CompanyId, EmployeeId)
        ON DELETE NO ACTION;
END
GO

IF OBJECT_ID('dbo.FK_OrgUnits_Leader', 'F') IS NULL
BEGIN
    ALTER TABLE dbo.OrgUnits
    ADD CONSTRAINT FK_OrgUnits_Leader
        FOREIGN KEY (CompanyId, LeaderEmployeeId)
        REFERENCES dbo.Employees (CompanyId, EmployeeId)
        ON DELETE NO ACTION;
END
GO

-- ============================================================================
-- UpdatedAt auto-maintenance
-- ============================================================================
CREATE OR ALTER TRIGGER dbo.TR_Companies_SetUpdatedAt
ON dbo.Companies
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE c
    SET UpdatedAt = SYSUTCDATETIME()
    FROM dbo.Companies c
    INNER JOIN inserted i ON i.CompanyId = c.CompanyId;
END
GO

CREATE OR ALTER TRIGGER dbo.TR_Employees_SetUpdatedAt
ON dbo.Employees
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE e
    SET UpdatedAt = SYSUTCDATETIME()
    FROM dbo.Employees e
    INNER JOIN inserted i ON i.EmployeeId = e.EmployeeId;
END
GO

CREATE OR ALTER TRIGGER dbo.TR_OrgUnits_SetUpdatedAt
ON dbo.OrgUnits
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ou
    SET UpdatedAt = SYSUTCDATETIME()
    FROM dbo.OrgUnits ou
    INNER JOIN inserted i ON i.OrgUnitId = ou.OrgUnitId;
END
GO

-- ============================================================================
-- Hierarchy validation trigger
-- Allowed model:
-- Division (root under company) -> Project -> Department
-- ============================================================================
CREATE OR ALTER TRIGGER dbo.TR_OrgUnits_ValidateHierarchy
ON dbo.OrgUnits
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Division cannot have parent.
    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        WHERE i.UnitType = 'Division'
          AND i.ParentOrgUnitId IS NOT NULL
    )
    BEGIN
        THROW 51001, 'Division cannot have a parent.', 1;
    END;

    -- Project must have parent Division in the same company.
    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        LEFT JOIN dbo.OrgUnits p
            ON p.CompanyId = i.CompanyId
           AND p.OrgUnitId = i.ParentOrgUnitId
        WHERE i.UnitType = 'Project'
          AND (
                i.ParentOrgUnitId IS NULL
                OR p.OrgUnitId IS NULL
                OR p.UnitType <> 'Division'
              )
    )
    BEGIN
        THROW 51002, 'Project must have parent Division in the same company.', 1;
    END;

    -- Department must have parent Project in the same company.
    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        LEFT JOIN dbo.OrgUnits p
            ON p.CompanyId = i.CompanyId
           AND p.OrgUnitId = i.ParentOrgUnitId
        WHERE i.UnitType = 'Department'
          AND (
                i.ParentOrgUnitId IS NULL
                OR p.OrgUnitId IS NULL
                OR p.UnitType <> 'Project'
              )
    )
    BEGIN
        THROW 51003, 'Department must have parent Project in the same company.', 1;
    END;
END
GO
