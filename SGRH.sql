-- Create SGRH_IPLSistemas database
CREATE DATABASE SGRH_DB;
GO

-- Use the created database
USE SGRH_DB;
GO

-- Creación de la tabla Departamentos
CREATE TABLE Departments (
    ID_Department INT PRIMARY KEY IDENTITY(1,1),
    Department_Name NVARCHAR(100) UNIQUE NOT NULL
);
GO

-- Creación de la tabla Puestos
CREATE TABLE Positions (
    ID_Position INT PRIMARY KEY IDENTITY(1,1),
    Position_Name NVARCHAR(100) UNIQUE NOT NULL
);
GO

DROP TABLE Roles
-- Create Roles table
CREATE TABLE Roles (
    ID_Role INT PRIMARY KEY IDENTITY(1,1),
    Role_Name NVARCHAR(50) UNIQUE
);
GO

-- Create Users table
CREATE TABLE Users (
    ID_User INT PRIMARY KEY IDENTITY(1,1),
    First_Name NVARCHAR(50) NOT NULL,
    Last_Name NVARCHAR(50) NOT NULL,
	DNI NVARCHAR(9) UNIQUE NOT NULL,
	Phone NVARCHAR(8) UNIQUE NOT NULL,
	Email NVARCHAR(100) UNIQUE NOT NULL,
	Username NVARCHAR(20) UNIQUE NOT NULL, 
    Password NVARCHAR(100) NOT NULL,
	ID_Department INT NOT NULL,
	ID_Position INT NOT NULL,
	FOREIGN KEY (ID_Department) REFERENCES Departments(ID_Department),
	FOREIGN KEY (ID_Position) REFERENCES Positions(ID_Position)
);
GO

DROP TABLE Users_Roles
-- Create Users_Roles table
CREATE TABLE Users_Roles (
    ID_User_Role INT PRIMARY KEY IDENTITY(1,1),
    ID_User INT FOREIGN KEY REFERENCES Users(ID_User),
    ID_Role INT FOREIGN KEY REFERENCES Roles(ID_Role)
);
GO

-- Create PersonalActions table
--- Vacations
--- OverTime
--- Liquidations or Benefits
--- Warnings
CREATE TABLE Personal_Actions (
    ID_Action INT PRIMARY KEY IDENTITY(1,1),
    Action_Type NVARCHAR(50) NOT NULL,
    Description NVARCHAR(250),
    Date DATE,
    ID_User INT FOREIGN KEY REFERENCES Users(ID_User),
	Is_Approved BIT NULL,
	Approved_By_User INT FOREIGN KEY REFERENCES Users(ID_User) NULL,
	Approval_Date DATE NULL,
	Actions_Status NVARCHAR(50) NULL
);
GO

-- Create Vacations table
CREATE TABLE Vacations (
    ID_Vacation INT PRIMARY KEY IDENTITY(1,1),
    ID_Action INT FOREIGN KEY REFERENCES Personal_Actions(ID_Action),
    Start_Date DATE,
    End_Date DATE,
	Comments NVARCHAR(150)
);
GO

-- Create Overtime table
CREATE TABLE Overtime (
    ID_Overtime INT PRIMARY KEY IDENTITY(1,1),
    ID_Action INT FOREIGN KEY REFERENCES Personal_Actions(ID_Action),
    Date DATE,
    Hours_Worked INT
);
GO

-- Create Benefits_Liquidations table
CREATE TABLE Benefits_Liquidations (
    ID_Liquidation INT PRIMARY KEY IDENTITY(1,1),
    ID_Action INT FOREIGN KEY REFERENCES Personal_Actions(ID_Action),
    Amount DECIMAL(18,2),
	Taxes DECIMAL(18,2) NOT NULL,
    Deductions DECIMAL(18,2) NOT NULL,
    Net_Amount DECIMAL(18,2) NOT NULL,
	Benefit_Type NVARCHAR(100),
	Period_Payment NVARCHAR(50) NOT NULL
);
GO

-- Create Warnings table
CREATE TABLE Warnings (
    ID_Warnings INT PRIMARY KEY IDENTITY(1,1),
    ID_Action INT FOREIGN KEY REFERENCES Personal_Actions(ID_Action),
    Reason NVARCHAR(500),
    Observations NVARCHAR(500)
);
GO

-- Create Attendance table
CREATE TABLE Attendance (
    ID_Attendance INT PRIMARY KEY IDENTITY(1,1),
    ID_User INT FOREIGN KEY REFERENCES Users(ID_User),
    Date DATE,
    Entry_Time TIME,
    Exit_Time TIME,
    Notes NVARCHAR(250)
);

-- Table to manage absence categories
CREATE TABLE AbsenceCategories (
    ID_Absence_Category INT PRIMARY KEY IDENTITY(1,1),
    Category_Name NVARCHAR(50) UNIQUE NOT NULL
);


-- Create Absences table
CREATE TABLE Absences (
    ID_Absence INT PRIMARY KEY IDENTITY(1,1),
    ID_User INT FOREIGN KEY REFERENCES Users(ID_User),
    ID_Absence_Category INT FOREIGN KEY REFERENCES AbsenceCategories(ID_Absence_Category),
    Start_Date DATE,
    End_Date DATE, 
    Absence_Comments NVARCHAR(500),
    Documentation_Absence NVARCHAR(100),
    ID_Supervisor INT FOREIGN KEY REFERENCES Users(ID_User),
    Request_Status NVARCHAR(50)
);

-- Create Dossier table
CREATE TABLE Dossier (
    ID_Record INT PRIMARY KEY IDENTITY(1,1),
    ID_User INT FOREIGN KEY REFERENCES Users(ID_User),
    Document_Type NVARCHAR(100),
    Description NVARCHAR(500),
    Date DATE,
	File_URL NVARCHAR(MAX)
);
GO

-- Create Payroll table
CREATE TABLE Payroll (
    ID_Payroll INT PRIMARY KEY IDENTITY(1,1),
    ID_User INT FOREIGN KEY REFERENCES Users(ID_User),
    Payroll_Period NVARCHAR(30),
	Base_Salary DECIMAL(18,2),
	Overtime_Rate DECIMAL(18,2),
	Total_Overtime_Hours DECIMAL(18,2),
    Total_Amount DECIMAL(18,2),
    Deductions DECIMAL(18,2),
	Payroll_Date DATE
);
GO
